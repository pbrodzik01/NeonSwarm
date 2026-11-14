using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NeonSwarm.Tests
{
    public class WaveStallDiagnostics
    {
        [UnityTest]
        [Timeout(240000)]
        public IEnumerator Level2_IdlePlayerWavesKeepProgressing()
        {
            var savePath = Path.Combine(Path.GetTempPath(), "neonswarm-stall.json");
            SaveSystem.UsePath(savePath);
            SaveSystem.Load();
            GameSession.StartRun(1);
            SceneManager.LoadScene(SceneNames.Level2);
            var end = Time.realtimeSinceStartup + 10f;
            while ((GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) && Time.realtimeSinceStartup < end)
            {
                yield return null;
            }
            Player.Instance.Health.ActivateShield(9999f);
            Time.timeScale = 4f;

            var lastProgress = Time.time;
            var lastRemaining = -1;
            var lastWave = -1;
            var deadline = Time.realtimeSinceStartup + 150f;
            var stalled = false;
            while (GameManager.Instance.State == GameState.Playing && Time.realtimeSinceStartup < deadline)
            {
                var spawner = GameManager.Instance.Spawner;
                if (spawner.RemainingInWave != lastRemaining || spawner.CurrentWave != lastWave)
                {
                    lastRemaining = spawner.RemainingInWave;
                    lastWave = spawner.CurrentWave;
                    lastProgress = Time.time;
                }
                if (Time.time - lastProgress > 90f)
                {
                    stalled = true;
                    break;
                }
                yield return null;
            }
            Time.timeScale = 1f;

            var spawnerState = GameManager.Instance.Spawner;
            var report = $"state={GameManager.Instance.State} wave={spawnerState.CurrentWave} remaining={spawnerState.RemainingInWave} pending={spawnerState.PendingSpawns} registry={EnemyRegistry.Count} t={Time.time:0}";
            EnemyRegistry.ForEach(enemy =>
            {
                var animator = enemy.GetComponent<Animator>();
                var info = animator.GetCurrentAnimatorStateInfo(0);
                var visual = enemy.transform.Find("Visual");
                report += $"\n  {enemy.name} kind={enemy.Kind} alive={enemy.IsAlive} active={enemy.gameObject.activeInHierarchy} pos={enemy.transform.position} vel={enemy.GetComponent<Rigidbody2D>().velocity} visualScale={visual.localScale} dead={info.IsName("Dead")} idle={info.IsName("Idle")} run={info.IsName("Run")} hurt={info.IsName("Hurt")} attack={info.IsName("Attack")} deadParam={animator.GetBool("Dead")}";
            });
            Debug.Log("[StallReport] " + report);
            SaveSystem.UsePath(null);
            Assert.IsFalse(stalled, report);
        }
    }
}
