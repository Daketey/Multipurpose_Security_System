using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using Face_Detection_in_forms.Properties;
using AForge.Imaging.Filters;
using System.IO.Ports;
using System.Runtime.CompilerServices;

namespace Face_Detection_in_forms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        FilterInfoCollection filter;
        VideoCaptureDevice device , device2;
        Rectangle[] rects;
        int i = 0;
        int k = 1;
        int imagenumber = 0;


        public Stopwatch watch { get; set; }
           
        
        private void Form1_Load(object sender, EventArgs e)
        {
            watch = Stopwatch.StartNew();

            filter = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo device in filter)
            {
                cboDevice.Items.Add(device.Name);
                //comboBox1.Items.Add(device.Name);
            }

            
            cboDevice.SelectedIndex = 0;
            //comboBox1.SelectedIndex = 0;
            serialPort1.Open();
            
                device = new VideoCaptureDevice(filter[cboDevice.SelectedIndex].MonikerString);
                
            device.NewFrame += Device_NewFrame;
            device.NewFrame += takesnap;            

            Console.WriteLine(111);
            int data_rx = serialPort1.ReadChar();
            Console.WriteLine(data_rx);
            i = data_rx;

            while (i < 1)
            {
                if (data_rx == 1)
                {
                    i = 1;
                }
            } 

            device.Start();

            writeToPort(new Point(280, 184));
            Console.WriteLine(i);
        }



        private void takesnap(object sender, NewFrameEventArgs e)
        {
            Bitmap bitmap = (Bitmap)e.Frame.Clone();
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

             

            if (watch.ElapsedMilliseconds > 3000)
            {
                watch = Stopwatch.StartNew();
                string j = "image" + imagenumber + ".jpg";
                capturedimage.Save(string.Format(@"C:\games\image\{0}", j));
                serialPort1.Write("N1");     //Here sending the string for calling
            }

        }

        static readonly CascadeClassifier faceClassifier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");
        static readonly CascadeClassifier eyeClassifier = new CascadeClassifier("haarcascade_eye.xml");

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

            Image<Bgr, byte> image = new Image<Bgr, byte>(bitmap);
            Rectangle[] facerectangles = faceClassifier.DetectMultiScale(image, 1.2, 1);
            Rectangle[] eyerectangles = eyeClassifier.DetectMultiScale(image, 1.2, 12);
           
          
            if (facerectangles.Length > 0)
            {
                int NewpointX = 510 - facerectangles[0].X;
                int NewpointY = 240 - facerectangles[0].Y;
                Console.WriteLine(facerectangles[0].X);
                Console.WriteLine(facerectangles[0].Y);
                textBox1.Invoke(new Action(() => textBox1.Text = facerectangles[0].X.ToString()));
                textBox2.Invoke(new Action(() => textBox2.Text = NewpointY.ToString()));


                Console.WriteLine("Width: {0}", pic.Width);
                Console.WriteLine("Height: {0}", pic.Height);
         

                writeToPort(new Point(NewpointX, NewpointY));
               
            }

            imagenumber = imagenumber + 1;              //proceeds to store image with different names of takesnap function
            rects = facerectangles
            
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
            foreach (Rectangle rectangle in eyerectangles)
            {
                using (Graphics graphics = Graphics.FromImage(bitmap))
                {
                    using (Pen pen = new Pen(Color.Green, 1))
                    {
                        graphics.DrawRectangle(pen, rectangle);
                    }
                }
            }

            pic.Image = bitmap;
        }

        private void writeToPort(Point point)
        {

                serialPort1.Write(String.Format("X{0}Y{1}", (180 - point.X / (Size.Width / 120)), (point.Y / (Size.Height / 120))));
            
        }
       
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (device.IsRunning)
            { Application.Exit();
              device.Stop();
              //device2.Stop();
              writeToPort(new Point(280, 184));
              serialPort1.DiscardOutBuffer();

            }

        }
    }
}
    

