"""Render frozen blend without rebuilding/exporting geometry. Four CPU threads."""
import bpy,sys
from pathlib import Path
from mathutils import Vector
ROOT=Path(__file__).resolve().parent
bpy.ops.wm.open_mainfile(filepath=str(ROOT/'Kestrel07-v2.blend'))
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=24;scene.cycles.use_denoising=True;scene.cycles.max_bounces=6;scene.cycles.diffuse_bounces=2;scene.cycles.glossy_bounces=3
scene.render.threads_mode='FIXED';scene.render.threads=4
scene.render.resolution_x=1600;scene.render.resolution_y=1200;scene.render.resolution_percentage=100
cam=scene.camera
views=[('hero-rear-three-quarter.png',(-8,10,6),(0,0,.2)),('hero-front-three-quarter.png',(9,-11,7),(0,-.1,.1)),('hero-chase.png',(0,10,4.4),(0,-.8,.05)),('hero-top.png',(0,-.001,15),(0,-.2,0)),('hero-sun-distance.png',(0,12,5.5),(0,-.8,.1))]
for filename,position,target in views:
    cam.location=position;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.ortho_scale=10 if 'top' in filename else 9.6
    if 'sun-distance' in filename:
        for light in bpy.data.objects:
            if light.type=='LIGHT':light.hide_render=True
        bpy.ops.object.light_add(type='SUN',location=(-4,-3,10));sun=bpy.context.object;sun.name='STUDIO distance sun';sun.data.energy=2.2;sun.data.angle=.04;sun.rotation_euler=(.45,-.40,-.50);cam.data.ortho_scale=23
        scene.world.node_tree.nodes.get('Background').inputs[1].default_value=.30
    scene.render.filepath=str(ROOT/'renders'/filename)
    bpy.ops.render.render(write_still=True)
