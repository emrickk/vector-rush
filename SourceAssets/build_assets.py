"""Vector Rush / KESTREL 07. Original authored racing craft.
Run: Blender --background --python SourceAssets/build_assets.py
Blender source coordinates: nose -Y, up +Z. FBX uses -Z forward / Y up.
The axis conversion maps source -Y to Unity +Z (see README).
"""
import bpy, bmesh, math, json
from mathutils import Vector
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'UnityProject/Assets/Resources/Art'
RENDER = ROOT / 'evidence/asset-renders'
OUT.mkdir(parents=True, exist_ok=True)
RENDER.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for b in bpy.data.materials: bpy.data.materials.remove(b)
craft = bpy.data.collections.new('KESTREL_07_EXPORT')
bpy.context.scene.collection.children.link(craft)

def material(name, color, metal=0, rough=.35, emission=None):
    m = bpy.data.materials.new(name); m.diffuse_color=(*color, 1); m.use_nodes=True
    n=m.node_tree.nodes.get('Principled BSDF')
    n.inputs['Base Color'].default_value=(*color,1)
    n.inputs['Metallic'].default_value=metal; n.inputs['Roughness'].default_value=rough
    if emission:
        n.inputs['Emission Color'].default_value=(*color,1)
        n.inputs['Emission Strength'].default_value=emission
    return m

M={
 'Ivory': material('Ivory',(.8,.84,.81),.44,.28),
 'Graphite': material('Graphite',(.024,.037,.045),.68,.31),
 'Glass': material('Glass',(.009,.042,.053),.79,.14),
 'Signal': material('Signal',(.34,.47,.001),.0,.58),
 'Engine': material('Engine',(.015,.88,.81),.3,.19,5),
 'Metal': material('Metal',(.19,.25,.27),.85,.28),
 'Ceramic': material('Ceramic',(.055,.08,.088),.35,.47),
 'Ink': material('Ink',(.02,.04,.045),.2,.5),
 'WhiteMark': material('WhiteMark',(.83,.91,.90),.2,.45)
}

def finish(o,name,mat,bevel=0):
    o.name=name
    for c in list(o.users_collection): c.objects.unlink(o)
    craft.objects.link(o)
    o.data.materials.append(M[mat])
    if bevel:
        b=o.modifiers.new('Machined edge radii','BEVEL'); b.width=bevel; b.segments=3
        b.affect='EDGES'
        b=o.modifiers.new('Face weighted normals','WEIGHTED_NORMAL'); b.keep_sharp=True; b.weight=30
    return o

def loft(name, sections, mat, bevel=.035):
    # section = Y, center X, center Z, half width, half height; octagonal edge profile
    v=[]
    profile=[(-.72,1),(.72,1),(1,.58),(1,-.54),(.63,-1),(-.63,-1),(-1,-.54),(-1,.58)]
    for y,x,z,w,h in sections:
        v += [(x+px*w,y,z+pz*h) for px,pz in profile]
    f=[tuple(reversed(range(8)))]
    for r in range(len(sections)-1):
        for i in range(8): f.append((r*8+i,r*8+(i+1)%8,(r+1)*8+(i+1)%8,(r+1)*8+i))
    f.append(tuple((len(sections)-1)*8+i for i in range(8)))
    mesh=bpy.data.meshes.new(name); mesh.from_pydata(v,[],f); mesh.update()
    o=bpy.data.objects.new(name,mesh); craft.objects.link(o); o.data.materials.append(M[mat])
    if bevel:
        b=o.modifiers.new('Soft manufactured edges','BEVEL'); b.width=bevel; b.segments=3
        b=o.modifiers.new('Weighted corner normals','WEIGHTED_NORMAL'); b.keep_sharp=True
    return o

def slab(name,outline,z,thickness,mat,bevel=.02):
    v=[(x,y,z) for x,y in outline]+[(x,y,z+thickness) for x,y in outline]
    n=len(outline); f=[tuple(reversed(range(n))),tuple(range(n,n*2))]
    f += [(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    mesh=bpy.data.meshes.new(name); mesh.from_pydata(v,[],f); mesh.update()
    o=bpy.data.objects.new(name,mesh); craft.objects.link(o); o.data.materials.append(M[mat])
    if bevel:
        b=o.modifiers.new('Aero edge fillet','BEVEL'); b.width=bevel;b.segments=3
        o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
    return o

def box(name,loc,scale,mat,bevel=.02,rotation=None):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc)
    o=bpy.context.object; o.dimensions=scale
    if rotation:o.rotation_euler=rotation
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True)
    return finish(o,name,mat,bevel)

def cylinder(name,loc,radius,depth,mat,vertices=40,rotation=(math.pi/2,0,0)):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices,radius=radius,depth=depth,location=loc,rotation=rotation)
    o=finish(bpy.context.object,name,mat,.012)
    for p in o.data.polygons:p.use_smooth=True
    return o

def ring(name,loc,radius,tube,mat):
    bpy.ops.mesh.primitive_torus_add(major_radius=radius,minor_radius=tube,major_segments=48,minor_segments=10,location=loc,rotation=(math.pi/2,0,0))
    o=finish(bpy.context.object,name,mat)
    for p in o.data.polygons:p.use_smooth=True
    return o

def rod(name,a,b,r,mat):
    d=Vector(b)-Vector(a);o=cylinder(name,(Vector(a)+Vector(b))/2,r,d.length,mat,16,(0,0,0))
    o.rotation_euler=d.to_track_quat('Z','Y').to_euler();return o

def label(name,text,loc,size,mat='Ink',rotation=(0,0,0)):
    bpy.ops.object.text_add(location=loc,rotation=rotation)
    o=bpy.context.object;o.data.body=text;o.data.size=size;o.data.extrude=.0008
    o.data.space_character=1.15;o.data.align_x='CENTER';o.data.align_y='CENTER'
    bpy.ops.object.convert(target='MESH');return finish(bpy.context.object,name,mat)

def glazing():
    # A curved, sealed glazing surface, with smooth arc normals rather than planar facets.
    sections=[(-1.48,.035,.43,.012),(-1.34,.23,.46,.04),(-1.10,.36,.47,.13),(-.72,.47,.48,.22),(-.10,.53,.49,.33),(.40,.50,.46,.28),(.90,.37,.39,.23),(1.28,.23,.34,.14),(1.60,.03,.31,.025)]
    verts=[];steps=24
    for y,w,base,h in sections:
        for j in range(steps+1):
            angle=-math.pi/2+math.pi*j/steps
            verts.append((math.sin(angle)*w,y,base+math.cos(angle)*h))
    n=steps+1;faces=[]
    for i in range(len(sections)-1):
        for j in range(steps):faces.append((i*n+j,i*n+j+1,(i+1)*n+j+1,(i+1)*n+j))
        faces.append((i*n,(i+1)*n,(i+1)*n+steps,i*n+steps))
    faces.extend([tuple(reversed(range(n))),tuple((len(sections)-1)*n+j for j in range(n))])
    mesh=bpy.data.meshes.new('Smoked curved canopy');mesh.from_pydata(verts,[],faces);mesh.update()
    o=bpy.data.objects.new('Smoked curved canopy',mesh);craft.objects.link(o);o.data.materials.append(M['Glass'])
    for p in mesh.polygons:p.use_smooth=True
    sub=o.modifiers.new('Glazing curvature','SUBSURF');sub.levels=2;sub.render_levels=2
    return o

# The structural bridge and central fuselage deliberately sit below the armor.
loft('Monocoque keel',[(-3.55,0,.08,.055,.06),(-2.6,0,.07,.36,.15),(-1.6,0,.04,.72,.21),(0,0,.02,.87,.24),(1.7,0,-.01,.8,.21),(2.95,0,-.08,.42,.17)],'Graphite')
loft('Central nose spear',[(-3.63,0,.15,.045,.035),(-2.75,0,.20,.31,.10),(-1.7,0,.30,.53,.12),(-1.10,0,.35,.48,.11)],'Ivory',.025)
for y in [.20,1.75]:
    slab('Exposed transverse spar', [(-1.92,y-.16),(-.7,y-.29),(.7,y-.29),(1.92,y-.16),(1.92,y+.16),(.6,y+.30),(-.6,y+.30),(-1.92,y+.16)],-.03,.18,'Metal')
    box('Bridge center web',(0,y,.12),(1.5,.12,.12),'Graphite')

# Sidepods are lofted, continuously tapering, with separate panels revealing seams.
for side,side_name in [(-1,'Port'),(1,'Starboard')]:
    def S(y,x,z,w,h): return (y,side*x,z,w,h)
    loft(side_name+' carbon pontoon',[S(-4.05,1.73,.1,.055,.075),S(-3.55,1.71,.10,.34,.18),S(-1.65,1.6,.05,.59,.29),S(.1,1.64,.03,.60,.34),S(1.85,1.65,-.02,.57,.36),S(3.1,1.65,-.05,.48,.29)],'Graphite')
    front_armor=loft(side_name+' forward armor',[S(-4.08,1.73,.21,.045,.025),S(-3.6,1.70,.28,.33,.10),S(-2.35,1.61,.38,.49,.13),S(-.77,1.60,.39,.53,.14)],'Ivory')
    loft(side_name+' mid armor',[S(-.69,1.60,.39,.53,.14),S(.55,1.64,.35,.54,.13),S(1.28,1.64,.28,.49,.13)],'Ivory')
    loft(side_name+' tail armor',[S(1.37,1.65,.27,.48,.12),S(2.18,1.65,.24,.48,.13),S(2.67,1.65,.15,.44,.13)],'Ivory')
    # Thin floating chine panels give the hull a carved mechanical underside.
    loft(side_name+' lower chine',[S(-3.48,1.73,-.02,.27,.035),S(-1.55,1.68,-.13,.57,.065),S(.50,1.71,-.20,.56,.06),S(2.08,1.72,-.20,.45,.045)],'Metal',.018)
    loft(side_name+' acid nose stripe',[S(-3.76,1.74,.339,.032,.005),S(-3.60,1.70,.388,.05,.007),S(-3.35,1.68,.414,.065,.007),S(-2.35,1.61,.518,.075,.007),S(-1.83,1.60,.5245,.075,.007)],'Signal',.005)
    loft(side_name+' acid aft stripe',[S(-1.29,1.60,.5314,.075,.007),S(-.85,1.60,.537,.075,.007)],'Signal',.005)
    # Physical shallow service recess; its dark seam remains visible in a standard Unity material.
    cutter=box('Temporary service recess',(side*1.60,-1.56,.544),(.66,.44,.08),'Graphite',.035)
    bpy.context.view_layer.objects.active=cutter
    for mod in list(cutter.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
    bpy.context.view_layer.objects.active=front_armor
    for mod in list(front_armor.modifiers):bpy.ops.object.modifier_apply(modifier=mod.name)
    mod=front_armor.modifiers.new('Inset service panel cavity','BOOLEAN');mod.operation='DIFFERENCE';mod.object=cutter
    bpy.ops.object.modifier_apply(modifier=mod.name);bpy.data.objects.remove(cutter,do_unlink=True)
    box(side_name+' recessed service gasket',(side*1.60,-1.56,.507),(.62,.40,.008),'Graphite',.03)
    box(side_name+' recessed service panel',(side*1.60,-1.56,.512),(.56,.34,.008),'Ivory',.023)
    for offset in [-.21,.21]:cylinder(side_name+' service latch',(side*1.60+offset,-1.56,.518),.018,.006,'Metal',12,(0,0,0))
    # Recessed vents on the top of each aft pod.
    box(side_name+' inlet recess',(side*1.65,1.8,.39),(.52,.53,.05),'Graphite',.03)
    for j in range(6):
        box(side_name+' cooling gill %02d'%j,(side*1.65,1.59+j*.078,.422),(.44,.025,.025),'Metal',.007)
    # Angled outer stabilizers, swept in the direction of travel.
    outline=[(side*x,y) for x,y in [(1.83,.30),(2.14,.65),(2.70,2.72),(2.65,3.05),(1.83,2.30)]]
    slab(side_name+' swept stabilizer',outline,.12,.105,'Ivory')
    outline=[(side*x,y) for x,y in [(2.28,1.46),(2.39,1.67),(2.72,2.85),(2.62,2.90)]]
    slab(side_name+' fin identifier',outline,.225,.01,'Signal',.005)
    # Canted upright tailplane.
    mesh=bpy.data.meshes.new(side_name+' tailplane')
    vs=[(side*1.98,1.94,.30),(side*2.13,3.08,.28),(side*2.38,3.08,1.08),(side*2.32,2.88,1.20)]
    vs += [(x+side*.065,y,z) for x,y,z in vs]
    mesh.from_pydata(vs,[],[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)])
    o=bpy.data.objects.new(side_name+' upright tailplane',mesh);craft.objects.link(o);o.data.materials.append(M['Graphite'])
    b=o.modifiers.new('Tail edge radii','BEVEL');b.width=.02;b.segments=3;o.modifiers.new('Tail normals','WEIGHTED_NORMAL')
    # Exposed aft engine: containment rings, twelve blades, cyan inner energizer.
    cylinder(side_name+' turbine casing',(side*1.65,2.90,-.03),.45,.73,'Ceramic')
    cylinder(side_name+' dark exhaust well',(side*1.65,3.292,-.03),.365,.024,'Graphite')
    ring(side_name+' nozzle rim',(side*1.65,3.31,-.03),.406,.047,'Metal')
    ring(side_name+' energized annulus',(side*1.65,3.33,-.03),.293,.027,'Engine')
    cylinder(side_name+' turbine center',(side*1.65,3.345,-.03),.122,.085,'Metal')
    cylinder(side_name+' turbine core',(side*1.65,3.397,-.03),.059,.02,'Engine')
    for j in range(12):
        a=j*math.tau/12
        x=side*1.65+math.sin(a)*.228;z=-.03+math.cos(a)*.228
        box(side_name+' turbine vane %02d'%j,(x,3.338,z),(.065,.05,.19),'Metal',.01,(0,a+.35,0))
    for j in range(3):ring(side_name+' casing collar %02d'%j,(side*1.65,2.61+j*.23,-.03),.453,.018,'Metal')
    # Inner hydraulic links stay visible through the channel next to the canopy.
    rod(side_name+' hydraulic body',(side*1.06,.2,.06),(side*1.15,2.04,.02),.062,'Metal')
    rod(side_name+' hydraulic rod',(side*1.15,1.88,.02),(side*1.32,2.74,-.02),.034,'Graphite')
    for y in [-.30,.32,.94]:
        cylinder(side_name+' captive fastener',(side*1.98,y,.488 if y<.6 else .435),.034,.018,'Metal',12,(0,0,0))
    box(side_name+' number panel',(side*1.65,-.09,.511),(.74,.69,.010),'Graphite',.045,(-.04,0,0))
    label(side_name+' race number','07',(side*1.65,-.15,.523),.46,'WhiteMark',rotation=(.04,0,math.pi))
    label(side_name+' class designation','AG / CLASS 01',(side*1.65,.37,.494),.073,rotation=(.04,0,math.pi))
    label(side_name+' hull wordmark','KESTREL',(side*1.65,-1.03,.543),.105,rotation=(0,0,math.pi))

# Recessed smoked canopy. A defined rim keeps it from reading as another armor block.
loft('Canopy surround',[(-1.51,0,.38,.29,.07),(-.88,0,.40,.58,.10),(.12,0,.44,.61,.12),(1.16,0,.39,.47,.09),(1.79,0,.29,.20,.06)],'Metal',.035)
glazing()
loft('Rear avionics spine',[(1.64,0,.22,.24,.12),(2.25,0,.18,.31,.16),(2.91,0,.07,.17,.10)],'Ivory')
box('Avionics service panel',(0,2.10,.351),(.32,.31,.02),'Graphite',.018)
for x in [-.09,0,.09]:box('Avionics heat vent',(x,2.10,.366),(.025,.23,.015),'Metal',.004)
slab('Nose delta insignia',[(-.16,-2.77),(.16,-2.77),(0,-3.08)],.307,.006,'Signal',.004)
for side in [-1,1]:
    box('Forward position lamp',(side*1.39,-3.13,.365),(.034,.31,.018),'Engine',.008)
    box('Flank illumination',(side*2.23,.34,.025),(.019,.78,.027),'Engine',.006)

# Apply all modifiers before export so Blender and Unity use identical geometry.
bpy.ops.object.select_all(action='DESELECT')
for o in list(craft.objects):
    if o.type!='MESH':continue
    bm=bmesh.new();bm.from_mesh(o.data)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
    bm.to_mesh(o.data);bm.free();o.data.update()
    bpy.context.view_layer.objects.active=o;o.select_set(True)
    for mod in list(o.modifiers):
        try:bpy.ops.object.modifier_apply(modifier=mod.name)
        except Exception as exc:print('Modifier warning:',o.name,exc)
    o.select_set(False)

# Editable source is intentionally individual meaningful objects, with names and materials.
scene=bpy.context.scene
scene.render.engine='CYCLES';scene.cycles.samples=32
scene.cycles.use_denoising=True
scene.render.resolution_x=1600;scene.render.resolution_y=1200;scene.render.resolution_percentage=100
scene.world.color=(.09,.09,.09)
world=scene.world;world.use_nodes=True
world.node_tree.nodes.get('Background').inputs[0].default_value=(.17,.20,.23,1)
world.node_tree.nodes.get('Background').inputs[1].default_value=.45
scene.view_settings.view_transform='AgX'
floor=material('STUDIO_floor',(.025,.035,.044),.12,.43)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.66));bpy.context.object.name='STUDIO ground';bpy.context.object.data.materials.append(floor)

def area(name,loc,power,color,size,target=(0,0,0)):
    bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.name=name
    o.data.energy=power;o.data.color=color;o.data.shape='DISK';o.data.size=size
    o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
area('STUDIO key',(-3,-3,8),1700,(.82,.92,1),7)
area('STUDIO edge',(5,4,5),2200,(.76,.95,1),5)
area('STUDIO warm bounce',(-5,4,2),1100,(1,.84,.56),4)
area('STUDIO frontal soft',(0,-7,3),700,(1,1,1),5)
bpy.ops.object.camera_add();cam=bpy.context.object;cam.name='STUDIO camera';scene.camera=cam
cam.data.type='ORTHO';cam.data.ortho_scale=10.5;cam.data.lens=55

# Runtime export is batched by material; source objects remain independently editable.
bpy.ops.object.select_all(action='DESELECT')
export_meshes=[]
for mat_name in M:
    copies=[]
    for source in list(craft.objects):
        if source.type=='MESH' and source.data.materials[0].name==mat_name:
            copy=source.copy();copy.data=source.data.copy();scene.collection.objects.link(copy)
            copies.append(copy)
    if not copies:continue
    bpy.ops.object.select_all(action='DESELECT')
    for copy in copies:copy.select_set(True)
    bpy.context.view_layer.objects.active=copies[0]
    bpy.ops.object.join()
    bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    combined=bpy.context.object;combined.name='Kestrel07_'+mat_name;export_meshes.append(combined)
bpy.ops.object.select_all(action='DESELECT')
for o in export_meshes:o.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'HeroShip.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False,path_mode='AUTO',use_mesh_modifiers=True)
export_mesh_count=len(export_meshes)
for o in export_meshes:bpy.data.objects.remove(o,do_unlink=True)
vertices=sum(len(o.data.vertices) for o in craft.objects if o.type=='MESH')
triangles=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in craft.objects if o.type=='MESH')
points=[o.matrix_world@Vector(c) for o in craft.objects for c in o.bound_box]
stats={'editable_source_objects':len(craft.objects),'exported_meshes':export_mesh_count,'vertices':vertices,'triangles':triangles,'source_dimensions_xyz':[round(max(p[i] for p in points)-min(p[i] for p in points),4) for i in range(3)],'source_forward':'-Y','source_up':'+Z','fbx_axis_forward':'-Z','fbx_axis_up':'+Y','materials':list(M.keys())}
(ROOT/'SourceAssets/asset-stats.json').write_text(json.dumps(stats,indent=2))
for filename,position,target in [('hero-front-three-quarter.png',(9,-11,8),(0,-.3,.1)),('hero-rear-three-quarter.png',(-8,10,6),(0,.0,.12)),('hero-top.png',(0,-.001,15),(0,-.3,0))]:
    cam.location=position;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath=str(RENDER/filename)
    if 'top' in filename:cam.data.ortho_scale=11.6
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'SourceAssets/Kestrel07.blend'))
    bpy.ops.render.render(write_still=True)
print('VECTOR_RUSH_ASSET_STATS',json.dumps(stats))
