using System.Collections.Generic;
using Patate.Application;
using Patate.Combat;
using Patate.Content;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Point d'entrée et racine de composition : charge le catalogue, prépare
    /// la caméra et confie la suite à l'enchaînement des écrans
    /// (<see cref="GameFlow"/>), à qui il transmet les images et le focus.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        GameFlow flow;

        public GameFlow Flow => flow;
        /// <summary>Combat en cours, pour les scripts de vérification (null hors combat).</summary>
        public FightScreen Fight => flow?.Current as FightScreen;
        public MatchSession Session => Fight?.Session;
        public bool ShowDebug { get => Fight?.ShowDebug ?? false; set { if (Fight != null) Fight.ShowDebug = value; } }
        /// <summary>Événements du dernier tick simulé (copie).</summary>
        public List<MatchEvent> LastEvents => Fight?.LastEvents;

        void Start()
        {
            UnityEngine.Application.targetFrameRate = 120;
            var catalog = FighterCatalog.Load(new ResourcesFighterSource());
            foreach (var rejected in catalog.Rejected) Debug.LogError($"Combattant « {rejected.Key} » refusé :\n{rejected.Value}");
            foreach (var fighter in catalog.Fighters)
                foreach (var warning in fighter.Warnings) Debug.LogWarning($"{fighter.Name} : {warning}");

            var camera = Camera.main ?? new GameObject("Caméra", typeof(Camera)).GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            flow = new GameFlow(catalog, camera, gameObject);
            // Lien d'invitation (?salon=CODE en WebGL, -salon CODE sur PC) : directement au salon en ligne.
            flow.Go(OnlineSettings.Has("salon") ? flow.Lobby() : flow.Title());
        }

        void Update() => flow?.Current?.Update(Time.unscaledDeltaTime);

        public void Automate(IInputProvider first, IInputProvider second) => Fight.Automate(first, second);
        public void StepAutomated(int ticks) => Fight.StepAutomated(ticks);
        public void Restart() => Fight.Restart();

        void OnApplicationFocus(bool hasFocus) => flow?.Current?.OnFocus(hasFocus);

        void OnGUI() => flow?.Current?.OnGUI();
    }
}
