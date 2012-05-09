using System.Drawing;
using System.IO;

namespace IDI.Framework.Model
{
    public class Face
    {
        public byte[] PixelData { get; set; }

        public int Height { get; set; }

        public int Width { get; set; }

        public Bitmap GetBitmap()
        {
            using (var ms = new MemoryStream(PixelData))
            {
                var bitmap = new Bitmap(ms);
                return bitmap;
            }

        }      
    }
}