using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Vision_Measurement
{
    public partial class Settings : Form
    {
        readonly Param temp = new Param();
        readonly Param _temp = new Param();

        public Settings(Param pars)
        {
            InitializeComponent();
            ControlBox = false;
            Text = "Settings";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            temp = pars;
            _temp.DistPerPixel = temp.DistPerPixel;
            _temp.MainColor = temp.MainColor;
            _temp.SubColor = temp.SubColor;
            _temp.EdgeDetectWidth = temp.EdgeDetectWidth;
        }

        private void BtnOkClick(object sender, EventArgs e)
        {
            Close();
        }

        private void SettingsLoad(object sender, EventArgs e)
        {
            propertyGrid1.SelectedObject = temp;
            propertyGrid1.ExpandAllGridItems();
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            temp.DistPerPixel = _temp.DistPerPixel;
            temp.MainColor = _temp.MainColor;
            temp.SubColor = _temp.SubColor;
            temp.EdgeDetectWidth= _temp.EdgeDetectWidth;
            Close();
        }
    }
    public class Param
    {
        private int _edgeDetectWidth = 20;
        private double _distPerPixel = 3.45;
        private Color _mainColor = Color.Cyan;
        private Color _subColor = Color.Yellow;

        [Category("Image Settings")]
        public double DistPerPixel
        {
            get { return _distPerPixel; }
            set { _distPerPixel = value; }
        }

        [Category("Measurement Settings")]
        public Color MainColor
        {
            get { return _mainColor; }
            set { _mainColor = value; }
        }

        [Category("Measurement Settings")]
        public Color SubColor
        {
            get { return _subColor; }
            set { _subColor = value; }
        }

        [Category("Measurement Settings")]
        public int EdgeDetectWidth
        { 
            get { return _edgeDetectWidth; }
            set { _edgeDetectWidth = value; }
        }
    }
}
