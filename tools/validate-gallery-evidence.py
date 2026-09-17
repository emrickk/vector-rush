#!/usr/bin/env python3
"""Validate original native gallery capture evidence without claiming appearance."""
import argparse, hashlib, json, struct, zlib
from pathlib import Path


def png_info(path):
    data = path.read_bytes()
    if data[:8] != b'\x89PNG\r\n\x1a\n':
        raise ValueError(f'Not a PNG: {path}')
    pos, compressed, size, ended = 8, bytearray(), None, False
    while pos < len(data):
        length = struct.unpack('>I', data[pos:pos+4])[0]
        kind = data[pos+4:pos+8]
        payload = data[pos+8:pos+8+length]
        crc = struct.unpack('>I', data[pos+8+length:pos+12+length])[0]
        if zlib.crc32(kind + payload) & 0xffffffff != crc:
            raise ValueError(f'PNG CRC mismatch: {path}')
        if kind == b'IHDR':
            size = struct.unpack('>II', payload[:8])
        if kind == b'IDAT':
            compressed.extend(payload)
        pos += 12 + length
        if kind == b'IEND':
            ended = True
            break
    if not ended or pos != len(data) or not size:
        raise ValueError(f'Incomplete PNG: {path}')
    zlib.decompress(compressed)
    return size, hashlib.sha256(data).hexdigest()


def check(folder):
    report = json.loads((folder / 'environment-evidence.json').read_text())
    assert report['complete'], 'Capture incomplete'
    assert report.get('simulationCaptureRate') == 24, 'Gallery evidence must use the declared 24 fps simulation cadence'
    assert len(report['frames']) == 288, 'Expected 288 simulation frames'
    assert len(report['anchorFrames']) == 9, 'Expected nine gallery anchors'
    images = []
    for frame in report['frames']:
        assert (frame['width'], frame['height']) == (1920, 1080), 'Unexpected native dimensions'
        if frame['captureRequested']:
            path = folder / frame['file']
            size, digest = png_info(path)
            assert size == (1920, 1080), 'Unexpected PNG dimensions'
            images.append({'file': frame['file'], 'sha256': digest})
    assert report['frames'][-1]['raceTime'] > report['frames'][0]['raceTime'], 'No race progression'
    return report, {'folder': str(folder.resolve()), 'buildGuid': report['buildGuid'],
                    'mode': report.get('exemplarMode'), 'imagesVerified': len(images),
                    'anchors': [report['frames'][i]['anchor'] for i in report['anchorFrames']],
                    'images': images, 'scope': 'PNG integrity, identity and capture coverage only; appearance/motion/audio/performance unassessed'}


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('control', type=Path)
    parser.add_argument('candidate', type=Path)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    a, ac = check(args.control)
    b, bc = check(args.candidate)
    assert a['buildGuid'] == b['buildGuid'], 'Control and candidate must use one binary'
    assert a['simulationCaptureRate'] == b['simulationCaptureRate'], 'Different simulation capture rates'
    assert a.get('exemplarMode') == 'off' and b.get('exemplarMode') == 'gallery', 'Wrong comparison modes'
    assert not a.get('exemplarAttached') and b.get('exemplarAttached'), 'Candidate did not attach or control was modified'
    assert a.get('additionalLightShadowsSupported') == b.get('additionalLightShadowsSupported'), 'Different pipeline shadow capability'
    assert len(b['comparisons']) == 9, 'Candidate lacks anchor pose comparisons'
    result = {'control': ac, 'candidate': bc,
              'matchedWithinTolerance': b['matchedWithinTolerance'], 'comparisons': b['comparisons']}
    args.output.write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps({'images': [ac['imagesVerified'], bc['imagesVerified']],
                      'matchedWithinTolerance': result['matchedWithinTolerance']}))
    if not result['matchedWithinTolerance']:
        raise SystemExit('Pose mismatch: captures cannot support a matched visual comparison')


if __name__ == '__main__':
    main()
