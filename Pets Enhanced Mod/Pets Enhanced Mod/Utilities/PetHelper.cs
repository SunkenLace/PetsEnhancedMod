using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StardewModdingAPI;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Characters;
using Pets_Enhanced_Mod.Utilities.Custom_Classes;
using static Pets_Enhanced_Mod.Utilities.Custom_Classes.SmartPet;
using StardewValley.Monsters;
using System.IO;
using StardewValley.Locations;
using Pets_Enhanced_Mod.Data;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using StardewValley.Extensions;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Pets;
using Microsoft.Xna.Framework.Graphics;
using static Pets_Enhanced_Mod.Utilities.CachePetData;
using System.Reflection;
using StardewValley.Buildings;
using StardewValley.BellsAndWhistles;
using Netcode;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Util;
using xTile.Layers;
using xTile.Tiles;
using xTile;
using System.Runtime.InteropServices;
using StardewValley.Constants;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Quests;
using System.Text.Json;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework.Media;
using Pets_Enhanced_Mod.Multiplayer;

namespace Pets_Enhanced_Mod.Utilities
{
    internal class PetHelper
    {
        public const string AlternateTextures_paint_Bucket_ItemQUID = "(T)PeacefulEnd.AlternativeTextures_PaintBucket";

        public const string AlternateTextures_scissors_ItemQUID = "(T)PeacefulEnd.AlternativeTextures_Scissors";

        public const string AlternateTextures_paint_brush_ItemQUID = "(T)PeacefulEnd.AlternativeTextures_PaintBrush";

        public const string AlternateTextures_spray_can_ItemQUID = "(T)PeacefulEnd.AlternativeTextures_SprayCan";

        public const string AlternateTextures_spray_can_rare_ItemQUID = "(T)PeacefulEnd.AlternativeTextures_SprayCanRare";

        public const string AlternateTextures_catalogue_ItemQUID = "(T)PeacefulEnd.AlternativeTextures_Catalogue";

        public static bool ItemQUIDisAlternateTexturesItem(string _itemQUID)
        {
            return _itemQUID == AlternateTextures_catalogue_ItemQUID || _itemQUID == AlternateTextures_paint_brush_ItemQUID || _itemQUID == AlternateTextures_paint_Bucket_ItemQUID || _itemQUID == AlternateTextures_scissors_ItemQUID || _itemQUID == AlternateTextures_spray_can_ItemQUID || _itemQUID == AlternateTextures_spray_can_rare_ItemQUID;
        }
        public static float GetDistance(Vector2 a, Vector2 b)
        {
            return (float)Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
        }
        public static float GetDistance(Point a, Point b)
        {
            return (float)Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
        }
        public static T DeSerializeString<T>(string serializedData)
        {
            T result = default(T);
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(serializedData))
            {
                result = (T)serializer.Deserialize(reader);
            }
            return result;
        }
        public static string SerializeObject<T>(T _obj)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            var result = "";
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, _obj);
                result = writer.ToString();
            }
            return result;
        }
        public static Texture2D TryLoadTextureEfficiently(string textureName)
        {
            if (string.IsNullOrEmpty(textureName)) { return null; }
            if (!CachePetData.TextureCache.ContainsKey(textureName))
            {
                if (Game1.content.DoesAssetExist<Texture2D>(textureName))
                {
                    try { CachePetData.TextureCache.TryAdd(textureName, Game1.content.Load<Texture2D>(textureName)); } catch { }
                }
                else { return null; }
            }
            return CachePetData.TextureCache[textureName];
        }
        public static bool FarmerCaughtFish(Farmer _farmer,string itemId, int size, bool from_fish_pond = false, int numberCaught = 1)
        {
            ItemMetadata metadata = ItemRegistry.GetMetadata(itemId);
            itemId = metadata.QualifiedItemId;
            bool num = !from_fish_pond && metadata.Exists() && !ItemContextTagManager.HasBaseTag(metadata.QualifiedItemId, "trash_item") && !(itemId == "(O)167") && (metadata.GetParsedData()?.ObjectType == "Fish" || metadata.QualifiedItemId == "(O)372");
            bool result = false;
            if (num)
            {
                if (_farmer.fishCaught.TryGetValue(itemId, out var value))
                {
                    value[0] += numberCaught;
                    if (size > _farmer.fishCaught[itemId][1])
                    {
                        value[1] = size;
                        result = true;
                    }

                    _farmer.fishCaught[itemId] = value;
                }
                else
                {
                    _farmer.fishCaught.Add(itemId, new int[2] { numberCaught, size });
                    _farmer.autoGenerateActiveDialogueEvent("fishCaught_" + metadata.LocalItemId);
                }

                _farmer.NotifyQuests((Quest quest) => quest.OnFishCaught(itemId, numberCaught, size));
                if (Utility.GetDayOfPassiveFestival("SquidFest") > 0 && itemId == "(O)151")
                {
                    Game1.stats.Increment(StatKeys.SquidFestScore(Game1.dayOfMonth, Game1.year), numberCaught);
                }
            }

            return result;
        }
        public static int BobberPointDistanceToLand(int tileX, int tileY, GameLocation location, bool landMustBeAdjacentToWalkableTile = false)
        {
            Rectangle r = new Rectangle(tileX - 1, tileY - 1, 3, 3);
            bool foundLand = false;
            int distance = 1;
            while (!foundLand && r.Width <= 11)
            {
                foreach (Vector2 v in Utility.getBorderOfThisRectangle(r))
                {
                    if (!location.isTileOnMap(v) || location.isWaterTile((int)v.X, (int)v.Y))
                    {
                        continue;
                    }
                    foundLand = true;
                    distance = r.Width / 2;
                    if (!landMustBeAdjacentToWalkableTile)
                    {
                        break;
                    }
                    foundLand = false;
                    Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(v);
                    foreach (Vector2 surroundings in surroundingTileLocationsArray)
                    {
                        if (location.isTilePassable(surroundings) && !location.isWaterTile((int)v.X, (int)v.Y))
                        {
                            foundLand = true;
                            break;
                        }
                    }
                    break;
                }
                r.Inflate(1, 1);
            }
            if (r.Width > 11)
            {
                distance = 6;
            }
            return distance - 1;
        }
        public static int GetMovementSpeed(float distance, float maxSpeed, float acceleration, float decelerationRange)
        {
            if (distance <= 0 || maxSpeed <= 0 || acceleration <= 0 || decelerationRange <= 0)
            {
                return 0;
            }

            // Calculate the deceleration distance
            float decelerationDistance = maxSpeed / decelerationRange;

            // Check if target is within deceleration range
            if (distance <= decelerationDistance)
            {
                // Decelerate towards target
                return (int)(maxSpeed * (distance / decelerationDistance));
            }
            else if (distance <= maxSpeed * acceleration + decelerationDistance)
            {
                // Accelerate towards max speed
                float timeToMaxSpeed = maxSpeed / acceleration;
                float distanceToMaxSpeed = timeToMaxSpeed * maxSpeed / 2;

                if (distance <= distanceToMaxSpeed)
                {
                    // Accelerate proportionally to distance
                    return (int)(acceleration * distance);
                }
                else
                {
                    // Maintain max speed
                    return (int)maxSpeed;
                }
            }
            else
            {
                // Maintain max speed
                return (int)maxSpeed;
            }
        }
        public static void SendSPetToPlayer(Farmer who, string responseKey)
        {
            if (who is not null && !responseKey.Equals("Cancel"))
            {
                CurrentPlayerClientPerformAnimationForPetFlute(responseKey);
            }
        }
        /// <summary>Get whether the given button is equivalent to <see cref="Options.useToolButton"/>.</summary>
        /// <param name="input">The button.</param>
        public static bool IsUseToolButton(SButton input)
        {
            bool result = input == SButton.ControllerX;
            if (!result)
            {
                for (int i = 0;i < Game1.options.useToolButton.Length; i++)
                {
                    if (Game1.options.useToolButton[i].ToSButton() == input)
                    {
                        return true;
                    }
                }
            }
            return result;
        }

        /// <summary>Get whether the given button is equivalent to <see cref="Options.actionButton"/>.</summary>
        /// <param name="input">The button.</param>
        public static bool IsActionButton(SButton input)
        {
            bool result = input == SButton.ControllerA;
            if (!result)
            {
                for (int i = 0; i < Game1.options.actionButton.Length; i++)
                {
                    if (Game1.options.actionButton[i].ToSButton() == input)
                    {
                        return true;
                    }
                }
            }
            return result;
        }
        public static bool isTileFishable(GameLocation _location,int tileX, int tileY, bool acceptFishPonds = true)
        {
            if (_location == null) { return false; }
            if (!acceptFishPonds && IsFishingTilePond(new(tileX, tileY),_location))
            {
                return false;
            }
            if (_location.isTileBuildingFishable(tileX, tileY))
            {
                return true;
            }

            if (!_location.isWaterTile(tileX, tileY) || _location.doesTileHaveProperty(tileX, tileY, "NoFishing", "Back") != null || _location.hasTileAt(tileX, tileY, "Buildings"))
            {
                return _location.doesTileHaveProperty(tileX, tileY, "Water", "Buildings") != null;
            }

            return true;
        }
        public static bool IsFishingTilePond(Vector2 fishingTile, GameLocation location)
        {
            if (location == null) { return false; }
            if (fishingTile != Vector2.Zero)
            {
                foreach (Building building in location.buildings)
                {
                    if (building is FishPond pond && pond.isTileFishable(fishingTile))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static void CurrentPlayerClientPerformAnimationForPetFlute(string petID)
        {
            Game1.player.faceDirection(2);
            Game1.MusicDuckTimer = 2000f;
            Game1.sounds.PlayAll("horse_flute",Game1.player.currentLocation, Game1.player.Tile, null, context: StardewValley.Audio.SoundContext.Default);
            Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[6]
            {
                new FarmerSprite.AnimationFrame(98, 400, secondaryArm: true, flip: false),
                        new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false),
                        new FarmerSprite.AnimationFrame(100, 200, secondaryArm: true, flip: false),
                        new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false),
                        new FarmerSprite.AnimationFrame(98, 400, secondaryArm: true, flip: false),
                        new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false)
            });
            Game1.player.freezePause = 1500;
            DelayedAction.functionAfterDelay(delegate
            {
                IfClientSendMessageElseDoAction(Game1.player,() => sendPetToPlayerWhoCalled(Game1.player, petID),petID, "AskHostToSendPetToPlayerForPetFluteCall", new[] { ModEntry.MainPlayerID });
            }, 1500);

        }
        public static T GetRandomItemFromWeightedList<T>((int Weight, T _item)[] weightedList)
        {
            if (weightedList is null || weightedList.Length == 0)
            {
                throw new ArgumentException("Weighted list cannot be empty.", nameof(weightedList));
            }
            int randomN = Game1.random.Next(0, weightedList.Sum(t => t.Weight));
            int currentWeight = 0;

            foreach(var item in weightedList)
            {
                if (randomN >= currentWeight && randomN < currentWeight + item.Weight)
                {
                    return item._item;
                }
                currentWeight += item.Weight;
            }
            return default(T);

        }
        public static void sendPetToPlayerWhoCalled(Farmer who, string petId)
        {
            foreach (var petInfoKit in CachePetData.PetCache.Values)
            {
                if (petInfoKit is not null && petInfoKit.Pet.GroupLeader is null && petInfoKit.Info.PetId.ToString().Equals(petId))
                {
                    var positionToWarp = GetPositionForCallingPet(who);

                    petInfoKit.Pet.OriginalPetInstance.Sprite.CurrentAnimation = null;
                    petInfoKit.Pet.OriginalPetInstance.Sprite.CurrentFrame = 0;
                    petInfoKit.Pet.OriginalPetInstance.CurrentBehavior = "Walk";

                    petInfoKit.Pet.animations.resetAnimations();
                    petInfoKit.Pet.PrevPetObjective = petInfoKit.Pet.CurrentPetObjective;
                    petInfoKit.Pet.CurrentPetObjective = PetObjective.Follow;
                    petInfoKit.Pet.Sprite.CurrentAnimation = null;
                    petInfoKit.Pet.Sprite.CurrentFrame = 0;
                    petInfoKit.Pet.ResetTargets();
                    petInfoKit.Pet.petSearchCooldown = 0;
                    petInfoKit.Pet.petSearchPatienceTimer = 0;
                    petInfoKit.Pet.pathToFollow.Clear();
                    if (!petInfoKit.Pet.CurrentLocation.NameOrUniqueName.EqualsIgnoreCase(who.currentLocation.NameOrUniqueName))
                    {
                        petInfoKit.Pet.CurrentLocation = who.currentLocation;
                        petInfoKit.Pet.Position = positionToWarp + new Vector2(-16, -16);
                    }
                    if (petInfoKit.Pet.OriginalPetInstance.currentLocation is not FarmHouse)
                    {
                        Game1.warpCharacter(petInfoKit.Pet.OriginalPetInstance, Utility.getHomeOfFarmer(Game1.player), new Vector2(-5000, 5000));
                    }
                    petInfoKit.Pet.Position = positionToWarp + new Vector2(-16, -16);
                    petInfoKit.Pet.OriginalPetInstance.Position = new Vector2(-5000, -5000);
                    petInfoKit.Pet.Sprite.UpdateSourceRect();
                    petInfoKit.Pet.OriginalPetInstance.Sprite.UpdateSourceRect();
                    CachePetData.CachePetTeams.RelocatePetToTeamOtherwiseCreateNew(petInfoKit.Info.PetId, who.UniqueMultiplayerID);

                    PetHelper.PlaySoundForAllPlayersAtFarmerLocation("wand", petInfoKit.Pet.Tile, who);
                    int indexU = -1;
                    while (++indexU < 8)
                    {
                        Game1.Multiplayer.broadcastSprites(petInfoKit.Pet.CurrentLocation, new TemporaryAnimatedSprite(10, new Vector2(petInfoKit.Pet.Tile.X + Utility.RandomFloat(-1f, 1f), petInfoKit.Pet.Tile.Y + Utility.RandomFloat(-1f, 0f)) * 64f, Color.White, 8, flipped: false, 50f)
                        {
                            layerDepth = 1f,
                            motion = new Vector2(Utility.RandomFloat(-0.5f, 0.5f), Utility.RandomFloat(-0.5f, 0.5f))
                        });
                    }
                    int j = -1;
                    while (++j < 8)
                    {
                        Game1.Multiplayer.broadcastSprites(petInfoKit.Pet.CurrentLocation, new TemporaryAnimatedSprite(10, new Vector2(petInfoKit.Pet.Tile.X + Utility.RandomFloat(-1f, 1f), petInfoKit.Pet.Tile.Y + Utility.RandomFloat(-1f, 0f)) * 64f, Color.White, 8, flipped: false, 50f)
                        {
                            layerDepth = 1f,
                            motion = new Vector2(Utility.RandomFloat(-0.5f, 0.5f), Utility.RandomFloat(-0.5f, 0.5f))
                        });
                    }
                    int num = 0;
                    int num2 = (int)petInfoKit.Pet.Tile.X + 2;
                    while (--num2 >= (int)petInfoKit.Pet.Tile.X - 2)
                    {
                        Game1.Multiplayer.broadcastSprites(petInfoKit.Pet.CurrentLocation, new TemporaryAnimatedSprite(6, new Vector2((float)num2, petInfoKit.Pet.Tile.Y) * 64f, Color.White, 8, flipped: false, 50f)
                        {
                            layerDepth = 1f,
                            delayBeforeAnimationStart = num * 25,
                            motion = new Vector2(-0.25f, 0f)
                        });
                        num++;
                    }

                    break;
                }
            }
        }
        public static Vector2 GetPositionForCallingPet(Farmer _player, int searchArea = 100, int maxAttempts = 25)
        {
            Rectangle playerBBox = _player.GetBoundingBox();
            Vector2 result = new Vector2(playerBBox.X,playerBBox.Y);
            Vector2 playerTile = (result / Game1.tileSize);

            int searchPositiveBounds = (int)(Math.Sqrt(searchArea) * 0.5d);
            int searchNegativeBounds = searchPositiveBounds * -1;

            for (int i = 0; i < maxAttempts; i++)
            {
                int randomTileX = (int)playerTile.X + Game1.random.Next(searchNegativeBounds, searchPositiveBounds);
                int randomTileY = (int)playerTile.Y + Game1.random.Next(searchNegativeBounds, searchPositiveBounds);
                if (!PetsEnhancedPathfindHelper.IsCollidingTilePosition(randomTileX, randomTileY, _player.currentLocation, false, false))
                {
                    result = new Vector2(randomTileX * Game1.tileSize, randomTileY * Game1.tileSize);
                    break;
                }
            }
            return result;
        }
        public static bool CheckIfPetConfigExistAnValid(PetInfo.Pet_Types petType,string _texturePath, out PE_Pet_Data data )
        {
            data = null;
            try
            {
                if (Game1.content.DoesAssetExist<Dictionary<string, PetConfigData>>("Mods\\SunkenLace.PetsEnhancedMod\\PetConfig"))
                {
                    var dic = Game1.content.Load<Dictionary<string, PetConfigData>>("Mods\\SunkenLace.PetsEnhancedMod\\PetConfig");
                    foreach (var _data in dic)
                    {
                        if (!string.IsNullOrEmpty(_data.Key) && DelocalizePath(_data.Key) == DelocalizePath(_texturePath))
                        {
                            return FixPetConfigData(petType, _data.Value, out data);
                        }
                    }
                }
            }
            catch {}
            return false;
        }
        public static T ConvertWithStream<T>(object incomingObject)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (Utf8JsonWriter writer = new Utf8JsonWriter(stream))
                    {
                        JsonSerializer.Serialize(writer, incomingObject);
                    }
                    stream.Position = 0;
                    return JsonSerializer.Deserialize<T>(stream);
                }
            }
            catch (Exception ex){ ModEntry.WriteMonitor($"Error while converting values. Details: {ex}", LogLevel.Error); return default; }
        }
        public static bool CheckIfSymaHatsOnPetsPlusHatConfigExistAnValid(string _petType,string breedID, out Dictionary<int, ModEntry.HatOffset_Simple> data)
        {
            data = null;
            if (string.IsNullOrEmpty(breedID) || string.IsNullOrEmpty(_petType)) { return false; }
            try
            {
                if (Game1.content.DoesAssetExist<Dictionary<string, object>>("Syma.HatsOnPetsPlus\\CustomPetData"))
                {
                    object dictionaryRaw = Game1.content.Load<object>("Syma.HatsOnPetsPlus\\CustomPetData");
                    var dicCooked = ConvertWithStream<Dictionary<string, RefAPIs.SymaHatsOnPetsPlusData.ExternalPetModData[]>>(dictionaryRaw);
                    if (dicCooked != default && dicCooked != null)
                    {
                        foreach (var _data in dicCooked)
                        {
                            if (_data.Value is not null && _data.Value.Length > 0)
                            {
                                for (int uIndex = 0; uIndex < _data.Value.Length; uIndex++)
                                {
                                    var uIndexObject = _data.Value[uIndex];
                                    if (uIndexObject is not null && uIndexObject.Type == _petType)
                                    {
                                        if (string.IsNullOrEmpty(uIndexObject.BreedId))
                                        {
                                            if (uIndexObject.BreedIdList is not null && uIndexObject.BreedIdList.Length > 0)
                                            {
                                                for (int i = 0; i < uIndexObject.BreedIdList.Length; i++)
                                                {
                                                    if (uIndexObject.BreedIdList[i] == breedID && uIndexObject.Sprites is not null)
                                                    {
                                                        data = ConvertSymaHatsOnPetsHatConfigToHatOffsetDictionary(uIndexObject.Sprites);
                                                        return data is not null;
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (uIndexObject.BreedId == breedID && uIndexObject.Sprites is not null)
                                            {
                                                data = ConvertSymaHatsOnPetsHatConfigToHatOffsetDictionary(uIndexObject.Sprites);
                                                return data is not null;
                                            }

                                        }

                                    }

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { ModEntry.WriteMonitor($"Error while loading Syma.HatsOnPetsPlus asset. Details: {ex}", LogLevel.Error); }
            return false;
        }
        public static Dictionary<int,ModEntry.HatOffset_Simple> ConvertSymaHatsOnPetsHatConfigToHatOffsetDictionary(RefAPIs.SymaHatsOnPetsPlusData.ExternalSpriteModData[] _data)
        {
            Dictionary<int, ModEntry.HatOffset_Simple> hatOffset = new();

            foreach (var hOset in _data)
            {

                if (!hatOffset.ContainsKey(hOset.SpriteId))
                {
                    int tempDir = 0;
                    if (hOset.Direction is not null)
                    {
                        tempDir = hOset.Direction.Value;
                    }
                    float tempX = 0;
                    if (hOset.HatOffsetX is not null)
                    {
                        tempX = (hOset.HatOffsetX.Value * 0.25f);
                    }
                    float tempY = 0;
                    if (hOset.HatOffsetY is not null)
                    {
                        tempY = (hOset.HatOffsetY.Value * 0.25f) - 7f;
                    }
                    bool tempDrawHat = true;
                    if (hOset.DoNotDrawHat is not null)
                    {
                        tempDrawHat = !hOset.DoNotDrawHat.Value;
                    }
                    float tempScale = 1f;
                    if (hOset.Scale is not null)
                    {
                        tempScale = hOset.Scale.Value / 1.3333334f;
                    }
                    hatOffset.Add(hOset.SpriteId, new(hOset.SpriteId, tempX, tempY,tempDir, tempDrawHat, tempScale));
                }

            }
            return hatOffset;
        }
        public static string DelocalizePath(string _og)
        {
            if (_og is null) {return "";}
            bool flag = false;
            char c = ((Environment.OSVersion.Platform == PlatformID.Win32NT) ? '\\' : '/');
            StringBuilder stringBuilder = new StringBuilder(_og.Length + 4);
            for (int i = 0; i < _og.Length; i++)
            {
                char c2 = _og[i];
                if (c2 == '/' || c2 == '\\')
                {
                    if (flag)
                    {
                        continue;
                    }

                    c2 = c;
                    flag = true;
                }
                else
                {
                    flag = false;
                }

                stringBuilder.Append(c2);
            }
            return stringBuilder.ToString();
        }
        public static bool FixPetConfigData(PetInfo.Pet_Types _petType,PetConfigData data, out PE_Pet_Data _result)
        {
            int petType = (_petType == PetInfo.Pet_Types.LegacyDog || _petType == PetInfo.Pet_Types.EnhancedDog) ? 0 : (_petType == PetInfo.Pet_Types.LegacyCat || _petType == PetInfo.Pet_Types.EnhancedCat) ? 1 : -1;
            bool hasWaitSkill = data.HasWaitSkill is not null ? data.HasWaitSkill.Value : petType == 1 ? PetContent.catPetData_config.HasWaitSkill.Value : petType == 0 ? PetContent.dog0PetData_config.HasWaitSkill.Value : true;
            bool hasFollowSkill = data.HasFollowSkill is not null ? data.HasFollowSkill.Value : petType == 1 ? PetContent.catPetData_config.HasFollowSkill.Value : petType == 0 ? PetContent.dog0PetData_config.HasFollowSkill.Value : true;
            bool hasHuntSkill = data.HasHuntSkill is not null ? data.HasHuntSkill.Value : petType == 1 ? PetContent.catPetData_config.HasHuntSkill.Value : petType == 0 ? PetContent.dog0PetData_config.HasHuntSkill.Value : false;
            bool hasForageSkill = data.HasForageSkill is not null ? data.HasForageSkill.Value : petType == 1 ? PetContent.catPetData_config.HasForageSkill.Value : petType == 0 ? PetContent.dog0PetData_config.HasForageSkill.Value : false;
            bool hasFishingSkill = data.HasFishingSkill is not null ? data.HasFishingSkill.Value : petType == 1 ? PetContent.catPetData_config.HasFishingSkill.Value : petType == 0 ? PetContent.dog0PetData_config.HasFishingSkill.Value : false;

            string trickLearningTreat = petType == 1 ? PetContent.DefaultCatTrickLearningTreat : PetContent.DefaultDogTrickLearningTreat;
            if (data.TrickLearningTreat is not null)
            {
                trickLearningTreat = data.TrickLearningTreat;
            }
            string dietListID = petType == 1? "cat" : "dog";

            if (data.DietListID is not null)
            {
                dietListID = data.DietListID;
            }
            float baseCooldownTime = data.BaseCooldownTime is not null? data.BaseCooldownTime.Value : petType == 1 ? PetContent.catPetData_config.BaseCooldownTime.Value : PetContent.dog0PetData_config.BaseCooldownTime.Value;
            int minDamage = data.MinDamage is not null ? data.MinDamage.Value : petType == 1 ? PetContent.catPetData_config.MinDamage.Value : PetContent.dog0PetData_config.MinDamage.Value;
            int maxDamage = data.MaxDamage is not null ? data.MaxDamage.Value : petType == 1 ? PetContent.catPetData_config.MaxDamage.Value : PetContent.dog0PetData_config.MaxDamage.Value;
            float critChance = data.CritChance is not null ? data.CritChance.Value : petType == 1 ? PetContent.catPetData_config.CritChance.Value : PetContent.dog0PetData_config.CritChance.Value;
            var isViciousType = data.IsViciousType is not null ? data.IsViciousType.Value : petType == 1 ? PetContent.catPetData_config.IsViciousType.Value : PetContent.dog0PetData_config.IsViciousType.Value;
            var attackEffect = data.AttackEffect is not null ? data.AttackEffect : petType == 1 ? PetContent.catPetData_config.AttackEffect : PetContent.dog0PetData_config.AttackEffect;
            if (minDamage >= maxDamage)
            {
                maxDamage = minDamage + 1;
            }


            var hatOffsetID = petType == 1 ? PetContent.catPetData_config.HatOffsetID : PetContent.dog0PetData_config.HatOffsetID;

            if (data.HatOffsetID is not null)
            {
                hatOffsetID = data.HatOffsetID;
            }
            
            _result = new(hatOffsetID, baseCooldownTime, minDamage, maxDamage, critChance, attackEffect, hasWaitSkill: hasWaitSkill, hasFollowSkill: hasFollowSkill, hasHuntSkill: hasHuntSkill, hasForageSkill: hasForageSkill, hasFishingSkill: hasFishingSkill, dietListID, trickLearningTreat, isViciousType);
            return true;
        }
        public static PetInfo.Pet_Types GetPetTypeFromTextureHeightAndOpetType(string _oPet_petType,int _textureHeight)
        {
            bool _isCat = _oPet_petType == "Cat" || _oPet_petType == "cat";
            bool _isDog = _oPet_petType == "Dog" || _oPet_petType == "dog";
            PetInfo.Pet_Types petType = PetInfo.Pet_Types.Undefined;
            if (_isCat && _textureHeight == 512)
            {
                petType = PetInfo.Pet_Types.EnhancedCat;
            }
            else if (_isCat)
            {
                petType = PetInfo.Pet_Types.LegacyCat;
            }
            else if (_isDog && _textureHeight == 352)
            {
                petType = PetInfo.Pet_Types.EnhancedDog;
            }
            else if (_isDog)
            {
                petType = PetInfo.Pet_Types.LegacyDog;
            }
            if (petType == PetInfo.Pet_Types.Undefined)
            {
                petType = _textureHeight == 288 ? PetInfo.Pet_Types.LegacyDog : PetInfo.Pet_Types.LegacyCat;
            }
            return petType;
        }
        public static void SmartPetSwap(Pet _original, PetInfo petInfo)
        {
            if (_original is null || petInfo is null) { return; }
            Guid _id = petInfo.PetId;
            var tPet = new SmartPet((int)_original.Tile.X, (int)_original.Tile.Y, _original, petInfo);
            CachePetData.PetCache.TryAdd(_id, new(petInfo, tPet));
            SynchronizationManager.InitializePetInventory(petInfo);

            if (!_original.GetPetData().IsNull(out var vPetData) && Utility.CreateDaySaveRandom(_original.timesPet.Value, 71928.0, _original.petId.Value.GetHashCode()).NextDouble() < (double)vPetData.GiftChance && vPetData.Gifts is not null)
            {
                var inventory = SynchronizationManager.TryGetPetInventory(SynchronizationManager.PetsEnhancedModPetInventoryStoragePasscode + petInfo.PetId);

                Item itemG = _original.TryGetGiftItem(vPetData.Gifts);
                if (itemG is not null && inventory is not null)
                {
                    var discardedItem = SynchronizationManager.TryAddItemToPetBackpackOrPocketSlot(inventory, itemG);
                    if (discardedItem is not null)
                    {
                        SynchronizationManager.TryAddItemListToInventory(inventory, new List<Item>() { discardedItem }, SynchronizationManager.PetsEnhancedModInventoryItemCouldntFitOnInventoryIndex);
                    }
                    else { tPet.SetNewItemHasBeenObtained(true); }
                }
            }

            ModEntry.WriteMonitor($"Pet swaped, its texture is {_original.getPetTextureName()}", LogLevel.Trace);
        }
        public static void PlaySoundForAllPlayersAtFarmerLocation(string sound,Vector2 tilePosition,Farmer emmissorFarmer, int? pitch = null)
        {
            if (string.IsNullOrEmpty(sound)) { return; }
            Farmer _alternateFarmer = emmissorFarmer ?? Game1.player;
            
            if (_alternateFarmer is not null)
            {
                IfClientSendMessageElseDoAction(_alternateFarmer, () => Game1.sounds.PlayAll(sound, _alternateFarmer.currentLocation, tilePosition, pitch, context: StardewValley.Audio.SoundContext.Default), new KeyValuePair<string, KeyValuePair<Vector2, int?>>(sound, new(tilePosition, pitch)), "PEM_ClientPlayAllSound", new[] { _alternateFarmer.UniqueMultiplayerID });
            }
        }
        public static void IfClientSendMessageElseDoAction<T>(Farmer who, Action _action, T _messageType, string _messageName, long[]? _toPlayers = null)
        {
            if (who is null) { return; }
            if (!who.IsMainPlayer)
            {
                ModEntry.AHelper.Multiplayer.SendMessage<T>(_messageType, _messageName, ModEntry.ModIDAsArray, _toPlayers);
                return;
            }
            _action();
        }
        public static PetInfo GetPetInfo(Guid petID, out Pet _pet)
        {
            _pet = PetHelper.GetPet(petID);
            if (_pet is null || !Context.IsWorldReady || TryLoadTextureEfficiently(_pet.getPetTextureName()).IsNull(out var texture)) {  return null; }
            PetInfo.Pet_Types petType = GetPetTypeFromTextureHeightAndOpetType(_pet.petType?.Value, texture.Height);
            if (!ModConfig_Helper.ModDataLibrary.TryGetValue(petID, out var _petInfo))
            {
                ModConfig_Helper.ModDataLibrary.TryAdd(petID, new PetInfo());
                ModConfig_Helper.ModDataLibrary[petID].PetId = petID;
                ModConfig_Helper.ModDataLibrary[petID].PetType = petType;
                ModConfig_Helper.ModDataLibrary[petID].Energy = MaxBaseEnergyNoUpgrade;
                ModConfig_Helper.ModDataLibrary[petID].Name = _pet.Name;
                if (ModConfig_Helper.oldModDataLibrary is not null && ModConfig_Helper.oldModDataLibrary.ContainsKey(petID) && ModConfig_Helper.oldModDataLibrary[petID] is not null)
                {
                    ModConfig_Helper.ModDataLibrary[petID].Age = ModConfig_Helper.oldModDataLibrary[petID].ageInDays + 28;
                    if (ModConfig_Helper.oldModDataLibrary[petID].petTricksUnlocked || ModConfig_Helper.oldModDataLibrary[petID].petManual.TricksUnlocked)
                    {
                        ModConfig_Helper.ModDataLibrary[petID].SkillMastery_level[0] = Math.Clamp(ModConfig_Helper.oldModDataLibrary[petID].petManual.WaitTrickUnlockPercent, 0, 1);
                        ModConfig_Helper.ModDataLibrary[petID].SkillMastery_level[1] = Math.Clamp(ModConfig_Helper.oldModDataLibrary[petID].petManual.FollowMeTrickUnlockPercent, 0, 1);
                        ModConfig_Helper.ModDataLibrary[petID].SkillMastery_level[2] = Math.Clamp(ModConfig_Helper.oldModDataLibrary[petID].petManual.SearchTrickUnlockPercent, 0, 1);
                        ModConfig_Helper.ModDataLibrary[petID].SkillMastery_level[3] = 0;
                        ModConfig_Helper.ModDataLibrary[petID].SkillMastery_level[4] = Math.Clamp(ModConfig_Helper.oldModDataLibrary[petID].petManual.HuntTrickUnlockPercent,0,1);
                    }
                    ModEntry.WriteMonitor($"v0.2.3 progress has been recovered for pet {_pet.Name}.", LogLevel.Debug);
                }

                return ModConfig_Helper.ModDataLibrary[petID];
            }
            _petInfo.PetType = petType;
            return _petInfo;
        }
        public static int TargetPetDistance(SmartPet _pet, Vector2 _targetTile) => (int)Utility.distance(_pet.Tile.X, _targetTile.X, _pet.Tile.Y, _targetTile.Y);
        public static List<Guid> GetAllPetIds(List<Guid> _listForUse)
        {
            _listForUse.Clear();
            var farm = Game1.getFarm();
            for (int i = 0; i < farm.characters.Count; i++)
            {
                if (farm.characters[i] is Pet item && item.petId?.Value is Guid id)
                {
                    _listForUse.Add(id);
                }
            }

            var _allFarmers = Game1.otherFarmers;
            foreach (var _item in _allFarmers)
            {
                if (Utility.getHomeOfFarmer(_item.Value) is FarmHouse house)
                {
                    for (int i = 0; i < house.characters.Count; i++)
                    {
                        if (house.characters[i] is Pet item && item.petId?.Value is Guid id)
                        {
                            _listForUse.Add(id);
                        }
                    }
                }

            }
            if (Utility.getHomeOfFarmer(Game1.player) is FarmHouse Mhouse)
            {
                for (int i = 0; i < Mhouse.characters.Count; i++)
                {
                    if (Mhouse.characters[i] is Pet item && item.petId?.Value is Guid id)
                    {
                        _listForUse.Add(id);
                    }
                }
            }

            return _listForUse;
        }
        public static void GetAllPets(List<Pet> _listForUse)
        {
            _listForUse.Clear();
            var farm = Game1.getFarm();
            for (int i = 0; i < farm.characters.Count; i++)
            {
                if (farm.characters[i] is Pet item && item.petId?.Value is Guid id)
                {
                    _listForUse.Add(item);
                }
            }

            var _allFarmers = Game1.otherFarmers;
            foreach (var _item in _allFarmers)
            {
                if (Utility.getHomeOfFarmer(_item.Value) is FarmHouse house)
                {
                    for (int i = 0; i < house.characters.Count; i++)
                    {
                        if (house.characters[i] is Pet item && item.petId?.Value is Guid id)
                        {
                            _listForUse.Add(item);
                        }
                    }
                }

            }
            if (Utility.getHomeOfFarmer(Game1.player) is FarmHouse Mhouse)
            {
                for (int i = 0; i < Mhouse.characters.Count; i++)
                {
                    if (Mhouse.characters[i] is Pet item && item.petId?.Value is Guid id)
                    {
                        _listForUse.Add(item);
                    }
                }
            }
        }
        public static Pet GetPet(Guid id)
        {
            var farm = Game1.getFarm();
            for (int i = 0; i < farm.characters.Count; i++)
            {
                if (farm.characters[i] is Pet item && item.petId?.Value == id)
                {
                    return item;
                }
            }

            foreach (Farmer f in Game1.getAllFarmers())
            {
                if (Utility.getHomeOfFarmer(f) is FarmHouse house)
                {
                    for (int i = 0; i < house.characters.Count; i++)
                    {
                        if (house.characters[i] is Pet item && item.petId?.Value == id)
                        {
                            return item;
                        }
                    }

                }
            }
            return null;
        }
        public static Vector2 GetPositionDirectlyBehind(Vector2 position, int facingDirection)
        {
            var adjacentPositions = Utility.getAdjacentTileLocationsArray(position);
            switch (facingDirection)
            {
                case 0:
                    return adjacentPositions[2];
                case 1:
                    return adjacentPositions[0];
                case 2:
                    return adjacentPositions[3];
                case 3:
                    return adjacentPositions[1];
                default:
                    return adjacentPositions[0];
            }
        }
        public static bool cFIs(Pet pet, List<int> frameList)
        {
            if (!Context.IsWorldReady) { return false; }
            var i = 0;
            while (i < frameList.Count)
            {
                if (!frameList[i].IsNull(out var frame) && pet.Sprite.currentFrame == frame)
                {
                    return true;
                }
                i++;
            }
            return false;
        }
        public static bool IsThereAPetInFarm(PetInfo.Pet_Types petType)
        {
            if (!Context.IsWorldReady) { return false; }
            for (int i = 0;i < ModEntry.allPets.Count; i++)
            {
                if (PetHelper.GetPetInfo(ModEntry.allPets[i], out _)?.PetType == petType)
                {
                    return true;
                }
            }

            return false;
        }
        public static bool anyPetWithAllSkillsLearned()
        {
            for (int i = 0; i < ModEntry.allPets.Count; i++)
            {
                if (CachePetData.PetCache.ContainsKey(ModEntry.allPets[i]) && CachePetData.PetCache[ModEntry.allPets[i]].Pet.AllSkillsLearned())
                {
                    return true;
                }
            }
            return false;
        }
        public static bool anyPetWithAllSkillsLearnedAndNoLeader()
        {
            for (int i = 0; i < ModEntry.allPets.Count; i++)
            {
                if (CachePetData.PetCache.ContainsKey(ModEntry.allPets[i]) && CachePetData.PetCache[ModEntry.allPets[i]].Pet.AllSkillsLearned() && CachePetData.PetCache[ModEntry.allPets[i]].Pet.GroupLeader is null)
                {
                    return true;
                }
            }
            return false;
        }

        public static void TurnOffPetLearnedANewSkillNotification(Guid _pet)
        {
            if (!Game1.player.IsMainPlayer)
            {
                ModEntry.AHelper.Multiplayer.SendMessage(_pet, "_TurnOffPetLearnedANewSkillNotification", ModEntry.ModIDAsArray, new[] { ModEntry.MainPlayerID });
                return;
            }
            if (CachePetData.PetCache.TryGetValue(_pet, out var petIK))
            {
                petIK?.Pet?.SetPetLearnedANewSkill(false);
            }
        }
        public static void TurnOffNewItemHasBeenObtained(Guid _pet)
        {
            if (!Game1.player.IsMainPlayer)
            {
                ModEntry.AHelper.Multiplayer.SendMessage(_pet, "_TurnOffNewItemHasBeenObtained", ModEntry.ModIDAsArray, new[] { ModEntry.MainPlayerID });
                return;
            }
            if (CachePetData.PetCache.TryGetValue(_pet, out var petIK))
            {
                petIK?.Pet?.SetNewItemHasBeenObtained(false);
            }
        }
        public static void ChangeBackgroundIndex(Guid _pet,int index)
        {
            if (!Game1.player.IsMainPlayer)
            {
                ModEntry.AHelper.Multiplayer.SendMessage((_pet,index), "_ChangeBackgroundIndex", ModEntry.ModIDAsArray, new[] { ModEntry.MainPlayerID });
                return;
            }
            if (CachePetData.PetCache.TryGetValue(_pet, out var petIK))
            {
                if (petIK?.Info?.BackgroundStyle_Index is not null)
                {
                    petIK.Info.BackgroundStyle_Index = index;
                }
            }
        }
        public static void MarkPetSkillPerkChecklist(Guid _pet, int skill,int perk)
        {
            if (!Game1.player.IsMainPlayer)
            {
                ModEntry.AHelper.Multiplayer.SendMessage((_pet, skill,perk), "_MarkPetSkillPerkChecklist", ModEntry.ModIDAsArray, new[] { ModEntry.MainPlayerID });
                return;
            }
            if (CachePetData.PetCache.TryGetValue(_pet, out var petIK))
            {
                if (petIK?.Info?.SkillPerkChecklist is not null)
                {
                    petIK.Info.SkillPerkChecklist[skill][perk] = true;
                }
            }
        }
    }
}
