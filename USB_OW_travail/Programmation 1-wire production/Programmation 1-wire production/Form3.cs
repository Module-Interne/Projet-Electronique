using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Programmation_1_wire_production
{
    public partial class Form3 : Form
    {

        public byte codeError = 0;

        public Form3(byte typeError)
        {
            InitializeComponent();
            codeError = typeError;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            switch (codeError)
            {
                case 0:
                    label1.Text = "Erreur acquisition de données";
                    break;

                case 1:
                    label1.Text = "Erreur données hors limite\nL'identifiant est limité à 10 caractères";
                    break;

                case 2:
                    label1.Text = "Erreur données hors limite\nVeuillez rentrer level 1 pour une rallonge et level 2 pour un cliquet";
                    break;

                case 3:
                    label1.Text = "Erreur données hors limite\nVeuillez rentrer un coefficient compris entre 0,1 et 10";
                    break;

                case 4:
                    label1.Text = "Erreur données hors limite\nLe couple maximal autorisé est de 2000 N.m";
                    break;

                case 5:
                    label1.Text = "Erreur données hors limite\nLa longueur maximale autorisée est de 4m";
                    break;

                case 6:
                    label1.Text = "Erreur données hors limite\nLa date rentrée n'existe pas";
                    break;

                case 7:
                    label1.Text = "Data saved succesfully";
                    this.Text = "Info";
                    break;

                case 10:
                    label1.Text = "Error 1: Wrong CRC";
                    this.Text = "Error";
                    break;

                case 11:
                    label1.Text = "Error 2: No accessory detected";
                    this.Text = "Error";
                    break;

                case 12:
                    label1.Text = "Error 3: More then 1 accessory";
                    this.Text = "Error";
                    break;

                case 13:
                    label1.Text = "Error 4: Write data failed";
                    this.Text = "Error";
                    break;

                case 14:
                    label1.Text = "Error 5: Datas false";
                    this.Text = "Error";
                    break;

                    case 20:
                    label1.Text = "Device ready";
                    this.Text = "Succes";
                    break;

                    case 21:
                    label1.Text = "Device not found, try another Port\nUnplug and replug the device to find it";
                    this.Text = "Connection failed";
                    break;

                default:
                    label1.Text = "Erreur passage d'argument";
                    break;
            }
        }
    }
}
