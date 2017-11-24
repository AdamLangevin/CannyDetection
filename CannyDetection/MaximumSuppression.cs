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
        public static int[,] EdgeMap;
        public static int[,] Visited;
        public static int[,] Edges;

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

            float[,] aDivX = convertToArray(divX);
            float[,] aDivY = convertToArray(divY);

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
            int[,] postHyst = new int[b.Width, b.Height];
            Edges = new int[b.Width, b.Height];

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

                for (int y=lim; y<= b.Height-1; ++y)
                {
                    for (int x=lim; x<= b.Width-1 ; ++x)
                    {
                        
                        c1 = aDivX[x, y] * aDivX[x,y];
                        c2 = aDivY[x, y] * aDivY[x,y];
                        nPixel = ((float)(Math.Sqrt(c1 +c2)));
                        Grade[x,y] = nPixel;


                    }

                }

                //Non Maximum suppression
                //Reduces the fuzzy bits around the solid lines of white areas
                pY = (byte*)(void*)Scan0Y;
                pX = (byte*)(void*)Scan0X;
                pGrade = (byte*)(void*)Scan0G;

                for(int y =0; y<b.Height; y++)
                {
                    for(int x=0; x<b.Width; x++)
                    {
                        NMax[x, y] = Grade[x, y];
                    }
                }

                //Looping through the bitmap, x and y are used as refrences.
                //We lose one pixel around the image, because we are using a 3x3
                //to calculate
                for (int y = lim; y<= nHeightN; y++) {
                    for (int x = lim; x <= nWidthN; x++) {


                        c1 = aDivX[x, y];
                        c2 = aDivY[x, y];
                        if (c1 == 0) {
                            tangent = 90F;
                        } else {
                            tangent = (float)(Math.Atan(c2 / c1) * 180 / Math.PI);
                        }

                        //using all three colour channels
                        //Horizontal Edge

                        //rework the reffrencing, must change to reffrences to the top left
                        //most pixle corrisponding to the differentiator.

                        c1 = Grade[x, y];
                        c2 = Grade[x, y + 1];
                        c3 = Grade[x, y - 1];
                        if ((-22.5 < tangent) && (tangent <= 22.5) || (157.5 < tangent) && (tangent <= -157.5))
                        {
                            if (c1 < c2 || c1 < c3)
                            {
                                NMax[x, y] = 0;
                            
                            }
                        }

                        //Vertical Edge, needs to calculate pixels above and bellow
                        c2 = Grade[x + 1, y];
                        c3 = Grade[x - 1, y];
                        if ((-112.5 < tangent) && (-67.5 <= tangent) || (67.5 < tangent) && (tangent <= 112.5))
                        {
                            if (c1 < c2 || c1 < c3)
                            {
                                NMax[x,y] = 0;

                            }
                        }

                        //+45 Degree Edge
                        c2 = Grade[x + 1, y - 1];
                        c3 = Grade[x - 1, y + 1];
                        if ((-67.5<tangent)&&(-22.5<=tangent) || (112.5<tangent)&&(tangent<=157.5))
                        {
                            if (c1 < c2 || c1 < c3)
                            {
                                NMax[x, y] = 0;

                            }
                        }

                        //-45 Degree Edge
                        c2 = Grade[x + 1, y + 1];
                        c3 = Grade[x - 1, y - 1];
                        if ((-157.5<tangent)&&(-112.5<=tangent) || (67.5<tangent)&&(tangent<=22.5))
                        {
                            if (c1<c2 || c1<c3)
                            {
                                NMax[x, y] = 0;

                            }
                        }

                        c1 = 0;
                        c2 = 0;
                        c3 = 0;

                    }
                }


                for (int x = 1; x < b.Width - 1; x++)
                {
                    for (int y = 1; y < b.Height - 1; y++)
                    {
                        postHyst[x, y] = (int)NMax[x, y];
                    }
                }

                float min, max;
                max = 0;
                min = 100;
                for (int x=1; x<=(b.Width - 1); x++)
                {
                    for (int y=1; y<=(b.Height - 1); y++)
                    {
                        if(postHyst[x,y] > max)
                        {
                            max = postHyst[x, y];
                        }
                        if(postHyst[x,y] < min && postHyst[x,y] > 0)
                        {
                            min = postHyst[x, y];
                        }
                    }
                }

                EdgeMap = new int[b.Width, b.Height];
                Visited = new int[b.Width, b.Height];

                float MaxHyst = 70f;
                float MinHyst = 30f;

                for (int x = 1; x< b.Width - 1; x++)
                {
                    for (int y = 1; y<b.Height - 1; y++)
                    {
                        if(postHyst[x,y] >= MaxHyst )
                        {
                            Edges[x, y] = 1;
                        }
                        if((postHyst[x,y] < MaxHyst) && (postHyst[x,y] >= MinHyst))
                        {
                            Edges[x, y] = 0;
                        }
                    }
                }

            }//end of unsafe code
            NonMax.UnlockBits(NonData);
            divY.UnlockBits(divYData);
            divX.UnlockBits(divXData);
            grade.UnlockBits(gradeData);

            threshold(Edges, b.Width, b.Height);

            NonMax = convertToBitmap(EdgeMap, b.Width, b.Height);

            return NonMax;
        }

        public static Bitmap convertToBitmap(float[,] src, int width, int height)
        {
            Bitmap bits= new Bitmap(width, height);
            BitmapData bitsD = bits.LockBits(new Rectangle(0, 0, width, height),
                                            ImageLockMode.ReadOnly, 
                                            PixelFormat.Format32bppArgb);

            unsafe {
                byte* p = (byte*)(void*)bitsD.Scan0;

                for (int y=0; y<height; y++)
                {
                    for (int x=0; x<width; x++)
                    {
                        p[0] = (byte)((int)src[x, y]);
                        p[1] = (byte)((int)src[x, y]);
                        p[2] = (byte)((int)src[x, y]);
                        p[3] = (byte)((int)255);

                        p += 4;
                    }
                    p += (bitsD.Stride - (bitsD.Width * 4));
                }
            }
            bits.UnlockBits(bitsD);

            return bits;
        }

        public static Bitmap convertToBitmap(int[,] src, int w, int h)
        {
            Bitmap bits = new Bitmap(w, h);
            BitmapData bitsD = bits.LockBits(new Rectangle(0, 0, w, h),
                                             ImageLockMode.ReadOnly,
                                             PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* p = (byte*)(void*)bitsD.Scan0;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        p[0] = (byte)((int)src[x, y]);
                        p[1] = (byte)((int)src[x, y]);
                        p[2] = (byte)((int)src[x, y]);
                        p[3] = (byte)((int)255);

                        p += 4;
                    }
                    p += (bitsD.Stride - (bitsD.Width * 4));
                }

            }
            bits.UnlockBits(bitsD);
            return bits;

        }

        public static float[,] convertToArray(Bitmap b)
        {
            float[,] res = new float[b.Width, b.Height];
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                            ImageLockMode.ReadOnly,
                                            PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* p = (byte*)(void*)bData.Scan0;

                for(int y=0; y<b.Height; y++)
                {
                    for(int x=0; x<b.Width; x++)
                    {
                        res[x, y] = (float)((p[0] + p[1] + p[2] + p[3])/4.0);
                        p += 4;
                    }
                    p += (bData.Stride - (bData.Width * 4));
                }
            }
            b.UnlockBits(bData);

            return res;
        }

        public static void threshold(int[,] Edges, int w, int h)
        {

            for(int x=1; x<=w-1; x++)
            {
                for(int y=1; y<=h-1; y++)
                {
                    if(Edges[x,y] == 1)
                    {
                        EdgeMap[x, y] = 255;
                        travers(x, y);
                        Visited[x, y] = 1;
                    }
                }
            }

            return;
        }

        private static void travers(int x, int y)
        {
            if(Visited[x,y] == 1)
            {
                return;
            }

            if(Edges[x + 1,y] == 1)
            {
                EdgeMap[x + 1, y] = 255;
                Visited[x + 1, y] = 1;
                travers(x + 1, y);
                return ;
            }
            if (Edges[x + 1, y - 1] == 1)
            {
                EdgeMap[1, y - 1] = 255;
                Visited[x + 1, y - 1] = 1;
                travers(x + 1, y - 1);
                return;
            }
            if (Edges[x, y - 1] == 1)
            {
                EdgeMap[x, y - 1] = 255;
                Visited[x, y - 1] = 1;
                travers(x, y - 1);
                return;
            }
            if (Edges[x - 1, y - 1] == 1)
            {
                EdgeMap[x - 1, y - 1] = 255;
                Visited[x - 1, y - 1] = 1;
                travers(x - 1, y - 1);
                return;
            }
            if (Edges[x - 1, y] == 1)
            {
                EdgeMap[x - 1, y] = 255;
                Visited[x - 1, y] = 1;
                travers(x - 1, y);
                return;
            }
            if (Edges[x - 1, y + 1] == 1)
            {
                EdgeMap[x - 1, y + 1] = 255;
                Visited[x - 1, y + 1] = 1;
                travers(x - 1, y + 1);
                return;
            }
            if (Edges[x, y + 1] == 1)
            {
                EdgeMap[x, y + 1] = 255;
                Visited[x, y + 1] = 1;
                travers(x, y + 1);
                return;
            }
            if (Edges[x + 1, y + 1] == 1)
            {
                EdgeMap[x + 1, y + 1] = 255;
                Visited[x + 1, y + 1] = 1;
                travers(x + 1, y + 1);
                return;
            }

            return;
        }

    }
}
