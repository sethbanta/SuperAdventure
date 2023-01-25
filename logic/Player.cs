using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace logic
{
    public class Player : LivingCreature
    {
        public int Gold { get; set; }
        public int ExperiencePoints { get; set; }
        public int Level { get; set; }
        public Location CurrentLocation { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }
        private Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints, int level) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;
            Level = level;
            Level = GetPlayerLevel(experiencePoints);
            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }
        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();
                playerData.LoadXml(xmlPlayerData);
                //Get current hit points
                int currentHitPoints =
                    Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHitPoints").InnerText);
                //Get max hit points
                int maximumHitPoints =
                    Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHitPoints").InnerText);
                //Get gold
                int gold =
                    Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                //Get experience
                int experiencePoints =
                    Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/ExperiencePoints").InnerText);
                //A player is ready to be created
                Player player = new Player(currentHitPoints, maximumHitPoints, gold, experiencePoints, 1);
                int currentLocationID =
                    Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationByID(currentLocationID);
                //for each inventory item in the xml, add it to the players inventory
                foreach(XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);
                    for (int i = 0; i < quantity; i++)
                    {
                        player.AddItemToInventory(World.ItemByID(id));
                    }
                }
                //for each quest in the xml, give it to the player
                foreach(XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest")) 
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);
                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
                    playerQuest.IsCompleted = isCompleted;
                    player.Quests.Add(playerQuest);
                }
                return player;
            }
            catch
            {
                return Player.CreateDefaultPlayer();
            }
        }

        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10, 10, 20, 0, 1);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);
            return player;
        }

        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null) //if no required item to enter this location
            {
                return true;
            }
            //See if the player has the item required in their inventory
            foreach (InventoryItem ii in Inventory)
            {
                if (ii.Details.ID == location.ItemRequiredToEnter.ID) //if the item is found
                {
                    return true;
                }
            }
            //Didnt find the item
            return false;
        }

        public bool HasThisQuest(Quest quest)
        {
            foreach (PlayerQuest pq in Quests)
            {
                if (pq.Details.ID == quest.ID)
                {
                    return true;
                }
            }
            return false;
        }
        public bool CompletedThisQuest(Quest quest)
        {
            foreach (PlayerQuest pq in Quests)
            {
                if (pq.Details.ID == quest.ID)
                {
                    if (pq.IsCompleted)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public bool HasAllQuestCompletionItems(Quest quest)
        {
            //See if they have the items needed to complete the quest here
            foreach(QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                bool foundItemInPlayersInventory = false;
                //Check each item in their inventory, and how much of it
                foreach(InventoryItem ii in Inventory)
                {
                    if(ii.Details.ID == qci.Details.ID) //player has the item
                    {
                        foundItemInPlayersInventory = true;
                        if(ii.Quantity < qci.Quantity) //not enough
                        {
                            return false;
                        }
                    }
                }
                //No item found
                if(!foundItemInPlayersInventory)
                {
                    return false;
                }
            }
            //If we got here, then the player must have the item, and enough of it
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach(QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                foreach(InventoryItem ii in Inventory)
                {
                    if(ii.Details.ID == qci.Details.ID) //found the item(s) in inventory
                    {
                        ii.Quantity -= qci.Quantity;
                        break;
                    }
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            foreach(InventoryItem ii in Inventory)
            {
                if(ii.Details.ID == itemToAdd.ID) //if they have the item already
                {
                    ii.Quantity++;
                    return;
                }
            }
            //getting here means they didnt have the item, add it
            Inventory.Add(new InventoryItem(itemToAdd, 1));
        }

        public void RemoveItemFromInventory(Item itemToRemove)
        {
            foreach(InventoryItem ii in Inventory)
            {
                if(ii.Details.ID == itemToRemove.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }
        }

        public void MarkQuestCompleted(Quest quest)
        {
            foreach(PlayerQuest pq in Quests)
            {
                if(pq.Details.ID == quest.ID)
                {
                    pq.IsCompleted = true;
                    return;
                }
            }
        }

        public int GetPlayerLevel(int experiencePoints)
        {
            if (experiencePoints < 25)
            {
                this.Level = 1;
                return 1;
            } else if (experiencePoints < 50)
            {
                this.Level = 2;
                return 2;
            } else if (experiencePoints < 75)
            {
                this.Level = 3;
                return 3;
            } else if (experiencePoints < 100)
            {
                this.Level = 4;
                return 4;
            }
            //if code reaches here they are > 100 xp so they are max level
            this.Level = 5;
            return 5;
        }

        public string ToXmlString()
        {
            XmlDocument playerData = new XmlDocument();
            //Create top level XML node
            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);
            //Create stats child node
            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);
            //Create children of stats currenthp, maxhp, gold, xp, currentlocation
            XmlNode currentHitPoints = playerData.CreateElement("CurrentHitPoints");
            //places text in the "middle" <CurrentHitPoints>7</CurrentHitPoints> i.e. 7
            currentHitPoints.AppendChild(playerData.CreateTextNode(this.CurrentHitPoints.ToString()));
            //places currentHitPoints in stats: <Stats>
            //                                          <CurrentHitPoints>7</CurrentHitPoints>
            //                                  </Stats>
            stats.AppendChild(currentHitPoints);
            //max hp
            XmlNode maximumHitPoints = playerData.CreateElement("MaximumHitPoints");
            maximumHitPoints.AppendChild(playerData.CreateTextNode(this.MaximumHitPoints.ToString()));
            stats.AppendChild(maximumHitPoints);
            //gold
            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(this.Gold.ToString()));
            stats.AppendChild(gold);
            //xp
            XmlNode experiencePoints = playerData.CreateElement("ExperiencePoints");
            experiencePoints.AppendChild(playerData.CreateTextNode(this.ExperiencePoints.ToString()));
            stats.AppendChild(experiencePoints);
            //currentlocation
            XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerData.CreateTextNode(this.CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            //Create inventoryitems section
            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);
            //cycle through each item in the inventory and append it to the inventory items section with its ID and quantity
            foreach(InventoryItem ii in this.Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");
                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = ii.Details.ID.ToString();
                inventoryItem.Attributes.Append(idAttribute);
                XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                quantityAttribute.Value = ii.Quantity.ToString();
                inventoryItem.Attributes.Append(quantityAttribute);
                inventoryItems.AppendChild(inventoryItem);
            }
            /*
             *     <PlayerQuests>
        <PlayerQuest ID="1" IsCompleted="true" />
        <PlayerQuest ID="2" IsCompleted="false" />
    </PlayerQuests>
             */
            //Create player quests child node
            XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);
            //cycle through every quest and append
            foreach(PlayerQuest pq in this.Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");
                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = pq.Details.ID.ToString();
                playerQuest.Attributes.Append(idAttribute);
                XmlAttribute isCompletedAttribute = playerData.CreateAttribute("IsCompleted");
                isCompletedAttribute.Value = pq.IsCompleted.ToString();
                playerQuest.Attributes.Append(isCompletedAttribute);
                playerQuests.AppendChild(playerQuest);
            }
            return playerData.InnerXml; // Returns the xml document as a string so we can save the data
        }
    }
}