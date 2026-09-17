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
def mesh(name,verts,faces,mat,uvs=None):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();ob=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(ob);me.materials.append(M[mat]);parts[mat].append(ob)
 for p in me.polygons:p.use_smooth=True
 uv=me.uv_layers.new()
 for p in me.polygons:
  for i in p.loop_indices:
   co=me.vertices[me.loops[i].vertex_index].co;uv.data[i].uv=uvs[me.loops[i].vertex_index] if uvs else (co.x*.07,co.z*.1)
 return ob
def tube(name,points,radius,mat='filament'):
 vv=[];ff=[];sides=6
 for i,p in enumerate(points):
  p=Vector(p);t=(Vector(points[min(i+1,len(points)-1)])-Vector(points[max(0,i-1)])).normalized();axis=t.cross(Vector((0,0,1)))
  if axis.length<.01:axis=t.cross(Vector((0,1,0)))
  axis.normalize();other=t.cross(axis).normalized();r=radius*(1-.8*(i/(len(points)-1))**3)
  for j in range(sides):vv.append(p+r*(axis*math.cos(j*math.tau/sides)+other*math.sin(j*math.tau/sides)))
 for i in range(len(points)-1):
  for j in range(sides):a=i*sides+j;b=i*sides+(j+1)%sides;ff.append((a,b,b+sides,a+sides))
 return mesh(name,vv,ff,mat)
def sphere(name,pos,scale,mat):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=32,ring_count=20,location=pos);ob=bpy.context.object;ob.name=name;ob.scale=scale;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);ob.data.materials.append(M[mat]);parts[mat].append(ob)
 for p in ob.data.polygons:p.use_smooth=True
# Broad head/shoulders taper smoothly to the caudal peduncle.
profile=[(0,.8),(.07,2.9),(.17,4.1),(.31,4.6),(.46,4.35),(.61,3.55),(.77,2.4),(.9,1.35),(1,.82)]
def radius(t):
 for (a,ra),(b,rb) in zip(profile,profile[1:]):
  if t<=b:u=(t-a)/(b-a);u=u*u*(3-2*u);return ra+(rb-ra)*u
 return .82
def center(t):return Vector((-14+26*t,.7*math.sin(t*math.pi*1.4),.6*math.sin(t*math.pi)))
vv=[];uvs=[];ff=[];N=112;K=72
for i in range(N+1):
 t=i/N;c=center(t);r=radius(t)
 for j in range(K+1):
  a=j/K*math.tau;vv.append(c+Vector((0,math.sin(a)*r*.80,math.cos(a)*r)));uvs.append((t,j/K))
for i in range(N):
 for j in range(K):
  a=i*(K+1)+j;ff.append((a+K+1,a+K+2,a+1,a))
ff+=[tuple(range(K)),tuple(reversed([N*(K+1)+j for j in range(K)]))]
mesh('Continuous koi anatomy',vv,ff,'body',uvs)
# Eyes sit in the head; small dark lenses, hot irises and raised orbital rims.
for side in [-1,1]:
 sphere('Recessed eye',(-10.7,side*2.45,1.25),(.48,.28,.5),'gill')
 sphere('Dark optical pupil',(-10.8,side*2.69,1.25),(.26,.12,.28),'eye')
 tube('Luminous iris',[(-10.8+.34*math.cos(a),side*2.80,1.25+.36*math.sin(a)) for a in [j*math.tau/48 for j in range(49)]],.045)
 # Fine curved gill boundary, with three short internal marks.
 for off in [0,.24]:tube('Gill contour',[(-7.2+off,side*(3.1+.3*math.sin(t*math.pi)),2.6-5*t) for t in [j/24 for j in range(25)]],.06,'gill')
 for j in range(2):tube('Whisker',[(-13.55,side*(.6+t*2.0),-.55-j*.45-1.1*math.sin(t*math.pi*.8)) for t in [k/28 for k in range(29)]],.065)
# Small mouth opening and fleshy luminous lips.
sphere('Mouth opening',(-14.03,0,-.2),(.12,.7,.38),'gill')
tube('Mouth rim',[(-14.13,.8*math.cos(a),-.2+.44*math.sin(a)) for a in [j*math.tau/48 for j in range(49)]],.07)
# Swept rays and curved membrane strips form the fins. Free ray ends extend beyond the web.
def fin(name,origins,tips,web=.78):
 rays=[]
 for j,(a,b) in enumerate(zip(origins,tips)):
  a,b=Vector(a),Vector(b);pts=[]
  for k in range(25):
   t=k/24;p=a.lerp(b,t);p.y+=math.sin(math.pi*t)*(.8+math.sin(j*.43)*.7);p.z+=(1.8 if name=='Tail' else .9)*math.sin(t*math.pi*1.25+j*.055)*t*t; p.x+=1.6*math.sin(t*math.pi)*t if name=='Tail' else 0;pts.append(p)
  rays.append(pts);tube(name+' luminous ray',pts,.10 if name=='Tail' else .07)
 vv=[];ff=[];rows=18
 for j,pts in enumerate(rays):
  for k in range(rows):
   t=k/(rows-1)*web*(.96+.04*math.sin(j*1.9));q=t*24;lo=min(int(q),23);vv.append(pts[lo].lerp(pts[lo+1],q-lo))
 for j in range(len(rays)-1):
  for k in range(rows-1):
   a=j*rows+k;ff.append((a,a+1,a+rows+1,a+rows))
 mesh(name+' flowing membrane',vv,ff,'membrane')
# Long twin caudal lobes and a deep central cleft: a continuous graceful tail silhouette.
orig=[];tips=[]
for j in range(59):
 u=j/58;z=2*u-1;orig.append((11.7,.68,z*.72));length=8+12*abs(z)**.55+2.4*math.sin(j*1.6)+1.5*math.sin(j*.43);tips.append((12+length,1.1+2*math.sin(u*math.pi*1.7),z*(7+2.5*math.sin(u*math.pi))+1.2))
fin('Tail',orig,tips,.55)
orig=[];tips=[]
for j in range(43):
 u=j/42;t=.24+.56*u;c=center(t);orig.append(c+Vector((0,0,radius(t)*.96)));tips.append((c.x+3+5*u,c.y+.3,c.z+radius(t)+2+7*math.sin(u*math.pi)**.65))
fin('Dorsal',orig,tips,.58)
for side in [-1,1]:
 orig=[];tips=[]
 for j in range(31):
  u=j/30;orig.append((-6+2*u,side*3.3,-1+u*.5));tips.append((-4+10*u,side*(6+5*math.sin(u*math.pi)), -2-4*math.sin(u*math.pi)+2*u))
 fin('Pectoral',orig,tips,.62)
 orig=[];tips=[]
 for j in range(19):
  u=j/18;orig.append((4+2*u,side*1.8,-1.5));tips.append((7+6*u,side*(3+3*math.sin(u*math.pi)),-3-2*math.sin(u*math.pi)))
 fin('Pelvic',orig,tips,.58)
# Join material groups with the same origin, so a single traveling-wave shader deforms all parts coherently.
final=[]
for mat,objects in parts.items():
 if not objects:continue
 bpy.ops.object.select_all(action='DESELECT')
 for o in objects:o.select_set(True);bpy.context.view_layer.objects.active=o
 bpy.ops.object.join();o=bpy.context.object;o.name='Koi_'+mat;bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');final.append(o)
bpy.ops.wm.save_as_mainfile(filepath=str(R/'HolographicKoi.blend'))
bpy.ops.export_scene.fbx(filepath=str(R/'HolographicKoi.fbx'),use_selection=False,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',axis_forward='-Z',axis_up='Y',bake_anim=False)
(R/'manifest.json').write_text(json.dumps({'materials':list(M),'triangles':sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in final),'renderers':len(final),'finRays':202,'source':'Authored geometry guided by the two owner-supplied luminous koi references; no generated bitmap used.'},indent=2)+'\n')
