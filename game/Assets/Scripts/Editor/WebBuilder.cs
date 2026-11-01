using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Patate.EditorTools
{
    /// <summary>
    /// Export navigateur (WebGL), sur le modèle d'Action Legends : fichiers
    /// compressés en Gzip avec décompression de repli (tout hébergeur statique
    /// convient, GitHub Pages compris), sans threads (pas d'en-têtes COOP/COEP
    /// à configurer), mémoire de 256 Mo extensible jusqu'à 1 Go.
    /// </summary>
    public static class WebBuilder
    {
        const string ScenePath = "Assets/Scenes/Combat.unity";
        // PATATE_WEB_DIR construit ailleurs (ex. WebBuild-essai) pendant que WebBuild/ est servi.
        const string FolderVariable = "PATATE_WEB_DIR";

        public static void Configure()
        {
            PlayerSettings.productName = "Patate de Forain";
            PlayerSettings.companyName = "Patate de Forain";
            PlayerSettings.defaultWebScreenWidth = 1280;
            PlayerSettings.defaultWebScreenHeight = 720;
            PlayerSettings.WebGL.template = "PROJECT:PatateDeForain";
            PlayerSettings.SplashScreen.show = false; // le gabarit montre déjà le logo du jeu pendant le chargement
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.threadsSupport = false;
            PlayerSettings.WebGL.initialMemorySize = 256;
            PlayerSettings.WebGL.maximumMemorySize = 1024;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
            PlayerSettings.WebGL.nameFilesAsHashes = true;
            PlayerSettings.WebGL.showDiagnostics = false;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Construit dans game/WebBuild/. En ligne de commande, depuis game/ :
        /// unity build --target WebGL --execute-method Patate.EditorTools.WebBuilder.Build
        /// </summary>
        [MenuItem("Patate de Forain/Construire la version navigateur")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new BuildFailedException("Arrêter le mode Play avant de compiler.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                throw new BuildFailedException("Choisir Web dans Build Profiles et activer cette plateforme avant la compilation.");
            var folder = Environment.GetEnvironmentVariable(FolderVariable);
            Configure();
            // Relatif au projet (game/), pas au dossier courant : GameCI lance Unity depuis la racine du dépôt.
            var project = Path.GetDirectoryName(UnityEngine.Application.dataPath);
            var path = Path.GetFullPath(Path.Combine(project, string.IsNullOrEmpty(folder) ? "WebBuild" : folder));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                target = BuildTarget.WebGL,
                locationPathName = path,
                options = BuildOptions.None,
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Compilation web échouée : " + report.summary.result);
            Debug.Log($"Version navigateur construite dans {path} ({report.summary.totalSize / (1024 * 1024)} Mo, {report.summary.totalTime.TotalSeconds:0} s).");
            // GameCI valide une méthode de build personnalisée en cherchant ce texte exact dans la sortie Unity.
            Console.WriteLine("Build succeeded!");
        }

        [PostProcessBuild(50)]
        public static void Finish(BuildTarget target, string path)
        {
            if (target != BuildTarget.WebGL) return;
            // GitHub Pages ignorerait sinon les fichiers et dossiers commençant par « _ ».
            File.WriteAllText(Path.Combine(path, ".nojekyll"), "");
            File.WriteAllText(Path.Combine(path, "build-info.json"),
                "{\"game\":\"Patate de Forain\",\"version\":\"" + UnityEngine.Application.version +
                "\",\"unity\":\"" + UnityEngine.Application.unityVersion +
                "\",\"built\":\"" + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + "\"}");
        }
    }
}
