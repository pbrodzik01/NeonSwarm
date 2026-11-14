using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonSwarm.EditorTools
{
    internal static class SceneFactory
    {
        public const string Folder = "Assets/Scenes";

        private static readonly Vector2 ArenaSize = new Vector2(8.4f, 13.6f);
        private const float WallThickness = 0.3f;

        public static void CreateAll(SpriteSet sprites, PrefabSet prefabs, LevelConfig[] levels)
        {
            Directory.CreateDirectory(Folder);
            currentSprites = sprites;
            currentPrefabs = prefabs;
            currentLevels = levels;
            CreateBoot();
            CreateMainMenu(sprites, levels);
            CreateSettings();
            for (var i = 0; i < levels.Length; i++)
            {
                CreateLevel(i, sprites, prefabs);
            }
            CreateGameOver();
            EditorBuildSettings.scenes = SceneNames.All.Select(name => new EditorBuildSettingsScene($"{Folder}/{name}.unity", true)).ToArray();
        }

        private static void CreateBoot()
        {
            var scene = NewScene();
            Camera(UiTheme.Current.background);
            new GameObject("Boot").AddComponent<Boot>().BuildUI();
            Save(scene, SceneNames.Boot);
        }

        private static void CreateMainMenu(SpriteSet sprites, LevelConfig[] levels)
        {
            var scene = NewScene();
            Camera(UiTheme.Current.background);
            var menu = new GameObject("MainMenu").AddComponent<MainMenu>();
            menu.BuildUI(new[] { sprites.Drifter, sprites.Chaser, sprites.Shooter, sprites.Ship });
            menu.LevelSelect.SetLevels(levels);
            EditorUtility.SetDirty(menu.LevelSelect);
            Save(scene, SceneNames.MainMenu);
        }

        private static void CreateSettings()
        {
            var scene = NewScene();
            Camera(UiTheme.Current.background);
            new GameObject("Settings").AddComponent<SettingsScreen>().BuildUI();
            Save(scene, SceneNames.Settings);
        }

        private static void CreateGameOver()
        {
            var scene = NewScene();
            Camera(UiTheme.Current.background);
            new GameObject("GameOver").AddComponent<GameOverScreen>().BuildUI();
            Save(scene, SceneNames.GameOver);
        }

        private static void CreateLevel(int index, SpriteSet sprites, PrefabSet prefabs)
        {
            var scene = NewScene();
            var sceneName = SceneNames.Levels[index];
            var config = currentLevels[index];
            var background = Color.Lerp(UiTheme.Current.background, config.accent, 0.04f);
            var camera = Camera(background);
            camera.gameObject.AddComponent<CameraRig>();

            var arenaRoot = new GameObject("Arena");
            var arena = arenaRoot.AddComponent<ArenaGenerator>();
            var floor = PrefabFactory.Renderer("Floor", arenaRoot.transform, sprites.Grid, SortingOrder.Background, Color.Lerp(Color.white, config.accent, 0.25f));
            floor.drawMode = SpriteDrawMode.Tiled;
            floor.size = new Vector2(14f, 22f);
            var walls = new GameObject("Walls").transform;
            walls.SetParent(arenaRoot.transform, false);
            var halfW = ArenaSize.x * 0.5f + WallThickness * 0.5f;
            var halfH = ArenaSize.y * 0.5f + WallThickness * 0.5f;
            Wall(walls, "Top", new Vector2(0f, halfH), new Vector2(ArenaSize.x + WallThickness * 2f, WallThickness), Vector2.up, sprites.Wall, config.accent);
            Wall(walls, "Bottom", new Vector2(0f, -halfH), new Vector2(ArenaSize.x + WallThickness * 2f, WallThickness), Vector2.down, sprites.Wall, config.accent);
            Wall(walls, "Left", new Vector2(-halfW, 0f), new Vector2(WallThickness, ArenaSize.y), Vector2.left, sprites.Wall, config.accent);
            Wall(walls, "Right", new Vector2(halfW, 0f), new Vector2(WallThickness, ArenaSize.y), Vector2.right, sprites.Wall, config.accent);
            var obstacles = new GameObject("Obstacles").transform;
            obstacles.SetParent(arenaRoot.transform, false);
            Ser.Set(arena, ("size", ArenaSize), ("obstaclePrefab", prefabs.Pillar), ("obstacleRoot", obstacles));

            var player = (GameObject)PrefabUtility.InstantiatePrefab(prefabs.Player.gameObject, scene);
            player.transform.position = new Vector3(0f, -3f, 0f);
            var playerComponent = player.GetComponent<Player>();

            var gameRoot = new GameObject("Game");
            var manager = gameRoot.AddComponent<GameManager>();
            var spawner = gameRoot.AddComponent<WaveSpawner>();
            var pickups = gameRoot.AddComponent<PickupSpawner>();
            var difficulty = gameRoot.AddComponent<DifficultyDirector>();
            gameRoot.AddComponent<ScoreManager>();
            Ser.Set(spawner, ("drifterPrefab", prefabs.Drifter), ("chaserPrefab", prefabs.Chaser), ("shooterPrefab", prefabs.Shooter));
            Ser.Set(pickups, ("coinPrefab", prefabs.Coin), ("medkitPrefab", prefabs.Medkit), ("shieldPrefab", prefabs.Shield), ("rapidPrefab", prefabs.Rapid));

            var hud = new GameObject("HUD").AddComponent<Hud>();
            hud.BuildUI();
            var pause = new GameObject("PauseMenu").AddComponent<PauseMenu>();
            pause.BuildUI();
            Ser.Set(pause, "game", manager);
            var complete = new GameObject("LevelComplete").AddComponent<LevelCompletePanel>();
            complete.BuildUI();

            Ser.Set(manager,
                ("config", config),
                ("player", playerComponent),
                ("arena", arena),
                ("spawner", spawner),
                ("pickups", pickups),
                ("difficulty", difficulty),
                ("hud", hud),
                ("pauseMenu", pause),
                ("levelCompletePanel", complete));
            Ser.Set(player.GetComponent<PlayerController>(), "joystick", hud.Joystick);
            Save(scene, sceneName);
        }

        private static void Wall(Transform parent, string name, Vector2 position, Vector2 size, Vector2 outward, Sprite sprite, Color color)
        {
            const float colliderDepth = 2f;
            var wall = new GameObject(name) { layer = Layers.Obstacle, tag = Tags.Obstacle };
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = position;
            var collider = wall.AddComponent<BoxCollider2D>();
            var horizontal = Mathf.Abs(outward.y) > 0.5f;
            collider.size = horizontal ? new Vector2(size.x + colliderDepth * 2f, colliderDepth) : new Vector2(colliderDepth, size.y + colliderDepth * 2f);
            collider.offset = outward * (colliderDepth * 0.5f - WallThickness * 0.5f);
            var renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
            renderer.color = color;
            renderer.sortingOrder = SortingOrder.Walls;
        }

        private static SpriteSet currentSprites;
        private static PrefabSet currentPrefabs;
        private static LevelConfig[] currentLevels;

        private static Scene NewScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (currentSprites != null)
            {
                AssetRefs.Refresh(currentSprites);
            }
            if (currentPrefabs != null)
            {
                AssetRefs.Refresh(currentPrefabs);
            }
            if (currentLevels != null)
            {
                AssetRefs.Refresh(currentLevels);
            }
            return scene;
        }

        private static Camera Camera(Color background)
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            go.transform.position = new Vector3(0f, 0f, -10f);
            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.3f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = background;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 50f;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            go.AddComponent<AudioListener>();
            return camera;
        }

        private static void Save(Scene scene, string name)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, $"{Folder}/{name}.unity");
        }
    }
}
