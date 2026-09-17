using System;
using System.Collections.Generic;

namespace VectorRush
{
    public readonly struct RacerFinish
    {
        public readonly int RacerId,Position;
        public readonly float Time;
        public readonly bool DidNotFinish;
        public RacerFinish(int racerId,int position,float time,bool dnf){RacerId=racerId;Position=position;Time=time;DidNotFinish=dnf;}
    }

    // Race clock remains independent of the player's frozen result presentation.
    public sealed class RaceFinishLedger
    {
        readonly List<RacerFinish> finishes=new List<RacerFinish>();
        readonly HashSet<int> complete=new HashSet<int>();
        readonly List<RacerFinish> adaptedFinishes=new List<RacerFinish>();
        readonly Dictionary<int,string> adaptedIds=new Dictionary<int,string>();
        readonly Dictionary<string,int> adaptedIndices=new Dictionary<string,int>(StringComparer.Ordinal);
        readonly int count,playerId;
        readonly RaceFinishLifecycle lifecycle;
        float clock;
        float playerFinishTime=-1;
        public IReadOnlyList<RacerFinish> Finishes
        {
            get
            {
                if(lifecycle==null)return finishes.AsReadOnly();
                adaptedFinishes.Clear();
                foreach(var record in lifecycle.Records)
                    adaptedFinishes.Add(new RacerFinish(adaptedIndices[record.RacerId],record.Position,record.FinishTime,
                        record.Status==RacerResultStatus.DidNotFinish));
                return adaptedFinishes.AsReadOnly();
            }
        }
        public float Clock=>lifecycle==null?clock:lifecycle.SimulationTime;
        public float PlayerFinishTime=>lifecycle==null?playerFinishTime:(lifecycle.PlayerFinished?lifecycle.PlayerFinishTime:-1f);
        public bool PlayerFinished=>PlayerFinishTime>=0;
        public bool AllComplete=>lifecycle==null?complete.Count==count:lifecycle.Records.Count==count;
        public float Deadline=>lifecycle==null?(PlayerFinished?PlayerFinishTime+60f:float.PositiveInfinity):lifecycle.FinishDeadline;
        public RaceFinishLedger(int count,int playerId){this.count=count;this.playerId=playerId;}

        internal RaceFinishLedger(RaceFinishLifecycle lifecycle,IReadOnlyList<RacerIdentity> identities)
        {
            this.lifecycle=lifecycle??throw new ArgumentNullException(nameof(lifecycle));
            if(identities==null)throw new ArgumentNullException(nameof(identities));
            count=identities.Count;
            playerId=-1;
            for(int i=0;i<identities.Count;i++)
            {
                adaptedIds.Add(i,identities[i].RacerId);
                adaptedIndices.Add(identities[i].RacerId,i);
                if(identities[i].IsPlayer)playerId=i;
            }
        }

        public bool IsComplete(int racerId)=>lifecycle==null?complete.Contains(racerId):
            adaptedIds.TryGetValue(racerId,out string id)&&lifecycle.IsResolved(id);
        public void Advance(float delta)
        {
            if(lifecycle!=null){lifecycle.Advance(delta);return;}
            // Convenience for a step with no crossing samples.
            BeginStep(delta);
            FinalizeTimeouts();
        }
        public void BeginStep(float delta)
        {
            if(lifecycle!=null){lifecycle.BeginStep(delta);return;}
            if(delta<0||float.IsNaN(delta)||float.IsInfinity(delta))throw new ArgumentOutOfRangeException(nameof(delta));
            if(AllComplete)return;clock+=delta;
        }
        public void FinalizeTimeouts()
        {
            if(lifecycle!=null){lifecycle.FinalizeTimeouts();return;}
            if(PlayerFinished&&Clock>=Deadline)
                for(int i=0;i<count;i++)if(complete.Add(i))finishes.Add(new RacerFinish(i,0,0,true));
        }
        public bool Cross(int racerId,float crossingTime)
        {
            if(racerId<0||racerId>=count||float.IsNaN(crossingTime)||float.IsInfinity(crossingTime)||crossingTime<0||crossingTime>Clock+.0001f)throw new ArgumentOutOfRangeException();
            // Exact deadline ties count as finishes. Adjudicate all sorted samples
            // in a step before marking its remaining racers DNF.
            if(lifecycle!=null)
            {
                string id=adaptedIds[racerId];
                if(crossingTime>Deadline||lifecycle.IsResolved(id))return false;
                return lifecycle.RecordFinish(id,crossingTime)!=null;
            }
            if(crossingTime>Deadline||!complete.Add(racerId))return false;
            finishes.Add(new RacerFinish(racerId,finishes.Count+1,crossingTime,false));
            if(racerId==playerId)playerFinishTime=crossingTime;
            return true;
        }
    }
}
