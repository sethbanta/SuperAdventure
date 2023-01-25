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
using System.IO;

namespace UI
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";
        public SuperAdventure()
        {
            InitializeComponent();
            if(File.Exists(PLAYER_DATA_FILE_NAME))
            {
                _player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else
            {
                _player = Player.CreateDefaultPlayer();
            }
            MoveTo(_player.CurrentLocation);


            //_player = new Player(10, 10, 20, 0, 1);
            //MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            //_player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
            Location location = new Location(1, "Home", "This is your house.");
        }
        
        //Travel North
        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        //Travel East
        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        //Travel South
        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        //Travel West
        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }


        //Function for allowing the player to move locations
        private void MoveTo(Location newLocation)
        {
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                //Didn't find the correct item
                rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name.ToString() + " to enter this location." + Environment.NewLine;
                return;
            }

            //Update the players location
            _player.CurrentLocation = newLocation;
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            //Check which directions have locations
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            //Heal the player from resting while moving locations
            _player.CurrentHitPoints = _player.MaximumHitPoints;
            lblHitPoints.Text = _player.CurrentHitPoints.ToString(); //update label

            // Does this location contain a quest?
            if (newLocation.QuestAvailableHere != null)
            {
                // Does the player have the quest? Have they completed it?
                bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);

                //Check if they have the quest
                if (playerAlreadyHasQuest)
                {
                    if (!playerAlreadyCompletedQuest)
                    {
                        bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);

                        // if we found the item and they have enough
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            //Display quest message
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;
                            //remove items
                            _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);
                            //display rewards
                            rtbMessages.Text += "You receive " + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name.ToString() + Environment.NewLine;
                            rtbMessages.Text += Environment.NewLine;
                            //give rewards
                            _player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                            lblExperience.Text = _player.ExperiencePoints.ToString();
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;
                            lblGold.Text = _player.Gold.ToString();
                            _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);
                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                            _player.GetPlayerLevel(_player.ExperiencePoints);

                        }
                    }
                }
                else
                {
                    rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " quest" + Environment.NewLine;
                    rtbMessages.Text += "Rewards Experience: " + newLocation.QuestAvailableHere.RewardExperiencePoints + " Gold: " + newLocation.QuestAvailableHere.RewardGold
                        + " Item: " + newLocation.QuestAvailableHere.RewardItem.Name.ToString() + Environment.NewLine;
                    rtbMessages.Text += "To complete, return with: " + Environment.NewLine;
                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                        }
                    }

                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            // Does this location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "You come across a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;

                //Make a new monster
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage, standardMonster.RewardExperiencePoints,
                    standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else
            {
                _currentMonster = null;
                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            //Refresh inventory
            UpdateInventoryListInUI();
            //Refresh players quest list
            UpdateQuestListInUI();
            //Refresh weapon combobox
            UpdateWeaponListInUI();
            //Refresh potion combobox
            UpdatePotionListInUI();
        }

        public void UpdateInventoryListInUI()
        {
            //Refresh players inventory list
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { ii.Details.Name, ii.Quantity.ToString() });
                }
            }
        }

        public void UpdateQuestListInUI()
        {
            //Refresh players quest list
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest pq in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { pq.Details.Name, pq.IsCompleted.ToString() });
            }
        }

        public void UpdateWeaponListInUI()
        {
            //Refresh player's weapons combobox
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details is Weapon)
                {
                    if (ii.Quantity > 0)
                    {
                        weapons.Add((Weapon)ii.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                //The player doesnt have weapons, hide the weapon combobox and the use button
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }
        }

        public void UpdatePotionListInUI()
        {
            //Refresh players potions combobox
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details is HealingPotion)
                {
                    if (ii.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)ii.Details);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                //The player has no potions, hide the combobox and use button
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";
                cboPotions.SelectedIndex = 0;
            }
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            //Get the current weapon
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;
            //Determine the amount of damage to do
            int damageToMonster = RandomNumberGenerator.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);
            //Apply the damage
            _currentMonster.CurrentHitPoints -= damageToMonster;
            //Display damage
            rtbMessages.Text += "You hit the " + _currentMonster.Name + " for " + damageToMonster.ToString() + " health points." + Environment.NewLine;
            //Check if the monster is dead
            if(_currentMonster.CurrentHitPoints <= 0)
            {
                //dead
                rtbMessages.Text += Environment.NewLine;
                rtbMessages.Text += "You defeated the " + _currentMonster.Name + Environment.NewLine;
                //give rewards
                rtbMessages.Text += "You receive " + _currentMonster.RewardExperiencePoints.ToString() + " experience points." + Environment.NewLine;
                _player.ExperiencePoints += _currentMonster.RewardExperiencePoints;
                _player.GetPlayerLevel(_player.ExperiencePoints);
                rtbMessages.Text += "You receive " + _currentMonster.RewardGold.ToString() + " gold." + Environment.NewLine;
                _player.Gold += _currentMonster.RewardGold;

                //Get random loot from monster
                List<InventoryItem> lootedItems = new List<InventoryItem>();

                //Add items to lootedItems list, and see if the percentage chance was hit
                foreach(LootItem lootItem in _currentMonster.LootTable)
                {
                    if(RandomNumberGenerator.NumberBetween(1,100) <= lootItem.DropPercentage)
                    {
                        lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                    }
                }

                //If no items were looted
                if(lootedItems.Count == 0)
                {
                    foreach(LootItem lootItem in _currentMonster.LootTable)
                    {
                        if(lootItem.IsDefaultItem)
                        {
                            //Give them a guaranteed default drop
                            lootedItems.Add(new InventoryItem(lootItem.Details, 1));
                        }
                    }
                }

                //Add the looted items to the players inventory
                foreach(InventoryItem ii in lootedItems)
                {
                    _player.AddItemToInventory(ii.Details);

                    if(ii.Quantity == 1)
                    {
                        rtbMessages.Text += "You receive " + ii.Quantity + " " + ii.Details.Name + Environment.NewLine;
                    } else
                    {
                        rtbMessages.Text += "You receive " + ii.Quantity + " " + ii.Details.NamePlural + Environment.NewLine;
                    }
                }

                //Refresh player information and controls
                lblHitPoints.Text = _player.CurrentHitPoints.ToString();
                lblGold.Text = _player.Gold.ToString();
                lblExperience.Text = _player.ExperiencePoints.ToString();
                lblLevel.Text = _player.Level.ToString();

                UpdateInventoryListInUI();
                UpdateWeaponListInUI();
                UpdatePotionListInUI();
                rtbMessages.Text += Environment.NewLine;
                MoveTo(_player.CurrentLocation);
            }
            else
            {
                //Monster is still alive
                //Determine the amount of damage the monster does to the player
                int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);
                //Display damage
                rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;
                _player.CurrentHitPoints -= damageToPlayer;
                lblHitPoints.Text = _player.CurrentHitPoints.ToString();
                
                //Is the player dead?
                if(_player.CurrentHitPoints <= 0)
                {
                    rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            HealingPotion currentPotion = (HealingPotion)cboPotions.SelectedItem;
            //Heal the player
            _player.CurrentHitPoints = (_player.CurrentHitPoints + currentPotion.AmountToHeal);
            //Make sure HP is at or below max
            if(_player.CurrentHitPoints > _player.MaximumHitPoints)
            {
                //Assuming they are above max, just set them to max
                _player.CurrentHitPoints = _player.MaximumHitPoints;
            }

            _player.RemoveItemFromInventory(currentPotion);
            rtbMessages.Text += "You healed using " + currentPotion.Name.ToString() + Environment.NewLine;
            //Monster swings while you drink the potion
            //Determine the amount of damage the monster does to the player
            int damageToPlayer = RandomNumberGenerator.NumberBetween(0, _currentMonster.MaximumDamage);
            //Display damage
            rtbMessages.Text += "The " + _currentMonster.Name + " did " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;
            _player.CurrentHitPoints -= damageToPlayer;
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            //Is the player dead?
            if (_player.CurrentHitPoints <= 0)
            {
                rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            //Refresh player data
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
        }




        private void SuperAdventure_Load(object sender, EventArgs e)
        {

        }

        private void rtbMessages_TextChanged(object sender, EventArgs e)
        {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());
        }
    }
}
