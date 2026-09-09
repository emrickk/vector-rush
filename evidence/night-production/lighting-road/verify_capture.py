#!/usr/bin/env python3
"""Validate native PNG/report integrity and select natural night-stage crossings.
No image is changed. Run from the repository root with a candidate directory.
"""
import argparse, hashlib, json, math, shutil, struct, zlib
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('candidate', type=Path)
parser.add_argument('--reference', type=Path, help='Use selections from a prior capture-validation.json instead of the original production baseline.')
parser.add_argument('--expected-width', type=int, default=1920)
parser.add_argument('--expected-height', type=int, default=1080)
args = parser.parse_args()
root = Path.cwd()
stage = args.candidate
capture = stage / 'full-lap'
report = json.loads((capture / 'environment-evidence.json').read_text())
baseline = json.loads((root / 'evidence/night-production/baseline/selection.json').read_text())
if args.reference:
    reference = json.loads(args.reference.read_text())
    baseline = {'selections': [entry for entry in reference['selections'] if entry['kind'] == 'primary'],
                'controls': [entry for entry in reference['selections'] if entry['kind'] == 'control']}
assert report['complete'], 'Incomplete native report'
frames = report['frames']
assert len(frames) == 1440, len(frames)

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

hashes = {}
for index, frame in enumerate(frames):
    assert frame['index'] == index and frame['captureRequested'], index
    path = capture / frame['file']
    w, h, sha = png_info(path)
    assert (w, h) == (frame['width'], frame['height']), path
    assert (w, h) == (args.expected_width, args.expected_height), ('Unexpected capture resolution', path, w, h)
    hashes[frame['file']] = sha

selected_dir = stage / 'selected'
selected_dir.mkdir(exist_ok=False)

def distance(a, b):
    return math.sqrt(sum((a[k] - b[k]) ** 2 for k in ('x', 'y', 'z')))

def angle(a, b):
    dot = sum(a[k] * b[k] for k in ('x', 'y', 'z', 'w'))
    size = math.sqrt(sum(v*v for v in a.values()) * sum(v*v for v in b.values()))
    return math.degrees(2 * math.acos(min(1, abs(dot / size))))

def crossing(t):
    return next(b for a, b in zip(frames, frames[1:])
                if a['lap'] == b['lap'] and a['progress'] < t <= b['progress']
                and 0 < b['progress'] - a['progress'] < .05)

selections = []
for kind, entry in [('primary', e) for e in baseline['selections']] + [('control', e) for e in baseline['controls']]:
    old = entry['frame']
    threshold = entry.get('threshold', old['progress'])
    current = crossing(threshold)
    source = capture / current['file']
    destination = selected_dir / source.name
    if not destination.exists():
        shutil.copy2(source, destination)
    delta = {
        'cameraMetres': distance(old['cameraPosition'], current['cameraPosition']),
        'cameraDegrees': angle(old['cameraRotation'], current['cameraRotation']),
        'fovDegrees': abs(old['fieldOfView'] - current['fieldOfView']),
        'playerMetres': distance(old['racers'][0]['position'], current['racers'][0]['position']),
        'progress': current['progress'] - old['progress'],
        'speedKph': current['speedKph'] - old['speedKph'],
        'raceSeconds': current['raceTime'] - old['raceTime']}
    matched = delta['cameraMetres'] <= .05 and delta['cameraDegrees'] <= .1 and delta['fovDegrees'] <= .05 and delta['playerMetres'] <= .05
    selections.append({'kind': kind,
                       'threshold': threshold, 'baselineIndex': old['index'], 'frame': current,
                       'sha256': hashes[current['file']], 'deltaFromBaseline': delta,
                       'exactPoseTolerancePassed': matched})

identity = json.loads((stage / 'build-identity.json').read_text())
source_changes = [p for p, sha in identity['files'].items() if hashlib.sha256((root / p).read_bytes()).hexdigest() != sha]
app_changes = [p for p, sha in identity['appFiles'].items() if hashlib.sha256((root / p).read_bytes()).hexdigest() != sha]
assert not source_changes and not app_changes, (source_changes, app_changes)
result = {'buildGuid': report['buildGuid'], 'scope': 'Natural native route crossings, no altered pixels or injected poses. Integrity and pose comparison only; not visual or watched-motion acceptance.',
          'referenceSelection': str(args.reference) if args.reference else 'evidence/night-production/baseline/selection.json',
          'expectedDimensions': [args.expected_width, args.expected_height],
          'nativeComplete': True, 'pngsValidated': len(hashes), 'pngHashes': hashes,
          'sourceFilesUnchanged': len(identity['files']), 'appFilesUnchanged': len(identity['appFiles']),
          'selections': selections}
(stage / 'capture-validation.json').write_text(json.dumps(result, indent=2))
print(json.dumps({'buildGuid': report['buildGuid'], 'pngsValidated': len(hashes),
                  'indices': [s['frame']['index'] for s in selections],
                  'allExactPoseTolerancePassed': all(s['exactPoseTolerancePassed'] for s in selections)}, indent=2))
