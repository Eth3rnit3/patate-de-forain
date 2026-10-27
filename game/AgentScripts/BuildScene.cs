using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Construit la scène de combat et applique les réglages d'import des paquets.
// Exécuté dans l'éditeur ouvert : unity command script run --file AgentScripts/BuildScene.cs
public static class BuildScene
{
    const string ScenePath = "Assets/Scenes/Combat.unity";

    public static object Main()
    {
        var report = new List<string>();

        // Les réglages de FighterAssetImport ne s'appliquent qu'à l'import : on force la réimportation.
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Resources/Fighters", "Assets/Resources/Stages" }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            report.Add($"{path} : {texture.width} × {texture.height}, {texture.format}, mipmaps {texture.mipmapCount}");
        }

        System.IO.Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Caméra", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.transform.position = new Vector3(0, 4.1f, -10);
        new GameObject("Patate de Forain", typeof(Patate.UnityAdapters.GameBootstrap));
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        PlayerSettings.productName = "Patate de Forain";
        PlayerSettings.companyName = "Patate de Forain";
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        AssetDatabase.SaveAssets();
        report.Add($"Scène {ScenePath} créée et ajoutée au build.");
        return string.Join("\n", report);
    }
}
