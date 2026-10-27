using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Patate.Content;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Paquets livrés avec le jeu, sous Assets/Resources/Fighters/&lt;id&gt;/.
    /// Découverte automatique : tout dossier contenant un fighter.json est
    /// proposé au catalogue, qui le valide avant de l'accepter.
    /// </summary>
    public sealed class ResourcesFighterSource : IFighterSource
    {
        public const string Root = "Fighters";

        public IEnumerable<string> PackageFolders()
        {
            // Resources ne donne pas les noms de dossiers : on lit l'identifiant de chaque manifeste.
            // Un identifiant qui ne correspond pas à son dossier sera refusé par la validation.
            foreach (var manifest in Resources.LoadAll<TextAsset>(Root).Where(t => t.name == "fighter"))
            {
                string id = null;
                try { id = JObject.Parse(manifest.text).Value<string>("id"); }
                catch (Newtonsoft.Json.JsonException) { }
                if (!string.IsNullOrEmpty(id) && FighterPackageReader.IdPattern.IsMatch(id)) yield return id;
                else Debug.LogError("Manifeste fighter.json illisible dans Resources/Fighters : identifiant introuvable.");
            }
        }

        public IPackageFiles Open(string folder) => new Files(folder);

        public static string ResourcePath(string folder, string relativePath)
        {
            int dot = relativePath.LastIndexOf('.');
            return $"{Root}/{folder}/{(dot > 0 ? relativePath.Substring(0, dot) : relativePath)}";
        }

        sealed class Files : IPackageFiles
        {
            readonly string folder;

            public Files(string folder)
            {
                this.folder = folder;
            }

            public string ReadText(string relativePath) => Resources.Load<TextAsset>(ResourcePath(folder, relativePath))?.text;
        }
    }
}
