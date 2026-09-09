#!/usr/bin/env python3
"""Record immutable source/app identities before native comparisons; run at repo root."""
import argparse
import hashlib
import json
import subprocess
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('stage', type=Path)
parser.add_argument('app', type=Path)
parser.add_argument('--scope', required=True)
args = parser.parse_args()
root = Path.cwd()
app = args.app.resolve()
app.relative_to(root)
assert app.is_dir(), app
paths = subprocess.check_output([
    'git', 'ls-files', '--cached', '--others', '--exclude-standard',
    'UnityProject/Assets', 'UnityProject/ProjectSettings', 'UnityProject/Packages'
], text=True).splitlines()
def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()
identity = {
    'sourceBase': subprocess.check_output(['git', 'rev-parse', 'HEAD'], text=True).strip(),
    'files': {p: sha(root / p) for p in sorted(set(paths)) if (root / p).is_file()},
    'appFiles': {str(p.relative_to(root)): sha(p) for p in sorted(app.rglob('*')) if p.is_file()},
    'scope': args.scope,
}
assert identity['files'] and identity['appFiles']
for mode in ('off', 'on'):
    folder = args.stage / mode
    folder.mkdir(parents=True, exist_ok=True)
    destination = folder / 'build-identity.json'
    assert not destination.exists(), f'Refusing to replace previous identity: {destination}'
    destination.write_text(json.dumps(identity, indent=2) + '\n')
control = json.loads(Path('evidence/nocturne-v2/ssr-01/control-identity.json').read_text())
assert all(sha(root / p) == h for p, h in control['appFiles'].items()), 'Original control changed'
print(json.dumps({'sourceFiles': len(identity['files']), 'appFiles': len(identity['appFiles']), 'originalControlUnchanged': True}))
