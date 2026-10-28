using System.Collections.Generic;
using System.Linq;
using Patate.Application;
using Patate.Combat;
using Patate.Content;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Le combat : lance la session décrite par un <see cref="MatchSetup"/>,
    /// fait avancer la simulation à pas fixe (60 ticks/s) quel que soit le
    /// rafraîchissement de l'écran, puis laisse le rendu, l'audio et
    /// l'interface observer le résultat.
    /// </summary>
    public sealed class FightScreen : IGameScreen
    {
        readonly GameFlow flow;
        readonly MatchSetup setup;
        MatchSession session;
        MatchView view;
        StageView stageView;
        AudioCues audioCues;
        Hud hud;
        FixedStepClock clock;
        ILocalInput[] inputs;
        bool showDebug, showHelp, paused, focused = true;
        string startupError;

        IInputProvider[] automated;

        /// <summary>Session en cours, pour les scripts de vérification.</summary>
        public MatchSession Session => session;
        public bool ShowDebug { get => showDebug; set => showDebug = value; }
        /// <summary>Événements du dernier tick simulé (copie).</summary>
        public List<MatchEvent> LastEvents { get; private set; } = new List<MatchEvent>();

        const string DebugPreference = "patate.debug-overlay";

        public FightScreen(GameFlow flow, MatchSetup setup)
        {
            this.flow = flow;
            this.setup = setup;
        }

        public void Enter()
        {
            if (setup.Fighters.Any(id => flow.Catalog.Get(id) == null))
            {
                startupError = $"Combattant introuvable ou invalide : {string.Join(" / ", setup.Fighters)}. Voir la console.";
                Debug.LogError(startupError);
                return;
            }

            inputs = setup.Players.Select((player, slot) => InputSources.Create(player, slot, () => session)).ToArray();
            stageView = new StageView(setup.Stage, flow.Camera);
            session = MatchSession.Start(flow.Catalog, setup.Fighters[0], setup.Fighters[1], MatchRules.Default(), StatPolicy.Free());
            view = new MatchView(flow.Camera, session.Context.Rules, Visuals(0), Visuals(1), stageView);
            audioCues = new AudioCues(flow.Host, Visuals(0), Visuals(1), stageView.Music);
            hud = new Hud();
            clock = new FixedStepClock(session.Context.Rules.TicksPerSecond);
            showDebug = PlayerPrefs.GetInt(DebugPreference, 0) == 1;
            Debug.Log($"Combat : {session.Context.Fighters[0].Ref} contre {session.Context.Fighters[1].Ref}");
        }

        public void Exit()
        {
            view?.Dispose();
            stageView?.Dispose();
            audioCues?.Dispose();
        }

        FighterVisuals Visuals(int slot) => flow.Visuals(setup.Fighters[slot]);

        public void Update(float deltaTime)
        {
            if (session == null) return;
            if (HandleGlobalKeys()) return;
            foreach (var input in inputs) input.Sample();

            int ticks = paused || !focused || automated != null ? 0 : clock.Consume(deltaTime);
            for (int i = 0; i < ticks; i++) Advance(session.Step(inputs[0], inputs[1]));
            view.Render(session.State);
            stageView.Follow();
        }

        /// <summary>
        /// Pilotage automatisé (vérifications, démonstrations) : les entrées
        /// viennent de scripts et la simulation n'avance que sur demande,
        /// tick par tick, indépendamment du temps réel.
        /// </summary>
        public void Automate(IInputProvider first, IInputProvider second)
        {
            automated = new[] { first, second };
            Restart();
        }

        public void StepAutomated(int ticks)
        {
            for (int i = 0; i < ticks; i++) Advance(session.Step(automated[0], automated[1]));
            view.Render(session.State);
            stageView.Follow();
        }

        /// <summary>Un tick simulé : la simulation décide, les observateurs réagissent.</summary>
        void Advance(IReadOnlyList<MatchEvent> events)
        {
            LastEvents = events.ToList();
            view.OnEvents(events);
            audioCues.OnEvents(events);
        }

        /// <summary>Touches de service. Renvoie vrai si l'écran a été quitté.</summary>
        bool HandleGlobalKeys()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            if (keyboard.f1Key.wasPressedThisFrame)
            {
                showDebug = !showDebug;
                PlayerPrefs.SetInt(DebugPreference, showDebug ? 1 : 0);
            }
            if (keyboard.f2Key.wasPressedThisFrame) showHelp = !showHelp;
            if (keyboard.escapeKey.wasPressedThisFrame) paused = !paused;
            if (keyboard.f3Key.wasPressedThisFrame) ChangeFighter(0);
            if (keyboard.f4Key.wasPressedThisFrame) ChangeFighter(1);
            if (keyboard.f5Key.wasPressedThisFrame) Restart();
            bool over = session.State.Round.Phase == RoundPhase.Over;
            if (over && automated == null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
            {
                flow.Go(flow.Result(session.State));
                return true;
            }
            return false;
        }

        /// <summary>Passe au combattant suivant du catalogue pour ce joueur et relance.</summary>
        void ChangeFighter(int slot)
        {
            var ids = flow.Catalog.Fighters.Select(f => f.Id).ToList();
            setup.Fighters[slot] = ids[(ids.IndexOf(setup.Fighters[slot]) + 1) % ids.Count];
            flow.Setup.Fighters[slot] = setup.Fighters[slot];
            session = MatchSession.Start(flow.Catalog, setup.Fighters[0], setup.Fighters[1], MatchRules.Default(), StatPolicy.Free());
            view.SetFighters(Visuals(0), Visuals(1));
            audioCues.SetFighters(Visuals(0), Visuals(1));
            Restart();
        }

        public void Restart()
        {
            session.Restart();
            view.Clear();
            clock.Reset();
            paused = false;
        }

        public void OnFocus(bool hasFocus)
        {
            focused = hasFocus;
            clock?.Reset();
        }

        public void OnGUI()
        {
            if (startupError != null)
            {
                GUI.Label(new Rect(20, 20, Screen.width - 40, 200), startupError, new GUIStyle { fontSize = 24, wordWrap = true, normal = { textColor = Color.red } });
                return;
            }
            if (session == null) return;
            var fighters = new[] { Visuals(0), Visuals(1) };
            if (showDebug) hud.DrawDebug(session.State, session.Context, view, StateCodec.Checksum(session.State), audioCues.Missing);
            hud.Draw(session.State, fighters, inputs, showHelp, paused);
        }
    }
}
