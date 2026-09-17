"""Original image assets, reproducible package assembly and stable Unity metadata.
Run with system Python (Pillow, numpy, fontTools). No Unity code is generated.
  python3 SourceAssets/Experience/scripts/package_art.py prepare a01
  blender --background --python .../build_city.py -- a01
  python3 SourceAssets/Experience/scripts/package_art.py publish a01
Immutable revisions refuse edits after READY.json is present.
"""
from pathlib import Path
import argparse, hashlib, json, math, shutil, uuid
import numpy as np
from PIL import Image, ImageDraw, ImageFont
from fontTools.ttLib import TTFont

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parents[1]
PAYLOAD = REPO / "UnityProject/Assets/Resources/ExperienceArt"
HASH = "598718f08e5c308a41b8e9f1b562ff8cd4e2bc9f661a91f4e34164cf7c820059"
MATERIALS = {
    # Colours below are linear physical base reflectance, not UI hex colours.
    "slate": ([.025, .041, .057], .10, .58, [0, 0, 0], 0),
    "mineral": ([.30, .36, .39], .04, .67, [0, 0, 0], 0),
    "titanium": ([.18, .23, .27], .78, .33, [0, 0, 0], 0),
    "glass": ([.015, .047, .070], .32, .23, [0, 0, 0], 0),
    "occupied": ([.045, .058, .066], .16, .38, [1, .48, .16], 1.25),
    "officeglass": ([.020, .049, .074], .24, .26, [.34, .62, .80], .72),
    "hotelglass": ([.026, .044, .060], .18, .31, [1, .55, .22], 1.0),
    "serviceglass": ([.031, .049, .056], .12, .43, [.66, .77, .64], .65),
    "cyan": ([.025, .15, .19], .08, .31, [.035, .65, 1], 3.2),
    "magenta": ([.17, .026, .078], .04, .40, [.85, .025, .21], 1.6),
    "white": ([.45, .55, .60], .05, .32, [.5, .8, 1], 1.2),
}

def dump(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + "\n")

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def srgb(x):
    x = np.clip(x, 0, 1)
    return np.where(x <= .0031308, x * 12.92, 1.055 * x ** (1 / 2.4) - .055)

def png(path, arr):
    path.parent.mkdir(parents=True, exist_ok=True)
    Image.fromarray(np.uint8(np.clip(arr, 0, 1) * 255 + .5)).save(path)

def font(size, display=False):
    name = "Rajdhani-SemiBold.ttf" if display else "Barlow-Medium.ttf"
    return ImageFont.truetype(str(REPO / "UnityProject/Assets/Resources/Fonts" / name), size)

def materials(package):
    size = 256
    y, x = np.mgrid[:size, :size] / size
    rng = np.random.default_rng(1926)
    grain = rng.normal(0, .012, (size, size))
    records = []
    for key, (base, metal, rough, colour, power) in MATERIALS.items():
        ident = "vrx_mat_" + key + "_a"
        prefix = "textures/" + ident
        seam = ((x < .009) | (y < .009)).astype(float)
        # Subtle tonal variation and formed panel seam, never painted-on damage.
        variation = grain + .015 * np.sin(2 * np.pi * x) - .12 * seam
        albedo = np.array(base)[None, None, :] * (1 + variation[:, :, None])
        emission = np.ones((size, size, 3))
        if key in ("occupied", "officeglass", "hotelglass", "serviceglass"):
            # Coarse occupied rooms with dark partitions, grouped occupancy.
            gx, gy = (2,1) if key=="hotelglass" else (1,1)
            gridx = (x * gx).astype(int)
            gridy = (y * gy).astype(int)
            room = ((x * gx) % 1 > .10) & ((x * gx) % 1 < .87)
            room &= ((y * gy) % 1 > .18) & ((y * gy) % 1 < (.78 if key!="officeglass" else .70))
            lit = room
            if key=="hotelglass":lit &= gridx==0
            if key=="serviceglass":lit &= x<.62
            emission[:] = lit[:, :, None] * .48
            albedo += lit[:, :, None] * np.array([.055, .04, .02])
        packed = np.zeros((size, size, 4))
        packed[:, :, :3] = metal
        packed[:, :, 3] = np.clip(1 - rough - grain * .8 - seam * .07, 0, 1)
        normal = np.zeros((size, size, 3)) + np.array([.5, .5, 1])
        normal[:, :, 0] += .028 * np.sin(2 * np.pi * x * 4)
        png(package / (prefix + "_base.png"), srgb(albedo))
        png(package / (prefix + "_normal.png"), normal)
        png(package / (prefix + "_ms.png"), packed)
        png(package / (prefix + "_ao.png"), np.ones((size, size, 3)))
        if power:
            png(package / (prefix + "_emission.png"), srgb(emission))
        records.append({
            "id": ident, "baseColor": prefix + "_base.png",
            "normal": prefix + "_normal.png", "metallicSmoothness": prefix + "_ms.png",
            "occlusion": prefix + "_ao.png", "emission": prefix + "_emission.png" if power else "",
            "metersPerTile": [4, 4], "uvMode": "metric", "normalScale": .35,
            "emissionColorLinear": colour, "emissionIntensity": power,
            "artisticMetallic": metal, "artisticRoughness": rough,
            "baseReflectanceLinear": base,
        })
    dump(package / "material-records.json", records)
    return records

def atlas(package, name, kind, frames=16):
    n = 256
    rows, cols = math.ceil(frames / 4), 4
    img = np.zeros((rows * n, cols * n, 4))
    v, u = np.mgrid[:n, :n] / (n - 1)
    t = (u - .04) / .92
    h = (v - .5) * 2
    valid = (t >= 0) & (t <= 1)
    all_alpha = []
    for frame in range(frames):
        phase = frame / frames * math.tau
        if kind in ("body", "core", "wake"):
            advect = 14 * t - phase
            bend = (.025 + .065 * t) * np.sin(advect) + .035 * t * np.sin(25 * t - 2 * phase)
            width = .14 + .29 * np.sin(np.clip(t, 0, 1) * np.pi) ** .7
            width *= 1 + .16 * np.sin(18 * t - phase)
            distance = (h - bend) / np.maximum(width, .03)
            mass = np.exp(-distance ** 2 * 2)
            breakup = .70 + .18 * np.sin(advect + 5 * h) + .10 * np.cos(28 * t - 2 * phase - 8 * h)
            alpha = mass * breakup * (1 - np.clip(t, 0, 1)) ** .60
            core = np.exp(-((h - bend * .25) / (.050 + .025 * np.cos(advect))) ** 2)
            core *= np.exp(-np.maximum(t, 0) * 6)
            if kind == "core":
                alpha = core * .90 + mass * np.exp(-np.maximum(t, 0) * 7) * .15
            elif kind == "wake":
                alpha *= .36 * np.clip(t * 4, 0, 1)
                core *= 0
            rgb = np.zeros((n, n, 3)) + np.array([.028, .23, .92])
            rgb += core[:, :, None] * np.array([.62, .65, .08])
            alpha *= valid * np.clip(t / .025, 0, 1) * np.clip((1 - t) / .08, 0, 1)
        elif kind == "impact":
            age = frame / (frames - 1)
            radius = np.sqrt(((u - .5) * 2) ** 2 + h ** 2)
            theta = np.arctan2(h, (u - .5) * 2)
            burst = np.exp(-(radius / (.10 + .7 * age)) ** 2 * 2)
            rays = np.maximum(0, np.cos(theta * 7 + radius * 4)) ** 16
            alpha = (burst + rays * burst * 1.8) * (1 - age) ** 1.8
            rgb = np.zeros((n, n, 3)) + np.array([1, .42 + .40 * (1 - age), .11 + .45 * (1 - age)])
        else:
            width = .02 + .035 * np.sin(frame * 2.3) ** 2
            alpha = np.exp(-(h / width) ** 2) * np.sin(np.clip(t, 0, 1) * np.pi) ** .7 * valid
            rgb = np.zeros((n, n, 3)) + np.array([1, .61, .17])
        tile = np.dstack((np.clip(rgb, 0, 1), np.clip(alpha, 0, 1)))
        tile[:4, :, 3] = tile[-4:, :, 3] = tile[:, :4, 3] = tile[:, -4:, 3] = 0
        row, col = divmod(frame, cols)
        img[row*n:(row+1)*n, col*n:(col+1)*n] = tile
        all_alpha.append(tile[:, :, 3])
    path = "vfx/" + name + ".png"
    png(package / path, img)
    # Visible animation preview on near-black, same pixels/phase as exported.
    previews = []
    for frame in range(frames):
        row, col = divmod(frame, cols)
        tile = img[row*n:(row+1)*n, col*n:(col+1)*n]
        rgb = tile[:, :, :3] * tile[:, :, 3:4] + np.array([.006, .012, .021])
        previews.append(Image.fromarray(np.uint8(srgb(rgb) * 255)))
    previews[0].save(ROOT / "evidence" / (name + ".gif"),
                     save_all=True, append_images=previews[1:], duration=42,
                     loop=0 if kind != "impact" else 1)
    diffs = [float(np.mean(np.abs(all_alpha[(i+1) % frames] - all_alpha[i]))) for i in range(frames)]
    return {
        "id": name, "path": path, "type": "vfx-atlas", "revision": 1,
        "source": "Original deterministic periodic field; scripts/package_art.py",
        "license": "Project-authored; no third-party content",
        "import": {"colorSpace": "linear", "sRGB": False, "alpha": "straight",
                   "mipmaps": False, "wrap": "clamp", "filter": "bilinear",
                   "compression": "uncompressed", "maxSize": max(img.shape[:2])},
        "atlas": {"grid": [cols, rows], "frameCount": frames, "fps": 24,
                  "frameOrder": "left-to-right, top-to-bottom",
                  "uvOrigin": "bottom-left; rowUV = rows - 1 - floor(frame / cols)",
                  "frameInsetPixels": 4, "channels": "RGB linear emitted colour; A coverage",
                  "longitudinalAxis": "+U nozzle-to-tail", "loop": kind in ("body", "core", "wake"),
                  "playback": "random static tile per spark" if kind == "spark" else "sequential"},
        "validation": {"adjacentAlphaMAE": diffs, "wrapAlphaMAE": diffs[-1],
                       "claim": "Numerical continuity only; native moving appearance pending"},
    }

def fonts(package):
    records = []
    original = REPO / "UnityProject/Assets/Resources/Fonts"
    for key, name, license_name, role, px in [
        ("display", "Rajdhani-SemiBold.ttf", "Rajdhani-OFL.txt", "speed/rank/title", [84, 70, 64]),
        ("text", "Barlow-Medium.ttf", "OFL-Barlow.txt", "menu/body/labels", [28, 22, 20]),
    ]:
        ident = "vrx_font_" + key + "_a"
        folder = package / "fonts"; folder.mkdir(exist_ok=True)
        shutil.copy2(original / name, folder / (ident + ".ttf"))
        shutil.copy2(original / license_name, folder / (ident + "_OFL.txt"))
        f = TTFont(original / name)
        cmap = set(f.getBestCmap())
        names = [r.toUnicode() for r in f["name"].names if r.nameID in [0, 8, 9, 11]]
        needed = set(map(ord, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz /:%+-."))
        records.append({
            "id": ident, "path": "fonts/" + ident + ".ttf", "type": "font",
            "revision": 1, "source": "Retained repository font: Resources/Fonts/" + name,
            "license": "SIL Open Font License 1.1", "licensePath": "fonts/" + ident + "_OFL.txt",
            "provenanceNames": sorted(set(names)), "weight": 600 if key == "display" else 500,
            "glyphCount": len(cmap), "requiredHUDGlyphsMissing": sorted(needed - cmap),
            "coverage": {"basicLatin": all(c in cmap for c in range(32,127)),
                         "CJK": all(c in cmap for c in [0x4E00, 0x4E2D, 0x6587]),
                         "note": "No CJK coverage promised; keep existing fallback for other scripts."},
            "intendedRole": role, "referencePixelsAt1080p": px,
            "import": {"fontRenderingMode": "dynamic", "fontSize": 32},
        })
        f.close()
    return records

def graphics(package):
    records = []
    specs = {
        "panel": (640, 240), "selection": (640, 104),
        "boost_track": (512, 64), "boost_fill": (512, 64),
        "chevron": (128, 128), "collision": (128, 128),
        "rank_up": (128, 128), "rank_down": (128, 128),
        "wordmark": (1024, 180), "advert": (1024, 512),
    }
    for key, (w, h) in specs.items():
        im = Image.new("RGBA", (w, h))
        d = ImageDraw.Draw(im)
        c, pale, ink = "#58D5FF", "#DCEEF5", "#091724"
        if key in ("panel", "selection"):
            cut = 26 if key == "panel" else 18
            p = [(0,0),(w-cut,0),(w,cut),(w,h),(cut,h),(0,h-cut)]
            d.polygon(p, fill=(8,18,29,225 if key == "panel" else 240))
            d.line(p+[p[0]], fill=(109,153,172,100), width=2)
            d.polygon([(0,0),(7,0),(7,h-26),(0,h-33)], fill=c)
            if key == "selection":
                d.polygon([(0,0),(w-cut,0),(w,cut),(w,28),(18,28),(0,10)], fill=(31,135,177,125))
        elif key.startswith("boost_"):
            for i in range(16):
                x = i * 32
                p = [(x+12,9),(x+29,9),(x+17,h-9),(x,h-9)]
                d.polygon(p, fill=c if key.endswith("fill") else (85,127,146,70))
        elif key == "chevron":
            d.polygon([(21,19),(48,19),(99,64),(48,109),(21,109),(70,64)], fill=c)
        elif key == "collision":
            d.polygon([(20,8),(108,8),(120,20),(120,108),(108,120),(20,120),(8,108),(8,20)], outline="#FFB057", width=6)
            d.polygon([(59,27),(80,27),(69,60),(88,60),(43,104),(56,70),(39,70)], fill="#FFB057")
        elif key.startswith("rank"):
            p = [(19,86),(64,37),(109,86),(109,59),(64,10),(19,59)]
            if key == "rank_down": p = [(x,128-y) for x,y in p]
            d.polygon(p, fill=c if key == "rank_up" else "#FFB057")
        elif key == "wordmark":
            d.text((8,-5), "VECTOR RUSH", font=font(132,True), fill=pale)
            d.polygon([(12,153),(908,153),(930,166),(12,166)], fill=c)
        elif key == "advert":
            d.rectangle((0,0,w,h), fill=ink)
            d.rectangle((40,40,48,h-40), fill="#D94487")
            d.text((88,42),"MERIDIAN", font=font(145,True), fill=pale)
            d.text((95,190),"AFTERHOURS",font=font(86,True),fill="#D94487")
            d.text((98,360),"CITY TRANSIT  /  ALL NIGHT",font=font(34),fill=c)
            for i in range(7): d.line((730+i*27,352,700+i*27,440),fill="#D94487",width=9)
        ident = "vrx_ui_" + key + "_a"
        path = "ui/" + ident + ".png"
        (package / "ui").mkdir(exist_ok=True); im.save(package / path)
        records.append({"id": ident, "path": path, "type": "ui-graphic", "revision": 1,
                        "source": "Original vector primitives and bundled OFL fonts; package_art.py",
                        "license": "Project-authored; font licences in fonts/",
                        "dimensions": [w,h],
                        "import": {"colorSpace": "sRGB", "alpha": "straight", "mipmaps": False,
                                   "wrap": "clamp", "filter": "bilinear", "compression": "uncompressed"},
                        "usage": "Texture2D/IMGUI; preserve aspect. Fill clipped by actual Boost01."})
    return records

def board(package):
    im=Image.new("RGB",(1800,1200),"#08131D");d=ImageDraw.Draw(im)
    def text(x,y,s,size=26,color="#BED0DA",display=False):
        d.text((x,y),s,font=font(size,display),fill=color)
    text(60,34,"VECTOR RUSH / MERIDIAN AFTERHOURS",68,"#E1F2F7",True)
    text(63,115,"ART DIRECTION  /  City, craft, propulsion and interface",28)
    text(63,158,"Design proposal. Native appearance and motion still require integration.",22,"#7EABB9")
    cols=["#142A3C","#768B94","#143D53","#58D5FF","#336EFF","#D94487","#FFB057"]
    labels=["Structure","Mineral","Glazing","Guidance","Propulsion","Signage","Contact"]
    for i,(c,label) in enumerate(zip(cols,labels)):
        x=60+i*243;d.rectangle((x,215,x+218,290),fill=c);text(x,306,label,25)
    # Purposeful architecture silhouettes, not reference images reused as assets.
    for i,(title,sub) in enumerate([
        ("01 / EXCHANGE","Broad occupied head / offset core"),
        ("02 / HOTEL","Slender split spine / crown"),
        ("03 / SERVICES","Receding terraces / deep podium"),
        ("04 / TRANSIT","Sawtooth hall / mechanical annex")]):
        x=60+i*435
        d.rectangle((x,366,x+404,700),fill="#0E2230")
        base=650
        def block(a,b,w,h,c="#456373"):
            d.rectangle((x+a,base-b-h,x+a+w,base-b),fill=c)
        if i==0:
            block(36,0,320,42);block(87,42,115,154);block(110,196,225,76,"#69818A");block(60,70,22,210)
        if i==1:
            block(50,0,300,35);block(118,35,58,225);block(192,35,72,245);block(124,266,132,13,"#58D5FF")
        if i==2:
            for k in range(5):block(50+k*22,k*42,286-k*39,42,"#527480" if k%2 else "#365260")
        if i==3:
            block(35,0,290,135)
            for k in range(5):
                d.polygon([(x+35+k*58,base-135),(x+83+k*58,base-183),(x+93+k*58,base-135)],fill="#71858B")
            block(303,0,43,228)
        text(x+16,672,title,27,"#DEF0F5",True);text(x+7,716,sub,20)
    text(62,784,"CRAFT + FEEDBACK",30,"#58D5FF",True)
    text(62,831,"Satin mineral hull. Cyan seam energy.",25)
    text(62,869,"Blue core + broad turbulent body + wake.",25)
    text(62,907,"Amber contact streaks; brief white flash.",25)
    text(62,945,"Keep the ship solid and recognisable.",25)
    text(735,784,"DISPLAY / RAJDHANI SEMIBOLD",30,"#58D5FF",True)
    text(731,814,"328",137,"#E3F4FA",True)
    text(953,894,"KM/H",24)
    text(1141,836,"03",106,"#E3F4FA",True)
    text(1270,890,"/ 08",31)
    text(735,973,"BARLOW MEDIUM / labels 20 px at 1080p",24)
    text(62,1105,"Silhouette first. Credible supports nearby. Restrained neon. Actual route geometry.",29,"#D7E7EE")
    im.save(ROOT/"evidence/style-board.png")

def metadata(path, semantic=None):
    relative=path.relative_to(PAYLOAD).as_posix()
    guid=uuid.uuid5(uuid.NAMESPACE_URL,"vector-rush/experience-art/"+relative).hex
    header=f"fileFormatVersion: 2\nguid: {guid}\n"
    if path.is_dir():
        return header+"folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n"
    if path.suffix == ".png":
        normal=semantic=="normal"; linear=semantic in ("normal","linear","vfx")
        mip=0 if semantic in ("vfx","ui") else 1
        template=(REPO/"UnityProject/Assets/Resources/Exhaust/BlueFlameAtlas.png.meta").read_text()
        import re
        template=re.sub(r"guid: [a-f0-9]+","guid: "+guid,template)
        template=re.sub(r"enableMipMap: \d+",f"enableMipMap: {mip}",template)
        template=re.sub(r"sRGBTexture: \d+",f"sRGBTexture: {0 if linear else 1}",template)
        template=re.sub(r"textureType: \d+",f"textureType: {1 if normal else 0}",template)
        template=re.sub(r"textureCompression: \d+","textureCompression: 0",template)
        template=re.sub(r"wrap([UVW]): \d+",lambda m:"wrap"+m[1]+": "+str(1 if not mip else 0),template)
        return template
    if path.suffix == ".ttf":
        template=(REPO/"UnityProject/Assets/Resources/Fonts/Barlow-Medium.ttf.meta").read_text()
        import re
        return re.sub(r"guid: [a-f0-9]+","guid: "+guid,template)
    if path.suffix == ".fbx":
        # Explicit stable minimal ModelImporter metadata. Unity fills defaults.
        return header+"""ModelImporter:
  serializedVersion: 22200
  internalIDToNameTable: []
  externalObjects: {}
  materials:
    materialImportMode: 0
  animations:
    importAnimation: 0
  meshes:
    globalScale: 1
    meshCompression: 0
    addColliders: 0
    useFileUnits: 1
    useFileScale: 1
    isReadable: 0
  tangentSpace:
    normalImportMode: 0
    tangentImportMode: 3
  importAnimation: 0
  userData: Astra original asset; preserve material-slot order from manifest.
"""
    return header+"DefaultImporter:\n  externalObjects: {}\n  userData: \n"

def prepare(package):
    package.mkdir(parents=True, exist_ok=True)
    materials(package)
    assets=fonts(package)+graphics(package)
    for name,kind,count in [
        ("vrx_vfx_plume_a","body",16), ("vrx_vfx_core_a","core",16),
        ("vrx_vfx_wake_a","wake",16), ("vrx_vfx_impact_a","impact",8),
        ("vrx_vfx_spark_a","spark",8)]:
        assets.append(atlas(package,name,kind,count))
    dump(package/"experience-assets.json",{
        "contractVersion":1,"revision":package.name,"courseHash":HASH,
        "assets":assets,"status":"assets ready for integration / native validation pending"})
    board(package)
    shutil.copy2(__file__,package/"package_recipe.py")
    shutil.copy2(ROOT/"design/ART-DIRECTION.md",package/"ART-DIRECTION.md")

def publish(package):
    # The Blender recipe supplies these exact ProductionArtManifest v1 files.
    required=["manifest.json","layout.json","lighting.json","track-profile.json","asset-stats.json"]
    for name in required:
        if not (package/name).exists(): raise RuntimeError("Missing "+name)
    manifest=json.loads((package/"manifest.json").read_text())
    roles={}
    for m in manifest["materials"]:
        for key,sem in [("baseColor","colour"),("normal","normal"),("metallicSmoothness","linear"),
                        ("occlusion","linear"),("emission","colour")]:
            if m.get(key):roles[m[key]]=sem
    for asset in manifest["assets"]:
        roles[asset["lod0"]]=roles[asset["lod1"]]="model"
    extra=json.loads((package/"experience-assets.json").read_text())
    for a in extra["assets"]:
        roles[a["path"]]="vfx" if a["type"]=="vfx-atlas" else "ui" if a["type"]=="ui-graphic" else "font"
        if a.get("licensePath"):roles[a["licensePath"]]="text"
    for rel,sem in roles.items():
        src=package/rel
        if not src.is_file(): raise RuntimeError("Missing runtime dependency "+rel)
        dest=PAYLOAD/rel;dest.parent.mkdir(parents=True,exist_ok=True)
        shutil.copy2(src,dest)
        meta=Path(str(dest)+".meta")
        if not meta.exists():meta.write_text(metadata(dest,sem))
    for p in [PAYLOAD]+list(PAYLOAD.rglob("*")):
        if p.is_dir() and not Path(str(p)+".meta").exists():Path(str(p)+".meta").write_text(metadata(p))
    for name in ["manifest.json","layout.json","experience-assets.json","asset-stats.json","collision-proxies.json"]:
        src=package/name
        if src.exists():
            dest=PAYLOAD/name;shutil.copy2(src,dest)
            if not Path(str(dest)+".meta").exists():Path(str(dest)+".meta").write_text(metadata(dest))
    files=[{"path":p.relative_to(package).as_posix(),"sha256":digest(p)}
           for p in sorted(package.rglob("*")) if p.is_file() and p.name not in ("READY.json","checksums.json")]
    dump(package/"checksums.json",{"contractVersion":1,"revision":package.name,"files":files})
    dump(package/"READY.json",{"contractVersion":1,"revision":package.name,"courseHash":HASH,
                               "checksumsSha256":digest(package/"checksums.json")})
    dump(ROOT/"evidence"/(package.name+"-publication.json"),{
        "revision":package.name,"files":len(files),"payloadBytes":sum((package/f["path"]).stat().st_size for f in files),
        "runtimeFiles":len(roles),"assetCount":len(manifest["assets"]),
        "materialCount":len(manifest["materials"]),"nativeImport":"pending"})
    print("READY",package)

if __name__=="__main__":
    p=argparse.ArgumentParser();p.add_argument("action",choices=["prepare","publish"]);p.add_argument("revision")
    args=p.parse_args();package=ROOT/"packages"/("vrx-"+args.revision)
    if (package/"READY.json").exists():raise RuntimeError("Immutable revision already published")
    (prepare if args.action=="prepare" else publish)(package)
