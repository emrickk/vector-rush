"""Original modular city geometry; Blender 4/5; metric FBX + editable assembly."""
import bpy, math, json, sys, random
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parent
OUT=ROOT/'meshes';OUT.mkdir(exist_ok=True)
RENDER=ROOT/'renders';RENDER.mkdir(exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
scene=bpy.context.scene;scene.unit_settings.system='METRIC';scene.unit_settings.scale_length=1
bpy.context.preferences.filepaths.save_version=0
scene.render.engine='CYCLES';scene.cycles.samples=32;scene.cycles.use_denoising=True
scene.render.threads_mode='FIXED';scene.render.threads=8
scene.render.resolution_x=1920;scene.render.resolution_y=1280;scene.render.resolution_percentage=100
scene.view_settings.view_transform='AgX'
M={};rec=json.loads((ROOT/'materials.json').read_text())['materials']
for r in rec:
 m=bpy.data.materials.new(r['id']);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');links=m.node_tree.links
 p.inputs['Metallic'].default_value=r['metallic'];p.inputs['Roughness'].default_value=r['roughness']
 def tex(key,data=False):
  t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=bpy.data.images.load(str(ROOT/'textures'/r[key]),check_existing=True)
  if data:t.image.colorspace_settings.name='Non-Color'
  return t
 t=tex('base');links.new(t.outputs['Color'],p.inputs['Base Color'])
 if r['normal']:
  nt=tex('normal',True);nm=m.node_tree.nodes.new('ShaderNodeNormalMap');nm.inputs['Strength'].default_value=.7;links.new(nt.outputs['Color'],nm.inputs['Color']);links.new(nm.outputs['Normal'],p.inputs['Normal'])
 if r['ms']:
  nt=tex('ms',True);channels=m.node_tree.nodes.new('ShaderNodeSeparateColor');links.new(nt.outputs['Color'],channels.inputs['Color']);links.new(channels.outputs['Red'],p.inputs['Metallic']);inv=m.node_tree.nodes.new('ShaderNodeMath');inv.operation='SUBTRACT';inv.inputs[0].default_value=1;links.new(nt.outputs['Alpha'],inv.inputs[1]);links.new(inv.outputs[0],p.inputs['Roughness'])
 if r['emission']:
  et=tex('emission');links.new(et.outputs['Color'],p.inputs['Emission Color']);p.inputs['Emission Strength'].default_value=r.get('emissionStrength',1)
 if r['id'].endswith('_wet'):
  mix=m.node_tree.nodes.new('ShaderNodeMixRGB');mix.blend_type='MULTIPLY';mix.inputs[0].default_value=1;mix.inputs[2].default_value=(*r['baseTint'],1);links.new(t.outputs['Color'],mix.inputs[1]);links.new(mix.outputs[0],p.inputs['Base Color'])
 M[r['id']]=m
for key,col,power,rough,metal in [('glass',(.018,.048,.058),0,.21,.5),('warm',(.9,.48,.16),2.2,.4,.1),('cyan',(.12,.7,.72),2,.4,.1),('red',(.85,.065,.03),3,.4,.1),('black',(.015,.024,.027),0,.6,.2)]:
 m=bpy.data.materials.new(key);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*col,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough;p.inputs['Emission Color'].default_value=(*col,1);p.inputs['Emission Strength'].default_value=power;M[key]=m
parts=[];lod=0
metric={r['id']:r['tileMeters'] for r in rec}
def uv_project(ob,mat):
 uv=ob.data.uv_layers.active or ob.data.uv_layers.new(name='UVMap');scale=metric.get(mat,2)
 for poly in ob.data.polygons:
  axis=max(range(3),key=lambda i:abs(poly.normal[i]));axes=[i for i in range(3) if i!=axis]
  for li in poly.loop_indices:
   co=ob.data.vertices[ob.data.loops[li].vertex_index].co
   uv.data[li].uv=(co[axes[0]]/scale,co[axes[1]]/scale)
def box(name,pos,size,mat='painted_graphite',bevel=.025,detail=False):
 if detail and lod:return
 bpy.ops.mesh.primitive_cube_add(size=1,location=pos);ob=bpy.context.object;ob.name=name;ob.dimensions=size
 bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);ob.data.materials.append(M[mat]);uv_project(ob,mat)
 if bevel and not lod:
  md=ob.modifiers.new('Manufactured edge radii','BEVEL');md.width=min(bevel,min(size)*.25);md.segments=2
  md=ob.modifiers.new('Weighted corner normals','WEIGHTED_NORMAL')
 parts.append(ob);return ob
def cyl(name,pos,r,depth,mat='brushed_alloy',axis='Z',detail=False):
 if detail and lod:return
 bpy.ops.mesh.primitive_cylinder_add(vertices=16 if lod else 48,radius=r,depth=depth,location=pos);ob=bpy.context.object;ob.name=name
 if axis=='Y':ob.rotation_euler[0]=math.pi/2
 if axis=='X':ob.rotation_euler[1]=math.pi/2
 ob.data.materials.append(M[mat]);parts.append(ob)
 for p in ob.data.polygons:p.use_smooth=len(p.vertices)==4
 return ob
def beam(name,a,b,r=.055,mat='brushed_alloy',detail=False):
 if detail and lod:return
 a,b=Vector(a),Vector(b);ob=cyl(name,(a+b)/2,r,(a-b).length,mat);ob.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();return ob
def face(name,pos,w,h,mat):
 # Front faces negative Y. UV is [0,1] and never repeats advertising artwork.
 xx,yy,zz=pos;mesh=bpy.data.meshes.new(name);mesh.from_pydata([(-w/2,0,-h/2),(w/2,0,-h/2),(w/2,0,h/2),(-w/2,0,h/2)],[],[(0,1,2,3)]);mesh.update()
 ob=bpy.data.objects.new(name,mesh);bpy.context.collection.objects.link(ob);ob.location=pos;mesh.materials.append(M[mat]);uv=mesh.uv_layers.new()
 for i,q in enumerate([(0,0),(1,0),(1,1),(0,1)]):uv.data[i].uv=q
 parts.append(ob);return ob
def feet(w,d):
 for x in [-w/2+.12,w/2-.12]:
  for y in [-d/2+.12,d/2-.12]:box('Elastomer mounting foot',(x,y,.10),(.23,.25,.2),'rubber')
def vents(pos,w,h,axis='front'):
 x,y,z=pos;box('Recessed intake',(x,y,z),(w,.06,h),'black',0)
 for k in range(3 if lod else 12):
  zz=z-h*.43+k*h*.86/(2 if lod else 11)
  box('Louver blade',(x,y-.05,zz),(w,.12,.025),'brushed_alloy',.005)
def bolts(w,y,z):
 for x in [-w*.44,w*.44]:cyl('Captive service bolt',(x,y,z),.022,.018,'brushed_alloy','Y',True)
def hvac():
 feet(2.7,1.9);box('Cooling unit enclosure',(0,0,1),(2.8,2,1.6));box('Service lid',(0,0,1.83),(2.9,2.1,.10),'brushed_alloy')
 vents((0,-1.01,1),2.3,.9)
 for x in [-.7,.7]:
  cyl('Fan recess',(x,0,1.9),.55,.10,'black');cyl('Fan hub',(x,0,1.99),.12,.09)
  for k in range(4 if lod else 8):
   ob=box('Fan blade',(x,0,1.96),(.83,.10,.035),'brushed_alloy',.008);ob.rotation_euler[2]=k*math.pi/(4 if lod else 8)
  for k in range(-3,4):box('Fan safety grille',(x+k*.13,0,2.03),(.015,1,.016),'painted_graphite',0,True)
 box('Service control',(1.405,-.35,1),(.04,.45,.6),'brushed_alloy');face('Unit ID',(0,-1.105,1.48),.7,.15,'sign_repair');bolts(2.5,-1.09,.45)
def duct():
 box('Duct insulated core',(0,0,.65),(3.8,1.2,1.1),'corrugated_steel')
 for x in [-1.8,-.6,.6,1.8]:box('Flanged reinforcement',(x,0,.65),(.09,1.28,1.18),'brushed_alloy')
 for x in [-1.2,1.2]:box('Support saddle',(x,0,.1),(.15,1.7,.2),'painted_graphite')
 vents((0,-.61,.65),3.45,.75)
def extractor():
 feet(1.2,1.2);box('Extractor base plate',(0,0,.22),(1.25,1.25,.12),'brushed_alloy');cyl('Extraction riser',(0,0,1.05),.48,1.8,'corrugated_steel');cyl('Rain collar',(0,0,1.96),.7,.16)
 cyl('Turbine cap',(0,0,2.1),.60,.23,'painted_graphite')
 for k in range(8 if lod else 16):
  a=k*math.tau/(8 if lod else 16);beam('Intake vanes',(.5*math.cos(a),.5*math.sin(a),1.85),(.6*math.cos(a+.15),.6*math.sin(a+.15),2.10),.018,'brushed_alloy')
def cabinet():
 feet(1.4,.65);box('Distribution cabinet',(0,0,1.13),(1.5,.72,2.1),'painted_graphite');box('Inset service door',(0,-.38,1.2),(1.31,.08,1.7),'brushed_alloy');vents((0,-.44,.65),1.1,.42)
 box('Door handle',(.5,-.5,1.3),(.04,.06,.23),'black');face('Service warning',(0,-.433,1.62),.78,.3,'sign_repair');cyl('Status lamp',(.51,-.46,1.85),.035,.02,'warm','Y')
 for x in [-.45,0,.45]:beam('Conduit',(x,.30,.15),(x,.30,2.5),.035,'oxidized_copper')
def tank():
 for x in [-.8,.8]:
  for y in [-.8,.8]:beam('Tank support',(x,y,0),(x,y,1.2),.07,'painted_graphite')
 cyl('Water vessel',(0,0,2.25),1.12,2.3,'corrugated_steel');cyl('Top cap',(0,0,3.43),1.14,.14)
 for z in [1.18,2.2,3.32]:cyl('Tank seam',(0,0,z),1.16,.06)
 for z in [i*.25 for i in range(14)]:beam('Ladder rung',(1.24,-.25,z),(1.24,.25,z),.025,'brushed_alloy',True)
 for y in [-.3,.3]:beam('Ladder stile',(1.24,y,0),(1.24,y,3.6),.04)
def cables():
 for x in [-1.8,1.8]:box('Cable hanger',(x,0,1.3),(.12,.5,2.6),'painted_graphite')
 for k in range(4):
  prev=None
  for i in range(9):
   x=-2+i*.5;p=(x,k*.075-.1,2.15-.27*(1-(x/2)**2))
   if prev:beam('Sagging cable',prev,p,.018,'rubber')
   prev=p
 box('Junction box',(1.8,-.05,1.2),(.36,.28,.55),'painted_graphite');face('Circuit number',(1.8,-.20,1.2),.25,.35,'sign_tower')
def walkway():
 box('Maintenance deck',(0,0,.3),(8,2,.28),'painted_graphite')
 for x in [-3.8,-2,0,2,3.8]:
  for y in [-.96,.96]:beam('Rail upright',(x,y,.4),(x,y,1.5),.038)
 for y in [-.96,.96]:
  for z in [.8,1.5]:beam('Handrail',(-4,y,z),(4,y,z),.035)
 for x in [-3.6,3.6]:box('Supported end shoe',(x,0,.1),(.5,2.25,.2),'brushed_alloy')
 for x in [i*.14-3.85 for i in range(56)]:box('Deck grating',(x,0,.46),(.03,1.8,.025),'brushed_alloy',0,True)
def gantry():
 for x in [-5,5]:
  box('Structural pier',(x,0,2.8),(.42,.55,5.6),'painted_graphite');box('Bolted base',(x,0,.1),(1,.9,.2),'brushed_alloy')
 for z in [5,5.8]:box('Truss chord',(0,0,z),(10.6,.36,.26),'painted_graphite')
 for i in range(10):beam('Triangulated web',(-5+i,0,5),(-4+i,0,5.8),.055,'brushed_alloy')
 for x in [-3.8,-1.9,1.9,3.8]:box('Warning lamp',(x,-.23,5.6),(.13,.12,.12),'red')
 face('Transit board',(0,-.22,4.6),4.3,1.4,'sign_metro');box('Board housing',(0,0,4.6),(4.5,.4,1.6))
def sign(kind):
 if kind=='blade':w,h,art=1.5,6,'sign_hotel'
 elif kind=='market':w,h,art=7,2.4,'sign_market'
 elif kind=='portrait':w,h,art=3.3,5,'ad_afterhours' if 'ad_afterhours' in M else 'sign_tower'
 elif kind=='koi':w,h,art=6,4,'ad_koi_market' if 'ad_koi_market' in M else 'sign_market'
 elif kind=='transit':w,h,art=2.8,4.2,'ad_transit' if 'ad_transit' in M else 'sign_metro'
 else:w,h,art=2.2,2.2,'sign_ramen'
 if kind=='square':
  cyl('Circular enamel housing',(0,0,1.75),1.17,.25,'painted_graphite','Y')
  bpy.ops.mesh.primitive_circle_add(vertices=32 if lod else 64,radius=1.1,fill_type='NGON',location=(0,-.135,1.75),rotation=(math.pi/2,0,0));ob=bpy.context.object;ob.name='Round noodle artwork';ob.data.materials.append(M[art]);uv=ob.data.uv_layers.new()
  for poly in ob.data.polygons:
   for li in poly.loop_indices:
    co=ob.data.vertices[ob.data.loops[li].vertex_index].co;uv.data[li].uv=(co.x/2.2+.5,co.y/2.2+.5)
  parts.append(ob)
  beam('Round sign mounting post',(0,.15,0),(0,.15,2.5),.06,'brushed_alloy');beam('Round sign brace',(0,.15,.15),(0,.75,1),.04,'brushed_alloy');return
 box('Pressed sign back',(0,0,h/2+.55),(w+.18,.28,h+.18),'painted_graphite');face('Replaceable artwork',(0,-.151,h/2+.55),w,h,art)
 for x in [-w/2-.055,w/2+.055]:box('Anodized edge',(x,-.10,h/2+.55),(.075,.22,h+.18),'brushed_alloy')
 for z in [.5,h+.60]:box('Edge cap',(0,-.10,z),(w+.2,.22,.075),'brushed_alloy')
 for x in [-w*.32,w*.32]:
  beam('Mount',(x,.05,0),(x,.05,h*.8),.055,'painted_graphite');beam('Braced mount',(x,.05,.1),(x,.7,1.2),.045,'brushed_alloy')
 beam('Power conduit',(-w*.42,.18,.25),(-w*.42,.18,h*.9),.025,'rubber',True)
 for z in [1,h-.2]:bolts(w,-.17,z)
def canopy():
 box('Shop facade',(0,.35,1.7),(6.5,.7,3.4),'ceramic_tiles');box('Shutter recess',(0,-.04,1.5),(4.6,.14,2.65),'corrugated_steel')
 for x in [-2.8,2.8]:box('Concrete shop pier',(x,-.1,1.7),(.65,.9,3.4),'cast_concrete')
 ob=box('Corrugated awning',(0,-1,3.55),(7,2.6,.12),'corrugated_steel_wet');ob.rotation_euler[0]=.10
 for x in [-2.8,2.8]:beam('Awning brace',(x,0,2.6),(x,-1.8,3.45),.045,'oxidized_copper')
 face('Shopfront sign',(0,-.13,3.0),4.6,.6,'sign_repair');box('Warm canopy practical',(0,-.8,3.40),(3,.13,.07),'warm')
 for x in [-2,2]:beam('Rain downpipe',(x,.8,.15),(x,.8,3.8),.06,'oxidized_copper')
def building(style):
 # Deliberately different stepped silhouettes; shared metric facade system.
 if style=='tenement':w,d,h=12,10,23;material='cast_concrete';fac='facade_tenement'
 elif style=='office':w,d,h=14,12,42;material='painted_graphite';fac='facade_office'
 else:w,d,h=9,11,31;material='cast_concrete';fac='facade_service'
 box('Podium',(0,0,2.3),(w+2,d+2,4.6),'cast_concrete');box('Building core',(0,0,4.6+h/2),(w,d,h),material)
 face('Occupied front',(0,-d/2-.015,4.6+h/2),w-.8,h-.6,fac)
 ob=face('Occupied side',(w/2+.015,0,4.6+h/2),d-.8,h-.6,fac);ob.rotation_euler[2]=math.pi/2
 for z in [4.6+k*3 for k in range(int(h/3)+1)]:
  box('Floor return',(0,0,z),(w+.42,d+.42,.18),'brushed_alloy',.025)
 for x in [-w/2,w/2]:box('Facade pier',(x,-d/2-.12,4.6+h/2),(.35,.32,h),material)
 box('Setback rooftop',(1,1,h+6),(w*.6,d*.65,2.8),'corrugated_steel');box('Rooftop coping',(0,0,h+4.7),(w+.5,d+.5,.25),'brushed_alloy')
 if style=='tenement':
  for z in [7,10,13,16,19,22,25]:
   box('Service balcony',(w/2+1.3,0,z),(2.6,3,.12),'painted_graphite')
   for yy in [-1.4,1.4]:
    for zz in [.55,1]:beam('Balcony safety rail',(w/2+.1,yy,z+zz),(w/2+2.6,yy,z+zz),.035,'brushed_alloy')
    for xx in [w/2+.15,w/2+2.5]:beam('Balcony rail post',(xx,yy,z),(xx,yy,z+1),.035,'brushed_alloy')
   for zz in [.55,1]:beam('Balcony outer rail',(w/2+2.5,-1.4,z+zz),(w/2+2.5,1.4,z+zz),.035,'brushed_alloy')
   for x in [-3.5,2.5]:
    box('AC enclosure',(x,-d/2-.3,z-.4),(1,.55,.6),'brushed_alloy',detail=True);vents((x,-d/2-.6,z-.4),.8,.4)
 elif style=='office':
  box('Asymmetric vertical fin',(-w/2-.5,1,h/2+5),(1.2,d+1,h+4),'brushed_alloy')
  box('Crown lantern',(0,0,h+8),(5,5,4),'glass');box('Crown red marker',(0,0,h+10.1),(5.4,5.4,.13),'red')
  for x in [-5,0,5]:box('Tower mullion',(x,-d/2-.05,h/2+5),(.12,.12,h),'brushed_alloy')
 else:
  for x in [-2.5,2.5]:beam('Service riser',(x,-d/2-.25,3),(x,-d/2-.25,h+5),.10,'oxidized_copper')
  for z in [8,14,20,26]:vents((0,-d/2-.06,z),5,1.8)
 cyl('Rooftop aerial',(2,2,h+9),.04,7,'brushed_alloy');box('Aerial red beacon',(2,2,h+12.6),(.13,.13,.2),'red')
def neon_roof():
 for x in [-3.3,3.3]:
  box('Roof anchor',(x,0,.08),(.6,.8,.16),'brushed_alloy')
  beam('Sign upright',(x,0,0),(x,0,3.2),.045,'painted_graphite')
  beam('Wind brace',(x,1.1,0),(x,0,2.9),.04,'brushed_alloy')
 for z in [.6,2.8]:beam('Sign crossbar',(-3.5,0,z),(3.5,0,z),.05,'painted_graphite')
 font=bpy.data.fonts.load(str(ROOT/'fonts/Rajdhani-SemiBold.ttf'))
 curve=bpy.data.curves.new('Editable neon lettering','FONT');curve.body='24 H';curve.font=font;curve.align_x='CENTER';curve.size=2.8;curve.extrude=.016;curve.bevel_depth=.012;curve.bevel_resolution=2
 ob=bpy.data.objects.new('Neon 24 H',curve);bpy.context.collection.objects.link(ob);ob.location=(0,-.10,.75);ob.rotation_euler=(math.pi/2,0,0);curve.materials.append(M['red']);bpy.context.view_layer.objects.active=ob;ob.select_set(True);bpy.ops.object.convert(target='MESH');ob=bpy.context.object;parts.append(ob)
 for x in [-2.8,2.8]:box('Transformer',(x,.12,.5),(.38,.2,.3),'black')
ASSETS=[('NeonRoofSign',neon_roof),('CoolingUnit',hvac),('DuctBank',duct),('RoofExtractor',extractor),('PowerCabinet',cabinet),('WaterTank',tank),('CableSpan',cables),('MaintenanceBridge',walkway),('TransitGantry',gantry),('Shopfront',canopy)]
ASSETS += [('Sign_'+k,lambda k=k:sign(k)) for k in ['blade','market','portrait','koi','transit','square']]
ASSETS += [('Building_'+k,lambda k=k:building(k)) for k in ['tenement','office','service']]
masters={};stats=[]
for name,fn in ASSETS:
 for lod in [0,1]:
  parts=[];fn();bpy.ops.object.select_all(action='DESELECT')
  for ob in parts:
   ob.select_set(True);bpy.context.view_layer.objects.active=ob
   for mod in list(ob.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
  bpy.context.view_layer.objects.active=parts[0];bpy.ops.object.join();ob=bpy.context.object;ob.name='NC_'+name+'_LOD'+str(lod)
  scene.cursor.location=(0,0,0);bpy.ops.object.origin_set(type='ORIGIN_CURSOR');bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
  ob.data.calc_loop_triangles();stats.append(dict(id=name,lod=lod,file=ob.name+'.fbx',triangles=len(ob.data.loop_triangles),vertices=len(ob.data.vertices),dimensions=list(ob.dimensions),materials=[m.name for m in ob.data.materials],uv=bool(ob.data.uv_layers)))
  bpy.ops.export_scene.fbx(filepath=str(OUT/(ob.name+'.fbx')),use_selection=True,object_types={'MESH'},apply_unit_scale=True,axis_forward='-Z',axis_up='Y',bake_space_transform=True,use_mesh_modifiers=True,add_leaf_bones=False,path_mode='STRIP',mesh_smooth_type='FACE')
  if lod==0:masters[name]=ob
  ob.hide_render=True;ob.hide_set(True)
(ROOT/'mesh-manifest.json').write_text(json.dumps(dict(units='meters',up='Y in FBX; Z in Blender',assets=stats),indent=2))
# Composed asset-stage using real exported source geometry, not generated concept art.
placements=[]
def place(name,pos,rot=0,scale=1):
 placements.append(dict(id=name,position=[pos[0],pos[2],-pos[1]],yaw=-rot*180/math.pi,scale=scale))
 ob=masters[name].copy();ob.data=masters[name].data;bpy.context.collection.objects.link(ob);ob.name='Stage_'+name;ob.location=pos;ob.rotation_euler[2]=rot;ob.scale=(scale,)*3;ob.hide_render=False;ob.hide_set(False);return ob
parts=[];lod=0
box('Review ground',(0,5,-.25),(75,60,.5),'asphalt_wet',0)
box('Foreground rooftop',(-8,1,1.5),(17,12,3),'cast_concrete');box('Roof sheet',(-8,1,3.08),(17.4,12.4,.15),'corrugated_steel_wet')
place('NeonRoofSign',(-3.3,-1.8,3.16));place('Shopfront',(-9,-5,0));place('Sign_market',(-12,-5.4,3.2));place('CoolingUnit',(-13,0,3.2));place('DuctBank',(-10,2,3.2));place('RoofExtractor',(-3,3,3.2));place('WaterTank',(-13,4,3.2));place('PowerCabinet',(-1,-2,0));place('CableSpan',(-3,-4,3.16))
place('Building_tenement',(-15,16,0));place('Building_office',(5,22,0));place('Building_service',(20,16,0));place('Building_office',(-30,30,0),0,1.3)
place('Sign_blade',(-7,10,9));place('Sign_portrait',(7,12,8));place('Sign_koi',(18,9,3));place('Sign_transit',(-22,9,12));place('Sign_square',(-15,-6,1.8))
place('MaintenanceBridge',(1,8,8));place('TransitGantry',(2,4,0));place('CoolingUnit',(15,6,0),.3);place('DuctBank',(17,6,0),.3);place('PowerCabinet',(13,7,0))
(ROOT/'assembly-layout.json').write_text(json.dumps(dict(placements=placements),indent=2))
world=bpy.data.worlds.new('Teal city ambient');scene.world=world;world.use_nodes=True;world.node_tree.nodes.get('Background').inputs[0].default_value=(.055,.12,.16,1);world.node_tree.nodes.get('Background').inputs[1].default_value=.45
def area(name,pos,col,power,size,target):
 data=bpy.data.lights.new(name,'AREA');data.energy=power;data.color=col;data.shape='DISK';data.size=size;ob=bpy.data.objects.new(name,data);bpy.context.collection.objects.link(ob);ob.location=pos;ob.rotation_euler=(Vector(target)-ob.location).to_track_quat('-Z','Y').to_euler();return ob
area('Cool overhead',(-8,-8,26),(.45,.7,1),22000,20,(0,5,0));area('Scarlet shop spill',(-8,-5,8),(1,.07,.025),5500,7,(-6,-2,0));area('Billboard bounce',(10,8,12),(.20,.8,1),6000,9,(2,0,2));area('Warm service practical',(-12,-5,4),(1,.48,.17),700,3,(-10,-7,0))
camd=bpy.data.cameras.new('Asset review camera');cam=bpy.data.objects.new('Asset review camera',camd);bpy.context.collection.objects.link(cam);scene.camera=cam
cam.location=(29,-35,19);target=Vector((-1,8,10));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();camd.lens=42
# Restrained glow only for rendered review; texture source stays sharp.

for material in M.values():material.use_fake_user=True
for image in bpy.data.images:
 if image.source=='FILE':image.filepath=bpy.path.relpath(image.filepath,start=str(ROOT))
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'NightCityKit.blend'))
scene.render.filepath=str(RENDER/'night-assembly.png');bpy.ops.render.render(write_still=True)
cam.location=(5,-16,10);cam.rotation_euler=(Vector((-7,-.5,4))-cam.location).to_track_quat('-Z','Y').to_euler();camd.lens=48;scene.render.filepath=str(RENDER/'rooftop-detail.png');bpy.ops.render.render(write_still=True)
print('NIGHT_CITY_EXPORTED',len(stats),'LOD meshes')
