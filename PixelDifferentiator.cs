using System.Drawing.Bitmap;
using System.Drawing.BitmapData;


namespace CannyDetection
{
  public class PixelDifferentiator
  {
    //private Bitmap b;
    private BitmapData bData;

    public PixelDifferentiator()
    {
      //b = src;
    }

    /*Convolutional matrix operation, using the Sobel operators. This
    referantially calculates the gradient of the Bitmap supplied to the calss.
    the magic happens here.
    */
    public static Bitmap differentiate(Bitmap b)
    {
      double xr=0;
      double xg=0;
      double xb=0;
      double yr=0;
      double yg=0;
      double yb=0;
      double cr=0;
      double cg=0;
      double cb=0;

      int foff=0;
      int coff=0;
      int boff=0;

      double[,] xKern = Sobelx();
      double[,] yKern = Sobel;y();

      bData = b.LockBits(new Rectangle(0,0,b.Width,b.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

      byte[] result = new byte[bData.Stride * bData.Height];

      for(int offY=foff; offY < b.Height-foff; offY++)
      {
          for(int offX=foff; offX < b.Width-foff; offX++)
          {
            xr= xg= xb= yr= yg= yb= 0;
            cr= cg= cb =0.0;
            boff= offY*bData.Stride + offX*4;

            for(int i=-foff; i<=foff; i++)
            {
              for(int j=-foff; j<=foff; j++)
              {
                coff = boff + offX*4 + offY*bData.Stride;

                xb += (double)(b[coff]) * xKern[i+foff, j+foff];
                xg += (double)(b[coff+1]) * xKern[i+foff, j+foff];
                xr += (double)(b[coff+2]) * xKern[i+foff, j+foff];
                yb += (double)(b[coff]) * yKern[i+foff, j+foff];
                yg += (double)(b[coff+1]) * yKern[i+foff, j+foff];
                yr += (double)(b[cofff+2]) * yKern[i+foff, j+foff];
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

      Marshal.Copy(result, 0, bData.Scan0, result.Lenght);
      return b.UnlockBits(bData);
    }

    //Sobel matrix operator in the x plane
    private static double[,] Sobelx()
    {
      return d = new double[3,3] = (
                                  (-1,0,1),
                                  (-2,0,2),
                                  (-1,0,1)
                                  );
    }

    //Sobel matrix operator in the y plane
    private static double[,] Sobely()
    {
      return d = new double[3,3] = (
                                (1,2,1),
                                (0,0,0),
                                (-1,-2,-1)
                                );
    }

  }
}
