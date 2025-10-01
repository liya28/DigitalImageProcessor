using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DigitalImageProcessor
{
    public partial class ConvolutionForm : Form
    {
        Bitmap originalImage;
        Bitmap processedImage;

        public ConvolutionForm()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e) {}

        private void ConvolutionForm_Load(object sender, EventArgs e) {}

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                originalImage = new Bitmap(ofd.FileName);
                processedImage = (Bitmap)originalImage.Clone();
                pictureBox1.Image = originalImage;
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (processedImage != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "PNG Image|*.png|JPEG Image|*.jpg";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    processedImage.Save(sfd.FileName);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ApplyFilter(BitmapFilter.Sharpen);
        }


        private void ApplyFilter(Func<Bitmap, bool> filterFunc)
        {
            if (originalImage == null) return;

            processedImage = (Bitmap)originalImage.Clone();
            filterFunc(processedImage);
            pictureBox2.Image = processedImage;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ApplyFilter(BitmapFilter.Smoothen);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ApplyFilter(BitmapFilter.GaussianBlur);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ApplyFilter(BitmapFilter.MeanRemoval);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ApplyFilter(BitmapFilter.EmbossLaplacian);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ApplyFilter(BitmapFilter.EmbossHorVert);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ApplyFilter(BitmapFilter.EmbossLossy);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ApplyFilter(BitmapFilter.EmbossAllDir);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            ApplyFilter(BitmapFilter.EmbossHorizontal);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ApplyFilter(BitmapFilter.EmbossVertical);
        }

        private void goBackToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            this.Hide();
            var mainForm = Application.OpenForms["Form1"];
            if (mainForm != null)
            {
                mainForm.Show();
            }
        }
    }
}
