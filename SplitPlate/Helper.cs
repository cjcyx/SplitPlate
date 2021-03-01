using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Aveva.Core.PMLNet;

[assembly: PMLNetCallable()]

namespace SplitPlate
{
    [PMLNetCallable()]
    public class RunSplitPlate
    {
        SplitPlateClass mw;
        [PMLNetCallable()]
        public event PMLNetDelegate.PMLNetEventHandler PmlNetEvent;
        [PMLNetCallable()]
        public void RaisePmlNetEvent(ArrayList args)
        {
            PmlNetEvent?.Invoke(args);
        }
        [PMLNetCallable()]
        public void Start()
        {
            mw = new SplitPlateClass();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(mw);
            mw.eventInvok += new SplitPlateClass.eventDelegate(RaisePmlNetEvent);
            //mw.checkPipeEvent += new SplitPlateClass.CheckPipeEvent(RaiseCheckPipeEvent);
            //mw.runISOEvent += new SplitPlateClass.RunISOEvent(RaiseRunISOEvent);
            //mw.runPipeISOEvent += new SplitPlateClass.RunPipeISOEvent(RaisePipeISOEvent);
            mw.Show();
        }
        [PMLNetCallable()]
        public RunSplitPlate()
        {

        }
        [PMLNetCallable()]
        public void Assign(RunSplitPlate that)
        {
            //No state
        }
        [PMLNetCallable()]
        public void GetPave(string name)
        {
            mw.PaneRef.Text = name;
        }
        [PMLNetCallable()]
        public void GetPipe(string E, string N, string x,string y)
        {
            mw.ePOS.Tag = Math.Round(Convert.ToDouble(E.Replace("mm", "")), 3);
            mw.nPOS.Tag = Math.Round(Convert.ToDouble(N.Replace("mm", "")), 3);
            if (mw.splitOnly.IsChecked == false)
            {
                double xlen = Convert.ToDouble(x.Replace("mm", ""));
                double ylen = Convert.ToDouble(y.Replace("mm", ""));
                if (xlen != 0)
                {
                    mw.xLen.Text = xlen.ToString("F3");
                    mw.yLen.Text = ylen.ToString("F3");
                }
                mw.ReAddLine();
                //mw.OnTop();
            }
        }
        [PMLNetCallable()]
        public void ChooseClose()
        {
            mw.OnTop();
        }
        [PMLNetCallable()]
        public void Run(Hashtable orangePAVE)
        {
            List<Pave> orangePave = new List<Pave>();
            for (double i = 1; i <= orangePAVE.Values.Count; i++)
            {
                string[] tmp = ((string)orangePAVE[i]).Replace("mm", "").Split(',');
                orangePave.Add(new Pave(Convert.ToDouble(tmp[0]), Convert.ToDouble(tmp[1]), Convert.ToDouble(tmp[2])));
            }
            double r = Convert.ToDouble(mw.xLen.Text) / 2 + Convert.ToDouble(mw.dist1.Text);
            double distFx = Convert.ToDouble(mw.xLen.Text) + 2 * Convert.ToDouble(mw.dist1.Text);
            double distFy = Convert.ToDouble(mw.yLen.Text) + 2 * Convert.ToDouble(mw.dist1.Text);
            bool multiSp = false;
            double splitDist = 0;
            double splitCount = 0;
            Pave center = new Pave(Convert.ToDouble(mw.ePOS.Text) + (double)mw.ePOS.Tag, Convert.ToDouble(mw.nPOS.Text) + (double)mw.nPOS.Tag, -r);
            if (mw.splitOnly.IsChecked == true && Convert.ToDouble(mw.yLen.Text) > 1)
            {
                splitDist = Convert.ToDouble(mw.xLen.Text);
                splitCount = Convert.ToDouble(mw.yLen.Text);
                multiSp = true;
            }
            double ang = (Convert.ToDouble(mw.ang.Text) % 180) * Math.PI / 180;
            ArrayList returnCommands = new ArrayList();
            Funcs.ReturnCommands(ref returnCommands,mw.fang.IsChecked, mw.yuan.IsChecked, orangePave, center, Convert.ToDouble(mw.dist.Text), r, distFx, distFy, ang, mw.PaneRef.Text, multiSp, splitDist, splitCount);
            //MessageBox.Show(Splited.Count.ToString() + " " + orangePave.Count.ToString());
            mw.RunCommand(returnCommands);
        }
    }
    public static class GetLines
    {
        public static Line line1;
        public static Line line2;
        public static Line GetLine(bool flag)
        {
            if (flag)
            {
                Line linetmp = line1;
                line1 = line2;
                line2 = linetmp;
                return line1;
            }
            else
            {
                return line1;
            }
        }
        public static void SetLine(bool flag, Line linea, Line lineb)
        {
            if (flag)
            {
                line1 = lineb;
                line2 = linea;
            }
            else
            {
                line1 = linea;
                line2 = lineb;
            }
        }
    }
    public static class GetPanes
    {
        public static List<Pave> pane1;
        public static List<Pave> pane2;
        public static List<Pave> GetPane(bool flag)
        {
            if (flag)
            {
                List<Pave> panetmp = pane1;
                pane1 = pane2;
                pane2 = panetmp;
                return pane1;
            }
            else
            {
                return pane1;
            }
        }
        public static void SetPane(List<Pave> panea, List<Pave> paneb)
        {
            pane1 = paneb;
            pane2 = panea;
        }
    }
    public class Line
    {
        Pave _point1;
        Pave _point2;
        Pave _point3;
        bool _type;
        Pave _center;
        public Pave point1
        {
            get
            {
                return this._point1;
            }
            set
            {
                this._point1 = value;
            }
        }
        public Pave point2
        {
            get
            {
                return this._point2;
            }
            set
            {
                this._point2 = value;
            }
        }
        public Pave point3
        {
            get
            {
                return this._point3;
            }
            set
            {
                this._point3 = value;
            }
        }
        public Type type
        {
            get
            {
                if (this._type)
                {
                    return Type.Circle;
                }
                else
                {
                    return Type.Line;
                }
            }
        }
        public Pave center
        {
            get
            {
                return this._center;
            }
        }
        public enum Type
        {
            Circle,
            Line
        }
        public Pave CrossLine(Line line2, ref bool Crossed)
        {
            Pave crossPoint = new Pave(0, 0, 0);
            double a1 = this.point1.pointy - this.point2.pointy;
            double b1 = this.point2.pointx - this.point1.pointx;
            double c1 = this.point1.pointx * this.point2.pointy - this.point2.pointx * this.point1.pointy;
            double a2 = line2.point1.pointy - line2.point2.pointy;
            double b2 = line2.point2.pointx - line2.point1.pointx;
            double c2 = line2.point1.pointx * line2.point2.pointy - line2.point2.pointx * line2.point1.pointy;
            double x = (b1 * c2 - b2 * c1) / (b2 * a1 - a2 * b1);
            double y = (a1 * c2 - a2 * c1) / (a2 * b1 - b2 * a1);
            //if(x)
            crossPoint.pointx = x;
            crossPoint.pointy = y;
            return crossPoint;
        }
        public Pave Cross(Line line2, ref bool Crossed)
        {
            Pave crossPoint = new Pave(0, 0, 0);
            //if (this.point2.radius == 0)
            {
                double a1 = this.point1.pointy - this.point2.pointy;
                double b1 = this.point2.pointx - this.point1.pointx;
                double c1 = this.point1.pointx * this.point2.pointy - this.point2.pointx * this.point1.pointy;
                double a2 = line2.point1.pointy - line2.point2.pointy;
                double b2 = line2.point2.pointx - line2.point1.pointx;
                double c2 = line2.point1.pointx * line2.point2.pointy - line2.point2.pointx * line2.point1.pointy;
                double x = (b1 * c2 - b2 * c1) / (b2 * a1 - a2 * b1);
                double y = (a1 * c2 - a2 * c1) / (a2 * b1 - b2 * a1);
                //Debug.Print(x + " " + y+this.ToString ()+" " + Math.Min(this.point1.pointy, this.point2.pointy) + " "+ Math.Max(this.point1.pointy, this.point2.pointy));
                if ((x > Math.Min(this.point1.pointx, this.point2.pointx) && x < Math.Max(this.point1.pointx, this.point2.pointx)) || (y > Math.Min(this.point1.pointy, this.point2.pointy) && y < Math.Max(this.point1.pointy, this.point2.pointy)))
                {
                    crossPoint.pointx = x;
                    crossPoint.pointy = y;
                    Crossed = true;
                }
            }
            /*
            else if (this.point2.radius < 0)
            {
                if (this.point2.Dist(this.point3) <= -this.point2.radius)
                {

                }
            }
            */
            return crossPoint;
        }
        public override string ToString()
        {
            return this.point1.ToString() + "-" + this.point2.ToString();
        }
        public Line(Pave point1, Pave point2)
        {
            this.point1 = point1;
            this.point2 = point2;
            if (point2.radius != 0)
            {
                this._type = true;
                if (point1.Dist(point2) > point2.radius)
                {

                }
                else
                {

                }
            }
            else
            {
                this._type = false;
            }
        }
        public Line(double x1, double y1, double ang)
        {
            this.point1 = new Pave(x1, y1, 0);
            double x2 = x1 + 100 * Math.Cos(ang);
            double y2 = y1 + 100 * Math.Sin(ang);
            this.point2 = new Pave(x2, y2, 0);
        }
    }
    public class Pave
    {
        double _pointx;
        double _pointy;
        double _radius;
        public double pointx
        {
            get
            {
                return _pointx;
            }
            set
            {
                this._pointx = value;
            }
        }
        public double pointy
        {
            get
            {
                return _pointy;
            }
            set
            {
                this._pointy = value;
            }
        }
        public double radius
        {
            get
            {
                return _radius;
            }
            set
            {
                this._radius = value;
            }
        }
        public Pave(double pointx, double pointy, double radius)
        {
            this.pointx = pointx;
            this.pointy = pointy;
            this.radius = radius;
        }
        public double Dist(Pave point2)
        {
            double dist = Math.Sqrt(Math.Pow((this.pointx - point2.pointx), 2) + Math.Pow((this.pointy - point2.pointy), 2));
            return dist;
        }
        public override string ToString()
        {
            return "(" + this.pointx.ToString() + "," + this.pointy.ToString() + ")," + this.radius;
        }
    }
    public static class Funcs
    {
        /// <summary>
        /// No Use Now
        /// </summary>
        /// <param name="cross1">交点1</param>
        /// <param name="cross2">交点2</param>
        /// <param name="center">圆心</param>
        /// <param name="R">半径</param>
        /// <returns></returns>
        public static Pave radPoint(Pave cross1, Pave cross2, Pave center, double R)
        {
            Pave D = new Pave((cross1.pointx + cross2.pointx) / 2, (cross1.pointy + cross2.pointy) / 2, 0);
            double OD = D.Dist(center);
            double OC = Math.Pow(R, 2) / OD;
            double x1 = center.pointx + OC / OD * (D.pointx - center.pointx);
            double y1 = center.pointy + OC / OD * (D.pointy - center.pointy);
            return new Pave(x1, y1, R);
        }
        /// <summary>
        /// 给定直线与圆心，返回交点
        /// </summary>
        /// <param name="line">直线</param>
        /// <param name="center">圆心</param>
        /// <param name="R">半径</param>
        /// <returns></returns>
        public static List<Pave> CrossPoints(Line line, Pave center, double R)
        {
            double A = line.point1.pointy - line.point2.pointy;
            double B = line.point2.pointx - line.point1.pointx;
            double C = line.point1.pointx * line.point2.pointy - line.point2.pointx * line.point1.pointy;
            double a = center.pointx;
            double b = center.pointy;
            List<Pave> crossPoints = new List<Pave>();
            if (B == 0)
            {
                double x1 = -(C / A);
                double x2 = -(C / A);
                double y1 = b + Math.Sqrt(Math.Pow(R, 2) - Math.Pow((C / A), 2) - 2 * a * C / A - Math.Pow(a, 2));
                double y2 = b - Math.Sqrt(Math.Pow(R, 2) - Math.Pow((C / A), 2) - 2 * a * C / A - Math.Pow(a, 2));
                crossPoints.Add(new Pave(x1, y1, 0));
                crossPoints.Add(new Pave(x2, y2, 0));
            }
            else
            {
                double m = (-A / B);
                double n = (-C / B);
                double p = n - b;
                double r = 1 + Math.Pow(m, 2);
                double s = 2 * m * p - 2 * a;
                double t = Math.Pow(a, 2) + Math.Pow(p, 2) - Math.Pow(R, 2);
                double x1 = (-s + Math.Sqrt(Math.Pow(s, 2) - 4 * r * t)) / (2 * r);
                double x2 = (-s - Math.Sqrt(Math.Pow(s, 2) - 4 * r * t)) / (2 * r);
                double y1 = m * x1 + n;
                double y2 = m * x2 + n;
                crossPoints.Add(new Pave(x1, y1, 0));
                crossPoints.Add(new Pave(x2, y2, 0));
            }
            return crossPoints;
        }
        /// <summary>
        /// 按顺序返回圆的点集信息
        /// </summary>
        /// <param name="crossPoint">交点</param>
        /// <param name="line">分割线</param>
        /// <param name="center">圆心</param>
        /// <param name="R">半径</param>
        /// <returns></returns>
        public static List<Pave> CircleReturn(Pave crossPoint, Line line, Pave center, double R)
        {
            var crossPoints = CrossPoints(line, center, R);
            List<Pave> returnPoints = new List<Pave>();
            if (crossPoint.Dist(crossPoints[0]) > crossPoint.Dist(crossPoints[1]))
            {
                returnPoints.Add(crossPoints[1]);
                returnPoints.Add(center);
                returnPoints.Add(crossPoints[0]);
            }
            else
            {
                returnPoints.Add(crossPoints[0]);
                returnPoints.Add(center);
                returnPoints.Add(crossPoints[1]);
            }
            return returnPoints;

        }
        /// <summary>
        /// 按顺序返回方孔信息
        /// </summary>
        /// <param name="line">分割线</param>
        /// <param name="center">圆心</param>
        /// <param name="cross">交点</param>
        /// <param name="distxF">沿直线方向长度</param>
        /// <param name="distyF">垂直直线方向长度</param>
        /// <param name="dist">分割距离</param>
        /// <param name="ang">角度</param>
        /// <returns></returns>
        public static List<Pave> SquareReturn(Line line, Pave center, Pave cross, double distxF, double distyF, double dist, double ang)
        {
            List<Pave> returnPoints = new List<Pave>();
            double xF = line.point1.pointx;
            double yF = line.point1.pointy;
            double deltXF = (distxF / 2) * Math.Cos(ang);
            double deltYF = (distxF / 2) * Math.Sin(ang);
            double x1F = xF + deltXF;
            double y1F = yF + deltYF;
            double x2F = xF - deltXF;
            double y2F = yF - deltYF;
            Pave paveaF = new Pave(x1F, y1F, 0);
            Pave pavebF = new Pave(x2F, y2F, 0);
            Pave paveF1;            
            if (cross.Dist(line.point1) <= distxF / 2)
            {
                paveF1 = cross;
                distxF = distxF /2 + cross.Dist(line.point1);
            }
            else
            {
                //MessageBox.Show(cross.Dist(line.point1).ToString() + " " + (distxF / 2).ToString());
                if (paveaF.Dist(cross) < pavebF.Dist(cross))
                {
                    paveF1 = paveaF;
                }
                else
                {
                    paveF1 = pavebF;
                }
            }
            xF = paveF1.pointx;
            yF = paveF1.pointy;
            deltXF = (distyF - dist) / 2 * Math.Sin(ang);
            deltYF = -(distyF - dist) / 2 * Math.Cos(ang);
            x1F = xF + deltXF;
            y1F = yF + deltYF;
            x2F = xF - deltXF;
            y2F = yF - deltYF;
            paveaF = new Pave(x1F, y1F, 0);
            pavebF = new Pave(x2F, y2F, 0);
            Pave paveF2;
            if (paveaF.Dist(center) > pavebF.Dist(center))
            {
                paveF2 = paveaF;
            }
            else
            {
                paveF2 = pavebF;
            }
            xF = paveF2.pointx;
            yF = paveF2.pointy;
            deltXF = (distxF) * Math.Cos(ang);
            deltYF = (distxF) * Math.Sin(ang);
            x1F = xF + deltXF;
            y1F = yF + deltYF;
            x2F = xF - deltXF;
            y2F = yF - deltYF;
            paveaF = new Pave(x1F, y1F, 0);
            pavebF = new Pave(x2F, y2F, 0);
            Pave paveF3;
            //if (paveF1 == cross)
            //{
            //    if (paveaF.Dist(center) > pavebF.Dist(center))
            //    {
            //        paveF3 = paveaF;
            //    }
            //    else
            //    {
            //        paveF3 = pavebF;
            //    }
            //}
            //else
            {
                if (paveaF.Dist(center) < pavebF.Dist(center))
                {
                    paveF3 = paveaF;
                }
                else
                {
                    paveF3 = pavebF;
                }
            }
            xF = paveF3.pointx;
            yF = paveF3.pointy;
            deltXF = ((distyF - dist) / 2) * Math.Sin(ang);
            deltYF = -((distyF - dist) / 2) * Math.Cos(ang);
            x1F = xF + deltXF;
            y1F = yF + deltYF;
            x2F = xF - deltXF;
            y2F = yF - deltYF;
            paveaF = new Pave(x1F, y1F, 0);
            pavebF = new Pave(x2F, y2F, 0);
            Pave paveF4;
            if (paveaF.Dist(center) < pavebF.Dist(center))
            {
                paveF4 = paveaF;
            }
            else
            {
                paveF4 = pavebF;
            }
            returnPoints.Add(paveF1);
            returnPoints.Add(paveF2);
            returnPoints.Add(paveF3);
            returnPoints.Add(paveF4);
            return returnPoints;
        }


        public static void ReturnCommands(ref ArrayList returnCommands , bool? fang, bool? yuan, List<Pave> originPane, Pave center, double dist, double R, double distxF, double distyF, double ang, string paveRef,bool multiSplit,double splitDist,double splitCount)
        {     
            double x = center.pointx;
            double y = center.pointy;
            //Line line = new Line(x, y, ang);
            double deltX = (dist / 2) * Math.Sin(ang);
            double deltY = -(dist / 2) * Math.Cos(ang);
            double x1 = x + deltX;
            double y1 = y + deltY;
            double x2 = x - deltX;
            double y2 = y - deltY;
            Pave pavea = new Pave(x1, y1, 0);
            Pave paveb = new Pave(x2, y2, 0);
            Line linea = new Line(x1, y1, ang);
            Line lineb = new Line(x2, y2, ang);
            GetLines.SetLine((pavea.Dist(originPane.First()) > paveb.Dist(originPane.First())), linea, lineb);
            bool cross = false;
            GetPanes.SetPane(new List<Pave>(), new List<Pave>());
            Line line = GetLines.GetLine(false);
            List<Pave> pane = GetPanes.GetPane(false);
            for (int i = 0; i < originPane.Count; i++)
            {
                int ii = i + 1;
                if (ii == originPane.Count)
                {
                    ii = 0;
                }
                Line line1 = new Line(originPane[i], originPane[ii]);
                cross = false;
                var point = line1.Cross(line, ref cross);
                if (cross)
                {
                    pane.Add(point);
                    if (yuan == true)
                    {
                        pane.AddRange(CircleReturn(point, line, center, R));
                    }
                    else if (fang == true)
                    {
                        var returnData = SquareReturn(line, center, point, distxF, distyF, dist, ang);
                        if (returnData[0] == point)
                        {
                            pane.RemoveAt(pane.Count - 1);
                            returnData.RemoveAt(0);
                        }
                        pane.AddRange(returnData);
                    }
                    pane = GetPanes.GetPane(true);
                    line = GetLines.GetLine(true);
                    for (int j = i; j < originPane.Count; j++)
                    {
                        int jj = j + 1;
                        if (jj == originPane.Count)
                        {
                            jj = 0;
                        }
                        cross = false;
                        line1 = new Line(originPane[j], originPane[jj]);
                        point = line1.Cross(line, ref cross);
                        if (cross)
                        {
                            pane.Add(point);
                            i = j;
                            ii = jj;
                            break;
                        }
                    }
                }
                pane.Add(originPane[ii]);
            }
            if (multiSplit)
            {
                if(splitCount >2)
                {
                    double splitDistAbs = Math.Abs(splitDist);
                    double xm = center.pointx;
                    double ym = center.pointy;
                    double xm1;
                    double ym1;
                    double deltXm = (dist  + splitDistAbs) * Math.Sin(ang);
                    double deltYm = -(dist  + splitDistAbs) * Math.Cos(ang);
                    if (splitDist > 0)
                    {
                        xm1 = xm + deltXm;
                        ym1 = ym + deltYm;
                    }
                    else
                    {
                        xm1 = xm - deltXm;
                        ym1 = ym - deltYm;
                    }
                    Pave newCenter = new Pave(xm1, ym1, center.radius);
                    #region 点在平面内 错误
                    Line linem = new Line(xm1, ym1, (ang + 45) / 180 * Math.PI);
                    bool inPlate = true;
                    List<Pave> pave = GetPanes.GetPane(false);
                    for (int i = 0; i < pave.Count; i++)
                    {
                        int j = i + 1;
                        if (j == pave.Count)
                        {
                            j = 0;
                        }
                        Line newLine = new Line(pave[i], pave[j]);
                        bool crossed = false;
                        var point = newLine.Cross(linem, ref crossed);
                        if (crossed && point.pointx > xm1)
                        {
                            inPlate = !inPlate;
                        }
                    }
                    List<Pave> orangePavem = new List<Pave>();
                    orangePavem.AddRange(GetPanes.GetPane(inPlate));
                    returnCommands.Add("!!SplitObject.CopyPANE('" + paveRef + "')");
                    foreach (var item in GetPanes.GetPane(true))
                    {
                        returnCommands.Add("new pave pos E " + item.pointx + " N " + item.pointy + " Frad " + item.radius);
                    }
                    #endregion 
                    splitCount--;
                    if (splitCount == 2) 
                    {
                        multiSplit = false;
                    }
                    ReturnCommands(ref returnCommands, fang, yuan, orangePavem, newCenter, dist, R, distxF, distyF, ang, paveRef, multiSplit, splitDist, splitCount);
                    //ReturnCommands(ref returnCommands, fang, yuan, GetPanes.GetPane(true), newCenter, dist, R, distxF, distyF, ang, paveRef, multiSplit, splitDist, splitCount);
                }
            }
            else
            {
                if (GetPanes.GetPane(false).Count != 0 && GetPanes.GetPane(true).Count != 0)
                {
                    returnCommands.Add("!!SplitObject.CopyPANE('" + paveRef + "')");
                    foreach (var item in GetPanes.GetPane(false))
                    {
                        returnCommands.Add("new pave pos E " + item.pointx + " N " + item.pointy + " Frad " + item.radius);
                    }
                    returnCommands.Add("!!SplitObject.CopyPANE('" + paveRef + "')");
                    foreach (var item in GetPanes.GetPane(true))
                    {
                        returnCommands.Add("new pave pos E " + item.pointx + " N " + item.pointy + " Frad " + item.radius);
                    }
                }
            }
            returnCommands.Add("PANE");
        }
    }
}
