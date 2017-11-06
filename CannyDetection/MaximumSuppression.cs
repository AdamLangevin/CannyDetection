using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CannyDetection
{
    class MaximumSuppression
    {

        public static Bitmap Suppression(Bitmap b)
        {
            Bitmap divX;
            Bitmap divY;
            byte[] AdivX;
            byte[] AdivY;

            /*
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                        ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppArgb);
            */

            divX = (Bitmap)b.Clone();

            ConvMatrix m = new ConvMatrix();
            m.TopLeft = m.MidLeft = m.BottomLeft = 1;
            m.TopRight = m.MidRight = m.BottomRight = -1;
            m.TopMid = m.Pixel = m.BottomMid = 0;

            divX = Filter.Conv(divX, m);

            m.TopLeft = m.TopMid = m.TopRight = 1;
            m.MidLeft = m.MidRight = m.Pixel = 0;
            m.BottomLeft = m.BottomMid = m.BottomRight = -1;

            divY = (Bitmap)b.Clone();

            divY = Filter.Conv(divY, m);

            BitmapData divXData = divX.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

            BitmapData divYData = divY.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);
            AdivX = new byte[divXData.Stride * divX.Height];
            AdivY = new byte[divYData.Stride * divY.Height];

            Marshal.Copy(divXData.Scan0, AdivX, 0, divXData.Stride * divX.Height);
            Marshal.Copy(divXData.Scan0, AdivY, 0, divYData.Stride * divY.Height);

            divY.UnlockBits(divYData);
            divX.UnlockBits(divXData);



            for (int i=0; i<=(b.Width-1); i++)
            {
                for(int j=0; j<=(b.Height-1); j++)
                {
                    
                }
            }

            return b;
        }
    }
}
