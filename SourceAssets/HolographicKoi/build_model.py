"""Full-bodied luminous koi with swept filament fins; metric editable Blender/FBX."""
import bpy,math,json
from pathlib import Path
from mathutils import Vector
R=Path(__file__).resolve().parent
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.context.preferences.filepaths.save_version=0
M={}
for name,col in [('body',(.8,.045,.012)),('membrane',(.9,.08,.015)),('filament',(1,.48,.15)),('eye',(.012,.018,.025)),('gill',(.2,.012,.005))]:
 m=bpy.data.materials.new(name);m.diffuse_color=(*col,1);M[name]=m
parts={k:[] for k in M}
def mesh(name,verts,faces,mat,uvs=None,colors=None):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();ob=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(ob);me.materials.append(M[mat]);parts[mat].append(ob)
 for p in me.polygons:p.use_smooth=True
 uv=me.uv_layers.new()
 for p in me.polygons:
  for i in p.loop_indices:
   co=me.vertices[me.loops[i].vertex_index].co;uv.data[i].uv=uvs[me.loops[i].vertex_index] if uvs else (co.x*.07,co.z*.1)
 color=me.color_attributes.new(name="Col",type="FLOAT_COLOR",domain="POINT")
 for i,c in enumerate(color.data):c.color=colors[i] if colors else (0,0,0,1)
 return ob
def tube(name,points,radius,mat='filament',phase=0,flex=False):
 vv=[];ff=[];sides=8
 for i,p in enumerate(points):
  p=Vector(p);t=(Vector(points[min(i+1,len(points)-1)])-Vector(points[max(0,i-1)])).normalized();axis=t.cross(Vector((0,0,1)))
  if axis.length<.01:axis=t.cross(Vector((0,1,0)))
  axis.normalize();other=t.cross(axis).normalized();r=radius*(1-.8*(i/(len(points)-1))**3)
  for j in range(sides):vv.append(p+r*(axis*math.cos(j*math.tau/sides)+other*math.sin(j*math.tau/sides)))
 for i in range(len(points)-1):
  for j in range(sides):a=i*sides+j;b=i*sides+(j+1)%sides;ff.append((a,b,b+sides,a+sides))
 return mesh(name,vv,ff,mat,colors=[(i//sides/(len(points)-1) if flex else 0,phase,0,1) for i in range(len(vv))])
def sphere(name,pos,scale,mat):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=32,ring_count=20,location=pos);ob=bpy.context.object;ob.name=name;ob.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);ob.data.materials.append(M[mat]);parts[mat].append(ob)
 for p in ob.data.polygons:p.use_smooth=True
# Broad head/shoulders taper smoothly to the caudal peduncle.
profile=[(0,.48),(.055,1.8),(.15,3.75),(.29,4.9),(.43,4.7),(.59,3.8),(.75,2.55),(.9,1.4),(1,.70)]
# Monotone cubic Hermite slopes: continuous curvature flow instead of a flat
# derivative at every body ring (which made the old body visibly corrugated).
slopes=[]
for i,(x,y) in enumerate(profile):
 if i==0:m=(profile[1][1]-y)/(profile[1][0]-x)
 elif i==len(profile)-1:m=(y-profile[i-1][1])/(x-profile[i-1][0])
 else:
  d0=(y-profile[i-1][1])/(x-profile[i-1][0]);d1=(profile[i+1][1]-y)/(profile[i+1][0]-x)
  m=0 if d0*d1<=0 else 2*d0*d1/(d0+d1)
 slopes.append(m)
def radius(t):
 for i,((a,ra),(b,rb)) in enumerate(zip(profile,profile[1:])):
  if t<=b:
   u=(t-a)/(b-a);return (2*u**3-3*u**2+1)*ra+(u**3-2*u**2+u)*(b-a)*slopes[i]+(-2*u**3+3*u**2)*rb+(u**3-u**2)*(b-a)*slopes[i+1]
 return .82
def center(t):
 u=min(t/.22,1);u=u*u*(3-2*u)
 return Vector((-20+36*t,.30*math.sin(t*math.pi*1.4),-1.0*(1-u)+.25*math.sin(t*math.pi)))
vv=[];uvs=[];ff=[];N=160;K=96
for i in range(N+1):
 t=i/N;c=center(t);r=radius(t)
 for j in range(K+1):
  a=j/K*math.tau;vv.append(c+Vector((0,math.sin(a)*r*.80,math.cos(a)*r)));uvs.append((t,j/K))
for i in range(N):
 for j in range(K):
  a=i*(K+1)+j;ff.append((a+K+1,a+K+2,a+1,a))
ff+=[tuple(range(K)),tuple(reversed([N*(K+1)+j for j in range(K)]))]
mesh('Continuous koi anatomy',vv,ff,'body',uvs)
# Integrated eyes and curved opercula follow the actual new body surface.
for side in [-1,1]:
 t=.14;c=center(t);rad=radius(t);z=1.0;depth=rad*.80*math.sqrt(max(0,1-((z-c.z)/rad)**2))
 eye=(c.x,side*(depth+.10),z)
 sphere('Inset eye socket',eye,(.45,.16,.43),'gill')
 sphere('Small glass pupil',(eye[0]-.06,eye[1]+side*.15,eye[2]),(.24,.10,.26),'eye')
 tube('Fine iris',[(eye[0]-.06+.29*math.cos(a),eye[1]+side*.235,eye[2]+.30*math.sin(a)) for a in [j*math.tau/48 for j in range(49)]],.022)
 gill=[]
 for j in range(49):
  q=j/48;z=3.5-6.5*q;x=-12.3+1.25*math.sin(q*math.pi);t=(x+20)/36;c=center(t);rad=radius(t);y=rad*.8*math.sqrt(max(.01,1-((z-c.z)/rad)**2));gill.append((x,side*(y+.035),z))
 tube('Curved gill edge',gill,.022,'gill');tube('Gill light catch',gill[:35],.014)
 for j in range(2):
  tube('Fine trailing barbel',[(-19.75+2.5*t,side*(.36+2.1*t),-1.2-j*.22-1.1*math.sin(t*math.pi*.7)) for t in [k/40 for k in range(41)]],.034)
sphere('Mouth recess',(-20.03,0,-1.0),(.10,.45,.25),'gill')
tube('Subtle upper lip',[(-20.08,.48*math.cos(a),-1+.28*math.sin(a)) for a in [j*math.tau/64 for j in range(65)]],.025)

# Each fin is a curved sheet with its own broad folds, not a fan of rods.
# Vertex colour stores a root-to-tip flexibility and a phase shared by its veins.
ribbons=[];vein_count=0
def bezier(points,t):
 a,b,c,d=map(Vector,points);return a*(1-t)**3+b*(3*(1-t)**2*t)+c*(3*(1-t)*t*t)+d*t**3

def ribbon(name,points,width,phase,axis=(0,0,1),twist=.4):
 global vein_count
 axis=Vector(axis).normalized();N=72;K=18
 def sample(t,v):
  curve_center=bezier(points,t);eps=.001;tangent=(bezier(points,min(1,t+eps))-bezier(points,max(0,t-eps))).normalized()
  across=axis-tangent*axis.dot(tangent)
  if across.length<.05:across=Vector((0,1,0))
  across.normalize();depth=tangent.cross(across).normalized()
  angle=twist*math.sin(t*math.pi*1.2+phase*2);direction=across*math.cos(angle)+depth*math.sin(angle)
  w=.18*(1-t)+width*math.sin(math.pi*t)**.8
  # Broad cloth fold changes smoothly along the fin; the border remains flowing.
  fold=math.sin(v*math.pi*2+t*3+phase*4)*math.sin(math.pi*t)*.30*width
  p=curve_center+direction*((v*2-1)*w)+depth*fold
  if name.startswith("Dorsal"):
   rootX=(-10 if name=="Dorsal main veil" else -4)+11*v;bodyT=(rootX+20)/36
   root=center(bodyT)+Vector((0,0,radius(bodyT)*.98))
   p+=(root-Vector(points[0]))*(1-t)**2
  return p
 vv=[];uv=[];colors=[];ff=[]
 for i in range(N+1):
  t=i/N
  for j in range(K+1):
   v=j/K;vv.append(sample(t,v));uv.append((t,v));colors.append((t,phase,0,1))
 for i in range(N):
  for j in range(K):
   a=i*(K+1)+j;ff.append((a,a+1,a+K+2,a+K+1))
 mesh(name,vv,ff,'membrane',uv,colors)
 # A few asymmetric veins carry the structure; most visible area is a soft sheet.
 for j,v in enumerate([.24,.72]):
  pts=[sample(t,v)+Vector((0,.006,0)) for t in [k/72 for k in range(73)]]
  tube(name+' light vein',pts,.032+.006*(j%2),phase=phase,flex=True);vein_count+=1
 # One very thin border gives a readable, softly glowing curled silhouette.
 pts=[sample(k/72,1) for k in range(73)];tube(name+' edge filament',pts,.030,phase=phase,flex=True);vein_count+=1
 ribbons.append(name)

# Separate upper and lower caudal sheets, layered at slightly different depths.
ribbon('Upper caudal silk',[(15.7,0,.35),(23,.8,5),(30,-1.0,11),(39,1.0,8)],3.1,.08,twist=.65)
ribbon('Upper caudal inner fold',[(15.8,.3,.3),(23,1.3,2.8),(32,2.0,8.5),(37,2.4,6)],2.0,.12,twist=-.65)
ribbon('Upper caudal streamer',[(16,-.3,.4),(23,-1.3,4),(31,-1,11),(41,-.2,11.0)],1.1,.1,twist=.85)
ribbon('Lower caudal silk',[(15.7,0,-.3),(24,-.8,-4.0),(30,1.4,-8.5),(39,0,-5)],2.4,.18,twist=-.6)
ribbon('Lower caudal inner fold',[(15.8,.2,-.2),(24,1,-2.3),(31,2,-6),(36,1.7,-3.5)],1.6,.2,twist=.65)
ribbon('Dorsal main veil',[(-10,0,4.35),(-5,0,9.5),(8,1,10),(20,.2,6.2)],2.6,.06,twist=.35)
ribbon('Dorsal folded veil',[(-6,.1,4.7),(1,-.6,9.2),(13,-1,9),(25,0,8)],1.4,.09,twist=-.6)
for side in [-1,1]:
 phase=.13 if side<0 else .17
 ribbon('Pectoral main '+str(side),[(-10,side*3.4,-1),(-7,side*7,-3),(3,side*9,-5),(13,side*5.8,-2.5)],2.2,phase,twist=side*.8)
 ribbon('Pectoral fold '+str(side),[(-9,side*3.5,-1.1),(-4,side*6,-3.5),(5,side*7,-5.5),(16,side*4.8,-3.6)],1.2,phase+.025,twist=-side*.5)
 ribbon('Pelvic veil '+str(side),[(5,side*2,-1.8),(9,side*4.6,-3.5),(16,side*5,-4.7),(23,side*3,-2.4)],1.3,phase+.03,twist=side*.65)
# Join material groups with the same origin, so a single traveling-wave shader deforms all parts coherently.
final=[]
for mat,objects in parts.items():
 if not objects:continue
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:
  if not o.data.color_attributes:
   attr=o.data.color_attributes.new(name='Col',type='FLOAT_COLOR',domain='POINT')
   for c in attr.data:c.color=(0,0,0,1)
  o.select_set(True);bpy.context.view_layer.objects.active=o
 bpy.ops.object.join();o=bpy.context.object;o.name='Koi_'+mat;bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');final.append(o)
bpy.ops.wm.save_as_mainfile(filepath=str(R/'HolographicKoi.blend'))
bpy.ops.export_scene.fbx(filepath=str(R/'HolographicKoi.fbx'),use_selection=False,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',axis_forward='-Z',axis_up='Y',bake_anim=False)
(R/'manifest.json').write_text(json.dumps({'materials':list(M),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in final),'renderers':len(final),'finMembranes':len(ribbons),'finVeins':vein_count,'source':'Authored geometry guided by the approved Scenario koi-overhead-target-01.png reference. Geometry, fin flexibility and shader are native assets, not a pasted concept image.'},indent=2)+'\n')
