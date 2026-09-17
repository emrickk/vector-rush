using System;
using UnityEngine;

namespace VectorRush
{
    /// <summary>Road-only A/B gate for the bounded opening light-response candidate.</summary>
    public static class OpeningRoadLightResponse
    {
        public const string BaselineArgument = "-vrRoadLightBaseline";

        // The candidate is the ordinary-launch behavior. The switch affects only the opening
        // fixture aim/intensity; it does not select an exhaust, camera or gameplay variant.
        public static bool Enabled { get; } = IsEnabled(Environment.GetCommandLineArgs());

        public static bool IsEnabled(string[] args)
            => args == null || Array.IndexOf(args, BaselineArgument) < 0;

        public static bool ContainsFixture(int index)
            => index >= 56 || index <= 18;

        public static void LogConfiguration()
        {
            Debug.Log("VR_OPENING_ROAD_LIGHT_RESPONSE enabled=" + Enabled +
                " baselineArgument=" + BaselineArgument +
                " source=existing-track-fixtures passage=56..57,0..18 cameraGameplayUnchanged=true");
        }
    }
}
