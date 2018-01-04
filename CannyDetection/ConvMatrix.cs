
namespace CannyDetection
{
    public class ConvMatrix
    {
        public int TopLeft = 0, TopMid = 0, TopRight = 0;
        public int MidLeft = 0, Pixel = 1, MidRight = 0;
        public int BottomLeft = 0, BottomMid = 0, BottomRight = 0;
        public int Factor = 1;
        public int Offset = 0;
        private int Width = 3;
        private int Height = 3;

        /*
         * A catch all for a 3x3 matrix used for mathimatical operations, most are 
         * convolvements between two array/maticies.
         * @returns nothing, but is used as an object itself
         * @Param int nVal: the initial matrix value(usually 1).
         */
        public void SetAll(int nVal)
        {
            TopLeft = TopMid = TopRight = MidLeft = Pixel = MidRight =
                      BottomLeft = BottomMid = BottomRight = nVal;
        }

        /*
         * Standard get method to retrieve the width of a convolutional matrix.
         */
        public int getWidth()
        {
            return Width;
        }

        /*
         * Standard get method for the height of a convolutional matrix.
         */
        public int getHeight()
        {
            return Height;
        }

    }
}
