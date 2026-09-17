#!/usr/bin/env python3
"""Encode native frames using their recorded real-time timestamps and captured game audio."""
import argparse,json,subprocess
from pathlib import Path
import array,math,statistics
p=argparse.ArgumentParser();p.add_argument('directory',type=Path);args=p.parse_args();root=args.directory.resolve()
d=json.loads((root/'preview.json').read_text());assert d['complete'], 'Preview did not complete a lap'
frames=list(d['frames']);captured=len(frames)
# The capture may include an instantaneous frame exactly at audio/lap end.
# It has no display duration; retain the raw capture and omit only that endpoint.
while frames and frames[-1]['seconds']>=d['endDsp']-d['startDsp']-1e-8:frames.pop()
assert len(frames)>2
lines=[]
for i,frame in enumerate(frames):
 path=root/frame['file'];assert path.is_file(),path
 duration=(frames[i+1]['seconds'] if i+1<len(frames) else d['endDsp']-d['startDsp'])-frame['seconds']
 assert duration>0
 # ffmpeg concat syntax permits single-quoted paths with escaped apostrophes.
 escaped=str(path).replace("'", "'\\''")
 lines.extend(["file '"+escaped+"'", "option framerate 120", f'duration {duration:.8f}'])
lines.extend([lines[-3], "option framerate 120"]);(root/'frames.ffconcat').write_text('\n'.join(lines)+'\n')
audio=array.array('f');audio.frombytes((root/'game-audio.f32').read_bytes())
assert audio,'No listener audio was captured'
peak=max(abs(x) for x in audio);rms=math.sqrt(sum(x*x for x in audio)/len(audio));assert peak>.0001,'Captured audio is silent'
intervals=[b['seconds']-a['seconds'] for a,b in zip(frames,frames[1:])]
report={'capturedFrames':captured,'captureMeanHz':(len(frames)-1)/(frames[-1]['seconds']-frames[0]['seconds']),'captureMedianIntervalMs':statistics.median(intervals)*1000,'captureMaxIntervalMs':max(intervals)*1000,'buildGuid':d['buildGuid'],'frames':len(frames),'durationSeconds':d['endDsp']-d['startDsp'],'audioSeconds':len(audio)/2/d['sampleRate'],'peak':peak,'rms':rms,'scope':d['scope']}
(root/'preview-validation.json').write_text(json.dumps(report,indent=2)+'\n')
subprocess.run(['ffmpeg','-y','-hide_banner','-loglevel','warning','-f','concat','-safe','0','-i',str(root/'frames.ffconcat'),'-f','f32le','-ar',str(d['sampleRate']),'-ac','2','-i',str(root/'game-audio.f32'),'-c:v','libx264','-preset','fast','-crf','19','-pix_fmt','yuv420p','-fps_mode','vfr','-c:a','aac','-b:a','192k','-shortest','-movflags','+faststart',str(root/'full-lap.mp4')],check=True)
print(json.dumps(report,indent=2))
