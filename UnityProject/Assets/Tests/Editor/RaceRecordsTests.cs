using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace VectorRush.Tests
{
    public sealed class RaceRecordsTests
    {
        string directory;

        [SetUp]
        public void SetUp() => directory = Path.Combine(Path.GetTempPath(), "VectorRush-Records-" + Guid.NewGuid().ToString("N"));

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [Test]
        public void ManualResultsCreatePreserveAndReplaceBestTimesWithSignedDelta()
        {
            var records = new RaceRecords(directory);
            RaceRecordComparison first = records.Submit("course-a", "rules-v1", 31f, 95f, false);
            Assert.That(first.HadPreviousRaceBest, Is.False);
            Assert.That(first.UpdatedRaceBest, Is.True);
            Assert.That(records.GetBest("course-a", "rules-v1").BestRace, Is.EqualTo(95f));

            RaceRecordComparison slower = records.Submit("course-a", "rules-v1", 33f, 99f, false);
            Assert.That(slower.PreviousBestRace, Is.EqualTo(95f));
            Assert.That(slower.RaceDelta, Is.EqualTo(4f));
            Assert.That(slower.UpdatedRaceBest, Is.False);
            Assert.That(records.GetBest("course-a", "rules-v1").BestRace, Is.EqualTo(95f));

            RaceRecordComparison faster = records.Submit("course-a", "rules-v1", 29f, 89f, false);
            Assert.That(faster.PreviousBestRace, Is.EqualTo(95f));
            Assert.That(faster.RaceDelta, Is.EqualTo(-6f));
            Assert.That(faster.UpdatedRaceBest, Is.True);
            Assert.That(records.GetBest("course-a", "rules-v1").BestRace, Is.EqualTo(89f));
            Assert.That(records.GetBest("course-a", "rules-v1").BestLap, Is.EqualTo(29f));
        }

        [Test]
        public void CourseAndRulesIdentityAreIsolatedAndAutomatedRunIsExcluded()
        {
            var records = new RaceRecords(directory);
            records.Submit("course-a", "rules-v1", 30f, 90f, false);
            records.Submit("course-a", "rules-v2", 20f, 60f, false);
            records.Submit("course-b", "rules-v1", 25f, 75f, false);
            RaceRecordComparison automated = records.Submit("course-a", "rules-v1", 10f, 30f, true);
            Assert.That(automated.ExcludedAutomatedRun, Is.True);
            Assert.That(records.GetBest("course-a", "rules-v1").BestRace, Is.EqualTo(90f));
            Assert.That(records.GetBest("course-a", "rules-v2").BestRace, Is.EqualTo(60f));
            Assert.That(records.GetBest("course-b", "rules-v1").BestRace, Is.EqualTo(75f));
        }

        [Test]
        public void RelaunchLoadsLastValidRecordAndAtomicWriteLeavesNoTemporaryFile()
        {
            var records = new RaceRecords(directory);
            records.Submit("course-a", "rules-v1", 30f, 90f, false);
            var relaunched = new RaceRecords(directory);
            RaceBest best = relaunched.GetBest("course-a", "rules-v1");
            Assert.That(best.BestLap, Is.EqualTo(30f));
            Assert.That(best.BestRace, Is.EqualTo(90f));
            Assert.That(Directory.GetFiles(directory).Select(Path.GetFileName), Is.EquivalentTo(new[] { "race-records.json" }));
        }

        [Test]
        public void CorruptFileFallsBackSafelyAndDoesNotEraseUnrelatedFiles()
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "race-records.json"), "{not-json");
            File.WriteAllText(Path.Combine(directory, "unrelated-save.dat"), "keep");
            var records = new RaceRecords(directory);
            RaceBest best = records.GetBest("course-a", "rules-v1");
            Assert.That(best.HasBestLap, Is.False);
            Assert.That(best.HasBestRace, Is.False);
            Assert.That(File.ReadAllText(Path.Combine(directory, "unrelated-save.dat")), Is.EqualTo("keep"));
            Assert.That(File.ReadAllText(Path.Combine(directory, "race-records.json")), Is.EqualTo("{not-json"));
        }
    }
}
