"""Repack licensed scan maps for URP without baking light or blur."""
from pathlib import Path
from PIL import Image
import numpy as np
ROOT=Path(__file__).resolve().parent
for asset,key,metal,tint in [('concrete_panels','cast_concrete',0,(.82,.87,.89)),('corrugated_iron','corrugated_steel',.75,(.72,.78,.8)),('asphalt_02','asphalt',.02,(.58,.65,.70))]:
 source=ROOT/'scans'/asset
 if not (source/'Diffuse.jpg').exists():continue
 tex=ROOT/'textures';base=np.asarray(Image.open(source/'Diffuse.jpg').convert('RGB'),dtype=float)/255
 base=np.clip(base*np.array(tint),0,1);Image.fromarray((base*255).astype('uint8')).save(tex/(key+'_base.png'))
 for channel,suffix in [('nor_gl','normal'),('AO','ao'),('Displacement','height')]:Image.open(source/(channel+'.jpg')).save(tex/(key+'_'+suffix+'.png'))
 rough=np.asarray(Image.open(source/'Rough.jpg').convert('L'),dtype=float)/255
 for wet in [False,True]:
  sm=1-rough
  if wet:sm=np.clip(.80+sm*.15,0,1)
  ms=np.zeros((*rough.shape,4));ms[:,:,0]=metal;ms[:,:,3]=sm
  Image.fromarray((ms*255).astype('uint8'),'RGBA').save(tex/(key+('_wet' if wet else '')+'_ms.png'))
 print('Packed scan:',key)
