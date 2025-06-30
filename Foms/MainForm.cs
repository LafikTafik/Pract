using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NAMI.Foms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void SimulationForm_Load(object sender, EventArgs e)
        {

        }

        private void roundedButton1_Click(object sender, EventArgs e)
        {
          
            SimulationForm simulationForm = new SimulationForm();
            this.Hide(); // скрываем текущую форму
            simulationForm.Show(); // показываем следующую
        }

        private void roundedButton2_Click(object sender, EventArgs e)
        {
            SettingsForm settingsForm = new SettingsForm();
            settingsForm.ShowDialog(); // показываем как диалоговое окно
        }

        private void roundedButton3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
