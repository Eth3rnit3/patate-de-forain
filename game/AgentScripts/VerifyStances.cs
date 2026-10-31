using System.Linq;
using Patate.Application;
using Patate.Combat;
using Patate.UnityAdapters;
using UnityEngine;

// Vérification des postures en mode Play : accroupi, coups bas, garde basse, sauts, coups sautés, KO.
public static class VerifyStances
{
    static Buttons PlayerOne(int t)
    {
        if (t >= 90 && t < 125) return Buttons.Right;                 // s'approche
        if (t >= 130 && t < 160) return Buttons.Down;                 // accroupi
        if (t == 160) return Buttons.Down | Buttons.LightPunch;       // poing accroupi (cible accroupie sans garde)
        if (t > 160 && t < 185) return Buttons.Down;
        if (t == 185) return Buttons.Down | Buttons.LightKick;        // pied accroupi (garde basse)
        if (t >= 225 && t < 229) return Buttons.Up;                   // saut vertical
        if (t == 245) return Buttons.HeavyKick;                       // pied sauté
        if (t >= 300 && t < 304) return Buttons.Up | Buttons.Right;   // saut avant
        if (t == 318) return Buttons.LightPunch;                      // poing sauté
        if (t == 380) return Buttons.LightPunch;                      // coup de grâce
        return Buttons.None;
    }

    static Buttons PlayerTwo(int t)
    {
        if (t >= 140 && t < 175) return Buttons.Down;                 // accroupi, sans garde
        if (t >= 178 && t < 215) return Buttons.Down | Buttons.Right; // garde basse
        return Buttons.None;
    }

    static GameBootstrap Game() => Object.FindAnyObjectByType<GameBootstrap>() ?? throw new System.Exception("Lancer le mode Play d'abord.");

    public static object Setup() => SetupWith("saint-louis", "saint-louis");

    public static object SetupWith(string first, string second)
    {
        var game = Game();
        game.Flow.Setup.Fighters = new[] { first, second };
        game.Flow.Go(game.Flow.Fight());
        game.ShowDebug = true;
        game.Automate(new ScriptedInput(PlayerOne), new ScriptedInput(PlayerTwo));
        return Describe(game);
    }

    public static object Step(int untilTick)
    {
        var game = Game();
        game.StepAutomated(Mathf.Max(0, untilTick - game.Session.State.Tick));
        return Describe(game) + "\n" + string.Join("\n", game.LastEvents);
    }

    public static object Weaken(int slot, int health)
    {
        var game = Game();
        game.Session.State.Fighters[slot].Health = health;
        return Describe(game);
    }

    static string Describe(GameBootstrap game)
    {
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        var s = game.Session.State;
        string F(FighterState f) => $"J{f.Slot + 1} x={f.X} y={f.Y} {f.Posture} PV={f.Health} {f.Action.Kind} {f.Action.MoveId} t{f.Action.Tick}";
        return $"tick {s.Tick} {s.Round.Phase} | {F(s.Fighters[0])} | {F(s.Fighters[1])}";
    }
}
