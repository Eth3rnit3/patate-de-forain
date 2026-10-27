using System.Collections.Generic;

namespace Patate.Combat
{
    /// <summary>Reconnaissance des commandes à partir de l'historique d'entrées (le plus récent en dernier).</summary>
    public static class Commands
    {
        public static bool WasPressed(IReadOnlyList<Buttons> history, Buttons button)
        {
            var current = history[history.Count - 1];
            var previous = history.Count > 1 ? history[history.Count - 2] : Buttons.None;
            return (current & button) != 0 && (previous & button) == 0;
        }

        public static bool Matches(CommandDefinition command, IReadOnlyList<Buttons> history, int facing)
        {
            if (!WasPressed(history, command.Button)) return false;
            if (command.Motion.Count > 0) return MatchesMotion(command.Motion, command.Window, history, facing);
            return command.Direction == 0 || InputRules.Direction(history[history.Count - 1], facing) == command.Direction;
        }

        /// <summary>
        /// Cherche la séquence de directions dans l'ordre, en remontant depuis le
        /// tick courant sans dépasser <paramref name="window"/> ticks. La dernière
        /// direction peut être tenue jusqu'à l'appui.
        /// </summary>
        static bool MatchesMotion(IReadOnlyList<int> motion, int window, IReadOnlyList<Buttons> history, int facing)
        {
            int step = motion.Count - 1;
            int oldest = System.Math.Max(0, history.Count - window);
            for (int index = history.Count - 1; index >= oldest && step >= 0; index--)
            {
                if (InputRules.Direction(history[index], facing) == motion[step]) step--;
            }
            return step < 0;
        }
    }
}
