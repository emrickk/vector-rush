"""Finalize and seal the Gallery 02 staging package; never runs Unity or Git."""
from pathlib import Path
import hashlib, json, shutil, struct, zlib

root = Path(__file__).resolve().parent.parent
out = root / 'exemplar-01-gallery02'
revision = 'exemplar-01-gallery02-lighting_a'
status = 'TECHNICAL_PAYLOAD_COMPLETE_BLOCKED_VISUAL_REVIEW'
manifest = json.loads((out / 'manifest.json').read_text())
layout = json.loads((out / 'layout.json').read_text())
stats = json.loads((out / 'asset-stats.json').read_text())
roundtrip = json.loads((out / 'checks/fbx-roundtrip.json').read_text())
clearance = json.loads((out / 'checks/placement-clearance.json').read_text())
lighting = json.loads((out / 'gallery-lighting.json').read_text())
log = (out / 'checks/blender-build.log').read_text()

def canonical_course_hash(data):
    frames = data['frames']
    payload = bytearray(b'VRCRS1')
    payload.extend(struct.pack('<i', len(frames) - 1))
    for value in (data['width'], data['length']):
        payload.extend(struct.pack('<f', float(value)))
    for frame in frames:
        payload.extend(struct.pack('<f', float(frame['progress'])))
        payload.extend(struct.pack('<f', float(frame['distanceMeters'])))
        for field in ('position', 'forward', 'right', 'up'):
            assert len(frame[field]) == 3
            for value in frame[field]:
                payload.extend(struct.pack('<f', float(value)))
    return hashlib.sha256(payload).hexdigest()

course = json.loads((out / 'course-data.json').read_bytes())
canonical_hash = canonical_course_hash(course)
assert 'COMPLETE_GALLERY02_AUTHORING' in log and 'STUDIO_RENDER_COMPLETE' in log
assert manifest['revision'] == layout['revision'] == revision
assert manifest['courseHash'] == layout['courseHash'] == lighting['courseHash']
assert manifest['courseHash'] == canonical_hash
assert manifest['courseSource']['sha256'] == hashlib.sha256((out / 'course-data.json').read_bytes()).hexdigest()
assert manifest['lights'] == [] and manifest['galleryLighting'] == 'gallery-lighting.json'
assert layout['layoutComplete'] is True
assert layout['gallerySpan'] == {'startProgress': 0.86, 'endProgress': 0.902}
assert layout['replacement']['mode'] == 'warm-details-and-local-lights'
assert layout['replacement']['rendererNames'] == [
    'Warm gallery primary portal structure', 'Warm gallery recessed ceramic cassettes',
    'Warm gallery folded service returns', 'Warm gallery light and maintenance housings',
    'Warm gallery integrated diffusers', 'Warm gallery maintenance markings']
assert layout['replacement']['lightNames'] == ['Warm gallery concealed surface wash', 'Warm gallery road pool']
station = [i for i in layout['instances'] if i['zoneId'] == 'station']
assert station and min(i['sourceFrame']['progress'] for i in station) <= .860001
assert max(i['sourceFrame']['progress'] for i in station) >= .9016
assert max(i['sourceFrame']['progress'] for i in station) - min(i['sourceFrame']['progress'] for i in station) <= .15
assert all(i['scale'][0] == i['scale'][1] == i['scale'][2] > 0 for i in layout['instances'])
for item in stats.values():
    for key in ['nonmanifoldEdges', 'degenerateTriangles', 'duplicateTriangles', 'zeroAreaUVTriangles', 'nonpositiveComponents']:
        assert item[key] == 0, (key, item)
for item in clearance.values():
    assert item['surfaceTriangleIntersections'] == item['verticesInsideProtectedCorridorSamples'] == 0, item
for asset in manifest['assets']:
    for lod in ('lod0', 'lod1'):
        path = out / asset[lod]
        key = path.stem
        digest = hashlib.sha256(path.read_bytes()).hexdigest()
        assert digest == stats[key]['fbxSha256']
        assert roundtrip[key]['boundsMaxErrorMeters'] < .0002
    assert all(any(material['id'] == slot for material in manifest['materials']) for slot in asset['materialSlots'])
assert 1 <= len(lighting['fixtures']) <= 32
assert len({fixture['id'] for fixture in lighting['fixtures']}) == len(lighting['fixtures'])
assert sum(fixture['type'] == 'spot' for fixture in lighting['fixtures']) >= 3
for fixture in lighting['fixtures']:
    assert fixture['type'] in ('point', 'spot') and 0 < fixture['intensity'] <= 2000 and 0 < fixture['range'] <= 60
    assert len(fixture['position']) == len(fixture['rgb']) == 3
    if fixture['type'] == 'spot':
        assert 10 <= fixture['outerAngle'] <= 120 and 0 <= fixture['innerAngle'] <= fixture['outerAngle']
        assert len(fixture['aimDirection']) == 3
    else:
        assert 'aimDirection' not in fixture and 'outerAngle' not in fixture and 'innerAngle' not in fixture
texture_checks = {}
for path in (out / 'textures').glob('*.png'):
    payload = path.read_bytes()
    assert payload[:8] == b'\x89PNG\r\n\x1a\n'
    position, compressed = 8, b''
    while position < len(payload):
        length = struct.unpack('>I', payload[position:position + 4])[0]
        kind = payload[position + 4:position + 8]
        value = payload[position + 8:position + 8 + length]
        crc = struct.unpack('>I', payload[position + 8 + length:position + 12 + length])[0]
        assert zlib.crc32(kind + value) & 0xffffffff == crc
        if kind == b'IHDR':
            width, height, depth, mode, _, _, _ = struct.unpack('>IIBBBBB', value)
        if kind == b'IDAT':
            compressed += value
        position += length + 12
    channels = 4 if mode == 6 else 3
    assert len(zlib.decompress(compressed)) == height * (1 + width * channels)
    texture_checks[path.name] = {'size': [width, height], 'bitDepth': depth, 'channels': channels,
                                 'crcAndDecompression': 'pass'}
(out / 'checks/texture-integrity.json').write_text(json.dumps(texture_checks, indent=2))
shutil.copy2(root / 'recipes/build_exemplar_gallery02.py', out / 'source/recipe_snapshot_gallery02.py')
for path in (out / 'source').glob('*.blend1'):
    path.unlink()
checksums = {str(path.relative_to(out)): hashlib.sha256(path.read_bytes()).hexdigest()
             for path in sorted(out.rglob('*'))
             if path.is_file() and path.name not in ('checksums.json', 'STAGED.json')}
(out / 'checksums.json').write_text(json.dumps(checksums, indent=2))
(out / 'STAGED.json').write_text(json.dumps({
    'revision': revision, 'status': status, 'courseHash': manifest['courseHash'],
    'checksumsSha256': hashlib.sha256((out / 'checksums.json').read_bytes()).hexdigest(),
    'layoutComplete': True, 'nativeImportVerified': False, 'rendered': True,
    'visualStatus': 'BLOCKED_VISUAL_REVIEW'
}, indent=2))
print('PACKAGE_INTEGRITY_PASS', len(stats), 'FBX', len(texture_checks), 'maps', len(layout['instances']), 'placements')
for asset in manifest['assets']:
    high, low = stats[asset['id'] + '_LOD0'], stats[asset['id'] + '_LOD1']
    dimensions = [round(b - a, 3) for a, b in zip(high['unityBoundsMin'], high['unityBoundsMax'])]
    print(asset['id'], high['triangles'], low['triangles'], dimensions)
print('FIXTURE_SUGGESTIONS', len(lighting['fixtures']))
print('MAX_ROUNDTRIP_BOUNDS_ERROR', max(item['boundsMaxErrorMeters'] for item in roundtrip.values()))
print('COURSE_HASH', manifest['courseHash'])
print('COURSE_SOURCE_SHA256', manifest['courseSource']['sha256'])
