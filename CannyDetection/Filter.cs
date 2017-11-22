using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace CannyDetection
{
    public class Filter
    {

        public Filter()
        {

        }

        public static Bitmap Gaussian(Bitmap b, int nWeight)//default to 4
        {
            ConvMatrix m = new ConvMatrix();
            m.SetAll(1);
            m.Pixel = nWeight;
            m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = 2;
            m.Factor = nWeight + 12;
            return Filter.Conv(b, m);
        }

        public static Bitmap GrayScale(Bitmap b) //needs to change to unsafe, take out setpixel
        {
            Bitmap temp = (Bitmap)b;
            Bitmap bmap = (Bitmap)temp.Clone();
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    byte gray = (byte)(.299 * c.R + .587 * c.G + .114 * c.B);

                    bmap.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }

            return b = (Bitmap)bmap.Clone();
        }

        public static Bitmap Conv(Bitmap b, ConvMatrix m)
        {
            // Avoid divide by zero errors
            if (0 == m.Factor)
                return b; Bitmap

            // GDI+ still lies to us - the return format is BGR, NOT RGB. 
            bSrc = (Bitmap)b.Clone();
            BitmapData bmData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                                        ImageLockMode.ReadWrite,
                                                        PixelFormat.Format32bppArgb);
            BitmapData bmSrc = bSrc.LockBits(new Rectangle(0, 0, bSrc.Width, bSrc.Height),
                                                        ImageLockMode.ReadWrite,
                                                        PixelFormat.Format32bppArgb);
            int stride = bmData.Stride;
            int stride2 = stride * 2;

            System.IntPtr Scan0 = bmData.Scan0;
            System.IntPtr SrcScan0 = bmSrc.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;
                byte* pSrc = (byte*)(void*)SrcScan0;
                int nOffset = stride - b.Width * 4;
                int nWidth = b.Width - 2;
                int nHeight = b.Height - 2;

                int nPixel;

                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        nPixel = ((((pSrc[3] * m.TopLeft) +
                                       (pSrc[7] * m.TopMid) +
                                       (pSrc[11] * m.TopRight) +
                                       (pSrc[3 + stride] * m.MidLeft) +
                                       (pSrc[7 + stride] * m.Pixel) +
                                       (pSrc[11 + stride] * m.MidRight) +
                                       (pSrc[3 + stride2] * m.BottomLeft) +
                                       (pSrc[7 + stride2] * m.BottomMid) +
                                       (pSrc[11 + stride2] * m.BottomRight))
                                        / m.Factor) + m.Offset);

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[7 + stride] = (byte)nPixel;

                        nPixel = ((((pSrc[2] * m.TopLeft) +
                                    (pSrc[6] * m.TopMid) +
                                    (pSrc[10] * m.TopRight) +
                                    (pSrc[2 + stride] * m.MidLeft) +
                                    (pSrc[6 + stride] * m.Pixel) +
                                    (pSrc[10 + stride] * m.MidRight) +
                                    (pSrc[2 + stride2] * m.BottomLeft) +
                                    (pSrc[6 + stride2] * m.BottomMid) +
                                    (pSrc[10 + stride2] * m.BottomRight))
                                    / m.Factor) + m.Offset);

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[6 + stride] = (byte)nPixel;

                        nPixel = ((((pSrc[1] * m.TopLeft) +
                                    (pSrc[5] * m.TopMid) +
                                    (pSrc[9] * m.TopRight) +
                                    (pSrc[1 + stride] * m.MidLeft) +
                                    (pSrc[5 + stride] * m.Pixel) +
                                    (pSrc[9 + stride] * m.MidRight) +
                                    (pSrc[1 + stride2] * m.BottomLeft) +
                                    (pSrc[5 + stride2] * m.BottomMid) +
                                    (pSrc[9 + stride2] * m.BottomRight))
                                    / m.Factor) + m.Offset);

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[5 + stride] = (byte)nPixel;

                        nPixel = ((((pSrc[0] * m.TopLeft) +
                                       (pSrc[4] * m.TopMid) +
                                       (pSrc[8] * m.TopRight) +
                                       (pSrc[0 + stride] * m.MidLeft) +
                                       (pSrc[4 + stride] * m.Pixel) +
                                       (pSrc[8 + stride] * m.MidRight) +
                                       (pSrc[0 + stride2] * m.BottomLeft) +
                                       (pSrc[4 + stride2] * m.BottomMid) +
                                       (pSrc[8 + stride2] * m.BottomRight))
                                        / m.Factor) + m.Offset);

                        if (nPixel < 0) nPixel = 0;
                        if (nPixel > 255) nPixel = 255;
                        p[4 + stride] = (byte)nPixel;
                       
                        p += 4;
                        pSrc += 4;
                    }

                    p += nOffset;
                    pSrc += nOffset;
                }
            }

            b.UnlockBits(bmData);
            bSrc.UnlockBits(bmSrc);
            return b;
        }

    }
}
