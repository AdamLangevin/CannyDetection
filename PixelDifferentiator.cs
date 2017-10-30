using System.Drawing.Bitmap;
using System.Drawing.BitmapData;


namespace CannyDetection
{
  public static PixelDifferentiator(Bitmap src)
  {
    private Bitmap b;
    private BitmapData bData;

    public PixelDifferentiator(Bitmap src)
    {
      b = src;
    }

    public static Bitmap differentiate()
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

      bData = b.LockBits(new Rectangle(0,0,b.Width,b.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format32bppArgb);

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

              }
            }

          }
      }
    }

    private static double[,] Sobelx()
    {
      return d = new double[,] = {
                                  {-1,0,1},
                                  {-2,0,2},
                                  {-1,0,1}
                                  };
    }
    private static double[,] Sobley()
    {
      return d = new double[,] = {
                                {1,2,1},
                                {0,0,0},
                                {-1,-2,-1}
                                };
    }

  }
}
