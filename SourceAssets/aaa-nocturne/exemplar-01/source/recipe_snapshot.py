"""Nocturne staged architecture. Run Blender --background --python this_file. NEVER renders."""
import bpy,bmesh,math,json,os,hashlib,shutil,zlib,struct
import numpy as np
from mathutils import Vector,Matrix
from mathutils.bvhtree import BVHTree
ROOT=os.path.abspath(os.path.join(os.path.dirname(__file__),'..'))
OUT=os.path.join(ROOT,'exemplar-01')
PROJECT=os.path.abspath(os.path.join(ROOT,'../..'))
COURSE=os.path.join(PROJECT,'evidence/nocturne-production/direct-02/context/course-data.json')
course=json.load(open(COURSE));course_hash=hashlib.sha256(open(COURSE,'rb').read()).hexdigest()
for p in ['source','meshes','textures','checks']:os.makedirs(os.path.join(OUT,p),exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
scene['Status']='BLOCKED_VISUAL_REVIEW. Geometry staging only. No render or art acceptance.'
scene['CourseHash']=course_hash
M={};material_records=[]
# Colours below are linear; texture albedo encodes sRGB. Source-generated, no external assets.
specs=[('NA_Slate',(.22,.29,.34),.67,.04),('NA_Ceramic',(.36,.39,.40),.42,.10),('NA_Graphite',(.043,.057,.071),.59,.32),('NA_Aluminium',(.26,.30,.34),.36,.78),('NA_Glass',(.023,.055,.067),.20,.45),('NA_Interior',(.13,.095,.061),.79,0),('NA_WarmDiffuser',(.68,.39,.17),.38,0),('NA_CoolDiffuser',(.55,.67,.73),.36,0)]
def png(path,array):
 h,w,c=array.shape
 def chunk(t,b):return struct.pack('>I',len(b))+t+b+struct.pack('>I',zlib.crc32(t+b)&0xffffffff)
 raw=b''.join(b'\0'+row.tobytes() for row in array)
 open(path,'wb').write(b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',w,h,8,6 if c==4 else 2,0,0,0))+chunk(b'IDAT',zlib.compress(raw,6))+chunk(b'IEND',b''))
n=2048;yy,xx=np.mgrid[0:n,0:n];rng=np.random.default_rng(711)
# Periodic shallow tooling profile: no baked shadows, panel lines or false bolts.
height=(.40*np.sin(xx*math.tau/n*32)+.12*np.sin(yy*math.tau/n*48)+.09*np.sin((xx+yy)*math.tau/n*117))
dx=(np.roll(height,-1,1)-np.roll(height,1,1))*.10;dy=(np.roll(height,-1,0)-np.roll(height,1,0))*.10
normal=np.stack([-dx,-dy,np.ones_like(dx)],-1);normal/=np.linalg.norm(normal,axis=2)[:,:,None]
png(os.path.join(OUT,'textures/NA_Micro_Normal.png'),np.uint8(np.clip(normal*.5+.5,0,1)*255))
noise=rng.random((n,n))-.5
for name,col,rough,metal in specs:
 m=bpy.data.materials.new(name);m.use_nodes=True;m.diffuse_color=(*col,1);p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*col,1);p.inputs['Roughness'].default_value=rough;p.inputs['Metallic'].default_value=metal
 emit=.65 if name=='NA_WarmDiffuser' else .8 if name=='NA_CoolDiffuser' else 0
 if emit:p.inputs['Emission Color'].default_value=(*col,1);p.inputs['Emission Strength'].default_value=emit
 record={'id':name,'baseColorLinear':list(col),'roughness':rough,'metallic':metal,'emissionColorLinear':list(col) if emit else [0,0,0],'emissionIntensity':emit,'metersPerTile':[4,4],'uvMode':'metric','normalScale':.3,'textureScale':[1,1]}
 if name in ['NA_Slate','NA_Ceramic','NA_Graphite','NA_Aluminium']:
  linear=np.clip(np.array(col)[None,None,:]*(1+height[:,:,None]*.035+noise[:,:,None]*.035),0,1)
  srgb=np.where(linear<=.0031308,linear*12.92,1.055*linear**(1/2.4)-.055)
  base='textures/'+name+'_BaseColor.png';mask='textures/'+name+'_MetallicSmoothness.png'
  png(os.path.join(OUT,base),np.uint8(srgb*255));a=np.zeros((n,n,4),dtype=np.uint8);a[:,:,0]=int(metal*255);a[:,:,3]=np.uint8(np.clip(1-rough+height*.018+noise*.015,0,1)*255);png(os.path.join(OUT,mask),a)
  record.update(baseColor=base,metallicSmoothness=mask,normal='textures/NA_Micro_Normal.png')
  for path,input_name,space in [(base,'Base Color','sRGB')]:
   tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(os.path.join(OUT,path));tex.image.colorspace_settings.name=space;m.node_tree.links.new(tex.outputs['Color'],p.inputs[input_name])
  tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(os.path.join(OUT,mask));tex.image.colorspace_settings.name='Non-Color';inv=m.node_tree.nodes.new('ShaderNodeMath');inv.operation='SUBTRACT';inv.inputs[0].default_value=1;m.node_tree.links.new(tex.outputs['Alpha'],inv.inputs[1]);m.node_tree.links.new(inv.outputs[0],p.inputs['Roughness'])
  tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=bpy.data.images.load(os.path.join(OUT,'textures/NA_Micro_Normal.png'),check_existing=True);tex.image.colorspace_settings.name='Non-Color';nm=m.node_tree.nodes.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.3;m.node_tree.links.new(tex.outputs['Color'],nm.inputs['Color']);m.node_tree.links.new(nm.outputs[0],p.inputs['Normal'])
 M[name]=m;material_records.append(record)
assets={};current=None

def begin(name):
 global current
 current=bpy.data.collections.new(name+'_EDITABLE');scene.collection.children.link(current);assets[name]=[]
def mesh(name,v,f,mat='NA_Slate',bevel=.04,detail=0):
 me=bpy.data.meshes.new(name);me.from_pydata(v,[],f);me.update();bm=bmesh.new();bm.from_mesh(me);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free()
 ob=bpy.data.objects.new(name,me);current.objects.link(ob);me.materials.append(M[mat]);ob['lodDetail']=detail;assets[current.name.removesuffix('_EDITABLE')].append(ob)
 if bevel:
  mod=ob.modifiers.new('Fabricated edge radius','BEVEL');mod.width=bevel;mod.segments=3;mod.affect='EDGES'
  mod=ob.modifiers.new('Area weighted normals','WEIGHTED_NORMAL');mod.keep_sharp=True
 return ob
F=[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)]
def box(name,c,s,mat='NA_Slate',bevel=.04,detail=0):
 v=[(c[0]+x*s[0]/2,c[1]+y*s[1]/2,c[2]+z*s[2]/2) for x,y,z in [(-1,-1,-1),(1,-1,-1),(1,1,-1),(-1,1,-1),(-1,-1,1),(1,-1,1),(1,1,1),(-1,1,1)]]
 return mesh(name,v,F,mat,bevel,detail)
def slab(name,xy,z,t,mat='NA_Slate',slope=0,bevel=.045):
 n=len(xy);v=[(x,y,z+slope*x+dz) for dz in [-t/2,t/2] for x,y in xy];f=[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)];return mesh(name,v,f,mat,bevel)
def profile(name,xz,y,depth,mat='NA_Slate',bevel=.045):
 n=len(xz);v=[(x,y+dy,z) for dy in [-depth/2,depth/2] for x,z in xz];return mesh(name,v,[tuple(range(n-1,-1,-1)),tuple(range(n,2*n))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)],mat,bevel)
def beam(name,a,b,w,d,mat='NA_Aluminium',detail=0):
 ob=box(name,(0,0,0),(w,d,(Vector(b)-Vector(a)).length),mat,min(w*.12,.045),detail);ob.location=(Vector(a)+Vector(b))/2;ob.rotation_euler=(Vector(b)-Vector(a)).to_track_quat('Z','Y').to_euler();return ob

def facade(name,bays,civic):
 begin(name);length=bays*6.4;h=13.6 if civic else 7.1;ys=[(i-(bays-1)/2)*6.4 for i in range(bays)]
 footprint=[(-5,-length/2+1),(-4,-length/2),(4.8,-length/2),(5.4,-length/2+.8),(5.4,length/2-.8),(4.6,length/2),(-4,length/2),(-5,length/2-1)]
 slab('Chamfered load-bearing podium',footprint,-1.15,2.3,'NA_Graphite',bevel=.12)
 slab('Podium wearing course',footprint,.12,.24,'NA_Slate',bevel=.06)
 # Open structural frame, rear enclosure, individual bays. No facade slapped over a solid core.
 box('Rear service spine',(4.6,0,h/2),(1.0,length-.8,h),'NA_Slate',.10)
 for y in [-length/2+.45,length/2-.45]:box('End shear wall',(.15,y,h/2),(8.2,.7,h),'NA_Slate',.12)
 for i in range(bays+1):
  y=-length/2+i*6.4
  profile('Splayed civic pier',[(-3.6,.24),(-2.7,.24),(-2.4,h-1.2),(-3.7,h-1.2),(-4.35,3.0)],y,.7,'NA_Slate',.07)
  box('Pier shoe',(-3.75,y,.63),(1.55,1.20,.75),'NA_Aluminium',.045)
  beam('Roof haunch',(-3.45,y,h-3),(-4.55,y,h-.4),.32,.45)
 for i,y in enumerate(ys):
  # Recessed occupied bay: 1.2 m reveal, interior slab and partition, opaque dark glazing.
  box('Recessed backing',(2.95,y,3.1),(.25,5.7,5.7),'NA_Interior',.025)
  box('Low spandrel',(-2.85,y,1.05),(.45,5.6,1.55),'NA_Slate',.04)
  if civic:
   for z in [5.2,9.1]:
    box('Deep bay sill',(-2.7,y,z-1.65),(1.25,5.65,.27),'NA_Ceramic',.035)
    box('Deep bay lintel',(-2.7,y,z+1.65),(1.25,5.65,.32),'NA_Slate',.045)
    for dy in [-2.66,2.66]:box('Window jamb reveal',(-2.6,y+dy,z),(1.2,.28,3.15),'NA_Aluminium',.025)
    box('Recessed glazing plane',(-2.04,y,z),(.14,5.02,2.95),'NA_Glass',.02)
    if (i+int(z))%3==0:box('Localized occupied transom',(-2.13,y-.72,z+.74),(.04,2.55,.70),'NA_WarmDiffuser',.01)
    for dy in [-.83,.89]:box('Window partition mullion',(-2.3,y+dy,z),(.24,.10,3.02),'NA_Aluminium',.012,1)
   box('Upper ventilated spandrel',(-2.73,y,11.57),(.6,5.62,1.1),'NA_Graphite',.045)
   for dy in [-2,-1.4,-.8,-.2,.4,1,1.6,2.2]:box('Spandrel louvre',(-3.06,y+dy,11.57),(.16,.13,.88),'NA_Aluminium',.015,1)
  else:
   for dy in [-2.4,2.4]:box('Service door deep return',(-3.0,y+dy,3.37),(1.35,.34,4.3),'NA_Slate',.035)
   box('Service opening lintel',(-3,y,5.60),(1.35,5.15,.4),'NA_Ceramic',.04)
   box('Recessed segmented shutter',(-2.43,y,3.30),(.20,4.4,3.8),'NA_Graphite',.035)
   for z in [1.75,2.45,3.15,3.85,4.55]:box('Folded shutter leaf',(-2.58,y,z),(.12,4.26,.58),'NA_Aluminium',.025)
   box('Door handle',(-2.70,y+1.6,2.65),(.14,.12,.62),'NA_Aluminium',.018,1)
  box('Recessed bay luminaire housing',(-3.49,y,h-1.36),(.50,3.8,.32),'NA_Graphite',.035)
  box('Shielded bay diffuser',(-3.55,y,h-1.52),(.30,3.25,.06),'NA_CoolDiffuser',.012)
  # Accessible lower drain with actual slot gaps.
  for dy in [-2,-1,0,1,2]:box('Podium drain grate',(-4.45,y+dy,.278),(.36,.82,.07),'NA_Aluminium',.008,1)
  beam('Canopy transverse bearer',(-4.8,y,h-.28),(4.6,y,h+.35),.28,.48)
 slab('Folded overhanging roof',[(x*1.035,y) for x,y in footprint],h+.25,.52,'NA_Ceramic',slope=.075,bevel=.085)
 slab('Recessed waterproof roof pan',[(-3.3,-length/2+1.2),(4.2,-length/2+1.2),(4.2,length/2-1.2),(-3.3,length/2-1.2)],h+.61,.14,'NA_Graphite',slope=.075)
 box('Rear parapet coping',(4.54,0,h+1.03),(.52,length-.4,.28),'NA_Aluminium',.04)
 if civic:
  # Asymmetrical stair lantern intentionally subordinate to existing signal landmark.
  y=ys[-1];box('Stair lantern solid side',(1.55,y,h+1.9),(3.6,4.35,2.7),'NA_Slate',.12)
  box('Stair clerestory inset',(-.31,y,h+1.92),(.10,3.3,1.65),'NA_Glass',.025)
  box('Stair warm landing strip',(-.38,y+.45,h+1.58),(.04,1.85,.36),'NA_WarmDiffuser',.01)
  slab('Lantern folded cap',[(-.8,y-2.6),(3.7,y-2.6),(4.1,y-2.1),(4.1,y+2.3),(-.8,y+2.3)],h+3.45,.42,'NA_Ceramic',.04,.06)
 else:
  box('Roof mechanical curb',(1.6,0,h+.85),(3.4,5.7,.5),'NA_Graphite',.07)
  for y in [-1.8,0,1.8]:box('Roof extract hood',(1.6,y,h+1.4),(2.7,1.4,.65),'NA_Aluminium',.10)
 # Physical underslung braced supports connect the podium to a lower level.
 for y in [-length*.32,length*.32]:
  profile('Tapered podium support',[(-.9,-8),(1.5,-8),(2.7,-2.3),(-2.0,-2.3)],y,2.2,'NA_Slate',.12)
  box('Lower bearing foot',(.35,y,-8.35),(4.5,3.6,.7),'NA_Graphite',.10)
  beam('Podium diagonal bracket',(.2,y,-6.0),(-3.75,y,-2.2),.75,1.05,'NA_Slate')
facade('NA_CivicFacade_A',4,True)
facade('NA_ServiceFacade_A',2,False)

inner=[(-13.85,0),(-13.85,10.6),(-10.1,14.6),(10.1,14.6),(13.85,10.6),(13.85,0)]
outer=[(-14.95,0),(-14.95,11.05),(-10.58,15.8),(10.58,15.8),(14.95,11.05),(14.95,0)]
def ring(name,y,depth,mat='NA_Slate'):
 # Welded continuous U-section. Open race passage, closed ring solid, no butt-joint cap overlaps.
 v=[]
 for yy in [y-depth/2,y+depth/2]:v.extend([(x,yy,z) for x,z in inner]);v.extend([(x,yy,z) for x,z in outer])
 f=[]
 for i in range(5):
  f.extend([(i,i+1,6+i+1,6+i),(12+i,18+i,18+i+1,12+i+1),(i,12+i,12+i+1,i+1),(6+i,6+i+1,18+i+1,18+i)])
 f.extend([(0,6,18,12),(5,17,23,11)])
 return mesh(name,v,f,mat,.055)
def gallery(name,entry):
 begin(name);depth=1.3 if entry else .72;ring('Continuous chamfered structural ring',0,depth)
 if entry:
  ring('Recessed second threshold ring',2.3,.64,'NA_Graphite')
  # Deep fascia along each shoulder and crown, connected to both threshold rings.
  for i in range(1,4):
   a=outer[i];b=outer[i+1];aa=inner[i];bb=inner[i+1]
   profile('Threshold exterior return',[a,b,(b[0],b[1]+.25),(a[0],a[1]+.25)],1.0,3.5,'NA_Ceramic',.035)
   profile('Inner threshold soffit',[(aa[0],aa[1]+.14),(bb[0],bb[1]+.14),(bb[0],bb[1]+.36),(aa[0],aa[1]+.36)],1.0,2.2,'NA_Aluminium',.025)
 for s in [-1,1]:
  box('Cast structural foot',(s*14.48,0,.54),(1.9,depth+1.1,1.08),'NA_Slate',.07)
  box('Isolation bearing',(s*14.47,0,1.16),(1.50,depth+.6,.18),'NA_Graphite',.025)
  box('Base steel collar',(s*14.46,0,1.39),(1.42,depth+.35,.22),'NA_Aluminium',.035)
  box('Pier outer folded service chase',(s*15.04,0,6.2),(.24,depth*.7,7.1),'NA_Graphite',.035)
  for z in [2.15,9.7]:
   box('Structural splice plate',(s*14.46,-depth/2-.05,z),(1.05,.16,.62),'NA_Aluminium',.025)
   for dx in [-.31,.31]:
    for dz in [-.16,.16]:box('Recessed splice boss',(s*14.46+dx,-depth/2-.16,z+dz),(.11,.07,.11),'NA_Graphite',.022,1)
  if entry:
   box('Entry lamp pocket',(s*13.83,.15,8.4),(.20,1.1,2.3),'NA_Graphite',.035)
   box('Entry shielded diffuser',(s*13.70,.15,8.4),(.045,.78,1.65),'NA_WarmDiffuser',.012)
 for x in [-6.4,0,6.4]:
  box('Crown bearing plate',(x,-depth/2-.055,15.2),(1.5,.18,.75),'NA_Aluminium',.025)
  beam('Crown short return',(x,-depth/2,15.95),(x,depth/2,15.95),.22,.25,'NA_Graphite')
 if entry:
  box('Brow luminaire tray',(0,1.23,14.35),(18.5,1.0,.30),'NA_Graphite',.04)
  box('Recessed brow diffuser',(0,1.23,14.18),(16.8,.35,.055),'NA_WarmDiffuser',.012)
gallery('NA_GalleryPortal_A',True);gallery('NA_GalleryRib_A',False)

begin('NA_GalleryPanel_A')
# Right wall module, 5.9m long. Real recessed cassette rather than additive overlay on legacy panels.
box('Backing tray',(14.82,0,6.53),(.24,5.9,9.66),'NA_Graphite',.04)
for y in [-2.86,2.86]:box('Folded vertical reveal',(14.45,y,6.53),(.60,.18,9.45),'NA_Aluminium',.025)
for z in [1.84,11.21]:box('Cassette end return',(14.46,0,z),(.58,5.73,.20),'NA_Aluminium',.025)
# Two different broad surfaces separated by maintenance recess; construction visible at metres scale.
for y in [-1.43,1.43]:
 for z,h in [(4.0,3.7),(9.43,3.0)]:box('Recessed formed ceramic cassette',(14.47,y,z),(.19,2.65,h),'NA_Ceramic',.055)
 box('Recessed service hatch',(14.68,y,6.86),(.14,2.40,1.55),'NA_Graphite',.025)
 for z in [6.40,6.73,7.06,7.39]:box('Hatch louvre',(14.47,y,z),(.29,2.18,.16),'NA_Aluminium',.025)
 for yy in [y-1.2,y+1.2]:box('Hatch return',(14.37,yy,6.86),(.38,.11,1.70),'NA_Aluminium',.018)
 box('Flush hatch latch',(14.18,y+.84,6.86),(.09,.11,.31),'NA_Graphite',.018,1)
box('Lower maintenance trunk',(14.48,0,1.42),(.66,5.72,.60),'NA_Graphite',.05)
box('Fixture recessed tray',(14.12,0,3.02),(.48,4.72,.25),'NA_Graphite',.025)
box('Shielded maintenance diffuser',(13.865,0,3.01),(.035,3.75,.065),'NA_WarmDiffuser',.008)
# Angled shoulder cassette matches original 14.68,11.25 to 10.5,15.48 shell.
profile('Chamfered shoulder tray',[(14.71,11.36),(10.54,15.57),(10.34,15.36),(14.48,11.15)],0,5.85,'NA_Graphite',.025)
profile('Inset shoulder ceramic',[(14.40,11.50),(10.88,15.07),(10.77,14.97),(14.30,11.4)],0,5.28,'NA_Ceramic',.025)
for y in [-2.79,2.79]:beam('Shoulder folded seam',(14.51,y,11.36),(10.57,y,15.35),.14,.17,'NA_Aluminium')
# 5.4 m cassette envelope leaves a real reveal at the sampled rib placements.
# Shorten longitudinal coordinates before beveling; no negative scale or runtime stretching.
for ob in assets['NA_GalleryPanel_A']:
 transform=Matrix.Diagonal((1,5.4/5.9,1,1)) @ ob.matrix_basis
 ob.data.transform(transform);ob.matrix_basis=Matrix.Identity(4)

# Runtime build uses hand-selected detail omission plus fewer bevel segments, not blind decimation.
runtime=bpy.data.collections.new('RUNTIME_LODS');scene.collection.children.link(runtime)
export_objects={};stats={};manifest_assets=[]
def uv_metric(me):
 for old in list(me.uv_layers):me.uv_layers.remove(old)
 uv=me.uv_layers.new(name='UV0_Metric4m')
 for p in me.polygons:
  axis=max(range(3),key=lambda k:abs(p.normal[k]));axes=[k for k in range(3) if k!=axis]
  for li in p.loop_indices:
   v=me.vertices[me.loops[li].vertex_index].co;uv.data[li].uv=(v[axes[0]]/4,v[axes[1]]/4)
 uv.active_render=True

def inspect(ob):
 me=ob.data;me.calc_loop_triangles();bm=bmesh.new();bm.from_mesh(me)
 boundary=sum(e.is_boundary for e in bm.edges);nonman=sum(not e.is_manifold for e in bm.edges)
 # Components' signed volumes detect inverted closed parts even if other solids mask them.
 unseen=set(bm.faces);volumes=[]
 while unseen:
  pending=[unseen.pop()];component=[]
  while pending:
   f=pending.pop();component.append(f)
   for e in f.edges:
    for nf in e.link_faces:
     if nf in unseen:unseen.remove(nf);pending.append(nf)
  vol=0
  for f in component:
   for j in range(1,len(f.verts)-1):vol+=f.verts[0].co.dot(f.verts[j].co.cross(f.verts[j+1].co))/6
  volumes.append(vol)
 bm.free();coords=np.array([list(v.co) for v in me.vertices]);uv=np.array([list(v.uv) for v in me.uv_layers.active.data])
 deg=sum(p.area<1e-10 for p in me.polygons);zero_uv=0
 for p in me.polygons:
  a,b,c=[uv[i] for i in p.loop_indices];ab=b-a;ac=c-a;zero_uv+=abs(ab[0]*ac[1]-ab[1]*ac[0])<1e-12
 lo=coords.min(0).tolist();hi=coords.max(0).tolist()
 seen=set();duplicates=0
 for p in me.polygons:
  key=tuple(sorted(tuple(round(float(v),6) for v in me.vertices[i].co) for i in p.vertices))
  if key in seen:duplicates+=1
  seen.add(key)
 result={'vertices':len(me.vertices),'triangles':len(me.loop_triangles),'sourceBoundsMin':lo,'sourceBoundsMax':hi,'unityBoundsMin':[lo[0],lo[2],-hi[1]],'unityBoundsMax':[hi[0],hi[2],-lo[1]],'boundaryEdges':boundary,'nonmanifoldEdges':nonman,'degenerateTriangles':deg,'duplicateTriangles':duplicates,'zeroAreaUVTriangles':int(zero_uv),'closedComponents':len(volumes),'nonpositiveComponents':sum(v<=0 for v in volumes),'finiteUV':bool(np.isfinite(uv).all()),'materialSlots':[m.name for m in me.materials]}
 assert nonman==0 and deg==0 and duplicates==0 and zero_uv==0 and result['nonpositiveComponents']==0 and result['finiteUV'],(ob.name,result)
 return result
for name,parts in assets.items():
 for lod in [0,1]:
  copies=[]
  for ob in parts:
   if lod and ob['lodDetail']>0:continue
   cp=ob.copy();cp.data=ob.data.copy();runtime.objects.link(cp);copies.append(cp)
   bpy.ops.object.select_all(action='DESELECT');cp.select_set(True);bpy.context.view_layer.objects.active=cp
   for mod in list(cp.modifiers):
    if mod.type=='BEVEL' and lod:mod.segments=1
    bpy.ops.object.modifier_apply(modifier=mod.name)
   bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
  bpy.ops.object.select_all(action='DESELECT')
  for cp in copies:cp.select_set(True)
  bpy.context.view_layer.objects.active=copies[0];bpy.ops.object.join();ob=bpy.context.object;ob.name=name+'_LOD'+str(lod)
  mats=[];remap={}
  for i,m in enumerate(ob.data.materials):
   if m not in mats:mats.append(m)
   remap[i]=mats.index(m)
  ids=[remap[p.material_index] for p in ob.data.polygons];ob.data.materials.clear()
  for m in mats:ob.data.materials.append(m)
  for p,i in zip(ob.data.polygons,ids):p.material_index=i
  tri=ob.modifiers.new('Export triangles','TRIANGULATE');bpy.ops.object.modifier_apply(modifier=tri.name);uv_metric(ob.data);ob.data.calc_tangents()
  result=inspect(ob);result['sourceParts']=len(copies)
  path='meshes/'+ob.name+'.fbx';bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,path),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',bake_space_transform=True,axis_forward='-Z',axis_up='Y',mesh_smooth_type='FACE',use_tspace=True,add_leaf_bones=False,path_mode='STRIP',use_custom_props=False)
  result['fbxSha256']=hashlib.sha256(open(os.path.join(OUT,path),'rb').read()).hexdigest();stats[ob.name]=result;export_objects[ob.name]=ob
  print('EXPORTED',ob.name,result['triangles'],flush=True)
  if lod==0:manifest_assets.append({'id':name,'lod0':path,'lod1':'meshes/'+name+'_LOD1.fbx','pivot':'route-level center; source +X right, -Y forward, +Z up','boundsMin':result['unityBoundsMin'],'boundsMax':result['unityBoundsMax'],'materialSlots':result['materialSlots'],'lightmapUVs':'generate','collider':'none','trackAssembly':False})
  ob.hide_set(True);ob.hide_render=True
# Source collections each kept in local coordinates, hidden except civic. Named parts editable.
for name in assets:
 col=bpy.data.collections[name+'_EDITABLE'];col.hide_viewport=name!='NA_CivicFacade_A';col.hide_render=name!='NA_CivicFacade_A'
 for ob in assets[name]:uv_metric(ob.data)
for image in bpy.data.images:
 if image.source=='FILE':image.filepath='//../textures/'+os.path.basename(image.filepath)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT,'source/Nocturne_Architecture_Exemplar.blend'))

# Proposed placements from exact supplied frames. No spline interpolation or camera edits.
instances=[]
def place(ident,asset,index,lateral=0,vertical=0,mode='banked',flip=False):
 f=course['frames'][index];r=Vector(f['right']);u=Vector(f['up']);fw=Vector(f['forward']);pos=Vector(f['position'])+r*lateral+u*vertical
 if mode=='upright':fw=Vector((fw.x,0,fw.z)).normalized();u=Vector((0,1,0));r=u.cross(fw).normalized()
 if flip:r=-r;fw=-fw
 mat=Matrix((r,u,fw)).transposed();q=mat.to_quaternion().normalized()
 instances.append({'id':ident,'assetId':asset,'zoneId':'viaduct' if index<200 else 'station','position':list(pos),'rotation':[q.x,q.y,q.z,q.w],'scale':[1,1,1],'frameMode':mode,'role':'near','gi':'probe','frameIndex':index,'sourceFrame':f,'offsetLateral':lateral,'offsetVertical':vertical,'flipAroundUp':flip})
place('opening-civic-right','NA_CivicFacade_A',95,23,-1.5,'upright')
place('opening-service-left','NA_ServiceFacade_A',120,-22,-2,'upright',True)
place('warm-entry','NA_GalleryPortal_A',1032)
place('warm-rib-01','NA_GalleryRib_A',1040)
place('warm-rib-02','NA_GalleryRib_A',1049)
for i in [1034,1038,1043,1047]:
 place('warm-panel-right-'+str(i),'NA_GalleryPanel_A',i)
 place('warm-panel-left-'+str(i),'NA_GalleryPanel_A',i,flip=True)
layout={'schema':'aaa-nocturne-exemplar-1','courseHash':course_hash,'layoutComplete':False,'status':'BLOCKED_VISUAL_REVIEW','instances':instances}
json.dump(layout,open(os.path.join(OUT,'layout.json'),'w'),indent=2)
fixtures=[]
for asset,parts in assets.items():
 for ob in parts:
  mat=ob.data.materials[0].name
  if 'Diffuser' not in mat:continue
  coords=[ob.matrix_world@v.co for v in ob.data.vertices];c=sum(coords,Vector())/len(coords)
  fixtures.append({'assetId':asset,'sourcePart':ob.name,'materialId':mat,'centerUnityLocal':[c.x,c.z,-c.y],'description':'Physical diffuser centroid, not a tested native light position. Offset the native source outside its emitting surface; source strength is material intent only.'})
json.dump({'fixtures':fixtures,'runtimeLightsAuthored':False},open(os.path.join(OUT,'fixture-sockets.json'),'w'),indent=2)
json.dump(stats,open(os.path.join(OUT,'asset-stats.json'),'w'),indent=2)
json.dump({'schema':'aaa-nocturne-exemplar-1','revision':'exemplar-01','courseHash':course_hash,'layoutComplete':False,'visualStatus':'BLOCKED_VISUAL_REVIEW','sourceBlend':'source/Nocturne_Architecture_Exemplar.blend','assets':manifest_assets,'materials':material_records,'exportConvention':{'axis_forward':'-Z','axis_up':'Y','bake_space_transform':True,'apply_scale_options':'FBX_SCALE_UNITS'},'sourceToUnity':'(x,z,-y)','unit':'metre','normals':'authored weighted split normals; FBX includes tangents','lightmapUVs':'Generate in Unity; UV0 deliberately overlaps metric tiles'},open(os.path.join(OUT,'manifest.json'),'w'),indent=2)

# FBX round-trip into a separate scene. Compare transformed bounds, material face counts and triangles.
verify=bpy.data.scenes.new('FBX_REIMPORT_CHECK');bpy.context.window.scene=verify
roundtrips={}
for name,original in export_objects.items():
 before=set(bpy.data.objects);bpy.ops.import_scene.fbx(filepath=os.path.join(OUT,'meshes/'+name+'.fbx'),use_custom_normals=True)
 imported=[ob for ob in set(bpy.data.objects)-before if ob.type=='MESH'];assert len(imported)==1
 ob=imported[0];me=ob.data;me.calc_loop_triangles();coords=np.array([list(ob.matrix_world@v.co) for v in me.vertices]);src=np.array([list(v.co) for v in original.data.vertices]);err=float(max(abs(coords.min(0)-src.min(0)).max(),abs(coords.max(0)-src.max(0)).max()))
 def counts(mesh):
  out={}
  for p in mesh.polygons:
   key=mesh.materials[p.material_index].name.split('.')[0];out[key]=out.get(key,0)+len(p.vertices)-2
  return out
 same=counts(me)==counts(original.data);assert err<.0002 and len(me.loop_triangles)==stats[name]['triangles'] and same,(name,err,same)
 uv=me.uv_layers.active;normals_finite=all(math.isfinite(x) for n in me.corner_normals for x in n.vector);assert uv and normals_finite
 roundtrips[name]={'boundsMaxErrorMeters':err,'trianglesMatch':True,'materialTriangleCountsMatch':same,'uvLayerPresent':True,'finiteCornerNormals':normals_finite,'reimportObjectScale':list(ob.scale),'reimportObjectEuler':list(ob.rotation_euler),'reimportObjectCount':1}
 bpy.data.objects.remove(ob,do_unlink=True)
json.dump(roundtrips,open(os.path.join(OUT,'checks/fbx-roundtrip.json'),'w'),indent=2)
# All-branch geometry/road clearance: BVH triangle overlap against each closed sampled corridor cell.
road_v=[];road_f=[]
for i in range(len(course['frames'])-1):
 base=len(road_v)
 for f in [course['frames'][i],course['frames'][i+1]]:
  p=np.array(f['position']);r=np.array(f['right']);u=np.array(f['up'])
  for x,y in [(-12.2,-.05),(12.2,-.05),(12.2,8),(-12.2,8)]:road_v.append(tuple(p+r*x+u*y))
 for face in F:
  road_f.extend([(base+face[0],base+face[1],base+face[2]),(base+face[0],base+face[2],base+face[3])])
road_bvh=BVHTree.FromPolygons(road_v,road_f,all_triangles=True)
clearance={}
frames=course['frames'];fp=np.array([f['position'] for f in frames]);fr=np.array([f['right'] for f in frames]);fu=np.array([f['up'] for f in frames]);ff=np.array([f['forward'] for f in frames])
from mathutils import Quaternion
for inst in instances:
 ob=export_objects[inst['assetId']+'_LOD0'];src=np.array([list(v.co) for v in ob.data.vertices]);unity=src[:,[0,2,1]].copy();unity[:,2]*=-1
 x,y,z,w=inst['rotation'];rot=np.array(Quaternion((w,x,y,z)).to_matrix());world=unity@rot.T+np.array(inst['position'])
 faces=[tuple(p.vertices) for p in ob.data.polygons];bvh=BVHTree.FromPolygons(world.tolist(),faces,all_triangles=True);overlap=bvh.overlap(road_bvh)
 # Surface intersection + per-vertex containment checks on EVERY branch; no sphere-only assertion.
 contained=0;min_dist=1e9
 for fi in range(len(frames)-1):
  delta=world-fp[fi];along=delta@ff[fi];near=abs(along)<=1.55
  if not near.any():continue
  a=delta[near];lat=a@fr[fi];up=a@fu[fi];contained+=int(((abs(lat)<12.2)&(up>-.05)&(up<8)).sum())
  sel=(up>-.05)&(up<8)
  if sel.any():min_dist=min(min_dist,float((abs(lat[sel])-12.2).min()))
 clearance[inst['id']]={'surfaceTriangleIntersections':len(overlap),'verticesInsideProtectedCorridorSamples':contained,'minLateralMarginWithinProtectedHeightMeters':None if min_dist==1e9 else min_dist,'method':'BVH triangle intersection against 1200 closed corridor cells plus vertex containment across all frame planes; static sampled conservative corridor only'}
json.dump(clearance,open(os.path.join(OUT,'checks/placement-clearance.json'),'w'),indent=2)
print('CLEARANCE',json.dumps(clearance),flush=True)
print('COMPLETE_NONRENDERING_AUTHORING',flush=True)
