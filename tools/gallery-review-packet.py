#!/usr/bin/env python3
"""Build labeled contact sheets from native anchors; originals remain the authority."""
import argparse,json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

p=argparse.ArgumentParser()
p.add_argument('control',type=Path);p.add_argument('candidate',type=Path);p.add_argument('--output',type=Path,required=True)
a=p.parse_args();a.output.mkdir(parents=True,exist_ok=True)
reports=[json.loads((folder/'environment-evidence.json').read_text()) for folder in [a.control,a.candidate]]
assert all(r['complete'] and len(r['anchorFrames'])==9 for r in reports)
font=ImageFont.truetype('/System/Library/Fonts/Helvetica.ttc',24)
for first in [0,3,6]:
 sheet=Image.new('RGB',(1920,3*(540+48)),(13,20,27));draw=ImageDraw.Draw(sheet)
 for row in range(3):
  for col,(folder,r) in enumerate(zip([a.control,a.candidate],reports)):
   f=r['frames'][r['anchorFrames'][first+row]]
   image=Image.open(folder/f['anchorFile']).convert('RGB');assert image.size==(1920,1080)
   sheet.paste(image.resize((960,540),Image.Resampling.LANCZOS),(col*960,row*588+48))
   draw.text((col*960+12,row*588+10),('CONTROL' if col==0 else 'CANDIDATE')+'  '+f['anchor'],font=font,fill=(220,234,244))
 sheet.save(a.output/f'comparison-{first//3+1:02d}.jpg',quality=95,subsampling=0)
print(a.output.resolve())
