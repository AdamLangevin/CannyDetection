using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace CannyDetection
{
  public class PixelDifferentiator
  {
  

    public PixelDifferentiator()
    {
    }

    /*Convolutional matrix operation, using the Sobel operators. This
    referantially calculates the gradient of the Bitmap supplied to the calss.
    the magic happens here.
    */
    public static Bitmap Differentiate(Bitmap b)
    {
        double xr=0.0;
        double xg=0.0;
        double xb=0.0;
        double yr=0.0;
        double yg=0.0;
        double yb=0.0;
        double cr=0.0;
        double cg=0.0;
        double cb=0.0;

        int foff=1;
        int coff=0;
        int boff=0;

        double[,] xKern = Sobelx;
        double[,] yKern = Sobely;

        BitmapData bData = b.LockBits(new Rectangle(0,0,b.Width,b.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

        byte[] u = new byte[bData.Stride * b.Height];
        byte[] result = new byte[bData.Stride * b.Height];

        Marshal.Copy(bData.Scan0, u, 0, bData.Stride * b.Height);

        b.UnlockBits(bData);

        for(int offY=foff; offY < b.Height-foff; offY++)
        {
            for(int offX=foff; offX < b.Width-foff; offX++)
            {
            xr= xg= xb= yr= yg= yb= 0;
            cr= cg= cb =0.0;
            boff= offY*bData.Stride + offX*4;

            for(int y=-foff; y<=foff; y++)
            {
                for(int x=-foff; x<=foff; x++)
                {

                    coff = boff + x*4 + y*bData.Stride;
                    xb += (double)(u[coff]) * xKern[y+foff, x+foff];
                    xg += (double)(u[coff+1]) * xKern[y+foff, x+foff];
                    xr += (double)(u[coff+2]) * xKern[y+foff, x+foff];
                    yb += (double)(u[coff]) * yKern[y+foff, x+foff];
                    yg += (double)(u[coff+1]) * yKern[y+foff, x+foff];
                    yr += (double)(u[coff+2]) * yKern[y+foff, x+foff];
                }
            }

            cb = Math.Sqrt((xb*xb) + (yb*yb));
            cg = Math.Sqrt((xg*xg) + (yg*yg));
            cr = Math.Sqrt((xr*xr) + (yr*yr));


            if (cb > 255)cb = 255;
            else if (cb<0)cb = 0;
            if(cg>255)  cg=255;
            else if(cg<0)cg=0;
            if(cr>255)  cr=255;
            else if(cr<0)cr=0;

            result[boff] = (byte)(cb);
            result[boff +1] = (byte)(cg);
            result[boff +2] = (byte)(cr);
            result[boff +3] = 255;
            }
        }

        Bitmap res = new Bitmap(b.Width, b.Height);

        BitmapData resD = res.LockBits(new Rectangle(0,0,b.Width,b.Height),
                                        ImageLockMode.WriteOnly,
                                        PixelFormat.Format32bppArgb);
        

        Marshal.Copy(result, 0, resD.Scan0, result.Length);
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
