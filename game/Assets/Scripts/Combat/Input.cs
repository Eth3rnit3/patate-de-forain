namespace Patate.Combat
{
    /// <summary>
    /// Commandes d'un joueur pour UN tick de simulation, sous forme de masque de bits.
    /// Indépendant du périphérique : clavier, manette, replay ou réseau produisent
    /// tous ce même entier. Les directions sont absolues (gauche/droite écran) ;
    /// le domaine les convertit en avant/arrière selon l'orientation.
    /// Six boutons d'attaque arcade sont prévus dès maintenant (le prototype en
    /// utilise trois) plus un bouton « spécial » direct, pour que le format des
    /// replays n'ait pas à changer quand ils arriveront.
    /// </summary>
    [System.Flags]
    public enum Buttons
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Up = 1 << 2,
        Down = 1 << 3,
        LightPunch = 1 << 4,
        MediumPunch = 1 << 5,
        HeavyPunch = 1 << 6,
        LightKick = 1 << 7,
        MediumKick = 1 << 8,
        HeavyKick = 1 << 9,
        Special = 1 << 10,
    }

    public enum HorizontalIntent { None, Forward, Back }

    public static class InputRules
    {
        public static bool IsHeld(Buttons input, Buttons button) => (input & button) != 0;

        /// <summary>
        /// Direction en notation « pavé numérique », relative à l'orientation :
        /// 6 = avant, 4 = arrière, 2 = bas, 3 = bas-avant, 5 = neutre…
        /// Gauche + droite s'annulent, haut + bas aussi (résultat déterministe).
        /// </summary>
        public static int Direction(Buttons input, int facing)
        {
            int right = IsHeld(input, Buttons.Right) ? 1 : 0;
            int left = IsHeld(input, Buttons.Left) ? 1 : 0;
            int horizontal = (right - left) * facing;
            int vertical = (IsHeld(input, Buttons.Up) ? 1 : 0) - (IsHeld(input, Buttons.Down) ? 1 : 0);
            return 5 + horizontal + vertical * 3;
        }

        /// <summary>Sans accroupissement ni saut, bas-avant marche et bas-arrière garde.</summary>
        public static HorizontalIntent Horizontal(Buttons input, int facing)
        {
            switch (Direction(input, facing))
            {
                case 3: case 6: case 9: return HorizontalIntent.Forward;
                case 1: case 4: case 7: return HorizontalIntent.Back;
                default: return HorizontalIntent.None;
            }
        }
    }
}
