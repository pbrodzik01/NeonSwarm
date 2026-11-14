using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NeonSwarm.EditorTools
{
    public static class WebGLBuilder
    {
        public const string OutputPath = "Builds/WebGL";

        [MenuItem("Neon Swarm/Build WebGL", priority = 21)]
        public static void BuildWebGL()
        {
            ProjectBuilder.ConfigureProject();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.defaultWebScreenWidth = 540;
            PlayerSettings.defaultWebScreenHeight = 960;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.WebGL, ManagedStrippingLevel.Low);
            EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;

            if (Directory.Exists(OutputPath))
            {
                Directory.Delete(OutputPath, true);
            }
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log($"[NeonSwarm] WebGL build {summary.result}: {summary.outputPath} in {summary.totalTime}");
            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"WebGL build failed with {summary.totalErrors} error(s)");
            }
            PatchIndexHtml(Path.Combine(OutputPath, "index.html"));
        }

        public static void BuildWebGLBatch()
        {
            ProjectBuilder.RunBatch(BuildWebGL);
        }

        private static void PatchIndexHtml(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }
            var html = File.ReadAllText(path);
            html = html.Replace("unityShowBanner('WebGL builds are not supported on mobile devices.');", string.Empty);
            html = html.Replace("<head>", "<head>\n    <meta name=\"apple-mobile-web-app-capable\" content=\"yes\">\n    <meta name=\"mobile-web-app-capable\" content=\"yes\">\n    <meta name=\"apple-mobile-web-app-status-bar-style\" content=\"black-translucent\">\n    <meta name=\"theme-color\" content=\"#0a0f1e\">\n    <style>html,body{background:#0a0f1e;overscroll-behavior:none;touch-action:none}</style>");
            File.WriteAllText(path, html);
        }
    }
}
