"""Original authored landmark families. Build staged FBX and editable source, never edit existing assets."""
import bpy,bmesh,math,os,json,hashlib,shutil
from mathutils import Vector,Matrix
ROOT=os.path.dirname(os.path.abspath(__file__));OUT=os.path.join(ROOT,'pass-01-landmarks');os.makedirs(OUT,exist_ok=True)
BLEND=os.path.join(OUT,'Nocturne_Landmark_Kit.blend')
if os.path.exists(BLEND):raise RuntimeError('Immutable pass already exists. Choose a new pass.')
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
scene.render.engine='CYCLES';scene.cycles.samples=12;scene.cycles.use_denoising=True;scene.render.threads_mode='FIXED';scene.render.threads=6
scene.render.resolution_x=1400;scene.render.resolution_y=1200;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX'
M={}
for name,col,rough,metal,emit in [('Ceramic',(.52,.57,.56),.50,.04,0),('Concrete',(.30,.37,.40),.75,.03,0),('Structure',(.075,.105,.12),.50,.30,0),('Titanium',(.34,.40,.42),.34,.66,0),('Oxide',(.28,.17,.10),.58,.38,0),('Glass',(.035,.085,.10),.23,.25,0),('Occupied',(.39,.25,.13),.45,.08,.48),('Lamp',(.62,.44,.27),.4,.1,1.1)]:
 m=bpy.data.materials.new(name);m.diffuse_color=(*col,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*col,1);p.inputs['Roughness'].default_value=rough;p.inputs['Metallic'].default_value=metal
 if emit:p.inputs['Emission Color'].default_value=(*col,1);p.inputs['Emission Strength'].default_value=emit
 M[name]=m
assets={};current=None
def begin(name):
 global current
 current=bpy.data.collections.new(name+'_EDITABLE');scene.collection.children.link(current);assets[name]=[]
def mesh(name,verts,faces,mat='Concrete',bevel=0):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();ob=bpy.data.objects.new(name,me);current.objects.link(ob);ob.data.materials.append(M[mat]);assets[current.name.removesuffix('_EDITABLE')].append(ob)
 if bevel:
  mod=ob.modifiers.new('Manufactured edge bevel','BEVEL');mod.width=bevel;mod.segments=2
  mod=ob.modifiers.new('Weighted structural normals','WEIGHTED_NORMAL');mod.keep_sharp=True
 return ob
def box(name,loc,size,mat='Concrete',bevel=.055):
 x,y,z=loc;a,b,c=[s*.5 for s in size];v=[(x+u*a,y+w*b,z+t*c)for u,w,t in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
 return mesh(name,v,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],mat,bevel)
def slab(name,xy,z,h,mat='Ceramic',bevel=.1):
 n=len(xy);return mesh(name,[(x,y,z+dz)for dz in [-h/2,h/2]for x,y in xy],[tuple(range(n-1,-1,-1)),tuple(range(n,n*2))]+[(i,(i+1)%n,(i+1)%n+n,i+n)for i in range(n)],mat,bevel)
def beam(name,a,b,w,d,mat='Titanium',bevel=.06):
 ob=box(name,(0,0,0),(w,d,(Vector(b)-Vector(a)).length),mat,bevel);direction=(Vector(b)-Vector(a)).normalized();ref=Vector((0,1,0))
 if abs(direction.dot(ref))>.99:ref=Vector((0,0,1))
 side=ref.cross(direction).normalized();depth=direction.cross(side).normalized();ob.location=(Vector(a)+Vector(b))/2;ob.rotation_euler=Matrix((side,depth,direction)).transposed().to_euler();return ob
def blade(name,a,b,low,high,mat='Ceramic'):
 v=[]
 for c,s in [(a,low),(b,high)]:
  x,y,z=c;w,d=s;v.extend([(x-w/2,y-d/2,z),(x+w/2,y-d/2,z),(x+w/2,y+d/2,z),(x-w/2,y+d/2,z)])
 return mesh(name,v,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],mat,.11)
def lathe(name,cx,cy,profile,mat='Ceramic',n=48):
 v=[(cx+r*math.cos(i*math.tau/n),cy+r*math.sin(i*math.tau/n),z)for z,r in profile for i in range(n)];faces=[tuple(range(n-1,-1,-1)),tuple(range((len(profile)-1)*n,len(profile)*n))]
 for k in range(len(profile)-1):faces.extend([(k*n+i,k*n+(i+1)%n,(k+1)*n+(i+1)%n,(k+1)*n+i)for i in range(n)])
 ob=mesh(name,v,faces,mat)
 for p in ob.data.polygons:
  if len(p.vertices)==4:p.use_smooth=True
 return ob
def collar(name,cx,cy,z,outer,inner,h,mat='Titanium',n=48):
 v=[(cx+r*math.cos(i*math.tau/n),cy+r*math.sin(i*math.tau/n),zz)for r,zz in [(outer,z-h/2),(outer,z+h/2),(inner,z+h/2),(inner,z-h/2)]for i in range(n)];faces=[]
 for k in range(4):faces.extend([(k*n+i,k*n+(i+1)%n,((k+1)%4)*n+(i+1)%n,((k+1)%4)*n+i)for i in range(n)])
 ob=mesh(name,v,faces,mat)
 for p in ob.data.polygons:p.use_smooth=(p.index//n in [0,2])
 return ob
def pipe(name,a,b,r,mat='Titanium'):
 ob=lathe(name,0,0,[(-math.dist(a,b)/2,r),(math.dist(a,b)/2,r)],mat,24);ob.location=(Vector(a)+Vector(b))/2;ob.rotation_euler=(Vector(b)-Vector(a)).to_track_quat('Z','Y').to_euler();return ob

def service_front(prefix,x,ys,base,sign=1):
 for i,y in enumerate(ys):
  box(prefix+' deep portal %02d'%i,(x,y,base+7),(.55,7.4,10.8),'Structure',.04)
  box(prefix+' inset shutter %02d'%i,(x+sign*.29,y,base+6.5),(.12,6.1,8.6),'Titanium',.035)
  for z in [base+3,base+5.5,base+8,base+10]:box(prefix+' folded shutter panel',(x+sign*.37,y,z),(.055,5.9,.10),'Structure',.012)
  box(prefix+' clerestory inset',(x+sign*.29,y,base+14),(.15,6.1,2.9),'Occupied' if i in [1,3]else'Glass',.03)
  box(prefix+' clerestory transom',(x+sign*.39,y,base+14.2),(.12,6.3,.16),'Titanium',.018)

begin('Nocturne_SplitSignal_Mast')
slab('Deep keyed foundation',[(-26,-32),(24,-32),(26,-29),(26,30),(23,32),(-25,32),(-26,29)],1,2,'Structure')
slab('Raised civic apron',[(-25,-31),(23,-31),(25,-28),(25,29),(22,31),(-24,31),(-25,28)],2.35,.65,'Concrete')
# The signal core is an actual leaning pair, not a solid tower with an outline.
blade('West primary ceramic pylon',(-10,-9,2.8),(-22,-9,123),(8,10),(4.0,5.8))
blade('East lower ceramic pylon',(12,-9,2.8),(-3.5,-9,112),(7,9),(3.8,5.4))
beam('Primary dark inner structural seam',(-6.3,-9,28),(-19.9,-9,121),.55,6.0,'Structure')
beam('Secondary dark inner structural seam',(8.5,-9,29),(-5.4,-9,109),.48,5.6,'Structure')
# Sparse cross structure preserves large sky openings above the lower service building.
beam('Lower sky tie',(-16.5,-9,64),(3.0,-9,64),2.4,5.0,'Titanium')
beam('Diagonal load transfer',(-16.4,-9,63),(0.6,-9,82),1.8,4.2,'Structure')
beam('Skyroom bearing transfer',(-18,-9,82),(0.5,-9,82),2.8,12.6,'Structure')
# Offset two-storey observation / switching room, thick ceramic floor and roof returns.
xy=[(-25,-19),(-23,-21),(5,-21),(8,-17),(8,-2),(5,1),(-23,1),(-25,-2)]
slab('Skyroom stepped lower tray',xy,84.0,2.0,'Ceramic')
slab('Skyroom recessed lower shadow',[(x*.985,y)for x,y in xy],85.35,.65,'Structure')
slab('Skyroom floating crown',[(x-0.2,y)for x,y in xy],97.0,1.9,'Ceramic')
box('Skyroom insulated core',(-8.5,-10,91),(27,15,10.0),'Structure',.15)
for side in [-1,1]:
 y=-10+side*8.05
 for i,x in enumerate([-20,-14,-8,-2,4]):
  box('Deep skyroom glazing', (x,y,90.8),(5.6,.13,8.1),'Occupied'if i in [1,2]else'Glass',.025)
  box('Skyroom projecting mullion',(x-2.83,y+side*.22,90.8),(.28,.65,8.9),'Titanium',.02)
 box('Skyroom two-storey transom',(-8,y+side*.18,91.6),(29,.28,.42),'Ceramic',.02)
 box('Skyroom deep continuous sill',(-8,y+side*.35,86.5),(29,.82,.48),'Ceramic',.035)
for x in [-22.2,5.2]:
 box('Skyroom side recessed glass',(x,-10,90.8),(.14,14.8,8),'Glass',.025)
 for y in [-15,-9,-3]:box('Skyroom side jamb',(x,-10+(y+10),90.8),(.55,.25,8.5),'Titanium',.025)
box('Skyroom roof plant',(-12,-9,99.4),(15,11,2.8),'Structure',.14)
for x in [-17,-12,-7]:box('Skyroom plant vent',(x,-9,101.0),(2.8,8,.35),'Titanium',.04)
# Road-facing lower annex makes the landmark have foreground and middle scale.
box('Grounded communications service hall',(15,1,17.8),(19,53,30.2),'Concrete',.16)
service_front('Signals service',24.62,[-20,-9,2,13,24],15,1)
slab('Long maintenance cantilever',[ (4,-28),(24,-28),(26,-25),(26,28),(24,29),(4,29)],35.2,1.25,'Ceramic')
for y in [-22,-5,12,26]:beam('Service canopy knee',(22.5,y,29.8),(25.8,y,34.3),.72,.95,'Titanium')
box('Lower back equipment hall',(-10,19,12.5),(28,22,19.6),'Structure',.15)
box('Back equipment roof',(-10,19,22.7),(29,23,.85),'Concrete',.08)
for x in [-18,-8,1]:
 box('Transformer ceramic enclosure',(x,19,26),(7,14,5.6),'Concrete',.12)
 for y in [15,19,23]:box('Transformer grouped louvre',(x,y,29),(5.9,1.4,.34),'Titanium',.04)
# Flanking lower terraces and side service stair: deliberately broad steps.
for i in range(7):box('External planted service stair',(-21.5,-25+i*.86,2.8+i*.55),(7,1.2,.56),'Concrete',.035)
box('Low service stair landing',(-21.5,-17.5,7),(7,5,.75),'Concrete')
for y in [-22,22]:box('Under canopy lamp housing',(24.9,y,34.3),(.4,3.2,.25),'Structure',.025);box('Under canopy lamp glass',(25.1,y,34.15),(.2,2.5,.12),'Lamp',.015)

begin('Nocturne_ThermalExchange_Works')
slab('Thermal site foundation',[(-22,-42),(19,-42),(22,-39),(22,39),(19,42),(-22,42)],1.05,2.1,'Structure')
# Substantial building, not three tanks resting on a paper-thin plane.
box('Three-storey service podium',(0,0,15.7),(42,80,27.4),'Concrete',.17)
box('Podium mechanical shadow fascia',(0,0,29.9),(43,81,1.05),'Structure',.07)
slab('Thermal working deck',[(-22,-41),(19,-41),(22,-38),(22,38),(19,41),(-22,41)],31.0,1.25,'Concrete')
service_front('Thermal service',-21.35,[-33,-21,-9,3,15,27],7,-1)
# Additional end portals keep all approach elevations intentional.
for y in [-40.08,40.08]:
 for x in [-14,-3,8]:
  box('End loading recess',(x,y,15),(8.7,.25,16),'Structure',.05)
  box('End shutter inset',(x,y+math.copysign(.16,y),14),(7.5,.12,13.6),'Titanium',.045)
  for z in [9,13,17]:box('End shutter seam',(x,y+math.copysign(.23,y),z),(7.3,.055,.10),'Structure',.01)
# Unequal cylindrical silhouettes. Wide cladding and annular construction read at speed.
for index,(y,r,top) in enumerate([(-24,8.2,75.5),(0,9.8,90.5),(24,7.9,64.0)]):
 x=8.0;bottom=32.0
 lathe('Drum %d pedestal'%index,x,y,[(31.7,r+.7),(34.0,r+.7),(35.3,r)],'Structure')
 lathe('Drum %d pressure body'%index,x,y,[(34.9,r*.93),(37,r),(top-9,r),(top-7,r*.965)],'Ceramic')
 collar('Drum %d top heat rejection shroud'%index,x,y,top-3.3,r*.995,r*.81,6.6,'Titanium')
 lathe('Drum %d recessed upper fan well'%index,x,y,[(top-7.1,r*.805),(top-6.8,r*.805)],'Structure')
 for frac in [.17,.54,.83]:
  z=37+(top-44)*frac
  collar('Drum %d circumferential cladding joint'%index,x,y,z,r+.11,r-.16,.38,'Structure')
 collar('Drum %d lower oxide thermal collar'%index,x,y,37.8,r+.23,r-.18,1.1,'Oxide')
 collar('Drum %d upper ceramic lip'%index,x,y,top+.15,r+0.20,r*.77,.60,'Ceramic')
 # Six broad reinforcing folds on the shroud, not a luminous grid.
 for j in range(6):
  a=j*math.tau/6;loc=(x+(r*.98)*math.cos(a),y+(r*.98)*math.sin(a),top-3.2)
  ob=box('Drum shroud reinforcing fold',loc,(.36,1.1,5.9),'Titanium',.025);ob.rotation_euler.z=a
 lathe('Drum fan spindle',x,y,[(top-6.7,1.1),(top-5.2,1.1)],'Titanium',24)
 for j in range(3):
  a=j*math.tau/3;ob=box('Recessed fan blade',(x+math.cos(a)*r*.4,y+math.sin(a)*r*.4,top-5.7),(r*.73,1.75,.34),'Structure',.08);ob.rotation_euler.z=a+.14
 # A broad insulated feed leads to the road-facing pipe gallery.
 pipe('Drum trunk feed',(x-r,y,39.8),(-13.5,y,39.8),1.25,'Oxide')
 pipe('Descending thermal main',(-13.5,y,9),(-13.5,y,39.8),1.25,'Titanium')
 for z in [18,31,38]:collar('Pipe saddle coupling',-13.5,y,z,1.46,1.18,.55,'Structure',24)
# Open pipe bridge/canopy layered in front of the drums. Every member is authored.
for y in [-36,-12,12,36]:
 blade('Pipe gallery tapered pier',(-18,y,2.2),(-18,y,44.5),(3.6,3.6),(2.0,2.5),'Concrete')
 beam('Pipe gallery transverse bearer',(-19.5,y,44.6),(0,y,44.6),1.3,2.4,'Titanium')
 beam('Open gallery knee',(-18,y,38),(-10,y,44.2),1.0,1.3,'Structure')
for x,z in [(-17.7,47.1),(-13.8,48.0)]:
 pipe('Elevated long pipe bridge',(x,-39,z),(x,39,z),1.3,'Titanium')
 for y in [-32,-12,12,32]:
  # Crosswise couplings get their orientation from the pipe axis.
  ob=collar('Long main mechanical sleeve',0,0,0,1.55,1.21,.7,'Oxide',24);ob.location=(x,y,z);ob.rotation_euler.x=math.pi/2
slab('Service gallery folded rain canopy',[(-22.7,-38),(-12,-38),(-10,-35),(-10,35),(-12,38),(-22.7,38)],36.2,.80,'Ceramic')
for y in [-29,27]:box('Gallery concealed light case',(-22.2,y,35.58),(.45,4,.35),'Structure');box('Gallery concealed light pane',(-22.45,y,35.5),(.10,3.3,.17),'Lamp',.012)
box('Lower pump annex',(-16,-32,34), (10,14,4.5),'Structure',.1)
box('Pump annex access roof',(-16,-32,36.6),(10.4,14.5,.48),'Concrete',.05)

runtime=bpy.data.collections.new('EXPORT_RUNTIME');scene.collection.children.link(runtime);exports={};stats={}
for name,source in assets.items():
 copies=[]
 for ob in source:
  cp=ob.copy();cp.data=ob.data.copy();runtime.objects.link(cp);copies.append(cp);bpy.ops.object.select_all(action='DESELECT');cp.select_set(True);bpy.context.view_layer.objects.active=cp
  for mod in list(cp.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
  bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 bpy.ops.object.select_all(action='DESELECT')
 for cp in copies:cp.select_set(True)
 bpy.context.view_layer.objects.active=copies[0];bpy.ops.object.join();ob=bpy.context.object;ob.name=name
 mats=[];remap={}
 for i,m in enumerate(ob.data.materials):
  if m not in mats:mats.append(m)
  remap[i]=mats.index(m)
 ids=[remap[p.material_index]for p in ob.data.polygons];ob.data.materials.clear()
 for m in mats:ob.data.materials.append(m)
 for p,i in zip(ob.data.polygons,ids):p.material_index=i
 tri=ob.modifiers.new('Runtime triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=tri.name)
 for old in list(ob.data.uv_layers):ob.data.uv_layers.remove(old)
 uv=ob.data.uv_layers.new(name='UV0_Metric_4m');uv.active_render=True
 for p in ob.data.polygons:
  axis=max(range(3),key=lambda k:abs(p.normal[k]));axes=[k for k in range(3)if k!=axis]
  for li in p.loop_indices:
   co=ob.data.vertices[ob.data.loops[li].vertex_index].co;uv.data[li].uv=(co[axes[0]]*.25,co[axes[1]]*.25)
 bm=bmesh.new();bm.from_mesh(ob.data);nonmanifold=sum(not e.is_manifold for e in bm.edges);bm.free();degenerate=sum(p.area<1e-10 for p in ob.data.polygons)
 if nonmanifold or degenerate:raise RuntimeError('%s invalid: nonmanifold=%d degenerate=%d'%(name,nonmanifold,degenerate))
 coords=[v.co for v in ob.data.vertices];lo=[min(v[a]for v in coords)for a in range(3)];hi=[max(v[a]for v in coords)for a in range(3)]
 path=os.path.join(OUT,name+'.fbx');bpy.ops.export_scene.fbx(filepath=path,use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_space_transform=True,axis_forward='-Z',axis_up='Y',mesh_smooth_type='FACE',use_tspace=True,add_leaf_bones=False)
 stats[name]=dict(editable_parts=len(source),runtime_meshes=1,material_slots=[m.name for m in mats],vertices=len(ob.data.vertices),triangles=len(ob.data.polygons),source_min_xyz=lo,source_max_xyz=hi,unity_dimensions_xyz=[hi[0]-lo[0],hi[2]-lo[2],hi[1]-lo[1]],nonmanifold_edges=nonmanifold,degenerate_triangles=degenerate,uv0='4 m metric box projection, overlapping for tiling by design',fbx_sha256=hashlib.sha256(open(path,'rb').read()).hexdigest())
 exports[name]=ob;ob.hide_render=True;ob.hide_set(True)
 # Source objects remain independently editable, organized per landmark.
 for obj in source:obj.hide_render=True
studio=bpy.data.collections.new('STUDIO_NOT_EXPORTED');scene.collection.children.link(studio)
def studio_object(ob):
 for co in list(ob.users_collection):co.objects.unlink(ob)
 studio.objects.link(ob)
bpy.ops.mesh.primitive_plane_add(size=700,location=(0,0,-.08));ground=bpy.context.object;ground.name='Neutral ground';ground.data.materials.append(M['Structure']);studio_object(ground)
world=bpy.data.worlds.new('Architectural studio');scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.20,.25,.30,1);world.node_tree.nodes['Background'].inputs[1].default_value=.55
for name,loc,energy,size in [('Broad key',(65,-65,130),190000,70),('Soft opposite',(-60,35,100),140000,65)]:
 data=bpy.data.lights.new(name,'AREA');data.energy=energy;data.shape='DISK';data.size=size;ob=bpy.data.objects.new(name,data);studio.objects.link(ob);ob.location=loc;ob.rotation_euler=(Vector((0,0,50))-ob.location).to_track_quat('-Z','Y').to_euler()
data=bpy.data.cameras.new('Studio camera');cam=bpy.data.objects.new('Studio camera',data);studio.objects.link(cam);scene.camera=cam;data.type='ORTHO';data.ortho_scale=146
scene.render.image_settings.file_format='PNG';scene['Contract']='Runtime exports have one combined mesh per landmark, editable parts retained separately. Blender X/Y/Z -> Unity -X/Z/Y with existing importer convention.'
shutil.copyfile(__file__,os.path.join(OUT,'recipe_snapshot.py'))
with open(os.path.join(OUT,'asset-stats.json'),'w')as f:json.dump(stats,f,indent=2)
# Save source with mast visible by default and thermal collection hidden only in viewport.
for ob in assets['Nocturne_SplitSignal_Mast']:ob.hide_render=False
for ob in assets['Nocturne_ThermalExchange_Works']:ob.hide_set(True)
cam.location=(160,-175,128);cam.rotation_euler=(Vector((0,0,56))-cam.location).to_track_quat('-Z','Y').to_euler()
bpy.ops.wm.save_as_mainfile(filepath=BLEND);print('LANDMARK_EXPORTS_READY',flush=True)
scene.render.filepath=os.path.join(OUT,'01-mast-studio.png');bpy.ops.render.render(write_still=True)
for ob in assets['Nocturne_SplitSignal_Mast']:ob.hide_render=True
for ob in assets['Nocturne_ThermalExchange_Works']:ob.hide_render=False;ob.hide_set(False)
cam.location=(-155,-180,118);cam.rotation_euler=(Vector((0,0,43))-cam.location).to_track_quat('-Z','Y').to_euler();data.ortho_scale=119
scene.render.filepath=os.path.join(OUT,'02-thermal-studio.png');bpy.ops.render.render(write_still=True)
print('LANDMARK_STUDIO_PREVIEWS_READY',flush=True)
