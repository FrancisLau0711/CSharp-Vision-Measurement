using System;
using System.ComponentModel;
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
            Close();
        }
    }
    public class Param
    {
        private double _distPerPixel = 3.45;

        [Category("Image Settings")]
        public double DistPerPixel
        {
            get { return _distPerPixel; }
            set { _distPerPixel = value; }
        }
    }
}
