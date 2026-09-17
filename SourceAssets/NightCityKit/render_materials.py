"""Dry/wet pairs under identical neutral lighting, using the exported kit materials."""
import bpy, json
from pathlib import Path
from mathutils import Vector

root = Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(root / 'NightCityKit.blend'))
scene = bpy.context.scene
# Blender drops unused materials when saving unless they have a retained user.
# Keep every engine material variant in the editable source, including spare wet variants.
records = json.loads((root / 'materials.json').read_text())['materials']
for r in records:
    m = bpy.data.materials.get(r['id']) or bpy.data.materials.new(r['id'])
    m.use_fake_user = True; m.use_nodes = True
    if m.node_tree.nodes.get('Principled BSDF') is None: continue
    p = m.node_tree.nodes['Principled BSDF']; links = m.node_tree.links
    def texture(key, data=False):
        node=m.node_tree.nodes.new('ShaderNodeTexImage')
        node.image=bpy.data.images.load(str(root/'textures'/r[key]),check_existing=True)
        if data: node.image.colorspace_settings.name='Non-Color'
        return node
    base=texture('base');mix=m.node_tree.nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=1;mix.inputs[2].default_value=(*r['baseTint'],1)
    links.new(base.outputs['Color'],mix.inputs[1]);links.new(mix.outputs[0],p.inputs['Base Color'])
    p.inputs['Metallic'].default_value=r['metallic'];p.inputs['Roughness'].default_value=r['roughness']
    if r['ms']:
        packed=texture('ms',True);channels=m.node_tree.nodes.new('ShaderNodeSeparateColor');links.new(packed.outputs['Color'],channels.inputs['Color']);links.new(channels.outputs['Red'],p.inputs['Metallic'])
        inv=m.node_tree.nodes.new('ShaderNodeMath');inv.operation='SUBTRACT';inv.inputs[0].default_value=1;links.new(packed.outputs['Alpha'],inv.inputs[1]);links.new(inv.outputs[0],p.inputs['Roughness'])
    if r['normal']:
        normal=texture('normal',True);nm=m.node_tree.nodes.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.7;links.new(normal.outputs['Color'],nm.inputs['Color']);links.new(nm.outputs['Normal'],p.inputs['Normal'])
for image in bpy.data.images:
    if image.source=='FILE': image.filepath=bpy.path.relpath(image.filepath,start=str(root))
bpy.ops.wm.save_as_mainfile(filepath=str(root/'NightCityKit.blend'))
for obj in list(scene.objects):
    bpy.data.objects.remove(obj, do_unlink=True)
for image in bpy.data.images:
    if image.source == 'FILE':
        image.reload()
scene.world.node_tree.nodes['Background'].inputs[0].default_value = (.16, .19, .22, 1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value = .55
scene.render.resolution_x = 1000
scene.render.resolution_y = 600
scene.render.resolution_percentage = 100
scene.cycles.samples = 24
scene.render.image_settings.file_format = 'PNG'
for name, pos, power, size in [('Key', (-3, -4, 5), 650, 3), ('Strip', (4, 1, 3), 900, 2)]:
    light = bpy.data.lights.new(name, 'AREA'); light.energy = power; light.shape = 'RECTANGLE'
    light.size = size; light.size_y = .5
    obj = bpy.data.objects.new(name, light); scene.collection.objects.link(obj); obj.location = pos
    obj.rotation_euler = (Vector((0, 0, 0)) - obj.location).to_track_quat('-Z', 'Y').to_euler()
camera = bpy.data.objects.new('Material camera', bpy.data.cameras.new('Material camera'))
scene.collection.objects.link(camera); scene.camera = camera
camera.location = (0, -7, 3); camera.rotation_euler = (Vector((0, 0, .1)) - camera.location).to_track_quat('-Z', 'Y').to_euler()
camera.data.type = 'ORTHO'; camera.data.ortho_scale = 5.2
spheres = []
for x in [-1.25, 1.25]:
    bpy.ops.mesh.primitive_uv_sphere_add(segments=64, ring_count=32, radius=1, location=(x, 0, 0))
    sphere = bpy.context.object
    for poly in sphere.data.polygons: poly.use_smooth = True
    spheres.append(sphere)
out = root / 'renders/materials'; out.mkdir(exist_ok=True)
records = json.loads((root / 'materials.json').read_text())['materials']
for record in records:
    name = record['id']
    if record['kind'] != 'surface' or name.endswith('_wet'): continue
    for obj, material in zip(spheres, [name, name + '_wet']):
        obj.data.materials.clear(); obj.data.materials.append(bpy.data.materials[material])
    scene.render.filepath = str(out / (name + '.png'))
    bpy.ops.render.render(write_still=True)
print('MATERIAL_PAIRS_COMPLETE 8')
