// Usage: node tools/render-work-record.mjs /absolute/path/to/marked/lib/marked.esm.js
// Produces a local HTML reading edition without changing linked source evidence.
import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const { marked } = await import(process.argv[2] ? pathToFileURL(process.argv[2]).href : 'marked');
const source = fs.readFileSync(path.join(root, 'docs/development-work-record.md'), 'utf8');
const sections = [];
let count = 0;
let body = marked.parse(source).replace(/<h([23])>([\s\S]*?)<\/h\1>/g, (_, level, label) => {
  const id = `section-${++count}`;
  if (level === '2') sections.push({ id, label: label.replace(/<[^>]*>/g, '') });
  return `<h${level} id="${id}">${label}</h${level}>`;
});
body = body.replace(/<img /g, '<img loading="lazy" ');
body = body.replace(/<table>/g, '<div class="table-scroll"><table>').replace(/<\/table>/g, '</table></div>');
const toc = sections.map(s => `<a href="#${s.id}">${s.label}</a>`).join('\n');
const html = `<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
<title>Vector Rush — Complete development work record</title>
<style>
:root{color-scheme:light;--ink:#172d34;--muted:#54686d;--accent:#176e70;--line:#d6dfdc;--paper:#f9faf6}*{box-sizing:border-box}html{scroll-behavior:smooth;scroll-padding-top:35px}body{margin:0;background:var(--paper);color:var(--ink);font:16px/1.75 -apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif}a{color:var(--accent);text-underline-offset:3px}a:hover{color:#0a4546}header{padding:25px 4vw;border-bottom:1px solid var(--line);display:flex;justify-content:space-between;align-items:center;gap:16px}header strong{letter-spacing:.18em;font-size:13px}header span{font-size:12px;color:var(--muted)}.layout{display:grid;grid-template-columns:270px minmax(0,1fr);max-width:1480px;margin:auto}aside{position:sticky;top:0;height:100vh;overflow:auto;padding:36px 22px 36px 28px;border-right:1px solid var(--line)}aside p{font-size:11px;letter-spacing:.1em;text-transform:uppercase;color:var(--muted);margin:0 0 17px}aside a{display:block;text-decoration:none;line-height:1.4;font-size:13px;padding:8px 10px;margin:2px 0;border-left:2px solid transparent}aside a:hover,aside a.active{background:#eaf0ea;border-left-color:var(--accent)}main{min-width:0;max-width:1030px;padding:52px 60px 100px}h1{font-size:clamp(35px,4.2vw,58px);line-height:1.08;letter-spacing:-.04em;font-weight:650;max-width:760px;margin:0 0 30px}h2{font-size:29px;line-height:1.25;letter-spacing:-.02em;margin:66px 0 25px;padding-top:26px;border-top:1px solid var(--line)}h3{font-size:21px;line-height:1.4;margin:38px 0 16px}p{margin:0 0 20px}li{margin:8px 0}code{font: .84em ui-monospace,SFMono-Regular,monospace;overflow-wrap:anywhere;background:#eaf0e9;padding:2px 4px;border-radius:3px}pre{overflow:auto;padding:18px;background:#eaf0e9}table{border-collapse:collapse;width:100%;font-size:13px;line-height:1.6}th,td{text-align:left;vertical-align:top;border-bottom:1px solid var(--line);padding:12px 10px;min-width:100px}th{background:#eaf0e9;font-weight:650}.table-scroll{overflow:auto;margin:24px 0 30px}img{display:block;width:100%;height:auto;margin:34px 0 12px;background:#eaf0e9}p:has(>em:only-child){font-size:13px;color:var(--muted);line-height:1.6}button{background:none;color:var(--ink);border:1px solid #9faeaa;padding:8px 14px;border-radius:4px;cursor:pointer;font:inherit;font-size:12px}.document-meta{font:12px/1.6 ui-monospace,monospace;color:var(--muted);margin-bottom:24px}footer{border-top:1px solid var(--line);padding:25px 4vw;color:var(--muted);font-size:12px}@media(max-width:1000px){.layout{grid-template-columns:220px minmax(0,1fr)}main{padding:38px 30px}aside{padding:25px 15px}}@media(max-width:720px){.layout{display:block}aside{position:static;height:auto;max-height:270px;border-right:0;border-bottom:1px solid var(--line)}main{padding:32px 20px 60px}header{padding:18px 20px}header span{display:none}h2{font-size:25px}table{font-size:12px}}@media print{aside,header button{display:none}.layout{display:block}main{max-width:none;padding:18px 0}body{background:white;font-size:10pt}h1{font-size:30pt}h2{font-size:18pt;break-after:avoid}h3{font-size:13pt;break-after:avoid}img,tr{break-inside:avoid}.table-scroll{overflow:visible}table{font-size:8pt}a{color:inherit}footer{padding:10px 0}}
</style></head><body>
<header><strong>VECTOR RUSH / FIELD RECORD</strong><span>SEPTEMBER 7–8, 2026 · SOURCE, DECISIONS & EVIDENCE</span><button onclick="window.print()">Print / Save PDF</button></header>
<div class="layout"><aside aria-label="Contents"><p>Contents · use ⌘F to search</p>${toc}<p style="margin-top:24px"><a href="development-work-record.md">Markdown source ↗</a></p></aside><main><div class="document-meta">60 historical milestones · checkpoint 3097bc8 · ${source.split(/\s+/).length.toLocaleString('en-US')} words<br>Original evidence is linked from this local repository.</div>${body}</main></div>
<footer>Historical record through checkpoint 3097bc8. Linked native captures, recipes and critiques retain their own acceptance limits.</footer>
<script>const links=[...document.querySelectorAll('aside a[href^="#"]')];const observer=new IntersectionObserver(entries=>{for(const e of entries){if(e.isIntersecting){links.forEach(a=>a.classList.toggle('active',a.hash==='#'+e.target.id));}}},{rootMargin:'0px 0px -65% 0px'});document.querySelectorAll('h2[id]').forEach(h=>observer.observe(h));</script>
</body></html>`;
fs.writeFileSync(path.join(root, 'docs/development-work-record.html'), html);
console.log(JSON.stringify({ sections: sections.length, bytes: Buffer.byteLength(html), output: 'docs/development-work-record.html' }));
