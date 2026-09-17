#!/usr/bin/env python3
"""Build a local Stage 2 review from existing identified native evidence, never placeholders."""
import argparse
import html
import json
import os
from pathlib import Path
from urllib.parse import quote
import xml.etree.ElementTree as ET

SIZES = ("1280x720", "1600x900", "1920x1080", "2560x1080")
SCREENS = (
    ("01-title.png", "Title"), ("02-settings.png", "Audio settings"),
    ("03-controls.png", "Controls"), ("03b-bindings.png", "Key bindings"),
    ("03c-comfort.png", "Display & comfort"), ("04-countdown.png", "Countdown"),
    ("05-racing.png", "Racing HUD"), ("06-paused.png", "Pause"),
    ("07-pause-settings.png", "Settings while paused"), ("08-resumed.png", "Resume"),
    ("12-returned-title.png", "Return to title"),
)
STAGES = ("cruise", "acceleration", "boost", "release", "recharge", "reboost")


def read(path):
    return json.loads(path.read_text())


def generate(root, output, manual=None, tests=None):
    esc = html.escape
    missing, excluded, captures = [], [], []
    identity_path = root / "build/build-identity.json"
    identity = read(identity_path) if identity_path.exists() else {}
    guid = identity.get("buildGuid")
    if not guid:
        missing.append("Final native build identity")
    for path in sorted(root.rglob("stage2-evidence.json")):
        data = read(path)
        if not guid or data.get("buildGuid") != guid:
            excluded.append(str(path.relative_to(root)) + ": different or unidentified build")
        elif data.get("complete") is not True:
            excluded.append(str(path.relative_to(root)) + ": incomplete capture")
        else:
            captures.append((path.parent, data))

    def url(path):
        return quote(os.path.relpath(path.resolve(), output.parent.resolve()), safe="/")

    def link(path, label):
        return f'<a href="{url(path)}">{esc(label)}</a>'

    def find(mode, treatment=True, dimensions=None):
        matches = [(folder, report) for folder, report in captures
                   if report.get("mode") == mode and report.get("propulsionEnabled") is treatment
                   and (dimensions is None or f'{report["width"]}x{report["height"]}' == dimensions)]
        if len(matches) > 1:
            excluded.append(f"Ambiguous {mode}/{treatment}/{dimensions}: using {matches[0][0].relative_to(root)}")
        return matches[0] if matches else (None, None)

    def still(capture, filename, caption):
        folder, report = capture
        listed = {v.get("file") for v in report.get("views", [])} if report else set()
        path = folder / filename if folder else None
        if not path or filename not in listed or not path.is_file():
            missing.append(caption)
            return f'<div class="pending">Pending: {esc(caption)}</div>'
        return (f'<figure><a href="{url(path)}" target="_blank"><img loading="lazy" '
                f'src="{url(path)}" alt="{esc(caption)}"></a><figcaption>{esc(caption)}'
                f'<span>{report["width"]} × {report["height"]}</span></figcaption></figure>')

    def clip(capture, label):
        folder, report = capture
        if not folder:
            missing.append(label)
            return f'<div class="pending">Pending: {esc(label)}</div>'
        found = []
        for metadata in sorted(root.rglob("*.mp4.encoding.json")):
            data = read(metadata)
            if (data.get("verified") is True and data.get("buildGuid") == guid
                    and Path(data.get("capture", "")).resolve() == folder.resolve()
                    and data.get("audioStreams") == 0 and data.get("rate") == 24
                    and data.get("frames") == report.get("sequenceFrames")):
                path = Path(data.get("file", ""))
                if path.is_file():
                    found.append((path, metadata, data))
        if not found:
            missing.append(label + " with verified encoding metadata")
            return f'<div class="pending">Pending: {esc(label)} — verified video not found.</div>'
        path, metadata, data = found[0]
        return (f'<figure><video controls preload="metadata" src="{url(path)}"></video>'
                f'<figcaption>{esc(label)}<span>{data["frames"]} frames · '
                f'{data["seconds"]:.2f} s · no audio</span></figcaption>'
                f'<p class="small">{link(metadata, "Encoding verification")}</p></figure>')

    # Resolve the final-candidate records once. Older candidate folders never fill missing slots.
    before, after = find("propulsion", False, "1600x900"), find("propulsion", True, "1600x900")
    loop = find("loop", True, "1600x900")
    propulsion = '<div class="twocol">' + clip(before, "Legacy propulsion") + clip(after, "Stage 2 propulsion") + '</div>'
    propulsion += '<button class="sync" type="button">Play / restart both comparison clips</button>'
    for stage in STAGES:
        propulsion += (f'<h3>{esc(stage.capitalize())}</h3><div class="twocol">'
                       + still(before, f"propulsion-{stage}.png", f"Legacy · {stage}")
                       + still(after, f"propulsion-{stage}.png", f"Stage 2 · {stage}") + '</div>')
    tabs, panels = [], []
    for i, size in enumerate(SIZES):
        capture = find("screens", True, size)
        tabs.append(f'<button type="button" role="tab" aria-selected="{str(i==1).lower()}" '
                    f'data-panel="size-{i}">{size.replace("x", " × ")}</button>')
        cards = "".join(still(capture, filename, label+" · "+size) for filename, label in SCREENS)
        panels.append(f'<div id="size-{i}" class="screen-panel" {"" if i==1 else "hidden"}><div class="twocol">{cards}</div></div>')
    walkthrough = clip(loop, "Complete programmatic screen walkthrough and test-autopilot race")
    walkthrough += '<div class="twocol">' + still(loop, "10-results-final.png", "Actual final race results") + still(loop, "11-retry.png", "Actual retry countdown") + '</div>'
    baseline_hud = Path(__file__).resolve().parents[1] / "references/interface-stage2a/backgrounds/current-hud.png"
    hud_comparison = ""
    if baseline_hud.is_file():
        hud_comparison = (
            '<h3>The HUD, before and after</h3><p class="small">Native screenshots from different driving moments; compare the interface styling.</p>'
            '<div class="twocol"><figure><a href="' + url(baseline_hud) + '" target="_blank">'
            '<img src="' + url(baseline_hud) + '" alt="Previous native HUD"></a>'
            '<figcaption>Previous HUD</figcaption></figure>'
            + still(find("screens", True, "1600x900"), "05-racing.png", "Polished native HUD")
            + '</div><h3>Menus and screens</h3>')

    performance = []
    for enabled, label in ((False, "Legacy"), (True, "Stage 2")):
        folder, report = find("performance", enabled, "1600x900")
        if not report:
            missing.append(label + " isolated performance run")
            performance.append(f'<tr><td>{label}</td><td colspan="4">Pending</td></tr>')
        else:
            performance.append(f'<tr><td>{link(folder / "stage2-evidence.json", label)}</td>'
                               f'<td>{report.get("performanceSamples", 0)}</td>'
                               + "".join(f'<td>{report.get(k, 0):.2f} ms</td>' for k in ("meanMs", "p95Ms", "p99Ms")) + '</tr>')

    validations = []
    for path in sorted(root.rglob("*validat*.json")):
        data = read(path)
        references = [data.get(k, {}).get("buildGuid") for k in ("capture", "candidate", "before", "after")
                      if isinstance(data.get(k), dict)]
        if guid not in references:
            continue
        passed = data.get("passed") is True
        text = "Passed integrity/state checks" if passed else "Failed: " + str(data.get("error", "inspect report"))
        validations.append(f'<li>{link(path, str(path.relative_to(root)))} — {esc(text)}</li>')
        if not passed:
            missing.append("Passing validation: " + str(path.relative_to(root)))
    if not validations:
        missing.append("Current-build external validation reports")
    selected_tests = tests.resolve() if tests is not None else root / "tests.xml"
    if tests is not None and not selected_tests.is_file():
        raise FileNotFoundError(f"Explicit test report does not exist: {selected_tests}")
    test_summary = ""
    test_record = None
    if selected_tests.exists():
        t = ET.parse(selected_tests).getroot()
        test_record = {"path": str(selected_tests.resolve()), "result": t.get("result", "unclassified"),
                       "passed": t.get("passed"), "failed": t.get("failed")}
        test_summary = (f'<p><strong>Selected final native test suite:</strong> '
                        f'{link(selected_tests, selected_tests.name)} — {esc(test_record["result"])}; '
                        f'{esc(t.get("passed", "?"))} passed, {esc(t.get("failed", "?"))} failed.</p>')
        attempts = []
        for previous in sorted(root.glob("tests*.xml")):
            if previous.resolve() == selected_tests.resolve():
                continue
            earlier = ET.parse(previous).getroot()
            attempts.append(f'<li>{link(previous, previous.name)} — '
                            f'{esc(earlier.get("result", "unclassified"))}; '
                            f'{esc(earlier.get("passed", "?"))} passed, '
                            f'{esc(earlier.get("failed", "?"))} failed.</li>')
        if attempts:
            test_summary += ('<details><summary>Preserved test attempts — not the selected final suite</summary>'
                             '<ul>' + "".join(attempts) + '</ul></details>')

    manual = manual or root / "manual-input-checks.json"
    manual_html = '<p class="pending">Manual pointer, keyboard, persistence and physical-controller observations have not been recorded for this build.</p>'
    manual_present = False
    if manual.exists():
        notes = read(manual)
        if notes.get("buildGuid") == guid and notes.get("checks"):
            manual_present = True
            rows = []
            for item in notes.get("checks", []):
                rows.append('<tr><td>' + esc(str(item.get("name", ""))) + '</td><td>'
                            + esc(str(item.get("status", "unassessed"))) + '</td><td>'
                            + esc(str(item.get("notes", ""))) + '</td></tr>')
            manual_html = ('<p>Recorded observations; independent of the automated capture walkthrough. '
                           + link(manual, "Read source observations") + '</p>'
                           + '<table><thead><tr><th>Check</th><th>Status</th><th>Observation</th></tr></thead><tbody>'
                           + "".join(rows) + '</tbody></table>')
        else:
            excluded.append("Manual observations belong to a different build")
    if not manual_present:
        missing.append("Current-build manual input and persistence observations")

    provenance_path = root / "provenance.json"
    provenance = read(provenance_path) if provenance_path.exists() else {}
    source = provenance.get("source", {})
    revision = source.get("revision", "Not recorded")
    state = "Local changes were present at build time." if source.get("dirty") else ""
    status = (f'{len(missing)} evidence item(s) pending' if missing else "Requested automated evidence is present")
    missing_html = "<ul>" + "".join(f"<li>{esc(item)}</li>" for item in missing) + "</ul>" if missing else "<p>No required automated capture slots are missing. Owner acceptance remains separate.</p>"
    excluded_html = "<ul>" + "".join(f"<li>{esc(item)}</li>" for item in excluded) + "</ul>" if excluded else "<p>No incompatible capture reports were included.</p>"
    page = TEMPLATE.replace("{{TITLE}}", esc(root.name))
    replacements = {
        "GUID": esc(guid or "No final build identity"), "REVISION": esc(revision), "SOURCE_STATE": esc(state),
        "STATUS": esc(status), "PROPULSION": propulsion, "TABS": "".join(tabs),
        "PANELS": "".join(panels), "HUD_COMPARISON": hud_comparison, "WALKTHROUGH": walkthrough, "PERFORMANCE": "".join(performance),
        "VALIDATIONS": "<ul>"+"".join(validations)+"</ul>", "TESTS": test_summary,
        "MANUAL": manual_html, "MISSING": missing_html, "EXCLUDED": excluded_html,
        "IDENTITY_LINK": link(identity_path, "Build identity") if identity_path.exists() else "Build identity pending",
    }
    for key, value in replacements.items():
        page = page.replace("{{"+key+"}}", value)
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(page)
    manifest = {"buildGuid": guid, "html": str(output.resolve()), "missing": missing,
                "excluded": excluded, "captureReportsIncluded": len(captures),
                "selectedTestReport": test_record}
    output.with_suffix(".manifest.json").write_text(json.dumps(manifest, indent=2)+"\n")
    return manifest


TEMPLATE = """<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Vector Rush · HUD & menu polish</title><style>
:root{color-scheme:dark;--ink:#08151a;--panel:#102229;--line:#29424a;--muted:#94aab0;--white:#ecf0e4;--lime:#dafa51}
*{box-sizing:border-box}body{margin:0;background:var(--ink);color:var(--white);font:15px/1.5 -apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif}
a{color:#8be5e5;text-decoration:none}a:hover{text-decoration:underline}header,main,footer{max-width:1500px;margin:auto;padding:32px 42px}
nav{display:flex;gap:26px;flex-wrap:wrap;border-bottom:1px solid var(--line);padding-bottom:22px;font-size:13px;text-transform:uppercase;letter-spacing:.1em}
nav strong{color:var(--lime);margin-right:auto}.eyebrow{font-size:13px;text-transform:uppercase;letter-spacing:.18em;color:var(--lime)}
h1{font-size:clamp(42px,6vw,88px);line-height:.97;letter-spacing:-.055em;margin:28px 0}h2{font-size:34px;letter-spacing:-.035em;margin:0 0 8px}h3{font-size:19px;margin:32px 0 12px}
.intro{max-width:850px;font-size:19px;color:#c5d4d4}.meta{display:flex;gap:28px;flex-wrap:wrap;font-size:12px;color:var(--muted);overflow-wrap:anywhere}
section{padding:36px 0 52px;border-top:1px solid var(--line)}section>p{max-width:1000px;color:var(--muted)}
.twocol{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:18px}
figure{margin:0;background:var(--panel);border:1px solid var(--line)}img,video{width:100%;display:block;background:#03080a}figcaption{padding:13px 16px;display:flex;gap:10px;justify-content:space-between}figcaption span{font-size:12px;color:var(--muted);text-align:right}
.pending{padding:28px;background:#261f15;color:#e9bf79;border:1px solid #665239;border-radius:3px}
.tabs{display:flex;gap:10px;flex-wrap:wrap;margin:24px 0}button{border:1px solid var(--line);background:var(--panel);padding:12px 18px;color:var(--white);cursor:pointer;font:inherit}
button:hover,button[aria-selected=true]{background:var(--lime);color:var(--ink);border-color:var(--lime)}.sync{margin-top:16px}
.small{font-size:12px;padding:0 16px;color:var(--muted)}table{width:100%;border-collapse:collapse;margin:22px 0}th,td{padding:14px;text-align:left;border-bottom:1px solid var(--line)}th{color:var(--muted);font-size:12px;text-transform:uppercase}
details{margin:20px 0}summary{cursor:pointer;color:var(--lime)}footer{color:var(--muted);font-size:13px}
@media(max-width:760px){header,main,footer{padding:24px 18px}.twocol{grid-template-columns:1fr}figcaption{flex-direction:column}figcaption span{text-align:left}table{font-size:12px}th,td{padding:8px}}
</style></head><body><header><nav><strong>Vector Rush / Nocturne</strong><a href="#screens">HUD & menus</a><a href="#propulsion">Exhaust</a><a href="#loop">In motion</a><a href="#checks">Checks</a></nav>
<p class="eyebrow">Stage 02 / {{TITLE}}</p><h1>HUD &amp;<br>menu polish.</h1>
<p class="intro">Stronger typography, cleaner composition and a consistent racing identity. These are actual screens from the playable build. Open any image at full size to inspect the finish.</p>
<p>{{STATUS}} · Owner review and acceptance remain open.</p><div class="meta"><span>BUILD {{GUID}}<br>{{IDENTITY_LINK}}</span><span>SOURCE {{REVISION}}<br>{{SOURCE_STATE}}</span></div></header>
<main><section id="screens"><p class="eyebrow">01 / The visual finish</p><h2>One identity, across the game.</h2>
<p>Title, race HUD, pause and settings at four window sizes. The gallery shows native layouts; direct input checks are recorded separately.</p>
{{HUD_COMPARISON}}
<div class="tabs" role="tablist">{{TABS}}</div>{{PANELS}}</section>
<section id="propulsion"><p class="eyebrow">02 / Thrust in motion</p><h2>Fuller, more dynamic exhaust.</h2>
<p>Matched legacy and Stage 2 runs use scripted virtual-gamepad inputs through normal physics. Videos are silent simulation-time recordings; they are not human driving or physical-controller tests.</p>{{PROPULSION}}</section>
<section id="loop"><p class="eyebrow">03 / Launch to retry</p><h2>The complete interface and race loop.</h2>
<p>Programmatic menu/lifecycle operations surround a normal full race driven by the game’s test autopilot. Results are read from that completed native race. This silent simulation-time walkthrough does not certify menu input or human handling.</p>{{WALKTHROUGH}}</section>
<section id="checks"><p class="eyebrow">04 / Checks and limits</p><h2>Build verification.</h2><details><summary>Open test results, input checks and performance measurements</summary>
<h3>Isolated sustained-boost frame intervals</h3><p>Source-reported delivered CPU frame intervals during real-time runs with the same scripted input schedule, without screenshots or an encoder. These are not isolated GPU timings; variable frame delivery can produce different real-time trajectories.</p>
<table><thead><tr><th>Propulsion</th><th>Samples</th><th>Mean</th><th>p95</th><th>p99</th></tr></thead><tbody>{{PERFORMANCE}}</tbody></table>
<h3>External evidence validation</h3>{{VALIDATIONS}}{{TESTS}}
<h3>Actual input and persistence observations</h3>{{MANUAL}}
<details open><summary>Pending evidence</summary>{{MISSING}}</details>
<details><summary>Excluded or ambiguous records</summary>{{EXCLUDED}}</details>
</details></section></main><footer>Local review artifact. Screens and recordings support your visual review; artistic acceptance remains yours.</footer>
<script>
document.querySelectorAll('[data-panel]').forEach(b=>b.addEventListener('click',()=>{
document.querySelectorAll('[data-panel]').forEach(x=>x.setAttribute('aria-selected',String(x===b)));
document.querySelectorAll('.screen-panel').forEach(p=>p.hidden=p.id!==b.dataset.panel);}));
document.querySelector('.sync').addEventListener('click',()=>{document.querySelectorAll('#propulsion video').forEach(v=>{v.currentTime=0;v.play().catch(()=>{});});});
</script></body></html>"""


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("candidate", type=Path)
    parser.add_argument("--output", type=Path)
    parser.add_argument("--manual-checks", type=Path,
                        help="JSON with this buildGuid and checks [{name,status,notes}]")
    parser.add_argument("--tests", type=Path,
                        help="Exact final NUnit XML report; defaults to candidate/tests.xml. "
                             "Other tests*.xml files remain linked as preserved attempts.")
    parser.add_argument("--require-complete", action="store_true")
    args = parser.parse_args()
    root = args.candidate.resolve()
    output = (args.output or root / "review.html").resolve()
    result = generate(root, output, args.manual_checks, args.tests)
    print(json.dumps({k: result[k] for k in ("buildGuid", "html", "captureReportsIncluded")}))
    if args.require_complete and result["missing"]:
        raise SystemExit("Review generated, but required evidence is missing. Inspect review.manifest.json.")


if __name__ == "__main__":
    main()
