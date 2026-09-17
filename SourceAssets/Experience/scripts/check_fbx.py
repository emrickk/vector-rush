"""Round-trip actual FBX exports and compare dimensions, UVs and material slots.
Blender --background --python this-file -- b01. Native Unity remains separate.
"""
import bpy,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
revision="vrx-"+sys.argv[sys.argv.index("--")+1];package=ROOT/"packages"/revision
manifest=json.loads((package/"manifest.json").read_text());report=[]
for asset in manifest["assets"]:
    for key in ["lod0","lod1"]:
        bpy.ops.object.select_all(action="SELECT");bpy.ops.object.delete(use_global=False)
        for m in list(bpy.data.materials):bpy.data.materials.remove(m)
        bpy.ops.import_scene.fbx(filepath=str(package/asset[key]))
        meshes=[o for o in bpy.context.selected_objects if o.type=="MESH"]
        assert meshes,asset["id"]
        coords=[];slots=[]
        for o in meshes:
            assert o.data.uv_layers,asset["id"]+" missing UV0"
            slots.extend(m.name for m in o.data.materials)
            for v in o.data.vertices:
                p=o.matrix_world@v.co;coords.append((p.x,p.z,-p.y))
        lo=[min(p[i] for p in coords) for i in range(3)];hi=[max(p[i] for p in coords) for i in range(3)]
        delta=max(abs(a-b) for a,b in zip(lo+hi,asset["boundsMin"]+asset["boundsMax"]))
        assert delta<.1,(asset["id"],key,"bounds delta",delta)
        assert slots==asset["materialSlots"],(asset["id"],slots,asset["materialSlots"])
        report.append({"assetId":asset["id"],"lod":key,"maxBoundsDeltaMetres":delta,
                       "uv0":True,"materialSlotOrder":True})
out={"status":"PASS FBX roundtrip","revision":revision,"models":len(report),"results":report,
     "limitations":"Blender FBX reimport validates exported shape and slots. Unity import and moving usability are separate."}
(ROOT/"evidence"/(revision+"-fbx-roundtrip.json")).write_text(json.dumps(out,indent=2)+"\n")
print("FBX_ROUNDTRIP_PASS",len(report))
