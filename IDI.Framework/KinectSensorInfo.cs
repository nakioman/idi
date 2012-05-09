using System.Linq;
using IDI.Framework.Exceptions;
using IDI.Framework.Model;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

namespace IDI.Framework
{
    public class KinectSensorInfo
    {
        private readonly KinectSensor _sensor;
        private readonly SpeechRecognitionEngine _speechRecognitionEngine;
        private bool _speechRecognitionOn;
        private readonly FaceDetectionRecognition _face;

        public User RecognizedUser { get; set; }

        public bool IsStarted { get; set; }

        public KinectSensorInfo(SpeechRecognitionEngine speechRecognitionEngine)
        {
            _face = new FaceDetectionRecognition();
            _speechRecognitionEngine = speechRecognitionEngine;
            _sensor = KinectSensor.KinectSensors.FirstOrDefault();
            if (_sensor == null)
            {
                throw new IDIRuntimeException("Can't find kinect sensor, is it connected?", null);
            }

            _sensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
            _sensor.SkeletonStream.Disable();
            _sensor.DepthStream.Disable();
            
            var audioSource = _sensor.AudioSource;
            audioSource.BeamAngleMode = BeamAngleMode.Adaptive;
            audioSource.EchoCancellationMode = EchoCancellationMode.CancellationAndSuppression;
            audioSource.NoiseSuppression = true;

            _sensor.Start();

            var kinectStream = audioSource.Start();

            _speechRecognitionEngine.SetInputToAudioStream(kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
        }

        public void Start()
        {
            _sensor.ColorFrameReady += ColorFrameReady;
        }

        private void ColorFrameReady(object sender, ColorImageFrameReadyEventArgs e)
        {
            using (var colorImageFrame = e.OpenColorImageFrame())
            {
                if (colorImageFrame == null) return;

                lock (_face)
                {
                    var pixelData = new byte[colorImageFrame.PixelDataLength];
                    colorImageFrame.CopyPixelDataTo(pixelData);
                    var detectedFace = _face.GetDetectedFace(pixelData, colorImageFrame.Height, colorImageFrame.Width);

                    if (detectedFace != null)
                    {
                        //if (!_speechRecognitionOn)
                        //{
                            _speechRecognitionOn = true;
                        var lalal = _speechRecognitionEngine.Recognize();
                        //}

                        RecognizedUser = _face.RecognizeFace(detectedFace);
                    }
                    else
                    {
                        _speechRecognitionEngine.RecognizeAsyncStop();
                        _speechRecognitionOn = false;
                    }
                }
            }
        }

        public void Stop()
        {
            _sensor.ColorFrameReady -= ColorFrameReady;
        }
    }
}