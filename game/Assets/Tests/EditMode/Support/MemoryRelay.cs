using System;
using System.Collections.Generic;
using Patate.Application;

namespace Patate.Tests
{
    /// <summary>
    /// Relais en mémoire aux mêmes règles que server/relay (deux places par
    /// salon, messages « # » du relais), avec une latence comptée en images
    /// de test et une gigue éventuelle. Comme une WebSocket, il ne perd ni ne
    /// réordonne rien.
    /// </summary>
    public sealed class MemoryRelay
    {
        readonly Dictionary<string, Socket[]> rooms = new Dictionary<string, Socket[]>();
        readonly Func<int, int> latency;
        int now;

        /// <param name="latency">Latence d'un message en images, selon son numéro d'ordre (gigue).</param>
        public MemoryRelay(Func<int, int> latency = null)
        {
            this.latency = latency ?? (_ => 0);
        }

        public int Delivered { get; private set; }

        /// <summary>Une image de test : les messages arrivés à échéance deviennent lisibles.</summary>
        public void Tick() => now++;

        public OnlineLink Join(string code, bool mustExist = false) => new OnlineLink(Connect(code, mustExist), code);

        public Socket Connect(string code, bool mustExist = false)
        {
            if (!rooms.TryGetValue(code, out var room)) rooms[code] = room = new Socket[2];
            var socket = new Socket(this, code);
            int present = (room[0] != null ? 1 : 0) + (room[1] != null ? 1 : 0);
            if (mustExist && present == 0) return socket.Refused("salon introuvable");
            int slot = room[0] == null ? 0 : room[1] == null ? 1 : -1;
            if (slot < 0) return socket.Refused("salon plein");
            room[slot] = socket;
            socket.Slot = slot;
            socket.Deliver($"#bienvenue {slot} {present + 1}");
            room[1 - slot]?.Deliver("#arrivee");
            return socket;
        }

        void Route(Socket from, string text)
        {
            if (text.Length == 0 || text[0] == '#') return;
            rooms[from.Code][1 - from.Slot]?.Deliver(text);
        }

        void Leave(Socket socket)
        {
            var room = rooms[socket.Code];
            if (room[socket.Slot] != socket) return;
            room[socket.Slot] = null;
            room[1 - socket.Slot]?.Deliver("#depart");
        }

        public sealed class Socket : IRelaySocket
        {
            readonly MemoryRelay relay;
            readonly Queue<(int at, string text)> inbox = new Queue<(int, string)>();
            int lastAt;

            public string Code { get; }
            public int Slot { get; set; } = -1;
            public RelaySocketState State { get; private set; } = RelaySocketState.Open;
            public string CloseReason { get; private set; }
            /// <summary>Messages envoyés par ce joueur, pour les vérifications.</summary>
            public List<string> Sent { get; } = new List<string>();

            public Socket(MemoryRelay relay, string code)
            {
                this.relay = relay;
                Code = code;
            }

            internal Socket Refused(string reason)
            {
                State = RelaySocketState.Closed;
                CloseReason = reason;
                return this;
            }

            internal void Deliver(string text)
            {
                // Ordre préservé : un message ne double jamais le précédent.
                lastAt = Math.Max(lastAt, relay.now + relay.latency(relay.Delivered++));
                inbox.Enqueue((lastAt, text));
            }

            public void Send(string text)
            {
                if (State != RelaySocketState.Open) return;
                Sent.Add(text);
                relay.Route(this, text);
            }

            public bool TryReceive(out string text)
            {
                if (inbox.Count > 0 && inbox.Peek().at <= relay.now)
                {
                    text = inbox.Dequeue().text;
                    return true;
                }
                text = null;
                return false;
            }

            public void Close()
            {
                if (State == RelaySocketState.Closed) return;
                State = RelaySocketState.Closed;
                relay.Leave(this);
            }
        }
    }
}
