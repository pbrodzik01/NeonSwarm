using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NeonSwarm.Tests
{
    public class GameFlowTests
    {
        private string savePath;

        [SetUp]
        public void SetUp()
        {
            savePath = Path.Combine(Path.GetTempPath(), $"neonswarm-play-{Guid.NewGuid():N}.json");
            SaveSystem.UsePath(savePath);
            SaveSystem.Load();
            Time.timeScale = 1f;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            SaveSystem.UsePath(null);
            foreach (var path in new[] { savePath, savePath + ".bak", savePath + ".tmp" })
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [UnityTest]
        public IEnumerator Boot_LoadsMainMenu()
        {
            SceneManager.LoadScene(SceneNames.Boot);
            yield return WaitFor(() => SceneManager.GetActiveScene().name == SceneNames.MainMenu, 10f);
            Assert.AreEqual(SceneNames.MainMenu, SceneManager.GetActiveScene().name);
            yield return new WaitForSeconds(0.5f);
            var menu = UnityEngine.Object.FindObjectOfType<MainMenu>();
            Assert.IsNotNull(menu);
            menu.LevelSelect.Show();
            yield return null;
            Assert.IsTrue(menu.LevelSelect.IsOpen);
        }

        [UnityTest]
        public IEnumerator SettingsScene_Loads()
        {
            SceneManager.LoadScene(SceneNames.Settings);
            yield return new WaitForSeconds(0.5f);
            var panel = UnityEngine.Object.FindObjectOfType<SettingsPanel>();
            Assert.IsNotNull(panel);
            Assert.IsTrue(panel.IsOpen);
        }

        [UnityTest]
        public IEnumerator Level1_StartsSpawnsEnemiesAndPauses()
        {
            GameSession.StartRun(0);
            yield return LoadLevelAndWaitForPlay(SceneNames.Level1);
            Assert.AreEqual(GameState.Playing, GameManager.Instance.State);
            Assert.AreEqual(3, Player.Instance.Health.Current);
            yield return WaitFor(() => EnemyRegistry.Count > 0, 8f);
            Assert.Greater(EnemyRegistry.Count, 0);

            GameManager.Instance.Pause();
            Assert.AreEqual(GameState.Paused, GameManager.Instance.State);
            Assert.AreEqual(0f, Time.timeScale);
            GameManager.Instance.Resume();
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator Player_DiesAndGameOverSceneLoads()
        {
            GameSession.StartRun(0);
            yield return LoadLevelAndWaitForPlay(SceneNames.Level1);
            Player.Instance.Health.TakeDamage(1, Vector2.zero);
            Assert.AreEqual(2, Player.Instance.Health.Current);
            Player.Instance.Health.TakeDamage(5, Vector2.zero);
            Assert.AreEqual(2, Player.Instance.Health.Current, "invulnerability frames should block the second hit");
            yield return new WaitForSeconds(1.3f);
            Player.Instance.Health.TakeDamage(5, Vector2.zero);
            Assert.IsFalse(Player.Instance.Health.IsAlive);
            yield return WaitFor(() => SceneManager.GetActiveScene().name == SceneNames.GameOver, 6f);
            Assert.AreEqual(SceneNames.GameOver, SceneManager.GetActiveScene().name);
            Assert.AreEqual(RunOutcome.Defeat, GameSession.Outcome);
            Assert.AreEqual(1, SaveSystem.Data.stats.gamesPlayed);
        }

        [UnityTest]
        public IEnumerator Level1_ClearingAllWavesUnlocksLevel2()
        {
            GameSession.StartRun(0);
            yield return LoadLevelAndWaitForPlay(SceneNames.Level1);
            Player.Instance.Health.ActivateShield(999f);
            var timeout = Time.realtimeSinceStartup + 90f;
            Time.timeScale = 3f;
            while (GameManager.Instance.State == GameState.Playing && Time.realtimeSinceStartup < timeout)
            {
                EnemyRegistry.ForEach(enemy => enemy.TakeDamage(10, Vector2.zero));
                yield return null;
            }
            Time.timeScale = 1f;
            Assert.AreEqual(GameState.LevelComplete, GameManager.Instance.State);
            Assert.AreEqual(2, SaveSystem.Data.unlockedLevels);
            Assert.GreaterOrEqual(GameSession.Score, ScoreRules.LevelComplete);
            Assert.Greater(SaveSystem.Data.stats.totalKills, 0);
        }

        [UnityTest]
        public IEnumerator Enemies_StayInsideArenaAfterStrongKnockback()
        {
            GameSession.StartRun(0);
            yield return LoadLevelAndWaitForPlay(SceneNames.Level1);
            Player.Instance.Health.ActivateShield(999f);
            yield return WaitFor(() => EnemyRegistry.Count > 0, 8f);
            for (var i = 0; i < 10; i++)
            {
                var push = i % 2 == 0 ? Vector2.up * 40f : Vector2.right * 40f;
                EnemyRegistry.ForEach(enemy => enemy.TakeDamage(0, push));
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(0.6f);
            var outside = 0;
            var bounds = ArenaGenerator.Bounds;
            EnemyRegistry.ForEach(enemy =>
            {
                if (!bounds.Contains(enemy.transform.position))
                {
                    outside++;
                }
            });
            Assert.AreEqual(0, outside);
        }

        private static IEnumerator LoadLevelAndWaitForPlay(string sceneName)
        {
            var previous = GameManager.Instance;
            SceneManager.LoadScene(sceneName);
            yield return WaitFor(() => GameManager.Instance != null && GameManager.Instance != previous, 10f);
            yield return WaitFor(() => GameManager.Instance.State == GameState.Playing, 10f);
            Assert.AreEqual(GameState.Playing, GameManager.Instance.State);
        }

        private static IEnumerator WaitFor(Func<bool> condition, float seconds)
        {
            var end = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < end)
            {
                yield return null;
            }
        }
    }
}
