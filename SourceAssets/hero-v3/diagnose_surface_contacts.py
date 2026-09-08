"""Read-only surface collision audit for actual visible pass03 artifacts."""
import bpy,json,sys,hashlib
from pathlib import Path
from mathutils import Vector
from mathutils.bvhtree import BVHTree
out=Path(sys.argv[sys.argv.index('--')+1]);out.parent.mkdir(parents=True,exist_ok=True)
def data(o):
 o.data.calc_loop_triangles();v=[o.matrix_world@p.co for p in o.data.vertices];tris=[tuple(t.vertices) for t in o.data.loop_triangles]
 return v,tris,BVHTree.FromPolygons(v,tris,all_triangles=True,epsilon=1e-5)
def clip_polygon(subject,clip):
 area=sum(clip[j][0]*clip[(j+1)%len(clip)][1]-clip[(j+1)%len(clip)][0]*clip[j][1] for j in range(len(clip)))
 sign=1 if area>=0 else -1
 for j in range(len(clip)):
  a=clip[j];b=clip[(j+1)%len(clip)];old=subject;subject=[]
  def inside(p):return sign*((b[0]-a[0])*(p[1]-a[1])-(b[1]-a[1])*(p[0]-a[0]))>=-1e-10
  def intersection(p,q):
   rx=q[0]-p[0];ry=q[1]-p[1];sx=b[0]-a[0];sy=b[1]-a[1];den=rx*sy-ry*sx
   if abs(den)<1e-15:return p
   t=((a[0]-p[0])*sy-(a[1]-p[1])*sx)/den;return (p[0]+t*rx,p[1]+t*ry)
  if not old:break
  previous=old[-1]
  for current in old:
   if inside(current):
    if not inside(previous):subject.append(intersection(previous,current))
    subject.append(current)
   elif inside(previous):subject.append(intersection(previous,current))
   previous=current
 return subject
def overlap_area(a,b,n):
 axis=max(range(3),key=lambda k:abs(n[k]));axes=[k for k in range(3) if k!=axis]
 aa=[(p[axes[0]],p[axes[1]]) for p in a];bb=[(p[axes[0]],p[axes[1]]) for p in b]
 intersection=clip_polygon(aa,bb)
 return abs(sum(intersection[j][0]*intersection[(j+1)%len(intersection)][1]-intersection[(j+1)%len(intersection)][0]*intersection[j][1] for j in range(len(intersection)))*.5)/max(abs(n[axis]),1e-9) if len(intersection)>2 else 0
cache={}
def pair(an,bn):
 a=bpy.data.objects[an];b=bpy.data.objects[bn]
 if an not in cache:cache[an]=data(a)
 if bn not in cache:cache[bn]=data(b)
 av,at,abvh=cache[an];bv,bt,bbvh=cache[bn]
 hits=abvh.overlap(bbvh);existing=set(hits)
 # Exact coplanar faces can be omitted by triangle-intersection BVH predicates.
 for plane in [-3.34,3.34,-2.55,.82]:
  ca=[i for i,t in enumerate(at) if all(abs(av[j].y-plane)<1e-5 for j in t)]
  cb=[i for i,t in enumerate(bt) if all(abs(bv[j].y-plane)<1e-5 for j in t)]
  for ai in ca:
   for bi in cb:
    if (ai,bi) not in existing:hits.append((ai,bi));existing.add((ai,bi))
 coplanar=[];cross=[]
 for ai,bi in hits:
  aa=[av[j] for j in at[ai]];bb=[bv[j] for j in bt[bi]];na=(aa[1]-aa[0]).cross(aa[2]-aa[0]).normalized();nb=(bb[1]-bb[0]).cross(bb[2]-bb[0]).normalized()
  distances=[abs((p-aa[0]).dot(na)) for p in bb]
  if abs(na.dot(nb))>.99999 and max(distances)<.00001:
   area=overlap_area(aa,bb,na)
   if area>1e-9:coplanar.append({'a_triangle':ai,'b_triangle':bi,'overlap_area_m2':area,'normal_dot':na.dot(nb),'centroid':list(sum(aa,Vector())/3)})
  else:cross.append({'a_triangle':ai,'b_triangle':bi,'a_centroid':list(sum(aa,Vector())/3),'normal_dot':na.dot(nb)})
 near=[]
 for tri in at:
  aa=[av[j] for j in tri];c=sum(aa,Vector())/3;na=(aa[1]-aa[0]).cross(aa[2]-aa[0]).normalized();p,nb,idx,d=bbvh.find_nearest(c)
  if d is not None and d<.002 and abs(na.dot(nb))>.9:near.append({'distance_m':d,'signed_m':(c-p).dot(nb),'position':list(c),'normal_dot':na.dot(nb)})
 return {'a':an,'b':bn,'intersection_triangle_pairs':len(hits),'positive_area_coplanar_pairs':len(coplanar),'coplanar_overlap_area_sum_m2':sum(x['overlap_area_m2'] for x in coplanar),'coplanar_examples':sorted(coplanar,key=lambda x:x['overlap_area_m2'],reverse=True)[:12],'noncoplanar_intersection_pairs':len(cross),'crossing_examples':cross[:12],'near_parallel_centroids_under_2mm':len(near),'near_examples':sorted(near,key=lambda x:x['distance_m'])[:12]}
report={'source':bpy.data.filepath,'source_sha256':hashlib.sha256(Path(bpy.data.filepath).read_bytes()).hexdigest(),'method':'BVH triangle surface intersections, positive-area coplanar clipping, near-parallel centroid distances; no texture/tangent evaluation','pairs':[]}
report['pairs'].append(pair('Canopy / controlled convex near-black glazing','Canopy / narrow continuous sill'))
report['pairs'].append(pair('Canopy / controlled convex near-black glazing','Monocoque / continuous load-bearing center'))
for side in [-1,1]:
 for name in ['Armor / waisted center crown ','Armor / engine shoulder crown ','Armor / aft changing lower return ']:report['pairs'].append(pair(name+str(side),'Nacelle / closed structural cavity shell '+str(side)))
out.write_text(json.dumps(report,indent=2))
print(json.dumps([{k:r[k] for k in ['a','b','intersection_triangle_pairs','positive_area_coplanar_pairs','coplanar_overlap_area_sum_m2','noncoplanar_intersection_pairs','near_parallel_centroids_under_2mm']} for r in report['pairs']],indent=2))
