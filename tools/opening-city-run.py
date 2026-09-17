"""Run one heavy command under the shared lease; failure to acquire never starts it."""
import datetime, os, pathlib, subprocess, sys, uuid

root = pathlib.Path(__file__).resolve().parents[1]
# Every experience workspace must contend for the same machine-wide project
# lease. Keeping the lease beside experience-workspaces makes clones share it.
project_root = root.parents[1]
lease = project_root / "locks/heavy-process.lease"
token = str(uuid.uuid4())
try:
    lease.mkdir()
except FileExistsError:
    print("Heavy-process lease is occupied; no command started.", file=sys.stderr)
    sys.exit(75)
owner = lease / "owner.txt"
owner.write_text(f"lane: Integrated Racing Experience\nworkspace: {root}\nprocess ID: {os.getpid()}\n"
                 f"start: {datetime.datetime.now().isoformat()}\n"
                 f"token: {token}\ncommand: {sys.argv[1] if len(sys.argv)>1 else '(missing)'}\n")
try:
    if token not in owner.read_text():
        raise RuntimeError("Lease ownership changed before launch")
    if len(sys.argv)<2:
        raise ValueError("Pass the command and its arguments")
    sys.exit(subprocess.call(sys.argv[1:],cwd=root))
finally:
    if owner.exists() and token in owner.read_text():
        owner.unlink()
        lease.rmdir()
