using System.IO;
using UnityEditor;
using UnityEngine;

namespace NeonSwarm.EditorTools
{
    internal sealed class SpriteSet
    {
        public Sprite Ship, ShipFlash, ThrusterSmall0, ThrusterSmall1, ThrusterBig0, ThrusterBig1, Muzzle;
        public Sprite Drifter, DrifterFlash, Chaser, ChaserFlash, Shooter, ShooterFlash, Core;
        public Sprite BulletPlayer, BulletEnemy, Coin, Medkit, ShieldIcon, Rapid, ShieldBubble;
        public Sprite Ring, Portal, Particle, Grid, Wall, Pillar;
        public Sprite UiRounded, UiOutline, UiCircle, JoystickBase, JoystickKnob;
        public Sprite Heart, HeartEmpty, Pause, Pulse, Lock, Star, Trophy;
        public Texture2D Icon;
    }

    internal static class SpriteFactory
    {
        private const string GameFolder = "Assets/Sprites/Game";
        private const string UiFolder = "Assets/Sprites/UI";
        private const string BackgroundFolder = "Assets/Sprites/Background";
        private const string IconPath = "Assets/Sprites/Icon/AppIcon.png";

        private static readonly Color Clear = new Color(1f, 1f, 1f, 0f);
        private static readonly Color White = Color.white;
        private static readonly Color Magenta = new Color(1f, 0.24f, 0.8f);
        private static readonly Color Orange = new Color(1f, 0.55f, 0.15f);
        private static readonly Color Green = new Color(0.45f, 1f, 0.35f);
        private static readonly Color Cyan = new Color(0.15f, 0.91f, 1f);
        private static readonly Color Gold = new Color(1f, 0.82f, 0.25f);

        public static SpriteSet GenerateAll()
        {
            Directory.CreateDirectory(GameFolder);
            Directory.CreateDirectory(UiFolder);
            Directory.CreateDirectory(BackgroundFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(IconPath));

            var set = new SpriteSet();
            var ship = Shapes.Polygon(new Vector2(0f, 0.92f), new Vector2(0.64f, -0.7f), new Vector2(0.22f, -0.42f), new Vector2(0f, -0.6f), new Vector2(-0.22f, -0.42f), new Vector2(-0.64f, -0.7f));
            var cockpit = Shapes.Circle(new Vector2(0f, 0.12f), 0.13f);

            set.Ship = Save(GameFolder, "ship", 128, 128, c =>
            {
                c.Draw(ship, new Color(0.32f, 0.32f, 0.32f, 1f), White, 0.12f, new Color(1f, 1f, 1f, 0.55f), 0.1f);
                c.Draw(cockpit, White);
            });
            set.ShipFlash = Save(GameFolder, "ship_flash", 128, 128, c => c.Draw(Shapes.Inflate(ship, 0.04f), White));

            set.ThrusterSmall0 = Flame("thruster_small_0", 0.55f, 0.32f);
            set.ThrusterSmall1 = Flame("thruster_small_1", 0.4f, 0.28f);
            set.ThrusterBig0 = Flame("thruster_big_0", 0.95f, 0.38f);
            set.ThrusterBig1 = Flame("thruster_big_1", 0.78f, 0.34f);
            set.Muzzle = Save(GameFolder, "muzzle", 64, 128, c =>
                c.Draw(Shapes.Polygon(Shapes.Star(4, 0.95f, 0.22f)), new Color(1f, 0.97f, 0.8f), default, 0f, new Color(1f, 0.85f, 0.4f, 0.8f), 0.12f));

            var diamond = Shapes.Polygon(new Vector2(0f, 0.82f), new Vector2(0.82f, 0f), new Vector2(0f, -0.82f), new Vector2(-0.82f, 0f));
            set.Drifter = Save(GameFolder, "enemy_drifter", 128, 128, c =>
            {
                c.Draw(diamond, Darken(Magenta), Magenta, 0.12f, WithAlpha(Magenta, 0.6f), 0.12f);
                c.Draw(Shapes.Polygon(new Vector2(0f, 0.36f), new Vector2(0.36f, 0f), new Vector2(0f, -0.36f), new Vector2(-0.36f, 0f)), Clear, Magenta, 0.06f);
            });
            set.DrifterFlash = Save(GameFolder, "enemy_drifter_flash", 128, 128, c => c.Draw(Shapes.Inflate(diamond, 0.04f), White));

            var star = Shapes.Polygon(Shapes.Star(5, 0.9f, 0.45f));
            set.Chaser = Save(GameFolder, "enemy_chaser", 128, 128, c =>
            {
                c.Draw(star, Darken(Orange), Orange, 0.11f, WithAlpha(Orange, 0.6f), 0.12f);
                c.Draw(Shapes.Polygon(new Vector2(0f, 0.35f), new Vector2(0.18f, 0.05f), new Vector2(-0.18f, 0.05f)), Orange);
            });
            set.ChaserFlash = Save(GameFolder, "enemy_chaser_flash", 128, 128, c => c.Draw(Shapes.Inflate(star, 0.04f), White));

            var hexagon = Shapes.Union(Shapes.Polygon(Shapes.Regular(6, 0.72f, 0f)), Shapes.Box(new Vector2(0f, 0.68f), new Vector2(0.13f, 0.26f), 0.05f));
            set.Shooter = Save(GameFolder, "enemy_shooter", 128, 128, c =>
            {
                c.Draw(hexagon, Darken(Green), Green, 0.11f, WithAlpha(Green, 0.6f), 0.12f);
                c.Draw(Shapes.Ring(Vector2.zero, 0.36f, 0.06f), Green);
            });
            set.ShooterFlash = Save(GameFolder, "enemy_shooter_flash", 128, 128, c => c.Draw(Shapes.Inflate(hexagon, 0.04f), White));

            set.Core = Save(GameFolder, "core", 64, 128, c => c.Draw(Shapes.Circle(Vector2.zero, 0.42f), White, default, 0f, new Color(1f, 1f, 1f, 0.7f), 0.18f));

            set.BulletPlayer = Save(GameFolder, "bullet_player", 64, 200, c =>
                c.Draw(Shapes.Capsule(new Vector2(0f, -0.45f), new Vector2(0f, 0.45f), 0.2f), new Color(0.85f, 1f, 1f), default, 0f, WithAlpha(Cyan, 0.9f), 0.14f));
            set.BulletEnemy = Save(GameFolder, "bullet_enemy", 64, 200, c =>
                c.Draw(Shapes.Circle(Vector2.zero, 0.38f), new Color(1f, 0.85f, 0.95f), default, 0f, WithAlpha(Magenta, 1f), 0.2f));

            set.Coin = Save(GameFolder, "coin", 64, 128, c =>
            {
                c.Draw(Shapes.Circle(Vector2.zero, 0.72f), Gold, new Color(1f, 0.95f, 0.6f), 0.1f, WithAlpha(Gold, 0.5f), 0.1f);
                c.Draw(Shapes.Ring(Vector2.zero, 0.4f, 0.08f), new Color(0.8f, 0.55f, 0.1f));
                c.Draw(Shapes.Box(Vector2.zero, new Vector2(0.06f, 0.28f), 0.03f), new Color(0.8f, 0.55f, 0.1f));
            });
            set.Medkit = Save(GameFolder, "medkit", 96, 150, c =>
            {
                c.Draw(Shapes.Box(Vector2.zero, new Vector2(0.72f, 0.72f), 0.2f), new Color(0.08f, 0.25f, 0.12f), Green, 0.1f, WithAlpha(Green, 0.5f), 0.1f);
                c.Draw(Shapes.Union(Shapes.Box(Vector2.zero, new Vector2(0.14f, 0.45f), 0.04f), Shapes.Box(Vector2.zero, new Vector2(0.45f, 0.14f), 0.04f)), White);
            });
            var shield = Shapes.Polygon(new Vector2(0f, 0.85f), new Vector2(0.72f, 0.58f), new Vector2(0.62f, -0.1f), new Vector2(0f, -0.88f), new Vector2(-0.62f, -0.1f), new Vector2(-0.72f, 0.58f));
            var blue = new Color(0.4f, 0.7f, 1f);
            set.ShieldIcon = Save(GameFolder, "shield", 96, 150, c => c.Draw(shield, Darken(blue), blue, 0.12f, WithAlpha(blue, 0.6f), 0.1f));
            var bolt = Shapes.Polygon(new Vector2(0.18f, 0.92f), new Vector2(-0.5f, -0.05f), new Vector2(-0.02f, -0.05f), new Vector2(-0.22f, -0.92f), new Vector2(0.5f, 0.12f), new Vector2(0.04f, 0.12f));
            set.Rapid = Save(GameFolder, "rapid", 96, 150, c => c.Draw(bolt, Gold, new Color(1f, 0.97f, 0.7f), 0.08f, WithAlpha(Gold, 0.6f), 0.12f));
            set.ShieldBubble = Save(GameFolder, "shield_bubble", 256, 200, c =>
            {
                c.Draw(Shapes.Circle(Vector2.zero, 0.86f), new Color(0.4f, 0.7f, 1f, 0.18f));
                c.Draw(Shapes.Ring(Vector2.zero, 0.86f, 0.05f), new Color(0.75f, 0.9f, 1f), default, 0f, new Color(0.4f, 0.7f, 1f, 0.7f), 0.06f);
            });
            set.Ring = Save(GameFolder, "ring", 256, 256, c => c.Draw(Shapes.Ring(Vector2.zero, 0.88f, 0.05f), White, default, 0f, new Color(1f, 1f, 1f, 0.6f), 0.06f));
            set.Portal = Save(GameFolder, "portal", 128, 128, c =>
            {
                c.Draw(Shapes.Circle(Vector2.zero, 0.75f), new Color(1f, 1f, 1f, 0.12f));
                c.Draw(Shapes.Polygon(Shapes.Star(8, 0.88f, 0.7f)), Clear, White, 0.07f, new Color(1f, 1f, 1f, 0.5f), 0.08f);
            });
            set.Particle = Save(GameFolder, "particle", 32, 64, c => c.Draw(Shapes.Circle(Vector2.zero, 0.3f), White, default, 0f, new Color(1f, 1f, 1f, 1f), 0.25f));
            set.Pillar = Save(GameFolder, "pillar", 128, 128, c =>
            {
                c.Draw(Shapes.Box(Vector2.zero, new Vector2(0.72f, 0.72f), 0.18f), new Color(0.08f, 0.12f, 0.24f), Cyan, 0.08f, WithAlpha(Cyan, 0.35f), 0.12f);
                c.Draw(Shapes.Box(Vector2.zero, new Vector2(0.32f, 0.32f), 0.08f), Clear, WithAlpha(Cyan, 0.6f), 0.05f);
            });

            set.Grid = Save(BackgroundFolder, "grid", 64, 64, c => c.Grid(new Color(0.04f, 0.06f, 0.12f), new Color(0.1f, 0.18f, 0.34f), 2.2f), wrap: TextureWrapMode.Repeat, fullRect: true);
            set.Wall = Save(GameFolder, "wall", 64, 200, c => c.Draw(Shapes.Box(Vector2.zero, new Vector2(0.8f, 0.8f), 0.4f), new Color(1f, 1f, 1f, 0.35f), White, 0.12f, new Color(1f, 1f, 1f, 0.5f), 0.1f), border: 26);

            set.UiRounded = Save(UiFolder, "ui_rounded", 64, 100, c => c.Draw(Shapes.Box(Vector2.zero, new Vector2(0.98f, 0.98f), 0.6f), White), border: 26);
            set.UiOutline = Save(UiFolder, "ui_outline", 64, 100, c => c.Draw(Shapes.Box(Vector2.zero, new Vector2(0.96f, 0.96f), 0.6f), Clear, White, 0.1f), border: 26);
            set.UiCircle = Save(UiFolder, "ui_circle", 128, 100, c => c.Draw(Shapes.Circle(Vector2.zero, 0.97f), White));
            set.JoystickBase = Save(UiFolder, "joystick_base", 256, 100, c =>
            {
                c.Draw(Shapes.Circle(Vector2.zero, 0.92f), new Color(1f, 1f, 1f, 0.12f));
                c.Draw(Shapes.Ring(Vector2.zero, 0.9f, 0.045f), new Color(1f, 1f, 1f, 0.75f));
                foreach (var angle in new[] { 0f, 90f, 180f, 270f })
                {
                    var dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                    var tip = dir * 0.78f;
                    var side = new Vector2(-dir.y, dir.x) * 0.08f;
                    c.Draw(Shapes.Polygon(tip, tip - dir * 0.12f + side, tip - dir * 0.12f - side), new Color(1f, 1f, 1f, 0.7f));
                }
            });
            set.JoystickKnob = Save(UiFolder, "joystick_knob", 128, 100, c => c.Draw(Shapes.Circle(Vector2.zero, 0.8f), new Color(1f, 1f, 1f, 0.85f), White, 0.1f, new Color(1f, 1f, 1f, 0.4f), 0.1f));

            var heart = Shapes.Union(Shapes.Circle(new Vector2(-0.38f, 0.28f), 0.42f), Shapes.Circle(new Vector2(0.38f, 0.28f), 0.42f), Shapes.Polygon(new Vector2(-0.78f, 0.16f), new Vector2(0.78f, 0.16f), new Vector2(0f, -0.82f)));
            set.Heart = Save(UiFolder, "heart", 64, 100, c => c.Draw(heart, White));
            set.HeartEmpty = Save(UiFolder, "heart_empty", 64, 100, c => c.Draw(heart, Clear, White, 0.16f));
            set.Pause = Save(UiFolder, "icon_pause", 64, 100, c => c.Draw(Shapes.Union(Shapes.Box(new Vector2(-0.3f, 0f), new Vector2(0.16f, 0.62f), 0.06f), Shapes.Box(new Vector2(0.3f, 0f), new Vector2(0.16f, 0.62f), 0.06f)), White));
            set.Pulse = Save(UiFolder, "icon_pulse", 128, 100, c =>
            {
                c.Draw(Shapes.Circle(Vector2.zero, 0.22f), White);
                c.Draw(Shapes.Ring(Vector2.zero, 0.5f, 0.1f), White);
                c.Draw(Shapes.Ring(Vector2.zero, 0.82f, 0.08f), new Color(1f, 1f, 1f, 0.7f));
            });
            set.Lock = Save(UiFolder, "icon_lock", 64, 100, c =>
            {
                c.Draw(Shapes.Box(new Vector2(0f, -0.25f), new Vector2(0.6f, 0.48f), 0.1f), White);
                c.Draw(Shapes.Subtract(Shapes.Ring(new Vector2(0f, 0.25f), 0.38f, 0.14f), Shapes.Box(new Vector2(0f, -0.3f), new Vector2(1f, 0.5f))), White);
            });
            set.Star = Save(UiFolder, "icon_star", 64, 100, c => c.Draw(Shapes.Polygon(Shapes.Star(5, 0.95f, 0.42f)), White));
            set.Trophy = Save(UiFolder, "icon_trophy", 64, 100, c =>
            {
                c.Draw(Shapes.Polygon(new Vector2(-0.6f, 0.8f), new Vector2(0.6f, 0.8f), new Vector2(0.4f, 0f), new Vector2(0.12f, -0.25f), new Vector2(-0.12f, -0.25f), new Vector2(-0.4f, 0f)), White);
                c.Draw(Shapes.Union(Shapes.Ring(new Vector2(-0.62f, 0.45f), 0.25f, 0.1f), Shapes.Ring(new Vector2(0.62f, 0.45f), 0.25f, 0.1f)), White);
                c.Draw(Shapes.Box(new Vector2(0f, -0.45f), new Vector2(0.08f, 0.22f)), White);
                c.Draw(Shapes.Box(new Vector2(0f, -0.78f), new Vector2(0.42f, 0.1f), 0.04f), White);
            });

            set.Icon = SaveIcon(ship);
            AssetDatabase.Refresh();
            return set;
        }

        private static Sprite Flame(string name, float length, float width)
        {
            return Save(GameFolder, name, 64, 128, c =>
            {
                var flame = Shapes.Polygon(new Vector2(-width, 0.85f), new Vector2(width, 0.85f), new Vector2(0f, 0.85f - length * 1.7f));
                c.Draw(Shapes.Inflate(flame, 0.06f), new Color(1f, 0.45f, 0.1f, 0.9f), default, 0f, new Color(1f, 0.4f, 0.1f, 0.7f), 0.15f);
                c.Draw(flame, new Color(1f, 0.9f, 0.45f));
            }, pivot: new Vector2(0.5f, 0.92f));
        }

        private static Texture2D SaveIcon(Sdf ship)
        {
            var canvas = new Canvas2D(512, 512, new Color(0.04f, 0.06f, 0.12f, 1f));
            canvas.Draw(Shapes.Ring(Vector2.zero, 0.86f, 0.03f), WithAlpha(Magenta, 0.8f), default, 0f, WithAlpha(Magenta, 0.5f), 0.06f);
            canvas.Draw(p => ship(p * 1.6f) / 1.6f, Darken(Cyan), Cyan, 0.05f, WithAlpha(Cyan, 0.7f), 0.08f);
            WritePng(IconPath, canvas);
            AssetDatabase.ImportAsset(IconPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(IconPath);
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = false;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        }

        private static Sprite Save(string folder, string name, int size, float pixelsPerUnit, System.Action<Canvas2D> paint, int border = 0, TextureWrapMode wrap = TextureWrapMode.Clamp, bool fullRect = false, Vector2? pivot = null)
        {
            var canvas = new Canvas2D(size, size);
            paint(canvas);
            var path = $"{folder}/{name}.png";
            WritePng(path, canvas);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = pixelsPerUnit;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = wrap;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Compressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = fullRect || border > 0 ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
            settings.spriteAlignment = pivot.HasValue ? (int)SpriteAlignment.Custom : (int)SpriteAlignment.Center;
            settings.spritePivot = pivot ?? new Vector2(0.5f, 0.5f);
            settings.spriteBorder = new Vector4(border, border, border, border);
            importer.SetTextureSettings(settings);
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = size,
                format = TextureImporterFormat.ASTC_6x6,
                compressionQuality = 50
            });
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void WritePng(string path, Canvas2D canvas)
        {
            var texture = new Texture2D(canvas.Width, canvas.Height, TextureFormat.RGBA32, false);
            texture.SetPixels(canvas.Pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        private static Color Darken(Color color)
        {
            return new Color(color.r * 0.25f, color.g * 0.25f, color.b * 0.25f, 1f);
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
