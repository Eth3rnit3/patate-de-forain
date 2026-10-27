using System;
using System.Collections.Generic;
using Patate.Content;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Ressources graphiques d'un paquet prêtes à l'affichage : un Sprite Unity
    /// par entrée de l'atlas, pivot compris, et le portrait. Les sprites sont
    /// créés à l'exécution depuis atlas.json, ce qui sera aussi le chemin des
    /// paquets importés (aucune configuration manuelle dans l'éditeur).
    /// </summary>
    public sealed class FighterVisuals
    {
        /// <summary>Unités monde de la simulation par unité Unity.</summary>
        public const float WorldUnitsPerUnityUnit = 100f;

        public readonly FighterPackage Package;
        public readonly Texture2D Portrait;
        readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

        public FighterVisuals(FighterPackage package)
        {
            Package = package;
            var textures = new List<Texture2D>();
            foreach (var page in package.Atlas.Pages)
            {
                var texture = Resources.Load<Texture2D>(ResourcesFighterSource.ResourcePath(package.Id, page.File))
                              ?? throw new InvalidOperationException($"{package.Name} : page d'atlas {page.File} introuvable dans Resources.");
                if (texture.width != page.Width || texture.height != page.Height)
                    throw new InvalidOperationException(
                        $"{package.Name} : {page.File} est importée en {texture.width} × {texture.height} au lieu de {page.Width} × {page.Height}. " +
                        "Vérifier les réglages d'import (taille native, sans redimensionnement).");
                textures.Add(texture);
            }
            float pixelsPerUnit = WorldUnitsPerUnityUnit / package.Presentation.UnitsPerPixel;
            foreach (var entry in package.Atlas.Sprites)
            {
                var s = entry.Value;
                var texture = textures[s.Page];
                // atlas.json : origine en haut à gauche ; Unity : origine en bas à gauche.
                var rect = new Rect(s.X, texture.height - s.Y - s.H, s.W, s.H);
                var pivot = new Vector2((float)s.PivotX / s.W, 1f - (float)s.PivotY / s.H);
                var sprite = UnityEngine.Sprite.Create(texture, rect, pivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
                sprite.name = entry.Key;
                sprites[entry.Key] = sprite;
            }
            if (package.Presentation.PortraitFile != null)
                Portrait = Resources.Load<Texture2D>(ResourcesFighterSource.ResourcePath(package.Id, package.Presentation.PortraitFile));
        }

        public Sprite Sprite(string name) => name != null && sprites.TryGetValue(name, out var sprite) ? sprite : null;
    }
}
