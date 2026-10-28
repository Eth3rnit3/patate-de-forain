using System.Collections.Generic;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Boîte à outils de l'interface (IMGUI), au style de la fête foraine :
    /// palette du logo (jaune, orange, rouge, bleu marine), textes détourés,
    /// panneaux à ampoules, fond de menu et mots d'annonce peints. Tout est
    /// dessiné dans un repère de 1080 unités de haut, quelle que soit la
    /// fenêtre ; la largeur suit le format de l'écran.
    /// </summary>
    public static class Ui
    {
        public const float ReferenceHeight = 1080f;

        public static readonly Color Navy = new Color32(0x0b, 0x16, 0x3a, 0xff);
        public static readonly Color NavyLight = new Color32(0x1c, 0x34, 0x7c, 0xff);
        public static readonly Color Yellow = new Color32(0xff, 0xd8, 0x2b, 0xff);
        public static readonly Color Orange = new Color32(0xff, 0x8a, 0x1c, 0xff);
        public static readonly Color Red = new Color32(0xe8, 0x2a, 0x1e, 0xff);
        public static readonly Color Cream = new Color32(0xfb, 0xf0, 0xd6, 0xff);
        public static readonly Color Muted = new Color32(0xb7, 0xbf, 0xd9, 0xff);
        /// <summary>Couleur de chaque joueur : J1 rouge, J2 bleu ciel.</summary>
        public static readonly Color[] PlayerColors = { new Color32(0xff, 0x4a, 0x2e, 0xff), new Color32(0x3d, 0xb4, 0xff, 0xff) };

        static Font font;
        static Texture2D circle, gradient;
        static readonly Dictionary<string, Texture2D> images = new Dictionary<string, Texture2D>();
        static readonly Dictionary<(int, FontStyle, TextAnchor, bool), GUIStyle> styles = new Dictionary<(int, FontStyle, TextAnchor, bool), GUIStyle>();

        /// <summary>Largeur courante du repère (1080 de haut).</summary>
        public static float Width { get; private set; } = 1920;
        public static float Scale => Screen.height / ReferenceHeight;

        /// <summary>Passe dans le repère de référence. À appeler au début de chaque OnGUI.</summary>
        public static Matrix4x4 Begin()
        {
            var previous = GUI.matrix;
            float scale = Mathf.Max(0.01f, Scale);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            Width = Screen.width / scale;
            return previous;
        }

        public static Font Font => font ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static GUIStyle Style(int size, TextAnchor align = TextAnchor.MiddleCenter, FontStyle fontStyle = FontStyle.Bold, bool wrap = false)
        {
            var key = (size, fontStyle, align, wrap);
            if (!styles.TryGetValue(key, out var style))
            {
                style = new GUIStyle { font = Font, fontSize = size, fontStyle = fontStyle, alignment = align, wordWrap = wrap, richText = true, clipping = TextClipping.Overflow };
                styles[key] = style;
            }
            return style;
        }

        /// <summary>Image de Resources/UI (null si elle n'est pas livrée : l'appelant prévoit un texte de repli).</summary>
        public static Texture2D Image(string name)
        {
            if (!images.TryGetValue(name, out var texture)) images[name] = texture = Resources.Load<Texture2D>($"UI/{name}");
            return texture;
        }

        // ------------------------------------------------------------ primitives

        public static void Fill(Rect rect, Color color)
        {
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        public static void Frame(Rect rect, Color color, float thickness)
        {
            Fill(new Rect(rect.x, rect.y, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.y, thickness, rect.height), color);
            Fill(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        /// <summary>Dégradé vertical : haut → bas.</summary>
        public static void Gradient(Rect rect, Color top, Color bottom)
        {
            gradient ??= MakeGradient();
            // La texture va du blanc (haut) au noir (bas) : deux passes teintées les mélangent.
            Fill(rect, bottom);
            var previous = GUI.color;
            GUI.color = top;
            GUI.DrawTexture(rect, gradient);
            GUI.color = previous;
        }

        public static void Disc(Vector2 center, float radius, Color color)
        {
            circle ??= MakeCircle();
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(center.x - radius, center.y - radius, radius * 2, radius * 2), circle);
            GUI.color = previous;
        }

        public static void Texture(Rect rect, Texture texture, bool mirror = false, float alpha = 1f, ScaleMode mode = ScaleMode.ScaleToFit)
        {
            if (texture == null) return;
            var previous = GUI.color;
            GUI.color = new Color(1, 1, 1, alpha * previous.a);
            if (mirror)
            {
                // ScaleToFit à la main, puis coordonnées de texture inversées.
                float aspect = (float)texture.width / texture.height;
                var fit = rect.width / rect.height > aspect
                    ? new Rect(rect.center.x - rect.height * aspect / 2, rect.y, rect.height * aspect, rect.height)
                    : new Rect(rect.x, rect.center.y - rect.width / aspect / 2, rect.width, rect.width / aspect);
                GUI.DrawTextureWithTexCoords(fit, texture, new Rect(1, 0, -1, 1));
            }
            else GUI.DrawTexture(rect, texture, mode);
            GUI.color = previous;
        }

        /// <summary>Texte détouré de bleu marine, avec ombre portée, comme le logo.</summary>
        public static void Text(Rect rect, string text, GUIStyle style, Color color, float outline = 3f, float alpha = 1f)
        {
            var normal = style.normal.textColor;
            if (outline > 0)
            {
                style.normal.textColor = new Color(Navy.r, Navy.g, Navy.b, alpha);
                GUI.Label(new Rect(rect.x + outline * 1.4f, rect.y + outline * 1.8f, rect.width, rect.height), text, style);
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * Mathf.PI / 4;
                    GUI.Label(new Rect(rect.x + Mathf.Cos(angle) * outline, rect.y + Mathf.Sin(angle) * outline, rect.width, rect.height), text, style);
                }
            }
            style.normal.textColor = new Color(color.r, color.g, color.b, color.a * alpha);
            GUI.Label(rect, text, style);
            style.normal.textColor = normal;
        }

        // ------------------------------------------------------------ éléments

        /// <summary>Fond des menus : la fête foraine peinte, ou un dégradé nocturne si l'image manque.</summary>
        public static void Background(float dim = 0.35f)
        {
            var screen = new Rect(0, 0, Width, ReferenceHeight);
            var image = Image("menu-background");
            if (image != null) GUI.DrawTexture(screen, image, ScaleMode.ScaleAndCrop);
            else Gradient(screen, new Color32(0x1c, 0x2a, 0x63, 0xff), new Color32(0x07, 0x0b, 0x1f, 0xff));
            if (dim > 0) Fill(screen, new Color(0.02f, 0.03f, 0.1f, dim));
        }

        public static void Logo(Rect rect, float alpha = 1f)
        {
            var logo = Image("logo");
            if (logo != null) Texture(rect, logo, alpha: alpha);
            else Text(rect, "PATATE DE FORAIN", Style(Mathf.RoundToInt(rect.height * 0.3f)), Yellow, 5, alpha);
        }

        /// <summary>
        /// Enseigne foraine : panneau bleu marine bordé d'ampoules. Les ampoules
        /// clignotent en chenillard quand le panneau est sélectionné.
        /// </summary>
        public static void Marquee(Rect rect, Color border, bool lit, float time, float bulbSpacing = 34f)
        {
            Fill(new Rect(rect.x + 6, rect.y + 8, rect.width, rect.height), new Color(0, 0, 0, 0.45f));
            Fill(rect, border);
            var inner = new Rect(rect.x + 10, rect.y + 10, rect.width - 20, rect.height - 20);
            Gradient(inner, lit ? new Color32(0x24, 0x3f, 0x93, 0xff) : new Color32(0x16, 0x26, 0x5c, 0xff), Navy);
            Bulbs(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10), lit, time, bulbSpacing);
        }

        public static void Bulbs(Rect rect, bool lit, float time, float spacing)
        {
            int across = Mathf.Max(2, Mathf.RoundToInt(rect.width / spacing));
            int down = Mathf.Max(1, Mathf.RoundToInt(rect.height / spacing));
            int step = Mathf.FloorToInt(time * 8);
            int index = 0;
            void Bulb(float x, float y)
            {
                bool on = !lit || (index + step) % 3 != 0;
                index++;
                if (on && lit) Disc(new Vector2(x, y), 9, new Color(1f, 0.85f, 0.3f, 0.35f));
                Disc(new Vector2(x, y), 4.5f, on ? new Color(1f, 0.96f, 0.75f) : new Color(0.55f, 0.4f, 0.2f));
            }
            for (int i = 0; i <= across; i++) Bulb(rect.x + rect.width * i / across, rect.y);
            for (int i = 1; i <= down; i++) Bulb(rect.xMax, rect.y + rect.height * i / down);
            for (int i = across - 1; i >= 0; i--) Bulb(rect.x + rect.width * i / across, rect.yMax);
            for (int i = down - 1; i >= 1; i--) Bulb(rect.x, rect.y + rect.height * i / down);
        }

        /// <summary>Bouton de menu : enseigne, allumée quand il est sélectionné.</summary>
        public static void Button(Rect rect, string label, string detail, bool selected, bool enabled, float time)
        {
            float grow = selected ? 8 + Mathf.Sin(time * 6) * 3 : 0;
            var r = new Rect(rect.x - grow, rect.y - grow / 2, rect.width + grow * 2, rect.height + grow);
            Marquee(r, selected ? Red : new Color32(0x6b, 0x1c, 0x1a, 0xff), selected, time);
            if (!enabled) Fill(r, new Color(0.02f, 0.03f, 0.1f, 0.55f)); // éteint : indisponible
            var color = !enabled ? Muted : selected ? Yellow : Cream;
            if (detail == null) Text(r, label, Style(46), color, 3);
            else
            {
                Text(new Rect(r.x, r.y + r.height * 0.12f, r.width, r.height * 0.5f), label, Style(42), color, 3);
                Text(new Rect(r.x + 30, r.y + r.height * 0.56f, r.width - 60, r.height * 0.34f), detail, Style(22, fontStyle: FontStyle.Normal), enabled ? Cream : Muted, 0);
            }
        }

        /// <summary>
        /// Mot d'annonce peint (Resources/UI/words/&lt;nom&gt;), avec un effet
        /// d'arrivée : grossit puis se pose. <paramref name="age"/> en secondes.
        /// </summary>
        public static void Word(string name, string fallback, Vector2 center, float height, float age)
        {
            float pop = age < 0.18f ? Mathf.Lerp(1.8f, 0.92f, age / 0.18f) : age < 0.3f ? Mathf.Lerp(0.92f, 1f, (age - 0.18f) / 0.12f) : 1f;
            float alpha = Mathf.Clamp01(age / 0.1f);
            height *= pop;
            var image = Image($"words/{name}");
            if (image != null)
            {
                float width = height * image.width / image.height;
                Texture(new Rect(center.x - width / 2, center.y - height / 2, width, height), image, alpha: alpha);
            }
            else
            {
                var style = Style(Mathf.RoundToInt(height * 0.62f));
                Text(new Rect(center.x - 1200, center.y - height / 2, 2400, height), fallback, style, Yellow, 6, alpha);
            }
        }

        /// <summary>Bandeau d'aide en bas d'écran.</summary>
        public static void Hint(string text, float y = ReferenceHeight - 54)
        {
            Fill(new Rect(0, y - 8, Width, 52), new Color(0.02f, 0.04f, 0.12f, 0.75f));
            Text(new Rect(0, y, Width, 36), text, Style(24, fontStyle: FontStyle.Normal), Cream, 0);
        }

        // ------------------------------------------------------------ textures

        static Texture2D MakeCircle()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size / 2f, size / 2f));
                    pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(size / 2f - d));
                }
            texture.SetPixels32(pixels);
            texture.Apply();
            return texture;
        }

        static Texture2D MakeGradient()
        {
            const int size = 64;
            var texture = new Texture2D(1, size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            // Rangée 0 en bas dans Unity : opaque en haut, transparent en bas.
            for (int y = 0; y < size; y++) texture.SetPixel(0, y, new Color(1, 1, 1, (float)y / (size - 1)));
            texture.Apply();
            return texture;
        }
    }
}
