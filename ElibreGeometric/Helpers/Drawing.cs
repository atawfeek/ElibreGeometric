using ClipperLib;
using Elibre.Domain;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApplication1;

namespace ElibreGeometric.Helpers
{
    public class Drawing
    {

        public void DrawBitmap(float scale, PictureBox pictureBox1, RadioButton rbNone, RadioButton rbIntersect, RadioButton rbUnion, RadioButton rbDifference, RadioButton rbXor, Polygons clips, Polygons subjects, Polygons solution, Bitmap mybitmap, bool justClip = false)
        {
            Cursor.Current = Cursors.WaitCursor;
            Form1 form = DependencyInjector.Retrieve<Form1>();
            try
            {
                if (!justClip)
                {
                    form.GenerateRandomPolygon((int)Constants.NudCount);
                }
                using (Graphics newgraphic = Graphics.FromImage(mybitmap))
                using (GraphicsPath path = new GraphicsPath())
                {
                    newgraphic.SmoothingMode = SmoothingMode.AntiAlias;
                    newgraphic.Clear(Color.White);

                    path.FillMode = FillMode.Winding;

                    //draw subjects ...
                    foreach (Polygon pg in subjects)
                    {
                        PointF[] pts = Form1.PolygonToPointFArray(pg, scale);
                        path.AddPolygon(pts);
                        pts = null;
                    }
                    using (Pen myPen = new Pen(Color.FromArgb(196, 0xC3, 0xC9, 0xCF), (float)0.6))
                    using (SolidBrush myBrush = new SolidBrush(Color.FromArgb(127, 0xDD, 0xDD, 0xF0)))
                    {
                        newgraphic.FillPath(myBrush, path);
                        newgraphic.DrawPath(myPen, path);
                        path.Reset();

                        path.FillMode = FillMode.Winding;

                        foreach (Polygon pg in clips)
                        {
                            PointF[] pts = Form1.PolygonToPointFArray(pg, scale);
                            path.AddPolygon(pts);
                            pts = null;
                        }
                        myPen.Color = Color.FromArgb(196, 0xF9, 0xBE, 0xA6);
                        myBrush.Color = Color.FromArgb(127, 0xFF, 0xE0, 0xE0);
                        newgraphic.FillPath(myBrush, path);
                        newgraphic.DrawPath(myPen, path);

                        //do the clipping ...
                        if ((clips.Count > 0 || subjects.Count > 0) && !rbNone.Checked)
                        {
                            List<List<IntPoint>> solution2 = new List<List<IntPoint>>();
                            Clipper c = new Clipper();
                            c.AddPaths(subjects, PolyType.ptSubject, true);
                            c.AddPaths(clips, PolyType.ptClip, true);
                            solution.Clear();
#if UsePolyTree
              bool succeeded = c.Execute(GetClipType(), solutionTree, GetPolyFillType(), GetPolyFillType());
              //nb: we aren't doing anything useful here with solutionTree except to show
              //that it works. Convert PolyTree back to Polygons structure ...
              Clipper.PolyTreeToPolygons(solutionTree, solution);
#else
                            ClipType clipType;
                            if (rbIntersect.Checked) clipType = ClipType.ctIntersection;
                            else if (rbUnion.Checked) clipType = ClipType.ctUnion;
                            else if (rbDifference.Checked) clipType = ClipType.ctDifference;
                            else if (rbXor.Checked) clipType = ClipType.ctXor;
                            else clipType = ClipType.ctXor;

                            bool succeeded = c.Execute(clipType, solution, form.GetPolyFillType(), form.GetPolyFillType());
#endif
                            if (succeeded)
                            {
                                //SaveToFile("solution", solution);
                                myBrush.Color = Color.Black;
                                path.Reset();

                                //It really shouldn't matter what FillMode is used for solution
                                //polygons because none of the solution polygons overlap. 
                                //However, FillMode.Winding will show any orientation errors where 
                                //holes will be stroked (outlined) correctly but filled incorrectly  ...
                                path.FillMode = FillMode.Winding;

                                //or for something fancy ...

                                if (Constants.NudOffset != 0)
                                {
                                    ClipperOffset co = new ClipperOffset();
                                    co.AddPaths(solution, JoinType.jtRound, EndType.etClosedPolygon);
                                    co.Execute(ref solution2, (double)Constants.NudOffset * scale);
                                }
                                else
                                    solution2 = new List<List<IntPoint>>(solution);

                                foreach (List<IntPoint> pg in solution2)
                                {
                                    PointF[] pts = Form1.PolygonToPointFArray(pg, scale);
                                    if (pts.Count() > 2)
                                        path.AddPolygon(pts);
                                    pts = null;
                                }
                                myBrush.Color = Color.FromArgb(127, 0x66, 0xEF, 0x7F);
                                myPen.Color = Color.FromArgb(255, 0, 0x33, 0);
                                myPen.Width = 1.0f;
                                newgraphic.FillPath(myBrush, path);
                                newgraphic.DrawPath(myPen, path);

                                //now do some fancy testing ...
                                using (Font f = new Font("Arial", 8))
                                using (SolidBrush b = new SolidBrush(Color.Navy))
                                {
                                    double subj_area = 0, clip_area = 0, int_area = 0, union_area = 0;
                                    c.Clear();
                                    c.AddPaths(subjects, PolyType.ptSubject, true);
                                    c.Execute(ClipType.ctUnion, solution2, form.GetPolyFillType(), form.GetPolyFillType());
                                    foreach (List<IntPoint> pg in solution2)
                                        subj_area += Clipper.Area(pg);
                                    c.Clear();
                                    c.AddPaths(clips, PolyType.ptClip, true);
                                    c.Execute(ClipType.ctUnion, solution2, form.GetPolyFillType(), form.GetPolyFillType());
                                    foreach (List<IntPoint> pg in solution2)
                                        clip_area += Clipper.Area(pg);
                                    c.AddPaths(subjects, PolyType.ptSubject, true);
                                    c.Execute(ClipType.ctIntersection, solution2, form.GetPolyFillType(), form.GetPolyFillType());
                                    foreach (List<IntPoint> pg in solution2)
                                        int_area += Clipper.Area(pg);
                                    c.Execute(ClipType.ctUnion, solution2, form.GetPolyFillType(), form.GetPolyFillType());
                                    foreach (List<IntPoint> pg in solution2)
                                        union_area += Clipper.Area(pg);

                                    using (StringFormat lftStringFormat = new StringFormat())
                                    using (StringFormat rtStringFormat = new StringFormat())
                                    {
                                        lftStringFormat.Alignment = StringAlignment.Near;
                                        lftStringFormat.LineAlignment = StringAlignment.Near;
                                        rtStringFormat.Alignment = StringAlignment.Far;
                                        rtStringFormat.LineAlignment = StringAlignment.Near;
                                        Rectangle rec = new Rectangle(pictureBox1.ClientSize.Width - 108,
                                                         pictureBox1.ClientSize.Height - 116, 104, 106);
                                        newgraphic.FillRectangle(new SolidBrush(Color.FromArgb(196, Color.WhiteSmoke)), rec);
                                        newgraphic.DrawRectangle(myPen, rec);
                                        rec.Inflate(new Size(-2, 0));
                                        newgraphic.DrawString("Areas", f, b, rec, rtStringFormat);
                                        rec.Offset(new Point(0, 14));
                                        newgraphic.DrawString("subj: ", f, b, rec, lftStringFormat);
                                        newgraphic.DrawString((subj_area / 100000).ToString("0,0"), f, b, rec, rtStringFormat);
                                        rec.Offset(new Point(0, 12));
                                        newgraphic.DrawString("clip: ", f, b, rec, lftStringFormat);
                                        newgraphic.DrawString((clip_area / 100000).ToString("0,0"), f, b, rec, rtStringFormat);
                                        rec.Offset(new Point(0, 12));
                                        newgraphic.DrawString("intersect: ", f, b, rec, lftStringFormat);
                                        newgraphic.DrawString((int_area / 100000).ToString("0,0"), f, b, rec, rtStringFormat);
                                        rec.Offset(new Point(0, 12));
                                        newgraphic.DrawString("---------", f, b, rec, rtStringFormat);
                                        rec.Offset(new Point(0, 10));
                                        newgraphic.DrawString("s + c - i: ", f, b, rec, lftStringFormat);
                                        newgraphic.DrawString(((subj_area + clip_area - int_area) / 100000).ToString("0,0"), f, b, rec, rtStringFormat);
                                        rec.Offset(new Point(0, 10));
                                        newgraphic.DrawString("---------", f, b, rec, rtStringFormat);
                                        rec.Offset(new Point(0, 10));
                                        newgraphic.DrawString("union: ", f, b, rec, lftStringFormat);
                                        newgraphic.DrawString((union_area / 100000).ToString("0,0"), f, b, rec, rtStringFormat);
                                        rec.Offset(new Point(0, 10));
                                        newgraphic.DrawString("---------", f, b, rec, rtStringFormat);
                                    }
                                }
                            } //end if succeeded
                        } //end if something to clip
                        pictureBox1.Image = mybitmap;
                    }
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
    }
}
