using System.Collections;
using System.Diagnostics;
using NUnit.Framework;
using Patate.Application;
using Patate.UnityAdapters;
using UnityEngine.TestTools;

namespace Patate.Tests
{
    /// <summary>
    /// Le transport de l'éditeur et du PC (ClientWebSocket) contre le vrai
    /// relais local. Explicite : il faut d'abord lancer le relais
    /// (cd server/relay &amp;&amp; npm start), puis demander ce test par son nom.
    /// </summary>
    [Explicit("relais local requis : cd server/relay && npm start")]
    public class OnlineRelayTests
    {
        static IEnumerator Until(System.Func<bool> condition, string what)
        {
            var clock = Stopwatch.StartNew();
            while (!condition())
            {
                if (clock.ElapsedMilliseconds > 5000) Assert.Fail($"délai dépassé : {what}");
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator DeuxJoueursSeParlentParLeRelaisLocal()
        {
            var code = "UNITY" + UnityEngine.Random.Range(1000, 9999);
            var host = new OnlineLink(OnlineRelay.Open(OnlineSettings.LocalRelay, code, join: false), code);
            LinkEvent next = default;
            yield return Until(() => host.TryNext(out next), "entrée de l'hôte");
            Assert.AreEqual(LinkEventKind.Entered, next.Kind);
            Assert.AreEqual(0, host.Slot);

            var guest = new OnlineLink(OnlineRelay.Open(OnlineSettings.LocalRelay, code, join: true), code);
            yield return Until(() => guest.TryNext(out next), "entrée de l'invité");
            Assert.AreEqual(1, guest.Slot);
            Assert.IsTrue(guest.PeerPresent);
            yield return Until(() => host.TryNext(out next), "arrivée de l'invité");
            Assert.AreEqual(LinkEventKind.PeerArrived, next.Kind);

            guest.Send("i 3 0 12 400");
            yield return Until(() => host.TryNext(out next), "message de l'invité");
            Assert.AreEqual("i 3 0 12 400", next.Text);

            guest.Close();
            yield return Until(() => host.TryNext(out next), "départ de l'invité");
            Assert.AreEqual(LinkEventKind.PeerLeft, next.Kind);
            host.Close();

            var lost = OnlineRelay.Open(OnlineSettings.LocalRelay, code + "X", join: true);
            yield return Until(() => lost.State == RelaySocketState.Closed, "refus du salon inconnu");
            Assert.AreEqual("salon introuvable", lost.CloseReason);
        }
    }
}
