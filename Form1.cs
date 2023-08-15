using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Vision_Measurement
{
    enum EMeasurement
    {
        LENGTH,
        PARALLEL,
        PERPENDICULAR,
        RADIUS,
        DIAMETER,
        ARC
    }

    public partial class Form1 : Form
    {
        private EMeasurement Measurement => (EMeasurement)tscbxMode.ComboBox.SelectedItem;
        public const int measurementMaxCount = Int16.MaxValue;
        public const double distPerPixel = 3.45;
        private const int displayDecimalPlaces = 4;
        private const float crossScale = 5;
        readonly Pen arrowHarrowT = new Pen(Color.Red);
        readonly Pen arrowT = new Pen(Color.Red);
        readonly Font font = new Font("Comic Sans MS", 8);
        readonly Length len = new Length();
        readonly Radius rad = new Radius();
        readonly Diameter dia = new Diameter();
        readonly Parallel par = new Parallel();
        readonly Perpendicular per = new Perpendicular();
        readonly Arc arc = new Arc();
        public Form1()
        {
            InitializeComponent();
            InitializeControl();
            InitializePen();
            Text = "Vision Measurement";
            WindowState = FormWindowState.Maximized;

        }
        private void InitializePen()
        {
            AdjustableArrowCap arrow = new AdjustableArrowCap(3, 3);
            arrowHarrowT.CustomStartCap = arrow;
            arrowHarrowT.CustomEndCap = arrow;
            arrowT.CustomEndCap = arrow;
        }
        private void InitializeControl()
        {
            panel1.Size = new Size(1260, 620);
            pictureBox1.SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBox1.SendToBack();
            tscbxMode.ComboBox.DataSource = Enum.GetValues(typeof(EMeasurement));
            tscbxMode.ComboBox.Font = font;
            tscbxMode.ForeColor = Color.Purple;
            tscbxMode.DropDownStyle = ComboBoxStyle.DropDownList;
            tscbxMode.Width = 150;
        }

        private void PictureBox1MouseClick(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    switch (Measurement)
                    {
                        case EMeasurement.LENGTH:
                            par.sequence = 0;
                            per.sequence = 0;
                            rad.sequence = 0;
                            dia.sequence = 0;
                            arc.sequence = 0;
                            if (len.removeSequence < 1)
                            {
                                len.isRemoveLine = false;
                                switch (len.sequence)
                                {
                                    case 0:
                                        len.startCoord = e.Location;
                                        len.endCoord = Point.Empty;
                                        len.sequence++;
                                        break;

                                    case 1:
                                        len.endCoord = len.movingCoord;
                                        len.lengthCount++;
                                        len.lines.Add(len.startCoord);
                                        len.lines.Add(len.endCoord);
                                        len.sequence--;
                                        break;
                                }
                            }
                            break;
                        case EMeasurement.RADIUS:
                            len.sequence = 0;
                            par.sequence = 0;
                            per.sequence = 0;
                            dia.sequence = 0;
                            arc.sequence = 0;
                            if (rad.removeSequence < 1)
                            {
                                switch (rad.sequence)
                                {
                                    case 0:
                                        rad.startCoord = e.Location;
                                        rad.coord2 = Point.Empty;
                                        rad.coord3 = Point.Empty;
                                        rad.endCoord = Point.Empty;
                                        rad.sequence++;
                                        break;
                                    case 1:
                                        rad.coord2 = e.Location;
                                        rad.sequence++;
                                        break;
                                    case 2:
                                        if (rad.coord3 != Point.Empty)
                                        {
                                            rad.endCoord = rad.coord3;
                                            rad.radiusCount++;
                                            rad.circles.Add(rad.center);
                                            rad.circles.Add(rad.startCoord);
                                            rad.circles.Add(rad.coord2);
                                            rad.circles.Add(rad.endCoord);
                                            rad.sequence = 0;
                                        }
                                        break;
                                }
                            }
                            break;
                        case EMeasurement.DIAMETER:
                            len.sequence = 0;
                            par.sequence = 0;
                            per.sequence = 0;
                            rad.sequence = 0;
                            arc.sequence = 0;
                            if (dia.removeSequence < 1)
                            {
                                switch (dia.sequence)
                                {
                                    case 0:
                                        dia.startCoord = e.Location;
                                        dia.coord2 = Point.Empty;
                                        dia.coord3 = Point.Empty;
                                        dia.endCoord = Point.Empty;
                                        dia.sequence++;
                                        break;
                                    case 1:
                                        dia.coord2 = e.Location;
                                        dia.sequence++;
                                        break;
                                    case 2:
                                        if (dia.coord3 != Point.Empty)
                                        {
                                            dia.endCoord = dia.coord3;
                                            dia.radiusCount++;
                                            dia.circles.Add(dia.center);
                                            dia.circles.Add(dia.startCoord);
                                            dia.circles.Add(dia.coord2);
                                            dia.circles.Add(dia.endCoord);
                                            dia.sequence = 0;
                                        }
                                        break;
                                }
                            }
                            break;
                        case EMeasurement.PARALLEL:
                            len.sequence = 0;
                            per.sequence = 0;
                            rad.sequence = 0;
                            dia.sequence = 0;
                            arc.sequence = 0;
                            if (par.removeSequence < 1)
                            {
                                par.isRemoveLine = false;
                                switch (par.sequence)
                                {
                                    case 0:
                                        par.startCoord = e.Location;
                                        par.sequence++;
                                        break;
                                    case 1:
                                        par.endCoord = par.movingCoord;
                                        par.sequence++;
                                        break;
                                    case 2:
                                        par.offsetCoord = par.movingCoord2;
                                        par.lines.Add(par.epolateCoord3);
                                        par.lines.Add(par.epolateCoord4);
                                        par.lines.Add(par.offsetCoord);
                                        par.lines.Add(par.perpendicularCoord);
                                        par.lengthCount++;
                                        break;
                                }
                            }
                            break;
                        case EMeasurement.PERPENDICULAR:
                            len.sequence = 0;
                            par.sequence = 0;
                            rad.sequence = 0;
                            dia.sequence = 0;
                            arc.sequence = 0;
                            if (per.removeSequence < 1)
                            {
                                per.isRemoveLine = false;
                                switch (per.sequence)
                                {
                                    case 0:
                                        per.startCoord = e.Location;
                                        per.sequence++;
                                        break;
                                    case 1:
                                        per.endCoord = per.movingCoord;
                                        per.sequence++;
                                        break;
                                    case 2:
                                        per.offsetCoord = per.movingCoord2;
                                        per.lines.Add(per.offsetCoord);
                                        per.lines.Add(per.perpendicularCoord);
                                        per.lengthCount++;
                                        break;
                                }
                            }
                            break;
                        case EMeasurement.ARC:
                            len.sequence = 0;
                            par.sequence = 0;
                            per.sequence = 0;
                            rad.sequence = 0;
                            dia.sequence = 0;
                            if (arc.removeSequence < 1)
                            {
                                switch (arc.sequence)
                                {
                                    case 0:
                                        arc.startCoord = e.Location;
                                        arc.coord2 = Point.Empty;
                                        arc.coord3 = Point.Empty;
                                        arc.endCoord = Point.Empty;
                                        arc.sequence++;
                                        break;
                                    case 1:
                                        arc.coord2 = e.Location;
                                        arc.sequence++;
                                        break;
                                    case 2:
                                        if (arc.coord3 != Point.Empty)
                                        {
                                            arc.endCoord = arc.coord3;
                                            arc.radiusCount++;
                                            arc.circles.Add(arc.center);
                                            arc.circles.Add(arc.startCoord);
                                            arc.circles.Add(arc.coord2);
                                            arc.circles.Add(arc.endCoord);
                                            arc.sequence = 0;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                    break;
                case MouseButtons.Right:
                    switch (Measurement)
                    {
                        case EMeasurement.LENGTH:
                            par.removeSequence = 0;
                            per.sequence = 0;
                            rad.removeSequence = 0;
                            dia.removeSequence = 0;
                            arc.removeSequence = 0;
                            if (len.sequence < 1)
                            {
                                len.isRemoveLine = true;
                                switch (len.removeSequence)
                                {
                                    case 0:
                                        len.endCoord = Point.Empty;
                                        len.startCoord = e.Location;
                                        len.removeSequence++;
                                        break;

                                    case 1:
                                        len.endCoord = len.movingCoord;
                                        int j = 0;
                                        for (int i = 0; i < (len.lines.Count + j); i += 2)
                                        {
                                            bool isLineSegmentsIntersect = len.CheckIntercept(len.startCoord, len.endCoord, len.lines[i - j], len.lines[i - j + 1]);
                                            if (isLineSegmentsIntersect)
                                            {
                                                len.RemoveLine(i - j);
                                                j += 2;
                                            }
                                        }
                                        len.removeSequence--;
                                        break;
                                }
                            }
                            break;
                        case EMeasurement.RADIUS:
                            len.removeSequence = 0;
                            par.removeSequence = 0;
                            per.removeSequence = 0;
                            dia.removeSequence = 0;
                            arc.removeSequence = 0;
                            if (rad.sequence < 1)
                            {
                                rad.isRemoveCircle = true;
                                switch (rad.removeSequence)
                                {
                                    case 0:
                                        rad.endCoord = Point.Empty;
                                        rad.startCoord = e.Location;
                                        rad.removeSequence++;
                                        break;

                                    case 1:
                                        rad.endCoord = rad.coord3;
                                        int j = 0;
                                        for (int i = 0; i < (rad.circles.Count + j); i += 4)
                                        {
                                            bool isLineIntersectCircle = rad.CheckIntercept(rad.startCoord, rad.endCoord, rad.circles[i - j], rad.finalRadius[(i - j)/4]);
                                            if (isLineIntersectCircle)
                                            {
                                                rad.RemoveCircle(i - j);
                                                j += 4;
                                            }
                                        }
                                        rad.removeSequence--;
                                        break;
                                }
                            }
                            break;
                        case EMeasurement.DIAMETER:
                            len.removeSequence = 0;
                            par.removeSequence = 0;
                            per.removeSequence = 0;
                            rad.removeSequence = 0;
                            arc.removeSequence = 0;
                            if (dia.sequence < 1)
                            {
                                dia.isRemoveCircle = true;
                                switch (dia.removeSequence)
                                {
                                    case 0:
                                        dia.endCoord = Point.Empty;
                                        dia.startCoord = e.Location;
                                        dia.removeSequence++;
                                        break;

                                    case 1:
                                        dia.endCoord = dia.coord3;
                                        int j = 0;
                                        for (int i = 0; i < (dia.circles.Count + j); i += 4)
                                        {
                                            bool isLineIntersectCircle = dia.CheckIntercept(dia.startCoord, dia.endCoord, dia.circles[i - j], dia.finalRadius[(i - j) / 4]);
                                            if (isLineIntersectCircle)
                                            {
                                                dia.RemoveCircle(i - j);
                                                j += 4;
                                            }
                                        }
                                        dia.removeSequence--;
                                        break;
                                }
                            }
                            break;
                        case EMeasurement.PARALLEL:
                            len.removeSequence = 0;
                            per.removeSequence = 0;
                            rad.removeSequence = 0;
                            dia.removeSequence = 0;
                            arc.removeSequence = 0;
                            if (par.sequence == 2 || par.sequence == 0)
                            {
                                par.isRemoveLine = true;
                                switch (par.removeSequence)
                                {
                                    case 0:
                                        par.endCoord = Point.Empty;
                                        par.startCoord = e.Location;
                                        par.removeSequence++;
                                        break;

                                    case 1:
                                        par.endCoord = par.movingCoord;
                                        int j = 0;
                                        for (int i = 0; i < (par.lines.Count + j); i += 4)
                                        {
                                            bool isLineSegmentsIntersect = par.CheckIntercept(par.startCoord, par.endCoord, par.lines[i - j], par.lines[i - j + 1]);
                                            if (isLineSegmentsIntersect)
                                            {
                                                par.RemoveLine(i - j);
                                                j += 4;
                                            }
                                        }
                                        if (par.lines.Count == 0)
                                        {
                                            par.movingCoord2 = Point.Empty;
                                            par.epolateCoord1 = Point.Empty;
                                            par.epolateCoord2 = Point.Empty;
                                        }
                                        par.sequence = 0;
                                        par.newEndCoord = Point.Empty;
                                        par.removeSequence--;
                                        break;
                                }
                            }
                            break;
                        case EMeasurement.PERPENDICULAR:
                            len.removeSequence = 0;
                            par.removeSequence = 0;
                            rad.removeSequence = 0;
                            dia.removeSequence = 0;
                            arc.removeSequence = 0;
                            if (per.sequence == 2 || per.sequence == 0)
                            {
                                per.isRemoveLine = true;
                                switch (per.removeSequence)
                                {
                                    case 0:
                                        per.endCoord = Point.Empty;
                                        per.startCoord = e.Location;
                                        per.removeSequence++;
                                        break;

                                    case 1:
                                        per.endCoord = per.movingCoord;
                                        int j = 0;
                                        for (int i = 0; i < (per.lines.Count + j); i += 2)
                                        {
                                            bool isLineSegmentsIntersect = per.CheckIntercept(per.startCoord, per.endCoord, per.lines[i - j], per.lines[i - j + 1]);
                                            if (isLineSegmentsIntersect)
                                            {
                                                per.RemoveLine(i - j);
                                                j += 2;
                                            }
                                        }
                                        if (per.lines.Count == 0)
                                        {
                                            per.epolateCoord1 = Point.Empty;
                                            per.epolateCoord2 = Point.Empty;
                                        }
                                        per.sequence = 0;
                                        per.startCoord = Point.Empty;
                                        per.movingCoord2 = Point.Empty;
                                        per.removeSequence--;
                                        break;
                                }
                            }
                            break;
                        case EMeasurement.ARC:
                            len.removeSequence = 0;
                            par.removeSequence = 0;
                            per.removeSequence = 0;
                            rad.removeSequence = 0;
                            dia.removeSequence = 0;
                            if (arc.sequence < 1)
                            {
                                arc.isRemoveCircle = true;
                                switch (arc.removeSequence)
                                {
                                    case 0:
                                        arc.endCoord = Point.Empty;
                                        arc.startCoord = e.Location;
                                        arc.removeSequence++;
                                        break;

                                    case 1:
                                        arc.endCoord = arc.coord3;
                                        int j = 0;
                                        for (int i = 0; i < (arc.circles.Count + j); i += 4)
                                        {
                                            bool isLineIntersectCircle1 = arc.CheckIntercept(arc.startCoord, arc.endCoord, arc.circles[i - j], arc.circles[i - j + 3]);
                                            bool isLineIntersectCircle2 = arc.CheckIntercept(arc.startCoord, arc.endCoord, arc.circles[i - j], arc.circles[i - j + 1]);
                                            if (isLineIntersectCircle1 || isLineIntersectCircle2)
                                            {
                                                arc.RemoveCircle(i - j);
                                                j += 4;
                                            }
                                        }
                                        arc.removeSequence--;
                                        break;
                                }
                            }
                            break;
                    }
                    break;
            }
        }

        private void PictureBox1MouseMove(object sender, MouseEventArgs e)
        {
            switch (Measurement)
            {
                case EMeasurement.LENGTH:
                    if (len.isRemoveLine)
                    {
                        if (len.startCoord != Point.Empty && len.endCoord == Point.Empty)
                        {
                            len.movingCoord = e.Location;
                            pictureBox1.Invalidate();
                        }
                        if (len.endCoord != Point.Empty)
                        {
                            len.startCoord = Point.Empty;
                            len.endCoord = Point.Empty;
                            len.isRemoveLine = false;
                            pictureBox1.Invalidate();
                        }
                    }
                    else
                    {
                        if (len.startCoord != Point.Empty && len.endCoord == Point.Empty)
                        {
                            len.CheckAngle();
                            len.movingCoord = e.Location;
                            if (len.lineHorizontal)
                            {
                                len.movingCoord.Y = len.startCoord.Y;
                            }
                            else if (len.lineVertical)
                            {
                                len.movingCoord.X = len.startCoord.X;
                            }
                            len.length = len.GetDistance(len.startCoord, len.movingCoord);
                            pictureBox1.Invalidate();
                        }
                        if (len.endCoord != Point.Empty)
                        {
                            len.length = 0;
                            len.finalLength[len.lengthCount - 1] = len.GetDistance(len.startCoord, len.endCoord);
                            pictureBox1.Invalidate();
                        }
                    }
                    break;
                case EMeasurement.RADIUS:
                    if (rad.isRemoveCircle)
                    {
                        if (rad.startCoord != Point.Empty && rad.endCoord == Point.Empty)
                        {
                            rad.coord3 = e.Location;
                            pictureBox1.Invalidate();
                        }
                        if (rad.endCoord != Point.Empty)
                        {
                            rad.startCoord = Point.Empty;
                            rad.endCoord = Point.Empty;
                            rad.isRemoveCircle = false;
                            pictureBox1.Invalidate();
                        }
                    }
                    else
                    {
                        if (rad.startCoord != Point.Empty)
                        {
                            if (rad.coord2 != Point.Empty)
                            {
                                rad.distance = rad.GetDistance(rad.coord2, e.Location);
                                if (rad.distance > 0)
                                {
                                    rad.coord3 = e.Location;
                                    rad.CircleEquation(rad.startCoord, rad.coord2, rad.coord3, crossScale + 1);
                                    pictureBox1.Invalidate();
                                }
                                if (rad.endCoord != Point.Empty)
                                {
                                    rad.distance = 0;
                                    rad.CircleEquation(rad.startCoord, rad.coord2, rad.endCoord, crossScale + 1);
                                    rad.finalRadius[rad.radiusCount - 1] = rad.radius;
                                    pictureBox1.Invalidate();
                                }
                            }
                            else
                            {
                                pictureBox1.Invalidate();
                            }

                        }
                    }
                    break;
                case EMeasurement.DIAMETER:
                    if (dia.isRemoveCircle)
                    {
                        if (dia.startCoord != Point.Empty && dia.endCoord == Point.Empty)
                        {
                            dia.coord3 = e.Location;
                            pictureBox1.Invalidate();
                        }
                        if (dia.endCoord != Point.Empty)
                        {
                            dia.startCoord = Point.Empty;
                            dia.endCoord = Point.Empty;
                            dia.isRemoveCircle = false;
                            pictureBox1.Invalidate();
                        }
                    }
                    else
                    {
                        if (dia.startCoord != Point.Empty)
                        {
                            if (dia.coord2 != Point.Empty)
                            {
                                dia.distance = dia.GetDistance(dia.coord2, e.Location);
                                if (dia.distance > 0)
                                {
                                    dia.coord3 = e.Location;
                                    dia.CircleEquation(dia.startCoord, dia.coord2, dia.coord3, crossScale + 1);
                                    pictureBox1.Invalidate();
                                }
                                if (dia.endCoord != Point.Empty)
                                {
                                    dia.distance = 0;
                                    dia.CircleEquation(dia.startCoord, dia.coord2, dia.endCoord, crossScale + 1);
                                    dia.finalRadius[dia.radiusCount - 1] = dia.radius;
                                    pictureBox1.Invalidate();
                                }
                            }
                            else
                            {
                                pictureBox1.Invalidate();
                            }

                        }
                    }
                    break;
                case EMeasurement.PARALLEL:
                    if (par.isRemoveLine)
                    {
                        if (par.startCoord != Point.Empty && par.endCoord == Point.Empty)
                        {
                            par.movingCoord = e.Location;
                            pictureBox1.Invalidate();
                        }
                        if (par.endCoord != Point.Empty)
                        {
                            par.startCoord = Point.Empty;
                            par.endCoord = Point.Empty;
                            par.movingCoord = Point.Empty;
                            par.isRemoveLine = false;
                            pictureBox1.Invalidate();
                        }
                    }
                    else
                    {
                        if (par.startCoord != Point.Empty && par.endCoord == Point.Empty)
                        {
                            par.CheckAngle();
                            par.movingCoord = e.Location;
                            if (par.lineHorizontal)
                            {
                                par.movingCoord.Y = par.startCoord.Y;
                            }
                            else if (par.lineVertical)
                            {
                                par.movingCoord.X = par.startCoord.X;
                            }
                            pictureBox1.Invalidate();
                        }
                        if (par.endCoord != Point.Empty)
                        {
                            par.movingCoord2 = e.Location;
                            par.newEndCoord = par.CalcNewCoord(par.startCoord, par.endCoord, par.movingCoord2);
                            (par.epolateCoord1, par.epolateCoord2) = par.Extrapolation(par.startCoord, par.endCoord, pictureBox1.Size.Width, pictureBox1.Height);
                            (par.epolateCoord3, par.epolateCoord4) = par.Extrapolation(par.movingCoord2, par.newEndCoord, pictureBox1.Size.Width, pictureBox1.Height);
                            (par.perpendicularCoord, par.length) = par.CalcPerpendicularDistance(par.epolateCoord1, par.epolateCoord2, par.movingCoord2);
                            pictureBox1.Invalidate();
                        }
                        if (par.offsetCoord != Point.Empty)
                        {
                            par.finalLength[par.lengthCount - 1] = par.length;
                            par.offsetCoord = Point.Empty;
                            pictureBox1.Invalidate();
                        }
                    }
                    break;
                case EMeasurement.PERPENDICULAR:
                    if (per.isRemoveLine)
                    {
                        if (per.startCoord != Point.Empty && per.endCoord == Point.Empty)
                        {
                            per.movingCoord = e.Location;
                            pictureBox1.Invalidate();
                        }
                        if (per.endCoord != Point.Empty)
                        {
                            per.startCoord = Point.Empty;
                            per.endCoord = Point.Empty;
                            per.movingCoord = Point.Empty;
                            per.isRemoveLine = false;
                            pictureBox1.Invalidate();
                        }
                    }
                    else
                    {
                        if (per.startCoord != Point.Empty && per.endCoord == Point.Empty)
                        {
                            per.CheckAngle();
                            per.movingCoord = e.Location;
                            if (per.lineHorizontal)
                            {
                                per.movingCoord.Y = per.startCoord.Y;
                            }
                            else if (per.lineVertical)
                            {
                                per.movingCoord.X = per.startCoord.X;
                            }
                            pictureBox1.Invalidate();
                        }
                        if (per.endCoord != Point.Empty)
                        {
                            per.movingCoord2 = e.Location;
                            (per.epolateCoord1, per.epolateCoord2) = per.Extrapolation(per.startCoord, per.endCoord, pictureBox1.Size.Width, pictureBox1.Height);
                            (per.perpendicularCoord, per.length) = per.CalcPerpendicularDistance(per.epolateCoord1, per.epolateCoord2, per.movingCoord2);
                            pictureBox1.Invalidate();
                        }
                        if (per.offsetCoord != Point.Empty)
                        {
                            per.finalLength[per.lengthCount - 1] = per.length;
                            per.offsetCoord = Point.Empty;
                        }
                    }
                    break;
                case EMeasurement.ARC:
                    if (arc.isRemoveCircle)
                    {
                        if (arc.startCoord != Point.Empty && arc.endCoord == Point.Empty)
                        {
                            arc.coord3 = e.Location;
                            pictureBox1.Invalidate();
                        }
                        if (arc.endCoord != Point.Empty)
                        {
                            arc.startCoord = Point.Empty;
                            arc.endCoord = Point.Empty;
                            arc.isRemoveCircle = false;
                            pictureBox1.Invalidate();
                        }
                    }
                    else
                    {
                        if (arc.startCoord != Point.Empty)
                        {
                            if (arc.coord2 != Point.Empty)
                            {
                                arc.distance = arc.GetDistance(arc.coord2, e.Location);
                                if (arc.distance > 0)
                                {
                                    arc.coord3 = e.Location;
                                    arc.CircleEquation(arc.startCoord, arc.coord2, arc.coord3, crossScale + 1);
                                    pictureBox1.Invalidate();
                                }
                                if (arc.endCoord != Point.Empty)
                                {
                                    arc.distance = 0;
                                    arc.CircleEquation(arc.startCoord, arc.coord2, arc.endCoord, crossScale + 1);
                                    arc.finalRadius[arc.radiusCount - 1] = arc.radius;
                                    arc.finalAngle[(arc.radiusCount - 1) * 2] = arc.startAngle;
                                    arc.finalAngle[2 * arc.radiusCount - 1] = arc.sweepAngle;
                                    pictureBox1.Invalidate();
                                }
                            }
                            else
                            {
                                pictureBox1.Invalidate();
                            }

                        }
                    }
                    break;
            }
        }

        private void LoadImage(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog();
            if (file.ShowDialog() == DialogResult.OK)
            {
                string filepath = file.FileName;
                Image image = Image.FromFile(filepath);
                pictureBox1.Size = image.Size;
                pictureBox1.Image = image;
            }
        }

        private void PictureBox1Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Pen dashedPen = new Pen(Color.Green)
            {
                DashPattern = new float[] { 4F, 2F, 1F, 3F }
            };
            Pen dashedPen2 = new Pen(Color.Red)
            {
                DashPattern = new float[] { 4F, 2F, 1F, 3F }
            };
            Graphics g = e.Graphics;
            SolidBrush sb = new SolidBrush(Color.Red);
            //Length
            for (int i = 0; i < len.lines.Count; i += 2)
            {
                string length = Math.Round(len.finalLength[i / 2], displayDecimalPlaces).ToString() + "µm";
                Point label_position = new Point { X = len.lines[i].X + (len.lines[i + 1].X - len.lines[i].X) / 2, 
                                                   Y = len.lines[i].Y + (len.lines[i + 1].Y - len.lines[i].Y) / 2 };
                DrawCross(ref g, len.lines[i]);
                DrawCross(ref g, len.lines[i + 1]);
                g.DrawLine(arrowHarrowT, len.lines[i], len.lines[i + 1]);
                g.DrawString(length, font, sb, label_position);
            }

            //Radius
            for (int i = 0; i < rad.circles.Count; i += 4)
            {
                float leftCornerX = rad.circles[i].X - (float)rad.finalRadius[i / 4];
                float leftCornerY = rad.circles[i].Y - (float)rad.finalRadius[i / 4];
                float axisLength = (float)(2 * rad.finalRadius[i / 4]);
                string radius = Math.Round((rad.finalRadius[i / 4]*distPerPixel), displayDecimalPlaces).ToString() + "µm";
                Point label_position = new Point { X = rad.circles[i].X + (rad.circles[i + 3].X - rad.circles[i].X) / 2,
                                                   Y = rad.circles[i].Y + (rad.circles[i + 3].Y - rad.circles[i].Y) / 2};
                DrawCross(ref g, rad.circles[i]);
                g.DrawLine(arrowT, rad.circles[i], rad.circles[i + 3]);
                g.DrawEllipse(Pens.Red, leftCornerX, leftCornerY, axisLength, axisLength);
                g.DrawString(radius, new Font("Comic Sans MS", 8), sb, label_position);
            }

            //Diameter
            for (int i = 0; i < dia.circles.Count; i += 4)
            {
                float leftCornerX = dia.circles[i].X - (float)dia.finalRadius[i / 4];
                float leftCornerY = dia.circles[i].Y - (float)dia.finalRadius[i / 4];
                float axisLength = (float)(2 * dia.finalRadius[i / 4]);
                string radius = Math.Round((dia.finalRadius[i / 4] * distPerPixel), displayDecimalPlaces).ToString() + "µm";
                Point label_position = new Point
                {
                    X = dia.circles[i].X + (dia.circles[i + 3].X - dia.circles[i].X) / 2,
                    Y = dia.circles[i].Y + (dia.circles[i + 3].Y - dia.circles[i].Y) / 2
                };
                Point extendedPoint = dia.ExtendLine(dia.circles[i + 3], dia.circles[i]);
                DrawCross(ref g, dia.circles[i]);
                g.DrawLine(arrowT, dia.circles[i + 3], extendedPoint);
                g.DrawEllipse(Pens.Red, leftCornerX, leftCornerY, axisLength, axisLength);
                g.DrawString(radius, new Font("Comic Sans MS", 8), sb, label_position);
            }

            //Parallel
            if(par.lines.Count > 0)
            {
                g.DrawLine(Pens.Yellow, par.epolateCoord1, par.epolateCoord2);
            }
            for(int i = 0; i < par.lines.Count; i += 4)
            {
                string perpendicularDistance = Math.Round(par.finalLength[i / 4] * distPerPixel, displayDecimalPlaces).ToString() + "µm";
                Point label_position = new Point
                {
                    X = par.lines[i + 2].X + (par.lines[i + 3].X - par.lines[i + 2].X) / 2,
                    Y = par.lines[i + 2].Y + (par.lines[i + 3].Y - par.lines[i + 2].Y) / 2
                };
                g.DrawLine(arrowHarrowT, par.lines[i + 2], par.lines[i + 3]);
                g.DrawString(perpendicularDistance, new Font("Comic Sans MS", 8), sb, label_position);
                g.DrawLine(dashedPen2, par.lines[i], par.lines[i + 1]);
            }

            //Perpendicular
            if (per.lines.Count > 0)
            {
                g.DrawLine(Pens.Yellow, per.epolateCoord1, per.epolateCoord2);
            }
            for (int i = 0; i < per.lines.Count; i += 2)
            {
                string perpendicularDistance = Math.Round(per.finalLength[i / 2] * distPerPixel, displayDecimalPlaces).ToString() + "µm";
                Point label_position = new Point
                {
                    X = per.lines[i].X + (per.lines[i + 1].X - per.lines[i].X) / 2,
                    Y = per.lines[i].Y + (per.lines[i + 1].Y - per.lines[i].Y) / 2
                };
                DrawCross(ref g, per.lines[i]);
                g.DrawLine(arrowHarrowT, per.lines[i], per.lines[i + 1]);
                g.DrawString(perpendicularDistance, new Font("Comic Sans MS", 8), sb, label_position);
            }

            // Arc
            for (int i = 0; i < arc.circles.Count; i += 4)
            {
                float leftCornerX = arc.circles[i].X - (float)arc.finalRadius[i / 4];
                float leftCornerY = arc.circles[i].Y - (float)arc.finalRadius[i / 4];
                float axisLength = (float)(2 * arc.finalRadius[i / 4]);
                string radius = Math.Round((arc.finalRadius[i / 4] * distPerPixel), displayDecimalPlaces).ToString() + "µm";
                string sweepAngle = Math.Round(Math.Abs(arc.finalAngle[(i / 2) + 1]), 2).ToString() + "°";
                Point label_position = new Point
                {
                    X = arc.circles[i].X + (arc.circles[i + 3].X - arc.circles[i].X) / 2,
                    Y = arc.circles[i].Y + (arc.circles[i + 3].Y - arc.circles[i].Y) / 2
                };
                DrawCross(ref g, arc.circles[i]);
                g.DrawArc(Pens.Red, leftCornerX, leftCornerY, axisLength, axisLength, (float)arc.finalAngle[i / 2], (float)arc.finalAngle[(i / 2) + 1]);
                g.DrawLine(dashedPen2, arc.circles[i], arc.circles[i + 1]);
                g.DrawLine(dashedPen2, arc.circles[i], arc.circles[i + 3]);
                g.DrawString(radius, new Font("Comic Sans MS", 8), sb, label_position);
                g.DrawString(sweepAngle, new Font("Comic Sans MS", 8), sb, arc.circles[i]);
            }
            switch (Measurement)
            {
                case EMeasurement.LENGTH:
                    if (len.isRemoveLine)
                    {
                        if (len.endCoord == Point.Empty)
                        {
                            g.DrawLine(dashedPen, len.startCoord, len.movingCoord);
                        }
                    }
                    else
                    {
                        if (len.length != 0)
                        {
                            if (len.endCoord == Point.Empty)
                            {
                                string length = Math.Round(len.length, displayDecimalPlaces).ToString() + "µm";
                                DrawCross(ref g, len.startCoord);
                                DrawCross(ref g, len.movingCoord);
                                Point label_position = new Point { X = len.startCoord.X + (len.movingCoord.X - len.startCoord.X) / 2, 
                                                                   Y = len.startCoord.Y + (len.movingCoord.Y - len.startCoord.Y) / 2 };
                                g.DrawLine(arrowHarrowT, len.startCoord, len.movingCoord);
                                g.DrawString(length, new Font("Comic Sans MS", 8), sb, label_position);
                            }
                        }
                    }
                    break;
                case EMeasurement.RADIUS:
                    if (rad.isRemoveCircle)
                    {
                        if (rad.endCoord == Point.Empty)
                        {
                            g.DrawLine(dashedPen, rad.startCoord, rad.coord3);
                        }
                    }
                    else
                    {
                        if (rad.startCoord != Point.Empty && rad.coord2 == Point.Empty)
                        {
                            DrawCross(ref g, rad.startCoord);
                        }
                        if (rad.distance != 0)
                        {
                            if (rad.endCoord == Point.Empty)
                            {
                                string radius = Math.Round((rad.radius * distPerPixel), displayDecimalPlaces).ToString() + "µm";
                                float leftCornerX = rad.center.X - (float)rad.radius;
                                float leftCornerY = rad.center.Y - (float)rad.radius;
                                float axisLength = (float)(rad.radius + rad.radius);
                                Point label_position = new Point
                                {
                                    X = rad.center.X + (rad.coord3.X - rad.center.X) / 2,
                                    Y = rad.center.Y + (rad.coord3.Y - rad.center.Y) / 2
                                };
                                DrawCross(ref g, rad.startCoord);
                                DrawCross(ref g, rad.coord2);
                                DrawCross(ref g, rad.coord3);
                                DrawCross(ref g, rad.center);
                                g.DrawLine(arrowT, rad.center, rad.coord3);
                                g.DrawEllipse(Pens.Red, leftCornerX, leftCornerY, axisLength, axisLength);
                                g.DrawString(radius, new Font("Comic Sans MS", 8), sb, label_position);
                            }
                        }
                    }
                    break;
                case EMeasurement.DIAMETER:
                    if (dia.isRemoveCircle)
                    {
                        if (dia.endCoord == Point.Empty)
                        {
                            g.DrawLine(dashedPen, dia.startCoord, dia.coord3);
                        }
                    }
                    else
                    {
                        if (dia.startCoord != Point.Empty && dia.coord2 == Point.Empty)
                        {
                            DrawCross(ref g, dia.startCoord);
                        }
                        if (dia.distance != 0)
                        {
                            if (dia.endCoord == Point.Empty)
                            {
                                string radius = Math.Round(dia.radius * distPerPixel, displayDecimalPlaces).ToString() + "µm";
                                float leftCornerX = dia.center.X - dia.radius;
                                float leftCornerY = dia.center.Y - dia.radius;
                                float axisLength = (float)(dia.radius + dia.radius);
                                Point label_position = new Point
                                {
                                    X = dia.center.X + (dia.coord3.X - dia.center.X) / 2,
                                    Y = dia.center.Y + (dia.coord3.Y - dia.center.Y) / 2
                                };
                                Point extendedPoint = dia.ExtendLine(dia.coord3, dia.center);
                                DrawCross(ref g, dia.startCoord);
                                DrawCross(ref g, dia.coord2);
                                DrawCross(ref g, dia.coord3);
                                DrawCross(ref g, dia.center);
                                g.DrawLine(arrowT, dia.coord3, extendedPoint);
                                g.DrawEllipse(Pens.Red, leftCornerX, leftCornerY, axisLength, axisLength);
                                g.DrawString(radius, new Font("Comic Sans MS", 8), sb, label_position);
                            }
                        }
                    }
                    break;
                case EMeasurement.PARALLEL:
                    if (par.isRemoveLine)
                    {
                        if (par.endCoord == Point.Empty)
                        {
                            g.DrawLine(dashedPen, par.startCoord, par.movingCoord);
                        }
                    }
                    else
                    {
                        if (par.movingCoord != Point.Empty && par.movingCoord2 == Point.Empty)
                        {
                            DrawCross(ref g, par.startCoord);
                            g.DrawLine(Pens.Red, par.startCoord, par.movingCoord);
                        }
                        if (par.newEndCoord != Point.Empty)
                        {
                            string perpendicularDistance = Math.Round(par.length * distPerPixel, displayDecimalPlaces).ToString() + "µm";
                            Point label_position = new Point
                            {
                                X = par.movingCoord2.X + (par.perpendicularCoord.X - par.movingCoord2.X) / 2,
                                Y = par.movingCoord2.Y + (par.perpendicularCoord.Y - par.movingCoord2.Y) / 2
                            };
                            DrawCross(ref g, par.startCoord);
                            DrawCross(ref g, par.endCoord);
                            DrawCross(ref g, par.movingCoord2);
                            DrawCross(ref g, par.newEndCoord);
                            g.DrawLine(Pens.Red, par.startCoord, par.endCoord);
                            g.DrawLine(Pens.Red, par.movingCoord2, par.newEndCoord);
                            g.DrawLine(arrowHarrowT, par.movingCoord2, par.perpendicularCoord);
                            g.DrawString(perpendicularDistance, new Font("Comic Sans MS", 8), sb, label_position);
                            g.DrawLine(dashedPen2, par.epolateCoord1, par.epolateCoord2);
                            g.DrawLine(dashedPen2, par.epolateCoord3, par.epolateCoord4);
                        }
                    }
                    break;
                case EMeasurement.PERPENDICULAR:
                    if (per.isRemoveLine)
                    {
                        if (per.endCoord == Point.Empty)
                        {
                            g.DrawLine(dashedPen, per.startCoord, per.movingCoord);
                        }
                    }
                    else
                    {
                        if (per.movingCoord != Point.Empty && per.movingCoord2 == Point.Empty)
                        {
                            DrawCross(ref g, per.startCoord);
                            g.DrawLine(Pens.Red, per.startCoord, per.movingCoord);
                        }
                        if (per.movingCoord2 != Point.Empty)
                        {
                            string perpendicularDistance = Math.Round(per.length * distPerPixel, displayDecimalPlaces).ToString() + "µm";
                            Point label_position = new Point
                            {
                                X = per.movingCoord2.X + (per.perpendicularCoord.X - per.movingCoord2.X) / 2,
                                Y = per.movingCoord2.Y + (per.perpendicularCoord.Y - per.movingCoord2.Y) / 2
                            };
                            DrawCross(ref g, per.startCoord);
                            DrawCross(ref g, per.endCoord);
                            DrawCross(ref g, per.movingCoord2);
                            g.DrawLine(Pens.Red, per.startCoord, per.endCoord);
                            g.DrawLine(arrowHarrowT, per.movingCoord2, per.perpendicularCoord);
                            g.DrawString(perpendicularDistance, new Font("Comic Sans MS", 8), sb, label_position);
                            g.DrawLine(dashedPen2, per.epolateCoord1, per.epolateCoord2);
                        }
                    }
                    break;
                case EMeasurement.ARC:
                    if (arc.isRemoveCircle)
                    {
                        if (arc.endCoord == Point.Empty)
                        {
                            g.DrawLine(dashedPen, arc.startCoord, arc.coord3);
                        }
                    }
                    else
                    {
                        if (arc.startCoord != Point.Empty && arc.coord2 == Point.Empty)
                        {
                            DrawCross(ref g, arc.startCoord);
                        }
                        if (arc.distance != 0)
                        {
                            if (arc.endCoord == Point.Empty)
                            {
                                string radius = Math.Round(arc.radius * distPerPixel, displayDecimalPlaces).ToString() + "µm";
                                string sweepAngle = Math.Round(Math.Abs(arc.sweepAngle), 2).ToString() + "°";
                                float leftCornerX = arc.center.X - arc.radius;
                                float leftCornerY = arc.center.Y - arc.radius;
                                float axisLength = (float)(arc.radius + arc.radius);
                                Point label_position = new Point
                                {
                                    X = arc.center.X + (arc.coord3.X - arc.center.X) / 2,
                                    Y = arc.center.Y + (arc.coord3.Y - arc.center.Y) / 2
                                };
                                DrawCross(ref g, arc.startCoord);
                                DrawCross(ref g, arc.coord2);
                                DrawCross(ref g, arc.coord3);
                                DrawCross(ref g, arc.center);
                                g.DrawArc(Pens.Red, leftCornerX, leftCornerY, axisLength, axisLength, (float)arc.startAngle, (float)arc.sweepAngle);
                                g.DrawLine(dashedPen2, arc.center, arc.startCoord);
                                g.DrawLine(dashedPen2, arc.center, arc.coord3);
                                g.DrawString(radius, new Font("Comic Sans MS", 8), sb, label_position);
                                g.DrawString(sweepAngle, new Font("Comic Sans MS", 8), sb, arc.center);
                            }
                        }
                    }
                    break;
            }
        }

        public void DrawCross(ref Graphics g, Point point)
        {
            g.DrawLine(new Pen(Color.Red), point.X - crossScale, point.Y - crossScale, point.X + crossScale, point.Y + crossScale);
            g.DrawLine(new Pen(Color.Red), point.X - crossScale, point.Y + crossScale, point.X + crossScale, point.Y - crossScale);
        }
    }

    public class Length
    {
        private const double thresholdAngleX = 5;
        private const double thresholdAngleY = 80;
        public bool isRemoveLine = false;
        public bool lineVertical = new bool();
        public bool lineHorizontal = new bool();
        public int lengthCount = 0;
        public int sequence = 0;
        public int removeSequence = 0;
        private int removeCount = 0;
        public List<Point> lines = new List<Point>();
        public Point startCoord;
        public Point movingCoord;
        public Point endCoord;
        public double length;
        public double[] finalLength = new double[Form1.measurementMaxCount];
        public double m = new double();
        public double c = new double();

        public virtual void RemoveLine(int index)
        {
            lines.RemoveAt(index);
            lines.RemoveAt(index);
            lengthCount--;
            removeCount++;
            for (int i = index; i < finalLength.Length - 1; i++)
            {
                finalLength[i] = finalLength[i + 1];
            };
            Array.Resize(ref finalLength, Form1.measurementMaxCount - removeCount);
        }
        public virtual double GetDistance(Point start, Point end)
        {
            double x_diff = end.X - start.X;
            double y_diff = end.Y - start.Y;
            x_diff *= Form1.distPerPixel;
            y_diff *= Form1.distPerPixel;
            return Math.Sqrt(Math.Pow(x_diff, 2) + Math.Pow(y_diff, 2));
        }
        public double GetAngle(Point start, Point end)
        {
            double x = end.X - start.X;
            double y = end.Y - start.Y;
            return Math.Abs(Math.Atan2(y, x) * (180 / Math.PI));
        }

        public void CheckAngle()
        {
            if (Control.ModifierKeys == Keys.Shift)
            {
                double angle = GetAngle(startCoord, movingCoord);
                if (angle < thresholdAngleX || angle > (180 - thresholdAngleX))
                {
                    lineVertical = false;
                    lineHorizontal = true;
                }
                else if (angle > thresholdAngleY && angle < (180 - thresholdAngleY))
                {
                    lineVertical = true;
                    lineHorizontal = false;
                }
                else
                {
                    lineVertical = false;
                    lineHorizontal = false;
                }
            }
            else
            {
                lineVertical = false;
                lineHorizontal = false;
            }
        }
        public bool CheckIntercept(Point startA, Point endA, Point startB, Point endB)
        {
            int orientation1 = Orientation(startA, endA, startB);
            int orientation2 = Orientation(startA, endA, endB);
            int orientation3 = Orientation(startB, endB, startA);
            int orientation4 = Orientation(startB, endB, endA);

            if (orientation1 != orientation2 && orientation3 != orientation4)
                return true;
            if (orientation1 == 0 && OnSegment(startA, startB, endA)) return true;
            if (orientation2 == 0 && OnSegment(startA, endB, endA)) return true;
            if (orientation3 == 0 && OnSegment(startB, startA, endB)) return true;
            if (orientation4 == 0 && OnSegment(startB, endA, endB)) return true;

            return false;
        }

        private bool OnSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.X) && q.Y >= Math.Min(p.Y, r.X))
            {
                return true;
            }
            return false;
        }

        private int Orientation(Point p, Point q, Point r)
        {
            int val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            if (val == 0) return 0;
            return (val > 0) ? 1 : 2;
        }
    }

    public class Parallel : Length
    {
        public Point movingCoord2;
        public Point offsetCoord;
        public Point newEndCoord;
        public Point epolateCoord1, epolateCoord2, epolateCoord3, epolateCoord4;
        public Point perpendicularCoord;
        private int removeCount;

        public override double GetDistance(Point start, Point end)
        {
            double x_diff = end.X - start.X;
            double y_diff = end.Y - start.Y;
            return Math.Sqrt(Math.Pow(x_diff, 2) + Math.Pow(y_diff, 2));
        }

        public Point CalcNewCoord(Point start, Point end, Point offsetCoord)
        {
            int dx = start.X - end.X;
            int dy = start.Y - end.Y;
            return new Point(offsetCoord.X + dx, offsetCoord.Y + dy);
        }

        public (Point, double) CalcPerpendicularDistance(Point start, Point end, Point offsetCoord)
        {
            double l = GetDistance(start, end);
            double d1 = GetDistance(start, offsetCoord);
            double d2 = GetDistance(end, offsetCoord);
            double theta = Math.Acos((Math.Pow(d1, 2) + Math.Pow(l, 2) - Math.Pow(d2, 2)) / (2 * d1 * l));
            double d = d1 * Math.Sin(theta);
            double s = d1 * Math.Cos(theta);
            return (new Point { X = (int)((double)start.X + (s / l) * (end.X - start.X)), Y = (int)((double)start.Y + (s / l) * (end.Y - start.Y)) }, d);
        }

        public (Point, Point) Extrapolation(Point start, Point end, int xMax, int yMax)
        {
            Point newStart, newEnd;
            start.Y = Math.Abs(start.Y - yMax);
            end.Y = Math.Abs(end.Y - yMax);
            double m = (double)(end.Y - start.Y) / (double)(end.X - start.X);
            double c = end.Y - m * end.X;
            if (c < 0)
            {
                newStart = new Point { X = (int)(-c / m), Y = 0 };
            }
            else if (c > yMax)
            {
                newStart = new Point { X = (int)((yMax - c) / m), Y = yMax };
            }
            else
            {
                newStart = new Point { X = 0, Y = (int)c };
            }
            double d = m * xMax + c;
            if (d < 0)
            {
                newEnd = new Point { X = (int)(-c / m), Y = 0 };
            }
            else if (d > yMax)
            {
                newEnd = new Point { X = (int)((yMax - c) / m), Y = yMax };
            }
            else
            {
                newEnd = new Point { X = xMax, Y = (int)d };
            }
            newStart.Y = Math.Abs(newStart.Y - yMax);
            newEnd.Y = Math.Abs(newEnd.Y - yMax);
            return (newStart, newEnd);
        }

        public override void RemoveLine(int index)
        {
            lines.RemoveAt(index);
            lines.RemoveAt(index);
            lines.RemoveAt(index);
            lines.RemoveAt(index);
            lengthCount--;
            removeCount++;
            for (int i = index/4; i < finalLength.Length - 1; i++)
            {
                finalLength[i] = finalLength[i + 1];
            };
            Array.Resize(ref finalLength, Form1.measurementMaxCount - removeCount);
        }
    }

    public class Perpendicular : Parallel
    {
        private int removeCount = 0;
        public override void RemoveLine(int index)
        {
            lines.RemoveAt(index);
            lines.RemoveAt(index);
            lengthCount--;
            removeCount++;
            for (int i = index/2; i < finalLength.Length - 1; i++)
            {
                finalLength[i] = finalLength[i + 1];
            };
            Array.Resize(ref finalLength, Form1.measurementMaxCount - removeCount);
        }
    }

    public class Radius
    {
        public bool isRemoveCircle = false;
        public int removeSequence = 0;
        public int sequence = 0;
        public int radiusCount = 0;
        private int removeCount = 0;
        public float radius = new float();
        public double distance = new double();
        public Point startCoord;
        public Point coord2;
        public Point coord3;
        public Point endCoord;
        public Point center;
        public List<Point> circles = new List<Point>();
        public double[] finalRadius = new double[Form1.measurementMaxCount];

        public virtual void RemoveCircle(int index)
        {
            circles.RemoveAt(index);
            circles.RemoveAt(index);
            circles.RemoveAt(index);
            circles.RemoveAt(index);
            radiusCount--;
            removeCount++;
            for (int i = index; i < finalRadius.Length - 1; i++)
            {
                finalRadius[i] = finalRadius[i + 1];
            };
            Array.Resize(ref finalRadius, Form1.measurementMaxCount - removeCount);
        }

        public virtual void CircleEquation(Point coord1, Point coord2, Point coord3, float threshold)
        {
            double x1 = coord1.X;
            double y1 = coord1.Y;
            double x2 = coord2.X;
            double y2 = coord2.Y;
            double x3 = coord3.X;
            double y3 = coord3.Y;

            double x12 = x1 - x2;
            double x13 = x1 - x3;

            double y12 = y1 - y2;
            double y13 = y1 - y3;

            double s1 = Math.Pow(x1, 2) + Math.Pow(y1, 2);
            double s2 = Math.Pow(x2, 2) + Math.Pow(y2, 2);
            double s3 = Math.Pow(x3, 2) + Math.Pow(y3, 2);

            double f = ((x12) * (s3 - s1) - (x13) * (s2 - s1)) / (2 * ((x12 * y13) - (y12 * x13)));
            double g = (s3 - s1 - 2 * f * y13) / (2 * x13);

            double c = -s1 - 2 * g * x1 - 2 * f * y1;
            double h = -g;
            double k = -f;
            if ((int)h > Int32.MaxValue - threshold || (int)h < Int32.MinValue + threshold)
            {
                return;
            }
            if ((int)k > Int32.MaxValue - threshold || (int)k < Int32.MinValue + threshold)
            {
                return;
            }
            double sr = h * h + k * k - c;
            radius = (float)Math.Sqrt(sr);
            center = new Point { X = (int)h, Y = (int)k };
        }

        public double GetDistance(Point start, Point end)
        {
            double x_diff = end.X - start.X;
            double y_diff = end.Y - start.Y;
            return Math.Sqrt(Math.Pow(x_diff, 2) + Math.Pow(y_diff, 2));
        }

        public virtual bool CheckIntercept(Point lineStart, Point lineEnd, Point center, double radius)
        {
            double l = GetDistance(lineStart, lineEnd);
            double d1 = GetDistance(lineStart, center);
            double d2 = GetDistance(lineEnd, center);
            double theta = Math.Acos((Math.Pow(d1, 2) + Math.Pow(l, 2) - Math.Pow(d2, 2)) / (2 * d1 * l));
            double d = d1 * Math.Sin(theta);
            if (d1 < radius || d2 < radius)
            {
                return false;
            }
            if (d > radius)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    public class Diameter : Radius
    {
        public Point ExtendLine(Point endPoint, Point center)
        {
            int dx = (center.X - endPoint.X) * 2;
            int dy = (center.Y - endPoint.Y) * 2;
            return new Point(endPoint.X + dx, endPoint.Y + dy);
        }
    }

    public class Arc : Radius
    {
        public double startAngle = new double();
        public double sweepAngle = new double();
        public int angleCount = 0;
        public double[] finalAngle = new double[Form1.measurementMaxCount];
        private int removeCount;

        public override void CircleEquation(Point coord1, Point coord2, Point coord3, float threshold)
        {
            double x1 = coord1.X;
            double y1 = coord1.Y;
            double x2 = coord2.X;
            double y2 = coord2.Y;
            double x3 = coord3.X;
            double y3 = coord3.Y;

            double x12 = x1 - x2;
            double x13 = x1 - x3;

            double y12 = y1 - y2;
            double y13 = y1 - y3;

            double s1 = Math.Pow(x1, 2) + Math.Pow(y1, 2);
            double s2 = Math.Pow(x2, 2) + Math.Pow(y2, 2);
            double s3 = Math.Pow(x3, 2) + Math.Pow(y3, 2);

            double f = ((x12) * (s3 - s1) - (x13) * (s2 - s1)) / (2 * ((x12 * y13) - (y12 * x13)));
            double g = (s3 - s1 - 2 * f * y13) / (2 * x13);

            double c = -s1 - 2 * g * x1 - 2 * f * y1;
            double h = -g;
            double k = -f;
            if ((int)h > Int32.MaxValue - threshold || (int)h < Int32.MinValue + threshold)
            {
                return;
            }
            if ((int)k > Int32.MaxValue - threshold || (int)k < Int32.MinValue + threshold)
            {
                return;
            }
            double sr = h * h + k * k - c;
            double d = GetDistance(coord1, coord3);
            radius = (float)Math.Sqrt(sr);
            center = new Point { X = (int)h, Y = (int)k };
            startAngle = Math.Atan2((y1 - center.Y), (x1 - center.X)) * 180 / Math.PI;
            double stopAngle = Math.Atan2((y3 - center.Y), (x3 - center.X)) * 180 / Math.PI;
            sweepAngle = stopAngle - startAngle;
        }

        public override void RemoveCircle(int index)
        {
            circles.RemoveAt(index);
            circles.RemoveAt(index);
            circles.RemoveAt(index);
            circles.RemoveAt(index);
            radiusCount--;
            removeCount++;
            for (int i = index; i < finalRadius.Length - 1; i++)
            {
                finalRadius[i] = finalRadius[i + 1];
            };
            Array.Resize(ref finalRadius, Form1.measurementMaxCount - removeCount);
            for (int i = index; i < finalAngle.Length - 3; i++)
            {
                finalAngle[i] = finalAngle[i + 2];
                finalAngle[i + 1] = finalAngle[i + 3];
            };
            Array.Resize(ref finalAngle, Form1.measurementMaxCount - (removeCount * 2));
        }

        public bool CheckIntercept(Point startA, Point endA, Point startB, Point endB)
        {
            int orientation1 = Orientation(startA, endA, startB);
            int orientation2 = Orientation(startA, endA, endB);
            int orientation3 = Orientation(startB, endB, startA);
            int orientation4 = Orientation(startB, endB, endA);

            if (orientation1 != orientation2 && orientation3 != orientation4)
                return true;
            if (orientation1 == 0 && OnSegment(startA, startB, endA)) return true;
            if (orientation2 == 0 && OnSegment(startA, endB, endA)) return true;
            if (orientation3 == 0 && OnSegment(startB, startA, endB)) return true;
            if (orientation4 == 0 && OnSegment(startB, endA, endB)) return true;

            return false;
        }

        private bool OnSegment(Point p, Point q, Point r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.X) && q.Y >= Math.Min(p.Y, r.X))
            {
                return true;
            }
            return false;
        }

        private int Orientation(Point p, Point q, Point r)
        {
            int val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            if (val == 0) return 0;
            return (val > 0) ? 1 : 2;
        }
    }
}
