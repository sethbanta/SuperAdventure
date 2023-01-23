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
        private Monster _currentMonster;
        public SuperAdventure()
        {
            InitializeComponent();


            _player = new Player(10, 10, 20, 0, 1);
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
            Location location = new Location(1, "Home", "This is your house.");
        }

        //Function for allowing the player to move locations
        private void MoveTo(Location newLocation)
        {
            if (newLocation.ItemRequiredToEnter != null)
            {
                bool playerHasRequiredItem = false;

                // Check what the player has
                foreach (InventoryItem ii in _player.Inventory)
                {
                    // If we find the correct item
                    if (ii.Details.ID == newLocation.ItemRequiredToEnter.ID)
                    {
                        //Correct item found
                        playerHasRequiredItem = true;
                        break; //exit the loop
                    }
                }

                if (!playerHasRequiredItem)
                {
                    //Didn't find the correct item
                    rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter + " to enter this location." + Environment.NewLine;
                    return;
                }
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
                bool playerAlreadyHasQuest = false;
                bool playerAlreadyCompletedQuest = false;

                foreach (PlayerQuest playerQuest in _player.Quests)
                {
                    if (playerQuest.Details.ID == newLocation.QuestAvailableHere.ID) //has the quest
                    {
                        playerAlreadyHasQuest = true;

                        if (playerQuest.IsCompleted) //completed it
                        {
                            playerAlreadyCompletedQuest = true;
                        }
                    }
                }

                //Check if they have the quest
                if (playerAlreadyHasQuest)
                {
                    if (!playerAlreadyCompletedQuest)
                    {
                        bool playerHasAllItemsToCompleteQuest = true;

                        foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                        {
                            bool foundItemInPlayersInventory = false;

                            foreach (InventoryItem ii in _player.Inventory)
                            {
                                // The player has the item
                                if (ii.Details.ID == qci.Details.ID)
                                {
                                    foundItemInPlayersInventory = true;

                                    if (ii.Quantity < qci.Quantity) // not enough items to complete quest
                                    {
                                        playerHasAllItemsToCompleteQuest = false;
                                        break; //exit loop because they cant finish
                                    }
                                    //Found the item, dont need to check the rest of the inventory (if theres any left)
                                    break;
                                }
                            }

                            //Didn't find the item
                            if (!foundItemInPlayersInventory)
                            {
                                //Couldn't find the item, stop
                                playerHasAllItemsToCompleteQuest = false;
                                break;
                            }
                        }
                        // if we found the item and they have enough
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            //Display quest message
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;
                            //remove items
                            foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                            {
                                foreach (InventoryItem ii in _player.Inventory)
                                {
                                    if (ii.Details.ID == qci.Details.ID)
                                    {
                                        //Subtract the amount of items the quest took
                                        ii.Quantity -= qci.Quantity;
                                        break;
                                    }
                                }
                            }
                            //give rewards
                            rtbMessages.Text += "You receive " + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.ToString() + Environment.NewLine;
                            rtbMessages.Text += Environment.NewLine;

                            _player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            bool addedItemToPlayerInventory = false;

                            foreach (InventoryItem ii in _player.Inventory)
                            {
                                if (ii.Details.ID == newLocation.QuestAvailableHere.RewardItem.ID)
                                {
                                    ii.Quantity++; // they have the item already, so add one as a reward
                                    addedItemToPlayerInventory = true;
                                    break;
                                }
                            }

                            if (!addedItemToPlayerInventory)
                            {
                                _player.Inventory.Add(new InventoryItem(newLocation.QuestAvailableHere.RewardItem, 1));
                            }

                            foreach (PlayerQuest pq in _player.Quests)
                            {
                                if (pq.Details.ID == newLocation.QuestAvailableHere.ID)
                                {
                                    pq.IsCompleted = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " quest" + Environment.NewLine;
                    rtbMessages.Text += "Rewards Experience: " + newLocation.QuestAvailableHere.RewardExperiencePoints + " Gold: " + newLocation.QuestAvailableHere.RewardGold
                        + " Item: " + newLocation.QuestAvailableHere.RewardItem + Environment.NewLine;
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

            //Refresh player's weapons combobox
            List<Weapon> weapons = new List<Weapon>();

            foreach(InventoryItem ii in _player.Inventory)
            {
                if(ii.Details is Weapon)
                {
                    if(ii.Quantity > 0)
                    {
                        weapons.Add((Weapon)ii.Details);
                    }
                }
            }

            if(weapons.Count == 0)
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

            //Refresh players potions combobox
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach(InventoryItem ii in _player.Inventory)
            {
                if(ii.Details is HealingPotion)
                {
                    if(ii.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)ii.Details);
                    }
                }
            }

            if(healingPotions.Count == 0)
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

        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {

        }


        private void SuperAdventure_Load(object sender, EventArgs e)
        {

        }
    }
}
