using System;
using UnityEngine;

namespace VectorRush
{
    /// <summary>Optional authored engine coordinates, in the imported visual root's local Unity space.</summary>
    public static class ShipEngineAnchors
    {
        public const string ResourcePath = "Art/HeroShipEngineAnchors";
        static string lastRejectedPayload;

        public sealed class Anchor
        {
            public readonly string Name;
            public readonly Vector3 Exit, Throat;
            public readonly float OpeningRadius, CompactCoreRadius;
            internal Anchor(Engine value)
            {
                Name=value.name; Exit=Vector(value.unity_exit); Throat=Vector(value.unity_throat);
                OpeningRadius=value.opening_radius; CompactCoreRadius=value.compact_core_radius;
            }
        }

        // Output order is always port/left, starboard/right, center, regardless of JSON order.
        public static bool TryLoad(out Anchor[] anchors)
        {
            var asset=Resources.Load<TextAsset>(ResourcePath);
            anchors=null;
            if(!asset) return false;
            if(TryParse(asset.text,out anchors,out string error)) return true;
            if(lastRejectedPayload!=asset.text)
            {
                Debug.LogWarning("Ship engine anchors rejected at Resources/"+ResourcePath+": "+error+" Using the unchanged V2 engine layout.",asset);
                lastRejectedPayload=asset.text;
            }
            return false;
        }

        public static bool TryParse(string json,out Anchor[] anchors,out string error)
        {
            anchors=null; error=null;
            if(string.IsNullOrWhiteSpace(json)) { error="Empty JSON payload."; return false; }
            Payload payload;
            try { payload=JsonUtility.FromJson<Payload>(json); }
            catch(ArgumentException exception) { error="Malformed JSON: "+exception.Message; return false; }
            if(payload==null || payload.unity_mapping!="(X,Z,-Y)" || payload.source_axes==null ||
               payload.source_axes.forward!="-Y" || payload.source_axes.up!="+Z")
            { error="Unsupported or missing coordinate mapping; expected source forward -Y/up +Z and Unity (X,Z,-Y)."; return false; }
            if(payload.engines==null || payload.engines.Length!=3)
            { error="Expected exactly three named engines: left, right, center."; return false; }
            var result=new Anchor[3];
            foreach(var engine in payload.engines)
            {
                if(engine==null) { error="Null engine record."; return false; }
                int index=engine.name=="left"?0:engine.name=="right"?1:engine.name=="center"?2:-1;
                if(index<0 || result[index]!=null) { error="Unknown or duplicate engine name: "+engine.name; return false; }
                if(!ValidVector(engine.unity_exit) || !ValidVector(engine.unity_throat) || !ValidVector(engine.unity_effect_direction))
                { error=engine.name+": exit, throat and effect direction must each contain three finite numbers within +/- 1000."; return false; }
                Vector3 exit=Vector(engine.unity_exit), throat=Vector(engine.unity_throat), direction=Vector(engine.unity_effect_direction);
                Vector3 depth=throat-exit;
                if((direction-Vector3.back).sqrMagnitude>.000001f || Mathf.Abs(depth.x)>.001f || Mathf.Abs(depth.y)>.001f || depth.z<=.02f || depth.z>10f)
                { error=engine.name+": unsupported nozzle axis or depth; expected rear-facing -Z with a throat directly forward of the exit."; return false; }
                if(!Finite(engine.opening_radius) || !Finite(engine.compact_core_radius) || engine.opening_radius<=.01f || engine.opening_radius>10f ||
                   engine.compact_core_radius<=0f || engine.compact_core_radius>engine.opening_radius*.5f)
                { error=engine.name+": opening radius must be 0.01–10 m and compact core radius must be positive and at most half the opening radius."; return false; }
                result[index]=new Anchor(engine);
            }
            anchors=result; return true;
        }

        static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        static bool ValidVector(float[] value)
        { return value!=null && value.Length==3 && Finite(value[0]) && Finite(value[1]) && Finite(value[2]) && Mathf.Abs(value[0])<=1000f && Mathf.Abs(value[1])<=1000f && Mathf.Abs(value[2])<=1000f; }
        static Vector3 Vector(float[] value) { return new Vector3(value[0],value[1],value[2]); }
        [Serializable] sealed class Payload { public SourceAxes source_axes; public string unity_mapping; public Engine[] engines; }
        [Serializable] sealed class SourceAxes { public string forward,up; }
        [Serializable] internal sealed class Engine
        {
            public string name;
            public float[] unity_exit,unity_throat,unity_effect_direction;
            public float opening_radius,compact_core_radius;
        }
    }
}
