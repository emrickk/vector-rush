"""Paired SVG/PNG original sign masters, one drawing description for both."""
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
from html import escape
import shutil
R=Path(__file__).resolve().parent;F=R/'fonts/Rajdhani-SemiBold.ttf'
for key,W,H,col in [('market',1536,512,'#ee513b'),('repair',1024,512,'#c9dedb'),('hotel',512,1536,'#f6533f'),('metro',1536,512,'#c8ded9'),('ramen',1024,1024,'#edb574'),('tower',512,1536,'#9fd8d5')]:
 im=Image.new('RGB',(W,H),'#0c2328');d=ImageDraw.Draw(im);svg=[f'<svg xmlns="http://www.w3.org/2000/svg" width="{W}" height="{H}"><defs><style>@font-face{{font-family:Rajdhani;src:url(../fonts/Rajdhani-SemiBold.ttf)}}</style></defs><rect width="100%" height="100%" fill="#0c2328"/>']
 def rect(b,fill=None,stroke=None,width=4):
  d.rectangle(b,fill=fill,outline=stroke,width=width);x,y,x2,y2=b;svg.append(f'<rect x="{x}" y="{y}" width="{x2-x}" height="{y2-y}" fill="{fill or "none"}" stroke="{stroke or "none"}" stroke-width="{width}"/>')
 def line(points,c=col,width=6):
  d.line(points,fill=c,width=width);svg.append(f'<polyline points="'+ ' '.join(f'{x},{y}' for x,y in points)+f'" fill="none" stroke="{c}" stroke-width="{width}"/>')
 def circle(b,stroke=col,width=6,fill=None):
  d.ellipse(b,fill=fill,outline=stroke,width=width);x,y,x2,y2=b;svg.append(f'<ellipse cx="{(x+x2)/2}" cy="{(y+y2)/2}" rx="{(x2-x)/2}" ry="{(y2-y)/2}" fill="{fill or "none"}" stroke="{stroke}" stroke-width="{width}"/>')
 def text(t,x,y,size,c=col):
  f=ImageFont.truetype(str(F),size);d.text((x,y),t,font=f,fill=c,anchor='mm');svg.append(f'<text x="{x}" y="{y}" text-anchor="middle" dominant-baseline="central" font-family="Rajdhani" font-size="{size}" fill="{c}">{escape(t)}</text>')
 if key=='hotel':
  rect((30,30,W-30,H-30),stroke=col,width=8)
  for i,t in enumerate('HOTEL'):text(t,W/2,170+i*236,265)
  rect((66,1320,446,1425),fill=col);text('VACANCY',256,1375,66,'#0c2328');text('07 / NIGHT DESK',256,1460,29)
 elif key=='market':
  line([(38,40),(1498,40)],width=10);line([(38,472),(1498,472)],width=10)
  text('NIGHT MARKET',745,225,220);text('FOOD   /   MUSIC   /   OPEN ALL NIGHT',745,391,50)
 elif key=='repair':
  rect((30,30,994,482),stroke=col,width=4);rect((30,30,205,482),fill='#d38a4c')
  text('07',118,190,135,'#0c2328');text('BAY',118,337,56,'#0c2328');text('REPAIR',590,175,185);text('SERVICE / 24 H',590,336,70)
  for xx in range(230,980,46):line([(xx,432),(xx+20,410)],'#d38a4c',9)
 elif key=='metro':
  circle((48,72,412,436),width=10);text('M',230,255,285);text('EASTBOUND',900,169,173);text('PLATFORM 02  /  NIGHT SERVICE',900,326,55)
  line([(470,400),(1420,400),(1380,375)],width=8)
 elif key=='ramen':
  circle((40,40,984,984),width=18);circle((75,75,949,949),width=3);text('NOODLE BAR',512,235,122)
  line([(267,500),(330,674),(420,725),(604,725),(694,674),(757,500),(267,500)],width=16)
  line([(398,768),(626,768)],width=13)
  for xx in [385,512,639]:line([(xx,451),(xx-16,410),(xx+20,370),(xx,332)],width=10)
  text('HOT FOOD / LATE HOURS',512,852,47)
 else:
  rect((28,28,484,1508),stroke=col,width=7);text('NOVA',256,140,132)
  line([(350,300),(162,708),(292,708),(154,1170),(379,595),(242,595),(350,300)],width=22)
  text('ELECTRIC',256,1310,90);text('POWER THE NIGHT',256,1440,39)
 svg.append('</svg>');(R/'sign-source'/f'{key}.svg').write_text(''.join(svg));im.save(R/'textures'/f'sign_{key}_base.png');im.save(R/'textures'/f'sign_{key}_emission.png')
