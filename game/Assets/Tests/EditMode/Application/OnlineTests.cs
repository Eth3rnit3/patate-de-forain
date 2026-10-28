using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Patate.Application;
using Patate.Combat;
using Patate.Content;

namespace Patate.Tests
{
    /// <summary>
    /// Combat en ligne entre deux « machines » simulées dans le même test :
    /// chacune a sa session, son contexte et sa connexion au relais en
    /// mémoire, avec latence. Elles doivent finir la manche sur le même état,
    /// celui qu'on obtient en rejouant hors ligne les commandes échangées.
    /// </summary>
    public class OnlineMatchTests
    {
        static readonly FighterCatalog Catalog = FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot));

        static MatchRules ShortRound()
        {
            var rules = MatchRules.Default();
            rules.IntroTicks = 30;
            rules.RoundTicks = 30 * 60;
            return rules;
        }

        static MatchSession Session(MatchRules rules) => MatchSession.Start(Catalog, "saint-louis", "saint-louis", rules, StatPolicy.Free());

        sealed class Peer
        {
            public readonly OnlineLink Link;
            public readonly OnlineMatch Match;
            readonly List<MatchEvent> events = new List<MatchEvent>();
            public int Hits;

            public Peer(OnlineLink link, MatchRules rules, int delay, int rollback)
            {
                Link = link;
                while (link.TryNext(out _)) { }
                var session = Session(rules);
                Match = new OnlineMatch(session.Context, session.State, link.Slot, link.Send, delay, rollback);
            }

            public void Frame(int ticks)
            {
                while (Link.TryNext(out var next))
                    if (next.Kind == LinkEventKind.Message) Match.Receive(next.Text);
                events.Clear();
                Match.Advance(ticks, OnlineRobot.InputFor(Match.LocalSlot, Match.State.Tick), events);
                Hits += events.Count(e => e.Type == MatchEventType.Hit);
            }
        }

        static (Peer, Peer) Play(MemoryRelay relay, int delay, int rollback, Func<int, int> ticksA = null, Func<int, int> ticksB = null, MatchRules rulesB = null)
        {
            var rules = ShortRound();
            var linkA = relay.Join("TEST");
            var linkB = relay.Join("TEST", mustExist: true);
            relay.Tick();
            var a = new Peer(linkA, rules, delay, rollback);
            var b = new Peer(linkB, rulesB ?? rules, delay, rollback);
            for (int frame = 0; frame < 40000 && !(a.Match.IsOver && b.Match.IsOver); frame++)
            {
                relay.Tick();
                a.Frame(ticksA?.Invoke(frame) ?? 1);
                b.Frame(ticksB?.Invoke(frame) ?? 1);
                if (a.Match.DesyncTick.HasValue || b.Match.DesyncTick.HasValue) break;
            }
            // Dernières empreintes en route.
            for (int frame = 0; frame < 50; frame++)
            {
                relay.Tick();
                a.Frame(0);
                b.Frame(0);
            }
            return (a, b);
        }

        static void AssertSameEnding(Peer a, Peer b)
        {
            TestContext.WriteLine($"fin au tick {a.Match.State.Tick}, vainqueur {a.Match.State.Round.Winner}, {a.Hits} coups, rollbacks {a.Match.Rollbacks}/{b.Match.Rollbacks}, " +
                $"ticks resimulés {a.Match.ResimulatedTicks}/{b.Match.ResimulatedTicks}, ticks cédés {a.Match.SkippedTicks}/{b.Match.SkippedTicks}, empreintes comparées {a.Match.ComparedChecksums}");
            Assert.IsTrue(a.Match.IsOver && b.Match.IsOver, $"la manche doit finir des deux côtés (ticks {a.Match.State.Tick} / {b.Match.State.Tick})");
            Assert.IsNull(a.Match.DesyncTick, "aucune désynchronisation");
            Assert.IsNull(b.Match.DesyncTick, "aucune désynchronisation");
            Assert.AreEqual(a.Match.State.Tick, b.Match.State.Tick);
            Assert.AreEqual(StateCodec.Checksum(a.Match.State), StateCodec.Checksum(b.Match.State));
            Assert.Greater(a.Match.ComparedChecksums, 10, "les empreintes ont été comparées en cours de manche");
            Assert.Greater(a.Hits, 0, "des coups ont été échangés");

            var replay = Session(ShortRound());
            foreach (var (first, second) in a.Match.ConfirmedInputs()) replay.Step(first, second);
            Assert.AreEqual(StateCodec.Checksum(a.Match.State), StateCodec.Checksum(replay.State), "rejouer hors ligne les commandes échangées donne le même état");
        }

        [Test]
        public void SansPredictionLesDeuxJoueursFinissentSurLeMemeEtat()
        {
            var (a, b) = Play(new MemoryRelay(_ => 2), delay: 3, rollback: 0);
            AssertSameEnding(a, b);
            Assert.AreEqual(0, a.Match.Rollbacks + b.Match.Rollbacks, "sans prédiction, jamais de retour en arrière");
        }

        [Test]
        public void AvecRollbackLesPredictionsFaussesSontCorrigees()
        {
            // Latence de 5 à 9 images : au-delà du délai d'entrée, il faut prédire puis corriger.
            var (a, b) = Play(new MemoryRelay(n => 5 + n * 7 % 5), delay: 2, rollback: 8);
            AssertSameEnding(a, b);
            Assert.Greater(a.Match.Rollbacks, 0);
            Assert.Greater(b.Match.Rollbacks, 0);
        }

        [Test]
        public void UnJoueurPlusRapideAttendLAutre()
        {
            // A voudrait simuler deux ticks par image, B un seul : A est freiné par l'attente et la synchronisation.
            var (a, b) = Play(new MemoryRelay(_ => 3), delay: 2, rollback: 8, ticksA: _ => 2);
            AssertSameEnding(a, b);
            Assert.Greater(a.Match.SkippedTicks, 0, "le joueur en avance a cédé des ticks");
        }

        [Test]
        public void UnEcartDEtatEstDetecte()
        {
            // Règles différentes chez B : même commandes, états différents dès le départ.
            var other = ShortRound();
            other.StartOffset += 10;
            var (a, b) = Play(new MemoryRelay(_ => 2), delay: 2, rollback: 8, rulesB: other);
            Assert.AreEqual(OnlineProtocol.ChecksumInterval, a.Match.DesyncTick);
            Assert.AreEqual(OnlineProtocol.ChecksumInterval, b.Match.DesyncTick);
            Assert.IsFalse(a.Match.IsOver);
        }

        [Test]
        public void LesCommandesVoyagentParLotsEnHexadecimal()
        {
            var inputs = new List<Buttons> { Buttons.None, Buttons.Right | Buttons.LightPunch, Buttons.Special };
            var text = OnlineProtocol.FormatInputs(7, -2, inputs, 1, 2);
            Assert.AreEqual("i 7 -2 12 400", text);
            var parsed = new List<Buttons>();
            Assert.IsTrue(OnlineProtocol.TryParseInputs(OnlineProtocol.Split(text), out int first, out int advantage, parsed));
            Assert.AreEqual(7, first);
            Assert.AreEqual(-2, advantage);
            CollectionAssert.AreEqual(inputs.Skip(1), parsed);
            Assert.IsFalse(OnlineProtocol.TryParseInputs(OnlineProtocol.Split("i 7 x 12"), out _, out _, parsed));
        }
    }

    /// <summary>Le salon : saluer, choisir, se déclarer prêt, commencer ensemble ou pas du tout.</summary>
    public class OnlineLobbyTests
    {
        static readonly FighterCatalog Catalog = FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot));

        static OnlineLobby Lobby(OnlineLink link, string stage = "training") =>
            new OnlineLobby(link, Catalog, MatchRules.Default(), "saint-louis", stage, PlayerSource.Keyboard(0));

        static void Run(MemoryRelay relay, params OnlineLobby[] lobbies)
        {
            for (int frame = 0; frame < 20; frame++)
            {
                relay.Tick();
                foreach (var lobby in lobbies) lobby.Update();
            }
        }

        [Test]
        public void DeuxJoueursPretsCommencentAvecDesReglagesEnMiroir()
        {
            var relay = new MemoryRelay(_ => 2);
            var host = Lobby(relay.Join("SALON"), "foire");
            Run(relay, host);
            Assert.IsFalse(host.CanBeReady, "seul dans le salon");

            var guest = Lobby(relay.Join("SALON", mustExist: true), "autre");
            Run(relay, host, guest);
            Assert.IsTrue(host.Greeted && guest.Greeted);
            Assert.AreEqual("saint-louis", host.RemoteFighter);

            host.SetReady();
            Run(relay, host, guest);
            Assert.IsNull(host.Started, "il faut que les deux soient prêts");
            Assert.IsTrue(guest.RemoteReady);

            guest.SetReady();
            Run(relay, host, guest);
            Assert.IsNotNull(host.Started);
            Assert.IsNotNull(guest.Started);
            Assert.AreEqual(0, host.Link.Slot);
            Assert.AreEqual(1, guest.Link.Slot);
            Assert.AreEqual(MatchMode.Online, host.Started.Mode);
            Assert.AreEqual("foire", guest.Started.Stage, "le décor est celui de qui a ouvert le salon");
            Assert.AreEqual(PlayerSource.Keyboard(0), host.Started.Players[0]);
            Assert.AreEqual(PlayerSource.Remote(), host.Started.Players[1]);
            Assert.AreEqual(PlayerSource.Remote(), guest.Started.Players[0]);
            Assert.AreEqual(PlayerSource.Keyboard(0), guest.Started.Players[1]);
        }

        [Test]
        public void LesMessagesDuCombatNeSontPasConsommesParLeSalon()
        {
            var relay = new MemoryRelay();
            var socket = relay.Connect("SALON");
            var host = Lobby(new OnlineLink(socket, "SALON"));
            var peer = relay.Connect("SALON");
            Run(relay, host);
            var hash = Catalog.Get("saint-louis").GameplayRef.Hash;
            peer.Send($"bonjour {OnlineProtocol.Version} {MatchRules.Default().Id} {MatchRules.Default().Version}");
            Run(relay, host);
            host.SetReady();
            peer.Send($"pret saint-louis {hash} training");
            peer.Send("accord");
            peer.Send("i 3 0 2");
            Run(relay, host);
            Assert.IsNotNull(host.Started);
            Assert.IsTrue(host.Link.TryNext(out var next));
            Assert.AreEqual("i 3 0 2", next.Text, "les commandes de l'adversaire restent pour l'écran de combat");
        }

        [Test]
        public void UnCombattantInconnuOuModifieEmpecheDeCommencer()
        {
            var relay = new MemoryRelay();
            var host = Lobby(relay.Join("SALON"));
            var peer = relay.Connect("SALON");
            Run(relay, host);
            peer.Send($"bonjour {OnlineProtocol.Version} {MatchRules.Default().Id} {MatchRules.Default().Version}");
            Run(relay, host);
            host.SetReady();
            peer.Send("pret saint-louis 0000 training");
            peer.Send("accord");
            Run(relay, host);
            Assert.IsNull(host.Started);
            StringAssert.Contains("pas le même gameplay", host.Problem);
            CollectionAssert.Contains(ReceivedBy(peer), "refus saint-louis");
        }

        [Test]
        public void UneVersionDifferenteEstSignalee()
        {
            var relay = new MemoryRelay();
            var host = Lobby(relay.Join("SALON"));
            var peer = relay.Connect("SALON");
            Run(relay, host);
            peer.Send("bonjour 999 patate-core 1");
            Run(relay, host);
            StringAssert.Contains("Versions incompatibles", host.Problem);
            Assert.IsFalse(host.CanBeReady);
        }

        [Test]
        public void SiLAdversairePartLeSalonSeRemetAZero()
        {
            var relay = new MemoryRelay();
            var host = Lobby(relay.Join("SALON"));
            var guest = Lobby(relay.Join("SALON"));
            Run(relay, host, guest);
            host.SetReady();
            Run(relay, host, guest);
            guest.Link.Close();
            Run(relay, host);
            Assert.IsFalse(host.Link.PeerPresent);
            Assert.IsFalse(host.LocalReady, "le nouvel adversaire devra être accueilli de nouveau");
            Assert.IsNull(host.RemoteFighter);
            Assert.AreEqual(RelaySocketState.Open, relay.Connect("SALON").State, "la place libérée se reprend");
            Assert.AreEqual("salon plein", relay.Connect("SALON").CloseReason);
        }

        static List<string> ReceivedBy(MemoryRelay.Socket socket)
        {
            var received = new List<string>();
            while (socket.TryReceive(out var text)) received.Add(text);
            return received;
        }
    }
}
