using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;

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
        public const int measurementMaxCount = short.MaxValue;
        public const int displayDecimalPlaces = 3;
        private readonly double distPerPixel = 3.45;
        private const float crossScale = 5;
        private const float smallArcScale = 10;
        private float scale = 1.0F;
        private float last_scale = 0.0F;
        private bool isGrayScale = false;
        private static readonly Color mainColor = Color.Cyan;
        private static readonly Color subColor = Color.Lime;
        private Image rawImage;
        readonly Pen regular = new Pen(mainColor);
        readonly Pen arrowHarrowT = new Pen(mainColor);
        readonly Pen arrowT = new Pen(mainColor);
        readonly Pen dashedarrowH = new Pen(mainColor)
        {
            DashPattern = new float[] { 4F, 2F, 1F, 3F }
        };
        readonly Font font = new Font("Comic Sans MS", 8);
        readonly Length len = new Length();
        readonly Parallel par = new Parallel();
        readonly Perpendicular per = new Perpendicular();
        readonly Radius rad = new Radius();
        readonly Diameter dia = new Diameter();
        readonly Arc arc = new Arc();
        readonly Dimensioning dim = new Dimensioning();

        public Form1()
        {
            InitializeComponent();
            InitializeControl();
            InitializePen();
            Text = "Vision Measurement";
            WindowState = FormWindowState.Maximized;
        }

        public Form1(Image<Bgr, byte> image, double ddp): this()
        {
            InitializeComponent();
            InitializeControl();
            InitializePen();
            Text = "Vision Measurement";
            WindowState = FormWindowState.Maximized;
            rawImage = image.ToBitmap();
            distPerPixel = ddp;
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

        private void InitializePen()
        {
            AdjustableArrowCap arrow = new AdjustableArrowCap(3, 3);
            arrowHarrowT.CustomStartCap = arrow;
            arrowHarrowT.CustomEndCap = arrow;
            arrowT.CustomEndCap = arrow;
            dashedarrowH.CustomStartCap = arrow;
        }

        private void LoadImage(object sender, EventArgs e)
        {
            pictureBox1.Enabled = false;
            OpenFileDialog file = new OpenFileDialog();
            if (file.ShowDialog() == DialogResult.OK)
            {
                string filepath = file.FileName;
                rawImage = Image.FromFile(filepath);
                pictureBox1.Size = rawImage.Size;
                pictureBox1.Image = rawImage;
            }
            pictureBox1.Enabled = true;
        }

        private void SaveImage(object sender, EventArgs e)
        {
            Bitmap image = new Bitmap(pictureBox1.ClientSize.Width, pictureBox1.ClientSize.Height);
            pictureBox1.DrawToBitmap(image, pictureBox1.ClientRectangle);
            pictureBox1.Enabled = false;
            SaveFileDialog file = new SaveFileDialog()
            {
                Title = "Save Image",
                DefaultExt = "png",
                Filter = "PNG Image|*.png|Bitmap Image|*.bmp|JPEG Image|*.jpeg",
                FilterIndex = 1,
                RestoreDirectory = true
            };
            if (file.ShowDialog() == DialogResult.OK)
            {
                System.IO.FileStream fs = (System.IO.FileStream)file.OpenFile();
                if (file.FileName != "")
                {
                    switch (file.FilterIndex)
                    {
                        case 1:
                            image.Save(fs, ImageFormat.Png);
                            break;
                        case 2:
                            image.Save(fs, ImageFormat.Bmp);
                            break;
                        case 3:
                            image.Save(fs, ImageFormat.Jpeg);
                            break;

                    }
                }
            }
            pictureBox1.Enabled = true;
        }

        private Bitmap MakeGrayscale(Bitmap original)
        {
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                ColorMatrix colorMatrix = new ColorMatrix(new float[][] { new float[] {.3f, .3f, .3f, 0, 0},
                                                                           new float[] {.59f, .59f, .59f, 0, 0},
                                                                           new float[] {.11f, .11f, .11f, 0, 0},
                                                                           new float[] {0, 0, 0, 1, 0},
                                                                           new float[] {0, 0, 0, 0, 1 } });
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    attributes.SetColorMatrix(colorMatrix);
                    g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height), 0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        private void ToggleColor(object sender, EventArgs e)
        {
            if (rawImage == null)
            {
                return;
            }
            isGrayScale = !isGrayScale;
            Bitmap bmp = ResizeImage(rawImage, (int)(rawImage.Width * scale), (int)(rawImage.Height * scale));
            if (isGrayScale)
            {
                Bitmap gray_bmp = MakeGrayscale(bmp);
                toolStripButton2.Text = "BGR";
                pictureBox1.Image = gray_bmp;
            }
            else
            {
                toolStripButton2.Text = "GrayScale";
                pictureBox1.Image = bmp;
            }
        }

        private void ZoomIn(object sender, EventArgs e)
        {
            scale += 0.1F;
            if (scale >= 2.0F)
            {
                scale = 2.0F;
            }
            scaleText.Text = ((int)Math.Round(scale * 100)).ToString();
            Bitmap bmp = ResizeImage(rawImage, (int)(rawImage.Width * scale), (int)(rawImage.Height * scale));
            pictureBox1.Image = bmp;
            if (last_scale != scale)
            {
                len.RescaleAll(scale);
                par.RescaleAll(scale);
                per.RescaleAll(scale);
                rad.RescaleAll(scale);
                dia.RescaleAll(scale);
                arc.RescaleAll(scale);
            }
            last_scale = scale;
            pictureBox1.Invalidate();
        }

        private void ZoomOut(object sender, EventArgs e)
        {
            scale -= 0.1F;
            if (scale <= 0.1F)
            {
                scale = 0.1F;
            }
            scaleText.Text = ((int)Math.Round(scale * 100)).ToString();
            Bitmap bmp = ResizeImage(rawImage, (int)(rawImage.Width * scale), (int)(rawImage.Height * scale));
            pictureBox1.Image = bmp;
            if (last_scale != scale)
            {
                len.RescaleAll(scale);
                par.RescaleAll(scale);
                per.RescaleAll(scale);
                rad.RescaleAll(scale);
                dia.RescaleAll(scale);
                arc.RescaleAll(scale);
            }
            last_scale = scale;
            pictureBox1.Invalidate();
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics g = Graphics.FromImage(destImage))
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    g.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
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
                                        len.offsetCoord = PointF.Empty;
                                        len.endCoord = PointF.Empty;
                                        len.sequence++;
                                        break;

                                    case 1:
                                        len.endCoord = len.movingCoord;
                                        if (len.startCoord.X > len.endCoord.X)
                                        {
                                            (len.startCoord, len.endCoord) = (len.endCoord, len.startCoord);
                                        }
                                        len.sequence++;
                                        break;
                                    case 2:
                                        len.offsetCoord = len.movingCoord2;
                                        len.lengthCount++;
                                        len.finalLength[len.lengthCount - 1] = len.length;
                                        len.RevertToOriginalSize(scale);
                                        len.rawLines.Add(len.startCoord);
                                        len.rawLines.Add(len.endCoord);
                                        len.rawLines.Add(len.offsetCoord);
                                        len.rawLines.Add(len.newEndCoord);
                                        len.rawLines.Add(len.extendedCoord);
                                        len.sequence = 0;
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
                            if (len.removeSequence < 1)
                            {
                                len.isRemoveLine = false;
                                switch (par.sequence)
                                {
                                    case 0:
                                        par.startCoord = e.Location;
                                        par.movingCoord2 = PointF.Empty;
                                        par.epolateCoord1 = PointF.Empty;
                                        par.epolateCoord2 = PointF.Empty;
                                        par.sequence++;
                                        break;
                                    case 1:
                                        par.endCoord = par.movingCoord;
                                        par.sequence++;
                                        break;
                                    case 2:
                                        par.offsetCoord = par.movingCoord2;
                                        par.sequence++;
                                        break;
                                    case 3:
                                        par.extendedCoord = par.movingCoord2;
                                        par.lengthCount++;
                                        par.finalLength[par.lengthCount - 1] = par.length;
                                        par.RevertToOriginalSize(scale);
                                        par.rawLines.Add(par.epolateCoord1);
                                        par.rawLines.Add(par.epolateCoord2);
                                        par.rawLines.Add(par.epolateCoord3);
                                        par.rawLines.Add(par.epolateCoord4);
                                        par.rawLines.Add(par.perpendicularCoord);
                                        par.rawLines.Add(par.perpendicularCoord2);
                                        par.rawLines.Add(par.extendedCoord);
                                        par.offsetCoord = PointF.Empty;
                                        par.extendedCoord = PointF.Empty;
                                        par.sequence = 2;
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
                            if (len.removeSequence < 1)
                            {
                                len.isRemoveLine = false;
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
                                        per.sequence++;
                                        break;
                                    case 3:
                                        per.extendedCoord = per.movingCoord2;
                                        per.lengthCount++;
                                        per.finalLength[per.lengthCount - 1] = per.length;
                                        per.RevertToOriginalSize(scale);
                                        per.rawLines.Add(per.epolateCoord1);
                                        per.rawLines.Add(per.epolateCoord2);
                                        per.rawLines.Add(per.offsetCoord);
                                        per.rawLines.Add(per.perpendicularCoord);
                                        per.rawLines.Add(per.perpendicularCoord2);
                                        per.rawLines.Add(per.extendedCoord);
                                        per.rawLines.Add(per.endCoord);
                                        per.offsetCoord = PointF.Empty;
                                        per.extendedCoord = PointF.Empty;
                                        per.sequence = 2;
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
                            if (len.removeSequence < 1)
                            {
                                len.isRemoveLine = false;
                                switch (rad.sequence)
                                {
                                    case 0:
                                        rad.startCoord = e.Location;
                                        rad.sequence++;
                                        break;
                                    case 1:
                                        rad.coord2 = e.Location;
                                        rad.sequence++;
                                        break;
                                    case 2:
                                        if (rad.coord3 != PointF.Empty)
                                        {
                                            rad.endCoord = rad.coord3;
                                            rad.sequence++;
                                        }
                                        break;
                                    case 3:
                                        rad.extendedCoord = rad.coord3;
                                        rad.RevertToOriginalSize(scale);
                                        rad.rawCircles.Add(rad.center);
                                        rad.rawCircles.Add(rad.offsetCoord);
                                        rad.rawCircles.Add(rad.extendedCoord);
                                        rad.radiusCount++;
                                        rad.finalRawRadius[rad.radiusCount - 1] = rad.radius / scale;
                                        rad.sequence = 0;
                                        rad.distance = 0;
                                        rad.offsetCoord = PointF.Empty;
                                        rad.startCoord = PointF.Empty;
                                        rad.coord2 = PointF.Empty;
                                        rad.coord3 = PointF.Empty;
                                        rad.endCoord = PointF.Empty;
                                        rad.extendedCoord = PointF.Empty;
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
                            if (len.removeSequence < 1)
                            {
                                len.isRemoveLine = false;
                                switch (dia.sequence)
                                {
                                    case 0:
                                        dia.startCoord = e.Location;
                                        dia.sequence++;
                                        break;
                                    case 1:
                                        dia.coord2 = e.Location;
                                        dia.sequence++;
                                        break;
                                    case 2:
                                        if (dia.coord3 != PointF.Empty)
                                        {
                                            dia.endCoord = dia.coord3;
                                            dia.sequence++;
                                        }
                                        break;
                                    case 3:
                                        dia.extendedCoord = dia.coord3;
                                        dia.RevertToOriginalSize(scale);
                                        dia.rawCircles.Add(dia.center);
                                        dia.rawCircles.Add(dia.offsetCoord);
                                        dia.rawCircles.Add(dia.extendedCoord);
                                        dia.radiusCount++;
                                        dia.finalRawRadius[dia.radiusCount - 1] = dia.radius / scale;
                                        dia.sequence = 0;
                                        dia.distance = 0;
                                        dia.offsetCoord = PointF.Empty;
                                        dia.startCoord = PointF.Empty;
                                        dia.coord2 = PointF.Empty;
                                        dia.coord3 = PointF.Empty;
                                        dia.endCoord = PointF.Empty;
                                        dia.extendedCoord = PointF.Empty;
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
                            if (len.removeSequence < 1)
                            {
                                len.isRemoveLine = false;
                                switch (arc.sequence)
                                {
                                    case 0:
                                        arc.startCoord = e.Location;
                                        arc.coord2 = PointF.Empty;
                                        arc.coord3 = PointF.Empty;
                                        arc.endCoord = PointF.Empty;
                                        arc.sequence++;
                                        break;
                                    case 1:
                                        arc.coord2 = e.Location;
                                        arc.sequence++;
                                        break;
                                    case 2:
                                        if (arc.coord3 != PointF.Empty)
                                        {
                                            arc.endCoord = arc.coord3;
                                            arc.radiusCount++;
                                            arc.finalRawRadius[arc.radiusCount - 1] = arc.radius / scale;
                                            arc.finalAngle[2 * arc.radiusCount - 2] = arc.startAngle;
                                            arc.finalAngle[2 * arc.radiusCount - 1] = arc.sweepAngle;
                                            arc.RevertToOriginalSize(scale);
                                            arc.rawCircles.Add(arc.center);
                                            arc.rawCircles.Add(arc.startCoord);
                                            arc.rawCircles.Add(arc.endCoord);
                                            arc.rawCircles.Add(arc.midCoord);
                                            arc.sequence = 0;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                    break;
                case MouseButtons.Right:
                    if (len.sequence < 1 &&
                       (par.sequence == 3 || par.sequence == 0) &&
                       (per.sequence == 3 || per.sequence == 0) &&
                        rad.sequence < 1 &&
                        dia.sequence < 1 &&
                        arc.sequence < 1)
                    {
                        len.isRemoveLine = true;
                        switch (len.removeSequence)
                        {
                            case 0:
                                len.endCoord = PointF.Empty;
                                len.startCoord = e.Location;
                                len.removeSequence++;
                                break;

                            case 1:
                                len.endCoord = len.movingCoord;
                                int j = 0;
                                for (int i = 0; i < (len.lines.Count + j); i += 5)
                                {
                                    bool isLineSegmentsIntersect = len.CheckIntercept(len.startCoord, len.endCoord, len.lines[i - j + 2], len.lines[i - j + 3]);
                                    if (isLineSegmentsIntersect)
                                    {
                                        len.RemoveLine(i - j);
                                        j += 5;
                                    }
                                }
                                j = 0;
                                for (int i = 0; i < (par.lines.Count + j); i += 7)
                                {
                                    bool isLineSegmentsIntersect = par.CheckIntercept(len.startCoord, len.endCoord, par.lines[i - j + 2], par.lines[i - j + 3]);
                                    if (isLineSegmentsIntersect)
                                    {
                                        par.RemoveLine(i - j);
                                        j += 7;
                                    }
                                }
                                if (par.lines.Count == 0)
                                {
                                    par.epolateCoord1 = PointF.Empty;
                                    par.epolateCoord2 = PointF.Empty;
                                }
                                j = 0;
                                for (int i = 0; i < (per.lines.Count + j); i += 7)
                                {
                                    bool isLineSegmentsIntersect = per.CheckIntercept(len.startCoord, len.endCoord, per.lines[i - j + 3], per.lines[i - j + 4]);
                                    if (isLineSegmentsIntersect)
                                    {
                                        per.RemoveLine(i - j);
                                        j += 7;
                                    }
                                }
                                if (per.lines.Count == 0)
                                {
                                    per.epolateCoord1 = PointF.Empty;
                                    per.epolateCoord2 = PointF.Empty;
                                }
                                j = 0;
                                for (int i = 0; i < (rad.circles.Count + j); i += 3)
                                {
                                    bool isLineIntersectCircle = rad.CheckIntercept(len.startCoord, len.endCoord, rad.circles[i - j], rad.finalRadius[(i - j) / 3]);
                                    if (isLineIntersectCircle)
                                    {
                                        rad.RemoveCircle(i - j);
                                        j += 3;
                                    }
                                }
                                j = 0;
                                for (int i = 0; i < (dia.circles.Count + j); i += 3)
                                {
                                    bool isLineIntersectCircle = dia.CheckIntercept(len.startCoord, len.endCoord, dia.circles[i - j], dia.finalRadius[(i - j) / 3]);
                                    if (isLineIntersectCircle)
                                    {
                                        dia.RemoveCircle(i - j);
                                        j += 3;
                                    }
                                }
                                j = 0;
                                for (int i = 0; i < (arc.circles.Count + j); i += 4)
                                {
                                    bool isLineIntersectCircle1 = len.CheckIntercept(len.startCoord, len.endCoord, arc.circles[i - j], arc.circles[i - j + 3]);
                                    bool isLineIntersectCircle2 = len.CheckIntercept(len.startCoord, len.endCoord, arc.circles[i - j], arc.circles[i - j + 1]);
                                    if (isLineIntersectCircle1 || isLineIntersectCircle2)
                                    {
                                        arc.RemoveCircle(i - j);
                                        j += 4;
                                    }
                                }
                                len.startCoord = PointF.Empty;
                                len.endCoord = PointF.Empty;
                                len.offsetCoord = PointF.Empty;
                                len.removeSequence--;
                                len.isRemoveLine = false;
                                pictureBox1.Invalidate();
                                break;
                        }
                    }
                    else
                    {
                        len.StopMeasurement();
                        par.StopMeasurement();
                        per.StopMeasurement();
                        rad.StopMeasurement();
                        dia.StopMeasurement();
                        arc.StopMeasurement();
                        pictureBox1.Invalidate();
                    }
                    break;
            }
        }

        private void PictureBox1MouseMove(object sender, MouseEventArgs e)
        {
            if (len.isRemoveLine)
            {
                if (len.startCoord != PointF.Empty && len.endCoord == PointF.Empty)
                {
                    len.movingCoord = e.Location;
                    pictureBox1.Invalidate();
                }
                if (len.endCoord != PointF.Empty)
                {
                    len.startCoord = PointF.Empty;
                    len.endCoord = PointF.Empty;
                    len.isRemoveLine = false;
                    pictureBox1.Invalidate();
                }
            }
            else
            {
                switch (Measurement)
                {
                    case EMeasurement.LENGTH:
                        if (len.startCoord != PointF.Empty && len.endCoord == PointF.Empty)
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
                            len.length = dim.GetDistance(len.startCoord, len.movingCoord, scale);
                            pictureBox1.Invalidate();
                        }
                        if (len.endCoord != PointF.Empty && len.offsetCoord == PointF.Empty)
                        {
                            int threshold = 100;
                            PointF epolate1, epolate2;
                            PointF temp;
                            len.extendedCoord = e.Location;
                            if (len.endCoord.Y + 100 > pictureBox1.Height)
                            {
                                threshold = -100;
                            }
                            temp = dim.CalcNormal(len.startCoord, len.endCoord, threshold);
                            (epolate1, epolate2) = dim.Extrapolation(len.endCoord, temp, pictureBox1.Width, pictureBox1.Height);
                            (len.movingCoord2, _) = dim.CalcPerpendicularDistance(epolate1, epolate2, e.Location, scale);
                            len.newEndCoord = len.CalcNewCoord(len.startCoord, len.endCoord, len.movingCoord2);
                            pictureBox1.Invalidate();
                        }
                        if (len.offsetCoord != PointF.Empty)
                        {
                            pictureBox1.Invalidate();
                        }
                        break;

                    case EMeasurement.PARALLEL:
                        if (par.startCoord != PointF.Empty && par.endCoord == PointF.Empty)
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
                        if (par.endCoord != PointF.Empty && par.offsetCoord == PointF.Empty)
                        {
                            par.movingCoord2 = e.Location;
                            par.newEndCoord = par.CalcNewCoord(par.startCoord, par.endCoord, par.movingCoord2);
                            (par.epolateCoord1, par.epolateCoord2) = dim.Extrapolation(par.startCoord, par.endCoord, pictureBox1.Size.Width, pictureBox1.Height);
                            (par.epolateCoord3, par.epolateCoord4) = dim.Extrapolation(par.movingCoord2, par.newEndCoord, pictureBox1.Size.Width, pictureBox1.Height);
                            (par.perpendicularCoord, par.length) = dim.CalcPerpendicularDistance(par.epolateCoord1, par.epolateCoord2, par.movingCoord2, scale);
                            pictureBox1.Invalidate();
                        }
                        if (par.offsetCoord != PointF.Empty)
                        {
                            par.movingCoord2 = e.Location;
                            (par.perpendicularCoord, _) = dim.CalcPerpendicularDistance(par.epolateCoord1, par.epolateCoord2, par.movingCoord2, scale);
                            (par.perpendicularCoord2, _) = dim.CalcPerpendicularDistance(par.epolateCoord3, par.epolateCoord4, par.movingCoord2, scale);
                            if (par.perpendicularCoord2.X < par.perpendicularCoord.X)
                            {
                                (par.perpendicularCoord, par.perpendicularCoord2) = (par.perpendicularCoord2, par.perpendicularCoord);
                            }
                            pictureBox1.Invalidate();
                        }
                        break;

                    case EMeasurement.PERPENDICULAR:
                        if (per.startCoord != PointF.Empty && per.endCoord == PointF.Empty)
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
                        if (per.endCoord != PointF.Empty && per.offsetCoord == PointF.Empty)
                        {
                            int threshold = 100;
                            PointF temp;
                            per.movingCoord2 = e.Location;
                            (per.epolateCoord1, per.epolateCoord2) = dim.Extrapolation(per.startCoord, per.endCoord, pictureBox1.Size.Width, pictureBox1.Height);
                            (per.perpendicularCoord, per.length) = dim.CalcPerpendicularDistance(per.epolateCoord1, per.epolateCoord2, per.movingCoord2, scale);
                            if (per.endCoord.Y + 100 > pictureBox1.Height)
                            {
                                threshold = -100;
                            }
                            temp = dim.CalcNormal(per.perpendicularCoord, per.movingCoord2, threshold);
                            (per.epolateCoord3, per.epolateCoord4) = dim.Extrapolation(per.movingCoord2, temp, pictureBox1.Width, pictureBox1.Height);
                            pictureBox1.Invalidate();
                        }
                        if (per.offsetCoord != PointF.Empty)
                        {
                            per.movingCoord2 = e.Location;
                            (per.endCoord, _) = dim.CalcPerpendicularDistance(per.epolateCoord1, per.epolateCoord2, per.offsetCoord, scale);
                            (per.perpendicularCoord, _) = dim.CalcPerpendicularDistance(per.epolateCoord1, per.epolateCoord2, per.movingCoord2, scale);
                            (per.perpendicularCoord2, _) = dim.CalcPerpendicularDistance(per.epolateCoord3, per.epolateCoord4, per.movingCoord2, scale);
                            pictureBox1.Invalidate();
                        }
                        break;
                    case EMeasurement.RADIUS:
                        if (rad.startCoord != PointF.Empty)
                        {
                            if (rad.coord2 != PointF.Empty && rad.endCoord == PointF.Empty)
                            {
                                rad.distance = dim.GetDistance(rad.coord2, e.Location);
                                if (rad.distance >= 1)
                                {
                                    rad.coord3 = e.Location;
                                    rad.radius = rad.CircleEquation(rad.startCoord, rad.coord2, rad.coord3, crossScale + 1);
                                }
                            }
                            if (rad.endCoord != PointF.Empty && rad.extendedCoord == PointF.Empty)
                            {
                                rad.coord3 = e.Location;
                                rad.distance = dim.GetDistance(rad.center, rad.coord3);
                                rad.offsetCoord.X = rad.center.X + (float)((rad.radius / rad.distance) * (rad.coord3.X - rad.center.X));
                                rad.offsetCoord.Y = rad.center.Y + (float)((rad.radius / rad.distance) * (rad.coord3.Y - rad.center.Y));
                            }
                            pictureBox1.Invalidate();
                        }
                        break;
                    case EMeasurement.DIAMETER:

                        if (dia.startCoord != PointF.Empty)
                        {
                            if (dia.coord2 != PointF.Empty && dia.endCoord == PointF.Empty)
                            {
                                dia.distance = dim.GetDistance(dia.coord2, e.Location);
                                if (dia.distance >= 1)
                                {
                                    dia.coord3 = e.Location;
                                    dia.radius = dia.CircleEquation(dia.startCoord, dia.coord2, dia.coord3, crossScale + 1);
                                }
                            }
                            if (dia.endCoord != PointF.Empty && dia.extendedCoord == PointF.Empty)
                            {
                                dia.coord3 = e.Location;
                                dia.distance = dim.GetDistance(dia.center, dia.coord3);
                                dia.offsetCoord.X = dia.center.X + (float)((dia.radius / dia.distance) * (dia.coord3.X - dia.center.X));
                                dia.offsetCoord.Y = dia.center.Y + (float)((dia.radius / dia.distance) * (dia.coord3.Y - dia.center.Y));
                            }
                            pictureBox1.Invalidate();
                        }
                        break;

                    case EMeasurement.ARC:
                        if (arc.startCoord != PointF.Empty)
                        {
                            if (arc.coord2 != PointF.Empty && arc.endCoord == PointF.Empty)
                            {
                                arc.distance = dim.GetDistance(arc.coord2, e.Location, scale);
                                if (arc.distance >= 1)
                                {
                                    arc.coord3 = e.Location;
                                    arc.radius = arc.CircleEquation(arc.startCoord, arc.coord2, arc.coord3, crossScale + 1);
                                }
                            }
                            if (arc.endCoord != PointF.Empty)
                            {
                                arc.distance = 0;
                                arc.radius = arc.CircleEquation(arc.startCoord, arc.coord2, arc.endCoord, crossScale + 1);
                            }
                            pictureBox1.Invalidate();
                        }
                        break;
                }
            }
        }

        private void PictureBox1Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            Pen dashedPen = new Pen(subColor)
            {
                DashPattern = new float[] { 4F, 2F, 1F, 3F }
            };
            Pen dashedPen2 = new Pen(mainColor)
            {
                DashPattern = new float[] { 4F, 2F, 1F, 3F }
            };
            g.SmoothingMode = SmoothingMode.AntiAlias;
            SolidBrush sb_black = new SolidBrush(Color.Black);
            SolidBrush sb_white = new SolidBrush(Color.White);
            //Length

            len.RescaleAll(scale);
            par.RescaleAll(scale);
            per.RescaleAll(scale);
            rad.RescaleAll(scale);
            dia.RescaleAll(scale);
            arc.RescaleAll(scale);
            for (int i = 0; i < len.lines.Count; i += 5)
            {
                string length = Math.Round(len.finalLength[i / 5] * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                SizeF size = g.MeasureString(length, font);
                RectangleF rectangle = new RectangleF(len.lines[i + 4], size);
                float dx = len.lines[i + 4].X - len.lines[i + 3].X;
                float dy = len.lines[i + 4].Y - len.lines[i + 3].Y;
                PointF newExtendedCoord = new PointF { X = len.lines[i + 2].X - dx, Y = len.lines[i + 2].Y - dy };
                DrawCross(ref g, len.lines[i]);
                DrawCross(ref g, len.lines[i + 1]);
                if (len.lines[i + 4].X > len.lines[i + 3].X && len.lines[i + 4].X < len.lines[i + 2].X)
                {
                    g.DrawLine(arrowHarrowT, len.lines[i + 2], len.lines[i + 3]);
                }
                else if (len.lines[i + 4].X > len.lines[i + 3].X && len.lines[i + 4].X > len.lines[i + 2].X)
                {
                    g.DrawLine(regular, len.lines[i + 2], len.lines[i + 3]);
                    g.DrawLine(dashedarrowH, len.lines[i + 2], len.lines[i + 4]);
                    g.DrawLine(dashedarrowH, len.lines[i + 3], newExtendedCoord);
                }
                else
                {
                    g.DrawLine(regular, len.lines[i + 2], len.lines[i + 3]);
                    g.DrawLine(dashedarrowH, len.lines[i + 3], len.lines[i + 4]);
                    g.DrawLine(dashedarrowH, len.lines[i + 2], newExtendedCoord);
                }
                g.DrawLine(dashedPen2, len.lines[i], len.lines[i + 3]);
                g.DrawLine(dashedPen2, len.lines[i + 1], len.lines[i + 2]);
                g.FillRectangle(sb_white, rectangle);
                g.DrawString(length, font, sb_black, len.lines[i + 4]);
            }

            //Parallel
            for (int i = 0; i < par.lines.Count; i += 7)
            {
                string perpendicularDistance = Math.Round(par.finalLength[i / 7] * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                SizeF size = g.MeasureString(perpendicularDistance, font);
                RectangleF rectangle = new RectangleF(par.lines[i + 6], size);
                float dx = par.lines[i + 6].X - par.lines[i + 5].X;
                float dy = par.lines[i + 6].Y - par.lines[i + 5].Y;
                PointF newExtendedCoord = new PointF { X = par.lines[i + 4].X - dx, Y = par.lines[i + 4].Y - dy };
                g.DrawLine(Pens.Yellow, par.lines[i], par.lines[i + 1]);
                g.DrawLine(regular, par.lines[i + 2], par.lines[i + 3]);
                if (par.lines[i + 6].X > par.lines[i + 4].X && par.lines[i + 6].X < par.lines[i + 5].X)
                {
                    g.DrawLine(arrowHarrowT, par.lines[i + 4], par.lines[i + 5]);
                }
                else if (par.lines[i + 6].X > par.lines[i + 4].X && par.lines[i + 6].X > par.lines[i + 5].X)
                {
                    g.DrawLine(regular, par.lines[i + 4], par.lines[i + 5]);
                    g.DrawLine(dashedarrowH, par.lines[i + 5], par.lines[i + 6]);
                    g.DrawLine(dashedarrowH, par.lines[i + 4], newExtendedCoord);
                }
                else
                {
                    g.DrawLine(regular, par.lines[i + 4], par.lines[i + 5]);
                    g.DrawLine(dashedarrowH, par.lines[i + 4], par.lines[i + 6]);
                    g.DrawLine(dashedarrowH, par.lines[i + 5], newExtendedCoord);
                }
                g.FillRectangle(sb_white, rectangle);
                g.DrawString(perpendicularDistance, new Font("Comic Sans MS", 8), sb_black, par.lines[i + 6]);
            }

            //Perpendicular
            for (int i = 0; i < per.lines.Count; i += 7)
            {
                string perpendicularDistance = Math.Round(per.finalLength[i / 7] * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                SizeF size = g.MeasureString(perpendicularDistance, font);
                RectangleF rectangle = new RectangleF(per.lines[i + 5], size);
                DrawCross(ref g, per.lines[i + 2]);
                if ((per.lines[i + 5].X > per.lines[i + 3].X && per.lines[i + 5].X < per.lines[i + 4].X) ||
                    (per.lines[i + 5].X < per.lines[i + 3].X && per.lines[i + 5].X > per.lines[i + 4].X))
                {
                    g.DrawLine(arrowHarrowT, per.lines[i + 3], per.lines[i + 4]);
                }
                else if (per.lines[i + 3].X < per.lines[i + 4].X)
                {
                    if (per.lines[i + 5].X < per.lines[i + 3].X)
                    {
                        float dx = per.lines[i + 5].X - per.lines[i + 3].X;
                        float dy = per.lines[i + 5].Y - per.lines[i + 3].Y;
                        PointF newExtendedCoord = new PointF { X = per.lines[i + 4].X - dx, Y = per.lines[i + 4].Y - dy };
                        g.DrawLine(regular, per.lines[i + 3], per.lines[i + 4]);
                        g.DrawLine(dashedarrowH, per.lines[i + 3], per.lines[i + 5]);
                        g.DrawLine(dashedarrowH, per.lines[i + 4], newExtendedCoord);
                    }
                    else
                    {
                        float dx = per.lines[i + 5].X - per.lines[i + 4].X;
                        float dy = per.lines[i + 5].Y - per.lines[i + 4].Y;
                        PointF newExtendedCoord = new PointF { X = per.lines[i + 3].X - dx, Y = per.lines[i + 3].Y - dy };
                        g.DrawLine(regular, per.lines[i + 3], per.lines[i + 4]);
                        g.DrawLine(dashedarrowH, per.lines[i + 4], per.lines[i + 5]);
                        g.DrawLine(dashedarrowH, per.lines[i + 3], newExtendedCoord);
                    }
                }
                else
                {
                    if (per.lines[i + 5].X < per.lines[i + 3].X)
                    {
                        float dx = per.lines[i + 5].X - per.lines[i + 4].X;
                        float dy = per.lines[i + 5].Y - per.lines[i + 4].Y;
                        PointF newExtendedCoord = new PointF { X = per.lines[i + 3].X - dx, Y = per.lines[i + 3].Y - dy };
                        g.DrawLine(regular, per.lines[i + 3], per.lines[i + 4]);
                        g.DrawLine(dashedarrowH, per.lines[i + 4], per.lines[i + 5]);
                        g.DrawLine(dashedarrowH, per.lines[i + 3], newExtendedCoord);
                    }
                    else
                    {
                        float dx = per.lines[i + 5].X - per.lines[i + 3].X;
                        float dy = per.lines[i + 5].Y - per.lines[i + 3].Y;
                        PointF newExtendedCoord = new PointF { X = per.lines[i + 4].X - dx, Y = per.lines[i + 4].Y - dy };
                        g.DrawLine(regular, per.lines[i + 3], per.lines[i + 4]);
                        g.DrawLine(dashedarrowH, per.lines[i + 3], per.lines[i + 5]);
                        g.DrawLine(dashedarrowH, per.lines[i + 4], newExtendedCoord);
                    }
                }
                g.DrawLine(Pens.Yellow, per.lines[i], per.lines[i + 1]);
                g.DrawLine(dashedPen2, per.lines[i + 2], per.lines[i + 4]);
                //g.DrawLine(dashedPen2, per.lines[i + 2], per.lines[i + 6]);
                g.FillRectangle(sb_white, rectangle);
                g.DrawString(perpendicularDistance, new Font("Comic Sans MS", 8), sb_black, per.lines[i + 5]);
            }

            //Radius
            for (int i = 0; i < rad.circles.Count; i += 3)
            {
                string radius = Math.Round(rad.finalRawRadius[i / 3] * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                SizeF size = g.MeasureString(radius, font);
                RectangleF rectangle = new RectangleF(rad.circles[i + 2], size);
                float leftCornerX = rad.circles[i].X - (float)rad.finalRadius[i / 3];
                float leftCornerY = rad.circles[i].Y - (float)rad.finalRadius[i / 3];
                float axisLength = (float)(2 * rad.finalRadius[i / 3]);
                double d = dim.GetDistance(rad.circles[i + 2], rad.circles[i], scale);
                DrawCross(ref g, rad.circles[i]);
                if (d > rad.finalRadius[i / 3])
                {
                    g.DrawLine(dashedarrowH, rad.circles[i + 1], rad.circles[i + 2]);
                    g.DrawLine(regular, rad.circles[i], rad.circles[i + 1]);
                }
                else
                {
                    g.DrawLine(arrowHarrowT, rad.circles[i], rad.circles[i + 1]);
                }
                g.DrawEllipse(regular, leftCornerX, leftCornerY, axisLength, axisLength);
                g.FillRectangle(sb_white, rectangle);
                g.DrawString(radius, new Font("Comic Sans MS", 8), sb_black, rad.circles[i + 2]);
            }

            //Diameter
            for (int i = 0; i < dia.circles.Count; i += 3)
            {
                string radius = Math.Round(2 * dia.finalRawRadius[i / 3] * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                SizeF size = g.MeasureString(radius, font);
                RectangleF rectangle = new RectangleF(dia.circles[i + 2], size);
                float leftCornerX = dia.circles[i].X - (float)dia.finalRadius[i / 3];
                float leftCornerY = dia.circles[i].Y - (float)dia.finalRadius[i / 3];
                float axisLength = (float)(2 * dia.finalRadius[i / 3]);
                double d = dim.GetDistance(dia.circles[i + 2], dia.circles[i], scale);
                PointF extendedCoord = dia.ExtendLine(dia.circles[i + 1], dia.circles[i]);
                PointF extendedCoord2 = dia.ExtendLine(dia.circles[i + 2], dia.circles[i]);
                DrawCross(ref g, dia.circles[i]);
                if (d > dia.finalRadius[i / 3])
                {
                    g.DrawLine(dashedarrowH, dia.circles[i + 1], dia.circles[i + 2]);
                    g.DrawLine(regular, extendedCoord, dia.circles[i + 1]);
                    g.DrawLine(dashedarrowH, extendedCoord, extendedCoord2);
                }
                else
                {
                    g.DrawLine(arrowHarrowT, extendedCoord, dia.circles[i + 1]);
                }
                g.DrawEllipse(regular, leftCornerX, leftCornerY, axisLength, axisLength);
                g.FillRectangle(sb_white, rectangle);
                g.DrawString(radius, new Font("Comic Sans MS", 8), sb_black, dia.circles[i + 2]);
            }

            // Arc
            for (int i = 0; i < arc.circles.Count; i += 4)
            {
                float leftCornerX = arc.circles[i].X - (float)arc.finalRadius[i / 4];
                float leftCornerY = arc.circles[i].Y - (float)arc.finalRadius[i / 4];
                float axisLength = (float)(2 * arc.finalRadius[i / 4]);
                float sleftCornerX = arc.circles[i].X - smallArcScale;
                float sleftCornerY = arc.circles[i].Y - smallArcScale;
                float saxisLength = 2 * smallArcScale;
                string radius = Math.Round(arc.finalAngle[(i / 2) + 1] * (Math.PI / 180) * arc.finalRawRadius[i / 4] * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                string sweepAngle = Math.Round(Math.Abs(arc.finalAngle[(i / 2) + 1]), 2).ToString() + "°";
                SizeF size = g.MeasureString(radius, font);
                SizeF size2 = g.MeasureString(sweepAngle, font);
                RectangleF rectangle = new RectangleF(arc.circles[i + 3], size);
                RectangleF rectangle2 = new RectangleF(arc.circles[i], size2);
                DrawCross(ref g, arc.circles[i]);
                g.DrawArc(arrowHarrowT, leftCornerX, leftCornerY, axisLength, axisLength, (float)arc.finalAngle[i / 2], (float)arc.finalAngle[(i / 2) + 1]);
                g.DrawArc(regular, sleftCornerX, sleftCornerY, saxisLength, saxisLength, (float)arc.finalAngle[i / 2], (float)arc.finalAngle[(i / 2) + 1]);
                g.DrawLine(dashedPen2, arc.circles[i], arc.circles[i + 1]);
                g.DrawLine(dashedPen2, arc.circles[i], arc.circles[i + 2]);
                g.FillRectangle(sb_white, rectangle);
                g.FillRectangle(sb_white, rectangle2);
                g.DrawString(radius, new Font("Comic Sans MS", 8), sb_black, arc.circles[i + 3]);
                g.DrawString(sweepAngle, new Font("Comic Sans MS", 8), sb_black, arc.circles[i]);
            }
            if (len.isRemoveLine)
            {
                if (len.endCoord == PointF.Empty)
                {
                    g.DrawLine(dashedPen, len.startCoord, len.movingCoord);
                }
            }
            else
            {
                switch (Measurement)
                {
                    case EMeasurement.LENGTH:
                        if (len.startCoord != PointF.Empty)
                        {
                            if (len.endCoord == PointF.Empty)
                            {
                                string length = Math.Round(len.length * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                                SizeF size = g.MeasureString(length, font);
                                PointF label_position = new PointF
                                {
                                    X = len.startCoord.X + (len.movingCoord.X - len.startCoord.X) / 2,
                                    Y = len.startCoord.Y + (len.movingCoord.Y - len.startCoord.Y) / 2
                                };
                                RectangleF rectangle = new RectangleF(label_position, size);
                                DrawCross(ref g, len.startCoord);
                                DrawCross(ref g, len.movingCoord);
                                g.DrawLine(arrowHarrowT, len.startCoord, len.movingCoord);
                                g.FillRectangle(sb_white, rectangle);
                                g.DrawString(length, new Font("Comic Sans MS", 8), sb_black, label_position);
                            }
                        }
                        if (len.endCoord != PointF.Empty && len.offsetCoord == PointF.Empty)
                        {
                            string length = Math.Round(len.length * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                            SizeF size = g.MeasureString(length, font);
                            RectangleF rectangle = new RectangleF(len.extendedCoord, size);
                            float dx = len.extendedCoord.X - len.newEndCoord.X;
                            float dy = len.extendedCoord.Y - len.newEndCoord.Y;
                            PointF newExtendedCoord = new PointF { X = len.movingCoord2.X - dx, Y = len.movingCoord2.Y - dy };
                            DrawCross(ref g, len.startCoord);
                            DrawCross(ref g, len.endCoord);
                            if (len.extendedCoord.X > len.newEndCoord.X && len.extendedCoord.X < len.movingCoord2.X)
                            {
                                g.DrawLine(arrowHarrowT, len.movingCoord2, len.newEndCoord);
                            }
                            else if (len.extendedCoord.X > len.newEndCoord.X && len.extendedCoord.X > len.movingCoord2.X)
                            {
                                g.DrawLine(regular, len.movingCoord2, len.newEndCoord);
                                g.DrawLine(dashedarrowH, len.movingCoord2, len.extendedCoord);
                                g.DrawLine(dashedarrowH, len.newEndCoord, newExtendedCoord);
                            }
                            else
                            {
                                g.DrawLine(regular, len.movingCoord2, len.newEndCoord);
                                g.DrawLine(dashedarrowH, len.newEndCoord, len.extendedCoord);
                                g.DrawLine(dashedarrowH, len.movingCoord2, newExtendedCoord);
                            }
                            g.DrawLine(dashedPen2, len.startCoord, len.newEndCoord);
                            g.DrawLine(dashedPen2, len.endCoord, len.movingCoord2);
                            g.FillRectangle(sb_white, rectangle);
                            g.DrawString(length, new Font("Comic Sans MS", 8), sb_black, len.extendedCoord);
                        }
                        break;
                    case EMeasurement.PARALLEL:
                        if (par.movingCoord != PointF.Empty && par.movingCoord2 == PointF.Empty)
                        {
                            DrawCross(ref g, par.startCoord);
                            g.DrawLine(regular, par.startCoord, par.movingCoord);
                        }
                        if (par.movingCoord2 != PointF.Empty && par.offsetCoord == PointF.Empty)
                        {
                            string perpendicularDistance = Math.Round(par.length * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                            PointF label_position = new PointF
                            {
                                X = par.movingCoord2.X + (par.perpendicularCoord.X - par.movingCoord2.X) / 2,
                                Y = par.movingCoord2.Y + (par.perpendicularCoord.Y - par.movingCoord2.Y) / 2
                            };
                            SizeF size = g.MeasureString(perpendicularDistance, font);
                            RectangleF rectangle = new RectangleF(label_position, size);
                            g.DrawLine(arrowHarrowT, par.movingCoord2, par.perpendicularCoord);
                            g.DrawLine(Pens.Yellow, par.epolateCoord1, par.epolateCoord2);
                            g.DrawLine(dashedPen2, par.epolateCoord3, par.epolateCoord4);
                            g.FillRectangle(sb_white, rectangle);
                            g.DrawString(perpendicularDistance, new Font("Comic Sans MS", 8), sb_black, label_position);
                        }
                        if (par.offsetCoord != PointF.Empty && par.extendedCoord == PointF.Empty)
                        {
                            string perpendicularDistance = Math.Round(par.length * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                            float dx = par.movingCoord2.X - par.perpendicularCoord2.X;
                            float dy = par.movingCoord2.Y - par.perpendicularCoord2.Y;
                            PointF newExtendedCoord = new PointF { X = par.perpendicularCoord.X - dx, Y = par.perpendicularCoord.Y - dy };
                            SizeF size = g.MeasureString(perpendicularDistance, font);
                            RectangleF rectangle = new RectangleF(par.movingCoord2, size);
                            if (par.movingCoord2.X > par.perpendicularCoord.X && par.movingCoord2.X < par.perpendicularCoord2.X)
                            {
                                g.DrawLine(arrowHarrowT, par.perpendicularCoord, par.perpendicularCoord2);
                            }
                            else if (par.movingCoord2.X > par.perpendicularCoord.X && par.movingCoord2.X > par.perpendicularCoord2.X)
                            {
                                g.DrawLine(regular, par.perpendicularCoord, par.perpendicularCoord2);
                                g.DrawLine(dashedarrowH, par.perpendicularCoord2, par.movingCoord2);
                                g.DrawLine(dashedarrowH, par.perpendicularCoord, newExtendedCoord);
                            }
                            else
                            {
                                g.DrawLine(regular, par.perpendicularCoord, par.perpendicularCoord2);
                                g.DrawLine(dashedarrowH, par.perpendicularCoord, par.movingCoord2);
                                g.DrawLine(dashedarrowH, par.perpendicularCoord2, newExtendedCoord);
                            }
                            g.DrawLine(Pens.Yellow, par.epolateCoord1, par.epolateCoord2);
                            g.DrawLine(regular, par.epolateCoord3, par.epolateCoord4);
                            g.FillRectangle(sb_white, rectangle);
                            g.DrawString(perpendicularDistance, new Font("Comic Sans MS", 8), sb_black, par.movingCoord2);
                        }
                        break;
                    case EMeasurement.PERPENDICULAR:
                        if (per.movingCoord != PointF.Empty && per.movingCoord2 == PointF.Empty)
                        {
                            DrawCross(ref g, per.startCoord);
                            g.DrawLine(regular, per.startCoord, per.movingCoord);
                        }
                        if (per.movingCoord2 != PointF.Empty && per.offsetCoord == PointF.Empty)
                        {
                            string perpendicularDistance = Math.Round(per.length * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                            PointF label_position = new PointF
                            {
                                X = per.movingCoord2.X + (per.perpendicularCoord.X - per.movingCoord2.X) / 2,
                                Y = per.movingCoord2.Y + (per.perpendicularCoord.Y - per.movingCoord2.Y) / 2
                            };
                            SizeF size = g.MeasureString(perpendicularDistance, font);
                            RectangleF rectangle = new RectangleF(label_position, size);
                            DrawCross(ref g, per.movingCoord2);
                            g.DrawLine(arrowHarrowT, per.movingCoord2, per.perpendicularCoord);
                            g.DrawLine(Pens.Yellow, per.epolateCoord1, per.epolateCoord2);
                            g.FillRectangle(sb_white, rectangle);
                            g.DrawString(perpendicularDistance, new Font("Comic Sans MS", 8), sb_black, label_position);
                        }
                        if (per.offsetCoord != PointF.Empty && per.extendedCoord == PointF.Empty)
                        {
                            string perpendicularDistance = Math.Round(per.length * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                            SizeF size = g.MeasureString(perpendicularDistance, font);
                            RectangleF rectangle = new RectangleF(per.movingCoord2, size);
                            DrawCross(ref g, per.offsetCoord);
                            if ((per.movingCoord2.X > per.perpendicularCoord.X && per.movingCoord2.X < per.perpendicularCoord2.X) ||
                                (per.movingCoord2.X < per.perpendicularCoord.X && per.movingCoord2.X > per.perpendicularCoord2.X))
                            {
                                g.DrawLine(arrowHarrowT, per.perpendicularCoord, per.perpendicularCoord2);
                            }
                            else if (per.movingCoord2.X < per.perpendicularCoord.X)
                            {
                                if (per.perpendicularCoord.X < per.perpendicularCoord2.X)
                                {
                                    float dx = per.movingCoord2.X - per.perpendicularCoord.X;
                                    float dy = per.movingCoord2.Y - per.perpendicularCoord.Y;
                                    PointF newExtendedCoord = new PointF { X = per.perpendicularCoord2.X - dx, Y = per.perpendicularCoord2.Y - dy };
                                    g.DrawLine(regular, per.perpendicularCoord, per.perpendicularCoord2);
                                    g.DrawLine(dashedarrowH, per.perpendicularCoord, per.movingCoord2);
                                    g.DrawLine(dashedarrowH, per.perpendicularCoord2, newExtendedCoord);
                                }
                                else
                                {
                                    float dx = per.movingCoord2.X - per.perpendicularCoord2.X;
                                    float dy = per.movingCoord2.Y - per.perpendicularCoord2.Y;
                                    PointF newExtendedCoord = new PointF { X = per.perpendicularCoord.X - dx, Y = per.perpendicularCoord.Y - dy };
                                    g.DrawLine(regular, per.perpendicularCoord, per.perpendicularCoord2);
                                    g.DrawLine(dashedarrowH, per.perpendicularCoord2, per.movingCoord2);
                                    g.DrawLine(dashedarrowH, per.perpendicularCoord, newExtendedCoord);
                                }
                            }
                            else
                            {
                                if (per.perpendicularCoord.X < per.perpendicularCoord2.X)
                                {
                                    float dx = per.movingCoord2.X - per.perpendicularCoord2.X;
                                    float dy = per.movingCoord2.Y - per.perpendicularCoord2.Y;
                                    PointF newExtendedCoord = new PointF { X = per.perpendicularCoord.X - dx, Y = per.perpendicularCoord.Y - dy };
                                    g.DrawLine(regular, per.perpendicularCoord, per.perpendicularCoord2);
                                    g.DrawLine(dashedarrowH, per.perpendicularCoord2, per.movingCoord2);
                                    g.DrawLine(dashedarrowH, per.perpendicularCoord, newExtendedCoord);
                                }
                                else
                                {
                                    float dx = per.movingCoord2.X - per.perpendicularCoord.X;
                                    float dy = per.movingCoord2.Y - per.perpendicularCoord.Y;
                                    PointF newExtendedCoord = new PointF { X = per.perpendicularCoord2.X - dx, Y = per.perpendicularCoord2.Y - dy };
                                    g.DrawLine(regular, per.perpendicularCoord, per.perpendicularCoord2);
                                    g.DrawLine(dashedarrowH, per.perpendicularCoord, per.movingCoord2);
                                    g.DrawLine(dashedarrowH, per.perpendicularCoord2, newExtendedCoord);
                                }
                            }
                            g.DrawLine(Pens.Yellow, per.epolateCoord1, per.epolateCoord2);
                            g.DrawLine(dashedPen2, per.offsetCoord, per.perpendicularCoord2);
                            //g.DrawLine(dashedPen2, per.offsetCoord, per.endCoord);
                            g.FillRectangle(sb_white, rectangle);
                            g.DrawString(perpendicularDistance, new Font("Comic Sans MS", 8), sb_black, per.movingCoord2);
                        }
                        break;
                    case EMeasurement.RADIUS:
                        if (rad.startCoord != PointF.Empty && rad.coord2 == PointF.Empty)
                        {
                            DrawCross(ref g, rad.startCoord);
                        }
                        if (rad.distance >= 1)
                        {
                            if (rad.endCoord == PointF.Empty && rad.offsetCoord == PointF.Empty)
                            {
                                string radius = Math.Round(rad.radius / scale * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                                float leftCornerX = rad.center.X - rad.radius;
                                float leftCornerY = rad.center.Y - rad.radius;
                                float axisLength = (float)(2 * rad.radius);
                                PointF label_position = new PointF
                                {
                                    X = rad.center.X + (rad.coord3.X - rad.center.X) / 2,
                                    Y = rad.center.Y + (rad.coord3.Y - rad.center.Y) / 2
                                };
                                SizeF size = g.MeasureString(radius, font);
                                RectangleF rectangle = new RectangleF(label_position, size);
                                DrawCross(ref g, rad.startCoord);
                                DrawCross(ref g, rad.coord2);
                                DrawCross(ref g, rad.coord3);
                                DrawCross(ref g, rad.center);
                                g.DrawLine(arrowT, rad.center, rad.coord3);
                                g.DrawEllipse(regular, leftCornerX, leftCornerY, axisLength, axisLength);
                                g.FillRectangle(sb_white, rectangle);
                                g.DrawString(radius, new Font("Comic Sans MS", 8), sb_black, label_position);
                            }
                            if (rad.offsetCoord != PointF.Empty)
                            {
                                string radius = Math.Round(rad.radius / scale * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                                float leftCornerX = rad.center.X - rad.radius;
                                float leftCornerY = rad.center.Y - rad.radius;
                                float axisLength = (float)(2 * rad.radius);
                                SizeF size = g.MeasureString(radius, font);
                                RectangleF rectangle = new RectangleF(rad.coord3, size);
                                DrawCross(ref g, rad.center);
                                if (rad.distance > rad.radius)
                                {
                                    g.DrawLine(dashedarrowH, rad.offsetCoord, rad.coord3);
                                    g.DrawLine(regular, rad.center, rad.offsetCoord);
                                }
                                else
                                {
                                    g.DrawLine(arrowHarrowT, rad.center, rad.offsetCoord);
                                }
                                g.DrawEllipse(regular, leftCornerX, leftCornerY, axisLength, axisLength);
                                g.FillRectangle(sb_white, rectangle);
                                g.DrawString(radius, new Font("Comic Sans MS", 8), sb_black, rad.coord3);
                            }
                        }
                        break;
                    case EMeasurement.DIAMETER:
                        if (dia.startCoord != PointF.Empty && dia.coord2 == PointF.Empty)
                        {
                            DrawCross(ref g, dia.startCoord);
                        }
                        if (dia.distance >= 1)
                        {
                            if (dia.endCoord == PointF.Empty && dia.offsetCoord == PointF.Empty)
                            {
                                string radius = Math.Round(2 * (dia.radius / scale) * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                                float leftCornerX = dia.center.X - dia.radius;
                                float leftCornerY = dia.center.Y - dia.radius;
                                float axisLength = (float)(2 * dia.radius);
                                PointF label_position = new PointF
                                {
                                    X = dia.center.X + (dia.coord3.X - dia.center.X) / 2,
                                    Y = dia.center.Y + (dia.coord3.Y - dia.center.Y) / 2
                                };
                                PointF extendedPoint = dia.ExtendLine(dia.coord3, dia.center);
                                SizeF size = g.MeasureString(radius, font);
                                RectangleF rectangle = new RectangleF(label_position, size);
                                DrawCross(ref g, dia.startCoord);
                                DrawCross(ref g, dia.coord2);
                                DrawCross(ref g, dia.coord3);
                                DrawCross(ref g, dia.center);
                                g.DrawLine(arrowHarrowT, dia.coord3, extendedPoint);
                                g.DrawEllipse(regular, leftCornerX, leftCornerY, axisLength, axisLength);
                                g.FillRectangle(sb_white, rectangle);
                                g.DrawString(radius, new Font("Comic Sans MS", 8), sb_black, label_position);
                            }
                            if (dia.offsetCoord != PointF.Empty)
                            {
                                string radius = Math.Round(2 * (dia.radius / scale) * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                                float leftCornerX = dia.center.X - (float)dia.radius;
                                float leftCornerY = dia.center.Y - (float)dia.radius;
                                float axisLength = (float)(2 * dia.radius);
                                PointF extendedCoord = dia.ExtendLine(dia.offsetCoord, dia.center);
                                PointF extendedCoord2 = dia.ExtendLine(dia.coord3, dia.center);
                                SizeF size = g.MeasureString(radius, font);
                                RectangleF rectangle = new RectangleF(dia.coord3, size);
                                DrawCross(ref g, dia.center);
                                if (dia.distance > dia.radius)
                                {
                                    g.DrawLine(dashedarrowH, dia.offsetCoord, dia.coord3);
                                    g.DrawLine(regular, extendedCoord, dia.offsetCoord);
                                    g.DrawLine(dashedarrowH, extendedCoord, extendedCoord2);
                                }
                                else
                                {
                                    g.DrawLine(arrowHarrowT, extendedCoord, dia.offsetCoord);
                                }
                                g.DrawEllipse(regular, leftCornerX, leftCornerY, axisLength, axisLength);
                                g.FillRectangle(sb_white, rectangle);
                                g.DrawString(radius, new Font("Comic Sans MS", 8), sb_black, dia.coord3);
                            }
                        }
                        break;
                    case EMeasurement.ARC:
                        if (arc.startCoord != PointF.Empty && arc.coord2 == PointF.Empty)
                        {
                            DrawCross(ref g, arc.startCoord);
                        }
                        if (arc.distance >= 1)
                        {
                            if (arc.endCoord == PointF.Empty)
                            {
                                string radius = Math.Round(arc.sweepAngle * (Math.PI / 180) * (arc.radius / scale) * distPerPixel, displayDecimalPlaces).ToString() + "μm";
                                string sweepAngle = Math.Round(Math.Abs(arc.sweepAngle), 2).ToString() + "°";
                                float leftCornerX = arc.center.X - arc.radius;
                                float leftCornerY = arc.center.Y - arc.radius;
                                float axisLength = (float)(2 * arc.radius);
                                float sleftCornerX = arc.center.X - smallArcScale;
                                float sleftCornerY = arc.center.Y - smallArcScale;
                                float saxisLength = 2 * smallArcScale;
                                SizeF size = g.MeasureString(radius, font);
                                SizeF size2 = g.MeasureString(sweepAngle, font);
                                RectangleF rectangle = new RectangleF(arc.midCoord, size);
                                RectangleF rectangle2 = new RectangleF(arc.center, size2);
                                DrawCross(ref g, arc.startCoord);
                                DrawCross(ref g, arc.coord2);
                                DrawCross(ref g, arc.coord3);
                                DrawCross(ref g, arc.center);
                                g.DrawArc(arrowHarrowT, leftCornerX, leftCornerY, axisLength, axisLength, (float)arc.startAngle, (float)arc.sweepAngle);
                                g.DrawArc(regular, sleftCornerX, sleftCornerY, saxisLength, saxisLength, (float)arc.startAngle, (float)arc.sweepAngle);
                                g.DrawLine(dashedPen2, arc.center, arc.startCoord);
                                g.DrawLine(dashedPen2, arc.center, arc.coord3);
                                g.FillRectangle(sb_white, rectangle);
                                g.FillRectangle(sb_white, rectangle2);
                                g.DrawString(radius, new Font("Comic Sans MS", 8), sb_black, arc.midCoord);
                                g.DrawString(sweepAngle, new Font("Comic Sans MS", 8), sb_black, arc.center);
                            }
                        }
                        break;
                }
            }
        }

        private void DrawCross(ref Graphics g, PointF point)
        {
            g.DrawLine(regular, point.X - crossScale, point.Y - crossScale, point.X + crossScale, point.Y + crossScale);
            g.DrawLine(regular, point.X - crossScale, point.Y + crossScale, point.X + crossScale, point.Y - crossScale);
        }

        private void Clear_All_Click(object sender, EventArgs e)
        {
            len.LengthClear();
            par.ParallelClear();
            per.PerpendicularClear();
            rad.RadiusClear();
            dia.DiameterClear();
            arc.ArcClear();
            pictureBox1.Invalidate();
        }
    }

    public class Length
    {
        private const double thresholdAngleX = 5;
        private const double thresholdAngleY = 80;
        public int removeCount = 0;
        public bool isRemoveLine = false;
        public bool lineVertical = new bool();
        public bool lineHorizontal = new bool();
        public int lengthCount = 0;
        public int sequence = 0;
        public int removeSequence = 0;
        public List<PointF> lines = new List<PointF>();
        public List<PointF> rawLines = new List<PointF>();
        public PointF startCoord;
        public PointF movingCoord;
        public PointF endCoord;
        public PointF offsetCoord;
        public PointF movingCoord2;
        public PointF newEndCoord;
        public PointF extendedCoord;
        public double length;
        public double[] finalLength = new double[Form1.measurementMaxCount];

        public virtual void RevertToOriginalSize(float scale)
        {
            startCoord.X = (float)(startCoord.X / scale);
            startCoord.Y = (float)(startCoord.Y / scale);
            endCoord.X = (float)(endCoord.X / scale);
            endCoord.Y = (float)(endCoord.Y / scale);
            offsetCoord.X = (float)(offsetCoord.X / scale);
            offsetCoord.Y = (float)(offsetCoord.Y / scale);
            newEndCoord.X = (float)(newEndCoord.X / scale);
            newEndCoord.Y = (float)(newEndCoord.Y / scale);
            extendedCoord.X = (float)(extendedCoord.X / scale);
            extendedCoord.Y = (float)(extendedCoord.Y / scale);
        }

        public void RescaleAll(float scale)
        {
            lines.Clear();
            for (int i = 0; i < rawLines.Count; i++)
            {
                lines.Add(new PointF { X = 0, Y = 0 });
            }
            for (int i = 0; i < rawLines.Count; i++)
            {
                lines[i] = new PointF { X = (float)(rawLines[i].X * scale), Y = (float)(rawLines[i].Y * scale) };
            }
        }

        public virtual void RemoveLine(int index)
        {
            int j = index;
            for (int i = 0; i < 5; i++)
            {
                lines.RemoveAt(j);
                rawLines.RemoveAt(j);
            }
            lengthCount--;
            removeCount++;
            for (int i = index / 5; i < finalLength.Length - 1; i++)
            {
                finalLength[i] = finalLength[i + 1];
            };
        }

        public double GetAngle(PointF start, PointF end)
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
        public bool CheckIntercept(PointF startA, PointF endA, PointF startB, PointF endB)
        {
            float orientation1 = Orientation(startA, endA, startB);
            float orientation2 = Orientation(startA, endA, endB);
            float orientation3 = Orientation(startB, endB, startA);
            float orientation4 = Orientation(startB, endB, endA);

            if (orientation1 != orientation2 && orientation3 != orientation4) return true;
            if (orientation1 == 0 && OnSegment(startA, startB, endA)) return true;
            if (orientation2 == 0 && OnSegment(startA, endB, endA)) return true;
            if (orientation3 == 0 && OnSegment(startB, startA, endB)) return true;
            if (orientation4 == 0 && OnSegment(startB, endA, endB)) return true;

            return false;
        }

        private bool OnSegment(PointF p, PointF q, PointF r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.X) && q.Y >= Math.Min(p.Y, r.X))
            {
                return true;
            }
            return false;
        }

        private float Orientation(PointF p, PointF q, PointF r)
        {
            float val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            if (val == 0) return 0;
            return (val > 0) ? 1 : 2;
        }

        public PointF CalcNewCoord(PointF start, PointF end, PointF offsetCoord)
        {
            float dx = start.X - end.X;
            float dy = start.Y - end.Y;
            return new PointF(offsetCoord.X + dx, offsetCoord.Y + dy);
        }

        public void LengthClear()
        {
            lengthCount = 0;
            removeCount = 0;
            sequence = 0;
            removeSequence = 0;
            startCoord = PointF.Empty;
            movingCoord = PointF.Empty;
            endCoord = PointF.Empty;
            offsetCoord = PointF.Empty;
            lines.Clear();
            rawLines.Clear();
            Array.Clear(finalLength, 0, finalLength.Length - 1);
        }

        public virtual void StopMeasurement()
        {
            sequence = 0;
            removeSequence = 0;
            startCoord = PointF.Empty;
            movingCoord = PointF.Empty;
            movingCoord2 = PointF.Empty;
            offsetCoord = PointF.Empty;
            endCoord = PointF.Empty;
        }
    }

    public class Parallel : Length
    {
        public PointF perpendicularCoord, perpendicularCoord2;
        public PointF epolateCoord1, epolateCoord2, epolateCoord3, epolateCoord4;

        public override void RevertToOriginalSize(float scale)
        {
            epolateCoord1.X = (float)(epolateCoord1.X / scale);
            epolateCoord1.Y = (float)(epolateCoord1.Y / scale);
            epolateCoord2.X = (float)(epolateCoord2.X / scale);
            epolateCoord2.Y = (float)(epolateCoord2.Y / scale);
            epolateCoord3.X = (float)(epolateCoord3.X / scale);
            epolateCoord3.Y = (float)(epolateCoord3.Y / scale);
            epolateCoord4.X = (float)(epolateCoord4.X / scale);
            epolateCoord4.Y = (float)(epolateCoord4.Y / scale);
            perpendicularCoord.X = (float)(perpendicularCoord.X / scale);
            perpendicularCoord.Y = (float)(perpendicularCoord.Y / scale);
            perpendicularCoord2.X = (float)(perpendicularCoord2.X / scale);
            perpendicularCoord2.Y = (float)(perpendicularCoord2.Y / scale);
            extendedCoord.X = (float)(extendedCoord.X / scale);
            extendedCoord.Y = (float)(extendedCoord.Y / scale);
        }

        public override void RemoveLine(int index)
        {
            int j = index;
            for (int i = 0; i < 7; i++)
            {
                lines.RemoveAt(j);
                rawLines.RemoveAt(j);
            }
            lengthCount--;
            removeCount++;
            for (int i = index / 7; i < finalLength.Length - 1; i++)
            {
                finalLength[i] = finalLength[i + 1];
            };
        }
        public void ParallelClear()
        {
            lines.Clear();
            lengthCount = 0;
            removeCount = 0;
            sequence = 0;
            removeSequence = 0;
            startCoord = PointF.Empty;
            movingCoord = PointF.Empty;
            movingCoord2 = PointF.Empty;
            offsetCoord = PointF.Empty;
            epolateCoord1 = PointF.Empty;
            epolateCoord2 = PointF.Empty;
            epolateCoord3 = PointF.Empty;
            epolateCoord4 = PointF.Empty;
            perpendicularCoord = PointF.Empty;
            perpendicularCoord2 = PointF.Empty;
            endCoord = PointF.Empty;
            lines.Clear();
            rawLines.Clear();
            Array.Clear(finalLength, 0, finalLength.Length - 1);
        }

        public override void StopMeasurement()
        {
            sequence = 0;
            startCoord = PointF.Empty;
            movingCoord = PointF.Empty;
            movingCoord2 = PointF.Empty;
            endCoord = PointF.Empty;
            offsetCoord = PointF.Empty;
            epolateCoord1 = PointF.Empty;
            epolateCoord2 = PointF.Empty;
            epolateCoord3 = PointF.Empty;
            epolateCoord4 = PointF.Empty;
            perpendicularCoord = PointF.Empty;
            perpendicularCoord2 = PointF.Empty;
        }
    }

    public class Perpendicular : Parallel
    {
        public override void RevertToOriginalSize(float scale)
        {
            epolateCoord1.X = (float)(epolateCoord1.X / scale);
            epolateCoord1.Y = (float)(epolateCoord1.Y / scale);
            epolateCoord2.X = (float)(epolateCoord2.X / scale);
            epolateCoord2.Y = (float)(epolateCoord2.Y / scale);
            offsetCoord.X = (float)(offsetCoord.X / scale);
            offsetCoord.Y = (float)(offsetCoord.Y / scale);
            perpendicularCoord.X = (float)(perpendicularCoord.X / scale);
            perpendicularCoord.Y = (float)(perpendicularCoord.Y / scale);
            perpendicularCoord2.X = (float)(perpendicularCoord2.X / scale);
            perpendicularCoord2.Y = (float)(perpendicularCoord2.Y / scale);
            extendedCoord.X = (float)(extendedCoord.X / scale);
            extendedCoord.Y = (float)(extendedCoord.Y / scale);
            endCoord.X = (float)(endCoord.X / scale);
            endCoord.Y = (float)(endCoord.Y / scale);
        }

        public override void RemoveLine(int index)
        {
            int j = index;
            for (int i = 0; i < 7; i++)
            {
                lines.RemoveAt(j);
                rawLines.RemoveAt(j);
            }
            lengthCount--;
            removeCount++;
            for (int i = index / 6; i < finalLength.Length - 1; i++)
            {
                finalLength[i] = finalLength[i + 1];
            };
        }

        public void PerpendicularClear()
        {
            lines.Clear();
            lengthCount = 0;
            removeCount = 0;
            sequence = 0;
            removeSequence = 0;
            startCoord = PointF.Empty;
            movingCoord = PointF.Empty;
            movingCoord2 = PointF.Empty;
            offsetCoord = PointF.Empty;
            epolateCoord1 = PointF.Empty;
            epolateCoord2 = PointF.Empty;
            perpendicularCoord = PointF.Empty;
            perpendicularCoord2 = PointF.Empty;
            endCoord = PointF.Empty;
            lines.Clear();
            rawLines.Clear();
            Array.Clear(finalLength, 0, finalLength.Length - 1);
        }

        public override void StopMeasurement()
        {
            sequence = 0;
            startCoord = PointF.Empty;
            movingCoord = PointF.Empty;
            movingCoord2 = PointF.Empty;
            endCoord = PointF.Empty;
            offsetCoord = PointF.Empty;
        }
    }

    public class Radius
    {
        public int sequence = 0;
        public int radiusCount = 0;
        public int removeCount = 0;
        public float radius = new float();
        public double distance = new double();
        public PointF startCoord;
        public PointF coord2;
        public PointF coord3;
        public PointF endCoord;
        public PointF center;
        public PointF offsetCoord;
        public PointF extendedCoord;
        public List<PointF> circles = new List<PointF>();
        public List<PointF> rawCircles = new List<PointF>();
        public double[] finalRawRadius = new double[Form1.measurementMaxCount];
        public double[] finalRadius = new double[Form1.measurementMaxCount];

        public virtual void RevertToOriginalSize(float scale)
        {
            center.X = (float)(center.X / scale);
            center.Y = (float)(center.Y / scale);
            offsetCoord.X = (float)(offsetCoord.X / scale);
            offsetCoord.Y = (float)(offsetCoord.Y / scale);
            extendedCoord.X = (float)(extendedCoord.X / scale);
            extendedCoord.Y = (float)(extendedCoord.Y / scale);
        }

        public void RescaleAll(float scale)
        {
            circles.Clear();
            for (int i = 0; i < rawCircles.Count; i++)
            {
                circles.Add(new PointF { X = 0, Y = 0 });
            }
            for (int i = 0; i < rawCircles.Count; i++)
            {
                circles[i] = new PointF { X = (int)(rawCircles[i].X * scale), Y = (int)(rawCircles[i].Y * scale) };
            }
            for (int i = 0; i < finalRawRadius.Length; i++)
            {
                finalRadius[i] = finalRawRadius[i] * scale;
            }
        }

        public virtual void RemoveCircle(int index)
        {
            int j = index;
            for (int i = 0; i < 3; i++)
            {
                circles.RemoveAt(j);
                rawCircles.RemoveAt(j);
            }
            radiusCount--;
            removeCount++;
            for (int i = index / 3; i < finalRawRadius.Length - 1; i++)
            {
                finalRawRadius[i] = finalRawRadius[i + 1];
            };
        }

        public virtual float CircleEquation(PointF coord1, PointF coord2, PointF coord3, float threshold)
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
            if ((int)h > int.MaxValue - threshold || (int)h < int.MinValue + threshold)
            {
                return 0.0F;
            }
            if ((int)k > int.MaxValue - threshold || (int)k < int.MinValue + threshold)
            {
                return 0.0F;
            }
            double sr = h * h + k * k - c;
            center = new PointF { X = (float)h, Y = (float)k };
            return (float)Math.Sqrt(sr);
        }

        public bool CheckIntercept(PointF lineStart, PointF lineEnd, PointF center, double radius)
        {
            Dimensioning tempDim = new Dimensioning();
            double l = tempDim.GetDistance(lineStart, lineEnd);
            double d1 = tempDim.GetDistance(lineStart, center);
            double d2 = tempDim.GetDistance(lineEnd, center);
            if (d1 < radius || d2 < radius)
            {
                return false;
            }
            double theta = Math.Acos((Math.Pow(d1, 2) + Math.Pow(l, 2) - Math.Pow(d2, 2)) / (2 * d1 * l));
            double theta2 = Math.Acos((Math.Pow(d2, 2) + Math.Pow(l, 2) - Math.Pow(d1, 2)) / (2 * d2 * l));
            if (theta >= (Math.PI / 2) || theta2 >= (Math.PI / 2))
            {
                return false;
            }
            double d = d1 * Math.Sin(theta);
            if (d > radius)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void RadiusClear()
        {
            circles.Clear();
            rawCircles.Clear();
            radiusCount = 0;
            removeCount = 0;
            sequence = 0;
            radius = 0;
            distance = 0;
            startCoord = PointF.Empty;
            coord2 = PointF.Empty;
            coord3 = PointF.Empty;
            endCoord = PointF.Empty;
            offsetCoord = PointF.Empty;
            extendedCoord = PointF.Empty;
            center = PointF.Empty;
            Array.Clear(finalRawRadius, 0, finalRawRadius.Length - 1);
        }

        public virtual void StopMeasurement()
        {
            sequence = 0;
            startCoord = PointF.Empty;
            coord2 = PointF.Empty;
            coord3 = PointF.Empty;
            endCoord = PointF.Empty;
            offsetCoord = PointF.Empty;
            extendedCoord = PointF.Empty;
            distance = 0;
        }
    }

    public class Diameter : Radius
    {
        public PointF ExtendLine(PointF endPoint, PointF center)
        {
            float dx = (center.X - endPoint.X) * 2;
            float dy = (center.Y - endPoint.Y) * 2;
            return new PointF(endPoint.X + dx, endPoint.Y + dy);
        }
        public void DiameterClear()
        {
            circles.Clear();
            rawCircles.Clear();
            radiusCount = 0;
            removeCount = 0;
            sequence = 0;
            radius = 0;
            distance = 0;
            startCoord = PointF.Empty;
            coord2 = PointF.Empty;
            coord3 = PointF.Empty;
            endCoord = PointF.Empty;
            offsetCoord = PointF.Empty;
            extendedCoord = PointF.Empty;
            center = PointF.Empty;
            Array.Clear(finalRawRadius, 0, finalRawRadius.Length - 1);
        }
    }

    public class Arc : Radius
    {
        public double startAngle = new double();
        public double sweepAngle = new double();
        public PointF midCoord;
        public int angleCount = 0;
        public double[] finalAngle = new double[Form1.measurementMaxCount];

        public override void RevertToOriginalSize(float scale)
        {
            center.X = (float)(center.X / scale);
            center.Y = (float)(center.Y / scale);
            startCoord.X = (float)(startCoord.X / scale);
            startCoord.Y = (float)(startCoord.Y / scale);
            endCoord.X = (float)(endCoord.X / scale);
            endCoord.Y = (float)(endCoord.Y / scale);
            midCoord.X = (float)(midCoord.X / scale);
            midCoord.Y = (float)(midCoord.Y / scale);
        }

        public override float CircleEquation(PointF coord1, PointF coord2, PointF coord3, float threshold)
        {
            double stopAngle;
            double midAngle;
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
            double sr = h * h + k * k - c;
            if ((int)h > int.MaxValue - threshold || (int)h < int.MinValue + threshold)
            {
                return (float)Math.Sqrt(sr);
            }
            if ((int)k > int.MaxValue - threshold || (int)k < int.MinValue + threshold)
            {
                return (float)Math.Sqrt(sr);
            }
            float tempRadius = (float)Math.Sqrt(sr);
            center = new PointF { X = (float)h, Y = (float)k };
            startAngle = Math.Atan2(y1 - center.Y, x1 - center.X) * 180 / Math.PI;
            stopAngle = Math.Atan2(y3 - center.Y, x3 - center.X) * 180 / Math.PI;
            sweepAngle = stopAngle - startAngle;
            if (sweepAngle < 0)
            {
                sweepAngle += 360;
            }
            midAngle = (sweepAngle) / 2 + startAngle;
            midCoord = new PointF
            {
                X = (float)(Math.Round(center.X + (float)(tempRadius * Math.Cos(midAngle * Math.PI / 180)), 2)),
                Y = (float)(Math.Round(center.Y + (float)(tempRadius * Math.Sin(midAngle * Math.PI / 180)), 2))
            };
            return (float)Math.Sqrt(sr);
        }

        public override void RemoveCircle(int index)
        {
            int j = index;
            for (int i = 0; i < 4; i++)
            {
                circles.RemoveAt(j);
                rawCircles.RemoveAt(j);
            }
            radiusCount--;
            removeCount++;
            for (int i = index / 4; i < finalRawRadius.Length - 1; i++)
            {
                finalRawRadius[i] = finalRawRadius[i + 1];
            }
            for (int i = index / 2; i < finalAngle.Length - 3; i++)
            {
                finalAngle[i] = finalAngle[i + 2];
                finalAngle[i + 1] = finalAngle[i + 3];
            }
        }

        public void ArcClear()
        {
            circles.Clear();
            rawCircles.Clear();
            radius = 0;
            distance = 0;
            radiusCount = 0;
            angleCount = 0;
            startAngle = 0;
            sweepAngle = 0;
            removeCount = 0;
            sequence = 0;
            startCoord = PointF.Empty;
            coord2 = PointF.Empty;
            coord3 = PointF.Empty;
            center = PointF.Empty;
            endCoord = PointF.Empty;
            Array.Clear(finalRawRadius, 0, finalRawRadius.Length - 1);
            Array.Clear(finalAngle, 0, finalAngle.Length - 1);
        }

        public override void StopMeasurement()
        {
            sequence = 0;
            startCoord = PointF.Empty;
            coord2 = PointF.Empty;
            coord3 = PointF.Empty;
            endCoord = PointF.Empty;
            distance = 0;
        }
    }

    public class Dimensioning
    {
        public (PointF, PointF) Extrapolation(PointF start, PointF end, int xMax, int yMax)
        {
            PointF newStart, newEnd;
            start.Y = Math.Abs(start.Y - yMax);
            end.Y = Math.Abs(end.Y - yMax);
            if (start.X == end.X)
            {
                newStart = new PointF { X = start.X, Y = 0 };
                newEnd = new PointF { X = start.X, Y = yMax };
                return (newStart, newEnd);
            }
            double m = (double)(end.Y - start.Y) / (double)(end.X - start.X);
            double c = end.Y - m * end.X;
            if (c < 0)
            {
                newStart = new PointF { X = (float)(-c / m), Y = 0 };
            }
            else if (c > yMax)
            {
                newStart = new PointF { X = (float)((yMax - c) / m), Y = yMax };
            }
            else
            {
                newStart = new PointF { X = 0, Y = (float)c };
            }
            double d = m * xMax + c;
            if (d < 0)
            {
                newEnd = new PointF { X = (float)(-c / m), Y = 0 };
            }
            else if (d > yMax)
            {
                newEnd = new PointF { X = (float)((yMax - c) / m), Y = yMax };
            }
            else
            {
                newEnd = new PointF { X = xMax, Y = (float)d };
            }
            newStart.Y = Math.Abs(newStart.Y - yMax);
            newEnd.Y = Math.Abs(newEnd.Y - yMax);
            return (newStart, newEnd);
        }

        public PointF CalcNormal(PointF start, PointF end, int threshold = 100)
        {
            PointF newStart;
            if (start.X == end.X)
            {
                newStart = new PointF { X = end.X + 1, Y = end.Y };
                return newStart;
            }
            else if (start.Y == end.Y)
            {
                newStart = new PointF { X = end.X, Y = end.Y + 1 };
                return newStart;
            }
            double m = (double)(end.Y - start.Y) / (double)(end.X - start.X);
            double new_m = -(1 / m);
            double c = (double)end.Y - (new_m * (double)end.X);
            double d = ((double)(end.Y + threshold) - c) / new_m;
            newStart = new PointF { X = (float)d , Y = end.Y + threshold };
            return newStart;
        }

        public (PointF, double) CalcPerpendicularDistance(PointF start, PointF end, PointF offsetCoord, float scale)
        {
            double l = GetDistance(start, end, scale);
            double d1 = GetDistance(start, offsetCoord, scale);
            double d2 = GetDistance(end, offsetCoord, scale);
            double theta = Math.Acos((Math.Pow(d1, 2) + Math.Pow(l, 2) - Math.Pow(d2, 2)) / (2 * d1 * l));
            double d = d1 * Math.Sin(theta);
            double s = d1 * Math.Cos(theta);
            return (new PointF { X = (float)(start.X + (s / l) * (end.X - start.X)), Y = (float)(start.Y + (s / l) * (end.Y - start.Y)) }, d);
        }

        public double GetDistance(PointF start, PointF end, float scale = 1)
        {
            double x_diff = (double)(end.X / scale) - (double)(start.X / scale);
            double y_diff = (double)(end.Y / scale) - (double)(start.Y / scale);
            return Math.Sqrt(Math.Pow(x_diff, 2) + Math.Pow(y_diff, 2));
        }
    }
}
