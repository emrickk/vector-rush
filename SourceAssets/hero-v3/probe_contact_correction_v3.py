import bpy,json,sys,runpy
from pathlib import Path
args=sys.argv[sys.argv.index('--')+1:];folder=Path(args[0]);folder.mkdir(parents=True,exist_ok=True)
root=Path(__file__).parent
module=runpy.run_path(str(root/'contact_correction_v3.py'));result=module['apply_contact_correction']()
(folder/'correction-probe.json').write_text(json.dumps(result,indent=2));print('CONTACT_CORRECTION',json.dumps(result),flush=True)
sys.argv=['diagnostic','--',str(folder/'pass04-probe-surface-contacts.json')]
runpy.run_path(str(root/'diagnose_surface_contacts.py'),run_name='__main__')
