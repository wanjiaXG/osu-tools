// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Net;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;

namespace PerformanceCalculator
{
    /// <summary>
    /// A <see cref="WorkingBeatmap"/> which reads from a .osu file.
    /// </summary>
    public class ProcessorWorkingBeatmap : WorkingBeatmap
    {

        public static string FilePath = "osuFile";

        public static int RetryCount = 5;

        private readonly Beatmap beatmap;

        /// <summary>
        /// Constructs a new <see cref="ProcessorWorkingBeatmap"/> from a .osu file.
        /// </summary>
        /// <param name="file">The .osu file.</param>
        /// <param name="beatmapId">An optional beatmap ID (for cases where .osu file doesn't have one).</param>
        public ProcessorWorkingBeatmap(string file, int? beatmapId = null)
            : this(readFromFile(file), beatmapId)
        {
        }

        public ProcessorWorkingBeatmap(int id)
            : this(readFromID(id), id)
        {
        }

        private ProcessorWorkingBeatmap(Beatmap beatmap, int? beatmapId = null)
            : base(beatmap.BeatmapInfo)
        {
            this.beatmap = beatmap;

            beatmap.BeatmapInfo.Ruleset = LegacyHelper.GetRulesetFromLegacyID(beatmap.BeatmapInfo.RulesetID).RulesetInfo;

            if (beatmapId.HasValue)
                beatmap.BeatmapInfo.OnlineBeatmapID = beatmapId;
        }

        private static Beatmap readFromFile(string filename)
        {
            using (var stream = File.OpenRead(filename))
            using (var streamReader = new StreamReader(stream))
                return Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader);
        }

        private static Beatmap readFromID(int id)
        {

            if (!Directory.Exists(FilePath))
                Directory.CreateDirectory(FilePath);

            string file = new DirectoryInfo(FilePath).FullName + "/" + id + ".osu";
            if (!File.Exists(file))
                downloadFile(id, file);

            return readFromFile(file);
        }

        private static void downloadFile(int id, string file)
        {
            WebClient client = new WebClient(); 
            for(int i = 0; i < RetryCount; i++)
            {
                try
                {
                    client.DownloadFile("https://osu.ppy.sh/osu/" + id, file);
                    break;
                }catch{ }
            }
        } 

        protected override IBeatmap GetBeatmap() => beatmap;
        protected override Texture GetBackground() => null;
        protected override Track GetTrack() => null;
    }
}
