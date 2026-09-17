"""Frame the exported city kit and close modules without cropping the landmark."""
import bpy
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1]
bpy.ops.wm.open_mainfile(filepath=str(ROOT/"packages/vrx-b01/source/vrx_city_kit.blend"))
scene=bpy.context.scene;cam=scene.camera
studio=bpy.data.collections["STUDIO_NOT_EXPORTED"]
models=[o for o in studio.objects if o.type=="MESH" and o.name.startswith("vrx_")]
def fit(objects):
    inv=cam.matrix_world.inverted()
    points=[inv@(o.matrix_world@Vector(v)) for o in objects for v in o.bound_box]
    lo=[min(p[k] for p in points) for k in [0,1]];hi=[max(p[k] for p in points) for k in [0,1]]
    offset=cam.rotation_euler.to_matrix()@Vector(((lo[0]+hi[0])/2,(lo[1]+hi[1])/2,0))
    cam.location+=offset
    cam.data.ortho_scale=max(hi[0]-lo[0],(hi[1]-lo[1])*scene.render.resolution_x/scene.render.resolution_y)*1.14
    bpy.context.view_layer.update()
fit(models)
scene.render.filepath=str(ROOT/"evidence/vrx-b01-kit.png");bpy.ops.render.render(write_still=True)
props=[o for o in models if any(k in o.name for k in ["podium","guidance","canopy","bridge","plant","advert"])]
for o in models:o.hide_render=o not in props
for i,o in enumerate(props):o.location=((i%3)*36,(i//3)*43,0)
cam.location=(115,-155,110);cam.rotation_euler=(Vector((35,20,7))-cam.location).to_track_quat("-Z","Y").to_euler()
bpy.context.view_layer.update();fit(props)
scene.render.filepath=str(ROOT/"evidence/vrx-b01-close-modules.png");bpy.ops.render.render(write_still=True)
