using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace CannyDetection
{
    public class Filter
    {
        /*
         * A function to add a Gaussian blur effect to a Bitmap.
         * Returns the Bitmap with the Gaussian blur applied
         * @Param Bitmap b: the source image data, expected to be in 32-bit ARGB word format.
         */
        public static Bitmap Gaussian(Bitmap b)
        {
            int sum = 0;
            int sum2 = 0;
            int sum3 = 0;
            int size = 5;
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                            ImageLockMode.ReadOnly,
                                            PixelFormat.Format32bppArgb);
            int weight;
            int[,] kernal = GaussKern(size, 5,out weight);

            unsafe
            {
                byte* p = (byte*)(void*)bData.Scan0;

                for(int y=size/2; y<(b.Height-size/2); y++)
                {
                    for(int x=size/2; x<(b.Width-size/2); x++)
                    {
                        sum = 0;
                        sum2 = 0;
                        sum3 = 0;
                        for(int i=-size/2; i<=size/2; i++)
                        {
                            for(int j=-size/2; j<size/2; j++)
                            {
                                sum += (p[0]*kernal[size/2 +i,size/2+j]);
                                sum2 += (p[1] * kernal[size / 2 + i, size / 2 + j]);
                                sum3 += (p[2] * kernal[size / 2 + i, size / 2 + j]);

                            }
                        }
                        p[0] = (byte)(Math.Round(sum / (float)weight));
                        p[1] = (byte)(Math.Round(sum2 / (float)weight));
                        p[2] = (byte)(Math.Round(sum3 / (float)weight));

                        p += 4;
                    }
                    p += (bData.Stride - b.Width*4);
                }
            }
            b.UnlockBits(bData);

            return b;
        }

        /*
         * A function to calculate the Kernal array of the Gaussian for array manipulations
         * returns the gaussian kernal, and the wieght value of the array
         * @Param int size: the size lenght of the required kernal.
         * @Param float sig: the deviation required for the specific kernal.
         */
        private static int[,] GaussKern(int size, float sig, out int weight)
        {
            float[,] kernal = new float[size, size];
            int[,] kern = new int[size, size];
            float a = 1 / (2 * (float)Math.PI * sig * sig);
            float b = 2 * sig * sig;
            float min = 1000;

            //calcauation of the keranl array values cell by cell
            for(int y=-size/2; y<=(size/2); y++)
            {
                for(int x=-size/2; x<=(size/2); x++)
                {
                    kernal[size / 2 + x, size / 2 + y] = ((1 / a) * (float)Math.Exp(-(x * x + y * y) / b));
                    if (kernal[size / 2 + x, size / 2 + y] < min)
                        min = kernal[size / 2 + x, size / 2 + y];
                }
            }

            //cacluation of the weight of the kernal
            int sum = 0;
             if(min>0 && min < 1)
            {
                for(int y=-size/2; y<=(size/2); y++)
                {
                    for(int x=-size/2; x<=(size/2); x++)
                    {
                        kernal[size / 2 + x, size / 2 + y] = (float)Math.Round(kernal[size / 2 + x, size / 2 + y] * ((int)(1 / min)), 0);
                        kern[size / 2 + x, size / 2 + y] = (int)kernal[size / 2 + x, size / 2 + y];
                        sum += kern[size / 2 + x, size / 2 + y];
                    }
                }
            } else
            {
                sum = 0;
                for (int y = -size / 2; y <= (size / 2); y++)
                {
                    for (int x = -size / 2; x <= (size / 2); x++)
                    {
                        kernal[size / 2 + x, size / 2 + y] = (float)Math.Round(kernal[size / 2 + x, size / 2 + y], 0);
                        kern[size / 2 + x, size / 2 + y] = (int)kernal[size / 2 + x, size / 2 + y];
                        sum += kern[size / 2 + x, size / 2 + y];
                    }
                }
            }
            weight = sum;

            
            return kern;
        }

        /*
         * Conversion function to a grey scaled image. The input is a Bitmap of ARGB 32-bit words.
         * returns the Bitmap of greyscaled equivalent image
         * @Param Bitmap b: The source image data in 32-bit ARGB word format.
         */
        public static Bitmap GrayScale(Bitmap b)
        {
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                                        ImageLockMode.ReadWrite,
                                                        PixelFormat.Format32bppArgb);
            //blue = 0
            //green = 1
            //red = 2

            unsafe
            {
                byte* p = (byte*)(void*)bData.Scan0;
                byte r, g, bl;
                for (int i = 0; i < b.Width; i++)
                {
                    for (int j = 0; j < b.Height; j++)
                    {
                        bl = p[0];
                        g = p[1];
                        r = p[2];

                        p[0] = p[1] = p[2] = (byte)(.299 * r + .587 * g + .114 * bl);


                        p += 4;   //alpha values are ignored for this operation, so we skip over them.
                    }
                    p += (bData.Stride - b.Width * 4);
                }
            }
            b.UnlockBits(bData);

            return b;
        }

        /*
         * A function to calculate the convolvement of a Bitmap and an array. This function supports 
         * many different mathimatical operations. The convelutional matrix is to represent the
         * mathimatical operator applied to the pixel values in the bitmap image.
         * returns the convolved bitmap
         * @Param Bitmap b: the source image data.
         * @Param ConvMatrix m: the array to be convolved with the bitmap
         */
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
