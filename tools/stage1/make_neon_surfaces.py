"""Author road roughness and shallow relief; all patterns are fixed in surface space."""
from pathlib import Path
import numpy as np
from PIL import Image, ImageFilter
root=Path(__file__).resolve().parents[2]/'UnityProject/Assets/Art/Stage1/Textures'
rng=np.random.default_rng(6543);w,h=1024,2048
field=Image.fromarray(rng.integers(0,256,(64,32),dtype=np.uint8)).resize((w,h),Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(8))
f=np.asarray(field,dtype=float)/255
fine=rng.normal(0,1,(h,w));y,x=np.mgrid[:h,:w]
ripple=np.asarray(Image.fromarray(rng.integers(0,256,(512,256),dtype=np.uint8)).resize((w,h),Image.Resampling.BICUBIC).filter(ImageFilter.GaussianBlur(2)),dtype=float)/255*.2
height=f*2+ripple+fine*.06
gy,gx=np.gradient(height)
normal=np.stack([-gx*1.4,-gy*1.4,np.ones_like(f)],axis=2);normal/=np.linalg.norm(normal,axis=2)[:,:,None]
Image.fromarray(np.uint8(np.clip(normal*.5+.5,0,1)*255)).save(root/'NeonRoad_Normal.png')
smooth=np.clip(.5+f*.47+ripple*.05,.45,.96)
metal=np.full_like(f,80);rgba=np.stack([metal,metal,metal,smooth*255],axis=2)
Image.fromarray(np.uint8(rgba)).save(root/'NeonRoad_Surface.png')
base=np.clip(64+f*22+fine*2,0,255);rgb=np.stack([base,base+1,base+3],axis=2)
Image.fromarray(np.uint8(rgb)).save(root/'NeonRoad_Base.png')
print('Road surface maps written')
