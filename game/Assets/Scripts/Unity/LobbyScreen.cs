using System.Linq;
using Patate.Application;
using Patate.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Salon du jeu en ligne : créer un salon (un code à donner à
    /// l'adversaire) ou en rejoindre un, choisir son combattant, se déclarer
    /// prêt. Le combat (<see cref="OnlineFightScreen"/>) commence quand les
    /// deux joueurs le sont. La logique du salon est dans
    /// <see cref="OnlineLobby"/> ; cet écran l'affiche en IMGUI.
    /// </summary>
    public sealed class LobbyScreen : IGameScreen
    {
        const float ReferenceHeight = 1080f;
        // Les paramètres de la page (?salon=…) ne servent qu'à la première ouverture du salon.
        static bool parametersUsed;

        readonly GameFlow flow;
        string relay, code = "", fighter, error;
        OnlineLobby lobby;
        bool robot;
        GUIStyle title, text, button, field, warning;

        public LobbyScreen(GameFlow flow)
        {
            this.flow = flow;
        }

        public void Enter()
        {
            // Deux fenêtres côte à côte : celle qui n'a pas le focus doit continuer à jouer.
            UnityEngine.Application.runInBackground = true;
            relay = OnlineSettings.Relay;
            fighter = flow.Catalog.Get(flow.Setup.Fighters[0]) != null ? flow.Setup.Fighters[0] : flow.Catalog.Fighters.First().Id;
            robot = OnlineSettings.Has("robot");
            var wanted = OnlineSettings.CleanCode(OnlineSettings.Parameter("salon"));
            if (!parametersUsed && wanted.Length > 0)
            {
                parametersUsed = true;
                code = wanted;
                Connect(join: !OnlineSettings.Has("creer"));
            }
        }

        public void Exit() { }

        public void OnFocus(bool hasFocus) { }

        /// <summary>Commandes du joueur local : celles choisies dans les menus, sinon la moitié gauche du clavier.</summary>
        PlayerSource LocalSource()
        {
            var chosen = flow.Setup.Players[0];
            return chosen.Kind == PlayerSourceKind.Keyboard || chosen.Kind == PlayerSourceKind.Gamepad ? chosen : PlayerSource.Keyboard(0);
        }

        void Connect(bool join)
        {
            error = null;
            if (code.Length < 4)
            {
                error = "Le code d'un salon compte au moins 4 lettres ou chiffres.";
                return;
            }
            OnlineSettings.Relay = relay;
            var link = new OnlineLink(OnlineRelay.Open(relay, code, join), code);
            lobby = new OnlineLobby(link, flow.Catalog, MatchRules.Default(), fighter, flow.Setup.Stage, LocalSource());
            Debug.Log($"En ligne : salon {code} sur {relay} ({(join ? "rejoindre" : "créer")})");
        }

        void Leave()
        {
            lobby.Link.Close();
            lobby = null;
        }

        public void Update(float deltaTime)
        {
            var keyboard = Keyboard.current;
            if (lobby == null)
            {
                if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame) flow.Go(flow.Title());
                return;
            }
            lobby.Update();
            if (lobby.Started != null)
            {
                flow.Go(new OnlineFightScreen(flow, lobby.Link, lobby.Started, robot));
                return;
            }
            if (lobby.Link.State == RelaySocketState.Closed)
            {
                error = $"Salon {code} : {lobby.Link.CloseReason ?? "connexion fermée"}.";
                lobby = null;
                return;
            }
            if (robot && lobby.CanBeReady) lobby.SetReady();
            if (keyboard == null) return;
            if (keyboard.escapeKey.wasPressedThisFrame) Leave();
            else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) lobby.SetReady();
        }

        void Styles()
        {
            if (title != null) return;
            title = new GUIStyle { fontSize = 72, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.85f, 0.3f) } };
            text = new GUIStyle { fontSize = 32, alignment = TextAnchor.MiddleCenter, wordWrap = true, richText = true, normal = { textColor = Color.white } };
            button = new GUIStyle(GUI.skin.button) { fontSize = 34, fontStyle = FontStyle.Bold };
            field = new GUIStyle(GUI.skin.textField) { fontSize = 34, alignment = TextAnchor.MiddleCenter };
            warning = new GUIStyle(text) { normal = { textColor = new Color(1f, 0.45f, 0.35f) } };
        }

        public void OnGUI()
        {
            Styles();
            var previous = GUI.matrix;
            float scale = Screen.height / ReferenceHeight;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float width = Screen.width / scale, center = width / 2;
            GUI.color = new Color(0, 0, 0, 0.75f);
            GUI.DrawTexture(new Rect(center - 560, 90, 1120, 900), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (lobby == null) DrawMenu(center);
            else DrawRoom(center);
            if (error != null) GUI.Label(new Rect(center - 520, 880, 1040, 90), error, warning);
            GUI.matrix = previous;
        }

        void DrawMenu(float center)
        {
            GUI.Label(new Rect(center - 520, 120, 1040, 100), "Combat en ligne", title);
            DrawFighterChoice(center, 250, true);
            if (GUI.Button(new Rect(center - 300, 370, 600, 80), "Créer un salon", button))
            {
                code = OnlineSettings.NewCode();
                Connect(join: false);
            }
            GUI.Label(new Rect(center - 520, 480, 1040, 50), "ou entrez le code d'un salon :", text);
            code = OnlineSettings.CleanCode(GUI.TextField(new Rect(center - 300, 540, 280, 76), code, field));
            if (GUI.Button(new Rect(center + 20, 540, 280, 76), "Rejoindre", button)) Connect(join: true);
            GUI.Label(new Rect(center - 520, 660, 1040, 40), "Relais :", new GUIStyle(text) { fontSize = 24 });
            relay = GUI.TextField(new Rect(center - 420, 705, 840, 50), relay, new GUIStyle(field) { fontSize = 24 });
            GUI.Label(new Rect(center - 520, 780, 1040, 50), "Échap : retour", new GUIStyle(text) { fontSize = 24 });
        }

        void DrawRoom(float center)
        {
            GUI.Label(new Rect(center - 520, 110, 1040, 60), "Salon", text);
            GUI.Label(new Rect(center - 520, 160, 1040, 100), code, new GUIStyle(title) { fontSize = 96 });
            GUI.Label(new Rect(center - 520, 260, 1040, 50), "Donnez ce code à votre adversaire.", new GUIStyle(text) { fontSize = 26 });

            var link = lobby.Link;
            string status;
            if (link.State == RelaySocketState.Connecting) status = "Connexion au relais…";
            else if (!link.PeerPresent) status = "En attente d'un adversaire…";
            else if (!lobby.Greeted) status = "Adversaire arrivé, présentation…";
            else status = $"Adversaire : <b>{Name(lobby.RemoteFighter)}</b>{(lobby.RemoteReady ? "  — prêt" : "")}";
            GUI.Label(new Rect(center - 520, 330, 1040, 60), status, text);

            DrawFighterChoice(center, 420, !lobby.LocalReady);
            GUI.enabled = lobby.CanBeReady;
            var label = lobby.LocalReady ? "Prêt — en attente de l'adversaire" : "Prêt (Entrée)";
            if (GUI.Button(new Rect(center - 330, 560, 660, 80), label, button)) lobby.SetReady();
            GUI.enabled = true;
            if (lobby.Problem != null) GUI.Label(new Rect(center - 520, 660, 1040, 110), lobby.Problem, warning);
            if (GUI.Button(new Rect(center - 220, 780, 440, 70), "Quitter le salon (Échap)", new GUIStyle(button) { fontSize = 26 })) Leave();
        }

        void DrawFighterChoice(float center, float top, bool enabled)
        {
            GUI.Label(new Rect(center - 520, top, 1040, 40), "Votre combattant", new GUIStyle(text) { fontSize = 26 });
            GUI.enabled = enabled;
            var ids = flow.Catalog.Fighters.Select(f => f.Id).ToList();
            int index = Mathf.Max(0, ids.IndexOf(fighter));
            int step = 0;
            if (GUI.Button(new Rect(center - 330, top + 45, 70, 64), "◀", button)) step = -1;
            if (GUI.Button(new Rect(center + 260, top + 45, 70, 64), "▶", button)) step = 1;
            GUI.Label(new Rect(center - 250, top + 45, 500, 64), $"<b>{Name(fighter)}</b>", new GUIStyle(text) { fontSize = 40 });
            GUI.enabled = true;
            if (step == 0) return;
            fighter = ids[(index + step + ids.Count) % ids.Count];
            flow.Setup.Fighters[0] = fighter;
            lobby?.Choose(fighter);
        }

        string Name(string id) => id == null ? "?" : flow.Catalog.Get(id)?.Name ?? id;
    }
}
