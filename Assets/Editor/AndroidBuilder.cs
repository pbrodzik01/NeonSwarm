using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NeonSwarm.EditorTools
{
    public static class AndroidBuilder
    {
        public const string ApkPath = "Builds/Android/NeonSwarm.apk";

        [MenuItem("Neon Swarm/Build Android APK", priority = 20)]
        public static void BuildApk()
        {
            ProjectBuilder.ConfigureProject();
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
            Directory.CreateDirectory(Path.GetDirectoryName(ApkPath));
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log($"[NeonSwarm] Build {summary.result}: {summary.outputPath} ({summary.totalSize / (1024f * 1024f):0.0} MB) in {summary.totalTime}");
            if (summary.result != BuildResult.Succeeded)
            {
                throw new Exception($"Android build failed with {summary.totalErrors} error(s)");
            }
        }

        public static void BuildApkBatch()
        {
            ProjectBuilder.RunBatch(BuildApk);
        }
    }
}
