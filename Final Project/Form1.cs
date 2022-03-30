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


namespace CMSC_169_Project_1
{
    public partial class Form1 : Form
    {
        //Global Variables
        Bitmap pcxBMP;
        int[,] rVAL, gVAL, bVAL;

        public Form1()
        {
            InitializeComponent();
        }

        //If user clicks File > Open
        //Allows user to open a new file and getting the PXC Header information
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "pcx files (*.pcx)|*.pcx|All files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    PCXHeaderEncoder(filePath);
                    ColorPalette(filePath);
                    ProcessRGB();
                }
            }
        }

        private Form2 Form2Instance;
        private void histogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2Instance = new Form2
            {
                Form2Image = pictureBox1.Image
            };
            Form2Instance.Show();
        }

        private Form3 Form3Instance;
        private void processToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3Instance = new Form3
            {
                Form3Image = pictureBox1.Image
            };
            Form3Instance.Show();
        }

        public void PCXHeaderEncoder(string filePath)
        {
            using (BinaryReader bReader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                byte[] buffer = new byte[2];

                //Get the data from PCX file 
                int manufac = bReader.ReadByte();
                int version = bReader.ReadByte();
                int encoding = bReader.ReadByte();
                int BPP = bReader.ReadByte();
                int xMin = bReader.ReadInt16();
                int yMin = bReader.ReadInt16();
                int xMax = bReader.ReadInt16();
                int yMax = bReader.ReadInt16();
                int HDPI = bReader.ReadInt16();
                int VDPI = bReader.ReadInt16();

                bReader.ReadBytes(48);
                bReader.ReadByte();

                int numCP = bReader.ReadByte();

                int BPL = bReader.ReadInt16();
                int paletteInfo = bReader.ReadByte();

                int hSS = bReader.ReadByte();
                int vSS = bReader.ReadByte();

                bReader.ReadBytes(54);

                //Display in all labels
                label1.Text = "Manufacturer: Zshoft .pcx (" + manufac + ")";
                label2.Text = "Version: " + version;
                label3.Text = "Encoding: " + encoding;
                label5.Text = "Bits per Pixel: " + BPP;
                label6.Text = "Image Dimensions: " + xMin + " " + yMin + " " + xMax + " " + yMax;
                label7.Text = "HDPI: " + HDPI;
                label8.Text = "VDPI: " + VDPI;
                label9.Text = "Number of Color Planes: " + numCP;
                label10.Text = "Bytes Per Lin: " + BPL;
                label11.Text = "Palette Informaion: " + paletteInfo;
                label11.Text = "Horizontal Screen Size: " + hSS;
                label12.Text = "Vertical Screen Size: " + vSS;
            }
        }

        private Form4 Form4Instance;
        private void filterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form4Instance = new Form4
            {
                /* set pictureBox in Guide05 */
                Form4Image = pictureBox1.Image
            };
            Form4Instance.Show();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private Form6 Form6Instance;
        private void videoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form6Instance = new Form6 { };
            Form6Instance.Show();
        }

        //Function to create the color palette and image
        public void ColorPalette(string filePath)
        {
            //Color Pallete
            byte[] PCXBytes = File.ReadAllBytes(filePath);
            List<Color> CP = new List<Color>();
            bool CPEmpty = true;
            if (PCXBytes.Length > 768)
            {
                if (PCXBytes[PCXBytes.Length - 768 - 1] == 0x0C)
                {
                    CPEmpty = false;
                    for (int i = PCXBytes.Length - 768; i < PCXBytes.Length; i += 3)
                    {
                        CP.Add(Color.FromArgb(PCXBytes[i], PCXBytes[i + 1], PCXBytes[i + 2]));
                    }
                }
            }

            //Color 
            Color[] savepalette = new Color[256 * 256];
            List<byte> palettevalue = new List<byte>();

            //Starting position is 128
            int position = 128;
            byte rCount = 0;
            byte rValue = 0;

            //Checks the type of byte (either 1-byte or 2-byte)
            do
            {
                byte Byte = PCXBytes[position++];
                if ((Byte & 0xC0) == 0xC0 && position < PCXBytes.Length)
                {
                    rCount = (byte)(Byte & 0x3F);
                    rValue = PCXBytes[position++];
                }
                else
                {
                    rCount = 1;
                    rValue = Byte;
                }
                for (int j = 0; j < rCount; j++)
                {
                    palettevalue.Add(rValue);
                }
            } while (position < PCXBytes.Length);

            //Forming the image
            pcxBMP = new Bitmap(256, 256);
            if (!CPEmpty)
            {
                for (int i = 0; i < 256 * 256; i++)
                {
                    savepalette[i] = CP[palettevalue[i]];
                    int y = i / 256;
                    int x = i - (256 * y);
                    pcxBMP.SetPixel(x, y, savepalette[i]);
                }
            }

            pictureBox1.Image = new Bitmap(pcxBMP);

            //Forming the color palette map
            Bitmap ColorMap = new Bitmap(128, 128);
            int z = 0;
            for (int i = 0; i < 80; i = i + 5)
            {
                for (int j = 0; j < 80; j = j + 5)
                {
                    using (Graphics gfx = Graphics.FromImage(ColorMap))
                    using (SolidBrush brush = new SolidBrush(CP[z]))
                    {
                        gfx.FillRectangle(brush, i, j, 5, 5);
                    }
                    z++;
                }
            }
            pictureBox2.Image = new Bitmap(ColorMap);
        }
        //Preparation of RGB components
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
    }
}

