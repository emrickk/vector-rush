#!/usr/bin/env python3
"""Create a local comparison page from validated native gallery evidence."""
import argparse, html, json, os
from pathlib import Path

p = argparse.ArgumentParser()
p.add_argument('control', type=Path)
p.add_argument('candidate', type=Path)
p.add_argument('--output', type=Path, required=True)
a = p.parse_args()
reports = [json.loads((folder/'environment-evidence.json').read_text()) for folder in [a.control,a.candidate]]
assert all(r['complete'] and len(r['anchorFrames']) == 9 for r in reports)
assert reports[0]['buildGuid'] == reports[1]['buildGuid']
rows = []
for index in range(9):
 frames = [r['frames'][r['anchorFrames'][index]] for r in reports]
 assert frames[0]['anchor'] == frames[1]['anchor']
 imgs=[]
 for folder,f,label in zip([a.control,a.candidate],frames,['Original control','Gallery candidate']):
  path=folder/f['anchorFile'];assert path.exists()
  url=html.escape(os.path.relpath(path.resolve(),a.output.resolve().parent))
  imgs.append(f'<figure><a href="{url}"><img loading="lazy" src="{url}" alt="{label}"></a><figcaption>{label} · progress {f["progress"]:.6f}</figcaption></figure>')
 rows.append('<section><h2>'+html.escape(frames[0]['anchor'])+'</h2><div class="pair">'+''.join(imgs)+'</div></section>')
match=reports[1]['matchedWithinTolerance']
text='Matched camera/vehicle anchor poses within recorded tolerances.' if match else 'POSE MISMATCH: these images cannot establish a controlled visual improvement.'
a.output.write_text('''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Vector Rush gallery native comparison</title><style>body{background:#10171e;color:#e9eff4;font:16px system-ui;margin:0 auto;padding:28px;max-width:1900px}h1{font-size:30px}p{line-height:1.6;max-width:1000px;color:#b6c7d2}.pair{display:grid;grid-template-columns:1fr 1fr;gap:16px}figure{margin:0}img{width:100%;height:auto}figcaption{padding:8px 0;color:#b6c7d2}section{margin:35px 0}h2{font-size:18px}@media(max-width:900px){.pair{grid-template-columns:1fr}}</style><h1>Gallery native comparison</h1><p>One native binary, original control and optional gallery candidate. Nine approach/interior/exit views. '''+html.escape(text)+''' Stills show appearance only; the automated, fixed-simulation sequence is not human play or a performance measurement. Click any image for its original resolution.</p><p>Build '''+html.escape(reports[0]['buildGuid'])+'</p>'+''.join(rows)+'</html>')
print(a.output.resolve())
