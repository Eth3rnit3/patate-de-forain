using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Patate.Combat;

namespace Patate.Content
{
    /// <summary>Accès aux fichiers texte d'un paquet, quelle que soit leur origine (Resources, disque, réseau).</summary>
    public interface IPackageFiles
    {
        /// <summary>Contenu du fichier, ou null s'il n'existe pas. <paramref name="relativePath"/> est déjà validé.</summary>
        string ReadText(string relativePath);
    }

    /// <summary>
    /// Frontière de validation des paquets de combattants : schéma, bornes,
    /// références internes, chemins autorisés, compatibilité de format. Le jeu
    /// n'accepte un combattant qu'à travers elle — aujourd'hui pour les
    /// paquets livrés, demain pour les paquets importés.
    /// </summary>
    public static class FighterPackageReader
    {
        public static class Limits
        {
            public const int Moves = 64, Segments = 32, Boxes = 8, Commands = 4, MotionLength = 8, Projectiles = 8;
            public const int Duration = 600, Coordinate = 4096, Size = 4096, Health = 100_000, Speed = 200, Damage = 10_000;
            public const int Animations = 128, Frames = 64, Sprites = 1024, AtlasPages = 8, PageSize = 4096, Actions = 4, Sounds = 64;
        }

        public static readonly Regex IdPattern = new Regex("^[a-z0-9][a-z0-9_-]{0,47}$");
        static readonly Regex VersionPattern = new Regex(@"^\d{1,4}\.\d{1,4}\.\d{1,4}$");
        /// <summary>Chemin relatif simple dans le paquet : pas de dossier parent, pas d'URL, extensions permises.</summary>
        static readonly Regex FilePattern = new Regex(@"^(?!.*\.\.)[a-zA-Z0-9_-]+(/[a-zA-Z0-9_-]+)*\.(json|png|ogg|wav)$");

        static readonly Dictionary<string, Buttons> ButtonNames = new Dictionary<string, Buttons>
        {
            ["lightPunch"] = Buttons.LightPunch, ["mediumPunch"] = Buttons.MediumPunch, ["heavyPunch"] = Buttons.HeavyPunch,
            ["lightKick"] = Buttons.LightKick, ["mediumKick"] = Buttons.MediumKick, ["heavyKick"] = Buttons.HeavyKick,
            ["special"] = Buttons.Special,
        };
        static readonly Dictionary<string, Phase> PhaseNames = new Dictionary<string, Phase>
        {
            ["startup"] = Phase.Startup, ["active"] = Phase.Active, ["recovery"] = Phase.Recovery,
        };
        static readonly Dictionary<string, HitLevel> LevelNames = new Dictionary<string, HitLevel>
        {
            ["high"] = HitLevel.High, ["mid"] = HitLevel.Mid, ["low"] = HitLevel.Low,
        };
        static readonly Dictionary<string, MoveStance> StanceNames = new Dictionary<string, MoveStance>
        {
            ["stand"] = MoveStance.Stand, ["crouch"] = MoveStance.Crouch, ["air"] = MoveStance.Air,
        };
        static readonly Dictionary<string, Playback> PlaybackNames = new Dictionary<string, Playback>
        {
            ["once"] = Playback.Once, ["loop"] = Playback.Loop, ["reverseLoop"] = Playback.ReverseLoop,
            ["stretch"] = Playback.Stretch, ["hold"] = Playback.Hold,
        };
        static readonly Dictionary<string, FighterVisualState> StateNames = new Dictionary<string, FighterVisualState>
        {
            ["idle"] = FighterVisualState.Idle, ["walkForward"] = FighterVisualState.WalkForward, ["walkBack"] = FighterVisualState.WalkBack,
            ["guard"] = FighterVisualState.Guard, ["hitstun"] = FighterVisualState.Hitstun, ["blockstun"] = FighterVisualState.Blockstun,
            ["ko"] = FighterVisualState.KO,
            ["crouch"] = FighterVisualState.Crouch, ["crouchGuard"] = FighterVisualState.CrouchGuard,
            ["crouchHitstun"] = FighterVisualState.CrouchHitstun, ["crouchBlockstun"] = FighterVisualState.CrouchBlockstun,
            ["preJump"] = FighterVisualState.PreJump, ["jump"] = FighterVisualState.Jump, ["land"] = FighterVisualState.Land,
            ["airHitstun"] = FighterVisualState.AirHitstun,
        };

        public sealed class Result
        {
            public FighterPackage Package;
            public ValidationReport Report;
            public bool Ok => Package != null;
        }

        /// <param name="expectedId">Nom du dossier du paquet : il doit correspondre à l'identifiant déclaré.</param>
        public static Result Read(IPackageFiles files, string expectedId)
        {
            var report = new ValidationReport();
            var manifestNode = JsonNode.Parse(files.ReadText(FighterPackage.ManifestFile), FighterPackage.ManifestFile, report);
            var manifest = manifestNode == null ? null : ReadManifest(manifestNode, expectedId);
            if (manifest == null || report.HasErrors) return new Result { Report = report };

            var gameplayNode = JsonNode.Parse(files.ReadText(FighterPackage.GameplayFile), FighterPackage.GameplayFile, report);
            var gameplay = gameplayNode == null ? null : ReadGameplay(gameplayNode);

            var presentationNode = JsonNode.Parse(files.ReadText(FighterPackage.PresentationFile), FighterPackage.PresentationFile, report);
            var presentation = presentationNode == null ? null : ReadPresentation(presentationNode);

            JsonNode atlasNode = null;
            AtlasDefinition atlas = null;
            if (presentation?.AtlasFile != null)
            {
                atlasNode = JsonNode.Parse(files.ReadText(presentation.AtlasFile), presentation.AtlasFile, report);
                if (atlasNode != null) atlas = ReadAtlas(atlasNode);
            }

            if (gameplay != null) CheckGameplay(gameplay, report);
            if (gameplay != null && presentation != null && atlas != null) CheckPresentation(gameplay, presentation, atlas, report);
            if (report.HasErrors || gameplay == null || presentation == null || atlas == null) return new Result { Report = report };

            string gameplayHash = ContentHash.Of(gameplayNode.Raw);
            string presentationHash = ContentHash.Of(presentationNode.Raw, atlasNode.Raw);
            var warnings = report.Issues.Where(i => i.Severity == Severity.Warning).ToList();
            return new Result
            {
                Package = new FighterPackage(manifest, gameplay, presentation, atlas, gameplayHash, presentationHash, warnings),
                Report = report,
            };
        }

        static PackageManifest ReadManifest(JsonNode node, string expectedId)
        {
            node.AllowOnly("format", "formatVersion", "id", "name", "version", "description", "credits");
            var format = node.String("format");
            if (format != null && format != FighterPackage.Format) node.Fail("format", $"doit valoir « {FighterPackage.Format} »");
            int formatVersion = node.Int("formatVersion", 1, 1000);
            if (formatVersion != FighterPackage.FormatVersion)
                node.Fail("formatVersion", $"version de format {formatVersion} non prise en charge par ce moteur (attendue : {FighterPackage.FormatVersion})");
            var manifest = new PackageManifest
            {
                Id = node.String("id", IdPattern, "minuscules, chiffres, - ou _"),
                Name = node.String("name", maxLength: 40),
                Version = node.String("version", VersionPattern, "majeure.mineure.correctif, ex. 0.1.0"),
                Description = node.String("description", maxLength: 500, required: false),
                Credits = node.Array("credits", 16, required: false).Select(c => c.Raw.Type == Newtonsoft.Json.Linq.JTokenType.String ? (string)c.Raw : null).Where(c => c != null).ToArray(),
            };
            if (manifest.Id != null && expectedId != null && manifest.Id != expectedId)
                node.Fail("id", $"« {manifest.Id} » ne correspond pas au dossier du paquet « {expectedId} » : renommer l'un ou l'autre");
            return manifest;
        }

        // ---------------------------------------------------------------- gameplay

        static FighterGameplay ReadGameplay(JsonNode node)
        {
            node.AllowOnly("stats", "body", "moves", "projectiles");
            var statsNode = node.Object("stats");
            var stats = statsNode == null ? null : ReadStats(statsNode);
            var bodyNode = node.Object("body");
            BodyDefinition body = null;
            if (bodyNode != null)
            {
                bodyNode.AllowOnly("pushbox", "hurtboxes", "crouch", "air");
                var stand = ReadShape(bodyNode);
                var crouch = bodyNode.Object("crouch", required: false);
                var air = bodyNode.Object("air", required: false);
                if (crouch != null) crouch.AllowOnly("pushbox", "hurtboxes");
                if (air != null) air.AllowOnly("pushbox", "hurtboxes");
                body = new BodyDefinition(stand.Pushbox, stand.Hurtboxes, crouch == null ? null : ReadShape(crouch), air == null ? null : ReadShape(air));
            }
            var moves = node.Array("moves", Limits.Moves, minCount: 1).Select(ReadMove).Where(m => m != null).ToList();
            var projectiles = node.Array("projectiles", Limits.Projectiles, required: false).Select(ReadProjectile).Where(p => p != null).ToList();
            return stats == null || body == null ? null : new FighterGameplay(stats, body, moves, projectiles);
        }

        /// <summary>Forme du corps dans une posture (debout, « crouch » ou « air ») : pushbox et hurtboxes.</summary>
        static BodyShape ReadShape(JsonNode node)
        {
            var pushbox = node.Object("pushbox");
            return new BodyShape(pushbox == null ? default : ReadBox(pushbox), ReadBoxes(node, "hurtboxes", minCount: 1));
        }

        static FighterStats ReadStats(JsonNode node)
        {
            node.AllowOnly("health", "walkForwardSpeed", "walkBackSpeed", "jump");
            var jumpNode = node.Object("jump", required: false);
            JumpStats jump = null;
            if (jumpNode != null)
            {
                var d = JumpStats.Default;
                jumpNode.AllowOnly("velocity", "gravity", "forwardSpeed", "backSpeed", "preJumpTicks", "landingTicks");
                jump = new JumpStats(jumpNode.Int("velocity", 1, Limits.Speed, d.Velocity), jumpNode.Int("gravity", 1, Limits.Speed, d.Gravity),
                    jumpNode.Int("forwardSpeed", 0, Limits.Speed, d.ForwardSpeed), jumpNode.Int("backSpeed", 0, Limits.Speed, d.BackSpeed),
                    jumpNode.Int("preJumpTicks", 1, 30, d.PreJumpTicks), jumpNode.Int("landingTicks", 0, 30, d.LandingTicks));
            }
            return new FighterStats(node.Int("health", 1, Limits.Health), node.Int("walkForwardSpeed", 0, Limits.Speed), node.Int("walkBackSpeed", 0, Limits.Speed), jump);
        }

        static Box ReadBox(JsonNode node)
        {
            node.AllowOnly("x", "y", "w", "h");
            return new Box(node.Int("x", -Limits.Coordinate, Limits.Coordinate), node.Int("y", -Limits.Coordinate, Limits.Coordinate),
                node.Int("w", 1, Limits.Size), node.Int("h", 1, Limits.Size));
        }

        static List<Box> ReadBoxes(JsonNode node, string key, bool required = true, int minCount = 0) =>
            node.Array(key, Limits.Boxes, required, minCount).Where(b => b.IsObject("rectangle attendu : { x, y, w, h }")).Select(ReadBox).ToList();

        static MoveDefinition ReadMove(JsonNode node)
        {
            if (!node.IsObject("coup attendu (objet)")) return null;
            node.AllowOnly("id", "commands", "priority", "segments", "hit", "stance");
            var id = node.String("id", IdPattern, "minuscules, chiffres, - ou _");
            var commands = node.Array("commands", Limits.Commands, minCount: 1).Select(ReadCommand).Where(c => c != null).ToList();
            var segments = node.Array("segments", Limits.Segments, minCount: 1).Select(ReadSegment).Where(s => s != null).ToList();
            var hitNode = node.Object("hit", required: false);
            return new MoveDefinition(id, commands, node.Int("priority", -100, 100, 0), segments, hitNode == null ? null : ReadHit(hitNode),
                node.Enum("stance", StanceNames, MoveStance.Stand, required: false));
        }

        static CommandDefinition ReadCommand(JsonNode node)
        {
            if (!node.IsObject("commande attendue (objet)")) return null;
            node.AllowOnly("button", "direction", "motion", "window");
            var button = node.Enum("button", ButtonNames, Buttons.None);
            int direction = node.Int("direction", 1, 9, 0);
            var motion = node.Array("motion", Limits.MotionLength, required: false).Select(d => d.AsInt(1, 9)).ToArray();
            int window = node.Int("window", 1, 60, motion.Length > 0 ? 15 : 0);
            if (motion.Length > 0 && direction != 0) node.Fail("direction", "incompatible avec « motion » : la dernière direction de la séquence fait office de direction");
            return new CommandDefinition(button, direction, motion, window);
        }

        static SegmentDefinition ReadSegment(JsonNode node)
        {
            if (!node.IsObject("segment attendu (objet)")) return null;
            node.AllowOnly("phase", "duration", "hitboxes", "hurtboxes", "advance", "onEnter");
            var phase = node.Enum("phase", PhaseNames, Phase.Startup);
            var hitboxes = ReadBoxes(node, "hitboxes", required: false);
            var hurtboxes = node.Has("hurtboxes") ? ReadBoxes(node, "hurtboxes") : null;
            var actions = node.Array("onEnter", Limits.Actions, required: false).Select(ReadAction).Where(a => a != null).ToList();
            return new SegmentDefinition(phase, node.Int("duration", 1, Limits.Duration), hitboxes, hurtboxes,
                node.Int("advance", -Limits.Speed, Limits.Speed, 0), actions);
        }

        /// <summary>Le vocabulaire des actions est celui du moteur (SegmentActions) : rien d'autre n'est accepté.</summary>
        static SegmentAction ReadAction(JsonNode node)
        {
            if (!node.IsObject("action attendue (objet)")) return null;
            var type = node.String("type");
            switch (type)
            {
                case SpawnProjectileAction.TypeName:
                    node.AllowOnly("type", "projectile", "offset");
                    var offset = node.Object("offset");
                    offset?.AllowOnly("x", "y");
                    return new SpawnProjectileAction(node.String("projectile", IdPattern, "identifiant de projectile"),
                        offset == null ? default : new PointI(offset.Int("x", -Limits.Coordinate, Limits.Coordinate), offset.Int("y", -Limits.Coordinate, Limits.Coordinate)));
                case null:
                    return null;
                default:
                    node.Fail("type", $"action « {type} » inconnue du moteur (actions disponibles : {string.Join(", ", SegmentActions.KnownTypes)})");
                    return null;
            }
        }

        static HitProperties ReadHit(JsonNode node)
        {
            node.AllowOnly("damage", "chipDamage", "hitstun", "blockstun", "hitstop", "pushback", "level", "blockable");
            var pushback = node.Object("pushback");
            pushback?.AllowOnly("hit", "block");
            return new HitProperties(
                node.Int("damage", 0, Limits.Damage), node.Int("chipDamage", 0, Limits.Damage, 0),
                node.Int("hitstun", 0, Limits.Duration), node.Int("blockstun", 0, Limits.Duration), node.Int("hitstop", 0, 60),
                pushback?.Int("hit", 0, Limits.Coordinate) ?? 0, pushback?.Int("block", 0, Limits.Coordinate) ?? 0,
                node.Enum("level", LevelNames, HitLevel.Mid, required: false), node.Bool("blockable", true));
        }

        static ProjectileDefinition ReadProjectile(JsonNode node)
        {
            if (!node.IsObject("projectile attendu (objet)")) return null;
            node.AllowOnly("id", "speed", "lifetime", "hitbox", "maxActive", "hit");
            var hitbox = node.Object("hitbox");
            var hit = node.Object("hit");
            return new ProjectileDefinition(node.String("id", IdPattern, "minuscules, chiffres, - ou _"), node.Int("speed", 1, Limits.Speed),
                node.Int("lifetime", 1, Limits.Duration), hitbox == null ? default : ReadBox(hitbox), node.Int("maxActive", 1, 3, 1),
                hit == null ? null : ReadHit(hit));
        }

        /// <summary>Cohérence interne du gameplay, au-delà du schéma.</summary>
        static void CheckGameplay(FighterGameplay gameplay, ValidationReport report)
        {
            const string file = FighterPackage.GameplayFile;
            foreach (var duplicate in gameplay.Moves.GroupBy(m => m.Id).Where(g => g.Key != null && g.Count() > 1))
                report.Error(file, "moves", $"identifiant de coup « {duplicate.Key} » déclaré {duplicate.Count()} fois");
            foreach (var duplicate in gameplay.Projectiles.GroupBy(p => p.Id).Where(g => g.Key != null && g.Count() > 1))
                report.Error(file, "projectiles", $"identifiant de projectile « {duplicate.Key} » déclaré {duplicate.Count()} fois");

            for (int m = 0; m < gameplay.Moves.Count; m++)
            {
                var move = gameplay.Moves[m];
                string path = $"moves[{m}] ({move.Id})";
                bool hasHitboxes = false;
                for (int s = 0; s < move.Segments.Count; s++)
                {
                    var segment = move.Segments[s];
                    if (segment.Hitboxes.Count > 0 && segment.Phase != Phase.Active)
                        report.Error(file, $"{path}.segments[{s}]", "des hitboxes ne sont permises qu'en phase « active »");
                    hasHitboxes |= segment.Hitboxes.Count > 0 && segment.Phase == Phase.Active;
                    foreach (var action in segment.OnEnter)
                        if (action is SpawnProjectileAction spawn && spawn.Projectile != null && gameplay.Projectiles.All(p => p.Id != spawn.Projectile))
                            report.Error(file, $"{path}.segments[{s}].onEnter", $"projectile « {spawn.Projectile} » non déclaré dans « projectiles »");
                }
                if (hasHitboxes && move.Hit == null) report.Error(file, path, "le coup a des hitboxes mais pas de propriétés « hit » (dégâts, recul…)");
                if (!hasHitboxes && move.Hit != null) report.Warning(file, path, "propriétés « hit » sans aucune hitbox active : elles ne serviront jamais");
            }
            for (int p = 0; p < gameplay.Projectiles.Count; p++)
                if (gameplay.Projectiles[p].Hit == null) report.Error(file, $"projectiles[{p}]", "propriétés « hit » manquantes");
        }

        // ------------------------------------------------------------ présentation

        static PresentationDefinition ReadPresentation(JsonNode node)
        {
            node.AllowOnly("atlas", "portrait", "unitsPerPixel", "animations", "states", "moves", "projectiles", "sounds");
            var presentation = new PresentationDefinition
            {
                AtlasFile = ReadFile(node, "atlas", ".json"),
                PortraitFile = node.Has("portrait") ? ReadFile(node, "portrait", ".png") : null,
                UnitsPerPixel = node.Int("unitsPerPixel", 1, 8, 1),
            };
            foreach (var entry in node.Entries("animations", Limits.Animations))
            {
                if (!IdPattern.IsMatch(entry.Key)) entry.Value.Fail(null, "nom d'animation invalide (minuscules, chiffres, - ou _)");
                entry.Value.AllowOnly("frames");
                var animation = new AnimationDefinition { Name = entry.Key };
                foreach (var frame in entry.Value.Array("frames", Limits.Frames, minCount: 1))
                {
                    frame.AllowOnly("sprite", "duration");
                    animation.Frames.Add(new AnimationFrame { Sprite = frame.String("sprite", maxLength: 80), Duration = frame.Int("duration", 1, Limits.Duration) });
                }
                presentation.Animations[entry.Key] = animation;
            }
            foreach (var entry in node.Entries("states", StateNames.Count))
            {
                if (!StateNames.TryGetValue(entry.Key, out var state))
                {
                    entry.Value.Fail(null, $"état inconnu (états : {string.Join(", ", StateNames.Keys)})");
                    continue;
                }
                presentation.States[state] = ReadBinding(entry.Value);
            }
            foreach (var entry in node.Entries("moves", Limits.Moves)) presentation.Moves[entry.Key] = ReadBinding(entry.Value);
            foreach (var entry in node.Entries("projectiles", Limits.Projectiles, required: false))
            {
                entry.Value.AllowOnly("animation", "impact");
                presentation.Projectiles[entry.Key] = new ProjectilePresentation
                {
                    Animation = entry.Value.String("animation"),
                    Impact = entry.Value.String("impact", required: false),
                };
            }
            foreach (var entry in node.Entries("sounds", Limits.Sounds, required: false))
            {
                presentation.Sounds[entry.Key] = entry.Value.Raw.Type == Newtonsoft.Json.Linq.JTokenType.String && FilePattern.IsMatch((string)entry.Value.Raw)
                    ? (string)entry.Value.Raw
                    : null;
                if (presentation.Sounds[entry.Key] == null) entry.Value.Fail(null, "chemin de son invalide (fichier .ogg ou .wav du paquet)");
            }
            return presentation;
        }

        static AnimationBinding ReadBinding(JsonNode node)
        {
            node.AllowOnly("animation", "playback", "frame");
            return new AnimationBinding
            {
                Animation = node.String("animation"),
                Playback = node.Enum("playback", PlaybackNames, Playback.Once, required: false),
                Frame = node.Int("frame", 0, Limits.Frames - 1, 0),
            };
        }

        static string ReadFile(JsonNode node, string key, string extension)
        {
            var path = node.String(key, FilePattern, "chemin relatif dans le paquet, sans « .. »");
            if (path == null || !FilePattern.IsMatch(path)) return null; // jamais lu s'il est refusé
            if (!path.EndsWith(extension))
            {
                node.Fail(key, $"fichier {extension} attendu");
                return null;
            }
            return path;
        }

        static AtlasDefinition ReadAtlas(JsonNode node)
        {
            node.AllowOnly("formatVersion", "generator", "pages", "sprites");
            if (node.Int("formatVersion", 1, 1000) != 1) node.Fail("formatVersion", "version d'atlas non prise en charge (attendue : 1)");
            var atlas = new AtlasDefinition();
            foreach (var page in node.Array("pages", Limits.AtlasPages, minCount: 1))
            {
                page.AllowOnly("file", "width", "height", "sha256");
                atlas.Pages.Add(new AtlasPage
                {
                    File = ReadFile(page, "file", ".png"),
                    Width = page.Int("width", 4, Limits.PageSize),
                    Height = page.Int("height", 4, Limits.PageSize),
                    Sha256 = page.String("sha256", new Regex("^[0-9a-f]{64}$"), "64 caractères hexadécimaux"),
                });
            }
            foreach (var entry in node.Entries("sprites", Limits.Sprites))
            {
                var s = entry.Value;
                s.AllowOnly("page", "x", "y", "w", "h", "pivotX", "pivotY");
                var sprite = new AtlasSprite
                {
                    Page = s.Int("page", 0, Limits.AtlasPages - 1),
                    X = s.Int("x", 0, Limits.PageSize), Y = s.Int("y", 0, Limits.PageSize),
                    W = s.Int("w", 1, Limits.PageSize), H = s.Int("h", 1, Limits.PageSize),
                    PivotX = s.Int("pivotX", -Limits.PageSize, 2 * Limits.PageSize), PivotY = s.Int("pivotY", -Limits.PageSize, 2 * Limits.PageSize),
                };
                if (sprite.Page < atlas.Pages.Count)
                {
                    var page = atlas.Pages[sprite.Page];
                    if (sprite.X + sprite.W > page.Width || sprite.Y + sprite.H > page.Height) s.Fail(null, "le rectangle dépasse de sa page d'atlas");
                }
                else s.Fail("page", $"page {sprite.Page} inexistante");
                atlas.Sprites[entry.Key] = sprite;
            }
            return atlas;
        }

        /// <summary>Références croisées gameplay ↔ présentation ↔ atlas, et synchronisation des durées.</summary>
        static void CheckPresentation(FighterGameplay gameplay, PresentationDefinition presentation, AtlasDefinition atlas, ValidationReport report)
        {
            const string file = FighterPackage.PresentationFile;
            foreach (var animation in presentation.Animations.Values)
                for (int f = 0; f < animation.Frames.Count; f++)
                    if (animation.Frames[f].Sprite != null && !atlas.Sprites.ContainsKey(animation.Frames[f].Sprite))
                        report.Error(file, $"animations.{animation.Name}.frames[{f}]", $"sprite « {animation.Frames[f].Sprite} » absent de l'atlas");

            foreach (var state in StateNames)
            {
                if (!presentation.States.TryGetValue(state.Value, out var binding))
                {
                    if (!FramePicker.Fallback.ContainsKey(state.Value)) report.Error(file, $"states.{state.Key}", "animation obligatoire manquante");
                }
                else CheckBinding(binding, $"states.{state.Key}", presentation, report);
            }

            foreach (var move in gameplay.Moves)
            {
                if (move.Id == null) continue;
                if (!presentation.Moves.TryGetValue(move.Id, out var binding))
                {
                    report.Error(file, $"moves.{move.Id}", "le coup n'a pas d'animation");
                    continue;
                }
                if (!CheckBinding(binding, $"moves.{move.Id}", presentation, report)) continue;
                if (binding.Playback != Playback.Once) report.Error(file, $"moves.{move.Id}.playback", "un coup se lit toujours « once » (synchronisé sur la simulation)");
                int moveDuration = move.Segments.Sum(s => s.Duration);
                int animationDuration = presentation.Animations[binding.Animation].TotalDuration;
                if (moveDuration != animationDuration)
                    report.Error(file, $"moves.{move.Id}", $"l'animation « {binding.Animation} » dure {animationDuration} ticks mais le coup en dure {moveDuration} dans {FighterPackage.GameplayFile} : les poses ne correspondraient plus aux phases actives");
            }
            foreach (var extra in presentation.Moves.Keys.Where(id => gameplay.Moves.All(m => m.Id != id)))
                report.Warning(file, $"moves.{extra}", "animation associée à un coup qui n'existe pas dans le gameplay");

            foreach (var projectile in gameplay.Projectiles)
            {
                if (projectile.Id == null) continue;
                if (!presentation.Projectiles.TryGetValue(projectile.Id, out var visual)) report.Error(file, $"projectiles.{projectile.Id}", "le projectile n'a pas d'animation");
                else
                {
                    if (visual.Animation != null && !presentation.Animations.ContainsKey(visual.Animation))
                        report.Error(file, $"projectiles.{projectile.Id}.animation", $"animation « {visual.Animation} » inconnue");
                    if (visual.Impact != null && !presentation.Animations.ContainsKey(visual.Impact))
                        report.Error(file, $"projectiles.{projectile.Id}.impact", $"animation « {visual.Impact} » inconnue");
                }
            }
        }

        static bool CheckBinding(AnimationBinding binding, string path, PresentationDefinition presentation, ValidationReport report)
        {
            if (binding.Animation == null) return false;
            if (!presentation.Animations.TryGetValue(binding.Animation, out var animation))
            {
                report.Error(FighterPackage.PresentationFile, path, $"animation « {binding.Animation} » inconnue");
                return false;
            }
            if (binding.Playback == Playback.Hold && binding.Frame >= animation.Frames.Count)
                report.Error(FighterPackage.PresentationFile, path + ".frame", $"l'animation n'a que {animation.Frames.Count} images");
            return true;
        }
    }
}
