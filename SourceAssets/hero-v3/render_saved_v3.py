"""Resume evidence from immutable saved V3 blend; geometry never changed or saved."""
import bpy,json,sys
from pathlib import Path
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:]
evidence=Path(args[0]);contract=json.loads((evidence/'capture-contract.json').read_text())
scene=bpy.context.scene;scene.render.threads_mode='FIXED';scene.render.threads=4
scene.cycles.use_denoising=False;scene.cycles.samples=32
contract['render_resume']={'reason':'CPU OIDN denoiser delayed output; same saved geometry and lighting resumed without denoising','denoising':False,'samples':32,'threads':4,'existing_01_samples':48 if contract.get('revision')=='pass-02' else 32,'priority':'side, top, chase first; existing images preserved'}
(evidence/'capture-contract.json').write_text(json.dumps(contract,indent=2))
cam=scene.camera;clay=bpy.data.materials.get('STUDIO medium gray clay')
if clay is None:
 clay=bpy.data.materials.new('STUDIO medium gray clay');clay.diffuse_color=(.31,.33,.34,1);clay.use_nodes=True
 n=clay.node_tree.nodes.get('Principled BSDF');n.inputs['Base Color'].default_value=(.31,.33,.34,1);n.inputs['Metallic'].default_value=0;n.inputs['Roughness'].default_value=.53
ordered=sorted(contract['views'],key=lambda v: {'08':0,'07':1,'05':2,'06':3,'02':4,'03':5,'04':6,'09':7,'10':8,'01':9}.get(v[0][:2],99)) if contract.get('revision')=='pass-02' else contract['views']
rendered_count=0;limit=int(args[1]) if len(args)>1 else None
for name,pos,target,scale,use_clay,projection in ordered:
 output=evidence/(name+'.png')
 if output.exists():continue
 scene.render.resolution_x=1920 if name.startswith('10-') else 1600;scene.render.resolution_y=1080 if name.startswith('10-') else 1200
 cam.location=pos;cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type=projection;cam.data.ortho_scale=scale;cam.data.lens=45
 scene.view_layers[0].material_override=clay if use_clay else None
 scene.render.filepath=str(output);bpy.ops.render.render(write_still=True)
 print('SHIP_V3_RENDER_READY',name,flush=True)
 rendered_count+=1
 if limit and rendered_count>=limit:break
