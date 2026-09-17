"""Publish C/D supplemental manifests and preserve prior runtime asset identities."""
from pathlib import Path
import sys,json,shutil
from package_art import ROOT,PAYLOAD,HASH,dump,digest,metadata

revision="vrx-"+sys.argv[1];package=ROOT/"packages"/revision
if (package/"READY.json").exists():raise RuntimeError("Immutable revision")
extra=json.loads((package/"experience-assets.json").read_text())
old=json.loads((PAYLOAD/"experience-assets.json").read_text())
combined={a["id"]:a for a in old["assets"]}
for a in extra["assets"]:
    src=package/a["path"];assert src.is_file(),src
    if a["type"]=="ship-texture":
        sem={"base":"colour","normal":"normal","energy":"linear","ms":"linear","ao":"linear"}[a["semantic"]]
    else:sem=None
    dest=PAYLOAD/a["path"];dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dest)
    meta=Path(str(dest)+".meta")
    if not meta.exists():meta.write_text(metadata(dest,sem))
    combined[a["id"]]=a
for filename in ["ship-finish-contract.json","volume-contract.json"]:
    if (package/filename).exists():
        dest=PAYLOAD/"ship"/filename;shutil.copy2(package/filename,dest)
        if not Path(str(dest)+".meta").exists():Path(str(dest)+".meta").write_text(metadata(dest))
for directory in [PAYLOAD]+list(PAYLOAD.rglob("*")):
    if directory.is_dir() and not Path(str(directory)+".meta").exists():Path(str(directory)+".meta").write_text(metadata(directory))
dump(PAYLOAD/"experience-assets.json",{"contractVersion":1,"revision":revision,"courseHash":HASH,
    "environmentRevision":"vrx-b01","assets":list(combined.values()),
    "status":"assets ready for integration / native validation pending"})
for p in package.rglob("*.blend1"):p.unlink()
files=[{"path":p.relative_to(package).as_posix(),"sha256":digest(p)} for p in sorted(package.rglob("*"))
       if p.is_file() and p.name not in ("READY.json","checksums.json")]
dump(package/"checksums.json",{"contractVersion":1,"revision":revision,"files":files})
dump(package/"READY.json",{"contractVersion":1,"revision":revision,"courseHash":HASH,
    "packageType":"experience-supplement","checksumsSha256":digest(package/"checksums.json"),
    "requiresEnvironmentRevision":"vrx-b01"})
dump(ROOT/"evidence"/(revision+"-validation.json"),{
    "status":"Supplement integrity and declared file check passed","assetCount":len(extra["assets"]),
    "checksumFiles":len(files),"sourceBytes":sum((package/f["path"]).stat().st_size for f in files),
    "nativeImport":"unverified","nativeUsability":"unverified"})
print("READY supplemental package",revision)
