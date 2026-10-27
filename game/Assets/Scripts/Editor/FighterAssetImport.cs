using System.Linq;
using Patate.Content;
using UnityEditor;
using UnityEngine;

namespace Patate.EditorTools
{
    /// <summary>
    /// Réglages d'import imposés aux images des paquets de combattants et de
    /// décors : taille native (les rectangles de l'atlas sont en pixels), pas
    /// de mipmaps ni de redimensionnement, filtrage linéaire pour le rendu HD.
    /// </summary>
    public sealed class FighterAssetImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Resources/Fighters/") && !assetPath.StartsWith("Assets/Resources/Stages/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.crunchedCompression = false;
        }
    }

    public static class FighterMenu
    {
        /// <summary>Valide tous les paquets de Resources/Fighters et affiche le rapport dans la console.</summary>
        [MenuItem("Patate de Forain/Valider les combattants")]
        public static void ValidateFighters()
        {
            var catalog = FighterCatalog.Load(new DirectoryFighterSource(System.IO.Path.Combine(UnityEngine.Application.dataPath, "Resources", "Fighters")));
            foreach (var fighter in catalog.Fighters)
            {
                Debug.Log($"✔ {fighter.Name} — gameplay {fighter.GameplayRef}, présentation #{fighter.PresentationHash.Substring(0, 12)}" +
                          (fighter.Warnings.Count > 0 ? "\n" + string.Join("\n", fighter.Warnings.Select(w => w.ToString())) : ""));
            }
            foreach (var rejected in catalog.Rejected) Debug.LogError($"✘ Paquet « {rejected.Key} » refusé :\n{rejected.Value}");
            if (catalog.Fighters.Count == 0 && catalog.Rejected.Count == 0) Debug.LogWarning("Aucun paquet trouvé dans Assets/Resources/Fighters.");
        }
    }
}
