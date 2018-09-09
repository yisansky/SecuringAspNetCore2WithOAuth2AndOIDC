using Microsoft.EntityFrameworkCore;


namespace ImageGallery.Client.Services
{  
        public class GalleryContext : DbContext
        {
            public GalleryContext(DbContextOptions<GalleryContext> options)
               : base(options)
            {
            }

            public DbSet<GalleryImg> Images { get; set; }
        }
}
