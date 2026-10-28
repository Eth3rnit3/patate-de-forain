using System.Collections.Generic;
using Patate.Application;
using Patate.Combat;
using Patate.Content;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>Un écran du jeu : titre, sélection, combat, résultat…</summary>
    public interface IGameScreen
    {
        void Enter();
        /// <summary>Une image d'affichage.</summary>
        void Update(float deltaTime);
        void OnGUI();
        void Exit();
        void OnFocus(bool hasFocus);
    }

    /// <summary>
    /// Enchaînement des écrans : titre → sélection → combat → résultat. Chaque
    /// écran décide du suivant avec <see cref="Go"/> ; les fabriques ci-dessous
    /// sont les points d'entrée des écrans (un nouveau mode ajoute la sienne,
    /// ex. un salon en ligne). Partage entre écrans : catalogue, caméra,
    /// visuels déjà chargés et réglages du prochain combat.
    /// </summary>
    public sealed class GameFlow
    {
        readonly Dictionary<string, FighterVisuals> visuals = new Dictionary<string, FighterVisuals>();

        public FighterCatalog Catalog { get; }
        public Camera Camera { get; }
        /// <summary>Objet hôte des composants créés par les écrans (sources audio…).</summary>
        public GameObject Host { get; }
        public MatchSetup Setup { get; set; } = MatchSetup.Default();
        public IGameScreen Current { get; private set; }

        public GameFlow(FighterCatalog catalog, Camera camera, GameObject host)
        {
            Catalog = catalog;
            Camera = camera;
            Host = host;
        }

        /// <summary>Change d'écran. Un écran peut appeler Go depuis son Enter pour passer directement au suivant.</summary>
        public void Go(IGameScreen next)
        {
            var previous = Current;
            Current = next;
            previous?.Exit();
            next.Enter();
        }

        public FighterVisuals Visuals(string id)
        {
            if (!visuals.TryGetValue(id, out var result)) visuals[id] = result = new FighterVisuals(Catalog.Get(id));
            return result;
        }

        public IGameScreen Title() => new TitleScreen(this);
        public IGameScreen CharacterSelect() => new CharacterSelectScreen(this);
        public IGameScreen Fight() => new FightScreen(this, Setup.Copy());
        public IGameScreen Result(MatchState final) => new ResultScreen(this, final);
        public IGameScreen Lobby() => new LobbyScreen(this);
    }
}
