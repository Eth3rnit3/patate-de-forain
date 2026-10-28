using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Patate.Content
{
    /// <summary>
    /// Décor : purement visuel, il n'influence pas le combat (les limites de
    /// l'arène sont dans MatchRules) : une couleur de fond, une couleur de sol
    /// et des calques d'image avec parallaxe.
    /// </summary>
    public sealed class StageDefinition
    {
        public string Id, Name;
        public string BackgroundColor = "#2b2f3a";
        public string GroundColor = "#1d2027";
        public List<StageLayer> Layers = new List<StageLayer>();
        /// <summary>Musique du décor (fichier du dossier du décor), absente pour l'instant.</summary>
        public string Music;

        static readonly Regex Color = new Regex("^#[0-9a-fA-F]{6}$");
        static readonly Regex Image = new Regex(@"^[a-zA-Z0-9_-]+\.png$");

        public static StageDefinition Read(string text, string file, ValidationReport report)
        {
            var node = JsonNode.Parse(text, file, report);
            if (node == null) return null;
            node.AllowOnly("formatVersion", "id", "name", "backgroundColor", "groundColor", "layers", "music");
            if (node.Int("formatVersion", 1, 1000) != 1) node.Fail("formatVersion", "version non prise en charge (attendue : 1)");
            var stage = new StageDefinition
            {
                Id = node.String("id", FighterPackageReader.IdPattern, "minuscules, chiffres, - ou _"),
                Name = node.String("name", maxLength: 60),
                BackgroundColor = node.String("backgroundColor", Color, "#rrggbb", required: false) ?? "#2b2f3a",
                GroundColor = node.String("groundColor", Color, "#rrggbb", required: false) ?? "#1d2027",
                Music = node.String("music", new Regex(@"^[a-zA-Z0-9_-]+\.(ogg|wav)$"), "fichier .ogg ou .wav", required: false),
            };
            foreach (var layer in node.Array("layers", 16, required: false))
            {
                layer.AllowOnly("image", "parallax", "scale", "y", "order");
                stage.Layers.Add(new StageLayer
                {
                    Image = layer.String("image", Image, "fichier .png du dossier du décor"),
                    ParallaxPercent = layer.Int("parallax", 0, 200, 100),
                    ScalePercent = layer.Int("scale", 25, 400, 100),
                    Y = layer.Int("y", -4096, 4096, 0),
                    Order = layer.Int("order", -100, 100, 0),
                });
            }
            return report.HasErrors ? null : stage;
        }
    }

    public sealed class StageLayer
    {
        public string Image;
        /// <summary>100 = suit le sol, 0 = immobile à l'écran.</summary>
        public int ParallaxPercent = 100;
        /// <summary>Agrandissement à l'affichage : 100 = un pixel par unité monde, 200 = deux unités par pixel.</summary>
        public int ScalePercent = 100;
        /// <summary>Position verticale du bas de l'image, en unités monde (0 = sol).</summary>
        public int Y;
        /// <summary>Ordre d'affichage ; négatif = derrière les combattants.</summary>
        public int Order;
    }
}
