import hashlib,json,struct,zlib,datetime,re,xml.etree.ElementTree as ET
from pathlib import Path
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

root=Path.cwd()
base=Path("evidence/night-production/road-response")
identity=json.loads((base/"control/build-identity.json").read_text())
counts={}
for key in ("files","appFiles"):
    bad=[p for p,h in identity[key].items() if hashlib.sha256(Path(p).read_bytes()).hexdigest()!=h]
    assert not bad,bad
    counts[key]=len(identity[key])
restoration=json.loads((base/"restoration-validation.json").read_text())
scene=hashlib.sha256(Path("UnityProject/Assets/Scenes/Solstice.unity").read_bytes()).hexdigest()
assert scene==restoration["sceneSha256"]
final=base/"final"
tests=ET.parse(final/"editmode-results.xml").getroot().attrib
assert tests["passed"]=="42" and tests["failed"]=="0" and tests["result"]=="Passed"
pngs={}
for p in sorted((final/"performance").glob("*.png")):
    w,h,sha=png_info(p);assert (w,h)==(1920,1080)
    pngs[p.name]={"width":w,"height":h,"sha256":sha}
assert len(pngs)==6
race=(final/"performance/race-verification.txt").read_text()
assert "FINISHED" in race and "Laps 3" in race and "Recoveries 0" in race and "False" not in race
assert race.count("Restart launch ")==2 and race.count("countdown pause frozen=True")==3
telemetry=(final/"performance/live-telemetry.txt").read_text()
assert "Phase Finished" in telemetry and telemetry.count("recoveries 0")==6
metrics=(final/"performance/runtime-metrics.txt").read_text()
assert "Resolution 1920x1080" in metrics and "Autopilot True" in metrics and "VSync 1" in metrics
log=(final/"native-performance.log").read_text(errors="replace")
errors=[line for line in log.splitlines() if re.search(r"Exception|Crash|Fatal|error CS[0-9]",line,re.I)]
assert not errors,errors
surfaces=re.findall(r"surface size (\d+x\d+)",log)
assert surfaces and set(surfaces)=={"1920x1080"},surfaces
report={"verifiedUtc":datetime.datetime.now(datetime.timezone.utc).isoformat(),"passed":True,"buildGuid":restoration["buildGuid"],"identitiesUnchanged":counts,"sceneSha256":scene,"tests":tests,"nativeProcessExitCode":0,"pngs":pngs,"surfaceSizes":surfaces,"raceVerification":race,"finalTelemetry":telemetry,"runtimeMetrics":metrics,"scope":"Restored exact control app. Existing tests, actual automated three-lap race/restarts, six PNG integrity checks and a separate 60-second real-time interval sample. No concurrent Unity build, bake or encoder. Full circuit/selected stills and decoded simulation-time clip are in ../control. No watched continuous motion, human input/audio or alternate-aspect acceptance. Candidate performance was not measured and is not inferred from these numbers."}
(final/"verification.json").write_text(json.dumps(report,indent=2)+"\n")
print(json.dumps({"passed":True,"identities":counts,"pngs":len(pngs),"tests":tests["passed"],"race":race,"metrics":metrics},indent=2))
