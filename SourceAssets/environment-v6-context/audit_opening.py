"""Reproduce rough opening placement and recorded-camera projection without Unity.
Run from any directory; writes placement-audit.json next to this recipe.
No rendered visibility or lighting claim: screen rectangles ignore occlusion.
"""
import bisect,hashlib,json,math,os
ROOT=os.path.dirname(os.path.abspath(__file__))
OUT=ROOT
K=[(0,32,-230),(135,33,-225),(242,40,-150),(262,51,-20),(180,62,85),(110,53,145),(150,39,225),(70,30,300),(-72,34,286),(-180,48,200),(-240,58,86),(-185,45,-5),(-270,33,-115),(-200,28,-220)]
def spline(t):
 u=t%1*len(K);j=int(u);u-=j;a,b,c,d=[K[n%len(K)] for n in (j-1,j,j+1,j+2)]
 return tuple(.5*(2*b[z]+(-a[z]+c[z])*u+(2*a[z]-5*b[z]+4*c[z]-d[z])*u*u+(-a[z]+3*b[z]-3*c[z]+d[z])*u*u*u) for z in range(3))
def norm(v):
 l=math.sqrt(sum(x*x for x in v));return tuple(x/l for x in v)
P=[spline(i/1200) for i in range(1201)];D=[0.]
for a,b in zip(P,P[1:]):D.append(D[-1]+math.dist(a,b))
def frame(p):
 d=p*D[-1];i=min(1199,bisect.bisect_right(D,d)-1);t=(i+(d-D[i])/(D[i+1]-D[i]))/1200
 pos=spline(t);a=spline(t-1e-6);b=spline(t+1e-6);f=norm((b[0]-a[0],0,b[2]-a[2]));r=(f[2],0,-f[0]);return pos,f,r

# Numerical placement/visibility dry run, not a Unity render or occlusion test.
def add(a,b):return tuple(x+y for x,y in zip(a,b))
def sub(a,b):return tuple(x-y for x,y in zip(a,b))
def mul(a,s):return tuple(x*s for x in a)
def dot(a,b):return sum(x*y for x,y in zip(a,b))
def cross(a,b):return (a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0])
def fullframe(progress):
 d=progress*D[-1];i=min(1199,bisect.bisect_right(D,d)-1);t=(i+(d-D[i])/(D[i+1]-D[i]))/1200
 f=norm(sub(spline(t+1e-6),spline(t-1e-6)));r=norm(cross((0,1,0),f));u=cross(f,r)
 return spline(t),f,r,u

def banked_right(progress):
 d=progress*D[-1];i=min(1199,bisect.bisect_right(D,d)-1);t=(i+(d-D[i])/(D[i+1]-D[i]))/1200
 p,f,r,u=fullframe(progress);a=sub(p,spline(t-5/1200));b=sub(spline(t+5/1200),p)
 incoming=norm((a[0],0,a[2]));outgoing=norm((b[0],0,b[2]));turn=math.degrees(math.atan2(cross(incoming,outgoing)[1],dot(incoming,outgoing)))
 bank=math.radians(max(-17,min(17,-turn*2.5)));v=add(mul(r,math.cos(bank)),mul(u,math.sin(bank)))
 return norm((v[0],0,v[2]))

def project(p,frame):
 cp=tuple(frame['cameraPosition'][k] for k in 'xyz');v=sub(p,cp)
 q=tuple(frame['cameraRotation'][k] for k in 'xyzw');xyz=mul(q[:3],-1)
 uv=cross(xyz,v);uuv=cross(xyz,uv);v=add(v,add(mul(uv,2*q[3]),mul(uuv,2)))
 if v[2]<=0:return None
 focal=540/math.tan(math.radians(frame['fieldOfView']*.5))
 return [960+v[0]/v[2]*focal,540-v[1]/v[2]*focal]

def reservation_audit(pieces):
 # Conservative source screening of every possible near-city family; native
 # logs remain authoritative for the exact accepted skyline/benchmark lists.
 route=[frame(i/720)[0] for i in range(720)]
 def basis(f):return ((f[2],0,-f[0]),f)
 def horizontal_clear(c,f,half,margin=41):
  r,f=basis(f)
  return min(math.hypot(max(abs(dot(sub(p,c),r))-half[0],0),max(abs(dot(sub(p,c),f))-half[1],0)) for p in route)>=margin
 obstacles=[]
 for x in range(-2,3):
  for z in range(-2,3):
   c=(x*70+12,0,z*70+14);angle=math.radians(((abs(x+z)%2)*(1 if x+z>=0 else -1))*90);f=(math.sin(angle),0,math.cos(angle))
   if horizontal_clear(c,f,(25,26)):obstacles.append(('inner grid',c,f,(22,24)))
 for i in range(5):
  progress=.04+i*.16;p,_,_=frame(progress);f=banked_right(progress);c=list(sub(p,mul(f,115)));c[1]=0
  if horizontal_clear(c,f,(43,32)):obstacles.append(('industrial',c,f,(37,21)))
 for progress in [.4,.67]:
  p,_,_=frame(progress);out=banked_right(progress);c=list(add(p,mul(out,135)));c[1]=0;f=mul(out,-1)
  for _ in range(12):
   if horizontal_clear(c,f,(82,77)):break
   c=add(c,mul(out,15))
  if horizontal_clear(c,f,(82,77)):obstacles.append(('district',c,f,(82,82)))
 # All possible middle-ring envelopes. The farther skyline's minimum radius
 # 421 exceeds every new vertex radius by >140 m, beyond its 54 m bound.
 for i in range(26):
  a=(i+.4)*math.pi*2/26;radius=345+(i%3)*17;c=(math.cos(a)*radius,0,math.sin(a)*radius);f=(-math.cos(a),0,-math.sin(a))
  obstacles.append(('middle ring conservative',c,f,(36,34)))
 for label,progress,offset,half in [('mast',.23,56,(30,36)),('thermal',.625,-51,(26,46))]:
  p,f,r=frame(progress);c=list(add(p,mul(r,offset)));c[1]=0;obstacles.append((label,c,f,half))
 # Transit candidates are all outside the opening footprint. Include even
 # rejected candidates and a broad envelope for their complete service sites.
 for label,progress,offset,half in [('station',.970,33,(36,36)),('opposite workshop',.948,-29,(15,15)),('terrace',.913,-86,(25,25)),('rear terrace',.926,91,(30,30)),('split tower',.970,122,(34,34)),('approach',.735,-65,(28,28)),('buttress',.792,26,(10,10)),('buttress',.826,26,(10,10)),('buttress',.848,26,(10,10))]:
  p,f,r=frame(progress);c=list(add(p,mul(r,offset)));c[1]=0;obstacles.append((label,c,f,half))
 conflicts=[]
 for part in pieces:
  lo=[min(v[k] for v in part['corners']) for k in range(3)];hi=[max(v[k] for v in part['corners']) for k in range(3)];c=tuple((lo[k]+hi[k])*.5 for k in range(3));half=((hi[0]-lo[0])*.5,(hi[2]-lo[2])*.5)
  for label,other,f,extent in obstacles:
   if math.hypot(other[0]-151.253,other[2]+54.532)<3:continue
   r,f=basis(f);delta=sub(c,other);separate=False
   for axis in [(1,0,0),(0,0,1),r,f]:
    reach=half[0]*abs(axis[0])+half[1]*abs(axis[2])+extent[0]*abs(dot(r,axis))+extent[1]*abs(dot(f,axis))
    if abs(dot(delta,axis))>=reach:separate=True;break
   if not separate:conflicts.append({'piece':part['label'],'obstacle':label,'center':other})
 return {'method':'Conservative world-AABB screening of all emitted pieces including lamp boxes against possible inner/middle/district/industrial sites and landmark/transit envelopes; exact accepted runtime OBB reservations still required. Far skyline excluded by radius separation.', 'obstacles_screened':len(obstacles),'conflicts':conflicts}

def group(progress,start,height_scale=1):
 p,f,r,u=fullframe(progress);foot=(p[0],1,p[2]);socket=add(foot,add(mul(r,-3.63),mul(u,.92)))
 forward=norm((socket[0]-start[0],0,socket[2]-start[2]));right=(forward[2],0,-forward[0]);length=math.dist((socket[0],socket[2]),(start[0],start[2]));room=length*(.70 if height_scale>1 else .32);floor=12.75*height_scale
 pieces=[];lamps=[]
 def transform(v):return add(start,add(mul(right,v[0]),add((0,v[1],0),mul(forward,v[2]))))
 def box(label,p,s,axes=None):
  axes=axes or ((1,0,0),(0,1,0),(0,0,1))
  corners=[transform(add(p,add(mul(axes[0],x*s[0]*.5),add(mul(axes[1],y*s[1]*.5),mul(axes[2],z*s[2]*.5))))) for x in [-1,1] for y in [-1,1] for z in [-1,1]]
  pieces.append(dict(label=label,center=p,size=s,corners=corners))
 def lamp(pos,target,intensity,range,angle):
  f=norm(sub(target,pos));r=norm(cross((0,1,0),f));u=cross(f,r)
  box('housing',add(pos,mul(f,-.28)),(.85,.34,.6),(r,u,f));box('lens',pos,(.68,.22,.035),(r,u,f))
  lamps.append(dict(position_xyz=transform(pos),target_xyz=transform(target),intensity=intensity,range=range,cone=angle))
 for after in [False,True]:
  begin=room+12.9 if after else 0;end=length if after else room-12.9
  box('walk',(0,floor-.5,(begin+end)*.5),(7,1,end-begin))
  for side in [-1,1]:box('walk curb',(side*3.36,floor+.28,(begin+end)*.5),(.28,.56,end-begin))
  begin=room+12.6 if after else 0;end=length if after else room-12.6
  box('dark walk top',(0,floor+.025,(begin+end)*.5),(6.5,.05,end-begin))
 for side in [-1,1]:box('longitudinal girder',(side*2.6,floor-1.35,length*.5),(.5,1.65,length))
 box('podium',(0,floor-.5,room),(13.8,1,25.8));box('podium top',(0,floor+.025,room),(13.2,.05,25.2));box('room rear',(-5.45,floor+3.8,room),(.7,7.6,23.2))
 for end in [-11.2,11.2]:
  box('end return',(-1,floor+3.8,room+end),(9.6,7.6,.9));box('room front pier',(3.55,floor+3.8,room+end),(1.1,7.6,1.7))
 box('room recessed wall',(1.9,floor+3.65,room),(.35,7.3,21.4))
 for bay in [-6,6]:
  box('room window cassette',(2.12,floor+4.15,room+bay),(.14,3.8,5.4));box('room occupied pane',(2.22,floor+4.15,room+bay),(.08,3.2,4.8));box('room lower return',(2.5,floor+1.15,room+bay),(.7,2.3,5.5));box('room window hood',(3.1,floor+6.5,room+bay),(2.1,.45,5.6))
 box('room door',(2.18,floor+2.6,room),(.18,5.2,3.2));box('room roof',(-.45,floor+8.05,room),(11.1,.9,24.9));box('room roof return',(4.7,floor+7.55,room),(.6,.7,24.9))
 for end in [-10,10]:
  for side in [-4.7,4.7]:
   box('room support column',(side,(floor-1)*.5,room+end),(1.8,floor-1,2.2));box('room support foot',(side,.4,room+end),(3.3,.8,3.7))
  box('room support cap',(0,floor-1.35,room+end),(12.8,1.3,2.5))
 for along in [length*.18,length*.46,length*.75]:
  if abs(along-room)<13:continue
  box('span column',(0,(floor-1)*.5,along),(2.6,floor-1,3.6));box('span foot',(0,.35,along),(4.2,.7,5.2));box('span bearing',(0,floor-1.35,along),(6.8,1.2,2.8))
 top=floor-.65
 box('existing foot socket',(0,(socket[1]+top)*.5,length),(1.8,top-socket[1],2.2));box('socket bearing',(0,floor-1.3,length-.6),(6.8,1.2,2.4))
 lamp((4.65,floor+6.98,room-7),(1.5,floor+2.8,room+2),240,27,108)
 if height_scale>1:
  box('foot lamp bracket',(3.05,floor-1.15,length-2.3),(1.5,.45,1.3));lamp((3.55,floor-1.5,length-2.3),(0,1.25,length),750,34,80)
 else:
  box('room base lamp bracket',(5.55,floor+2.3,room+10),(2.3,.4,.6));lamp((6.3,floor+2.15,room+10),(4.7,.6,room+10),430,25,82)
 vertices=[c for part in pieces for c in part['corners']]
 bounds=[[min(v[k] for v in vertices) for k in range(3)],[max(v[k] for v in vertices) for k in range(3)]]
 selections=json.load(open(os.path.join(ROOT,'../../evidence/nocturne-v2/ssr-02/off/capture-validation.json')))['selections']
 projections=[]
 for selection in selections[:5]:
  pixels=[project(v,selection['frame']) for v in vertices];pixels=[p for p in pixels if p]
  room_pixels=[project(v,selection['frame']) for part in pieces if (part['label'].startswith('room') and not part['label'].startswith('room support')) or part['label']=='end return' for v in part['corners']];room_pixels=[p for p in room_pixels if p]
  room_bounds=[max(0,min(p[0] for p in room_pixels)),max(0,min(p[1] for p in room_pixels)),min(1920,max(p[0] for p in room_pixels)),min(1080,max(p[1] for p in room_pixels))] if room_pixels else None
  if room_bounds and (room_bounds[2]<=room_bounds[0] or room_bounds[3]<=room_bounds[1]):room_bounds=None
  visible=pixels and [max(0,min(p[0] for p in pixels)),max(0,min(p[1] for p in pixels)),min(1920,max(p[0] for p in pixels)),min(1080,max(p[1] for p in pixels))]
  valid=bool(visible and visible[2]>visible[0] and visible[3]>visible[1])
  projections.append({'progress':selection['threshold'],'room_only_unoccluded_screen_bounds_px':room_bounds,'approx_unoccluded_screen_bounds_px':visible if valid else None,'bounding_rectangle_frame_percent':round((visible[2]-visible[0])*(visible[3]-visible[1])/(1920*1080)*100,2) if valid else 0})
 step=D[-1]/1200;underbody=13.3*math.sin(math.radians(17))+1.8+3+step;minimum=float('inf');nearest=None
 route=[frame(i/1200)[0] for i in range(1200)]
 for part in pieces:
  lo=[min(v[k] for v in part['corners']) for k in range(3)];hi=[max(v[k] for v in part['corners']) for k in range(3)]
  for i,p in enumerate(route):
   dx=max(lo[0]-p[0],0,p[0]-hi[0]);dz=max(lo[2]-p[2],0,p[2]-hi[2])
   if dx*dx+dz*dz<(18+step)**2:
    gap=p[1]-underbody-hi[1]
    if gap<minimum:minimum=gap;nearest=i/1200
 return {'reservation_audit':reservation_audit(pieces),'service_floor_m':floor,'room_roof_m':floor+8.5,'lamps':lamps,'audited_housing_lens_boxes':4,'support_progress':progress,'frontage_xyz':start,'actual_foot_center_xyz':foot,'socket_xyz':socket,'length_m':length,'bounds_min_max_xyz':bounds,'pieces':len(pieces),'added_lights':2,'minimum_below_protected_deck_m':minimum,'limiting_course_progress':nearest,'projections':projections}

if __name__=='__main__':
 p,f,r=frame(.2);r=banked_right(.2);industrial=list(sub(p,mul(r,115)));industrial[1]=0
 east=list(add(industrial,mul(r,20)));east[0]=max(east[0],172);north=(152,0,-37.82)
 report={'status':'Opening02 correction source-computed placement, not native acceptance. Projection is a loose bounding rectangle with no occlusion.', 'source_sha256':{name:hashlib.sha256(open(os.path.join(ROOT,'../../',name),'rb').read()).hexdigest() for name in ['UnityProject/Assets/Scripts/World/NightDistrict.cs','UnityProject/Assets/Scripts/World/TrackPath.cs','evidence/nocturne-v2/ssr-02/off/capture-validation.json']},'route_length_m':D[-1],'existing_industrial_center_xyz':industrial,'groups':[group(.2,tuple(east)),group(.275,north,22/12.75)]}
 with open(os.path.join(ROOT,'placement-audit.json'),'w') as out:json.dump(report,out,indent=2)
 print(json.dumps(report,indent=2))
