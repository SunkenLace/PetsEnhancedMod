using System;
using System.Collections.Generic;
using StardewValley;
using StardewModdingAPI;
using StardewValley.Characters;
using static StardewValley.Minigames.CraneGame;
using StardewValley.GameData.Pets;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using System.Threading;
using static Pets_Enhanced_Mod.Utilities.SunkenLaceUtilities;
using System.Xml.Serialization;
using StardewValley.Objects;
using StardewValley.Tools;

namespace Pets_Enhanced_Mod.Utilities.Custom_Classes
{
    public class PetInfo
    {
        public Guid PetId { get; set; }

        public int Age { get; set; } = 28;
        public string Name = "";
        public enum Pet_Types
        {
            LegacyDog = 0,
            EnhancedDog = 1,
            LegacyCat = 2,
            EnhancedCat = 3,
            Undefined = 4,
        }
        public Pet_Types PetType = Pet_Types.Undefined;

        public string PetInventorySerialized;
        public bool HasBeenGivenTreatToday = false;
        public string[] KnownEdibleItems = Array.Empty<string>();

        /// <summary>Whether this pet is ready to be trained/teached based on the ammount of days elapsed since last trained</summary>
        public bool TrainedToday { get; set; } = false;
        public long? showTrickLearningDialogueTo { get; set; } = null;

        ///<summary>Defines the background style showed at the Pet Status Window.</summary>
        public int BackgroundStyle_Index = Game1.random.Next(16);

        /// <summary>Index: 0 = Wait skill, 1 = Follow skill, 2 = Forage skill, 3 = Fishing skill, 4 = Hunt skill,</summary>
        public double[] SkillMastery_level = new[] {-1d,-1d,-1d,-1d,-1d};

        /// <summary>Index: 0 = Wait skill, 1 = Follow skill, 2 = Forage skill, 3 = Fishing skill, 4 = Hunt skill,</summary>
        public bool[][] SkillPerkChecklist = new[] { new[] { false, false, false, false }, new[] { false, false, false, false }, new[] { false, false, false, false }, new[] { false, false, false, false }, new[] { false, false, false, false } };

        public bool AllSkillsLearned(bool _HasWaitSkill, bool _HasFollowSkill, bool _HasForageSkill, bool _HasFishingSkill, bool _HasHuntSkill)
        {
            bool _result = (_HasWaitSkill && SkillMastery_level[0] < 1) || ((_HasFollowSkill && SkillMastery_level[1] < 1) || !_HasFollowSkill) || (_HasForageSkill && SkillMastery_level[2] < 1) || (_HasFishingSkill && SkillMastery_level[3] < 1) || (_HasHuntSkill && SkillMastery_level[4] < 1) ? false : true;
            return _result;
        }

        public bool SkillsUnlocked => SkillMastery_level[0] >= 0 && SkillMastery_level[1] >= 0 && SkillMastery_level[2] >= 0 && SkillMastery_level[3] >= 0 && SkillMastery_level[4] >= 0;

        ///<summary>Stat used to perform actions.</summary>
        public int Energy = SmartPet.MaxBaseEnergyNoUpgrade;

        public int MaxBaseEnergy = SmartPet.MaxBaseEnergyNoUpgrade;

    }

    public struct SPetInventory
    {
        public List<Item> Backpack = new List<Item>() { null, null, null, null, null, null };
        public bool AllBackpackSpotsOcupied
        {
            get
            {
                if (Backpack is not null)
                {
                    bool allOcupied = true;
                    var index = 0;
                    while (index < Backpack.Count)
                    {
                        if (Backpack[index] is null)
                        {
                            allOcupied = false;
                            break;
                        }
                        index++;
                    }
                    return allOcupied;
                }
                return true;
            }
        }
        public void fixInventory()
        {
            PocketItemAsList = new() { ItemRegistry.GetMetadata(PocketItemAsList[0]?.QualifiedItemId) is not null ? PocketItemAsList[0] : PocketItemAsList[0] is not null? ItemRegistry.Create(PocketItemAsList[0]?.QualifiedItemId, PocketItemAsList[0].Stack) : null };
            HatAsList = new() { ItemRegistry.GetMetadata(HatAsList[0]?.QualifiedItemId) is not null ? HatAsList[0] : HatAsList[0] is not null ? ItemRegistry.Create(HatAsList[0]?.QualifiedItemId, HatAsList[0].Stack) : null };
            AccessoryAsList = new() { ItemRegistry.GetMetadata(AccessoryAsList[0]?.QualifiedItemId) is not null ? AccessoryAsList[0] : AccessoryAsList[0] is not null ? ItemRegistry.Create(AccessoryAsList[0]?.QualifiedItemId, AccessoryAsList[0].Stack) : null };
            for (int i = 0; i < Backpack.Count; i++)
            {
                Backpack[i] = ItemRegistry.GetMetadata(Backpack[i]?.QualifiedItemId) is not null ? Backpack[i] : Backpack[i] is not null ? ItemRegistry.Create(Backpack[i]?.QualifiedItemId, Backpack[i].Stack) : null;
            }
        }
        public bool DoesBackpackContainAnyItem
        {
            get
            {
                if (Backpack is not null)
                {
                    bool doesIt = false;
                    var index = 0;
                    while (index < Backpack.Count)
                    {
                        if (Backpack[index] is not null)
                        {
                            doesIt = true;
                            break;
                        }
                        index++;
                    }
                    return doesIt;
                }
                return false;
            }
        }
        public bool BackpackUnlocked
        {
            get
            {
                //Replace this later with backpack type.
                return PocketItem is not null && PocketItem.QualifiedItemId.Equals("(O)SunkenLace.PetsEnhancedMod.PetBackpack");
            }
        }
        public Item PocketItem = null;
        public List<Item> PocketItemAsList
        {
            get
            {
                return new List<Item>() { this.PocketItem };
            }
            set
            {
                this.PocketItem = value[0];
            }
        }
        public Hat Hat = null;
        public List<Item> HatAsList
        {
            get
            {
                return new List<Item>() { this.Hat };
            }
            set
            {
                this.Hat = value[0] as Hat;
            }
        }

        public Item Accessory = null;
        public List<Item> AccessoryAsList
        {
            get
            {
                return new List<Item>() { this.Accessory };
            }
            set
            {
                this.Accessory = value[0];
            }
        }

        public SPetInventory(List<Item> _inventory = null, Hat _hat = null, Item _accessory = null, Item _pocketItem = null)
        {
            if (_inventory is not null)
            {
                this.Backpack = _inventory;
            }
            this.Hat = _hat;
            this.Accessory = _accessory;
            this.PocketItem = _pocketItem;
        }

        public Item TryAddHat(Item hat)
        {
            if (hat as Hat is not null && this.Hat is null)
            {
                this.Hat = hat as Hat;
                return null;
            }
            return hat;
        }
        public Item TryAddAccessory(Item acc)
        {
            List<Item> iList = this.AccessoryAsList;
            Item item = Utility.addItemToThisInventoryList(acc, iList, 1);
            this.AccessoryAsList = iList;

            return item;
        }
        public Item TryAddPocketItem(Item pItem)
        {
            List<Item> iList = this.PocketItemAsList;
            Item item = Utility.addItemToThisInventoryList(pItem, iList, 1);
            this.PocketItemAsList = iList;
            return item;
        }
        public Item TryAddItemToBackpack(Item item)
        {
            return Utility.addItemToThisInventoryList(item, this.Backpack, 6);
        }
    }


    public class PE_Pet_Data
    {
        public PE_Pet_Data() { }
        public PE_Pet_Data(string hatOffSetID, float baseCooldownTime, int minDamage, int maxDamage, float critChance, string attackEffect, bool hasWaitSkill, bool hasFollowSkill, bool hasHuntSkill, bool hasForageSkill, bool hasFishingSkill, string dietListID, string _trickLearningTreat, bool isViciousType)
        {
            this.MinDamage = minDamage;
            this.MaxDamage = maxDamage;
            this.BaseCooldownTime = baseCooldownTime;
            this.CritChance = critChance;
            this.HatOffSetID = hatOffSetID;
            this.HasWaitSkill = hasWaitSkill;
            this.HasFollowSkill = hasFollowSkill;
            this.HasHuntSkill = hasHuntSkill;
            this.HasForageSkill = hasForageSkill;
            this.HasFishingSkill = hasFishingSkill;
            this.DietListID = dietListID;
            this.TrickLearningTreat = _trickLearningTreat;
            this.AttackEffect = attackEffect;
            this.IsViciousType = isViciousType;
        }

        public string TrickLearningTreat { get; set; }

        public string HatOffSetID { get; set; }

        public float BaseCooldownTime { get; set; }


        public int MinDamage { get; set; }



        public int MaxDamage { get; set; }



        public float CritChance { get; set; }


        public bool IsViciousType { get; set; }



        public string AttackEffect { get; set; }


        public bool HasWaitSkill { get; set; }


        public bool HasFollowSkill { get; set; }


        public bool HasForageSkill { get; set; }


        public bool HasHuntSkill { get; set; }


        public bool HasFishingSkill { get; set; }


        public string DietListID { get; set; }
    }
}

