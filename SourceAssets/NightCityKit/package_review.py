"""Build a portable, source-backed asset gallery from inspected native and Blender renders."""
import hashlib, html, json, math, shutil
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

root = Path(__file__).resolve().parent
repo = root.parents[1]
out = repo / 'docs/night-city-art-2026-09-17'
images = out / 'images'; images.mkdir(exist_ok=True)
import argparse
parser=argparse.ArgumentParser();parser.add_argument('--evidence',type=Path,default=Path('/Users/anping.wang/output/vector-rush-night-city-art-2026-09-17'));evidence=parser.parse_args().evidence

def jpg(source, name, size=(1920, 1600)):
    im = Image.open(source).convert('RGB'); im.thumbnail(size)
    im.save(images / (name + '.jpg'), quality=94)
    return 'images/' + name + '.jpg'

names = {
    'NeonRoofSign':'Rooftop neon frame', 'CoolingUnit':'Twin-fan cooling unit',
    'DuctBank':'Insulated duct bank', 'RoofExtractor':'Roof extractor',
    'PowerCabinet':'Power cabinet', 'WaterTank':'Rooftop water tank',
    'CableSpan':'Cable hangers', 'MaintenanceBridge':'Maintenance bridge',
    'TransitGantry':'Transit gantry', 'Shopfront':'Service shopfront',
    'Sign_blade':'Vertical hotel sign', 'Sign_market':'Market fascia',
    'Sign_portrait':'After Hours display', 'Sign_koi':'Night Market display',
    'Sign_transit':'Last Train display', 'Sign_square':'Circular noodle sign',
    'Building_tenement':'Apartment block', 'Building_office':'Office tower',
    'Building_service':'Service tower'
}
manifest = json.loads((root / 'mesh-manifest.json').read_text())
records = []
for item in manifest['assets']:
    if item['lod']: continue
    low = next(v for v in manifest['assets'] if v['id'] == item['id'] and v['lod'] == 1)
    key = item['id']; group = 'Architecture' if key.startswith('Building') or key in ['MaintenanceBridge','Shopfront','TransitGantry'] else 'Signs' if key.startswith('Sign') or key == 'NeonRoofSign' else 'Equipment'
    records.append(dict(name=names[key], group=group, image=jpg(root / 'renders/catalog' / (key+'.png'), key),
        detail=f"{item['triangles']:,} / {low['triangles']:,} triangles · LOD0 / LOD1",
        dimensions=' × '.join(f'{v:.2f}' for v in [item['dimensions'][0],item['dimensions'][2],item['dimensions'][1]])+' m · W/H/D',
        prefab=f'../../UnityProject/Assets/Art/NightCityKit/Prefabs/NC_{key}.prefab'))
for item in json.loads((root/'materials.json').read_text())['materials']:
    key = item['id']
    if item['kind'] != 'surface' or key.endswith('_wet'): continue
    records.append(dict(name=key.replace('_',' ').title(), group='Materials', image=jpg(root/'renders/materials'/(key+'.png'),'material-'+key),
        detail='Dry left · wet right · identical studio lighting', dimensions='Blender material render · shared base map, different surface response'))
hero = jpg(evidence/'native-review-light/unity-assembly.png','unity-assembly')
close = jpg(evidence/'native-review-light/unity-rooftop.png','unity-rooftop')
jpg(root/'surface-proof.jpg','surface-proof')
for key in ['afterhours','koi_market','transit']:
    jpg(root/'scenario'/key/'art.png','art-'+key)
reference = Path('/var/folders/yx/hf7ht73550q2156rvymtwnrr0000gn/T/codex-clipboard-4b5bdd2b-e37f-47a2-9c71-903ee482fa0f.png')
if reference.exists(): jpg(reference, 'owner-reference')
(out/'gallery.json').write_text(json.dumps(records, indent=2)+'\n')
audit = json.loads((evidence/'import-validation.json').read_text())
shutil.copy2(evidence/'import-validation.json', out/'import-validation.json')

# Regenerate the previously stale contact sheet from the current individual renders.
sheet=Image.new('RGB',(1600,math.ceil(19/4)*340),'#15252b');draw=ImageDraw.Draw(sheet)
font=ImageFont.truetype(str(root/'fonts/Rajdhani-SemiBold.ttf'),22)
for n,item in enumerate(records[:19]):
    im=Image.open(out/item['image']);im.thumbnail((310,300));x=(n%4)*400;y=(n//4)*340
    sheet.paste(im,(x+(400-im.width)//2,y));draw.text((x+18,y+310),item['name'],font=font,fill='#e7eee9')
sheet.save(root/'renders/catalog.jpg',quality=94);jpg(root/'renders/catalog.jpg','catalog')

page='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Vector Rush · Night city art kit</title><style>
:root{color-scheme:dark;font-family:system-ui,sans-serif;color:#e9eee9;background:#0d1519}*{box-sizing:border-box}body{margin:0}main{max-width:1320px;margin:auto;padding:56px 32px}a{color:#a7d8d0}header{max-width:880px;margin-bottom:32px}.eyebrow{font-size:12px;letter-spacing:.16em;text-transform:uppercase;color:#a7d8d0}h1{font-size:clamp(36px,6vw,70px);line-height:1.06;font-weight:560;letter-spacing:-.045em;margin:16px 0}h2{font-size:28px;font-weight:550;margin:44px 0 14px}p{line-height:1.65;color:#b7c6c7;max-width:800px}.facts{display:flex;gap:12px;flex-wrap:wrap;margin:24px 0}.facts span{border:1px solid #33454a;padding:9px 14px;border-radius:4px;font-size:14px}img{max-width:100%;display:block}.hero{width:100%;border:1px solid #2f4248;cursor:zoom-in}.caption{font-size:13px;margin:10px 0 28px;color:#97aeaf}.pair{display:grid;grid-template-columns:1.5fr 1fr;gap:24px;align-items:start}.pair img{width:100%;cursor:zoom-in}.art{display:grid;grid-template-columns:1fr 1.5fr 1fr;gap:20px;align-items:center}.art img{max-height:500px;object-fit:contain;cursor:zoom-in}nav{display:flex;gap:10px;flex-wrap:wrap;margin:20px 0}button{font:inherit;color:inherit;background:#17262c;border:1px solid #40565c;border-radius:5px;padding:10px 18px;cursor:pointer}button[aria-pressed=true]{background:#bdded6;color:#102329}.grid{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:22px}.card{border:1px solid #304249;background:#142127;overflow:hidden}.card>button{border:0;padding:0;width:100%;border-radius:0;background:#213037;cursor:zoom-in}.card img{width:100%;aspect-ratio:1.4;object-fit:contain}.info{padding:18px}.info h3{font-size:18px;margin:0 0 8px;font-weight:550}.info p{font-size:12px;margin:3px 0}.note{border-left:3px solid #8dafab;padding:0 20px;margin:40px 0}dialog{padding:16px;background:#0d1519;border:1px solid #456066;max-width:96vw;max-height:96vh}dialog::backdrop{background:#000c}dialog img{max-height:80vh;max-width:90vw;margin:auto}dialog button{display:block;margin:10px 0 10px auto}.links{display:flex;gap:24px;flex-wrap:wrap;margin-top:26px}footer{border-top:1px solid #2d4146;margin-top:50px;padding-top:20px;color:#8ba5a8;font-size:13px}@media(max-width:800px){main{padding:30px 18px}.grid{grid-template-columns:repeat(2,minmax(0,1fr))}.pair{grid-template-columns:1fr}}@media(max-width:500px){.grid{grid-template-columns:1fr}.art{gap:8px}}
</style><main><header><div class="eyebrow">Vector Rush / September 17, 2026</div><h1>Night city.<br>Built for a closer look.</h1><p>A reusable set of architecture, rooftop machinery, sign hardware and surface materials drawn from the teal-and-red city reference. Prepared as an asset review candidate.</p><div class="facts"><span>19 modular prefabs</span><span>38 LOD meshes</span><span>33 Unity materials</span><span>3 Scenario artworks</span></div></header>
<img class="hero zoom" src="images/unity-assembly.jpg" alt="Actual standalone Unity asset inspection scene"><p class="caption">Native Unity capture · dedicated asset inspection scene · no racing integration or motion blur.</p>
<div class="pair"><div><h2>Details that survive a slow look</h2><img class="zoom" src="images/unity-rooftop.jpg" alt="Native Unity close-up of roof equipment and sign construction"><p class="caption">Native Unity close-up: mounting hardware, scan-based roof material and localized red light.</p></div><div><h2>The reference</h2><img class="zoom" src="images/owner-reference.jpg" alt="Owner supplied night city reference"><p class="caption">Owner-supplied visual direction. The kit supplies reusable ingredients; this inspection scene is not the finished reference city.</p><p>Deep teal shadows, concentrated red light, occupied windows and layered rooftop infrastructure guide the material and shape choices.</p></div></div>
<h2>Inspect the kit</h2><p>Open an asset to inspect its latest neutral render. Material comparisons use the same light for dry and wet variants.</p><nav aria-label="Asset category"></nav><div id="grid" class="grid"></div>
<h2>Original advertising artwork</h2><p>Three full-resolution textures generated through Scenario, mounted on exported sign geometry. Lettering and artwork have been inspected.</p><div class="art"><img class="zoom" src="images/art-afterhours.jpg" alt="After Hours portrait advertisement"><img class="zoom" src="images/art-koi_market.jpg" alt="Night Market koi advertisement"><img class="zoom" src="images/art-transit.jpg" alt="Last Train transit advertisement"></div>
<div class="note"><h2>Ready for asset review</h2><p>Unity import checks cover scale, orientation, triangle counts, UVs, material bindings and LOD references. Native captures verify the rebuilt inspection scene. Blender close-ups show the source geometry and material pairs.</p><p>Race placement, driving-distance readability, animated billboard integration and full-city polish remain separate work. No performance or artistic acceptance is claimed.</p></div>
<div class="links"><a href="README.md">Source and rebuild guide</a><a href="inventory.json">File inventory</a><a href="import-validation.json">Unity import checks</a><a href="PROVENANCE.md">Artwork and material provenance</a></div>
<footer>Source assets and engine assets are retained together. The current game’s ship, HUD, handling, course and weather remain unchanged.</footer></main><dialog id="viewer"><button autofocus>Close</button><img alt=""><p></p></dialog><script>
const records=__DATA__, grid=document.querySelector('#grid'), nav=document.querySelector('nav'), dlg=document.querySelector('dialog');
function zoom(src,title){dlg.querySelector('img').src=src;dlg.querySelector('img').alt=title;dlg.querySelector('p').textContent=title;dlg.showModal()}
function render(category){grid.replaceChildren();for(const a of records.filter(x=>category==='All'||x.group===category)){const card=document.createElement('article');card.className='card';const b=document.createElement('button');b.setAttribute('aria-label','Inspect '+a.name);const im=document.createElement('img');im.src=a.image;im.alt=a.name;im.loading='lazy';b.append(im);b.onclick=()=>zoom(a.image,a.name+' · '+a.detail);const info=document.createElement('div');info.className='info';for(const [tag,value]of[['h3',a.name],['p',a.detail],['p',a.dimensions]]){const e=document.createElement(tag);e.textContent=value;info.append(e)}card.append(b,info);grid.append(card)}}
for(const c of ['All','Architecture','Equipment','Signs','Materials']){const b=document.createElement('button');b.textContent=c;b.setAttribute('aria-pressed',c==='All');b.onclick=()=>{for(const other of nav.children)other.setAttribute('aria-pressed',other===b);render(c)};nav.append(b)}
for(const im of document.querySelectorAll('.zoom')){im.tabIndex=0;im.onclick=()=>zoom(im.src,im.alt);im.onkeydown=e=>{if(e.key==='Enter')im.click()}}
dlg.querySelector('button').onclick=()=>dlg.close();dlg.onclick=e=>{if(e.target===dlg)dlg.close()};render('All');
</script></html>'''
(out/'index.html').write_text(page.replace('__DATA__',json.dumps(records)))
print('GALLERY_COMPLETE', len(records), 'entries')
