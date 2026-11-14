using System;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace NeonSwarm.EditorTools
{
    public static class ProjectBuilder
    {
        private const string AtlasPath = "Assets/Sprites/NeonSwarm.spriteatlas";

        [MenuItem("Neon Swarm/Rebuild Generated Content", priority = 1)]
        public static void BuildAll()
        {
            ConfigureProject();
            var sprites = SpriteFactory.GenerateAll();
            CreateAtlas();
            var sfx = AudioFactory.GenerateSfx();
            var music = AudioFactory.GenerateMusic();
            var mixer = MixerFactory.Create();
            var audioLibrary = EffectFactory.CreateAudioLibrary(sfx, music);
            EffectFactory.CreateTheme(sprites);
            AssetDatabase.SaveAssets();
            UiTheme.ClearCache();
            EffectFactory.CreateFx(sprites);
            EffectFactory.CreateSystemsPrefab(mixer, audioLibrary);
            var animations = AnimationFactory.Create(sprites);
            var prefabs = PrefabFactory.Create(sprites, animations);
            var levels = LevelFactory.Create();
            AssetDatabase.SaveAssets();
            SceneFactory.CreateAll(sprites, prefabs, levels);
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { sprites.Icon });
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[NeonSwarm] Generated content rebuilt.");
        }

        public static void BuildAllBatch()
        {
            RunBatch(BuildAll);
        }

        internal static void RunBatch(Action action)
        {
            try
            {
                action();
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[NeonSwarm] Batch step failed: {exception}");
                EditorApplication.Exit(1);
            }
        }

        [MenuItem("Neon Swarm/Configure Project Settings", priority = 2)]
        public static void ConfigureProject()
        {
            ConfigureTagsAndLayers();
            PhysicsSetup.Apply();
            Physics2D.gravity = Vector2.zero;

            PlayerSettings.companyName = "NeonSwarm";
            PlayerSettings.productName = "Neon Swarm";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.neonswarm.game");
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.runInBackground = false;
            PlayerSettings.colorSpace = ColorSpace.Gamma;
            PlayerSettings.SplashScreen.backgroundColor = new Color(0.04f, 0.06f, 0.12f);
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low);
            var architectures = AndroidArchitecture.ARM64;
            if (Enum.TryParse<AndroidArchitecture>("X86_64", out var x64))
            {
                architectures |= x64;
            }
            PlayerSettings.Android.targetArchitectures = architectures;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
            PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
            PlayerSettings.Android.optimizedFramePacing = true;
            PlayerSettings.Android.startInFullscreen = true;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            EditorUserBuildSettings.buildAppBundle = false;

            for (var level = 0; level < QualitySettings.names.Length; level++)
            {
                QualitySettings.SetQualityLevel(level, false);
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.antiAliasing = 0;
                QualitySettings.vSyncCount = 0;
                QualitySettings.pixelLightCount = 0;
                QualitySettings.softParticles = false;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.globalTextureMipmapLimit = 0;
            }
            QualitySettings.SetQualityLevel(QualitySettings.names.Length - 1, false);
            EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
            AssetDatabase.SaveAssets();
        }

        private static void ConfigureTagsAndLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tags = tagManager.FindProperty("tags");
            foreach (var tag in Tags.Custom)
            {
                var exists = false;
                for (var i = 0; i < tags.arraySize; i++)
                {
                    if (tags.GetArrayElementAtIndex(i).stringValue == tag)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    tags.InsertArrayElementAtIndex(tags.arraySize);
                    tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
                }
            }
            var layers = tagManager.FindProperty("layers");
            for (var i = 0; i < Layers.CustomIndices.Length; i++)
            {
                layers.GetArrayElementAtIndex(Layers.CustomIndices[i]).stringValue = Layers.CustomNames[i];
            }
            tagManager.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateAtlas()
        {
            AssetDatabase.DeleteAsset(AtlasPath);
            var atlas = new SpriteAtlas();
            atlas.SetPackingSettings(new SpriteAtlasPackingSettings
            {
                enableRotation = false,
                enableTightPacking = false,
                padding = 4
            });
            atlas.SetTextureSettings(new SpriteAtlasTextureSettings
            {
                filterMode = FilterMode.Bilinear,
                generateMipMaps = false,
                readable = false,
                sRGB = true
            });
            atlas.Add(new Object[]
            {
                AssetDatabase.LoadAssetAtPath<Object>("Assets/Sprites/Game"),
                AssetDatabase.LoadAssetAtPath<Object>("Assets/Sprites/UI")
            });
            AssetDatabase.CreateAsset(atlas, AtlasPath);
        }
    }
}
