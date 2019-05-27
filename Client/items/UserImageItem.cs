using System.Windows.Media.Imaging;
using Client.ServiceReference;

namespace Client
{
    public class UserImageItem
    {
        public AvatarWCF AvatarWCF { get; set; }
        public BitmapSource BitmapSource { get; set; }
    }
}