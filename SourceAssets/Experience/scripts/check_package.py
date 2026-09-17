"""Validate immutable payload integrity and the key production import contract.
This is source/package validation, not a substitute for Unity native validation.
"""
from pathlib import Path
import sys,json,hashlib,math
from PIL import Image

def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
root=Path(sys.argv[1]).resolve()
def read(name):return json.loads((root/name).read_text())
ready=read("READY.json");checksums=read("checksums.json")
assert sha(root/"checksums.json")==ready["checksumsSha256"],"READY digest"
declared={x["path"] for x in checksums["files"]}
actual={p.relative_to(root).as_posix() for p in root.rglob("*") if p.is_file() and p.name not in ("READY.json","checksums.json")}
assert declared==actual,"Checksum coverage"
for x in checksums["files"]:assert sha(root/x["path"])==x["sha256"],x["path"]
m=read("manifest.json");layout=read("layout.json");stats=read("asset-stats.json")
assert m["courseHash"]==layout["courseHash"]==ready["courseHash"]
assert m["revision"]==ready["revision"]==layout["revision"]
def unique(records):assert len({x["id"] for x in records})==len(records),"duplicate IDs"
unique(m["assets"]);unique(m["materials"]);unique(layout["instances"])
mats={x["id"] for x in m["materials"]};assets={a["id"]:a for a in m["assets"]}
for a in m["assets"]:
    for field in ["lod0","lod1"]:assert (root/a[field]).is_file()
    assert a["pivot"]=="ground-center" and a["collider"]=="none"
    assert set(a["materialSlots"])<=mats
    assert all(b>a for a,b in zip(a["boundsMin"],a["boundsMax"]))
    for lod in stats[a["id"]]:
        assert lod["degenerateTriangles"]==lod["nonmanifoldEdges"]==0
    assert stats[a["id"]][1]["triangles"]<stats[a["id"]][0]["triangles"]
for i in layout["instances"]:
    assert i["assetId"] in assets
    assert abs(sum(x*x for x in i["rotation"])-1)<.001
    assert i["scale"]==[1,1,1]
    assert i["frameMode"] in ["upright","banked"]
for mat in m["materials"]:
    for k in ["baseColor","normal","metallicSmoothness","occlusion","emission"]:
        if mat[k]:
            with Image.open(root/mat[k]) as im:im.verify()
extra=read("experience-assets.json");unique(extra["assets"])
uncompressed=0
for a in extra["assets"]:
    p=root/a["path"];assert p.is_file()
    if p.suffix==".png":
        with Image.open(p) as im:
            uncompressed+=im.width*im.height*4
            if a["type"]=="vfx-atlas":
                atlas=a["atlas"];assert atlas["frameCount"]<=math.prod(atlas["grid"])
    if a["type"]=="font":assert not a["requiredHUDGlyphsMissing"]
print(json.dumps({"status":"PASS source/package checks only","revision":m["revision"],
    "assets":len(m["assets"]),"instances":len(layout["instances"]),
    "lod0TrianglesAllUniqueModels":sum(v[0]["triangles"] for v in stats.values()),
    "lod1TrianglesAllUniqueModels":sum(v[1]["triangles"] for v in stats.values()),
    "extraImageUncompressedRGBABytes":uncompressed,"checksummedFiles":len(actual),
    "nativeImport":"unverified","movingUsability":"unverified"},indent=2))
