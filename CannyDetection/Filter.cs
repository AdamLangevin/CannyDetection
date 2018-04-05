using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace CannyDetection
{
    public class Filter
    {
        private static int SIZE = 5;
        private static float sigma = 1.0f;
        /*
         * A function to add a Gaussian blur effect to a Bitmap.
         * @Returns the Bitmap with the Gaussian blur applied
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
            int[,] kernal = GaussKern(size, 1.4f,out weight);

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
         * An integer array version of the Gaussian blur filter. 
         * varraibles affecting the outcome are the size of teh kernal, 
         * and the sigma value, controlling the spread of values.
         * @Returns: an integer array of pixel values calculated from a Bitmap
         * @Params: int[,] dat: an array of pixel data points
         */
        public static int[,] ArrayGaussian(int[,] dat)
        {
            float sum = 0;
            int weight = 0;
            int w = dat.GetLength(0);
            int h = dat.GetLength(1);
            int[,] kernal = GaussKern(SIZE, sigma, out weight);
            int[,] op = new int[dat.GetLength(0), dat.GetLength(1)];

            op = dat;

            for(int x=SIZE/2; x<(w-SIZE/2); x++)
            {
                for(int y=SIZE/2; y<(h-SIZE/2); y++)
                {
                    sum = 0;
                    for(int i=-SIZE/2; i<=SIZE/2; i++)
                    {
                        for(int j=-SIZE/2; j<=SIZE/2; j++)
                        {
                            sum += ((float)dat[x + i, y + j] * kernal[SIZE / 2 + i, SIZE / 2 + j]);
                        }
                    }
                    op[x, y] = (int)(Math.Round(sum / (float)weight));
                }
            }
            
            return op;
        }

        /*
         * A function to calculate the Kernal array of the Gaussian for array manipulations
         * @returns the gaussian kernal, and the wieght value of the array
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
         * This function removes any green color in the background
         * @returns a bitmap, with the changes applied to the background color.
         * @Param Bitmap b: the inout bitmap that will have the filter applied to it
         * @Param out int[,] blackedout: alternate output of the image blacked out, as 
         * as an integer array representing the grey scaled color of the input Bitmap.
         */
        public static Bitmap blackBackground(Bitmap b, out int[,] blackedout)
        {
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                                        ImageLockMode.ReadWrite,
                                                        PixelFormat.Format32bppArgb);
            int[,] back = new int[b.Width, b.Height];

            unsafe
            {
                byte* p = (byte*)(void*)bData.Scan0;
                for (int j = 0; j < b.Height; j++)
                {
                    for (int i = 0; i < b.Width; i++)
                    {
                        //values used to select out green shades, then grey scale others
                        if ((p[0] < 180) && (p[1] < 255) && (p[2] < 180)) 
                        {
                            back[i, j] = 0;  

                        } else
                        {
                            back[i, j] = (int)((p[0] + p[1] + p[2]) / 3);
                        }
                        p += 4;
                    }
                    p += (bData.Stride - b.Width * 4);
                }
            }
            b.UnlockBits(bData);
            blackedout = back;

            return b;
        }

        /*
         * Conversion function to a grey scaled image. The input is a Bitmap of ARGB 32-bit words.
         * @returns the Bitmap of greyscaled equivalent image
         * @Param Bitmap b: The source image data in 32-bit ARGB word format.
         */
        public static Bitmap GrayScale(Bitmap b, out int[,] greyImage)
        {
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                                        ImageLockMode.ReadWrite,
                                                        PixelFormat.Format32bppArgb);

            int[,] grey = new int[b.Width, b.Height];

            unsafe
            {
                byte* p = (byte*)(void*)bData.Scan0;
                for (int j = 0; j < b.Height; j++)
                {
                    for (int i = 0; i < b.Width; i++)
                    {
                        grey[i, j] = (int)((p[0] + p[1] + p[2]) / 3.0);
                        p += 4;   //alpha values are ignored for this operation, so we skip over them.
                    }
                    p += (bData.Stride - b.Width * 4);
                }
            }
            b.UnlockBits(bData);
            greyImage = grey;

            return b;
        }

        /*
         * A function to calculate the convolvement of a Bitmap and an array. This function supports 
         * many different mathimatical operations. The convelutional matrix is to represent the
         * mathimatical operator applied to the pixel values in the bitmap image.
         * @returns the convolved bitmap
         * @Param Bitmap b: the source image data.
         * @Param ConvMatrix m: the array to be convolved with the bitmap
         */
        public static Bitmap Conv(Bitmap b, ConvMatrix m)
        {
            // Avoid divide by zero errors
            if (0 == m.Factor)
                return b; Bitmap

            res = (Bitmap)b.Clone();
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                                        ImageLockMode.ReadWrite,
                                                        PixelFormat.Format32bppArgb);
            BitmapData srcData = res.LockBits(new Rectangle(0, 0, res.Width, res.Height),
                                                        ImageLockMode.ReadWrite,
                                                        PixelFormat.Format32bppArgb);
            int stride = bData.Stride;

            unsafe
            {
                byte* p = (byte*)(void*)bData.Scan0;
                byte* pSrc = (byte*)(void*)srcData.Scan0;
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
                                    (pSrc[3 +2*  stride] * m.BottomLeft) +
                                    (pSrc[7 + 2 * stride] * m.BottomMid) +
                                    (pSrc[11 + 2 * stride] * m.BottomRight))
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
                                    (pSrc[2 + 2 * stride] * m.BottomLeft) +
                                    (pSrc[6 + 2 * stride] * m.BottomMid) +
                                    (pSrc[10 + 2 * stride] * m.BottomRight))
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
                                    (pSrc[1 + 2 * stride] * m.BottomLeft) +
                                    (pSrc[5 + 2 * stride] * m.BottomMid) +
                                    (pSrc[9 + 2 * stride] * m.BottomRight))
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
                                    (pSrc[0 + 2 * stride] * m.BottomLeft) +
                                    (pSrc[4 + 2 * stride] * m.BottomMid) +
                                    (pSrc[8 + 2 * stride] * m.BottomRight))
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

            b.UnlockBits(bData);
            res.UnlockBits(srcData);
            return b;
        }

    }
}
