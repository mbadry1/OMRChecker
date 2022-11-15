using System;
using OpenCvSharp;
using System.Collections;

namespace OpenCVSharpSample14
{
    class Program
    {
        static void Main(string[] args)
        {
            //read img
            string imgs = "E:\\images\\t1.png";
            //////////////////////////////
            
            //calling algo
            show_bublle_Size_Rows_Columns_Gaps(imgs);
        }
        #region algorithm detect size&rows&columns
        static void show_bublle_Size_Rows_Columns_Gaps(string imgs)
        {
            CircleSquareRectangle(imgs);
        }
        #endregion
        #region  Implement Circle/Square/Rectangle in images detector
        static void CircleSquareRectangle(string imgs)
        {
            /* init code and creating variables */
            Point gaps;
            Dictionary<int, int> numXPoint = new Dictionary<int, int>();
            Dictionary<int, int> numYPoint = new Dictionary<int, int>();
            ArrayList xaxis = new ArrayList();
            ArrayList yaxis = new ArrayList();
            int xBubble = 0, yBubble = 0, coun = 0;
            float total_size = 0,avgSize=0;
            int bubbleSize;
            int totalGapX = 0, totalGapY = 0;
            Point[][] contours;

            Mat blurred = new Mat();
            Mat threshold = new Mat();

            /* convert imag to gray*/
            Mat gray = new Mat(imgs, ImreadModes.Grayscale);
            Mat src = new Mat(imgs, ImreadModes.Color);
            

            //Blurring to reduce high frequency noise to make our contour detection process more accurate.
            
            blurred = gray.GaussianBlur(new Size(5, 5), 0);

            //Binarization of the image.             
           
            threshold = blurred.Canny(60, 255);

            //find contours
            
            HierarchyIndex[] hierarchyIndexes;
            Cv2.FindContours(
                 threshold,
                 out contours,
                 out hierarchyIndexes,
                 RetrievalModes.External,
                 ContourApproximationModes.ApproxNone);

            /* drawig contours*/
            drawig_contours(src, contours);

            //loop over the contours determine total size
            foreach (var c in contours)
            {
                float equi_diameter = (int)Math.Sqrt(4 * Cv2.ContourArea(c) / Math.PI);
                //Console.WriteLine("diameter = " + equi_diameter); //print diameter
                total_size += equi_diameter;
                coun++;
            }

            /* determine avarge of bubble size */
            bubbleSize = (int)total_size / coun;

            /*determine reduis*/
            bubbleSize /= 2;

            //loop over the contours to determine number of rows amd columns
            foreach (var c in contours)
            {
                Moments m = Cv2.Moments(c);
                /*get shape of contour */
                string shape = GetShape(c);
                if (shape == "circle"||shape== "rectangle"||shape== "square")
                {
                    Point pnt = new Point(m.M10 / m.M00, m.M01 / m.M00); //center point
                    int d = (int)(m.M10 / m.M00);  //  x of center
                    if (!numXPoint.ContainsKey(d))
                    {
                        bool flag = true;
                        for(int i=d;i<d+ bubbleSize; i++) //If the bubble is slightly skewed right
                        {
                            if(numXPoint.ContainsKey(i))
                            {
                                flag = false;
                                break;
                            }
                        }
                        for (int i = d;( i > d - bubbleSize) &&flag; i--) //If the bubble is slightly skewed left
                        {
                            if (numXPoint.ContainsKey(i))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            yBubble++;
                            numXPoint.Add(d, 1);
                            xaxis.Add(d);
                        }
                    }
                    d = (int)(m.M01 / m.M00);      //y of center
                    if (!numYPoint.ContainsKey(d))
                    {
                        bool flag = true;
                        for (int i = d; i < d + bubbleSize; i++) //If the bubble is slightly skewed down
                        {
                            if (numYPoint.ContainsKey(i))
                            {
                                flag = false;
                                break;
                            }
                        }
                        for (int i = d; (i > d - bubbleSize) && flag; i--) //If the bubble is slightly skewed top
                        {
                            if (numYPoint.ContainsKey(i))
                            {
                                flag = false;
                                break;
                            }
                        }
                        if(flag)
                        {
                            xBubble++;
                            numYPoint.Add(d, 1);
                            yaxis.Add(d);
                        }
                    }
                    /*Drawing point in center of shape */
                    Cv2.Circle(src, pnt, 5, Scalar.Red, -1);

                    //Cv2.PutText(src, shape, pnt, HersheyFonts.HersheySimplex, 0.5, Scalar.Brown, 2);
                }
            }
            xaxis.Sort();
            // get total gaps for X 
            totalGapX= get_total_gaps_x(xaxis);

            yaxis.Sort();
            // get total gaps for Y 
            totalGapY = get_total_gaps_y(yaxis);

            /*  result                                      */
            //   xBubble --> number of rows
            //   yBubble --> number of columns
            avgSize = total_size / coun;
            gaps = new Point((totalGapX / (xaxis.Count - 1)), (totalGapY / (yaxis.Count - 1)));
          
            /* print */
            print_Result(src, xBubble, yBubble, avgSize, gaps);

        }
        static string GetShape(Point[] c)
        {
            string shape = "unidentified";
            double peri = Cv2.ArcLength(c, true);
            Point[] approx = Cv2.ApproxPolyDP(c, 0.01 * peri, true);


            if (approx.Length == 3) //if the shape is a triangle, it will have 3 vertices
            {
                shape = "triangle";
            }
            else if (approx.Length == 4)    //if the shape has 4 vertices, it is either a square or a rectangle
            {
                Rect rect;
                rect = Cv2.BoundingRect(approx);
                double ar = rect.Width / (double)rect.Height;

                if (ar >= 0.95 && ar <= 1.05) shape = "square";
                else shape = "rectangle";
            }
            else if (approx.Length == 5)    //if the shape has 5 vertice, it is a pantagon
            {
                shape = "pentagon";
            }
            else   //otherwise, shape is a circle
            {
                shape = "circle";
            }
            return shape;
        }
        static void drawig_contours(Mat src, Point[][] cont)
        {
            Cv2.DrawContours(src, cont, -1, Scalar.Green, 3);
        }
        static int get_total_gaps_x(ArrayList xaxis)
        {
            int totalGapX = 0;
            for (int i = 1; i < xaxis.Count; i++)
            {
                totalGapX += (int)xaxis[i] - (int)xaxis[i - 1];
            }
            return totalGapX;
        }
        static int get_total_gaps_y(ArrayList yaxis)
        {
            int totalGapY = 0;
            for (int i = 1; i < yaxis.Count; i++)
            {
                totalGapY += (int)yaxis[i] - (int)yaxis[i - 1];
            }
            return totalGapY;
        }
        static void print_Result(Mat src,int xBubble,int yBubble,float avgSize,Point gaps)
        {
            Console.WriteLine("\nnumber of row = " + xBubble);
            Console.WriteLine("number of column = " + yBubble);
            Console.WriteLine("\navg size = " + avgSize);
            Console.WriteLine("\nGaps = (" + gaps.X + " , " + gaps.Y + ")");
            using (new Window("src", src))
            {
                Cv2.WaitKey(0);
            }
        }
        #endregion
    }
}