"""Editable architectural koi sculpture. Blender metric FBX, split fin pivots."""
import bpy, math, json
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parent
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
bpy.context.preferences.filepaths.save_version=0
M={}
for name,col,metal,rough,em in [
 ('graphite',(.027,.042,.046),.65,.36,0),('bronze',(.38,.18,.052),.8,.27,0),
 ('ivory',(.88,.79,.58),.18,.3,.15),('red',(.55,.033,.012),.3,.3,.2),
 ('enamel',(.95,.9,.78),.15,.3,.25),('fin',(.58,.27,.064),.55,.3,.28),
 ('amber',(1,.38,.055),.1,.3,3),('window',(.72,.40,.16),.2,.3,.7),
 ('glass',(.012,.028,.03),.55,.19,0),('eye',(.006,.008,.009),.6,.13,0)]:
 m=bpy.data.materials.new(name);m.diffuse_color=(*col,1);m.use_nodes=True;p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*col,1);p.inputs['Metallic'].default_value=metal;p.inputs['Roughness'].default_value=rough;p.inputs['Emission Color'].default_value=(*col,1);p.inputs['Emission Strength'].default_value=em;M[name]=m
tex=ROOT/'scenario/enamel/art.png'
if tex.exists():
 m=M['enamel'];t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=bpy.data.images.load(str(tex));m.node_tree.links.new(t.outputs['Color'],m.node_tree.nodes.get('Principled BSDF').inputs['Base Color'])
parts=[];moving=[]
def assign(ob,name,mat):
 ob.name=name;ob.data.materials.append(M[mat]);parts.append(ob);return ob
def box(name,p,s,mat='graphite',bevel=.05):
 bpy.ops.mesh.primitive_cube_add(size=1,location=p);o=bpy.context.object;o.dimensions=s;bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);assign(o,name,mat)
 if bevel:
  q=o.modifiers.new('Edge radii','BEVEL');q.width=bevel;q.segments=2
  o.modifiers.new('Weighted normals','WEIGHTED_NORMAL')
 return o
def tube(name,pts,r,mat='bronze'):
 c=bpy.data.curves.new(name,'CURVE');c.dimensions='3D';c.resolution_u=2;c.bevel_depth=r;c.bevel_resolution=2;s=c.splines.new('POLY');s.points.add(len(pts)-1)
 for v,p in zip(s.points,pts):v.co=(*p,1)
 o=bpy.data.objects.new(name,c);bpy.context.collection.objects.link(o);assign(o,name,mat);return o
def sphere(name,p,s,mat):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=32,ring_count=20,location=p);o=bpy.context.object;o.scale=s;assign(o,name,mat)
 for f in o.data.polygons:f.use_smooth=True
 return o
# Market tower: reinforced base, vertically articulated facades, occupied floor bands.
box('Foundation',(0,2,-34.5),(23,19,13),'graphite',.3)
box('Service shaft',(0,2,11),(15,12,84),'graphite',.3)
for z in range(-25,55,5):
 box('Floor cornice',(0,2,z),(17,14,.32),'bronze',.06)
 for x in [-6,-3,0,3,6]:
  for y in [-4.08,8.08]:
   box('Recessed window',(x,y,z+2),(2.5,.12,3.4),'window' if (int(z)+int(x))%4 else 'glass',.02)
 for y in [-2,1,4,7]:
  for x in [-7.56,7.56]:box('Side window',(x,y,z+2),(.12,2.4,3.4),'window' if int(z)%3 else 'glass',.02)
for x in [-8,-4,0,4,8]:
 for y in [-5.2,9.2]:box('Copper facade spine',(x,y,12),(.24,.48,86),'bronze')
for z in [42,53]:
 box('Cantilever market canopy',(0,1,z),(22,19,.65),'graphite',.12)
 box('Warm canopy fascia',(0,-8.53,z),(21,.14,.12),'amber',.01)
 for x in [-10,10]:tube('Diagonal canopy bracket',[(x*.7,1,z-6),(x,-7,z)],.22)
# Maintenance balcony with thin guard rail.
box('Lantern deck',(0,0,55),(24,17,1),'graphite',.18)
for x in range(-11,12,2):tube('Balcony upright',[(x,-8,55.6),(x,-8,57)],.045)
for z in [56,57]:tube('Balcony handrail',[(-11.5,-8,z),(11.5,-8,z)],.055)
# Circular structural lantern is open geometry, not an image plane.
CZ=70;R=13
for y,r,th,mat in [(1,13,.45,'graphite'),(0.5,12.8,.20,'bronze'),(-.15,12.5,.38,'amber'),(2.5,13,.35,'graphite')]:
 tube('Lantern ring',[(r*math.cos(a),y,CZ+r*math.sin(a)) for a in [i*math.tau/160 for i in range(161)]],th,mat)
for i in range(48):
 a=i*math.tau/48;rr=13
 tube('Ring standoff',[(rr*math.cos(a),0,CZ+rr*math.sin(a)),(rr*math.cos(a),2.5,CZ+rr*math.sin(a))],.07)
for x in [-10,10]:
 tube('Arch pier',[(x,3,55),(x,3,62),(x*.85,3,78)],.36,'graphite')
 tube('Arch brace',[(x,5,55),(x*.65,2.5,60)],.20)
# Curved koi body: front faces -Y, head upper-left, tail lower-right.
# Continuous ellipse sections along a cubic Bezier spine.
P=[Vector((-7,-2,76)),Vector((-1,-4,78)),Vector((7,-3,72)),Vector((5,-2,63))]
def center(t):return (1-t)**3*P[0]+3*(1-t)**2*t*P[1]+3*(1-t)*t*t*P[2]+t**3*P[3]
def tangent(t):return (3*(1-t)**2*(P[1]-P[0])+6*(1-t)*t*(P[2]-P[1])+3*t*t*(P[3]-P[2])).normalized()
verts=[];uvs=[];faces=[];N=64;K=40
for i in range(N+1):
 t=i/N;c=center(t);along=tangent(t);cross=Vector((-along.z,0,along.x)).normalized();depth=Vector((0,1,0));radius=(.5+2.7*math.sin(math.pi*(t*.91+.08)))*(1-.73*t)
 for j in range(K+1):
  a=j/K*math.tau;v=c+cross*(math.cos(a)*radius)+depth*(math.sin(a)*radius*.72);verts.append(v);uvs.append((t*1.4,j/K))
for i in range(N):
 for j in range(K):
  a=i*(K+1)+j;faces.append((a,a+1,a+K+2,a+K+1))
faces.extend([tuple(reversed(range(K))),tuple(N*(K+1)+j for j in range(K))])
mesh=bpy.data.meshes.new('Sculpted koi body');mesh.from_pydata(verts,[],[tuple(reversed(f)) for f in faces]);mesh.update();o=bpy.data.objects.new('Koi porcelain body',mesh);bpy.context.collection.objects.link(o);assign(o,o.name,'enamel');uv=mesh.uv_layers.new()
for poly in mesh.polygons:
 poly.use_smooth=True
 for li in poly.loop_indices:uv.data[li].uv=uvs[mesh.loops[li].vertex_index]
# Anatomical head, mouth lip, eyes, gills and whiskers.
sphere('Koi forehead',(-7.2,-2.25,76.1),(2.6,1.75,1.95),'ivory')
sphere('Vermilion crown',(-7.15,-3.64,76.78),(1.8,.27,.84),'red')
sphere('Mouth recess',(-9.35,-2.9,75.8),(.35,.70,.57),'eye')
tube('Porcelain mouth lip',[(-9.42,-2.9+.77*math.cos(a),75.8+.62*math.sin(a)) for a in [i*math.tau/40 for i in range(41)]],.14,'ivory')
for y in [-3.86,-.68]:
 sphere('Eye socket',(-7.95,y,76.63),(.47,.22,.47),'bronze');sphere('Glass koi eye',(-8,y-.12 if y<0 else y+.12,76.65),(.31,.18,.31),'eye')
 sphere('Eye glint',(-8.07,y-.29,76.76),(.08,.045,.08),'ivory')
for y,sgn in [(-3.42,-1),(-1.15,1)]:
 tube('Ceramic barbel',[(-9.35,y,75.65),(-10.0,y+sgn*.4,75.15),(-10.45,y+sgn*.5,74.3),(-11.3,y+sgn*.7,74.1)],.09,'ivory')
for off in [0,.35]:tube('Gill plate seam',[(-5.75+off,-3.6,77.4),(-5.2+off,-4.08,76.5),(-5.45+off,-4.13,75.3),(-6.1+off,-3.55,74.7)],.055,'bronze')
# Fins are individual rigid sculptures with ribbed membrane and gentle pivot motion.
def fin(name,pivot,boundary):
 before=set(parts);origin=Vector(pivot);control=[Vector(p) for p in boundary];edge=[]
 for j in range(len(control)-1):
  a,b,c,d=control[max(j-1,0)],control[j],control[j+1],control[min(j+2,len(control)-1)]
  for k in range(10):
   t=k/10;edge.append(.5*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t))
 edge.append(control[-1]);vv=[];ff=[];rows=9;cols=len(edge)
 for row in range(rows+1):
  t=row/rows
  for j,e in enumerate(edge):
   v=origin.lerp(e,t);v.y+=math.sin(t*math.pi)*(.38+.10*math.cos(j*math.pi/2));vv.append(v)
 for row in range(rows):
  for j in range(cols-1):
   v=row*cols+j;ff.append((v,v+1,v+cols+1,v+cols))
 mm=bpy.data.meshes.new(name);mm.from_pydata(vv,[],ff);mm.update();ob=bpy.data.objects.new(name,mm);bpy.context.collection.objects.link(ob);assign(ob,name,'fin')
 for p in mm.polygons:p.use_smooth=True
 solid=ob.modifiers.new('Fin thickness','SOLIDIFY');solid.thickness=.045
 tube(name+' perimeter',edge,.04,'ivory')
 for j in range(0,len(edge),3):
  e=edge[j];points=[]
  for k in range(10):
   t=k/9;v=origin.lerp(e,t);v.y+=math.sin(t*math.pi)*(.38+.10*math.cos(j*math.pi/2))-.04;points.append(v)
  tube(name+' rib',points,.025,'bronze')
 group=[o for o in parts if o not in before];moving.append((name,origin,group))
fin('Fin_near',(-3.5,-4,75),[(-2.8,-4.4,74.8),(-.5,-6.6,72.8),(-.9,-7.2,70.8),(-2.2,-7,70.5),(-4.1,-5.8,72.5),(-4.4,-4.2,74.6)])
fin('Fin_far',(-3.5,-.1,75),[(-3,-.1,75),(-.4,3,74.4),(.8,3.8,72.7),(-.7,3.2,72),(-3.5,.5,73.8)])
fin('Fin_dorsal',(1,-2.5,74.6),[(.8,-2.6,76),(2.1,-2.4,80),(4.8,-2.1,79.2),(6.5,-2.0,75),(6.1,-2,71.5)])
fin('Fin_tail',(5,-2,63),[(4.9,-2,63.4),(1.5,-2.3,60.8),(.2,-2.8,57.5),(3.8,-2.7,58.1),(6.4,-2.2,60.8),(9.8,-1.6,58.7),(12,-1.5,60.2),(9.8,-1.6,63.8),(6,-1.9,64.2)])
# Physical fine cables from the ring support; sculpture body itself stays fixed.
for a,b in [((-7,1,81),(-6,-1,77)),((8,1,79),(4,-1,74)),((10,1,62),(5,-1,64))]:tube('Sculpture suspension cable',[a,b],.045,'bronze')
# Join static geometry per material; retain four exact fin pivots.
animparts={o for _,_,g in moving for o in g}
def join_group(objects,name,origin):
 bpy.ops.object.select_all(action='DESELECT')
 for ob in objects:
  ob.select_set(True);bpy.context.view_layer.objects.active=ob
 bpy.ops.object.convert(target='MESH');bpy.ops.object.join();ob=bpy.context.object;ob.name=name;bpy.context.scene.cursor.location=origin;bpy.ops.object.origin_set(type='ORIGIN_CURSOR');return ob
static_groups={mat:[o for o in parts if o not in animparts and o.data.materials and o.data.materials[0].name==mat] for mat in M}
final=[]
for name,origin,group in moving:final.append(join_group(group,name,origin))
for mat,group in static_groups.items():
 if group:final.append(join_group(group,'Structure_'+mat,(0,0,0)))
# Enlarge the rooftop sculpture for readability through the recorded turn.
for ob in final:
 if ob.name.startswith('Fin_'):
  ob.location.x*=1.2;ob.location.y*=1.2;ob.location.z=55+(ob.location.z-55)*1.2;ob.scale*=1.2
 else:
  for vertex in ob.data.vertices:
   if vertex.co.z>55.5:
    vertex.co.x*=1.2;vertex.co.y*=1.2;vertex.co.z=55+(vertex.co.z-55)*1.2
# Export only model, keeping the source scene editable at material/part level.
bpy.ops.object.select_all(action='DESELECT')
for ob in final:ob.select_set(True)
bpy.ops.file.pack_all()
bpy.ops.wm.save_as_mainfile(filepath=str(ROOT/'KoiLanternTower.blend'))
bpy.ops.export_scene.fbx(filepath=str(ROOT/'KoiLanternTower.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',axis_forward='-Z',axis_up='Y',bake_anim=False,path_mode='AUTO')
tri=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in final)
(ROOT/'manifest.json').write_text(json.dumps({'materials':list(M),'triangles':tri,'objects':len(final),'finPivots':[n for n,_,_ in moving],'units':'metres','front':'Blender -Y / Unity +Z'},indent=2)+'\n')
print('KOI_EXPORTED',tri,'triangles',len(final),'objects')
