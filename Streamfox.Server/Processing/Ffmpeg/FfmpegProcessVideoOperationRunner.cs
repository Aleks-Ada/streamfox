﻿namespace Streamfox.Server.Processing.Ffmpeg
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using Streamfox.Server.VideoProcessing;

    public class FfmpegProcessVideoOperationRunner : IVideoCoercer, IFileSystemThumbnailExtractor,
                                                     IVideoFramesFetcher, IVideoMetadataGrabber
    {
        private readonly IFfmpegProcessRunner _ffmpegProcessRunner;

        public FfmpegProcessVideoOperationRunner(IFfmpegProcessRunner ffmpegProcessRunner)
        {
            _ffmpegProcessRunner = ffmpegProcessRunner;
        }

        public async Task ExtractThumbnail(string videoPath, string thumbnailPath)
        {
            await _ffmpegProcessRunner.RunFfmpeg(
                    $"-i \"{videoPath}\" -vframes 1 -q:v 2 -vf scale=-1:225 \"{thumbnailPath}\"");
        }

        public async Task<VideoMetadata> GrabMetadata(string videoPath)
        {
            string ffprobeOutput = await _ffmpegProcessRunner.RunFfprobe(
                    $"-v quiet -show_streams -show_format -print_format json \"{videoPath}\"");

            FfmpegVideoMetadata ffmpegVideoMetadata =
                    JsonConvert.DeserializeObject<FfmpegVideoMetadata>(ffprobeOutput);

            return FfmpegOutputToVideoMetadata(ffmpegVideoMetadata);
        }

        public async Task<int> FetchVideoFrames(string videoPath)
        {
            string ffprobeOutput = await _ffmpegProcessRunner.RunFfprobe(
                    $"-v quiet -show_streams -show_format -print_format json \"{videoPath}\"");

            FfmpegVideoMetadata ffmpegVideoMetadata =
                    JsonConvert.DeserializeObject<FfmpegVideoMetadata>(ffprobeOutput);

            VideoMetadata videoMetadata = FfmpegOutputToVideoMetadata(ffmpegVideoMetadata);

            if (videoMetadata.VideoCodec == VideoCodec.Invalid)
            {
                return 0;
            }

            string frames = ffmpegVideoMetadata.Streams[0].Frames;

            if (int.TryParse(frames, out int frameCount))
            {
                return frameCount;
            }

            string averageFrameRate = ffmpegVideoMetadata.Streams[0].AverageFrameRate;

            int averageFrameRateInt = int.Parse(averageFrameRate.Split('/')[0]);
            double duration = float.Parse(ffmpegVideoMetadata.Format.Duration);

            return (int)Math.Round(averageFrameRateInt * duration);
        }

        public async Task<IProgressLogger> CoerceToVp9(string sourcePath, string outputPath)
        {
            return await _ffmpegProcessRunner.RunFfmpegWithProgressLogging(
                    $"-i \"{sourcePath}\" -c:v vp9 -crf 30 -b:v 0 -f webm \"{outputPath}\"");
        }

        public Task<IProgressLogger> CopyWithoutCoercing(string sourcePath, string outputPath)
        {
            File.Copy(sourcePath, outputPath);
            return Task.FromResult<IProgressLogger>(new EmptyProgressLogger());
        }

        private static VideoMetadata FfmpegOutputToVideoMetadata(
                FfmpegVideoMetadata ffmpegVideoMetadata)
        {
            if (ffmpegVideoMetadata == null)
            {
                return new VideoMetadata(VideoCodec.Invalid, VideoFormat.Other);
            }

            return new VideoMetadata(
                    FindVideoCodec(ffmpegVideoMetadata),
                    FindVideoFormat(ffmpegVideoMetadata));
        }

        private static VideoCodec FindVideoCodec(FfmpegVideoMetadata ffmpegVideoMetadata)
        {
            if (ffmpegVideoMetadata.Streams == null ||
                ffmpegVideoMetadata.Streams.Length == 0 ||
                ffmpegVideoMetadata.Streams.All(stream => stream.CodecType != "video"))
            {
                return VideoCodec.Invalid;
            }

            AudioVideoStream stream =
                    ffmpegVideoMetadata.Streams.First(stream => stream.CodecType == "video");

            if (stream.CodecName == "h264")
            {
                return VideoCodec.H264;
            }

            if (stream.CodecName == "vp9")
            {
                return VideoCodec.Vp9;
            }

            if (double.Parse(ffmpegVideoMetadata.Format.Duration) > 1d)
            {
                return VideoCodec.Other;
            }

            return VideoCodec.Invalid;
        }

        private static VideoFormat FindVideoFormat(FfmpegVideoMetadata ffmpegVideoMetadata)
        {
            string formatName = ffmpegVideoMetadata.Format?.FormatName;

            if (formatName == null)
            {
                return VideoFormat.Other;
            }

            if (formatName.Contains("mp4"))
            {
                return VideoFormat.Mp4;
            }

            if (formatName.Contains("webm"))
            {
                return VideoFormat.Webm;
            }

            return VideoFormat.Other;
        }

        private class EmptyProgressLogger : IProgressLogger
        {
            private readonly Queue<ProgressReport> _progressReports = new Queue<ProgressReport>();

            public EmptyProgressLogger()
            {
                _progressReports.Enqueue(new ProgressReport(int.MaxValue));
            }

            public Task<bool> HasMoreProgress()
            {
                return Task.FromResult(_progressReports.Count > 0);
            }

            public Task<ProgressReport> GetNextProgress()
            {
                return Task.FromResult(_progressReports.Dequeue());
            }
        }
    }
}