"""Refresh assembly renders from the delivered editable source."""
import bpy
from pathlib import Path
from mathutils import Vector
root=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(root/'NightCityKit.blend'))
scene=bpy.context.scene
for image in bpy.data.images:
 if image.source=='FILE':image.reload()
cam=scene.camera
cam.location=(29,-35,19);cam.rotation_euler=(Vector((-1,8,10))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=42
scene.render.filepath=str(root/'renders/night-assembly.png');bpy.ops.render.render(write_still=True)
cam.location=(5,-16,10);cam.rotation_euler=(Vector((-7,-.5,4))-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=48
scene.render.filepath=str(root/'renders/rooftop-detail.png');bpy.ops.render.render(write_still=True)
print('ASSEMBLY_COMPLETE')
