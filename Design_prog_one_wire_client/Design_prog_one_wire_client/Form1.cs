using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Design_prog_one_wire_client
{
    public partial class Form1 : Form
    {
        private Bitmap MyImage;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.Columns[0].Name = "Données";
            dataGridView1.Columns[1].Name = "Accessoire";
            dataGridView1.Rows.Add("ID", "");
            dataGridView1.Rows.Add("Level", "");
            dataGridView1.Rows.Add("Couple Max", "");
            dataGridView1.Rows.Add("Coeff", "");
            dataGridView1.Rows.Add("Longueur", "");
            dataGridView1.Rows.Add("Coeff Flexion", "");
            dataGridView1.Rows.Add("Date", "");

            comboBox1.Items.Add("RALLONGE 3/8 court");
            comboBox1.Items.Add("RALLONGE 200");
            comboBox1.Items.Add("RALLONGE 300");
            comboBox1.Items.Add("RALLONGE 400");
            comboBox1.Items.Add("CLIQUET_1");
            comboBox1.Items.Add("CLIQUET_2");

            if (MyImage != null)
            {
                MyImage.Dispose();
            }

            // Stretches the image to fit the pictureBox.
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            MyImage = new Bitmap(@"C:\Users\amiramont\Documents\Visual Studio 2010\Projects\Programmation_Rallonges_WiFi_V1_0\Moment_Alpha_logo.png");
            //pictureBox1.ClientSize = new Size(50, 90);
            pictureBox1.Image = (Image)MyImage;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

        }
    }
}
