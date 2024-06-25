using System.Diagnostics;

namespace MultimediaService.Services
{
    public class FFmpeg
    {
        static string ffmpegPath = @"C:\ffmpeg\bin\ffmpeg.exe";
        public static async Task<bool> CompressVideoAsync(string inputFilePath, string outputFilePath)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-i \"{inputFilePath}\" -vcodec libx264 -crf 28 \"{outputFilePath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                using (var process = new Process { StartInfo = startInfo })
                {
                    var stdOutput = new StringWriter();
                    var stdError = new StringWriter();

                    process.OutputDataReceived += (sender, args) => stdOutput.WriteLine(args.Data);
                    process.ErrorDataReceived += (sender, args) => stdError.WriteLine(args.Data);

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        // Log error
                        Debug.WriteLine("FFmpeg Error: " + stdError.ToString());
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
                return false;
            }

            return true;
        }
        public static async Task AddMetadataAsync(string filePath)
        {
            var metadata = new Dictionary<string, string>
            {
                { "title", "Example Video" },
                { "author", "Your Name" },
                { "description", "This is an example video with metadata." },
                { "comment", "Encoded using FFmpeg" }
            };

            foreach (var item in metadata)
            {
                var tempFilePath = $"{filePath}_temp.mp4";
                var startInfo = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = $"-i \"{filePath}\" -metadata {item.Key}=\"{item.Value}\" -codec copy \"{tempFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                try
                {
                    using (var process = new Process { StartInfo = startInfo })
                    {
                        var stdOutput = new StringWriter();
                        var stdError = new StringWriter();

                        process.OutputDataReceived += (sender, args) => stdOutput.WriteLine(args.Data);
                        process.ErrorDataReceived += (sender, args) => stdError.WriteLine(args.Data);

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        await process.WaitForExitAsync();

                        if (process.ExitCode != 0)
                        {
                            Debug.WriteLine("FFmpeg Error: " + stdError.ToString());
                            return;
                        }
                    }

                    System.IO.File.Delete(filePath);
                    System.IO.File.Move(tempFilePath, filePath);
                }
                catch (Exception ex)
                {
                    // Log exception
                    Debug.WriteLine("Exception: " + ex.Message);
                    return;
                }
            }
        }
        public static void CompressImage(string inputPath, string outputPath)
        {
            var arguments = $"-i \"{inputPath}\" -qscale:v 20 \"{outputPath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg exited with code {process.ExitCode}: {error}");
                }
            }
        }
        public static void CompressAudio(string inputPath, string outputPath)
        {
            var arguments = $"-i \"{inputPath}\" -b:a 128k \"{outputPath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                process.WaitForExit();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg exited with code {process.ExitCode}: {error}");
                }
            }
        }
        public static string RandomName(int length)
        {
            if (length < 10) throw new Exception("Length must be 20 or above!");
            var random = new Random();
            string name = DateTime.Now.ToString("yyyyMMddHHmmss"); 
            for (int i = name.Length; i < length; i++)
            {
                name += random.Next(10);
            }

            return name;
        }

    }
}
