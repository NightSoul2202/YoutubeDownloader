using AngleSharp.Dom;
using FFMediaToolkit.Decoding;
using FFMediaToolkit.Encoding;
using FFmpeg.AutoGen;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;


namespace YoutubeDownloader.Services
{
    public class YouTubeService
    {
        private readonly YoutubeClient _youtubeClient;

        public YouTubeService()
        {
            _youtubeClient = new YoutubeClient();
        }

        public async Task<string> GetTitle(string videoUrl)
        {
            var video = await _youtubeClient.Videos.GetAsync(videoUrl);
            return video.Title;
        }

        public async Task<List<IStreamInfo>> GetAllStreamsAsync(string videoUrl)
        {
            var video = await _youtubeClient.Videos.GetAsync(videoUrl);
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);

            var muxedStreams = streamManifest.GetMuxedStreams()
                .OrderByDescending(s => s.VideoQuality.Label)
                .ThenByDescending(s => s.Bitrate.BitsPerSecond)
                .Cast<IStreamInfo>()
                .ToList();

            var videoOnlyStreams = streamManifest.GetVideoOnlyStreams()
                 .OrderByDescending(s => s.VideoQuality)
                 .ThenByDescending(s => s.Bitrate.BitsPerSecond)
                 .Cast<IStreamInfo>()
                 .ToList();

            var audioOnlyStreams = streamManifest.GetAudioOnlyStreams()
                .OrderByDescending(s => s.Bitrate.BitsPerSecond)
                .Cast<IStreamInfo>()
                .ToList();

            var allStreams = new List<IStreamInfo>();
            allStreams.AddRange(muxedStreams);
            allStreams.AddRange(videoOnlyStreams);
            allStreams.AddRange(audioOnlyStreams);

            return allStreams;
        }

        public async Task<Video> GetVideoAsync(string url)
        {
            var video = await _youtubeClient.Videos.GetAsync(url);
            return video;
        }

        public async Task DownloadVideoAsync(string videoUrl, string videoQualityLabel, string outputPath)
        {
            var video = await _youtubeClient.Videos.GetAsync(videoUrl);
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);

            var videoStream = streamManifest.GetMuxedStreams().FirstOrDefault(s => s.VideoQuality.Label == videoQualityLabel);
            if (videoStream != null)
            {
                await _youtubeClient.Videos.Streams.DownloadAsync(videoStream, outputPath);
            }
        }

        public async Task DownloadVideoOnlyAsync(string videoUrl, string videoQualityLabel, string outputPath)
        {
            var video = await _youtubeClient.Videos.GetAsync(videoUrl);
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);

            var videoStream = streamManifest.GetVideoOnlyStreams().FirstOrDefault(s => s.VideoQuality.Label == videoQualityLabel);
            if (videoStream != null)
            {
                await _youtubeClient.Videos.Streams.DownloadAsync(videoStream, outputPath);
            }
        }

        public async Task DownloadAudioOnlyAsync(string videoUrl, string audioQualityLabel, string outputPath)
        {
            var video = await _youtubeClient.Videos.GetAsync(videoUrl);
            var streamManifest = await _youtubeClient.Videos.Streams.GetManifestAsync(video.Id);

            var audioStream = streamManifest.GetAudioOnlyStreams().FirstOrDefault(s => s.Bitrate.ToString() == audioQualityLabel);
            if (audioStream != null)
            {
                await _youtubeClient.Videos.Streams.DownloadAsync(audioStream, outputPath);
            }
        }
    }
}
