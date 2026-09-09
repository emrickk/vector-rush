#!/usr/bin/env python3
"""Validate sparse opening draft evidence: original PNGs and every natural pose, not motion."""
import argparse, hashlib, json, math, shutil, struct, zlib
from pathlib import Path
parser=argparse.ArgumentParser()
parser.add_argument('candidate',type=Path)
parser.add_argument('--reference',type=Path,required=True)
args=parser.parse_args()
root=Path.cwd();stage=args.candidate;capture=stage/'full-lap'
report=json.loads((capture/'environment-evidence.json').read_text())
reference=json.loads(args.reference.read_text())
assert report['complete'], 'Native capture incomplete'
frames=report['frames'];assert len(frames)==1440,len(frames)
assert frames[-1]['lap']>frames[0]['lap'] and frames[-1]['raceTime']-frames[0]['raceTime']>40
assert len(report['anchorFrames'])==9,len(report['anchorFrames'])
def png_info(path):
    data = path.read_bytes()
    assert data[:8] == b'\x89PNG\r\n\x1a\n', path
    pos, payload, dimensions = 8, bytearray(), None
    ended = False
    while pos < len(data):
        length = struct.unpack('>I', data[pos:pos+4])[0]
        tag = data[pos+4:pos+8]
        body = data[pos+8:pos+8+length]
        checksum = struct.unpack('>I', data[pos+8+length:pos+12+length])[0]
        assert zlib.crc32(tag + body) & 0xffffffff == checksum, (path, tag)
        if tag == b'IHDR':
            w, h, depth, color, compression, filtering, interlace = struct.unpack('>IIBBBBB', body)
            assert depth == 8 and color in (2, 6) and interlace == 0, (path, depth, color, interlace)
            dimensions = (w, h, 3 if color == 2 else 4)
        elif tag == b'IDAT':
            payload.extend(body)
        elif tag == b'IEND':
            ended = True
            assert pos + 12 == len(data), path
        pos += length + 12
    assert ended and dimensions, path
    w, h, channels = dimensions
    decoder = zlib.decompressobj()
    raw = decoder.decompress(payload) + decoder.flush()
    assert decoder.eof and not decoder.unused_data and len(raw) == h * (w * channels + 1), path
    return w, h, hashlib.sha256(data).hexdigest()

def distance(a, b):
    return math.sqrt(sum((a[k] - b[k]) ** 2 for k in ('x', 'y', 'z')))

def angle(a, b):
    dot = sum(a[k] * b[k] for k in ('x', 'y', 'z', 'w'))
    size = math.sqrt(sum(v*v for v in a.values()) * sum(v*v for v in b.values()))
    return math.degrees(2 * math.acos(min(1, abs(dot / size))))

selected=stage/'selected';selected.mkdir(exist_ok=False)
thresholds=[.15,.22,.30,.37,.43,.58051056,.61970216,.87306988,.94621426]
selections=[];hashes={}
assert len({e['index'] for e in frames})==1440
for i,f in enumerate(frames):
    assert f['index']==i
    assert (f['width'],f['height'])==(1920,1080)
    assert bool(f['captureRequested'])==(i in report['anchorFrames'])
for threshold,index in zip(thresholds,report['anchorFrames']):
    assert index>0
    current=frames[index];prev=frames[index-1]
    # Match Unity's single-precision anchor literals, including exact-equality crossings.
    actual=struct.unpack('f',struct.pack('f',threshold))[0]
    assert prev['lap']==current['lap'] and prev['progress']<actual<=current['progress'],(index,actual,prev['progress'],current['progress'])
    assert 0<current['progress']-prev['progress']<.05
    entry=min(reference['selections'],key=lambda s:abs(s['threshold']-threshold))
    assert abs(entry['threshold']-threshold)<1e-6
    old=entry['frame'];path=capture/current['file']
    w,h,sha=png_info(path);assert (w,h)==(1920,1080)
    assert current['file']==current['anchorFile'], 'Captured frame must name its actual original PNG'
    hashes[current['file']]=sha;shutil.copy2(path,selected/path.name)
    delta={'cameraMetres':distance(old['cameraPosition'],current['cameraPosition']),
           'cameraDegrees':angle(old['cameraRotation'],current['cameraRotation']),
           'fovDegrees':abs(old['fieldOfView']-current['fieldOfView']),
           'playerMetres':distance(old['racers'][0]['position'],current['racers'][0]['position']),
           'progress':current['progress']-old['progress'],
           'speedKph':current['speedKph']-old['speedKph'],
           'raceSeconds':current['raceTime']-old['raceTime']}
    matched=delta['cameraMetres']<=.05 and delta['cameraDegrees']<=.1 and delta['fovDegrees']<=.05 and delta['playerMetres']<=.05
    selections.append({'kind':entry['kind'],'threshold':threshold,'baselineIndex':old['index'],'frame':current,'sha256':sha,'deltaFromBaseline':delta,'exactPoseTolerancePassed':matched})
assert len(list(capture.glob('*.png')))==9 and not list((capture/'frames').glob('*.png'))
identity=json.loads((stage/'build-identity.json').read_text())
for group in ['files','appFiles']:
    changed=[p for p,h in identity[group].items() if not (root/p).is_file() or hashlib.sha256((root/p).read_bytes()).hexdigest()!=h]
    assert not changed,(group,changed)
result={'buildGuid':report['buildGuid'],'scope':'Nine original native draft stills from natural crossings; all1440 pose frames retained. No injected poses, pixel edits, motion recording or visual acceptance implied.',
        'referenceSelection':str(args.reference),'expectedDimensions':[1920,1080],'nativeComplete':True,'pngsValidated':9,'pngHashes':hashes,
        'sourceFilesUnchanged':len(identity['files']),'appFilesUnchanged':len(identity['appFiles']),'selections':selections}
(stage/'capture-validation.json').write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps({'buildGuid':result['buildGuid'],'pngsValidated':9,'indices':[s['frame']['index'] for s in selections],'allExactPoseTolerancePassed':all(s['exactPoseTolerancePassed'] for s in selections)},indent=2))
