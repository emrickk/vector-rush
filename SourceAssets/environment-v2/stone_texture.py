"""Portable periodic stone textures: run inside Blender, requires only bundled NumPy."""
import bpy, numpy as np, os
N=1024
rng=np.random.default_rng(2904)
freq=np.fft.fftfreq(N)
x,y=np.meshgrid(freq,freq)
k=np.sqrt(x*x+y*y);k[0,0]=1
noise=np.fft.ifft2(np.fft.fft2(rng.standard_normal((N,N)))*(k**-1.15)).real
noise=(noise-noise.mean())/noise.std()
broad=np.fft.ifft2(np.fft.fft2(rng.standard_normal((N,N)))*(k**-1.9)).real
broad=(broad-broad.mean())/broad.std()
micro=rng.standard_normal((N,N))
streak_k=np.sqrt((x*.65)**2+(y*2.8)**2);streak_k[0,0]=1
streak=np.fft.ifft2(np.fft.fft2(rng.standard_normal((N,N)))*(streak_k**-1.25)).real
streak=(streak-streak.mean())/streak.std()
value=np.clip(.67+.035*noise+.035*broad+.018*streak+.012*micro,.43,.87)
color=np.empty((N,N,4),dtype=np.float32)
color[:,:,0]=value;color[:,:,1]=value*.987;color[:,:,2]=value*.912;color[:,:,3]=1
# Warm gray limestone, mineral tint carried by a portable albedo image.
im=bpy.data.images.new('Solstice_Limestone_Albedo',N,N,alpha=False)
im.pixels.foreach_set(color.ravel());im.filepath_raw=os.path.join(ROOT,'textures','Solstice_Limestone_Albedo.png');im.file_format='PNG';im.save()
# A restrained tangent normal carries grain, while the eroded shape remains mesh geometry.
gy,gx=np.gradient(noise+.18*micro)
vec=np.stack((-gx*.22,-gy*.22,np.ones_like(gx)),axis=-1);vec/=np.linalg.norm(vec,axis=-1,keepdims=True)
norm=np.ones((N,N,4),dtype=np.float32);norm[:,:,:3]=vec*.5+.5
nm=bpy.data.images.new('Solstice_Limestone_Normal',N,N,alpha=False);nm.colorspace_settings.name='Non-Color';nm.pixels.foreach_set(norm.ravel());nm.filepath_raw=os.path.join(ROOT,'textures','Solstice_Limestone_Normal.png');nm.file_format='PNG';nm.save()
mat=bpy.data.materials['Rock'];nodes=mat.node_tree.nodes;links=mat.node_tree.links;p=nodes.get('Principled BSDF')
a=nodes.new('ShaderNodeTexImage');a.image=im;a.label='Portable stone base color';links.new(a.outputs['Color'],p.inputs['Base Color'])
n=nodes.new('ShaderNodeTexImage');n.image=nm;n.label='Portable mineral grain normal';normal=nodes.new('ShaderNodeNormalMap');normal.inputs['Strength'].default_value=.55;links.new(n.outputs['Color'],normal.inputs['Color']);links.new(normal.outputs['Normal'],p.inputs['Normal'])

rough=np.ones((N,N,4),dtype=np.float32);rough[:,:,:3]=np.clip(.85+.028*noise,.74,.95)[:,:,None]
rm=bpy.data.images.new('Solstice_Limestone_Roughness',N,N,alpha=False);rm.colorspace_settings.name='Non-Color';rm.pixels.foreach_set(rough.ravel());rm.filepath_raw=os.path.join(ROOT,'textures','Solstice_Limestone_Roughness.png');rm.file_format='PNG';rm.save()
rn=nodes.new('ShaderNodeTexImage');rn.image=rm;links.new(rn.outputs['Color'],p.inputs['Roughness'])

packed=np.zeros((N,N,4),dtype=np.float32);packed[:,:,3]=1-rough[:,:,0]
pm=bpy.data.images.new('Solstice_Limestone_MetallicSmoothness',N,N,alpha=True);pm.colorspace_settings.name='Non-Color';pm.pixels.foreach_set(packed.ravel());pm.filepath_raw=os.path.join(ROOT,'textures','Solstice_Limestone_MetallicSmoothness.png');pm.file_format='PNG';pm.save()
