﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoEditor
{
    public partial class FormFiltro : Form
    {
        public FormFiltro()
        {
            InitializeComponent();
        }

        public string FilterName { get { return textBox1.Text; } }
    }
}
