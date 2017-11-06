using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CannyDetection
{
    public partial class Form1 : Form
    {
        private Bitmap m;      //active bitmap field
        private Bitmap u;      //copy of active bitmap
        private MainMenu mainMenu;
        private double zoom =1.0;

        public Form1()
        {
            InitializeComponent();
            m = new Bitmap(2, 2);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
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

            open.InitialDirectory = "c:\\";
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

            save.InitialDirectory = "c://";
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
            u = (Bitmap)m.Clone();
            m = Filter.Gaussian(m, 4);
            this.Refresh();
        }

        private void GrayScaleItem(object sender, EventArgs e)
        {
            u = (Bitmap)m.Clone();
            m = Filter.GrayScale(m);
            this.Refresh();
        }

        private void sobleClick(object sender, EventArgs e)
        {
            u = (Bitmap)m.Clone();
            m = PixelDifferentiator.Differentiate(m);
            this.Refresh();
        }
    }
}
