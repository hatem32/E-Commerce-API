namespace AdminDashboard.Helpers
{
    // Mirrors E_Commerce.Application.Profiles.PictureUrlResolver, which is what the API
    // uses to turn the relative PictureUrl stored in the database into a full URL.
    // The Admin Dashboard reads Product entities directly from the database (not through
    // the API's DTOs), so it needs to build that same full URL itself before rendering it
    // in an <img> tag.
    public static class ImageUrlHelper
    {
        private const string FallbackImage = "/favicon.ico";

        public static string ToFullImageUrl(this string? pictureUrl, string? baseUrl)
        {
            if (string.IsNullOrWhiteSpace(pictureUrl))
            {
                return FallbackImage;
            }

            // Already a full URL (e.g. manually pasted into the Picture URL field).
            if (pictureUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                pictureUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return pictureUrl;
            }

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return FallbackImage;
            }

            var trimmedBase = baseUrl.TrimEnd('/');
            var trimmedPath = pictureUrl.TrimStart('/');

            return $"{trimmedBase}/Files/{trimmedPath}";
        }
    }
}