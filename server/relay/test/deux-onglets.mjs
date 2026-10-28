// Essai de bout en bout : deux onglets d'un même Chrome (sans interface, donc
// jamais mis en pause) jouent deux manches en ligne via un relais en marche,
// en robots (?robot), avec une revanche entre les deux. Chaque onglet écrit sa
// fin de manche et son empreinte d'état dans la console ; elles doivent être
// identiques.
//   npm run essai:onglets -- http://localhost:8000/ [dossier des captures, sinon le dossier temporaire]
// Chrome : variable CHROME, sinon l'emplacement habituel sous macOS.
import os from 'node:os';
import {chromium} from 'playwright-core';

const base = process.argv[2] || 'http://localhost:8799/';
const out = process.argv[3] || os.tmpdir();
const code = 'TAB' + Math.floor(Math.random() * 1e5);
const browser = await chromium.launch({
  executablePath: process.env.CHROME || '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome',
  headless: true,
  args: ['--disable-background-timer-throttling', '--disable-renderer-backgrounding', '--disable-backgrounding-occluded-windows', '--ignore-gpu-blocklist', '--enable-unsafe-swiftshader'],
});
const context = await browser.newContext({viewport: {width: 1280, height: 720}});
const reports = {A: [], B: []};
function watch(page, name) {
  page.on('console', m => {
    const t = m.text();
    if (/En ligne|xception|rror/.test(t) && !/getInternalformatParameter/.test(t)) {
      reports[name].push(t);
      console.log(`[${name}] ${t}`);
    }
  });
  page.on('pageerror', e => console.log(`[${name}] erreur de page : ${e.message}`));
}
const a = await context.newPage();
watch(a, 'A');
await a.goto(`${base}?salon=${code}&creer&robot`);
const start = Date.now();
while (!reports.A.some(t => t.includes('créer')) && Date.now() - start < 60000) await a.waitForTimeout(500);
await a.waitForTimeout(1500);
const b = await context.newPage();
watch(b, 'B');
await b.goto(`${base}?salon=${code}&robot`);

const fps = page => page.evaluate(() => new Promise(r => { let n = 0; const t0 = performance.now(); const f = () => { n++; performance.now() - t0 < 1000 ? requestAnimationFrame(f) : r(n); }; requestAnimationFrame(f); }));
const finished = (k, round) => reports[k].find(t => new RegExp(`manche ${round} : (finie|désynchronisation)`).test(t));
const sum = t => (/empreinte (\w+)/.exec(t || '') || [])[1];
let ok = true;
for (const round of [1, 2]) {
  while (!(finished('A', round) && finished('B', round)) && Date.now() - start < 600000) {
    await a.waitForTimeout(15000);
    console.log(`… manche ${round}, ${Math.round((Date.now() - start) / 1000)} s, images/s : A ${await fps(a)}, B ${await fps(b)}`);
  }
  await a.screenshot({path: `${out}/onglet-A-manche${round}.png`});
  await b.screenshot({path: `${out}/onglet-B-manche${round}.png`});
  const [ea, eb] = [finished('A', round), finished('B', round)];
  const same = sum(ea) && sum(ea) === sum(eb) && /aucune désynchronisation/.test(ea) && /aucune désynchronisation/.test(eb);
  console.log(`\nManche ${round}\nA : ${ea}\nB : ${eb}\n${same ? 'MÊME ÉTAT FINAL' : 'ÉTATS DIFFÉRENTS OU MANCHE INACHEVÉE'}\n`);
  ok = ok && same;
  if (round === 1) {
    // Revanche : Entrée d'un côté, puis de l'autre un peu plus tard.
    const enter = async page => { await page.keyboard.down('Enter'); await page.waitForTimeout(150); await page.keyboard.up('Enter'); };
    await enter(a);
    await a.waitForTimeout(2000);
    await enter(b);
  }
}
console.log(ok ? 'RÉSULTAT : DEUX MANCHES SANS DÉSYNCHRONISATION' : 'RÉSULTAT : ÉCHEC');
await browser.close();
process.exit(ok ? 0 : 1);
