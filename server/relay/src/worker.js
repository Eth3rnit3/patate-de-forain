// Relais déployé : un Worker Cloudflare oriente chaque connexion vers le
// Durable Object de son salon (un objet par code). Mêmes règles que le relais
// local (rooms.js). L'API d'hibernation laisse l'objet s'endormir entre deux
// messages : un salon inactif ne coûte rien.
import {DurableObject} from 'cloudflare:workers';
import {admit, arrival, check, departure, FloodGate, forwardable, originAllowed, parseOrigins, Refusal, roomCode, welcome} from './rooms.js';

/** Ouvre puis ferme aussitôt une connexion : le navigateur ne lit que le code de fermeture, pas un statut HTTP. */
function refuse(refusal) {
  const [client, server] = Object.values(new WebSocketPair());
  server.accept();
  server.close(...refusal);
  return new Response(null, {status: 101, webSocket: client});
}

export default {
  async fetch(request, env) {
    if (request.headers.get('Upgrade') !== 'websocket')
      return new Response('Relais Patate de Forain.\n', {headers: {'content-type': 'text/plain; charset=utf-8', 'access-control-allow-origin': '*'}});
    if (!originAllowed(request.headers.get('Origin'), parseOrigins(env.ORIGINES)))
      return new Response('Origine refusée.\n', {status: 403});
    const code = roomCode(new URL(request.url).pathname);
    if (!code) return refuse(Refusal.invalidCode);
    return env.SALONS.get(env.SALONS.idFromName(code)).fetch(request);
  },
};

export class Salon extends DurableObject {
  constructor(ctx, env) {
    super(ctx, env);
    /** Compteurs anti-inondation ; perdus à l'hibernation, ce qui est sans gravité. */
    this.gates = new Map();
  }

  open(except) {
    return this.ctx.getWebSockets().filter(ws => ws !== except && ws.readyState === WebSocket.OPEN);
  }

  async fetch(request) {
    const others = this.open();
    const taken = others.map(ws => ws.deserializeAttachment()?.slot).filter(slot => slot !== undefined);
    const decision = admit(taken, new URL(request.url).searchParams.has('rejoindre'));
    if (decision.refusal) return refuse(decision.refusal);

    const [client, server] = Object.values(new WebSocketPair());
    this.ctx.acceptWebSocket(server);
    server.serializeAttachment({slot: decision.slot});
    server.send(welcome(decision.slot, others.length + 1));
    for (const ws of others) ws.send(arrival);
    return new Response(null, {status: 101, webSocket: client});
  }

  async webSocketMessage(ws, message) {
    if (!this.gates.has(ws)) this.gates.set(ws, new FloodGate());
    const refusal = check(message) ?? (this.gates.get(ws).pass() ? null : Refusal.flood);
    if (refusal) return ws.close(...refusal);
    const text = forwardable(message);
    if (text === null) return;
    for (const peer of this.open(ws)) peer.send(text);
  }

  async webSocketClose(ws, code) {
    this.leave(ws);
    try { ws.close(code === 1005 ? 1000 : code, 'fin'); } catch { /* déjà fermée */ }
  }

  async webSocketError(ws) {
    this.leave(ws);
  }

  leave(ws) {
    this.gates.delete(ws);
    for (const peer of this.open(ws)) peer.send(departure);
  }
}
