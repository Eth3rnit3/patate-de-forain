import {test} from 'node:test';
import assert from 'node:assert/strict';
import {admit, FloodGate, forwardable, MAX_MESSAGES_PER_SECOND, originAllowed, parseOrigins, Refusal, roomCode} from '../src/rooms.js';
import {startRelay} from '../src/local.js';
import {scenario} from '../src/verify.js';

test('le code de salon se lit dans le chemin, en majuscules', () => {
  assert.equal(roomCode('/salon/ab3k9z'), 'AB3K9Z');
  assert.equal(roomCode('/salon/AB3K9Z/'), 'AB3K9Z');
  assert.equal(roomCode('/salon/a'), null);
  assert.equal(roomCode('/salon/a-b-c-d'), null);
  assert.equal(roomCode('/autre/ABCD'), null);
});

test('deux places par salon, et rejoindre refuse un salon vide', () => {
  assert.deepEqual(admit([], false), {slot: 0});
  assert.deepEqual(admit([0], false), {slot: 1});
  assert.deepEqual(admit([1], true), {slot: 0});
  assert.deepEqual(admit([0, 1], false), {refusal: Refusal.full});
  assert.deepEqual(admit([], true), {refusal: Refusal.unknown});
});

test('un joueur ne peut pas imiter un message du relais', () => {
  assert.equal(forwardable('#depart'), null);
  assert.equal(forwardable(''), null);
  assert.equal(forwardable('i 12 0 10'), 'i 12 0 10');
});

test('les messages sont limités par seconde', () => {
  let now = 0;
  const gate = new FloodGate(() => now);
  for (let i = 0; i < MAX_MESSAGES_PER_SECOND; i++) assert.ok(gate.pass());
  assert.equal(gate.pass(), false);
  now = 1000;
  assert.ok(gate.pass());
});

test('seules les pages du jeu passent, les clients sans origine aussi', () => {
  const allowed = parseOrigins('https://eth3rnit3.github.io, http://localhost:8000');
  assert.ok(originAllowed('https://eth3rnit3.github.io', allowed));
  assert.ok(originAllowed('http://localhost:8000', allowed));
  assert.ok(!originAllowed('https://exemple.com', allowed));
  assert.ok(originAllowed(undefined, allowed), 'version PC et éditeur : pas d\'en-tête Origin');
  assert.ok(originAllowed('https://exemple.com', parseOrigins('')), 'liste vide : toutes');
});

test('le relais local réunit deux joueurs et transmet leurs messages', async () => {
  const relay = await startRelay({port: 0, log: () => {}});
  try {
    await scenario(`ws://localhost:${relay.port}`);
  } finally {
    await relay.close();
  }
});
