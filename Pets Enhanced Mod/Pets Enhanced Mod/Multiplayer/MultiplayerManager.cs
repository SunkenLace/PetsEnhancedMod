using Pets_Enhanced_Mod.Utilities.Custom_Classes;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Pets_Enhanced_Mod.ModEntry;
using Microsoft.Xna.Framework;
using Pets_Enhanced_Mod.Utilities;
using StardewModdingAPI;
using StardewValley.Extensions;
using StardewValley.Mods;
using StardewValley.Characters;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Inventories;
using StardewValley.Network;
using Pets_Enhanced_Mod.Serializer;
using System.Runtime.InteropServices;

namespace Pets_Enhanced_Mod.Multiplayer
{
    /// <summary>Helps organize and sync information across all instances.</summary>
    public static class SynchronizationManager
    {
        public static readonly HashSet<long> FarmhandsReady = new();
        private static readonly object petInformationDataLock = new object();
        private static readonly List<PetInformation> recyclableInformationList = new();
        public static readonly Dictionary<Guid, PetInformation> CurrentClientInformation = new();
        public static void SendInformationToClients()
        {
            lock (petInformationDataLock)
            {
                var _allFarmers = Game1.otherFarmers;
                foreach (var farmer in _allFarmers)
                {
                    if (!string.IsNullOrEmpty(farmer.Value?.currentLocation?.NameOrUniqueName) && FarmhandsReady.Contains(farmer.Value.UniqueMultiplayerID))
                    {
                        recyclableInformationList.Clear();
                        foreach (var petPackage in CachePetData.PetCache)
                        {
                            if (!string.IsNullOrEmpty(petPackage.Value?.Pet?.CurrentLocation?.NameOrUniqueName) && petPackage.Value.Pet.CurrentLocation.NameOrUniqueName.EqualsIgnoreCase(farmer.Value.currentLocation.NameOrUniqueName))
                            {
                                recyclableInformationList.Add(new(petPackage.Value.Pet));
                            }
                        }
                        if (recyclableInformationList.Count > 0)
                        {
                            ModEntry.AHelper.Multiplayer.SendMessage(SerializerManager.SerializePets(recyclableInformationList), "UPetI00", ModIDAsArray, new long[1] { farmer.Value.UniqueMultiplayerID });
                        }
                    }
                }
            }
        }
        public static void Clear()
        {
            FarmhandsReady.Clear();
            recyclableInformationList.Clear();
            ExtrapolatePetPositions.Clear();
            CurrentClientInformation.Clear();
        }

        public static void PopulateCurrentClientInformation(string data)
        {
            CurrentClientInformation.Clear();
            var deserializedData = SerializerManager.DeserializeInto(data);
            foreach(var pet in deserializedData)
            {
                CurrentClientInformation.TryAdd(pet.ID, pet);
            }
        }

        public static readonly Dictionary<long, Dictionary<Guid, LatencyCompensatedPredictor>> ExtrapolatePetPositions = new();
        public static Vector2 ExtrapolateDeadReckoning(Guid _petID, Vector2 NewPosition, Vector2 velocity)
        {
            if (!ExtrapolatePetPositions.ContainsKey(Game1.player.UniqueMultiplayerID))
            {
                ExtrapolatePetPositions.TryAdd(Game1.player.UniqueMultiplayerID, new());
            }
            if (!ExtrapolatePetPositions[Game1.player.UniqueMultiplayerID].ContainsKey(_petID))
            {
                ExtrapolatePetPositions[Game1.player.UniqueMultiplayerID].TryAdd(_petID, new(NewPosition, velocity));
            }
            if (NewPosition != ExtrapolatePetPositions[Game1.player.UniqueMultiplayerID][_petID].originalPosition)
            {
                ExtrapolatePetPositions[Game1.player.UniqueMultiplayerID][_petID].OnReceiveServerUpdate(NewPosition, velocity);
            }
            var result = ExtrapolatePetPositions[Game1.player.UniqueMultiplayerID][_petID].CurrentPosition;
            ExtrapolatePetPositions[Game1.player.UniqueMultiplayerID][_petID].Update(velocity);
            return result;
        }



        //----------------------------------------------------------------------------------------------| ModData Area |----------------------------------------------------------------------------------------------

        public static void UpdateModDataForPet(SmartPet _forPet)
        {

            if (_forPet?.OriginalPetInstance?.modData is null) { return; }

            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_PetGlobalPetInventoryKey, _forPet.GlobalPetInventoryKey);
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_PetAge, _forPet._petInfo.Age);

            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_LeaderUniqueMultiplayerID, _forPet.GroupLeader?.UniqueMultiplayerID);

            var skillMasteryLevel = _forPet._petInfo.SkillMastery_level;
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_Waiting_SkillMasteryLevel, skillMasteryLevel[0]);
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_Following_SkillMasteryLevel, skillMasteryLevel[1]);
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_Foraging_SkillMasteryLevel, skillMasteryLevel[2]);
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_Fishing_SkillMasteryLevel, skillMasteryLevel[3]);
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_Hunting_SkillMasteryLevel, skillMasteryLevel[4]);
            uint skillPerkTierCheckListUint = 0;
            var skillPerkChecklistArray = _forPet._petInfo.SkillPerkChecklist;
            for (int i = 0; i < skillPerkChecklistArray.Length; i++)
            {
                for (int j = 0; j < skillPerkChecklistArray[i].Length; j++)
                {
                    SetBoolByte(i, j, skillPerkChecklistArray[i][j], ref skillPerkTierCheckListUint);
                }
            }
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_SkillPerkTierChecklist, skillPerkTierCheckListUint);

            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_PetLearnedNewSkill, _forPet.GetPetLearnedANewSkill());
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_NewItemObtained, _forPet.GetNewItemHasBeenObtained());
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_BackgroundStyleIndex, _forPet._petInfo.BackgroundStyle_Index);
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_PetEnergy, _forPet._petInfo.Energy);
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_CurrentPetObjective, (int)_forPet.CurrentPetObjective);

            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_HasBeenGivenTreatToday, _forPet._petInfo.HasBeenGivenTreatToday);
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_CanBeGivenBadTreat, _forPet.foodSaturationLevel.negative > -5);
            UpdateModDataIfDifferent(_forPet.OriginalPetInstance.modData, PetModDataKey_CanBeGivenGoodTreat, _forPet.foodSaturationLevel.positive < 6);
        }
        
        public static int? GetPetBackgroundStyleIndex(ModDataDictionary _dictionary)
        {

            if (_dictionary is null) { return null; }

            if (_dictionary.TryGetValue(PetModDataKey_BackgroundStyleIndex, out var bsi) && !string.IsNullOrEmpty(bsi) && int.TryParse(bsi,out int bsiParsed))
            {
                return bsiParsed;
            }
            return null;
        }

        public static void TryAddKnownEdibleItemToInventoryTag(Inventory toInventory,string edibleItemQUID)
        {
            if (toInventory.Count <= PetsEnhancedModInventoryKnownEdibleItemsBookIndex || toInventory[PetsEnhancedModInventoryKnownEdibleItemsBookIndex] is null)
            {
                TryAddItemListToInventory(toInventory,new List<Item>(1) { ItemRegistry.Create("(O)88", 1) }, PetsEnhancedModInventoryKnownEdibleItemsBookIndex); //(O)88 is coconut
            }
            string passcode = PetModDataKey_KnownEdibleItemIDPasscode + edibleItemQUID;
            if (!toInventory[PetsEnhancedModInventoryKnownEdibleItemsBookIndex].modData.ContainsKey(passcode))
            {
                toInventory[PetsEnhancedModInventoryKnownEdibleItemsBookIndex].modData.TryAdd(passcode, "IsKnown");
            }
        }
        public static bool IsItemQUIDKnownInInventoryBook(Inventory inInventory, string edibleItemQUID)
        {
            if (inInventory.Count > PetsEnhancedModInventoryKnownEdibleItemsBookIndex && !inInventory[PetsEnhancedModInventoryKnownEdibleItemsBookIndex].IsNull(out var knownEdibleItemsList))
            {
                if (knownEdibleItemsList.modData is not null && knownEdibleItemsList.modData.ContainsKey(PetModDataKey_KnownEdibleItemIDPasscode + edibleItemQUID))
                {
                    return true;
                }
            }
            return false;
        }


        //----------------------------------------------------------------------------------------------| Helper Methods |----------------------------------------------------------------------------------------------
        private static void UpdateModDataIfDifferent(ModDataDictionary dictionary, string key, Guid value)
        {
            if (dictionary.TryGetValue(key, out var _stringValue))
            {
                if (!Guid.TryParse(_stringValue, out Guid _valueParsed) || _valueParsed != value)
                {
                    dictionary[key] = value.ToString();
                }
            }
            else
            {
                dictionary.TryAdd(key, value.ToString());
            }
        }
        private static void UpdateModDataIfDifferent(ModDataDictionary dictionary, string key, bool value)
        {
            if (dictionary.TryGetValue(key, out var _stringValue))
            {
                if (value)
                {
                    if (_stringValue != "1")
                    {
                        dictionary[key] = "1";
                    }
                }
                else
                {
                    if (_stringValue != "0")
                    {
                        dictionary[key] = "0";
                    }
                }

            }
            else
            {
                dictionary.TryAdd(key, value ? "1" : "0");
            }
        }
        private static void UpdateModDataIfDifferent(ModDataDictionary dictionary,string key, int value)
        {
            if (dictionary.TryGetValue(key, out var _stringValue))
            {
                if (!int.TryParse(_stringValue, out int _valueParsed) || _valueParsed != value)
                {
                    dictionary[key] = value.ToString();
                }
            }
            else
            {
                dictionary.TryAdd(key, value.ToString());
            }
        }
        private static void UpdateModDataIfDifferent(ModDataDictionary dictionary, string key, uint value)
        {
            if (dictionary.TryGetValue(key, out var _stringValue))
            {
                if (!uint.TryParse(_stringValue, out uint _valueParsed) || _valueParsed != value)
                {
                    dictionary[key] = value.ToString();
                }
            }
            else
            {
                dictionary.TryAdd(key, value.ToString());
            }
        }
        private static void UpdateModDataIfDifferent(ModDataDictionary dictionary, string key, float value)
        {
            if (dictionary.TryGetValue(key, out var _stringValue))
            {
                if (!float.TryParse(_stringValue, out float _valueParsed) || _valueParsed != value)
                {
                    dictionary[key] = value.ToString();
                }
            }
            else
            {
                dictionary.TryAdd(key, value.ToString());
            }
        }
        private static void UpdateModDataIfDifferent(ModDataDictionary dictionary, string key, double value)
        {
            if (dictionary.TryGetValue(key, out var _stringValue))
            {
                if (!double.TryParse(_stringValue, out double _valueParsed) || _valueParsed != value)
                {
                    dictionary[key] = value.ToString();
                }
            }
            else
            {
                dictionary.TryAdd(key, value.ToString());
            }
        }
        private static void UpdateModDataIfDifferent(ModDataDictionary dictionary, string key, string value)
        {
            if (dictionary.TryGetValue(key, out var _stringValue))
            {
                if (_stringValue != value)
                {
                    dictionary[key] = value;
                }
            }
            else
            {
                dictionary.TryAdd(key, value);
            }
        }
        private static void UpdateModDataIfDifferent(ModDataDictionary dictionary, string key, long? value)
        {
            if (dictionary.TryGetValue(key, out var _stringValue))
            {
                if (value is null)
                {
                    if (_stringValue is not null)
                    {
                        dictionary[key] = null;
                    }
                }
                else if (_stringValue is null)
                {
                    if (value is not null)
                    {
                        dictionary[key] = value.Value.ToString();
                    }
                }
                else if (!long.TryParse(_stringValue, out long _valueParsed) || _valueParsed != value.Value)
                {
                    dictionary[key] = value.ToString();
                }
            }
            else
            {
                dictionary.TryAdd(key, value is null? null : value.Value.ToString());
            }
        }

        /// <summary>Check the dictionary and the key for null, else it will throw an exception. </summary>
        public static bool TryParseModData(ModDataDictionary dictionary, string key, out int result)
        {
            result = 0;
            if (dictionary.TryGetValue(key, out var valueString))
            {
                if (!string.IsNullOrEmpty(valueString) && int.TryParse(valueString, out var valueParsed))
                {
                    result = valueParsed;
                    return true;
                }
            }
            return false;
        }/// <summary>Check the dictionary and the key for null, else it will throw an exception. </summary>
        public static bool TryParseModData(ModDataDictionary dictionary, string key, out uint result)
        {
            result = 0;
            if (dictionary.TryGetValue(key, out var valueString))
            {
                if (!string.IsNullOrEmpty(valueString) && uint.TryParse(valueString, out var valueParsed))
                {
                    result = valueParsed;
                    return true;
                }
            }
            return false;
        }
        /// <summary>Check the dictionary and the key for null, else it will throw an exception. </summary>
        public static bool TryParseModData(ModDataDictionary dictionary, string key, out bool result)
        {
            result = false;
            if (dictionary.TryGetValue(key, out var valueString))
            {
                if (!string.IsNullOrEmpty(valueString))
                {
                    result = valueString == "1";
                    return true;
                }
            }
            return false;
        }
        /// <summary>Check the dictionary and the key for null, else it will throw an exception. </summary>
        public static bool TryParseModData(ModDataDictionary dictionary, string key, out double result)
        {
            result = 0;
            if (dictionary.TryGetValue(key, out var valueString))
            {
                if (!string.IsNullOrEmpty(valueString) && double.TryParse(valueString, out var valueParsed))
                {
                    result = valueParsed;
                    return true;
                }
            }
            return false;
        }
        /// <summary>Check the dictionary and the key for null, else it will throw an exception. </summary>
        public static bool TryParseModData(ModDataDictionary dictionary, string key, out long? result)
        {
            result = null;
            if (dictionary.TryGetValue(key, out var valueString))
            {
                if (string.IsNullOrEmpty(valueString))
                {
                    return true;
                }
                else if (long.TryParse(valueString, out var valueParsed))
                {
                    result = valueParsed;
                    return true;
                }
            }
            return false;
        }

        public static bool GetBoolByte(uint source ,int row, int col)
        {
            int bitPos = (row * 4) + col;
            return (source & (1u << bitPos)) != 0;
        }
        public static void SetBoolByte(int row, int col, bool value, ref uint _varToOverride)
        {
            if (row > 4 || col > 3) { return; }

            int bitPos = (row * 4) + col;
            if (value)
            {
                _varToOverride |= (1u << bitPos);
            }
            else
            {
                _varToOverride &= ~(1u << bitPos);
            }
        }
        public static void SetFlag(int bitIndex, bool value, ref byte _varToOverride)
        {
            byte mask = (byte)(1 << bitIndex); // Creates the "stencil"

            if (value)
            {
                _varToOverride |= mask;  // Set bit to 1
            }
            else
            {
                _varToOverride &= (byte)~mask; // Set bit to 0
            }
        }
        public static bool IsBitSet(int bitIndex, byte sourceVar)
        {
            byte mask = (byte)(1 << bitIndex);

            return (sourceVar & mask) != 0;
        }


        //----------------------------------------------------------------------------------------------| Structs Area |----------------------------------------------------------------------------------------------
        
        
        public struct SkillMasteryLevelStruct
        {
            public double WaitingSkillMastery;
            public double FollowingSkillMastery;
            public double ForagingSkillMastery;
            public double FishingSkillMastery;
            public double HuntingSkillMastery;
            public SkillMasteryLevelStruct(ModDataDictionary dic, int valueForErrorWhenLoading = 0)
            {
                if (dic is null) { WaitingSkillMastery = FollowingSkillMastery = ForagingSkillMastery = FishingSkillMastery = HuntingSkillMastery = valueForErrorWhenLoading; return; }

                if (!TryParseModData(dic, PetModDataKey_Waiting_SkillMasteryLevel, out WaitingSkillMastery))
                {
                    WaitingSkillMastery = valueForErrorWhenLoading;
                }
                if (!TryParseModData(dic, PetModDataKey_Following_SkillMasteryLevel, out FollowingSkillMastery))
                {
                    FollowingSkillMastery = valueForErrorWhenLoading;
                }
                if (!TryParseModData(dic, PetModDataKey_Foraging_SkillMasteryLevel, out ForagingSkillMastery))
                {
                    ForagingSkillMastery = valueForErrorWhenLoading;
                }
                if (!TryParseModData(dic, PetModDataKey_Fishing_SkillMasteryLevel, out FishingSkillMastery))
                {
                    FishingSkillMastery = valueForErrorWhenLoading;
                }
                if (!TryParseModData(dic, PetModDataKey_Hunting_SkillMasteryLevel, out HuntingSkillMastery))
                {
                    HuntingSkillMastery = valueForErrorWhenLoading;
                }
            }
            public static bool AllSkillsUnlocked(ModDataDictionary dic)
            {
                if (dic is null) { return false; }

                TryParseModData(dic, PetModDataKey_Waiting_SkillMasteryLevel, out double waitingSkillMastery);
                TryParseModData(dic, PetModDataKey_Following_SkillMasteryLevel, out double followingSkillMastery);
                TryParseModData(dic, PetModDataKey_Foraging_SkillMasteryLevel, out double foragingSkillMastery);
                TryParseModData(dic, PetModDataKey_Fishing_SkillMasteryLevel, out double fishingSkillMastery);
                TryParseModData(dic, PetModDataKey_Hunting_SkillMasteryLevel, out double huntingSkillMastery);

                return waitingSkillMastery >= 0 && followingSkillMastery >= 0 && foragingSkillMastery >= 0 && fishingSkillMastery >= 0 && huntingSkillMastery >= 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PetInformation
        {
            public Guid ID;
            public int PetPositionX;
            public int PetPositionY;
            public short VelocityX;
            public short VelocityY;
            public short CurrentEmoteFrame;
            public byte CurrentFrame;

            /// <summary>Bit 0: DrawFlip | Bit 1: IsBoardSinking | Bit 2: IsSwimming | Bit 3: IsEmoting | Bit 4: drawPet</summary>
            public byte Flags;
            public PetInformation(SmartPet pet)
            {
                this.ID = pet._petInfo.PetId;
                this.VelocityX = (short)(pet.Motion.X * 100f);
                this.VelocityY = (short)(pet.Motion.Y * 100f);
                this.PetPositionX = (int)pet.Position.X;
                this.PetPositionY = (int)pet.Position.Y;
                this.CurrentEmoteFrame = (short)pet.currentEmoteFrame;
                this.CurrentFrame = (byte)pet.Sprite.CurrentFrame;
                this.Flags = 0;
                SetFlag(0, pet.flip, ref this.Flags);
                SetFlag(1, pet.BoardSinking, ref this.Flags);
                SetFlag(2, pet.Swimming, ref this.Flags);
                SetFlag(3, pet.isEmoting, ref this.Flags);
                SetFlag(4, pet.CurrentPetObjective != SmartPet.PetObjective.None, ref this.Flags);
            }
        }

        public readonly struct PetSpriteInformation
        {
            public readonly bool DrawPet = false;
            public readonly Texture2D PetTexture = null;
            public readonly Vector2 PetDrawPosition = Vector2.Zero;
            public readonly Rectangle DrawPetSourceRect = Rectangle.Empty;
            public readonly Vector2 DrawPetOrigin = Vector2.Zero;
            public readonly bool DrawFlip = false;
            public readonly float PetDrawLayerDepth = 0;

            public readonly bool DrawHat = false;
            public readonly Texture2D HatTexture = null;
            public readonly Vector2 HatDrawPosition = Vector2.Zero;
            public readonly Rectangle HatDrawSourceRect = Rectangle.Empty;
            public readonly float HatDrawScale = 1;
            public readonly Color HatDrawColor = Color.White;
            public readonly float HatDrawLayerDepth = 0;

            public readonly bool DrawSwimmingBoard = false;
            public readonly Texture2D SwimmingBoardTexture = null;
            public readonly Vector2 SwimmingBoardDrawPosition = Vector2.Zero;
            public readonly Rectangle SwimmingBoardDrawSourceRect = Rectangle.Empty;
            public readonly float SwimmingBoardDrawLayerDepth = 0;

            public readonly bool DrawEmote = false;
            public readonly Vector2 EmoteDrawPosition = Vector2.Zero;
            public readonly Rectangle EmoteDrawSourceRect = Rectangle.Empty;
            public readonly float EmoteDrawLayerDepth = 0;

            public PetSpriteInformation(Pet _clientSideInfo, PetInformation dynamicInformation, bool extrapolate = true)
            {
                if (_clientSideInfo?.Sprite?.Texture is null) { return; }

                var petID = _clientSideInfo.petId.Value;

                this.PetTexture = _clientSideInfo.Sprite.Texture;

                var facingDirection = 0;
                var currentFrame = 0;
                var petType = PetHelper.GetPetTypeFromTextureHeightAndOpetType(_clientSideInfo.petType?.Value, _clientSideInfo.Sprite.Texture.Height);
                DrawFlip = IsBitSet(0, dynamicInformation.Flags);
                DrawPet = IsBitSet(4, dynamicInformation.Flags);
                if (!DrawPet)
                {
                    facingDirection = _clientSideInfo.FacingDirection;
                    currentFrame = _clientSideInfo.Sprite.CurrentFrame;
                    int OPETcAI = _clientSideInfo.Sprite.currentAnimationIndex;
                    DrawFlip = _clientSideInfo.flip || (OPETcAI >= 0 && _clientSideInfo.Sprite?.CurrentAnimation?[OPETcAI] is not null && _clientSideInfo.Sprite.CurrentAnimation[OPETcAI].flip);
                }
                else
                {
                    currentFrame = dynamicInformation.CurrentFrame;
                    facingDirection = SmartPet.GetActualPetDirection(currentFrame, petType, DrawFlip);
                }
                if (petType == PetInfo.Pet_Types.LegacyCat || petType == PetInfo.Pet_Types.EnhancedCat)
                {
                    if ((currentFrame >= 0 && currentFrame <= 23) || (currentFrame >= 59 && currentFrame <= 61))
                    {
                        DrawFlip = false;
                    }
                }
                else if (petType == PetInfo.Pet_Types.LegacyDog || petType == PetInfo.Pet_Types.EnhancedDog)
                {
                    if ((currentFrame >= 0 && currentFrame < 20) || currentFrame == 27 || (currentFrame >= 36 && currentFrame < 39) || currentFrame == 42)
                    {
                        DrawFlip = false;
                    }
                }



                PetDrawLayerDepth = _clientSideInfo.isSleepingOnFarmerBed.Value ? (((float)_clientSideInfo.StandingPixel.Y + 112f) + _clientSideInfo.StandingPixel.X / 20000f) / 10000f : ((float)_clientSideInfo.StandingPixel.Y + _clientSideInfo.StandingPixel.X / 20000f) / 10000f;
                bool isBoardSinking = IsBitSet(1, dynamicInformation.Flags);
                bool isSwimming = IsBitSet(2, dynamicInformation.Flags);
                int ySwimmingOffset = isBoardSinking? 4 : 0;
                Vector2 petPosition = _clientSideInfo.Position;
                int currentEmoteFrame = dynamicInformation.CurrentEmoteFrame;
                bool isEmoting = IsBitSet(3, dynamicInformation.Flags);

                petPosition = !DrawPet? _clientSideInfo.Position + new Vector2(32,0) : !extrapolate? new Vector2(dynamicInformation.PetPositionX, dynamicInformation.PetPositionY) : ExtrapolateDeadReckoning(petID, new Vector2(dynamicInformation.PetPositionX, dynamicInformation.PetPositionY), new Vector2((float)(dynamicInformation.VelocityX) * 0.01f, (float)(dynamicInformation.VelocityY) * 0.01f));

                var petBBox = new Rectangle((int)petPosition.X + 18, (int)petPosition.Y + 18, 28, 28);
                this.PetDrawPosition = petPosition + new Vector2(_clientSideInfo.Sprite.SpriteWidth, (petBBox.Height / 2) + (isBoardSinking == true ? 4 : 0)) + new Vector2(0f, 2f);
                var drawOffset = new Vector2(_clientSideInfo.Sprite.SpriteWidth, (petBBox.Height / 2) + (isBoardSinking == true ? 4 : 0)) + new Vector2(0f, 2f);

                int num = _clientSideInfo.Sprite.SpriteWidth;
                int num2 = _clientSideInfo.Sprite.SpriteHeight;
                int num3 = _clientSideInfo.Sprite.Texture.Width;
                int num4 = _clientSideInfo.Sprite.Texture.Height;
                DrawPetSourceRect = AnimatedSprite.GetSourceRect(num3, num, num2, currentFrame);
                if ((DrawPetSourceRect.Right > num3 || DrawPetSourceRect.Bottom > num4))
                {
                    DrawPetSourceRect = AnimatedSprite.GetSourceRect(num3, num, num2, 0);
                }

                DrawPetOrigin = new Vector2(_clientSideInfo.Sprite.SpriteWidth / 2, (float)_clientSideInfo.Sprite.SpriteHeight * 3f / 4f);
                PetDrawLayerDepth = ((float)(petBBox.Center.Y + 4) + petPosition.X / 20000f) / 10000f;

                string petIDstring = null;
                if (_clientSideInfo.modData.TryGetValue(PetModDataKey_PetGlobalPetInventoryKey,out var idString))
                {
                    petIDstring = idString;
                }

                if (petIDstring is not null && !Game1.player.team.GetOrCreateGlobalInventory(petIDstring).IsNull(out var petInventory) && GetHatForInventory(petInventory, out var hatItem))
                {
                    ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(hatItem.QualifiedItemId);
                    string text = dataOrErrorItem.GetTextureName();
                    int spriteIndex = dataOrErrorItem.SpriteIndex;
                    string textMerged = CacheReciclerHelper.ConcatenateStringEfficient(text,"_animals");
                    Texture2D _texture = PetHelper.TryLoadTextureEfficiently(textMerged);
                    if (_texture is null && text.StartsWith("JA")) { _texture = PetHelper.TryLoadTextureEfficiently(text); }
                    if (_texture is not null)
                    {
                        var petData = CachePetData.GetPetDataForPet(_clientSideInfo);
                        this.HatTexture = _texture;

                        Vector2 vectorOPI = ((_clientSideInfo.shakeTimer > 0 && !_clientSideInfo.isSleepingOnFarmerBed.Value) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero);
                        (Vector2 _position, int _direction, bool _drawHat, float drawScale) _hatOffsetValue = SmartPet.getHatOffsetForPet(CachePetData.GetHatOffsetFromID(_clientSideInfo.getPetTextureName(), petData is not null ? petData.HatOffSetID : Data.PetContent.nonRegisteredPetData_config.HatOffsetID, _clientSideInfo.petType?.Value, _clientSideInfo.whichBreed?.Value), this.DrawFlip, currentFrame, petType);
                        this.HatDrawSourceRect = new Rectangle(spriteIndex * 20 % _texture.Width, spriteIndex * 20 / _texture.Width * 20 * 4 + _hatOffsetValue._direction * 20, 20, 20);
                        this.HatDrawLayerDepth = PetDrawLayerDepth + 0.000002f;
                        this.HatDrawScale = 3 * _hatOffsetValue.drawScale;
                        this.DrawHat = _hatOffsetValue._drawHat;
                        this.HatDrawPosition = PetDrawPosition + (DrawPet? Vector2.Zero : vectorOPI) + _hatOffsetValue._position + new Vector2(0, DrawPet ? 0 : _clientSideInfo.yJumpOffset) + new Vector2(-26f, -44f);
                        bool isPrismatic = hatItem.isPrismatic?.Value ?? false;
                        this.HatDrawColor = isPrismatic ? (Utility.GetPrismaticColor() * 1f) : (Color.White * 1f);
                    }
                }

                int xSinkingSrcRectOffset = isBoardSinking? 32 : 0;
                if (isSwimming && !PetHelper.TryLoadTextureEfficiently("Mods\\SunkenLace.PetsEnhancedMod\\Textures\\SwimmingFloats").IsNull(out var _swimmingFloatTexture))
                {
                    this.DrawSwimmingBoard = true;
                    this.SwimmingBoardTexture = _swimmingFloatTexture;
                    this.SwimmingBoardDrawSourceRect = new Rectangle(petType == PetInfo.Pet_Types.EnhancedCat ? 192 + xSinkingSrcRectOffset : (facingDirection == 1 || facingDirection == 3) ? 64 + xSinkingSrcRectOffset : facingDirection == 0 ? 0 + xSinkingSrcRectOffset : 128 + xSinkingSrcRectOffset, SmartPet.GetBoardSpriteSrcYPosition(GetPetBackgroundStyleIndex(_clientSideInfo.modData) ?? 0), 32, 32);
                    var YSwimmingBoardOffset = (petType == PetInfo.Pet_Types.EnhancedCat ? 0 : facingDirection == 2 ? 20 : 12) - ySwimmingOffset;
                    this.SwimmingBoardDrawPosition = PetDrawPosition + new Vector2(0, YSwimmingBoardOffset + 2);
                    this.SwimmingBoardDrawLayerDepth = PetDrawLayerDepth - 0.000064f;
                }
                if (isEmoting)
                {
                    Point point = _clientSideInfo.GetPetData()?.EmoteOffset ?? Point.Zero;
                    DrawEmote = true;
                    EmoteDrawPosition = petPosition + new Vector2(point.X, point.Y - 96f + ySwimmingOffset); //new Vector2(localPosition.X + (float)point.X, localPosition.Y - 96f + (float)point.Y + ySwimmingOffset)
                    EmoteDrawSourceRect = new Rectangle(currentEmoteFrame * 16 % Game1.emoteSpriteSheet.Width, currentEmoteFrame * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16);
                    EmoteDrawLayerDepth = PetDrawLayerDepth + 0.0001f;
                }
            }

            public void DrawSprite(SpriteBatch spriteBatch, Vector2 viewport)
            {
                if (DrawSwimmingBoard)
                {
                    spriteBatch.Draw(SwimmingBoardTexture, SwimmingBoardDrawPosition - viewport, SwimmingBoardDrawSourceRect, Color.White, 0, DrawPetOrigin, 4f, DrawFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, SwimmingBoardDrawLayerDepth);
                }
                if (DrawPet)
                {
                    spriteBatch.Draw(PetTexture, PetDrawPosition - viewport, DrawPetSourceRect, Color.White, 0, DrawPetOrigin, 4f, DrawFlip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, PetDrawLayerDepth);
                }
                if (DrawHat)
                {
                    spriteBatch.Draw(HatTexture, HatDrawPosition - viewport, HatDrawSourceRect, HatDrawColor, 0, new Vector2(3, 3), HatDrawScale, SpriteEffects.None, HatDrawLayerDepth);
                }
                if (DrawEmote)
                {
                    spriteBatch.Draw(Game1.emoteSpriteSheet, EmoteDrawPosition - viewport, EmoteDrawSourceRect, Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, EmoteDrawLayerDepth);
                }
            }
        }



        //----------------------------------------------------------------------------------------------| Inventory Management Area |----------------------------------------------------------------------------------------------

        public static void InitializePetInventory(PetInfo _info)
        {
            if (_info is null) { return; }

            var petInventory = Game1.player.team.GetOrCreateGlobalInventory(PetsEnhancedModPetInventoryStoragePasscode + _info.PetId);

            if (string.IsNullOrEmpty(_info.PetInventorySerialized)) { return; }

            SPetInventory? inventoryDeserialized = null;
            try
            {
                inventoryDeserialized = PetHelper.DeSerializeString<SPetInventory>(_info.PetInventorySerialized);
                inventoryDeserialized?.fixInventory();
            }
            catch (Exception ex)
            {
                inventoryDeserialized = null;
                ModEntry.WriteMonitor($"error while deserializing pet inventory. Details:{ex}", LogLevel.Error);
            }

            if (inventoryDeserialized is null) { return;}

            if (petInventory.Count < 30)
            {
                int maxValue = 30 - petInventory.Count;
                for (int i = 0; i < maxValue; i++)
                {
                    petInventory.Add(null);
                }
            }

            if (inventoryDeserialized.Value.Hat is not null)
            {
                petInventory[PetsEnhancedModInventoryHatSlotIndex] = inventoryDeserialized.Value.Hat;
            }
            if (inventoryDeserialized.Value.Accessory is not null)
            {
                petInventory[PetsEnhancedModInventoryAccessorySlotIndex] = inventoryDeserialized.Value.Accessory;
            }
            if (inventoryDeserialized.Value.PocketItem is not null)
            {
                petInventory[PetsEnhancedModInventoryPetPocketSlotIndex] = inventoryDeserialized.Value.PocketItem;
            }
            for (int i = 0; i < inventoryDeserialized.Value.Backpack.Count; i++)
            {
                if (inventoryDeserialized.Value.Backpack[i] is null) { continue;}

                petInventory[i + PetsEnhancedModInventoryBackpackStartIndex] = inventoryDeserialized.Value.Backpack[i];
            }

        }
        public static Item TryGetPetAccessory(Pet fromPet)
        {
            if (fromPet.modData.TryGetValue(PetModDataKey_PetGlobalPetInventoryKey, out string keyParsed) && !string.IsNullOrEmpty(keyParsed))
            {
                var petInventory = Game1.player.team.GetOrCreateGlobalInventory(keyParsed);

                if (petInventory is null) { return null; }

                if (petInventory.Count == 0)
                {
                    petInventory.Add(null);
                    petInventory.Add(null);
                }
                else if (petInventory.Count == 1)
                {
                    petInventory.Add(null);
                }

                return petInventory[PetsEnhancedModInventoryAccessorySlotIndex];
            }
            return null;
        }
        public static Inventory TryGetPetInventory(string inventoryKey)
        {
            Inventory petInventory = null;
            if (!string.IsNullOrEmpty(inventoryKey))
            {
                petInventory = Game1.player.team.GetOrCreateGlobalInventory(inventoryKey);
            }
            if (petInventory is not null && petInventory.Count < 30)
            {
                int maxValue = 30 - petInventory.Count;
                for (int i = 0; i < maxValue; i++)
                {
                    petInventory.Add(null);
                }
            }
            return petInventory;
        }
        public static Inventory TryGetPetInventory(Pet pet)
        {
            Inventory petInventory = null;
            if (!pet.modData.TryGetValue(PetModDataKey_PetGlobalPetInventoryKey, out string inventoryKey)) { return null; }

            if (!string.IsNullOrEmpty(inventoryKey))
            {
                petInventory = Game1.player.team.GetOrCreateGlobalInventory(inventoryKey);
            }
            if (petInventory is not null && petInventory.Count < 30)
            {
                int maxValue = 30 - petInventory.Count;
                for (int i = 0; i < maxValue; i++)
                {
                    petInventory.Add(null);
                }
            }
            return petInventory;
        }
        public static Inventory TryGetPetInventoryWithMutex(Pet pet, out NetMutex mutex)
        {
            mutex = null;
            Inventory petInventory = null;
            if (!pet.modData.TryGetValue(PetModDataKey_PetGlobalPetInventoryKey, out string inventoryKey)) { return null; }

            if (!string.IsNullOrEmpty(inventoryKey))
            {
                petInventory = Game1.player.team.GetOrCreateGlobalInventory(inventoryKey);
                mutex = Game1.player.team.GetOrCreateGlobalInventoryMutex(inventoryKey);
            }
            if (petInventory is not null && petInventory.Count < 30)
            {
                int maxValue = 30 - petInventory.Count;
                for (int i = 0; i < maxValue; i++)
                {
                    petInventory.Add(null);
                }
            }
            return petInventory;
        }
        public static bool GetHatForInventory(Inventory inventory, out StardewValley.Objects.Hat hat)
        {
            if (inventory is not null && inventory.Count > PetsEnhancedModInventoryHatSlotIndex && !inventory[PetsEnhancedModInventoryHatSlotIndex].IsNull(out var _item) && _item is StardewValley.Objects.Hat hatItem)
            {
                hat = hatItem;
                return true;
            }
            hat = null;
            return false;
        }
        public static bool GetItemFromInventoryAtIndex(Inventory inventory, int index, out Item item)
        {
            if (inventory is not null && inventory.Count > index && !inventory[index].IsNull(out var _item))
            {
                item = _item;
                return true;
            }
            item = null;
            return false;
        }

        public static void TryAddItemListToInventory(Inventory toInventory,List<Item> fromList, int startIndex)
        {
            if (toInventory is null || fromList is null || startIndex < 0) { return; }

            int maxCount = startIndex + fromList.Count;
            if (toInventory.Count < maxCount)
            {
                int maxValue = maxCount - toInventory.Count;
                for (int i = 0; i < maxValue; i++)
                {
                    toInventory.Add(null);
                }
            }

            for (int index = 0; index < fromList.Count; index++)
            {
                toInventory[index + startIndex] = fromList[index];
            }
        }
        public static List<Item> TryGetItemListFromInventory(Inventory fromInventory, List<Item> toList, int startIndex)
        {
            if (fromInventory is null || toList is null || startIndex < 0) { return toList; }

            int maxCount = startIndex + toList.Count;
            if (fromInventory.Count < maxCount)
            {
                int maxValue = maxCount - fromInventory.Count;
                for (int i = 0; i < maxValue; i++)
                {
                    fromInventory.Add(null);
                }
            }

            for (int index = 0; index < toList.Count; index++)
            {
                toList[index] = fromInventory[index + startIndex];
            }

            return toList;
        }

        /// <summary>Tries to add an item to the backpack (if unlocked) or the pocket slot.</summary><returns>If the item was fully added "null", else the item with its stack reduced.</returns>
        public static Item TryAddItemToPetBackpackOrPocketSlot(Inventory toInventory, Item _item)
        {
            if (_item is null) { return null; }

            Item residualItem = null;
            if (toInventory.Count > SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex && toInventory[SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex]?.QualifiedItemId == SmartPet.PetBackpackQUID)
            {
                var inventoryList = SynchronizationManager.TryGetItemListFromInventory(toInventory, new List<Item>(6) { null, null, null, null, null, null }, SynchronizationManager.PetsEnhancedModInventoryBackpackStartIndex);
                residualItem = Utility.addItemToThisInventoryList(_item, inventoryList, 6);

                SynchronizationManager.TryAddItemListToInventory(toInventory, inventoryList, SynchronizationManager.PetsEnhancedModInventoryBackpackStartIndex);
            }
            else
            {
                var pocketSlotList = SynchronizationManager.TryGetItemListFromInventory(toInventory, new List<Item>(1) { null }, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
                residualItem = Utility.addItemToThisInventoryList(_item, pocketSlotList, 1);

                SynchronizationManager.TryAddItemListToInventory(toInventory, pocketSlotList, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
            }
            return residualItem;
        }


        //----------------------------------------------------------------------------------------------| Constants Area |----------------------------------------------------------------------------------------------


        public const string PetsEnhancedModPetInventoryStoragePasscode = "PetsEnhancedMod.PetInventoryStorage:";

        public const string PetModDataKey_PetGlobalPetInventoryKey = "PetsEnhancedMod.ModData.GlobalPetInventoryKey";
        public const string PetModDataKey_PetAge = "PetsEnhancedMod.ModData.PetAge";
        public const string PetModDataKey_LeaderUniqueMultiplayerID = "PetsEnhancedMod.ModData.SmartPet.LeaderUniqueMultiplayerID";
        public const string PetModDataKey_Waiting_SkillMasteryLevel = "PetsEnhancedMod.ModData.SmartPet.Waiting_SkillMasteryLevel";
        public const string PetModDataKey_Following_SkillMasteryLevel = "PetsEnhancedMod.ModData.SmartPet.Following_SkillMasteryLevel";
        public const string PetModDataKey_Foraging_SkillMasteryLevel = "PetsEnhancedMod.ModData.SmartPet.Foraging_SkillMasteryLevel";
        public const string PetModDataKey_Fishing_SkillMasteryLevel = "PetsEnhancedMod.ModData.SmartPet.Fishing_SkillMasteryLevel";
        public const string PetModDataKey_Hunting_SkillMasteryLevel = "PetsEnhancedMod.ModData.SmartPet.Hunting_SkillMasteryLevel";
        public const string PetModDataKey_SkillPerkTierChecklist = "PetsEnhancedMod.ModData.SmartPet.SkillPerkTierChecklist";
        public const string PetModDataKey_PetLearnedNewSkill = "PetsEnhancedMod.ModData.SmartPet.LearnedNewSkill";
        public const string PetModDataKey_NewItemObtained = "PetsEnhancedMod.ModData.SmartPet.NewItemObtained";
        public const string PetModDataKey_BackgroundStyleIndex = "PetsEnhancedMod.ModData.SmartPet.BackgroundStyleIndex";
        public const string PetModDataKey_PetEnergy = "PetsEnhancedMod.ModData.SmartPet.Energy";
        public const string PetModDataKey_CurrentPetObjective = "PetsEnhancedMod.ModData.SmartPet.CurrentPetObjective";
        public const string PetModDataKey_HasBeenGivenTreatToday = "PetsEnhancedMod.ModData.SmartPet.HasBeenGivenTreatToday";
        public const string PetModDataKey_CanBeGivenBadTreat = "PetsEnhancedMod.ModData.SmartPet.CanBeGivenBadTreat";
        public const string PetModDataKey_CanBeGivenGoodTreat = "PetsEnhancedMod.ModData.SmartPet.CanBeGivenGoodTreat";

        public const string PetModDataKey_KnownEdibleItemIDPasscode = "PetsEnhancedMod.ModData.EdibleItem:";

        public const int PetsEnhancedModInventoryHatSlotIndex = 0;
        public const int PetsEnhancedModInventoryAccessorySlotIndex = 1;
        public const int PetsEnhancedModInventoryPetPocketSlotIndex = 2;
        public const int PetsEnhancedModInventoryItemCouldntFitOnInventoryIndex = 7;
        public const int PetsEnhancedModInventoryGiftedItemIndex = 8;
        public const int PetsEnhancedModInventoryKnownEdibleItemsBookIndex = 9;
        public const int PetsEnhancedModInventoryBackpackStartIndex = 10;
    }
}
