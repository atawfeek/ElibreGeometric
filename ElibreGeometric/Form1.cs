//#define UsePolyTree

using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using ClipperLib;
using ElibreGeometric;
using ElibreGeometric.Helpers;
using Elibre.Domain;
using Log;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        private Bitmap mybitmap;
        Drawing draw = new Drawing();

        private Polygons subjects = ElibreFactory.Factory.Create("subject");
        private Polygons clips = ElibreFactory.Factory.Create("clip"); 
        private Polygons solution = ElibreFactory.Factory.Create("solution"); 
#if UsePolyTree
    private PolyTree solutionTree = new PolyTree();
#endif
        //Here we are scaling all coordinates up by 100 when they're passed to Clipper 
        //via Polygon (or Polygons) objects because Clipper no longer accepts floating  
        //point values. Likewise when Clipper returns a solution in a Polygons object, 
        //we need to scale down these returned values by the same amount before displaying.
        private float scale = 100; //or 1 or 10 or 10000 etc for lesser or greater precision.

        static public PointF[] PolygonToPointFArray(List<IntPoint> pg, float scale)
        {
            PointF[] result = new PointF[pg.Count];
            for (int i = 0; i < pg.Count; ++i)
            {
                result[i].X = (float)pg[i].X / scale;
                result[i].Y = (float)pg[i].Y / scale;
            }
            return result;
        }
        
        private ILogger _ILogger;

        public Form1(ILogger ILogger)
        {
            _ILogger = ILogger;
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(Form1_MouseWheel);
            mybitmap = new Bitmap(
              pictureBox1.ClientRectangle.Width,
              pictureBox1.ClientRectangle.Height,
              PixelFormat.Format32bppArgb);
        }
        //---------------------------------------------------------------------

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0 && Constants.NudOffset < 10) Constants.NudOffset += (decimal)0.5;
            else if (e.Delta < 0 && Constants.NudOffset > -10) Constants.NudOffset -= (decimal)0.5;
        }
        //---------------------------------------------------------------------

        private void bRefresh_Click(object sender, EventArgs e)
        {
            draw.DrawBitmap(scale, pictureBox1, rbNone, rbIntersect, rbUnion, rbDifference, rbXor, clips, subjects, solution, mybitmap);
        }
        //---------------------------------------------------------------------

        private IntPoint GenerateRandomPoint(int l, int t, int r, int b, Random rand)
        {
            int Q = 10;
            return new IntPoint(
              Convert.ToInt64((rand.Next(r / Q) * Q + l + 10) * scale),
              Convert.ToInt64((rand.Next(b / Q) * Q + t + 10) * scale));
        }
        //---------------------------------------------------------------------

        public void GenerateRandomPolygon(int count)
        {
            try
            {
                int Q = 10;
                Random rand = new Random();
                int l = 10;
                int t = 10;
                int r = (pictureBox1.ClientRectangle.Width - 20) / Q * Q;
                int b = (pictureBox1.ClientRectangle.Height - 20) / Q * Q;

                subjects.Clear();
                clips.Clear();

                Polygon subj = new Polygon();
                for (int i = 0; i < count; ++i)
                    subj.Add(GenerateRandomPoint(l, t, r, b, rand));
                subjects.Add(subj);

                Polygon clip = new Polygon();
                for (int i = 0; i < count; ++i)
                    clip.Add(GenerateRandomPoint(l, t, r, b, rand));
                clips.Add(clip);

                //Testing
                //https://www.c-sharpcorner.com/article/dependency-injection-using-unity-resolve-dependency-of-dependencies/
                //throw new Exception("Test ILogger by Dependency Injection");
            }
            catch (Exception ex)
            {
                _ILogger.Log(ex);
            }
        }
       
        public PolyFillType GetPolyFillType()
        {
            return PolyFillType.pftNonZero;
        }
        //---------------------------------------------------------------------

        bool LoadFromFile(string filename, Polygons ppg, double scale = 0,
          int xOffset = 0, int yOffset = 0)
        {
            double scaling = Math.Pow(10, scale);
            ppg.Clear();
            if (!File.Exists(filename)) return false;
            using (StreamReader sr = new StreamReader(filename))
            {
                string line;
                if ((line = sr.ReadLine()) == null)
                    return false;
                int polyCnt, vertCnt;
                if (!Int32.TryParse(line, out polyCnt) || polyCnt < 0)
                    return false;
                ppg.Capacity = polyCnt;
                for (int i = 0; i < polyCnt; i++)
                {
                    if ((line = sr.ReadLine()) == null)
                        return false;
                    if (!Int32.TryParse(line, out vertCnt) || vertCnt < 0)
                        return false;
                    Polygon pg = new Polygon();
                    ppg.Add(pg);
                    for (int j = 0; j < vertCnt; j++)
                    {
                        double x, y;
                        if ((line = sr.ReadLine()) == null)
                            return false;
                        char[] delimiters = new char[] { ',', ' ' };
                        string[] vals = line.Split(delimiters);
                        if (vals.Length < 2)
                            return false;
                        if (!double.TryParse(vals[0], out x))
                            return false;
                        if (!double.TryParse(vals[1], out y))
                            if (vals.Length < 2 || !double.TryParse(vals[2], out y))
                                return false;
                        x = x * scaling + xOffset;
                        y = y * scaling + yOffset;
                        pg.Add(new IntPoint((int)Math.Round(x), (int)Math.Round(y)));
                    }
                }
            }
            return true;
        }
        //------------------------------------------------------------------------------

        void SaveToFile(string filename, Polygons ppg, int scale = 0)
        {
            double scaling = Math.Pow(10, scale);
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.Write("{0}\n", ppg.Count);
                foreach (Polygon pg in ppg)
                {
                    writer.Write("{0}\n", pg.Count);
                    foreach (IntPoint ip in pg)
                        writer.Write("{0:0.0000}, {1:0.0000}\n", ip.X / scaling, ip.Y / scaling);
                }
            }
        }
        //---------------------------------------------------------------------------


        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text =
                "Tip: Double click on polygons area to generate new polygons.";
            draw.DrawBitmap(scale, pictureBox1, rbNone, rbIntersect, rbUnion, rbDifference, rbXor, clips, subjects, solution, mybitmap);
        }
        //---------------------------------------------------------------------

        private void bClose_Click(object sender, EventArgs e)
        {
            Close();
        }
        //---------------------------------------------------------------------

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (pictureBox1.ClientRectangle.Width == 0 ||
                pictureBox1.ClientRectangle.Height == 0) return;
            if (mybitmap != null)
                mybitmap.Dispose();
            mybitmap = new Bitmap(
                pictureBox1.ClientRectangle.Width,
                pictureBox1.ClientRectangle.Height,
                PixelFormat.Format32bppArgb);
            pictureBox1.Image = mybitmap;
            draw.DrawBitmap(scale, pictureBox1, rbNone, rbIntersect, rbUnion, rbDifference, rbXor, clips, subjects, solution, mybitmap);
        }
        //---------------------------------------------------------------------

        private void rbNonZero_Click(object sender, EventArgs e)
        {
            draw.DrawBitmap(scale, pictureBox1, rbNone, rbIntersect, rbUnion, rbDifference, rbXor, clips, subjects, solution, mybitmap, true);
        }
        //---------------------------------------------------------------------

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    this.Close();
                    return;
                case Keys.Oemplus:
                case Keys.Add:
                    if (Constants.NudOffset == 10) return;
                    Constants.NudOffset += (decimal)0.5;
                    e.Handled = true;
                    break;
                case Keys.OemMinus:
                case Keys.Subtract:
                    if (Constants.NudOffset == -10) return;
                    Constants.NudOffset -= (decimal)0.5;
                    e.Handled = true;
                    break;
                case Keys.NumPad0:
                case Keys.D0:
                    if (Constants.NudOffset == 0) return;
                    Constants.NudOffset = (decimal)0;
                    e.Handled = true;
                    break;
                default: return;
            }

        }
        //---------------------------------------------------------------------

        private void nudCount_ValueChanged(object sender, EventArgs e)
        {
            draw.DrawBitmap(scale, pictureBox1, rbNone, rbIntersect, rbUnion, rbDifference, rbXor, clips, subjects, solution, mybitmap, true);
        }
        //---------------------------------------------------------------------

        private void bSave_Click(object sender, EventArgs e)
        {
            //save to SVG ...
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                SVGBuilder svg = new SVGBuilder();
                svg.style.brushClr = Color.FromArgb(0x10, 0, 0, 0x9c);
                svg.style.penClr = Color.FromArgb(0xd3, 0xd3, 0xda);
                svg.AddPolygons(subjects);
                svg.style.brushClr = Color.FromArgb(0x10, 0x9c, 0, 0);
                svg.style.penClr = Color.FromArgb(0xff, 0xa0, 0x7a);
                svg.AddPolygons(clips);
                svg.style.brushClr = Color.FromArgb(0xAA, 0x80, 0xff, 0x9c);
                svg.style.penClr = Color.FromArgb(0, 0x33, 0);
                svg.AddPolygons(solution);
                svg.SaveToFile(saveFileDialog1.FileName, 1.0 / scale);
            }
        }
        //---------------------------------------------------------------------

    }
}
