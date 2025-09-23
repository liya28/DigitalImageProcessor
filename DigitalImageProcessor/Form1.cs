using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WebCamLib;
using static Emgu.CV.OCR.Tesseract;

namespace DigitalImageProcessor
{
    public partial class Form1 : Form
    {
        Bitmap originalImage;
        Bitmap processedImage;
        Bitmap imageA;
        Bitmap imageB;
        Bitmap resultImage;

        private Device webcam;

        Timer webcamTimer = new Timer();

        enum ProcessingMode { None, Grayscale, Invert, Sepia }
        ProcessingMode currentMode = ProcessingMode.None;

        public Form1()
        {
            InitializeComponent();

            webcamTimer.Interval = 30;
            webcamTimer.Tick += WebcamTimer_Tick;
        }

        private Bitmap GetWebcamFrame()
        {
            try
            {
                webcam.Sendmessage();

                Image clipboardImage = Clipboard.GetImage();
                if (clipboardImage == null)
                    return null;

                Bitmap bmp32 = new Bitmap(clipboardImage.Width, clipboardImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (Graphics g = Graphics.FromImage(bmp32))
                {
                    g.DrawImage(clipboardImage, new Rectangle(0, 0, bmp32.Width, bmp32.Height));
                }

                return bmp32;
            }
            catch
            {
                return null;
            }
        }

        private void WebcamTimer_Tick(object sender, EventArgs e)
        {
            Bitmap currentFrame = GetWebcamFrame();
            if (currentFrame == null) return;

            try
            {
                switch (currentMode)
                {
                    case ProcessingMode.Grayscale:
                        ApplyGrayscale(currentFrame);
                        break;

                    case ProcessingMode.Invert:
                        ApplyInvert(currentFrame);
                        break;

                    case ProcessingMode.Sepia:
                        ApplySepia(currentFrame);
                        break;
                }

                var oldImage = pictureBox2.Image;
                pictureBox2.Image = currentFrame;
                oldImage?.Dispose();
            }
            catch
            {
                currentFrame.Dispose();
            }
        }

        private void ApplyGrayscale(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(data.Stride) * data.Height;
            byte[] pixels = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, bytes);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte gray = (byte)((pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3);
                pixels[i] = gray;      // B
                pixels[i + 1] = gray;  // G
                pixels[i + 2] = gray;  // R
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, bytes);
            bmp.UnlockBits(data);
        }
        private void ApplyInvert(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(data.Stride) * data.Height;
            byte[] pixels = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, bytes);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = (byte)(255 - pixels[i]);       // B
                pixels[i + 1] = (byte)(255 - pixels[i + 1]); // G
                pixels[i + 2] = (byte)(255 - pixels[i + 2]); // R
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, bytes);
            bmp.UnlockBits(data);
        }

        private void ApplySepia(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var data = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int bytes = Math.Abs(data.Stride) * data.Height;
            byte[] pixels = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, bytes);

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte B = pixels[i];
                byte G = pixels[i + 1];
                byte R = pixels[i + 2];

                int tr = (int)(0.393 * R + 0.769 * G + 0.189 * B);
                int tg = (int)(0.349 * R + 0.686 * G + 0.168 * B);
                int tb = (int)(0.272 * R + 0.534 * G + 0.131 * B);

                pixels[i] = (byte)Math.Min(255, tb); // B
                pixels[i + 1] = (byte)Math.Min(255, tg); // G
                pixels[i + 2] = (byte)Math.Min(255, tr); // R
            }

            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, bytes);
            bmp.UnlockBits(data);
        }

        private void processToolStripMenuItem_Click(object sender, EventArgs e) { }

        private void pictureBox1_Click(object sender, EventArgs e) { }

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

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;

            processedImage = (Bitmap)originalImage.Clone();
            for (int y = 0; y < processedImage.Height; y++)
            {
                for (int x = 0; x < processedImage.Width; x++)
                {
                    Color pixel = processedImage.GetPixel(x, y);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    processedImage.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
            pictureBox2.Image = processedImage;
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalImage != null)
            {
                processedImage = (Bitmap)originalImage.Clone();
                pictureBox2.Image = processedImage;
            }
        }

        private void invertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;

            processedImage = (Bitmap)originalImage.Clone();

            for (int y = 0; y < processedImage.Height; y++)
            {
                for (int x = 0; x < processedImage.Width; x++)
                {
                    Color pixel = processedImage.GetPixel(x, y);
                    processedImage.SetPixel(x, y, Color.FromArgb(255 - pixel.R, 255 - pixel.G, 255 - pixel.B));
                }
            }
            pictureBox2.Image = processedImage;
        }

        private void sepiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;

            processedImage = (Bitmap)originalImage.Clone();
            for (int y = 0; y < processedImage.Height; y++)
            {
                for (int x = 0; x < processedImage.Width; x++)
                {
                    Color pixel = processedImage.GetPixel(x, y);
                    int tr = (int)(0.393 * pixel.R + 0.769 * pixel.G + 0.189 * pixel.B);
                    int tg = (int)(0.349 * pixel.R + 0.686 * pixel.G + 0.168 * pixel.B);
                    int tb = (int)(0.272 * pixel.R + 0.534 * pixel.G + 0.131 * pixel.B);

                    tr = Math.Min(255, tr);
                    tg = Math.Min(255, tg);
                    tb = Math.Min(255, tb);

                    processedImage.SetPixel(x, y, Color.FromArgb(tr, tg, tb));
                }
            }
            pictureBox2.Image = processedImage;
        }

        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (originalImage == null) return;

            int[] histogram = new int[256];
            processedImage = (Bitmap)originalImage.Clone();

            for (int y = 0; y < processedImage.Height; y++)
            {
                for (int x = 0; x < processedImage.Width; x++)
                {
                    Color pixel = processedImage.GetPixel(x, y);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    histogram[gray]++;
                    processedImage.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }

            int max = histogram.Max();
            Bitmap histImage = new Bitmap(256, 100);

            using (Graphics g = Graphics.FromImage(histImage))
            {
                g.Clear(Color.White);
                for (int i = 0; i < 256; i++)
                {
                    int barHeight = (int)(histogram[i] * 100.0 / max);
                    g.DrawLine(Pens.Black, i, 100, i, 100 - barHeight);
                }
            }
            pictureBox2.Image = histImage;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imageB = new Bitmap(ofd.FileName);
                pictureBox1.Image = imageB;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imageA = new Bitmap(ofd.FileName);
                pictureBox2.Image = imageA;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (imageA == null || imageB == null)
            {
                MessageBox.Show("Please load both images first.");
                return;
            }

            Color myGreen = Color.FromArgb(0, 255, 0);
            int greyGreen = (myGreen.R + myGreen.G + myGreen.B) / 3;
            int threshold = 60;

            int width = Math.Min(imageA.Width, imageB.Width);
            int height = Math.Min(imageA.Height, imageB.Height);

            resultImage = new Bitmap(width, height);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color pixel = imageB.GetPixel(x, y);
                    Color backPixel = imageA.GetPixel(x, y);

                    int grey = (pixel.R + pixel.G + pixel.B) / 3;
                    int subtractValue = Math.Abs(grey - greyGreen);

                    bool isGreen = subtractValue < threshold && pixel.G > pixel.R && pixel.G > pixel.B;
                    resultImage.SetPixel(x, y, isGreen ? backPixel : pixel);
                }
            }

            pictureBox3.Image = resultImage;
        }

        private void startWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1.Visible = false;
            button3.Visible = false;

            Device[] devices = DeviceManager.GetAllDevices();
            if (devices.Length == 0)
            {
                MessageBox.Show("No webcam devices found.");
                return;
            }

            webcam = devices.FirstOrDefault(d => d.Name.Contains("ManyCam")) ?? devices[0];

            try
            {
                webcam.ShowWindow(pictureBox1);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start webcam: " + ex.Message);
            }
        }

        private void stopWebcamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            webcam?.Stop();
        }

        private void pictureBox2_Click(object sender, EventArgs e) { }

        private void grayscaleToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            currentMode = ProcessingMode.Grayscale;
            webcamTimer.Start();
        }

        private void sepiaToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            currentMode = ProcessingMode.Sepia;
            webcamTimer.Start();
        }

        private void colorInversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentMode = ProcessingMode.Invert;
            webcamTimer.Start();
        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void subtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image == null)
            {
                MessageBox.Show("Please load a background first!");
                return;
            }

            Bitmap newBackground = new Bitmap(pictureBox2.Image); 
            Bitmap webcamFrame = GetWebcamFrame(); 

            if (webcamFrame == null)
            {
                MessageBox.Show("No webcam frame available. Make sure the webcam is active.");
                return;
            }

            int width = Math.Min(newBackground.Width, webcamFrame.Width);
            int height = Math.Min(newBackground.Height, webcamFrame.Height);

            Bitmap result = new Bitmap(width, height);

            Color myGreen = Color.FromArgb(0, 255, 0);
            int greyGreen = (myGreen.R + myGreen.G + myGreen.B) / 3;
            int threshold = 60;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color camPixel = webcamFrame.GetPixel(x, y);
                    Color bgPixel = newBackground.GetPixel(x, y);

                    int grey = (camPixel.R + camPixel.G + camPixel.B) / 3;
                    int subtractValue = Math.Abs(grey - greyGreen);

                    bool isGreen = subtractValue < threshold && camPixel.G > camPixel.R && camPixel.G > camPixel.B;

                    result.SetPixel(x, y, isGreen ? bgPixel : camPixel);
                }
            }

            pictureBox3.Image?.Dispose();
            pictureBox3.Image = result;
        }
        }
    }
