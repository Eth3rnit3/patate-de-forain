using System;
using System.Collections.Generic;
using System.Linq;
using Patate.Combat;

namespace Patate.Tests
{
    /// <summary>
    /// Combattant fictif, volontairement sans lien avec Saint Louis : si les
    /// règles fonctionnent avec lui, elles ne dépendent d'aucun personnage.
    /// </summary>
    public static class SyntheticFighter
    {
        public static readonly HitProperties JabHit = new HitProperties(
            damage: 50, chipDamage: 0, hitstun: 12, blockstun: 8, hitstop: 6, pushbackHit: 40, pushbackBlock: 30, level: HitLevel.High, blockable: true);

        public static readonly HitProperties OrbHit = JabHit.With(damage: 80);

        public const int JabDuration = 12;
        public const int JabFirstActive = 4;
        public const int OrbSpawnTick = 10;

        public static FighterGameplay Gameplay(HitProperties jabHit = null, int orbSpeed = 10, int orbLifetime = 120) => new FighterGameplay(
            new FighterStats(health: 1000, walkForwardSpeed: 8, walkBackSpeed: 6),
            new BodyDefinition(new Box(-60, 0, 120, 400), new[] { new Box(-70, 0, 140, 420) }),
            new[]
            {
                new MoveDefinition("jab", new[] { new CommandDefinition(Buttons.LightPunch) }, 0, new[]
                {
                    new SegmentDefinition(Phase.Startup, 4),
                    new SegmentDefinition(Phase.Active, 2, hitboxes: new[] { new Box(60, 300, 120, 40) }),
                    new SegmentDefinition(Phase.Recovery, 6),
                }, jabHit ?? JabHit),
                new MoveDefinition("orb_throw", new[]
                {
                    new CommandDefinition(Buttons.Special),
                    new CommandDefinition(Buttons.LightPunch, motion: new[] { 2, 3, 6 }, window: 15),
                }, 10, new[]
                {
                    new SegmentDefinition(Phase.Startup, OrbSpawnTick),
                    new SegmentDefinition(Phase.Recovery, 20, onEnter: new SegmentAction[] { new SpawnProjectileAction("orb", new PointI(100, 300)) }),
                }, null),
            },
            new[] { new ProjectileDefinition("orb", orbSpeed, orbLifetime, new Box(-30, -30, 60, 60), 1, OrbHit) });

        public static CompiledFighter Compile(FighterGameplay gameplay = null) =>
            new CompiledFighter(new DefinitionRef("mannequin", "0.0.1", "test"), gameplay ?? Gameplay());
    }

    /// <summary>Petit banc d'essai : un combat prêt à se battre, des entrées scriptées, les événements collectés.</summary>
    public sealed class Bench
    {
        public MatchState State;
        public readonly SimulationContext Context;
        public readonly List<MatchEvent> Events = new List<MatchEvent>();

        public Bench(int? gap = null, FighterGameplay gameplay = null, MatchRules rules = null)
        {
            var fighter = SyntheticFighter.Compile(gameplay);
            Context = new SimulationContext(rules ?? MatchRules.Default(), fighter, fighter);
            State = MatchState.Create(Context.Rules, fighter.Ref, 1000, fighter.Ref, 1000);
            State.Round.Phase = RoundPhase.Fight;
            if (gap.HasValue)
            {
                State.Fighters[0].X = -gap.Value / 2;
                State.Fighters[1].X = State.Fighters[0].X + gap.Value;
            }
        }

        public FighterState P1 => State.Fighters[0];
        public FighterState P2 => State.Fighters[1];

        /// <summary>Avance <paramref name="ticks"/> fois ; le script reçoit l'indice relatif du tick (0, 1, 2…).</summary>
        public Bench Run(int ticks, Func<int, (Buttons, Buttons)> script = null, Action<MatchState, List<MatchEvent>, int> onTick = null)
        {
            for (int index = 0; index < ticks; index++)
            {
                var (first, second) = script?.Invoke(index) ?? (Buttons.None, Buttons.None);
                var tickEvents = new List<MatchEvent>();
                State = Simulation.Step(State, first, second, Context, tickEvents);
                Events.AddRange(tickEvents);
                onTick?.Invoke(State, tickEvents, index);
            }
            return this;
        }

        public int Count(MatchEventType type) => Events.Count(e => e.Type == type);

        public static Buttons Tap(int index, int at, Buttons buttons) => index == at ? buttons : Buttons.None;
    }
}
