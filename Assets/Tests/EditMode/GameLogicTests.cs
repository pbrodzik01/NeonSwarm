using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace NeonSwarm.Tests
{
    public class GameLogicTests
    {
        private string tempSave;

        [SetUp]
        public void SetUp()
        {
            tempSave = Path.Combine(Path.GetTempPath(), $"neonswarm-test-{System.Guid.NewGuid():N}.json");
            SaveSystem.UsePath(tempSave);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var path in new[] { tempSave, tempSave + ".bak", tempSave + ".tmp" })
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            SaveSystem.UsePath(null);
        }

        [Test]
        public void SaveSystem_RoundTripsProgressAndSettings()
        {
            var data = SaveSystem.Load();
            data.bestScore = 2500;
            data.unlockedLevels = 3;
            data.coins = 125;
            data.settings.music = false;
            data.settings.volume = 0.3f;
            SaveSystem.Save();

            var loaded = SaveSystem.Load();
            Assert.AreEqual(2500, loaded.bestScore);
            Assert.AreEqual(3, loaded.unlockedLevels);
            Assert.AreEqual(125, loaded.coins);
            Assert.IsFalse(loaded.settings.music);
            Assert.AreEqual(0.3f, loaded.settings.volume, 0.0001f);
        }

        [Test]
        public void SaveSystem_RecoversFromCorruptedFileUsingDefaults()
        {
            File.WriteAllText(tempSave, "{ this is not json");
            var loaded = SaveSystem.Load();
            Assert.AreEqual(1, loaded.unlockedLevels);
            Assert.AreEqual(0, loaded.bestScore);
            Assert.AreEqual(MissionService.ActiveCount, loaded.missions.Count);
        }

        [Test]
        public void Leaderboard_KeepsTopTenSortedDescending()
        {
            var save = new SaveData();
            for (var i = 1; i <= 12; i++)
            {
                Leaderboard.Submit(save, "P" + i, i * 100, 1, "2026-11-14");
            }
            Assert.AreEqual(Leaderboard.MaxEntries, save.leaderboard.Count);
            Assert.AreEqual(1200, save.leaderboard[0].score);
            Assert.AreEqual(300, save.leaderboard[9].score);
            Assert.AreEqual(-1, Leaderboard.Submit(save, "Low", 50, 1, "x"));
            Assert.AreEqual(0, Leaderboard.Submit(save, "Top", 5000, 3, "x"));
        }

        [Test]
        public void Shop_BuyingDeductsCoinsAndRespectsMaxLevel()
        {
            var save = new SaveData { coins = 1000 };
            Assert.AreEqual(PurchaseResult.Purchased, ShopService.TryBuy(save, ShopCatalog.MagnetId));
            Assert.AreEqual(750, save.coins);
            Assert.AreEqual(PurchaseResult.MaxLevel, ShopService.TryBuy(save, ShopCatalog.MagnetId));
            Assert.AreEqual(PurchaseResult.Purchased, ShopService.TryBuy(save, ShopCatalog.ArmorId));
            Assert.AreEqual(1, ShopService.BuildLoadout(save).BonusHp);
        }

        [Test]
        public void Shop_RefusesPurchaseWithoutCoins()
        {
            var save = new SaveData { coins = 10 };
            Assert.AreEqual(PurchaseResult.NotEnoughCoins, ShopService.TryBuy(save, "skin_royal"));
            Assert.AreEqual(10, save.coins);
        }

        [Test]
        public void Missions_CompletingGrantsRewardAndRefillsSlots()
        {
            var save = new SaveData();
            MissionService.EnsureActiveMissions(save);
            Assert.AreEqual(MissionService.ActiveCount, save.missions.Count);
            var completed = MissionService.Report(save, MissionStat.Kills, 20);
            Assert.AreEqual(1, completed.Count);
            Assert.AreEqual(50, save.coins);
            Assert.AreEqual(MissionService.ActiveCount, save.missions.Count);
        }

        [Test]
        public void Achievements_UnlockOnce()
        {
            var save = new SaveData();
            save.stats.totalKills = 1;
            Assert.AreEqual(1, AchievementService.Evaluate(save).Count);
            Assert.AreEqual(0, AchievementService.Evaluate(save).Count);
            Assert.IsTrue(AchievementService.IsUnlocked(save, "a_first_blood"));
        }

        [Test]
        public void ScoreRules_TimeBonusRewardsFasterClears()
        {
            Assert.AreEqual(0, ScoreRules.TimeBonus(60f, 75f));
            Assert.AreEqual(50, ScoreRules.TimeBonus(60f, 55f));
            Assert.AreEqual(150, ScoreRules.TimeBonus(60f, 35f));
            Assert.AreEqual(200, ScoreRules.ForEnemy(EnemyKind.Shooter));
        }

        [Test]
        public void WavePlanner_IsDeterministicForSeedAndRespectsUnlockWave()
        {
            var weights = new[]
            {
                new EnemySpawnWeight { kind = EnemyKind.Drifter, weight = 1, fromWave = 1 },
                new EnemySpawnWeight { kind = EnemyKind.Shooter, weight = 5, fromWave = 3 }
            };
            var first = WavePlanner.PlanWave(weights, 0, 8, new System.Random(42));
            var again = WavePlanner.PlanWave(weights, 0, 8, new System.Random(42));
            CollectionAssert.AreEqual(first, again);
            CollectionAssert.DoesNotContain(first, EnemyKind.Shooter);
            var third = WavePlanner.PlanWave(weights, 2, 8, new System.Random(1));
            CollectionAssert.Contains(third, EnemyKind.Shooter);
        }

        [Test]
        public void DifficultyModel_StaysWithinBounds()
        {
            var model = new DifficultyModel();
            for (var i = 0; i < 20; i++)
            {
                model.Evaluate(1f, 60f, 100f);
            }
            Assert.AreEqual(DifficultyModel.Max, model.Intensity, 0.0001f);
            for (var i = 0; i < 20; i++)
            {
                model.Evaluate(0.2f, 0f, 1f);
            }
            Assert.AreEqual(DifficultyModel.Min, model.Intensity, 0.0001f);
        }

        [Test]
        public void FinishRun_FlagsNewRecordEvenAfterLevelEndUpdatedBestScore()
        {
            SaveSystem.Load().bestScore = 1000;
            GameSession.StartRun(0);
            GameSession.AddScore(1500);
            ProgressTracker.RecordLevelScore(0, 1500);
            Assert.AreEqual(1500, SaveSystem.Data.bestScore);
            ProgressTracker.FinishRun(RunOutcome.Defeat);
            Assert.IsTrue(GameSession.NewRecord);
            Assert.AreEqual(1, SaveSystem.Data.leaderboard.Count);
        }

        [Test]
        public void GameSession_RestartRestoresScoreFromLevelStart()
        {
            GameSession.StartRun(0);
            GameSession.AddScore(700);
            GameSession.BeginLevel(1);
            GameSession.AddScore(300);
            GameSession.RestartLevel();
            Assert.AreEqual(700, GameSession.Score);
            Assert.AreEqual(1, GameSession.LevelIndex);
        }
    }
}
