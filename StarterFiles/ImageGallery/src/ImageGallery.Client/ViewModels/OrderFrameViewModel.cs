using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.Client.ViewModels
{
    public class OrderFrameViewModel
    {
        public string Address { get; private set; } = string.Empty;
        public string ExtraInfo { get; set; }
        public string Role { get; set; }

        public OrderFrameViewModel(string address)
        {
            Address = address;
        }
    }
}
