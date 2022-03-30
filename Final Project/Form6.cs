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
using System.IO;


namespace CMSC_169_Project_1
{
    public partial class Form6 : Form
    {
        public Form6()
        {
            InitializeComponent();
        }

        List<Image> imgList;
        bool play;
        int fileCount=0;


        private void openMultipleImagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "IMAGE FILES | *.jpg;*.jpeg;*.tiff;*.png";
            ofd.Multiselect = true;
            ofd.Title = "Open Image Files";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imgList = new List<Image>();

                foreach (string file in ofd.FileNames)
                {
                    Image loadedImage = Image.FromFile(file);
                    imgList.Add(loadedImage);
                    fileCount++;
                }

                pictureBox1.Image = imgList[0];
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

                foreach (Image i in imgList)
                {
                    Console.WriteLine(i.Width + " " + i.Height);
                }
            }
        }

        int vIndex = 0;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (play == true && imgList != null)
            {
                pictureBox1.Image = imgList[vIndex];
                vIndex++;

                if (vIndex == imgList.Count)
                {
                    vIndex = 0;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            play = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            play = false;
        }
    }
}

