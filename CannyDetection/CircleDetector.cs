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
        private static float MinAcceptDist = 0.3f;      //needs to be refined
        private static float RelDistLim = 0.03f;        //needs to be refined, seems to be on the threshold though for the current setup
                                                        //not sure how noise will change this

        private const float FOCALLENGTH = 3.04f;        //Rpi camera focal length in mm.
        private const float SENSORHEIGHT = 2.76f;       //The Rpi camera physical sensor height in mm.
        private const float GOLFBALLHIEGHT = 42.67f;    //The physical height of a golf ball in mm.

        //a structure to hold the center points and radius information
        private struct circ
        {
            AForge.Point p;
            float rad;

            public circ(AForge.Point point, float r)
            {
                p = point;
                rad = r;
            }
            public float getRad() { return rad; }
            public AForge.Point getP() { return p; }
        }

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

            List<AForge.Point> centers = new List<AForge.Point>();
            List<circ> cents = new List<circ>();

            //The actual checker looking for any circle like objects in the frame.
            for (int i=0; i< blobs.Length; i++)
            {
                List<IntPoint> edges = blober.GetBlobsEdgePoints(blobs[i]);
                circ c;
                if (shaper.IsCircle(edges, out AForge.Point center, out float rad))
                {
                    g.DrawEllipse(pen,
                        (float)(center.X - rad), (float)(center.Y - rad),
                        (float)(rad * 2), (float)(rad * 2));
                    centers.Add(center);

                    c = new circ(center, rad);
                    cents.Add(c);

                    drawCent(center, g);
                }
            }

            if (centers.Count() == 0)
            {
                System.Console.WriteLine("Empty List, no circles detected.");
            } else
            {
                writeToTxtFile(cents, b.Height, b.Width);
                foreach (AForge.Point p in centers)
                {
                    System.Console.WriteLine("Circle located at: {0}, {1}", p.X, p.Y);
                }
            }

            return b;
        }

        //Handles the drawing of the center cross in each circle that has been detected.
        private static void drawCent(AForge.Point center, Graphics g)
        {
            System.Drawing.Point p1 = new System.Drawing.Point((int)center.X, (int)center.Y);
            Pen pen = new Pen(Color.Green, 2);

            g.DrawLine(pen, new System.Drawing.Point(p1.X - 3, p1.Y), new System.Drawing.Point(p1.X + 3, p1.Y));
            g.DrawLine(pen, new System.Drawing.Point(p1.X, p1.Y - 3), new System.Drawing.Point(p1.X, p1.Y + 3));
        }

        //A class to writh the found circle locations as distances and angles to a text file.
        private static void writeToTxtFile(List<circ> c, int height, int width)
        {
            int CX = width / 2;
            int CY = height / 2;

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\py120\Desktop\Dev\objectDetection\CannyDetection\distances.txt"))
            {
                foreach (circ cir in c)
                {
                    String line;
                    int dist;
                    double angle;

                    dist = (int)((FOCALLENGTH * GOLFBALLHIEGHT * height) / (cir.getRad() * 2 * SENSORHEIGHT));
                    angle = (180/Math.PI) * Math.Atan((CX - cir.getP().X)/dist); 

                    line = Convert.ToString(dist);
                    line +=" , " + Convert.ToString(angle);

                    System.Console.WriteLine("Circle located at a distance of: {0}, angle of {1} degrees from center.", dist, angle);
                        //cir.getP().X, cir.getP().Y, cir.getRad()*2);
                
                    file.WriteLine(line);   
                }
            }
        }
    }
}
