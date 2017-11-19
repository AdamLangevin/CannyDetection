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
            Bitmap grade;
            Bitmap NonMax;

            
            /*
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                        ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppArgb);

            Second order differentiation is needed to get a gradient to determine
            the directions the change is the values are moving in. This helps to determine
            the pixels that are needed to be suppressed.
            */
            grade = (Bitmap)b.Clone();
            divX = (Bitmap)b.Clone();
            divY = (Bitmap)b.Clone();
            NonMax = (Bitmap)b.Clone();


            ConvMatrix m = new ConvMatrix();
            m.TopLeft = m.MidLeft = m.BottomLeft = 1;
            m.TopRight = m.MidRight = m.BottomRight = -1;
            m.TopMid = m.Pixel = m.BottomMid = 0;

            divX = Filter.Conv(divX, m);

            m.TopLeft = m.TopMid = m.TopRight = 1;
            m.MidLeft = m.MidRight = m.Pixel = 0;
            m.BottomLeft = m.BottomMid = m.BottomRight = -1;


            divY = Filter.Conv(divY, m);

            BitmapData divXData = divX.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

            BitmapData divYData = divY.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);
            BitmapData gradeData = grade.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                       ImageLockMode.ReadOnly,
                       PixelFormat.Format32bppArgb);
            BitmapData NonData = NonMax.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

            float[,] Grade = new float[b.Width, b.Height];
            float[,] NMax = new float[b.Width, b.Height];

            int strideX = divXData.Stride;
            int strideY = divYData.Stride;
            int strideG = gradeData.Stride;
            int strideN = NonData.Stride;

            IntPtr Scan0X = divXData.Scan0;
            IntPtr Scan0Y = divYData.Scan0;
            IntPtr Scan0G = gradeData.Scan0;
            IntPtr Scan0N = NonData.Scan0;

            unsafe {
                byte* pX = (byte*)(void*)Scan0X;
                byte* pY = (byte*)(void*)Scan0Y;
                byte* pGrade = (byte*)(void*)Scan0G;
                byte* pN = (byte*)(void*)NonData.Scan0;

                int nOffsetX = strideX - divX.Width * 4;
                int nOffsetY = strideY - divY.Width * 4;
                int nOffsetG = strideG - grade.Width * 4;
                int nOffsetN = strideN - NonMax.Width * 4;

                int nWidthX = divX.Width - 2;
                int nWidthY = divY.Width - 2;
                int nWidthG = grade.Width - 2;
                int nWidthN = NonMax.Width - 2;

                int nHeightX = divX.Height - 2;
                int nHeightY = divY.Height - 2;
                int nHeightG = grade.Height - 2;
                int nHeightN = NonMax.Height - 2;

                float nPixel;
                int lim = 1;
                float tangent = 0;
                float c1 = 0;     //center pixel to be differentiated against
                float c2 = 0;     //test pixel one
                float c3 = 0;     //test pixel two
                int pixleSize = 4;  //standard pixel size, for calculations

                for (int y=lim; y<= b.Height-1; ++y)
                {
                    for (int x=lim; x<= b.Width-1 ; ++x)
                    {
                        //c1 = pY[3] * pY[3];
                        //c2 = pX[3] * pX[3];
                        //nPixel = ((float)Math.Sqrt(c1 + c2));
                        //if (nPixel > 255) nPixel = 255;
                        //else if (nPixel < 0) nPixel = 0;
                        //Grade[x+3,y] = (byte)nPixel;

                        //c1 = pY[2] * pY[2];
                        //c2 = pX[2] * pX[2];
                        //nPixel = ((float)Math.Sqrt(c1 + c2));
                        //if (nPixel > 255) nPixel = 255;
                        //else if (nPixel < 0) nPixel = 0;
                        //Grade[x+2,y] = (byte)nPixel;

                        //c1 = pY[1] * pY[1];
                        //c2 = pX[1] * pX[1];
                        //nPixel = ((float)Math.Sqrt(c1 + c2));
                        //if (nPixel > 255) nPixel = 255;
                        //else if (nPixel < 0) nPixel = 0;
                        //Grade[x+1,y] = (byte)nPixel;

                        c1 = ((pY[0] + pY[1] + pY[2] + pY[3]) / 4 )* ((pY[0] + pY[1] + pY[2] + pY[3]) / 4); 
                        c2 = ((pX[0] + pX[1] + pX[2] + pX[3]) / 4 )* ((pX[0] + pX[1] + pX[2] + pX[3]) / 4);
                        nPixel = ((float)(Math.Sqrt(c1 +c2)));
                        if (nPixel > 255) nPixel = 255;
                        else if (nPixel < 0) nPixel = 0;
                        Grade[x,y] = (byte)nPixel;

                        pY += 4;
                        pX += 4;
                        //pY++;
                        //pX++;
                    }
                    pY += nOffsetY;
                    pX += nOffsetX;
                }

                //Non Maximum suppression
                //Reduces the fuzzy bits around the solid lines of white areas
                pY = (byte*)(void*)Scan0Y;
                pX = (byte*)(void*)Scan0X;
                pGrade = (byte*)(void*)Scan0G;

                //Looping through the bitmap, x and y are used as refrences.
                //We lose one pixel around the image, because we are using a 3x3
                //to calculate
                for (int y = lim; y<= nHeightN; y++) {
                    for (int x = lim; x <= nWidthN; x++) {

                        //c1 = pX[pixleSize + strideG] + pX[1 + pixleSize + strideG] + pX[2 + pixleSize + strideG] + pX[3 + pixleSize + strideG];
                        //c2 = pY[pixleSize + strideG] + pY[1 + pixleSize + strideG] + pY[2 + pixleSize + strideG] + pY[3 + pixleSize + strideG];
                        c1 = (pX[0] + pX[1] + pX[2] + pX[3])/4;
                        c2 = (pY[0] + pY[1] + pY[2] + pY[3])/4;
                        if (c1 == 0) {
                            tangent = 90F;
                        } else {
                            tangent = (float)(Math.Atan(c2 / c1) * 180 / Math.PI);
                        }

                        //using all three colour channels
                        //Horizontal Edge

                        //rework the reffrencing, must change to reffrences to the top left
                        //most pixle corrisponding to the differentiator.

                        //c1 = (Grade[pixleSize + strideG,y] + pGrade[pixleSize + 1 + strideG] + pGrade[pixleSize + 2 + strideG] + pGrade[3 + pixleSize + strideG]);
                        //c2 = (pGrade[pixleSize] + pGrade[1 + pixleSize] + pGrade[2 + pixleSize] + pGrade[3+pixleSize]);
                        //c3 = (pGrade[pixleSize + 2 * strideG] + pGrade[1 + pixleSize + 2*strideG] + pGrade[2 + pixleSize + 2*strideG] + pGrade[3 + pixleSize + 2*strideG]);
                        c1 = Grade[x, y];
                        c2 = Grade[x, y + 1];
                        c3 = Grade[x, y - 1];
                        if ((-22.5 < tangent) && (tangent <= 22.5) || (157.5 < tangent) && (tangent <= -157.5))
                        {
                            if (c1 < c2 || c1 < c3)
                            {
                                NMax[x, y] = 0;
                                //pN[pixleSize + strideG] = 0;
                                //pN[1 + pixleSize + strideG] = 0;
                                //pN[2 + pixleSize + strideG] = 0;
                                //pN[3 + pixleSize + strideG] = 0;
                            
                            }
                        }

                        //Vertical Edge, needs to calculate pixels above and bellow
                        //c2 = (pGrade[0 + strideG] + pGrade[1 + strideG] + pGrade[2 + strideG] + pGrade[3 + strideG]);
                        //c3 = (pGrade[2 * pixleSize + strideG] + pGrade[1 + 2 * pixleSize + strideG] + pGrade[2 + 2 * pixleSize + strideG] + pGrade[3 + 2*pixleSize + strideG]);
                        c2 = Grade[x + 1, y];
                        c3 = Grade[x - 1, y];
                        if ((-112.5 < tangent) && (-67.5 <= tangent) || (67.5 < tangent) && (tangent <= 112.5))
                        {
                            if (c1 < c2 || c1 < c3)
                            {
                                NMax[x,y] = 0;
                                //pN[pixleSize + strideG] = 0;
                                //pN[1 + pixleSize + strideG] = 0;
                                //pN[2 + pixleSize + strideG] = 0;
                                //pN[3 + pixleSize + strideG] = 0;
                            }
                        }

                        //+45 Degree Edge
                        //c2 = (pGrade[2*pixleSize] + pGrade[1 + 2*pixleSize] + pGrade[2 + 2*pixleSize] + pGrade[3+ 2*pixleSize]);
                        //c3 = (pGrade[2*strideG] + pGrade[1 + 2*strideG] + pGrade[2 + 2*strideG] + pGrade[3 + 2*strideG]);
                        c2 = Grade[x + 1, y - 1];
                        c3 = Grade[x - 1, y + 1];
                        if ((-67.5<tangent)&&(-22.5<=tangent) || (112.5<tangent)&&(tangent<=157.5))
                        {
                            if (c1 < c2 || c1 < c3)
                            {
                                NMax[x, y] = 0;
                                //pN[pixleSize + strideG] = 0;
                                //pN[1 + pixleSize + strideG] = 0;
                                //pN[2 + pixleSize + strideG] = 0;
                                //pN[3 + pixleSize + strideG] = 0;
                            }
                        }

                        //-45 Degree Edge
                        //c2 = pGrade[0] + pGrade[1] + pGrade[2] + pGrade[3];
                        //c3 = pGrade[2*pixleSize + 2* strideG] + pGrade[1 + 2*pixleSize + 2*strideG] + pGrade[2 + 2*pixleSize + 2*strideG] + pGrade[3+ 2*pixleSize + 2*strideG];
                        c2 = Grade[x + 1, y + 1];
                        c3 = Grade[x - 1, y - 1];
                        if ((-157.5<tangent)&&(-112.5<=tangent) || (67.5<tangent)&&(tangent<=22.5))
                        {
                            if (c1<c2 || c1<c3)
                            {
                                NMax[x, y] = 0;
                                //pN[pixleSize + strideG] = 0;
                                //pN[1 + pixleSize + strideG] = 0;
                                //pN[2 + pixleSize + strideG] = 0;
                                //pN[3 + pixleSize + strideG] = 0;
                            }
                        }

                        NMax[x, y] = Grade[x, y];

                        c1 = 0;
                        c2 = 0;
                        c3 = 0;

                        //pGrade += 4;
                        //pN += 4;
                        pY += 4;
                        pX += 4;
                    }
                    //pGrade += nOffsetG;
                    //pN += nOffsetN;
                    pX += nOffsetX;
                    pY += nOffsetY;
                }
                float[,] postHyst = new float[b.Width, b.Height];

                postHyst = NMax.Clone() as float[,];

                float min, max;
                max = 100;
                min = 0;
                for (int x=1; x<(b.Width - 1); x++)
                {
                    for (int y=1; y<(b.Height - 1); y++)
                    {
                        if(postHyst[x,y] > max)
                        {
                            max = postHyst[x, y];
                        }
                        if(postHyst[x,y] < min || postHyst[x,y] > 0)
                        {
                            min = postHyst[x, y];
                        }
                    }
                }

                float MaxHyst = 20f;
                float MinHyst = 10f;
                float[,] Edges = new float[b.Width, b.Height];

                for (int x = 1; x< b.Width - 1; x++)
                {
                    for (int y = 1; y<b.Height - 1; y++)
                    {

                    }
                }
            }//end of unsafe code
            NonMax.UnlockBits(NonData);
            divY.UnlockBits(divYData);
            divX.UnlockBits(divXData);
            grade.UnlockBits(gradeData);

            NonMax = convertToBitmap(Grade, b.Width, b.Height);


            return NonMax;
        }

        public static Bitmap convertToBitmap(float[,] src, int width, int height)
        {
            Bitmap bits= new Bitmap(width, height);
            BitmapData bitsD = bits.LockBits(new Rectangle(0, 0, width, height),
                                            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            unsafe {
                byte* p = (byte*)(void*)bitsD.Scan0;

                for (int y=0; y<height; y++)
                {
                    for (int x=0; x<width; x++)
                    {
                        p[0] = (byte)((int)src[x, y]);
                        p[1] = (byte)((int)src[x, y]);
                        p[2] = (byte)((int)src[x, y]);
                        p[3] = (byte)((int)src[x, y]);

                        p += 4;
                    }
                    p += (bitsD.Stride - (bitsD.Width * 4));
                }
            }
            bits.UnlockBits(bitsD);

            return bits;
        }

    }
}
