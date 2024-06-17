//using System.IO;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Hosting;
//using YoutubeDownloader.Services;
//using FFMediaToolkit.Decoding;
//using YoutubeExplode.Videos.Streams;
//using YoutubeExplode.Videos;

//namespace YoutubeDownloader.Controllers
//{
//    public class DownloadController : Controller
//    {
//        private readonly YouTubeService _youTubeService;
//        private readonly IWebHostEnvironment _webHostEnvironment;

//        public DownloadController(YouTubeService youTubeService, IWebHostEnvironment webHostEnvironment)
//        {
//            _youTubeService = youTubeService;
//            _webHostEnvironment = webHostEnvironment;
//        }

//        [HttpPost]
//        public async Task<IActionResult> Download(string videoUrl, string qualityLabel)
//        {
//            if (string.IsNullOrWhiteSpace(videoUrl) || string.IsNullOrWhiteSpace(qualityLabel))
//            {
//                return BadRequest("Invalid URL or quality");
//            }

//            var outputDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "downloads");
//            if (!Directory.Exists(outputDirectory))
//            {
//                Directory.CreateDirectory(outputDirectory);
//            }

//            string outputPath;
//            if (qualityLabel.StartsWith("Muxed:"))
//            {
//                var videoQualityLabel = qualityLabel.Substring(6).Trim();
//                videoQualityLabel = videoQualityLabel.Replace(" | mp4", "").Replace(" | webm", "");
//                outputPath = Path.Combine(outputDirectory, $"{await _youTubeService.GetTitle(videoUrl)}.mp4");
//                await _youTubeService.DownloadVideoAsync(videoUrl, videoQualityLabel, outputPath);
//                return PhysicalFile(outputPath, "video/mp4", $"{await _youTubeService.GetTitle(videoUrl)}.mp4");
//            }

//            if (qualityLabel.StartsWith("Video-only:"))
//            {
//                var videoQualityLabel = qualityLabel.Substring(11).Trim();
//                videoQualityLabel = videoQualityLabel.Replace(" | mp4", "").Replace(" | webm", "");
//                outputPath = Path.Combine(outputDirectory, $"{await _youTubeService.GetTitle(videoUrl)}.mp4");
//                await _youTubeService.DownloadVideoOnlyAsync(videoUrl, videoQualityLabel, outputPath);
//                return PhysicalFile(outputPath, "video/mp4", $"{await _youTubeService.GetTitle(videoUrl)}.mp4");
//            }

//            if (qualityLabel.StartsWith("Audio-only:"))
//            {
//                var audioQualityLabel = qualityLabel.Substring(11).Trim();
//                audioQualityLabel = audioQualityLabel.Replace(" | mp3", "");
//                outputPath = Path.Combine(outputDirectory, $"{await _youTubeService.GetTitle(videoUrl)}.mp3");
//                await _youTubeService.DownloadAudioOnlyAsync(videoUrl, audioQualityLabel, outputPath);
//                return PhysicalFile(outputPath, "audio/mp3", $"{await _youTubeService.GetTitle(videoUrl)}.mp3");
//            }

//            return BadRequest("Invalid quality selection");
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetThumbnail(string url)
//        {
//            if (string.IsNullOrWhiteSpace(url))
//            {
//                return BadRequest("Invalid URL");
//            }

//            var video = await _youTubeService.GetVideoAsync(url);
//            if (video == null || video.Thumbnails == null || !video.Thumbnails.Any())
//            {
//                return BadRequest("Thumbnail not found");
//            }

//            var thumbnail = video.Thumbnails.OrderByDescending(t => t.Resolution.Width * t.Resolution.Height).FirstOrDefault();
//            if (thumbnail == null)
//            {
//                return BadRequest("Thumbnail not found");
//            }

//            return Json(thumbnail.Url);
//        }


//        [HttpGet]
//        public IActionResult Index()
//        {
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> GetQualities([FromBody] VideoUrlRequest request)
//        {
//            if (string.IsNullOrWhiteSpace(request.VideoUrl))
//            {
//                return BadRequest("Invalid URL");
//            }

//            var allStreams = await _youTubeService.GetAllStreamsAsync(request.VideoUrl);
//            var qualities = allStreams.Select(s =>
//            {
                
//                var type = s is MuxedStreamInfo ? "Muxed" : s is VideoOnlyStreamInfo ? "Video-only" : "Audio-only";
//                var quality = s is VideoOnlyStreamInfo videoStream ? videoStream.VideoQuality.Label : s is AudioOnlyStreamInfo audioStream ? audioStream.Bitrate.ToString() : s is MuxedStreamInfo msi ? msi.VideoQuality.Label : "Unknown";
//                var container = s is AudioOnlyStreamInfo audio ? "mp3" : s.Container.ToString();
//                return $"{type}: {quality} | {container}";
//            }).ToList();


//            return Json(qualities);
//        }
//    }

//    public class VideoUrlRequest
//    {
//        public string VideoUrl { get; set; }
//    }
//}
