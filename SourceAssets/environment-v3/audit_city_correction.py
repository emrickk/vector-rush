"""Place the proposed exported envelopes and conservatively audit every track branch."""
import bisect,json,math,os
ROOT=os.path.dirname(os.path.abspath(__file__))
OUT=os.path.join(ROOT,'pass-03-city-correction')
ASSET_BASE=os.path.join(ROOT,'pass-02-transit')
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
with open(os.path.join(ASSET_BASE,'asset-stats.json')) as f:assets=json.load(f)
# 12,000 arc-distance samples; deduct max segment length from distance to make
# the bound conservative between samples. The existing Unity path uses 1,200.
N=12000;route=[frame(i/N)[0] for i in range(N)]
step=max(math.dist(route[i],route[(i+1)%N]) for i in range(N))
def audit(name,progress,offset,datum,reverse=False):
 pos,f,r=frame(progress);origin=[pos[i]+r[i]*offset for i in range(3)];origin[1]+=datum
 if reverse:f=tuple(-v for v in f);r=tuple(-v for v in r)
 a=assets[name];slo=a['source_min_xyz'];shi=a['source_max_xyz'];lo=(slo[0],-shi[1]);hi=(shi[0],-slo[1])
 nearest=(float('inf'),None)
 for i,p in enumerate(route):
  d=[p[k]-origin[k] for k in range(3)];x=sum(d[k]*r[k] for k in range(3));z=sum(d[k]*f[k] for k in range(3))
  dx=max(lo[0]-x,0,x-hi[0]);dz=max(lo[1]-z,0,z-hi[1]);distance=math.hypot(dx,dz)
  if distance<nearest[0]:nearest=(distance,i/N)
 minimum=nearest[0]-step
 return {'asset':name,'anchor_progress':progress,'horizontal_right_offset_m':offset,'floor_below_track_m':-datum,'origin_unity_xyz':origin,'yaw_degrees':math.degrees(math.atan2(f[0],f[2])),'unity_local_axes':{'+X':r,'+Y':(0,1,0),'+Z':f},'uniform_scale':1,'dimensions_xyz_m':a['unity_dimensions_xyz'],'min_centerline_to_oriented_export_bounds_m':minimum,'nearest_track_progress':nearest[1],'at_least_14m_clear':minimum>=14,'at_least_18m_camera_margin':minimum>=18,'footprint_world_xz':[[origin[0]+r[0]*x+f[0]*z,origin[2]+r[2]*x+f[2]*z] for x,z in [(lo[0],lo[1]),(hi[0],lo[1]),(hi[0],hi[1]),(lo[0],hi[1])]],'fbx_sha256':a['fbx_sha256']}
placements=[audit('Nocturne_TransitStation_A',.970,33,-7,True),audit('Nocturne_ServiceWorkshop_A',.948,-29,-3)]
# Buttresses attach to the station's foundation. Their top ledge is 11.135 m,
# so a variable-height concrete pedestal below the complete unscaled module
# reaches the station floor datum. Use station axes, no banking or scale stretch.
station=placements[0];buttresses=[]
for longitudinal in [-21,20]:
 r=station['unity_local_axes']['+X'];f=station['unity_local_axes']['+Z'];o=station['origin_unity_xyz'];base=[o[i]+r[i]*3+f[i]*longitudinal for i in range(3)]
 base[1]=o[1]-11.135
 buttresses.append({'origin_unity_xyz':base,'yaw_degrees':station['yaw_degrees'],'uniform_scale':1,'pedestal_from_ground_to_y_m':base[1],'pedestal_footprint_xz_m':[4.4,7.2],'note':'Keep structural buttresses beneath station. They share its conservative 23 × 64 m footprint and therefore its all-branch lateral clearance.'})
result={'status':'STAGED PROPOSAL — parent composition approval and actual camera validation pending','path_source':'UnityProject/Assets/Scripts/World/TrackPath.cs','path_length_m':D[-1],'clearance_method':'Horizontal distance from 12,000 all-course arc-length samples to oriented full exported bounding rectangles; subtract maximum sample segment length. Vertical separation never used to excuse lateral intrusion. No props colliders.','sample_segment_deduction_m':step,'placements':placements,'station_supports':buttresses,'integration_notes':['Native candidate01 exposed the rear with the previous rotation. Station and workshop each rotate 180 degrees from candidate01; source X assumption was not a verified Unity imported facing.','Do not create continuous extra lamps; use grouped WarmWindow material for the recessed rooms.','Remove or recompose existing nearby generic blocks before adding these assets; no stacked district overlay.','Station datum is elevated; create the specified grounded supports and explicit service level before native review.','Existing Terrace A and Split B remain uniform scale only; final camera-driven coordinates belong in NightDistrict after approval.']}
assert all(p['at_least_14m_clear'] for p in placements)
with open(os.path.join(OUT,'placement-contract.json'),'w') as f:json.dump(result,f,indent=2)
print(json.dumps(result,indent=2))

# Reproduce the complete integration's reserve ordering and the exact runtime
# 720-sample gate, independently of Unity. This is predicted placement evidence;
# the parent still must verify the instantiated geometry in the native build.
def dot(a,b):return sum(x*y for x,y in zip(a,b))
def basis(yaw):
 a=math.radians(yaw);return (math.cos(a),0,-math.sin(a)),(math.sin(a),0,math.cos(a))
def course_candidate(label,progress,offset,datum,half,extra_yaw=0,absolute_y=None):
 p,f,r=frame(progress);c=[p[i]+r[i]*offset for i in range(3)];c[1]=p[1]+datum if absolute_y is None else absolute_y
 return dict(label=label,center=c,yaw=math.degrees(math.atan2(f[0],f[2]))+extra_yaw,half=half)
runtime_candidates=[
 course_candidate('exit transit station',.970,33,-7,(11.6,32),180),
 course_candidate('opposite low service room',.948,-29,-3,(4,9)),
 course_candidate('low left terrace',.913,-86,0,(19*.75,21*.75),90,3.9),
 course_candidate('station rear terrace',.926,91,0,(19,21),-90,3.9),
 course_candidate('split landmark',.970,122,0,(19*1.18,21*1.18),-90,3.9)]
approach=course_candidate('approach terrace over service court',.735,-65,0,(18,20),absolute_y=9)
approach['yaw']=math.degrees(math.atan2(-212.6821-approach['center'][0],41.7193-approach['center'][2]))
runtime_candidates.append(approach)
for p in [.792,.826,.848]:runtime_candidates.append(course_candidate('approach ledge '+str(p),p,26,-13.2,(3.2,4.3)))
bp,bf,br=frame(.918)
for x,z,w,d,h in [(172,305,28,33,126),(211,337,25,29,151),(-226,467,34,31,113)]:
 c=[bp[i]+br[i]*x+bf[i]*z for i in range(3)];c[1]=0
 runtime_candidates.append(dict(label='quiet skyline '+str(h),center=c,yaw=math.degrees(math.atan2(bf[0],bf[2])),half=(w*.6,d*.6)))
runtime_route=[frame(i/720)[0] for i in range(720)];runtime_margin=18+D[-1]/720;accepted=[]
for item in runtime_candidates:
 c=item['center'];a,b=basis(item['yaw']);hx,hz=item['half']
 def distance(p):
  d=tuple(p[i]-c[i] for i in range(3));return math.hypot(max(abs(dot(d,a))-hx,0),max(abs(dot(d,b))-hz,0))
 sample_min=min(distance(p) for p in runtime_route);precise_min=min(distance(p) for p in route)-step
 conflict=None
 for prior in accepted:
  pc=prior['center'];u,v=basis(prior['yaw']);phx,phz=prior['half'];delta=tuple(c[i]-pc[i] for i in range(3));separate=False
  for axis in [a,b,u,v]:
   radius=hx*abs(dot(a,axis))+hz*abs(dot(b,axis))+phx*abs(dot(u,axis))+phz*abs(dot(v,axis))+4
   if abs(dot(delta,axis))>radius:separate=True;break
  if not separate:conflict=prior['label'];break
 item.update(runtime_sample_clearance_m=sample_min,runtime_required_m=runtime_margin,conservative_all_course_clearance_m=precise_min,overlap_with=conflict,predicted_accepted=sample_min>=runtime_margin and not conflict)
 if item['predicted_accepted']:accepted.append(item)
integration={'status':'Numerical dry run of current NightDistrict placements; actual native object presence pending','candidates':runtime_candidates,'all_predicted_accepted':len(accepted)==len(runtime_candidates),'placement_count':len(accepted)}
with open(os.path.join(OUT,'integration-placement-audit.json'),'w') as f:json.dump(integration,f,indent=2)
for item in runtime_candidates:print(item['label'],item['predicted_accepted'],round(item['conservative_all_course_clearance_m'],3),item['overlap_with'])
