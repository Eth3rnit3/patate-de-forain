// Relais local pour jouer et tester sans rien déployer :
//   npm start                  (ws://localhost:8787/salon/<code>)
//   RETARD_MS=60 npm start     (chaque message transmis avec 60 ms de retard : essais de latence)
// Mêmes règles que le relais Cloudflare (rooms.js).
import http from 'node:http';
import {WebSocketServer} from 'ws';
import {admit, arrival, check, departure, FloodGate, forwardable, originAllowed, parseOrigins, Refusal, roomCode, welcome} from './rooms.js';

export function startRelay({port = 8787, origins = [], delay = 0, log = console.log} = {}) {
  /** code → Map(place → socket) */
  const rooms = new Map();

  const server = http.createServer((request, response) => {
    response.writeHead(200, {'content-type': 'text/plain; charset=utf-8', 'access-control-allow-origin': '*'});
    response.end(`Relais Patate de Forain : ${rooms.size} salon(s) ouvert(s).\n`);
  });
  const sockets = new WebSocketServer({noServer: true, maxPayload: 64 * 1024});

  server.on('upgrade', (request, socket, head) => {
    const url = new URL(request.url, 'http://relais');
    const code = roomCode(url.pathname);
    if (!originAllowed(request.headers.origin, origins)) {
      socket.end('HTTP/1.1 403 Forbidden\r\n\r\n');
      return;
    }
    sockets.handleUpgrade(request, socket, head, ws => {
      if (!code) return ws.close(...Refusal.invalidCode);
      const room = rooms.get(code) ?? new Map();
      const decision = admit([...room.keys()], url.searchParams.has('rejoindre'));
      if (decision.refusal) return ws.close(...decision.refusal);

      const slot = decision.slot;
      room.set(slot, ws);
      rooms.set(code, room);
      log(`[${code}] place ${slot} entre (${room.size}/2)`);
      ws.send(welcome(slot, room.size));
      for (const [other, peer] of room) if (other !== slot) peer.send(arrival);

      const gate = new FloodGate();
      ws.on('message', (data, isBinary) => {
        const text = isBinary ? null : data.toString();
        const refusal = check(text) ?? (gate.pass() ? null : Refusal.flood);
        if (refusal) return ws.close(...refusal);
        const message = forwardable(text);
        if (message === null) return;
        for (const [other, peer] of room) {
          if (other === slot) continue;
          // Même retard pour tous les messages : l'ordre est conservé.
          if (delay > 0) setTimeout(() => peer.readyState === peer.OPEN && peer.send(message), delay);
          else peer.send(message);
        }
      });
      ws.on('close', () => {
        if (room.get(slot) !== ws) return;
        room.delete(slot);
        log(`[${code}] place ${slot} sort (${room.size}/2)`);
        for (const peer of room.values()) peer.send(departure);
        if (room.size === 0) rooms.delete(code);
      });
    });
  });

  return new Promise(resolve => server.listen(port, () => resolve({
    port: server.address().port,
    close: () => new Promise(done => {
      for (const ws of sockets.clients) ws.terminate();
      server.close(done);
    }),
  })));
}

if (import.meta.url === `file://${process.argv[1]}`) {
  const port = Number(process.env.PORT || 8787);
  const delay = Number(process.env.RETARD_MS || 0);
  const relay = await startRelay({port, delay, origins: parseOrigins(process.env.ORIGINES)});
  console.log(`Relais Patate de Forain à l'écoute : ws://localhost:${relay.port}/salon/<code>${delay ? ` (retard simulé : ${delay} ms)` : ''}`);
}
