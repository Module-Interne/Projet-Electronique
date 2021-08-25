using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Collections.Generic;
using System.Management;

namespace Programmation_1_wire_production
{

    public partial class Form1 : Form
    {
        private static bool crc_emission = false;
        private static bool crc_reception = true;

        private bool form_gestion_xml = false;

        private bool try_connect = false;
        private uint cpt_timeout = 0;

        private string appVersion = "V1.0";
        private string appDate = "20/07/21";

        private bool flagEmissionLecture = false;
        private bool flagEmissionEcriture = false;

        private byte cpt_frame_send = 0;

        public int[] bufferR = new int[100];
        private byte[] bufferE = new byte[100];
        public bool flag = false;
        private char[] bufferR_convert = new char[100];

        private static string[] acc_name_list = new string[100];
        private static string[] acc_path_list = new string[100];
        private int selected_accessory_index = 0;

        private char[] bufferR_xml = new char[25000];

        private bool flagFormatDataOk = false;
        private string[] cell_compare = new string[8];

        public byte cellRow = 0;
        public byte cellColumn = 0;

        private int[] buffer_circulaire = new int[5000];
        private int index_circulaire = 0;
        private int previous_index_circulaire = 0;
        private int header_index = 0;
        private int crc_index = 0;
        private bool flag_get_new_message = false;
        
        //Form2 form2 = new Form2(acc_name_list, acc_path_list);
        //private delegate void display_data_on_combobox();
        //event EventHandler? Disposed;

        //private Thread trd;

        static SerialPort serialPort;
        Accessoire accessoire = new Accessoire();
        

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            init();
            get_com_list();
            //serialPort.Open();
            //serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            //form2.Disposed += new EventHandler(display_data_on_combobox);
        }

        /***********************************************************************************************************************************************************/
        // Test get com
        /***********************************************************************************************************************************************************/

        void get_com_list()
        {
            string[] ports = SerialPort.GetPortNames();

            if (comboBox2.Items.Count != ports.Length)
            {
                comboBox2.Items.Clear();
                for (int i = 0; i < ports.Length; i++) comboBox2.Items.Add(ports[i]);
            }
        }

        /***********************************************************************************************************************************************************/
        // Main
        /***********************************************************************************************************************************************************/

        private void timer1_Tick(object sender, EventArgs e)
        {
            char[] buffTemp = new char[8];
            //buffTemp[0] = '5';
            /*if (flag)
                listBox1.Items.Add("bien recu!" + " header : " + header_index + ", index : " + index_circulaire + ", longueur : " + buffer_circulaire[header_index + 1]);*/
            if (flagEmissionLecture || flag)
            {
                //listBox1.Items.Add("main ok");
                //serialPort.Open();
                //listBox1.Items.Add("step 1");
                /*listBox1.Items.Add(serialPort.ReadByte());
                listBox1.Items.Add("step 2");
                listBox1.Items.Add(serialPort.ReadByte());
                listBox1.Items.Add("step 3");
                listBox1.Items.Add(serialPort.ReadByte());
                listBox1.Items.Add("step 4");*/

                for (int i = 0; i < bufferR[1]; i++)
                    listBox1.Items.Add(i + " : " + Convert.ToString(bufferR[i],16));

                /*for (int i = 0; i < 24; i++)
                    listBox1.Items.Add(i + " : " + Convert.ToString(bufferR[i]));*/

                /*listBox1.Items.Add(Convert.ToString(bufferR[0]));
                listBox1.Items.Add(Convert.ToString(bufferR[1]));
                listBox1.Items.Add(Convert.ToString(bufferR[2]));*/
                listBox1.Items.Add("");
                //serialPort.Close();

                if (bufferR[2] == 6)
                {
                    button2.Enabled = true;
                    button3.Enabled = true;
                    comboBox2.Enabled = false;
                    clear_bufferR();
                    try_connect = false;
                    cpt_timeout = 0;
                    cpt_frame_send = 0;
                    timer3.Enabled = false;
                    Form3 form3 = new Form3(20);
                    form3.ShowDialog();
                }

                if (bufferR[2] == 3)
                {
                    get_rallonge_data();
                    display_detected_accessory();
                    clear_bufferR();
                }

                if (bufferR[2] == 4)
                {
                    clear_bufferR();
                    label5.Text = "Datas saved succesfully";
                    label5.ForeColor = System.Drawing.Color.Green;
                    Form3 form3 = new Form3(7);
                    form3.ShowDialog();
                    
                }

                if (bufferR[2] == 5)
                {
                    switch (bufferR[3])
                    {
                        case 1:
                            clear_bufferR();
                            label5.ForeColor = System.Drawing.Color.Red; 
                            label5.Text = "Error 1: Wrong CRC";
                            break;


                        case 2:
                            clear_bufferR();
                            label5.ForeColor = System.Drawing.Color.Red; 
                            label5.Text = "Error 2: No accessory detected";
                            break;


                        case 3:
                            clear_bufferR();
                            label5.ForeColor = System.Drawing.Color.Red; 
                            label5.Text = "Error 3: More then 1 accessory";
                            break;


                        case 4:
                            clear_bufferR();
                            label5.ForeColor = System.Drawing.Color.Red; 
                            label5.Text = "Error 4: Write data failed";
                            break;


                        case 5:
                            clear_bufferR();
                            label5.ForeColor = System.Drawing.Color.Red; 
                            label5.Text = "Error 5: Datas false";
                            break;


                        case 6:
                            //label5.Text = "Error 6: Mauvais CRC, veuillez vérifier la communication entre le PC et l'adaptateur";
                            listBox1.Items.Add("v");
                            break;

                        case 7:
                            listBox1.Items.Add("f");
                            break;


                        default:
                            break;
                    }
                }

                flagEmissionLecture = false;
                flag = false;
            }

            if (flagEmissionEcriture)
            {
                flagEmissionEcriture = false;
            }
        }

        /***********************************************************************************************************************************************************/
        // Handlers
        /***********************************************************************************************************************************************************/

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;// Dans main, ajouter test (3 ou 5 fois si donnée reçu. Si non reception, reset index circulaire (pour place dans buffer circulaire)
            previous_index_circulaire = index_circulaire;

            /*for (int i = 0; sp.BytesToRead > 0; i++)
            {
                //bufferR[i] = sp.ReadByte();
                bufferR[i] = sp.ReadByte();
            }*/

            for (int i = index_circulaire; sp.BytesToRead > 0; i++)
            {
                //bufferR[i] = sp.ReadByte();
                buffer_circulaire[i] = sp.ReadByte();
                index_circulaire = i;
            }

            for (int i = previous_index_circulaire; i < index_circulaire; i++)
            {

                if (buffer_circulaire[i] == 77)
                {
                    header_index = i;

                }
            }

            if (buffer_circulaire[header_index + 1] <= index_circulaire - (header_index - 1))// -1 car l'index du header compte dans la longueur de la trame
            {
                flag = true;
                for (int i = 0; i < buffer_circulaire[header_index + 1]; i++)
                    bufferR[i] = buffer_circulaire[header_index + i];
            }
            //label5.Text = "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2(acc_name_list,acc_path_list);
            form_gestion_xml = true;
            form2.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            flagEmissionLecture = true;
            string frame = "";

            try
            {
                frame = create_read_frame();
                //listBox1.Items.Add("Envoyé : " + frame);
                serialPort.WriteLine(frame);
                label5.Text = "";
            }
            catch(Exception ex)
            {
                MessageBox.Show("Serial port undefined");
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Convert.ToString(dataGridView1[1, 0].Value) != "" && Convert.ToString(dataGridView1[1, 1].Value) != "" &&
                Convert.ToString(dataGridView1[1, 2].Value) != "" && Convert.ToString(dataGridView1[1, 3].Value) != "" &&
                Convert.ToString(dataGridView1[1, 4].Value) != "" && Convert.ToString(dataGridView1[1, 5].Value) != "" &&
                Convert.ToString(dataGridView1[1, 6].Value) != "" && Convert.ToString(dataGridView1[1, 7].Value) != "" &&
                Convert.ToString(dataGridView1[1, 6].Value) != "0/0/0" && Convert.ToString(dataGridView1[1, 7].Value) != "0/0/0")
            {

                flagEmissionEcriture = true;
                string frame = "";

                read_cells();
                frame = create_write_frame();
                //listBox1.Items.Add("Envoyé : " + frame);
                serialPort.WriteLine(frame);
                label5.Text = "";
            }
            else MessageBox.Show("Data array not completed, please complete it before saving datas");
        }

        /*private void button4_Click(object sender, EventArgs e)
        {
            label5.Text = "";
            read_accessory_list();
        }*/

        private void button5_Click(object sender, EventArgs e)
        {
            string frame = "";
            timer3.Enabled = true;
            try_connect = true;

            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.PortName = comboBox2.Text;
                    serialPort.Open();
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    //listBox1.Items.Add("Connected succefully");
                    frame = create_start_frame();
                    serialPort.WriteLine(frame);
                    button5.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                try_connect = false;
            }
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            if (form_gestion_xml)
            {
                //listBox1.Items.Add("Entered (refresh list)");
                label5.Text = "";
                read_accessory_list();
                form_gestion_xml = false;
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            label5.Text = "";
            selected_accessory_index = comboBox1.SelectedIndex;
            //previous_path = acc_path_list[selected_accessory_index];
            read_xml(false);
            read_cells_compare();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            label5.Text = "";
            if (dataGridView1.CurrentCell.RowIndex == 6 || dataGridView1.CurrentCell.RowIndex == 7)
            {
                cellRow = (byte)(dataGridView1.CurrentCell.RowIndex);
                cellColumn = (byte)(dataGridView1.CurrentCell.ColumnIndex);
                monthCalendar1.Visible = true;
            }
            else monthCalendar1.Visible = false;
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            label5.Text = "";
            //listBox1.Items.Add("selected");
            monthCalendar1.Visible = false;
            //listBox1.Items.Add(monthCalendar1.SelectionRange.Start.ToShortDateString());
            //string date = Convert.ToString(System.Text.Encoding.ASCII.GetBytes(dataGridView1[dataGridView1.CurrentCell.ColumnIndex, dataGridView1.CurrentCell.RowIndex].Value))
            string sdate = monthCalendar1.SelectionRange.Start.ToShortDateString();
            byte[] bdate = new byte[10];
            bdate = System.Text.Encoding.ASCII.GetBytes(sdate);
            sdate = "";
            bdate[6] = bdate[8];
            bdate[7] = bdate[9];
            bdate[8] = 0;
            bdate[9] = 0;
            for (int i = 0; i < 8; i++)
            {

                sdate += Convert.ToString(bdate[i] - 48);
                if (i == 1 || i == 4)
                {
                    sdate += "/";
                    i++;
                }
            }
            dataGridView1[cellColumn, cellRow].Value = sdate;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            button5.Enabled = true;
        }

        private void comboBox2_Enter(object sender, EventArgs e)
        {
            get_com_list();
            timer2.Enabled = true;
        }

        private void comboBox2_Leave(object sender, EventArgs e)
        {
            timer2.Enabled = false;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            get_com_list();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            if (try_connect)
            {
                if (cpt_frame_send < 5)
                {
                    if (cpt_timeout <= 10)
                    {
                        //listBox2.Items.Add(cpt_timeout);
                        cpt_timeout++;
                    }
                    else
                    {
                        string frame = "";
                        frame = create_start_frame();
                        serialPort.WriteLine(frame);
                        cpt_frame_send++;
                    }
                }
                else
                {
                    cpt_frame_send = 0;
                    cpt_timeout = 0;
                    try_connect = false;
                    serialPort.Close();
                    Form3 form3 = new Form3(21);
                    form3.ShowDialog();
                    button5.Enabled = true;
                }
            }
        }

        /***********************************************************************************************************************************************************/
        // Frame construction
        /***********************************************************************************************************************************************************/

        private string create_start_frame()
        {
            string frame = "";

            bufferE[0] = 0x4D;
            bufferE[1] = 0x04;
            bufferE[2] = 0x06;
            bufferE[3] = get_CRC(crc_emission);
            for (int i = 0; i < 4; i++)
            {
                frame += Convert.ToString(bufferE[i]);
                if (i == 0)
                    frame += '>';
                else if (i + 1 < bufferE[1])
                    frame += ';';
                else frame += "!";
            }
            return frame;
        }

        private string create_read_frame()
        {
            string frame = "";

            bufferE[0] = 0x4D;
            bufferE[1] = 0x04;
            bufferE[2] = 0x01;
            bufferE[3] = get_CRC(crc_emission);
            for (int i = 0; i < 4; i++)
            {
                frame += Convert.ToString(bufferE[i]);
                if (i == 0)
                    frame += '>';
                else if (i + 1 < bufferE[1])
                    frame += ';';
                else frame += "!";
            }
            return frame;
        }

        private string create_write_frame()
        {
            string frame = "";

            switch (type_trame_data_write())
            {
                case 1:
                    frame = create_write_all_data_frame();
                    break;

                case 2:
                    frame = create_write_coeff_frame();
                    break;

                case 3:
                    frame = create_write_ID_frame();
                    break;

                case 4:
                    frame = create_write_def_datas_frame();
                    break;

                default:
                    listBox1.Items.Add("Aucune modification");
                    break;
            }
            return frame;
        }

        private string create_write_all_data_frame()
        {
            string frame = "";

            bufferE[0] = 0x4D;
            bufferE[1] = 29;
            bufferE[2] = 0x02;
            bufferE[3] = 0x01;

            for (int i = 0; i < 2; i++) bufferE[4 + i] = accessoire.Correction_Coeff_data[i];
            for (int i = 0; i < 2; i++) bufferE[6 + i] = accessoire.date_calibration[i];
            for (int i = 0; i < 4; i++) bufferE[8 + i] = 0x00;
            for (int i = 0; i < 8; i++) bufferE[12 + i] = accessoire.ID[i];
            for (int i = 0; i < 8; i++) bufferE[20 + i] = accessoire.def[i];

            bufferE[28] = get_CRC(crc_emission);
            for (int i = 0; i < bufferE[1]; i++)
            {
                frame += Convert.ToString(bufferE[i]);
                if (i == 0)
                    frame += '>';
                else if (i + 1 < bufferE[1])
                    frame += ';';
                else frame += "!";
            }
            return frame;
        }

        private string create_write_coeff_frame()
        {
            string frame = "";

            bufferE[0] = 0x4D;
            bufferE[1] = 13;
            bufferE[2] = 0x02;
            bufferE[3] = 0x02;

            for (int i = 0; i < 2; i++) bufferE[4 + i] = accessoire.Correction_Coeff_data[i];
            for (int i = 0; i < 2; i++) bufferE[6 + i] = accessoire.date_calibration[i];
            for (int i = 0; i < 4; i++) bufferE[8 + i] = 0x00;

            bufferE[12] = get_CRC(crc_emission);
            for (int i = 0; i < bufferE[1]; i++)
            {
                frame += Convert.ToString(bufferE[i]);
                if (i == 0)
                    frame += '>';
                else if (i + 1 < bufferE[1])
                    frame += ';';
                else frame += "!";
            }
            return frame;
        }

        private string create_write_ID_frame()
        {
            string frame = "";

            bufferE[0] = 0x4D;
            bufferE[1] = 13;
            bufferE[2] = 0x02;
            bufferE[3] = 0x03;

            for (int i = 0; i < 8; i++) bufferE[4 + i] = accessoire.ID[i];

            bufferE[12] = get_CRC(crc_emission);
            for (int i = 0; i < bufferE[1]; i++)
            {
                frame += Convert.ToString(bufferE[i]);
                if (i == 0)
                    frame += '>';
                else if (i + 1 < bufferE[1])
                    frame += ';';
                else frame += "!";
            }
            return frame;
        }

        private string create_write_def_datas_frame()
        {
            string frame = "";

            bufferE[0] = 0x4D;
            bufferE[1] = 13;
            bufferE[2] = 0x02;
            bufferE[3] = 0x04;

            for (int i = 0; i < 8; i++) bufferE[4 + i] = accessoire.def[i];

            bufferE[12] = get_CRC(crc_emission);
            for (int i = 0; i < bufferE[1]; i++)
            {
                frame += Convert.ToString(bufferE[i]);
                if (i == 0)
                    frame += '>';
                else if (i + 1 < bufferE[1])
                    frame += ';';
                else frame += "!";
            }
            return frame;
        }

        private byte get_CRC(bool reception)
        {
            long sum_crc = 0;
            if (reception)
            {
                for (int i = 0; i < bufferR[1] - 1; i++)
                    sum_crc += bufferR[i];
            }

            else
            {
                for (int i = 0; i < bufferE[1] - 1; i++)
                    sum_crc += bufferE[i];
            }
            return (byte)(sum_crc % 256);
        }

        /***********************************************************************************************************************************************************/
        // Get and analyse reception frames
        /***********************************************************************************************************************************************************/

        void get_rallonge_data()
        {
            if (bufferR[2] == 3)
            {
                //printf("ACK, OK\nCoeff: ");
                for (int i = 0; i < 2; i++)
                {
                    accessoire.Correction_Coeff_data[i] = (byte)bufferR[3 + i];
                    //printf("%x.",rall->coeff[i]);
                }

                for (int i = 0; i < 6; i++)
                {
                    accessoire.date_calibration[i] = (byte)bufferR[5 + i];
                }

                for (int i = 0; i < 8; i++)
                {
                    //accessoire[typeRallonge].nameCode[i] = buffReception[15 + i];
                    accessoire.ID[i] = (byte)bufferR[11 + i];
                }

                for (int i = 0; i < 8; i++)
                {
                    accessoire.def[i] = (byte)bufferR[19 + i];
                }

                calcul_rallonge_data();
                deconcatener();
                display_data();
            }
            else ; //printf("renvoyer trame à implémenter");
            //printf("\n");
        }

        void deconcatener()
        {
            uint buffM = 0;
            uint buffL = 0;

            buffM = (uint)((accessoire.ID[0] << 24));
            buffM += (uint)((accessoire.ID[1] << 16));
            buffM += (uint)((accessoire.ID[2] << 8));
            buffM += (uint)((accessoire.ID[3] << 0));
            buffL = (uint)((accessoire.ID[4] << 24));
            buffL += (uint)((accessoire.ID[5] << 16));
            buffL += (uint)((accessoire.ID[6] << 8));
            buffL += (uint)((accessoire.ID[7] << 0));

            try
            {
                accessoire.user_ID[0] = (byte)((buffM & 0xFC000000) >> 26);
                accessoire.user_ID[1] = (byte)((buffM & 0x03F00000) >> 20);
                accessoire.user_ID[2] = (byte)((buffM & 0x000FC000) >> 14);
                accessoire.user_ID[3] = (byte)((buffM & 0x00003F00) >> 8);
                accessoire.user_ID[4] = (byte)((buffM & 0x000000FC) >> 2);
                accessoire.user_ID[5] = (byte)((buffM & 0x00000003) << 4);

                accessoire.user_ID[5] += (byte)((buffL & 0xF0000000) >> 28);
                accessoire.user_ID[6] = (byte)((buffL & 0x0FC00000) >> 22);
                accessoire.user_ID[7] = (byte)((buffL & 0x003F0000) >> 16);
                accessoire.user_ID[8] = (byte)((buffL & 0x0000FC00) >> 10);
                accessoire.user_ID[9] = (byte)((buffL & 0x000003F0) >> 4);
                accessoire.Level = (byte)(buffL & 0x0000000F);
            }
            catch (Exception ex)
            {
                listBox2.Items.Add(ex);
            }

            decoder_name();
        }

        void decoder_name()
        {
            for (int i = 0; i < accessoire.user_ID.Length; i++)
            {
                accessoire.user_ID[i] = (byte)(accessoire.user_ID[i] + 32);
            }
        }

        void clear_bufferR()
        {
            for (int i = 0; i < bufferR.Length; i++)
            {
                bufferR[i] = 0;
            }
        }

        /***********************************************************************************************************************************************************/
        // Get and verify array datas
        /***********************************************************************************************************************************************************/

        void read_array()
        {
            string ID_string = "";
            char[] test = new char[10];

            ID_string = Convert.ToString(dataGridView1[1, 0].Value);
            accessoire.user_ID = System.Text.Encoding.ASCII.GetBytes(ID_string);
            test = ID_string.ToCharArray();
            for (int i = 0; i < test.Length; i++) accessoire.user_ID[i] = Convert.ToByte(test[i]);
            //listBox1.Items.Add("string = " + ID_string + ", ID : " + accessoire.user_ID[0] + accessoire.user_ID[1] + accessoire.user_ID[2] + accessoire.user_ID[3]);
            //listBox1.Items.Add("string = " + ID_string + ", ID : " + test[0] + test[1] + test[2] + test[3]);

            accessoire.user_ID = System.Text.Encoding.ASCII.GetBytes(test);
            accessoire.Level = Convert.ToByte(dataGridView1[1, 1].Value);
            accessoire.Max_Torque = Convert.ToUInt16(dataGridView1[1, 2].Value);
            accessoire.Correction_Coeff = Convert.ToDouble(dataGridView1[1, 3].Value);
            accessoire.Length = Convert.ToUInt16(dataGridView1[1, 4].Value);
            accessoire.Flexion_Coeff = Convert.ToUInt16(dataGridView1[1, 5].Value);
            
        }

        public void read_cells_compare()
        {

            for (int i = 0; i < 8; i++)
            {
                cell_compare[i] = Convert.ToString(dataGridView1[1, i].Value);
                //listBox2.Items.Add("["+i+","+j+"]"+cell[i, j]);
            }
        }

        public void read_cells()
        {
            //flagFormatDataOk = true;

            read_date();

                //listBox1.Items.Add("coeff: " + cell[1 + i, 4]);
                try
                {
                    read_array();
                }
                catch (Exception e)
                {
                    flagFormatDataOk = false;
                    //listBox1.Items.Add("step 0");
                    //listBox2.Items.Add("I: " + i + " - " + "Read call :  " + e);
                    //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                    //toolTip1.Show("Erreur dans la saisie des données", this, MousePosition.X, MousePosition.Y, 2000);
                    Form3 form3 = new Form3(0);
                    listBox1.Items.Add("read_cells");
                    form3.ShowDialog();
                    display_data();
                }

                    try
                {

                    if (accessoire.user_ID.Length > 10)
                    {
                        string ID_string = Convert.ToString(cell_compare[0]);
                        accessoire.user_ID = System.Text.Encoding.ASCII.GetBytes(ID_string);
                        flagFormatDataOk = false;
                        Form3 form3 = new Form3(1);
                        form3.ShowDialog();

                    }
                    else if (accessoire.user_ID.Length < 10)
                    {
                        for (int j = 9; j > accessoire.user_ID.Length; j--)
                        {
                            //se fait "automatiquement"
                            //verification_format_ID[i] = 32;
                        }
                    }
                    else if (accessoire.Level > 2 || accessoire.Level < 1)
                    {
                        accessoire.Level = Convert.ToByte(cell_compare[1]);
                        flagFormatDataOk = false;
                        Form3 form3 = new Form3(2);
                        form3.ShowDialog();
                        //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                        //toolTip1.Show("Level 1 pour une rallonge et level 2 pour un cliquet", this, MousePosition.X, MousePosition.Y, 2000);

                        /*if (verification_format_level > 2) verification_format_level = 2;
                        else verification_format_level = 1;*/

                    }
                    else if (accessoire.Correction_Coeff < 0.1)
                    {
                        //verification_format_coeff = accessoire[i].user_coeff;//Coeff négatif NOK
                        accessoire.Correction_Coeff = Convert.ToDouble(cell_compare[3]);
                        flagFormatDataOk = false;
                        Form3 form3 = new Form3(3);
                        form3.ShowDialog();
                        //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                        //toolTip1.Show("Veuillez rentrer un coefficient compris entre 0,1 et 10", this, MousePosition.X, MousePosition.Y, 2000);
                    }

                    else if (accessoire.Correction_Coeff > 10)
                    {
                        //verification_format_coeff = accessoire[i].user_coeff;//Coeff négatif NOK
                        accessoire.Correction_Coeff = Convert.ToDouble(cell_compare[3]);
                        flagFormatDataOk = false;
                        Form3 form3 = new Form3(3);
                        form3.ShowDialog();
                        //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                        //toolTip1.Show("Veuillez rentrer un coefficient compris entre 0,1 et 10", this, MousePosition.X, MousePosition.Y, 2000);
                    }

                    else if (accessoire.Max_Torque > 2000)
                    {
                        //verification_format_coeff = accessoire[i].user_coeff;//Coeff négatif NOK
                        accessoire.Max_Torque = Convert.ToUInt16(cell_compare[2]);
                        flagFormatDataOk = false;
                        Form3 form3 = new Form3(4);
                        form3.ShowDialog();
                        //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                        //toolTip1.Show("Veuillez rentrer un couple max inférieur à 2000 N.m", this, MousePosition.X, MousePosition.Y, 2000);
                    }

                    else if (accessoire.Length > 4000)
                    {
                        //verification_format_coeff = accessoire[i].user_coeff;//Coeff négatif NOK
                        accessoire.Length = Convert.ToUInt16(cell_compare[4]);
                        flagFormatDataOk = false;
                        Form3 form3 = new Form3(5);
                        form3.ShowDialog();
                        //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                        //toolTip1.Show("Veuillez rentrer une longueur inférieur à 4 m", this, MousePosition.X, MousePosition.Y, 2000);
                    }
                        }
                catch (Exception e)
                {
                    flagFormatDataOk = false;
                    //listBox1.Items.Add("step 1");
                    //listBox2.Items.Add("I: " + i + " - " + "Read call :  " + e);
                    //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                    //toolTip1.Show("Erreur dans la saisie des données", this, MousePosition.X, MousePosition.Y, 2000);
                    Form3 form3 = new Form3(0);
                listBox1.Items.Add("read_cells (try/ catch");
                form3.ShowDialog();
                    display_data();
                }

                    /*accessoire.user_ID = verification_format_ID;
                    //listBox1.Items.Add("I: " + i + " - " + "Read call, ID: " + verification_format_ID);
                    accessoire[i].level = verification_format_level;
                    //listBox1.Items.Add("I: " + i + " - " + "Read call, Level: " + verification_format_level);
                    //listBox2.Items.Add("level " + i + ": " + accessoire[i].level);
                    accessoire[i].user_torque = verification_format_torque;
                    //listBox1.Items.Add("I: " + i + " - " + "Read call, torque: " + verification_format_torque);
                    accessoire[i].user_coeff = verification_format_coeff;
                    //listBox1.Items.Add("I: " + i + " - " + "Read call, coeff: " + verification_format_coeff);
                    accessoire[i].user_length = verification_format_length;
                    //listBox1.Items.Add("I: " + i + " - " + "Read call, longueur: " + verification_format_length);
                    accessoire[i].user_flexion = verification_format_flexion;
                    //listBox1.Items.Add("I: " + i + " - " + "Read call, flexion: " + accessoire[i].user_flexion);*/


                        try
                {
                    coder_name();
                    //listBox2.Items.Add("step 1");
                            }
                catch (Exception e)
                {
                    flagFormatDataOk = false;
                    //listBox1.Items.Add("step 2");
                    //listBox2.Items.Add("I: " + i + " - " + "Read call :  " + e);
                    //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                    //toolTip1.Show("Erreur dans la saisie des données", this, MousePosition.X, MousePosition.Y, 2000);
                    Form3 form3 = new Form3(0);
                listBox1.Items.Add("read_cells (try/ catch 2)");
                form3.ShowDialog();
                    display_data();
                }
               //             try
                //{
                    concatener();
                    //listBox2.Items.Add("step 2");
                                /*}
                catch (Exception e)
                {
                    flagFormatDataOk = false;
                    listBox1.Items.Add("step 3");
                    //listBox2.Items.Add("I: " + i + " - " + "Read call :  " + e);
                    //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                    //toolTip1.Show("Erreur dans la saisie des données", this, MousePosition.X, MousePosition.Y, 2000);
                    Form3 form3 = new Form3(0);
                    form3.ShowDialog();
                    display_data();
                }*/
                                try
                {
                    decoder_name();
                    //listBox2.Items.Add("step 3");
                                    }
                catch (Exception e)
                {
                    flagFormatDataOk = false;
                    //listBox1.Items.Add("step 4");
                    //listBox2.Items.Add("I: " + i + " - " + "Read call :  " + e);
                    //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                    //toolTip1.Show("Erreur dans la saisie des données", this, MousePosition.X, MousePosition.Y, 2000);
                    Form3 form3 = new Form3(0);
                listBox1.Items.Add("read_cells (try/ catch 3)");
                form3.ShowDialog();
                    display_data();
                }
                                    try
                {
                    formating_accessory_datas();
                    //listBox2.Items.Add("step 4");
                }
                                    catch (Exception e)
                                    {
                                        flagFormatDataOk = false;
                                        //listBox1.Items.Add("step 5");
                                        //listBox2.Items.Add("I: " + i + " - " + "Read call :  " + e);
                                        //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                                        //toolTip1.Show("Erreur dans la saisie des données", this, MousePosition.X, MousePosition.Y, 2000);
                                        Form3 form3 = new Form3(0);
                listBox1.Items.Add("read_cells (try/ catch 4)");
                form3.ShowDialog();
                                        display_data();
                                    }
                                        try
                {
                    calcul_rallonge_data();
                    display_data();
                    //listBox2.Items.Add("step 5");

                }
                catch (Exception e)
                {
                    flagFormatDataOk = false;
                    //listBox1.Items.Add("step 6");
                    //listBox2.Items.Add("I: " + i + " - " + "Read call :  " + e);
                    //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                    //toolTip1.Show("Erreur dans la saisie des données", this, MousePosition.X, MousePosition.Y, 2000);
                    Form3 form3 = new Form3(0);
                listBox1.Items.Add("read_cells (try/ catch 5)");
                form3.ShowDialog();
                    display_data();
                }
            }

        public void read_date()
        {
            byte monthFormat = 0;
            byte[] date = new byte[16];
            byte day = 0;
            byte month = 0;
            byte year = 0;
            for (byte i = 0; i < 2; i++)
            {
                string date_string = Convert.ToString(dataGridView1[1, 6 + i].Value);
                date = System.Text.Encoding.ASCII.GetBytes(date_string);
                try
                {
                    if (date[4] == 47) monthFormat = 7; // mois sur 1 ou 2 charactères (regarde emplacement séparateur '/')
                    else monthFormat = 8;
                    for (int j = 0; j < monthFormat; j++)
                    {
                        //listBox2.Items.Add("format : " + Convert.ToString(monthFormat) + ", j = " + j);
                        //listBox2.Items.Add(Convert.ToString(date[i]));

                        date[j] = (byte)(date[j] - 48);
                    }
                    day = (byte)((date[0] * 10) + date[1]);
                    if (monthFormat == 7)
                    {
                        month = (byte)date[3];
                        year = (byte)((date[5] * 10) + date[6]);
                    }
                    else
                    {
                        month = (byte)((date[3] * 10) + date[4]);
                        year = (byte)((date[6] * 10) + date[7]);
                    }

                    format_date(0, i, day, month, year);

                }
                catch (Exception e)
                {
                    listBox2.Items.Add(e);
                }
            }
        }

        public void format_date(byte typeRallonge, byte typeDate, byte day, byte month, byte year)
        {
            switch (month)
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    if (day > 31)
                    {
                        flagFormatDataOk = false;
                        //listBox2.Items.Add("Jour innexistant");
                        //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                        //toolTip1.Show("Ce jour n'existe pas", this, MousePosition.X, MousePosition.Y, 2000);
                        Form3 form3 = new Form3(6);
                        form3.ShowDialog();
                        display_data();
                    }
                    else
                    {
                        if (typeDate == 0)
                        {
                            accessoire.day = day;
                            accessoire.month = month;
                            accessoire.year = year;
                        }
                        else
                        {
                            accessoire.dayCalibration = day;
                            accessoire.monthCalibration = month;
                            accessoire.yearCalibration = year;
                        }
                    }
                    break;
                case 4:
                case 6:
                case 9:
                case 11:
                    if (day > 30)
                    {
                        flagFormatDataOk = false;
                        //listBox2.Items.Add("Jour innexistant");
                        //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                        //toolTip1.Show("Ce jour n'existe pas", this, MousePosition.X, MousePosition.Y, 2000);
                        Form3 form3 = new Form3(6);
                        form3.ShowDialog();
                        display_data();
                    }
                    else
                    {
                        if (typeDate == 0)
                        {
                            accessoire.day = day;
                            accessoire.month = month;
                            accessoire.year = year;
                        }
                        else
                        {
                            accessoire.dayCalibration = day;
                            accessoire.monthCalibration = month;
                            accessoire.yearCalibration = year;
                        }
                    }
                    break;
                case 2:
                    if ((day > 28 && year % 4 != 0) || day > 29 && year % 4 == 0)
                    {
                        flagFormatDataOk = false;
                        //listBox2.Items.Add("Jour innexistant");
                        //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                        //toolTip1.Show("Ce jour n'existe pas", this, MousePosition.X, MousePosition.Y, 2000);
                        Form3 form3 = new Form3(6);
                        form3.ShowDialog();
                        display_data();
                    }
                    else
                    {
                        if (typeDate == 0)
                        {
                            accessoire.day = day;
                            accessoire.month = month;
                            accessoire.year = year;
                        }
                        else
                        {
                            accessoire.dayCalibration = day;
                            accessoire.monthCalibration = month;
                            accessoire.yearCalibration = year;
                        }
                    }
                    break;

                default:
                    flagFormatDataOk = false;
                    //listBox2.Items.Add("Mois innexistant");
                    //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                    //toolTip1.Show("Ce mois n'existe pas", this, MousePosition.X, MousePosition.Y, 2000);
                    Form3 formulaire3 = new Form3(6);
                    formulaire3.ShowDialog();
                    display_data();
                    break;
            }
        }

        void coder_name()//variables utilisées "ID" correspond à name de la rallonge
        {
            //printf("Char:%s\n",ID);
            /*for (int i = 0; i < accessoire[typeRallonge].name.Length; i++)
            {
                if (accessoire[typeRallonge].name[i] != 0)
                accessoire[typeRallonge].name[i] = (byte)(accessoire[typeRallonge].name[i] - 32);
                if(accessoire[typeRallonge].name[i]>=65 && accessoire[typeRallonge].name[i] <= 90)//si entre a et z
                {
                    accessoire[typeRallonge].name[i] -= 32;//on le passe en majuscule
                    
                }
                //listBox2.Items.Add("name: " + accessoire[typeRallonge].name[i]);
            }*/

            for (int i = 0; i < accessoire.user_ID.Length; i++)
            {
                if (accessoire.user_ID[i] != 0) //prévoir ID < 10 charactères
                    //listBox2.Items.Add("Longueur ID: " + accessoire[typeRallonge].user_ID.Length);
                    accessoire.user_ID[i] = (byte)(accessoire.user_ID[i] - 32);
                if (accessoire.user_ID[i] >= 65 && accessoire.user_ID[i] <= 90)//si entre a et z
                {
                    accessoire.user_ID[i] -= (byte)32;//on le passe en majuscule
                }
            }
        }

        void concatener()
        {
            uint buffM = 0;
            uint buffL = 0;

            if (accessoire.user_ID.Length > 0)
                buffM += (uint)(accessoire.user_ID[0] & 0x3F) << 26;
            if (accessoire.user_ID.Length > 1)
                buffM += (uint)(accessoire.user_ID[1] & 0x3F) << 20;
            if (accessoire.user_ID.Length > 2)
                buffM += (uint)(accessoire.user_ID[2] & 0x3F) << 14;
            if (accessoire.user_ID.Length > 3)
                buffM += (uint)(accessoire.user_ID[3] & 0x3F) << 8;
            if (accessoire.user_ID.Length > 4)
                buffM += (uint)(accessoire.user_ID[4] & 0x3F) << 2;

            if (accessoire.user_ID.Length > 5)
                buffL += (uint)(accessoire.user_ID[5] & 0x3F) << 26;
            if (accessoire.user_ID.Length > 6)
                buffL += (uint)(accessoire.user_ID[6] & 0x3F) << 20;
            if (accessoire.user_ID.Length > 7)
                buffL += (uint)(accessoire.user_ID[7] & 0x3F) << 14;
            if (accessoire.user_ID.Length > 8)
                buffL += (uint)(accessoire.user_ID[8] & 0x3F) << 8;
            if (accessoire.user_ID.Length > 9)
                buffL += (uint)(accessoire.user_ID[9] & 0x3F) << 2;

            if (((buffL % 2147483648) - (buffL - 2147483648)) == 0)
            {
                buffM += 2;
                buffL -= 2147483648;
            }

            if (((buffL % 1073741824) - (buffL - 1073741824)) == 0)
            {
                buffM += 1;
                buffL -= 1073741824;
            }
            buffL = buffL << 2;

            accessoire.ID[0] = (byte)((buffM & 0xFF000000) >> 24);
            accessoire.ID[1] = (byte)((buffM & 0x00FF0000) >> 16);
            accessoire.ID[2] = (byte)((buffM & 0x0000FF00) >> 8);
            accessoire.ID[3] = (byte)((buffM & 0x000000FF) >> 0);

            accessoire.ID[4] = (byte)((buffL & 0xFF000000) >> 24);
            accessoire.ID[5] = (byte)((buffL & 0x00FF0000) >> 16);
            accessoire.ID[6] = (byte)((buffL & 0x0000FF00) >> 8);
            accessoire.ID[7] = (byte)((buffL & 0x000000FF) >> 0);
            accessoire.ID[7] += accessoire.Level;
            listBox2.Items.Add("Level = " + accessoire.Level);
        }

        public void formating_accessory_datas()// Regler PB modulo Torc et Coeff (tester 256 dans torc)
        {

            uint coeff_temp = (uint)(accessoire.Correction_Coeff * 5000);

            //listBox2.Items.Add("Coeff temp: " + coeff_temp);
            uint torque_temp = (uint)(accessoire.Max_Torque * 10);
            accessoire.Correction_Coeff_data[0] = (byte)((coeff_temp & 0xFF00) >> 8);
            accessoire.Correction_Coeff_data[1] = (byte)(coeff_temp & 0x00FF);
            accessoire.def[0] = (byte)((accessoire.Length & 0xFF00) / 256);
            accessoire.def[1] = (byte)(accessoire.Length & 0x00FF);
            //listBox2.Items.Add("Coeff tab: " + accessoire[i].coeff[0] + "  -  " + accessoire[i].coeff[1]);
            /*accessoire[i].def[2] = (byte)(((accessoire[i].user_torque & 0xFF00) *10)/ 256);
            accessoire[i].def[3] = (byte)((accessoire[i].user_torque & 0x00FF) * 10);*/
            accessoire.def[2] = (byte)((torque_temp & 0xFF00) / 256);
            accessoire.def[3] = (byte)(torque_temp & 0x00FF);
            accessoire.def[6] = (byte)((accessoire.Flexion_Coeff & 0xFF00) / 256);
            accessoire.def[7] = (byte)(accessoire.Flexion_Coeff & 0x00FF);
            accessoire.def[4] = (byte)(accessoire.year << 1);
            if (accessoire.month >= 8)
            {
                accessoire.def[4] += 1;
            }
            accessoire.def[5] = (byte)((accessoire.month & 0x07) << 5);
            accessoire.def[5] += (byte)(accessoire.day & 0x1F);

            accessoire.date_calibration[0] = (byte)(accessoire.yearCalibration << 1);
            if (accessoire.monthCalibration >= 8)
            {
                accessoire.date_calibration[0] += 1;
            }
            accessoire.date_calibration[1] = (byte)((accessoire.monthCalibration & 0x07) << 5);
            accessoire.date_calibration[1] += (byte)(accessoire.dayCalibration & 0x1F);
            //}
            //listBox2.Items.Add("Coeff: " + accessoire[0].def[2] + "  -  " + accessoire[0].def[3]);
        }

        void calcul_rallonge_data()
        {
            accessoire.Correction_Coeff = accessoire.Correction_Coeff * 5000;
            //listBox2.Items.Add("Coeff" + typeRallonge + ": " + accessoire[typeRallonge].coeff[0] + "  -  " + accessoire[typeRallonge].coeff[1] + " = " + accessoire[typeRallonge].user_coeff);
            accessoire.Correction_Coeff = (double)((double)((uint)(accessoire.Correction_Coeff_data[0] * 256) + accessoire.Correction_Coeff_data[1]) / 5000);
            accessoire.Length = (uint)(accessoire.def[0] * 256 + accessoire.def[1]);
            accessoire.Max_Torque = (uint)(((accessoire.def[2] * 256) + accessoire.def[3]) / 10);
            accessoire.Flexion_Coeff = (uint)(accessoire.def[6] * 256 + accessoire.def[7]);
            //listBox2.Items.Add("Coeff"+ typeRallonge+ ": " + accessoire[typeRallonge].coeff[0] + "  -  " + accessoire[typeRallonge].coeff[1] + " = " + accessoire[typeRallonge].user_coeff);
            accessoire.year = accessoire.def[4];
            accessoire.month = 0;
            if ((byte)(accessoire.def[4] & 0x01) == 1)
            {
                accessoire.month = 8;
            }
            accessoire.year = ((byte)(accessoire.year >> 1));
            accessoire.month += ((byte)(accessoire.def[5] >> 5));
            accessoire.day = (byte)(accessoire.def[5] & 0x1F);

            accessoire.yearCalibration = accessoire.date_calibration[0];
            accessoire.monthCalibration = 0;
            if ((byte)(accessoire.date_calibration[0] & 0x01) == 1)
            {
                accessoire.monthCalibration = 8;
            }
            accessoire.yearCalibration = ((byte)(accessoire.yearCalibration >> 1));
            accessoire.monthCalibration += ((byte)(accessoire.date_calibration[1] >> 5));
            accessoire.dayCalibration = (byte)(accessoire.date_calibration[1] & 0x1F);
        }

        public byte type_trame_data_write()
        {
            bool write_coeff = false;
            bool write_ID = false;
            bool write_def = false;

            if (cell_compare[0] != Convert.ToString(dataGridView1[1,0].Value)
                || cell_compare[1] != Convert.ToString(dataGridView1[1,1].Value))
            {
                write_ID = true;
            }

            if (cell_compare[3] != Convert.ToString(dataGridView1[1, 3].Value)
                || cell_compare[7] != Convert.ToString(dataGridView1[1, 7].Value))
            {
                write_coeff = true;
            }

            if (cell_compare[2] != Convert.ToString(dataGridView1[1, 2].Value)
                || cell_compare[4] != Convert.ToString(dataGridView1[1, 4].Value)
                || cell_compare[5] != Convert.ToString(dataGridView1[1, 5].Value)
                || cell_compare[6] != Convert.ToString(dataGridView1[1, 6].Value))
            {
                write_def = true;
            }

            /*if (write_ID && write_coeff || write_ID && write_def || write_coeff && write_def) return 1;
            else if (write_coeff) return 2;
            else if (write_ID) return 3;
            else if (write_def) return 4;*/ //Permet de choisir le type de trame d'écriture (ne réecrit qu'un octet si besoin) pas implémenté car non écriture de la puce par fichier xml 
            //else return 0;
            return 1;
        }

        /***********************************************************************************************************************************************************/
        // Read XML
        /***********************************************************************************************************************************************************/

        private bool read_xml(bool compare)
        {
            try
            {
                string buffer_reception;
                
                StreamReader sr = new StreamReader(acc_path_list[selected_accessory_index] + "Accessory " + acc_name_list[selected_accessory_index] + ".xml");
                buffer_reception = sr.ReadToEnd();
                //bufferR = buffer_reception.ToCharArray();
                bufferR_convert = buffer_reception.ToCharArray();

                maskedTextBox1.Text = buffer_reception;
                //sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception ex)
            {
                listBox2.Items.Add("fichier introuvable");
            }

            if (compare)
            {
                return compare_data_from_xml(); 
            }
            else
            {
                get_data_from_xml();
                maskedTextBox1.Text = "";
                return false;
            }
        }

        private void get_data_from_xml()
        {
            string dataName = "";
            string data = "";

            for (int i = 0; i < bufferR_convert.Length; i++)
            {
                if (bufferR_convert[i] == '<' && bufferR_convert[i + 1] != '/' && bufferR_convert[i + 1] <= 90 && bufferR_convert[i + 1] >= 65)
                {
                    i++;
                    while (bufferR_convert[i] != '>')
                    {
                        dataName += bufferR_convert[i];
                        i++;
                    }

                    i++;

                    while (bufferR_convert[i] != '<')
                    {
                        data += bufferR_convert[i];
                        i++;
                    }

                    char[] ID_char_array = new char[10];
                    switch (dataName)
                    {
                        case "ID":
                            //accessoire.ID = System.Text.Encoding.ASCII.GetChar(data);
                            ID_char_array = data.ToCharArray();
                            //for (int j = 0 ; i < 10 ; i++) ID_char_array[j] = ID_char_array[j] - 
                            accessoire.user_ID = System.Text.Encoding.ASCII.GetBytes(data);
                            break;

                        case "Level":
                            accessoire.Level = Convert.ToByte(data);
                            break;

                        case "Max_Torque":
                            accessoire.Max_Torque = Convert.ToUInt16(data);
                            break;

                        case "Correction_Coeff":
                            accessoire.Correction_Coeff = Convert.ToDouble(data);
                            break;

                        case "Length":
                            accessoire.Length = Convert.ToUInt16(data);
                            break;

                        case "Flexion_Coeff":
                            accessoire.Flexion_Coeff = Convert.ToUInt16(data);
                            break;

                        default:
                            break;
                    }

                    dataName = data = "";

                    //maskedTextBox1.Text = dataName;
                }
            }
            display_data_xml();
        }

        private void read_accessory_list()
        {
            string buffer_all_the_file;
            string index = "";

            for (int i = 0; acc_name_list[i] != null && acc_path_list[i] != null; i++)
                acc_path_list[i] = acc_name_list[i] = "";

            try
            {
                StreamReader sr = new StreamReader("./" + "Accessory_list.txt");
                //listBox1.Items.Add();
            
            buffer_all_the_file = sr.ReadToEnd();
            sr.Close();
            bufferR_xml = buffer_all_the_file.ToCharArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                StreamWriter sw = new StreamWriter("./" + "Accessory_list.txt");
                sw.WriteLine("~");
                sw.Close();
            }
            //listBox1.Items.Add(buffer_all_the_file);
            
            /*int i = 0;
            int j = 0;
            int k = 0;*/
            int file_length = 0;

            for (int i = 0; bufferR_xml[i] != '~'; i++)
            {
                file_length = i;
            }
            //listBox1.Items.Add(file_length);

            for (int i = 0; i < file_length - 1; i++)
            {
                for (i = i; bufferR_xml[i] != ';'; i++)
                {
                    index += bufferR_xml[i];
                }
                i++;

                for (i = i; bufferR_xml[i] != ';'; i++)
                {
                    acc_name_list[Convert.ToInt16(index)] += bufferR_xml[i];
                }
                //listBox1.Items.Add("index : " + Convert.ToInt16(index) + ", name : " + acc_name_list[Convert.ToInt16(index)]);
                i++;

                for (i = i; bufferR_xml[i] != ';'; i++)
                {
                    acc_path_list[Convert.ToInt16(index)] += bufferR_xml[i];
                }
                index = "";
            }
            display_data_combobox();
        }

        /***********************************************************************************************************************************************************/
        // Affichage
        /***********************************************************************************************************************************************************/

        private void display_data()
        {
            string display_ID = "";
            for (int i = 0; i < accessoire.user_ID.Length ; i++) display_ID += Convert.ToString(accessoire.user_ID[i]);

            dataGridView1.Rows[0].SetValues("ID", Encoding.Default.GetString(accessoire.user_ID));
            dataGridView1.Rows[1].SetValues("Level", accessoire.Level);
            dataGridView1.Rows[2].SetValues("Max torque", accessoire.Max_Torque);
            dataGridView1.Rows[3].SetValues("Correction coefficient", accessoire.Correction_Coeff);
            dataGridView1.Rows[4].SetValues("Longueur", accessoire.Length);
            dataGridView1.Rows[5].SetValues("Flexion", accessoire.Flexion_Coeff);
            dataGridView1.Rows[6].SetValues("Date", accessoire.day + "/" + accessoire.month + "/" + accessoire.year);
            dataGridView1.Rows[7].SetValues("Date", accessoire.dayCalibration + "/" + accessoire.monthCalibration + "/" + accessoire.yearCalibration);
        }

        private void display_data_xml()
        {
            string display_ID = "";
            for (int i = 0; i < accessoire.user_ID.Length ; i++) display_ID += Convert.ToString(accessoire.user_ID[i]);

            dataGridView1.Rows[0].SetValues("ID", Encoding.Default.GetString(accessoire.user_ID));
            dataGridView1.Rows[1].SetValues("Level", accessoire.Level);
            dataGridView1.Rows[2].SetValues("Max torque", accessoire.Max_Torque);
            dataGridView1.Rows[3].SetValues("Correction coefficient", accessoire.Correction_Coeff);
            dataGridView1.Rows[4].SetValues("Longueur", accessoire.Length);
            dataGridView1.Rows[5].SetValues("Flexion", accessoire.Flexion_Coeff);

            if (dataGridView1[1, 6].Value == "" && dataGridView1[1, 7].Value == "")
            {
                dataGridView1.Rows[6].SetValues("Date", accessoire.day + "/" + accessoire.month + "/" + accessoire.year);
                dataGridView1.Rows[7].SetValues("Date", accessoire.dayCalibration + "/" + accessoire.monthCalibration + "/" + accessoire.yearCalibration);
            }
            else if (dataGridView1[1, 6].Value == "")
                dataGridView1.Rows[6].SetValues("Date", accessoire.day + "/" + accessoire.month + "/" + accessoire.year);
            else if (dataGridView1[1, 7].Value == "")
                dataGridView1.Rows[7].SetValues("Date", accessoire.dayCalibration + "/" + accessoire.monthCalibration + "/" + accessoire.yearCalibration);
        }

        private void display_data_combobox()
        {
            comboBox1.Items.Clear();
            for (int i = 0; acc_name_list[i] != null && acc_path_list[i] != null; i++)
            {
                if (acc_name_list[i] == "" && acc_path_list[i] == "")
                    acc_name_list[i] = acc_path_list[i] = null;
                else
                    comboBox1.Items.Add(acc_name_list[i]);
            }
            //listBox1.Items.Add("Form2 disposed!");
        }

        /***********************************************************************************************************************************************************/
        // accessory compare
        /***********************************************************************************************************************************************************/

        void display_detected_accessory()
        {
            int index_detected_accessory = accessory_compare();
            if (index_detected_accessory != -1)
                maskedTextBox1.Text = acc_name_list[index_detected_accessory];
            else maskedTextBox1.Text = "Unknown accessory";
            //listBox2.Items.Add("detected accessory: " + index_detected_accessory);
        }

        int accessory_compare()
        {
            for (int i = 0; i < comboBox1.Items.Count; i++)
            {
                selected_accessory_index = i;
                if (read_xml(true))
                {
                    selected_accessory_index = comboBox1.SelectedIndex;
                    return i;
                }
            }

            listBox2.Items.Add(comboBox1.Items.Count);
            selected_accessory_index = comboBox1.SelectedIndex;

            return -1;
        }

        bool compare_data_from_xml()
        {
            string dataName = "";
            string data = "";
            int same_data_cpt = 0;
            string array_data = "";
            char[] array_char_data = new char[10];

            for (int i = 0; i < bufferR_convert.Length; i++)
            {
                if (bufferR_convert[i] == '<' && bufferR_convert[i + 1] != '/' && bufferR_convert[i + 1] <= 90 && bufferR_convert[i + 1] >= 65)
                {
                    i++;
                    while (bufferR_convert[i] != '>')
                    {
                        dataName += bufferR_convert[i];
                        i++;
                    }

                    i++;

                    while (bufferR_convert[i] != '<')
                    {
                        data += bufferR_convert[i];
                        i++;
                    }

                    char[] ID_char_array = new char[10];
                    switch (dataName)
                    {
                        case "ID":
                            array_data = Convert.ToString(dataGridView1[1, 0].Value);
                            array_char_data = array_data.ToCharArray();
                            array_data = "";

                            for (int j = 0; j < array_char_data.Length; j++)
                            {
                                //listBox2.Items.Add(array_char_data[j]);
                                if (array_char_data[j] != ' ')
                                    array_data += array_char_data[j];
                            }

                            if (String.Equals(data, array_data))
                            {
                                if (same_data_cpt == 0)
                                    same_data_cpt = 1;
                            }
                            else
                                same_data_cpt = 0;
                                    break;
                            

                        case "Level":
                            array_data = Convert.ToString(dataGridView1[1, 1].Value);
                            array_char_data = array_data.ToCharArray();
                            array_data = "";

                            for (int j = 0; j < array_char_data.Length; j++)
                            {
                                //listBox2.Items.Add(array_char_data[j]);
                                if (array_char_data[j] != ' ')
                                    array_data += array_char_data[j];
                            }

                            if (String.Equals(data, array_data))
                            {
                                if (same_data_cpt == 1)
                                    same_data_cpt = 2;
                            }
                            else
                                same_data_cpt = 0;
                            break;


                        case "Max_Torque":
                            array_data = Convert.ToString(dataGridView1[1, 2].Value);
                            array_char_data = array_data.ToCharArray();
                            array_data = "";

                            for (int j = 0; j < array_char_data.Length; j++)
                            {
                                //listBox2.Items.Add(array_char_data[j]);
                                if (array_char_data[j] != ' ')
                                    array_data += array_char_data[j];
                            }

                            if (String.Equals(data, array_data))
                            {
                                if (same_data_cpt == 2)
                                    same_data_cpt = 3;
                            }
                            else
                                same_data_cpt = 0;
                            break;


                        case "Correction_Coeff":
                            char[] double_limiter = new char[16];
                            array_data = Convert.ToString(dataGridView1[1, 3].Value);
                            array_char_data = array_data.ToCharArray();
                            array_data = "";

                            for (int j = 0; j < array_char_data.Length; j++)
                            {
                                //listBox2.Items.Add(array_char_data[j]);
                                if (array_char_data[j] != ' ')
                                    array_data += array_char_data[j];
                            }

                            try
                            {
                                double_limiter = data.ToCharArray();
                                data = "";
                                for (int j = 0; j < 6; j++) data += double_limiter[j];
                            }
                            catch (Exception ex)
                            { }

                            if (String.Equals(data, array_data))
                            {
                                if (same_data_cpt == 3)
                                    same_data_cpt = 4;
                            }
                            else
                                same_data_cpt = 0;
                            break;


                        case "Length":
                            array_data = Convert.ToString(dataGridView1[1, 4].Value);
                            array_char_data = array_data.ToCharArray();
                            array_data = "";

                            for (int j = 0; j < array_char_data.Length; j++)
                            {
                                //listBox2.Items.Add(array_char_data[j]);
                                if (array_char_data[j] != ' ')
                                    array_data += array_char_data[j];
                            }

                            if (String.Equals(data, array_data))
                            {
                                if (same_data_cpt == 4)
                                    same_data_cpt = 5;
                            }
                            else
                                same_data_cpt = 0;
                            break;


                        case "Flexion_Coeff":
                            array_data = Convert.ToString(dataGridView1[1, 5].Value);
                            array_char_data = array_data.ToCharArray();
                            array_data = "";

                            for (int j = 0; j < array_char_data.Length; j++)
                            {
                                //listBox2.Items.Add(array_char_data[j]);
                                if (array_char_data[j] != ' ')
                                    array_data += array_char_data[j];
                            }

                            if (String.Equals(data, array_data))
                            {
                                if (same_data_cpt == 5)
                                    same_data_cpt = 6;
                            }
                            else
                                same_data_cpt = 0;
                            break;

                        default:
                            break;
                    }

                    dataName = data = array_data = "";
                    for (int j = 0; j < array_char_data.Length ; j++)
                        array_char_data[j] = ' ';
                    //maskedTextBox1.Text = dataName;
                }
            }
            if (same_data_cpt == 6)
                return true;
            else
                return false;
        }

        /***********************************************************************************************************************************************************/
        // Structure
        /***********************************************************************************************************************************************************/

        public struct Accessoire
        {
            public byte[] testID;
            public byte[] ID;
            public byte[] user_ID;
            public byte Level;
            public byte[] Correction_Coeff_data;
            public double Correction_Coeff;
            public byte[] def;
            public uint Length;
            public uint Max_Torque;
            public byte year;
            public byte month;
            public byte day;
            public uint Flexion_Coeff;
            public byte[] date_calibration;
            public byte yearCalibration;
            public byte monthCalibration;
            public byte dayCalibration;
        }

        /***********************************************************************************************************************************************************/
        // Initialisation
        /***********************************************************************************************************************************************************/

        private void init()
        {
            dataGridView1.Columns[0].Name = "Data";
            dataGridView1.Columns[1].Name = "Accessory";
            dataGridView1.Rows.Add("ID", "");
            dataGridView1.Rows.Add("Level", "");
            dataGridView1.Rows.Add("Max torque", "");
            dataGridView1.Rows.Add("Correction coefficient", "");
            dataGridView1.Rows.Add("Length", "");
            dataGridView1.Rows.Add("Flexion coefficient", "");
            dataGridView1.Rows.Add("Date", "");
            dataGridView1.Rows.Add("Fabrication date", "");
            label1.Text = appVersion + " - " + appDate;
            label5.Text = "";
            button5.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;

            accessoire.ID = new byte[8];
            accessoire.user_ID = new byte[10];
            accessoire.Correction_Coeff_data = new byte[2];
            accessoire.def = new byte[8];
            accessoire.date_calibration = new byte[6];

            for (int i = 0; i < 8; i++)
            {
                accessoire.ID[i] = 0;
                accessoire.user_ID[i] = 0;
                accessoire.def[i] = 0;
            }
            accessoire.user_ID[8] = 0;
            accessoire.user_ID[9] = 0;

            accessoire.Correction_Coeff_data[0] = 0;
            accessoire.Correction_Coeff_data[1] = 0;

            for (int i = 0; i < 6; i++)
            {
                accessoire.date_calibration[i] = 0;
            }

            serialPort = new SerialPort();
            serialPort.PortName = "com78";
            serialPort.BaudRate = 115200;
            serialPort.DataBits = 8;
            try
            {
                read_accessory_list();
            }
            catch (Exception ex)
            {

            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form4 form4 = new Form4();
            form4.ShowDialog();
        }
    }
}
