using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NeonSwarm.Tests
{
    public class PoolingVisualTests
    {
        [UnityTest]
        public IEnumerator ReusedEnemyFromPool_HasFullSizeVisualAndIsNotDead()
        {
            GameSession.StartRun(0);
            SceneManager.LoadScene(SceneNames.Level1);
            yield return null;
            yield return null;
            var spawner = Object.FindObjectOfType<WaveSpawner>();
            var prefabField = typeof(WaveSpawner).GetField("drifterPrefab", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var prefab = (DrifterEnemy)prefabField.GetValue(spawner);
            var player = Player.Instance.transform;

            for (var cycle = 0; cycle < 3; cycle++)
            {
                var enemy = PoolManager.Spawn(prefab, new Vector3(0f, 4f, 0f));
                enemy.Init(player, 1f, 1f, 0f);
                yield return new WaitForSeconds(0.3f);
                var visual = enemy.transform.Find("Visual");
                var animator = enemy.GetComponent<Animator>();
                Assert.Greater(visual.localScale.x, 0.7f, $"cycle {cycle}: visual scale {visual.localScale}");
                Assert.IsFalse(animator.GetCurrentAnimatorStateInfo(0).IsName("Dead"), $"cycle {cycle}: animator in Dead state");
                enemy.TakeDamage(99, Vector2.zero);
                yield return new WaitForSeconds(0.6f);
                Assert.IsFalse(enemy.gameObject.activeSelf, $"cycle {cycle}: enemy should be back in the pool");
            }
        }
    }
}
