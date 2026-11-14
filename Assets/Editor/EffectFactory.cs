using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace NeonSwarm.EditorTools
{
    internal static class EffectFactory
    {
        private const string MaterialPath = "Assets/Materials/ParticlesAdditive.mat";

        public static FxLibrary CreateFx(SpriteSet sprites)
        {
            Directory.CreateDirectory(PrefabFactory.Folder + "/Effects");
            Directory.CreateDirectory("Assets/Materials");
            Directory.CreateDirectory("Assets/Resources");

            var material = ParticleMaterial(sprites.Particle.texture);
            var library = LoadOrCreate<FxLibrary>("Assets/Resources/FxLibrary.asset");
            library.explosion = Effect("Explosion", material, sprites.Ring, 24, 0.3f, 0.65f, 1.5f, 5.5f, 0.07f, 0.2f, 0.6f, 0.2f, 1.6f, false, 10);
            library.hitSpark = Effect("HitSpark", material, null, 7, 0.12f, 0.28f, 2f, 4.5f, 0.04f, 0.1f, 0.32f, 0f, 0f, false, 16);
            library.sparkle = Effect("Sparkle", material, sprites.Ring, 14, 0.35f, 0.7f, 0.8f, 2.5f, 0.05f, 0.12f, 0.7f, 0.2f, 1.1f, false, 8);
            library.pulseRing = Effect("PulseRing", material, sprites.Ring, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0.45f, 0.3f, 2f, false, 2);
            library.spawnPortal = Effect("SpawnPortal", material, sprites.Portal, 0, 0f, 0f, 0f, 0f, 0f, 0f, 0.6f, 1.4f, 0.5f, true, 6);
            library.floatingText = FloatingTextPrefab();
            EditorUtility.SetDirty(library);
            return library;
        }

        public static UiTheme CreateTheme(SpriteSet sprites)
        {
            var theme = LoadOrCreate<UiTheme>("Assets/Resources/UiTheme.asset");
            theme.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            theme.rounded = sprites.UiRounded;
            theme.roundedOutline = sprites.UiOutline;
            theme.circle = sprites.UiCircle;
            theme.joystickBase = sprites.JoystickBase;
            theme.joystickKnob = sprites.JoystickKnob;
            theme.heart = sprites.Heart;
            theme.heartEmpty = sprites.HeartEmpty;
            theme.coin = sprites.Coin;
            theme.pause = sprites.Pause;
            theme.pulse = sprites.Pulse;
            theme.lockIcon = sprites.Lock;
            theme.star = sprites.Star;
            theme.trophy = sprites.Trophy;
            theme.shield = sprites.ShieldIcon;
            theme.rapid = sprites.Rapid;
            theme.ship = sprites.Ship;
            EditorUtility.SetDirty(theme);
            return theme;
        }

        public static AudioLibrary CreateAudioLibrary(Dictionary<SfxId, AudioClip> sfx, Dictionary<MusicId, AudioClip> music)
        {
            var library = LoadOrCreate<AudioLibrary>("Assets/Audio/AudioLibrary.asset");
            var volumes = new Dictionary<SfxId, float>
            {
                [SfxId.Click] = 0.6f,
                [SfxId.Shoot] = 0.45f,
                [SfxId.EnemyShoot] = 0.6f,
                [SfxId.Hit] = 0.6f,
                [SfxId.Explosion] = 0.8f,
                [SfxId.Coin] = 0.6f,
                [SfxId.Spawn] = 0.45f
            };
            var entries = new List<AudioLibrary.SfxEntry>();
            foreach (var pair in sfx)
            {
                entries.Add(new AudioLibrary.SfxEntry
                {
                    id = pair.Key,
                    clip = pair.Value,
                    volume = volumes.TryGetValue(pair.Key, out var volume) ? volume : 0.9f,
                    pitchVariance = pair.Key == SfxId.Shoot || pair.Key == SfxId.Hit || pair.Key == SfxId.Explosion ? 0.08f : 0.02f,
                    minInterval = pair.Key == SfxId.Shoot ? 0.06f : 0.03f
                });
            }
            library.sfx = entries.ToArray();
            var tracks = new List<AudioLibrary.MusicEntry>();
            foreach (var pair in music)
            {
                tracks.Add(new AudioLibrary.MusicEntry { id = pair.Key, clip = pair.Value, volume = pair.Key == MusicId.Menu ? 0.55f : 0.5f });
            }
            library.music = tracks.ToArray();
            EditorUtility.SetDirty(library);
            return library;
        }

        public static GameObject CreateSystemsPrefab(AudioMixer mixer, AudioLibrary library)
        {
            var root = new GameObject("Systems");
            var audio = root.AddComponent<AudioManager>();
            Ser.Set(audio, ("mixer", mixer), ("library", library));
            root.AddComponent<SceneLoader>();
            root.AddComponent<Toast>();
            root.AddComponent<FpsCounter>();
            var path = "Assets/Resources/" + SystemsBootstrap.SystemsResource + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static PooledEffect Effect(string name, Material material, Sprite ringSprite, int burst, float lifeMin, float lifeMax, float speedMin, float speedMax, float sizeMin, float sizeMax, float duration, float ringStart, float ringEnd, bool spins, int prewarm)
        {
            var root = new GameObject(name);
            ParticleSystem particles = null;
            if (burst > 0)
            {
                particles = root.AddComponent<ParticleSystem>();
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var main = particles.main;
                main.duration = duration;
                main.loop = false;
                main.playOnAwake = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
                main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
                main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
                main.maxParticles = burst * 2;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
                var emission = particles.emission;
                emission.rateOverTime = 0f;
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)burst) });
                var shape = particles.shape;
                shape.shapeType = ParticleSystemShapeType.Circle;
                shape.radius = 0.12f;
                var colors = particles.colorOverLifetime;
                colors.enabled = true;
                var gradient = new Gradient();
                gradient.SetKeys(
                    new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                    new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
                colors.color = gradient;
                var size = particles.sizeOverLifetime;
                size.enabled = true;
                size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.1f));
                var renderer = root.GetComponent<ParticleSystemRenderer>();
                renderer.sharedMaterial = material;
                renderer.sortingOrder = SortingOrder.Effects;
            }

            SpriteRenderer ring = null;
            if (ringSprite != null)
            {
                ring = PrefabFactory.Renderer("Ring", root.transform, ringSprite, SortingOrder.Effects - 1, new Color(1f, 1f, 1f, 0f));
            }

            var poolable = root.AddComponent<Poolable>();
            Ser.Set(poolable, "prewarmCount", prewarm);
            var effect = root.AddComponent<PooledEffect>();
            Ser.Set(effect,
                ("particles", particles),
                ("ring", ring),
                ("duration", duration),
                ("ringStartScale", ringStart),
                ("ringEndScale", ringEnd),
                ("ringSpins", spins));
            return PrefabFactory.Save(root, "Effects/" + name).GetComponent<PooledEffect>();
        }

        private static FloatingText FloatingTextPrefab()
        {
            var root = new GameObject("FloatingText");
            var mesh = root.AddComponent<TextMesh>();
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mesh.font = font;
            mesh.fontSize = 64;
            mesh.characterSize = 0.05f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.fontStyle = FontStyle.Bold;
            mesh.text = "+100";
            root.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            var poolable = root.AddComponent<Poolable>();
            Ser.Set(poolable, "prewarmCount", 10);
            root.AddComponent<FloatingText>();
            return PrefabFactory.Save(root, "Effects/FloatingText").GetComponent<FloatingText>();
        }

        private static Material ParticleMaterial(Texture texture)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            var shader = Shader.Find("Legacy Shaders/Particles/Additive") ?? Shader.Find("Sprites/Default");
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            material.shader = shader;
            material.mainTexture = texture;
            EditorUtility.SetDirty(material);
            return material;
        }

        internal static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }
    }
}
