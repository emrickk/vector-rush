import bpy,bmesh,json,sys
from pathlib import Path
args=sys.argv[sys.argv.index('--')+1:]
report={}
for o in bpy.data.collections['KESTREL_07_EXPORT'].objects:
 if o.type!='MESH':continue
 bm=bmesh.new();bm.from_mesh(o.data)
 edges=[]
 for e in bm.edges:
  if not e.is_manifold:edges.append({'faces':len(e.link_faces),'length':e.calc_length(),'points':[[round(float(v),5) for v in p.co] for p in e.verts]})
 if edges:report[o.name]=edges
 bm.free()
Path(args[0]).write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
