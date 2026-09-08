"""Bounded finish over immutable pass02: paint shader contract and separate core material.
Load pass02 blend with Blender; -- output_dir evidence_dir livery_asset_dir
"""
import bpy,bmesh,json,sys,hashlib,shutil
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:];OUT=Path(args[0]);EVIDENCE=Path(args[1]);PAINT=Path(args[2]);SOURCE=Path(bpy.data.filepath)
OUT.mkdir(parents=True,exist_ok=True);EVIDENCE.mkdir(parents=True,exist_ok=True)
if (OUT/'Kestrel07-v3.blend').exists():raise RuntimeError('Immutable revision exists; use a new finish directory.')
shutil.copy2(__file__,OUT/'finish_recipe_snapshot.py')
for f in PAINT.iterdir():
 if f.is_file():shutil.copy2(f,OUT/f.name)
for f in ['recipe_snapshot.py','engine-anchors.json']:
 shutil.copy2(SOURCE.parent/f,OUT/('form_'+f if f=='recipe_snapshot.py' else f))
# The reusable livery group survives replacement of the coating material by the baker.
g=bpy.data.node_groups.new('SHIP_LIVERY','ShaderNodeTree')
g.interface.new_socket(name='BaseColor',in_out='INPUT',socket_type='NodeSocketColor')
g.interface.new_socket(name='Color',in_out='OUTPUT',socket_type='NodeSocketColor')
n=g.nodes;l=g.links
i=n.new('NodeGroupInput');o=n.new('NodeGroupOutput');geo=n.new('ShaderNodeNewGeometry');sep=n.new('ShaderNodeSeparateXYZ');l.new(geo.outputs['Position'],sep.inputs[0])
def mathnode(op,a,b):
 node=n.new('ShaderNodeMath');node.operation=op
 for index,v in enumerate([a,b]):
  if isinstance(v,(float,int)):node.inputs[index].default_value=v
  else:l.new(v,node.inputs[index])
 return node.outputs[0]
u=mathnode('MULTIPLY_ADD',sep.outputs['X'],1/5.5);u.node.inputs[2].default_value=.5
v=mathnode('MULTIPLY_ADD',sep.outputs['Y'],1/7.2);v.node.inputs[2].default_value=.5
coord=n.new('ShaderNodeCombineXYZ');l.new(u,coord.inputs['X']);l.new(v,coord.inputs['Y'])
tex=n.new('ShaderNodeTexImage');tex.name='Packed authored paint mask';tex.image=bpy.data.images.load(str(OUT/'ship-livery-mask.png'));tex.image.colorspace_settings.name='Non-Color';tex.image.pack();tex.interpolation='Linear';tex.extension='CLIP';l.new(coord.outputs[0],tex.inputs['Vector'])
channels=n.new('ShaderNodeSeparateColor');channels.mode='RGB';l.new(tex.outputs['Color'],channels.inputs[0])
normal=n.new('ShaderNodeSeparateXYZ');l.new(geo.outputs['Normal'],normal.inputs[0])
up=mathnode('GREATER_THAN',normal.outputs['Z'],.42);above=mathnode('GREATER_THAN',sep.outputs['Z'],.12);mask=mathnode('MULTIPLY',up,above)
ink=mathnode('MULTIPLY',channels.outputs['Red'],mask);citron=mathnode('MULTIPLY',channels.outputs['Green'],mask)
paint=n.new('ShaderNodeMixRGB');paint.blend_type='MIX';l.new(ink,paint.inputs[0]);l.new(i.outputs['BaseColor'],paint.inputs[1]);paint.inputs[2].default_value=(.016,.024,.032,1)
accent=n.new('ShaderNodeMixRGB');accent.blend_type='MIX';l.new(citron,accent.inputs[0]);l.new(paint.outputs[0],accent.inputs[1]);accent.inputs[2].default_value=(.48,.62,.025,1);l.new(accent.outputs[0],o.inputs['Color'])
for key in ['Ivory','Graphite']:
 mat=bpy.data.materials[key];nodes=mat.node_tree.nodes;links=mat.node_tree.links;bsdf=nodes.get('Principled BSDF');group=nodes.new('ShaderNodeGroup');group.name='SHIP_LIVERY';group.label='Flush painted identity / bake-safe source projection';group.node_tree=g;group.inputs['BaseColor'].default_value=bsdf.inputs['Base Color'].default_value;links.new(group.outputs['Color'],bsdf.inputs['Base Color'])
# Match simple preview category values to parent coating's base hierarchy.
for key,color,metal,rough in [('Ivory',(.76,.785,.80),0,.36),('Graphite',(.037,.045,.054),.12,.49),('Metal',(.24,.275,.30),.78,.32),('Ceramic',(.085,.095,.108),0,.54)]:
 m=bpy.data.materials[key];bs=m.node_tree.nodes.get('Principled BSDF');m.diffuse_color=(*color,1);bs.inputs['Metallic'].default_value=metal;bs.inputs['Roughness'].default_value=rough
 if key in ['Ivory','Graphite']:m.node_tree.nodes['SHIP_LIVERY'].inputs['BaseColor'].default_value=(*color,1)
 else:bs.inputs['Base Color'].default_value=(*color,1)
ring=bpy.data.materials['Engine'];bs=ring.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(.08,.42,.56,1);bs.inputs['Emission Color'].default_value=(.06,.42,.58,1);bs.inputs['Emission Strength'].default_value=.7
core=ring.copy();core.name='EngineCore';bs=core.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(.80,.94,1,1);bs.inputs['Emission Color'].default_value=(.80,.94,1,1);bs.inputs['Emission Strength'].default_value=4.5;core.diffuse_color=(.80,.94,1,1)
craft=bpy.data.collections['KESTREL_07_EXPORT'];runtime=bpy.data.collections['EXPORT_RUNTIME'];runtime.hide_viewport=False;runtime.hide_render=False
for obj in craft.objects:
 if obj.type=='MESH' and ('compact thrust core' in obj.name or 'compact impulse core' in obj.name):obj.data.materials.clear();obj.data.materials.append(core)
# Preserve all approved coated-mesh UVs verbatim. Only the Engine material is split.
for obj in list(runtime.objects):
 if obj.type=='MESH' and obj.data.materials[0].name=='Engine':bpy.data.objects.remove(obj,do_unlink=True)
for key in ['Engine','EngineCore']:
 copies=[]
 for src in craft.objects:
  if src.type=='MESH' and src.data.materials[0].name==key:
   obj=src.copy();obj.data=src.data.copy();runtime.objects.link(obj);copies.append(obj)
 bpy.ops.object.select_all(action='DESELECT')
 for obj in copies:obj.select_set(True)
 bpy.context.view_layer.objects.active=copies[0];bpy.ops.object.join();obj=bpy.context.object;obj.name='Kestrel07_V3_'+key;bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 mod=obj.modifiers.new('Final runtime triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=mod.name)
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.151917,island_margin=.008,area_weight=0.,correct_aspect=True,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT')
bpy.ops.object.select_all(action='DESELECT')
for obj in runtime.objects:obj.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'HeroShip.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False,use_tspace=True,path_mode='AUTO')
runtime.hide_render=True;runtime.hide_viewport=True
stats=json.loads((SOURCE.parent/'asset-stats.json').read_text());stats.update({'revision':OUT.name,'primary_form_only':False,'form_source_sha256':hashlib.sha256(SOURCE.read_bytes()).hexdigest(),'finish_recipe_sha256':hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),'runtime_meshes':len(runtime.objects),'materials_used':[obj.data.materials[0].name for obj in runtime.objects],'finish_scope':'Flush authored paint shader, coating preview hierarchy, compact EngineCore separated from dim cyan Engine rings. Major shape unchanged. Parent owns baked coating maps/native material integration.','runtime_uv_policy':'Pass02 Ivory/Graphite/Glass/Metal/Ceramic runtime UVs preserved verbatim. Engine and EngineCore re-atlased after material split. EXPORT_RUNTIME is exact final FBX source; do not re-unwrap after bake.'})
(OUT/'asset-stats.json').write_text(json.dumps(stats,indent=2))
contract={'group_node_name':'SHIP_LIVERY','node_group_name':g.name,'input':'BaseColor','output':'Color','materials_using_group':['Ivory','Graphite'],'projection':'Source/world XY with upper-normal and height masks','packed_image':'ship-livery-mask.png','livery_geometry_added':False,'EngineCore':'compact near-white disk meshes only; native map before Engine substring','Engine':'thin dim cyan annuli only','source_form_unchanged':True}
(OUT/'finish-contract.json').write_text(json.dumps(contract,indent=2))
scene=bpy.context.scene;scene.render.threads_mode='FIXED';scene.render.threads=4;scene.cycles.samples=32;scene.cycles.use_denoising=False
views=[('01-neutral-rear-quarter',(-8,10,6),(0,0,.2),9.6,False,'ORTHO'),('02-neutral-chase',(0,10,4.4),(0,-.8,.05),9.6,False,'ORTHO'),('03-neutral-front-quarter',(9,-11,7),(0,-.1,.1),9.6,False,'ORTHO'),('04-livery-top',(0,-.001,15),(0,-.2,0),10,False,'ORTHO'),('05-engine-detail',(-3.5,7,2.8),(-1.15,2.6,.02),4.2,False,'ORTHO')]
(EVIDENCE/'capture-contract.json').write_text(json.dumps({'revision':OUT.name,'resolution':[1600,1200],'neutral_white_studio':True,'bloom':False,'view_transform':'AgX','views':views,'perspective_focal_length_mm':45,'samples':32,'denoising':False,'threads':4},indent=2))
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Kestrel07-v3.blend'))
print('SHIP_V3_FINISH_SOURCE_READY',json.dumps(stats),flush=True)
