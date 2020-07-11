using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV.Util;
using Emgu.CV.Ocl;
using System.Diagnostics;

namespace Imagecaptureexp
{
    public partial class Form1 : Form
    {
        public EigenFaceRecognizer faceRecognizer { get; set; }
        public List<Image<Gray, byte>> Faces { get; set; }
        public List<int> ID { get; set; }
        public int timercounter { get; set; } = 0;
        public int timeelimit { get; set; } = 30;

        public int scancounter { get; set; } = 0;

        public string ymlpath { get; set; } = @"C:\games\image\harc.xml";

        List<Mat> Faces1 = new List<Mat>();
        public Form1()
        {
            InitializeComponent();
            faceRecognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);
            ID = new List<int>();
        }

        FilterInfoCollection filter;
        VideoCaptureDevice device;
        Rectangle[] rects;
        int imagenumber = 0;


        private void Form1_Load(object sender, EventArgs e)
        {
            filter = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            device = new VideoCaptureDevice(filter[0].MonikerString);
            device.NewFrame += Device_NewFrame;
            device.NewFrame += takesnap;
            Console.WriteLine(111);
            device.Start();
            watch = Stopwatch.StartNew();
            IDbox.Invoke(new Action(() => IDbox.Text = "Enter Some value and click Train"));

        }

        public Stopwatch watch { get; set; }

        private void takesnap(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            try
            {
                var filter = new Mirror(false, true);
                filter.ApplyInPlace(bitmap);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
            Image<Bgr, byte> capturedimage = new Image<Bgr, byte>(bitmap);

            Image<Gray, byte> gray_image = capturedimage.Convert<Gray, byte>();

            
            

            if (rects.Length > 0)
            {
                string j = "image" + imagenumber + ".jpg";
                //capturedimage.Save(string.Format(@"C:\games\image\{0}", j));
                imagenumber = imagenumber + 1;
            }


        }
        static readonly CascadeClassifier faceClassifier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");
        private void Device_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {

            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            try
            {
                var filter = new Mirror(false, true);
                filter.ApplyInPlace(bitmap);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Image<Bgr, byte> capturedimage = new Image<Bgr, byte>(bitmap);

            Image<Gray, byte> image = capturedimage.Convert<Gray, byte>();
            Rectangle[] facerectangles = faceClassifier.DetectMultiScale(capturedimage, 1.2, 1);

            if (facerectangles.Length > 0)
            {
                int NewpointX = 472 - facerectangles[0].X;
                int NewpointY = 240 - facerectangles[0].Y;
                Console.WriteLine(facerectangles[0].X);
                Console.WriteLine(facerectangles[0].Y);
                textBox1.Invoke(new Action(() => textBox1.Text = facerectangles[0].X.ToString()));
                textBox2.Invoke(new Action(() => textBox2.Text = NewpointY.ToString()));
            }

            rects = facerectangles;

            foreach (Rectangle rectangle in facerectangles)
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    using (Pen pen = new Pen(Color.Green, 1))
                    {
                        graphics.DrawRectangle(pen, rectangle);
                    }
                }

            }

            picbox.Image = bitmap;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (device.IsRunning)
            {
                Application.Exit();
                device.Stop();
            }
        }
        string i;
        private void Train_Click(object sender, EventArgs e)
        {
            device.NewFrame += trainmodel;
        }

        private void trainmodel(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            try
            {
                var filter = new Mirror(false, true);
                filter.ApplyInPlace(bitmap);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Image<Bgr, byte> capturedimage = new Image<Bgr, byte>(bitmap);
            Image<Gray, byte> gray_image = capturedimage.Convert<Gray, byte>();

            if (IDbox.Text != "Tranning Stopped")
            {
                Rectangle[] faces = faceClassifier.DetectMultiScale(gray_image, 1.2, 1);

                if (faces.Count() > 0)
                {
                    var processedimage = gray_image.Copy(faces[0]).Resize(120, 120, Emgu.CV.CvEnum.Inter.Cubic);
                    processedimage.Save(string.Format(@"C:\games\image\img.jpg"));
                    Faces1.Add(processedimage.Mat);
                    ID.Add(Convert.ToInt32(IDbox.Text));
                    i = IDbox.Text;
                    scancounter++;
                    textBox3.Invoke(new Action(() => textBox3.Text = $"{scancounter}Succesful scans taken"));

                }

                if (scancounter == 100)
                {
                    watch = Stopwatch.StartNew();
                    faceRecognizer.Train(Faces1.ToArray(), ID.ToArray());
                    faceRecognizer.Write(ymlpath);
                    textBox3.Invoke(new Action(() => textBox3.Text = "Tranning complete"));
                    IDbox.Invoke(new Action(() => IDbox.Text = "Tranning Stopped"));
                    
                }

                   

            }

        }

        private void Predict_Click(object sender, EventArgs e)
        {
            device.NewFrame += predict;
        }

        private void predict(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            try
            {
                var filter = new Mirror(false, true);
                filter.ApplyInPlace(bitmap);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
            Image<Bgr, byte> capturedimage = new Image<Bgr, byte>(bitmap);

            Image<Gray, byte> gray_image = capturedimage.Convert<Gray, byte>();

            Rectangle[] faces = faceClassifier.DetectMultiScale(gray_image, 1.2, 1);

            if (faces.Count() > 0)
            {
                var processedimage = gray_image.Copy(faces[0]).Resize(120, 120, Emgu.CV.CvEnum.Inter.Cubic);
                var result = faceRecognizer.Predict(processedimage);

                if (result.Label.ToString() == i)
                {
                    textBox4.Invoke(new Action(() => textBox4.Text = "YOURNAME"));
                }

                if((result.Label.ToString() != i))
                {
                    textBox4.Invoke(new Action(() => textBox4.Text = "Not YOURNAME :("));
                }
            }
        }

       
    }
}
    

