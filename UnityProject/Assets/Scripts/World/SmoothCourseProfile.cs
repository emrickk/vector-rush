using System;
using UnityEngine;

namespace VectorRush
{
    // Periodic cubic B-spline of spatially filtered samples. Position, tangent and
    // curvature are continuous at every sample and at the lap seam.
    public sealed class SmoothCourseProfile
    {
        readonly Vector3[] points;
        readonly float[] banks;
        public SmoothCourseProfile(Vector3[] source, float length)
        {
            int n=source.Length;
            points=new Vector3[n]; banks=new float[n];
            float spacing=length/n;
            Filter(n,8f/spacing,(i,j,w)=>points[i]+=source[j]*w);
            var targets=new float[n];
            for(int i=0;i<n;i++)
            {
                Derivatives(i/(float)n,out var v,out var a);
                float denominator=Mathf.Pow(v.x*v.x+v.z*v.z,1.5f);
                float curvature=denominator>1e-9f?(v.z*a.x-v.x*a.z)/denominator:0;
                // Smooth saturation avoids the slope corners of a hard clamp.
                targets[i]=-17f*(float)Math.Tanh(curvature*1000f/17f);
            }
            Filter(n,12f/spacing,(i,j,w)=>banks[i]+=targets[j]*w);
        }
        static void Filter(int n,float sigma,Action<int,int,float> add)
        {
            int radius=Mathf.CeilToInt(3*sigma);var weights=new float[2*radius+1];float total=0;
            for(int j=-radius;j<=radius;j++){float w=Mathf.Exp(-.5f*j*j/(sigma*sigma));weights[j+radius]=w;total+=w;}
            for(int i=0;i<n;i++)for(int j=-radius;j<=radius;j++)add(i,(i+j+n)%n,weights[j+radius]/total);
        }
        void Basis(float t,out int k,out float u)
        {float p=Mathf.Repeat(t,1)*points.Length;k=Mathf.FloorToInt(p);u=p-k;}
        Vector3 P(int k)=>points[(k+points.Length)%points.Length];
        float B(int k)=>banks[(k+banks.Length)%banks.Length];
        public Vector3 Position(float t)
        {
            Basis(t,out int k,out float u);float q=1-u;
            return (P(k-1)*(q*q*q)+P(k)*(3*u*u*u-6*u*u+4)+P(k+1)*(-3*u*u*u+3*u*u+3*u+1)+P(k+2)*(u*u*u))/6f;
        }
        public void Derivatives(float t,out Vector3 velocity,out Vector3 acceleration)
        {
            Basis(t,out int k,out float u);float q=1-u;
            velocity=(-P(k-1)*(q*q)+P(k)*(3*u*u-4*u)+P(k+1)*(-3*u*u+2*u+1)+P(k+2)*(u*u))*.5f;
            acceleration=P(k-1)*q+P(k)*(3*u-2)+P(k+1)*(1-3*u)+P(k+2)*u;
        }
        public float Bank(float t)
        {
            Basis(t,out int k,out float u);float q=1-u;
            return (B(k-1)*q*q*q+B(k)*(3*u*u*u-6*u*u+4)+B(k+1)*(-3*u*u*u+3*u*u+3*u+1)+B(k+2)*u*u*u)/6f;
        }
        public TrackFrame Evaluate(float t)
        {Derivatives(t,out var v,out _);return new TrackFrame(Position(t),v,Bank(t));}
    }
}
