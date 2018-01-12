using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AForge;
using AForge.Imaging;
using AForge.Math.Geometry;
using System.Drawing;
using System.Drawing.Imaging;

namespace CannyDetection
{
    public class CircleDetector
    {
        private static float MinAcceptDist = 0.7f;      //needs to be refined
        private static float RelDistLim = 0.15f;        //needs to be refined, seems to be on the threshold though for the current setup
                                                        //not sure how noise will change this

        /*
         * A class top handle the detection of circular objects inside the Bitmap frame
         * @Returns: Bitmap that is modified with the detected objects
         * @Param: Bitmap b: the input bitmap to be searched.
         */
        public static Bitmap Cirlces(Bitmap b)
        {
            BitmapData bDat = b.LockBits(new Rectangle(0, 0, b.Width, b.Height),
                                         ImageLockMode.ReadOnly,
                                         PixelFormat.Format32bppArgb);

            //check for any closed objects as a blob, this means any line that has a defined edge
            BlobCounter blober = new BlobCounter();

            blober.FilterBlobs = true;
            blober.MinHeight = 5;
            blober.MinWidth = 5;

            blober.ProcessImage(bDat);
            Blob[] blobs = blober.GetObjectsInformation();

            //to help understand where the blobs end and what is being examined im using convex hulls
            //to identify the blobs that are apprent in the picture.
            GrahamConvexHull hullFinder = new GrahamConvexHull();
            foreach(Blob bl in blobs)
            {
                List<IntPoint> edgeP = new List<IntPoint>();
                blober.GetBlobsLeftAndRightEdges(bl, out List<IntPoint> leftP, out List<IntPoint> rightP);

                edgeP.AddRange(leftP);
                edgeP.AddRange(rightP);

                List<IntPoint> hull = hullFinder.FindHull(edgeP);
                Drawing.Polygon(bDat, hull, Color.Red);

            }

            b.UnlockBits(bDat);

            SimpleShapeChecker shaper = new SimpleShapeChecker();
            shaper.MinAcceptableDistortion = MinAcceptDist;
            shaper.RelativeDistortionLimit = RelDistLim;                         

            Graphics g = Graphics.FromImage(b);
            Pen pen = new Pen(Color.Blue, 3);

            //The actual checker looking for any circle like objects in the frame.
            for (int i=0; i< blobs.Length; i++)
            {
                List<IntPoint> edges = blober.GetBlobsEdgePoints(blobs[i]);

                if (shaper.IsCircle(edges, out AForge.Point center, out float rad))
                {
                    g.DrawEllipse(pen,
                        (float)(center.X - rad), (float)(center.Y - rad),
                        (float)(rad * 2), (float)(rad * 2));
                }
            }

            return b;
        }

    }
}
