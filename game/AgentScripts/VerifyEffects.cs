using System.Linq;
using Patate.Application;
using Patate.Combat;
using Patate.UnityAdapters;
using UnityEngine;

// Vérification des étincelles et du HUD en jeu (mode Play) : lance un combat
// sur le décor demandé, puis avance jusqu'au prochain coup qui touche ou qui est gardé.
//   unity command editor.playmode start
//   unity command script run --file AgentScripts/VerifyEffects.cs --entry VerifyEffects.Fight --args '["austerlitz"]'
//   unity command script run --file AgentScripts/VerifyEffects.cs --entry VerifyEffects.StepUntil --args '["Hit", 600, 2]'
//   unity command script run --file AgentScripts/VerifyEffects.cs --entry VerifyEffects.Result
//   unity command capture game_view --save_path ...
public static class VerifyEffects
{
    // J1 s'approche et frappe en boucle ; J2 garde (tient arrière) un temps sur deux.
    static Buttons PlayerOne(int t) => t < 90 ? Buttons.None : ((t - 90) % 60) switch { < 20 => Buttons.Right, 24 => Buttons.HeavyKick, 44 => Buttons.LightPunch, _ => Buttons.None };
    static Buttons PlayerTwo(int t) => (t / 240) % 2 == 1 ? Buttons.Right : Buttons.None;

    static GameBootstrap Game() => Object.FindAnyObjectByType<GameBootstrap>() ?? throw new System.Exception("Lancer le mode Play d'abord.");

    public static object Fight(string stage)
    {
        var game = Game();
        game.Flow.Setup = new MatchSetup { Fighters = new[] { "saint-louis", "napoleon" }, Stage = stage };
        game.Flow.Go(game.Flow.Fight());
        game.Automate(new ScriptedInput(PlayerOne), new ScriptedInput(PlayerTwo));
        return $"combat sur {stage}, tick {game.Session.State.Tick}";
    }

    /// <summary>Passe à l'écran de résultat avec l'état courant du combat.</summary>
    public static object Result()
    {
        var game = Game();
        var state = game.Session.State;
        game.Flow.Go(game.Flow.Result(state));
        return $"résultat, vainqueur {state.Round.Winner}";
    }

    /// <summary>Avance jusqu'au prochain événement du type donné, puis de <paramref name="after"/> ticks.</summary>
    public static object StepUntil(string eventType, int limit, int after)
    {
        var game = Game();
        var type = (MatchEventType)System.Enum.Parse(typeof(MatchEventType), eventType);
        for (int i = 0; i < limit; i++)
        {
            game.StepAutomated(1);
            var e = game.LastEvents.FirstOrDefault(x => x.Type == type);
            if (e == null) continue;
            game.StepAutomated(after);
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            return $"{e} au point ({e.X}, {e.Y}), puis {after} ticks : tick {game.Session.State.Tick}";
        }
        return $"pas de {eventType} en {limit} ticks";
    }
}
