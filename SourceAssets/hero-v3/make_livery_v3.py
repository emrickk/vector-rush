"""Authored vector paint mask; new image creation, no source photograph or asset editing.
R = graphite paint, G = citron paint, B reserved. World bounds x[-2.75,2.75],y[-3.6,3.6].
"""
from PIL import Image,ImageDraw
from pathlib import Path
import sys,json
out=Path(sys.argv[1]);out.mkdir(parents=True,exist_ok=True)
if (out/'ship-livery-mask.png').exists():raise RuntimeError('Preserve existing authored paint; use a new output directory.')
W,H=2048,4096;S=2
im=Image.new('RGB',(W*S,H*S),(0,0,0));draw=ImageDraw.Draw(im)
def xy(x,y):return ((x+2.75)/5.5*W*S,(3.6-y)/7.2*H*S)
def poly(points,color):draw.polygon([xy(x,y) for x,y in points],fill=color)
def rect(x0,y0,x1,y1,color):poly([(x0,y0),(x1,y0),(x1,y1),(x0,y1)],color)
# One paired registration graphic. Rear-readable direction is -X right, -Y up.
# Two hand-drawn solid stencil glyphs, not a text plane or plaque.
outer0=[(.03,.12),(.03,.88),(.10,.96),(.39,.96),(.46,.88),(.46,.12),(.39,.04),(.10,.04)]
inner0=[(.16,.22),(.16,.78),(.21,.84),(.29,.84),(.33,.78),(.33,.22),(.29,.16),(.21,.16)]
def numeral(cx,cy,w,h):
 def glyph(poly_points,color):poly([(cx+(.5-u)*w,cy+(v-.5)*h) for u,v in poly_points],color)
 glyph(outer0,(255,0,0));glyph(inner0,(0,0,0))
 glyph([(.55,.04),(.98,.04),(.98,.18),(.55,.18)],(255,0,0))
 glyph([(.81,.18),(.97,.18),(.71,.96),(.55,.96)],(255,0,0))
 # Small lower registration line with two precision breaks.
 for a,b in [(.03,.30),(.34,.60),(.64,.98)]:glyph([(a,1.07),(b,1.07),(b,1.095),(a,1.095)],(255,0,0))
numeral(1.70,2.30,.70,.60)
# Asymmetric small team index on opposite aft shoulder (spare service access region).
for j in range(3):rect(-1.93+j*.13,2.04,-1.86+j*.13,2.29,(255,0,0))
rect(-1.94,2.40,-1.56,2.425,(255,0,0))
# Restrained citron wedges live inside the top shell profile, away from panel seams.
for side in [-1,1]:
 poly([(side*1.10,1.40),(side*1.24,1.40),(side*1.30,2.12),(side*1.11,2.02)],(0,255,0))
 # One small forebody painted index, not a repeating stripe array.
 poly([(side*1.52,-2.43),(side*1.67,-2.40),(side*1.72,-2.15),(side*1.56,-2.18)],(0,255,0))
# Dark center spine identifiers, captured in Graphite's group when baked.
for side in [-1,1]:
 poly([(side*.20,1.05),(side*.31,1.12),(side*.26,1.54),(side*.16,1.47)],(0,255,0))
im.resize((W,H),Image.Resampling.LANCZOS).save(out/'ship-livery-mask.png')
(out/'livery-mask-contract.json').write_text(json.dumps({'size':[W,H],'space':'Blender source/world metres','projection':'XY; U=(X+2.75)/5.5, V=(Y+3.6)/7.2; normal Z and position Z limit paint to upper surfaces','channels':{'R':'graphite paint mask','G':'citron paint mask','B':'reserved'},'read_direction':'From rear: source -X is screen right; source -Y is screen up','number':'07, authored filled stencil glyphs','geometry':'No added plaque, text mesh or overlapping decal faces'},indent=2))
