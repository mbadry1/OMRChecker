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
            string imgs = "E:\\images\\test bubble filled\\29.png";
          /*  Mat img = new Mat(imgs);
            Mat img2 = new Mat();
            Mat img3 = new Mat();
            //byte[] bytes = (byte[])(new ImageConverter()).ConvertTo(img, typeof(byte[]));
            //byte[] bytes2;
            img3 = img;
            Cv2.Erode(img,img2,new Mat(),null,3,BorderTypes.Constant,null);
            
            new Window("src", img);
            using (new Window("result",img2))
            {
                Cv2.WaitKey(0);
            }*/

            
            Method m = new Method();
            // return point , (src image,sensitive)
            Convert_json js = new Convert_json();
           // js.set("E:\\images\\test bubble filled\\2.json");
           // js.LoadJson();
            m.filled_bubble(imgs,60);
           
        }
    }
    public struct Data
    {
        public int x;
        public int y;
    }
    #region read from json
    public class Convert_json
    {
        string path;

        public void set(string s)
        {
            path = s;
        }
        public List<Data> LoadJson()
        {

            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                List<Data> items = JsonConvert.DeserializeObject<List<Data>>(json);
                foreach (var t in items)
                {
                  //  Console.WriteLine(t.x);
                  //  Console.WriteLine(t.y);
                }
                // Console.WriteLine(items.size);
                return items;
            }
        }
    }
    #endregion
    public class Method
    {
      
        public List<Data> filled_bubble(string imgs,int sen)
        {
            List<Data> items=new List<Data>();
            //sen = 100 - sen;
            sen = sen - (sen / 2);
            Point[][] contours;
            float equi_diameter;
            int radius;
            Point pnt=new Point();
            int num_pixels_empty = 0, con2 = 0;

            Mat blurred = new Mat();
            Mat threshold = new Mat();

            /* convert imag to gray*/
            Mat gray = new Mat(imgs, ImreadModes.Grayscale);
            Mat src = new Mat(imgs, ImreadModes.Color);
            Mat mat2 = new Mat();
            //Cv2.Erode(gray, gray, new Mat(), null, 1, BorderTypes.Constant, null);
            //Cv2.Dilate(gray, gray, new Mat(), null, 3, BorderTypes.Constant, null);

            
            Cv2.Erode(gray, gray, new Mat(), null, 2, BorderTypes.Constant, null);
            Cv2.Dilate(gray, gray, new Mat(), null, 6, BorderTypes.Constant, null);//4
            
            Cv2.Threshold(gray, mat2, 120, 255, ThresholdTypes.Binary);
          

            blurred = mat2.GaussianBlur(new Size(5, 5), 0);
            threshold = blurred.Canny(60, 255);
            HierarchyIndex[] hierarchyIndexes;
            Cv2.FindContours(
                 threshold,
                 out contours,
                 out hierarchyIndexes,
                 RetrievalModes.External,
                 ContourApproximationModes.ApproxNone);

            
            foreach (var c in contours)
            {
                equi_diameter = (int)Math.Sqrt(4 * Cv2.ContourArea(c) / Math.PI);

                Moments m = Cv2.Moments(c);
                radius = (int)equi_diameter / 2;
         
                pnt = new Point(m.M10 / m.M00, m.M01 / m.M00); //center point

                // Console.WriteLine(" contours number   "+con++);
                int num_black = 0;
                for (int x = pnt.X - radius; x < pnt.X + radius; x++)
                {
                    for (int y = pnt.Y - radius; y < pnt.Y + radius; y++)
                    {
                        if (mat2.Get<int>(y, x) != 0)
                        {
                            num_pixels_empty++;
                        }
                        else
                        {
                            num_black++;
                        }
                    }
                }
                int num_pixels = (int)equi_diameter* (int)equi_diameter;
                //num_pixels-= num_pixels_empty
                if (num_black >= num_pixels / (100/sen)&&(pnt.X>=0&&pnt.Y>=0)&& num_black>0)
                {

                    Console.WriteLine("\n con " + con2 + " = ( " + pnt.X + "," + pnt.Y + ") ");//= " + num_black);
                    Cv2.Rectangle(src, new Point(pnt.X + radius, pnt.Y + radius), new Point(pnt.X - radius, pnt.Y - radius), 255, 1);

                    Cv2.Circle(src, pnt, radius, Scalar.Red, -1);
                    Data t = new Data();
                    t.x = pnt.X;
                    t.y = pnt.Y;
                    items.Add(t);
      
                }
                con2++;
                num_pixels_empty = 0;
            }

            Mat img3 = new Mat(imgs);
            new Window("threshold", mat2);
            new Window("src", threshold);
            using (new Window("result", src))
            {
                Cv2.WaitKey(0);
            }
            Point p = new Point(pnt.X,pnt.Y);
         
            return items;
        }
    }
}