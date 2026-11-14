using UnityEngine;

namespace NeonSwarm
{
    public static class SceneNames
    {
        public const string Boot = "Boot";
        public const string MainMenu = "MainMenu";
        public const string Settings = "Settings";
        public const string GameOver = "GameOver";
        public const string Level1 = "Level1";
        public const string Level2 = "Level2";
        public const string Level3 = "Level3";

        public static readonly string[] Levels = { Level1, Level2, Level3 };
        public static readonly string[] All = { Boot, MainMenu, Settings, Level1, Level2, Level3, GameOver };

        public static int LevelCount => Levels.Length;

        public static string ForLevel(int levelIndex)
        {
            return Levels[Mathf.Clamp(levelIndex, 0, Levels.Length - 1)];
        }
    }

    public static class Tags
    {
        public const string Player = "Player";
        public const string Enemy = "Enemy";
        public const string Coin = "Coin";
        public const string Medkit = "Medkit";
        public const string PowerUp = "PowerUp";
        public const string Obstacle = "Obstacle";

        public static readonly string[] Custom = { Enemy, Coin, Medkit, PowerUp, Obstacle };
    }

    public static class Layers
    {
        public const int Player = 8;
        public const int Enemy = 9;
        public const int PlayerBullet = 10;
        public const int EnemyBullet = 11;
        public const int Pickup = 12;
        public const int Obstacle = 13;

        public static readonly int[] CustomIndices = { Player, Enemy, PlayerBullet, EnemyBullet, Pickup, Obstacle };
        public static readonly string[] CustomNames = { "Player", "Enemy", "PlayerBullet", "EnemyBullet", "Pickup", "Obstacle" };

        public static int Mask(params int[] layers)
        {
            var mask = 0;
            foreach (var layer in layers)
            {
                mask |= 1 << layer;
            }
            return mask;
        }
    }

    public static class SortingOrder
    {
        public const int Background = -100;
        public const int Walls = -20;
        public const int Obstacles = -10;
        public const int Portals = -5;
        public const int Pickups = 0;
        public const int Enemies = 10;
        public const int Player = 20;
        public const int Projectiles = 30;
        public const int Effects = 40;
        public const int FloatingText = 50;
    }

    public static class PhysicsSetup
    {
        public static void Apply()
        {
            var custom = Layers.CustomIndices;
            for (var i = 0; i < custom.Length; i++)
            {
                for (var j = 0; j < 32; j++)
                {
                    Physics2D.IgnoreLayerCollision(custom[i], j, true);
                }
            }

            Allow(Layers.Player, Layers.Enemy);
            Allow(Layers.Player, Layers.EnemyBullet);
            Allow(Layers.Player, Layers.Pickup);
            Allow(Layers.Player, Layers.Obstacle);
            Allow(Layers.Enemy, Layers.Enemy);
            Allow(Layers.Enemy, Layers.PlayerBullet);
            Allow(Layers.Enemy, Layers.Obstacle);
            Allow(Layers.PlayerBullet, Layers.Obstacle);
            Allow(Layers.EnemyBullet, Layers.Obstacle);
        }

        private static void Allow(int a, int b)
        {
            Physics2D.IgnoreLayerCollision(a, b, false);
        }
    }
}
