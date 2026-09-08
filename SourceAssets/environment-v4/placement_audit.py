"""TrackPath parity and non-rendered placement checks for new authored landmarks."""
import math, json, os, bisect
ROOT=os.path.dirname(os.path.abspath(__file__))
K=[(0,32,-230),(135,33,-225),(242,40,-150),(262,51,-20),(180,62,85),(110,53,145),(150,39,225),(70,30,300),(-72,34,286),(-180,48,200),(-240,58,86),(-185,45,-5),(-270,33,-115),(-200,28,-220)]
def add(a,b):return tuple(x+y for x,y in zip(a,b))
def sub(a,b):return tuple(x-y for x,y in zip(a,b))
def mul(a,k):return tuple(x*k for x in a)
def dot(a,b):return sum(x*y for x,y in zip(a,b))
def norm(a):return mul(a,1/math.sqrt(dot(a,a)))
def spline(t):
 u=t%1*len(K);j=int(u);u-=j;a,b,c,d=[K[i%len(K)]for i in [j-1,j,j+1,j+2]]
 return tuple(.5*(2*b[n]+(-a[n]+c[n])*u+(2*a[n]-5*b[n]+4*c[n]-d[n])*u*u+(-a[n]+3*b[n]-3*c[n]+d[n])*u**3)for n in range(3))
P=[spline(i/1200) for i in range(1201)];D=[0.]
for i in range(1,len(P)):D.append(D[-1]+math.dist(P[i],P[i-1]))
L=D[-1]
def frame(p):
 dist=p%1*L;i=min(1199,bisect.bisect_right(D,dist)-1);t=(i+(dist-D[i])/(D[i+1]-D[i]))/1200
 pos=spline(t);f=sub(spline(t+.00001),spline(t-.00001));f=norm((f[0],0,f[2]));r=(f[2],0,-f[0]);return pos,f,r
CLUSTERS=[dict(name='Nocturne_SplitSignal_Mast',progress=.230,side=56.,halfSize=[27.,33.],height=123.),dict(name='Nocturne_ThermalExchange_Works',progress=.625,side=-51.,halfSize=[23.,43.],height=91.)]
S=[frame(i/12000)[0] for i in range(12000)]
for c in CLUSTERS:
 pos,f,r=frame(c['progress']);center=add(pos,mul(r,c['side']));center=(center[0],0.,center[2]);c.update(position=center,yaw=math.degrees(math.atan2(f[0],f[2])),forward=f,right=r)
 checks=[]
 for i,p in enumerate(S):
  v=sub(p,center);x,z=abs(dot(v,r)),abs(dot(v,f));d=math.hypot(max(0,x-c['halfSize'][0]),max(0,z-c['halfSize'][1]));checks.append((d,i/12000))
 c['minimum_centerline_clearance'],c['nearest_progress']=min(checks);c['minimum_road_edge_clearance']=c['minimum_centerline_clearance']-11.
 c['required_runtime_clearance']=18+L/1200
 assert c['minimum_centerline_clearance']>=c['required_runtime_clearance'],c

def qrotate(q,v):
 x,y,z,w=[q[k]for k in ['x','y','z','w']];t=(2*(y*v[2]-z*v[1]),2*(z*v[0]-x*v[2]),2*(x*v[1]-y*v[0]));return add(v,add(mul(t,w),(y*t[2]-z*t[1],z*t[0]-x*t[2],x*t[1]-y*t[0])))
def projection(world,fr):
 q=fr['cameraRotation'].copy()
 for k in ['x','y','z']:q[k]=-q[k]
 v=qrotate(q,sub(world,tuple(fr['cameraPosition'][k]for k in ['x','y','z'])))
 if v[2]<=0:return None
 scale=540/math.tan(math.radians(fr['fieldOfView']/2));return [round(960+v[0]/v[2]*scale,1),round(540-v[1]/v[2]*scale,1),round(v[2],1)]
E=json.load(open(os.path.join(ROOT,'../../evidence/close-race-final/full-lap/environment-evidence.json')))
for c in CLUSTERS:
 c['projections']=[]
 for p in ([.14,.17,.20,.22,.24] if c['progress']<.3 else [.52,.55,.58,.60,.62,.64]):
  fr=min((f for f in E['frames'] if f['lap']==1),key=lambda f:abs(f['progress']-p))
  ctr=c['position'];c['projections'].append(dict(cameraProgress=fr['progress'],frame=fr['index'],base=projection(add(ctr,(0,33,0)),fr),roof=projection(add(ctr,(0,c['height'],0)),fr)))
with open(os.path.join(ROOT,'placement-audit.json'),'w')as f:json.dump(dict(trackLength=L,samples=12000,roadHalfWidth=11,clusters=CLUSTERS,scope='Analytic track/footprint and prior native camera projection only. Native occlusion and lighting need parent validation.'),f,indent=2)
for c in CLUSTERS:print(json.dumps(c,indent=2))
