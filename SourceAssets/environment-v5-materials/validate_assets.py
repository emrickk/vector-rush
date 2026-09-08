"""Read-only validation of staged/live authored textures and integration metadata."""
import os,struct,zlib,json,hashlib,re,subprocess,sys,math
ROOT=os.path.dirname(os.path.abspath(__file__));PROJECT=os.path.abspath(os.path.join(ROOT,'../..'))
OUT=os.path.join(ROOT,'candidate-01');LIVE=os.path.join(PROJECT,'UnityProject/Assets/Resources/Art/Environment/Finishes')
source=json.load(open(os.path.join(OUT,'texture-stats.json')))
def sha(path):return hashlib.sha256(open(path,'rb').read()).hexdigest()
def read_png(path):
 data=open(path,'rb').read();assert data[:8]==b'\x89PNG\r\n\x1a\n';pos=8;compressed=bytearray();w=h=None
 while pos<len(data):
  n=struct.unpack('>I',data[pos:pos+4])[0];kind=data[pos+4:pos+8];body=data[pos+8:pos+8+n];crc=struct.unpack('>I',data[pos+8+n:pos+12+n])[0];assert zlib.crc32(kind+body)&0xffffffff==crc
  if kind==b'IHDR':
   w,h,depth,color,compression,filtering,interlace=struct.unpack('>IIBBBBB',body);assert (depth,color,compression,filtering,interlace)==(8,6,0,0,0)
  if kind==b'IDAT':compressed.extend(body)
  pos+=12+n
 raw=zlib.decompress(compressed);stride=w*4+1;assert len(raw)==stride*h
 pixels=bytearray()
 for y in range(h):assert raw[y*stride]==0;pixels.extend(raw[y*stride+1:(y+1)*stride])
 return w,h,pixels
report={}
for name,expected in source['files'].items():
 stage=os.path.join(OUT,name);live=os.path.join(LIVE,name);assert sha(stage)==sha(live)==expected['sha256']
 w,h,data=read_png(live);assert(w,h)==(512,512)
 srgb='_BaseColor'in name;meta=open(live+'.meta').read()
 for required in ['sRGBTexture: '+str(int(srgb)),'enableMipMap: 1','filterMode: 2','aniso: 8','wrapU: 0','wrapV: 0','wrapW: 0','maxTextureSize: 512','alphaIsTransparency: 0']:
  assert required in meta,(name,required)
 assert all(c=='0'for c in re.findall(r'textureCompression: (\d+)',meta))
 assert re.search(r'^guid: (\w+)',meta,re.M).group(1)==expected['guid']
 for y in range(h):assert data[y*w*4:y*w*4+4]==data[(y*w+w-1)*4:(y*w+w)*4]
 assert data[:w*4]==data[(h-1)*w*4:]
 stats={}
 for k,label in enumerate(['R','G','B','A']):
  values=data[k::4];stats[label]=dict(min=min(values)/255,max=max(values)/255,mean=sum(values)/len(values)/255,distinct_values=len(set(values)))
  for field in ['min','max','mean']:assert abs(stats[label][field]-expected['statistics'][label][field])<1e-10
 if srgb:assert stats['A']['min']==stats['A']['max']==1
 else:
  assert stats['G']['max']==stats['B']['max']==0
  assert stats['A']['distinct_values']>=12 and .1<stats['A']['min']<stats['A']['max']<.7
  assert stats['R']['distinct_values']>=2
 report[name]=dict(status='PASS',packed_smoothness_alpha=not srgb,color_space='sRGB'if srgb else'linear',mips=True,repeat=True,uncompressed=True,channel_statistics=stats)
mat=open(os.path.join(LIVE,'ArchitectureFinishLit.mat')).read()
keywords=re.search(r'm_ValidKeywords:\n(.*?)  m_InvalidKeywords:',mat,re.S).group(1)
assert set(re.findall(r'- (\S+)',keywords))=={'_EMISSION','_METALLICSPECGLOSSMAP'}
assert '_EmissionColor: {r: 0, g: 0, b: 0, a: 1}'in mat
assert '- _Smoothness: 1\n'in mat and '- _SmoothnessTextureChannel: 0\n'in mat and '- _WorkflowMode: 1\n'in mat
for field,name in [('_BaseMap','WarmCeramic_BaseColor.png'),('_MetallicGlossMap','WarmCeramic_MetallicSmoothness.png')]:
 assert field+':\n        m_Texture: {fileID: 2800000, guid: '+source['files'][name]['guid']+', type: 3}'in mat
# Exercise only the existing-pass guard: it must fail before touching any output.
before={n:sha(os.path.join(LIVE,n))for n in os.listdir(LIVE)if os.path.isfile(os.path.join(LIVE,n))}
p=subprocess.run([sys.executable,os.path.join(ROOT,'generate_finishes.py')],capture_output=True,text=True)
assert p.returncode!=0 and 'Immutable candidate already exists'in p.stderr
assert before=={n:sha(os.path.join(LIVE,n))for n in before}
result=dict(status='PASS_DATA_AND_METADATA_ONLY',textures=report,serialized_keyword_variant_retained=True,template_default_emission_black=True,immutable_guard_leaves_all_live_files_unchanged=True,provenance='Original procedural CPU authoring',native_compile_or_visual_validation=False)
with open(os.path.join(OUT,'validation.json'),'w')as f:json.dump(result,f,indent=2)
for n,r in report.items():
 if r['packed_smoothness_alpha']:
  print(n,'metal mean',round(r['channel_statistics']['R']['mean'],4),'smooth min/mean/max',*[round(r['channel_statistics']['A'][k],4)for k in ['min','mean','max']])
print('PASS: PNG integrity/hashes, packed channel variation, repeat edges, color space, mips, template references/keywords, black emission and immutable guard. No native pass claimed.')
