"""Deterministic 64-frame advected density atlas with a seamless time loop."""
from pathlib import Path
import numpy as np
from PIL import Image
size=128
x,y=np.meshgrid(np.linspace(-1,1,size),np.linspace(-1,1,size))
atlas=np.zeros((size*8,size*8),dtype=np.uint8)
for frame in range(64):
 t=frame/64*2*np.pi
 q=x+.14*np.sin(y*7-t)+.06*np.sin(y*17-t*2)
 envelope=np.exp(-q*q*7-y*y*2.5)*np.clip((.94-np.abs(x))*9,0,1)*np.clip((.94-np.abs(y))*9,0,1)
 turbulence=.57+.24*np.sin(q*13+y*8-t*3)+.12*np.sin(q*27-y*19+t*5)+.07*np.sin(x*41+y*31-t*7)
 density=np.clip(envelope*np.clip(turbulence,0,1)*1.45,0,1)
 row,col=divmod(frame,8);atlas[row*size:(row+1)*size,col*size:(col+1)*size]=(density*255).astype('uint8')
p=Path(__file__).resolve().parents[1]/'UnityProject/Assets/Resources/Exhaust/BlueFlameAtlas.png'
p.parent.mkdir(exist_ok=True);Image.fromarray(atlas).convert('RGB').save(p)
print(p)
