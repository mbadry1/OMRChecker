using System;
using OpenCvSharp;
using System.Collections;
using Newtonsoft.Json;

namespace OpenCVSharpSample14
{
    class Program
    {
        static void Main(string[] args)
        {
            string imgs = "E:\\images\\testing\\31.png";
            Method m = new Method();
            data d=m.CircleSquareRectangle(imgs);
            //  Method m= new Method(4,5);
            //Console.WriteLine(m.mult());
            // Convert_json j = new Convert_json();
            //j.LoadJson();
        }
    }
    #region read from json
    public class Convert_json
    {
        string path;

        public void set(string s)
        {
            path = s;
        }
        public data LoadJson()
        {
            
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                data items = JsonConvert.DeserializeObject<data>(json);
                Console.WriteLine(items.stat);
                Console.WriteLine(items.x);
                Console.WriteLine(items.y);
               // Console.WriteLine(items.size);
                return items;
            }
        }
    }
    #endregion
    #region struct data
    public struct data
    {
        public string stat;
        public int x;
        public int y;
    }
    #endregion
    #region class Method
    public class Method
    {
       
        public data CircleSquareRectangle(string imgs)
        {
            /* init code and creating variables */
            Point gaps;
            Dictionary<int, int> numXPoint = new Dictionary<int, int>();
            Dictionary<int, int> numYPoint = new Dictionary<int, int>();
            ArrayList xaxis = new ArrayList();
            ArrayList yaxis = new ArrayList();
            int xBubble = 0, yBubble = 0, coun = 0;
            float total_size = 0, avgSize = 0;
            int bubbleSize=0;
            int totalGapX = 0, totalGapY = 0;
            Point[][] contours;
            data result;
            int maxSize = 0, minSize = 900000, maxD = 0;
            int maxgx = 0, mingx = 900000, maxgy = 0, mingy = 900000;

            Mat blurred = new Mat();
            Mat threshold = new Mat();

            /* convert imag to gray*/
            Mat gray = new Mat(imgs, ImreadModes.Grayscale);
            Mat src = new Mat(imgs, ImreadModes.Color);


            //Blurring to reduce high frequency noise to make our contour detection process more accurate.
            if (!src.Empty())
                blurred = gray.GaussianBlur(new Size(5, 5), 0);
            else
            {
                Console.WriteLine("\nError");
                result.x = 0;
                result.y = 0;
                result.stat = "error";
                return result;
            }

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
          

            float last= (int)Math.Sqrt(4 * Cv2.ContourArea(contours[0]) / Math.PI);
            foreach (var c in contours)
            {
                float equi_diameter = (int)Math.Sqrt(4 * Cv2.ContourArea(c) / Math.PI);
                if (maxD < Math.Abs(last - equi_diameter))
                    maxD = (int)Math.Abs(last - equi_diameter);
                last = equi_diameter;

            }
            maxD /= 2;
            coun = 0;
            foreach (var c in contours)
            {
                float equi_diameter = (int)Math.Sqrt(4 * Cv2.ContourArea(c) / Math.PI);
                // Console.WriteLine("diameter = " + equi_diameter); //print diameter
                if ((int)equi_diameter >maxD)
                {
                    total_size += equi_diameter;
                    if ((int)equi_diameter > maxSize)
                        maxSize = (int)equi_diameter;
                    if ((int)equi_diameter < minSize)
                        minSize = (int)equi_diameter;
                    coun++;
                }
            }

            /* determine avarge of bubble size */
            if (coun != 0)
                bubbleSize = (int)total_size / coun;
            else
            {
                Console.WriteLine("\nError");
                result.x = 0;
                result.y = 0;
                result.stat = "error";
                return result;
            }

            /*determine reduis*/
            bubbleSize /= 2;

            //loop over the contours to determine number of rows amd columns
            foreach (var c in contours)
            {
                Moments m = Cv2.Moments(c);
                /*get shape of contour */
                string shape = GetShape(c);
                if (shape == "circle" || shape == "rectangle" || shape == "square")
                {
                    Point pnt = new Point(m.M10 / m.M00, m.M01 / m.M00); //center point
                    int d = (int)(m.M10 / m.M00);  //  x of center
                    float diameter = (int)Math.Sqrt(4 * Cv2.ContourArea(c) / Math.PI);
                    if (!numXPoint.ContainsKey(d)&& diameter>maxD)
                    {
                        bool flag = true;
                        for (int i = d; i < d + bubbleSize; i++) //If the bubble is slightly skewed right
                        {
                            if (numXPoint.ContainsKey(i))
                            {
                                flag = false;
                                break;
                            }
                        }
                        for (int i = d; (i > d - bubbleSize) && flag; i--) //If the bubble is slightly skewed left
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
                    if (!numYPoint.ContainsKey(d) && diameter > maxD)
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
                        if (flag)
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
            // get total gaps for X 
            xaxis.Sort();
            totalGapX = get_total_gaps(xaxis);

            yaxis.Sort();
            // get total gaps for Y 
            totalGapY = get_total_gaps(yaxis);

            /*  result                       */
            //   xBubble --> number of rows
            //   yBubble --> number of columns
            avgSize = total_size / coun;
            
            for (int i = 1; i < xaxis.Count; i++)
            {
                int temp = (int)xaxis[i] - (int)xaxis[i - 1];
                if (temp > maxgx)
                    maxgx = temp;
                if (temp < mingx)
                    mingx = temp;
            }
            for (int i = 1; i < yaxis.Count; i++)
            {
                int temp = (int)yaxis[i] - (int)yaxis[i - 1];
                if (temp > maxgy)
                    maxgy = temp;
                if (temp < mingy)
                    mingy = temp;
            }

            if (maxSize - minSize >= avgSize)// || xaxis.Count == 1 || yaxis.Count == 1)
            {
                Console.WriteLine("\nError1");
                result.x = 0;
                result.y = 0;
                result.stat = "error";
                return result;
            }
            else
            {
                int xgap, ygap;
                if (xaxis.Count > 1)
                    xgap = xaxis.Count - 1;
                else
                    xgap = xaxis.Count ;
                if (yaxis.Count > 1)
                    ygap = yaxis.Count - 1;
                else
                    ygap = yaxis.Count;
                if(xgap==0||ygap==0)
                {
                    Console.WriteLine("\nError2");
                    result.x = 0;
                    result.y = 0;
                    result.stat = "error";
                    return result;
                }
                gaps = new Point((totalGapX / xgap), (totalGapY / ygap));
                
              
                if(maxgx - mingx >= (totalGapX / xgap)|| maxgy - mingy >= (totalGapY / ygap)|| ((totalGapY / ygap)==0&& (totalGapX / xgap)==0))
                {
                    Console.WriteLine("\nError3");
                    result.x = 0;
                    result.y = 0;
                    result.stat = "error";
                    return result;
                }
                result.x = xBubble;
                result.y = yBubble;
                result.stat = "valid";
                /* print */
                print_Result(src, imgs, xBubble, yBubble, avgSize, gaps);
                return result;
            }
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
            else if(approx.Length==2)
            {
                shape = "line";
            }
            else if(approx.Length == 1)
            {
                shape = "point";
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
        static int get_total_gaps(ArrayList axis)
        {
            int totalGap = 0;
            for (int i = 1; i < axis.Count; i++)
            {
                totalGap += (int)axis[i] - (int)axis[i - 1];
            }
            return totalGap;
        }
        static void print_Result(Mat src, string imgs, int xBubble, int yBubble, float avgSize, Point gaps)
        {
            Mat srccolor = new Mat(imgs, ImreadModes.Color);
            Console.WriteLine("\nnumber of row = " + xBubble);
            Console.WriteLine("number of column = " + yBubble);
            Console.WriteLine("\navg size = " + avgSize);
            Console.WriteLine("\nGaps = (" + gaps.X + " , " + gaps.Y + ")");
            new Window("src", srccolor);
            using (new Window("result", src))
            {
                Cv2.WaitKey(0);
            }
        }

    }
    #endregion
}