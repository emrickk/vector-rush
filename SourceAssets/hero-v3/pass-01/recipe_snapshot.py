"""KESTREL V3: form/construction rebuild. Nose -Y; up +Z; metres.
Blender --background --python build_hero_v3.py -- --output .../pass-01 --evidence .../pass-01
Every output directory is immutable once its main blend exists. New revisions need new dirs.
"""
import bpy, bmesh, math, json, sys, argparse, hashlib, shutil
from mathutils import Vector
from pathlib import Path

parser=argparse.ArgumentParser()
parser.add_argument('--output',required=True)
parser.add_argument('--evidence',required=True)
parser.add_argument('--first-only',action='store_true')
args=parser.parse_args(sys.argv[sys.argv.index('--')+1:] if '--' in sys.argv else [])
OUT=Path(args.output).resolve(); EVIDENCE=Path(args.evidence).resolve()
OUT.mkdir(parents=True,exist_ok=True); EVIDENCE.mkdir(parents=True,exist_ok=True)
if (OUT/'Kestrel07-v3.blend').exists():raise RuntimeError('Preserve the existing candidate: choose a new --output folder.')
shutil.copy2(__file__,OUT/'recipe_snapshot.py')
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
for m in list(bpy.data.materials):bpy.data.materials.remove(m)
craft=bpy.data.collections.new('KESTREL_07_EXPORT');bpy.context.scene.collection.children.link(craft)

def material(name,color,metal=0,rough=.4,emission=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*color,1);m.use_nodes=True;n=m.node_tree.nodes.get('Principled BSDF')
 n.inputs['Base Color'].default_value=(*color,1);n.inputs['Metallic'].default_value=metal;n.inputs['Roughness'].default_value=rough
 if emission:n.inputs['Emission Color'].default_value=(*color,1);n.inputs['Emission Strength'].default_value=emission
 return m
M={
 'Ivory':material('Ivory',(.74,.76,.72),.06,.35),
 'Graphite':material('Graphite',(.036,.044,.049),.24,.49),
 'Glass':material('Glass',(.009,.013,.016),.18,.19),
 'Signal':material('Signal',(.43,.55,.017),0,.42),
 'Engine':material('Engine',(.20,.76,.86),.04,.25,2.2),
 'Metal':material('Metal',(.19,.22,.24),.66,.43),
 'Ceramic':material('Ceramic',(.072,.085,.092),.08,.6),
 'Ink':material('Ink',(.016,.021,.024),.05,.56),
 'WhiteMark':material('WhiteMark',(.79,.83,.81),0,.46)
}
clay=material('STUDIO medium gray clay',(.31,.33,.34),0,.53)

PROFILE=[(-.66,1),(.66,1),(1,.42),(1,-.47),(.62,-1),(-.62,-1),(-1,-.47),(-1,.42)]

def normalise(o):
 bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()

def mesh_obj(name,verts,faces,mat,bevel=.016):
 me=bpy.data.meshes.new(name);me.from_pydata(verts,[],faces);me.update();o=bpy.data.objects.new(name,me);craft.objects.link(o);o.data.materials.append(M[mat]);normalise(o)
 if bevel:
  mod=o.modifiers.new('Manufactured edge / centimetres','BEVEL');mod.width=bevel;mod.segments=2;mod.limit_method='ANGLE';mod.angle_limit=.36
  mod=o.modifiers.new('Plane weighted normals','WEIGHTED_NORMAL');mod.keep_sharp=True;mod.weight=35
 return o

def apply_mods(o):
 bpy.context.view_layer.objects.active=o
 for m in list(o.modifiers):
  bpy.ops.object.modifier_apply(modifier=m.name)

def loft(name,sections,mat,bevel=.016,profile=PROFILE):
 # Cross sections are y,cx,cz,width/2,height/2. Broad planes, no subdivision.
 n=len(profile);v=[(cx+px*w,y,cz+pz*h) for y,cx,cz,w,h in sections for px,pz in profile]
 f=[tuple(reversed(range(n)))]
 for r in range(len(sections)-1):
  for i in range(n):f.append((r*n+i,r*n+(i+1)%n,(r+1)*n+(i+1)%n,(r+1)*n+i))
 f.append(tuple((len(sections)-1)*n+i for i in range(n)))
 return mesh_obj(name,v,f,mat,bevel)

def shell_panel(name,sections,indices,mat='Ivory',thickness=.085,bevel=.012):
 # A genuinely thick formed shell, following selected octagonal faces.
 outer=[];inner=[]
 for y,cx,cz,w,h in sections:
  for j in indices:
   px,pz=PROFILE[j];outer.append((cx+px*w,y,cz+pz*h));inner.append((cx+px*(w-thickness),y,cz+pz*(h-thickness)))
 n=len(indices);nr=len(sections);off=len(outer);v=outer+inner;f=[]
 for r in range(nr-1):
  for j in range(n-1):
   a=r*n+j;b=a+1;c=a+n+1;d=a+n;f.append((a,b,c,d));f.append((off+d,off+c,off+b,off+a))
  a=r*n;b=(r+1)*n;f.append((a,b,off+b,off+a))
  a=r*n+n-1;b=(r+1)*n+n-1;f.append((a,off+a,off+b,b))
 for j in range(n-1):
  f.append((j,off+j,off+j+1,j+1));a=(nr-1)*n+j;f.append((a,a+1,off+a+1,off+a))
 return mesh_obj(name,v,f,mat,bevel)

def box(name,location,size,mat,bevel=.014,rotation=None):
 bpy.ops.mesh.primitive_cube_add(size=1,location=location);o=bpy.context.object;o.dimensions=size
 if rotation:o.rotation_euler=rotation
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
 for c in list(o.users_collection):c.objects.unlink(o)
 craft.objects.link(o);o.name=name;o.data.materials.append(M[mat])
 if bevel:
  mod=o.modifiers.new('Machined edge','BEVEL');mod.width=bevel;mod.segments=2
  mod=o.modifiers.new('Plane normals','WEIGHTED_NORMAL');mod.keep_sharp=True
 return o

def beam(name,a,b,width,depth,mat):
 mid=(Vector(a)+Vector(b))*.5;o=box(name,mid,(width,depth,(Vector(b)-Vector(a)).length),mat,.018)
 o.rotation_euler=(Vector(b)-Vector(a)).to_track_quat('Z','Y').to_euler();return o

def tube(name,cx,cz,stations,mat='Metal',segments=40):
 # Closed radial profile: stations form the shell section (axial Y,radius).
 v=[]
 for y,r in stations:
  for i in range(segments):
   a=math.tau*i/segments;v.append((cx+math.cos(a)*r,y,cz+math.sin(a)*r))
 f=[]
 for j in range(len(stations)):
  j2=(j+1)%len(stations)
  for i in range(segments):f.append((j*segments+i,j*segments+(i+1)%segments,j2*segments+(i+1)%segments,j2*segments+i))
 o=mesh_obj(name,v,f,mat,.004)
 for p in o.data.polygons:p.use_smooth=True
 return o

def disk(name,cx,y,cz,r,mat):
 bpy.ops.mesh.primitive_cylinder_add(vertices=40,radius=r,depth=.025,location=(cx,y,cz),rotation=(math.pi/2,0,0))
 o=bpy.context.object
 for c in list(o.users_collection):c.objects.unlink(o)
 craft.objects.link(o);o.name=name;o.data.materials.append(M[mat]);return o

def cut_pocket(target,loc,size):
 apply_mods(target);cutter=box('TEMP closed pocket cutter',loc,size,'Graphite',0)
 bpy.context.view_layer.objects.active=target;m=target.modifiers.new('Actual recessed inlet','BOOLEAN');m.operation='DIFFERENCE';m.solver='EXACT';m.object=cutter
 bpy.ops.object.modifier_apply(modifier=m.name);bpy.data.objects.remove(cutter,do_unlink=True);normalise(target)

# Central monocoque: subordinate to nacelles, blunt angular nose and continuous aft taper.
spine=loft('Monocoque / continuous load-bearing center',[
 (-3.53,0,.06,.22,.18),(-2.80,0,.18,.40,.23),(-1.85,0,.28,.56,.28),(-.55,0,.29,.58,.31),(.64,0,.21,.48,.27),(1.85,0,.07,.31,.23),(2.75,0,-.01,.205,.195)],'Graphite',.022)
loft('Nose / small ceramic lower keel',[(-3.49,0,-.08,.2,.085),(-2.78,0,-.04,.38,.09),(-1.98,0,-.04,.46,.09)],'Ceramic',.016)
# Low, planar long glazing with a narrow graphite sill. No silver bubble gasket.
canopy_sections=[(-2.55,0,.48,.12,.024),(-1.98,0,.62,.34,.10),(-.68,0,.735,.43,.145),(.18,0,.70,.37,.14),(.82,0,.48,.23,.06)]
loft('Canopy / narrow continuous sill',[(y,x,z-.035,w+.026,h+.009) for y,x,z,w,h in canopy_sections],'Graphite',.014)
loft('Canopy / long faceted near-black glazing',canopy_sections,'Glass',.012)
# A dark rear deck continues the cockpit spine instead of a pale dorsal cap.
loft('Spine / aft avionics mantle',[(.79,0,.40,.27,.105),(1.32,0,.29,.34,.11),(2.03,0,.13,.27,.11),(2.58,0,.07,.20,.095)],'Graphite',.015)
for side in [-1,1]:
 # Thin cheek armor with structural seat against the central keel.
 shell_panel('Center / formed rear cheek '+str(side),[(.76,0,.18,.49,.26),(1.38,0,.08,.39,.26),(2.21,0,.015,.245,.205),(2.63,0,-.005,.205,.18)],([1,2,3] if side==1 else [5,6,7]),'Ivory',.065,.009)
 # Two deep crossmembers, with mounting lands rather than loose diagonal rods.
 for station in [-1.15,1.55]:
  beam('Chassis / boxed crossmember '+str(side)+' '+str(station),(side*.42,station,-.06),(side*1.12,station+.05,-.035),.22,.37,'Graphite')
  box('Chassis / nacelle mounting saddle',(side*1.04,station+.06,.045),(.26,.44,.26),'Ceramic',.014)
  box('Chassis / central mounting flange',(side*.53,station,.05),(.17,.46,.23),'Metal',.01)
 # Broad truncated housings; carry the entire section into the recessed engine mouth.
 sections=[(-3.34,side*1.58,.035,.50,.35),(-2.60,side*1.61,.055,.69,.49),(-1.30,side*1.66,.06,.78,.53),(.00,side*1.69,.055,.83,.565),(1.20,side*1.70,.035,.87,.60),(2.42,side*1.66,.005,.89,.64),(3.34,side*1.65,-.005,.86,.63)]
 # The radial path comes back inside the aft mouth, producing a watertight recess.
 core_sections=[(y,x,z,w-.09,h-.08) for y,x,z,w,h in sections]
 core_sections += [(3.34,side*1.65,-.005,.59,.54),(2.98,side*1.65,-.005,.53,.49),(2.36,side*1.65,-.005,.34,.33)]
 core=loft('Nacelle / closed structural cavity shell '+str(side),core_sections,'Graphite',.018)
 # Unequal large service-panel groups with sharp shoulders and physical return lips.
 fore=shell_panel('Armor / broad forward crown '+str(side),sections[:3],[7,0,1,2],'Ivory',.085,.014)
 middle_sections=[(-1.26,side*1.661,.06,.782,.532),sections[3],(1.16,side*1.70,.037,.869,.599)]
 middle=shell_panel('Armor / center shoulder panel '+str(side),middle_sections,[7,0,1,2],'Ivory',.085,.014)
 aft=shell_panel('Armor / aft engine-integrating crown '+str(side),[(1.20,side*1.7,.035,.87,.60),sections[5],sections[6]],[7,0,1,2],'Ivory',.085,.014)
 outer=[2,3,4] if side==1 else [4,5,6,7]
 shell_panel('Armor / forward lower return '+str(side),sections[:3],outer,'Ivory',.08,.012)
 shell_panel('Armor / aft lower return '+str(side),[(-1.25,side*1.66,.06,.782,.532),sections[3],sections[4],sections[5],sections[6]],outer,'Ivory',.08,.012)
 # Small inset intake is cut through formed armor down into the structural shell.
 pocket=(side*1.69,-.12,.69);pocket_size=(.61,.81,.87)
 cut_pocket(middle,pocket,pocket_size);cut_pocket(core,pocket,pocket_size)
 for j in range(3):box('Intake / deep turning vane '+str(side)+' '+str(j),(side*1.69,-.34+j*.22,.29),(.49,.055,.105),'Ceramic',.008,(-.25,0,0))
 # Inner side shields define the open channel with small upper/lower structural reveals.
 inner=[5,6,7] if side==1 else [1,2,3]
 shell_panel('Nacelle / inner heat shield '+str(side),[(-1.18,side*1.66,.04,.72,.43),(1.20,side*1.70,.02,.81,.5),(2.37,side*1.66,0,.83,.57)],inner,'Ceramic',.045,.01)
 # A restrained propulsion manifold with both ends seated inside named housings.
 beam('Manifold / connected transfer line '+str(side),(side*.39,.72,.02),(side*.97,1.63,.025),.105,.13,'Metal')
 box('Manifold / feed socket center '+str(side),(side*.46,.8,.02),(.22,.28,.22),'Graphite',.015)
 box('Manifold / feed socket nacelle '+str(side),(side*.96,1.58,.025),(.22,.3,.22),'Graphite',.015)
 # Dark chamber -> narrow dull metal collar -> compact recessed core.
 x=side*1.65;z=-.005
 tube('Engine / recessed ceramic throat '+str(side),x,z,[(3.31,.535),(3.31,.505),(2.54,.286),(2.52,.32)],'Ceramic')
 tube('Engine / restrained machined lip '+str(side),x,z,[(3.335,.535),(3.355,.528),(3.355,.509),(3.31,.50),(3.30,.518)],'Metal')
 tube('Engine / internal stage collar '+str(side),x,z,[(2.92,.424),(2.945,.427),(2.95,.408),(2.92,.405)],'Metal')
 tube('Engine / thin dim energizer '+str(side),x,z,[(2.71,.337),(2.72,.338),(2.72,.329),(2.71,.329)],'Engine')
 disk('Engine / dark chamber rear '+str(side),x,2.53,z,.285,'Graphite')
 disk('Engine / compact thrust core '+str(side),x,2.56,z,.14,'Engine')
 # Main fairing mouth is wrapped by the white outer crown and return, no bolted-on can.
 for j in range(5):
  angle=j*math.tau/5;ax=x+math.cos(angle)*.37;az=z+math.sin(angle)*.37
  beam('Engine / internal fixed guide '+str(side)+' '+str(j),(ax,2.80,az),(x+math.cos(angle)*.485,3.23,z+math.sin(angle)*.485),.022,.022,'Ceramic')
# Smaller center impulse unit is structurally subordinate and recessed in the aft keel.
tube('Spine / integrated center chamber',0,-.01,[(2.70,.208),(2.865,.198),(2.865,.145),(2.51,.11),(2.50,.15)],'Ceramic',32)
tube('Spine / narrow impulse collar',0,-.01,[(2.866,.195),(2.88,.188),(2.88,.173),(2.862,.172)],'Metal',32)
disk('Spine / compact impulse core',0,2.59,-.01,.074,'Engine')

# Clean manifold checks and UVs on named editable source parts.
for o in list(craft.objects):
 if o.type!='MESH':continue
 apply_mods(o);normalise(o)
 bpy.context.view_layer.objects.active=o;o.select_set(True)
 bpy.ops.object.select_all(action='DESELECT');o.select_set(True)
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.151917,island_margin=.008);bpy.ops.object.mode_set(mode='OBJECT')
 o.select_set(False)

# Fixed studio: broad neutral white lights, matte gray ground, no bloom or glare.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.device='CPU';scene.cycles.use_denoising=True
scene.cycles.max_bounces=5;scene.cycles.diffuse_bounces=2;scene.cycles.glossy_bounces=3
scene.render.threads_mode='FIXED';scene.render.threads=6
scene.render.resolution_x=1600;scene.render.resolution_y=1200;scene.render.resolution_percentage=100
scene.world.use_nodes=True;scene.world.node_tree.nodes.get('Background').inputs[0].default_value=(.18,.19,.20,1);scene.world.node_tree.nodes.get('Background').inputs[1].default_value=.48
scene.view_settings.view_transform='AgX';scene.view_settings.exposure=0
floor=material('STUDIO floor',(.17,.18,.19),0,.75)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.85));bpy.context.object.name='STUDIO / floor';bpy.context.object.data.materials.append(floor)
def area(name,loc,power,size,target=(0,0,0)):
 bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.name=name;o.data.energy=power;o.data.shape='RECTANGLE';o.data.size=size;o.data.size_y=size*.65
 o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
area('STUDIO / neutral broad key',(-4,-4,8),1350,6)
area('STUDIO / neutral aft edge',(5,4,6),1100,5)
area('STUDIO / neutral fill',(-5,4,3),500,5)
area('STUDIO / bow bounce',(0,-7,2),450,4)
bpy.ops.object.camera_add();cam=bpy.context.object;cam.name='STUDIO / fixed comparison camera';scene.camera=cam;cam.data.type='ORTHO';cam.data.ortho_scale=9.6

# Store exact joined runtime meshes with unique material-atlas UVs in a second collection.
runtime=bpy.data.collections.new('EXPORT_RUNTIME');scene.collection.children.link(runtime)
exports=[]
for key in M:
 copies=[]
 for source in list(craft.objects):
  if source.type=='MESH' and source.data.materials[0].name==key:
   o=source.copy();o.data=source.data.copy();runtime.objects.link(o);copies.append(o)
 if not copies:continue
 bpy.ops.object.select_all(action='DESELECT')
 for o in copies:o.select_set(True)
 bpy.context.view_layer.objects.active=copies[0];bpy.ops.object.join();o=bpy.context.object;o.name='Kestrel07_V3_'+key
 bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.uv.smart_project(angle_limit=1.151917,island_margin=.008,area_weight=0.0,correct_aspect=True,scale_to_bounds=True);bpy.ops.object.mode_set(mode='OBJECT')
 exports.append(o)
bpy.ops.object.select_all(action='DESELECT')
for o in exports:o.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'HeroShip.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False,use_tspace=True,path_mode='AUTO')
runtime.hide_render=True;runtime.hide_viewport=True

points=[o.matrix_world@Vector(corner) for o in craft.objects for corner in o.bound_box]
nonmanifold={}
for o in craft.objects:
 bm=bmesh.new();bm.from_mesh(o.data);count=sum(1 for e in bm.edges if not e.is_manifold);bm.free()
 if count:nonmanifold[o.name]=count
stats={'revision':OUT.name,'recipe_sha256':hashlib.sha256(Path(__file__).read_bytes()).hexdigest(),'source_dimensions_xyz':[round(max(p[i] for p in points)-min(p[i] for p in points),4) for i in range(3)],'source_forward':'-Y','source_up':'+Z','unity_mapping':'(X,Z,-Y)','source_meshes':len(craft.objects),'runtime_meshes':len(exports),'triangles':sum(sum(len(f.vertices)-2 for f in o.data.polygons) for o in craft.objects),'nonmanifold_edges_by_source_mesh':nonmanifold,'materials_used':[o.data.materials[0].name for o in exports],'primary_form_only':True,'studio_bloom':False,'runtime_uv_policy':'Unique SmartProject atlas AFTER joining by material; EXPORT_RUNTIME is exact FBX source. Do not re-unwrap after baking.'}
(OUT/'asset-stats.json').write_text(json.dumps(stats,indent=2))
anchors={'source_axes':{'forward':'-Y','up':'+Z'},'unity_mapping':'(X,Z,-Y)','engines':[]}
for side,label in [(-1,'left'),(1,'right')]:
 anchors['engines'].append({'name':label,'source_exit':[side*1.65,3.355,-.005],'source_throat':[side*1.65,2.56,-.005],'unity_exit':[side*1.65,-.005,-3.355],'unity_throat':[side*1.65,-.005,-2.56],'unity_effect_direction':[0,0,-1],'opening_radius':.509,'compact_core_radius':.14})
anchors['engines'].append({'name':'center','source_exit':[0,2.88,-.01],'source_throat':[0,2.59,-.01],'unity_exit':[0,-.01,-2.88],'unity_throat':[0,-.01,-2.59],'unity_effect_direction':[0,0,-1],'opening_radius':.173,'compact_core_radius':.074})
(OUT/'engine-anchors.json').write_text(json.dumps(anchors,indent=2))

views=[
 ('01-clay-rear-quarter',(-8,10,6),(0,0,.2),9.6,True,'ORTHO'),
 ('02-neutral-rear-quarter',(-8,10,6),(0,0,.2),9.6,False,'ORTHO'),
 ('03-clay-front-quarter',(9,-11,7),(0,-.1,.1),9.6,True,'ORTHO'),
 ('04-neutral-front-quarter',(9,-11,7),(0,-.1,.1),9.6,False,'ORTHO'),
 ('05-clay-chase',(0,10,4.4),(0,-.8,.05),9.6,True,'ORTHO'),
 ('06-neutral-chase',(0,10,4.4),(0,-.8,.05),9.6,False,'ORTHO'),
 ('07-clay-top',(0,-.001,15),(0,-.2,0),10,True,'ORTHO'),
 ('08-clay-side',(12,0,.8),(0,0,.10),9.6,True,'ORTHO'),
 ('09-C-perspective',(-4.1,10.8,4.2),(0,-.3,.06),9.6,False,'PERSP')]
(EVIDENCE/'capture-contract.json').write_text(json.dumps({'revision':OUT.name,'resolution':[1600,1200],'neutral_white_studio':True,'bloom':False,'view_transform':'AgX','views':views},indent=2))
# Save before first render so the parent can inspect editable and UV-stable runtime parts.
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Kestrel07-v3.blend'))
for name,pos,target,scale,use_clay,projection in views:
 cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type=projection;cam.data.ortho_scale=scale;cam.data.lens=45
 scene.view_layers[0].material_override=clay if use_clay else None
 scene.render.filepath=str(EVIDENCE/(name+'.png'));bpy.ops.render.render(write_still=True)
 print('SHIP_V3_RENDER_READY',name,flush=True)
 if args.first_only:break
print('SHIP_V3_CANDIDATE_READY',json.dumps(stats),flush=True)
