namespace Patate.Combat
{
    /// <summary>
    /// Règles du moteur, communes à tous les combattants. Versionnées : un replay
    /// ou une partie en ligne doit référencer exactement la même version.
    /// </summary>
    public sealed class MatchRules
    {
        public string Id = "patate-core";
        public int Version = 3;
        public int TicksPerSecond = 60;
        /// <summary>Limites de l'arène en x ; les pushboxes restent à l'intérieur.</summary>
        public int ArenaLeft = -1200, ArenaRight = 1200;
        /// <summary>Écart maximal entre les deux combattants (largeur cadrée par la caméra).</summary>
        public int MaxSeparation = 1500;
        /// <summary>Distance initiale de chaque combattant au centre.</summary>
        public int StartOffset = 300;
        public int IntroTicks = 90;
        public int RoundTicks = 99 * 60;
        /// <summary>Durée entre le KO (ou la fin du temps) et la fin de manche.</summary>
        public int OutroTicks = 150;
        /// <summary>Durée sur laquelle un recul est réparti.</summary>
        public int PushbackTicks = 8;
        /// <summary>En dessous de cette distance, tenir arrière face à une attaque met en garde sans reculer.</summary>
        public int ProximityGuardDistance = 420;
        /// <summary>Ticks d'entrées conservés pour reconnaître les commandes.</summary>
        public int InputHistoryLength = 32;
        /// <summary>Marge au-delà de l'arène avant destruction d'un projectile.</summary>
        public int ProjectileArenaMargin = 200;

        public static MatchRules Default() => new MatchRules();

        public MatchRules Copy() => (MatchRules)MemberwiseClone();
    }
}
