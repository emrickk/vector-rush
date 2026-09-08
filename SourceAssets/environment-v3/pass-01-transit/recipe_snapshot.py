"""Author the bounded Nocturne transit family. Never writes live Unity assets."""
import bpy, bmesh, math, os, json, hashlib, shutil
from mathutils import Vector

ROOT=os.path.dirname(os.path.abspath(__file__))
OUT=os.path.join(ROOT,'pass-01-transit')
EVIDENCE=os.path.abspath(os.path.join(ROOT,'../../evidence/environment-v3/pass-01-transit'))
os.makedirs(OUT,exist_ok=True);os.makedirs(EVIDENCE,exist_ok=True)
BLEND=os.path.join(OUT,'Nocturne_Transit_Kit.blend')
if os.path.exists(BLEND):raise RuntimeError('Immutable pass already exists; choose a new pass.')
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=16;scene.cycles.use_denoising=False
scene.render.threads_mode='FIXED';scene.render.threads=4
scene.render.resolution_x=1400;scene.render.resolution_y=1000;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
M={}
for name,col,rough,metal,emit in [
 ('Concrete',(.38,.46,.51),.64,.05,0),('Charcoal',(.035,.052,.063),.62,.12,0),
 ('Glass',(.024,.065,.083),.24,.35,0),('Metal',(.22,.28,.31),.44,.55,0),
 ('WarmWindow',(.52,.31,.14),.58,0,.65),('Interior',(.18,.15,.12),.78,0,0)]:
 m=bpy.data.materials.new(name);m.diffuse_color=(*col,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF')
 p.inputs['Base Color'].default_value=(*col,1);p.inputs['Roughness'].default_value=rough;p.inputs['Metallic'].default_value=metal
 if emit:p.inputs['Emission Color'].default_value=(*col,1);p.inputs['Emission Strength'].default_value=emit
 M[name]=m
assets={};current=None
def begin(name):
 global current
 current=bpy.data.collections.new(name+'_SOURCE');scene.collection.children.link(current);assets[name]=[]
def mesh(name,verts,faces,mat='Concrete',bevel=0):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();ob=bpy.data.objects.new(name,me);current.objects.link(ob)
 ob.data.materials.append(M[mat]);assets[current.name.removesuffix('_SOURCE')].append(ob)
 if bevel:
  mod=ob.modifiers.new('Designed edge radius','BEVEL');mod.width=bevel;mod.segments=2
  norm=ob.modifiers.new('Area weighted surface normals','WEIGHTED_NORMAL');norm.keep_sharp=True
 return ob
def box(name,loc,size,mat='Concrete',bevel=.04):
 x,y,z=loc;a,b,c=[n/2 for n in size]
 verts=[(x+sx*a,y+sy*b,z+sz*c) for sx,sy,sz in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
 return mesh(name,verts,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],mat,bevel)
def plan_slab(name,xy,z,thickness,mat='Concrete',slope=0,bevel=.06):
 n=len(xy);v=[(x,y,z+slope*x+dz) for dz in [-thickness/2,thickness/2] for x,y in xy]
 return mesh(name,v,[tuple(range(n-1,-1,-1)),tuple(range(n,n*2))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)],mat,bevel)
def beam(name,a,b,width,depth,mat='Concrete'):
 ob=box(name,(0,0,0),(width,depth,(Vector(b)-Vector(a)).length),mat,min(width*.08,.045))
 ob.location=(Vector(a)+Vector(b))/2;ob.rotation_euler=(Vector(b)-Vector(a)).to_track_quat('Z','Y').to_euler();return ob
def taper(name,cx,cy,z,h,low,high,mat='Concrete'):
 v=[]
 for dz,(w,d) in [(0,low),(h,high)]:v.extend([(cx-w/2,cy-d/2,z+dz),(cx+w/2,cy-d/2,z+dz),(cx+w/2,cy+d/2,z+dz),(cx-w/2,cy+d/2,z+dz)])
 return mesh(name,v,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],mat,.06)
def window_bay(name,y,warm=False):
 # A deep open frame with separate glazing and partition reveals. No overlaid faces.
 box(name+' inset pane',(-2.46,y,5.6),(.10,4.9,5.3),'WarmWindow' if warm else 'Glass',.015)
 box(name+' deep sill',(-3.05,y,2.75),(1.25,5.4,.32),'Concrete',.04)
 box(name+' lintel',(-3.03,y,8.42),(1.20,5.4,.36),'Concrete',.04)
 for side in [-1,1]:box(name+' reveal jamb',(-3.02,y+side*2.64,5.6),(1.18,.28,5.65),'Concrete',.025)
 if warm:
  box(name+' opaque interior partition',(-2.57,y+.82,5.5),(.18,.14,5.1),'Charcoal',.012)
  box(name+' upper transom',(-2.57,y,7.55),(.18,4.8,.12),'Metal',.012)

begin('Nocturne_TransitStation_A')
platform=[(-11,-28),(-8,-32),(9.4,-32),(11.2,-27),(11.2,27),(8.5,32),(-8.8,32),(-11,28)]
plan_slab('Deep platform foundation',platform,.65,1.30,'Charcoal',bevel=.10)
plan_slab('Civic platform wearing course',[(x*.985,y*.994) for x,y in platform],1.42,.24,'Concrete',bevel=.06)
# Service spine deliberately occupies the back; the track side remains a real open platform.
box('Rear occupied concourse', (3.8,1.2,5.1),(12.4,54.8,7.1),'Charcoal',.14)
for i,y in enumerate([-22,-15.7,-9.4,-3.1,3.2,9.5,15.8,22.1]):window_bay('Concourse room %02d'%i,y,i in [1,2,5])
# Platform-level rhythm is construction, with grouped occupied rooms rather than pixel lights.
for y in [-26,-12,3,18,28]:
 taper('Tapered platform column',-4.5,y,1.55,9.6,(.82,1.22),(1.2,1.55))
 beam('Cantilever knee',(-4.5,y,8.5),(-9.65,y,11.88),.48,.68,'Concrete')
 beam('Roof transverse rib',(-10.2,y,11.72),(9.8,y,14.12),.35,.72,'Metal')
# Two formed roof leaves leave a continuous narrow clerestory between them.
left=[(-11.4,-28),(-8.2,-32),(1.5,-32),(1.5,31),(-8.8,31),(-11.4,27)]
right=[(2.9,-32),(8.8,-32),(11.6,-26),(11.6,27),(8.2,31),(2.9,31)]
plan_slab('Main folded cantilever roof',left,13.0,.72,'Concrete',.12,.075)
plan_slab('Rear roof return',right,13.0,.66,'Concrete',.12,.075)
box('Clerestory recessed dark glazing',(2.2,0,12.84),(1.38,60,.14),'Glass',.025)
for y in [-27,-12,3,18,28]:box('Clerestory cross brace',(2.2,y,13.24),(1.6,.18,.32),'Metal',.018)
# A dark underside band is a physical return, broken at the roof ends.
beam('Inner roof fascia',(-11.18,-26,11.57),(-11.18,25,11.57),.20,.38,'Metal')
# Asymmetric circulation head: small core, separated landing and rounded-off roof profile.
taper('Circulation tower plinth',5.8,-21,1.55,1.1,(9.1,11.6),(8.5,10.8))
box('Circulation stair core',(6.0,-21,11.7),(7.7,9.7,18.1),'Concrete',.20)
box('Recessed vertical stair light',(2.08,-21,11.6),(.12,2.3,15.7),'Glass',.025)
for level in [4.3,8.5,12.7,16.9]:
 box('Stair landing reveal',(1.98,-21,level),(.25,2.5,.23),'Metal',.015)
 box('Landing occupied pane',(1.99,-21.15,level+1.2),(.12,1.6,1.8),'WarmWindow',.015)
plan_slab('Circulation tower floating crown',[(1.6,-26.7),(9.8,-26.7),(10.3,-25.7),(10.3,-16),(9.2,-15.4),(1.6,-15.4)],21.4,.65,'Concrete',.025,.10)
box('Circulation mechanical termination',(6.4,-21.3,22.03),(5.5,6.3,.64),'Charcoal',.1)
# Occupied but restrained service level, seating and edge protection on outer two ends.
for y in [-17.5,5,23.5]:
 box('Platform bench solid base',(-7.1,y,1.95),(1.0,3.6,.64),'Charcoal',.05)
 box('Platform bench seat',(-7.1,y,2.33),(1.18,3.75,.18),'Concrete',.05)
for y in [-29.1,29.1]:
 for x in [-8,-4,0,4,8]:box('End platform guard upright',(x,y,2.18),(.10,.12,1.35),'Metal',.015)
 box('End platform solid lower rail',(0,y,1.82),(16.1,.18,.45),'Charcoal',.03)
 box('End platform cap',(0,y,2.85),(16.3,.12,.12),'Metal',.015)
for y in [-20,-2,17]:
 box('Recessed floor service channel',(-9.65,y,1.56),(.20,6,.035),'Metal',.004)
# Screened compact roof plant on rear return: hierarchy stays below the stair crown.
box('Plant bed',(7.9,17,14.5),(4.2,7.5,.45),'Charcoal',.07)
for y in [15,19]:
 box('Roof heat exchanger',(7.9,y,15.15),(2.8,2.6,.85),'Metal',.10)
 for x in [6.8,7.35,7.9,8.45,9.0]:box('Plant louvre',(x,y,15.6),(.20,2.4,.10),'Charcoal',.015)

begin('Nocturne_ServiceWorkshop_A')
box('Workshop grounded plinth',(0,0,.42),(8,18,.84),'Charcoal',.08)
box('Workshop body',(.6,0,3.25),(6.5,16.6,4.9),'Concrete',.13)
for y in [-5.2,0,5.2]:
 box('Deep service portal',(-2.78,y,3.0),(.32,4.25,3.5),'Charcoal',.045)
 box('Inset service door',(-2.96,y,2.92),(.06,3.5,2.95),'WarmWindow' if y==0 else 'Metal',.02)
 if y!=0:
  for z in [1.7,2.25,2.8,3.35,3.9]:box('Door folded panel seam',(-3.0,y,z),(.045,3.25,.045),'Charcoal',.003)
plan_slab('Workshop folded canopy',[(-4,-8.5),(3.5,-8.5),(4,-7.8),(4,7.8),(3.5,8.5),(-4,8.5)],5.9,.40,'Concrete',.075,.07)
for y in [-7.2,7.2]:beam('Workshop canopy knee',(-2.75,y,4.8),(-3.75,y,5.62),.22,.26,'Metal')
box('Workshop screened plant',(1,3,6.5),(3.6,4.4,.75),'Charcoal',.08)
for y in [1.5,2.25,3,3.75,4.5]:box('Plant grouped louvre',(1,y,6.94),(3.2,.22,.09),'Metal',.015)

begin('Nocturne_PlatformButtress_A')
box('Buttress foot',(0,0,.40),(4.4,7.2,.8),'Charcoal',.10)
taper('Tapered monolithic pier',.3,0,.8,8.6,(2.3,4.4),(2.7,5.3),'Concrete')
beam('Cantilever structural knee',(.3,0,6.1),(-1.65,0,10.6),1.05,4.5,'Concrete')
box('Cap bearing pad',(-.1,0,9.85),(4.1,6.7,.65),'Charcoal',.06)
box('Supported service ledge',(-.2,0,10.6),(5.1,8, .85),'Concrete',.08)
box('Recessed ledge strip',(-.2,0,11.08),(4.85,7.7,.11),'Charcoal',.025)

runtime=bpy.data.collections.new('EXPORT_RUNTIME');scene.collection.children.link(runtime)
exports={};stats={}
for name,source in assets.items():
 copies=[]
 for ob in source:
  cp=ob.copy();cp.data=ob.data.copy();runtime.objects.link(cp);copies.append(cp)
  bpy.ops.object.select_all(action='DESELECT');cp.select_set(True);bpy.context.view_layer.objects.active=cp
  for mod in list(cp.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
  bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 bpy.ops.object.select_all(action='DESELECT')
 for ob in copies:ob.select_set(True)
 bpy.context.view_layer.objects.active=copies[0];bpy.ops.object.join();ob=bpy.context.object;ob.name=name
 # Material slot deduplication, preserving exact face identities.
 mats=[];remap={}
 for i,m in enumerate(ob.data.materials):
  if m not in mats:mats.append(m)
  remap[i]=mats.index(m)
 ids=[remap[p.material_index] for p in ob.data.polygons];ob.data.materials.clear()
 for m in mats:ob.data.materials.append(m)
 for p,i in zip(ob.data.polygons,ids):p.material_index=i
 tri=ob.modifiers.new('Final runtime triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=tri.name)
 for old in list(ob.data.uv_layers):ob.data.uv_layers.remove(old)
 uv=ob.data.uv_layers.new(name='UV0_Metric_4m');ob.data.uv_layers.active_index=0;uv.active_render=True
 for p in ob.data.polygons:
  axis=max(range(3),key=lambda k:abs(p.normal[k]));axes=[k for k in range(3) if k!=axis]
  for li in p.loop_indices:
   co=ob.data.vertices[ob.data.loops[li].vertex_index].co;uv.data[li].uv=(co[axes[0]]*.25,co[axes[1]]*.25)
 bm=bmesh.new();bm.from_mesh(ob.data)
 nonmanifold=sum(not e.is_manifold for e in bm.edges);bm.free()
 degenerate=sum(p.area<1e-10 for p in ob.data.polygons)
 if nonmanifold or degenerate:raise RuntimeError('%s invalid: nonmanifold=%d degenerate=%d'%(name,nonmanifold,degenerate))
 coords=[v.co for v in ob.data.vertices];lo=[min(v[a] for v in coords) for a in range(3)];hi=[max(v[a] for v in coords) for a in range(3)]
 path=os.path.join(OUT,name+'.fbx')
 bpy.ops.export_scene.fbx(filepath=path,use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_space_transform=True,axis_forward='-Z',axis_up='Y',mesh_smooth_type='FACE',use_tspace=True,add_leaf_bones=False)
 exports[name]=ob
 stats[name]={'source_parts':len(source),'vertices':len(ob.data.vertices),'triangles':len(ob.data.polygons),'materials':[m.name for m in mats],'source_min_xyz':lo,'source_max_xyz':hi,'unity_dimensions_xyz':[hi[0]-lo[0],hi[2]-lo[2],hi[1]-lo[1]],'nonmanifold_edges':nonmanifold,'degenerate_triangles':degenerate,'uv0':'Metric tiling: 4 metres per tile; overlapping by design','fbx_sha256':hashlib.sha256(open(path,'rb').read()).hexdigest()}
 ob.hide_render=True;ob.hide_set(True)
# Keep editable source assets separate in the file, with station at origin.
for name,source in assets.items():
 if name=='Nocturne_ServiceWorkshop_A':
  for ob in source:ob.location.x+=29;ob.location.y+=3
 if name=='Nocturne_PlatformButtress_A':
  for ob in source:ob.location.x+=-26;ob.location.z+=0
studio=bpy.data.collections.new('STUDIO_NOT_EXPORTED');scene.collection.children.link(studio)
bpy.ops.mesh.primitive_plane_add(size=500,location=(0,0,-.09));ground=bpy.context.object;ground.name='Studio ground';ground.data.materials.append(M['Charcoal'])
for col in list(ground.users_collection):col.objects.unlink(ground)
studio.objects.link(ground)
world=bpy.data.worlds.new('Neutral daylight studio');scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.15,.19,.23,1);world.node_tree.nodes['Background'].inputs[1].default_value=.55
def area(name,loc,energy,size):
 data=bpy.data.lights.new(name,'AREA');data.energy=energy;data.shape='DISK';data.size=size;ob=bpy.data.objects.new(name,data);studio.objects.link(ob);ob.location=loc;ob.rotation_euler=(Vector((0,0,7))-ob.location).to_track_quat('-Z','Y').to_euler()
area('Large cool key',(-45,-35,70),65000,55);area('Architectural fill',(35,30,55),45000,45)
data=bpy.data.cameras.new('Studio architectural camera');cam=bpy.data.objects.new('Studio architectural camera',data);studio.objects.link(cam);cam.location=(-82,-93,61);target=Vector((0,0,9));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();data.type='ORTHO';data.ortho_scale=107;scene.camera=cam
scene.render.image_settings.file_format='PNG'
scene['AssetContract']='Staged source only. Runtime meshes at asset-local origin, editable workshop/buttress offset only for kit presentation.'
shutil.copyfile(__file__,os.path.join(OUT,'recipe_snapshot.py'))
with open(os.path.join(OUT,'asset-stats.json'),'w') as f:json.dump(stats,f,indent=2)
bpy.ops.wm.save_as_mainfile(filepath=BLEND)
print('STAGED_ASSETS_SAVED '+BLEND,flush=True)
scene.render.filepath=os.path.join(EVIDENCE,'01-transit-kit-studio.png');bpy.ops.render.render(write_still=True)
print('STUDIO_PREVIEW_COMPLETE',flush=True)
