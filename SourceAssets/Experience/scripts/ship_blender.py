"""Kestrel coating preview/source plus a true three-dimensional plume shell.
The retained ship mesh is imported from the existing FBX, never remodelled.
Blender --background --python this-file
"""
import bpy,math,json,hashlib,shutil
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parents[1];REPO=ROOT.parents[1];OUT=ROOT/"packages/vrx-c01"
if (OUT/"READY.json").exists():raise RuntimeError("Immutable revision")
(OUT/"source").mkdir(exist_ok=True);(OUT/"meshes").mkdir(exist_ok=True)
bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;bpy.context.preferences.filepaths.save_version=0
scene.unit_settings.system="METRIC";scene.render.engine="CYCLES";scene.cycles.samples=20;scene.cycles.use_denoising=True
scene.render.threads_mode="FIXED";scene.render.threads=6;scene.view_settings.view_transform="AgX"
bpy.ops.import_scene.fbx(filepath=str(REPO/"UnityProject/Assets/Resources/Art/HeroShip.fbx"))
ship=list(bpy.context.selected_objects)
coatings={};engine_materials=[]
for ob in ship:
    if ob.type!="MESH":continue
    for mat in ob.data.materials:
        if mat is None or mat.name in coatings:continue
        name=mat.name.lower()
        key=next((k for k in ["Ivory","Graphite","Metal","Ceramic"] if k.lower() in name),None)
        mat.use_nodes=True;nodes=mat.node_tree.nodes;links=mat.node_tree.links;nodes.clear()
        output=nodes.new("ShaderNodeOutputMaterial");p=nodes.new("ShaderNodeBsdfPrincipled");links.new(p.outputs[0],output.inputs[0])
        if key:
            ident="vrx_ship_"+key.lower()+"_a"
            p.inputs["Roughness"].default_value=.48 if key=="Ivory" else .39
            p.inputs["Metallic"].default_value=.70 if key=="Metal" else .08
            for suffix,socket,noncolour in [("base","Base Color",False),("normal",None,True)]:
                tex=nodes.new("ShaderNodeTexImage");tex.image=bpy.data.images.load(str(OUT/"ship"/(ident+"_"+suffix+".png")))
                if noncolour:tex.image.colorspace_settings.name="Non-Color"
                tex.image.pack()
                if socket:links.new(tex.outputs["Color"],p.inputs[socket])
                else:
                    norm=nodes.new("ShaderNodeNormalMap");links.new(tex.outputs["Color"],norm.inputs["Color"]);links.new(norm.outputs[0],p.inputs["Normal"])
            energy=nodes.new("ShaderNodeTexImage");energy.name="Authored boost energy";energy.image=bpy.data.images.load(str(OUT/"ship"/(ident+"_energy.png")))
            energy.image.colorspace_settings.name="Non-Color";energy.image.pack()
            split=nodes.new("ShaderNodeSeparateColor");links.new(energy.outputs["Color"],split.inputs[0])
            floor=nodes.new("ShaderNodeMath");floor.operation="MULTIPLY";floor.inputs[1].default_value=.45/3.4
            links.new(split.outputs[1],floor.inputs[0])
            total=nodes.new("ShaderNodeMath");total.operation="ADD"
            links.new(split.outputs[0],total.inputs[0]);links.new(floor.outputs[0],total.inputs[1])
            rim=nodes.new("ShaderNodeFresnel");rim.inputs["IOR"].default_value=1.28
            rimScale=nodes.new("ShaderNodeMath");rimScale.operation="MULTIPLY";rimScale.inputs[1].default_value=1.8/3.4
            links.new(rim.outputs[0],rimScale.inputs[0])
            add=nodes.new("ShaderNodeMath");add.operation="ADD";links.new(total.outputs[0],add.inputs[0]);links.new(rimScale.outputs[0],add.inputs[1])
            mix=nodes.new("ShaderNodeMixRGB");mix.blend_type="MULTIPLY";mix.inputs[0].default_value=1;mix.inputs[2].default_value=(.035,.62,1,1)
            links.new(add.outputs[0],mix.inputs[1]);links.new(mix.outputs[0],p.inputs["Emission Color"])
            p.inputs["Emission Strength"].default_value=0
            coatings[mat.name]=p
        else:
            p.inputs["Base Color"].default_value=(.023,.055,.08,1);p.inputs["Metallic"].default_value=.42;p.inputs["Roughness"].default_value=.27
            if "engine" in name:
                p.inputs["Emission Color"].default_value=(.035,.58,1,1);p.inputs["Emission Strength"].default_value=1.4
                engine_materials.append(p)
# Geometry is a longitudinal surface with 32 radial segments, not crossed cards.
verts=[];faces=[];rings=13;segments=32
for i in range(rings):
    t=i/(rings-1)
    radius=(.20+.35*math.sin(math.pi*t)**.8)*(1-t*.75)
    for j in range(segments+1):
        theta=j/segments*math.tau
        # Blender +Y is rearward, yielding Unity -Z behind nozzle.
        verts.append((radius*math.cos(theta),t*4,radius*math.sin(theta)))
for i in range(rings-1):
    for j in range(segments):
        a=i*(segments+1)+j;faces.append((a,a+segments+1,a+segments+2,a+1))
me=bpy.data.meshes.new("vrx_vfx_volume_a");me.from_pydata(verts,[],faces);me.update()
assert all(p.normal.dot(Vector((p.center.x,0,p.center.z)))>0 for p in me.polygons),"Shell normals must face outward"
volume=bpy.data.objects.new("vrx_vfx_volume_a",me);scene.collection.objects.link(volume)
uv=me.uv_layers.new(name="UV0_Longitudinal")
for p in me.polygons:
    for li in p.loop_indices:
        vi=me.loops[li].vertex_index;i,j=divmod(vi,segments+1)
        uv.data[li].uv=(i/(rings-1),j/segments)
    p.use_smooth=True
colour=me.color_attributes.new(name="Colour",type="FLOAT_COLOR",domain="POINT")
for i in range(len(verts)):
    t=verts[i][1]/4
    colour.data[i].color=(1,1,1,(1-t)**1.4)
bpy.ops.object.select_all(action="DESELECT");volume.select_set(True);bpy.context.view_layer.objects.active=volume
bpy.ops.export_scene.fbx(filepath=str(OUT/"meshes/vrx_vfx_volume_a.fbx"),use_selection=True,object_types={"MESH"},
    apply_unit_scale=True,apply_scale_options="FBX_SCALE_UNITS",bake_space_transform=True,
    axis_forward="-Z",axis_up="Y",mesh_smooth_type="FACE",use_tspace=True,add_leaf_bones=False)
volume.hide_render=True
contract={"id":"vrx_vfx_volume_a","path":"meshes/vrx_vfx_volume_a.fbx","type":"vfx-mesh","revision":1,
    "source":"Original Astra longitudinal shell; ship_blender.py","license":"Project-authored",
    "triangles":len(faces)*2,"vertices":len(verts),"pivot":"nozzle exit at origin",
    "unityBoundsMin":[min(v[0] for v in verts),min(v[2] for v in verts),-4],
    "unityBoundsMax":[max(v[0] for v in verts),max(v[2] for v in verts),0],
    "outwardSideNormalsVerified":True,
    "uv":"U 0 nozzle to 1 tail; V 0..1 around circumference, duplicated seam. This is a shell UV, not billboard width.",
    "vertexColor":"RGB one; A longitudinal (1-U)^1.4 fade",
    "shaderRequest":"Sample a separate scalar longitudinal/circumferential field or map body atlas V by view-facing normal. Do not wrap the flat body atlas around the tube without adapting its cross-section.",
    "normals":"smooth outward sides, open ends intentionally; soft alpha and depth intersection fade required",
    "import":{"normals":"import","tangents":"calculateMikk","collider":"none","scale":1},
    "usage":"Optional core/body volume foundation. Bind to actual engine exit; preserve existing craft mesh."}
(OUT/"volume-contract.json").write_text(json.dumps(contract,indent=2)+"\n")
assets=json.loads((OUT/"experience-assets.json").read_text())
assets["assets"]=[a for a in assets["assets"] if a["id"]!=contract["id"]]+[contract]
(OUT/"experience-assets.json").write_text(json.dumps(assets,indent=2)+"\n")
world=bpy.data.worlds.new("Neutral coating studio");scene.world=world;world.use_nodes=True
world.node_tree.nodes["Background"].inputs[0].default_value=(.15,.19,.23,1);world.node_tree.nodes["Background"].inputs[1].default_value=.45
def area(loc,power,size):
    data=bpy.data.lights.new("Studio softbox","AREA");data.energy=power;data.size=size
    ob=bpy.data.objects.new("Studio softbox",data);scene.collection.objects.link(ob);ob.location=loc
    ob.rotation_euler=(Vector((0,0,0))-ob.location).to_track_quat("-Z","Y").to_euler()
area((-5,3,7),900,6);area((4,-2,6),700,5)
data=bpy.data.cameras.new("Coating rear-quarter");cam=bpy.data.objects.new(data.name,data);scene.collection.objects.link(cam)
cam.location=(7,11,6);cam.rotation_euler=(Vector((0,0,.1))-cam.location).to_track_quat("-Z","Y").to_euler()
data.type="ORTHO";data.ortho_scale=10.7;scene.camera=cam
scene.render.resolution_x=1400;scene.render.resolution_y=1000
shutil.copy2(__file__,OUT/"ship_blender_recipe.py")
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"source/vrx_ship_finish.blend"))
for name,power in [("cruise",0),("boost",3.4)]:
    for node in coatings.values():node.inputs["Emission Strength"].default_value=power
    scene.render.filepath=str(ROOT/"evidence"/("vrx-c01-ship-"+name+".png"));bpy.ops.render.render(write_still=True)
print("SHIP_SOURCE_AND_VOLUME_READY",len(coatings),flush=True)
