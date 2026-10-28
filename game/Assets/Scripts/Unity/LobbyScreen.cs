using System.Collections.Generic;
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
    /// <see cref="OnlineLobby"/> ; cet écran l'affiche dans le style des
    /// menus, au clavier, à la manette ou à la souris.
    /// </summary>
    public sealed class LobbyScreen : IGameScreen
    {
        const int MaxCodeLength = 12;
        // Les paramètres de la page (?salon=…) ne servent qu'à la première ouverture du salon.
        static bool parametersUsed;

        enum Item { Fighter, Create, Join }

        readonly GameFlow flow;
        string relay, code = "", fighter, error;
        OnlineLobby lobby;
        bool robot, editingRelay;
        Item selected = Item.Create;
        int roomSelected = 1; // 0 combattant, 1 prêt, 2 quitter
        float age;

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
            roomSelected = 1;
            Debug.Log($"En ligne : salon {code} sur {relay} ({(join ? "rejoindre" : "créer")})");
        }

        void Create()
        {
            code = OnlineSettings.NewCode();
            Connect(join: false);
        }

        void Leave()
        {
            lobby.Link.Close();
            lobby = null;
        }

        void ChangeFighter(int step)
        {
            var ids = flow.Catalog.Fighters.Select(f => f.Id).ToList();
            int index = Mathf.Max(0, ids.IndexOf(fighter));
            fighter = ids[(index + step + ids.Count) % ids.Count];
            flow.Setup.Fighters[0] = fighter;
            lobby?.Choose(fighter);
        }

        public void Update(float deltaTime)
        {
            age += deltaTime;
            if (age < 0.2f || editingRelay) return; // laisse retomber l'appui qui a ouvert l'écran ; la souris édite le relais
            if (lobby == null) UpdateMenu();
            else UpdateRoom();
        }

        void UpdateMenu()
        {
            MenuCommand command;
            if (selected == Item.Join)
            {
                // Le code se tape au clavier : les lettres vont au code, pas aux commandes des menus.
                foreach (var c in TypedCharacters())
                    if (code.Length < MaxCodeLength) code = OnlineSettings.CleanCode(code + c);
                if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame && code.Length > 0)
                    code = code.Substring(0, code.Length - 1);
                command = TypingCommands();
            }
            else command = MenuInput.Any();

            if ((command & MenuCommand.Back) != 0)
            {
                flow.Go(new ModeSelectScreen(flow));
                return;
            }
            if ((command & MenuCommand.Up) != 0) selected = (Item)(((int)selected + 2) % 3);
            if ((command & MenuCommand.Down) != 0) selected = (Item)(((int)selected + 1) % 3);
            if (selected == Item.Fighter && (command & MenuCommand.Left) != 0) ChangeFighter(-1);
            if (selected == Item.Fighter && (command & MenuCommand.Right) != 0) ChangeFighter(1);
            if ((command & MenuCommand.Confirm) == 0) return;
            if (selected == Item.Create) Create();
            else if (selected == Item.Join) Connect(join: true);
        }

        /// <summary>
        /// Lettres et chiffres appuyés cette image. Les lettres suivent la
        /// disposition du clavier (nom affiché de la touche) ; les chiffres
        /// se lisent à leur place, sans Maj, même sur un clavier AZERTY.
        /// </summary>
        static IEnumerable<char> TypedCharacters()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) yield break;
            foreach (var key in Letters)
            {
                if (!keyboard[key].wasPressedThisFrame) continue;
                var name = keyboard[key].displayName;
                yield return name.Length == 1 && char.IsLetter(name[0]) ? name[0] : (char)('A' + (key - Key.A));
            }
            for (int digit = 0; digit < 10; digit++)
                if (keyboard[Digits[digit]].wasPressedThisFrame || keyboard[NumpadDigits[digit]].wasPressedThisFrame) yield return (char)('0' + digit);
        }

        static readonly Key[] Letters = Enumerable.Range(0, 26).Select(i => Key.A + i).ToArray();
        static readonly Key[] Digits = { Key.Digit0, Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9 };
        static readonly Key[] NumpadDigits = { Key.Numpad0, Key.Numpad1, Key.Numpad2, Key.Numpad3, Key.Numpad4, Key.Numpad5, Key.Numpad6, Key.Numpad7, Key.Numpad8, Key.Numpad9 };

        /// <summary>Pendant la saisie du code : flèches, Entrée, Échap, Retour arrière, et la manette.</summary>
        static MenuCommand TypingCommands()
        {
            var result = MenuCommand.None;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.upArrowKey.wasPressedThisFrame) result |= MenuCommand.Up;
                if (keyboard.downArrowKey.wasPressedThisFrame) result |= MenuCommand.Down;
                if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) result |= MenuCommand.Confirm;
                if (keyboard.escapeKey.wasPressedThisFrame) result |= MenuCommand.Back;
            }
            foreach (var pad in Gamepad.all) result |= MenuInput.Of(PlayerSource.Gamepad(pad.deviceId));
            return result;
        }

        void UpdateRoom()
        {
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

            var command = MenuInput.Any();
            if ((command & MenuCommand.Back) != 0)
            {
                Leave();
                return;
            }
            if ((command & MenuCommand.Up) != 0) roomSelected = (roomSelected + 2) % 3;
            if ((command & MenuCommand.Down) != 0) roomSelected = (roomSelected + 1) % 3;
            if (roomSelected == 0 && !lobby.LocalReady && (command & MenuCommand.Left) != 0) ChangeFighter(-1);
            if (roomSelected == 0 && !lobby.LocalReady && (command & MenuCommand.Right) != 0) ChangeFighter(1);
            if ((command & MenuCommand.Confirm) == 0) return;
            if (roomSelected == 2) Leave();
            else lobby.SetReady();
        }

        public void OnGUI()
        {
            var previous = Ui.Begin();
            float time = Time.unscaledTime, center = Ui.Width / 2;
            Ui.Background(0.55f);
            Ui.Logo(new Rect(center - 200, 16, 400, 200));
            Ui.Text(new Rect(0, 210, Ui.Width, 70), "COMBAT EN LIGNE", Ui.Style(56), Ui.Yellow, 4);

            if (lobby == null) DrawMenu(center, time);
            else DrawRoom(center, time);
            if (error != null) Ui.Text(new Rect(center - 700, 930, 1400, 60), error, Ui.Style(28, wrap: true), new Color(1f, 0.55f, 0.45f), 2);
            GUI.matrix = previous;
        }

        /// <summary>Bouton d'enseigne qui répond aussi à la souris.</summary>
        bool Button(Rect rect, string label, string detail, bool isSelected, bool enabled, float time)
        {
            Ui.Button(rect, label, detail, isSelected, enabled, time);
            return enabled && GUI.Button(rect, GUIContent.none, GUIStyle.none);
        }

        void DrawMenu(float center, float time)
        {
            const float width = 820, height = 130, gap = 30;
            float x = center - width / 2, y = 310;
            if (Button(new Rect(x, y, width, height), $"<   {Name(fighter).ToUpperInvariant()}   >", "Votre combattant : Gauche / Droite", selected == Item.Fighter, true, time))
            {
                selected = Item.Fighter;
                ChangeFighter(1);
            }
            if (Button(new Rect(x, y + height + gap, width, height), "CRÉER UN SALON", "Un code à donner à votre adversaire", selected == Item.Create, true, time))
                Create();
            string shown = code.Length > 0 ? code : "____";
            string cursor = selected == Item.Join && Mathf.Repeat(time, 1f) < 0.5f ? "|" : " ";
            if (Button(new Rect(x, y + 2 * (height + gap), width, height), "REJOINDRE", $"Code : {shown}{cursor}   (tapez-le au clavier, puis Entrée)", selected == Item.Join, true, time))
            {
                if (selected == Item.Join) Connect(join: true);
                selected = Item.Join;
            }

            // Réglage avancé, à la souris : l'adresse du relais.
            Ui.Text(new Rect(center - 420, 800, 840, 30), "Relais", Ui.Style(22, fontStyle: FontStyle.Normal), Ui.Muted, 0);
            var field = new GUIStyle(GUI.skin.textField) { fontSize = 22, alignment = TextAnchor.MiddleCenter };
            // Entrée ou Échap rend la main aux menus.
            if (editingRelay && Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
            {
                GUI.FocusControl(null);
                Event.current.Use();
            }
            GUI.SetNextControlName("relais");
            relay = GUI.TextField(new Rect(center - 420, 832, 840, 44), relay, field);
            editingRelay = GUI.GetNameOfFocusedControl() == "relais";

            Ui.Hint(selected == Item.Join
                ? "Lettres et chiffres : code      Retour arrière : effacer      Entrée ou A : rejoindre      Haut / Bas : choisir      Échap : retour"
                : "Haut / Bas : choisir      Gauche / Droite : combattant      Entrée, Espace ou A : valider      Échap ou B : retour");
        }

        void DrawRoom(float center, float time)
        {
            var codePanel = new Rect(center - 360, 300, 720, 150);
            Ui.Marquee(codePanel, Ui.Red, true, time);
            Ui.Text(new Rect(codePanel.x, codePanel.y + 14, codePanel.width, 30), "SALON", Ui.Style(24, fontStyle: FontStyle.Normal), Ui.Cream, 0);
            Ui.Text(new Rect(codePanel.x, codePanel.y + 40, codePanel.width, 80), code, Ui.Style(72), Ui.Yellow, 4);
            Ui.Text(new Rect(codePanel.x, codePanel.y + 112, codePanel.width, 28), "Donnez ce code à votre adversaire", Ui.Style(22, fontStyle: FontStyle.Normal), Ui.Cream, 0);

            var link = lobby.Link;
            string status;
            if (link.State == RelaySocketState.Connecting) status = "Connexion au relais…";
            else if (!link.PeerPresent) status = "En attente d'un adversaire…";
            else if (!lobby.Greeted) status = "Adversaire arrivé, présentation…";
            else status = $"Adversaire : {Name(lobby.RemoteFighter)}{(lobby.RemoteReady ? " — prêt !" : "")}";
            Ui.Text(new Rect(0, 470, Ui.Width, 50), status, Ui.Style(34), lobby.RemoteReady ? Ui.Yellow : Ui.Cream, 3);

            const float width = 820, height = 110, gap = 26;
            float x = center - width / 2, y = 545;
            if (Button(new Rect(x, y, width, height), $"<   {Name(fighter).ToUpperInvariant()}   >", "Votre combattant : Gauche / Droite", roomSelected == 0, !lobby.LocalReady, time))
            {
                roomSelected = 0;
                ChangeFighter(1);
            }
            string ready = lobby.LocalReady ? "PRÊT !" : "PRÊT";
            string readyDetail = lobby.LocalReady ? "En attente de l'adversaire" : lobby.CanBeReady ? "Le combat commence quand les deux joueurs sont prêts" : "En attente de l'adversaire";
            if (Button(new Rect(x, y + height + gap, width, height), ready, readyDetail, roomSelected == 1, lobby.CanBeReady && !lobby.LocalReady, time))
                lobby.SetReady();
            if (Button(new Rect(x, y + 2 * (height + gap), width, height), "QUITTER LE SALON", null, roomSelected == 2, true, time))
                Leave();
            if (lobby?.Problem != null) Ui.Text(new Rect(center - 700, 960, 1400, 50), lobby.Problem, Ui.Style(26, wrap: true), new Color(1f, 0.55f, 0.45f), 2);
            Ui.Hint("Haut / Bas : choisir      Gauche / Droite : combattant      Entrée, Espace ou A : valider      Échap ou B : quitter le salon");
        }

        string Name(string id) => id == null ? "?" : flow.Catalog.Get(id)?.Name ?? id;
    }
}
