using Patate.Combat;

namespace Patate.Application
{
    /// <summary>
    /// Joueur automatique des vérifications en ligne : avance vers
    /// l'adversaire et frappe, au hasard mais de façon reproductible (même
    /// place, même tick, même commande). Une manche entre deux robots finit
    /// par un KO ou au temps, avec des coups échangés : de quoi éprouver la
    /// synchronisation sans personne au clavier.
    /// </summary>
    public static class OnlineRobot
    {
        public static Buttons InputFor(int slot, int tick)
        {
            // Hachage entier (pas de System.Random : son algorithme n'est pas garanti d'une plateforme à l'autre).
            uint h = (uint)(tick / 6) * 2654435761u ^ (uint)(slot + 1) * 40503u;
            h ^= h >> 15;
            h *= 2246822519u;
            h ^= h >> 13;
            var forward = slot == 0 ? Buttons.Right : Buttons.Left;
            var back = slot == 0 ? Buttons.Left : Buttons.Right;
            switch (h % 12)
            {
                case 0: case 1: case 2: case 3: case 4: return forward;
                case 5: return Buttons.LightPunch;
                case 6: return Buttons.LightKick;
                case 7: return Buttons.HeavyKick;
                case 8: return tick % 6 < 3 ? Buttons.Special : Buttons.None;
                case 9: return back;
                default: return Buttons.None;
            }
        }
    }
}
