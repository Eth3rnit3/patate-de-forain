
using System.IO;
using System.Text;

namespace Patate.Combat
{
    /// <summary>
    /// Sauvegarde et restauration exactes de l'état, en binaire compact (rapide
    /// pour un rollback), plus une empreinte pour détecter une désynchronisation
    /// (replay, réseau). L'empreinte compare, elle n'authentifie pas.
    /// Toute modification de MatchState doit être répercutée ici (les tests
    /// d'aller-retour le vérifient).
    /// </summary>
    public static class StateCodec
    {
        public static byte[] Serialize(MatchState state)
        {
            using (var stream = new MemoryStream())
            using (var w = new BinaryWriter(stream, Encoding.UTF8))
            {
                w.Write(MatchState.FormatVersion);
                WriteString(w, state.RulesId);
                w.Write(state.RulesVersion);
                w.Write(state.Tick);
                w.Write((int)state.Round.Phase);
                w.Write(state.Round.PhaseTick);
                w.Write(state.Round.Timer);
                w.Write(state.Round.Winner);
                foreach (var f in state.Fighters) WriteFighter(w, f);
                w.Write(state.Projectiles.Count);
                foreach (var p in state.Projectiles)
                {
                    w.Write(p.Id); w.Write(p.Owner); WriteString(w, p.ProjectileId);
                    w.Write(p.X); w.Write(p.Y); w.Write(p.Facing); w.Write(p.Tick);
                }
                w.Write(state.NextProjectileId);
                w.Flush();
                return stream.ToArray();
            }
        }

        public static MatchState Deserialize(byte[] bytes)
        {
            using (var r = new BinaryReader(new MemoryStream(bytes), Encoding.UTF8))
            {
                int version = r.ReadInt32();
                if (version != MatchState.FormatVersion) throw new InvalidDataException($"Format d'état non pris en charge : {version}");
                var state = new MatchState { RulesId = ReadString(r), RulesVersion = r.ReadInt32(), Tick = r.ReadInt32() };
                state.Round.Phase = (RoundPhase)r.ReadInt32();
                state.Round.PhaseTick = r.ReadInt32();
                state.Round.Timer = r.ReadInt32();
                state.Round.Winner = r.ReadInt32();
                state.Fighters[0] = ReadFighter(r);
                state.Fighters[1] = ReadFighter(r);
                int count = r.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    state.Projectiles.Add(new ProjectileState
                    {
                        Id = r.ReadInt32(), Owner = r.ReadInt32(), ProjectileId = ReadString(r),
                        X = r.ReadInt32(), Y = r.ReadInt32(), Facing = r.ReadInt32(), Tick = r.ReadInt32(),
                    });
                }
                state.NextProjectileId = r.ReadInt32();
                return state;
            }
        }

        /// <summary>FNV-1a 32 bits sur la forme sérialisée, en hexadécimal.</summary>
        public static string Checksum(MatchState state)
        {
            uint hash = 2166136261;
            foreach (byte b in Serialize(state))
            {
                hash ^= b;
                hash *= 16777619;
            }
            return hash.ToString("x8");
        }

        static void WriteFighter(BinaryWriter w, FighterState f)
        {
            w.Write(f.Slot);
            WriteString(w, f.Definition.Id); WriteString(w, f.Definition.Version); WriteString(w, f.Definition.Hash);
            w.Write(f.X); w.Write(f.Y); w.Write(f.Facing); w.Write(f.Health); w.Write(f.Hitstop);
            w.Write((int)f.Posture); w.Write(f.VelocityX); w.Write(f.VelocityY); w.Write(f.AirMoveUsed);
            var a = f.Action;
            w.Write((int)a.Kind); w.Write(a.Tick); w.Write(a.Forward); WriteString(w, a.MoveId); w.Write(a.HasHit); w.Write(a.HitLanded); w.Write(a.Duration); w.Write(a.Direction);
            w.Write(f.Push != null);
            if (f.Push != null) { w.Write(f.Push.Remaining); w.Write(f.Push.TicksLeft); w.Write(f.Push.Direction); w.Write(f.Push.TransferTo); }
            w.Write(f.InputHistory.Length);
            foreach (var input in f.InputHistory) w.Write((int)input);
        }

        static FighterState ReadFighter(BinaryReader r)
        {
            var f = new FighterState { Slot = r.ReadInt32() };
            f.Definition = new DefinitionRef(ReadString(r), ReadString(r), ReadString(r));
            f.X = r.ReadInt32(); f.Y = r.ReadInt32(); f.Facing = r.ReadInt32(); f.Health = r.ReadInt32(); f.Hitstop = r.ReadInt32();
            f.Posture = (Posture)r.ReadInt32(); f.VelocityX = r.ReadInt32(); f.VelocityY = r.ReadInt32(); f.AirMoveUsed = r.ReadBoolean();
            f.Action = new ActionState
            {
                Kind = (ActionKind)r.ReadInt32(), Tick = r.ReadInt32(), Forward = r.ReadBoolean(),
                MoveId = ReadString(r), HasHit = r.ReadBoolean(), HitLanded = r.ReadBoolean(), Duration = r.ReadInt32(), Direction = r.ReadInt32(),
            };
            if (r.ReadBoolean()) f.Push = new PushState { Remaining = r.ReadInt32(), TicksLeft = r.ReadInt32(), Direction = r.ReadInt32(), TransferTo = r.ReadInt32() };
            f.InputHistory = new Buttons[r.ReadInt32()];
            for (int i = 0; i < f.InputHistory.Length; i++) f.InputHistory[i] = (Buttons)r.ReadInt32();
            return f;
        }

        static void WriteString(BinaryWriter w, string value)
        {
            w.Write(value != null);
            if (value != null) w.Write(value);
        }

        static string ReadString(BinaryReader r) => r.ReadBoolean() ? r.ReadString() : null;
    }
}
