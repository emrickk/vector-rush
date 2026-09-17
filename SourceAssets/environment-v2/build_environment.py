import bpy, math, os, json, random
from mathutils import Vector
from math import sin, cos, pi
ROOT=os.path.dirname(os.path.abspath(__file__))
random.seed(482)
bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
for d in list(bpy.data.materials): bpy.data.materials.remove(d)
scene=bpy.context.scene
scene.unit_settings.system='METRIC'; scene.unit_settings.scale_length=1.0
scene.render.engine='CYCLES'; scene.cycles.device='CPU'; scene.cycles.samples=24
scene.cycles.use_denoising=True
scene.render.threads_mode='FIXED'; scene.render.threads=4
scene.render.resolution_x=1600; scene.render.resolution_y=1100; scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
MATS={}
for name,color,metal,rough in [('Ivory',(.72,.735,.67),.1,.36),('Graphite',(.035,.049,.053),.42,.42),('Glass',(.035,.135,.17),.68,.19),('Metal',(.27,.33,.34),.78,.27),('Rock',(.40,.405,.35),0,.86),('Vegetation',(.145,.20,.07),0,.9)]:
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*color,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough;MATS[name]=m
parts=[]
def mesh(name,v,f,mat='Ivory',smooth=False):
 me=bpy.data.meshes.new(name);me.from_pydata(v,[],f);me.update();ob=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(ob);ob.data.materials.append(MATS[mat]);parts.append(ob)
 for p in me.polygons:p.use_smooth=smooth
 return ob

def box(name,loc,size,mat='Ivory',bevel=0):
 x,y,z=loc;a,b,c=[v/2 for v in size]
 v=[(x+sx*a,y+sy*b,z+sz*c) for sx,sy,sz in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
 ob=mesh(name,v,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],mat)
 if bevel:
  mod=ob.modifiers.new('Light catching edge','BEVEL');mod.width=bevel;mod.segments=2
  mod=ob.modifiers.new('Weighted corner normals','WEIGHTED_NORMAL');mod.keep_sharp=True
 return ob

def profile(w,d,r,cx=0,cy=0,n=4):
 pts=[]
 for x,y,a in [(w/2-r,d/2-r,0),(-w/2+r,d/2-r,pi/2),(-w/2+r,-d/2+r,pi),(w/2-r,-d/2+r,pi*1.5)]:
  for i in range(n+1):
   theta=a+i*pi/(2*n);pts.append((cx+x+r*cos(theta),cy+y+r*sin(theta)))
 return pts

def prism(name,w,d,z,h,mat='Ivory',r=.8,cx=0,cy=0):
 pts=profile(w,d,min(r,w/3,d/3),cx,cy);N=len(pts)
 v=[(x,y,z+level) for level in [0,h] for x,y in pts];f=[tuple(range(N-1,-1,-1)),tuple(range(N,N*2))]
 f += [(i,(i+1)%N,(i+1)%N+N,i+N) for i in range(N)]
 ob=mesh(name,v,f,mat)
 # Curved corner faces interpolate; planar horizontal/vertical faces stay flat.
 for p in ob.data.polygons[2:]:p.use_smooth=True
 ob.modifiers.new('Facade weighted normals','WEIGHTED_NORMAL').keep_sharp=True
 return ob

def beam(name,a,b,width,depth,mat='Ivory'):
 mid=(Vector(a)+Vector(b))/2;ob=box(name,(0,0,0),(width,depth,(Vector(b)-Vector(a)).length),mat,.05);ob.location=mid;ob.rotation_euler=(Vector(b)-Vector(a)).to_track_quat('Z','Y').to_euler();return ob

def plant(x,y,z,s=1):
 # Wind-shorn scrub mass; 3 lobed crowns, no individual billboard leaves.
 for j in range(3):
  bpy.ops.mesh.primitive_uv_sphere_add(segments=8,ring_count=4,radius=1,location=(x+sin(j*2.1)*s*.34,y+cos(j*2.1)*s*.34,z+s*.35))
  ob=bpy.context.object;ob.name='Wind clipped roof scrub';ob.scale=(s*.58,s*.49,s*.35);ob.data.materials.append(MATS['Vegetation']);parts.append(ob)
  for p in ob.data.polygons:p.use_smooth=True

def planter(x,y,z,w,d):
 box('Raised terrace planter',(x,y,z+.27),(w,d,.54),'Ivory',.08);box('Recessed soil',(x,y,z+.555),(w-.23,d-.23,.055),'Graphite')
 for i in range(max(1,int(w/1.15))):plant(x-w/2+.65+i*1.15,y,z+.53,.70)

def rail(x,y,z,w,axis='X'):
 # Opaque smoked glass, consistent with runtime Glass material.
 size=(w,.055,.85) if axis=='X' else (.055,w,.85)
 box('Recessed terrace balustrade',(x,y,z+.50),size,'Glass');size=(w,.075,.06) if axis=='X' else (.075,w,.06)
 box('Terrace handrail',(x,y,z+.93),size,'Metal')

def roof_plant(cx,cy,z,w=4):
 box('Screened plant plinth',(cx,cy,z+.18),(w+1,4.5,.36),'Graphite',.06)
 for i in [-1,1]:
  box('Heat exchanger housing',(cx+i*w*.26,cy,z+.8),(w*.40,2.5,1.35),'Metal',.12)
  for j in range(9):box('Mechanical cooling louvre',(cx+i*w*.26,cy-1.26,z+.30+j*.12),(w*.35,.055,.045),'Graphite')
 for side in [-1,1]:
  for j in range(9):box('Plant privacy screen',(cx,cy+side*2,z+.35+j*.18),(w+1,.09,.09),'Ivory')

def podium(w,d,h):
 prism('Tidal foundation',w+2,d+2,0,.85,'Graphite',1.5)
 prism('Public promenade',w+1.3,d+1.3,.85,.42,'Ivory',1.5)
 prism('Recessed glazed lobby',w-4,d-4,1.27,h-1.75,'Glass',2)
 for side in [-1,1]:
  for x in [-w*.37,0,w*.37]:beam('Splayed civic column',(x,side*(d*.42),1.1),(x*.86,side*(d*.39),h),.7,.85)
 prism('Podium roof canopy',w,d,h,.46,'Ivory',2)
 # Separate entry canopy and steps read as an occupied place.
 box('Entry canopy',(0,-d/2-1.5,3.55),(8,4.7,.25),'Ivory',.1)
 for j in range(3):box('Entrance stair',(0,-d/2-2.8+j*.5,1.02+j*.07),(7,2.0-j*.4,.15),'Ivory',.04)
 for x in [-w*.38,w*.38]:planter(x,-d*.38,h+.46,3.4,1.4)

def finish(name,start):
 objs=parts[start:]
 bpy.ops.object.select_all(action='DESELECT')
 print('Consolidating',name,len(objs),'parts',flush=True)
 for ob in objs:ob.select_set(True)
 bpy.context.view_layer.objects.active=objs[0]
 bpy.ops.object.convert(target='MESH')
 bpy.ops.object.join();ob=bpy.context.object;ob.name=name
 bpy.context.scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR')
 # Join leaves duplicated identical material slots; remove duplicates without changing faces.
 mats=[]; remap={}
 for i,m in enumerate(ob.data.materials):
  if m not in mats:mats.append(m)
  remap[i]=mats.index(m)
 ids=[remap[p.material_index] for p in ob.data.polygons]
 ob.data.materials.clear()
 for m in mats:ob.data.materials.append(m)
 for p,i in zip(ob.data.polygons,ids):p.material_index=i
 bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
 # Metric box-projection UV0, overlapping for shared tiling; Unity can generate UV1 for baking.
 if name=='Solstice_CoastalCliff_C':
  for old_uv in list(ob.data.uv_layers):ob.data.uv_layers.remove(old_uv)
 uv=ob.data.uv_layers.new(name='MetricSurfaceUV')
 if name=='Solstice_CoastalCliff_C':uv.active_render=True;ob.data.uv_layers.active_index=0
 for p in ob.data.polygons:
  normal=p.normal;axis=max(range(3),key=lambda k:abs(normal[k]));axes=[k for k in range(3) if k!=axis]
  for loop_id in p.loop_indices:
   co=ob.data.vertices[ob.data.loops[loop_id].vertex_index].co
   uv.data[loop_id].uv=(co[axes[0]]*.1,co[axes[1]]*.1)
 print('Finished',name,len(ob.data.vertices),'vertices',flush=True)
 return ob

start=len(parts)
podium(25,25,6.8)
for i in range(19):
 z=7.30+i*3.35
 setback=max(0,i-13)*.50
 w=22.6-setback*.95;d=21.7-setback*.58;cx=-setback*.14
 terrace=i in [5,11,17]
 prism('Curved ceramic floor edge',w,d,z,.28,'Ivory',2.15,cx)
 prism('Slab soffit shadow reveal',w-.22,d-.22,z-.13,.13,'Graphite',2.05,cx)
 gw=w-(4.0 if terrace else 1.32);gd=d-(4.6 if terrace else 1.32)
 prism('Deep set curtain glazing',gw,gd,z+.28,3.00,'Glass',1.75,cx)
 # Overscale floor-to-floor piers provide legible medium-frequency structure.
 if not terrace:
  for side in [-1,1]:
   for j in range(7):
    x=cx+(j-3)*(w-5)/6
    box('Deep vertical ceramic fin',(x,side*(d/2-.44),z+1.68),(.16,.75,3.07),'Ivory',.025)
   for j in range(6):
    y=(j-2.5)*(d-5)/5
    box('Side facade mullion',(cx+side*(w/2-.64),y,z+1.66),(.11,.13,2.99),'Metal')
 else:
  for side in [-1,1]:
   rail(cx,side*(d/2-.33),z+.28,w-4)
   rail(cx+side*(w/2-.34),0,z+.28,d-4,'Y')
   planter(cx+side*(w*.28),-d*.35,z+.28,3.5,1.1)
  for x in [-w*.36,w*.36]:box('Terrace structural pier',(cx+x,0,z+1.7),(.5,gd,3.1),'Ivory',.06)
# Low-profile roof framed by two rising sail fins.
rz=7.3+19*3.35
prism('Setback crown',18.9,19.6,rz,.52,'Ivory',2.1,-.35)
roof_plant(-.35,2,rz+.52,5)
planter(-.35,-6.4,rz+.52,9,1.8)
for side in [-1,1]:beam('Crown tapered vane',(side*7.8,7.8,rz),(side*7.8,-3.5,rz+3.0),.26,.7)
A=finish('Solstice_TerraceTower_A',start)

start=len(parts)
podium(27,24,6.4)
# Two unequally tall inhabited blades with a 5m open vertical slot.
for side,count in [(-1,18),(1,14)]:
 cx=side*7.6
 for i in range(count):
  z=6.91+i*3.5
  # Solid ceramic end walls versus broad recessed glass flanks.
  d=20.0-max(0,i-count+4)*.65
  prism('Blade floor',10.2,d,z,.26,'Ivory',1.4,cx)
  prism('Blade glazing',9.3,d-1.2,z+.26,3.19,'Glass',1.12,cx)
  for end in [-1,1]:
   box('Massive ceramic end wall',(cx,end*(d/2-.38),z+1.80),(8.1,.72,3.08),'Ivory',.12)
   # Singular inset vertical service notch breaks the wall without greeble.
   box('End wall inset reveal',(cx+side*2.65,end*(d/2+.001),z+1.68),(.35,.045,2.8),'Graphite')
  for j in range(7):
   y=(j-3)*(d-4)/6
   box('Blade glass mullion',(cx+side*4.68,y,z+1.84),(.08,.11,3.1),'Metal')
  # Dark slot-facing glazing is bridged only on 3 intentional floor levels.
  if i in [3,9,13]:
   prism('Occupied skybridge deck',8.2,8.0,z,.32,'Ivory',.6,0,1.0)
   box('Skybridge glazing',(0,1.0,z+1.60),(6.0,6.7,2.8),'Glass')
   box('Skybridge roof',(0,1.0,z+3.1),(7.2,8.0,.30),'Ivory',.15)
 z=6.91+count*3.5
 prism('Blade roof terrace',10.2,18.2,z,.4,'Ivory',1.4,cx)
 roof_plant(cx,3,z+.4,3.4)
 planter(cx,-5.9,z+.4,5.8,1.5)
 rail(cx,-8.8,z+.4,7.7)
# A deep projecting shared observation terrace reinforces the two-blade silhouette.
prism('Observation garden platform',26,9.5,42.1,.60,'Ivory',1.3,0,-6.6)
rail(0,-11.1,42.7,23)
for x in [-10,10]:planter(x,-7,42.7,3.0,2)
B=finish('Solstice_SplitTower_B',start)

# Revised geology: fractured polygonal headland, interrupted bedding and isolated ledge scrub.
start=len(parts)
N=196;L=63
verts=[];faces=[]
outline_points=[(-42,-20),(-29,-29),(-14,-22),(-8,-29),(6,-26),(14,-12),(34,-11),(40,3),(31,20),(12,24),(2,16),(-14,25),(-29,18),(-37,4)]
P=len(outline_points)
def wrap(a):return (a+pi)%(2*pi)-pi
def gauss(a,c,s):return math.exp(-(wrap(a-c)/s)**2)
def edgepoint(k):
 t=k/N*P;i=int(t);f=t-i;a=outline_points[i];b=outline_points[(i+1)%P]
 return a[0]*(1-f)+b[0]*f,a[1]*(1-f)+b[1]*f
def rock_noise(x,y,seed=0):
 ix=math.floor(x);iy=math.floor(y);u=x-ix;v=y-iy
 def val(a,b):return 2*((sin(a*127.1+b*311.7+seed*73.13)*43758.5453)%1)-1
 n00=val(ix,iy);n10=val(ix+1,iy);n01=val(ix,iy+1);n11=val(ix+1,iy+1)
 if u+v<=1:return n00+(n10-n00)*u+(n01-n00)*v
 return n11+(n01-n11)*(1-u)+(n10-n11)*(1-v)
for j in range(L):
 t=j/(L-1)
 for k in range(N):
  a=2*pi*k/N;x0,y0=edgepoint(k)
  h=36+5*sin(x0*.072)+3*cos(y0*.12)+2*sin(a*3.0+.7)
  # Coherent planar masses. Small surface deviations preserve their large angular faces.
  taper=1-.09*t
  shelf=.20*(1-math.tanh((t-.13)/.065))/2*(.25+.75*gauss(a,1.25,1.1))
  bedding=0
  for center,span,level,depth in [(.55,.6,.27,.040),(1.5,.50,.54,-.032),(2.6,.55,.42,.031),(3.3,.40,.69,-.025),(4.4,.62,.21,.037),(5.2,.44,.59,.03),(5.9,.34,.81,-.023)]:
   zbed=level+.018*sin(a*3+center)
   bedding+=depth*gauss(a,center,span)*(1-math.tanh((t-zbed)/.014))/2
  fractures=0
  for c,w,depth in [(.39,.032,.08),(.98,.041,.14),(1.91,.033,.09),(2.63,.041,.12),(3.42,.035,.08),(4.01,.032,.105),(4.71,.048,.09),(5.39,.029,.08),(5.98,.033,.11)]:
   fractures+=depth*gauss(a,c+.019*sin(t*4+c),w)*(sin(pi*t)**.40)
  micro=.003*sin(a*83+t*29)+.002*sin(a*137-t*37)+.0018*cos(a*57+t*97)
  # Bedding plates shift laterally at fault lines, rather than inflating into rounded rings.
  u=k/N*265;zapprox=t*(h+3)
  # Fractured planar noise at three physical scales, subtly warped to avoid a grid.
  uw=u+.75*sin(zapprox*.37);zw=zapprox+.7*sin(u*.26)
  rupture=1.35*rock_noise(uw/7.5,zw/5.7,3)+.64*rock_noise(uw/3.1,zw/2.4,17)+.20*rock_noise(uw/1.8,zw/1.0,41)
  r=taper+shelf+bedding-fractures+micro+rupture*min(1,t*9)/math.sqrt(x0*x0+y0*y0)
  x=x0*r+2.0*t;y=y0*r-1.0*t
  z=-3+t*(h+3)+.13*sin(a*51+t*15)*sin(pi*t)
  verts.append((x,y,z))
for j in range(L-1):
 for k in range(N):
  kn=(k+1)%N;v=j*N+k;vn=j*N+kn
  faces.append((v,vn,vn+N,v+N))
# Broad broken summit with smaller planar breaks, avoiding one central radial peak.
K=24
for j in range(1,K+1):
 f=1-j/(K+1)
 for k in range(N):
  a=2*pi*k/N;outer=verts[(L-1)*N+k]
  x=2+(outer[0]-2)*f;y=-1+(outer[1]+1)*f
  centerh=38+3*sin(x*.10)+2*cos(y*.16)
  z=outer[2]*f+centerh*(1-f)+(1.6*rock_noise(x/7.1,y/5.9,11)+.65*rock_noise(x/3.1,y/3.5,23))*sin(pi*f)
  verts.append((x,y,z))
for j in range(K):
 row=L-1+j
 for k in range(N):
  kn=(k+1)%N;v=row*N+k;vn=row*N+kn;faces.append((v,vn,vn+N,v+N))
ci=len(verts);verts.append((2,-1,40))
for k in range(N):faces.append(((L+K-1)*N+k,(L+K-1)*N+(k+1)%N,ci))
rockob=mesh('Faulted limestone planes',verts,faces,'Rock',True)
# Sharp creases survive; gentler weathered surfaces retain interpolated normals.
rockob.data.set_sharp_from_angle(angle=math.radians(27))
for x,y,z,w,d,h,angle in [(-27,-28,.3,13,6,3.8,-.3),(-15,-29,0,11,6,2.4,.4),(18,-15,.4,9,5,3.4,-.5),(33,10,.7,10,6,5,.6),(-38,5,.2,11,5,4,-.8)]:
 p=[(-.5,-.5,0),(.4,-.53,0),(.56,.26,0),(-.36,.51,0),(-.41,-.38,.65),(.3,-.45,1),(.41,.24,.80),(-.25,.42,.76)]
 v=[(x+w*a*cos(angle)-d*b*sin(angle),y+w*a*sin(angle)+d*b*cos(angle),z+h*c) for a,b,c in p]
 mesh('Wave cut fallen slab',v,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],'Rock')
# Small three-dimensional scrub groups replace the rejected polygon-color patches.
for j,k,s in [(4,20,1.05),(7,25,.85),(4,82,1.1),(9,110,.9),(14,133,1.3),(9,170,.8),(5,181,.9),(12,44,1.2)]:
 x,y,z=verts[(L+j)*N+k]
 for dx,dy,ss in [(0,0,s),(s*.7,.5*s,s*.65),(-s*.5,s*.3,s*.6)]:plant(x+dx,y+dy,z-.05,ss)
C=finish('Solstice_CoastalCliff_C',start)
exec(compile(open(os.path.join(ROOT,'rock3_material.py')).read(),'rock3_material.py','exec'))

assets=[A,B,C]
stats={}
for ob in assets:
 bpy.ops.object.select_all(action='DESELECT');ob.select_set(True);bpy.context.view_layer.objects.active=ob
 # Bake export axis conversion into FBX basis. Blender -Y forward becomes Unity +Z.
 bpy.ops.export_scene.fbx(filepath=os.path.join(ROOT,'exports',ob.name+'.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_space_transform=True,use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,path_mode='COPY',embed_textures=True)
 ob.data.calc_loop_triangles()
 stats[ob.name]={'vertices':len(ob.data.vertices),'triangles':len(ob.data.loop_triangles),'materials':[m.name for m in ob.data.materials],'dimensions_metres_blender_xyz':list(ob.dimensions),'origin':'local ground center; cliff underwater skirt extends -3m','fbx_axes':'Y up, -Z forward export convention for Unity; model front is Blender -Y','renderers':1}
with open(os.path.join(ROOT,'asset_stats.json'),'w') as f:json.dump(stats,f,indent=2)
# Asset transforms below are for the separate inspection scene only; exported assets stay at origin.
A.location=(-31,5,0);B.location=(3,10,0);C.location=(64,30,0)
# Finite ground and ocean-blue world provide readable contact and neutral material assessment.
mat=bpy.data.materials.new('Preview ground only');mat.diffuse_color=(.24,.31,.31,1);mat.use_nodes=True;mat.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.24,.31,.31,1);mat.node_tree.nodes.get('Principled BSDF').inputs['Roughness'].default_value=.8
bpy.ops.mesh.primitive_plane_add(size=2000,location=(0,0,-3.05));ground=bpy.context.object;ground.name='PREVIEW ONLY Ground';ground.data.materials.append(mat)
world=bpy.data.worlds.new('Clear coastal studio');scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.40,.57,.70,1);world.node_tree.nodes['Background'].inputs[1].default_value=.5
bpy.ops.object.light_add(type='SUN',location=(0,0,130));sun=bpy.context.object;sun.rotation_euler=(math.radians(24),math.radians(-28),math.radians(-35));sun.data.energy=2.5;sun.data.angle=.075
bpy.ops.object.light_add(type='AREA',location=(-30,-70,130));light=bpy.context.object;light.data.energy=200000;light.data.shape='DISK';light.data.size=100;light.rotation_euler=(Vector((10,0,30))-light.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.object.camera_add();cam=bpy.context.object;scene.camera=cam;cam.data.lens=55;cam.data.clip_end=2500

def render(name,pos,target,lens=55,res=(1600,1100)):
 cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.lens=lens;scene.render.resolution_x=res[0];scene.render.resolution_y=res[1];scene.render.filepath=os.path.join(ROOT,'renders',name+'.png');bpy.ops.render.render(write_still=True)

bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Solstice_Environment_Kit.blend'))
bpy.ops.file.make_paths_relative()
render('01-kit-hero',(-138,-190,118),(22,14,28),52)
render('02-tower-A-medium',(-91,-85,48),(-31,5,36),57,(1100,1350))
render('03-cliff-medium',(122,-79,63),(65,29,17),55,(1400,1050))
render('04-runtime-distance',(175,-300,34),(5,17,32),52,(1600,900))
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Solstice_Environment_Kit.blend'))
