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
    /// Point d'entrée et racine de composition : charge le catalogue, lance le
    /// combat, fait avancer la simulation à pas fixe (60 ticks/s) quel que soit
    /// le rafraîchissement de l'écran, puis laisse le rendu, l'audio et
    /// l'interface observer le résultat.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] string firstFighter = "saint-louis";
        [SerializeField] string secondFighter = "saint-louis";
        [SerializeField] string stage = "training";

        FighterCatalog catalog;
        MatchSession session;
        MatchView view;
        StageView stageView;
        AudioCues audioCues;
        Hud hud;
        FixedStepClock clock;
        readonly KeyboardInput[] inputs = { KeyboardInput.PlayerOne(), KeyboardInput.PlayerTwo() };
        readonly Dictionary<string, FighterVisuals> visuals = new Dictionary<string, FighterVisuals>();
        bool showDebug, showHelp, paused, focused = true;
        string startupError;

        IInputProvider[] automated;

        /// <summary>Session en cours, pour les scripts de vérification.</summary>
        public MatchSession Session => session;
        public bool ShowDebug { get => showDebug; set => showDebug = value; }
        /// <summary>Événements du dernier tick simulé (copie).</summary>
        public List<MatchEvent> LastEvents { get; private set; } = new List<MatchEvent>();

        const string DebugPreference = "patate.debug-overlay";

        void Start()
        {
            UnityEngine.Application.targetFrameRate = 120;
            catalog = FighterCatalog.Load(new ResourcesFighterSource());
            foreach (var rejected in catalog.Rejected) Debug.LogError($"Combattant « {rejected.Key} » refusé :\n{rejected.Value}");
            foreach (var fighter in catalog.Fighters)
                foreach (var warning in fighter.Warnings) Debug.LogWarning($"{fighter.Name} : {warning}");
            if (catalog.Get(firstFighter) == null || catalog.Get(secondFighter) == null)
            {
                startupError = $"Combattant introuvable ou invalide : {firstFighter} / {secondFighter}. Voir la console.";
                Debug.LogError(startupError);
                return;
            }

            var camera = Camera.main ?? new GameObject("Caméra", typeof(Camera)).GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            stageView = new StageView(stage, camera);
            session = MatchSession.Start(catalog, firstFighter, secondFighter, MatchRules.Default(), StatPolicy.Free());
            view = new MatchView(camera, session.Context.Rules, Visuals(firstFighter), Visuals(secondFighter), stageView);
            audioCues = new AudioCues(gameObject, Visuals(firstFighter), Visuals(secondFighter), stageView.Music);
            hud = new Hud();
            clock = new FixedStepClock(session.Context.Rules.TicksPerSecond);
            showDebug = PlayerPrefs.GetInt(DebugPreference, 0) == 1;
            Debug.Log($"Combat : {session.Context.Fighters[0].Ref} contre {session.Context.Fighters[1].Ref}");
        }

        FighterVisuals Visuals(string id)
        {
            if (!visuals.TryGetValue(id, out var result)) visuals[id] = result = new FighterVisuals(catalog.Get(id));
            return result;
        }

        void Update()
        {
            if (session == null) return;
            HandleGlobalKeys();
            foreach (var input in inputs) input.Sample();

            int ticks = paused || !focused || automated != null ? 0 : clock.Consume(Time.unscaledDeltaTime);
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

        void HandleGlobalKeys()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.f1Key.wasPressedThisFrame)
            {
                showDebug = !showDebug;
                PlayerPrefs.SetInt(DebugPreference, showDebug ? 1 : 0);
            }
            if (keyboard.f2Key.wasPressedThisFrame) showHelp = !showHelp;
            if (keyboard.escapeKey.wasPressedThisFrame) paused = !paused;
            if (keyboard.f3Key.wasPressedThisFrame) ChangeFighter(0);
            if (keyboard.f4Key.wasPressedThisFrame) ChangeFighter(1);
            bool over = session.State.Round.Phase == RoundPhase.Over;
            if (keyboard.f5Key.wasPressedThisFrame || (over && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))) Restart();
        }

        /// <summary>Sélection : passe au combattant suivant du catalogue pour ce joueur et relance.</summary>
        void ChangeFighter(int slot)
        {
            var ids = catalog.Fighters.Select(f => f.Id).ToList();
            var current = slot == 0 ? firstFighter : secondFighter;
            var next = ids[(ids.IndexOf(current) + 1) % ids.Count];
            if (slot == 0) firstFighter = next;
            else secondFighter = next;
            session = MatchSession.Start(catalog, firstFighter, secondFighter, MatchRules.Default(), StatPolicy.Free());
            view.SetFighters(Visuals(firstFighter), Visuals(secondFighter));
            audioCues.SetFighters(Visuals(firstFighter), Visuals(secondFighter));
            Restart();
        }

        public void Restart()
        {
            session.Restart();
            view.Clear();
            clock.Reset();
            paused = false;
        }

        void OnApplicationFocus(bool hasFocus)
        {
            focused = hasFocus;
            clock?.Reset();
        }

        void OnGUI()
        {
            if (startupError != null)
            {
                GUI.Label(new Rect(20, 20, Screen.width - 40, 200), startupError, new GUIStyle { fontSize = 24, wordWrap = true, normal = { textColor = Color.red } });
                return;
            }
            if (session == null) return;
            var fighters = new[] { Visuals(firstFighter), Visuals(secondFighter) };
            if (showDebug) hud.DrawDebug(session.State, session.Context, view, StateCodec.Checksum(session.State), audioCues.Missing);
            hud.Draw(session.State, fighters, inputs, showHelp, paused);
        }
    }
}
