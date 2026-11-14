using UnityEngine;

namespace NeonSwarm
{
    [CreateAssetMenu(menuName = "Neon Swarm/FX Library", fileName = "FxLibrary")]
    public class FxLibrary : ScriptableObject
    {
        public PooledEffect explosion;
        public PooledEffect hitSpark;
        public PooledEffect sparkle;
        public PooledEffect pulseRing;
        public PooledEffect spawnPortal;
        public FloatingText floatingText;

        private static FxLibrary current;
        private static bool searched;

        public static FxLibrary Current
        {
            get
            {
                if (current == null && !searched)
                {
                    searched = true;
                    current = Resources.Load<FxLibrary>("FxLibrary");
                }
                return current;
            }
        }
    }

    public static class Fx
    {
        private static int effectsThisFrame;
        private static int frame;
        private const int MaxEffectsPerFrame = 6;

        public static void Explosion(Vector3 position, Color color, float scale = 1f)
        {
            Spawn(FxLibrary.Current?.explosion, position, color, scale, true);
        }

        public static void Hit(Vector3 position, Color color)
        {
            Spawn(FxLibrary.Current?.hitSpark, position, color, 1f, false);
        }

        public static void Sparkle(Vector3 position, Color color)
        {
            Spawn(FxLibrary.Current?.sparkle, position, color, 1f, false);
        }

        public static void PulseRing(Vector3 position, float radius, Color color)
        {
            Spawn(FxLibrary.Current?.pulseRing, position, color, radius, true);
        }

        public static void Portal(Vector3 position, Color color)
        {
            Spawn(FxLibrary.Current?.spawnPortal, position, color, 1f, true);
        }

        public static void Text(Vector3 position, string text, Color color)
        {
            var library = FxLibrary.Current;
            if (library == null || library.floatingText == null || !Budget(false))
            {
                return;
            }
            PoolManager.Spawn(library.floatingText, position).Show(text, color);
        }

        public static void Prewarm()
        {
            var library = FxLibrary.Current;
            if (library == null)
            {
                return;
            }
            PoolManager.Prewarm(library.explosion);
            PoolManager.Prewarm(library.hitSpark);
            PoolManager.Prewarm(library.sparkle);
            PoolManager.Prewarm(library.pulseRing);
            PoolManager.Prewarm(library.spawnPortal);
            PoolManager.Prewarm(library.floatingText);
        }

        private static void Spawn(PooledEffect prefab, Vector3 position, Color color, float scale, bool important)
        {
            if (prefab == null || !Budget(important))
            {
                return;
            }
            PoolManager.Spawn(prefab, position).Play(color, scale);
        }

        private static bool Budget(bool important)
        {
            if (frame != Time.frameCount)
            {
                frame = Time.frameCount;
                effectsThisFrame = 0;
            }
            if (!important && effectsThisFrame >= MaxEffectsPerFrame)
            {
                return false;
            }
            effectsThisFrame++;
            return true;
        }
    }
}
