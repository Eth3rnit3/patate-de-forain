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
    /// Combat en ligne : comme <see cref="FightScreen"/>, mais la simulation
    /// avance par <see cref="OnlineMatch"/>, qui échange les commandes avec
    /// l'adversaire et corrige ses prédictions. Pas de pause ni de changement
    /// de combattant ; à la fin de la manche, Entrée propose une revanche,
    /// qui commence quand les deux l'ont demandée.
    /// </summary>
    public sealed class OnlineFightScreen : IGameScreen
    {
        const float ReferenceHeight = 1080f;
        const string DebugPreference = "patate.debug-overlay";

        readonly GameFlow flow;
        readonly OnlineLink link;
        readonly MatchSetup setup;
        readonly bool robot;
        readonly List<MatchEvent> events = new List<MatchEvent>();
        MatchSession session;
        OnlineMatch match;
        MatchView view;
        StageView stageView;
        AudioCues audioCues;
        Hud hud;
        FixedStepClock clock;
        ILocalInput local;
        ILocalInput[] inputs;
        bool showDebug, showHelp, localRematch, remoteRematch, peerGone, reported;
        float waiting, pingClock;
        int ping = -1, round = 1;
        string startupError;

        /// <summary>Combat en cours, pour les scripts de vérification.</summary>
        public OnlineMatch Match => match;

        public OnlineFightScreen(GameFlow flow, OnlineLink link, MatchSetup setup, bool robot)
        {
            this.flow = flow;
            this.link = link;
            this.setup = setup;
            this.robot = robot;
        }

        int LocalSlot => link.Slot;

        public void Enter()
        {
            if (setup.Fighters.Any(id => flow.Catalog.Get(id) == null))
            {
                startupError = $"Combattant introuvable ou invalide : {string.Join(" / ", setup.Fighters)}. Voir la console.";
                Debug.LogError(startupError);
                return;
            }
            local = InputSources.Create(setup.Players[LocalSlot], LocalSlot, () => session);
            inputs = new ILocalInput[2];
            inputs[LocalSlot] = local;
            inputs[1 - LocalSlot] = new RemotePlayer();
            stageView = new StageView(setup.Stage, flow.Camera);
            Begin();
            view = new MatchView(flow.Camera, session.Context.Rules, Visuals(0), Visuals(1), stageView);
            audioCues = new AudioCues(flow.Host, Visuals(0), Visuals(1), stageView.Music);
            hud = new Hud();
            clock = new FixedStepClock(session.Context.Rules.TicksPerSecond);
            showDebug = PlayerPrefs.GetInt(DebugPreference, 0) == 1;
            Debug.Log($"En ligne, salon {link.Code}, place {LocalSlot} : {session.Context.Fighters[0].Ref} contre {session.Context.Fighters[1].Ref}");
        }

        /// <summary>Nouvelle manche, au tick 0 des deux côtés.</summary>
        void Begin()
        {
            session = MatchSession.Start(flow.Catalog, setup.Fighters[0], setup.Fighters[1], MatchRules.Default(), StatPolicy.Free());
            match = new OnlineMatch(session.Context, session.State, LocalSlot, link.Send);
            localRematch = remoteRematch = reported = false;
            view?.Clear();
            clock?.Reset();
        }

        public void Exit()
        {
            view?.Dispose();
            stageView?.Dispose();
            audioCues?.Dispose();
            link.Close();
        }

        FighterVisuals Visuals(int slot) => flow.Visuals(setup.Fighters[slot]);

        public void Update(float deltaTime)
        {
            if (session == null) return;
            Receive();
            if (HandleKeys()) return;
            local.Sample();
            var input = robot ? OnlineRobot.InputFor(LocalSlot, match.State.Tick) : local.InputFor(match.State.Tick + 1);

            // Pas de pause en ligne : la perte du focus n'arrête pas le temps, l'adversaire continue.
            int ticks = clock.Consume(deltaTime);
            if (peerGone || link.State == RelaySocketState.Closed) ticks = 0;
            events.Clear();
            match.Advance(ticks, input, events);
            view.OnEvents(events);
            audioCues.OnEvents(events);
            waiting = match.Waiting ? waiting + deltaTime : 0;
            view.Render(match.State);
            stageView.Follow();

            pingClock += deltaTime;
            if (pingClock >= 1)
            {
                pingClock = 0;
                link.Send(OnlineProtocol.Words(OnlineProtocol.Ping, Milliseconds()));
            }
            Report();
        }

        static int Milliseconds() => (int)(Time.realtimeSinceStartupAsDouble * 1000);

        void Receive()
        {
            while (link.TryNext(out var next))
            {
                if (next.Kind == LinkEventKind.PeerLeft) peerGone = true;
                if (next.Kind != LinkEventKind.Message || match.Receive(next.Text)) continue;
                var parts = OnlineProtocol.Split(next.Text);
                switch (parts[0])
                {
                    case OnlineProtocol.Rematch:
                        remoteRematch = true;
                        TryRematch();
                        break;
                    case OnlineProtocol.Quit:
                        peerGone = true;
                        break;
                    case OnlineProtocol.Ping when parts.Length >= 2:
                        link.Send(OnlineProtocol.Words(OnlineProtocol.Pong, parts[1]));
                        break;
                    case OnlineProtocol.Pong when parts.Length >= 2 && OnlineProtocol.TryInt(parts[1], out int sent):
                        ping = Milliseconds() - sent;
                        break;
                }
            }
        }

        void TryRematch()
        {
            if (!localRematch || !remoteRematch) return;
            round++;
            Begin();
        }

        /// <summary>Touches de service. Renvoie vrai si l'écran a été quitté.</summary>
        bool HandleKeys()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            if (keyboard.f1Key.wasPressedThisFrame)
            {
                showDebug = !showDebug;
                PlayerPrefs.SetInt(DebugPreference, showDebug ? 1 : 0);
            }
            if (keyboard.f2Key.wasPressedThisFrame) showHelp = !showHelp;
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                link.Send(OnlineProtocol.Quit);
                flow.Go(flow.Lobby());
                return true;
            }
            bool enter = keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame;
            if (enter && match.IsOver && !localRematch && !peerGone)
            {
                localRematch = true;
                link.Send(OnlineProtocol.Rematch);
                TryRematch();
            }
            return false;
        }

        /// <summary>Fin de manche ou désynchronisation, une fois, dans la console (et celle du navigateur).</summary>
        void Report()
        {
            if (reported || (!match.IsOver && !match.DesyncTick.HasValue)) return;
            reported = true;
            var state = match.State;
            var stats = $"{match.Rollbacks} rollbacks ({match.ResimulatedTicks} ticks resimulés), {match.ComparedChecksums} empreintes comparées";
            if (match.DesyncTick.HasValue)
                Debug.LogError($"En ligne, salon {link.Code}, manche {round} : désynchronisation au tick {match.DesyncTick} ; {stats}.");
            else
                Debug.Log($"En ligne, salon {link.Code}, manche {round} : finie au tick {state.Tick}, vainqueur {(state.Round.Winner == RoundState.Draw ? "aucun" : $"J{state.Round.Winner + 1}")}, " +
                    $"empreinte {StateCodec.Checksum(state)} ; {stats}, aucune désynchronisation.");
        }

        public void OnFocus(bool hasFocus) { }

        public void OnGUI()
        {
            if (startupError != null)
            {
                GUI.Label(new Rect(20, 20, Screen.width - 40, 200), startupError, new GUIStyle { fontSize = 24, wordWrap = true, normal = { textColor = Color.red } });
                return;
            }
            if (session == null) return;
            var fighters = new[] { Visuals(0), Visuals(1) };
            if (showDebug) hud.DrawDebug(match.State, session.Context, view, StateCodec.Checksum(match.State), audioCues.Missing);
            hud.Draw(match.State, fighters, inputs, showHelp, false);
            DrawOnline();
        }

        /// <summary>Par-dessus le HUD : salon, latence, attente, revanche, et l'aide du mode en ligne.</summary>
        void DrawOnline()
        {
            var previous = GUI.matrix;
            float scale = Screen.height / ReferenceHeight;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float width = Screen.width / scale;
            var small = new GUIStyle { fontSize = 22, alignment = TextAnchor.MiddleCenter, richText = true, normal = { textColor = new Color(0.92f, 0.92f, 0.95f) } };
            var big = new GUIStyle(small) { fontSize = 40, fontStyle = FontStyle.Bold };

            GUI.Label(new Rect(0, 112, width, 30), $"En ligne · salon {link.Code} · vous êtes J{LocalSlot + 1}{(ping >= 0 ? $" · {ping} ms" : "")}", small);

            // Remplace la ligne d'aide du HUD, dont les touches F3 à F5 et Échap ne valent pas en ligne.
            Fill(new Rect(0, ReferenceHeight - 46, width, 46), new Color(0.05f, 0.05f, 0.08f, 1f));
            GUI.Label(new Rect(0, ReferenceHeight - 44, width, 40), "F1 : débogage   F2 : commandes   Entrée en fin de manche : revanche   Échap : quitter le salon", small);

            string message = null;
            if (link.State == RelaySocketState.Closed) message = $"<color=#ff7359>Connexion au relais perdue : {link.CloseReason}</color>\nÉchap : revenir au salon";
            else if (peerGone) message = "<color=#ff7359>L'adversaire a quitté le salon.</color>\nÉchap : revenir au salon";
            else if (match.DesyncTick.HasValue) message = $"<color=#ff7359>Désynchronisation au tick {match.DesyncTick} : les deux jeux ne voient plus le même combat.</color>\nÉchap : quitter";
            else if (match.IsOver) message = localRematch ? "Revanche demandée, en attente de l'adversaire…" : remoteRematch ? "L'adversaire veut sa revanche : Entrée pour l'accepter" : "Entrée : revanche   Échap : quitter le salon";
            else if (waiting > 0.3f) message = "Attente de l'adversaire…";
            if (message != null)
            {
                Fill(new Rect(width / 2 - 620, 620, 1240, 120), new Color(0, 0, 0, 0.75f));
                GUI.Label(new Rect(width / 2 - 600, 625, 1200, 110), message, big);
            }
            GUI.matrix = previous;
        }

        static void Fill(Rect rect, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        /// <summary>Place de l'adversaire pour le HUD : ses commandes arrivent par le réseau.</summary>
        sealed class RemotePlayer : ILocalInput
        {
            public string Label => "Adversaire en ligne";
            public void Sample() { }
            public Buttons InputFor(int tick) => Buttons.None;

            public IEnumerable<(string action, string keys)> Help()
            {
                yield return ("Commandes", "jouées sur l'autre machine");
            }
        }
    }
}
