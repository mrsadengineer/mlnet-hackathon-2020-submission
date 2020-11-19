using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



using Emgu.CV;
using Emgu.CV.Structure;

using System.Runtime.InteropServices;

using System.IO;
using OnnxObjectDetection;
using Microsoft.ML;
using System.Threading;
using OpenCvSharp;
using System.Drawing;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace WpfApp1
{

    public partial class MainWindow : System.Windows.Window
    {




        #region first main
        Emgu.CV.VideoCapture m_capture = new Emgu.CV.VideoCapture();
        Emgu.CV.VideoWriter videoWriter;

        double totalFrames;
        double fps;

        string i = "nameofrecording";
        string destin = "";//"E:\\rtest\\"; //"C:\\Users\\ITNOA\\Desktop\\savedVideoDHS\\"

        bool fileChanged;
        System.Windows.Threading.DispatcherTimer dispatcherTimer; //= new System.Windows.Threading.DispatcherTimer();

        int snapshotIndex = 0;
        private bool takeSnapshot = false;

        public int startIndex = 0;

        #endregion




        private CancellationTokenSource cameraCaptureCancellationTokenSource;
        private OnnxOutputParser outputParser;
        private PredictionEngine<ImageInputData, TinyYoloPrediction> tinyYoloPredictionEngine;
        private PredictionEngine<ImageInputData, CustomVisionPrediction> customVisionPredictionEngine;

        private static readonly string modelsDirectory = System.IO.Path.Combine(Environment.CurrentDirectory, @"ML\OnnxModels");




        public MainWindow()
        {

            #region first main 

            InitializeComponent();

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);


            fileChanged = true;
            LoadModel();

            #endregion






        }



        private void LoadModel()
        {
            // Check for an Onnx model exported from Custom Vision

            var customVisionExport = Directory.GetFiles(modelsDirectory, "*.zip").FirstOrDefault();

            // If there is one, use it.
            if (customVisionExport != null)
            {
                var customVisionModel = new CustomVisionModel(customVisionExport);
                var modelConfigurator = new OnnxModelConfigurator(customVisionModel);

                outputParser = new OnnxOutputParser(customVisionModel);
                customVisionPredictionEngine = modelConfigurator.GetMlNetPredictionEngine<CustomVisionPrediction>();
            }
            else // Otherwise default to Tiny Yolo Onnx model
            {
                var tinyYoloModel = new TinyYoloModel(System.IO.Path.Combine(modelsDirectory, "TinyYolo2_model.onnx"));
                var modelConfigurator = new OnnxModelConfigurator(tinyYoloModel);

                outputParser = new OnnxOutputParser(tinyYoloModel);
                tinyYoloPredictionEngine = modelConfigurator.GetMlNetPredictionEngine<TinyYoloPrediction>();
            }
        }






        private void M_capture_ImageGrabbed(object sender, EventArgs e)
        {
            // Console.WriteLine("test: "  + startIndex.ToString());
            //  startIndex++;

            destin = SaveRecordingLocation_textbox.Text;

            if (fileChanged)
            {
                // destin = SaveRecordingLocation_textbox.Text;
                totalFrames = m_capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameCount);
                fps = m_capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps);
                int fourcc = Convert.ToInt32(m_capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FourCC));
                int frameHeight = Convert.ToInt32(m_capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight));
                int frameWidth = Convert.ToInt32(m_capture.GetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth));
                string destination = destin + i + ".avi";
                videoWriter = new Emgu.CV.VideoWriter(destination, Emgu.CV.VideoWriter.Fourcc('I', 'Y', 'U', 'V'), fps, new System.Drawing.Size(frameWidth, frameHeight), true);
                fileChanged = false;
            }


            Emgu.CV.Mat m = new Emgu.CV.Mat();
            m_capture.Retrieve(m);
            // pictureBox1.Image = m.ToImage<Bgr, byte>().Bitmap;
            videoWriter.Write(m);





            //throw new NotImplementedException();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            if (m_capture == null)
                m_capture = new Emgu.CV.VideoCapture();




            using (Image<Bgr, byte> nextFrame = m_capture.QueryFrame().ToImage<Bgr, Byte>())
            {
                if (nextFrame != null)
                {
                    Image<Gray, byte> grayframe = nextFrame.Convert<Gray, byte>();


                    TestImage1.Source = ToBitmapSource(nextFrame);


                    if (takeSnapshot)
                    {
                        nextFrame.Save(SnapshotLocation_textbox.Text + "\\image" + snapshotIndex.ToString().PadLeft(3, '0') + ".jpg");
                        takeSnapshot = !takeSnapshot;
                        snapshotIndex++;
                    }


                }
            }
        }


        #region before feature
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(Image<Bgr, Byte> image)
        {
            using (System.Drawing.Bitmap source = image.ToBitmap())
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap  

                BitmapSource bs = System.Windows.Interop
                  .Imaging.CreateBitmapSourceFromHBitmap(
                  ptr,
                  IntPtr.Zero,
                  Int32Rect.Empty,
                  System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap  
                return bs;
            }
        }

        private void StartWebCamButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Start();
        }

        private void StopWebCamButton_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            TestImage1.Source = null;
        }

        private void PauseWebCamButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();

            //should check if dispatchertimer is enabled
            //stop displa
        }

        private void TakeSnapshotButton_Click_1(object sender, RoutedEventArgs e)
        {
            takeSnapshot = !takeSnapshot;
        }

        private void RecordVideoButton_Click_2(object sender, RoutedEventArgs e)
        {
            m_capture.ImageGrabbed += M_capture_ImageGrabbed;
        }

        private void PlayVideoButton_Click_3(object sender, RoutedEventArgs e)
        {

        }

        private void StopRecordingButton_Click(object sender, RoutedEventArgs e)
        {
            m_capture.ImageGrabbed -= M_capture_ImageGrabbed;



        }

        #endregion



        #region

        private void StartPreviewCameraButton_Click(object sender, RoutedEventArgs e)
        {
            StartCameraCapture();
        }

        private void StopPreviewCameraButton_Click(object sender, RoutedEventArgs e)
        {
            StopCameraCapture();
        }


        private void StartCameraCapture()
        {
            cameraCaptureCancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => CaptureCamera(cameraCaptureCancellationTokenSource.Token), cameraCaptureCancellationTokenSource.Token);
        }

        private void StopCameraCapture() => cameraCaptureCancellationTokenSource?.Cancel();

        private OpenCvSharp.VideoCapture capture;

        private async Task CaptureCamera(CancellationToken token)
        {
            if (capture == null)
                capture = new OpenCvSharp.VideoCapture(CaptureDevice.DShow);

            capture.Open(0);

            m_capture.Start();

            if (capture.IsOpened())
            //  if(m_capture.IsOpened)
            {
                while (!token.IsCancellationRequested)
                {
                    using MemoryStream memoryStream = capture.RetrieveMat().Flip(FlipMode.Y).ToMemoryStream();
                    //  using MemoryStream memoryStream = m_capture.QueryFrame()..RetrieveMat().Flip(FlipMode.Y).ToMemoryStream();


                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var imageSource = new BitmapImage();

                        imageSource.BeginInit();
                        imageSource.CacheOption = BitmapCacheOption.OnLoad;
                        imageSource.StreamSource = memoryStream;
                        imageSource.EndInit();

                        OpenCVSharpImageSource.Source = imageSource;
                    });

                    var bitmapImage = new Bitmap(memoryStream);

                    await ParseWebCamFrame(bitmapImage, token);
                }

                capture.Release();
            }
        }

        async Task ParseWebCamFrame(Bitmap bitmap, CancellationToken token)
        {
            if (customVisionPredictionEngine == null && tinyYoloPredictionEngine == null)
                return;

            var frame = new ImageInputData { Image = bitmap };
            var filteredBoxes = DetectObjectsUsingModel(frame);

            if (!token.IsCancellationRequested)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    DrawOverlays(filteredBoxes, OpenCVSharpImageSource.ActualHeight, OpenCVSharpImageSource.ActualWidth);
                });
            }
        }



        public List<BoundingBox> DetectObjectsUsingModel(ImageInputData imageInputData)
        {
            var labels = customVisionPredictionEngine?.Predict(imageInputData).PredictedLabels ?? tinyYoloPredictionEngine?.Predict(imageInputData).PredictedLabels;
            var boundingBoxes = outputParser.ParseOutputs(labels);
            var filteredBoxes = outputParser.FilterBoundingBoxes(boundingBoxes, 5, 0.5f);
            return filteredBoxes;
        }

        private void DrawOverlays(List<BoundingBox> filteredBoxes, double originalHeight, double originalWidth)
        {
            WebCamCanvas.Children.Clear();

            foreach (var box in filteredBoxes)
            {
                // process output boxes
                double x = Math.Max(box.Dimensions.X, 0);
                double y = Math.Max(box.Dimensions.Y, 0);
                double width = Math.Min(originalWidth - x, box.Dimensions.Width);
                double height = Math.Min(originalHeight - y, box.Dimensions.Height);

                // fit to current image size
                x = originalWidth * x / ImageSettings.imageWidth;
                y = originalHeight * y / ImageSettings.imageHeight;
                width = originalWidth * width / ImageSettings.imageWidth;
                height = originalHeight * height / ImageSettings.imageHeight;

                var boxColor = box.BoxColor.ToMediaColor();

                var objBox = new Rectangle
                {
                    Width = width,
                    Height = height,
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(boxColor),
                    StrokeThickness = 2.0,
                    Margin = new Thickness(x, y, 0, 0)
                };

                var objDescription = new TextBlock
                {
                    Margin = new Thickness(x + 4, y + 4, 0, 0),
                    Text = box.Description,
                    FontWeight = FontWeights.Bold,
                    Width = 126,
                    Height = 21,
                    TextAlignment = TextAlignment.Center
                };

                var objDescriptionBackground = new Rectangle
                {
                    Width = 134,
                    Height = 29,
                    Fill = new SolidColorBrush(boxColor),
                    Margin = new Thickness(x, y, 0, 0)
                };

                WebCamCanvas.Children.Add(objDescriptionBackground);
                WebCamCanvas.Children.Add(objDescription);
                WebCamCanvas.Children.Add(objBox);
            }
        }

        #endregion



    }

    internal static class ColorExtensions
    {
        internal static System.Windows.Media.Color ToMediaColor(this System.Drawing.Color drawingColor)
        {
            return System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }
    }
}






//   var faces = m_cascade.DetectMultiScale(grayframe, 1.1, 3, new System.Drawing.Size(20, 20));

//foreach (var face in faces)
//{
//    nextFrame.Draw(face, new Bgr(0, 0, 0), 3);
//}