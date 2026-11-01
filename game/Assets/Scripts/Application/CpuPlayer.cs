using System;
using System.Collections.Generic;
using Patate.Combat;

namespace Patate.Application
{
    /// <summary>
    /// Adversaire contrôlé par l'ordinateur. Avant chaque tick, lit le combat
    /// (distance, action adverse, projectiles) et produit les mêmes
    /// <see cref="Buttons"/> qu'un joueur. Ne connaît aucun personnage :
    /// portée, démarrage et projectiles sont lus dans le gameplay compilé.
    /// Le niveau règle le temps de réaction et la fréquence des bons choix.
    /// Hasard à graine (xorshift) : même graine et mêmes entrées adverses
    /// donnent la même partie, sur toutes les plateformes.
    /// </summary>
    public sealed class CpuPlayer : IInputProvider
    {
        sealed class Level
        {
            public string Name;
            /// <summary>Retard de perception de l'action adverse, en ticks.</summary>
            public int Reaction;
            /// <summary>Ticks entre deux choix de déplacement ou d'attaque.</summary>
            public int Interval;
            /// <summary>
            /// Probabilités en pourcentage : garder une menace, attaquer à portée,
            /// choisir alors le coup le plus rapide, lancer un projectile, punir
            /// une récupération, frapper dans le vide.
            /// </summary>
            public int Guard, Attack, Fastest, Projectile, Punish, Whiff;
        }

        static readonly Level[] levels =
        {
            new Level { Name = "Facile", Reaction = 24, Interval = 20, Guard = 20, Attack = 35, Fastest = 0, Projectile = 15, Punish = 10, Whiff = 15 },
            new Level { Name = "Normal", Reaction = 14, Interval = 12, Guard = 50, Attack = 45, Fastest = 40, Projectile = 30, Punish = 45, Whiff = 5 },
            new Level { Name = "Difficile", Reaction = 7, Interval = 6, Guard = 85, Attack = 60, Fastest = 70, Projectile = 40, Punish = 85, Whiff = 0 },
        };

        /// <summary>Niveaux disponibles : 0 facile, 1 normal, 2 difficile (indice de <see cref="PlayerSource.Cpu"/>).</summary>
        public static int LevelCount => levels.Length;
        public static string LevelName(int level) => levels[Clamp(level)].Name;
        static int Clamp(int level) => Math.Max(0, Math.Min(levels.Length - 1, level));

        /// <summary>Ce que l'IA sait d'un coup, déduit de sa définition.</summary>
        sealed class MoveInfo
        {
            public CompiledMove Move;
            /// <summary>Commande la plus simple à exécuter (appui direct de préférence).</summary>
            public CommandDefinition Command;
            /// <summary>Lance un projectile plutôt que de frapper au corps.</summary>
            public bool Ranged;
            /// <summary>Premier tick actif (ou d'émission du projectile).</summary>
            public int Startup;
            /// <summary>Distance d'émission du projectile devant le pivot.</summary>
            public int Reach;
        }

        readonly Level level;
        readonly int slot;
        readonly Func<MatchSession> session;
        readonly uint seed;
        readonly List<MatchState> seen = new List<MatchState>();
        readonly Queue<Buttons> plan = new Queue<Buttons>();
        SimulationContext analyzed;
        List<MoveInfo> moves, opponentMoves;
        uint random;
        int lastTick, nextChoice, threat, punished;
        bool guarding;
        Buttons intent;

        public int Slot => slot;
        public string Name => level.Name;

        /// <param name="level">0 facile, 1 normal, 2 difficile.</param>
        /// <param name="session">Combat lu à chaque tick ; peut être null au moment de la création.</param>
        public CpuPlayer(int level, int slot, Func<MatchSession> session = null, uint seed = 1)
        {
            this.level = levels[Clamp(level)];
            this.slot = slot;
            this.session = session;
            this.seed = seed == 0 ? 0x9E3779B9u : seed;
            Reset();
        }

        public Buttons InputFor(int tick)
        {
            var current = session?.Invoke();
            return current == null ? Buttons.None : Think(current.State, current.Context);
        }

        /// <summary>Commandes du prochain tick, à partir de l'état qui le précède.</summary>
        public Buttons Think(MatchState state, SimulationContext context)
        {
            if (state.Tick < lastTick || context != analyzed) Reset();
            if (context != analyzed) Analyze(context);
            lastTick = state.Tick;
            seen.Add(state);
            if (seen.Count > level.Reaction + 1) seen.RemoveAt(0);
            return Decide(state, context);
        }

        /// <summary>Nouvelle manche (ou nouveau combat) : mémoire et hasard repartent de zéro.</summary>
        void Reset()
        {
            seen.Clear();
            plan.Clear();
            random = seed;
            lastTick = nextChoice = 0;
            threat = punished = -1;
            guarding = false;
            intent = Buttons.None;
        }

        void Analyze(SimulationContext context)
        {
            analyzed = context;
            moves = Analyze(context.Fighters[slot]);
            opponentMoves = Analyze(context.Fighters[MatchState.Opponent(slot)]);
        }

        static List<MoveInfo> Analyze(CompiledFighter fighter)
        {
            var result = new List<MoveInfo>();
            foreach (var move in fighter.MovesByPriority)
            {
                CommandDefinition simplest = null;
                foreach (var command in move.Definition.Commands)
                    if (simplest == null || command.Motion.Count < simplest.Motion.Count) simplest = command;
                bool ranged = false;
                int startup = -1, reach = 0;
                foreach (var segment in move.Segments)
                {
                    foreach (var action in segment.Definition.OnEnter)
                        if (action is SpawnProjectileAction spawn)
                        {
                            ranged = true;
                            reach = spawn.Offset.X;
                        }
                    if (startup < 0 && (segment.Definition.Phase == Phase.Active || ranged)) startup = segment.Start;
                }
                // Les coups sautés ne partent qu'en l'air : l'IA reste au sol. Les suites d'enchaînement ne partent pas du neutre.
                if (simplest == null || (move.Definition.Hit == null && !ranged) || move.Definition.Stance == MoveStance.Air || move.Definition.CancelOnly) continue;
                result.Add(new MoveInfo { Move = move, Command = simplest, Ranged = ranged, Startup = Math.Max(0, startup), Reach = reach });
            }
            return result;
        }

        Buttons Decide(MatchState state, SimulationContext context)
        {
            if (state.Round.Phase != RoundPhase.Fight)
            {
                plan.Clear();
                return Buttons.None;
            }
            // Commande en plusieurs ticks déjà engagée (quart de cercle, relâchement).
            if (plan.Count > 0) return plan.Dequeue();

            var me = state.Fighters[slot];
            var opponent = state.Fighters[MatchState.Opponent(slot)];
            int toward = opponent.X >= me.X ? 1 : -1;
            var back = toward == 1 ? Buttons.Left : Buttons.Right;
            var forward = toward == 1 ? Buttons.Right : Buttons.Left;
            bool free = FighterBehavior.IsActionable(me) && me.Hitstop == 0;

            // Ce que l'IA a vu, avec son temps de réaction.
            var perceived = seen[0];
            var perceivedMe = perceived.Fighters[slot];
            var perceivedOpponent = perceived.Fighters[MatchState.Opponent(slot)];

            // Menace : garder ou non, décidé une seule fois par coup ou projectile.
            // Le projectile qui suit immédiatement son lancer garde la même décision.
            int key = ThreatKey(perceived, perceivedMe, context);
            if (key != threat)
            {
                bool thrown = threat >= 0 && key >= 0 && key % 2 == 1;
                threat = key;
                if (!thrown) guarding = key >= 0 && Roll(level.Guard);
            }
            // Tenir arrière sans menace réelle ferait reculer : la garde n'est prise
            // que face à un coup ou un projectile proche, ou juste avant une émission.
            var guard = back | (ThreatensLow(perceivedOpponent, context) ? Buttons.Down : Buttons.None);
            if (key >= 0 && guarding && (FighterBehavior.IsThreatened(me, context, state) || ThrowImminent(perceivedOpponent))) return guard;
            if (me.Action.Kind == ActionKind.Blockstun) return guard;
            if (!free) return Buttons.None;

            // Punition : l'adversaire finit un coup, frapper avant qu'il ne récupère.
            if (perceivedOpponent.Action.Kind == ActionKind.Move)
            {
                var move = context.Of(perceivedOpponent).Move(perceivedOpponent.Action.MoveId);
                int start = perceived.Tick - perceivedOpponent.Action.Tick;
                if (move != null && perceivedOpponent.Action.Tick >= move.LastActiveEnd && start != punished)
                {
                    punished = start;
                    var reaching = Reaching(moves, me, context.Of(me), opponent, context.Of(opponent), toward, 0);
                    if (reaching.Count > 0 && Roll(level.Punish)) return Press(Choose(reaching), toward);
                }
            }

            if (state.Tick < nextChoice) return intent;
            nextChoice = state.Tick + level.Interval;

            var hitting = Reaching(moves, me, context.Of(me), opponent, context.Of(opponent), toward, 0);
            if (hitting.Count > 0)
            {
                if (Roll(level.Attack)) return Press(Choose(hitting), toward);
                // Reculer à portée adverse, c'est déjà garder.
                bool threatened = Reaching(opponentMoves, opponent, context.Of(opponent), me, context.Of(me), -toward, 0).Count > 0;
                intent = threatened && Roll(level.Guard / 2) ? back : Buttons.None;
                return intent;
            }

            var ranged = Ranged(me, context, state);
            if (ranged != null && Roll(level.Projectile)) return Press(ranged, toward);
            if (Roll(level.Whiff))
            {
                var melee = moves.FindAll(info => !info.Ranged);
                if (melee.Count > 0) return Press(melee[Next(melee.Count)], toward);
            }
            int choice = Next(100);
            intent = choice < 75 ? forward : choice < 90 ? Buttons.None : back;
            return intent;
        }

        /// <summary>
        /// Coup adverse pas encore fini à proximité ou lancer de projectile en
        /// préparation (clé paire : tick de départ), projectile qui approche
        /// (clé impaire : identifiant). -1 sinon.
        /// </summary>
        int ThreatKey(MatchState state, FighterState me, SimulationContext context)
        {
            var opponent = state.Fighters[MatchState.Opponent(me.Slot)];
            // Le projectile peut naître déjà tout près : réagir au geste, pas au projectile.
            if (opponent.Action.Kind == ActionKind.Move)
            {
                var thrown = opponentMoves.Find(info => info.Move.Id == opponent.Action.MoveId);
                if (thrown != null && thrown.Ranged && opponent.Action.Tick <= thrown.Startup) return (state.Tick - opponent.Action.Tick) * 2;
            }
            if (!FighterBehavior.IsThreatened(me, context, state)) return -1;
            if (opponent.Action.Kind == ActionKind.Move && Math.Abs(opponent.X - me.X) <= context.Rules.ProximityGuardDistance)
            {
                var move = context.Of(opponent).Move(opponent.Action.MoveId);
                if (move != null && opponent.Action.Tick < move.LastActiveEnd) return (state.Tick - opponent.Action.Tick) * 2;
            }
            foreach (var projectile in state.Projectiles)
                if (projectile.Owner != me.Slot) return projectile.Id * 2 + 1;
            return -1;
        }

        /// <summary>
        /// Lancer adverse sur le point d'émettre : le projectile peut naître
        /// déjà au contact. Le geste est vu avec retard ; ajouter le temps de
        /// réaction donne l'instant réel.
        /// </summary>
        bool ThrowImminent(FighterState perceivedOpponent)
        {
            if (perceivedOpponent.Action.Kind != ActionKind.Move) return false;
            var thrown = opponentMoves.Find(info => info.Move.Id == perceivedOpponent.Action.MoveId);
            int now = perceivedOpponent.Action.Tick + level.Reaction;
            return thrown != null && thrown.Ranged && now >= thrown.Startup - 2 && now <= thrown.Startup + 1;
        }

        /// <summary>Coup adverse vu en cours qui frappe bas : la garde se prend accroupi.</summary>
        static bool ThreatensLow(FighterState perceivedOpponent, SimulationContext context)
        {
            if (perceivedOpponent.Action.Kind != ActionKind.Move) return false;
            var move = context.Of(perceivedOpponent).Move(perceivedOpponent.Action.MoveId);
            return move?.Definition.Hit != null && move.Definition.Hit.Level == HitLevel.Low;
        }

        /// <summary>Coups au corps dont une hitbox active toucherait la cible depuis les positions actuelles.</summary>
        static List<MoveInfo> Reaching(List<MoveInfo> candidates, FighterState attacker, CompiledFighter attackerDefinition,
            FighterState target, CompiledFighter targetDefinition, int facing, int slack)
        {
            var hurtboxes = Hitboxes.Hurtboxes(target, targetDefinition);
            var result = new List<MoveInfo>();
            foreach (var info in candidates)
            {
                if (info.Ranged) continue;
                int advance = slack;
                foreach (var segment in info.Move.Segments)
                {
                    var definition = segment.Definition;
                    if (definition.Phase == Phase.Active)
                    {
                        var hitboxes = new List<WorldBox>();
                        foreach (var box in definition.Hitboxes) hitboxes.Add(Geometry.ToWorld(box, attacker.X + advance * facing, attacker.Y, facing));
                        if (Geometry.FirstOverlap(hitboxes, hurtboxes, out _, out _))
                        {
                            result.Add(info);
                            break;
                        }
                    }
                    advance += definition.Advance * definition.Duration;
                }
            }
            return result;
        }

        /// <summary>Le plus rapide des coups possibles, ou l'un d'eux au hasard.</summary>
        MoveInfo Choose(List<MoveInfo> candidates)
        {
            if (!Roll(level.Fastest)) return candidates[Next(candidates.Count)];
            var fastest = candidates[0];
            foreach (var info in candidates) if (info.Startup < fastest.Startup) fastest = info;
            return fastest;
        }

        /// <summary>
        /// Lancer possible maintenant : sans projectile adverse trop proche, qui
        /// toucherait pendant la préparation ; plus loin, les deux s'annulent.
        /// </summary>
        MoveInfo Ranged(FighterState me, SimulationContext context, MatchState state)
        {
            foreach (var info in moves)
            {
                if (!info.Ranged || !SegmentActions.CanStartMove(info.Move, me, context.Of(me), state)) continue;
                bool clear = true;
                foreach (var projectile in state.Projectiles)
                {
                    var definition = context.Fighters[projectile.Owner].Projectile(projectile.ProjectileId);
                    if (projectile.Owner == slot || definition == null) continue;
                    int margin = definition.Hitbox.W + Math.Abs(info.Reach);
                    if (Math.Abs(projectile.X - me.X) - definition.Speed * info.Startup <= margin) clear = false;
                }
                if (clear) return info;
            }
            return null;
        }

        /// <summary>Exécute la commande : directions du mouvement tick par tick, appui, puis relâchement.</summary>
        Buttons Press(MoveInfo info, int facing)
        {
            var command = info.Command;
            for (int index = 0; index < command.Motion.Count - 1; index++) plan.Enqueue(DirectionButtons(command.Motion[index], facing));
            var last = command.Motion.Count > 0 ? command.Motion[command.Motion.Count - 1] : command.Direction;
            // Un coup accroupi sans direction imposée se lance bas tenu.
            if (last == 0 && info.Move.Definition.Stance == MoveStance.Crouch) last = 2;
            plan.Enqueue(DirectionButtons(last, facing) | command.Button);
            plan.Enqueue(Buttons.None);
            intent = Buttons.None;
            return plan.Dequeue();
        }

        /// <summary>Direction « pavé numérique » relative (6 = avant) vers les boutons absolus.</summary>
        static Buttons DirectionButtons(int direction, int facing)
        {
            if (direction <= 0) return Buttons.None;
            int horizontal = ((direction - 1) % 3 - 1) * facing;
            int vertical = (direction - 1) / 3 - 1;
            var result = Buttons.None;
            if (horizontal > 0) result |= Buttons.Right;
            if (horizontal < 0) result |= Buttons.Left;
            if (vertical > 0) result |= Buttons.Up;
            if (vertical < 0) result |= Buttons.Down;
            return result;
        }

        int Next(int max)
        {
            random ^= random << 13;
            random ^= random >> 17;
            random ^= random << 5;
            return (int)(random % (uint)max);
        }

        bool Roll(int percent) => percent > 0 && Next(100) < percent;
    }
}
