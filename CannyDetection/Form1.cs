using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using AForge.Imaging;
using System.Drawing.Imaging;
using AForge.Math.Geometry;
using System.Collections.Generic;
using AForge;
using System.IO;

namespace CannyDetection
{
    public partial class Form1 : Form
    {
        private Bitmap m;      //active bitmap field
        private Bitmap u;      //copy of active bitmap
        private MainMenu mainMenu;
        private double zoom =1.0;
        private Stopwatch watch;
        private double timeElapsed;
        private static TraceSource _source = new TraceSource("TestLog");
        private Logger l;
        private string w;
        private int[,] greyImage;

        public Form1()
        {
            w = DateTime.Now.ToLongDateString();
            l = new Logger(w);
            timeElapsed = 0.0;
            watch = new Stopwatch();
            InitializeComponent();
            m = new Bitmap(2, 2);
        }

        public void startUp(FileStream fileStream, Boolean runFull)
        {
            if (runFull)
            {
                this.Item(fileStream);
            } else
            {
                this.nonAuto(fileStream);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImage(m, new Rectangle(this.AutoScrollPosition.X,this.AutoScrollPosition.Y,(int)(m.Width*zoom),(int)(m.Height*zoom)));
        }

        private void LoadItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();

            open.InitialDirectory = "c:\\Users\\misfi\\Pictures";
            open.Filter = "Bitmap Files(*.bmp)|*.bmp|Jpeg files(*.jpg)|*.jpg|Png files(*.png)|*.png|All Vaild files(*.bmp/*.png/*.jpg)|*.bmp/*.png/*.jpg";
            open.FilterIndex = 2;
            open.RestoreDirectory = true;

            if(DialogResult.OK == open.ShowDialog())
            {
                m = (Bitmap)Bitmap.FromFile(open.FileName, false);
                this.AutoScroll = true;
                this.AutoScrollMinSize = new Size((int)(m.Width*zoom) , (int)(m.Height*zoom));
                this.Invalidate();
            }
        }

        private void Item(FileStream fileStream)
        {
            String filePath = @"C:\Users\py120\Desktop\Dev\objectDetection\CannyDetection\Test_2Balls.jpg";
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                fileStream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite);
                m = (Bitmap)System.Drawing.Image.FromStream(fileStream);
                Application.DoEvents();
                this.ProcessToNonMax(this,new EventArgs());
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("File not found, Error on the opening function: {0}", ex.ToString());
            }
        }

        private void nonAuto(FileStream filestream)
        {
            try
            {
                LoadItem_Click(this, new EventArgs());
            }
            catch(FileNotFoundException ex)
            {
                Console.WriteLine("Error occured: {0}", ex.ToString());
            }
        }

        private void SaveItem(object sender, EventArgs e)
        {
            l.log(">------------end-------------<");
            SaveFileDialog save = new SaveFileDialog();

            save.InitialDirectory = "c:\\Users\\";
            save.Filter = "Bitmap Files(*.bmp)|*.bmp|Jpeg files(*.jpg)|*.jpg|Png files(*.png)|*.png|All Vaild files(*.bmp/*.png/*.jpg)|*.bmp/*.png/*.jpg";
            save.FilterIndex = 1;
            save.RestoreDirectory = true;

            if(DialogResult.OK == save.ShowDialog())
            {
                m.Save(save.FileName);
            }
        }

        private void ExitItem(object sender, EventArgs e)
        {
            l.log(">-----------end--------------<");
            this.Close();
        }

        private void GaussianItem(object sender, EventArgs e)
        {
            watch.Start();

            u = (Bitmap)m.Clone();
            m = MaximumSuppression.convertToBitmap(Filter.ArrayGaussian(greyImage), greyImage.GetLength(0), greyImage.GetLength(1));

            watch.Stop();

            timeElapsed += watch.ElapsedMilliseconds / 1000;
            _source.TraceEvent(TraceEventType.Information, 0, "Gaussian Time: {0}", watch.ElapsedMilliseconds / 1000);
            _source.TraceEvent(TraceEventType.Information, 0, "Total Time: {0}", timeElapsed);
            l.log("Gaussian Time: " + watch.ElapsedMilliseconds / 1000);
            this.Refresh();
        }

        private void GrayScaleItem(object sender, EventArgs e)
        {
            watch.Start();
            u = (Bitmap)m.Clone();
            m = Filter.GrayScale(m,out greyImage);
            m = MaximumSuppression.convertToBitmap(greyImage, greyImage.GetLength(0), greyImage.GetLength(1));
            watch.Stop();

            timeElapsed += watch.ElapsedMilliseconds / 1000;
            _source.TraceEvent(TraceEventType.Information, 0, "Grey Scale Time: {0}", watch.ElapsedMilliseconds / 1000);
            _source.TraceEvent(TraceEventType.Information, 0, "Total Time: {0}", timeElapsed);
            l.log("Grey Scale  Time: " + watch.ElapsedMilliseconds / 1000);

            this.Refresh();
            
        }

        private void sobleClick(object sender, EventArgs e)
        {
            watch.Start();

            u = (Bitmap)m.Clone();
            m = PixelDifferentiator.Differentiate(m);
            watch.Stop();

            timeElapsed += watch.ElapsedMilliseconds / 1000;
            _source.TraceEvent(TraceEventType.Information, 0, "Sobel Filtering Time: {0}", watch.ElapsedMilliseconds / 1000);
            _source.TraceEvent(TraceEventType.Information, 0, "Total Time: {0}", timeElapsed);
            l.log("Soble Filtering Time: " + watch.ElapsedMilliseconds / 1000);

            this.Refresh();
        }

        private void MaxSupressionClick(object sender, EventArgs e)
        {
            watch.Start();

            u = (Bitmap)m.Clone();
            m = MaximumSuppression.Suppression(m);

            watch.Stop();

            timeElapsed += watch.ElapsedMilliseconds / 1000;
            _source.TraceEvent(TraceEventType.Information, 0, "Maximum Suppression  Time: {0}", watch.ElapsedMilliseconds / 1000);
            _source.TraceEvent(TraceEventType.Information, 0, "Total Time: {0}", timeElapsed);
            l.log("Maximum Suppression Time: " + watch.ElapsedMilliseconds / 1000 + " Total Time: " + watch.ElapsedMilliseconds / 1000);
            timeElapsed = 0.0;

            this.Refresh();
        }

        private void ProcessToNonMax(object sender, EventArgs e)
        {
            watch.Start();

            u = (Bitmap)m.Clone();
            m = Filter.blackBackground(m, out greyImage);
            m = MaximumSuppression.convertToBitmap(greyImage, greyImage.GetLength(0), greyImage.GetLength(1));
            m = MaximumSuppression.convertToBitmap(Filter.ArrayGaussian(greyImage), greyImage.GetLength(0), greyImage.GetLength(1));
            m = MaximumSuppression.convertToBitmap(Filter.ArrayGaussian(greyImage), greyImage.GetLength(0), greyImage.GetLength(1));
            m = PixelDifferentiator.Differentiate(m);
            m = MaximumSuppression.Suppression(m);
            m = CircleDetector.Cirlces(m);

            watch.Stop();

            _source.TraceEvent(TraceEventType.Information, 0, "Time: {0}", watch.ElapsedMilliseconds / 1000);
            l.log("Total Process Time: " + watch.ElapsedMilliseconds / 1000);

            timeElapsed = 0.0;
            
            this.Refresh();
        }

        private void onClose(object sender, FormClosingEventArgs e)
        {
            l.log(">------------end------------<");

        }

        private void DetectorClick(object sender, EventArgs e)
        {
            
            u = (Bitmap)m.Clone();
            m = CircleDetector.Cirlces(m);

            this.Refresh();
        }

        private void background(object sender, EventArgs e)
        {
            u = (Bitmap)m.Clone();
            m = Filter.blackBackground(m, out greyImage);
            m = MaximumSuppression.convertToBitmap(greyImage, greyImage.GetLength(0), greyImage.GetLength(1));

            this.Refresh();
        }
    }
}
