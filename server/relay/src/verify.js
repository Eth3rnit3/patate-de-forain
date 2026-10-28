// Vérifie un relais en marche, local ou déployé :
//   npm run verifier -- wss://patate-relais.<compte>.workers.dev
import assert from 'node:assert/strict';
import {WebSocket} from 'ws';
import {Refusal} from './rooms.js';

/** Client de test : file des messages reçus et attente du suivant. */
function client(url) {
  const ws = new WebSocket(url);
  const inbox = [];
  const waiting = [];
  ws.on('message', data => {
    const text = data.toString();
    const next = waiting.shift();
    if (next) next(text); else inbox.push(text);
  });
  ws.closed = new Promise(resolve => ws.on('close', (code, reason) => resolve({code, reason: reason.toString()})));
  ws.next = () => inbox.length ? Promise.resolve(inbox.shift()) : new Promise(resolve => waiting.push(resolve));
  return ws;
}

/** Deux joueurs dans un salon, un troisième refusé, un départ signalé, un salon inconnu refusé. */
export async function scenario(base) {
  const code = 'VERIF' + Math.floor(Math.random() * 1e6);
  const host = client(`${base}/salon/${code}`);
  assert.equal(await host.next(), '#bienvenue 0 1');

  const guest = client(`${base}/salon/${code.toLowerCase()}?rejoindre`);
  assert.equal(await guest.next(), '#bienvenue 1 2');
  assert.equal(await host.next(), '#arrivee');

  guest.send('bonjour 1');
  assert.equal(await host.next(), 'bonjour 1');
  host.send('#depart');
  host.send('i 1 0 0');
  assert.equal(await guest.next(), 'i 1 0 0', 'le faux message du relais est ignoré');

  const third = client(`${base}/salon/${code}`);
  assert.deepEqual(await third.closed, {code: Refusal.full[0], reason: Refusal.full[1]});

  guest.close();
  assert.equal(await host.next(), '#depart');
  host.close();

  const lost = client(`${base}/salon/${code}X?rejoindre`);
  assert.equal((await lost.closed).code, Refusal.unknown[0]);
}

if (import.meta.url === `file://${process.argv[1]}`) {
  const base = (process.argv[2] || 'ws://localhost:8787').replace(/\/$/, '');
  await scenario(base);
  console.log(`Relais ${base} : conforme.`);
  process.exit(0);
}
