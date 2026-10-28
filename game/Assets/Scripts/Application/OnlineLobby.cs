using Patate.Combat;
using Patate.Content;

namespace Patate.Application
{
    /// <summary>
    /// Salon en ligne, une fois entré : on se salue (même version du
    /// protocole et des règles), chacun choisit son combattant, puis se
    /// déclare prêt. Le combat commence quand chacun a reçu le « prêt » de
    /// l'autre, l'a accepté (même combattant, même empreinte de gameplay) et
    /// a reçu son accord : les deux commencent ensemble ou aucun.
    /// Être prêt est définitif tant que l'adversaire reste.
    /// </summary>
    public sealed class OnlineLobby
    {
        public const string Accept = "accord", Refuse = "refus";

        readonly OnlineLink link;
        readonly FighterCatalog catalog;
        readonly MatchRules rules;
        readonly PlayerSource localSource;
        string remoteStage;
        bool remoteAgreed;

        public string LocalFighter { get; private set; }
        public string Stage { get; }
        /// <summary>Combattant annoncé par l'adversaire, ou null.</summary>
        public string RemoteFighter { get; private set; }
        /// <summary>L'adversaire a salué avec une version compatible.</summary>
        public bool Greeted { get; private set; }
        public bool LocalReady { get; private set; }
        public bool RemoteReady { get; private set; }
        /// <summary>Ce qui empêche de jouer ensemble, ou null.</summary>
        public string Problem { get; private set; }
        /// <summary>Réglages du combat, une fois les deux joueurs d'accord.</summary>
        public MatchSetup Started { get; private set; }
        public OnlineLink Link => link;
        public bool CanBeReady => !LocalReady && Greeted && Problem == null && link.PeerPresent;

        public OnlineLobby(OnlineLink link, FighterCatalog catalog, MatchRules rules, string fighter, string stage, PlayerSource localSource)
        {
            this.link = link;
            this.catalog = catalog;
            this.rules = rules;
            this.localSource = localSource;
            LocalFighter = fighter;
            Stage = stage;
        }

        /// <summary>Traite les messages reçus, jusqu'au début du combat exclu : la suite revient au combat.</summary>
        public void Update()
        {
            while (Started == null && link.TryNext(out var next)) Handle(next);
        }

        public void Choose(string fighter)
        {
            if (LocalReady || catalog.Get(fighter) == null) return;
            LocalFighter = fighter;
            if (link.PeerPresent) link.Send(Announce(OnlineProtocol.Choice));
        }

        public void SetReady()
        {
            if (!CanBeReady) return;
            LocalReady = true;
            link.Send(Announce(OnlineProtocol.Ready));
            TryStart();
        }

        void Handle(LinkEvent next)
        {
            switch (next.Kind)
            {
                case LinkEventKind.Entered:
                    if (link.PeerPresent) Greet();
                    break;
                case LinkEventKind.PeerArrived:
                    Forget();
                    Greet();
                    break;
                case LinkEventKind.PeerLeft:
                    Forget();
                    break;
                case LinkEventKind.Message:
                    Receive(OnlineProtocol.Split(next.Text));
                    break;
            }
        }

        void Greet()
        {
            link.Send(OnlineProtocol.Words(OnlineProtocol.Hello, OnlineProtocol.Version, rules.Id, rules.Version));
            link.Send(Announce(OnlineProtocol.Choice));
        }

        /// <summary>Nouvel adversaire ou départ : tout est à refaire, y compris être prêt.</summary>
        void Forget()
        {
            Greeted = LocalReady = RemoteReady = remoteAgreed = false;
            RemoteFighter = remoteStage = null;
            Problem = null;
        }

        void Receive(string[] parts)
        {
            switch (parts[0])
            {
                case OnlineProtocol.Hello when parts.Length >= 4:
                    if (parts[1] != OnlineProtocol.Version.ToString() || parts[2] != rules.Id || parts[3] != rules.Version.ToString())
                        Problem = $"Versions incompatibles : protocole {parts[1]}, règles {parts[2]} v{parts[3]} chez l'adversaire ; ici protocole {OnlineProtocol.Version}, règles {rules.Id} v{rules.Version}. Mettez le jeu à jour.";
                    else Greeted = true;
                    break;
                case OnlineProtocol.Choice when parts.Length >= 4:
                    RemoteFighter = parts[1];
                    break;
                case OnlineProtocol.Ready when parts.Length >= 4:
                    RemoteFighter = parts[1];
                    remoteStage = parts[3];
                    var problem = Unplayable(parts[1], parts[2]);
                    if (problem != null)
                    {
                        Problem = problem;
                        link.Send(OnlineProtocol.Words(Refuse, parts[1]));
                        break;
                    }
                    RemoteReady = true;
                    link.Send(Accept);
                    TryStart();
                    break;
                case Accept:
                    remoteAgreed = true;
                    TryStart();
                    break;
                case Refuse:
                    Problem = $"L'adversaire n'a pas ce combattant ({LocalFighter}) dans la même version. Mettez le jeu à jour.";
                    break;
            }
        }

        string Unplayable(string fighter, string hash)
        {
            var package = catalog.Get(fighter);
            if (package == null) return $"Combattant de l'adversaire inconnu ici : {fighter}. Mettez le jeu à jour.";
            if (package.GameplayRef.Hash != hash) return $"{package.Name} n'a pas le même gameplay chez l'adversaire. Mettez le jeu à jour.";
            return null;
        }

        string Announce(string word)
        {
            var package = catalog.Get(LocalFighter);
            return OnlineProtocol.Words(word, LocalFighter, package?.GameplayRef.Hash ?? "-", Stage);
        }

        void TryStart()
        {
            if (!LocalReady || !RemoteReady || !remoteAgreed || Problem != null) return;
            int local = link.Slot, remote = 1 - link.Slot;
            var setup = new MatchSetup { Mode = MatchMode.Online };
            setup.Fighters[local] = LocalFighter;
            setup.Fighters[remote] = RemoteFighter;
            // Le décor est celui de la place 0, celle de qui a ouvert le salon.
            setup.Stage = local == 0 ? Stage : remoteStage;
            setup.Players[local] = localSource;
            setup.Players[remote] = PlayerSource.Remote();
            Started = setup;
        }
    }
}
