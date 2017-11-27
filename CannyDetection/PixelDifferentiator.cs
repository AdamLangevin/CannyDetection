using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace CannyDetection
{
  public class PixelDifferentiator
  {
    /*
     * Convolutional matrix operation, using the Sobel operators. This
     * referantially calculates the gradient of the Bitmap supplied to the calss.
     * the magic happens here. The soble opperator is a matrix opperator, convolving
     * it with a Bitmap yeilds the pixel data used to calculate the gradeient feild later
     * returns the Bitmap filtered byt the Sobel operation
     * @Param Bitmap b: The source image to be filtered
    */
    public static Bitmap Differentiate(Bitmap b)
    {
        double xr=0.0;
        double xg=0.0;
        double xb=0.0;
        double yr=0.0;
        double yg=0.0;
        double yb=0.0;
        double xa = 0.0;
        double ya = 0.0;
        double cr=0.0;
        double cg=0.0;
        double cb=0.0;
        double ca = 0.0;

        double[,] xKern = Sobelx;
        double[,] yKern = Sobely;

        BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                        ImageLockMode.ReadOnly,
                                        PixelFormat.Format32bppArgb);

        Bitmap res = (Bitmap)b.Clone();

        BitmapData resD = res.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                        ImageLockMode.WriteOnly,
                                        PixelFormat.Format32bppArgb);

        byte[] u = new byte[bData.Stride * b.Height];
        byte[] result = new byte[bData.Stride * b.Height];
        
        
        unsafe {
            byte* p = (byte*)(void*)resD.Scan0;
            int nOffset = bData.Stride - b.Width * 4;
            int nWdith = b.Width - 2;
            int nHeight = b.Height - 2;
            int pixelSize = 4;
            int stride = resD.Stride;

            for (int y =1; y < b.Height - 1; y++) {
                for (int x=1; x < b.Width - 1; x++) {
                        xr = p[0] * -1
                            + p[2 * pixelSize]
                            + p[stride] * -2
                            + p[stride + 2 * pixelSize] * 2
                            + p[2 * stride] * -1
                            + p[2 * stride + 2 * pixelSize];
                        yr = p[0]
                            + p[pixelSize] * 2
                            + p[2 * pixelSize]
                            + p[2 * stride] * -1
                            + p[pixelSize + 2 * stride] * -2
                            + p[2 * stride + 2 * pixelSize] * -1;

                        xb = p[1] * -1
                            + p[1 + 2 * pixelSize]
                            + p[1 + stride] * -2
                            + p[1 + stride + 2 * pixelSize] * 2
                            + p[1 + 2 * stride] * -1
                            + p[1 + 2 * stride + 2 * pixelSize];
                        yb = p[1] 
                            + p[1 + pixelSize] * 2
                            + p[1 + 2 * pixelSize]
                            + p[1 + 2 * stride] * -1
                            + p[1 + pixelSize + 2 * stride] * -2
                            + p[1 + 2 * stride + 2 * pixelSize] * -1;

                        xg = p[2] * -1
                            + p[2 + 2 * pixelSize] 
                            + p[2 + stride] * -2
                            + p[2 + stride + 2 * pixelSize] * 2
                            + p[2 + 2 * stride] * -1
                            + p[2 + 2 * stride + 2 * pixelSize];
                        yg = p[2]
                            + p[2 + pixelSize] * 2
                            + p[2 + 2 * pixelSize]
                            + p[2 + 2 * stride] * -1
                            + p[2 + pixelSize + 2 * stride] * -2
                            + p[2 + 2 * stride + 2 * pixelSize] * -1;

                        xa = p[3] * -1
                            + p[3 + 2 * pixelSize]
                            + p[3 + stride] * -2
                            + p[3 + stride + 2 * pixelSize] * 2
                            + p[3 + 2 * stride] * -1
                            + p[3 + 2 * stride + 2 * pixelSize];
                        ya = p[3]
                            + p[3 + pixelSize] * 2
                            + p[3 + 2 * pixelSize]
                            + p[3 + 2 * stride] * -1
                            + p[3 + pixelSize + 2 * stride] * -2
                            + p[3 + 2 * stride + 2 * pixelSize];

                        //Basic Pythagoran's Theroem
                        cr = Math.Sqrt(xr*xr + yr*yr);
                        cb = Math.Sqrt(xb*xb + yb*yb);
                        cg = Math.Sqrt(xg*xg + yg*yg);
                        ca = Math.Sqrt(xa * xa + ya * ya);

                        //checking for values inside the bounds of the 8bit limit to a colour vlaue.
                        if (cr > 255) cr = 255;
                        else if(cr < 0) cr = 0;
                        if (cb > 255) cb = 255;
                        else if (cb < 0) cb = 0;
                        if (cg > 255) cg = 255;
                        else if (cg < 0) cg = 0;
                        if (ca > 255) ca = 255;
                        else if (ca < 0) ca = 0;

                        //set each pixel value to the new filtered value
                        p[0] = (byte)cr;
                        p[1] = (byte)cb;
                        p[2] = (byte)cg;
                        p[3] = (byte)ca;

                        xr = yr = xb = yb = xg = yg = xa = ya = ca = cr = cg = cb = 0;
                        //Console.WriteLine("x: " + x + " y: " + y);
                        //Console.ReadLine();
                        p += 4;
                    }
                    p += nOffset;
            }
        } //end of unsafe code               
        b.UnlockBits(bData);       
        res.UnlockBits(resD);
        return res;

    }

    //Sobel matrix operator in the x plane
    private static double[,] Sobelx
    {
        get
        {
            return new double[,] {
                { -1, 0, 1},
                { -2, 0, 2},
                { -1, 0, 1}
            };
        }
    }

    //Sobel matrix operator in the y plane
    private static double[,] Sobely
    {
        get
        {
            return new double[,] {
                { 1, 2, 1},
                { 0, 0, 0},
                { -1, -2, -1}
            };
        }
    }

  }
}
