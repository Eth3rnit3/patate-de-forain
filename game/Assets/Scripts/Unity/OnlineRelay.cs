using System;
using System.Linq;
using Patate.Application;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#else
using System.Collections.Concurrent;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Connexion au relais en ligne (server/relay) : WebSocket du navigateur
    /// en WebGL (Plugins/WebGL/PatateRelay.jslib), ClientWebSocket de .NET
    /// dans l'éditeur et sur PC.
    /// </summary>
    public static class OnlineRelay
    {
        public static IRelaySocket Open(string relay, string code, bool join)
        {
            var url = $"{relay.TrimEnd('/')}/salon/{Uri.EscapeDataString(code)}{(join ? "?rejoindre" : "")}";
#if UNITY_WEBGL && !UNITY_EDITOR
            return new BrowserRelaySocket(url);
#else
            return new DotNetRelaySocket(url);
#endif
        }
    }

    /// <summary>
    /// Réglages du jeu en ligne. Paramètres de la page (<c>?relais=…&amp;salon=…</c>)
    /// en WebGL, de la ligne de commande (<c>-relais … -salon …</c>) sur PC :
    /// <list type="bullet">
    /// <item><c>relais</c> : adresse du relais (ws:// ou wss://) ;</item>
    /// <item><c>salon</c> : code du salon à rejoindre dès l'ouverture, <c>creer</c> pour l'ouvrir ;</item>
    /// <item><c>robot</c> : le joueur local joue tout seul et se déclare prêt (vérifications).</item>
    /// </list>
    /// </summary>
    public static class OnlineSettings
    {
        /// <summary>
        /// Relais déployé, à renseigner après son déploiement (server/relay/README.md),
        /// ex. « wss://patate-relais.compte.workers.dev ». Vide : relais local.
        /// </summary>
        public const string DeployedRelay = "";
        public const string LocalRelay = "ws://localhost:8787";
        const string RelayPreference = "patate.online.relay";

        /// <summary>Valeur d'un paramètre (« » s'il est présent sans valeur), ou null.</summary>
        public static string Parameter(string name)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var url = UnityEngine.Application.absoluteURL ?? "";
            int start = url.IndexOf('?');
            if (start < 0) return null;
            int end = url.IndexOf('#', start);
            var query = end < 0 ? url.Substring(start + 1) : url.Substring(start + 1, end - start - 1);
            foreach (var pair in query.Split('&'))
            {
                int equals = pair.IndexOf('=');
                var key = Uri.UnescapeDataString(equals < 0 ? pair : pair.Substring(0, equals));
                if (key == name) return equals < 0 ? "" : Uri.UnescapeDataString(pair.Substring(equals + 1).Replace('+', ' '));
            }
            return null;
#else
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-" + name);
            if (index < 0) return null;
            return index + 1 < args.Length && !args[index + 1].StartsWith("-") ? args[index + 1] : "";
#endif
        }

        public static bool Has(string name) => Parameter(name) != null;

        /// <summary>Relais à utiliser : paramètre, sinon dernier saisi, sinon déployé (hors page locale), sinon local.</summary>
        public static string Relay
        {
            get
            {
                var chosen = Parameter("relais");
                if (!string.IsNullOrEmpty(chosen)) return chosen;
                chosen = PlayerPrefs.GetString(RelayPreference, "");
                if (!string.IsNullOrEmpty(chosen)) return chosen;
                return DeployedRelay.Length > 0 && !LocalPage ? DeployedRelay : LocalRelay;
            }
            set
            {
                bool usual = value == (DeployedRelay.Length > 0 && !LocalPage ? DeployedRelay : LocalRelay);
                if (usual) PlayerPrefs.DeleteKey(RelayPreference);
                else PlayerPrefs.SetString(RelayPreference, value);
            }
        }

        /// <summary>Jeu servi depuis cette machine (ou lancé dans l'éditeur) : relais local par défaut.</summary>
        static bool LocalPage
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                var url = UnityEngine.Application.absoluteURL ?? "";
                return new[] { "://localhost", "://127.0.0.1", "://[::1]" }.Any(url.Contains) || url.StartsWith("file:");
#else
                return UnityEngine.Application.isEditor;
#endif
            }
        }

        const string CodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

        /// <summary>Code de salon à donner à l'adversaire : sans 0/O ni 1/I/L, faciles à confondre.</summary>
        public static string NewCode() => new string(Enumerable.Range(0, 5).Select(_ => CodeAlphabet[UnityEngine.Random.Range(0, CodeAlphabet.Length)]).ToArray());

        public static string CleanCode(string text) =>
            new string((text ?? "").ToUpperInvariant().Where(c => (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')).Take(12).ToArray());
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    sealed class BrowserRelaySocket : IRelaySocket
    {
        [DllImport("__Internal")] static extern void PatateRelayOpen(string url);
        [DllImport("__Internal")] static extern int PatateRelayState();
        [DllImport("__Internal")] static extern void PatateRelaySend(string text);
        [DllImport("__Internal")] static extern string PatateRelayReceive();
        [DllImport("__Internal")] static extern string PatateRelayReason();
        [DllImport("__Internal")] static extern void PatateRelayClose();

        public BrowserRelaySocket(string url) => PatateRelayOpen(url);

        public RelaySocketState State => (RelaySocketState)PatateRelayState();
        public string CloseReason => State == RelaySocketState.Closed ? PatateRelayReason() : null;
        public void Send(string text) => PatateRelaySend(text);

        public bool TryReceive(out string text)
        {
            text = PatateRelayReceive();
            return text != null;
        }

        public void Close() => PatateRelayClose();
    }
#else
    /// <summary>
    /// WebSocket de .NET. Réception et envoi tournent en tâches de fond ; le
    /// jeu ne fait que lire et remplir des files.
    /// </summary>
    sealed class DotNetRelaySocket : IRelaySocket
    {
        readonly ClientWebSocket socket = new ClientWebSocket();
        readonly CancellationTokenSource stop = new CancellationTokenSource();
        readonly ConcurrentQueue<string> inbox = new ConcurrentQueue<string>(), outbox = new ConcurrentQueue<string>();
        readonly SemaphoreSlim pending = new SemaphoreSlim(0);
        volatile RelaySocketState state = RelaySocketState.Connecting;
        volatile string reason;

        public DotNetRelaySocket(string url)
        {
            Task.Run(() => Run(url));
        }

        public RelaySocketState State => state;
        public string CloseReason => reason;

        async Task Run(string url)
        {
            try
            {
                await socket.ConnectAsync(new Uri(url), stop.Token);
                state = RelaySocketState.Open;
                _ = Task.Run(SendLoop);
                var buffer = new byte[8192];
                var message = new MemoryStream();
                while (socket.State == WebSocketState.Open)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), stop.Token);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        reason = string.IsNullOrEmpty(result.CloseStatusDescription) ? $"connexion fermée ({(int?)result.CloseStatus})" : result.CloseStatusDescription;
                        break;
                    }
                    message.Write(buffer, 0, result.Count);
                    if (!result.EndOfMessage) continue;
                    inbox.Enqueue(Encoding.UTF8.GetString(message.GetBuffer(), 0, (int)message.Length));
                    message.SetLength(0);
                }
            }
            catch (Exception e)
            {
                if (reason == null) reason = stop.IsCancellationRequested ? "connexion fermée" : $"relais injoignable ({e.GetBaseException().Message})";
            }
            finally
            {
                state = RelaySocketState.Closed;
                if (reason == null) reason = "connexion fermée";
            }
        }

        async Task SendLoop()
        {
            try
            {
                while (!stop.IsCancellationRequested)
                {
                    await pending.WaitAsync(stop.Token);
                    if (!outbox.TryDequeue(out var text)) continue;
                    await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(text)), WebSocketMessageType.Text, true, stop.Token);
                }
            }
            catch (Exception)
            {
                // Fermeture ou coupure : Run le signale.
            }
        }

        public void Send(string text)
        {
            if (state != RelaySocketState.Open) return;
            outbox.Enqueue(text);
            pending.Release();
        }

        public bool TryReceive(out string text) => inbox.TryDequeue(out text);

        public void Close()
        {
            if (state == RelaySocketState.Closed) return;
            reason = reason ?? "connexion fermée";
            state = RelaySocketState.Closed;
            var closing = socket.State == WebSocketState.Open
                ? socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "fin", CancellationToken.None)
                : Task.CompletedTask;
            closing.ContinueWith(_ => stop.Cancel());
        }
    }
#endif
}
