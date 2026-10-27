namespace Patate.Combat
{
    public enum MatchEventType
    {
        RoundStart,
        MoveStart,
        Hit,
        Block,
        ProjectileSpawn,
        ProjectileEnd,
        KO,
        TimeOver,
        RoundOver,
    }

    public enum ProjectileEndReason { None, Hit, Block, Clash, Expired, OutOfArena }

    /// <summary>
    /// Fait survenu pendant un tick. La simulation les produit ; rendu, audio et
    /// effets les observent. Ils ne modifient jamais l'état.
    /// </summary>
    public sealed class MatchEvent
    {
        public MatchEventType Type;
        public int Tick;
        /// <summary>Combattant concerné : auteur du coup, lanceur, ou combattant KO.</summary>
        public int Slot = -1;
        /// <summary>Hit / Block : cible.</summary>
        public int Defender = -1;
        /// <summary>Identifiant du coup (MoveStart, Hit/Block au corps).</summary>
        public string MoveId;
        /// <summary>Identifiant du type de projectile (ProjectileSpawn/End, Hit/Block par projectile).</summary>
        public string ProjectileId;
        public int ProjectileInstance;
        public ProjectileEndReason Reason;
        public int Damage;
        /// <summary>Point de contact ou position, pour placer les effets.</summary>
        public int X, Y;
        public int Facing;
        /// <summary>RoundOver : 0, 1 ou RoundState.Draw.</summary>
        public int Winner = -1;

        public override string ToString() => $"[{Tick}] {Type} slot={Slot} move={MoveId} projectile={ProjectileId} dmg={Damage}";
    }
}
