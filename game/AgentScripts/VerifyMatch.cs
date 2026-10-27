using System.Linq;
using Patate.Application;
using Patate.Combat;
using Patate.UnityAdapters;
using UnityEngine;

// Vérification en jeu (mode Play) avec des entrées scriptées, tick par tick.
//   unity command editor.playmode start
//   unity command script run --file AgentScripts/VerifyMatch.cs --entry VerifyMatch.Setup
//   unity command script run --file AgentScripts/VerifyMatch.cs --entry VerifyMatch.Step --args '[148]'
//   unity command capture game_view --save_path ...
public static class VerifyMatch
{
    // Ticks absolus depuis le début de la manche (l'introduction dure 90 ticks).
    static Buttons PlayerOne(int t)
    {
        if (t >= 90 && t < 130) return Buttons.Right;   // s'approche
        if (t == 130) return Buttons.HeavyKick;          // pied puissant
        if (t == 200) return Buttons.Special;            // fleur de lys
        // Ensuite : avance puis enchaîne poing, pied rapide, pied puissant, en boucle.
        if (t >= 300) return ((t - 300) % 64) switch { < 24 => Buttons.Right, 24 => Buttons.LightPunch, 40 => Buttons.LightKick, 52 => Buttons.HeavyKick, _ => Buttons.None };
        return Buttons.None;
    }

    // Le joueur 2 garde (tient arrière, c'est-à-dire la droite) pendant le spécial.
    static Buttons PlayerTwo(int t) => t >= 190 && t < 270 ? Buttons.Right : Buttons.None;

    static GameBootstrap Game() => Object.FindAnyObjectByType<GameBootstrap>() ?? throw new System.Exception("Lancer le mode Play d'abord.");

    public static object Setup()
    {
        var game = Game();
        game.ShowDebug = true;
        game.Automate(new ScriptedInput(PlayerOne), new ScriptedInput(PlayerTwo));
        return Describe(game);
    }

    /// <summary>Avance jusqu'au tick absolu <paramref name="untilTick"/> et décrit l'état.</summary>
    public static object Step(int untilTick)
    {
        var game = Game();
        game.StepAutomated(Mathf.Max(0, untilTick - game.Session.State.Tick));
        return Describe(game);
    }

    /// <summary>Avance jusqu'au prochain événement du type donné (ou au plus <paramref name="limit"/> ticks).</summary>
    public static object StepUntil(string eventType, int limit)
    {
        var game = Game();
        var type = (MatchEventType)System.Enum.Parse(typeof(MatchEventType), eventType);
        for (int i = 0; i < limit; i++)
        {
            int before = game.Session.State.Tick;
            game.StepAutomated(1);
            var events = game.LastEvents;
            if (events.Any(e => e.Type == type)) return $"{eventType} au tick {before + 1}\n{Describe(game)}\n{string.Join("\n", events)}";
        }
        return $"pas de {eventType} en {limit} ticks\n{Describe(game)}";
    }

    static string Describe(GameBootstrap game)
    {
        // La vue Game n'est redessinée que si l'éditeur avance : on la force pour que la capture soit à jour.
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        var s = game.Session.State;
        string F(FighterState f) => $"J{f.Slot + 1} x={f.X} PV={f.Health} {f.Action.Kind} {f.Action.MoveId} t{f.Action.Tick} gel={f.Hitstop}";
        return $"tick {s.Tick} manche {s.Round.Phase} | {F(s.Fighters[0])} | {F(s.Fighters[1])} | projectiles {s.Projectiles.Count} | #{StateCodec.Checksum(s)}";
    }
}
