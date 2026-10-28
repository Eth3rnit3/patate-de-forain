using System.Collections.Generic;
using Patate.Content;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Décor chargé depuis Resources/Stages/&lt;id&gt;/stage.json : fond uni, bande
    /// de sol sous y = 0 et calques d'image avec parallaxe.
    /// </summary>
    public sealed class StageView
    {
        public readonly StageDefinition Definition;
        public readonly Color Background;
        public readonly AudioClip Music;
        readonly Camera camera;
        readonly List<(Transform transform, StageLayer layer)> layers = new List<(Transform, StageLayer)>();
        readonly Transform root;
        const float Scale = 1f / FighterVisuals.WorldUnitsPerUnityUnit;

        public StageView(string id, Camera camera)
        {
            this.camera = camera;
            var report = new ValidationReport();
            var text = Resources.Load<TextAsset>($"Stages/{id}/stage")?.text;
            Definition = StageDefinition.Read(text, $"Stages/{id}/stage.json", report);
            foreach (var issue in report.Issues) Debug.LogWarning(issue.ToString());
            Definition ??= new StageDefinition { Id = id, Name = id };
            ColorUtility.TryParseHtmlString(Definition.BackgroundColor, out Background);
            ColorUtility.TryParseHtmlString(Definition.GroundColor, out var ground);
            if (Definition.Music != null) Music = Resources.Load<AudioClip>($"Stages/{id}/{Strip(Definition.Music)}");

            root = new GameObject($"Décor : {Definition.Name}").transform;
            CreateGround(root, ground);
            foreach (var layer in Definition.Layers)
            {
                var texture = Resources.Load<Texture2D>($"Stages/{id}/{Strip(layer.Image)}");
                if (texture == null)
                {
                    Debug.LogWarning($"Décor {id} : image {layer.Image} introuvable.");
                    continue;
                }
                var go = new GameObject(layer.Image);
                go.transform.SetParent(root, false);
                var renderer = go.AddComponent<SpriteRenderer>();
                // Une unité monde par pixel à l'échelle 100, ancré en bas au centre.
                float pixelsPerUnit = FighterVisuals.WorldUnitsPerUnityUnit * 100f / layer.ScalePercent;
                renderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0f), pixelsPerUnit);
                renderer.sortingOrder = layer.Order;
                layers.Add((go.transform, layer));
            }
        }

        /// <summary>Parallaxe : chaque calque suit la caméra selon son coefficient.</summary>
        public void Follow()
        {
            float cameraX = camera.transform.position.x;
            foreach (var (transform, layer) in layers)
                transform.position = new Vector3(cameraX * (1f - layer.ParallaxPercent / 100f), layer.Y * Scale, 0);
        }

        /// <summary>Retire le décor de la scène (fin du combat).</summary>
        public void Dispose() => Object.Destroy(root.gameObject);

        static void CreateGround(Transform root, Color color)
        {
            var texture = Texture2D.whiteTexture;
            var go = new GameObject("Sol");
            go.transform.SetParent(root, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 1f), texture.width);
            renderer.color = color;
            renderer.sortingOrder = -50;
            go.transform.localScale = new Vector3(100f, 5f, 1f); // bande de 100 × 5 unités Unity sous le sol
        }

        static string Strip(string file) => file.Substring(0, file.LastIndexOf('.'));
    }
}
