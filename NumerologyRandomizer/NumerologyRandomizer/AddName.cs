﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NumerologyRandomizer
{
    public partial class AddName : Form
    {
        public AddName()
        {
            InitializeComponent();
        }

        public string AddedName { get; set; }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.AddedName = txtName.Text;
            this.Close();
        }
    }
}
