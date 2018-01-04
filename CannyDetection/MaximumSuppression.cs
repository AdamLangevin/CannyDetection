using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace CannyDetection
{
    class MaximumSuppression
    {
        //To beable to efficiently traverse the edges recursivly having the edges as global variables
        //helps to keep the memory useage down.
        public static int[,] EdgeMap;
        public static int[,] Visited;
        public static int[,] Edges;

        /*
        * This class provides the ssecond order differentiation that is needed to get the gradient 
        * used to determine the directions the change is the values are moving in. This helps to
        * determine the pixels that are needed to be suppressed.
        * @returns the Bitmap of the filtered image shown in black and white 32-bit ARGB word format.
        * @Param Bitmap b: the source Bitmap
        */
        public static Bitmap Suppression(Bitmap b)
        {
            Bitmap divX;
            Bitmap divY;
            Bitmap NonMax;
            divX = (Bitmap)b.Clone();
            divY = (Bitmap)b.Clone();
            NonMax = (Bitmap)b.Clone();

            //m is used to calcualte the derivative of the pixel values surrounding it,
            //with respect to each rectangular direction.
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
            float[,] Grade = new float[b.Width, b.Height];
            float[,] NMax = new float[b.Width, b.Height];
            int[,] postHyst = new int[b.Width, b.Height];
            int lim = (int)(m.getWidth() / 2);      //the allowable limit due to the size of the convolvement array
            int nWidthN = NonMax.Width - lim;
            int nHeightN = NonMax.Height - lim;
            float nPixel;
            float tangent = 0;
            float c1 = 0;     //center pixel to be differentiated against
            float c2 = 0;     //test pixel one
            float c3 = 0;     //test pixel two

            Edges = new int[b.Width, b.Height];

            //calculation of the gradeint of values pixel by pixel.
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
            for (int y = lim; y< nHeightN; y++) {
                for (int x = lim; x < nWidthN; x++) {

                    //setting the tangent angle
                    c1 = aDivX[x, y];
                    c2 = aDivY[x, y];
                    if (c1 == 0) {
                        tangent = 90F;
                    } else {
                        tangent = (float)(Math.Atan(c2 / c1) * 180 / Math.PI);
                    }

                    //using all three colour channels
                    //Horizontal Edge
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

            //copy the non-max suppressed array, to go and check
            for (int x = 1; x < b.Width - 1; x++)
            {
                for (int y = 1; y < b.Height - 1; y++)
                {
                    postHyst[x, y] = (int)NMax[x, y];
                }
            }

            EdgeMap = new int[b.Width, b.Height];
            Visited = new int[b.Width, b.Height];

            float MaxHyst = 80f;            //upper threshold, values above this are true edges
            float MinHyst = 60f;            //lower threshold, values below this are not edges
            
            //check for pixels above the Max, and values that are inbetween
            //The values inbetween might be edges, but require more processing
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
                        Edges[x, y] = 2;
                    }
                }
            }

            //checks the vlaues that lie between the thresholds to find the true edges, and remove the single pixels.
            threshold(Edges, b.Width, b.Height);

            //reset all values to 255, ie all three RGB values will end up 255, showing white pixels
            for(int y=0; y<b.Height; y++)
            {
                for(int x=0; x<b.Width; x++)
                {
                    EdgeMap[x, y] = EdgeMap[x, y] * 255;
                }
            }

            //convert to Bitmap to be able to display the image
            NonMax = convertToBitmap(EdgeMap, b.Width, b.Height);

            return NonMax;
        }

        /*
         * A function that converts a float array to a Bitmap.
         * @returns the Bitmap after processing.
         * @Param float[,] src: the source image to be traslated to a Bitmap.
         * @Param int width: the target image width.
         * @Param int height: the target image height.
         */
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
                        p[3] = (byte)((int)255);            //alpha value s.t. background colour is not visable.

                        p += 4;
                    }
                    p += (bitsD.Stride - (bitsD.Width * 4));
                }
            }
            bits.UnlockBits(bitsD);

            return bits;
        }

        //A useful funciton to convert integer arrays to bitmaps
        /*
         * A function to convert an integer array to a Bitmap image. The output file type is unknown at this stage.
         * @returns the new Bitmap in 32-bit ARGB word format.
         * @Param int[,] src: the source integer array to be translated
         * @Param int w: the target width of the array of values.
         * @Param int h: the target height of the arry of values.
         */
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

        /*
         * This is a function to convert a Bitmap containing a 32-bit ARGB word to a float array.
         * @Returns a float array
         * @Param Bitmap b: the source image, can be  a  jpeg, BMP, or PNG image.
         */
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

        /*
         *A function to check the threshold values of nighbours to the pixel elements
         * @Returns no explicite values, implicily changes the EdgeMap, Visited, and Edges arrays.
         * @Param int[,] Edges: is an array which represents 8-bit colour values of a grey image
         * @Param int w: Is the width of the array, and source image.
         * @Param int h: The value of the hight of the source image array.
         */
        public static void threshold(int[,] Edges, int w, int h)
        {

            for(int x=1; x<=w-1; x++)
            {
                for(int y=1; y<=h-1; y++)
                {
                    if(Edges[x,y] == 1)
                    {
                        EdgeMap[x, y] = 1;
                        travers(x, y);
                        Visited[x, y] = 1;
                    }
                }
            }

            return;
        }

        /*
         * A recursive function used to travers all neighbours to the testing pixel, if the pixel has a value of 2,
         * the pixel has a chance to be an edge member.
         * @Returns implicially the vlaues to the Visited array, and EdgeMap array
         * @Param int x: the current test pixel x-location.
         * @Param int y: the current test pixel y-location.
         */
        private static void travers(int x, int y)
        {
            //base case, the pixel has already been visited.
            if(Visited[x,y] == 1)
            {
                return;
            }

            //middle right neighbour
            if(Edges[x + 1,y] == 2)
            {
                EdgeMap[x + 1, y] = 1;
                Visited[x + 1, y] = 1;
                travers(x + 1, y);
                return ;
            }

            //top rigth neighbour
            if (Edges[x + 1, y - 1] == 2)
            {
                EdgeMap[1, y - 1] = 1;
                Visited[x + 1, y - 1] = 1;
                travers(x + 1, y - 1);
                return;
            }

            //top middle neighbour
            if (Edges[x, y - 1] == 2)
            {
                EdgeMap[x, y - 1] = 1;
                Visited[x, y - 1] = 1;
                travers(x, y - 1);
                return;
            }

            //top left neighbour
            if (Edges[x - 1, y - 1] == 2)
            {
                EdgeMap[x - 1, y - 1] = 1;
                Visited[x - 1, y - 1] = 1;
                travers(x - 1, y - 1);
                return;
            }

            //middle left neighbour
            if (Edges[x - 1, y] == 2)
            {
                EdgeMap[x - 1, y] = 1;
                Visited[x - 1, y] = 1;
                travers(x - 1, y);
                return;
            }

            //bottom left neighbour
            if (Edges[x - 1, y + 1] == 2)
            {
                EdgeMap[x - 1, y + 1] = 1;
                Visited[x - 1, y + 1] = 1;
                travers(x - 1, y + 1);
                return;
            }

            //bottom middle neighbour
            if (Edges[x, y + 1] == 2)
            {
                EdgeMap[x, y + 1] = 1;
                Visited[x, y + 1] = 1;
                travers(x, y + 1);
                return;
            }

            //bottom right neighbour
            if (Edges[x + 1, y + 1] == 2)
            {
                EdgeMap[x + 1, y + 1] = 1;
                Visited[x + 1, y + 1] = 1;
                travers(x + 1, y + 1);
                return;
            }

            return;
        }

    }
}
