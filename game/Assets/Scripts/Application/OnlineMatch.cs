using System;
using System.Collections.Generic;
using Patate.Combat;

namespace Patate.Application
{
    /// <summary>
    /// Combat en ligne à deux, sans serveur de jeu : chaque machine simule
    /// toute la manche à partir des commandes des deux joueurs. Seules les
    /// commandes circulent, avec un délai d'entrée fixe : celle lue au tick t
    /// est jouée au tick t + délai, ce qui laisse au réseau le temps de
    /// l'apporter. Si elle tarde, le joueur continue jusqu'à
    /// <c>maxRollback</c> ticks en prédisant que l'adversaire maintient ses
    /// dernières touches ; à l'arrivée d'une commande différente, l'état est
    /// restauré au tick fautif et la suite resimulée (rollback). Au-delà, il
    /// attend. L'état est immuable (Simulation.Step ne modifie pas l'état
    /// reçu) : l'historique ne garde que des références.
    ///
    /// Les deux joueurs comparent régulièrement l'empreinte des états
    /// confirmés (commandes réelles des deux côtés) : une différence signale
    /// une désynchronisation et arrête le combat.
    ///
    /// Les événements (sons, effets) ne sont émis qu'à la première simulation
    /// d'un tick ; ceux d'une resimulation sont perdus, comme dans la plupart
    /// des jeux à rollback.
    /// </summary>
    public sealed class OnlineMatch
    {
        readonly SimulationContext context;
        readonly Action<string> send;
        readonly int localSlot, delay, maxRollback, checksumInterval, batch;
        // Indice = tick (1, 2…) ; l'indice 0 est inutilisé.
        readonly List<Buttons> local = new List<Buttons> { Buttons.None };
        readonly List<Buttons> remote = new List<Buttons> { Buttons.None };
        readonly List<Buttons> remoteUsed = new List<Buttons> { Buttons.None };
        readonly MatchState[] history;
        readonly Dictionary<int, string> localSums = new Dictionary<int, string>(), remoteSums = new Dictionary<int, string>();
        readonly List<MatchEvent> discarded = new List<MatchEvent>();
        readonly List<Buttons> parsed = new List<Buttons>();
        int sent, confirmed, highestSimulated, mispredicted = int.MaxValue, remoteAdvantage;

        /// <summary>État affiché : confirmé, ou prédit au-delà de <see cref="ConfirmedTick"/>.</summary>
        public MatchState State { get; private set; }
        public int LocalSlot => localSlot;
        /// <summary>Dernier tick simulé avec les vraies commandes des deux joueurs.</summary>
        public int ConfirmedTick => confirmed;
        /// <summary>Dernier tick dont la commande de l'adversaire est connue.</summary>
        public int RemoteKnown => remote.Count - 1;
        /// <summary>Avance locale estimée sur l'adversaire, en ticks.</summary>
        public int LocalAdvantage => State.Tick - (RemoteKnown - delay);
        /// <summary>La manche est finie et confirmée : plus rien n'est simulé.</summary>
        public bool IsOver { get; private set; }
        /// <summary>Premier tick dont les empreintes diffèrent, ou null.</summary>
        public int? DesyncTick { get; private set; }
        /// <summary>Le dernier appel à Advance a dû attendre les commandes de l'adversaire.</summary>
        public bool Waiting { get; private set; }
        public int Rollbacks { get; private set; }
        public int ResimulatedTicks { get; private set; }
        public int SkippedTicks { get; private set; }
        public int ComparedChecksums { get; private set; }

        public OnlineMatch(SimulationContext context, MatchState initial, int localSlot, Action<string> send,
            int delay = OnlineProtocol.InputDelay, int maxRollback = OnlineProtocol.MaxRollback, int checksumInterval = OnlineProtocol.ChecksumInterval,
            int inputsPerMessage = OnlineProtocol.InputsPerMessage)
        {
            // Sinon chacun pourrait attendre une commande que l'autre garde pour compléter son lot.
            if (delay + maxRollback < inputsPerMessage || inputsPerMessage < 1)
                throw new ArgumentException("Le délai d'entrée plus la prédiction doivent couvrir un lot de commandes.");
            this.context = context;
            this.localSlot = localSlot;
            this.send = send;
            this.delay = delay;
            this.maxRollback = maxRollback;
            this.checksumInterval = checksumInterval;
            batch = inputsPerMessage;
            history = new MatchState[maxRollback + 4];
            State = initial;
            history[initial.Tick % history.Length] = initial;
            // Les premiers ticks du délai n'ont de commande ni d'un côté ni de l'autre.
            for (int t = 1; t <= delay; t++)
            {
                local.Add(Buttons.None);
                remote.Add(Buttons.None);
            }
            sent = delay;
        }

        /// <summary>
        /// Une image : applique un éventuel rollback, puis avance d'au plus
        /// <paramref name="ticks"/> ticks avec la commande locale lue à cette
        /// image et envoie les commandes nouvelles à l'adversaire.
        /// </summary>
        /// <param name="events">Reçoit (en plus de son contenu) les événements des ticks simulés pour la première fois.</param>
        public void Advance(int ticks, Buttons localInput, List<MatchEvent> events)
        {
            if (IsOver || DesyncTick.HasValue) return;
            Rollback();
            // Même sans simuler (attente, aucun tick), les commandes reçues confirment des ticks : fin de manche, empreintes.
            Confirm();
            Waiting = false;
            // Synchronisation du temps : si l'on est nettement en avance, on laisse l'adversaire rattraper.
            if (ticks > 0 && LocalAdvantage - remoteAdvantage >= 4)
            {
                ticks--;
                SkippedTicks++;
            }
            for (int i = 0; i < ticks && !IsOver; i++)
            {
                int next = State.Tick + 1;
                if (next - RemoteKnown > maxRollback)
                {
                    Waiting = true;
                    break;
                }
                if (local.Count == next + delay) local.Add(localInput);
                Simulate(next, next > highestSimulated ? events : discarded);
                Confirm();
            }
            Flush();
        }

        /// <summary>Message de l'adversaire concernant le combat (commandes, empreintes). Renvoie faux s'il ne le concerne pas.</summary>
        public bool Receive(string message)
        {
            var parts = OnlineProtocol.Split(message);
            switch (parts[0])
            {
                case OnlineProtocol.Inputs:
                    if (!OnlineProtocol.TryParseInputs(parts, out int first, out int advantage, parsed)) return true;
                    remoteAdvantage = advantage;
                    for (int i = 0; i < parsed.Count; i++)
                    {
                        int tick = first + i;
                        // Flux ordonné et sans perte (WebSocket) : seul le tick suivant est attendu.
                        if (tick != RemoteKnown + 1) continue;
                        remote.Add(parsed[i]);
                        if (tick <= State.Tick && remoteUsed[tick] != parsed[i]) mispredicted = Math.Min(mispredicted, tick);
                    }
                    return true;
                case OnlineProtocol.Checksum:
                    if (parts.Length >= 3 && OnlineProtocol.TryInt(parts[1], out int at))
                    {
                        remoteSums[at] = parts[2];
                        Compare(at);
                    }
                    return true;
                default:
                    return false;
            }
        }

        void Simulate(int tick, List<MatchEvent> events)
        {
            Buttons other = tick <= RemoteKnown ? remote[tick] : remote[RemoteKnown];
            if (remoteUsed.Count == tick) remoteUsed.Add(other);
            else remoteUsed[tick] = other;
            if (events == discarded) discarded.Clear();
            State = localSlot == 0
                ? Simulation.Step(State, local[tick], other, context, events)
                : Simulation.Step(State, other, local[tick], context, events);
            history[tick % history.Length] = State;
            highestSimulated = Math.Max(highestSimulated, tick);
        }

        void Rollback()
        {
            if (mispredicted > State.Tick)
            {
                mispredicted = int.MaxValue;
                return;
            }
            int target = State.Tick;
            State = history[(mispredicted - 1) % history.Length];
            for (int tick = mispredicted; tick <= target; tick++) Simulate(tick, discarded);
            Rollbacks++;
            ResimulatedTicks += target - mispredicted + 1;
            mispredicted = int.MaxValue;
            Confirm();
        }

        /// <summary>Note les ticks nouvellement confirmés : empreintes à comparer, fin de manche.</summary>
        void Confirm()
        {
            int reachable = Math.Min(State.Tick, RemoteKnown);
            while (confirmed < reachable)
            {
                confirmed++;
                var state = history[confirmed % history.Length];
                bool over = state.Round.Phase == RoundPhase.Over;
                if (confirmed % checksumInterval == 0 || over)
                {
                    localSums[confirmed] = StateCodec.Checksum(state);
                    send(OnlineProtocol.Words(OnlineProtocol.Checksum, confirmed, localSums[confirmed]));
                    Compare(confirmed);
                }
                if (over)
                {
                    // Les ticks prédits au-delà de la fin sont abandonnés : les deux joueurs s'arrêtent au même état.
                    IsOver = true;
                    State = state;
                    return;
                }
            }
        }

        void Compare(int tick)
        {
            if (!localSums.TryGetValue(tick, out var mine) || !remoteSums.TryGetValue(tick, out var theirs)) return;
            localSums.Remove(tick);
            remoteSums.Remove(tick);
            ComparedChecksums++;
            if (mine != theirs && (!DesyncTick.HasValue || tick < DesyncTick)) DesyncTick = tick;
        }

        void Flush()
        {
            int count = local.Count - 1 - sent;
            if (count < batch) return;
            send(OnlineProtocol.FormatInputs(sent + 1, LocalAdvantage, local, sent + 1, count));
            sent += count;
        }

        /// <summary>Commandes des deux joueurs, par tick confirmé (replay de la manche jouée).</summary>
        public IEnumerable<(Buttons, Buttons)> ConfirmedInputs()
        {
            for (int tick = 1; tick <= confirmed; tick++)
                yield return localSlot == 0 ? (local[tick], remote[tick]) : (remote[tick], local[tick]);
        }
    }
}
