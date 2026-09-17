"""Neutral, unblurred renders of the actual LOD0 source meshes."""
import bpy,json,math
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'NightCityKit.blend'))
scene=bpy.context.scene
for o in scene.objects:
 if o.type=='MESH':o.hide_render=True
 if o.type=='LIGHT':o.hide_render=True
scene.world.node_tree.nodes.get('Background').inputs[0].default_value=(.22,.26,.3,1);scene.world.node_tree.nodes.get('Background').inputs[1].default_value=.6
ld=bpy.data.lights.new('Neutral softbox','AREA');ld.energy=2200;ld.size=10;lo=bpy.data.objects.new('Neutral softbox',ld);scene.collection.objects.link(lo)
scene.render.resolution_x=640;scene.render.resolution_y=640;scene.cycles.samples=16
scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
cam=scene.camera;cam.data.type='ORTHO'
out=ROOT/'renders/catalog';out.mkdir(exist_ok=True)
records=json.loads((ROOT/'mesh-manifest.json').read_text())['assets']
for record in records:
 if record['lod']:continue
 ob=bpy.data.objects['NC_'+record['id']+'_LOD0'];ob.hide_render=False
 corners=[ob.matrix_world@Vector(v) for v in ob.bound_box];center=sum(corners,Vector())/8
 size=max(ob.dimensions);cam.location=center+Vector((.95,-1.6,.85))*size;cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=size*1.35
 lo.location=center+Vector((-.5,-.8,1.3))*size;lo.rotation_euler=(center-lo.location).to_track_quat('-Z','Y').to_euler();ld.energy=100*size*size;ld.size=size*1.2
 scene.render.filepath=str(out/(record['id']+'.png'));bpy.ops.render.render(write_still=True);ob.hide_render=True
print('CATALOG_COMPLETE',sum(1 for x in records if not x['lod']))
