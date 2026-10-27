using System;
using System.Collections.Generic;

namespace Patate.Combat
{
    /// <summary>
    /// Résolution des contacts d'un tick. Tous les contacts sont d'abord détectés
    /// à partir des positions courantes, puis appliqués dans un ordre fixe : deux
    /// coups actifs au même instant s'échangent sans favoriser un joueur.
    /// </summary>
    public static class CombatResolution
    {
        sealed class Contact
        {
            public int Attacker, Defender;
            public HitProperties Hit;
            public string MoveId;
            public ProjectileState Projectile;
            /// <summary>Sens du recul de la cible.</summary>
            public int Direction;
            public WorldBox Area, Hurt;
        }

        public static void Resolve(MatchState state, SimulationContext context, List<MatchEvent> events)
        {
            ResolveProjectileClashes(state, context, events);
            var contacts = new List<Contact>();
            DetectMoveContacts(state, context, contacts);
            DetectProjectileContacts(state, context, contacts);
            foreach (var contact in contacts) Apply(contact, state, context, events);
        }

        static void DetectMoveContacts(MatchState state, SimulationContext context, List<Contact> contacts)
        {
            for (int slot = 0; slot < 2; slot++)
            {
                var attacker = state.Fighters[slot];
                var defender = state.Fighters[MatchState.Opponent(slot)];
                if (attacker.Action.Kind != ActionKind.Move) continue;
                var move = context.Of(attacker).Move(attacker.Action.MoveId);
                if (move?.Definition.Hit == null) continue;
                if (!Geometry.FirstOverlap(Hitboxes.Active(attacker, context.Of(attacker)), Hitboxes.Hurtboxes(defender, context.Of(defender)), out var area, out var hurt)) continue;
                contacts.Add(new Contact
                {
                    Attacker = slot, Defender = defender.Slot, Hit = move.Definition.Hit, MoveId = move.Id,
                    Direction = attacker.Facing, Area = area, Hurt = hurt,
                });
            }
        }

        static void DetectProjectileContacts(MatchState state, SimulationContext context, List<Contact> contacts)
        {
            foreach (var projectile in state.Projectiles)
            {
                var owner = context.Fighters[projectile.Owner];
                var definition = owner.Projectile(projectile.ProjectileId);
                if (definition == null || !Hitboxes.Projectile(projectile, owner, out var box)) continue;
                var defender = state.Fighters[MatchState.Opponent(projectile.Owner)];
                if (!Geometry.FirstOverlap(new[] { box }, Hitboxes.Hurtboxes(defender, context.Of(defender)), out var area, out var hurt)) continue;
                contacts.Add(new Contact
                {
                    Attacker = projectile.Owner, Defender = defender.Slot, Hit = definition.Hit, Projectile = projectile,
                    Direction = projectile.Facing, Area = area, Hurt = hurt,
                });
            }
        }

        /// <summary>Deux projectiles adverses qui se touchent s'annulent.</summary>
        static void ResolveProjectileClashes(MatchState state, SimulationContext context, List<MatchEvent> events)
        {
            var destroyed = new List<ProjectileState>();
            foreach (var a in state.Projectiles)
            {
                if (a.Owner != 0 || destroyed.Contains(a)) continue;
                foreach (var b in state.Projectiles)
                {
                    if (b.Owner != 1 || destroyed.Contains(b)) continue;
                    if (!Hitboxes.Projectile(a, context.Fighters[0], out var boxA) || !Hitboxes.Projectile(b, context.Fighters[1], out var boxB)) continue;
                    if (!boxA.Overlaps(boxB)) continue;
                    destroyed.Add(a);
                    destroyed.Add(b);
                    break;
                }
            }
            foreach (var projectile in destroyed) RemoveProjectile(state, projectile, ProjectileEndReason.Clash, events);
        }

        /// <summary>Garde debout : tenir la direction opposée à la source, dans un état qui le permet.</summary>
        public static bool CanBlock(FighterState defender, HitProperties hit, int sourceDirection)
        {
            if (!hit.Blockable || hit.Level == HitLevel.Low) return false;
            var kind = defender.Action.Kind;
            if (kind != ActionKind.Idle && kind != ActionKind.Walk && kind != ActionKind.Guard && kind != ActionKind.Blockstun) return false;
            // « Arrière » = s'éloigner de la source, quelle que soit l'orientation actuelle.
            return InputRules.Horizontal(defender.CurrentInput, -sourceDirection) == HorizontalIntent.Back;
        }

        static void Apply(Contact contact, MatchState state, SimulationContext context, List<MatchEvent> events)
        {
            var attacker = state.Fighters[contact.Attacker];
            var defender = state.Fighters[contact.Defender];
            if (defender.Action.Kind == ActionKind.KO) return;
            var hit = contact.Hit;
            bool blocked = CanBlock(defender, hit, contact.Direction);
            int damage = blocked ? hit.ChipDamage : hit.Damage;
            bool melee = contact.Projectile == null;

            defender.Health = Math.Max(0, defender.Health - damage);
            defender.Hitstop = hit.Hitstop;
            defender.Push = new PushState
            {
                Remaining = blocked ? hit.PushbackBlock : hit.PushbackHit,
                TicksLeft = context.Rules.PushbackTicks,
                Direction = contact.Direction,
                TransferTo = melee ? attacker.Slot : -1,
            };
            if (defender.Health == 0) defender.Action = ActionState.Of(ActionKind.KO);
            else if (blocked) defender.Action = new ActionState { Kind = ActionKind.Blockstun, Duration = hit.Blockstun };
            else defender.Action = new ActionState { Kind = ActionKind.Hitstun, Duration = hit.Hitstun };

            if (melee && attacker.Action.Kind == ActionKind.Move)
            {
                attacker.Action.HasHit = true;
                attacker.Hitstop = hit.Hitstop;
            }

            events.Add(new MatchEvent
            {
                Type = blocked ? MatchEventType.Block : MatchEventType.Hit,
                Tick = state.Tick,
                Slot = attacker.Slot,
                Defender = defender.Slot,
                MoveId = contact.MoveId,
                ProjectileId = contact.Projectile?.ProjectileId,
                Damage = damage,
                X = (Math.Max(contact.Area.Left, contact.Hurt.Left) + Math.Min(contact.Area.Right, contact.Hurt.Right)) / 2,
                Y = (Math.Max(contact.Area.Bottom, contact.Hurt.Bottom) + Math.Min(contact.Area.Top, contact.Hurt.Top)) / 2,
                Facing = contact.Direction,
            });
            if (!melee) RemoveProjectile(state, contact.Projectile, blocked ? ProjectileEndReason.Block : ProjectileEndReason.Hit, events);
            if (defender.Health == 0) events.Add(new MatchEvent { Type = MatchEventType.KO, Tick = state.Tick, Slot = defender.Slot });
        }

        public static void RemoveProjectile(MatchState state, ProjectileState projectile, ProjectileEndReason reason, List<MatchEvent> events)
        {
            if (!state.Projectiles.Remove(projectile)) return;
            events.Add(new MatchEvent
            {
                Type = MatchEventType.ProjectileEnd,
                Tick = state.Tick,
                Slot = projectile.Owner,
                ProjectileId = projectile.ProjectileId,
                ProjectileInstance = projectile.Id,
                Reason = reason,
                X = projectile.X,
                Y = projectile.Y,
                Facing = projectile.Facing,
            });
        }
    }
}
