using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace Vision_Measurement
{
    public partial class Comment : Form
    {
        private readonly Form1 _tempForm;
        static int lastFontIndex = -1;
        static int lastForeColorIndex = -1;
        static int lastFontSize;
        public Comment(Form1 form1)
        {
            InitializeComponent();
            ControlBox = false;
            Text = "Add Comment";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            string[] fonts = new string[FontFamily.Families.Length];
            for(int i = 0; i < FontFamily.Families.Length; i++) 
            {
                fonts[i] = FontFamily.Families[i].Name;
            }
            comboBox1.DataSource = fonts;
            if(lastFontIndex > -1)
            {
                comboBox1.SelectedIndex = lastFontIndex;
            }
            comboBox2.DataSource = Enum.GetValues(typeof(KnownColor)).Cast<KnownColor>().Where(k => k > KnownColor.Transparent && k < KnownColor.ButtonFace)
                                       .Select(k => Color.FromKnownColor(k)).ToList();
            if (lastForeColorIndex > -1)
            {
                comboBox2.SelectedIndex = lastForeColorIndex;
            }
            if(lastFontSize > 0)
            {
                textBox2.Text = lastFontSize.ToString();
            }
            _tempForm = form1;
        }

        private void CancelClick(object sender, EventArgs e)
        {
            Close();
        }

        private void OkClick(object sender, EventArgs e)
        {
            if(textBox1.Text != null)
            {
                _tempForm.lab.comment = textBox1.Text;
                _tempForm.lab.isPlacing = true;
            }
            _tempForm.lab.fontFamily = comboBox1.Items[comboBox1.SelectedIndex].ToString();
            _tempForm.lab.fontColor = comboBox2.Items[comboBox2.SelectedIndex].ToString().Replace("Color [", "").Replace("]", "");
            if(int.TryParse(textBox2.Text, out int result))
            {
                if (result > 5)
                {
                    _tempForm.lab.fontSize = result;
                }
            }
            lastFontIndex = comboBox1.SelectedIndex;
            lastForeColorIndex = comboBox2.SelectedIndex;
            lastFontSize = _tempForm.lab.fontSize;
            Close();
        }

        private void PreviewText(object sender, EventArgs e)
        {
            if(textBox1.Text != null)
            {
                label5.Text = textBox1.Text;
            }
            if (int.TryParse(textBox2.Text, out int result))
            {
                if (result > 5 && comboBox1.SelectedIndex >= 0)
                {
                    label5.Font = new Font(comboBox1.Items[comboBox1.SelectedIndex].ToString(), result);
                }
            }
            if(comboBox2.SelectedIndex >= 0)
            {
                label5.ForeColor = Color.FromName(comboBox2.Items[comboBox2.SelectedIndex].ToString().Replace("Color [", "").Replace("]", ""));
            }
        }
    }
}
