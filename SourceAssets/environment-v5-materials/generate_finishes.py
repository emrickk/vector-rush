"""Original deterministic architecture finish data. Python standard library, CPU only.
No photographs, AI images, third-party textures, tangent normal maps or geometry.
"""
import os,math,json,struct,zlib,hashlib,uuid,re
ROOT=os.path.dirname(os.path.abspath(__file__))
PROJECT=os.path.abspath(os.path.join(ROOT,'../..'))
OUT=os.path.join(ROOT,'candidate-01');LIVE=os.path.join(PROJECT,'UnityProject/Assets/Resources/Art/Environment/Finishes')
if os.path.exists(OUT):
 raise RuntimeError('Immutable candidate already exists; choose a new OUT candidate before generation. No files were changed.')
os.makedirs(OUT);os.makedirs(LIVE,exist_ok=True)
N=512;TAU=math.tau
CONFIG={
 'WarmCeramic':dict(tint=(.985,.967,.929),albedo_grain=.009,metal=.035,metal_grain=.007,smooth=.435,smooth_grain=.026,vary=.013,seed=145),
 'CastConcrete':dict(tint=(.961,.964,.952),albedo_grain=.028,metal=.025,metal_grain=.003,smooth=.225,smooth_grain=.060,vary=.020,seed=276),
 'SatinTitanium':dict(tint=(.925,.957,.985),albedo_grain=.013,metal=.665,metal_grain=.035,smooth=.575,smooth_grain=.047,vary=.019,seed=391),
 'ServiceCoating':dict(tint=(.970,.966,.951),albedo_grain=.018,metal=.335,metal_grain=.019,smooth=.350,smooth_grain=.042,vary=.016,seed=477),
}
def png(path,w,h,data):
 def chunk(t,d):return struct.pack('>I',len(d))+t+d+struct.pack('>I',zlib.crc32(t+d)&0xffffffff)
 raw=b''.join(b'\0'+bytes(data[y*w*4:(y+1)*w*4])for y in range(h))
 open(path,'wb').write(b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',w,h,8,6,0,0,0))+chunk(b'IDAT',zlib.compress(raw,9))+chunk(b'IEND',b''))
def q(v):return max(0,min(255,round(v*255)))
def hnoise(x,y,seed):
 h=(x*73856093^y*19349663^seed*83492791)&0xffffffff;h=(h^(h>>13))*1274126177&0xffffffff;return(h&65535)/32767.5-1

def grid_noise(cells,seed):
 grid=[[hnoise(x,y,seed)for x in range(cells)]for y in range(cells)]
 coords=[]
 for i in range(N):
  a=i/(N-1)*cells;k=int(a);f=a-k;coords.append((k%cells,(k+1)%cells,f*f*(3-2*f)))
 result=[]
 for y in range(N):
  ya,yb,fy=coords[y]
  for x in range(N):
   xa,xb,fx=coords[x];a=grid[ya][xa]*(1-fx)+grid[ya][xb]*fx;b=grid[yb][xa]*(1-fx)+grid[yb][xb]*fx;result.append(a*(1-fy)+b*fy)
 return result

def stats(data):
 return {c:dict(min=min(data[i::4])/255,max=max(data[i::4])/255,mean=sum(data[i::4])/255/(N*N))for i,c in enumerate(['R','G','B','A'])}
def edge_error(data):
 return max([abs(data[(y*N)*4+c]-data[(y*N+N-1)*4+c])for y in range(N)for c in range(4)]+[abs(data[x*4+c]-data[((N-1)*N+x)*4+c])for x in range(N)for c in range(4)])

texture_template=open(os.path.join(PROJECT,'UnityProject/Assets/Resources/Art/ShipSurfaces/Ivory_MetallicSmoothness.png.meta')).read()
def meta(path,srgb):
 old=path+'.meta'
 guid=re.search(r'^guid: (\w+)',open(old).read(),re.M).group(1)if os.path.exists(old)else uuid.uuid4().hex
 s=re.sub(r'^guid: \w+','guid: '+guid,texture_template,flags=re.M)
 s=s.replace('sRGBTexture: 0','sRGBTexture: '+str(int(srgb))).replace('wrapU: 1','wrapU: 0').replace('wrapV: 1','wrapV: 0').replace('wrapW: 1','wrapW: 0').replace('maxTextureSize: 2048','maxTextureSize: 512')
 # Preserve small variations and packed alpha exactly for this bounded candidate.
 s=re.sub(r'textureCompression: [0-9]+','textureCompression: 0',s)
 open(old,'w').write(s);return guid
report={};previews={};guids={}
for name,cfg in CONFIG.items():
 micro=grid_noise(86,cfg['seed']);meso=grid_noise(23,cfg['seed']+17)
 albedo=bytearray();physical=bytearray();smoothpreview=bytearray()
 for y in range(N):
  v=y/(N-1);wave=.54*math.sin(TAU*v+.8)+.29*math.sin(TAU*2*v+2.3)+.17*math.sin(TAU*3*v+4.1)
  for x in range(N):
   i=y*N+x;grain=.68*micro[i]+.32*meso[i]
   # Broad modulation is V-only. On cylinder sides UV V remains source Z/4
   # when dominant-axis projection switches between source X and Y.
   brightness=cfg['albedo_grain']*grain+.0035*wave
   albedo.extend([q(t+brightness)for t in cfg['tint']]+[255])
   metallic=cfg['metal']+cfg['metal_grain']*grain
   smoothness=cfg['smooth']+cfg['smooth_grain']*grain+cfg['vary']*wave
   physical.extend((q(metallic),0,0,q(smoothness)))
   smoothpreview.extend((q(smoothness),q(smoothness),q(smoothness),255))
 for suffix,data,srgb in [('BaseColor',albedo,True),('MetallicSmoothness',physical,False)]:
  filename=name+'_'+suffix+'.png';path=os.path.join(OUT,filename);png(path,N,N,data)
  live=os.path.join(LIVE,filename);open(live,'wb').write(open(path,'rb').read());guids[filename]=meta(live,srgb)
  assert edge_error(data)==0,(name,suffix,edge_error(data))
  report[filename]=dict(width=N,height=N,color_space='sRGB'if srgb else'linear',channels='RGBA color multiplier; opaque alpha'if srgb else'R = metallic; G/B = 0 unused; A = smoothness',statistics=stats(data),opposing_edge_max_byte_difference=0,sha256=hashlib.sha256(open(path,'rb').read()).hexdigest(),guid=guids[filename])
 previews[name]=(albedo,smoothpreview)
# Source preview: top half albedo multipliers, bottom half true smoothness alpha.
W=N*4;H=N*2;board=bytearray(W*H*4)
for col,(name,maps)in enumerate(previews.items()):
 for row,data in enumerate(maps):
  for y in range(N):
   dest=((row*N+y)*W+col*N)*4;board[dest:dest+N*4]=data[y*N*4:(y+1)*N*4]
png(os.path.join(OUT,'finish-contact-sheet.png'),W,H,board)
# This serialized material retains the exact variant used by Apply in a build.
mat=open(os.path.join(PROJECT,'UnityProject/Assets/Resources/SurfaceLit.mat')).read().replace('m_Name: SurfaceLit','m_Name: ArchitectureFinishLit').replace('  - _EMISSION\n','  - _EMISSION\n  - _METALLICSPECGLOSSMAP\n',1)
mat=mat.replace('- _Smoothness: 0.5','- _Smoothness: 1').replace('- _Metallic: 0','- _Metallic: 0.035')
mat=mat.replace('_EmissionColor: {r: 1, g: 1, b: 1, a: 1}', '_EmissionColor: {r: 0, g: 0, b: 0, a: 1}')
for prop,fn in [('_BaseMap','WarmCeramic_BaseColor.png'),('_MetallicGlossMap','WarmCeramic_MetallicSmoothness.png')]:
 mat=mat.replace('    - '+prop+':\n        m_Texture: {fileID: 0}','    - '+prop+':\n        m_Texture: {fileID: 2800000, guid: '+guids[fn]+', type: 3}')
path=os.path.join(LIVE,'ArchitectureFinishLit.mat');open(path,'w').write(mat)
if not os.path.exists(path+'.meta'):open(path+'.meta','w').write('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nNativeFormatImporter:\n  externalObjects: {}\n  mainObjectFileID: 2100000\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n')
if not os.path.exists(LIVE+'.meta'):open(LIVE+'.meta','w').write('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n')
with open(os.path.join(OUT,'texture-stats.json'),'w')as f:json.dump(dict(provenance='Original seeded procedural values; authored for this project. No external texture sources.',generator='generate_finishes.py',seed_config=CONFIG,texture_count=8,total_uncompressed_rgba_mib_with_mips=8*N*N*4*4/3/1048576,files=report),f,indent=2)
print('Eight 512-square textures, exact repeat edges, material template and metas published.')
