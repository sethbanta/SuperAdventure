using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using logic;

namespace UI
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        public SuperAdventure()
        {
            InitializeComponent();
            _player = new Player();
            _player.currentHitPoints = 10;
            _player.maximumHitPoints = 20;
            _player.gold = 20;
            _player.experiencePoints = 0;
            _player.level = 1;
            lblHitPoints.Text = _player.currentHitPoints.ToString();
            lblGold.Text = _player.gold.ToString();
            lblExperience.Text = _player.experiencePoints.ToString();
            lblLevel.Text = _player.level.ToString();
        }

        private void SuperAdventure_Load(object sender, EventArgs e)
        {

        }
    }
}
