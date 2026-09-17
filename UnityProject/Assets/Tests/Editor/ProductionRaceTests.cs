using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace VectorRush.Tests
{
    public sealed class ProductionRaceTests
    {
        [Test] public void LaterRivalsKeepTheirOwnTimeAndPlayerResultNeverChanges()
        {
            var ledger=new RaceFinishLedger(3,0);ledger.Advance(100);ledger.Cross(0,99.9f);
            ledger.Advance(3);ledger.Cross(1,102.9f);ledger.Advance(4);ledger.Cross(2,106.9f);
            Assert.That(ledger.PlayerFinishTime,Is.EqualTo(99.9f));Assert.That(ledger.Finishes[1].Time,Is.EqualTo(102.9f));
            Assert.That(ledger.AllComplete,Is.True);ledger.Cross(0,100);Assert.That(ledger.Finishes.Count,Is.EqualTo(3));
        }
        [Test] public void TimeoutMarksOnlyRemainingRacersAndPauseDoesNotAdvanceClock()
        {
            var ledger=new RaceFinishLedger(3,0);ledger.Advance(10);ledger.Cross(0,10);ledger.Advance(4);ledger.Cross(1,14);
            ledger.Advance(0);Assert.That(ledger.Clock,Is.EqualTo(14));ledger.Advance(56);
            Assert.That(ledger.AllComplete,Is.True);Assert.That(ledger.Finishes[0].DidNotFinish,Is.False);Assert.That(ledger.Finishes[1].DidNotFinish,Is.False);Assert.That(ledger.Finishes[2].DidNotFinish,Is.True);
            var restarted=new RaceFinishLedger(3,0);Assert.That(restarted.Finishes,Is.Empty);Assert.That(restarted.Clock,Is.Zero);
        }
        [Test] public void RecordsPersistBestTimesAndExcludeAutomationAndOtherCourses()
        {
            string folder=Path.Combine(Path.GetTempPath(),"vector-record-test-"+Guid.NewGuid().ToString("N"));string path=Path.Combine(folder,"records.json");
            try
            {
                var store=new RaceRecords(path);Assert.That(store.Record("course","rules",40,125,true),Is.False);Assert.That(File.Exists(path),Is.False);
                Assert.That(store.Record("course","rules",40,125,false),Is.True);Assert.That(store.Record("course","rules",42,130,false),Is.False);
                Assert.That(store.Record("course","rules",39,124,false),Is.True);store.Record("other","rules",50,160,false);
                var loaded=new RaceRecords(path);Assert.That(loaded.Get("course","rules").race,Is.EqualTo(124));Assert.That(loaded.Get("other","rules").race,Is.EqualTo(160));Assert.That(loaded.Get("course","newrules"),Is.Null);
                File.WriteAllText(path,"{broken");var corrupt=new RaceRecords(path);Assert.That(corrupt.Get("course","rules"),Is.Null);
            }
            finally{if(Directory.Exists(folder))Directory.Delete(folder,true);}
        }
        [Test] public void DirectorKeepsUnfinishedRivalsActiveAfterPlayerFinishAndClearsOnRestart()
        {
            var trackObject=new GameObject("Lifecycle track");var directorObject=new GameObject("Lifecycle director");
            var objects=new System.Collections.Generic.List<GameObject>();
            try
            {
                var track=trackObject.AddComponent<TrackPath>();track.Ensure();
                var vehicles=new System.Collections.Generic.List<HoverVehicle>();
                for(int i=0;i<3;i++){var go=new GameObject("Lifecycle racer "+i);objects.Add(go);var v=go.AddComponent<HoverVehicle>();v.Initialize(track,i==0,i);vehicles.Add(v);}
                var director=directorObject.AddComponent<RaceDirector>();
                typeof(RaceDirector).GetMethod("Awake",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(director,null);
                director.Initialize(track,vehicles);director.StartRace();
                var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
                var ledger=(RaceFinishLedger)typeof(RaceDirector).GetField("finishLedger",flags).GetValue(director);
                ledger.Advance(100);ledger.Cross(0,100);
                typeof(RaceDirector).GetProperty("Phase").SetValue(director,RacePhase.Finished);
                Assert.That(director.CanSimulate(vehicles[0]),Is.False);Assert.That(director.CanSimulate(vehicles[1]),Is.True);
                director.TogglePause();Assert.That(director.CanSimulate(vehicles[1]),Is.False);
                director.TogglePause();Assert.That(director.CanSimulate(vehicles[1]),Is.True);
                ledger.Advance(3);ledger.Cross(1,103);Assert.That(director.FinishTime,Is.EqualTo(100));
                Assert.That(director.CanSimulate(vehicles[1]),Is.False);Assert.That(director.CanSimulate(vehicles[2]),Is.True);
                vehicles[0].Body.detectCollisions=false;
                Assert.That(director.IsFinished(vehicles[0]),Is.True);
                director.RestartRace();Assert.That(vehicles[0].Body.detectCollisions,Is.True);Assert.That(director.IsFinished(vehicles[0]),Is.False);Assert.That(director.Phase,Is.EqualTo(RacePhase.Countdown));Assert.That(director.FinishRecords,Is.Empty);Assert.That(director.RaceTime,Is.Zero);
            }
            finally{UnityEngine.Object.DestroyImmediate(directorObject);foreach(var go in objects)UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(trackObject);Time.timeScale=1;}
        }

        [Test] public void PreferencesClampRejectConflictsAndRestoreDefaults()
        {
            var p=new PreferenceData{master=2,music=-1,effects=float.NaN,sensitivity=20};p.Normalize();
            Assert.That(p.master,Is.EqualTo(1));Assert.That(p.music,Is.Zero);Assert.That(p.effects,Is.EqualTo(.85f));Assert.That(p.sensitivity,Is.EqualTo(1.5f));
            Assert.That(p.Bind(0,p.right),Is.False);Assert.That(p.Bind(0,KeyCode.P),Is.False);Assert.That(p.Bind(0,KeyCode.J),Is.True);
            var restored=JsonUtility.FromJson<PreferenceData>(JsonUtility.ToJson(p));Assert.That(restored.left,Is.EqualTo(KeyCode.J));
            restored.right=KeyCode.J;restored.Normalize();Assert.That(restored.left,Is.EqualTo(KeyCode.A));Assert.That(restored.BindingsValid(),Is.True);
        }
    }
}
