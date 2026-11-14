using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NeonSwarm.EditorTools
{
    internal static class AudioFactory
    {
        private const int Rate = 22050;
        private const string SfxFolder = "Assets/Audio/SFX";
        private const string MusicFolder = "Assets/Audio/Music";

        private static System.Random noise = new System.Random(7);

        public static Dictionary<SfxId, AudioClip> GenerateSfx()
        {
            Directory.CreateDirectory(SfxFolder);
            noise = new System.Random(7);
            var clips = new Dictionary<SfxId, AudioClip>
            {
                [SfxId.Click] = Sfx("click", Render(0.06f, t => Sine(1400f, t) * Decay(t, 0.02f) * 0.5f)),
                [SfxId.Shoot] = Sfx("shoot", Render(0.09f, t => Square(Sweep(1100f, 450f, t, 0.09f), t) * Decay(t, 0.04f) * 0.35f)),
                [SfxId.EnemyShoot] = Sfx("enemy_shoot", Render(0.16f, t => Saw(Sweep(520f, 220f, t, 0.16f), t) * Decay(t, 0.07f) * 0.4f)),
                [SfxId.Hit] = Sfx("hit", Render(0.1f, t => (Noise() * 0.6f + Sine(180f, t) * 0.6f) * Decay(t, 0.03f) * 0.6f)),
                [SfxId.Explosion] = Sfx("explosion", LowPass(Render(0.5f, t => (Noise() * 0.9f + Sine(Sweep(140f, 40f, t, 0.5f), t) * 0.8f) * Decay(t, 0.16f)), 0.25f)),
                [SfxId.Coin] = Sfx("coin", Render(0.18f, t => Square(t < 0.06f ? 988f : 1319f, t) * Decay(t, 0.09f) * 0.3f)),
                [SfxId.PowerUp] = Sfx("powerup", Render(0.36f, t => Square(Arp(t, 0.06f, 523f, 659f, 784f, 1047f, 1319f, 1568f), t) * Decay(t, 0.25f) * 0.28f)),
                [SfxId.Heal] = Sfx("heal", Render(0.4f, t => (Sine(Sweep(440f, 880f, t, 0.4f), t) + Sine(Sweep(660f, 1320f, t, 0.4f), t) * 0.4f) * Envelope(t, 0.02f, 0.4f) * 0.4f)),
                [SfxId.PlayerHurt] = Sfx("player_hurt", Render(0.32f, t => (Saw(Sweep(420f, 90f, t, 0.32f), t) * 0.6f + Noise() * 0.3f) * Decay(t, 0.14f) * 0.55f)),
                [SfxId.Pulse] = Sfx("pulse", LowPass(Render(0.6f, t => (Sine(Sweep(260f, 50f, t, 0.6f), t) * 0.9f + Noise() * 0.5f * Decay(t, 0.1f)) * Envelope(t, 0.01f, 0.6f)), 0.35f)),
                [SfxId.Win] = Sfx("win", Render(1.3f, t => Melody(t, 0.13f, new[] { 523f, 659f, 784f, 1047f, 0f, 784f, 1047f, 1047f, 1047f, 1047f }) * 0.3f)),
                [SfxId.Lose] = Sfx("lose", Render(1.4f, t => Melody(t, 0.22f, new[] { 659f, 587f, 523f, 466f, 392f, 392f }, saw: true) * 0.3f)),
                [SfxId.Countdown] = Sfx("countdown", Render(0.14f, t => Square(660f, t) * Decay(t, 0.06f) * 0.3f)),
                [SfxId.Go] = Sfx("go", Render(0.35f, t => (Square(990f, t) + Square(1320f, t) * 0.5f) * Decay(t, 0.15f) * 0.25f)),
                [SfxId.Spawn] = Sfx("spawn", Render(0.4f, t => Sine(Sweep(200f, 900f, t, 0.4f), t) * Envelope(t, 0.15f, 0.4f) * 0.25f))
            };
            return clips;
        }

        public static Dictionary<MusicId, AudioClip> GenerateMusic()
        {
            Directory.CreateDirectory(MusicFolder);
            return new Dictionary<MusicId, AudioClip>
            {
                [MusicId.Gameplay] = Music("music_gameplay", RenderSong(128f, true)),
                [MusicId.Menu] = Music("music_menu", RenderSong(100f, false))
            };
        }

        private static float[] RenderSong(float bpm, bool energetic)
        {
            var beat = 60f / bpm;
            const int bars = 8;
            var length = bars * 4 * beat;
            var samples = new float[(int)(length * Rate)];
            var roots = new[] { 110f, 87.31f, 130.81f, 98f };
            var chords = new[]
            {
                new[] { 220f, 261.63f, 329.63f },
                new[] { 174.61f, 220f, 261.63f },
                new[] { 261.63f, 329.63f, 392f },
                new[] { 196f, 246.94f, 293.66f }
            };
            var bass = 0f;
            var hatLow = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                var t = (float)i / Rate;
                var beatPos = t / beat;
                var bar = (int)(beatPos / 4f) % bars;
                var chord = bar / 2 % 4;
                var inBeat = beatPos - Mathf.Floor(beatPos);
                var eighth = beatPos * 2f - Mathf.Floor(beatPos * 2f);
                var sixteenth = beatPos * 4f - Mathf.Floor(beatPos * 4f);
                var step = (int)(beatPos * 4f);

                var kickTime = inBeat * beat;
                var kick = Sine(Mathf.Lerp(150f, 45f, Mathf.Clamp01(kickTime / 0.12f)), kickTime) * Decay(kickTime, 0.09f);
                if (!energetic && ((int)beatPos % 2 == 1))
                {
                    kick = 0f;
                }

                var bassFreq = roots[chord] * (energetic && (int)(beatPos * 2f) % 4 == 3 ? 2f : 1f);
                var bassRaw = Saw(bassFreq, t) * Decay(eighth * beat * 0.5f, energetic ? 0.12f : 0.3f);
                bass += (bassRaw - bass) * 0.08f;

                var tones = chords[chord];
                var arpNote = tones[step % 3] * (step % 6 < 3 ? 2f : 4f);
                var arp = Square(arpNote, t) * Decay(sixteenth * beat * 0.25f, 0.05f) * (energetic ? 0.12f : 0.08f);

                var pad = 0f;
                foreach (var tone in tones)
                {
                    pad += Saw(tone, t) * 0.33f + Saw(tone * 1.005f, t) * 0.33f;
                }
                pad *= energetic ? 0.05f : 0.09f;

                var hatNoise = Noise();
                hatLow += (hatNoise - hatLow) * 0.3f;
                var hat = (hatNoise - hatLow) * Decay(eighth * beat * 0.5f, 0.02f) * (eighth > 0.5f || !energetic ? 0.18f : 0.08f);

                var snare = 0f;
                if (energetic && ((int)beatPos % 4 == 1 || (int)beatPos % 4 == 3))
                {
                    snare = (Noise() * 0.6f + Sine(190f, kickTime) * 0.4f) * Decay(kickTime, 0.07f) * 0.5f;
                }

                samples[i] = kick * 0.85f + bass * (energetic ? 0.6f : 0.4f) + arp + pad + hat + snare;
            }
            Normalize(samples, 0.85f);
            return samples;
        }

        private static float Melody(float t, float noteLength, float[] notes, bool saw = false)
        {
            var index = Mathf.Min((int)(t / noteLength), notes.Length - 1);
            var local = t - index * noteLength;
            var freq = notes[index];
            if (freq <= 0f)
            {
                return 0f;
            }
            var tone = saw ? Saw(freq, t) * 0.6f + Sine(freq * 0.5f, t) * 0.4f : Square(freq, t) * 0.6f + Sine(freq * 2f, t) * 0.2f;
            var tail = index == notes.Length - 1 ? Decay(local, 0.35f) : Envelope(local, 0.005f, noteLength);
            return tone * tail;
        }

        private static float Arp(float t, float step, params float[] notes)
        {
            return notes[Mathf.Min((int)(t / step), notes.Length - 1)];
        }

        private static float[] Render(float seconds, Func<float, float> generator)
        {
            var samples = new float[(int)(seconds * Rate)];
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] = generator((float)i / Rate);
            }
            var fade = Mathf.Min(64, samples.Length);
            for (var i = 0; i < fade; i++)
            {
                samples[samples.Length - 1 - i] *= (float)i / fade;
            }
            return samples;
        }

        private static float[] LowPass(float[] samples, float amount)
        {
            var state = 0f;
            for (var i = 0; i < samples.Length; i++)
            {
                state += (samples[i] - state) * amount;
                samples[i] = state;
            }
            return samples;
        }

        private static void Normalize(float[] samples, float peak)
        {
            var max = 0.0001f;
            foreach (var sample in samples)
            {
                max = Mathf.Max(max, Mathf.Abs(sample));
            }
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] = (float)Math.Tanh(samples[i] / max * 1.2f) * peak;
            }
        }

        private static float Sine(float freq, float t) => Mathf.Sin(2f * Mathf.PI * freq * t);
        private static float Square(float freq, float t) => Mathf.Repeat(freq * t, 1f) < 0.5f ? 1f : -1f;
        private static float Saw(float freq, float t) => Mathf.Repeat(freq * t, 1f) * 2f - 1f;
        private static float Noise() => (float)(noise.NextDouble() * 2.0 - 1.0);
        private static float Decay(float t, float tau) => Mathf.Exp(-t / tau);
        private static float Sweep(float from, float to, float t, float duration) => Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));

        private static float Envelope(float t, float attack, float length)
        {
            var a = Mathf.Clamp01(t / attack);
            var r = Mathf.Clamp01((length - t) / (length * 0.4f));
            return a * r;
        }

        private static AudioClip Sfx(string name, float[] samples)
        {
            var path = $"{SfxFolder}/{name}.wav";
            WriteWav(path, samples);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono = true;
            importer.defaultSampleSettings = new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.DecompressOnLoad,
                compressionFormat = AudioCompressionFormat.ADPCM,
                quality = 1f,
                sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate
            };
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private static AudioClip Music(string name, float[] samples)
        {
            var path = $"{MusicFolder}/{name}.wav";
            WriteWav(path, samples);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            importer.forceToMono = true;
            importer.loadInBackground = true;
            importer.defaultSampleSettings = new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.Streaming,
                compressionFormat = AudioCompressionFormat.Vorbis,
                quality = 0.45f,
                sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate
            };
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private static void WriteWav(string path, float[] samples)
        {
            using var stream = new FileStream(path, FileMode.Create);
            using var writer = new BinaryWriter(stream);
            var dataSize = samples.Length * 2;
            writer.Write(new[] { 'R', 'I', 'F', 'F' });
            writer.Write(36 + dataSize);
            writer.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(Rate);
            writer.Write(Rate * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            writer.Write(new[] { 'd', 'a', 't', 'a' });
            writer.Write(dataSize);
            foreach (var sample in samples)
            {
                writer.Write((short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));
            }
        }
    }
}
