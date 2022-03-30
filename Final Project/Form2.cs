using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace CMSC_169_Project_1
{
    public partial class Form2 : Form
    {
        //Global Variables
        Bitmap pcxBMP, rBMP, gBMP, bBMP;
        int[,] rVAL, gVAL, bVAL;

        public Form2()
        {
            InitializeComponent();
        }

        //Function to take the image from the initial form
        public Image Form2Image
        {
            get { return pictureBox1.Image; }
            set
            {
                pictureBox1.Image = value;
                pcxBMP = new Bitmap(value);
                ProcessRGB();
            }
        }

        //Display Red Channel
        private void button1_Click(object sender, EventArgs e)
        {
            rBMP = new Bitmap(256, 256);
            DisplayChannel(rBMP, rVAL, 'r');
        }

        //Display Green Channel
        private void button2_Click(object sender, EventArgs e)
        {
            gBMP = new Bitmap(256, 256);
            DisplayChannel(gBMP, gVAL, 'g');
        }

        //Display Blue Channel
        private void button3_Click(object sender, EventArgs e)
        {
            bBMP = new Bitmap(256, 256);
            DisplayChannel(bBMP, bVAL, 'b');
        }


        private void ProcessRGB()
        {
            rVAL = new int[256, 256];
            gVAL = new int[256, 256];
            bVAL = new int[256, 256];

            //Collects intensity level per pixel of each channel
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    rVAL[i, j] = pcxBMP.GetPixel(i, j).R;
                    gVAL[i, j] = pcxBMP.GetPixel(i, j).G;
                    bVAL[i, j] = pcxBMP.GetPixel(i, j).B;
                }
            }
        }

        private void DisplayChannel(Bitmap cBMP, int[,] cVAL, char channel)
        {
            int[] freq = new int[255];
            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {
                    if (channel == 'r')
                        cBMP.SetPixel(i, j, Color.FromArgb(cVAL[i, j], 0, 0));
                    else if (channel == 'g')
                        cBMP.SetPixel(i, j, Color.FromArgb(0, cVAL[i, j], 0));
                    else if (channel == 'b')
                        cBMP.SetPixel(i, j, Color.FromArgb(0, 0, cVAL[i, j]));

                    //For Histogram
                    freq[cVAL[i, j]] += 1;
                }
            }
            pictureBox1.Image = cBMP;

            //Histogram
            chart1.Series.Clear();
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = 255;
            chart1.ChartAreas[0].AxisX.Interval = 50;

            Series pixels = new Series();
            pixels.Name = "Pixels";

            for (int i = 0; i < freq.Length; i++) pixels.Points.AddXY(i, freq[i]);

            chart1.Series.Add(pixels);
        }
    }
}
