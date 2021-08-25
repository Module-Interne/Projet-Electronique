using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Programmation_1_wire_production
{
    public partial class Form2 : Form
    {
        private static bool Save_dialog = true;
        private static byte Folder_dialog = 0;

        private byte Level = 0;
        private uint Max_Torque = 0;
        private double Correction_Coeff = 0;
        private uint Length = 0;
        private uint Flexion_Coeff = 0;
        private byte[] user_ID = new byte[10];

        private char[] bufferR = new char[500];
        private string[] acc_name_list = new string[100];
        private string[] acc_path_list = new string[100];
        private int new_length = 0;

        string previous_path = "";
        private string selected_path = "";
        private int selected_accessory_index = 0;

        Accessoire accessoire = new Accessoire();

        public Form2(string[] name_list, string[] path_list)
        {
            InitializeComponent();
            for (int i = 0; i < 100; i++)
            {
                acc_name_list[i] = name_list[i];
                acc_path_list[i] = path_list[i];
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            init();
        }

        /***********************************************************************************************************************************************************/
        // Main
        /***********************************************************************************************************************************************************/

        private void get_path_from_browse(bool unused)
        {
            string extract_name = "";
            string[] splited_path = new string[50];
            char[] trim_extension = { '.', 'x', 'm', 'l' };

            extract_name = saveFileDialog1.FileName;
            splited_path = extract_name.Split('\\');

            acc_path_list[selected_accessory_index] = "";

            for (int i = 0; i < (splited_path.Length - 1); i++)
            {
                acc_path_list[selected_accessory_index] += splited_path[i];
                acc_path_list[selected_accessory_index] += '\\';
                acc_path_list[selected_accessory_index] += '\\';
            }

            acc_name_list[selected_accessory_index] = splited_path[(splited_path.Length - 1)];
            acc_name_list[selected_accessory_index] = acc_name_list[selected_accessory_index].TrimEnd(trim_extension);
            listBox1.Items.Add(acc_name_list[selected_accessory_index]);
        }

        private void get_path_from_browse(byte unused)
        {
            string extract_name = "";
            string[] splited_path = new string[50];
            char[] trim_extension = { '.', 'x', 'm', 'l' };

            extract_name = saveFileDialog1.FileName;
            splited_path = extract_name.Split('\\');

            //acc_path_list[selected_accessory_index] = "";

            for (int i = 0; i < (splited_path.Length - 1); i++)
            {
                acc_path_list[selected_accessory_index] += splited_path[i];
                acc_path_list[selected_accessory_index] += '\\';
                acc_path_list[selected_accessory_index] += '\\';
            }

            acc_name_list[selected_accessory_index] = splited_path[(splited_path.Length - 1)];
            acc_name_list[selected_accessory_index] = acc_name_list[selected_accessory_index].TrimEnd(trim_extension);
            listBox1.Items.Add(acc_name_list[selected_accessory_index]);
        }

        void delete_duplicate()
        {
            previous_path = "";
            previous_path += acc_path_list[selected_accessory_index] + acc_name_list[selected_accessory_index] + ".xml";
            File.Delete(previous_path);
        }

        bool is_path_modified()
        {
            string[] splited_path = new string[50];

            splited_path = folderBrowserDialog1.SelectedPath.Split('\\');
            selected_path = "";
            for (int i = 0 ; i < splited_path.Length; i++)
            {
                selected_path += splited_path[i] + "\\\\";
            }

            return (String.Equals(acc_path_list[selected_accessory_index], selected_path) && !String.Equals("\\\\", selected_path));
        }

        /***********************************************************************************************************************************************************/
        // Handlers
        /***********************************************************************************************************************************************************/

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                checkBox2.Checked = false;
                maskedTextBox1.Show();
                maskedTextBox1.Hide();
                comboBox1.Enabled = false;
                button4.Hide();
                button1.Hide();
                button5.Show();
            }
            
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                checkBox1.Checked = false;
                maskedTextBox1.Hide();
                comboBox1.Enabled = true;
                button4.Show();
                button1.Show();
                button5.Hide();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            //openFileDialog1.ShowDialog();
            //saveFileDialog1.ShowDialog();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                //if (datas_ok(false)) // false for "no display" wich can erase the array before writing a xml file. All others cases are "with display" wich meen passing the argument true 
                //Add_accessory_to_index_file();
            }
            else
            {
                /*if (acc_path_list[selected_accessory_index] != previous_path)
                    modify_index_path();*/
                if(is_path_modified()) modify_index_path();
                if (datas_ok(false)) modify_xml_file();
                else listBox1.Items.Add("Veuillez rmplir les champs");
                    
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            delete_accessory();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //openFileDialog1.ShowDialog();
            if (datas_ok(false))
            saveFileDialog1.ShowDialog();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selected_accessory_index = comboBox1.SelectedIndex;
            previous_path = acc_path_list[selected_accessory_index];
            read_xml();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            //listBox1.Items.Add(saveFileDialog1.OpenFile());
            //saveFileDialog1.FileName;
            /*StreamWriter sw = new StreamWriter(saveFileDialog1.OpenFile());
            sw.WriteLine("test");
            sw.Close();
            sw.Dispose();*/
            write_xml_file(saveFileDialog1.OpenFile());
            //listBox1.Items.Add(saveFileDialog1.FileName);
            //MessageBox.Show(saveFileDialog1.FileName);

            Add_accessory_to_index_file();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            if (checkBox2.Checked)
            {
                selected_path = openFileDialog1.FileName;
                listBox1.Items.Add("test" + selected_path);
            }
        }

        /***********************************************************************************************************************************************************/
        // Write XML
        /***********************************************************************************************************************************************************/

        private void Add_accessory_to_index_file()
        {
            bool add_accessory = false;

            if (datas_ok(true))
            {
                try
                {
                    StreamWriter sw = new StreamWriter("./" + "Accessory_list.txt");

                    for (int i = 0; acc_name_list[i] != null && acc_path_list[i] != null; i++)
                    {
                        sw.WriteLine(i + ";" + acc_name_list[i] + ";" + acc_path_list[i] + ";");
                        new_length = i + 1;
                    }

                    selected_accessory_index = new_length;
                    get_path_from_browse(Save_dialog);
                    //acc_name_list[new_length] = maskedTextBox1.Text;

                    write_xml_file();
                    //sw.WriteLine((new_length) + ";" + maskedTextBox1.Text + ";" + acc_path_list[new_length] + ";");
                    sw.WriteLine((new_length) + ";" + acc_name_list[selected_accessory_index] + ";" + acc_path_list[new_length] + ";");
                    sw.WriteLine("~");
                    //for (int i = 0; i < 500; i++);
                    sw.Close();
                    delete_duplicate();
                    add_accessory = true;
                }

                catch(Exception ex)
                {
                    listBox1.Items.Add(ex);
                }
                update_backup();
                if (add_accessory)
                    this.Dispose();
            }
        }

        /*private void Add_accessory_to_index_file()
        {
            bool add_accessory = false;

            if (folderBrowserDialog1.SelectedPath != "" && maskedTextBox1.Text != "" && datas_ok(true))
            {
                try
                {
                    StreamWriter sw = new StreamWriter("./" + "Accessory_list.txt");

                    for (int i = 0; acc_name_list[i] != null && acc_path_list[i] != null; i++)
                    {
                        sw.WriteLine(i + ";" + acc_name_list[i] + ";" + acc_path_list[i] + ";");
                        new_length = i + 1;
                    }

                    selected_accessory_index = new_length;
                    get_path_from_browse();
                    acc_name_list[new_length] = maskedTextBox1.Text;

                    write_xml_file();
                    sw.WriteLine((new_length) + ";" + maskedTextBox1.Text + ";" + acc_path_list[new_length] + ";");
                    sw.WriteLine("~");
                    //for (int i = 0; i < 500; i++) ;
                    sw.Close();
                    add_accessory = true;
                }

                catch (Exception ex)
                {
                    listBox1.Items.Add(ex);
                }
                update_backup();
                if (add_accessory)
                    this.Dispose();
            }
        }*/

        private bool write_xml_file(Stream xml_path)
        {
            try
            {
                //StreamWriter sw_xml = new StreamWriter(acc_path_list[selected_accessory_index] + "Accessory " + acc_name_list[selected_accessory_index] + ".xml");
                StreamWriter sw_xml = new StreamWriter(xml_path);
                sw_xml.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sw_xml.WriteLine("<!--USB_Onewire_Adapter-->");
                sw_xml.WriteLine("<serie>");
                sw_xml.WriteLine("<ID>" + accessoire.ID + "</ID>");
                sw_xml.WriteLine("<Level>" + accessoire.Level + "</Level>");
                sw_xml.WriteLine("<Max_Torque>" + accessoire.Max_Torque + "</Max_Torque>");
                sw_xml.WriteLine("<Correction_Coeff>" + accessoire.Correction_Coeff + "</Correction_Coeff>");
                sw_xml.WriteLine("<Length>" + accessoire.Length + "</Length>");
                sw_xml.WriteLine("<Flexion_Coeff>" + accessoire.Flexion_Coeff + "</Flexion_Coeff>");
                sw_xml.WriteLine("</serie>");
                sw_xml.Close();

                return true;
            }

            catch (Exception ex)
            {
                listBox1.Items.Add(ex);
                return false;
            }
        }

        private bool write_xml_file()
        {
            try
            {
                StreamWriter sw_xml = new StreamWriter(acc_path_list[selected_accessory_index] + "Accessory " + acc_name_list[selected_accessory_index] + ".xml");
                sw_xml.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                sw_xml.WriteLine("<!--USB_Onewire_Adapter-->");
                sw_xml.WriteLine("<serie>");
                sw_xml.WriteLine("<ID>" + accessoire.ID + "</ID>");
                sw_xml.WriteLine("<Level>" + accessoire.Level +"</Level>");
                sw_xml.WriteLine("<Max_Torque>" + accessoire.Max_Torque +"</Max_Torque>");
                sw_xml.WriteLine("<Correction_Coeff>" + accessoire.Correction_Coeff +"</Correction_Coeff>");
                sw_xml.WriteLine("<Length>" + accessoire.Length +"</Length>");
                sw_xml.WriteLine("<Flexion_Coeff>" + accessoire.Flexion_Coeff +"</Flexion_Coeff>");
                sw_xml.WriteLine("</serie>");
                sw_xml.Close();

                return true;
            }

            catch (Exception ex)
            {
                listBox1.Items.Add(ex.Message);
                listBox1.Items.Add(acc_path_list[selected_accessory_index] + "Accessory " + acc_name_list[selected_accessory_index] + ".xml");
                return false;
            }
        }

        private bool datas_ok(bool with_display)
        {
            if (dataGridView1.RowCount == 6)
            {
                //listBox1.Items.Add(dataGridView1.RowCount);
                if (Convert.ToString(dataGridView1[1, 0].Value) != "" && Convert.ToString(dataGridView1[1, 1].Value) != "" && Convert.ToString(dataGridView1[1, 2].Value) != "" && Convert.ToString(dataGridView1[1, 3].Value) != "" && Convert.ToString(dataGridView1[1, 4].Value) != "" && Convert.ToString(dataGridView1[1, 5].Value) != "")
                {
                    if (read_cells(with_display))
                    {
                        accessoire.ID = Convert.ToString(dataGridView1[1, 0].Value);
                        coder_name();
                        accessoire.Level = Convert.ToString(dataGridView1[1, 1].Value);
                        accessoire.Max_Torque = Convert.ToString(dataGridView1[1, 2].Value);
                        accessoire.Correction_Coeff = Convert.ToString(dataGridView1[1, 3].Value);
                        accessoire.Length = Convert.ToString(dataGridView1[1, 4].Value);
                        accessoire.Flexion_Coeff = Convert.ToString(dataGridView1[1, 5].Value);
                        return true;
                    }
                    else return false;
                }
                else
                {
                    listBox1.Items.Add("Erreur : " + Convert.ToString(dataGridView1[1, 0].Value) + ", " + Convert.ToString(dataGridView1[1, 1].Value) + ", " + Convert.ToString(dataGridView1[1, 2].Value) + ", " + Convert.ToString(dataGridView1[1, 3].Value) + ", " + Convert.ToString(dataGridView1[1, 4].Value) + ", " + Convert.ToString(dataGridView1[1, 5].Value));
                    return false;
                }
            }
            else
            {
                listBox1.Items.Add(dataGridView1.RowCount);
                return false;
            }
        }

        void coder_name()//variables utilisées "ID" correspond à name de la rallonge
        {
            char[] id_convert = accessoire.ID.ToCharArray();
            //byte[] id_convert = new byte[10];
            //for (byte i = 0; i < id_converter.Length; i++) id_convert[i] = (byte)id_converter[i];

            /*for (int i = 0; i < id_convert.Length; i++)
            {
                if (id_convert[i] != 0) //prévoir ID < 10 charactères
                    //listBox2.Items.Add("Longueur ID: " + accessoire[typeRallonge].user_ID.Length);
                    id_convert[i] = (char)(id_convert[i] - 32);
                if (id_convert[i] >= 65 && id_convert[i] <= 90)//si entre a et z
                {
                    id_convert[i] -= (char)32;//on le passe en majuscule
                }
            }*/
            for (int i = 0; i < id_convert.Length; i++)
            {
                if (id_convert[i] != 0) //prévoir ID < 10 charactères
                    //listBox2.Items.Add("Longueur ID: " + accessoire[typeRallonge].user_ID.Length);
                if (id_convert[i] >= 'a' && id_convert[i] <= 'z')//si entre a et z
                {
                    id_convert[i] -= (char)32;//on le passe en majuscule
                }
            }
            accessoire.ID = "";
            for (byte i = 0; i < id_convert.Length; i++) accessoire.ID += id_convert[i];
            listBox1.Items.Add(accessoire.ID);
        }

        private void modify_xml_file()
        {
            for (int i = 0; acc_name_list[i] != null && acc_path_list[i] != null; i++)
            {
                //listBox1.Items.Add(acc_name_list[i] + " -> " + acc_path_list[i]);
                new_length = i;
            }
            //get_path_from_browse();
            if (write_xml_file())
                this.Dispose();
        }

        private void modify_index_path()
        {
            //string previous_path = acc_path_list[selected_accessory_index];
            /*if (acc_path_list[selected_accessory_index] != folderBrowserDialog1.SelectedPath) listBox1.Items.Add("previous différent");
            listBox1.Items.Add("ancien : " + acc_path_list[selected_accessory_index]);
            listBox1.Items.Add("nouveau : " + folderBrowserDialog1.SelectedPath);*/


            //get_path_from_browse(Folder_dialog);
            acc_path_list[selected_accessory_index] = selected_path;
            StreamWriter sw = new StreamWriter("./" + "Accessory_list.txt");

            for (int i = 0; acc_name_list[i] != null && acc_path_list[i] != null; i++)
            {
                    sw.WriteLine(i + ";" + acc_name_list[i] + ";" + acc_path_list[i] + ";");   
            }
            sw.WriteLine("~");
            sw.Close();

            update_backup();
            delete_previous_xml_file();
            //File.Delete(previous_path + "Accessory " + acc_name_list[selected_accessory_index] + ".xml");
            //this.Dispose();
        }

        private void delete_previous_xml_file()
        {
            char[] acc_path_list_completion = new char[1000];
            int[] add_character = new int[100];
            add_character[0] = 0;//compte le nombre de fois qu'il faut ajouter le signe "\"

            acc_path_list_completion = previous_path.ToCharArray();
            previous_path = "";

            for (int i = 0; i < acc_path_list_completion.Length; i++)
            {

                if (acc_path_list_completion[i] != '\\')
                {
                    previous_path += acc_path_list_completion[i];
                }
                else
                {
                    previous_path += "\\";
                    i++;
                }
            }

            //previous_path += '\\';
            previous_path += "Accessory " + acc_name_list[selected_accessory_index] + ".xml";
            //listBox1.Items.Add(previous_path);
            //textBox1.Text = previous_path;
            File.Delete(previous_path);
            //File.Delete(previous_path + "Accessory " + acc_name_list[selected_accessory_index] + ".xml");
            //File.Delete("C:\\Users\\amiramont\\Documents\\Visual Studio 2010\\Projects\\Programmation 1-wire production\\Programmation 1-wire production\\Ressources\\Nouveau dossier\\Accessory Rallonge 300.xml");
            //listBox1.Items.Add("prev : " + previous_path + "Accessory " + acc_name_list[selected_accessory_index] + ".xml");
        }

        private void update_backup()
        {
            try
            {
                string buffer_transfert = "";
                StreamReader sr = new StreamReader("./" + "Accessory_list.txt");
                buffer_transfert = sr.ReadToEnd();
                sr.Close();

                //for (int i = 0; i < 50000; i++) ;

                StreamWriter sw = new StreamWriter("./" + "Accessory_list_Backup.txt");
                sw.Write(buffer_transfert);
                sw.Close();
            }

            catch (Exception ex)
            {
                listBox1.Items.Add(ex);
            }
        }

        /***********************************************************************************************************************************************************/
        // Read XML
        /***********************************************************************************************************************************************************/

        private void read_xml()
        {
            try
            {
                string buffer_reception;
                StreamReader sr = new StreamReader(acc_path_list[selected_accessory_index] + "Accessory " + acc_name_list[selected_accessory_index] + ".xml");
                buffer_reception = sr.ReadToEnd();
                bufferR = buffer_reception.ToCharArray();
                //maskedTextBox1.Text = buffer_reception;
                //sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception ex)
            {
                maskedTextBox1.Text = "fichier introuvable";
            }

            get_data_from_xml();
        }

        private void get_data_from_xml()
        {
            string dataName = "";
            string data = "";

            for (int i = 0; i < bufferR.Length ; i++)
            {
                if (bufferR[i] == '<' && bufferR[i + 1] != '/' && bufferR[i + 1] <= 90 && bufferR[i + 1] >= 65)
                {
                    i++;
                    while (bufferR[i] != '>')
                    {
                        dataName += bufferR[i];
                        i++;
                    }

                    i++;

                    while (bufferR[i] != '<')
                    {
                        data += bufferR[i];
                        i++;
                    }

                    switch (dataName)
                    {
                        case "ID":
                            accessoire.ID = data;
                            break;

                        case "Level":
                            accessoire.Level = data;
                            break;

                        case "Max_Torque":
                            accessoire.Max_Torque = data;
                            break;

                        case "Correction_Coeff":
                            accessoire.Correction_Coeff = data;
                            break;

                        case "Length":
                            accessoire.Length = data;
                            break;

                        case "Flexion_Coeff":
                            accessoire.Flexion_Coeff = data;
                            break;

                        default:
                            break;
                    }
                    
                    dataName = data = "";

                    //maskedTextBox1.Text = dataName;
                }
            }
            display_data();
        }

        /***********************************************************************************************************************************************************/
        // Affichage
        /***********************************************************************************************************************************************************/

        private void display_data()
        {
            dataGridView1.Rows[0].SetValues("ID", accessoire.ID);
            dataGridView1.Rows[1].SetValues("Level", accessoire.Level);
            dataGridView1.Rows[2].SetValues("Max torque", accessoire.Max_Torque);
            dataGridView1.Rows[3].SetValues("Correction coefficient", accessoire.Correction_Coeff);
            dataGridView1.Rows[4].SetValues("Longueur", accessoire.Length);
            dataGridView1.Rows[5].SetValues("Flexion", accessoire.Flexion_Coeff);
        }

        /***********************************************************************************************************************************************************/
        // Suppression accessoires
        /***********************************************************************************************************************************************************/

        private void delete_accessory()
        {
            string file_path_to_delete = acc_path_list[selected_accessory_index] + "Accessory " + acc_name_list[selected_accessory_index] + ".xml";
            char[] path_modifier = new char[500];
            bool delete_complete = false;

            path_modifier = file_path_to_delete.ToCharArray();
            file_path_to_delete = "";

            for (int i = 0; i < path_modifier.Length; i++)
            {

                if (path_modifier[i] != '\\')
                {
                    file_path_to_delete += path_modifier[i];
                }
                else
                {
                    file_path_to_delete += "\\";
                    i++;
                }
            }

            File.Delete(file_path_to_delete);
            try
            {
                StreamWriter sw = new StreamWriter("./" + "Accessory_list.txt");

                int j = 0;
                for (int i = 0; acc_name_list[i] != null && acc_path_list[i] != null; i++)
                {
                    if (i != selected_accessory_index)
                    {
                        sw.WriteLine(j + ";" + acc_name_list[i] + ";" + acc_path_list[i] + ";");
                        j++;
                    }
                }
                sw.WriteLine("~");
                sw.Close();
                acc_path_list[selected_accessory_index] = acc_name_list[selected_accessory_index] = null;
                delete_complete = true;
            }

            catch (Exception ex)
            {
                listBox1.Items.Add(ex);
            }

            update_backup();
            if (delete_complete)
                this.Dispose();
        }

        /***********************************************************************************************************************************************************/
        // Verify data
        /***********************************************************************************************************************************************************/

        void read_array()
        {
            string ID_string = "";
            char[] test = new char[10];

            ID_string = Convert.ToString(dataGridView1[1, 0].Value);

            user_ID = System.Text.Encoding.ASCII.GetBytes(ID_string);
            test = ID_string.ToCharArray();
            //for (int i = 0; i < 10; i++) user_ID[i] = Convert.ToByte(test[i]);
            user_ID = System.Text.Encoding.ASCII.GetBytes(test);

            //listBox1.Items.Add("string = " + ID_string + ", ID : " + user_ID[0] + user_ID[1] + user_ID[2] + user_ID[3]);
            //listBox1.Items.Add("string = " + ID_string + ", ID : " + test[0] + test[1] + test[2] + test[3]);

            Level = Convert.ToByte(dataGridView1[1, 1].Value);
            Max_Torque = Convert.ToUInt16(dataGridView1[1, 2].Value);
            Correction_Coeff = Convert.ToDouble(dataGridView1[1, 3].Value);
            Length = Convert.ToUInt16(dataGridView1[1, 4].Value);
            Flexion_Coeff = Convert.ToUInt16(dataGridView1[1, 5].Value);
        }

        public bool read_cells(bool with_display)
        {
            bool data_not_valid = false;
            //try
            //{
                read_array();
            //}
            /*catch (Exception e)
            {
                listBox1.Items.Add("step 0");
                Form3 form3 = new Form3(0);
                form3.ShowDialog();
                display_data();
                return false;
            }*/

            try
            {
                if (user_ID.Length > 10)
                {
                    //string ID_string = Convert.ToString(cell_compare[0]);
                    //user_ID = System.Text.Encoding.ASCII.GetBytes(ID_string);
                    dataGridView1.Rows[0].SetValues("ID", "");
                    Form3 form3 = new Form3(1);
                    form3.ShowDialog();
                    data_not_valid = true;
                }
                else if (Level > 2 || Level < 1)
                {
                    //Level = Convert.ToByte(cell_compare[1]);
                    dataGridView1.Rows[1].SetValues("Level", "");
                    Form3 form3 = new Form3(2);
                    form3.ShowDialog();
                    data_not_valid = true;
                }
                else if (Correction_Coeff < 0.1)
                {
                    //Correction_Coeff = Convert.ToDouble(cell_compare[3]);
                    dataGridView1.Rows[3].SetValues("Correction coefficient", "");
                    Form3 form3 = new Form3(3);
                    form3.ShowDialog();
                    data_not_valid = true;
                }

                else if (Correction_Coeff > 10)
                {
                    //Correction_Coeff = Convert.ToDouble(cell_compare[3]);
                    dataGridView1.Rows[3].SetValues("Correction coefficient", "");
                    Form3 form3 = new Form3(3);
                    form3.ShowDialog();
                    data_not_valid = true;
                }

                else if (Max_Torque > 2000)
                {
                    //Max_Torque = Convert.ToUInt16(cell_compare[2]);
                    dataGridView1.Rows[2].SetValues("Max torque", "");
                    Form3 form3 = new Form3(4);
                    form3.ShowDialog();
                    data_not_valid = true;
                }

                else if (Length > 4000)
                {
                    //Length = Convert.ToUInt16(cell_compare[4]);
                    dataGridView1.Rows[4].SetValues("Longueur", "");
                    Form3 form3 = new Form3(5);
                    form3.ShowDialog();
                    data_not_valid = true;
                }
                if (!data_not_valid)
                {
                    if (with_display)
                    display_data();
                    return true;
                }
                else
                {
                    if (with_display) display_data();
                    return false;
                }
            }
            catch (Exception e)
            {
                //listBox1.Items.Add("step 1");
                Form3 form3 = new Form3(0);
                form3.ShowDialog();
                display_data();
                return false;
            }


            /*try
            {
                coder_name();
                //listBox2.Items.Add("step 1");
            }
            catch (Exception e)
            {
                listBox1.Items.Add("step 2");
                Form3 form3 = new Form3(0);
                form3.ShowDialog();
                display_data();
                return false;
            }

            concatener();

            try
            {
                decoder_name();
                //listBox2.Items.Add("step 3");
            }
            catch (Exception e)
            {
                listBox1.Items.Add("step 4");
                //listBox2.Items.Add("I: " + i + " - " + "Read call :  " + e);
                //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                //toolTip1.Show("Erreur dans la saisie des données", this, MousePosition.X, MousePosition.Y, 2000);
                Form3 form3 = new Form3(0);
                form3.ShowDialog();
                display_data();
                return false;
            }
            try
            {
                formating_accessory_datas();
                //listBox2.Items.Add("step 4");
            }
            catch (Exception e)
            {
                listBox1.Items.Add("step 5");
                //listBox2.Items.Add("I: " + i + " - " + "Read call :  " + e);
                //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                //toolTip1.Show("Erreur dans la saisie des données", this, MousePosition.X, MousePosition.Y, 2000);
                Form3 form3 = new Form3(0);
                form3.ShowDialog();
                display_data();
                return false;
            }
            try
            {
                calcul_rallonge_data();
                display_data();
                //listBox2.Items.Add("step 5");

            }
            catch (Exception e)
            {
                listBox1.Items.Add("step 6");
                //listBox2.Items.Add("I: " + i + " - " + "Read call :  " + e);
                //toolTip1.ToolTipTitle = "Input Rejected - Data is Out of Range";
                //toolTip1.Show("Erreur dans la saisie des données", this, MousePosition.X, MousePosition.Y, 2000);
                Form3 form3 = new Form3(0);
                form3.ShowDialog();
                display_data();
                return false;
            }*/
        }

        /***********************************************************************************************************************************************************/
        // Structure
        /***********************************************************************************************************************************************************/

        public struct Accessoire
        {
            public string ID;
            public string Level;
            public string Correction_Coeff;
            public string Max_Torque;
            public string Length;
            public string Flexion_Coeff;
        }

        /***********************************************************************************************************************************************************/
        // Initialisation
        /***********************************************************************************************************************************************************/

        private void init()
        {
            dataGridView1.Columns[0].Name = "Data";
            dataGridView1.Columns[1].Name = "Accessory";
            dataGridView1.Rows.Add("ID", "");
            //dataGridView1.Rows[0].SetValues("ID", "");
            dataGridView1.Rows.Add("Level", "");
            dataGridView1.Rows.Add("Max torque", "");
            dataGridView1.Rows.Add("Correction coefficient", "");
            dataGridView1.Rows.Add("Length", "");
            dataGridView1.Rows.Add("Flexion coefficient", "");
            //dataGridView1.Rows[5].SetValues("Flexion coefficient", "");
            
            button4.Hide();
            button1.Hide();
            button5.Show();

            maskedTextBox1.Hide();
            comboBox1.Show();
            comboBox1.Items.Clear();
            comboBox1.Enabled = false;

            for (int i = 0; acc_name_list[i] != null && acc_path_list[i] != null; i++)
            {
                comboBox1.Items.Add(acc_name_list[i]);
                //listBox1.Items.Add("combo, i = " + i);
            }

            selected_path = folderBrowserDialog1.SelectedPath;

            saveFileDialog1.Filter = "XML files (*.xml)|*.xml";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if(is_path_modified()) listBox1.Items.Add("true");
            else listBox1.Items.Add("false");
        }
    }
}
