using UnityEditor;
using UnityEngine;

namespace Patate.EditorTools
{
    /// <summary>
    /// Réglages d'import des images de l'interface (Resources/UI) : taille
    /// native jusqu'à 2048, pas de mipmaps (affichées à l'écran à leur taille
    /// ou presque), filtrage linéaire, compression de qualité.
    /// </summary>
    public sealed class UiAssetImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Resources/UI/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.crunchedCompression = false;
        }
    }
}
