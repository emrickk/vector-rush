"""Original coating and boost-region assets over the retained Kestrel UVs.
Run system Python first. This changes no geometry and writes no runtime code.
"""
from pathlib import Path
import json,shutil,hashlib
import numpy as np
from PIL import Image,ImageFilter
from package_art import ROOT,REPO,PAYLOAD,dump,png,metadata,digest

OUT=ROOT/"packages/vrx-c01"
if (OUT/"READY.json").exists():raise RuntimeError("Published revision is immutable")
(OUT/"ship").mkdir(parents=True,exist_ok=True)
source=REPO/"UnityProject/Assets/Resources/Art/ShipSurfaces"
records=[];provenance=[]
for key, factor, smooth in [
    ("Ivory",[.73,.87,.93],.52),("Graphite",[.70,.87,1.05],.39),
    ("Metal",[.82,.92,1.0],.60),("Ceramic",[.76,.87,.93],.37)]:
    original=np.array(Image.open(source/(key+"_BaseColor.png")).convert("RGB"))/255.
    # Preserve exact UV island layout, wear and race-number ink. Shift the existing
    # yellow-green accent to cyan by chromatic selection, never paint over text.
    accent=(original[:,:,1]>original[:,:,2]*1.45)&(original[:,:,0]>original[:,:,2]*1.1)&(original[:,:,1]>.20)
    colour=original*np.array(factor)
    colour[accent]=np.array([.12,.69,.83])*np.maximum(original[accent].max(axis=1)[:,None],.50)
    ident="vrx_ship_"+key.lower()+"_a"
    name="ship/"+ident+"_base.png";png(OUT/name,colour)
    # Packed RGB = accent region, subtle existing surface-edge region, reserved 0.
    # Coverage derives from existing surface textures; Fresnel outline remains 03 code.
    gray=Image.fromarray(np.uint8(original.mean(axis=2)*255))
    edges=np.array(gray.filter(ImageFilter.FIND_EDGES))/255.
    energy=np.zeros((*accent.shape,4));energy[:,:,0]=accent
    energy[:,:,1]=.12+np.clip(edges*.45,0,.22);energy[:,:,3]=1
    mask="ship/"+ident+"_energy.png";png(OUT/mask,energy)
    for suffix,sem in [("normal","normal"),("ms","linear"),("ao","linear")]:
        srcsuffix={"normal":"Normal","ms":"MetallicSmoothness","ao":"Occlusion"}[suffix]
        src=source/(key+"_"+srcsuffix+".png");dest=OUT/"ship"/(ident+"_"+suffix+".png")
        if suffix=="ms":
            arr=np.array(Image.open(src).convert("RGBA"))/255.
            # Deliberate satin finish while retaining the original metallic regions.
            arr[:,:,3]=np.clip(arr[:,:,3]*.2+smooth*.8,0,1);png(dest,arr)
        else:shutil.copy2(src,dest)
    for suffix,sem in [("base","colour"),("energy","linear"),("normal","normal"),("ms","linear"),("ao","linear")]:
        path="ship/"+ident+"_"+suffix+".png"
        records.append({"id":ident+"_"+suffix,"path":path,"type":"ship-texture","revision":1,
                        "source":"Retained repository Kestrel07 v3 UV-compatible map; Astra coating derivation",
                        "license":"Project-authored derivation of existing project assets",
                        "import":{"colorSpace":"sRGB" if sem=="colour" else "linear","normalMap":sem=="normal",
                                  "mipmaps":True,"wrap":"clamp","filter":"bilinear"},
                        "materialKey":key,"semantic":suffix})
    provenance.append({"materialKey":key,"baseSource":str((source/(key+"_BaseColor.png")).relative_to(REPO)),
                       "sourceSha256":digest(source/(key+"_BaseColor.png")),"changedAccentPixels":int(accent.sum())})
shutil.copy2(REPO/"UnityProject/Assets/Resources/Art/HeroShipEngineAnchors.json",OUT/"ship/engine-anchors.json")
dump(OUT/"experience-assets.json",{"contractVersion":1,"revision":"vrx-c01","assets":records,
                                 "status":"assets ready for integration / native validation pending"})
dump(OUT/"ship-finish-contract.json",{
    "revision":"vrx-c01","geometry":"Retain Art/HeroShip verbatim; no hull/collider/UV/anchor changes",
    "sourceGeometrySha256":digest(REPO/"UnityProject/Assets/Resources/Art/HeroShip.fbx"),
    "materials":provenance,"energyChannels":{"R":"existing livery accent mask",
        "G":"0.12 full-hull floor plus low-amplitude existing surface-edge mask","B":"reserved zero","A":"one"},
    "boost":{"opaqueHull":True,"baseAlbedoRetention":.78,"accentEmissionLinear":[.035,.62,1],
             "accentEmissionIntensity":3.4,"surfaceEdgeEmissionIntensity":.45,
             "fresnelOutlineWidthPixelsAt1080p":[1.0,2.2],"fresnelEmissionIntensity":1.8,
             "onsetMs":80,"peakMs":160,"settleMs":240,"releaseMs":180,
             "rule":"Material-only on IsBoosting. No shield, transparency, geometry morph or gameplay."},
    "normalizedBindings":{"_VRThrust":"engine brightness only; preserve 0.25 idle minimum at core",
                          "_VRBoost":"blend energy floor, accent emission and local cyan Fresnel outline",
                          "_VRBoostIgnition":"<=80 ms cyan-white accent emphasis, never whole-screen white",
                          "_VRBoostRelease":"180 ms hull energy decay; already emitted particles follow their lifetime"},
    "shaderRequestsFor03":"Use local per-renderer material properties; exclude canopy glass/nozzle interiors from whole-hull fill. Keep saturated cyan accents and a solid dark mass. Proposed emission: colour*(3.4*R + .45*G + 1.8*edgeFresnel)*boost, with soft bounded silhouette width.",
    "limitations":"R mask follows existing livery. G includes a constant floor and UV texture-edge detail, not geometric curvature. Outline comes from shader, not the bitmap."})
shutil.copy2(__file__,OUT/"ship_finish_recipe.py")
print("SHIP_TEXTURES_READY",len(records))
