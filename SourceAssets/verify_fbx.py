"""Read-only FBX integrity and axis check, using the standard Python library."""
import json, math, struct, zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
blob = (ROOT / 'UnityProject/Assets/Resources/Art/HeroShip.fbx').read_bytes()
assert blob.startswith(b'Kaydara FBX Binary'), 'Expected binary FBX'
version = struct.unpack_from('<I', blob, 23)[0]
header_size, header_fmt = (25, '<QQQB') if version >= 7500 else (13, '<IIIB')

def read_node(pos):
    end, prop_count, prop_length, name_length = struct.unpack_from(header_fmt, blob, pos)
    pos += header_size
    if not end: return None, pos
    name = blob[pos:pos + name_length].decode(); pos += name_length
    props = []
    for _ in range(prop_count):
        kind = chr(blob[pos]); pos += 1
        if kind in 'SR':
            length = struct.unpack_from('<I', blob, pos)[0]; pos += 4
            value = blob[pos:pos + length]; pos += length
            if kind == 'S': value = value.decode(errors='replace')
        elif kind in 'fdlibc':
            count, encoding, length = struct.unpack_from('<III', blob, pos); pos += 12
            raw = blob[pos:pos + length]; pos += length
            if encoding: raw = zlib.decompress(raw)
            fmt = {'f':'f','d':'d','l':'q','i':'i','b':'b','c':'b'}[kind]
            value = struct.unpack('<' + str(count) + fmt, raw)
        else:
            fmt = {'Y':'h','C':'?','I':'i','F':'f','D':'d','L':'q'}[kind]
            value = struct.unpack_from('<' + fmt, blob, pos)[0]; pos += struct.calcsize(fmt)
        props.append(value)
    children = []
    while pos < end - header_size:
        child, pos = read_node(pos)
        if child: children.append(child)
    return (name, props, children), end

nodes = []; cursor = 27
while cursor < len(blob) - header_size:
    item, cursor = read_node(cursor)
    if not item: break
    nodes.append(item)
globals_node = next(n for n in nodes if n[0] == 'GlobalSettings')
properties = {p[1][0]: p[1][-1] for n in globals_node[2] if n[0] == 'Properties70' for p in n[2]}
assert (properties['UpAxis'], properties['UpAxisSign']) == (1, 1), 'FBX must be +Y up'
assert properties['UnitScaleFactor'] == 1, 'Expected FBX centimetre metadata'
objects = next(n for n in nodes if n[0] == 'Objects')[2]
geometry = [n for n in objects if n[0] == 'Geometry']
assert len(geometry) == 9, 'Expected one mesh for each of nine used materials'
all_vertices = []; polygons = triangles = vertex_count = 0
for item in geometry:
    children = {n[0]:n[1] for n in item[2]}
    verts = children['Vertices'][0]; indices = children['PolygonVertexIndex'][0]
    assert len(verts) % 3 == 0 and all(math.isfinite(v) for v in verts)
    count = len(verts) // 3; vertex_count += count
    all_vertices.extend(zip(verts[0::3], verts[1::3], verts[2::3]))
    face_size = 0
    for index in indices:
        decoded = -index - 1 if index < 0 else index
        assert 0 <= decoded < count, 'Invalid mesh index'
        face_size += 1
        if index < 0:
            assert face_size >= 3
            polygons += 1; triangles += face_size - 2; face_size = 0
    assert face_size == 0, 'Unterminated polygon'
bounds_cm = [[min(v[i] for v in all_vertices),max(v[i] for v in all_vertices)] for i in range(3)]
dimensions_m = [(hi-lo)/100 for lo,hi in bounds_cm]
source_stats = json.loads((ROOT/'SourceAssets/asset-stats.json').read_text())
expected = [source_stats['source_dimensions_xyz'][i] for i in [0,2,1]]
assert all(abs(a-b) < .005 for a,b in zip(dimensions_m,expected)), (dimensions_m,expected)
assert bounds_cm[2][1] > 400 and bounds_cm[2][0] < -300, 'Nose/tail bounds differ from intended +Z orientation'
assert triangles == source_stats['triangles']
report = {'status':'passed','fbx_version':version,'meshes':len(geometry),'vertices':vertex_count,'polygons':polygons,'triangles':triangles,'dimensions_unity_xyz_m':[round(v,4) for v in dimensions_m],'bounds_fbx_cm':bounds_cm,'checks':['valid finite vertices and polygon indices','nine material mesh groups','centimetre metadata','positive Y up','metre-scale bounds agree with authored source','positive Z nose extent','triangle count matches source']}
(ROOT/'evidence/asset-renders/fbx-validation.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
