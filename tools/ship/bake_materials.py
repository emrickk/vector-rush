"""Bake restrained coatings into the exact runtime UV meshes, without changing geometry.

Blender --background --python tools/ship/bake_materials.py --
  --blend <staged runtime .blend> --out <new output directory> [--size 2048]
The input blend and live Unity assets are never overwritten. Review maps before integration.
"""
import argparse
import hashlib
import json
import random
import sys
from pathlib import Path

import bpy

parser = argparse.ArgumentParser()
parser.add_argument('--blend', required=True)
parser.add_argument('--out', required=True)
parser.add_argument('--size', type=int, default=2048)
parser.add_argument('--samples', type=int, default=24)
args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:])
source = Path(args.blend).resolve()
out = Path(args.out).resolve()
if out.exists() and any(out.iterdir()):
    raise RuntimeError('Choose a fresh output directory; reviewed maps must not be overwritten.')
out.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(source))
scene = bpy.context.scene
scene.render.engine = 'CYCLES'
scene.cycles.device = 'CPU'
scene.cycles.samples = args.samples
scene.render.threads_mode = 'FIXED'
scene.render.threads = 4
scene.render.bake.margin = 16
scene.render.bake.use_selected_to_active = False
runtime_collection = bpy.data.collections.get('EXPORT_RUNTIME')
if runtime_collection:
    runtime = [o for o in runtime_collection.all_objects if o.type == 'MESH']
else:
    runtime = [o for o in scene.objects if o.type == 'MESH' and o.name.startswith('Kestrel07_')]
if not runtime:
    raise RuntimeError('No EXPORT_RUNTIME collection or Kestrel07_ runtime meshes found.')
for o in scene.objects:
    if o.type == 'MESH':
        o.hide_render = o not in runtime
        o.hide_set(o not in runtime)
for o in runtime:
    if not o.data.uv_layers.active:
        raise RuntimeError('Runtime UVs missing: ' + o.name)


def panel_tones(mesh, seed):
    """Stable, slight component variation; individual manufactured panels stay coherent."""
    neighbors = [[] for _ in mesh.vertices]
    for edge in mesh.edges:
        a, b = edge.vertices
        neighbors[a].append(b)
        neighbors[b].append(a)
    values = [None] * len(neighbors)
    rng = random.Random(seed)
    for start in range(len(values)):
        if values[start] is not None:
            continue
        tone = rng.uniform(.975, 1.015)
        stack = [start]
        values[start] = tone
        while stack:
            for adjacent in neighbors[stack.pop()]:
                if values[adjacent] is None:
                    values[adjacent] = tone
                    stack.append(adjacent)
    attribute = mesh.color_attributes.get('PanelTone') or mesh.color_attributes.new(name='PanelTone', type='FLOAT_COLOR', domain='CORNER')
    for loop in mesh.loops:
        v = values[loop.vertex_index]
        attribute.data[loop.index].color = (v, v, v, 1)


def image_node(material, name, color_space='Non-Color'):
    image = bpy.data.images.new(name, width=args.size, height=args.size, alpha=True)
    image.colorspace_settings.name = color_space
    node = material.node_tree.nodes.new('ShaderNodeTexImage')
    node.image = image
    material.node_tree.nodes.active = node
    return image


def save(image, name):
    image.filepath_raw = str(out / (name + '.png'))
    image.file_format = 'PNG'
    image.save()
    return {'file': name + '.png', 'sha256': hashlib.sha256((out / (name + '.png')).read_bytes()).hexdigest(), 'color_space': image.colorspace_settings.name}


def coating(obj, key, base, metallic, roughness, seed):
    panel_tones(obj.data, seed)
    mat = bpy.data.materials.new('Baked coating / ' + key)
    mat.use_nodes = True
    nodes, links = mat.node_tree.nodes, mat.node_tree.links
    nodes.clear()
    output = nodes.new('ShaderNodeOutputMaterial')
    bsdf = nodes.new('ShaderNodeBsdfPrincipled')
    bsdf.inputs['Metallic'].default_value = metallic
    links.new(bsdf.outputs['BSDF'], output.inputs['Surface'])
    geometry = nodes.new('ShaderNodeNewGeometry')
    grain = nodes.new('ShaderNodeTexNoise')
    grain.inputs['Scale'].default_value = 170 if key == 'Ivory' else 95
    grain.inputs['Detail'].default_value = 2
    grain.inputs['Roughness'].default_value = .55
    links.new(geometry.outputs['Position'], grain.inputs['Vector'])
    rough = nodes.new('ShaderNodeMapRange')
    rough.inputs['From Min'].default_value = 0
    rough.inputs['From Max'].default_value = 1
    rough.inputs['To Min'].default_value = roughness - .035
    rough.inputs['To Max'].default_value = roughness + .035
    links.new(grain.outputs['Fac'], rough.inputs['Value'])
    links.new(rough.outputs['Result'], bsdf.inputs['Roughness'])
    bump = nodes.new('ShaderNodeBump')
    bump.inputs['Strength'].default_value = .14 if key == 'Ivory' else .22
    bump.inputs['Distance'].default_value = .0012 if key == 'Ivory' else .002
    links.new(grain.outputs['Fac'], bump.inputs['Height'])
    links.new(bump.outputs['Normal'], bsdf.inputs['Normal'])
    tone = nodes.new('ShaderNodeAttribute')
    tone.attribute_name = 'PanelTone'
    tint = nodes.new('ShaderNodeMixRGB')
    tint.blend_type = 'MULTIPLY'
    tint.inputs[0].default_value = 1
    tint.inputs[2].default_value = (*base, 1)
    links.new(tone.outputs['Color'], tint.inputs[1])
    links.new(tint.outputs[0], bsdf.inputs['Base Color'])
    obj.data.materials.clear()
    obj.data.materials.append(mat)
    bpy.ops.object.select_all(action='DESELECT')
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    normal = image_node(mat, key + '_Normal')
    bpy.ops.object.bake(type='NORMAL', normal_space='TANGENT')
    records = [save(normal, key + '_Normal')]
    emission = nodes.new('ShaderNodeEmission')
    links.new(emission.outputs[0], output.inputs['Surface'])
    links.new(tint.outputs[0], emission.inputs['Color'])
    color = image_node(mat, key + '_BaseColor', 'sRGB')
    bpy.ops.object.bake(type='EMIT')
    records.append(save(color, key + '_BaseColor'))
    links.new(rough.outputs['Result'], emission.inputs['Color'])
    rough_image = image_node(mat, key + '_Roughness')
    bpy.ops.object.bake(type='EMIT')
    records.append(save(rough_image, key + '_Roughness'))
    rough_pixels = list(rough_image.pixels)
    packed = bpy.data.images.new(key + '_MetallicSmoothness', width=args.size, height=args.size, alpha=True)
    packed.colorspace_settings.name = 'Non-Color'
    pixels = [0.] * len(rough_pixels)
    for i in range(0, len(pixels), 4):
        pixels[i:i+4] = (metallic, metallic, metallic, 1 - rough_pixels[i])
    packed.pixels.foreach_set(pixels)
    records.append(save(packed, key + '_MetallicSmoothness'))
    links.new(bsdf.outputs['BSDF'], output.inputs['Surface'])
    ao = image_node(mat, key + '_Occlusion')
    bpy.ops.object.bake(type='AO')
    records.append(save(ao, key + '_Occlusion'))
    # Save a preview using the baked maps, rather than a prettier procedural substitute.
    base_tex = nodes.new('ShaderNodeTexImage'); base_tex.image = color
    rough_tex = nodes.new('ShaderNodeTexImage'); rough_tex.image = rough_image
    normal_tex = nodes.new('ShaderNodeTexImage'); normal_tex.image = normal
    normal_map = nodes.new('ShaderNodeNormalMap')
    links.new(base_tex.outputs['Color'], bsdf.inputs['Base Color'])
    links.new(rough_tex.outputs['Color'], bsdf.inputs['Roughness'])
    links.new(normal_tex.outputs['Color'], normal_map.inputs['Color'])
    links.new(normal_map.outputs['Normal'], bsdf.inputs['Normal'])
    return records


settings = {'Ivory': ((.76, .785, .80), 0., .36, 401), 'Graphite': ((.037, .045, .054), .12, .49, 402), 'Metal': ((.24, .275, .30), .78, .32, 403), 'Ceramic': ((.085, .095, .108), 0., .54, 404)}
manifest = {'input_blend': str(source), 'input_sha256': hashlib.sha256(source.read_bytes()).hexdigest(), 'size': args.size, 'scope': 'Candidate coatings baked into existing runtime UVs. Geometry and UV coordinates are unchanged. No visual acceptance implied.', 'normal_convention': 'Tangent space, OpenGL +Y; import as Unity NormalMap without green inversion.', 'packed_contract': 'MetallicSmoothness RGB metallic, A 1 minus linear roughness; import linear. Occlusion green channel, linear. BaseColor sRGB.', 'materials': {}}
for key, (base, metallic, roughness, seed) in settings.items():
    candidates = [o for o in runtime if any(m and m.name.split('.')[0] == key for m in o.data.materials)]
    if len(candidates) != 1:
        raise RuntimeError(f'Expected one joined runtime object for {key}, found {len(candidates)}')
    manifest['materials'][key] = coating(candidates[0], key, base, metallic, roughness, seed)
(out / 'material-manifest.json').write_text(json.dumps(manifest, indent=2) + '\n')
bpy.ops.wm.save_as_mainfile(filepath=str(out / 'Kestrel07-baked-materials.blend'))
print('SHIP_MATERIAL_BAKE_COMPLETE', str(out))
