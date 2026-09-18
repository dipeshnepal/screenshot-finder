using System.Drawing;
using System.Runtime.InteropServices;

namespace PhotoFindExtensions;

public sealed class ImageAnalyzer
{
    private static readonly HashSet<string> PhotoExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bmp", ".gif", ".jpeg", ".jpg", ".png", ".tiff", ".webp"
    };

    private static readonly int[] CameraExifTags =
    {
        271, 272, 305, 306, 36867, 36868, 42016, 42032, 42033
    };

    public List<string> ProcessDirectory(string directoryPath)
    {
        var matchedFiles = new List<string>();

        foreach (var file in EnumerateFiles(directoryPath))
        {
            if (!PhotoExtensions.Contains(Path.GetExtension(file)))
            {
                continue;
            }

            if (IsScreenshot(file))
            {
                matchedFiles.Add(file);
            }
        }

        return matchedFiles;
    }

    private static IEnumerable<string> EnumerateFiles(string directoryPath)
    {
        var pendingDirectories = new Stack<string>();
        pendingDirectories.Push(directoryPath);

        while (pendingDirectories.Count > 0)
        {
            var currentDirectory = pendingDirectories.Pop();

            string[] files;
            try
            {
                files = Directory.GetFiles(currentDirectory);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var file in files)
            {
                yield return file;
            }

            string[] childDirectories;
            try
            {
                childDirectories = Directory.GetDirectories(currentDirectory);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                continue;
            }

            foreach (var childDirectory in childDirectories)
            {
                pendingDirectories.Push(childDirectory);
            }
        }
    }

    private static bool IsScreenshot(string filePath)
    {
        try
        {
            using var image = Image.FromFile(filePath);
            var score = 0;

            if (image.Height > image.Width
                && image.Width is >= 800 and <= 1600
                && image.Height is >= 1500 and <= 4000
                && (double)image.Height / image.Width >= 1.8)
            {
                score += 3;
            }

            var hasCameraTags = image.PropertyIdList.Any(id => Array.Exists(CameraExifTags, tag => tag == id));
            if (!hasCameraTags)
            {
                score++;
            }

            using var bitmap = new Bitmap(image, new Size(128, 128));
            var adjacentPairs = Math.Max(1, bitmap.Width * (bitmap.Height - 1) + (bitmap.Width - 1) * bitmap.Height);
            var flatPixels = 0;
            var edgePixels = 0;

            for (var y = 0; y < bitmap.Height; y++)
            {
                for (var x = 0; x < bitmap.Width; x++)
                {
                    var currentGray = ToGray(bitmap.GetPixel(x, y));

                    if (x + 1 < bitmap.Width)
                    {
                        CountDifference(currentGray, ToGray(bitmap.GetPixel(x + 1, y)), ref flatPixels, ref edgePixels);
                    }

                    if (y + 1 < bitmap.Height)
                    {
                        CountDifference(currentGray, ToGray(bitmap.GetPixel(x, y + 1)), ref flatPixels, ref edgePixels);
                    }
                }
            }

            var flatFraction = (double)flatPixels / adjacentPairs;
            var edgeFraction = (double)edgePixels / adjacentPairs;

            if (flatFraction >= 0.65)
            {
                score++;
            }

            if (edgeFraction <= 0.065)
            {
                score++;
            }

            return score >= 4;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (ExternalException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static int ToGray(Color color) => (int)(color.R * 0.3 + color.G * 0.59 + color.B * 0.11);

    private static void CountDifference(int first, int second, ref int flatPixels, ref int edgePixels)
    {
        var difference = Math.Abs(first - second);

        if (difference <= 3)
        {
            flatPixels++;
        }

        if (difference >= 40)
        {
            edgePixels++;
        }
    }
}