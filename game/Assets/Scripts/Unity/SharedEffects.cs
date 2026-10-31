using System.Collections.Generic;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Effets communs à tous les combattants, purement visuels : étincelle d'un
    /// coup au corps qui touche ou qui est gardé. Images de
    /// Resources/Effects/&lt;nom&gt;/1.png, 2.png… lues dans l'ordre, dessinées
    /// vers la droite, le point de contact au tiers gauche de l'image et à mi-hauteur
    /// (voir assets/ui/README.md). Un effet absent n'est simplement pas affiché.
    /// </summary>
    public static class SharedEffects
    {
        /// <summary>Unités monde par pixel des images d'effet.</summary>
        const float WorldUnitsPerPixel = 0.6f;
        static readonly Vector2 Pivot = new Vector2(1f / 3f, 0.5f);
        /// <summary>Durée de chaque image, en ticks.</summary>
        const int FrameTicks = 3;

        static readonly Dictionary<string, Animation> loaded = new Dictionary<string, Animation>();

        public static Animation Hit => Load("etincelle-coup");
        public static Animation Block => Load("etincelle-garde");

        public sealed class Animation
        {
            public Sprite[] Frames;
            public int Duration => Frames.Length * FrameTicks;
            public Sprite At(int tick) => Frames[Mathf.Clamp(tick / FrameTicks, 0, Frames.Length - 1)];
        }

        static Animation Load(string name)
        {
            if (loaded.TryGetValue(name, out var animation)) return animation;
            var frames = new List<Sprite>();
            for (int i = 1; ; i++)
            {
                var texture = Resources.Load<Texture2D>($"Effects/{name}/{i}");
                if (texture == null) break;
                float pixelsPerUnit = FighterVisuals.WorldUnitsPerUnityUnit / WorldUnitsPerPixel;
                frames.Add(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Pivot, pixelsPerUnit));
            }
            loaded[name] = animation = frames.Count == 0 ? null : new Animation { Frames = frames.ToArray() };
            return animation;
        }
    }
}
