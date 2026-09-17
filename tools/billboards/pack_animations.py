#!/usr/bin/env python3
"""Pack Scenario-generated advertising videos into Unity GPU flipbooks.

No API credentials required. Input is the completed local generation folder.
The original videos and exact request prompts are retained as editable source.
"""
import argparse, hashlib, json, subprocess, tempfile, shutil
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageChops, ImageStat

p=argparse.ArgumentParser();p.add_argument('input',type=Path);args=p.parse_args()
repo=Path(__file__).resolve().parents[2]
dest=repo/'UnityProject/Assets/Art/AnimatedBillboardsStage6';dest.mkdir(parents=True,exist_ok=True)
source=repo/'SourceAssets/BillboardsStage6';source.mkdir(parents=True,exist_ok=True)
names=['aurora','volt','echo','orbit'];records=[]
for theme,name in enumerate(names):
    folder=args.input/name;video=folder/'animation.mp4'
    if not video.exists():raise SystemExit('Video not ready: '+str(video))
    with tempfile.TemporaryDirectory(prefix='vector-billboard-') as tmp:
        subprocess.run(['ffmpeg','-v','error','-i',str(video),'-vf','fps=12.8,scale=384:576:force_original_aspect_ratio=decrease,pad=384:576:(ow-iw)/2:(oh-ih)/2:color=black','-frames:v','64',str(Path(tmp)/'%03d.png')],check=True)
        frames=sorted(Path(tmp).glob('*.png'))
        if len(frames)!=64:raise SystemExit(f'Expected 64 frames, got {len(frames)} for {name}')
        atlas=Image.new('RGB',(3072,4608));images=[Image.open(f).convert('RGB') for f in frames]
        # Blend the last quarter-second toward the first frame to conceal small loop drift.
        if name!="echo":
            for i in range(60,64):images[i]=Image.blend(images[i],images[0],(i-59)/5)
        for i,im in enumerate(images):atlas.paste(im,(i%8*384,i//8*576))
        motion=sum(ImageStat.Stat(ImageChops.difference(images[0],images[32])).mean)/3
        if motion<1:raise SystemExit('Video appears static: '+name)
        atlas.save(dest/f'Frames{theme}.png',optimize=True)
        contact=Image.new('RGB',(384*4,576))
        for i,index in enumerate([0,16,32,48]):contact.paste(images[index],(i*384,0))
        contact.save(args.input/f'{name}-motion.jpg',quality=88)
    original=source/name;original.mkdir(exist_ok=True)
    for file in ['art.png','animation.mp4','request.json','job.json','video-request.json','video-job.json']:
        shutil.copy2(folder/file,original/file)
    records.append({'campaign':name,'frameCount':64,'duration':10 if name=='echo' else 5,'playback':'ping-pong' if name=='echo' else 'loop','atlasSize':[3072,4608],'frameSize':[384,576],'motionDifferenceMean':motion,'videoSha256':hashlib.sha256(video.read_bytes()).hexdigest()})

font=repo/'UnityProject/Assets/Resources/Fonts/Saira-BlackItalic.ttf'
medium=repo/'UnityProject/Assets/Resources/Fonts/Saira-MediumItalic.ttf'
colors=['#ca8bff','#d5fc12','#ff5937','#ffaa44']
slogans=['AFTER DARK','CHARGE THE NIGHT','FEEL THE SIGNAL','BEYOND LIMITS']
ticker=Image.new('RGB',(2048,512));titles=Image.new('RGB',(512,2048))
for i,name in enumerate(names):
    # Shader samples campaigns bottom to top, so vertically reverse the four bands.
    y=(3-i)*128;d=ImageDraw.Draw(ticker);d.rectangle((0,y,2048,y+127),fill='#090d16')
    d.text((30,y+15),f'{name.upper()}   /   {slogans[i]}   /   NOCTURNE FREQUENCY   /',font=ImageFont.truetype(str(font),61),fill=colors[i])
    y=(3-i)*512;d=ImageDraw.Draw(titles);d.rectangle((0,y,512,y+512),fill='#080d15')
    size=94
    while ImageFont.truetype(str(font),size).getlength(name.upper())>470:size-=1
    d.text((24,y+112),name.upper(),font=ImageFont.truetype(str(font),size),fill=colors[i])
    for j,word in enumerate(slogans[i].split()):d.text((28,y+240+j*44),word,font=ImageFont.truetype(str(medium),34),fill='#d5dae7')
    d.text((28,y+42),'NOCTURNE / '+str(i+1).zfill(2),font=ImageFont.truetype(str(medium),21),fill='#637181')
ticker.save(dest/'Ticker.png');titles.save(dest/'Titles.png')
(source/'animation-manifest.json').write_text(json.dumps(records,indent=2)+'\n')
print(json.dumps(records,indent=2))
