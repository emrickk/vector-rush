"""Only confirmed competing graphite surfaces; keep approved armor/glass vertices and all UVs."""
import bpy,math
from mathutils import Vector

def apply_contact_correction():
 changes={};moved={}
 def move_vertices(obj,new_points):
  count=0
  for v,new in zip(obj.data.vertices,new_points):
   old=obj.matrix_world@v.co
   if (new-old).length>1e-8:
    key=tuple(round(float(c),6) for c in old);moved[key]=new.copy();v.co=obj.matrix_world.inverted()@new;count+=1
  obj.data.update();changes[obj.name]=count
 sill=bpy.data.objects['Canopy / narrow continuous sill'];points=[]
 for v in sill.data.vertices:
  p=sill.matrix_world@v.co;p.z-=.040;p.y=-.865+(p.y+.865)*.99;points.append(p)
 move_vertices(sill,points)
 for side in [-1,1]:
  obj=bpy.data.objects['Nacelle / closed structural cavity shell '+str(side)]
  polygon=[(side*(1.75+x),y) for x,y in [(-.26,-.91),(.13,-.91),(.31,.59),(.23,.75),(-.28,.75)]]
  area=sum(polygon[j][0]*polygon[(j+1)%len(polygon)][1]-polygon[(j+1)%len(polygon)][0]*polygon[j][1] for j in range(len(polygon)))
  edges=[]
  for j,a in enumerate(polygon):
   b=polygon[(j+1)%len(polygon)];e=Vector((b[0]-a[0],b[1]-a[1]));length=e.length;e/=length
   outward=Vector((e.y,-e.x))*(1 if area>0 else -1);edges.append((Vector(a),e,length,outward))
  points=[]
  for vertex in obj.data.vertices:
   p=obj.matrix_world@vertex.co;original=p.copy()
   # Existing rear and front dark returns sit 15 mm behind the white cap planes.
   if p.y>3.25:p.y-=.015*min(1.,(p.y-3.25)/.09)
   if p.y< -3.25:p.y+=.015*min(1.,(-3.25-p.y)/.09)
   # Enlarge only the graphite intake cut, creating a recessed backing edge.
   normals=[];xy=Vector((original.x,original.y))
   for a,e,length,outward in edges:
    delta=xy-a;t=delta.dot(e);distance=abs(delta.dot(outward))
    if distance<.00002 and -.00002<=t<=length+.00002:normals.append(outward)
   if normals:
    direction=sum(normals,Vector((0,0)))
    denominator=max(direction.dot(normals[0]),.1)
    shift=direction*(.015/denominator);p.x+=shift.x;p.y+=shift.y
   points.append(p)
  move_vertices(obj,points)
 # Runtime material meshes keep their exact topology, face order and UV loops.
 runtime_count={};runtime=bpy.data.collections['EXPORT_RUNTIME']
 for obj in runtime.objects:
  if obj.type!='MESH':continue
  count=0
  for v in obj.data.vertices:
   old=obj.matrix_world@v.co;key=tuple(round(float(c),6) for c in old)
   if key in moved:v.co=obj.matrix_world.inverted()@moved[key];count+=1
  if count:obj.data.update();runtime_count[obj.name]=count
 return {'source_moved_vertices':changes,'runtime_moved_vertices':runtime_count,'unique_position_edits':len(moved),'sill_lowering_m':.04,'sill_longitudinal_scale':.99,'graphite_return_recess_m':.015,'graphite_intake_expansion_m':.015,'armor_and_glass_geometry_changed':False,'uv_coordinates_changed':False}
