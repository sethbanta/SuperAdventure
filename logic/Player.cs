using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public Player(int currentHitPoints, int maximumHitPoints, int gold, int experiencePoints, int level) : base(currentHitPoints, maximumHitPoints)
        {
            Gold = gold;
            ExperiencePoints = experiencePoints;
            Level = level;
            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
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
    }
}