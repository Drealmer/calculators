using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;

namespace mkgen
{
    static class ImageExtensions
    {
        public static void NormalizeOrientation(this Image img)
        {
            if (Array.IndexOf(img.PropertyIdList, 274) > -1)
            {
                var orientation = (int)img.GetPropertyItem(274).Value[0];
                switch (orientation)
                {
                    case 1:
                        // No rotation required.
                        break;
                    case 2:
                        img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        break;
                    case 3:
                        img.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        break;
                    case 4:
                        img.RotateFlip(RotateFlipType.Rotate180FlipX);
                        break;
                    case 5:
                        img.RotateFlip(RotateFlipType.Rotate90FlipX);
                        break;
                    case 6:
                        img.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        break;
                    case 7:
                        img.RotateFlip(RotateFlipType.Rotate270FlipX);
                        break;
                    case 8:
                        img.RotateFlip(RotateFlipType.Rotate270FlipNone);
                        break;
                }
                // This EXIF data is now invalid and should be removed.
                img.RemovePropertyItem(274);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using var input = new StreamReader("calculators.txt");
            using var output = new StreamWriter("../README.md");

            output.WriteLine("| Brand | Model | Picture |");
            output.WriteLine("|-|-|-|");

            var count = 0;

            string line;
            var data = new Dictionary<string, string>();
            do
            {
                line = input.ReadLine();
                //var thumb = GetThumbnail("IMG_20210925_203808");

                if (String.IsNullOrEmpty(line))
                {
                    if (data.Count > 0)
                    {
                        var brand = data.GetValueOrDefault("brand", "???");
                        var model = data.GetValueOrDefault("model", "???");
                        var picture = data.GetValueOrDefault("picture", null);
                        if(picture != null) picture = GetThumbnail(picture);
                        output.WriteLine($"| {brand} | {model} | ![]({picture})");
                    }
                    count += 1;
                    data.Clear();
                }
                else
                {
                    var match = Regex.Match(line, @"^(.+):(.+)$");
                    var key = match.Groups[1].ToString().Trim();
                    var value = match.Groups[2].ToString().Trim();
                    data.Add(key, value);
                }
            }
            while (line != null);

            output.WriteLine($"{count} entries in database");
        }

        private static string GetThumbnail(string filename)
        {
            var bitmap = Bitmap.FromFile($"Calculators/{filename}.jpg");
            bitmap.NormalizeOrientation();
            var resized = new Bitmap(bitmap, bitmap.Size / 20);

            var encoderInfo = GetEncoder();
            var result = $"thumbnails/{filename}.jpg";
            resized.Save("../" + result, encoderInfo.codec, encoderInfo.parameters);

            return result;
        }

        private static (ImageCodecInfo codec, EncoderParameters parameters) GetEncoder()
        {
            var codec = GetEncoderInfo("image/jpeg");
            var parameters = new EncoderParameters(1);
            parameters.Param[0] = new EncoderParameter(Encoder.Quality, 85L);
            return (codec, parameters);
        }

        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
    }
}
