using System;
using System.Collections.Generic;
using Patate.Application;
using Patate.Combat;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Adversaire de l'ordinateur vu comme une source locale. Rien à lire par
    /// image : <see cref="CpuPlayer"/> décide à chaque tick en lisant la session.
    /// </summary>
    public sealed class CpuInput : ILocalInput
    {
        readonly CpuPlayer player;

        public CpuInput(int level, int slot, Func<MatchSession> session, uint seed)
        {
            player = new CpuPlayer(level, slot, session, seed);
        }

        public string Label => "Ordinateur";

        public void Sample() { }

        public Buttons InputFor(int tick) => player.InputFor(tick);

        public IEnumerable<(string action, string keys)> Help()
        {
            yield return ("Niveau", player.Name);
        }
    }
}
