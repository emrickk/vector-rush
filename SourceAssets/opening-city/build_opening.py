"""Reproducible original architecture for Vector Rush's Meridian opening.
Run Blender --background --python this-file. Metres, Z-up authoring;
FBX -Z-forward/Y-up with baked axis conversion. No external asset inputs.
The .blend retains individually named construction parts; FBX batches by finish.
"""
import bpy, math, pathlib, json, hashlib
from mathutils import Vector

ROOT = pathlib.Path(__file__).resolve().parent
EXPORT = ROOT.parents[1] / "UnityProject/Assets/Resources/OpeningCity"
EXPORT.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action="SELECT")
bpy.ops.object.delete(use_global=False)
bpy.context.scene.unit_settings.system = "METRIC"
bpy.context.scene.unit_settings.scale_length = 1
M = {}
for name, color, metal, rough in [
    ("OC_Concrete", (.32,.365,.39), .04,.65),
    ("OC_Pale", (.48,.49,.46), .06,.42),
    ("OC_Steel", (.055,.085,.11), .55,.39),
    ("OC_Bronze", (.24,.15,.075), .65,.4),
    ("OC_Glass", (.035,.085,.12), .25,.22),
    ("OC_Rooms", (.22,.17,.11), .1,.5),
    ("OC_Warm", (.85,.49,.19), 0,.35),
    ("OC_Cool", (.35,.55,.64), 0,.35),
]:
    mat=bpy.data.materials.new(name);mat.diffuse_color=(*color,1);mat.use_nodes=True
    p=mat.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value=(*color,1)
    p.inputs["Metallic"].default_value=metal;p.inputs["Roughness"].default_value=rough
    M[name]=mat

parts=[]
collection=None
def box(name, xyz, whd, material="Concrete", bevel=.12):
    # Public coordinates use X horizontal, Y height, Z depth like Unity.
    x,y,z=xyz; w,h,d=whd
    bpy.ops.mesh.primitive_cube_add(size=1,location=(x,-z,y))
    o=bpy.context.object;o.name=name;o.dimensions=(w,d,h)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    o.data.materials.append(M["OC_"+material])
    if bevel:
        mod=o.modifiers.new("Cast edge return","BEVEL");mod.width=min(bevel,min(w,h,d)*.22);mod.segments=2
        mod.affect="EDGES"
        bpy.ops.object.modifier_apply(modifier=mod.name)
        normals=o.modifiers.new("Weighted corner normals","WEIGHTED_NORMAL")
        bpy.ops.object.modifier_apply(modifier=normals.name)
    for c in list(o.users_collection):c.objects.unlink(o)
    collection.objects.link(o);parts.append(o)
    return o

def beam(name,a,b,width,material="Steel"):
    av=Vector((a[0],-a[2],a[1]));bv=Vector((b[0],-b[2],b[1]))
    o=box(name,(0,0,0),(width,(bv-av).length,width),material,.06)
    o.location=(av+bv)/2
    o.rotation_euler=(bv-av).to_track_quat("Z","Y").to_euler()
    return o

def tower(name,cx,base,height,w,d,style=0):
    # Continuous recessed glazed shaft with real spandrels and corner piers.
    box(name+" recessed occupied shaft",(cx,base+height/2,0),(w-2,height,d-2),"Rooms",.15)
    for side in [-1,1]:
        for edge in [-1,1]:
            box(name+" corner pier",(cx+side*(w/2-.6),base+height/2,edge*(d/2-.6)),(1.3,height,1.3),"Pale",.14)
        # Deeper sidewall shear blade breaks the four identical glass faces.
        box(name+" service blade",(cx+side*(w/2+.25),base+height/2,-d*.16),(1.4,height,d*.28),"Concrete",.12)
    floors=int(height/4)
    for level in range(floors+1):
        y=base+level*4
        box(name+" floor return %02d"%level,(cx,y,0),(w+.4,.38,d+.4),"Concrete",.06)
        if level%6==0:
            box(name+" mechanical belt",(cx,y+.7,0),(w+1.2,1.0,d+1.2),"Steel",.1)
            for side in [-1,1]:
                box(name+" inset belt light",(cx,y+.26,side*(d/2+.64)),(w-2,.14,.12),"Warm",.025)
    for side in [-1,1]:
        for i in range(1,int(w/4)):
            x=cx-w/2+i*4
            box(name+" vertical bronze mullion",(x,base+height/2,side*(d/2-.32)),(.15,height,.3),"Bronze",.015)
    box(name+" deep roof coping",(cx,base+height+.8,0),(w+2,1.5,d+2),"Pale",.16)
    box(name+" recessed plant penthouse",(cx,base+height+3,-2),(w*.58,4,d*.62),"Steel",.18)
    for i in range(5):
        box(name+" roof cooling louvre",(cx-w*.25+i*w*.125,base+height+5.2,-2),(.4,.3,d*.54),"Concrete",.03)

def podium(name,w,d,h):
    box(name+" foundation",(0,1,0),(w+5,2,d+5),"Concrete",.3)
    box(name+" continuous podium",(0,h/2+1,0),(w,h,d),"Steel",.35)
    for level in [4,h]:
        box(name+" stone belt",(0,level,0),(w+2,.9,d+2),"Concrete",.16)
    for side in [-1,1]:
        box(name+" entrance glazing",(0,5.6,side*(d/2+.05)),(w-6,4,.2),"Rooms",.03)
        for x in range(-int(w/2)+3,int(w/2),6):
            box(name+" arcade pier",(x,h/2+1,side*(d/2+.6)),(1.1,h,2.1),"Concrete",.1)
        box(name+" canopy",(0,8.5,side*(d/2+2)),(w+3,.8,7),"Steel",.16)
        box(name+" canopy soffit diffuser",(0,8.04,side*(d/2+3)),(w-5,.12,.7),"Warm",.025)

def meridian():
    podium("Meridian civic concourse",64,44,12)
    tower("West observation tower",-18,12,126,22,28)
    tower("East exchange tower",18,12,94,19,25)
    # A legible occupied bridge, set against the sky gap between unequal towers.
    box("Sky hall structural tray",(0,99,0),(61,3,31),"Pale",.4)
    box("Sky hall recessed occupied rooms",(0,104,0),(55,7,26),"Rooms",.18)
    box("Sky hall projecting roof",(0,108.2,0),(64,1.2,34),"Steel",.22)
    for side in [-1,1]:
        for x in range(-26,27,4):
            box("Sky hall frame",(x,104,side*13.4),(.45,8,1),"Bronze",.06)
        box("Sky hall warm reveal",(0,99.8,side*15.55),(49,.25,.16),"Warm",.035)
    for sign in [-1,1]:
        beam("Sky hall diagonal transfer",(-7*sign,92,0),(8*sign,98,0),1.1)
    box("Observation crown blade",(-24,146,-2),(3,17,22),"Pale",.25)
    box("Observation crown second blade",(-12,141,-2),(2,7,20),"Concrete",.18)
    box("Aviation light",(-24,154.7,-2),(.55,.35,.55),"Warm",.04)
    # Grade-level steps terminate against the podium, not floating decoration.
    for s in range(5):
        box("Concourse broad stairs",(0,.3+s*.36,28-s*.9),(26,.6+s*.72,2),"Concrete",.05)

def transit():
    podium("Lower city interchange",94,50,9)
    # Three stepped occupied stories with usable roof terraces.
    for level,(w,d) in enumerate([(88,43),(72,35),(50,28)]):
        y=10+level*7
        box("Recessed concourse level",(0,y+2.6,-level*3),(w-3,5.3,d-3),"Rooms",.12)
        box("Projecting roof terrace",(0,y+5.5,-level*3),(w+3,.85,d+4),"Concrete",.22)
        for side in [-1,1]:
            box("Terrace shadow fascia",(0,y+5.08,-level*3+side*(d/2+1.6)),(w+2,.5,.5),"Steel",.07)
            for x in range(-int(w/2)+3,int(w/2),7):
                box("Concourse stone pier",(x,y+2.6,-level*3+side*(d/2-.35)),(.8,5.4,1.3),"Pale",.09)
            box("Concourse warm soffit",(0,y+4.7,-level*3+side*(d/2+.3)),(w-5,.14,.5),"Warm",.025)
    for x in [-36,36]:
        box("Stair and lift enclosure",(x,15,-8),(9,28,18),"Concrete",.25)
        box("Stair glazing slit",(x,15,1.2),(5,23,.25),"Glass",.025)
        for y in range(5,28,4):
            box("Stair landing",(x,y,1.5),(6,.5,1),"Steel",.04)
    for x in [-26,-13,0,13,26]:
        box("Roof ventilation cowling",(x,26,-12),(7,2,5),"Steel",.2)
    for side in [-1,1]:
        box("Service loading apron",(0,.25,side*32),(100,.5,13),"Concrete",.15)

def offices():
    podium("Riverside office base",45,35,9)
    tower("Riverside low wing",-10,9,42,19,24)
    tower("Riverside stepped wing",11,9,66,17,25)
    box("Shared roof room",(0,50,0),(43,5,28),"Steel",.2)
    for s in [-1,1]:
        box("Shared roof room occupied strip",(0,50,s*14.1),(35,2,.2),"Rooms",.025)

def utility():
    podium("Canal works",50,36,8)
    box("Upper service hall",(0,13,0),(42,9,30),"Concrete",.25)
    for side in [-1,1]:
        box("Continuous clerestory",(0,15,side*15.15),(35,3,.3),"Rooms",.04)
        for i in range(-3,4):
            box("Cast buttress",(i*6,9,side*16.1),(1.3,17,2.4),"Pale",.14)
    box("Service hall roof",(0,18,0),(46,1,34),"Steel",.22)
    for i in range(-2,3):
        box("Raised roof sawtooth",(i*8,20,0),(5,3,22),"Steel",.16)
        box("Northlight glazing",(i*8,20,-11.15),(4,2,.2),"Glass",.025)

recipes={"MeridianExchange":meridian,"TransitTerraces":transit,"RiversideOffices":offices,"CanalWorks":utility}
records=[]
for name,recipe in recipes.items():
    collection=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(collection)
    parts=[];recipe()
    bpy.ops.object.select_all(action="DESELECT")
    # Keep named source parts; export merged copies with one submesh per finish.
    copies=[]
    for p in parts:
        c=p.copy();c.data=p.data.copy();bpy.context.scene.collection.objects.link(c);copies.append(c);c.select_set(True)
    bpy.context.view_layer.objects.active=copies[0];bpy.ops.object.join()
    merged=bpy.context.object;merged.name=name
    bpy.context.scene.cursor.location=(0,0,0)
    bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
    out=EXPORT/(name+".fbx")
    bpy.ops.export_scene.fbx(filepath=str(out),use_selection=True,object_types={"MESH"},apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",bake_space_transform=True,axis_forward="-Z",axis_up="Y",
        mesh_smooth_type="FACE",use_tspace=False,add_leaf_bones=False,bake_anim=False)
    records.append({"name":name,"parts":len(parts),"vertices":len(merged.data.vertices),
                    "triangles":sum(len(p.vertices)-2 for p in merged.data.polygons),
                    "fbxSha256":hashlib.sha256(out.read_bytes()).hexdigest()})
    bpy.data.objects.remove(merged,do_unlink=True)
    collection.hide_viewport=True;collection.hide_render=True
bpy.data.collections["MeridianExchange"].hide_viewport=False
bpy.context.scene["Provenance"]="Original geometry authored for Vector Rush. No third-party assets."
bpy.context.scene["Runtime"]="One local metre equals one Unity metre; Unity binds OC_* finish slots."
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/"MeridianOpening.blend"))
(ROOT/"asset-provenance.json").write_text(json.dumps({"generator":"build_opening.py","units":"metres","assets":records},indent=2))
print(json.dumps(records,indent=2))
