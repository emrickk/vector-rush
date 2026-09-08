using UnityEngine;

namespace VectorRush
{
    public struct TrackFrame
    {
        public Vector3 Position, Forward, Right, Up;
        public TrackFrame(Vector3 p, Vector3 f, float bank)
        {
            Position = p; Forward = f.normalized;
            Right = Quaternion.AngleAxis(bank, Forward) * Vector3.Cross(Vector3.up, Forward).normalized;
            Up = Vector3.Cross(Forward, Right).normalized;
        }
    }

    public sealed class TrackPath : MonoBehaviour
    {
        public float Width = 22f;
        public float Length { get; private set; }
        const int Samples = 1200;
        Vector3[] points;
        float[] distances;
        readonly Vector3[] knots = {
            new Vector3(0,32,-230),new Vector3(135,33,-225),new Vector3(242,40,-150),
            new Vector3(262,51,-20),new Vector3(180,62,85),new Vector3(110,53,145),
            new Vector3(150,39,225),new Vector3(70,30,300),new Vector3(-72,34,286),
            new Vector3(-180,48,200),new Vector3(-240,58,86),new Vector3(-185,45,-5),
            new Vector3(-270,33,-115),new Vector3(-200,28,-220)
        };

        void Awake() { Ensure(); }
        public void Ensure()
        {
            if (points != null) return;
            points = new Vector3[Samples+1]; distances = new float[Samples+1];
            for (int i=0;i<=Samples;i++) {
                points[i] = Spline((float)i/Samples);
                if(i>0) distances[i]=distances[i-1]+Vector3.Distance(points[i-1],points[i]);
            }
            Length=distances[Samples];
        }
        Vector3 Spline(float t)
        {
            float u=Mathf.Repeat(t,1)*knots.Length; int j=Mathf.FloorToInt(u); u-=j;
            Vector3 a=knots[(j+knots.Length-1)%knots.Length],b=knots[j%knots.Length],
                c=knots[(j+1)%knots.Length],d=knots[(j+2)%knots.Length];
            return .5f*((2*b)+(-a+c)*u+(2*a-5*b+4*c-d)*u*u+(-a+3*b-3*c+d)*u*u*u);
        }
        int Segment(float distance)
        {
            int lo=0,hi=Samples;
            while(lo+1<hi){int mid=(lo+hi)/2;if(distances[mid]<=distance)lo=mid;else hi=mid;}
            return lo;
        }
        public TrackFrame Evaluate(float normalized)
        {
            Ensure(); float d=Mathf.Repeat(normalized,1)*Length; int i=Segment(d);
            float a=Mathf.InverseLerp(distances[i],distances[i+1],d);
            Vector3 p=Vector3.Lerp(points[i],points[i+1],a);
            Vector3 f=(points[(i+2)%Samples]-points[(i+Samples-1)%Samples]).normalized;
            Vector3 before=points[(i+Samples-5)%Samples],after=points[(i+5)%Samples];
            float turn=Vector3.SignedAngle(p-before,after-p,Vector3.up);
            return new TrackFrame(p,f,Mathf.Clamp(-turn*2.5f,-17,17));
        }
        public float ClosestProgress(Vector3 position)
        {
            Ensure(); float best=float.MaxValue; int idx=0; float fraction=0;
            for(int i=0;i<Samples;i++) {
                Vector3 ab=points[i+1]-points[i]; float f=Mathf.Clamp01(Vector3.Dot(position-points[i],ab)/ab.sqrMagnitude);
                float d=(position-points[i]-ab*f).sqrMagnitude;
                if(d<best){best=d;idx=i;fraction=f;}
            }
            return Mathf.Repeat(Mathf.Lerp(distances[idx],distances[idx+1],fraction)/Length,1);
        }
    }
}
