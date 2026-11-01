using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Patate.Combat;
using Patate.Content;

namespace Patate.Tests
{
    /// <summary>Paquet en mémoire, construit à partir du vrai paquet de Saint Louis puis altéré.</summary>
    sealed class MemoryFiles : IPackageFiles
    {
        public readonly Dictionary<string, string> Files = new Dictionary<string, string>();
        public string ReadText(string path) => Files.TryGetValue(path, out var text) ? text : null;
    }

    public class PackageValidationTests
    {
        public static string FightersRoot => Path.Combine(UnityEngine.Application.dataPath, "Resources", "Fighters");

        static MemoryFiles SaintLouisCopy()
        {
            var files = new MemoryFiles();
            foreach (var name in new[] { "fighter.json", "gameplay.json", "presentation.json", "atlas.json" })
                files.Files[name] = File.ReadAllText(Path.Combine(FightersRoot, "saint-louis", name));
            return files;
        }

        /// <summary>Applique une modification au JSON d'un fichier puis valide le paquet.</summary>
        static FighterPackageReader.Result ReadWith(string file, System.Action<JObject> change)
        {
            var files = SaintLouisCopy();
            var json = JObject.Parse(files.Files[file]);
            change(json);
            files.Files[file] = json.ToString();
            return FighterPackageReader.Read(files, "saint-louis");
        }

        static void AssertError(FighterPackageReader.Result result, string file, string pathFragment, string messageFragment)
        {
            Assert.IsFalse(result.Ok, "le paquet aurait dû être refusé");
            var match = result.Report.Issues.FirstOrDefault(i => i.Severity == Severity.Error && i.File == file && i.Path.Contains(pathFragment) && i.Message.Contains(messageFragment));
            Assert.IsNotNull(match, $"erreur attendue sur {file} › {pathFragment} (« {messageFragment} »). Reçu :\n{result.Report}");
        }

        [Test]
        public void LePaquetDeSaintLouisEstValide()
        {
            var catalog = FighterCatalog.Load(new DirectoryFighterSource(FightersRoot));
            Assert.IsEmpty(catalog.Rejected.Keys, string.Join("\n", catalog.Rejected.Select(r => $"{r.Key} :\n{r.Value}")));
            var saintLouis = catalog.Get("saint-louis");
            Assert.IsNotNull(saintLouis);
            Assert.AreEqual("0.1.0", saintLouis.GameplayRef.Version);
            Assert.AreEqual(64, saintLouis.GameplayRef.Hash.Length);
            CollectionAssert.AreEquivalent(new[] { "special_royal_lily", "stand_punch_light", "stand_kick_light", "stand_kick_heavy", "crouch_punch", "crouch_kick", "jump_punch", "jump_kick" }, saintLouis.Gameplay.Moves.Select(m => m.Id));
        }

        [Test]
        public void LEmpreinteNeDependPasDeLaMiseEnForme()
        {
            var compact = SaintLouisCopy();
            compact.Files["gameplay.json"] = JObject.Parse(compact.Files["gameplay.json"]).ToString(Newtonsoft.Json.Formatting.None);
            var a = FighterPackageReader.Read(SaintLouisCopy(), "saint-louis").Package.GameplayRef.Hash;
            var b = FighterPackageReader.Read(compact, "saint-louis").Package.GameplayRef.Hash;
            Assert.AreEqual(a, b);
            var changed = ReadWith("gameplay.json", g => g["stats"]["health"] = 999).Package.GameplayRef.Hash;
            Assert.AreNotEqual(a, changed);
        }

        [Test]
        public void RefuseUnChampObligatoireManquant() =>
            AssertError(ReadWith("gameplay.json", g => ((JObject)g["stats"]).Remove("health")), "gameplay.json", "stats.health", "obligatoire");

        [Test]
        public void RefuseUnNombreNonEntier() =>
            AssertError(ReadWith("gameplay.json", g => g["moves"][1]["segments"][0]["duration"] = 4.5), "gameplay.json", "moves[1].segments[0].duration", "entier");

        [Test]
        public void RefuseUneValeurHorsBornes() =>
            AssertError(ReadWith("gameplay.json", g => g["stats"]["health"] = 5_000_000), "gameplay.json", "stats.health", "entre");

        [Test]
        public void RefuseUnChampInconnuPourAttraperLesFautesDeFrappe() =>
            AssertError(ReadWith("gameplay.json", g => g["moves"][1]["segments"][0]["duraton"] = 3), "gameplay.json", "duraton", "champ inconnu");

        [Test]
        public void RefuseUneHitboxHorsPhaseActive() =>
            AssertError(ReadWith("gameplay.json", g => g["moves"][1]["segments"][0]["hitboxes"] = JArray.Parse("[{\"x\":0,\"y\":0,\"w\":10,\"h\":10}]")),
                "gameplay.json", "segments[0]", "phase « active »");

        [Test]
        public void RefuseUnProjectileNonDeclare() =>
            AssertError(ReadWith("gameplay.json", g => g["moves"][0]["segments"][1]["onEnter"][0]["projectile"] = "boule_de_feu"), "gameplay.json", "onEnter", "non déclaré");

        [Test]
        public void RefuseUneActionInconnueDuMoteur() =>
            AssertError(ReadWith("gameplay.json", g => g["moves"][0]["segments"][1]["onEnter"][0]["type"] = "executeScript"), "gameplay.json", "onEnter[0].type", "inconnue du moteur");

        [Test]
        public void RefuseUneAnnulationVersUnCoupInconnu() =>
            AssertError(ReadWith("gameplay.json", g => g["moves"][1]["cancels"] = JArray.Parse("[\"coup_de_tete\"]")), "gameplay.json", "moves[1] (stand_punch_light).cancels", "inconnu");

        [Test]
        public void RefuseUneAnnulationVersUnCoupSaute() =>
            AssertError(ReadWith("gameplay.json", g => g["moves"][1]["cancels"] = JArray.Parse("[\"jump_kick\"]")), "gameplay.json", "moves[1] (stand_punch_light).cancels", "coup sauté");

        [Test]
        public void RefuseUnCoupQuiSAnnuleEnLuiMeme() =>
            AssertError(ReadWith("gameplay.json", g => g["moves"][1]["cancelsOnHit"] = JArray.Parse("[\"stand_punch_light\"]")), "gameplay.json", "moves[1] (stand_punch_light).cancelsOnHit", "en lui-même");

        [Test]
        public void RefuseUneAnimationDeCoupDesynchroniseeDuGameplay() =>
            AssertError(ReadWith("presentation.json", p => p["animations"]["stand_kick_heavy"]["frames"][0]["duration"] = 7), "presentation.json", "moves.stand_kick_heavy", "dure 43 ticks mais le coup en dure 40");

        [Test]
        public void RefuseUnSpriteAbsentDeLAtlas() =>
            AssertError(ReadWith("presentation.json", p => p["animations"]["idle"]["frames"][0]["sprite"] = "idle_99"), "presentation.json", "animations.idle.frames[0]", "absent de l'atlas");

        [Test]
        public void RefuseUnCheminQuiSortDuPaquet() =>
            AssertError(ReadWith("presentation.json", p => p["atlas"] = "../autre/atlas.json"), "presentation.json", "atlas", "format attendu");

        [Test]
        public void RefuseUneVersionDeFormatFuture() =>
            AssertError(ReadWith("fighter.json", m => m["formatVersion"] = 2), "fighter.json", "formatVersion", "non prise en charge");

        [Test]
        public void RefuseUnIdentifiantDifferentDuDossier() =>
            AssertError(ReadWith("fighter.json", m => m["id"] = "louis-xiv"), "fighter.json", "id", "ne correspond pas au dossier");

        [Test]
        public void RefuseUnEtatSansAnimation() =>
            AssertError(ReadWith("presentation.json", p => ((JObject)p["states"]).Remove("ko")), "presentation.json", "states.ko", "obligatoire");

        [Test]
        public void RefuseUnJsonIllisible()
        {
            var files = SaintLouisCopy();
            files.Files["gameplay.json"] = "{ \"stats\": ";
            AssertError(FighterPackageReader.Read(files, "saint-louis"), "gameplay.json", "", "JSON invalide");
        }

        [Test]
        public void RefuseUneCleEnDouble()
        {
            var files = SaintLouisCopy();
            files.Files["fighter.json"] = files.Files["fighter.json"].Replace("\"name\": \"Saint Louis\",", "\"name\": \"Saint Louis\", \"name\": \"Autre\",");
            AssertError(FighterPackageReader.Read(files, "saint-louis"), "fighter.json", "", "JSON invalide");
        }
    }

    public class PresentationTests
    {
        static FighterPackage SaintLouis() => FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot)).Get("saint-louis");

        [Test]
        public void LaPoseAffichePendantLaPhaseActiveEstLaPoseDeFrappe()
        {
            // Pose 4 du pied rapide = frappe ; pose 5 du pied puissant = frappe (moveset-prototype-v01.md).
            var package = SaintLouis();
            var fighter = package.Compile();
            foreach (var (moveId, strikePose) in new[] { ("stand_kick_light", 4), ("stand_kick_heavy", 5) })
            {
                var move = fighter.Move(moveId);
                foreach (var segment in move.Segments.Where(s => s.Definition.Phase == Phase.Active))
                {
                    for (int tick = segment.Start; tick < segment.End; tick++)
                    {
                        var state = new FighterState { Action = new ActionState { Kind = ActionKind.Move, MoveId = moveId, Tick = tick } };
                        var pick = FramePicker.ForFighter(state, package.Presentation);
                        Assert.AreEqual(strikePose - 1, pick.FrameIndex, $"{moveId} tick {tick}");
                    }
                }
            }
        }

        [Test]
        public void LeProjectileApparaitALEntreeDeLaCinquiemePose()
        {
            var package = SaintLouis();
            var move = package.Compile().Move("special_royal_lily");
            var spawnSegment = move.Segments.First(s => s.Definition.OnEnter.Count > 0);
            var animation = package.Presentation.Animations["special_royal_lily_cast"];
            Assert.AreEqual(4, FramePicker.IndexAt(animation, spawnSegment.Start));
            Assert.AreEqual(3, FramePicker.IndexAt(animation, spawnSegment.Start - 1));
        }

        [Test]
        public void LaReactionEstEtireeSurLaDureeDeLEtourdissement()
        {
            var presentation = SaintLouis().Presentation;
            var binding = presentation.States[FighterVisualState.Hitstun];
            Assert.AreEqual(0, FramePicker.Resolve(binding, presentation, 0, 30).FrameIndex);
            Assert.AreEqual(2, FramePicker.Resolve(binding, presentation, 29, 30).FrameIndex);
        }
    }

    public class StatPolicyTests
    {
        [Test]
        public void LeModeLibreAccepteToutPaquetValide()
        {
            var package = FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot)).Get("saint-louis");
            Assert.IsEmpty(StatPolicy.Free().Check(package));
            Assert.IsEmpty(StatPolicy.Balanced().Check(package));
        }

        [Test]
        public void LeModeEquilibreRefuseDesStatistiquesExcessives()
        {
            var files = new MemoryFiles();
            foreach (var name in new[] { "fighter.json", "gameplay.json", "presentation.json", "atlas.json" })
                files.Files[name] = File.ReadAllText(Path.Combine(PackageValidationTests.FightersRoot, "saint-louis", name));
            var gameplay = JObject.Parse(files.Files["gameplay.json"]);
            gameplay["stats"]["health"] = 50_000;
            files.Files["gameplay.json"] = gameplay.ToString();
            var overpowered = FighterPackageReader.Read(files, "saint-louis").Package;

            Assert.IsEmpty(StatPolicy.Free().Check(overpowered), "valide, donc jouable en mode libre");
            Assert.IsNotEmpty(StatPolicy.Balanced().Check(overpowered), "mais pas en mode équilibré");
        }

        [Test]
        public void UneListeDApprobationNAdmetQueLesVersionsConnues()
        {
            var package = FighterCatalog.Load(new DirectoryFighterSource(PackageValidationTests.FightersRoot)).Get("saint-louis");
            var policy = StatPolicy.Balanced();
            policy.ApprovedGameplayHashes = new HashSet<string> { "0000" };
            Assert.IsNotEmpty(policy.Check(package));
            policy.ApprovedGameplayHashes.Add(package.GameplayRef.Hash);
            Assert.IsEmpty(policy.Check(package));
        }
    }
}
