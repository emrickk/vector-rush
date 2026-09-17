"""Original metric PBR surfaces and editable sign artwork; no baked lighting or blur."""
from pathlib import Path
import json,math,shutil
import numpy as np
from PIL import Image,ImageDraw,ImageFont,ImageFilter
ROOT=Path(__file__).resolve().parent
TEX=ROOT/'textures';TEX.mkdir(exist_ok=True)
SVG=ROOT/'sign-source';SVG.mkdir(exist_ok=True)
FONT=ROOT/'fonts/Rajdhani-SemiBold.ttf'
BODY=ROOT/'fonts/Barlow-Medium.ttf'
N=1024
v=np.arange(N)/N;x,y=np.meshgrid(v,v)
rng=np.random.default_rng(240917)
def noise(seed,octaves=7):
 # Periodic multiscale value noise: no preferred diagonal or baked light gradient.
 r=np.random.default_rng(seed);out=np.zeros((N,N));weight=0
 for octave in range(octaves):
  f=2**(octave+1);grid=r.random((f,f));u=x*f;v=y*f
  ix=np.floor(u).astype(int)%f;iy=np.floor(v).astype(int)%f
  tx=u-np.floor(u);ty=v-np.floor(v);tx=tx*tx*(3-2*tx);ty=ty*ty*(3-2*ty)
  layer=(grid[iy,ix]*(1-tx)+grid[iy,(ix+1)%f]*tx)*(1-ty)+(grid[(iy+1)%f,ix]*(1-tx)+grid[(iy+1)%f,(ix+1)%f]*tx)*ty
  w=1/(2**(octave*.7));out+=layer*w;weight+=w
 out/=weight;return (out-out.min())/(out.max()-out.min())
n=noise(10);micro=noise(99,9);stain=noise(88,4)
records=[]
def save(name,a):
 a=np.clip(a,0,1);Image.fromarray((a*255+.5).astype('uint8')).save(TEX/(name+'.png'))
def physical(key,color,metal,rough,height,tile=2,wear=None):
 tone=.93+.05*n+.04*micro
 base=np.stack([tone*c for c in color],axis=-1)
 if wear is not None:base=base*(1-wear[...,None]*.35)+wear[...,None]*np.array([.12,.052,.018])
 # Color variation follows the material, not a common lighting-like pattern.
 if key=='oxidized_copper':
  patina=np.clip((stain-.38)*2.4,0,.88)
  base=base*(1-patina[...,None])+patina[...,None]*np.array([.12,.25,.23])
 if key=='ceramic_tiles':
  grout=((x%.125)<.004)|((y%.0625)<.004)
  cells=rng.uniform(.88,1.04,(16,8));variation=cells[(y*16).astype(int)%16,(x*8).astype(int)%8]
  base*=variation[...,None];base[grout]=[.13,.155,.15]
 if key=='brushed_alloy':
  grain=rng.normal(0,.008,(N,1));base=np.clip(base+grain[...,None],0,1)
 h=height
 dx=(np.roll(h,-1,1)-np.roll(h,1,1))*N/tile;dy=(np.roll(h,-1,0)-np.roll(h,1,0))*N/tile
 normal=np.stack([-dx,dy,np.ones_like(dx)],axis=-1);normal/=np.linalg.norm(normal,axis=-1,keepdims=True)
 ao=np.clip(.88+h*2, .65,1)
 save(key+'_base',base);save(key+'_normal',normal*.5+.5);save(key+'_ao',ao);save(key+'_height',np.clip(.5+h*12,0,1))
 for wet in [False,True]:
  k=key+('_wet' if wet else '')
  smooth=np.clip(1-rough+(n-.5)*.16,.05,.94)
  if wet:smooth=np.maximum(smooth,.72+.18*stain)
  ms=np.zeros((N,N,4));ms[:,:,0]=metal;ms[:,:,3]=smooth
  if key=='oxidized_copper':
   ms[:,:,0]*=(1-patina);ms[:,:,3]*=(1-.35*patina)
  if key=='ceramic_tiles':ms[:,:,0][grout]=0;ms[:,:,3][grout]=.22 if not wet else .58
  save(k+'_ms',ms)
  records.append(dict(id=k,base=f'{key}_base.png',normal=f'{key}_normal.png',ao=f'{key}_ao.png',ms=f'{k}_ms.png',emission='',tileMeters=tile,baseTint=[.68,.73,.76] if wet else [1,1,1],metallic=metal,roughness=float(rough),kind='surface'))
seam=((x%.5)<.005)|((y%.5)<.005)
physical('cast_concrete',[.36,.40,.405],0,.83,(n-.5)*.005+(micro-.5)*.001-seam*.004,tile=4)
physical('painted_graphite',[.13,.18,.20],.45,.5,(micro-.5)*.0006+np.sin(x*2*np.pi*8)*.0002,wear=np.clip((stain-.72)*2,0,.18))
physical('corrugated_steel',[.22,.255,.26],.8,.42,np.cos(x*2*np.pi*24)*.005+(micro-.5)*.0003,wear=np.clip((stain-.6)*.5,0,.18))
physical('brushed_alloy',[.40,.43,.44],.92,.3,np.sin(y*2*np.pi*256)*.0002+(micro-.5)*.0002)
physical('oxidized_copper',[.26,.19,.12],.8,.53,(micro-.5)*.0006,wear=np.clip((stain-.4)*.8,0,.4))
physical('ceramic_tiles',[.53,.55,.50],.05,.38,((((x%.125)<.004)|((y%.0625)<.004))*(-.003))+(micro-.5)*.00015)
physical('rubber',[.033,.041,.045],.02,.86,(micro-.5)*.0008)
physical('asphalt',[.10,.125,.135],.05,.82,(micro-.5)*.002+(n-.5)*.0005,tile=4)
# Windows are deliberately unlit base-color plus separate occupancy emission.
for style,cols,rows in [('tenement',6,12),('office',10,16),('service',4,8)]:
 W,H=1024,2048;im=Image.new('RGB',(W,H),(26,36,39));d=ImageDraw.Draw(im);em=Image.new('RGB',(W,H));e=ImageDraw.Draw(em)
 rr=np.random.default_rng(400+cols)
 for j in range(rows):
  for i in range(cols):
   cw,ch=W/cols,H/rows;xx=int(i*cw+cw*.17);yy=int(j*ch+ch*.15);ex=int((i+1)*cw-cw*.17);ey=int((j+1)*ch-ch*.17)
   d.rectangle((xx-5,yy-5,ex+5,ey+5),fill=(48,56,58));d.rectangle((xx,yy,ex,ey),fill=(9,24,29))
   if rr.random()<.42:
    c=[(145,89,39),(88,138,143),(178,151,102)][int(rr.integers(0,3))]
    e.rectangle((xx+3,yy+3,ex-3,ey-3),fill=c)
    # Curtains, blinds and mullions create occupied rooms, not uniform white slots.
    if rr.random()<.55:
     for by in range(yy+8,ey,9):e.line((xx,by,ex,by),fill=tuple(int(v*.48) for v in c),width=3)
    else:e.rectangle((xx,yy,xx+int((ex-xx)*.27),ey),fill=tuple(int(v*.24) for v in c))
   mid=(xx+ex)//2
   d.rectangle((mid-3,yy,mid+3,ey),fill=(51,60,62));e.rectangle((mid-3,yy,mid+3,ey),fill=0)
   if style=='tenement' and (i+j)%4==0:d.rectangle((xx,ey+5,xx+42,ey+18),fill=(75,78,74))
 im.save(TEX/f'facade_{style}_base.png');em.save(TEX/f'facade_{style}_emission.png')
 records.append(dict(id='facade_'+style,base=f'facade_{style}_base.png',normal='',ao='',ms='',emission=f'facade_{style}_emission.png',tileMeters=12,baseTint=[1,1,1],metallic=.28,roughness=.4,emissionStrength=1.4,kind='facade'))
SIGNS=[('market','NIGHT','MARKET','OPEN / 24 HOURS','#f14c36',1536,512),('repair','REPAIR','SERVICE','BAY 07 / ALL NIGHT','#b0e1db',1024,512),('hotel','HOTEL','07','ROOMS / VACANCY','#f04a35',512,1536),('metro','METRO','M 04','EASTBOUND / PLATFORM 2','#d5e4d8',1536,512),('ramen','NOODLE','BAR','HOT FOOD / LATE HOURS','#e8ad62',1024,1024),('tower','NOVA','ELECTRIC','POWER / THE NIGHT','#a1dbda',512,1536)]
for key,a,b,small,color,W,H in SIGNS:
 bg='#10272c';im=Image.new('RGB',(W,H),bg);d=ImageDraw.Draw(im)
 margin=int(min(W,H)*.075);d.rectangle((margin,margin,W-margin,H-margin),outline=color,width=max(3,margin//6))
 def textfit(text,y,maxsize):
  size=maxsize
  while d.textbbox((0,0),text,font=ImageFont.truetype(str(FONT),size))[2]>W-4*margin:size-=2
  f=ImageFont.truetype(str(FONT),size);box=d.textbbox((0,0),text,font=f);d.text(((W-box[2])/2,y),text,font=f,fill=color)
  return size
 if H>W:
  textfit(a,H*.13,100);textfit(b,H*.38,150);textfit(small,H*.79,28)
 else:
  textfit(a,H*.12,110);textfit(b,H*.40,150);textfit(small,H*.79,30)
 im.save(TEX/f'sign_{key}_base.png');im.save(TEX/f'sign_{key}_emission.png')
 # SVG is the editable typographic master; Python records the exact raster layout.
 svg=f'''<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}" viewBox="0 0 {W} {H}"><rect width="100%" height="100%" fill="{bg}"/><rect x="{margin}" y="{margin}" width="{W-2*margin}" height="{H-2*margin}" fill="none" stroke="{color}" stroke-width="6"/><g text-anchor="middle" fill="{color}" font-family="Saira, sans-serif"><text x="{W/2}" y="{H*.28}" font-size="{min(W*.18,H*.22)}">{a}</text><text x="{W/2}" y="{H*.65}" font-size="{min(W*.24,H*.3)}">{b}</text><text x="{W/2}" y="{H*.87}" font-size="{min(W*.045,H*.06)}">{small}</text></g></svg>'''
 (SVG/f'{key}.svg').write_text(svg)
 records.append(dict(id='sign_'+key,base=f'sign_{key}_base.png',normal='',ao='',ms='',emission=f'sign_{key}_emission.png',tileMeters=1,baseTint=[1,1,1],metallic=.1,roughness=.36,emissionStrength=2,kind='sign'))
for name in ['afterhours','koi_market','transit']:
 art=ROOT/'scenario'/name/'art.png'
 if art.exists():
  Image.open(art).convert('RGB').save(TEX/f'ad_{name}_base.png')
  records.append(dict(id='ad_'+name,base=f'ad_{name}_base.png',normal='',ao='',ms='',emission=f'ad_{name}_base.png',tileMeters=1,baseTint=[1,1,1],metallic=.05,roughness=.4,emissionStrength=.9,kind='sign'))
(ROOT/'materials.json').write_text(json.dumps({'materials':records},indent=2))
exec((ROOT/'apply_scans.py').read_text())
exec((ROOT/'build_signs.py').read_text())
# Flat proof sheet with captions; source textures remain separate, full-resolution.
cols=5;cw,ch=340,290;sheet=Image.new('RGB',(cols*cw,math.ceil(len(records)/cols)*ch),'#121c22');d=ImageDraw.Draw(sheet)
for i,r in enumerate(records):
 xx=(i%cols)*cw;yy=(i//cols)*ch;im=Image.open(TEX/r['base']);im.thumbnail((320,240));sheet.paste(im,(xx+10,yy+5));d.text((xx+10,yy+252),r['id'],font=ImageFont.truetype(str(BODY),16),fill='#d7e6e5')
(ROOT/'surface-proof.jpg').parent.mkdir(exist_ok=True);sheet.save(ROOT/'surface-proof.jpg',quality=92)
print('Materials:',len(records),'texture maps:',len(list(TEX.glob('*.png'))))
