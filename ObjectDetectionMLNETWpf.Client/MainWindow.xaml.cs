using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace ObjectDetectionMLNETWpf.Client
{

    public partial class MainWindow : Window
    {
        VideoCapture m_capture = new VideoCapture();


        VideoWriter videoWriter;

        double totalFrames;
        double fps;

        string i = "nameofrecording";
        string destin = "";//"E:\\rtest\\"; //"C:\\Users\\ITNOA\\Desktop\\savedVideoDHS\\"

        bool fileChanged;
        System.Windows.Threading.DispatcherTimer dispatcherTimer; //= new System.Windows.Threading.DispatcherTimer();

        int snapshotIndex = 0;
        private bool takeSnapshot = false;
       
        public int startIndex = 0;
      
        
        
        public MainWindow()
        {
            InitializeComponent();

            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            //dispatcherTimer.Start();

            


            m_capture = new VideoCapture();

            fileChanged = true;
          

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
                videoWriter = new VideoWriter(destination, VideoWriter.Fourcc('I', 'Y', 'U', 'V'), fps, new System.Drawing.Size(frameWidth, frameHeight), true);
                fileChanged = false;
            }


            Mat m = new Mat();
            m_capture.Retrieve(m);
           // pictureBox1.Image = m.ToImage<Bgr, byte>().Bitmap;
            videoWriter.Write(m);





            //throw new NotImplementedException();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
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

        //##################


        private void TakeSnapshotButton_Click_1(object sender, RoutedEventArgs e)
        {
            takeSnapshot = !takeSnapshot;
        } 



     //###########################
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

    }
}






//   var faces = m_cascade.DetectMultiScale(grayframe, 1.1, 3, new System.Drawing.Size(20, 20));

//foreach (var face in faces)
//{
//    nextFrame.Draw(face, new Bgr(0, 0, 0), 3);
//}