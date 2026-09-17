from PIL import Image,ImageDraw,ImageFont,ImageFilter
from pathlib import Path
import random,math
root=Path(__file__).resolve().parents[2]/'UnityProject/Assets/Art/Stage1/Textures';root.mkdir(parents=True,exist_ok=True)
r=random.Random(823)
# Each facade tile spans 12m x 24m: four narrow bays and eight occupied floors.
for variant in range(4):
 base=Image.new('RGB',(512,1024),(30,42,50));emit=Image.new('RGB',base.size);b=ImageDraw.Draw(base);e=ImageDraw.Draw(emit)
 for row in range(8):
  for col in range(4):
   x,y=col*128,row*128;lit=r.random()<(.25+variant*.06)
   glass=(34+r.randrange(6),49+r.randrange(9),59+r.randrange(10));b.rectangle((x+15,y+16,x+112,y+100),fill=glass)
   if lit:
    tint=r.choice([(170,137,91),(96,144,165),(215,185,133),(118,166,175)])
    e.rectangle((x+18,y+19,x+109,y+97),fill=tint)
    # Blinds, a sill, interior mullions and partial occupancy break repeated blocks.
    for yy in range(y+21,y+96,9):e.line((x+18,yy,x+109,yy),fill=tuple(int(v*.7) for v in tint),width=2)
    if r.random()<.6:e.rectangle((x+18,y+19,x+55,y+97),fill=tuple(int(v*.32) for v in tint))
   b.line((x+64,y+16,x+64,y+100),fill=(10,15,20),width=3);e.line((x+64,y+16,x+64,y+100),fill=(0,0,0),width=3)
   b.rectangle((x,y+112,x+127,y+125),fill=(34,42,47));b.line((x+3,y,x+3,y+128),fill=(43,52,59),width=3)
 base.save(root/f'Facade{variant}_Base.png');emit.save(root/f'Facade{variant}_Emission.png')
# Asphalt/brushed graphite: dark albedo, pale aggregate and strongly longitudinal micrograin.
im=Image.new('RGB',(512,1024));px=im.load()
for y in range(1024):
 for x in range(512):
  v=int(88+r.gauss(0,7)+3*math.sin(x*.6));px[x,y]=(v,v+3,v+5)
im.save(root/'GraphiteRoad.png')
font='/System/Library/Fonts/Supplemental/Arial Bold.ttf';small='/System/Library/Fonts/Supplemental/Arial.ttf'
for i,(word,sub,c) in enumerate([('NOCTURNE','THE CITY NEVER STOPS',(0,193,228)),('ION','PURE ELECTRIC FORCE',(242,40,124)),('V E S P E R','BEYOND THE LIMIT',(37,145,233)),('NOVA 07','NEXT GENERATION',(247,166,77))]):
 im=Image.new('RGB',(512,1024),(8,14,21));d=ImageDraw.Draw(im)
 for y in range(1024):
  a=max(0,1-abs(y-430)/600);d.line((0,y,511,y),fill=tuple(int(v*.2*a)+8 for v in c))
 for k in range(5):d.ellipse((50+k*22,220+k*22,460-k*22,630-k*22),outline=c,width=4 if k else 13)
 d.line((50,880,462,880),fill=c,width=3)
 fs=66 if i else 56
 d.text((32,70),word,font=ImageFont.truetype(font,fs),fill=(235,249,255))
 d.text((34,758),sub,font=ImageFont.truetype(font,23),fill=(226,240,244))
 d.text((34,907),'SECTOR 01  /  NIGHT SERIES',font=ImageFont.truetype(small,21),fill=c)
 d.polygon([(143,520),(283,303),(363,351),(223,568)],fill=c)
 im.save(root/f'Advert{i}.png')
print(root)
# Consume the delivered ship accent mask, keeping the body opaque during boost.
energy_root=Path(__file__).resolve().parents[2]/'UnityProject/Assets/Resources/Stage1/ShipEnergy'
energy_root.mkdir(parents=True,exist_ok=True)
for key in ['Ivory','Metal','Graphite','Ceramic']:
 source=Path(__file__).resolve().parents[2]/'SourceAssets/Experience/packages/vrx-c01/ship'/f'vrx_ship_{key.lower()}_a_energy.png'
 Image.eval(Image.open(source).convert('RGB').split()[0],lambda x:x*.85).save(energy_root/f'Stage1Energy_{key}.png')
# A landscape inset advert for the foreground service facade.
im=Image.new('RGB',(1024,512),(9,20,29));d=ImageDraw.Draw(im)
for k in range(16):
 d.line((420+k*26,55,250+k*26,455),fill=(15,47,62),width=11)
d.rectangle((32,32,46,480),fill=(240,40,120))
d.text((82,58),'NOVA',font=ImageFont.truetype(font,112),fill=(230,248,250))
d.text((90,197),'ELECTRIC / AFTER DARK',font=ImageFont.truetype(font,30),fill=(62,215,238))
d.text((90,398),'CITYGRID 07',font=ImageFont.truetype(font,32),fill=(220,233,236))
for k in range(3):
 d.ellipse((620+k*28,126+k*28,940-k*28,446-k*28),outline=(50,196,227),width=5)
d.line((83,357,541,357),fill=(240,40,120),width=8)
im.save(root/'ServiceAdvert.png')
