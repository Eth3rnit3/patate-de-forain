using System.Collections.Generic;
using System.Linq;
using Patate.Application;
using Patate.Content;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Sélection des personnages : chaque joueur rejoint en appuyant sur la
    /// validation de son appareil (moitié de clavier ou manette), choisit son
    /// combattant dans la grille des portraits, puis le décor s'il y en a
    /// plusieurs. Les règles sont dans <see cref="CharacterSelection"/> ;
    /// l'écran lit les appareils et dessine.
    /// </summary>
    public sealed class CharacterSelectScreen : IGameScreen
    {
        const float LaunchDelay = 0.9f;
        /// <summary>Appuis ignorés à l'arrivée : la validation de l'écran précédent ne fait pas rejoindre.</summary>
        const float EntryGuard = 0.25f;

        readonly GameFlow flow;
        readonly bool keepPlayers;
        CharacterSelection selection;
        IReadOnlyList<FighterPackage> fighters;
        SortedDictionary<string, string> stageNames;
        string notice;
        float noticeAge = 99, age, doneAge;
        readonly float[] lockAge = { 99, 99 };

        /// <param name="keepPlayers">Garder les joueurs déjà placés (retour depuis le résultat).</param>
        public CharacterSelectScreen(GameFlow flow, bool keepPlayers = false)
        {
            this.flow = flow;
            this.keepPlayers = keepPlayers;
        }

        public void Enter()
        {
            fighters = flow.Catalog.Fighters;
            stageNames = Stages.Available();
            var setup = flow.Setup;
            PlayerSource? cpu = setup.Players[1].Kind == PlayerSourceKind.Cpu ? setup.Players[1] : (PlayerSource?)null;
            selection = new CharacterSelection(setup.Mode, fighters.Select(f => f.Id).ToList(), stageNames.Keys.ToList(), setup, cpu, keepPlayers);
            age = 0;
        }

        public void Update(float deltaTime)
        {
            age += deltaTime;
            noticeAge += deltaTime;
            for (int slot = 0; slot < 2; slot++) lockAge[slot] += deltaTime;
            if (selection.Phase == SelectionPhase.Done)
            {
                doneAge += deltaTime;
                if (doneAge >= LaunchDelay)
                {
                    flow.Setup = selection.ToSetup();
                    flow.Go(flow.Fight());
                }
                return;
            }
            if (age < EntryGuard) return;
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                flow.Go(new ModeSelectScreen(flow));
                return;
            }
            if (fighters.Count == 0) return;

            bool[] wasLocked = selection.Slots.Select(s => s.Locked).ToArray();
            foreach (var device in MenuInput.Devices())
            {
                var command = MenuInput.Of(device);
                if (command == MenuCommand.None) continue;
                if (selection.SlotOf(device) >= 0) selection.Command(device, command);
                else if ((command & MenuCommand.Confirm) != 0) TryJoin(device);
                else if ((command & MenuCommand.Back) != 0 && selection.Slots.All(s => !s.Joined || s.IsCpu)) flow.Go(new ModeSelectScreen(flow));
                if (flow.Current != this) return;
            }
            for (int slot = 0; slot < 2; slot++)
                if (selection.Slots[slot].Locked && !wasLocked[slot]) lockAge[slot] = 0;
            if (selection.Phase == SelectionPhase.Done) doneAge = 0;
        }

        void TryJoin(PlayerSource device)
        {
            if (!InputSupport.Available(device))
            {
                Notice($"{MenuInput.DeviceName(device)} : pas encore prise en charge en combat. Jouez au clavier pour l'instant.");
                return;
            }
            if (selection.Join(device) < 0) Notice(selection.Mode == MatchMode.Solo ? "Le mode Solo se joue seul : l'autre place est à l'ordinateur." : "Les deux places sont prises.");
        }

        void Notice(string text)
        {
            notice = text;
            noticeAge = 0;
        }

        // ------------------------------------------------------------ dessin

        public void OnGUI()
        {
            var previous = Ui.Begin();
            float time = Time.unscaledTime, width = Ui.Width;
            Ui.Background(0.6f);
            string title = selection.Mode == MatchMode.Solo ? "CHOISISSEZ VOTRE COMBATTANT" : "CHOISISSEZ VOS COMBATTANTS";
            Ui.Text(new Rect(0, 26, width, 70), title, Ui.Style(54), Ui.Yellow, 4);

            if (fighters.Count == 0)
            {
                Ui.Text(new Rect(0, 400, width, 80), "Aucun combattant valide dans le catalogue. Voir la console.", Ui.Style(36), Ui.Red, 3);
                GUI.matrix = previous;
                return;
            }

            float panelWidth = Mathf.Min(520, width * 0.3f);
            DrawPlayer(0, new Rect(30, 120, panelWidth, 760), time);
            DrawPlayer(1, new Rect(width - 30 - panelWidth, 120, panelWidth, 760), time);
            DrawGrid(new Rect(30 + panelWidth + 30, 150, width - 2 * (60 + panelWidth), 500), time);
            DrawStage(new Rect(width / 2 - 360, 690, 720, 110), time);

            if (notice != null && noticeAge < 4)
            {
                float alpha = Mathf.Clamp01((4 - noticeAge) / 0.5f);
                Ui.Fill(new Rect(width / 2 - 620, 820, 1240, 56), new Color(0.5f, 0.06f, 0.04f, 0.85f * alpha));
                Ui.Text(new Rect(width / 2 - 620, 820, 1240, 56), notice, Ui.Style(26), Ui.Cream, 2, alpha);
            }
            if (selection.Phase == SelectionPhase.Done) Ui.Word("combat", "COMBAT !", new Vector2(width / 2, 470), 260, doneAge);

            string pad = MenuInput.Devices().Any(d => d.Kind == PlayerSourceKind.Gamepad) ? "      Manette : A valider, B retour" : "";
            Ui.Hint($"Clavier gauche : {Keys(Key.W, Key.A, Key.S, Key.D)} + {MenuInput.KeyName(Key.F)} valider, {MenuInput.KeyName(Key.G)} retour" +
                    $"      Clavier droit : flèches + {MenuInput.KeyName(Key.K)} ou Entrée valider, {MenuInput.KeyName(Key.L)} retour{pad}      Échap : modes");
            GUI.matrix = previous;
        }

        static string Keys(params Key[] keys) => string.Concat(keys.Select(MenuInput.KeyName));

        void DrawPlayer(int slot, Rect rect, float time)
        {
            var state = selection.Slots[slot];
            var color = Ui.PlayerColors[slot];
            Ui.Marquee(rect, state.Joined ? color : new Color(0.25f, 0.25f, 0.35f), state.Locked, time, 40);
            var inner = new Rect(rect.x + 24, rect.y + 24, rect.width - 48, rect.height - 48);
            string tag = state.IsCpu ? "ORDINATEUR" : $"JOUEUR {slot + 1}";
            Ui.Text(new Rect(inner.x, inner.y, inner.width, 50), tag, Ui.Style(36), state.Joined ? color : Ui.Muted, 3);

            if (!state.Joined)
            {
                float blink = 0.6f + 0.4f * Mathf.Sin(time * 4 + slot);
                Ui.Text(new Rect(inner.x, inner.y + 200, inner.width, 120), "APPUYEZ POUR\nREJOINDRE", Ui.Style(44), Ui.Yellow, 3, blink);
                var lines = new List<string>();
                if (Keyboard.current != null)
                {
                    lines.Add($"Clavier gauche : {MenuInput.ConfirmName(PlayerSource.Keyboard(0))}");
                    lines.Add($"Clavier droit : {MenuInput.ConfirmName(PlayerSource.Keyboard(1))}");
                }
                // Dans le navigateur, une manette ne se signale qu'après un premier appui : le suivant la fait rejoindre.
                lines.Add(Gamepad.all.Count == 0 ? "Manette : bouton A (deux fois au premier branchement)" : "Manette : bouton A");
                Ui.Text(new Rect(inner.x, inner.y + 380, inner.width, 200), string.Join("\n", lines), Ui.Style(26, fontStyle: FontStyle.Normal, wrap: true), Ui.Cream, 0);
                return;
            }

            var fighter = fighters[state.Cursor];
            var visuals = flow.Visuals(fighter.Id);
            float pop = Mathf.Clamp01(lockAge[slot] / 0.2f);
            var portrait = new Rect(inner.x, inner.y + 60, inner.width, inner.width);
            if (state.Locked) Ui.Disc(portrait.center, portrait.width * 0.48f * (1.1f - 0.1f * pop), new Color(color.r, color.g, color.b, 0.35f));
            // Portrait dessiné regardant à droite : miroir pour le joueur 2, face à son adversaire.
            Ui.Texture(portrait, visuals.Portrait, mirror: slot == 1);
            Ui.Text(new Rect(inner.x, portrait.yMax + 6, inner.width, 64), fighter.Name.ToUpperInvariant(), Ui.Style(46), Ui.Cream, 4);

            string status;
            if (selection.Phase == SelectionPhase.Done || state.Locked) status = "PRÊT !";
            else if (state.IsCpu) status = selection.Slots[0].Locked ? "Le joueur 1 choisit pour l'ordinateur" : "En attente du joueur 1";
            else status = $"{MenuInput.DeviceName(state.Source.Value)} : {MenuInput.ConfirmName(state.Source.Value)} pour valider";
            var statusStyle = Ui.Style(state.Locked ? 44 : 24, fontStyle: state.Locked ? FontStyle.Bold : FontStyle.Normal, wrap: true);
            Ui.Text(new Rect(inner.x, inner.yMax - 90, inner.width, 90), status, statusStyle, state.Locked ? Ui.Yellow : Ui.Cream, state.Locked ? 3 : 0);
        }

        void DrawGrid(Rect area, float time)
        {
            int columns = selection.Columns;
            int rows = Mathf.CeilToInt(fighters.Count / (float)columns);
            float size = Mathf.Min(200, (area.width - (columns - 1) * 24) / columns, (area.height - (rows - 1) * 24) / rows);
            float gridWidth = columns * size + (columns - 1) * 24;
            float gridHeight = rows * size + (rows - 1) * 24;
            float left = area.center.x - gridWidth / 2, top = area.center.y - gridHeight / 2;
            for (int i = 0; i < fighters.Count; i++)
            {
                var tile = new Rect(left + i % columns * (size + 24), top + i / columns * (size + 24), size, size);
                Ui.Fill(new Rect(tile.x + 5, tile.y + 7, tile.width, tile.height), new Color(0, 0, 0, 0.45f));
                Ui.Gradient(tile, Ui.NavyLight, Ui.Navy);
                Ui.Texture(new Rect(tile.x + 6, tile.y + 6, tile.width - 12, tile.height - 12), flow.Visuals(fighters[i].Id).Portrait);
                Ui.Frame(tile, new Color(1, 1, 1, 0.25f), 3);
                for (int slot = 0; slot < 2; slot++)
                {
                    var state = selection.Slots[slot];
                    if (!state.Joined || state.Cursor != i) continue;
                    var color = Ui.PlayerColors[slot];
                    float pulse = state.Locked ? 0 : 3 + Mathf.Sin(time * 8) * 3;
                    // Deux curseurs sur la même case : celui du joueur 2 un peu plus large.
                    float grow = pulse + slot * 10;
                    Ui.Frame(new Rect(tile.x - grow, tile.y - grow, tile.width + grow * 2, tile.height + grow * 2), color, 7);
                    // J1 au-dessus de la case, J2 en dessous : lisibles même sur la même case.
                    var label = slot == 0 ? new Rect(tile.x - grow, tile.y - grow - 40, 120, 40) : new Rect(tile.xMax + grow - 120, tile.yMax + grow, 120, 40);
                    Ui.Fill(label, color);
                    Ui.Text(label, state.IsCpu ? "ORDI" : $"J{slot + 1}", Ui.Style(28), Ui.Cream, 2);
                }
            }
        }

        void DrawStage(Rect rect, float time)
        {
            bool choosing = selection.Phase == SelectionPhase.Stage;
            Ui.Marquee(rect, choosing ? Ui.Red : new Color32(0x6b, 0x1c, 0x1a, 0xff), choosing, time, 30);
            string id = selection.Stages[selection.StageCursor];
            string name = stageNames.TryGetValue(id, out var n) ? n : id;
            bool several = selection.Stages.Count > 1;
            Ui.Text(new Rect(rect.x, rect.y + 14, rect.width, 32), several ? "DÉCOR" : "DÉCOR (un seul pour l'instant)", Ui.Style(24, fontStyle: FontStyle.Normal), Ui.Muted, 0);
            Ui.Text(new Rect(rect.x, rect.y + 46, rect.width, 50), several ? $"<   {name}   >" : name, Ui.Style(38), choosing ? Ui.Yellow : Ui.Cream, 3);
            if (choosing) Ui.Text(new Rect(rect.x, rect.yMax + 4, rect.width, 32), "Gauche / Droite pour choisir, valider pour combattre", Ui.Style(22, fontStyle: FontStyle.Normal), Ui.Cream, 0);
        }

        public void Exit() { }
        public void OnFocus(bool hasFocus) { }
    }

    /// <summary>Décors livrés : tout dossier de Resources/Stages qui contient un stage.json lisible.</summary>
    public static class Stages
    {
        /// <summary>Identifiant → nom affiché, dans l'ordre des identifiants.</summary>
        public static SortedDictionary<string, string> Available()
        {
            var result = new SortedDictionary<string, string>(System.StringComparer.Ordinal);
            foreach (var file in Resources.LoadAll<TextAsset>("Stages").Where(t => t.name == "stage"))
            {
                var stage = StageDefinition.Read(file.text, "stage.json", new ValidationReport());
                if (stage?.Id != null) result[stage.Id] = stage.Name ?? stage.Id;
            }
            return result;
        }
    }
}
