using System;
using System.Collections.Generic;
using System.Linq;

namespace ImageGallery.Client.Services
{
    public class GalleryRepository : IDisposable
    {
        GalleryContext _context;

        public GalleryRepository(GalleryContext galleryContext)
        {
            _context = galleryContext;
        }
        public bool ImageExists(Guid id)
        {
            return _context.Images.Any(i => i.Id == id);
        }       

        public GalleryImg GetImage(Guid id)
        {
            return _context.Images.FirstOrDefault(i => i.Id == id);
        }
  
        public IEnumerable<GalleryImg> GetImages(string ownerId, bool isAdmin)
        {
            if (isAdmin)
            {
                return _context.Images.OrderBy(i => i.Title).ToList();
            }
            return _context.Images.Where(i=>i.OwnerId==ownerId)
                .OrderBy(i => i.Title).ToList();
        }

        public bool IsImageOwner(Guid id, string ownerId)
        {
            return _context.Images.Any(i => i.Id == id && i.OwnerId == ownerId);
        }


        public void AddImage(GalleryImg image)
        {
            _context.Images.Add(image);
        }

        public void UpdateImage(GalleryImg image)
        {
            // no code in this implementation
        }

        public void DeleteImage(GalleryImg image)
        {
            _context.Images.Remove(image);

            // Note: in a real-life scenario, the image itself should also 
            // be removed from disk.  We don't do this in this demo
            // scenario, as we refill the DB with image URIs (that require
            // the actual files as well) for demo purposes.
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }

            }
        }     
    }
}
