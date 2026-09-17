"""Inspect exact baked runtime meshes; invoke Blender with the baked blend loaded."""
import hashlib
import json
import sys
from pathlib import Path
import bpy
from mathutils import Vector

out = Path(sys.argv[sys.argv.index('--') + 1]).resolve()
out.mkdir(parents=True, exist_ok=True)
scene = bpy.context.scene
scene.render.engine = 'CYCLES'
scene.cycles.device = 'CPU'
scene.cycles.samples = 32
scene.cycles.use_denoising = False
scene.render.threads_mode = 'FIXED'
scene.render.threads = 4
scene.render.resolution_x = 1600
scene.render.resolution_y = 1200
scene.render.resolution_percentage = 100
scene.view_layers[0].material_override = None
views = [('01-baked-rear', (-8,10,6), (0,0,.2)),
         ('02-baked-chase', (0,10,4.4), (0,-.8,.05))]
camera = scene.camera
camera.data.type = 'ORTHO'
camera.data.ortho_scale = 9.6
source = Path(bpy.data.filepath)
(out / 'capture-contract.json').write_text(json.dumps({
    'scope': 'Cycles inspection of the exact runtime meshes with baked maps, original fixed studio lights, no bloom. Not native game evidence.',
    'blend': str(source), 'sha256': hashlib.sha256(source.read_bytes()).hexdigest(),
    'resolution': [1600,1200], 'samples': 32, 'denoising': False,
    'projection': 'ORTHO', 'ortho_scale': 9.6, 'views': views,
}, indent=2) + '\n')
for name, position, target in views:
    path = out / (name + '.png')
    if path.exists():
        raise RuntimeError('Refusing to overwrite reviewed image: ' + str(path))
    camera.location = position
    camera.rotation_euler = (Vector(target)-camera.location).to_track_quat('-Z','Y').to_euler()
    scene.render.filepath = str(path)
    bpy.ops.render.render(write_still=True)
    print('BAKED_PREVIEW_READY', str(path), flush=True)
