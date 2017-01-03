using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Simple_Video_Editing
{
    internal class MyFFMpegProgress
    {
        private static Regex DurationRegex;

        private static Regex ProgressRegex;

        internal float? Seek = null;

        internal float? MaxDuration = null;

        private Action<ConvertProgressEventArgs> ProgressCallback;

        private ConvertProgressEventArgs lastProgressArgs;

        private bool Enabled = true;

        private int progressEventCount;

        static MyFFMpegProgress()
        {
            MyFFMpegProgress.DurationRegex = new Regex("Duration:\\s(?<duration>[0-9:.]+)([,]|$)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
            MyFFMpegProgress.ProgressRegex = new Regex("time=(?<progress>[0-9:.]+)\\s", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
        }

        internal MyFFMpegProgress(Action<ConvertProgressEventArgs> progressCallback, bool enabled)
        {
            this.ProgressCallback = progressCallback;
            this.Enabled = enabled;
        }

        internal void Complete()
        {
            if ((!this.Enabled || this.lastProgressArgs == null ? false : this.lastProgressArgs.Processed < this.lastProgressArgs.TotalDuration))
            {
                this.ProgressCallback(new ConvertProgressEventArgs(this.lastProgressArgs.TotalDuration, this.lastProgressArgs.TotalDuration));
            }
        }

        private TimeSpan CorrectDuration(TimeSpan totalDuration)
        {
            if (totalDuration != TimeSpan.Zero)
            {
                if (this.Seek.HasValue)
                {
                    TimeSpan timeSpan = TimeSpan.FromSeconds((double)this.Seek.Value);
                    totalDuration = (totalDuration > timeSpan ? totalDuration.Subtract(timeSpan) : TimeSpan.Zero);
                }
                if (this.MaxDuration.HasValue)
                {
                    TimeSpan timeSpan1 = TimeSpan.FromSeconds((double)this.MaxDuration.Value);
                    if (totalDuration > timeSpan1)
                    {
                        totalDuration = timeSpan1;
                    }
                }
            }
            return totalDuration;
        }

        internal void ParseLine(string line)
        {
            if (this.Enabled)
            {
                TimeSpan timeSpan = (this.lastProgressArgs != null ? this.lastProgressArgs.TotalDuration : TimeSpan.Zero);
                Match match = MyFFMpegProgress.DurationRegex.Match(line);
                if (match.Success)
                {
                    TimeSpan zero = TimeSpan.Zero;
                    if (TimeSpan.TryParse(match.Groups["duration"].Value, out zero))
                    {
                        TimeSpan timeSpan1 = timeSpan.Add(zero);
                        this.lastProgressArgs = new ConvertProgressEventArgs(TimeSpan.Zero, timeSpan1);
                    }
                }
                Match match1 = MyFFMpegProgress.ProgressRegex.Match(line);
                if (match1.Success)
                {
                    TimeSpan zero1 = TimeSpan.Zero;
                    if (TimeSpan.TryParse(match1.Groups["progress"].Value, out zero1))
                    {
                        if (this.progressEventCount == 0)
                        {
                            timeSpan = this.CorrectDuration(timeSpan);
                        }
                        this.lastProgressArgs = new ConvertProgressEventArgs(zero1, (timeSpan != TimeSpan.Zero ? timeSpan : zero1));
                        this.ProgressCallback(this.lastProgressArgs);
                        MyFFMpegProgress fFMpegProgress = this;
                        fFMpegProgress.progressEventCount = fFMpegProgress.progressEventCount + 1;
                    }
                }
            }
        }

        internal void Reset()
        {
            this.progressEventCount = 0;
            this.lastProgressArgs = null;
        }
    }
}
