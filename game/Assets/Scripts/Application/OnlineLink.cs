using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Patate.Combat;

namespace Patate.Application
{
    public enum RelaySocketState { Connecting, Open, Closed }

    /// <summary>
    /// Connexion texte au relais : WebSocket du navigateur en WebGL, de .NET
    /// sur PC et dans l'éditeur. Lue une fois par image, jamais bloquante.
    /// </summary>
    public interface IRelaySocket
    {
        RelaySocketState State { get; }
        /// <summary>Raison lisible de la fermeture (refus du relais, réseau…).</summary>
        string CloseReason { get; }
        void Send(string text);
        bool TryReceive(out string text);
        void Close();
    }

    public enum LinkEventKind { Entered, PeerArrived, PeerLeft, Message }

    public readonly struct LinkEvent
    {
        public readonly LinkEventKind Kind;
        public readonly string Text;

        public LinkEvent(LinkEventKind kind, string text = null)
        {
            Kind = kind;
            Text = text;
        }
    }

    /// <summary>
    /// Le salon vu par un joueur : sa place, la présence de l'adversaire et
    /// les messages de celui-ci, un par un. Interprète les messages « # » du
    /// relais (server/relay/src/rooms.js) ; le reste vient de l'adversaire.
    /// </summary>
    public sealed class OnlineLink
    {
        readonly IRelaySocket socket;

        public string Code { get; }
        /// <summary>Place dans le salon et donc dans le combat (0 ou 1), -1 avant l'entrée.</summary>
        public int Slot { get; private set; } = -1;
        public bool PeerPresent { get; private set; }
        public RelaySocketState State => socket.State;
        public string CloseReason => socket.CloseReason;

        public OnlineLink(IRelaySocket socket, string code)
        {
            this.socket = socket;
            Code = code;
        }

        public void Send(string message)
        {
            if (socket.State == RelaySocketState.Open) socket.Send(message);
        }

        /// <summary>
        /// Événement suivant, un seul à la fois : un écran qui en passe la
        /// main à un autre (salon → combat) ne consomme pas ceux du suivant.
        /// </summary>
        public bool TryNext(out LinkEvent next)
        {
            while (socket.TryReceive(out var text))
            {
                if (text.Length == 0) continue;
                if (text[0] != '#')
                {
                    next = new LinkEvent(LinkEventKind.Message, text);
                    return true;
                }
                var parts = text.Split(' ');
                switch (parts[0])
                {
                    case "#bienvenue" when parts.Length >= 3 && int.TryParse(parts[1], out int slot) && int.TryParse(parts[2], out int present):
                        Slot = slot;
                        PeerPresent = present >= 2;
                        next = new LinkEvent(LinkEventKind.Entered);
                        return true;
                    case "#arrivee":
                        PeerPresent = true;
                        next = new LinkEvent(LinkEventKind.PeerArrived);
                        return true;
                    case "#depart":
                        PeerPresent = false;
                        next = new LinkEvent(LinkEventKind.PeerLeft);
                        return true;
                }
            }
            next = default;
            return false;
        }

        public void Close() => socket.Close();
    }

    /// <summary>
    /// Messages échangés entre les deux joueurs : une ligne de texte, un mot
    /// en tête, des champs séparés par des espaces. Tout ce qui est mal formé
    /// est ignoré.
    /// </summary>
    public static class OnlineProtocol
    {
        /// <summary>À changer dès que le format ou les constantes du combat en ligne changent.</summary>
        public const int Version = 1;
        /// <summary>Délai d'entrée : une commande lue au tick t est jouée au tick t + délai, des deux côtés.</summary>
        public const int InputDelay = 2;
        /// <summary>Ticks qu'un joueur peut simuler d'avance en prédisant l'adversaire (0 : attente stricte).</summary>
        public const int MaxRollback = 8;
        /// <summary>
        /// Commandes regroupées par message : deux ticks (33 ms) divisent par
        /// deux le nombre de messages, donc ce que compte le relais déployé.
        /// </summary>
        public const int InputsPerMessage = 2;
        /// <summary>Intervalle entre deux comparaisons d'empreinte d'état.</summary>
        public const int ChecksumInterval = 30;

        public const string Hello = "bonjour", Choice = "choix", Ready = "pret", Inputs = "i", Checksum = "h",
            Rematch = "revanche", Quit = "quitte", Ping = "p", Pong = "q";

        public static string Words(params object[] fields)
        {
            var text = new StringBuilder();
            foreach (var field in fields)
            {
                if (text.Length > 0) text.Append(' ');
                text.Append(System.Convert.ToString(field, CultureInfo.InvariantCulture));
            }
            return text.ToString();
        }

        public static string[] Split(string message) => message.Split(' ');

        public static bool TryInt(string text, out int value) =>
            int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value);

        /// <summary>Commandes locales des ticks first, first+1… et avance locale sur l'adversaire.</summary>
        public static string FormatInputs(int first, int advantage, IReadOnlyList<Buttons> inputs, int from, int count)
        {
            var text = new StringBuilder(Inputs).Append(' ').Append(first).Append(' ').Append(advantage);
            for (int i = 0; i < count; i++) text.Append(' ').Append(((int)inputs[from + i]).ToString("x", CultureInfo.InvariantCulture));
            return text.ToString();
        }

        public static bool TryParseInputs(string[] parts, out int first, out int advantage, List<Buttons> into)
        {
            into.Clear();
            advantage = 0;
            if (parts.Length < 4 || parts[0] != Inputs || !TryInt(parts[1], out first) || !TryInt(parts[2], out advantage))
            {
                first = 0;
                return false;
            }
            for (int i = 3; i < parts.Length; i++)
            {
                if (!int.TryParse(parts[i], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out int mask)) return false;
                into.Add((Buttons)mask);
            }
            return true;
        }
    }
}
