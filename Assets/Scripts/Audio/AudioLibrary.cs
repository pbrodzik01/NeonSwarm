using System;
using UnityEngine;

namespace NeonSwarm
{
    public enum SfxId
    {
        Click,
        Shoot,
        EnemyShoot,
        Hit,
        Explosion,
        Coin,
        PowerUp,
        Heal,
        PlayerHurt,
        Pulse,
        Win,
        Lose,
        Countdown,
        Go,
        Spawn
    }

    public enum MusicId
    {
        None,
        Menu,
        Gameplay
    }

    [CreateAssetMenu(menuName = "Neon Swarm/Audio Library", fileName = "AudioLibrary")]
    public class AudioLibrary : ScriptableObject
    {
        [Serializable]
        public class SfxEntry
        {
            public SfxId id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 1f;
            [Range(0f, 0.3f)] public float pitchVariance = 0.05f;
            public float minInterval = 0.03f;
        }

        [Serializable]
        public class MusicEntry
        {
            public MusicId id;
            public AudioClip clip;
            [Range(0f, 1f)] public float volume = 0.6f;
        }

        public SfxEntry[] sfx = Array.Empty<SfxEntry>();
        public MusicEntry[] music = Array.Empty<MusicEntry>();

        public SfxEntry Find(SfxId id)
        {
            foreach (var entry in sfx)
            {
                if (entry.id == id)
                {
                    return entry;
                }
            }
            return null;
        }

        public MusicEntry Find(MusicId id)
        {
            foreach (var entry in music)
            {
                if (entry.id == id)
                {
                    return entry;
                }
            }
            return null;
        }
    }
}
