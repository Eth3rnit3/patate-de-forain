using NUnit.Framework;
using Patate.Application;

namespace Patate.Tests
{
    public class MatchSetupTests
    {
        [Test]
        public void ParDefautDeuxJoueursSePartagentLeClavier()
        {
            var setup = MatchSetup.Default();
            Assert.AreEqual(MatchMode.Versus, setup.Mode);
            Assert.AreEqual(new[] { PlayerSource.Keyboard(0), PlayerSource.Keyboard(1) }, setup.Players);
        }

        [Test]
        public void UneCopieNePartagePasSesTableaux()
        {
            var setup = MatchSetup.Default();
            var copy = setup.Copy();
            copy.Fighters[0] = "napoleon";
            copy.Players[1] = PlayerSource.Cpu(1);
            Assert.AreEqual("saint-louis", setup.Fighters[0]);
            Assert.AreEqual(PlayerSource.Keyboard(1), setup.Players[1]);
        }
    }
}
