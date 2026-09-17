using System;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

namespace VectorRush
{
    /// <summary>
    /// Runtime-safe identity for the exact 1,201 sampled course frames. The hash uses raw
    /// IEEE-754 float bits rather than JSON text, so editor/runtime decimal formatting cannot
    /// change identity. Source JSON bytes are tracked separately by AAAExemplar provenance.
    /// </summary>
    public static class AAACourseIdentity
    {
        const int Samples = 1200;
        public static string Compute(TrackPath track)
        {
            if (!track) throw new ArgumentNullException(nameof(track));
            track.Ensure();
            using (var bytes = new MemoryStream(16 + (Samples + 1) * 14 * sizeof(float)))
            using (var writer = new BinaryWriter(bytes))
            {
                writer.Write(new byte[] { (byte)'V', (byte)'R', (byte)'C', (byte)'R', (byte)'S', (byte)'1' });
                writer.Write(Samples);
                Write(writer, track.Width); Write(writer, track.Length);
                for (int i = 0; i <= Samples; i++)
                {
                    float progress = i / (float)Samples;
                    TrackFrame frame = track.Evaluate(progress);
                    Write(writer, progress); Write(writer, progress * track.Length);
                    Write(writer, frame.Position); Write(writer, frame.Forward);
                    Write(writer, frame.Right); Write(writer, frame.Up);
                }
                writer.Flush();
                using (var sha = SHA256.Create())
                    return BitConverter.ToString(sha.ComputeHash(bytes.ToArray())).Replace("-", "").ToLowerInvariant();
            }
        }

        public static bool IsCanonical(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 64) return false;
            for (int i = 0; i < value.Length; i++)
                if (!((value[i] >= '0' && value[i] <= '9') || (value[i] >= 'a' && value[i] <= 'f'))) return false;
            return true;
        }
        static void Write(BinaryWriter writer, float value) => writer.Write(value);
        static void Write(BinaryWriter writer, Vector3 value)
        { writer.Write(value.x); writer.Write(value.y); writer.Write(value.z); }
    }
}
