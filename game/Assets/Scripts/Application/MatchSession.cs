using System;
using System.Collections.Generic;
using Patate.Combat;
using Patate.Content;

namespace Patate.Application
{
    /// <summary>
    /// Fournit les commandes d'un joueur tick par tick. Le clavier local, une
    /// IA, un replay ou un futur transport réseau implémentent cette même
    /// interface : la session ne sait pas d'où viennent les entrées.
    /// </summary>
    public interface IInputProvider
    {
        Buttons InputFor(int tick);
    }

    /// <summary>Entrées calculées par tick : replays, démonstrations, scripts de vérification.</summary>
    public sealed class ScriptedInput : IInputProvider
    {
        readonly Func<int, Buttons> script;

        public ScriptedInput(Func<int, Buttons> script)
        {
            this.script = script;
        }

        public Buttons InputFor(int tick) => script(tick);
    }

    /// <summary>
    /// Cas d'usage d'un combat : lancer avec deux combattants choisis, avancer
    /// la simulation d'un tick, recommencer. Conserve le journal des entrées,
    /// suffisant pour rejouer la manche à l'identique.
    /// </summary>
    public sealed class MatchSession
    {
        readonly List<MatchEvent> events = new List<MatchEvent>();
        readonly List<(Buttons, Buttons)> inputLog = new List<(Buttons, Buttons)>();

        public FighterPackage[] Packages { get; }
        public SimulationContext Context { get; }
        public MatchState State { get; private set; }
        /// <summary>Entrées de chaque tick depuis le début de la manche (replay).</summary>
        public IReadOnlyList<(Buttons, Buttons)> InputLog => inputLog;

        MatchSession(FighterPackage first, FighterPackage second, MatchRules rules)
        {
            Packages = new[] { first, second };
            Context = new SimulationContext(rules, first.Compile(), second.Compile());
            Restart();
        }

        /// <summary>
        /// Sélectionne les combattants et lance la manche. Refuse un combattant
        /// inconnu ou non admis par la politique de statistiques du mode.
        /// </summary>
        public static MatchSession Start(FighterCatalog catalog, string firstId, string secondId, MatchRules rules, StatPolicy policy)
        {
            var first = catalog.Get(firstId) ?? throw new ArgumentException($"Combattant inconnu : {firstId}");
            var second = catalog.Get(secondId) ?? throw new ArgumentException($"Combattant inconnu : {secondId}");
            foreach (var package in new[] { first, second })
            {
                var problems = policy.Check(package);
                if (problems.Count > 0) throw new InvalidOperationException($"{package.Name} n'est pas admis en mode « {policy.Id} » : {string.Join(" ; ", problems)}");
            }
            return new MatchSession(first, second, rules);
        }

        public void Restart()
        {
            var a = Context.Fighters[0];
            var b = Context.Fighters[1];
            State = MatchState.Create(Context.Rules, a.Ref, a.Gameplay.Stats.Health, b.Ref, b.Gameplay.Stats.Health);
            inputLog.Clear();
        }

        /// <summary>Avance d'un tick. Les événements renvoyés ne sont valides que jusqu'au prochain appel.</summary>
        public IReadOnlyList<MatchEvent> Step(Buttons first, Buttons second)
        {
            events.Clear();
            inputLog.Add((first, second));
            State = Simulation.Step(State, first, second, Context, events);
            return events;
        }

        public IReadOnlyList<MatchEvent> Step(IInputProvider first, IInputProvider second)
        {
            int tick = State.Tick + 1;
            return Step(first.InputFor(tick), second.InputFor(tick));
        }

        public bool IsOver => State.Round.Phase == RoundPhase.Over;
    }

    /// <summary>
    /// Pas fixe indépendant du rendu : convertit le temps écoulé en nombre de
    /// ticks à simuler. Plafonné pour ne pas « rattraper » après une pause.
    /// </summary>
    public sealed class FixedStepClock
    {
        readonly double tickDuration;
        readonly int maxTicksPerUpdate;
        double accumulator;

        public FixedStepClock(int ticksPerSecond, int maxTicksPerUpdate = 4)
        {
            tickDuration = 1.0 / ticksPerSecond;
            this.maxTicksPerUpdate = maxTicksPerUpdate;
        }

        public int Consume(double elapsedSeconds)
        {
            accumulator += Math.Max(0, elapsedSeconds);
            int ticks = (int)(accumulator / tickDuration);
            if (ticks > maxTicksPerUpdate)
            {
                ticks = maxTicksPerUpdate;
                accumulator = 0;
            }
            else accumulator -= ticks * tickDuration;
            return ticks;
        }

        public void Reset() => accumulator = 0;
    }
}
