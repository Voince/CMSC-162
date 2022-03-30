using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CMSC_169_Project_1
{
    public partial class Form4 : Form
    {
        Bitmap pcxBMP;
        public Form4()
        {
            InitializeComponent();
        }

        //Function to take the image from the initial form
        public Image Form4Image
        {
            get { return pictureBox1.Image; }
            set
            {
                pictureBox1.Image = ToGrayscale((Bitmap)value); //Change to grayscale
                pcxBMP = ToGrayscale((Bitmap)value); 
            }
        }

        //Set to orig image
        private void button1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(pcxBMP);
            groupBox1.Text = "orig Grayscale";
        }

        //Salt and Pepper Noise
        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap saltPepper = (Bitmap)pictureBox1.Image.Clone();
            Random r = new Random(); //generate random

            double totSum = 0;

            for (int x = 0; x < saltPepper.Width; x++)
            {
                for (int y = 0; y < saltPepper.Height; y++)
                {
                    Color c = saltPepper.GetPixel(x, y);

                    double diff;

                    int max = 100;
                    int noise = r.Next(max + 1); //generate random num
                    if (noise == 0)
                    {
                        saltPepper.SetPixel(x, y, Color.FromArgb(c.A, 0, 0, 0));
                        diff = c.R - 0;
                    }
                    else if (noise == max)
                    {
                        saltPepper.SetPixel(x, y, Color.FromArgb(c.A, 255, 255, 255));
                        diff = c.R - 255;
                    }
                    else
                    {
                        saltPepper.SetPixel(x, y, Color.FromArgb(c.A, c.R, c.G, c.B));
                        diff = c.R - c.R;
                    }
                    totSum = totSum + Math.Pow(diff, 2);
                }
            }
            double mse = totSum / (saltPepper.Width * saltPepper.Height);
            double ratio = PSNR(255, mse);

            pictureBox1.Image = new Bitmap(saltPepper);
            groupBox1.Text = "With Noise: PSNR = " + ratio.ToString();
        }

        //LowPass Filters
        private void button3_Click(object sender, EventArgs e)
        {
            avgFilter();
            medianCross();
            medianSquare();

            groupBox5.Visible = false;
        }

        //Higpass Filters
        private void button4_Click(object sender, EventArgs e)
        {
            // get blurred image from average filter
            avgFilter();
            Bitmap blur = (Bitmap)pictureBox2.Image.Clone();

            lapFiltering();
            unsharpMasking(blur);
            highboostFiltering(blur);

            groupBox5.Visible = true;
        }

        //Gradient using Sobel Feldmann Operator
        private void button5_Click(object sender, EventArgs e)
        {
            sfX();
            sfY();
            sfMagnitiude();

            groupBox5.Visible = false;
        }

        private static Bitmap ToGrayscale(Bitmap image)
        {
            Bitmap result = new Bitmap(image.Width, image.Height);

            using (Graphics g = Graphics.FromImage(result))
            {
                ColorMatrix gMatrix = new ColorMatrix(
                    new float[][]
                    {
                        new float[] { 0.299f, 0.299f, 0.299f, 0, 0},
                        new float[] { 0.587f, 0.587f, 0.587f, 0, 0},
                        new float[] { 0.114f, 0.114f, 0.114f, 0, 0},
                        new float[] { 0, 0, 0, 1, 0},
                        new float[] { 0, 0, 0, 0, 1}
                    }
                );

                using (ImageAttributes imageAttribute = new ImageAttributes())
                {
                    imageAttribute.SetColorMatrix(gMatrix);
                    g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                        0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttribute);
                }
            }
            return result;
        }

        private double PSNR(int max, double mse)
        {
            return 20 * Math.Log10(max) - 10 * Math.Log10(mse);
        }

        //Averaging Filter
        private void avgFilter()
        {
            Bitmap avgFilter = (Bitmap)pictureBox1.Image.Clone();

            double sumColor = 0;
            double sumMSE = 0;
            double diff = 0;
            for (int x = 0; x < avgFilter.Width - 5; x++)
            {
                for (int y = 0; y < avgFilter.Height - 5; y++)
                {
                    for (int i = x; i < x + 5; i++)
                    {
                        for (int j = y; j < y + 5; j++)
                        {
                            Color c = avgFilter.GetPixel(i, j);
                            sumColor = sumColor + c.R;
                            diff = diff + c.R;
                        }
                    }
                    int nColor = (int)Math.Round(sumColor / 25, 10); 
                    diff /= nColor; 
                    sumMSE = sumMSE + Math.Pow(diff, 2);

                    avgFilter.SetPixel(x + 1, y + 1, Color.FromArgb(nColor, nColor, nColor));
                    sumColor = 0;
                    diff = 0;
                }
            }
            double mse = sumMSE / (avgFilter.Width * avgFilter.Height);
            double ratio = PSNR(255, mse);

            pictureBox2.Image = avgFilter;
            groupBox2.Text = "Average Filter 5: PSNR = " + ratio.ToString();
        }

        //Median Cross Function
        private void medianCross()
        {
            Bitmap medianCross = (Bitmap)pictureBox1.Image.Clone();
            List<int> colorNeighbor = new List<int>();

            double sumMSE = 0;
            double diff = 0;
            for (int x = 0; x < medianCross.Width - 5; x++)
            {
                for (int y = 0; y < medianCross.Height - 5; y++)
                {
                    for (int i = x; i < x + 5; i++)
                    {
                        if (i == x + 2)
                        {
                            for (int j = y; j < y + 5; j++)
                            {
                                Color c = medianCross.GetPixel(i, j);
                                colorNeighbor.Add(c.R);
                                diff = diff + c.R;
                            }
                        }
                        else
                        {
                            Color c = medianCross.GetPixel(i, y + 3);
                            colorNeighbor.Add(c.R);
                            diff = diff + c.R;
                        }

                    }
                    colorNeighbor.Sort();
                    int nColor = colorNeighbor[(colorNeighbor.Count - 1) / 2];
                    diff = diff / nColor; 
                    sumMSE = sumMSE + Math.Pow(diff, 2);

                    medianCross.SetPixel(x + 1, y + 1, Color.FromArgb(nColor, nColor, nColor));

         
                    colorNeighbor.Clear();
                    diff = 0;
                }
            }
            double mse = sumMSE / (medianCross.Width * medianCross.Height);
            double ratio = PSNR(255, mse);

            pictureBox3.Image = medianCross;
            groupBox3.Text = "P-Median Cross 5: PSNR = " + ratio.ToString();
        }

        //Median Square Function
        private void medianSquare()
        {
            Bitmap medianSquare = (Bitmap)pictureBox1.Image.Clone();
            List<int> colorNeighbor = new List<int>();

            double sumMSE = 0;
            double diff = 0;
            for (int x = 0; x < medianSquare.Width - 5; x++)
            {
                for (int y = 0; y < medianSquare.Height - 5; y++)
                {
                    for (int i = x; i < x + 5; i++)
                    {
                        for (int j = y; j < y + 5; j++)
                        {
                            Color c = medianSquare.GetPixel(i, j);
                            colorNeighbor.Add(c.R);
                            diff = diff + c.R;
                        }
                    }
                    colorNeighbor.Sort();
                    int nColor = colorNeighbor[(colorNeighbor.Count - 1) / 2];
                    diff = diff / nColor; 
                    sumMSE = sumMSE + Math.Pow(diff, 2);

                    medianSquare.SetPixel(x + 1, y + 1, Color.FromArgb(nColor, nColor, nColor));

                    colorNeighbor.Clear();
                    diff = 0;
                }
            }
            double mse = sumMSE / (medianSquare.Width * medianSquare.Height);
            double ratio = PSNR(255, mse);

            pictureBox4.Image = medianSquare;
            groupBox4.Text = "P-Median Square 5: PSNR = " + ratio.ToString();
        }

        //lapFiltering Function
        private void lapFiltering()
        {
            Bitmap laplacianMasking = (Bitmap)pictureBox1.Image.Clone();
            List<int> selectedColors = new List<int>();

            double sumMSE = 0;
            double diff = 0;
            for (int x = 0; x < laplacianMasking.Width - 3; x++)
            {
                for (int y = 0; y < laplacianMasking.Height - 3; y++)
                {
                    for (int i = x; i < x + 3; i++)
                    {
                        if (i == x || i == x + 2)
                        {
                            Color c = laplacianMasking.GetPixel(i, y + 1);
                            selectedColors.Add(c.R);
                            diff = diff + c.R;
                        }
                        else
                        {
                            for (int j = y; j < y + 3; j++)
                            {
                                Color c = laplacianMasking.GetPixel(i, j);
                                if (j == y + 1)
                                {
                                    selectedColors.Add(-4 * c.R);
                                    diff = diff + -4 * c.R;
                                }
                                else
                                {
                                    selectedColors.Add(c.R);
                                    diff = diff + c.R;
                                }
                            }
                        }
                    }
                    int nColor = selectedColors.Sum();
                    nColor = Math.Abs(nColor) > 255 ? 255 : Math.Abs(nColor);
                    sumMSE = sumMSE + Math.Pow(diff, 2);

                    laplacianMasking.SetPixel(x, y, Color.FromArgb(nColor, nColor, nColor));

                    // reset
                    selectedColors.Clear();
                    diff = 0;
                }
            }
            double mse = sumMSE / (laplacianMasking.Width * laplacianMasking.Height);
            double ratio = PSNR(255, mse);

            pictureBox3.Image = laplacianMasking;
            groupBox3.Text = "Laplacian 3: PSNR = " + ratio.ToString();
        }

        private void unsharpMasking(Bitmap blur)
        {
            Bitmap orig = (Bitmap)pictureBox1.Image.Clone();
            Bitmap mask = (Bitmap)pictureBox1.Image.Clone();

            double sumMSE = 0;
            for (int x = 0; x < blur.Width; x++)
            {
                for (int y = 0; y < blur.Height; y++)
                {
                    Color cBlur = blur.GetPixel(x, y);
                    Color cOrig = orig.GetPixel(x, y);

                    int getMask = (cOrig.R - cBlur.R) < 0 ? 0 : cOrig.R - cBlur.R;

                    int addMask = cOrig.R + 1 * getMask > 255 ? 255 : cOrig.R + 1 * getMask;

                    mask.SetPixel(x, y, Color.FromArgb(addMask, addMask, addMask));

                    double diff = cOrig.R - addMask;
                    sumMSE = sumMSE + Math.Pow(diff, 2);
                }
            }
            double mse = sumMSE / (mask.Width * mask.Height);
            double ratio = PSNR(255, mse);

            pictureBox2.Image = new Bitmap(mask);
            groupBox2.Text = "Unsharp Masking: PSNR = " + ratio.ToString();
        }

        private void highboostFiltering(Bitmap blur)
        {
            int k = trackBar1.Value;

            Bitmap orig = (Bitmap)pictureBox1.Image.Clone();
            Bitmap filter = (Bitmap)pictureBox1.Image.Clone();
            double sumMSE = 0;

            for (int x = 0; x < blur.Width; x++)
            {
                for (int y = 0; y < blur.Height; y++)
                {
                    Color cBlur = blur.GetPixel(x, y);
                    Color cOrig = orig.GetPixel(x, y);

                    int getMask = (cOrig.R - cBlur.R) < 0 ? 0 : cOrig.R - cBlur.R;
                    int addMask = cOrig.R + k * getMask > 255 ? 255 : cOrig.R + k * getMask;

                    filter.SetPixel(x, y, Color.FromArgb(addMask, addMask, addMask));

                    double diff = cOrig.R - addMask;
                    sumMSE = sumMSE + Math.Pow(diff, 2);
                }
            }
            double mse = sumMSE / (filter.Width * filter.Height);
            double ratio = PSNR(255, mse);

            pictureBox4.Image = new Bitmap(filter);
            groupBox4.Text = "Highboost Filtering "
                + k
                + ": PSNR = "
                + ratio.ToString();
        }

        //Function for Sobel X 
        private void sfX()
        {
            Bitmap sobel = (Bitmap)pictureBox1.Image.Clone();
            List<int> selectedColors = new List<int>();

            double sumMSE = 0;
            double diff = 0;
            for (int x = 0; x < sobel.Width - 3; x++)
            {
                for (int y = 0; y < sobel.Height - 3; y++)
                {
                    for (int i = x; i < x + 3; i++)
                    {
                        if (i == x || i == x + 2)
                        {
                            for (int j = y; j < y + 3; j++)
                            {
                                Color c = sobel.GetPixel(i, j);
                                if (j == y)
                                {
                                    selectedColors.Add(-1 * c.R);
                                    diff = diff + -1 * c.R;
                                }
                                else if (j == y + 2)
                                {
                                    selectedColors.Add(c.R);
                                    diff = diff + c.R;
                                }
                            }
                        }
                        else
                        {
                            for (int j = y; j < y + 3; j++)
                            {
                                Color c = sobel.GetPixel(i, j);
                                if (j == y)
                                {
                                    selectedColors.Add(-2 * c.R);
                                    diff = diff + -2 * c.R;
                                }
                                else if (j == y + 2)
                                {
                                    selectedColors.Add(2 * c.R);
                                    diff = diff + 2 * c.R;
                                }
                            }
                        }
                    }
                    int nColor = selectedColors.Sum();
                    nColor = Math.Abs(nColor) > 255 ? 255 : Math.Abs(nColor);
                    sumMSE = sumMSE + Math.Pow(diff, 2);

                    sobel.SetPixel(x, y, Color.FromArgb(nColor, nColor, nColor));

                    // reset
                    selectedColors.Clear();
                    diff = 0;
                }
            }
            double mse = sumMSE / (sobel.Width * sobel.Height);
            double ratio = PSNR(255, mse);

            pictureBox2.Image = sobel;
            groupBox2.Text = "Sobel X-Gradient: PSNR = " + ratio.ToString();
        }

        //Function for Sobel Y 
        private void sfY()
        {
            Bitmap sobel = (Bitmap)pictureBox1.Image.Clone();
            List<int> selectedColors = new List<int>();

            double sumMSE = 0;
            double diff = 0;
            for (int x = 0; x < sobel.Width - 3; x++)
            {
                for (int y = 0; y < sobel.Height - 3; y++)
                {
                    for (int i = x; i < x + 3; i++)
                    {
                        if (i == x)
                        {
                            for (int j = y; j < y + 3; j++)
                            {
                                Color c = sobel.GetPixel(i, j);
                                if (j == y || j == y + 2)
                                {
                                    selectedColors.Add(-1 * c.R);
                                    diff = diff + -1 * c.R;
                                }
                                else
                                {
                                    selectedColors.Add(-2 * c.R);
                                    diff = diff + -2 * c.R;
                                }
                            }
                        }
                        else if (i == x + 2)
                        {
                            for (int j = y; j < y + 3; j++)
                            {
                                Color c = sobel.GetPixel(i, j);
                                if (j == y || j == y + 2)
                                {
                                    selectedColors.Add(1 * c.R);
                                    diff = diff + 1 * c.R;
                                }
                                else
                                {
                                    selectedColors.Add(2 * c.R);
                                    diff = diff + 2 * c.R;
                                }
                            }
                        }
                    }
                    int nColor = selectedColors.Sum();
                    nColor = Math.Abs(nColor) > 255 ? 255 : Math.Abs(nColor);
                    sumMSE = sumMSE + Math.Pow(diff, 2);

                    sobel.SetPixel(x, y, Color.FromArgb(nColor, nColor, nColor));

                    selectedColors.Clear();
                    diff = 0;
                }
            }
            double mse = sumMSE / (sobel.Width * sobel.Height);
            double ratio = PSNR(255, mse);

            pictureBox3.Image = sobel;
            groupBox3.Text = "Sobel Y-Gradient: PSNR = " + ratio.ToString();
        }

        //Function for Sobel Magnitude 
        private void sfMagnitiude()
        {
            Bitmap sfMagnitiude = (Bitmap)pictureBox1.Image.Clone();

            Bitmap sfX = (Bitmap)pictureBox2.Image.Clone();
            Bitmap sfY = (Bitmap)pictureBox3.Image.Clone();

            double sumMSE = 0;
            for (int x = 0; x < sfMagnitiude.Width; x++)
            {
                for (int y = 0; y < sfMagnitiude.Height; y++)
                {
                    Color cX = sfX.GetPixel(x, y);
                    Color cY = sfY.GetPixel(x, y);

                    double gradient = Math.Sqrt(Math.Pow(cX.R, 2) + Math.Pow(cY.R, 2));

                    int nColor = (int)gradient;
                    nColor = Math.Abs(nColor) > 255 ? 255 : Math.Abs(nColor);
                    sumMSE = sumMSE + Math.Pow(gradient, 2);

                    sfMagnitiude.SetPixel(x, y, Color.FromArgb(nColor, nColor, nColor));
                }
            }
            double mse = sumMSE / (sfMagnitiude.Width * sfMagnitiude.Height);
            double ratio = PSNR(255, mse);

            pictureBox4.Image = sfMagnitiude;
            groupBox4.Text = "Sobel Magnitude: PSNR = " + ratio.ToString();
        }
    }
}
