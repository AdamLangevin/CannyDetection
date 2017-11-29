using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

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

        public Form1()
        {
            timeElapsed = 0.0;
            watch = new Stopwatch();
            InitializeComponent();
            m = new Bitmap(2, 2);
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

        private void SaveItem(object sender, EventArgs e)
        {
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
            this.Close();
        }

        private void GaussianItem(object sender, EventArgs e)
        {
            watch.Start();

            u = (Bitmap)m.Clone();
            m = Filter.Gaussian(m);

            watch.Stop();
            Console.WriteLine("Gaussian blur filtering took: " + watch.ElapsedMilliseconds / 1000);
            timeElapsed += watch.ElapsedMilliseconds / 1000;
            _source.TraceEvent(TraceEventType.Information, 0, "Time: {0}", watch.ElapsedMilliseconds / 1000);
            _source.TraceEvent(TraceEventType.Information, 0, "Total Time: {0}", timeElapsed);


            this.Refresh();
        }

        private void GrayScaleItem(object sender, EventArgs e)
        {
            watch.Start();
            u = (Bitmap)m.Clone();
            m = Filter.GrayScale(m);

            watch.Stop();
            Console.WriteLine("Grey Scale took: " + watch.ElapsedMilliseconds/1000);
            timeElapsed += watch.ElapsedMilliseconds / 1000;
            _source.TraceEvent(TraceEventType.Information, 0, "Time: {0}", watch.ElapsedMilliseconds / 1000);
            _source.TraceEvent(TraceEventType.Information, 0, "Total Time: {0}", timeElapsed);


            this.Refresh();
            
        }

        private void sobleClick(object sender, EventArgs e)
        {
            watch.Start();

            u = (Bitmap)m.Clone();
            m = PixelDifferentiator.Differentiate(m);
            watch.Stop();
            Console.WriteLine("Sobel filtering took: " + watch.ElapsedMilliseconds / 1000);
            timeElapsed += watch.ElapsedMilliseconds / 1000;
            _source.TraceEvent(TraceEventType.Information, 0, "Time: {0}", watch.ElapsedMilliseconds / 1000);
            _source.TraceEvent(TraceEventType.Information, 0, "Total Time: {0}", timeElapsed);


            this.Refresh();
        }

        private void MaxSupressionClick(object sender, EventArgs e)
        {
            watch.Start();

            u = (Bitmap)m.Clone();
            m = MaximumSuppression.Suppression(m);

            watch.Stop();
            Console.WriteLine("Maximum Suppression took: " + watch.ElapsedMilliseconds / 1000);
            timeElapsed += watch.ElapsedMilliseconds / 1000;
            Console.WriteLine("The total filtering took: " + timeElapsed);
            _source.TraceEvent(TraceEventType.Information, 0, "Time: {0}", watch.ElapsedMilliseconds / 1000);
            _source.TraceEvent(TraceEventType.Information, 0, "Total Time: {0}", timeElapsed);
            timeElapsed = 0.0;

            this.Refresh();
        }

        private void ProcessToNonMax(object sender, EventArgs e)
        {
            watch.Start();

            u = (Bitmap)m.Clone();
            m = Filter.GrayScale(m);
            m = Filter.Gaussian(m);
            m = PixelDifferentiator.Differentiate(m);
            m = MaximumSuppression.Suppression(m);

            watch.Stop();
            Console.WriteLine("The total filering time took: " + watch.ElapsedMilliseconds / 1000);
            _source.TraceEvent(TraceEventType.Information, 0, "Time: {0}", watch.ElapsedMilliseconds / 1000);

            timeElapsed = 0.0;
            
            this.Refresh();
        }
    }
}
