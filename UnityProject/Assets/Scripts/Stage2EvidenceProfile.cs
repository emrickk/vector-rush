using System;
using System.IO;

namespace VectorRush
{
    /// <summary>Shared opt-in storage override, resolved before any player services load.</summary>
    public static class Stage2EvidenceProfile
    {
        public static string Directory => Resolve(Environment.GetCommandLineArgs());
        public static bool Enabled => Directory!=null;

        public static string Resolve(string[] args)
        {
            int profile=Array.IndexOf(args,"-stage2Profile");
            if(profile>=0)
            {
                if(profile+1>=args.Length||!Path.IsPathRooted(args[profile+1]))
                    throw new ArgumentException("-stage2Profile requires an absolute directory.");
                return Path.GetFullPath(args[profile+1]);
            }
            int evidence=Array.IndexOf(args,"-stage2Evidence");
            if(evidence<0)return null;
            if(evidence+1>=args.Length||!Path.IsPathRooted(args[evidence+1]))
                throw new ArgumentException("-stage2Evidence requires an absolute output directory.");
            return Path.Combine(Path.GetFullPath(args[evidence+1]),"profile");
        }
    }
}
