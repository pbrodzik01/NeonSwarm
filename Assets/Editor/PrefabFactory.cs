using System.IO;
using UnityEditor;
using UnityEngine;

namespace NeonSwarm.EditorTools
{
    internal sealed class PrefabSet
    {
        public Player Player;
        public DrifterEnemy Drifter;
        public ChaserEnemy Chaser;
        public ShooterEnemy Shooter;
        public Projectile PlayerBullet;
        public Projectile EnemyBullet;
        public CoinPickup Coin;
        public MedkitPickup Medkit;
        public ShieldPickup Shield;
        public RapidFirePickup Rapid;
        public Transform Pillar;
    }

    internal static class PrefabFactory
    {
        public const string Folder = "Assets/Prefabs";

        public static readonly Color DrifterColor = new Color(1f, 0.24f, 0.8f);
        public static readonly Color ChaserColor = new Color(1f, 0.55f, 0.15f);
        public static readonly Color ShooterColor = new Color(0.45f, 1f, 0.35f);

        public static PrefabSet Create(SpriteSet sprites, AnimationSet animations)
        {
            Directory.CreateDirectory(Folder + "/Actors");
            Directory.CreateDirectory(Folder + "/Projectiles");
            Directory.CreateDirectory(Folder + "/Items");
            Directory.CreateDirectory(Folder + "/Environment");

            var set = new PrefabSet();
            set.PlayerBullet = Bullet("PlayerBullet", sprites.BulletPlayer, Layers.PlayerBullet, 14f, 0.75f, 0.11f, new Color(0.5f, 1f, 1f), 30);
            set.EnemyBullet = Bullet("EnemyBullet", sprites.BulletEnemy, Layers.EnemyBullet, 6f, 3.5f, 0.12f, DrifterColor, 20);
            set.Player = Player(sprites, animations, set.PlayerBullet);
            set.Drifter = Enemy<DrifterEnemy>("Drifter", EnemyKind.Drifter, sprites.Drifter, sprites.DrifterFlash, sprites.Core, DrifterColor, animations.Drifter, 0.36f, 1, 2.1f, 10f);
            set.Chaser = Enemy<ChaserEnemy>("Chaser", EnemyKind.Chaser, sprites.Chaser, sprites.ChaserFlash, sprites.Core, ChaserColor, animations.Chaser, 0.38f, 2, 2.3f, 9f);
            set.Shooter = Enemy<ShooterEnemy>("Shooter", EnemyKind.Shooter, sprites.Shooter, sprites.ShooterFlash, sprites.Core, ShooterColor, animations.Shooter, 0.36f, 3, 1.7f, 8f, enemy => Ser.Set(enemy, "bulletPrefab", set.EnemyBullet));
            set.Coin = Pickup<CoinPickup>("Coin", Tags.Coin, sprites.Coin, new Color(1f, 0.82f, 0.25f), 9f, true, 16);
            set.Medkit = Pickup<MedkitPickup>("Medkit", Tags.Medkit, sprites.Medkit, new Color(0.45f, 1f, 0.35f), 10f, false, 3);
            set.Shield = Pickup<ShieldPickup>("ShieldPowerUp", Tags.PowerUp, sprites.ShieldIcon, new Color(0.4f, 0.7f, 1f), 10f, false, 3);
            set.Rapid = Pickup<RapidFirePickup>("RapidFirePowerUp", Tags.PowerUp, sprites.Rapid, new Color(1f, 0.82f, 0.25f), 10f, false, 3);
            set.Pillar = PillarPrefab(sprites.Pillar);
            return set;
        }

        private static Player Player(SpriteSet sprites, AnimationSet animations, Projectile bullet)
        {
            var root = new GameObject("Player") { tag = Tags.Player, layer = Layers.Player };
            var body = root.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = root.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f;
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = animations.Player;

            var visual = Child("Visual", root.transform);
            var hull = Renderer("Body", visual, sprites.Ship, SortingOrder.Player, Color.white);
            var thruster = Renderer("Thruster", visual, sprites.ThrusterSmall0, SortingOrder.Player - 1, Color.white);
            thruster.transform.localPosition = new Vector3(0f, -0.28f, 0f);
            var muzzle = Renderer("Muzzle", visual, sprites.Muzzle, SortingOrder.Player + 1, new Color(1f, 1f, 1f, 0f));
            muzzle.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            Renderer("Flash", visual, sprites.ShipFlash, SortingOrder.Player + 2, new Color(1f, 1f, 1f, 0f));
            var shield = Renderer("Shield", root.transform, sprites.ShieldBubble, SortingOrder.Player + 3, new Color(1f, 1f, 1f, 0.55f));
            shield.enabled = false;

            var player = root.AddComponent<Player>();
            Ser.Set(player, "body", hull);
            Ser.Set(root.GetComponent<PlayerController>(), ("visual", visual), ("animator", animator));
            Ser.Set(root.GetComponent<PlayerHealth>(), ("animator", animator), ("body", hull), ("shieldBubble", shield));
            Ser.Set(root.GetComponent<PlayerShooter>(), ("bulletPrefab", bullet), ("animator", animator));
            Ser.Set(root.GetComponent<PlayerPulse>(), "animator", animator);
            return Save(root, "Actors/Player").GetComponent<Player>();
        }

        private static T Enemy<T>(string name, EnemyKind kind, Sprite bodySprite, Sprite flashSprite, Sprite coreSprite, Color color, RuntimeAnimatorController controller, float radius, int hp, float speed, float acceleration, System.Action<T> extra = null) where T : EnemyBase
        {
            var root = new GameObject(name) { tag = Tags.Enemy, layer = Layers.Enemy };
            var body = root.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.None;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.mass = 0.8f;
            var collider = root.AddComponent<CircleCollider2D>();
            collider.radius = radius;
            var animator = root.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            var poolable = root.AddComponent<Poolable>();
            Ser.Set(poolable, "prewarmCount", 8);

            var visual = Child("Visual", root.transform);
            Renderer("Body", visual, bodySprite, SortingOrder.Enemies, Color.white);
            var core = Renderer("Core", visual, coreSprite, SortingOrder.Enemies + 1, Color.Lerp(color, Color.white, 0.35f));
            core.transform.localScale = Vector3.one * 0.8f;
            var flash = Renderer("Flash", visual, flashSprite, SortingOrder.Enemies + 2, new Color(1f, 1f, 1f, 0f));

            var enemy = root.AddComponent<T>();
            Ser.Set(enemy,
                ("kind", kind),
                ("maxHp", hp),
                ("moveSpeed", speed),
                ("acceleration", acceleration),
                ("contactDamage", 1),
                ("color", color),
                ("animator", animator),
                ("visual", visual),
                ("flash", flash));
            extra?.Invoke(enemy);
            return Save(root, "Actors/" + name).GetComponent<T>();
        }

        private static Projectile Bullet(string name, Sprite sprite, int layer, float speed, float lifetime, float radius, Color impact, int prewarm)
        {
            var root = new GameObject(name) { layer = layer };
            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.useFullKinematicContacts = true;
            body.gravityScale = 0f;
            var collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = radius;
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = SortingOrder.Projectiles;
            var poolable = root.AddComponent<Poolable>();
            Ser.Set(poolable, "prewarmCount", prewarm);
            var projectile = root.AddComponent<Projectile>();
            Ser.Set(projectile, ("speed", speed), ("lifetime", lifetime), ("impactColor", impact));
            return Save(root, "Projectiles/" + name).GetComponent<Projectile>();
        }

        private static T Pickup<T>(string name, string tag, Sprite sprite, Color color, float lifetime, bool magnetic, int prewarm) where T : Pickup
        {
            var root = new GameObject(name) { tag = tag, layer = Layers.Pickup };
            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            var collider = root.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;
            var visual = Child("Visual", root.transform);
            Renderer("Sprite", visual, sprite, SortingOrder.Pickups, Color.white);
            var poolable = root.AddComponent<Poolable>();
            Ser.Set(poolable, "prewarmCount", prewarm);
            var pickup = root.AddComponent<T>();
            Ser.Set(pickup, ("color", color), ("visual", visual), ("lifetime", lifetime), ("magnetic", magnetic));
            return Save(root, "Items/" + name).GetComponent<T>();
        }

        private static Transform PillarPrefab(Sprite sprite)
        {
            var root = new GameObject("Pillar") { tag = Tags.Obstacle, layer = Layers.Obstacle };
            var collider = root.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.72f, 0.72f);
            var renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = SortingOrder.Obstacles;
            return Save(root, "Environment/Pillar").transform;
        }

        internal static Transform Child(string name, Transform parent)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.gameObject.layer = parent.gameObject.layer;
            return child;
        }

        internal static SpriteRenderer Renderer(string name, Transform parent, Sprite sprite, int order, Color color)
        {
            var child = Child(name, parent);
            var renderer = child.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            renderer.color = color;
            return renderer;
        }

        internal static GameObject Save(GameObject root, string relativePath)
        {
            var path = $"{Folder}/{relativePath}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }
    }
}
