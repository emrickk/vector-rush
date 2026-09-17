"""Static composition diagnostics using the actual course and ChaseCamera formula.
Blender --background --python this-file -- a01
Not a native camera recording, lighting validation or performance measurement.
"""
import bpy,sys,json,math
from pathlib import Path
from mathutils import Vector,Matrix,Quaternion
ROOT=Path(__file__).resolve().parents[1]
revision="vrx-"+sys.argv[sys.argv.index("--")+1]
package=ROOT/"packages"/revision
bpy.ops.wm.open_mainfile(filepath=str(package/"source/vrx_city_kit.blend"))
scene=bpy.context.scene
for col in list(bpy.data.collections):
    col.hide_render=True;col.hide_viewport=True
preview=bpy.data.collections.new("ROUTE_COMPOSITION_NOT_NATIVE");scene.collection.children.link(preview)
C=Matrix(((1,0,0),(0,0,-1),(0,1,0)))
def cv(x):return C@Vector(x)
manifest=json.loads((package/"manifest.json").read_text())
layout=json.loads((package/"layout.json").read_text())
course=json.loads((ROOT/"context/course-data.json").read_text())
def frame(p):return course["frames"][round(p*1200)]
for inst in layout["instances"]:
    source=bpy.data.objects[inst["assetId"]+"_LOD0"]
    ob=source.copy();ob.data=source.data;preview.objects.link(ob)
    q=inst["rotation"];r=Quaternion((q[3],*q[:3])).to_matrix()
    ob.matrix_world=(C@r@C.transposed()).to_4x4()
    ob.location=cv(inst["position"]);ob.hide_render=False;ob.hide_viewport=False
def mesh(name,verts,faces,mat):
    me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update()
    ob=bpy.data.objects.new(name,me);preview.objects.link(ob);me.materials.append(mat)
    return ob
def material(name,col,emission=0):
    m=bpy.data.materials.new(name);m.use_nodes=True;p=m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value=(*col,1);p.inputs["Roughness"].default_value=.46
    p.inputs["Emission Color"].default_value=(*col,1);p.inputs["Emission Strength"].default_value=emission
    return m
roadmat=material("Preview road only",(.016,.026,.037))
edge=material("Preview continuous rail only",(.015,.45,.72),2)
stripe=material("Preview road paint",(.17,.24,.27))
for name,left,right,y,mat in [
    ("Actual course surface",-11,11,0,roadmat),
    ("Left edge reference",-11.12,-10.98,.16,edge),("Right edge reference",10.98,11.12,.16,edge),
    ("Left inner road line",-8.5,-8.40,.02,stripe),("Right inner road line",8.4,8.5,.02,stripe)]:
    vertices=[]
    for f in course["frames"]:
        for lateral in [left,right]:
            vertices.append(cv(Vector(f["position"])+Vector(f["right"])*lateral+Vector(f["up"])*y))
    faces=[(i*2,i*2+1,i*2+3,i*2+2) for i in range(len(course["frames"])-1)]
    mesh(name,vertices,faces,mat)
floor=mesh("Unexported ground",[(-2500,-2500,-8),(2500,-2500,-8),(2500,2500,-8),(-2500,2500,-8)],[(0,1,2,3)],roadmat)
# The existing real ship supplies silhouette only in this Blender comparison.
ship_path=ROOT.parents[1]/"UnityProject/Assets/Resources/Art/HeroShip.fbx"
bpy.ops.import_scene.fbx(filepath=str(ship_path))
ship=list(bpy.context.selected_objects)
ship_root=bpy.data.objects.new("Existing craft silhouette preview",None);preview.objects.link(ship_root)
for ob in ship:
    for coll in list(ob.users_collection):coll.objects.unlink(ob)
    preview.objects.link(ob)
    if ob.parent is None:ob.parent=ship_root
    if ob.type=="MESH":
        for i,m in enumerate(ob.data.materials):
            n=m.name.lower() if m else ""
            ident="vrx_mat_mineral_a" if any(k in n for k in ["ivory","paint","porcelain"]) else "vrx_mat_titanium_a"
            if "engine" in n:ident="vrx_mat_cyan_a"
            if "glass" in n or "graphite" in n:ident="vrx_mat_slate_a"
            ob.data.materials[i]=bpy.data.materials[ident]
world=bpy.data.worlds.new("Composition ambient only");scene.world=world;world.use_nodes=True
world.node_tree.nodes["Background"].inputs[0].default_value=(.036,.070,.092,1)
world.node_tree.nodes["Background"].inputs[1].default_value=.8
sun=bpy.data.lights.new("Unexported composition fill","SUN");sun.energy=1.25;sun.angle=math.radians(20)
light=bpy.data.objects.new("Unexported composition fill",sun);preview.objects.link(light)
light.rotation_euler=(.4,-.5,-.5)
data=bpy.data.cameras.new("Static ChaseCamera formula diagnostic");cam=bpy.data.objects.new(data.name,data);preview.objects.link(cam);scene.camera=cam
scene.render.resolution_x=1600;scene.render.resolution_y=900;scene.cycles.samples=12
scene.render.threads_mode="FIXED";scene.render.threads=6
reports=[]
for name,progress,speed in [("entry",.025,120),("turn",.15,245),("exit",.28,180)]:
    f=frame(progress);p=Vector(f["position"]);fw=Vector(f["forward"]);up=Vector(f["up"]);r=Vector(f["right"])
    ratio=speed/380;craft=p+up*1.1
    pos=craft-fw*(10.5+ratio*.75)+up*4.6
    focus=craft+up+fw*(10+ratio*8)
    cam.location=cv(pos)
    # Track up is supplied; this is the settled baseline, without dynamic smoothing/shake.
    z=(cv(pos)-cv(focus)).normalized();x=cv(up).cross(z).normalized();y=z.cross(x).normalized()
    cam.rotation_euler=Matrix((x,y,z)).transposed().to_euler()
    data.type="PERSP";data.sensor_fit="VERTICAL";data.sensor_height=24
    vfov=65+ratio*12;data.lens=24/(2*math.tan(math.radians(vfov)/2))
    rotation=Matrix((r,up,fw)).transposed()
    ship_root.matrix_world=(C@rotation@C.transposed()).to_4x4();ship_root.location=cv(craft)
    scene.render.filepath=str(ROOT/"evidence"/(revision+"-"+name+"-composition.png"))
    bpy.ops.render.render(write_still=True)
    reports.append({"view":name,"progress":progress,"speedForCameraFormulaKph":speed,"verticalFov":vfov,
                    "unityCameraPosition":list(pos),"unityFocus":list(focus),
                    "claim":"Static Blender composition; settled normal-camera formula; no native motion or URP proof"})
(ROOT/"evidence"/(revision+"-composition-cameras.json")).write_text(json.dumps(reports,indent=2)+"\n")
# Keep this isolated Blender scene, outside immutable package and outside Unity.
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/"evidence"/(revision+"-route-preview.blend")))
