"""Build and capture a full native opening milestone through opening-city-run.py."""
import pathlib, plistlib, shlex, subprocess, sys

root=pathlib.Path(__file__).resolve().parents[1]
tag=sys.argv[1]
base=root.parent/"artifacts/opening-city"/tag
base.mkdir(parents=True,exist_ok=False)
app=root.parent/"builds"/("Vector Rush Meridian "+tag+".app")
args=["-aaaBuildOutput",str(app),"-aaaEvidence",str(base/"build")]
subprocess.run(["unity","build",str(root/"UnityProject"),"--editor-version","6000.6.0f1",
    "--target","StandaloneOSX","--execute-method","VectorRush.Editor.OpeningCityImport.Build",
    "--output-path",str(app),"--log-file",str(base/"editor.log"),"--no-tail",
    "--allow-dirty-build","--args",shlex.join(args),"--provenance-path",str(base/"provenance.json")],check=True)
with (app/"Contents/Info.plist").open("rb") as f:
    exe=app/"Contents/MacOS"/plistlib.load(f)["CFBundleExecutable"]
for mode in ["after","before"]:
    command=[str(exe),"-screen-fullscreen","0","-screen-width","1600","-screen-height","900",
        "-openingEvidence",str(base/mode),"-logFile",str(base/(mode+"-player.log"))]
    if mode=="before":command+=["-vrOpeningCity","off"]
    subprocess.run(command,check=True,timeout=240)
print("Native opening milestone:",base)
