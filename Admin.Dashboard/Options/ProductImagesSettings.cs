namespace AdminDashboard.Options
{
    // Points to the physical "Files/images/products" folder inside the E_Commerce.API
    // project, so uploads from this dashboard land exactly where the API serves them from.
    public class ProductImagesSettings
    {
        public string PhysicalPath { get; set; } = default!;

        // Relative path prefix stored in the database / used to build the public URL,
        // matching what E_Commerce.Application.Profiles.PictureUrlResolver expects.
        public string RelativePath { get; set; } = "images/products";
    }
}