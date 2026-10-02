using System.Text.RegularExpressions;

public class YoutubeParse
{
    private static string videoIdPattern = @"(?:https?:\/\/)?(?:www\.)?(?:youtube\.com\/(?:embed\/|v\/|shorts\/|watch\?(?:[^#&]+&)*v=)|youtu\.be\/)([a-zA-Z0-9_-]{11})";
    
    public static string GetYouTubeVideoID(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        url = url.Trim();
        if (Regex.IsMatch(url, @"^[a-zA-Z0-9_-]{11}$"))
        {
            return url;
        }

        Regex regex = new Regex(videoIdPattern);
        Match match = regex.Match(url);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }

        return null;
    }
}
