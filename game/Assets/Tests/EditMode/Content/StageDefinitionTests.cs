using System.IO;
using NUnit.Framework;
using Patate.Application;
using Patate.Content;

namespace Patate.Tests
{
    public class StageDefinitionTests
    {
        static string StagesRoot => Path.Combine(UnityEngine.Application.dataPath, "Resources", "Stages");

        [Test]
        public void ChaqueDecorEstValideEtSesImagesExistent()
        {
            foreach (var folder in Directory.GetDirectories(StagesRoot))
            {
                var id = Path.GetFileName(folder);
                var report = new ValidationReport();
                var stage = StageDefinition.Read(File.ReadAllText(Path.Combine(folder, "stage.json")), $"Stages/{id}/stage.json", report);
                Assert.IsNotNull(stage, report.ToString());
                Assert.AreEqual(id, stage.Id, "l'identifiant doit être le nom du dossier");
                foreach (var layer in stage.Layers)
                    FileAssert.Exists(Path.Combine(folder, layer.Image));
            }
        }

        [Test]
        public void LeDecorParDefautExiste()
        {
            FileAssert.Exists(Path.Combine(StagesRoot, MatchSetup.Default().Stage, "stage.json"));
        }
    }
}
