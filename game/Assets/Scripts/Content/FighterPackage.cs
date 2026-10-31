using System.Collections.Generic;
using Patate.Combat;

namespace Patate.Content
{
    /// <summary>
    /// Paquet de combattant validé : un dossier de données et de ressources,
    /// sans code. Gameplay et présentation sont séparés ; chacun est identifié
    /// par une empreinte de contenu pour qu'un replay ou une partie en ligne
    /// sache exactement quelle version a été jouée.
    /// </summary>
    public sealed class FighterPackage
    {
        public const string Format = "patate-fighter";
        public const int FormatVersion = 1;
        public const string ManifestFile = "fighter.json";
        public const string GameplayFile = "gameplay.json";
        public const string PresentationFile = "presentation.json";

        public readonly PackageManifest Manifest;
        public readonly FighterGameplay Gameplay;
        public readonly PresentationDefinition Presentation;
        public readonly AtlasDefinition Atlas;
        /// <summary>Identité exacte du gameplay (sert à la simulation et aux replays).</summary>
        public readonly DefinitionRef GameplayRef;
        /// <summary>Empreinte de la présentation (présentation + atlas, qui contient l'empreinte de chaque image).</summary>
        public readonly string PresentationHash;
        public readonly IReadOnlyList<ValidationIssue> Warnings;

        public FighterPackage(PackageManifest manifest, FighterGameplay gameplay, PresentationDefinition presentation, AtlasDefinition atlas,
            string gameplayHash, string presentationHash, IReadOnlyList<ValidationIssue> warnings)
        {
            Manifest = manifest;
            Gameplay = gameplay;
            Presentation = presentation;
            Atlas = atlas;
            GameplayRef = new DefinitionRef(manifest.Id, manifest.Version, gameplayHash);
            PresentationHash = presentationHash;
            Warnings = warnings;
        }

        public string Id => Manifest.Id;
        public string Name => Manifest.Name;
        public CompiledFighter Compile() => new CompiledFighter(GameplayRef, Gameplay);
    }

    public sealed class PackageManifest
    {
        public string Id, Name, Version, Description;
        public string[] Credits;
    }

    /// <summary>Données d'affichage : animations, correspondance états → animations, effets, sons.</summary>
    public sealed class PresentationDefinition
    {
        public string AtlasFile;
        public string PortraitFile;
        /// <summary>Unités monde par pixel d'atlas (1 = un pixel par unité).</summary>
        public int UnitsPerPixel = 1;
        public Dictionary<string, AnimationDefinition> Animations = new Dictionary<string, AnimationDefinition>();
        public Dictionary<FighterVisualState, AnimationBinding> States = new Dictionary<FighterVisualState, AnimationBinding>();
        public Dictionary<string, AnimationBinding> Moves = new Dictionary<string, AnimationBinding>();
        public Dictionary<string, ProjectilePresentation> Projectiles = new Dictionary<string, ProjectilePresentation>();
        /// <summary>Signal sonore (ex. "moveStart:stand_kick_heavy") → fichier du paquet. Vide tant que l'audio n'est pas livré.</summary>
        public Dictionary<string, string> Sounds = new Dictionary<string, string>();
    }

    /// <summary>
    /// États visuels hors coups. Les sept premiers sont obligatoires ; les suivants
    /// (accroupi, saut) sont facultatifs et retombent sur un état voisin
    /// (FramePicker.Fallback) tant que le paquet n'en fournit pas.
    /// </summary>
    public enum FighterVisualState
    {
        Idle, WalkForward, WalkBack, Guard, Hitstun, Blockstun, KO,
        Crouch, CrouchGuard, CrouchHitstun, CrouchBlockstun, PreJump, Jump, Land, AirHitstun,
    }

    public sealed class AnimationDefinition
    {
        public string Name;
        public List<AnimationFrame> Frames = new List<AnimationFrame>();
        public int TotalDuration
        {
            get { int total = 0; foreach (var f in Frames) total += f.Duration; return total; }
        }
    }

    public sealed class AnimationFrame
    {
        public string Sprite;
        public int Duration;
    }

    /// <summary>Comment lire une animation pour un état ou un coup.</summary>
    public enum Playback
    {
        /// <summary>Lecture unique, synchronisée sur le tick de l'action (obligatoire pour les coups).</summary>
        Once,
        Loop,
        /// <summary>Boucle à l'envers (ex. marche arrière à partir de la marche avant).</summary>
        ReverseLoop,
        /// <summary>Étirée sur la durée de l'état (réactions dont la durée dépend du coup reçu).</summary>
        Stretch,
        /// <summary>Image fixe (Frame).</summary>
        Hold,
    }

    public sealed class AnimationBinding
    {
        public string Animation;
        public Playback Playback = Playback.Once;
        public int Frame;
    }

    public sealed class ProjectilePresentation
    {
        public string Animation;
        /// <summary>Effet joué une fois au contact (touché, bloqué, annulé).</summary>
        public string Impact;
    }

    /// <summary>Atlas généré par l'outil d'export : pages d'image et rectangles des sprites.</summary>
    public sealed class AtlasDefinition
    {
        public List<AtlasPage> Pages = new List<AtlasPage>();
        public Dictionary<string, AtlasSprite> Sprites = new Dictionary<string, AtlasSprite>();
    }

    public sealed class AtlasPage
    {
        public string File;
        public int Width, Height;
        public string Sha256;
    }

    /// <summary>Rectangle en pixels (origine en haut à gauche) et position du pivot dans ce rectangle.</summary>
    public sealed class AtlasSprite
    {
        public int Page, X, Y, W, H;
        public int PivotX, PivotY;
    }
}
