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

def group(progress,start,height_scale=1):
 p,f,r,u=fullframe(progress);foot=(p[0],1,p[2]);socket=add(foot,add(mul(r,-3.63),mul(u,.92)))
 forward=norm((socket[0]-start[0],0,socket[2]-start[2]));right=(forward[2],0,-forward[0]);length=math.dist((socket[0],socket[2]),(start[0],start[2]));room=length*(.70 if height_scale>1 else .32)
 pieces=[]
 def box(label,p,s):
  p=(p[0],p[1]*height_scale,p[2]);s=(s[0],s[1]*height_scale,s[2])
  if label=='existing foot socket':
   top=12.1*height_scale;p=(p[0],(socket[1]+top)*.5,p[2]);s=(s[0],top-socket[1],s[2])
  corners=[add(start,add(mul(right,p[0]+x*s[0]*.5),add((0,p[1]+y*s[1]*.5,0),mul(forward,p[2]+z*s[2]*.5)))) for x in [-1,1] for y in [-1,1] for z in [-1,1]]
  pieces.append(dict(label=label,center=p,size=s,corners=corners))
 box('deck',(0,12.1,length*.5),(11.8,1.3,length));box('fascia',(5.78,11.55,length*.5),(.26,1.15,length))
 box('room floor',(0,.4,room),(14.8,.8,25));box('room rear',(-6.65,6.2,room),(.7,11.6,24))
 for end in [-11.6,11.6]:box('end return',(0,6.2,room+end),(14,11.6,.8))
 box('room roof',(0,12.1,room),(14.8,1.3,25));box('glass',(-6.24,5.7,room),(.12,7,18));box('door',(-6.12,3.8,room+5),(.14,6,4));box('apron',(8.7,.45,room),(3.4,.9,25))
 for along in [length*.18,length*.46,length*.75]:
  if abs(along-room)<13:continue
  box('column',(0,6.025,along),(2.6,12.05,3.6));box('column foot',(0,.35,along),(4.2,.7,5.2));box('bearing',(0,11.45,along),(9.6,1.2,2.8))
 box('existing foot socket',(0,socket[1]+(12.1-socket[1])*.5,length),(1.8,12.1-socket[1],2.2));box('socket bearing',(0,11.45,length-.6),(8.8,1.2,2.4))
 box('roof source mast',(5.1,15.4,room-9),(.36,5.3,.36));box('roof source arm',(6.5,18,room-9),(3.2,.35,.65))
 box('apron source mast',(8.7,5.5,room+10),(.38,10.2,.38));box('apron housing',(8.7,10.7,room+10),(.9,.45,1.2))
 vertices=[c for part in pieces for c in part['corners']]
 bounds=[[min(v[k] for v in vertices) for k in range(3)],[max(v[k] for v in vertices) for k in range(3)]]
 selections=json.load(open(os.path.join(ROOT,'../../evidence/nocturne-v2/ssr-02/off/capture-validation.json')))['selections']
 projections=[]
 for selection in selections[:5]:
  pixels=[project(v,selection['frame']) for v in vertices];pixels=[p for p in pixels if p]
  room_pixels=[project(v,selection['frame']) for part in pieces if part['label'].startswith('room') or part['label']=='end return' for v in part['corners']];room_pixels=[p for p in room_pixels if p]
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
 return {'support_progress':progress,'frontage_xyz':start,'actual_foot_center_xyz':foot,'socket_xyz':socket,'length_m':length,'bounds_min_max_xyz':bounds,'pieces':len(pieces),'added_lights':2,'minimum_below_protected_deck_m':minimum,'limiting_course_progress':nearest,'projections':projections}

if __name__=='__main__':
 p,f,r=frame(.2);r=banked_right(.2);industrial=list(sub(p,mul(r,115)));industrial[1]=0
 east=list(add(industrial,mul(r,20)));east[0]=max(east[0],172);north=(152,0,-37.82)
 report={'status':'Source-computed placement, not native acceptance. Projection is a loose bounding rectangle with no occlusion.', 'source_sha256':{name:hashlib.sha256(open(os.path.join(ROOT,'../../',name),'rb').read()).hexdigest() for name in ['UnityProject/Assets/Scripts/World/NightDistrict.cs','UnityProject/Assets/Scripts/World/TrackPath.cs','evidence/nocturne-v2/ssr-02/off/capture-validation.json']},'route_length_m':D[-1],'existing_industrial_center_xyz':industrial,'groups':[group(.2,tuple(east)),group(.275,north,22/12.75)]}
 with open(os.path.join(ROOT,'placement-audit.json'),'w') as out:json.dump(report,out,indent=2)
 print(json.dumps(report,indent=2))
