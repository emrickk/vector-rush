import bpy,math
from pathlib import Path
from mathutils import Vector
r=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(r/'KoiLanternTower.blend'))
s=bpy.context.scene;s.render.engine='CYCLES';s.cycles.samples=32;s.cycles.use_denoising=True;s.render.threads_mode='FIXED';s.render.threads=8
s.render.resolution_x=1400;s.render.resolution_y=1100;s.render.resolution_percentage=100
s.world.color=(.07,.09,.11);s.view_settings.view_transform='AgX'
for name,p,power,size,col in [('Key',(-20,-25,94),22000,25,(1,.83,.63)),('Fill',(22,-15,73),14000,20,(.5,.7,1)),('Rim',(0,15,85),18000,18,(1,.48,.12))]:
 bpy.ops.object.light_add(type='AREA',location=p);o=bpy.context.object;o.name=name;o.data.energy=power;o.data.shape='DISK';o.data.size=size;o.data.color=col;o.rotation_euler=(Vector((0,0,65))-o.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add(location=(27,-73,82));c=bpy.context.object;c.rotation_euler=(Vector((0,0,66))-c.location).to_track_quat('-Z','Y').to_euler();c.data.type='ORTHO';c.data.ortho_scale=60;s.camera=c
s.render.filepath=str(r/'asset-review.png');bpy.ops.render.render(write_still=True)
