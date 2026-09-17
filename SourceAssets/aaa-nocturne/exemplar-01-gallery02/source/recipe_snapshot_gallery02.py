"""Nocturne full-span warm gallery revision. Run with Blender --background --python.

The recipe preserves the two opening facade FBX payloads from exemplar-01 byte-for-byte,
authors a materially new gallery assembly, exports LODs, renders a studio inspection still,
and produces measured package evidence. It does not run Unity or claim native acceptance.
"""
import bpy, bmesh, hashlib, json, math, os, shutil, struct, zlib
import numpy as np
from mathutils import Matrix, Quaternion, Vector
from mathutils.bvhtree import BVHTree

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))
OUT = os.path.join(ROOT, 'exemplar-01-gallery02')
PREVIOUS = os.path.join(ROOT, 'exemplar-01')
PROJECT = os.path.abspath(os.path.join(ROOT, '../..'))
EVIDENCE_COURSE = os.path.join(PROJECT, 'evidence/aaa-rebuild/resume-02/context/course-data.json')
PACKAGED_COURSE = os.path.join(OUT, 'course-data.json')
COURSE = EVIDENCE_COURSE if os.path.isfile(EVIDENCE_COURSE) else PACKAGED_COURSE
REVISION = 'exemplar-01-gallery02-lighting_a'
STATUS = 'TECHNICAL_PAYLOAD_COMPLETE_BLOCKED_VISUAL_REVIEW'
STATIONS = [1032, 1040, 1049, 1057, 1066, 1074, 1082]

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

assert os.path.isfile(COURSE), 'Missing both evidence and packaged course-data.json'
course_bytes = open(COURSE, 'rb').read()
course = json.loads(course_bytes)
course_hash = canonical_course_hash(course)
course_source_hash = hashlib.sha256(course_bytes).hexdigest()
assert len(course['frames']) == 1201 and course['width'] == 22.0
for path in ['source', 'meshes', 'textures', 'checks']:
    os.makedirs(os.path.join(OUT, path), exist_ok=True)
if os.path.abspath(COURSE) != os.path.abspath(PACKAGED_COURSE):
    shutil.copy2(COURSE, PACKAGED_COURSE)

bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)
for collection in list(bpy.data.collections):
    if collection != bpy.context.scene.collection:
        bpy.data.collections.remove(collection)
scene = bpy.context.scene
scene.unit_settings.system = 'METRIC'
scene.unit_settings.scale_length = 1
bpy.context.preferences.filepaths.file_preview_type = 'NONE'
scene['Status'] = STATUS + '. Native review pending.'
scene['Revision'] = REVISION
scene['CourseHash'] = course_hash
scene['CourseSourceSha256'] = course_source_hash

M = {}
material_records = []
specs = [
    ('NA_Slate', (.22, .29, .34), .67, .04, 4.0),
    ('NA_Ceramic', (.36, .39, .40), .42, .10, 4.0),
    ('NA_Graphite', (.085, .102, .120), .59, .32, 3.0),
    ('NA_Aluminium', (.26, .30, .34), .36, .78, 2.0),
    ('NA_Glass', (.023, .055, .067), .20, .45, 4.0),
    ('NA_Interior', (.13, .095, .061), .79, 0.0, 4.0),
    ('NA_WarmDiffuser', (.88, .55, .25), .34, 0.0, 4.0),
    ('NA_CoolDiffuser', (.55, .67, .73), .36, 0.0, 4.0),
    ('NA_BronzePanel', (.320, .200, .115), .48, .18, 3.0),
    ('NA_WarningMark', (.55, .34, .035), .40, .12, 2.0),
]

def write_png(path, array):
    h, w, channels = array.shape
    def chunk(kind, data):
        return struct.pack('>I', len(data)) + kind + data + struct.pack('>I', zlib.crc32(kind + data) & 0xffffffff)
    raw = b''.join(b'\0' + row.tobytes() for row in array)
    mode = 6 if channels == 4 else 2
    payload = b'\x89PNG\r\n\x1a\n' + chunk(b'IHDR', struct.pack('>IIBBBBB', w, h, 8, mode, 0, 0, 0))
    payload += chunk(b'IDAT', zlib.compress(raw, 6)) + chunk(b'IEND', b'')
    open(path, 'wb').write(payload)

n = 2048
yy, xx = np.mgrid[0:n, 0:n]
rng = np.random.default_rng(90277)
# Low-amplitude fabricated-surface relief: no baked shadows, bolts, seams or lighting.
height = (.28 * np.sin(xx * math.tau / n * 37) + .11 * np.sin(yy * math.tau / n * 53)
          + .06 * np.sin((xx + yy) * math.tau / n * 131))
dx = (np.roll(height, -1, 1) - np.roll(height, 1, 1)) * .075
dy = (np.roll(height, -1, 0) - np.roll(height, 1, 0)) * .075
normal = np.stack([-dx, -dy, np.ones_like(dx)], -1)
normal /= np.linalg.norm(normal, axis=2)[:, :, None]
write_png(os.path.join(OUT, 'textures/NA_Micro_Normal.png'), np.uint8(np.clip(normal * .5 + .5, 0, 1) * 255))
noise = rng.random((n, n)) - .5
mapped = {'NA_Slate', 'NA_Ceramic', 'NA_Graphite', 'NA_Aluminium', 'NA_BronzePanel'}
for name, color, roughness, metallic, metres_per_tile in specs:
    mat = bpy.data.materials.new(name)
    mat.use_nodes = True
    mat.diffuse_color = (*color, 1)
    bsdf = mat.node_tree.nodes.get('Principled BSDF')
    bsdf.inputs['Base Color'].default_value = (*color, 1)
    bsdf.inputs['Roughness'].default_value = roughness
    bsdf.inputs['Metallic'].default_value = metallic
    emission = .42 if name == 'NA_WarmDiffuser' else .55 if name == 'NA_CoolDiffuser' else 0
    if emission:
        bsdf.inputs['Emission Color'].default_value = (*color, 1)
        bsdf.inputs['Emission Strength'].default_value = emission
    record = {
        'id': name, 'baseColorLinear': list(color), 'roughness': roughness, 'metallic': metallic,
        'emissionColorLinear': list(color) if emission else [0, 0, 0], 'emissionIntensity': emission,
        'metersPerTile': [metres_per_tile, metres_per_tile], 'uvMode': 'metric',
        'normalScale': .22, 'textureScale': [4.0 / metres_per_tile, 4.0 / metres_per_tile]
    }
    if name in mapped:
        color_field = np.array(color)[None, None, :] * (1 + height[:, :, None] * .025 + noise[:, :, None] * .025)
        linear = np.clip(color_field, 0, 1)
        srgb = np.where(linear <= .0031308, linear * 12.92, 1.055 * linear ** (1 / 2.4) - .055)
        base = 'textures/' + name + '_BaseColor.png'
        mask = 'textures/' + name + '_MetallicSmoothness.png'
        write_png(os.path.join(OUT, base), np.uint8(srgb * 255))
        packed = np.zeros((n, n, 4), dtype=np.uint8)
        packed[:, :, 0] = int(metallic * 255)
        packed[:, :, 3] = np.uint8(np.clip(1 - roughness + height * .012 + noise * .01, 0, 1) * 255)
        write_png(os.path.join(OUT, mask), packed)
        record.update(baseColor=base, metallicSmoothness=mask, normal='textures/NA_Micro_Normal.png')
        tex = mat.node_tree.nodes.new('ShaderNodeTexImage')
        tex.image = bpy.data.images.load(os.path.join(OUT, base))
        mat.node_tree.links.new(tex.outputs['Color'], bsdf.inputs['Base Color'])
        tex = mat.node_tree.nodes.new('ShaderNodeTexImage')
        tex.image = bpy.data.images.load(os.path.join(OUT, mask))
        tex.image.colorspace_settings.name = 'Non-Color'
        invert = mat.node_tree.nodes.new('ShaderNodeMath')
        invert.operation = 'SUBTRACT'
        invert.inputs[0].default_value = 1
        mat.node_tree.links.new(tex.outputs['Alpha'], invert.inputs[1])
        mat.node_tree.links.new(invert.outputs[0], bsdf.inputs['Roughness'])
        tex = mat.node_tree.nodes.new('ShaderNodeTexImage')
        tex.image = bpy.data.images.load(os.path.join(OUT, 'textures/NA_Micro_Normal.png'), check_existing=True)
        tex.image.colorspace_settings.name = 'Non-Color'
        node = mat.node_tree.nodes.new('ShaderNodeNormalMap')
        node.inputs['Strength'].default_value = .22
        mat.node_tree.links.new(tex.outputs['Color'], node.inputs['Color'])
        mat.node_tree.links.new(node.outputs['Normal'], bsdf.inputs['Normal'])
    M[name] = mat
    material_records.append(record)

assets = {}
current = None
fixture_locals = {}

def begin(name):
    global current
    current = bpy.data.collections.new(name + '_EDITABLE')
    scene.collection.children.link(current)
    assets[name] = []
    fixture_locals[name] = []

def mesh(name, vertices, faces, material='NA_Slate', bevel=.04, detail=0):
    data = bpy.data.meshes.new(name)
    data.from_pydata(vertices, [], faces)
    data.update()
    bm = bmesh.new()
    bm.from_mesh(data)
    bmesh.ops.recalc_face_normals(bm, faces=list(bm.faces))
    bm.to_mesh(data)
    bm.free()
    obj = bpy.data.objects.new(name, data)
    current.objects.link(obj)
    data.materials.append(M[material])
    obj['lodDetail'] = detail
    assets[current.name.removesuffix('_EDITABLE')].append(obj)
    if bevel:
        mod = obj.modifiers.new('Fabricated edge radius', 'BEVEL')
        mod.width = bevel
        mod.segments = 3
        mod.affect = 'EDGES'
        mod = obj.modifiers.new('Area weighted normals', 'WEIGHTED_NORMAL')
        mod.keep_sharp = True
    return obj

FACES = [(0, 3, 2, 1), (4, 5, 6, 7), (0, 1, 5, 4), (1, 2, 6, 5), (2, 3, 7, 6), (3, 0, 4, 7)]

def box(name, center, size, material='NA_Slate', bevel=.04, detail=0):
    vertices = [(center[0] + x * size[0] / 2, center[1] + y * size[1] / 2, center[2] + z * size[2] / 2)
                for x, y, z in [(-1, -1, -1), (1, -1, -1), (1, 1, -1), (-1, 1, -1),
                                (-1, -1, 1), (1, -1, 1), (1, 1, 1), (-1, 1, 1)]]
    return mesh(name, vertices, FACES, material, bevel, detail)

def profile(name, xz, y, depth, material='NA_Slate', bevel=.04, detail=0):
    count = len(xz)
    vertices = [(x, y + offset, z) for offset in (-depth / 2, depth / 2) for x, z in xz]
    faces = [tuple(range(count - 1, -1, -1)), tuple(range(count, 2 * count))]
    faces += [(i, (i + 1) % count, (i + 1) % count + count, i + count) for i in range(count)]
    return mesh(name, vertices, faces, material, bevel, detail)

def beam(name, a, b, width, depth, material='NA_Aluminium', detail=0):
    obj = box(name, (0, 0, 0), (width, depth, (Vector(b) - Vector(a)).length), material,
              min(width * .12, .05), detail)
    obj.location = (Vector(a) + Vector(b)) / 2
    obj.rotation_euler = (Vector(b) - Vector(a)).to_track_quat('Z', 'Y').to_euler()
    return obj

inner = [(-13.75, .15), (-13.75, 10.45), (-9.85, 14.38), (9.85, 14.38), (13.75, 10.45), (13.75, .15)]
outer = [(-15.15, .15), (-15.15, 11.18), (-10.62, 15.62), (10.62, 15.62), (15.15, 11.18), (15.15, .15)]

def ring(name, y, depth, material='NA_Graphite'):
    vertices = []
    for yy2 in [y - depth / 2, y + depth / 2]:
        vertices.extend([(x, yy2, z) for x, z in inner])
        vertices.extend([(x, yy2, z) for x, z in outer])
    faces = []
    for i in range(5):
        faces.extend([(i, i + 1, 6 + i + 1, 6 + i),
                      (12 + i, 18 + i, 18 + i + 1, 12 + i + 1),
                      (i, 12 + i, 12 + i + 1, i + 1),
                      (6 + i, 6 + i + 1, 18 + i + 1, 18 + i)])
    faces.extend([(0, 6, 18, 12), (5, 17, 23, 11)])
    return mesh(name, vertices, faces, material, .065)

def build_portal():
    name = 'NA_GalleryPortal_G02'
    begin(name)
    ring('Monolithic entry frame', 0, 1.55, 'NA_Graphite')
    ring('Recessed load collar', -1.38, .46, 'NA_Aluminium')
    for sign in (-1, 1):
        box('Integrated bearing plinth', (sign * 14.40, -.22, .72), (1.45, 2.25, 1.15), 'NA_Slate', .09)
        box('Elastomer isolation pad', (sign * 14.40, -.22, 1.35), (1.25, 1.62, .18), 'NA_Graphite', .025)
        box('Closed service spine', (sign * 14.78, -.16, 6.0), (.52, 1.42, 8.15), 'NA_Slate', .045)
        for z in (2.3, 7.0, 10.35):
            box('Bolted rib splice plate', (sign * 14.36, -.84, z), (1.18, .16, .72), 'NA_Aluminium', .025)
            for xoff in (-.33, .33):
                for zoff in (-.19, .19):
                    box('Recessed splice fastener', (sign * (14.36 + xoff), -.94, z + zoff), (.10, .07, .10), 'NA_Graphite', .02, 1)
    box('Entry crown fascia', (0, -.30, 14.88), (20.4, 1.55, 1.28), 'NA_Slate', .07)
    box('Entry crown shadow reveal', (0, -.98, 14.37), (18.8, .22, .30), 'NA_Graphite', .03)
    box('Shielded entry fixture tray', (0, -.80, 14.13), (17.7, .72, .42), 'NA_Graphite', .045)
    box('Entry downward diffuser', (0, -.80, 13.90), (15.9, .38, .07), 'NA_WarmDiffuser', .012)
    fixture_locals[name].append({'sourcePart': 'Entry downward diffuser', 'kind': 'ceiling',
                                 'centerUnityLocal': [0, 13.86, .80], 'aimUnityLocal': [0, -1, .14]})
    for x in (-7.2, 0, 7.2):
        box('Crown load spreader', (x, .54, 15.18), (1.6, .64, .38), 'NA_Aluminium', .035)
    return name

def build_rib():
    name = 'NA_GalleryRib_G02'
    begin(name)
    ring('Deep continuous gallery rib', 0, 1.08, 'NA_Graphite')
    ring('Rib inner wear liner', -.32, .18, 'NA_Aluminium')
    for sign in (-1, 1):
        box('Rib connected foot', (sign * 14.35, 0, .70), (1.35, 1.56, 1.15), 'NA_Slate', .08)
        box('Rib lower armor', (sign * 14.05, -.36, 3.15), (.55, .24, 3.65), 'NA_Aluminium', .035)
        beam('Shoulder compression cleat', (sign * 14.18, -.42, 10.78), (sign * 10.32, -.42, 14.73), .34, .24, 'NA_Aluminium')
    for x in (-6.7, 0, 6.7):
        box('Crown keyed joint', (x, -.42, 15.18), (1.65, .28, .62), 'NA_Aluminium', .03)
    return name

def add_wall_side(sign, length):
    side = 'R' if sign > 0 else 'L'
    ymid = -length / 2
    box(side + ' continuous carrier pan', (sign * 14.83, ymid, 6.18), (.34, length, 10.25), 'NA_Graphite', .045)
    box(side + ' connected lower trunk', (sign * 14.48, ymid, 1.40), (1.25, length, .76), 'NA_Graphite', .06)
    box(side + ' trunk wearing cap', (sign * 14.30, ymid, 1.82), (.70, length, .16), 'NA_Aluminium', .025)
    segments = 2
    seg = length / segments
    for j in range(segments):
        cy = -(j + .5) * seg
        usable = seg - .34
        # A deep formed cassette with a shadowed return; no surface decal substitutes for construction.
        box(side + ' bronze cassette back', (sign * 14.50, cy, 6.63), (.58, usable, 8.05), 'NA_BronzePanel', .07)
        box(side + ' cassette inner face', (sign * 14.22, cy, 6.63), (.14, usable - .20, 7.60), 'NA_BronzePanel', .045)
        for z in (2.76, 10.50):
            box(side + ' horizontal folded return', (sign * 14.32, cy, z), (.36, usable, .23), 'NA_Aluminium', .025)
        # Central maintenance recess is materially and geometrically distinct.
        box(side + ' service recess', (sign * 14.12, cy, 6.74), (.20, usable * .66, 1.78), 'NA_Graphite', .035)
        for z in (6.22, 6.56, 6.90, 7.24):
            box(side + ' captive ventilation blade', (sign * 13.98, cy, z), (.10, usable * .58, .12), 'NA_Aluminium', .018, 1)
        box(side + ' attached ID plaque', (sign * 13.95, cy - usable * .33, 9.56), (.09, .78, .62), 'NA_WarningMark', .025, 1)
    for j in range(segments + 1):
        y = -j * seg
        box(side + ' full-height cassette mullion', (sign * 14.18, y, 6.30), (.62, .28, 9.35), 'NA_Aluminium', .04)
        box(side + ' mullion root shoe', (sign * 14.15, y, 1.26), (1.00, .56, .52), 'NA_Slate', .045)
    # Shoulder tray, liner and seams connect wall cassettes into the retained crown shell.
    sx = lambda x: sign * x
    shoulder_outer = [(sx(14.77), 10.90), (sx(10.45), 15.24), (sx(10.20), 14.99), (sx(14.48), 10.68)]
    shoulder_inner = [(sx(14.39), 11.12), (sx(10.72), 14.82), (sx(10.56), 14.67), (sx(14.23), 10.98)]
    profile(side + ' continuous shoulder tray', shoulder_outer, ymid, length, 'NA_Graphite', .035)
    profile(side + ' warm shoulder liner', shoulder_inner, ymid, length - .18, 'NA_BronzePanel', .04)
    for j in range(segments + 1):
        y = -j * seg
        beam(side + ' shoulder locking seam', (sx(14.43), y, 11.03), (sx(10.58), y, 14.91), .18, .22, 'NA_Aluminium')
    # Shielded linear fixture: housing, visor, and recessed inward-facing diffuser are one assembly.
    fy = -length * .55
    box(side + ' fixture structural bracket', (sign * 14.25, fy, 3.42), (.90, length * .46, .68), 'NA_Graphite', .045)
    box(side + ' fixture glare visor', (sign * 13.95, fy, 3.20), (.40, length * .43, .46), 'NA_Slate', .035)
    box(side + ' inward wall diffuser', (sign * 13.73, fy, 3.12), (.065, length * .38, .20), 'NA_WarmDiffuser', .012)
    fixture_locals[current.name.removesuffix('_EDITABLE')].append({
        'sourcePart': side + ' inward wall diffuser', 'kind': 'wall',
        'centerUnityLocal': [sign * 13.69, 3.12, length * .55],
        'aimUnityLocal': [-sign * .78, -.60, .16]
    })

def build_wall_bay(name, length):
    begin(name)
    add_wall_side(1, length)
    add_wall_side(-1, length)
    return name

def build_ceiling_bay(name, length):
    begin(name)
    ymid = -length / 2
    box('Connected crown service tray', (0, ymid, 15.08), (20.25, length, .62), 'NA_Graphite', .055)
    for x in (-6.65, 0, 6.65):
        box('Acoustic crown panel', (x, ymid, 14.70), (6.25, length - .22, .20), 'NA_BronzePanel', .04)
    for x in (-9.88, -3.32, 3.32, 9.88):
        box('Crown longitudinal folded seam', (x, ymid, 14.58), (.18, length, .24), 'NA_Aluminium', .025)
    for fraction in (0, .5, 1):
        y = -length * fraction
        box('Crown transverse tie', (0, y, 14.86), (20.5, .30, .50), 'NA_Aluminium', .035)
    fy = -length * .58
    box('Ceiling fixture suspension bridge', (0, fy, 14.77), (18.7, 1.42, .48), 'NA_Aluminium', .045)
    box('Ceiling fixture black housing', (0, fy, 14.46), (17.9, 1.04, .46), 'NA_Graphite', .05)
    box('Ceiling fixture glare lip forward', (0, fy - .46, 14.20), (18.15, .16, .34), 'NA_Graphite', .025)
    box('Ceiling fixture glare lip rear', (0, fy + .46, 14.20), (18.15, .16, .34), 'NA_Graphite', .025)
    box('Ceiling downward diffuser', (0, fy, 14.19), (16.35, .52, .075), 'NA_WarmDiffuser', .012)
    fixture_locals[name].append({'sourcePart': 'Ceiling downward diffuser', 'kind': 'ceiling',
                                 'centerUnityLocal': [0, 14.14, length * .58], 'aimUnityLocal': [0, -1, .12]})
    return name

portal_asset = build_portal()
rib_asset = build_rib()
wall_short = build_wall_bay('NA_GalleryWallBay_S_G02', 12.30)
wall_long = build_wall_bay('NA_GalleryWallBay_L_G02', 13.84)
ceiling_short = build_ceiling_bay('NA_GalleryCeilingBay_S_G02', 12.30)
ceiling_long = build_ceiling_bay('NA_GalleryCeilingBay_L_G02', 13.84)

# Runtime construction: authored detail omission and bevel reduction, never blind decimation.
runtime = bpy.data.collections.new('RUNTIME_LODS')
scene.collection.children.link(runtime)
export_objects = {}
previous_stats = json.load(open(os.path.join(PREVIOUS, 'asset-stats.json')))
stats = {key: value for key, value in previous_stats.items()
         if key.startswith('NA_CivicFacade_A_') or key.startswith('NA_ServiceFacade_A_')}
manifest_assets = []

def metric_uv(data):
    for old in list(data.uv_layers):
        data.uv_layers.remove(old)
    uv = data.uv_layers.new(name='UV0_Metric4m')
    for polygon in data.polygons:
        axis = max(range(3), key=lambda k: abs(polygon.normal[k]))
        axes = [k for k in range(3) if k != axis]
        for loop_index in polygon.loop_indices:
            vertex = data.vertices[data.loops[loop_index].vertex_index].co
            uv.data[loop_index].uv = (vertex[axes[0]] / 4, vertex[axes[1]] / 4)
    uv.active_render = True

def inspect(obj):
    data = obj.data
    data.calc_loop_triangles()
    bm = bmesh.new()
    bm.from_mesh(data)
    boundary = sum(edge.is_boundary for edge in bm.edges)
    nonmanifold = sum(not edge.is_manifold for edge in bm.edges)
    unseen = set(bm.faces)
    volumes = []
    while unseen:
        pending = [unseen.pop()]
        component = []
        while pending:
            face = pending.pop()
            component.append(face)
            for edge in face.edges:
                for neighbour in edge.link_faces:
                    if neighbour in unseen:
                        unseen.remove(neighbour)
                        pending.append(neighbour)
        volume = 0
        for face in component:
            for j in range(1, len(face.verts) - 1):
                volume += face.verts[0].co.dot(face.verts[j].co.cross(face.verts[j + 1].co)) / 6
        volumes.append(volume)
    bm.free()
    coords = np.array([list(vertex.co) for vertex in data.vertices])
    uvs = np.array([list(loop.uv) for loop in data.uv_layers.active.data])
    duplicate_count = 0
    seen = set()
    for polygon in data.polygons:
        key = tuple(sorted(tuple(round(float(v), 6) for v in data.vertices[index].co) for index in polygon.vertices))
        duplicate_count += key in seen
        seen.add(key)
    zero_uv = 0
    for polygon in data.polygons:
        a, b, c = [uvs[i] for i in polygon.loop_indices]
        ab, ac = b - a, c - a
        zero_uv += abs(ab[0] * ac[1] - ab[1] * ac[0]) < 1e-12
    low, high = coords.min(0).tolist(), coords.max(0).tolist()
    result = {
        'vertices': len(data.vertices), 'triangles': len(data.loop_triangles),
        'sourceBoundsMin': low, 'sourceBoundsMax': high,
        'unityBoundsMin': [low[0], low[2], -high[1]], 'unityBoundsMax': [high[0], high[2], -low[1]],
        'boundaryEdges': boundary, 'nonmanifoldEdges': nonmanifold,
        'degenerateTriangles': sum(p.area < 1e-10 for p in data.polygons),
        'duplicateTriangles': int(duplicate_count), 'zeroAreaUVTriangles': int(zero_uv),
        'closedComponents': len(volumes), 'nonpositiveComponents': sum(v <= 0 for v in volumes),
        'finiteUV': bool(np.isfinite(uvs).all()), 'materialSlots': [m.name for m in data.materials]
    }
    assert result['nonmanifoldEdges'] == result['degenerateTriangles'] == result['duplicateTriangles'] == 0, result
    assert result['zeroAreaUVTriangles'] == result['nonpositiveComponents'] == 0 and result['finiteUV'], result
    return result

for name, parts in assets.items():
    for lod in (0, 1):
        copies = []
        for source in parts:
            if lod and source['lodDetail'] > 0:
                continue
            copy = source.copy()
            copy.data = source.data.copy()
            runtime.objects.link(copy)
            copies.append(copy)
            bpy.ops.object.select_all(action='DESELECT')
            copy.select_set(True)
            bpy.context.view_layer.objects.active = copy
            for modifier in list(copy.modifiers):
                if modifier.type == 'BEVEL' and lod:
                    modifier.segments = 1
                bpy.ops.object.modifier_apply(modifier=modifier.name)
            bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
        bpy.ops.object.select_all(action='DESELECT')
        for copy in copies:
            copy.select_set(True)
        bpy.context.view_layer.objects.active = copies[0]
        bpy.ops.object.join()
        obj = bpy.context.object
        obj.name = name + '_LOD' + str(lod)
        unique, remap = [], {}
        for i, material in enumerate(obj.data.materials):
            if material not in unique:
                unique.append(material)
            remap[i] = unique.index(material)
        ids = [remap[p.material_index] for p in obj.data.polygons]
        obj.data.materials.clear()
        for material in unique:
            obj.data.materials.append(material)
        for polygon, index in zip(obj.data.polygons, ids):
            polygon.material_index = index
        tri = obj.modifiers.new('Export triangles', 'TRIANGULATE')
        bpy.ops.object.modifier_apply(modifier=tri.name)
        metric_uv(obj.data)
        obj.data.calc_tangents()
        result = inspect(obj)
        result['sourceParts'] = len(copies)
        relative = 'meshes/' + obj.name + '.fbx'
        bpy.ops.object.select_all(action='DESELECT')
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, relative), use_selection=True, object_types={'MESH'},
            apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS', bake_space_transform=True,
            axis_forward='-Z', axis_up='Y', mesh_smooth_type='FACE', use_tspace=True,
            add_leaf_bones=False, path_mode='STRIP', use_custom_props=False)
        result['fbxSha256'] = hashlib.sha256(open(os.path.join(OUT, relative), 'rb').read()).hexdigest()
        stats[obj.name] = result
        export_objects[obj.name] = obj
        print('EXPORTED', obj.name, result['triangles'], flush=True)
        if lod == 0:
            manifest_assets.append({
                'id': name, 'lod0': relative, 'lod1': 'meshes/' + name + '_LOD1.fbx',
                'pivot': 'route center at bay start; source +X right, -Y forward, +Z up',
                'boundsMin': result['unityBoundsMin'], 'boundsMax': result['unityBoundsMax'],
                'materialSlots': result['materialSlots'], 'lightmapUVs': 'generate',
                'collider': 'none', 'trackAssembly': False
            })
        obj.hide_set(True)
        obj.hide_render = True

# Preserve opening payload bytes and manifest geometry exactly; only course-frame provenance is refreshed.
old_manifest = json.load(open(os.path.join(PREVIOUS, 'manifest.json')))
for asset_id in ('NA_CivicFacade_A', 'NA_ServiceFacade_A'):
    old_asset = next(a for a in old_manifest['assets'] if a['id'] == asset_id)
    manifest_assets.insert(0 if asset_id == 'NA_CivicFacade_A' else 1, old_asset)
    for lod in (0, 1):
        filename = asset_id + '_LOD' + str(lod) + '.fbx'
        shutil.copy2(os.path.join(PREVIOUS, 'meshes', filename), os.path.join(OUT, 'meshes', filename))

for name in assets:
    collection = bpy.data.collections[name + '_EDITABLE']
    collection.hide_viewport = True
    collection.hide_render = True
    for obj in assets[name]:
        metric_uv(obj.data)
for image in bpy.data.images:
    if image.source == 'FILE':
        image.filepath = '//../textures/' + os.path.basename(image.filepath)

instances = []
def place(identifier, asset, index, lateral=0, vertical=0, mode='banked', flip=False):
    frame = course['frames'][index]
    right, up, forward = Vector(frame['right']), Vector(frame['up']), Vector(frame['forward'])
    position = Vector(frame['position']) + right * lateral + up * vertical
    if mode == 'upright':
        forward = Vector((forward.x, 0, forward.z)).normalized()
        up = Vector((0, 1, 0))
        right = up.cross(forward).normalized()
    if flip:
        right, forward = -right, -forward
    q = Matrix((right, up, forward)).transposed().to_quaternion().normalized()
    instances.append({
        'id': identifier, 'assetId': asset, 'zoneId': 'viaduct' if index < 200 else 'station',
        'position': list(position), 'rotation': [q.x, q.y, q.z, q.w], 'scale': [1, 1, 1],
        'frameMode': mode, 'role': 'near', 'gi': 'probe', 'frameIndex': index,
        'sourceFrame': frame, 'offsetLateral': lateral, 'offsetVertical': vertical, 'flipAroundUp': flip
    })

place('opening-civic-right', 'NA_CivicFacade_A', 95, 23, -1.5, 'upright')
place('opening-service-left', 'NA_ServiceFacade_A', 120, -22, -2, 'upright', True)
place('warm-entry-portal', portal_asset, STATIONS[0])
place('warm-rib-01', rib_asset, STATIONS[1])
place('warm-rib-02', rib_asset, STATIONS[2])
place('warm-rib-03', rib_asset, STATIONS[3])
place('warm-rib-04', rib_asset, STATIONS[4])
place('warm-rib-05', rib_asset, STATIONS[5])
place('warm-exit-portal', portal_asset, STATIONS[6], flip=True)
span_assets = [
    (wall_short, ceiling_short), (wall_long, ceiling_long), (wall_short, ceiling_short),
    (wall_long, ceiling_long), (wall_short, ceiling_short), (wall_short, ceiling_short)
]
for sequence, (index, pair) in enumerate(zip(STATIONS[:-1], span_assets), 1):
    place('warm-wall-span-%02d' % sequence, pair[0], index)
    place('warm-ceiling-span-%02d' % sequence, pair[1], index)

replacement_renderers = [
    'Warm gallery primary portal structure', 'Warm gallery recessed ceramic cassettes',
    'Warm gallery folded service returns', 'Warm gallery light and maintenance housings',
    'Warm gallery integrated diffusers', 'Warm gallery maintenance markings'
]
replacement_lights = ['Warm gallery concealed surface wash', 'Warm gallery road pool']
layout = {
    'schema': 'aaa-nocturne-exemplar-1', 'revision': REVISION, 'courseHash': course_hash,
    'layoutComplete': True, 'status': 'BLOCKED_VISUAL_REVIEW',
    'gallerySpan': {'startProgress': .86, 'endProgress': .902},
    'replacement': {'mode': 'warm-details-and-local-lights',
                    'rendererNames': replacement_renderers, 'lightNames': replacement_lights},
    'instances': instances
}
json.dump(layout, open(os.path.join(OUT, 'layout.json'), 'w'), indent=2)

def world_transform(instance, local):
    x, y, z, w = instance['rotation']
    rotation = Quaternion((w, x, y, z)).to_matrix()
    return Vector(instance['position']) + rotation @ Vector(local)

lighting = []
sockets = []
for instance in instances:
    if instance['zoneId'] != 'station':
        continue
    for socket_index, socket in enumerate(fixture_locals.get(instance['assetId'], []), 1):
        position = world_transform(instance, socket['centerUnityLocal'])
        direction = (world_transform(instance, Vector(socket['centerUnityLocal']) + Vector(socket['aimUnityLocal'])) - position).normalized()
        is_ceiling = socket['kind'] == 'ceiling'
        suffix = 'ceiling' if is_ceiling else ('left-wall' if socket['centerUnityLocal'][0] < 0 else 'right-wall')
        fixture_id = instance['id'] + '-' + suffix + '-%02d' % socket_index
        fixture = {
            'id': fixture_id, 'type': 'spot' if is_ceiling else 'point',
            'position': list(position), 'rgb': [1.0, .67, .36],
            'intensity': 500.0 if is_ceiling else 120.0,
            'range': 25.0 if is_ceiling else 8.5,
            'shadows': 'soft' if is_ceiling else 'none',
            'sourcePlacement': instance['id'], 'sourcePart': socket['sourcePart']
        }
        if is_ceiling:
            fixture.update(aimDirection=list(direction), outerAngle=76.0, innerAngle=36.0)
        lighting.append(fixture)
        sockets.append({'assetId': instance['assetId'], 'placementId': instance['id'], **socket,
                        'worldPosition': list(position), 'worldAimDirection': list(direction)})
json.dump({'schema': 'aaa-nocturne-gallery-lighting-1', 'courseHash': course_hash, 'fixtures': lighting},
          open(os.path.join(OUT, 'gallery-lighting.json'), 'w'), indent=2)
json.dump({'fixtures': sockets, 'runtimeLightsAuthored': False,
           'note': 'Geometry-derived sockets; optional native suggestions are in gallery-lighting.json.'},
          open(os.path.join(OUT, 'fixture-sockets.json'), 'w'), indent=2)

manifest = {
    'schema': 'aaa-nocturne-exemplar-1', 'revision': REVISION, 'courseHash': course_hash,
    'courseSource': {'path': 'course-data.json', 'sha256': course_source_hash},
    'layoutComplete': True, 'visualStatus': 'BLOCKED_VISUAL_REVIEW',
    'sourceBlend': 'source/Nocturne_Gallery_Exemplar_02.blend',
    'galleryLighting': 'gallery-lighting.json', 'lights': [],
    'assets': manifest_assets, 'materials': material_records,
    'exportConvention': {'axis_forward': '-Z', 'axis_up': 'Y', 'bake_space_transform': True,
                         'apply_scale_options': 'FBX_SCALE_UNITS'},
    'sourceToUnity': '(x,z,-y)', 'unit': 'metre',
    'normals': 'authored weighted split normals; FBX includes tangents',
    'lightmapUVs': 'Generate in Unity; UV0 deliberately overlaps metric tiles',
    'replacementScope': 'Warm detail renderer groups and scoped warm lights only; retained continuous shell excluded.',
    'revisionNote': 'Native lighting correction: brighter graphite and bronze response plus short-range point washes at wall diffusers.'
}
json.dump(manifest, open(os.path.join(OUT, 'manifest.json'), 'w'), indent=2)
json.dump(stats, open(os.path.join(OUT, 'asset-stats.json'), 'w'), indent=2)

# Build a straight studio assembly from the same editable parts. Studio lights/camera never enter FBX.
studio = bpy.data.collections.new('STUDIO_REVIEW_ONLY')
scene.collection.children.link(studio)
def studio_copy(asset, y_offset):
    for source in assets[asset]:
        copy = source.copy()
        copy.data = source.data
        copy.location = source.location + Vector((0, y_offset, 0))
        copy.rotation_euler = source.rotation_euler
        studio.objects.link(copy)
distances = [course['frames'][STATIONS[i + 1]]['distanceMeters'] - course['frames'][STATIONS[i]]['distanceMeters'] for i in range(6)]
offsets = [0]
for distance in distances:
    offsets.append(offsets[-1] - distance)
studio_copy(portal_asset, offsets[0])
for i in range(1, 6):
    studio_copy(rib_asset, offsets[i])
studio_copy(portal_asset, offsets[6])
for i, (wall, ceiling) in enumerate(span_assets):
    studio_copy(wall, offsets[i])
    studio_copy(ceiling, offsets[i])
floor_mat = bpy.data.materials.new('Studio wet road')
floor_mat.use_nodes = True
floor_bsdf = floor_mat.node_tree.nodes.get('Principled BSDF')
floor_bsdf.inputs['Base Color'].default_value = (.025, .03, .038, 1)
floor_bsdf.inputs['Metallic'].default_value = .2
floor_bsdf.inputs['Roughness'].default_value = .24
floor = box('Studio road', (0, offsets[-1] / 2, -.18), (26.0, abs(offsets[-1]) + 16, .30), 'NA_Graphite', .02)
floor.data.materials[0] = floor_mat
current.objects.unlink(floor)
studio.objects.link(floor)
for y in [offsets[i] - distances[i] * .58 for i in range(6)]:
    data = bpy.data.lights.new('Studio warm pool', 'AREA')
    data.energy = 2200
    data.color = (1.0, .50, .20)
    data.shape = 'RECTANGLE'
    data.size = 12
    data.size_y = 1.2
    obj = bpy.data.objects.new('Studio warm pool', data)
    studio.objects.link(obj)
    obj.location = (0, y, 14.0)
    obj.rotation_euler = (0, 0, 0)
cool = bpy.data.lights.new('Studio cool exit', 'AREA')
cool.energy = 1100
cool.color = (.08, .36, 1.0)
cool.shape = 'RECTANGLE'
cool.size = 18
cool.size_y = 8
cool_obj = bpy.data.objects.new('Studio cool exit', cool)
studio.objects.link(cool_obj)
cool_obj.location = (0, offsets[-1] - 3, 8)
cool_obj.rotation_euler = (math.radians(90), 0, 0)
camera_data = bpy.data.cameras.new('Studio gallery camera')
camera = bpy.data.objects.new('Studio gallery camera', camera_data)
studio.objects.link(camera)
camera.location = (0, 8.5, 4.6)
target = Vector((0, -28, 5.1))
camera.rotation_euler = (target - camera.location).to_track_quat('-Z', 'Y').to_euler()
camera_data.lens = 38
camera_data.sensor_width = 36
scene.camera = camera
world = bpy.data.worlds.new('Studio world') if not scene.world else scene.world
scene.world = world
world.use_nodes = True
world.node_tree.nodes['Background'].inputs['Color'].default_value = (.0025, .006, .012, 1)
world.node_tree.nodes['Background'].inputs['Strength'].default_value = .16
scene.render.engine = 'CYCLES'
scene.cycles.device = 'CPU'
scene.cycles.samples = 24
scene.cycles.use_denoising = False
scene.render.resolution_x = 960
scene.render.resolution_y = 540
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = 'PNG'
scene.render.filepath = os.path.join(OUT, 'checks/studio-gallery.png')
scene.render.film_transparent = False
scene.view_settings.look = 'AgX - Medium High Contrast'
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, 'source/Nocturne_Gallery_Exemplar_02.blend'))
bpy.ops.render.render(write_still=True)
print('STUDIO_RENDER_COMPLETE', scene.render.filepath, flush=True)

# FBX round trip for both preserved and newly authored payloads.
verify = bpy.data.scenes.new('FBX_REIMPORT_CHECK')
bpy.context.window.scene = verify
roundtrips = {}
clearance_objects = {}
for asset in manifest_assets:
    for lod_key in ('lod0', 'lod1'):
        relative = asset[lod_key]
        key = os.path.splitext(os.path.basename(relative))[0]
        before = set(bpy.data.objects)
        bpy.ops.import_scene.fbx(filepath=os.path.join(OUT, relative), use_custom_normals=True)
        imported = [obj for obj in set(bpy.data.objects) - before if obj.type == 'MESH']
        assert len(imported) == 1, (key, len(imported))
        obj = imported[0]
        data = obj.data
        data.calc_loop_triangles()
        coords = np.array([list(obj.matrix_world @ vertex.co) for vertex in data.vertices])
        expected = stats[key]
        error = float(max(abs(coords.min(0) - np.array(expected['sourceBoundsMin'])).max(),
                          abs(coords.max(0) - np.array(expected['sourceBoundsMax'])).max()))
        materials = [mat.name.split('.')[0] for mat in data.materials]
        assert error < .0002 and len(data.loop_triangles) == expected['triangles'], (key, error, len(data.loop_triangles))
        assert set(materials) == set(expected['materialSlots']), (key, materials, expected['materialSlots'])
        assert data.uv_layers.active and all(math.isfinite(x) for normal in data.corner_normals for x in normal.vector)
        roundtrips[key] = {'boundsMaxErrorMeters': error, 'trianglesMatch': True,
                           'materialSlotsMatch': True, 'uvLayerPresent': True,
                           'finiteCornerNormals': True, 'reimportObjectCount': 1}
        if lod_key == 'lod0':
            clearance_objects[asset['id']] = obj
        else:
            bpy.data.objects.remove(obj, do_unlink=True)
json.dump(roundtrips, open(os.path.join(OUT, 'checks/fbx-roundtrip.json'), 'w'), indent=2)

# All-branch static clearance against the protected road/craft envelope.
road_vertices, road_faces = [], []
for i in range(len(course['frames']) - 1):
    base = len(road_vertices)
    for frame in (course['frames'][i], course['frames'][i + 1]):
        position, right, up = np.array(frame['position']), np.array(frame['right']), np.array(frame['up'])
        for lateral, vertical in [(-12.2, -.05), (12.2, -.05), (12.2, 8), (-12.2, 8)]:
            road_vertices.append(tuple(position + right * lateral + up * vertical))
    for face in FACES:
        road_faces.extend([(base + face[0], base + face[1], base + face[2]),
                           (base + face[0], base + face[2], base + face[3])])
road_bvh = BVHTree.FromPolygons(road_vertices, road_faces, all_triangles=True)
frames = course['frames']
frame_positions = np.array([f['position'] for f in frames])
frame_right = np.array([f['right'] for f in frames])
frame_up = np.array([f['up'] for f in frames])
frame_forward = np.array([f['forward'] for f in frames])
clearance = {}
for instance in instances:
    obj = clearance_objects[instance['assetId']]
    source = np.array([list(obj.matrix_world @ vertex.co) for vertex in obj.data.vertices])
    unity = source[:, [0, 2, 1]].copy()
    unity[:, 2] *= -1
    x, y, z, w = instance['rotation']
    rotation = np.array(Quaternion((w, x, y, z)).to_matrix())
    world_vertices = unity @ rotation.T + np.array(instance['position'])
    faces = [tuple(p.vertices) for p in obj.data.polygons]
    overlap = BVHTree.FromPolygons(world_vertices.tolist(), faces, all_triangles=True).overlap(road_bvh)
    contained, min_margin = 0, 1e9
    for fi in range(len(frames) - 1):
        delta = world_vertices - frame_positions[fi]
        along = delta @ frame_forward[fi]
        near = abs(along) <= 15.0
        if not near.any():
            continue
        sample = delta[near]
        lateral = sample @ frame_right[fi]
        vertical = sample @ frame_up[fi]
        contained += int(((abs(lateral) < 12.2) & (vertical > -.05) & (vertical < 8)).sum())
        in_height = (vertical > -.05) & (vertical < 8)
        if in_height.any():
            min_margin = min(min_margin, float((abs(lateral[in_height]) - 12.2).min()))
    clearance[instance['id']] = {
        'surfaceTriangleIntersections': len(overlap),
        'verticesInsideProtectedCorridorSamples': contained,
        'minLateralMarginWithinProtectedHeightMeters': None if min_margin == 1e9 else min_margin,
        'method': 'BVH against all 1200 closed corridor cells plus every-frame vertex containment; 15m longitudinal candidate window.'
    }
json.dump(clearance, open(os.path.join(OUT, 'checks/placement-clearance.json'), 'w'), indent=2)
print('CLEARANCE', json.dumps(clearance), flush=True)
print('COMPLETE_GALLERY02_AUTHORING', flush=True)
