import bpy, math, os, json, random, ast
from mathutils import Vector
from math import sin,cos,pi
ROOT=os.path.dirname(os.path.abspath(__file__))
bpy.ops.wm.open_mainfile(filepath=os.path.join(ROOT,'Solstice_Environment_Kit.blend'))
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=12;scene.cycles.use_denoising=True;scene.render.threads_mode='FIXED';scene.render.threads=4
MATS={name:bpy.data.materials[name] for name in ['Ivory','Graphite','Glass','Metal','Rock','Vegetation']};parts=[]
source=open(os.path.join(ROOT,'build_environment.py')).read();module=ast.parse(source)
functions=[n for n in module.body if isinstance(n,(ast.FunctionDef,ast.AsyncFunctionDef))]
exec(compile(ast.Module(body=functions,type_ignores=[]),'build_environment.py','exec'))
old=bpy.data.objects.get('Solstice_CoastalCliff_C');bpy.data.objects.remove(old,do_unlink=True)
start=source.index('# Revised geology:');end=source.index('\nassets=[A,B,C]',start)
exec(compile(source[start:end],'build_environment.py','exec'))
bpy.ops.object.select_all(action='DESELECT');C.select_set(True);bpy.context.view_layer.objects.active=C
bpy.ops.export_scene.fbx(filepath=os.path.join(ROOT,'exports',C.name+'.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_UNITS',axis_forward='-Z',axis_up='Y',bake_space_transform=True,use_mesh_modifiers=True,mesh_smooth_type='FACE',add_leaf_bones=False,path_mode='COPY',embed_textures=True)
C.data.calc_loop_triangles();stats=json.load(open(os.path.join(ROOT,'asset_stats.json')));stats[C.name].update(vertices=len(C.data.vertices),triangles=len(C.data.loop_triangles),materials=[m.name for m in C.data.materials],dimensions_metres_blender_xyz=list(C.dimensions),textures=['Solstice_Limestone_Albedo.png','Solstice_Limestone_Normal.png','Solstice_Limestone_Roughness.png','Solstice_Limestone_MetallicSmoothness.png'])
json.dump(stats,open(os.path.join(ROOT,'asset_stats.json'),'w'),indent=2)
C.location=(64,30,0)
cam=scene.camera
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Solstice_Environment_Kit.blend'))
render('08-cliff-fracture-final',(128,-83,63),(65,25,17),55,(1280,960))
render('11-tower-A-medium',(-91,-85,48),(-31,5,36),57,(1000,1280))
render('09-kit-final',(-138,-190,118),(22,14,28),52,(1400,1000))
render('10-runtime-distance-final',(175,-300,34),(5,17,32),52,(1600,900))
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'Solstice_Environment_Kit.blend'))
