using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace IDI.Framework
{
    public class FaceDetectionRecognition
    {
        private readonly HaarCascade _face;
        private readonly HaarCascade _mouth;

        private const int FaceDataWidth = 100;
        private const int FaceDataHeight = 100;

        public FaceDetectionRecognition()
        {
            _face = new HaarCascade("Cascades/haarcascade_frontalface_alt_tree.xml");
            _mouth = new HaarCascade("Cascades/haarcascade_mcs_mouth.xml");
        }

        /// <summary>
        /// This get the face detected by the pixel data
        /// NOTE: This will only return the first face data with a visible mouth
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public Image<Gray, byte> GetDetectedFace(byte[] pixelData, int height, int width)
        {
            var bitmap = BytesToBitmap(pixelData, height, width);
            var image = new Image<Bgr, byte>(bitmap);
            var grayImage = image.Convert<Gray, Byte>();

            //Face Detector
            var facesDetected = _face.Detect(grayImage, 1.2, 10, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));

            //Action for each element detected
            foreach (var faceFound in facesDetected)
            {
                var face = image.Copy(faceFound.rect).Convert<Gray, byte>().Resize(FaceDataWidth, FaceDataHeight, INTER.CV_INTER_CUBIC);

                face._EqualizeHist();
                if (IsMouthDetected(face))
                {
                    return face;
                }
            }

            return null;
        }

        private static Bitmap BytesToBitmap(byte[] pixelData, int height, int width)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            var ptr = bitmapData.Scan0;

            Marshal.Copy(pixelData, 0, ptr, pixelData.Length);
            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        /// <summary>
        /// This method detect true if we found a mouth in the image
        /// NOTE: The idea is to transform this method in true, when the user is speaking (mouth open)
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private bool IsMouthDetected(Image<Gray, byte> face)
        {
            var detectRectangle = new Rectangle(0, face.Height * 2 / 3, face.Width, face.Height / 3);
            var whereMouthShouldBe = face.GetSubRect(detectRectangle);
            var mouths = _mouth.Detect(whereMouthShouldBe, 1.2, 10, HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(5, 5));

            return mouths.Any();
        }
    }
}