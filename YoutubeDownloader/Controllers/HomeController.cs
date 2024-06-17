using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;
using YoutubeDownloader.Models;
using YoutubeDownloader.Services;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Controllers
{
    public class HomeController : Controller
    {
        private readonly YouTubeService _youTubeService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(YouTubeService youTubeService, IWebHostEnvironment webHostEnvironment)
        {
            _youTubeService = youTubeService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpPost]
        public async Task<IActionResult> Download(string videoUrl, string qualityLabel)
        {
            if (string.IsNullOrWhiteSpace(videoUrl) || string.IsNullOrWhiteSpace(qualityLabel))
            {
                return BadRequest("Invalid URL or quality");
            }

            var outputDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "downloads");
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            else
            {
                foreach (var file in Directory.GetFiles(outputDirectory))
                {
                    System.IO.File.Delete(file);
                }
            }

            var title = await _youTubeService.GetTitle(videoUrl);
            ViewData["VideoTitle"] = title;

            var outputFileName = Regex.Replace(title, "[*\\/:?\"<>|]", "");
            string fileExtension;
            switch (qualityLabel)
            {
                case string q when q.StartsWith("Muxed:"):
                    fileExtension = q.EndsWith(".mp4") ? ".mp4" : ".webm";
                    var muxedQualityLabel = q.Substring(6).Trim().Replace(" | mp4", "").Replace(" | webm", "");
                    var muxedOutputPath = Path.Combine(outputDirectory, $"{outputFileName}{fileExtension}");
                    await _youTubeService.DownloadVideoAsync(videoUrl, muxedQualityLabel, muxedOutputPath);
                    return PhysicalFile(muxedOutputPath, "video/mp4", $"{outputFileName}{fileExtension}");

                case string q when q.StartsWith("Video-only:"):
                    fileExtension = q.EndsWith(".mp4") ? ".mp4" : ".webm";
                    var videoOnlyQualityLabel = q.Substring(11).Trim().Replace(" | mp4", "").Replace(" | webm", "");
                    var videoOnlyOutputPath = Path.Combine(outputDirectory, $"{outputFileName}{fileExtension}");
                    await _youTubeService.DownloadVideoOnlyAsync(videoUrl, videoOnlyQualityLabel, videoOnlyOutputPath);
                    return PhysicalFile(videoOnlyOutputPath, "video/mp4", $"{outputFileName}{fileExtension}");

                case string q when q.StartsWith("Audio-only:"):
                    var audioOnlyQualityLabel = q.Substring(11).Trim().Replace(" | mp3", "");
                    var audioOnlyOutputPath = Path.Combine(outputDirectory, $"{outputFileName}.mp3");
                    await _youTubeService.DownloadAudioOnlyAsync(videoUrl, audioOnlyQualityLabel, audioOnlyOutputPath);
                    return PhysicalFile(audioOnlyOutputPath, "audio/mp3", $"{outputFileName}.mp3");

                default:
                    return BadRequest("Invalid quality selection");
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetThumbnail(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("Invalid URL");
            }

            var video = await _youTubeService.GetVideoAsync(url);
            if (video == null || video.Thumbnails == null || !video.Thumbnails.Any())
            {
                return BadRequest("Thumbnail not found");
            }

            var thumbnail = video.Thumbnails.OrderByDescending(t => t.Resolution.Width * t.Resolution.Height).FirstOrDefault();
            if (thumbnail == null)
            {
                return BadRequest("Thumbnail not found");
            }

            return Json(thumbnail.Url);
        }

        [HttpPost]
        public async Task<IActionResult> GetQualities([FromBody] VideoUrlRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.VideoUrl))
            {
                return BadRequest("Invalid URL");
            }
            ViewData["VideoTitle"] = await _youTubeService.GetTitle(request.VideoUrl);

            var allStreams = await _youTubeService.GetAllStreamsAsync(request.VideoUrl);
            var qualities = allStreams.Select(s =>
            {

                var type = s is MuxedStreamInfo ? "Muxed" : s is VideoOnlyStreamInfo ? "Video-only" : "Audio-only";
                var quality = s is VideoOnlyStreamInfo videoStream ? videoStream.VideoQuality.Label : s is AudioOnlyStreamInfo audioStream ? audioStream.Bitrate.ToString() : s is MuxedStreamInfo msi ? msi.VideoQuality.Label : "Unknown";
                var container = s is AudioOnlyStreamInfo audio ? "mp3" : s.Container.ToString();
                return $"{type}: {quality} | {container}";
            }).ToList();


            return Json(qualities);
        }

        [HttpGet]
        public async Task<IActionResult> GetVideoTitle(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest("Invalid URL");
            }

            var title = await _youTubeService.GetTitle(url);
            if (string.IsNullOrEmpty(title))
            {
                return NotFound("Title not found");
            }

            return Ok(title);
        }

        [HttpPost]
        public IActionResult CheckDownloadStatus(string videoUrl)
        {
            bool isDownloaded = true;

            if (isDownloaded)
            {
                return Ok("Downloaded");
            }
            else
            {
                return NotFound();
            }
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class VideoUrlRequest
    {
        public string VideoUrl { get; set; }
    }
}

