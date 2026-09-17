# Uses CC0 Rock 3 by Rob Tuytel / Poly Haven. See textures/Rock3_PROVENANCE.md.
# Unmodified source maps plus a separately named derived URP mask.
import bpy,os
m=bpy.data.materials['Rock'];m.use_nodes=True;nodes=m.node_tree.nodes;nodes.clear();links=m.node_tree.links
p=nodes.new('ShaderNodeBsdfPrincipled');p.inputs['Base Color'].default_value=(1,1,1,1);p.inputs['Metallic'].default_value=0
out=nodes.new('ShaderNodeOutputMaterial');links.new(p.outputs['BSDF'],out.inputs['Surface'])
uv=nodes.new('ShaderNodeTexCoord');mapping=nodes.new('ShaderNodeMapping');mapping.inputs['Scale'].default_value=(4,4,4);links.new(uv.outputs['UV'],mapping.inputs['Vector'])
for filename,socket,noncolor in [('Rock3_CC0_Albedo.jpg','Base Color',False),('Rock3_CC0_NormalGL.png','Normal',True),('Rock3_CC0_Roughness.jpg','Roughness',True)]:
 im=bpy.data.images.load(os.path.join(ROOT,'textures',filename),check_existing=True)
 if noncolor:im.colorspace_settings.name='Non-Color'
 node=nodes.new('ShaderNodeTexImage');node.image=im;links.new(mapping.outputs['Vector'],node.inputs['Vector'])
 if socket=='Normal':
  normal=nodes.new('ShaderNodeNormalMap');normal.inputs['Strength'].default_value=.75;links.new(node.outputs['Color'],normal.inputs['Color']);links.new(normal.outputs['Normal'],p.inputs['Normal'])
 else:links.new(node.outputs['Color'],p.inputs[socket])
 if bpy.data.filepath and os.path.dirname(bpy.data.filepath)==ROOT:im.filepath='//textures/'+filename
m.diffuse_color=(.42,.37,.30,1)
