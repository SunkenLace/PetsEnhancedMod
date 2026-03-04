using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Characters;
using Pets_Enhanced_Mod.Utilities.Custom_Classes;
using Pets_Enhanced_Mod.Utilities;
using System.Threading;
using static Pets_Enhanced_Mod.Utilities.SunkenLaceUtilities;
using System.Linq;
using Pets_Enhanced_Mod.Data;
using ContentPatcher;
using System.Diagnostics;
using StardewValley.Objects;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using static Pets_Enhanced_Mod.Utilities.CachePetData;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Emit;
using static System.Net.WebRequestMethods;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework.Media;
using System.Text.RegularExpressions;
using Pets_Enhanced_Mod.Multiplayer;
using static Pets_Enhanced_Mod.Utilities.Custom_Classes.SmartPet;
using StardewValley.GameData.Pets;
using Microsoft.VisualBasic;

namespace Pets_Enhanced_Mod
{
    public class ModEntry : Mod
    {
        public static IMonitor AMonitor;
        public static IModHelper AHelper;
        public ModEntry AContext;
        public static ModConfig TempModConfig;

        public static ModConfig CurrentModConfig;

        public static List<Guid> allPets = new();

        public static bool ShouldTimePass = false;
        public static string ModID;
        public static string[] ModIDAsArray;
        public static long MainPlayerID;
        public static bool CanUpdatePets = false;
        private static IContentPatcherAPI _contentPatcherApi;
        public override void Entry(IModHelper helper)
        {
            I18n.Init(helper.Translation);
            ModEntry.AMonitor = Monitor;
            AHelper = helper;
            this.AContext = this;
            CacheReciclerHelper.InitializePool();
            ModConfig_Helper.CheckforModConfigFile(helper);
            PetContent.WriteConfigFile();
            ModEntry.AMonitor = Monitor;
            AHelper = helper;
            this.AContext = this;
            TempModConfig = ModConfig_Helper.ModConfigFileToClass(helper);
            CurrentModConfig = ModConfig_Helper.ModConfigFileToClass(helper);
            CacheReciclerHelper.InitializePool();
            ModID = this.ModManifest.UniqueID;
            ModIDAsArray = new string[1]{ModID};
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.GameLoop.DayEnding += this.BeforeEndingDay;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            helper.Events.Multiplayer.PeerDisconnected += this.OnPlayerDisconected;
            helper.Events.Input.ButtonPressed += this.Input_ButtonPressed;
            helper.Events.Player.Warped += this.OnPlayerWarped;
            helper.Events.Multiplayer.ModMessageReceived += this.OnMessageReceived;
            helper.Events.Display.RenderedStep += this.OnRenderedStep;

        }
        public static void WriteMonitor(string text, LogLevel level)
        {
            ModEntry.AMonitor.Log(text, level);
        }
        public static IModHelper GetModHelper()
        {
            return AHelper;
        }
        public void OnPlayerDisconected(object sender, PeerDisconnectedEventArgs e)
        {
            if (isHostScreenId())
            {
                CachePetData.CachePetTeams.returnTeamWhenLeaderDisconected(e.Peer.PlayerID);
                SynchronizationManager.ExtrapolatePetPositions.Remove(e.Peer.PlayerID);
                CachePetData.SendPetToPlayerCache.Remove(e.Peer.PlayerID);
                SynchronizationManager.FarmhandsReady.Remove(e.Peer.PlayerID);
            }
        }
        public void OnPlayerWarped(object sender, WarpedEventArgs e)
        {
            if (isHostScreenId())
            {
                foreach (var pData in CachePetData.PetCache)
                {
                    pData.Value.Pet.OnWarp(e.Player);
                }
            }
            else
            {
                (int _facingDirection, Vector2 _position, string _locationNameOrUniqueName) _data = (Game1.player.FacingDirection, Game1.player.getStandingPosition(), Game1.player.currentLocation.NameOrUniqueName);
                ModEntry.AHelper.Multiplayer.SendMessage(_data, "PEM_FarmhandHasWarped", ModEntry.ModIDAsArray, new[] { MainPlayerID });
            }
        }
        public void OnRenderedStep(object sender, RenderedStepEventArgs e)
        {
            if (e.Step == StardewValley.Mods.RenderSteps.World_Sorted)
            {
                string playerLocation = Game1.player?.currentLocation?.NameOrUniqueName;
                Vector2 viewport = new(Game1.viewport.X, Game1.viewport.Y);
                if (isHostScreenId())
                {
                    if (Game1.CurrentEvent is null)
                    {
                        foreach (PetInfoKit s in CachePetData.PetCache.Values)
                        {
                            string locationName = s?.Pet?.CurrentLocation?.NameOrUniqueName;
                            if (locationName is not null && locationName.EqualsIgnoreCase(playerLocation))
                            {
                                s.Pet.Draw(e.SpriteBatch, viewport);
                            }
                        }
                    }
                }
                else if (Game1.CurrentEvent is null)
                {
                    var petList = CacheReciclerHelper.RentReciclablePetList();
                    PetHelper.GetAllPets(petList);

                    foreach (var pet in petList)
                    {

                        if (pet?.petId?.Value is null) { continue; }

                        if (SynchronizationManager.CurrentClientInformation.TryGetValue(pet.petId.Value, out var information))
                        {
                            var spriteInfo = new SynchronizationManager.PetSpriteInformation(pet, information);

                            spriteInfo.DrawSprite(e.SpriteBatch, viewport);

                        }

                    }

                    CacheReciclerHelper.Return(petList);
                }
            }
        }
        public void OnMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == ModID && !string.IsNullOrEmpty(e.Type))
            {
                if (isHostScreenId())
                {
                    if (HostDoPerMessageReceived.TryGetValue(e.Type, out var action))
                    {
                        action(e);
                    }
                }
                else
                {
                    if (ClientDoPerMessageReceived.TryGetValue(e.Type, out var action)) //prev 500.000 bytes |
                    {
                        action(e);
                    }
                }
            }
        }
        public void CallPetDialogueBoxKeyPress(ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu is not null && Game1.activeClickableMenu is PetDialogueBox)
            {
                var dBox = Game1.activeClickableMenu as PetDialogueBox;
                dBox.buttonPressedEventArgs = e;
            }
        }
        public static bool CheckClickStatic(Pet _pet, SynchronizationManager.PetInformation information,Vector2 mousePos)
        {
            Rectangle playerBBox = Game1.player.GetBoundingBox();
            Point bboxPos = playerBBox.Location;
            switch (Game1.player.FacingDirection)
            {
                case 0:
                    bboxPos.Y -= playerBBox.Height;
                    break;
                case 1:
                    bboxPos.X += playerBBox.Width;
                    break;
                case 2:
                    bboxPos.Y += playerBBox.Height;
                    break;
                case 3:
                    bboxPos.X -= playerBBox.Width;
                    break;
                default:
                    break;
            }
            Rectangle farmerNonMouseInteractionFrontBBox = new Rectangle(bboxPos.X, bboxPos.Y, playerBBox.Width, playerBBox.Height);

            Rectangle petFixedBBox = SmartPet.GetBoundingBoxFixed(information.PetPositionX, information.PetPositionY);
            Rectangle petInteractiveFixedBBox = SmartPet.GetInteractiveBoundingBoxFixed(information.PetPositionX, information.PetPositionY);
            bool isPetInTileAtFront = petFixedBBox.Intersects(farmerNonMouseInteractionFrontBBox);

            float distanceFromPlayer = MathF.Max(0, Utility.distance(petInteractiveFixedBBox.Center.X, Game1.player.StandingPixel.X, petInteractiveFixedBBox.Center.Y, Game1.player.StandingPixel.Y));

            if ((isPetInTileAtFront || (petInteractiveFixedBBox.Contains(mousePos) && distanceFromPlayer <= 128)))
            {
                SmartPet.CustomCheckActionStatic(Game1.player, Game1.currentLocation, _pet, information);
                return true;
            }
            return false;
        }
        public void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            bool CanInteract = Game1.activeClickableMenu is null && !Game1.dialogueUp && ShouldTimePass && Game1.player.CanMove && Game1.CurrentEvent is null;
            CallPetDialogueBoxKeyPress(e);

            if (!Context.IsWorldReady || !CanInteract || CurrentModConfig is null) { return; }
            bool playerHasItemInHands = Game1.player.CurrentItem is not null;
            bool butterFlyPowderOrHatInHand = playerHasItemInHands && (Game1.player.CurrentItem.QualifiedItemId.Equals("(O)ButterflyPowder") || Game1.player.CurrentItem is Hat);

            if (playerHasItemInHands && PetHelper.ItemQUIDisAlternateTexturesItem(Game1.player.CurrentItem.QualifiedItemId)) { return; }

            if (PetHelper.IsActionButton(e.Button) && Game1.player.CurrentItem is not null && Game1.player.CurrentItem.QualifiedItemId.Equals(SmartPet.PetFluteQUID))
            {
                ModEntry.AHelper.Input.Suppress(e.Button);
                PetFluteClicked(Game1.player);

                return;
            }
            else if ((CurrentModConfig.KeyboardInteractionKey == SButton.None && PetHelper.IsActionButton(e.Button)) || e.Button == CurrentModConfig.KeyboardInteractionKey || (PetHelper.IsActionButton(e.Button) && butterFlyPowderOrHatInHand))
            {
                if (isHostScreenId())
                {
                    var mousePos = ModEntry.AHelper.Input.GetCursorPosition().AbsolutePixels;
                    foreach (var petKit in CachePetData.PetCache.Values)
                    {
                        if (petKit.Info is not null && petKit.Pet is not null && petKit.Pet.CurrentLocation.NameOrUniqueName.EqualsIgnoreCase(Game1.player.currentLocation.NameOrUniqueName))
                        {
                            if (CheckClickStatic(petKit.Pet.OriginalPetInstance, new(petKit.Pet), mousePos))
                            {
                                ModEntry.AHelper.Input.Suppress(e.Button);
                                break;
                            }

                        }
                    }
                }
                else
                {
                    var petList = CacheReciclerHelper.RentReciclablePetList();
                    try
                    {
                        var mousePos = ModEntry.AHelper.Input.GetCursorPosition().AbsolutePixels;
                        PetHelper.GetAllPets(petList);
                        foreach (var pet in petList)
                        {
                            if (pet?.petId?.Value is null) { continue; }

                            if (SynchronizationManager.CurrentClientInformation.TryGetValue(pet.petId.Value, out var information))
                            {
                                if (CheckClickStatic(pet, information,mousePos))
                                {
                                    ModEntry.AHelper.Input.Suppress(e.Button);
                                    break;
                                }

                            }
                        }
                    }
                    finally { CacheReciclerHelper.Return(petList); }
                }
            }
        }
        public static void PetFluteClicked(Farmer who)
        {
            if (!who.currentLocation.IsOutdoors)
            {
                if (Game1.hudMessages.Count == 0)
                {
                    Game1.showRedMessage(I18n.CannotUseItemHere());
                    Game1.playSound("cancel");
                }
                return;
            }

            var petList = CacheReciclerHelper.RentReciclablePetList();
            var _petCandidates = CacheReciclerHelper.RentReciclablePetList();

            try
            {
                PetHelper.GetAllPets(petList);

                bool isThereAnyPetWithAllSkillsLearned = false;
                int currentPetInTeamCount = 0;
                foreach (var pet in petList)
                {
                    if (pet?.modData is null) { continue; }

                    var petData = CachePetData.GetPetDataForPet(pet);
                    var skillMasteryStruct = new SynchronizationManager.SkillMasteryLevelStruct(pet.modData, -1);

                    bool allSkillsLearned = (petData.HasWaitSkill && skillMasteryStruct.WaitingSkillMastery < 1) || ((petData.HasFollowSkill && skillMasteryStruct.FollowingSkillMastery < 1) || !petData.HasFollowSkill) || (petData.HasForageSkill && skillMasteryStruct.ForagingSkillMastery < 1) || (petData.HasFishingSkill && skillMasteryStruct.FishingSkillMastery < 1) || (petData.HasHuntSkill && skillMasteryStruct.HuntingSkillMastery < 1) ? false : true;
                    if (allSkillsLearned)
                    {
                        isThereAnyPetWithAllSkillsLearned = true;
                    }
                    if (SynchronizationManager.TryParseModData(pet.modData, SynchronizationManager.PetModDataKey_LeaderUniqueMultiplayerID, out long? groupLeader))
                    {
                        if (groupLeader is null && allSkillsLearned) { _petCandidates.Add(pet); }
                        else if (groupLeader == who.UniqueMultiplayerID)
                        {
                            currentPetInTeamCount += 1;
                        }
                    }

                    if (currentPetInTeamCount > 2)
                    {
                        if (Game1.hudMessages.Count == 0)
                        {
                            Game1.showRedMessage(I18n.TooManyPetsOnYourTeamPetFluteWarning());
                            Game1.playSound("cancel");
                        }
                        return;
                    }
                }
                if (!isThereAnyPetWithAllSkillsLearned)
                {
                    if (Game1.hudMessages.Count == 0)
                    {
                        Game1.showRedMessage(I18n.PetFluteWarningNoTrainedPets());
                        Game1.playSound("cancel");
                    }
                    return;
                }
                if (_petCandidates.Count == 0)
                {
                    if (Game1.hudMessages.Count == 0)
                    {
                        Game1.showRedMessage(I18n.NoPetsAvailable());
                        Game1.playSound("cancel");
                    }
                    return;
                }

                Game1.player.currentLocation.createQuestionDialogue(I18n.CallPetQuestion(), PetResponses.CallPetOptions(who, _petCandidates), PetHelper.SendSPetToPlayer, null);
            }
            finally { CacheReciclerHelper.Return(petList); CacheReciclerHelper.Return(_petCandidates); }

        }
        private void DoForTick()
        {
            UpdatePets();
            CachePetData.CachePetTeams.UpdateTeams();
        }
        private static void UpdatePets()
        {
            Span<Guid> _allPets = CollectionsMarshal.AsSpan(allPets).Slice(0, allPets.Count);
            if (CanUpdatePets)
            {
                for (int i = 0; i < _allPets.Length; i++)
                {
                    try
                    {
                        if (CachePetData.PetCache.TryGetValue(_allPets[i], out var petInfoKit))
                        {
                            if (ShouldTimePass)
                            {
                                petInfoKit.Pet.Update(Game1.currentGameTime);
                            }
                            SynchronizationManager.UpdateModDataForPet(petInfoKit.Pet);
                        }
                        else if (!PetHelper.GetPetInfo(_allPets[i], out Pet _pet).IsNull(out var petInfo))
                        {
                            PetHelper.SmartPetSwap(_pet, petInfo);
                        }
                    }
                    catch (Exception uEx) { ModEntry.WriteMonitor($"Error at registering pet. Details:{uEx}", LogLevel.Error); }
                }
            }
        }
        public void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            try
            {
                if (isHostScreenId())
                {

                    if (!Context.IsWorldReady)
                    {
                        CachePetData.Clear();
                        allPets.Clear();
                        SynchronizationManager.Clear();
                        CacheReciclerHelper.ClearPool();
                        CacheReciclerHelper.ClearConcatenatedStringRegister();
                    }
                    else
                    {
                        if (e.IsOneSecond)
                        {
                            allPets = PetHelper.GetAllPetIds(allPets);
                        }
                        ShouldTimePass = Game1.shouldTimePass();
                        DoForTick();

                        if (Game1.IsMultiplayer) { SynchronizationManager.SendInformationToClients(); }

                        if (e.IsOneSecond) { CheckForPetsWithBuffsAndApplyThem(); }

                    }
                }
                else if (Context.IsWorldReady)
                {
                    MainPlayerID = Game1.MasterPlayer.UniqueMultiplayerID;
                    if (!SynchronizationManager.FarmhandsReady.Contains(Game1.player.UniqueMultiplayerID))
                    {
                        ModEntry.AHelper.Multiplayer.SendMessage<int>(1, "PEM_FarmhandIsReady", ModIDAsArray, new[] { ModEntry.MainPlayerID });
                        SynchronizationManager.FarmhandsReady.Add(Game1.player.UniqueMultiplayerID);
                    }
                    ShouldTimePass = Game1.shouldTimePass();
                    UpdateMouseClient();

                    if (e.IsOneSecond) { CheckForPetsWithBuffsAndApplyThem(); }
                }
                else { ShouldTimePass = false; }
            }
            catch (Exception ex)
            {
                ModEntry.WriteMonitor($"Error at updating {ex}", LogLevel.Error);
            }
        }
        /// <summary>Checks if player is host in singleplayer, multiplayer and splitscreen</summary>
        public static bool isHostScreenId()
        {
            return ((Context.IsSplitScreen && Context.ScreenId == 0) || !Context.IsSplitScreen) && Game1.IsMasterGame;
        }
        public static void applySpeedBuff(int _speedBuff)
        {
            if (_speedBuff > 0)
            {
                float buffCalculated = ((5f * (((float)_speedBuff * 0.01f) + 1f)) - 5f);

                if (Game1.player.buffs.AppliedBuffs.TryGetValue("PetsEnhancedMod.Buff.Speed", out Buff _speedBuffPresent))
                {
                    _speedBuffPresent.effects.Speed.Set(buffCalculated);
                    _speedBuffPresent.visible = false;
                    _speedBuffPresent.millisecondsDuration = 1000;
                }
                else
                {
                    StardewValley.GameData.Buffs.BuffAttributesData bData = new();
                    bData.Speed = buffCalculated;
                    var sBuff = new Buff("PetsEnhancedMod.Buff.Speed", duration: 1000, effects: new StardewValley.Buffs.BuffEffects(bData));
                    sBuff.visible = false;
                    Game1.player.buffs.Apply(sBuff);
                }
            }
        }
        public void CheckForPetsWithBuffsAndApplyThem()
        {
            int speedMultiplier = 0;
            var petList = CacheReciclerHelper.RentReciclablePetList();
            try
            {
                PetHelper.GetAllPets(petList);

                foreach (var pet in petList)
                {
                    if (pet?.modData is null) { continue; }

                    if (SynchronizationManager.TryParseModData(pet.modData, SynchronizationManager.PetModDataKey_LeaderUniqueMultiplayerID, out long? leaderID) && leaderID == Game1.player.UniqueMultiplayerID)
                    {
                        if (!SynchronizationManager.TryGetPetAccessory(pet).IsNull(out var accItem) && accItem.QualifiedItemId == LightweightCollarQUID)
                        {
                            speedMultiplier += 4;
                        }
                        SynchronizationManager.TryParseModData(pet.modData, SynchronizationManager.PetModDataKey_CurrentPetObjective, out int currPetObjective);
                        SynchronizationManager.TryParseModData(pet.modData, SynchronizationManager.PetModDataKey_Following_SkillMasteryLevel, out double followingSkillMastery);
                        if (currPetObjective == 1 && followingSkillMastery >= 4)
                        {
                            speedMultiplier += 4;
                        }
                    }
                }
            }
            finally { CacheReciclerHelper.Return(petList); }

            applySpeedBuff(speedMultiplier);
        }
        public void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ModConfig_Helper.RegisterModOnGMCM(this.AContext, AHelper, TempModConfig);
            if (AHelper.ModRegistry.IsLoaded("Pathoschild.ContentPatcher") && ModConfig_Helper.HookIntoContentPatcherAPI(this.AContext))
            {
                _contentPatcherApi = ModConfig_Helper.GetContentPatcherInterface();
                _contentPatcherApi.RegisterToken(this.ModManifest, "DogTreatsDisplayName", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.DogTreatsDisplayName() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.DogTreatsDisplayName() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "DogTreatsDescription", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.DogTreatsDescription() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.DogTreatsDescription() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "CatTreatsDisplayName", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.CatTreatsDisplayName() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.CatTreatsDisplayName() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "CatTreatsDescription", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.CatTreatsDescription() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.CatTreatsDescription() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "CrunchyTreatsDisplayName", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.CrunchyTreatsDisplayName() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.CrunchyTreatsDisplayName() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "CrunchyTreatsDescription", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.CrunchyTreatsDescription() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.CrunchyTreatsDescription() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "PetFluteDisplayName", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.PetFluteDisplayName() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.PetFluteDisplayName() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "PetFluteDescription", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.PetFluteDescription() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.PetFluteDescription() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "PetManualDisplayName", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.PetManualDisplayName() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.PetManualDisplayName() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "PetManualDescription", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.PetManualDescription() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.PetManualDescription() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "PetBackpackDisplayName", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.PetBackpackDisplayName() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.PetBackpackDisplayName() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "PetBackpackDescription", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.PetBackpackDescription() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.PetBackpackDescription() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "MarnieGiftsPetFluteMail", () =>
                    Context.IsWorldReady ? new[] { I18n.MarnieGiftsPetFluteMailContent(I18n.PetFluteDisplayName()) + $"%item id {SmartPet.PetFluteQUID} 1 %%[#]" + I18n.MarnieGiftsPetFluteMailTitle() } :
                    SaveGame.loaded?.player is not null ? new[] { I18n.MarnieGiftsPetFluteMailContent(I18n.PetFluteDisplayName()) + $"%item id {SmartPet.PetFluteQUID} 1 %%[#]" + I18n.MarnieGiftsPetFluteMailTitle() } :
                    null
                );
                _contentPatcherApi.RegisterToken(this.ModManifest, "MarnieGiftsLightweightCollarMail", () =>
                    Context.IsWorldReady ? new[] { I18n.MarnieGiftsLightweightCollarMailContent(I18n.LightweightCollarDisplayName()) + "%item craftingRecipe SunkenLace.PetsEnhancedMod.LightweightCollar_recipe%%[#]" + I18n.MarnieGiftsLightweightCollarMailTitle() } :
                    SaveGame.loaded?.player is not null ? new[] { I18n.MarnieGiftsLightweightCollarMailContent(I18n.LightweightCollarDisplayName()) + "%item craftingRecipe SunkenLace.PetsEnhancedMod.LightweightCollar_recipe%%[#]" + I18n.MarnieGiftsLightweightCollarMailTitle() } :
                    null
                );
                _contentPatcherApi.RegisterToken(this.ModManifest, "MarnieGiftsRoughCollarMail", () =>
                    Context.IsWorldReady ? new[] { I18n.MarnieGiftsRoughCollarMailContent(I18n.RoughCollarDisplayName()) + "%item craftingRecipe SunkenLace.PetsEnhancedMod.RoughCollar_recipe%%[#]" + I18n.MarnieGiftsRoughCollarMailTitle() } :
                    SaveGame.loaded?.player is not null ? new[] { I18n.MarnieGiftsRoughCollarMailContent(I18n.RoughCollarDisplayName()) + "%item craftingRecipe SunkenLace.PetsEnhancedMod.RoughCollar_recipe%%[#]" + I18n.MarnieGiftsRoughCollarMailTitle() } :
                    null
                );
                _contentPatcherApi.RegisterToken(this.ModManifest, "MarnieGiftsFloweryCollarMail", () =>
                    Context.IsWorldReady ? new[] { I18n.MarnieGiftsFloweryCollarMailContent(I18n.FloweryCollarDisplayName()) + "%item craftingRecipe SunkenLace.PetsEnhancedMod.FloweryCollar_recipe%%[#]" + I18n.MarnieGiftsFloweryCollarMailTitle() } :
                    SaveGame.loaded?.player is not null ? new[] { I18n.MarnieGiftsFloweryCollarMailContent(I18n.FloweryCollarDisplayName()) + "%item craftingRecipe SunkenLace.PetsEnhancedMod.FloweryCollar_recipe%%[#]" + I18n.MarnieGiftsFloweryCollarMailTitle() } :
                    null
                );
                _contentPatcherApi.RegisterToken(this.ModManifest, "MarnieGiftsSeagoingCollarMail", () =>
                    Context.IsWorldReady ? new[] { I18n.MarnieGiftsSeagoingCollarMailContent(I18n.SeagoingCollarDisplayName()) + $"%item craftingRecipe SunkenLace.PetsEnhancedMod.SeagoingCollar_recipe%%[#]" + I18n.MarnieGiftsSeagoingCollarMailTitle() } :
                    SaveGame.loaded?.player is not null ? new[] { I18n.MarnieGiftsSeagoingCollarMailContent(I18n.SeagoingCollarDisplayName()) + $"%item craftingRecipe SunkenLace.PetsEnhancedMod.SeagoingCollar_recipe%%[#]" + I18n.MarnieGiftsSeagoingCollarMailTitle() } :
                    null
                );
                _contentPatcherApi.RegisterToken(this.ModManifest, "MarnieGiftsLuxuryCollarMail", () =>
                    Context.IsWorldReady ? new[] { I18n.MarnieGiftsLuxuryCollarMailContent(I18n.LuxuryCollarDisplayName()) + $"%item id {SmartPet.LuxuryCollarQUID} 1 %%[#]" + I18n.MarnieGiftsLuxuryCollarMailTitle() } :
                    SaveGame.loaded?.player is not null ? new[] { I18n.MarnieGiftsLuxuryCollarMailContent(I18n.LuxuryCollarDisplayName()) + $"%item id {SmartPet.LuxuryCollarQUID} 1 %%[#]" + I18n.MarnieGiftsLuxuryCollarMailTitle() } :
                    null
                );
                _contentPatcherApi.RegisterToken(this.ModManifest, "LuxuryCollarDisplayName", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.LuxuryCollarDisplayName() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.LuxuryCollarDisplayName() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "LuxuryCollarDescription", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.LuxuryCollarDescription() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.LuxuryCollarDescription() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "LightweightCollarDisplayName", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.LightweightCollarDisplayName() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.LightweightCollarDisplayName() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "LightweightCollarDescription", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.LightweightCollarDescription() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.LightweightCollarDescription() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "RoughCollarDisplayName", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.RoughCollarDisplayName() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.RoughCollarDisplayName() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "RoughCollarDescription", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.RoughCollarDescription() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.RoughCollarDescription() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "FloweryCollarDisplayName", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.FloweryCollarDisplayName() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.FloweryCollarDisplayName() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "FloweryCollarDescription", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.FloweryCollarDescription() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.FloweryCollarDescription() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "SeagoingCollarDisplayName", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.SeagoingCollarDisplayName() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.SeagoingCollarDisplayName() }; }
                    return null;
                });
                _contentPatcherApi.RegisterToken(this.ModManifest, "SeagoingCollarDescription", () =>
                {
                    if (Context.IsWorldReady) { return new[] { I18n.SeagoingCollarDescription() }; }
                    if (SaveGame.loaded?.player is not null) { return new[] { I18n.SeagoingCollarDescription() }; }
                    return null;
                });
            }
            ModConfig_Helper.RegisterModOnQuickSaveMod(AHelper);
        }
        public void UpdateMouseClient()
        {
            bool CanInteract = Game1.activeClickableMenu is null && !Game1.dialogueUp && ShouldTimePass && Game1.player.CanMove && Game1.CurrentEvent is null;
            if (!CanInteract) { return; }

            var mousePos = ModEntry.AHelper.Input.GetCursorPosition().AbsolutePixels;
            if (CanInteract)
            {
                var petList = CacheReciclerHelper.RentReciclablePetList();
                try
                {
                    PetHelper.GetAllPets(petList);
                    foreach (var pet in petList)
                    {
                        if (pet?.petId?.Value is null) { continue; }

                        if (SynchronizationManager.CurrentClientInformation.TryGetValue(pet.petId.Value, out var information))
                        {
                            var interactiveBBox = SmartPet.GetInteractiveBoundingBoxFixed(information.PetPositionX, information.PetPositionY);
                            if (interactiveBBox.Contains(mousePos))
                            {
                                var petData = CachePetData.GetPetDataForPet(pet);
                                float distanceFromPlayer = MathF.Max(0, Utility.distance(interactiveBBox.Center.X, Game1.player.StandingPixel.X, interactiveBBox.Center.Y, Game1.player.StandingPixel.Y));
                                if (((Game1.player.Items.Count > Game1.player.CurrentToolIndex && Game1.player.Items[Game1.player.CurrentToolIndex] is not null && Game1.player.Items[Game1.player.CurrentToolIndex] is Hat)))
                                {
                                    Game1.mouseCursor = 2;
                                    Game1.mouseCursorTransparency = distanceFromPlayer <= 128 ? 1f : 0.5f;
                                }
                                else if (Game1.player.CurrentItem is not null && CachePetData.GetDietListFromID(petData.DietListID).ContainsKey(Game1.player.CurrentItem.QualifiedItemId))
                                {
                                    Game1.mouseCursor = Game1.cursor_gift;
                                    Game1.mouseCursorTransparency = distanceFromPlayer <= 128 ? 1f : 0.5f;
                                }
                                else if (Game1.player.CurrentItem is not null && Game1.player.CurrentItem.QualifiedItemId.Equals("(O)ButterflyPowder"))
                                {
                                    Game1.mouseCursor = Game1.cursor_harvest;
                                    Game1.mouseCursorTransparency = distanceFromPlayer <= 128 ? 1f : 0.5f;
                                }
                                else
                                {
                                    Game1.mouseCursor = Game1.cursor_talk;
                                    Game1.mouseCursorTransparency = distanceFromPlayer <= 128 ? 1f : 0.5f;
                                }
                                break;
                            }
                        }
                    }
                }
                finally { CacheReciclerHelper.Return(petList); }
            }
        }
        public void BeforeEndingDay(object sender, DayEndingEventArgs e)
        {
            if (!Context.IsWorldReady) { return; }
            try
            {
                SendMail();
                if (isHostScreenId())
                {
                    for (int i = 0; i < allPets.Count; i++)
                    {
                        if (CachePetData.PetCache.TryGetValue(allPets[i], out CachePetData.PetInfoKit data) && data is not null)
                        {
                            data.Info.PetInventorySerialized = null;
                            if (!Game1.isRaining)
                            {
                                data.Pet.OriginalPetInstance.WarpToPetBowl();
                            }
                            else
                            {
                                data.Pet.OriginalPetInstance.warpToFarmHouse(Game1.MasterPlayer);
                            }
                            data.Pet.OriginalPetInstance.Sprite.CurrentAnimation = null;
                            data.Pet.OriginalPetInstance.Sprite.CurrentFrame = 0;
                            data.Pet.OriginalPetInstance.CurrentBehavior = "Walk";
                        }
                    }
                }
                CanUpdatePets = false;
                CachePetData.Clear();
            }
            catch (Exception ex) { ModEntry.WriteMonitor($"Error before ending day, details: {ex}", LogLevel.Error); }

        }
        public void SendMail()
        {
            string _giftPetFluteMail = "SunkenLace.PetsEnhancedMod.GiftPetFluteMail";
            string _giftLightweightCollarRecipe = "SunkenLace.PetsEnhancedMod.GiftLightweightCollarRecipeMail";
            string _giftRoughCollarRecipe = "SunkenLace.PetsEnhancedMod.GiftRoughCollarCollarRecipeMail";
            string _giftFloweryCollarRecipe = "SunkenLace.PetsEnhancedMod.GiftFloweryCollarCollarRecipeMail";
            string _giftSeagoingCollarRecipe = "SunkenLace.PetsEnhancedMod.GiftSeagoingCollarRecipeMail";
            string _giftLuxuryCollar = "SunkenLace.PetsEnhancedMod.GiftLuxuryCollarRecipeMail";
            if (Game1.player.mailReceived.Contains("MarniePetAdoption") && !Game1.player.hasOrWillReceiveMail(_giftPetFluteMail))
            {
                Game1.addMailForTomorrow(_giftPetFluteMail);
            }
            if (Game1.player.MaxStamina >= 370 && !Game1.player.hasOrWillReceiveMail(_giftLightweightCollarRecipe))
            {
                Game1.addMailForTomorrow(_giftLightweightCollarRecipe);
            }
            if (Game1.player.combatLevel.Value >= 5 && !Game1.player.hasOrWillReceiveMail(_giftRoughCollarRecipe))
            {
                Game1.addMailForTomorrow(_giftRoughCollarRecipe);
            }
            if (Game1.player.foragingLevel.Value >= 5 && !Game1.player.hasOrWillReceiveMail(_giftFloweryCollarRecipe))
            {
                Game1.addMailForTomorrow(_giftFloweryCollarRecipe);
            }
            if (Game1.player.fishingLevel.Value >= 5 && !Game1.player.hasOrWillReceiveMail(_giftSeagoingCollarRecipe))
            {
                Game1.addMailForTomorrow(_giftSeagoingCollarRecipe);
            }
            if (Game1.player.totalMoneyEarned >= 150000 && !Game1.player.hasOrWillReceiveMail(_giftLuxuryCollar))
            {
                Game1.addMailForTomorrow(_giftLuxuryCollar);
            }
        }
        public void OnSaving(object sender, SavingEventArgs e)
        {
            if (!isHostScreenId() || !Context.IsWorldReady) { return; }
            try
            {
                ModConfig_Helper.SaveModDataValues(AHelper);
            }
            catch
            {
                ModEntry.WriteMonitor($"Error in OnSaving method", LogLevel.Error);
            }
        }
        public void OnDayStarted(object sender, EventArgs e)
        {
            if (!isHostScreenId()) { return; }
            try
            {
                ModConfig_Helper.LoadModDataValues(AHelper);
            }
            catch
            {
                ModEntry.WriteMonitor($"Error in OnLoading method", LogLevel.Error);
            }
            try
            {
                allPets = PetHelper.GetAllPetIds(allPets);
                for (int i = 0; i < allPets.Count; i++)
                {
                    if (PetHelper.GetPetInfo(allPets[i], out Pet _pet) is PetInfo petInfo)
                    {
                        if (_pet?.currentLocation?.characters is not null && _pet.currentLocation.characters.Count > 0)
                        {
                            _pet.currentLocation.characters.RemoveWhere(x => x is StardewValley.Characters.Pet _Doppelganger && _Doppelganger.petId?.Value == _pet.petId?.Value && _Doppelganger.Name == _pet.Name);
                            _pet.currentLocation.characters.Add(_pet);
                        }
                        petInfo.HasBeenGivenTreatToday = false;
                        petInfo.TrainedToday = false;
                        petInfo.Age++;
                        petInfo.Name = _pet.Name;
                        petInfo.Energy = petInfo.MaxBaseEnergy = (SmartPet.MaxBaseEnergyNoUpgrade + (petInfo.SkillMastery_level[1] >= 5 ? 80 : 0));
                        PetHelper.SmartPetSwap(_pet, petInfo);
                    }
                }
                CanUpdatePets = true;
                ModEntry.WriteMonitor($"New day, pet count is {allPets.Count}", LogLevel.Trace);
            }
            catch (Exception ex) { ModEntry.WriteMonitor($"Error when starting day, details: {ex}", LogLevel.Error); }
        }

        public struct HatOffset_Simple
        {
            public HatOffset_Simple(int _frame, float _offsetX, float _offsetY, int _direction,bool drawHat, float scale)
            {
                this.Frame = _frame;
                this.OffsetX = _offsetX;
                this.OffsetY = _offsetY;
                this.Direction = _direction;
                this.DrawHat = drawHat;
                this.Scale = scale;
            }
            public int Frame;
            public float OffsetX;
            public float OffsetY;
            public int Direction;
            public bool DrawHat;
            public float Scale;
        }
        public static void SelectPetforWarping(long _leaderNameOrUniqueName, (int _facingDirection, Vector2 _position, string _locationNameOrUniqueName) _petitionInfo)
        {
            foreach (var pData in CachePetData.PetCache)
            {
                pData.Value.Pet.OnWarpFarmhand(_leaderNameOrUniqueName, _petitionInfo._facingDirection, _petitionInfo._position, _petitionInfo._locationNameOrUniqueName);
            }
        }

        public Dictionary<string, Action<ModMessageReceivedEventArgs>> HostDoPerMessageReceived = new() {
            { "CommandPetCall_client", e => { var v = e.ReadAs<KeyValuePair<Guid, string>>(); if (CachePetData.PetCache.ContainsKey(v.Key) && !Game1.GetPlayer(e.FromPlayerID, true).IsNull(out var player)) { CachePetData.PetCache[v.Key].Pet.CommandPet(v.Value, player, v.Key);}} },
            { "_ChangeBackgroundIndex", e => { var (id, index) = e.ReadAs<(Guid,int)>(); PetHelper.ChangeBackgroundIndex(id,index); } },
            { "_MarkPetSkillPerkChecklist", e => { var (id, skill, perk) = e.ReadAs<(Guid,int,int)>(); PetHelper.MarkPetSkillPerkChecklist(id,skill,perk); } },
            { "AskHostToSendPetToPlayerForPetFluteCall", e =>  {var pet = e.ReadAs<string>(); if (!Game1.GetPlayer(e.FromPlayerID, true).IsNull(out var player)){PetHelper.sendPetToPlayerWhoCalled(player, pet); } }},
            { "_TurnOffNewItemHasBeenObtained",e => PetHelper.TurnOffNewItemHasBeenObtained(e.ReadAs<Guid>())},
            { "_TurnOffPetLearnedANewSkillNotification",e => PetHelper.TurnOffPetLearnedANewSkillNotification(e.ReadAs<Guid>()) },
            { "PEM_FarmhandHasWarped",e => SelectPetforWarping(e.FromPlayerID,e.ReadAs<(int _facingDirection,Vector2 _position,string _locationNameOrUniqueName)>()) },
            { "PEM_FarmhandIsReady",e => SynchronizationManager.FarmhandsReady.Add(e.FromPlayerID) },
            { "PEM_PetHappyReactionRequest",e => {if (CachePetData.PetCache.TryGetValue(e.ReadAs<Guid>(), out var petKit)){petKit?.Pet?.DoHappyReaction(); } } },
            { "PEM_PetAngryReactionRequest",e => {if (CachePetData.PetCache.TryGetValue(e.ReadAs<Guid>(), out var petKit)){petKit?.Pet?.DoAngryReaction(); } } },
            { "PEM_UnlockSkillsRequest",e => {if (CachePetData.PetCache.TryGetValue(e.ReadAs<Guid>(), out var petKit)){petKit?.Pet?.UnlockSkills(); } } },
        };

        public Dictionary<string, Action<ModMessageReceivedEventArgs>> ClientDoPerMessageReceived = new() {
            { "UPetI00", e => SynchronizationManager.PopulateCurrentClientInformation(e.ReadAs<string>()) },
            { "ClientHealPlayer20", e =>  Game1.player.health = Math.Min(Game1.player.health + 20, Game1.player.maxHealth) },
            { "ReduceItemCall", e => Game1.player.Items.ReduceId(e.ReadAs<KeyValuePair<string, int>>().Key, e.ReadAs<KeyValuePair<string, int>>().Value)},
            { "ReduceActiveItemIdByOne", e => { if (Game1.player.CurrentItem is not null && Game1.player.CurrentItem.QualifiedItemId == e.ReadAs<string>()) {Game1.player.reduceActiveItemByOne(); } else { Game1.player.Items.ReduceId(e.ReadAs<string>(), 1); } } },
            { "DOD_PetAlreadyFollowingFarmerAlert", e => Game1.drawObjectDialogue(Game1.parseText(I18n.PetAlreadyFollowingFarmerAlert(PetName: e.ReadAs<KeyValuePair<string, string>>().Key, FarmerName: e.ReadAs<KeyValuePair<string, string>>().Value))) },
            { "DOD_PetNeedsABreakAlert", e => Game1.drawObjectDialogue(Game1.parseText(I18n.PetNeedsABreakAlert(PetName: e.ReadAs<string>()))) },
            { "DOD_FeatureOnDevelopmentAlert", e => Game1.drawObjectDialogue(Game1.parseText(I18n.FeatureOnDevelopment())) },
            { "PEMPFarmerCaughtFish", e => Game1.player.caughtFish(e.ReadAs<KeyValuePair<string, KeyValuePair<int, int>>>().Key, e.ReadAs<KeyValuePair<string, KeyValuePair<int, int>>>().Value.Key,false,e.ReadAs<KeyValuePair<string, KeyValuePair<int, int>>>().Value.Value)},
            { "FCTextboxPetHasReturnedHome", e => Game1.addHUDMessage(HUDMessage.ForCornerTextbox(I18n.PetReturnedHomeMessage(e.ReadAs<string>()))) },
            { "DOD_CommandUnlockedAlert", e => {var data = e.ReadAs<KeyValuePair<string, string>>(); Game1.drawObjectDialogue(Game1.parseText(I18n.CommandUnlockedAlert(PetName: data.Key, CommandName: data.Value == "Fishing"?I18n.FishingLessonName(): data.Value == "Forage" ? I18n.ForageLessonName(): data.Value == "Follow" ? I18n.FollowMeLessonName(): data.Value == "Hunt" ? I18n.HuntLessonName() : I18n.WaitLessonName())));} },
            { "DOD_PetProgressedWithLessonRandomAns3", e => { var data = e.ReadAs<KeyValuePair<string, string>>(); Game1.drawObjectDialogue(Game1.parseText(I18n.PetProgressedWithLessonRandomAns3(PetName: data.Key, LessonName: data.Value == "Fishing" ? I18n.FishingLessonName() : data.Value == "Forage" ? I18n.ForageLessonName() : data.Value == "Follow" ? I18n.FollowMeLessonName() : data.Value == "Hunt" ? I18n.HuntLessonName() : I18n.WaitLessonName())));} },
            { "DOD_PetProgressedWithLessonRandomAns2", e => { var data = e.ReadAs<KeyValuePair<string, string>>(); Game1.drawObjectDialogue(Game1.parseText(I18n.PetProgressedWithLessonRandomAns2(PetName: data.Key, LessonName: data.Value == "Fishing" ? I18n.FishingLessonName() : data.Value == "Forage" ? I18n.ForageLessonName() : data.Value == "Follow" ? I18n.FollowMeLessonName() : data.Value == "Hunt" ? I18n.HuntLessonName() : I18n.WaitLessonName())));} },
            { "DOD_PetProgressedWithLessonRandomAns1", e => { var data = e.ReadAs<KeyValuePair<string, string>>(); Game1.drawObjectDialogue(Game1.parseText(I18n.PetProgressedWithLessonRandomAns1(PetName: data.Key, LessonName: data.Value == "Fishing" ? I18n.FishingLessonName() : data.Value == "Forage" ? I18n.ForageLessonName() : data.Value == "Follow" ? I18n.FollowMeLessonName() : data.Value == "Hunt" ? I18n.HuntLessonName() : I18n.WaitLessonName())));} },
            { "DOD_NotEnoughTreatsAlert", e => Game1.drawObjectDialogue(Game1.parseText(I18n.NotEnoughTreatsAlert()))},
            { "DOD_TooManyPetsOnYourTeamWarning", e => Game1.drawObjectDialogue(Game1.parseText(I18n.TooManyPetsOnYourTeamWarning()))},
            { "PEM_ClientPlayAllSound", e => {var u = e.ReadAs<KeyValuePair<string,KeyValuePair<Vector2,int?>>>(); Game1.sounds.PlayAll(u.Key, Game1.player.currentLocation, u.Value.Key, u.Value.Value, context: StardewValley.Audio.SoundContext.Default); } },
            { "AskClientToDeleteTempSpriteAtCurrentLocation", e => Game1.player.currentLocation.TemporarySprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.id == e.ReadAs<KeyValuePair<Vector2,int>>().Value && sprite.Position == e.ReadAs<KeyValuePair<Vector2,string>>().Key)},
            { "PerformUseActionOnTerrainFeature", e => {if (Game1.currentLocation.NameOrUniqueName.EqualsIgnoreCase(e.ReadAs<KeyValuePair<string,Vector2>>().Key))
                {
                    List<LargeTerrainFeature> feature = (Game1.currentLocation.largeTerrainFeatures).Where((StardewValley.TerrainFeatures.LargeTerrainFeature feature) => feature is not null && feature is Bush b && feature.isActionable() && feature.Tile == e.ReadAs<KeyValuePair<string,Vector2>>().Value).ToList();
                    if (feature.Count > 0 && feature.FirstOrDefault() is not null && feature.FirstOrDefault() is Bush b) {b.performUseAction(e.ReadAs<KeyValuePair<string,Vector2>>().Value); }
                }

        }} };
    }

}

