"""Astra's original city kit, LOD exports and course-based composition.
Blender --background --python this-file -- a01
Blender --background --python this-file -- b01
Run through tools/opening-city-run.py. Never edits a Unity scene or C#.
"""
import bpy, bmesh, sys, json, math, shutil, hashlib, random
from pathlib import Path
from mathutils import Vector, Matrix

ROOT=Path(__file__).resolve().parents[1]
revision="vrx-"+sys.argv[sys.argv.index("--")+1]
OUT=ROOT/"packages"/revision
if (OUT/"READY.json").exists():raise RuntimeError("Published revision is immutable")
if not (OUT/"material-records.json").exists():raise RuntimeError("Run package_art.py prepare first")
EXPANDED="b" in revision.split("-")[-1]
HASH="598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059"
for name in ["meshes","source"]:(OUT/name).mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene
bpy.context.preferences.filepaths.save_version=0
scene.unit_settings.system="METRIC";scene.unit_settings.scale_length=1
scene.render.engine="CYCLES";scene.cycles.device="CPU";scene.cycles.samples=12
scene.cycles.use_denoising=True;scene.render.threads_mode="FIXED";scene.render.threads=6
scene.render.resolution_x=1600;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.view_settings.view_transform="AgX"
M={}
material_records=json.loads((OUT/"material-records.json").read_text())
for rec in material_records:
    key=rec["id"].split("_")[2]
    m=bpy.data.materials.new(rec["id"]);m.use_nodes=True
    p=m.node_tree.nodes.get("Principled BSDF")
    p.inputs["Base Color"].default_value=(*rec["baseReflectanceLinear"],1)
    p.inputs["Metallic"].default_value=rec["artisticMetallic"]
    p.inputs["Roughness"].default_value=rec["artisticRoughness"]
    m.diffuse_color=(*rec["baseReflectanceLinear"],1)
    if rec["emission"]:
        p.inputs["Emission Color"].default_value=(*rec["emissionColorLinear"],1)
        p.inputs["Emission Strength"].default_value=rec["emissionIntensity"]
        if key in ("occupied","officeglass","hotelglass","serviceglass"):
            tex=m.node_tree.nodes.new("ShaderNodeTexImage")
            tex.image=bpy.data.images.load(str(OUT/rec["emission"]));tex.image.pack()
            # Correct four-metre UV scale from the runtime material contract.
            uv=m.node_tree.nodes.new("ShaderNodeTexCoord")
            mapping=m.node_tree.nodes.new("ShaderNodeVectorMath");mapping.operation="SCALE"
            mapping.inputs[3].default_value=.25
            m.node_tree.links.new(uv.outputs["UV"],mapping.inputs[0])
            m.node_tree.links.new(mapping.outputs[0],tex.inputs["Vector"])
            mix=m.node_tree.nodes.new("ShaderNodeMixRGB");mix.blend_type="MULTIPLY";mix.inputs[0].default_value=1
            mix.inputs[2].default_value=(*rec["emissionColorLinear"],1)
            m.node_tree.links.new(tex.outputs["Color"],mix.inputs[1])
            m.node_tree.links.new(mix.outputs[0],p.inputs["Emission Color"])
    M[key]=m

parts=[];collection=None;LOD=0;counter=0
def unity(v): return Vector((v[0],-v[2],v[1]))
def primitive(name,verts,faces,mat,detail=False):
    global counter
    if detail and LOD: return None
    counter+=1
    me=bpy.data.meshes.new(name);me.from_pydata([unity(v) for v in verts],[],faces);me.update()
    ob=bpy.data.objects.new(name,me);collection.objects.link(ob);ob.data.materials.append(M[mat]);parts.append(ob)
    return ob
def box(name,loc,size,mat="slate",detail=False,bevel=0):
    x,y,z=loc;a,b,c=[q*.5 for q in size]
    verts=[(x+sx*a,y+sy*b,z+sz*c) for sx,sy,sz in
           [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
    ob=primitive(name,verts,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],mat,detail)
    if ob and bevel and not LOD:
        mod=ob.modifiers.new("Formed edge radius","BEVEL");mod.width=bevel;mod.segments=2
    return ob
def beam(name,a,b,width,mat="titanium",detail=False):
    ob=box(name,(0,0,0),(width,(Vector(b)-Vector(a)).length,width),mat,detail)
    if ob:
        ob.location=(unity(a)+unity(b))/2
        ob.rotation_euler=(unity(b)-unity(a)).to_track_quat("Z","Y").to_euler()
    return ob
def hall(name,cx,cy,cz,w,h,d,mat="occupied"):
    box(name+" recessed rooms",(cx,cy+h/2,cz),(w-.7,h,d-.7),mat)
    box(name+" opaque structural soffit",(cx,cy,cz),(w+1,.65,d+1),"slate")
    box(name+" opaque roof cap",(cx,cy+h,cz),(w+1,.65,d+1),"titanium")
    for sx in [-1,1]:
        for sz in [-1,1]:
            box(name+" corner reveal",(cx+sx*w/2,cy+h/2,cz+sz*d/2),(.65,h,.65),"mineral")
    for k in range(0,int(h),6):
        box(name+" continuous floor return",(cx,cy+k+.4,cz),(w+1,.6,d+1),"slate",detail=k%12!=0)
    for side in [-1,1]:
        box(name+" crown edge",(cx,cy+h,cz+side*d/2),(w,.38,.38),"mineral")
def base(w,d,h=6):
    box("Grounded mineral foundation",(0,1,0),(w+2,2,d+2),"mineral",bevel=.12)
    box("Podium occupied body",(0,h/2+1,0),(w,h,d),"slate")
    for s in [-1,1]:
        box("Arcade recess",(0,h*.50+1,s*(d/2+.05)),(w-3,h*.6,.20),"occupied")
        for x in range(-int(w*.5)+2,int(w*.5),6):
            box("Arcade pier",(x,h*.5+1,s*(d/2+.55)),(.70,h,1.1),"mineral")
        box("Supported canopy",(0,h+1,s*(d/2+.8)),(w+1,.8,3.6),"titanium",bevel=.06)
        box("Recessed canopy light",(0,h+.53,s*(d/2+1)),(w-2,.12,.30),"cyan")
def commercial(variant=0):
    if not variant:
        base(31,25,8)
        hall("Recessed exchange shaft",-4,9,0,20,47,19,"officeglass")
        box("Asymmetric structural blade",(-13,43,2),(4,68,20),"mineral",bevel=.12)
        hall("Cantilevered occupied trading head",3,55,0,35,22,26,"officeglass")
        box("Crown shadow gap",(3,78,0),(35,1.8,26),"slate")
        box("Lipped titanium crown",(3,80,0),(37,1.2,28),"titanium",bevel=.10)
        for z in [-10,0,10]:
            beam("Visible transfer knee",(-11,48,z),(11,55,z),1.0)
        for x in range(-11,20,5):
            box("Head vertical fin",(x,66,-13.65),(.36,22,1.2),"mineral",detail=True)
        box("One recessed cyan address",(19.1,69,-2),(.15,10,1.1),"cyan")
    else:
        base(39,29,9)
        hall("Offset north volume",-8,10,-3,18,62,21,"officeglass")
        hall("Low occupied south volume",11,10,4,17,38,26,"officeglass")
        box("Dorsal concrete spine",(-7,52,5),(3.2,85,5),"mineral")
        hall("Set-back roof observatory",-8,73,-3,15,12,18,"officeglass")
        box("Long connecting mezzanine",(1,37,0),(38,2,26),"titanium")
        box("Recessed advertising blade",(20,30,-7),(.5,18,3),"magenta")
        for z in [-11,10]:box("Occupied deck rim",(1,38,z),(37,.3,.3),"cyan")
def hotel(variant=0):
    base(22,22,7)
    if not variant:
        hall("Hotel east slender bar",5,8,-2,9,106,17,"hotelglass")
        hall("Hotel west slender bar",-6,8,3,8,83,14,"hotelglass")
        box("Recessed circulation spine",(-.7,60,2),(3,105,8),"slate")
        box("Offset crown fin",(8,118,0),(2,13,23),"mineral",bevel=.08)
        box("Crown blue slit",(9.05,117,-2),(.12,8,8),"cyan")
        for y in [31,59,86]:box("Unequal occupied connecting level",(-1,y,0),(19,2,20),"titanium")
    else:
        hall("Taper base occupied bar",0,8,0,17,55,18,"hotelglass")
        hall("Set-back upper lodging",3,64,1,11,38,15,"hotelglass")
        box("Blade connecting both volumes",(-8,54,4),(2.2,98,8),"mineral")
        box("Slanted crown blade",(4,105,2),(14,3,18),"titanium")
        for y in [18,34,49,73,88]:
            box("Balcony shade",(3,y,-10),(14,.7,3.0),"mineral",detail=True)
        box("Vertical hotel address",(-9.15,72,-2),(.18,23,1.5),"magenta")
def services(variant=0):
    base(37,31,7)
    for k in range(4):
        w=32-k*5;d=27-k*4;x=-k*1.6
        hall("Receding occupied terrace",x,8+k*10,k*1.5,w,9,d,"serviceglass")
        box("Terrace deep lip",(x,17.3+k*10,k*1.5),(w+1.8,.7,d+1.8),"mineral",bevel=.05)
        for z in [-1,1]:
            box("Terrace low parapet",(x,18.1+k*10,k*1.5+z*(d*.5+.3)),(w+1.6,.8,.5),"slate")
        if not LOD and k<3:
            for j in range(3):box("Rooftop service cabinet",(x-5+j*4.2,18.3+k*10,k*1.5),(2.4,1.6,3),"titanium")
    box("External circulation block",(15,24,7),(5,43,9),"slate")
    box("Service address",(17.55,27,7),(.15,11,1.1),"cyan")
    if variant:
        box("Offset delivery wing",(-23,11,7),(10,20,23),"mineral")
        for y in [7,14]:box("Wing mechanical louvre",(-28.1,y,7),(.2,2,16),"slate")
def industrial(variant=0):
    base(40,32,6)
    hall("Transit works occupied hall",0,7,0,38,15,30,"glass")
    for k in range(5):
        x=-19+k*7.6
        primitive("Sawtooth folded roof",[
            (x,22,-16),(x+6.4,28,-16),(x+7.6,22,-16),
            (x,22,16),(x+6.4,28,16),(x+7.6,22,16)],
            [(0,2,1),(3,4,5),(0,1,4,3),(1,2,5,4),(2,0,3,5)],"mineral")
        box("Clerestory glazing",(x+6.45,25,0),(.2,4,29),"occupied")
    for z in [-1,1]:
        for x in [-18,-9,0,9,18]:
            box("External hall buttress",(x,13,z*16),(1.2,24,1.4),"slate")
            beam("Hall diagonal tie",(x,8,z*16.8),(x+4,21,z*16.8),.24,detail=True)
    box("Asymmetric services chimney",(22,21,6),(5,40,9),"slate")
    box("Chimney crown",(22,41.5,6),(6,1.0,10),"titanium")
    if variant:
        box("Low delivery annex",(-25,6,4),(10,10,25),"mineral")
        box("Annex sectional door",(-30.1,5,4),(.15,7,15),"titanium")
def podium():base(22,18,9)
def guidance():
    # Local +Z runs along track. Ground-centre pivot, placement is banked.
    box("Recessed guidance mount",(0,.22,0),(.44,.44,2.8),"slate",bevel=.025)
    box("Continuous short diffuser",(0,.46,0),(.38,.10,2.5),"cyan")
    for z in [-.80,0,.80]:
        beam("Arrow leg left",(-.21,.52,z-.16),(0,.52,z+.10),.09,"cyan")
        beam("Arrow leg right",(.21,.52,z-.16),(0,.52,z+.10),.09,"cyan")
    for z in [-1,1]:box("Support shoe",(0,.08,z),(.75,.16,.35),"titanium")
def landmark():
    base(36,29,10)
    hall("Landmark occupied base",0,11,0,28,40,23)
    for s,h in [(-1,137),(1,154)]:
        box("Tuning fork mineral blade",(s*12,11+h/2,0),(7,h,17),"mineral",bevel=.12)
        box("Recessed energy inlay",(s*8.4,80,0),(.16,76,10),"cyan")
        for y in range(20,140,18):
            box("Blade mechanical interruption",(s*12,y,0),(7.3,1.8,17.3),"slate")
    box("Open crown lintel",(0,141,0),(28,4,16),"titanium")
    box("Occupied sky room",(0,97,1),(29,7,14),"occupied")
    box("Sky room soffit",(0,92.5,1),(31,1.2,16),"slate")
def canopy():
    for z in [-12,-4,5,13]:
        box("Column founded foot",(3,.3,z),(2,.6,2),"mineral")
        box("Transit column",(3,5.1,z),(.65,9.5,.7),"mineral")
        beam("Canopy structural knee",(3,6,z),(-4,10,z),.40)
    box("Folded transit canopy",(-.7,10.3,0),(12,.65,29),"mineral",bevel=.06)
    box("Canopy recessed luminous edge",(-6.6,10.15,0),(.16,.18,28),"cyan")
    box("Occupied information kiosk",(3,2,6),(3,4,6),"occupied")
def bridge():
    for z in [-11,11]:
        box("Connector grounded bearing foot",(0,.5,z),(9,1,4),"mineral")
        for x in [-2.7,2.7]:
            box("Connector support pier",(x,6.5,z),(1.4,12,2),"mineral")
        beam("Connector bracing knee",(-2.7,8,z),(2.7,12,z),.48)
    box("Deep connector tray",(0,13.1,0),(8,2.2,28),"slate",bevel=.1)
    hall("Occupied connector",0,14.2,0,7,5,27)
    box("Connector roof",(0,19.8,0),(8.6,.9,29),"mineral")
    for s in [-1,1]:
        for z in [-11,-4,3,10]:
            beam("Exposed bridge diagonal",(s*4,14,z),(s*4,19,z+4),.23)
def plant():
    box("Screened plant curb",(0,.3,0),(7,.6,10),"slate")
    for z in [-2.5,2.5]:
        box("Heat exchanger casing",(0,1.5,z),(5.5,2.1,3.5),"titanium",bevel=.08)
        for x in [-2,-1,0,1,2]:box("Plant louvre",(x,2.61,z),(.38,.13,3.1),"slate",detail=True)
    for x in [-3,3]:box("Plant screen",(x,1.5,0),(.25,2.8,9),"mineral")
def advertisement():
    for x in [-3.5,3.5]:box("Sign founded post",(x,4,0),(.55,8,.65),"titanium")
    box("Sign rear housing",(0,6,0),(10,4.8,.85),"slate",bevel=.05)
    box("Inset graphic surface",(0,6,-.45),(9.4,4.1,.06),"magenta")
    box("Sign white header",(0,7.55,-.49),(8.5,.18,.05),"white")
    for x in [-3,-1,1,3]:box("Original emblem bars",(x,5.8,-.5),(.6,1.6,.05),"cyan")

builders=[
 ("vrx_city_exchange_a",commercial),("vrx_city_hotel_a",hotel),
 ("vrx_city_services_a",services),("vrx_city_works_a",industrial),
 ("vrx_city_podium_a",podium),("vrx_track_guidance_a",guidance)]
if EXPANDED:
    builders += [
        ("vrx_city_exchange_b",lambda:commercial(1)),("vrx_city_hotel_b",lambda:hotel(1)),
        ("vrx_city_services_b",lambda:services(1)),("vrx_city_works_b",lambda:industrial(1)),
        ("vrx_city_landmark_a",landmark),("vrx_city_canopy_a",canopy),
        ("vrx_city_bridge_a",bridge),("vrx_city_plant_a",plant),("vrx_city_advert_a",advertisement)]

assets=[];stats={};exports={};proxies=[]
for ident,build in builders:
    record={"id":ident,"pivot":"ground-center","trackAssembly":ident.startswith("vrx_track"),
            "collider":"none","lightmapUVs":"generate"}
    lodstats=[]
    for LOD in [0,1]:
        collection=bpy.data.collections.new(ident+"_LOD"+str(LOD));scene.collection.children.link(collection)
        parts=[];build()
        for ob in parts:
            bpy.ops.object.select_all(action="DESELECT");ob.select_set(True);bpy.context.view_layer.objects.active=ob
            for mod in list(ob.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
            bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
        bpy.ops.object.select_all(action="DESELECT")
        for ob in parts:ob.select_set(True)
        bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();ob=bpy.context.object;ob.name=ident+"_LOD"+str(LOD)
        # Keep slot order identical between both LODs even for detail-only slots.
        used=[]
        for m in ob.data.materials:
            if m not in used:used.append(m)
        if LOD==0:slot_names=[m.name for m in used]
        else:
            slot_names=record["materialSlots"]
            used=[bpy.data.materials[name] for name in slot_names]
        faces=[used.index(ob.data.materials[p.material_index]) for p in ob.data.polygons]
        ob.data.materials.clear()
        for m in used:ob.data.materials.append(m)
        for p,k in zip(ob.data.polygons,faces):p.material_index=k
        bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type="ORIGIN_CURSOR")
        # Applied box meshes form closed disconnected solids. Recalculate outward normals.
        bm=bmesh.new();bm.from_mesh(ob.data);bmesh.ops.recalc_face_normals(bm,faces=bm.faces);bm.to_mesh(ob.data);bm.free()
        tri=ob.modifiers.new("Export triangles","TRIANGULATE");bpy.ops.object.modifier_apply(modifier=tri.name)
        uv=ob.data.uv_layers.new(name="UV0_MetricMetres")
        for p in ob.data.polygons:
            axis=max(range(3),key=lambda k:abs(p.normal[k]));axes=[k for k in range(3) if k!=axis]
            for li in p.loop_indices:
                co=ob.data.vertices[ob.data.loops[li].vertex_index].co
                uv.data[li].uv=(co[axes[0]],co[axes[1]])
        bm=bmesh.new();bm.from_mesh(ob.data)
        nonmanifold=sum(not e.is_manifold for e in bm.edges);bm.free()
        degenerate=sum(p.area<1e-10 for p in ob.data.polygons)
        if nonmanifold or degenerate:raise RuntimeError(ident+" invalid topology")
        coords=[(v.co.x,v.co.z,-v.co.y) for v in ob.data.vertices]
        lo=[min(v[i] for v in coords) for i in range(3)];hi=[max(v[i] for v in coords) for i in range(3)]
        file="meshes/"+ob.name+".fbx"
        bpy.ops.export_scene.fbx(filepath=str(OUT/file),use_selection=True,object_types={"MESH"},
            apply_unit_scale=True,apply_scale_options="FBX_SCALE_UNITS",bake_space_transform=True,
            axis_forward="-Z",axis_up="Y",mesh_smooth_type="FACE",use_tspace=True,add_leaf_bones=False)
        record["lod"+str(LOD)]=file
        if LOD==0:
            record.update(boundsMin=lo,boundsMax=hi,materialSlots=slot_names)
            exports[ident]=ob
        lodstats.append({"lod":LOD,"triangles":len(ob.data.polygons),"vertices":len(ob.data.vertices),
                         "nonmanifoldEdges":nonmanifold,"degenerateTriangles":degenerate,
                         "materialSlots":slot_names,"boundsMinUnity":lo,"boundsMaxUnity":hi})
        collection.hide_render=True;collection.hide_viewport=True
    assets.append(record);stats[ident]=lodstats
    proxies.append({"assetId":ident,"use":"Non-gameplay spatial query only; 01 owns movement collision",
                    "boxes":[{"center":[(a+b)/2 for a,b in zip(record["boundsMin"],record["boundsMax"])],
                              "size":[b-a for a,b in zip(record["boundsMin"],record["boundsMax"])]}]})
    print("EXPORTED",ident,lodstats[0]["triangles"],lodstats[1]["triangles"],flush=True)

def dump(name,value): (OUT/name).write_text(json.dumps(value,indent=2)+"\n")
dump("asset-stats.json",stats)
dump("collision-proxies.json",{"contractVersion":1,"proxies":proxies,
                              "warning":"Conservative boxes, not detailed physics surfaces. Never bind buildings as track colliders."})
base_doc={"contractVersion":1,"revision":revision,"courseHash":HASH}
dump("manifest.json",dict(base_doc,layoutComplete=False,sourceBlend="source/vrx_city_kit.blend",
                         assets=assets,materials=material_records))
dump("track-profile.json",{"contractVersion":1,"courseHash":HASH,"strips":[]})
dump("lighting.json",dict(base_doc,environment={
    "fogColorLinear":[.012,.028,.039],"fogDensity":.0015,"exposureEV":0,
    "ambientTintLinear":[.055,.09,.12],"skyMaterialId":"production-night-sky"},
    lights=[],reflectionVolumes=[],applicationPolicy="Art intent only; 01 owns global rendering. Do not auto-apply."))

# Place in the real sampled course with conservative road-clearance and building overlap tests.
course=json.loads((ROOT/"context/course-data.json").read_text())
frames=course["frames"];rng=random.Random(150926)
asset_map={a["id"]:a for a in assets}
instances=[];rejected=[];instance_counts={}
def frame(p):return frames[round((p%1)*1200)]
def basis(f,banked):
    if banked:return Matrix((Vector(f["right"]),Vector(f["up"]),Vector(f["forward"]))).transposed()
    fw=Vector((f["forward"][0],0,f["forward"][2])).normalized()
    right=Vector((fw.z,0,-fw.x))
    return Matrix((right,Vector((0,1,0)),fw)).transposed()
def bounds_world(asset,pos,rot):
    lo,hi=asset["boundsMin"],asset["boundsMax"]
    return [pos+rot@Vector((x,y,z)) for x in [lo[0],hi[0]] for y in [lo[1],hi[1]] for z in [lo[2],hi[2]]]
def clear_road(asset,pos,rot):
    # Swept road samples checked in asset-local OBB, expanded by the exact
    # projected protected road prism. Conservative against every course branch.
    inv=rot.transposed();lo=Vector(asset["boundsMin"]);hi=Vector(asset["boundsMax"])
    for f in frames[:-1]:
        p=inv@(Vector(f["position"])+Vector(f["up"])*4-pos)
        ri=inv@Vector(f["right"]);up=inv@Vector(f["up"]);fw=inv@Vector(f["forward"])
        ext=Vector(tuple(abs(ri[i])*12.2+abs(up[i])*4.05+abs(fw[i])*.85 for i in range(3)))
        if all(lo[i]-ext[i]<=p[i]<=hi[i]+ext[i] for i in range(3)):return False
    return True
def overlap_buildings(asset,pos,rot):
    # Conservative footprint separating-axis test; allow rooftop plant, not towers,
    # to overlap only through a separately requested attachment, absent here.
    a=bounds_world(asset,pos,rot);ax=[rot@Vector((1,0,0)),rot@Vector((0,0,1))]
    for inst in instances:
        if inst["assetId"]=="vrx_track_guidance_a":continue
        b_asset=asset_map[inst["assetId"]];bp=Vector(inst["position"])
        br=Matrix.LocRotScale(None,__import__("mathutils").Quaternion((inst["rotation"][3],*inst["rotation"][:3])),None).to_3x3()
        b=bounds_world(b_asset,bp,br)
        if max(v.y for v in a)<min(v.y for v in b) or max(v.y for v in b)<min(v.y for v in a):continue
        axes=ax+[br@Vector((1,0,0)),br@Vector((0,0,1))]
        if not any(max(v.dot(k) for v in a)+1<min(v.dot(k) for v in b) or
                   max(v.dot(k) for v in b)+1<min(v.dot(k) for v in a) for k in axes):return True
    return False
def add(ident,p,lateral,vertical,role="middle",banked=False,check=True):
    f=frame(p);rot=basis(f,banked);asset=asset_map[ident]
    pos=Vector(f["position"])+Vector(f["right"])*lateral+Vector(f["up"])*vertical
    if check and (not clear_road(asset,pos,rot) or overlap_buildings(asset,pos,rot)):
        rejected.append({"assetId":ident,"progress":p,"lateral":lateral});return False
    count=instance_counts.get(ident,0)+1;instance_counts[ident]=count
    quat=rot.to_quaternion()
    instances.append({"id":ident+f"_{count:03d}","assetId":ident,"zoneId":"vrx_zone_exemplar_a",
                      "position":list(pos),"rotation":[quat.x,quat.y,quat.z,quat.w],"scale":[1,1,1],
                      "frameMode":"banked" if banked else "upright","role":role,"gi":"probe",
                      "sourceProgress":f["progress"],"sourceLateral":lateral,"sourceVertical":vertical})
    return True

towers=[a["id"] for a in assets if any(k in a["id"] for k in ["exchange","hotel","services","works"])]
if EXPANDED:
    # Landmarks first to protect designed anchor views.
    add("vrx_city_landmark_a",.34,-105,-45,"far")
    add("vrx_city_landmark_a",.58,145,-35,"far")
    for p,side in [(.095,1),(.255,-1),(.46,1)]:
        add("vrx_city_bridge_a",p,side*52,-12,"near")
limit=.70 if EXPANDED else .32
for k in range(60 if EXPANDED else 24):
    p=(k/(60 if EXPANDED else 24))*limit+rng.uniform(.001,.006)
    side=-1 if k%2 else 1
    ident=towers[k%len(towers)]
    lateral=side*rng.uniform(44,76);vertical=-rng.uniform(17,30)
    for attempt in range(4):
        if add(ident,p,lateral,vertical,"middle"):break
        lateral+=side*26
for k in range(18 if EXPANDED else 9):
    p=.01+k*limit/(18 if EXPANDED else 9)
    side=1 if k%2 else -1
    ident=towers[(k*5+1)%len(towers)]
    add(ident,p,side*rng.uniform(150,230),-rng.uniform(14,35),"far")
for k in range(24 if EXPANDED else 10):
    p=.012+k*limit/(24 if EXPANDED else 10)
    side=-1 if k%2 else 1
    # A low mass is below the road sightline, with its highest soffit near deck level.
    add("vrx_city_podium_a",p,side*30,-7,"near")
if EXPANDED:
    for ident,ps,lateral,vertical in [
        ("vrx_city_canopy_a",[.062,.21,.39,.56],29,0),
        ("vrx_city_advert_a",[.048,.174,.30,.49,.65],26,0)]:
        for i,p in enumerate(ps):add(ident,p,lateral*(-1 if i%2 else 1),vertical,"near")
    # Roof plant is physically attached to the upper service terrace, never an
    # unrelated floating route decoration. Elevation comes from authored roof.
    from mathutils import Quaternion
    parents=[i for i in instances if i["assetId"].startswith("vrx_city_services")][:5]
    for parent in parents:
        q=parent["rotation"];rot=Quaternion((q[3],*q[:3])).to_matrix()
        pos=Vector(parent["position"])+rot@Vector((-4.8,47.65,4.5))
        if not clear_road(asset_map["vrx_city_plant_a"],pos,rot):continue
        count=instance_counts.get("vrx_city_plant_a",0)+1;instance_counts["vrx_city_plant_a"]=count
        instances.append({"id":f"vrx_city_plant_a_{count:03d}","assetId":"vrx_city_plant_a",
            "zoneId":"vrx_zone_exemplar_a","position":list(pos),"rotation":q,"scale":[1,1,1],
            "frameMode":"upright","role":"middle","gi":"probe","attachmentParentId":parent["id"],
            "attachmentNote":"Upper terrace lip at local (-4.8,47.65,4.5); validate against imported dimensions."})
for k in range(int(limit*course["length"]/9)):
    p=.003+k*9/course["length"]
    for side in [-1,1]:
        # Broad banked shoe begins beyond protected lateral 12.2m.
        add("vrx_track_guidance_a",p,side*12.75,.04,"near",True)
# Also dress the actual player grid before the wrap.
for p in [.985,.990,.995]:
    for s in [-1,1]:add("vrx_track_guidance_a",p,s*12.75,.04,"near",True)
dump("layout.json",dict(base_doc,zones=[{"id":"vrx_zone_exemplar_a","startProgress":0,"endProgress":1}],
                       instances=instances,scope="Only .98482 through wrap to .70. No full-circuit expansion."))
dump("placement-checks.json",{"accepted":len(instances),"rejectedCandidates":len(rejected),
                              "courseSamples":1201,"protectedLateral":[-12.2,12.2],
                              "protectedVertical":[-.05,8],"method":"Conservative OBB versus expanded sampled road prisms; footprint SAT between buildings",
                              "limitations":"Camera volume, assembled old city overlap and moving occlusion require 01 native validation.",
                              "rejected":rejected})
shutil.copy2(__file__,OUT/"city_recipe.py")

# Studio render of the actual authored exports. Does not overwrite native scene.
studio=bpy.data.collections.new("STUDIO_NOT_EXPORTED");scene.collection.children.link(studio)
for i,(ident,_) in enumerate(builders):
    src=exports[ident];ob=src.copy();ob.data=src.data;studio.objects.link(ob)
    ob.location=Vector(((i%5)*63,(i//5)*110,0));ob.hide_render=False;ob.hide_viewport=False
world=bpy.data.worlds.new("Neutral studio");scene.world=world;world.use_nodes=True
world.node_tree.nodes["Background"].inputs[0].default_value=(.065,.10,.15,1)
world.node_tree.nodes["Background"].inputs[1].default_value=.5
def area(name,loc,power,size,target):
    data=bpy.data.lights.new(name,"AREA");data.energy=power;data.shape="DISK";data.size=size
    ob=bpy.data.objects.new(name,data);studio.objects.link(ob);ob.location=loc
    ob.rotation_euler=(Vector(target)-ob.location).to_track_quat("-Z","Y").to_euler()
area("Broad studio key",(80,-120,220),1700000,170,(95,50,40))
area("Cool form fill",(-100,80,140),650000,120,(100,50,40))
bpy.ops.mesh.primitive_plane_add(size=2000,location=(100,60,-.04));floor=bpy.context.object;floor.data.materials.append(M["slate"])
for c in list(floor.users_collection):c.objects.unlink(floor)
studio.objects.link(floor)
data=bpy.data.cameras.new("Kit studio camera");cam=bpy.data.objects.new("Kit studio camera",data);studio.objects.link(cam)
target=Vector((110,100 if EXPANDED else 10,55 if EXPANDED else 35))
cam.location=target+Vector((270,-400,245));cam.rotation_euler=(target-cam.location).to_track_quat("-Z","Y").to_euler()
data.type="ORTHO";data.ortho_scale=540 if EXPANDED else 350;scene.camera=cam
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/"source/vrx_city_kit.blend"))
scene.render.filepath=str(ROOT/"evidence"/(revision+"-kit.png"))
bpy.ops.render.render(write_still=True)
print("CITY_PACKAGE_COMPLETE",revision,len(assets),len(instances),flush=True)
