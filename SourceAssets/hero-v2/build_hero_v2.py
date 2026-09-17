"""Vector Rush / KESTREL 07. Original authored racing craft.
Run: Blender --background --python SourceAssets/build_assets.py
Blender source coordinates: nose -Y, up +Z. FBX uses -Z forward / Y up.
The axis conversion maps source -Y to Unity +Z (see README).
"""
import bpy, bmesh, math, json, sys
from mathutils import Vector
from pathlib import Path

ROOT = Path(__file__).resolve().parent
OUT = ROOT
RENDER = ROOT / 'renders'
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
 'Ivory': material('Ivory',(.8,.84,.81),.18,.40),
 'Graphite': material('Graphite',(.018,.025,.03),.35,.46),
 'Glass': material('Glass',(.009,.042,.053),.79,.14),
 'Signal': material('Signal',(.34,.47,.001),.0,.58),
 'Engine': material('Engine',(.015,.88,.81),.3,.19,5),
 'Metal': material('Metal',(.19,.25,.27),.85,.28),
 'Ceramic': material('Ceramic',(.036,.047,.052),.22,.49),
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

# KESTREL 07 / SCULPTED V2. All painted details are geometry; no external textures.
def curved(name, sections, mat, side=1, arc=32):
    """Longitudinal Catmull-Rom surface with a rounded, asymmetrical hull section."""
    samples=[]
    for i in range(len(sections)-1):
        p0=Vector(sections[max(0,i-1)]);p1=Vector(sections[i]);p2=Vector(sections[i+1]);p3=Vector(sections[min(len(sections)-1,i+2)])
        for k in range(6):
            t=k/6
            samples.append(.5*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t))
    samples.append(Vector(sections[-1]))
    manufactured=mat=='Ivory'
    profile=[(0,1),(.65,1),(.88,.84),(1,.40),(.97,-.48),(.68,-.88),(0,-1),(-.68,-.88),(-.97,-.48),(-1,.40),(-.88,.84),(-.65,1)]
    if manufactured:arc=len(profile)
    vs=[]
    for y,x,z,w,h in samples:
        for j in range(arc):
            a=j*math.tau/arc
            # Soft shoulder above, drawn-in lower chine below.
            cx,sz=profile[j] if manufactured else (math.cos(a),math.sin(a))
            vs.append((side*(x+max(.008,w)*cx*(1 if sz>0 else .90)),y,z+max(.008,h)*sz))
    fs=[tuple(reversed(range(arc)))]
    for i in range(len(samples)-1):
        for j in range(arc):fs.append((i*arc+j,i*arc+(j+1)%arc,(i+1)*arc+(j+1)%arc,(i+1)*arc+j))
    fs.append(tuple((len(samples)-1)*arc+j for j in range(arc)))
    me=bpy.data.meshes.new(name);me.from_pydata(vs,[],fs);me.update()
    o=bpy.data.objects.new(name,me);craft.objects.link(o);o.data.materials.append(M[mat])
    for p in me.polygons:p.use_smooth=True
    me.polygons[0].use_smooth=False;me.polygons[-1].use_smooth=False
    if manufactured:
        for e in me.edges:
            a,b=e.vertices
            if a%arc==b%arc and a%arc in [1,3,5,7,9,11]:e.use_edge_sharp=True
    return o

def tube_profile(name,x,z,profiles,mat,segments=64):
    # Revolved open exhaust wall. Profiles traverse exterior lip and interior throat.
    vs=[(x+math.cos(j*math.tau/segments)*r,y,z+math.sin(j*math.tau/segments)*r) for y,r in profiles for j in range(segments)]
    fs=[]
    for i in range(len(profiles)-1):
        for j in range(segments):fs.append((i*segments+j,i*segments+(j+1)%segments,(i+1)*segments+(j+1)%segments,(i+1)*segments+j))
    me=bpy.data.meshes.new(name);me.from_pydata(vs,[],fs);me.update();o=bpy.data.objects.new(name,me);craft.objects.link(o);o.data.materials.append(M[mat])
    for p in me.polygons:p.use_smooth=True
    return o

def cowl(name,x,z,profiles,start,end,mat):
    n=13;vs=[]
    for offset in [0,-.032]:
        for y,r in profiles:
            for j in range(n):
                a=math.radians(start+(end-start)*j/(n-1));vs.append((x+math.cos(a)*(r+offset),y,z+math.sin(a)*(r+offset)))
    count=len(profiles)*n;fs=[]
    for layer in [0,1]:
        for i in range(len(profiles)-1):
            for j in range(n-1):
                a=layer*count+i*n+j;fs.append((a,a+1,a+n+1,a+n))
    for i in range(len(profiles)-1):
        for j in [0,n-1]:
            a=i*n+j;fs.append((a,a+n,a+n+count,a+count))
    for i in [0,len(profiles)-1]:
        for j in range(n-1):
            a=i*n+j;fs.append((a,a+count,a+1+count,a+1))
    me=bpy.data.meshes.new(name);me.from_pydata(vs,[],fs);me.update();o=bpy.data.objects.new(name,me);craft.objects.link(o);o.data.materials.append(M[mat])
    for p in me.polygons:p.use_smooth=True
    return o

def pathrod(name,pts,r,mat):
    cu=bpy.data.curves.new(name,'CURVE');cu.dimensions='3D';cu.resolution_u=16;cu.bevel_depth=r;cu.bevel_resolution=3
    sp=cu.splines.new('BEZIER');sp.bezier_points.add(len(pts)-1)
    for p,co in zip(sp.bezier_points,pts):p.co=co;p.handle_left_type='AUTO';p.handle_right_type='AUTO'
    o=bpy.data.objects.new(name,cu);craft.objects.link(o);o.data.materials.append(M[mat]);bpy.context.view_layer.objects.active=o;o.select_set(True);bpy.ops.object.convert(target='MESH');o.select_set(False);return o

# An integrated central monocoque, lower and tighter than the shoulder armor.
curved('Central carbon keel',[(-3.80,0,.07,.035,.045),(-3.30,0,.12,.20,.13),(-2.25,0,.14,.52,.29),(-.9,0,.20,.58,.32),(.65,0,.12,.57,.30),(1.8,0,.04,.34,.24),(2.30,0,-.03,.18,.15)],'Graphite')
curved('Ceramic nose crown',[(-3.78,0,.15,.022,.024),(-3.2,0,.27,.21,.095),(-2.4,0,.36,.40,.17),(-1.72,0,.39,.49,.16)],'Ivory')
# Cockpit is a high, visibly curved teardrop supported by an exposed perimeter frame.
curved('Cockpit lower frame',[(-1.98,0,.38,.03,.02),(-1.4,0,.58,.41,.16),(-.55,0,.67,.54,.25),(.4,0,.58,.54,.27),(1.2,0,.35,.38,.22),(1.58,0,.22,.02,.025)],'Metal')
curved('Cockpit smoked canopy',[(-1.91,0,.46,.02,.02),(-1.36,0,.65,.39,.26),(-.55,0,.73,.50,.32),(.30,0,.69,.49,.32),(1.07,0,.42,.30,.23),(1.40,0,.27,.025,.02)],'Glass')
for s in [-1,1]:
    pathrod('Canopy arch edge',[(s*.03,-1.90,.48),(s*.40,-1.32,.70),(s*.505,-.45,.80),(s*.47,.45,.68),(s*.25,1.13,.43),(s*.025,1.40,.29)],.017,'Metal')
pathrod('Canopy transverse rollover frame',[(-.495,.20,.70),(-.36,.20,.96),(0,.20,1.035),(.36,.20,.96),(.495,.20,.70)],.038,'Graphite')
curved('Rear dorsal cowl',[(1.34,0,.35,.10,.085),(1.73,0,.41,.34,.20),(2.24,0,.29,.30,.20),(2.66,0,.13,.16,.10)],'Ivory')
for s in [-1,1]:
    # Large spars: a structural source for the floating shoulder masses.
    for y,z in [(-.85,-.07),(1.40,-.12)]:
        rod('Suspension structural link',(s*.48,y,z),(s*1.80,y+.15,z+.02),.125,'Graphite')
        rod('Suspension titanium ram',(s*.80,y-.12,z+.14),(s*1.46,y+.10,z+.20),.053,'Metal')
        cylinder('Suspension pivot',(s*.91,y,.04),.15,.10,'Metal',32,(0,0,0))
    # Shoulders are fuller centrally, gently twist outward then draw into the nozzle.
    pod_core=curved('Carbon pod core', [(-3.50,1.61,.12,.04,.04),(-2.90,1.57,.11,.34,.20),(-1.60,1.60,.10,.63,.40),(-.1,1.72,.06,.72,.45),(1.35,1.76,.00,.64,.48),(2.35,1.68,-.03,.47,.38)],'Graphite',s)
    # Remove underlying carbon crown from the intake instead of letting it occlude the well.
    bm=bmesh.new();bm.from_mesh(pod_core.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(pod_core.data);bm.free()
    cutter=box('Temporary intake cavity',(s*1.73,.11,.65),(1.08,1.40,.66),'Graphite',0)
    bpy.context.view_layer.objects.active=pod_core
    cut=pod_core.modifiers.new('True intake recess','BOOLEAN');cut.operation='DIFFERENCE';cut.object=cutter;cut.solver='EXACT'
    bpy.ops.object.modifier_apply(modifier=cut.name);bpy.data.objects.remove(cutter,do_unlink=True)
    front_armor=curved('Forward ceramic shoulder',[(-3.54,1.60,.19,.026,.023),(-2.92,1.56,.28,.33,.16),(-2.06,1.59,.42,.51,.25),(-1.19,1.64,.47,.60,.30),(-.60,1.68,.43,.61,.24)],'Ivory',s)
    # Floating exterior flank gives separate reveal below the broad shoulder.
    curved('Outer floating ceramic cheek',[(-2.43,1.95,.18,.095,.07),(-1.45,2.12,.18,.18,.15),(.05,2.29,.03,.18,.17),(1.48,2.26,-.03,.17,.18),(2.51,2.09,-.08,.10,.11)],'Ceramic',s,24)
    curved('Outer forward armor facet',[(-2.40,1.96,.17,.065,.06),(-1.63,2.09,.18,.14,.12),(-.73,2.24,.095,.13,.115)],'Ivory',s)
    curved('Outer rear armor facet',[(.18,2.28,.00,.12,.115),(1.10,2.28,-.025,.13,.13),(2.35,2.12,-.06,.085,.08)],'Ivory',s)
    # Deliberate black intake trough between forward and aft armor, surrounded by volume.
    curved('Intake floor',[(-.53,1.68,.345,.51,.055),(.03,1.73,.345,.52,.055),(.67,1.75,.325,.49,.045)],'Ceramic',s,24)
    for offset in [-.44,.44]:
        curved('Intake raised ceramic rail',[(-.56,1.68+offset,.47,.065,.075),(-.12,1.73+offset,.55,.065,.085),(.50,1.75+offset,.49,.065,.08),(.80,1.75+offset,.43,.055,.06)],'Ivory',s,24)
    # Thin vanes are down in the channel; this is a recess, not a grille pasted on top.
    for i in range(7):
        box('Intake sunken turning vane',(s*1.73,-.32+i*.125,.465),(.65,.022,.062),'Metal',.006,(-.68,0,0))
    rear_armor=curved('Rear shoulder mantle',[(.75,1.76,.42,.55,.22),(1.25,1.76,.49,.60,.29),(1.90,1.72,.35,.56,.24),(2.30,1.68,.27,.49,.19)],'Ivory',s)
    # Bright race sash broad enough to read from the game camera.
    curved('Acid forward livery',[(-3.30,1.59,.288,.040,.013),(-2.93,1.55,.449,.09,.017),(-2.06,1.59,.684,.11,.022),(-1.79,1.60,.747,.11,.014)],'Signal',s,16)
    curved('Acid aft livery',[(1.68,1.74,.704,.13,.014),(1.85,1.72,.64,.13,.021),(2.30,1.69,.477,.12,.018)],'Signal',s,16)
    # Flank slash and distinct lateral light position.
    pathrod('Flank light pipe',[(s*2.10,-1.40,.14),(s*2.38,-.30,.045),(s*2.42,.55,-.035)],.020,'Engine')
    pathrod('Front marker',[(s*1.28,-2.89,.315),(s*1.27,-2.63,.387),(s*1.23,-2.38,.43)],.025,'Engine')
    # Angled uprights, heavier at root, wing sweep clear against the shoulder.
    me=bpy.data.meshes.new('Tail vertical airfoil')
    pts=[(s*2.13,1.62,.15),(s*2.22,2.91,.04),(s*2.58,2.94,1.24),(s*2.47,2.51,1.40),(s*2.17,1.97,.59)]
    vs=pts+[(x+s*.07,y,z) for x,y,z in pts];n=len(pts)
    fs=[tuple(reversed(range(n))),tuple(range(n,n*2))]+[(i,(i+1)%n,(i+1)%n+n,i+n) for i in range(n)]
    me.from_pydata(vs,[],fs);me.update();o=bpy.data.objects.new('Canted rear stabilizer',me);craft.objects.link(o);o.data.materials.append(M['Graphite']);b=o.modifiers.new('Airfoil edge radius','BEVEL');b.width=.045;b.segments=3;o.modifiers.new('Airfoil normals','WEIGHTED_NORMAL');fin=o
    me=bpy.data.meshes.new('Inset flat fin livery');me.from_pydata([(s*2.4,2.45,.90),(s*2.5,2.60,1.20),(s*2.6,2.82,1.16),(s*2.5,2.69,.90)],[],[(0,1,2,3)]);me.update();o=bpy.data.objects.new('Inset fin acid flash',me);craft.objects.link(o);o.data.materials.append(M['Signal'])
    for v in o.data.vertices:
        hit,co,normal,index=fin.ray_cast(Vector((s*4,v.co.y,v.co.z)),Vector((-s,0,0)))
        if hit:v.co.x=co.x+s*.006
    # Visible engine has 0.70m of real recessed depth behind an annular nozzle lip.
    x=s*1.68;z=-.035
    for a,b in [(22,77),(103,158)]:cowl('Formed nozzle armor shingle',x,z,[(2.28,.595),(2.66,.58),(3.18,.50)],a,b,'Ivory')
    tube_profile('Exhaust sculpted shroud',x,z,[(2.22,.43),(2.55,.53),(2.91,.54),(3.32,.46),(3.38,.43),(3.34,.38),(3.10,.35),(2.73,.29),(2.51,.27)],'Ceramic')
    tube_profile('Nozzle titanium rolled lip',x,z,[(3.22,.462),(3.34,.466),(3.405,.432),(3.405,.400),(3.34,.376),(3.26,.368)],'Metal')
    tube_profile('Inner hot throat',x,z,[(2.50,.266),(2.68,.27),(2.78,.30)],'Engine')
    cylinder('Deep dark chamber floor',(x,2.49,z),.262,.035,'Graphite')
    ring('Intermediate energized throat',(x,3.11,z),.330,.027,'Engine')
    ring('Recessed energizer ring',(x,2.72,z),.245,.024,'Engine')
    cylinder('Deep thrust core',(x,2.53,z),.14,.025,'Engine')
    # Six lengthwise staves make inner nozzle structure visible without occluding it.
    for j in range(6):
        a=j*math.tau/6
        rod('Nozzle ceramic stave',(x+math.cos(a)*.286,2.77,z+math.sin(a)*.286),(x+math.cos(a)*.354,3.27,z+math.sin(a)*.354),.018,'Metal')
    for j in range(8):
        a=j*math.tau/8
        pathrod('External nozzle cooling rib',[(x+math.cos(a)*.46,2.33,z+math.sin(a)*.46),(x+math.cos(a)*.55,2.80,z+math.sin(a)*.55),(x+math.cos(a)*.485,3.20,z+math.sin(a)*.485)],.023,'Metal')
    # Exposed propulsion feed lines in inner channels, deliberately visible from chase.
    pathrod('Propellant supply',[ (s*.70,.59,.17),(s*.99,1.22,.10),(s*1.12,2.02,.10),(s*1.33,2.61,.10)],.048,'Metal')
    pathrod('Insulated power conduit',[(s*.67,1.0,.08),(s*.92,1.60,-.015),(s*1.10,2.40,-.035)],.066,'Ceramic')
    for y in [1.3,1.65,2.0]:cylinder('Power bus coupling',(s*.99,y,.03),.084,.062,'Signal',24)
    # Flush dark race insets follow the manufactured armor crown exactly.
    for target,word,y,size,rot in [(front_armor,'07',-1.33,.48,math.pi),(rear_armor,'07',1.22,.34,0)]:
        center=s*(1.62 if y<0 else 1.75);width=.78 if y<0 else .62;length=.62 if y<0 else .44
        vs=[];n=9
        for iy in range(n):
            for ix in range(n):
                px=center+(ix/(n-1)-.5)*width;py=y+(iy/(n-1)-.5)*length
                hit,co,normal,index=target.ray_cast(Vector((px,py,3)),Vector((0,0,-1)))
                vs.append((px,py,co.z+.005 if hit else .5))
        fs=[(iy*n+ix,iy*n+ix+1,(iy+1)*n+ix+1,(iy+1)*n+ix) for iy in range(n-1) for ix in range(n-1)]
        me=bpy.data.meshes.new('Conformal carbon number inset');me.from_pydata(vs,[],fs);me.update();patch=bpy.data.objects.new('Flush carbon number inset',me);craft.objects.link(patch);patch.data.materials.append(M['Ink'])
        num=label('Conformal race number',word,(center,y,2),size,'WhiteMark',rotation=(0,0,rot))
        for v in num.data.vertices:
            world=num.matrix_world @ v.co
            hit,co,normal,index=target.ray_cast(Vector((world.x,world.y,3)),Vector((0,0,-1)))
            if hit:
                world.z=co.z+.011+(world.z-2)
                v.co=num.matrix_world.inverted() @ world
# Center aft heat sink, small central impulse nozzle and undertray vanes.
box('Avionics vent gasket',(0,1.92,.596),(.36,.33,.03),'Graphite',.06,(-.24,0,0))
for j in range(4):box('Avionics vent fin',(0,1.80+j*.073,.628-j*.018),(.27,.022,.036),'Metal',.007)
for s in [-1,1]:
    curved('Rear diffuser rail',[(1.70,.33,-.12,.075,.09),(2.35,.45,-.22,.075,.11),(2.81,.43,-.27,.038,.12)],'Ceramic',s,16)
    rod('Rear diffuser transverse brace',(s*.45,2.74,-.24),(s*1.18,2.62,-.24),.045,'Metal')
tube_profile('Central impulse nozzle',0,-.08,[(2.33,.19),(2.65,.21),(2.76,.18),(2.76,.14),(2.40,.11)],'Metal',48)
cylinder('Central impulse core',(0,2.43,-.08),.105,.03,'Engine')
ring('Central impulse exit annulus',(0,2.67,-.08),.14,.017,'Engine')
label('Nose marque','KESTREL',(0,-2.43,.544),.115,'Ink',rotation=(0,0,math.pi))
slab('Nose signal chevron',[(-.11,-3.10),(0,-3.30),(.11,-3.10),(.07,-3.07),(0,-3.19),(-.07,-3.07)],.374,.008,'Signal',.003)

# Resolve modifiers and consistent outward normals, preserve authored objects in blend.
bpy.ops.object.select_all(action='DESELECT')
for o in list(craft.objects):
    if o.type!='MESH':continue
    bm=bmesh.new();bm.from_mesh(o.data);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(o.data);bm.free()
    bpy.context.view_layer.objects.active=o;o.select_set(True)
    for mod in list(o.modifiers):
        try:bpy.ops.object.modifier_apply(modifier=mod.name)
        except Exception as exc:print('Modifier warning:',o.name,exc)
    o.select_set(False)
scene=bpy.context.scene
scene.render.engine='CYCLES';scene.cycles.samples=24;scene.cycles.device='CPU';scene.cycles.use_denoising=True;scene.cycles.max_bounces=6;scene.cycles.diffuse_bounces=2;scene.cycles.glossy_bounces=3;scene.cycles.caustics_reflective=False;scene.cycles.caustics_refractive=False
scene.render.threads_mode='FIXED';scene.render.threads=4
scene.render.resolution_x=1600;scene.render.resolution_y=1200;scene.render.resolution_percentage=62 if '--preview' in sys.argv else 100
scene.world.use_nodes=True;scene.world.node_tree.nodes.get('Background').inputs[0].default_value=(.13,.16,.21,1);scene.world.node_tree.nodes.get('Background').inputs[1].default_value=.40
scene.view_settings.view_transform='AgX'
floor=material('STUDIO_floor',(.035,.045,.055),.12,.44)
bpy.ops.mesh.primitive_plane_add(size=200,location=(0,0,-.69));bpy.context.object.name='STUDIO ground';bpy.context.object.data.materials.append(floor)
def area(name,loc,power,color,size,target=(0,0,.2)):
    bpy.ops.object.light_add(type='AREA',location=loc);o=bpy.context.object;o.name=name;o.data.energy=power;o.data.color=color;o.data.shape='DISK';o.data.size=size;o.rotation_euler=(Vector(target)-o.location).to_track_quat('-Z','Y').to_euler()
area('STUDIO soft key',(-4,-4,8),1800,(.88,.95,1),6)
area('STUDIO edge',(5,3,6),2400,(.68,.87,1),4)
area('STUDIO warm fill',(-5,4,3),1300,(1,.80,.57),5)
area('STUDIO bow bounce',(0,-7,2),650,(1,1,1),4)
bpy.ops.object.camera_add();cam=bpy.context.object;cam.name='STUDIO camera';scene.camera=cam;cam.data.type='ORTHO';cam.data.ortho_scale=9.6
export_meshes=[]
for mat_name in M:
    copies=[]
    for source in list(craft.objects):
        if source.type=='MESH' and source.data.materials[0].name==mat_name:
            copy=source.copy();copy.data=source.data.copy();scene.collection.objects.link(copy);copies.append(copy)
    if not copies:continue
    bpy.ops.object.select_all(action='DESELECT')
    for copy in copies:copy.select_set(True)
    bpy.context.view_layer.objects.active=copies[0];bpy.ops.object.join();bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
    combined=bpy.context.object;combined.name='Kestrel07_'+mat_name;export_meshes.append(combined)
bpy.ops.object.select_all(action='DESELECT')
for o in export_meshes:o.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/'HeroShip.fbx'),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',global_scale=1,apply_unit_scale=True,bake_space_transform=True,add_leaf_bones=False,path_mode='AUTO',use_mesh_modifiers=True)
export_mesh_count=len(export_meshes)
for o in export_meshes:bpy.data.objects.remove(o,do_unlink=True)
vertices=sum(len(o.data.vertices) for o in craft.objects if o.type=='MESH');triangles=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in craft.objects if o.type=='MESH')
points=[o.matrix_world@Vector(c) for o in craft.objects for c in o.bound_box]
stats={'revision':'KESTREL 07 sculpted V2','editable_source_objects':len(craft.objects),'exported_meshes':export_mesh_count,'vertices':vertices,'triangles':triangles,'source_dimensions_xyz':[round(max(p[i] for p in points)-min(p[i] for p in points),4) for i in range(3)],'source_forward':'-Y','source_up':'+Z','fbx_axis_forward':'-Z','fbx_axis_up':'+Y','materials':list(M.keys()),'textures_required':False}
(ROOT/'asset-stats.json').write_text(json.dumps(stats,indent=2))
for filename,position,target in [('hero-front-three-quarter.png',(9,-11,7),(0,-.10,.1)),('hero-rear-three-quarter.png',(-8,10,6),(0,0,.2)),('hero-chase.png',(0,10,4.4),(0,-.8,.05)),('hero-top.png',(0,-.001,15),(0,-.2,0)),('hero-sun-distance.png',(0,12,5.5),(0,-.8,.1))]:
    if '--preview' in sys.argv and filename not in ['hero-front-three-quarter.png','hero-chase.png']:continue
    cam.location=position;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=10 if 'top' in filename else 9.6
    if 'sun-distance' in filename:
        for light in bpy.data.objects:
            if light.type=='LIGHT':light.hide_render=True
        bpy.ops.object.light_add(type='SUN',location=(-4,-3,10));sun=bpy.context.object;sun.name='STUDIO distance sun';sun.data.energy=2.2;sun.data.angle=.04;sun.rotation_euler=(.45,-.40,-.50);cam.data.ortho_scale=23
        scene.world.node_tree.nodes.get('Background').inputs[1].default_value=.30
    scene.render.filepath=str(RENDER/filename)
    bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'Kestrel07-v2.blend'));bpy.ops.render.render(write_still=True)
print('VECTOR_RUSH_ASSET_STATS',json.dumps(stats))
