using System;
using System.Collections;
using System.Reflection;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Pets_Enhanced_Mod.Utilities.Custom_Classes;
using Pets_Enhanced_Mod.RefAPIs;
using Microsoft.Xna.Framework.Graphics;
using System.Timers;
using StardewValley.Monsters;
using ContentPatcher.Framework;
using ContentPatcher;
using static Pets_Enhanced_Mod.Utilities.SunkenLaceUtilities;
using xTile.Tiles;
using static StardewValley.Minigames.BoatJourney;
using StardewModdingAPI.Events;
using Microsoft.Xna.Framework;
using System.Linq;
using static Pets_Enhanced_Mod.Utilities.CachePetData;

namespace Pets_Enhanced_Mod.Utilities
{

    public class ModConfig_Helper
    {
        public static IContentPatcherAPI contentPatcherApi;
        public static Dictionary<Guid, PetInfo> ModDataLibrary { get; set; } = new();

        public static Dictionary<Guid, Outdated_PetInfo> oldModDataLibrary { get; set; } = null;
        public static void CheckforModConfigFile(IModHelper helper)
        {
            ModConfig modConfig = helper.Data.ReadJsonFile<ModConfig>("ModConfig.json") ?? new ModConfig(true);
            helper.Data.WriteJsonFile("ModConfig.json", modConfig);
        }
        public static ModConfig ModConfigFileToClass(IModHelper helper)
        {
            try
            {
                ModConfig modConfig = helper.Data.ReadJsonFile<ModConfig>("ModConfig.json");
                return modConfig;
            }
            catch (Exception ex)
            {
                ModEntry.WriteMonitor($"Error when loading ModConfig, details:{ex}", LogLevel.Error);
                return new(true);
            }
        }
        public static void ResetModConfig(IModHelper helper, ModConfig _save)
        {
            _save.Reset();
            ModEntry.CurrentModConfig = _save;
            helper.Data.WriteJsonFile("ModConfig.json", _save);
        }
        public static void SaveModConfig(IModHelper helper, ModConfig _save)
        {
            ModEntry.CurrentModConfig = _save;
            helper.Data.WriteJsonFile("ModConfig.json", _save);
        }
        private static Dictionary<Guid, PetInfo> FixOutdatedData()
        {
            return null;
        }
        public static void LoadModDataValues(IModHelper helper)
        {
            try
            {
                try
                {
                    if (!helper.Data.ReadSaveData<Dictionary<Guid, Outdated_PetInfo>>("ModDataLibrary_PetsEnhancedModTemporal").IsNull(out var oldData))
                    {
                        oldModDataLibrary = oldData;
                    }
                }
                catch (Exception ex) { ModEntry.WriteMonitor($"Attempt on recovering progress from outdated version 0.2.3 of Pets Enhanced Mod has resulted in failure. Details:{ex}", LogLevel.Error); }

                if (helper.Data.ReadSaveData<Dictionary<Guid, PetInfo>>("ModDataLibrary_PetsEnhancedMod").IsNull(out var pModData) || pModData == null) { return; }

                ModConfig_Helper.ModDataLibrary = pModData;
            }
            catch (Exception ex)
            {
                ModEntry.WriteMonitor($"Error when loading Pet Data, details:{ex}", LogLevel.Error);
            }
        }
        public static void SaveModDataValues(IModHelper helper)
        {
            try
            {
                helper.Data.WriteSaveData("ModDataLibrary_PetsEnhancedMod", ModConfig_Helper.ModDataLibrary);
            }
            catch
            {
                ModEntry.WriteMonitor($"Error when saving Pet Data", LogLevel.Error);
            }

        }
        public static bool HookIntoContentPatcherAPI(ModEntry entry)
        {
            contentPatcherApi = ModEntry.AHelper.ModRegistry.GetApi<ContentPatcher.IContentPatcherAPI>("Pathoschild.ContentPatcher");
            if (contentPatcherApi is null) 
            {
                ModEntry.WriteMonitor("Failed to hook into Pathoschild.ContentPatcher.", LogLevel.Error);
                return false; 
            }

            ModEntry.WriteMonitor("Successfully hooked into Pathoschild.ContentPatcher.", LogLevel.Debug);
            return true;

        }
        public static void RegisterModOnGMCM(ModEntry entry, IModHelper helper, ModConfig modConfig)
        {
            var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            var modManifest = entry.ModManifest;
            var modHelper = helper;
            if (configMenu is null) return;

            configMenu.Register(
                mod: modManifest,
                reset: () => ResetModConfig(helper, modConfig),
                save: () => SaveModConfig(helper, modConfig)
            );
            configMenu.AddSectionTitle(
                mod: modManifest,
                text: () => I18n.GMCMKeyboardSectionTitle(),
                tooltip: () => I18n.GMCMKeyboardSectionTitleDescription()
                );
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => I18n.GMCMKeyboardActionButtom(),
                getValue: () => modConfig.KeyboardInteractionKey,
                setValue: value => modConfig.KeyboardInteractionKey = value,
                tooltip: () => I18n.GMCMKeyboardActionButtomTooltip());
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => I18n.GMCMKeyboardStatusTabHotkey(),
                getValue: () => modConfig.KeyboardStatusTabHotkey,
                setValue: value => modConfig.KeyboardStatusTabHotkey = value,
                tooltip: () => I18n.GMCMKeyboardStatusTabHotkeyTooltip());
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => I18n.GMCMKeyboardInventoryTabHotkey(),
                getValue: () => modConfig.KeyboardInventoryTabHotkey,
                setValue: value => modConfig.KeyboardInventoryTabHotkey = value,
                tooltip: () => I18n.GMCMKeyboardInventoryTabHotkeyTooltip());
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => I18n.GMCMKeyboardHatTabHotkey(),
                getValue: () => modConfig.KeyboardHatTabHotkey,
                setValue: value => modConfig.KeyboardHatTabHotkey = value,
                tooltip: () => I18n.GMCMKeyboardHatTabHotkeyTooltip());
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => I18n.GMCMKeyboardAccessoryTabHotkey(),
                getValue: () => modConfig.KeyboardAccessoryTabHotkey,
                setValue: value => modConfig.KeyboardAccessoryTabHotkey = value,
                tooltip: () => I18n.GMCMKeyboardAccessoryTabHotkeyTooltip());
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.GMCMSubstituteDNumpadWithSymbols(),
                getValue: () => modConfig.SubstituteDNumpadWithSymbols,
                setValue: value => modConfig.SubstituteDNumpadWithSymbols = value,
                tooltip: () => I18n.GMCMSubstituteDNumpadWithSymbolsTooltip());
            configMenu.AddSectionTitle(
                mod:modManifest,
                text: () => I18n.GMCMGamepadSectionTitle(),
                tooltip: () => I18n.GMCMGamepadSectionTitleDescription()
                );
            configMenu.AddTextOption(
                mod: modManifest,
                name: () => I18n.GMCMGamepadLayoutOption(),
                getValue: () => modConfig.CurrentGamepadLayout.ToString(),
                setValue: value => modConfig.CurrentGamepadLayout = ModConfig.stringToControllerLayout(value),
                allowedValues: ModConfig.getAllowedControllerLayoutList(),
                tooltip: () => I18n.GMCMGamepadLayoutOptionDescription());
            configMenu.AddTextOption(
                mod: modManifest,
                name: () => I18n.GMCMGrabItemButtonOption(),
                getValue: () => modConfig.GrabItemButtonSubstituteForGamepad.ToString(),
                setValue: value => modConfig.GrabItemButtonSubstituteForGamepad = ModConfig.stringToControllerSButton(value),
                allowedValues: ModConfig.getAllowedControllerButtonList(),
                tooltip: () => I18n.GMCMGrabItemButtonOptionDescription());
            configMenu.AddTextOption(
                mod: modManifest,
                name: () => I18n.GMCMStackItemButtonOption(),
                getValue: () => modConfig.StackItemButtonSubstituteForGamepad.ToString(),
                setValue: value => modConfig.StackItemButtonSubstituteForGamepad = ModConfig.stringToControllerSButton(value),
                allowedValues: ModConfig.getAllowedControllerButtonList(),
                tooltip: () => I18n.GMCMStackItemButtonOptionDescription());
            configMenu.AddTextOption(
                mod: modManifest,
                name: () => I18n.GMCMCloseUIButtonOption(),
                getValue: () => modConfig.CloseUIButtonSubstituteForGamepad.ToString(),
                setValue: value => modConfig.CloseUIButtonSubstituteForGamepad = ModConfig.stringToControllerSButton(value),
                allowedValues: ModConfig.getAllowedControllerButtonList(),
                tooltip: () => I18n.GMCMCloseUIButtonOptionDescription());
            configMenu.AddTextOption(
                mod: modManifest,
                name: () => I18n.GMCMOpenPetInventoryButtonOption(),
                getValue: () => modConfig.OpenInventoryHotkeySubstituteForGamepad.ToString(),
                setValue: value => modConfig.OpenInventoryHotkeySubstituteForGamepad = ModConfig.stringToControllerSButton(value),
                allowedValues: ModConfig.getAllowedControllerButtonList(),
                tooltip: () => I18n.GMCMOpenPetInventoryButtonOptionDescription());
            configMenu.AddTextOption(
                mod: modManifest,
                name: () => I18n.GMCMOpenPetStatusWindowButtonOption(),
                getValue: () => modConfig.OpenStatusWindowHotkeySubstituteForGamepad.ToString(),
                setValue: value => modConfig.OpenStatusWindowHotkeySubstituteForGamepad = ModConfig.stringToControllerSButton(value),
                allowedValues: ModConfig.getAllowedControllerButtonList(),
                tooltip: () => I18n.GMCMOpenPetStatusWindowButtonOptionDescription());
            configMenu.AddTextOption(
                mod: modManifest,
                name: () => I18n.GMCMOpenPetHatSlotButtonOption(),
                getValue: () => modConfig.OpenHatSlotHotkeySubstituteForGamepad.ToString(),
                setValue: value => modConfig.OpenHatSlotHotkeySubstituteForGamepad = ModConfig.stringToControllerSButton(value),
                allowedValues: ModConfig.getAllowedControllerButtonList(),
                tooltip: () => I18n.GMCMOpenPetHatSlotButtonOptionDescription());
            configMenu.AddTextOption(
                mod: modManifest,
                name: () => I18n.GMCMOpenPetAccessorySlotButtonOption(),
                getValue: () => modConfig.OpenAccessorySlotHotkeySubstituteForGamepad.ToString(),
                setValue: value => modConfig.OpenAccessorySlotHotkeySubstituteForGamepad = ModConfig.stringToControllerSButton(value),
                allowedValues: ModConfig.getAllowedControllerButtonList(),
                tooltip: () => I18n.GMCMOpenPetAccessorySlotButtonOptionDescription());
            configMenu.AddTextOption(
                mod: modManifest,
                name: () => I18n.GMCMSubstituteButtonForLShiftOnGamepadOption(),
                getValue: () => modConfig.LShiftSubstituteForGamepad.ToString(),
                setValue: value => modConfig.LShiftSubstituteForGamepad = ModConfig.stringToControllerSButton(value),
                allowedValues: ModConfig.getAllowedControllerButtonList(),
                tooltip: () => I18n.GMCMSubstituteButtonForLShiftOnGamepadOptionDescription()
                );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => I18n.GMCMUseL1R1ShortcutsOption(),
                getValue: () => modConfig.UseL1andR1backgroundChangeShortcuts,
                setValue: value => modConfig.UseL1andR1backgroundChangeShortcuts = value,
                tooltip: () => I18n.GMCMUseL1R1ShortcutsOptionDescription());
        }
        public static IContentPatcherAPI GetContentPatcherInterface()
        {
            return contentPatcherApi;
        }
        public static void RegisterModOnQuickSaveMod(IModHelper helper)
        {
            var quickSaveAPI = helper.ModRegistry.GetApi<IQuickSaveAPI>("DLX.QuickSave");
            if (quickSaveAPI is null) return;

            quickSaveAPI.SavingEvent += OnQuickSaveSavingEvent;
            quickSaveAPI.LoadingEvent += OnQuickSaveLoadingEvent;
            quickSaveAPI.LoadedEvent += OnQuickSaveLoadedEvent;
        }

        private static Dictionary<Guid, QuickSave_PetInformation> _SavedQuickSave_PetInformation = new();
        public static void OnQuickSaveSavingEvent(object sender, ISavingEventArgs e)
        {
            foreach (var _petInfoKit in CachePetData.PetCache)
            {
                if (_petInfoKit.Value is not null && _petInfoKit.Value.Pet is not null && _petInfoKit.Value.Info is not null)
                {
                    if (!_SavedQuickSave_PetInformation.ContainsKey(_petInfoKit.Key))
                    {
                        _SavedQuickSave_PetInformation.TryAdd(_petInfoKit.Key, new QuickSave_PetInformation(_petInfoKit.Value.Pet));
                    }
                    else { _SavedQuickSave_PetInformation[_petInfoKit.Key] = new QuickSave_PetInformation(_petInfoKit.Value.Pet); }
                }
            }
        }
        public static void OnQuickSaveLoadingEvent(object sender, ILoadingEventArgs e)
        {
            foreach (var _petInfoKit in CachePetData.PetCache)
            {
                if (_petInfoKit.Value is not null && _petInfoKit.Value.Pet is not null && _petInfoKit.Value.Info is not null)
                {
                    if (!_SavedQuickSave_PetInformation.ContainsKey(_petInfoKit.Key))
                    {
                        _SavedQuickSave_PetInformation.TryAdd(_petInfoKit.Key, new QuickSave_PetInformation(_petInfoKit.Value.Pet));
                    }
                }
            }
            CachePetData.PetCache.Clear();
            CachePetData.CachePetTeams.Clear();
            CachePetData.SendPetToPlayerCache.Clear();
        }
        public static void OnQuickSaveLoadedEvent(object sender, ILoadedEventArgs e)
        {
            if (_SavedQuickSave_PetInformation is null) { _SavedQuickSave_PetInformation = new(); }

            foreach (var _petInfoKit in CachePetData.PetCache)
            {
                if (_petInfoKit.Value is not null && _petInfoKit.Value.Pet is not null && _petInfoKit.Value.Info is not null)
                {
                    if (_SavedQuickSave_PetInformation.TryGetValue(_petInfoKit.Key, out var _petInformationLoaded))
                    {
                        _petInformationLoaded.LoadSmartpet(_petInfoKit.Value.Pet, _petInfoKit.Value.Info);
                    }
                    if (ModDataLibrary.ContainsKey(_petInfoKit.Key))
                    {
                        ModDataLibrary[_petInfoKit.Key] = _petInfoKit.Value.Info;
                    }
                }
            }
        }
        public readonly struct QuickSave_PetInformation
        {
            public readonly Guid PetID;
            private readonly Vector2 SmartPet_position;
            private readonly Vector2 OriginalPet_position;
            private readonly bool OriginalPet_flip;
            private readonly bool SmartPet_flip;
            private readonly string OriginalPet_LocationNameOrUniqueName;
            private readonly string SmartPet_LocationNameOrUniqueName;
            private readonly SmartPet.PetObjective SmartPet_petObjective;
            private readonly SmartPet.PetObjective SmartPet_PrevPetObjective;
            private readonly bool SmartPet_IsSwimming;
            private readonly Vector2 SmartPet_Motion;
            private readonly int OriginalPet_CurrentFrame;
            private readonly int SmartPet_CurrentFrame;
            private readonly long? SmartPet_GroupLeader;
            private readonly int SmartPet_AttackCooldown;
            private readonly int SmartPet_petSearchPatienceTimer;
            private readonly int SmartPet_petSearchCooldown;

            private readonly int SmartPet_foodSaturationTimer;
            private readonly int SmartPet_emoteCooldown;
            private readonly bool SmartPet_IsEmoting;
            private readonly bool SmartPet_EmoteIsFading;
            private readonly float SmartPet_EmoteInterval;
            private readonly int SmartPet_currentEmoteFrame;
            private readonly int SmartPet_currentEmote;

            private readonly int OriginalPetFriendshipTowardFarmer;
            private readonly bool OriginalPetGrantedFriendshipForPet;
            public QuickSave_PetInformation(SmartPet _pet)
            {
                PetID = _pet.OriginalPetInstance.petId.Value;
                SmartPet_position = _pet.Position;
                OriginalPet_position = _pet.OriginalPetInstance.Position;
                OriginalPet_LocationNameOrUniqueName = _pet.OriginalPetInstance?.currentLocation?.NameOrUniqueName;
                SmartPet_LocationNameOrUniqueName = _pet.CurrentLocation?.NameOrUniqueName;
                OriginalPet_flip = _pet.OriginalPetInstance.flip;
                SmartPet_flip = _pet.flip;
                SmartPet_petObjective = _pet.CurrentPetObjective;
                SmartPet_PrevPetObjective = _pet.PrevPetObjective;
                SmartPet_Motion = _pet.Motion;
                OriginalPet_CurrentFrame = _pet.OriginalPetInstance.Sprite?.CurrentFrame ?? 0;
                SmartPet_CurrentFrame = _pet.Sprite?.CurrentFrame ?? 0;
                SmartPet_IsSwimming = _pet.Swimming;
                SmartPet_GroupLeader = _pet.GroupLeader?.UniqueMultiplayerID;
                PetInfo_Age = _pet._petInfo.Age;
                PetInfo_HasBeenGivenTreatToday = _pet._petInfo.HasBeenGivenTreatToday;
                PetInfo_TrainedToday = _pet._petInfo.TrainedToday;
                PetInfo_BackgroundStyle_Index = _pet._petInfo.BackgroundStyle_Index;

                PetInfo_SkillMastery_level = new double[_pet._petInfo.SkillMastery_level.Length];
                _pet._petInfo.SkillMastery_level.AsSpan().CopyTo(PetInfo_SkillMastery_level);

                PetInfo_SkillPerkChecklist = new bool[_pet._petInfo.SkillPerkChecklist.Length][];
                _pet._petInfo.SkillPerkChecklist.AsSpan().CopyTo(PetInfo_SkillPerkChecklist);

                PetInfo_Energy = _pet._petInfo.Energy;
                PetInfo_MaxBaseEnergy = _pet._petInfo.MaxBaseEnergy;
                SmartPet_AttackCooldown = _pet.AttackCooldown;
                SmartPet_petSearchPatienceTimer = _pet.petSearchPatienceTimer;
                SmartPet_petSearchCooldown = _pet.petSearchCooldown;

                SmartPet_foodSaturationTimer = _pet.foodSaturationTimer;
                SmartPet_emoteCooldown = _pet.emoteCooldown;
                SmartPet_IsEmoting = _pet.isEmoting;
                SmartPet_EmoteIsFading = _pet.EmoteFading;
                SmartPet_EmoteInterval = _pet.emoteInterval;
                SmartPet_currentEmoteFrame = _pet.currentEmoteFrame;
                SmartPet_currentEmote = _pet.currentEmote;

                OriginalPetFriendshipTowardFarmer = _pet.OriginalPetInstance.friendshipTowardFarmer.Value;
                OriginalPetGrantedFriendshipForPet = _pet.OriginalPetInstance.grantedFriendshipForPet.Value;
            }
            public void LoadSmartpet(SmartPet _pet, PetInfo _info)
            {
                _pet.Position = SmartPet_position;
                _pet.OriginalPetInstance.Position = OriginalPet_position;
                if (!string.IsNullOrEmpty(OriginalPet_LocationNameOrUniqueName))
                {
                    var _location = Game1.getLocationFromName(OriginalPet_LocationNameOrUniqueName);
                    if (_location is not null)
                    {
                        _pet.OriginalPetInstance.currentLocation?.characters?.RemoveWhere(x => x is StardewValley.Characters.Pet _IsPet && _IsPet.petId?.Value == _pet.OriginalPetInstance.petId?.Value && _IsPet.Name == _pet.OriginalPetInstance.Name);
                        _location.characters?.RemoveWhere(x => x is StardewValley.Characters.Pet _IsPet && _IsPet.petId?.Value == _pet.OriginalPetInstance.petId?.Value && _IsPet.Name == _pet.OriginalPetInstance.Name);
                        Game1.warpCharacter(_pet.OriginalPetInstance, _location, OriginalPet_position);
                    }
                }
                if (!string.IsNullOrEmpty(SmartPet_LocationNameOrUniqueName))
                {
                    var _location = Game1.getLocationFromName(SmartPet_LocationNameOrUniqueName);
                    if (_location is not null)
                    {
                        _pet.CurrentLocation = _location;
                    }
                }
                _pet.Swimming = SmartPet_IsSwimming;
                _pet.CurrentPetObjective = SmartPet_PrevPetObjective;
                if (SmartPet_GroupLeader is not null)
                {
                    if (Game1.GetPlayer(SmartPet_GroupLeader.Value, true).IsNull(out var _farmer))
                    {
                        CachePetData.CachePetTeams.RemovePetFromTeam(_pet._petInfo.PetId, _pet.GroupLeader?.UniqueMultiplayerID);
                        _pet.SetPetObjective(SmartPet.PetObjective.None, Game1.player);
                        _pet.ResetVariables();
                        _pet.SetOPetAtFarmPosition();
                        _pet.SetSPetToCopyOPet();
                    }
                    else
                    {
                        _pet.SetPetObjective(SmartPet_petObjective, _farmer);
                        if (SmartPet_petObjective == SmartPet.PetObjective.Wait)
                        {
                            CachePetData.CachePetTeams.RelocatePetToTeamOtherwiseCreateNew(_pet._petInfo.PetId, SmartPet_GroupLeader.Value);
                            _pet.Position = SmartPet_position;
                        }
                    }
                }
                else { _pet.SetPetObjective(SmartPet_petObjective, Game1.player); }

                _pet._petInfo.Age = PetInfo_Age;
                _pet._petInfo.HasBeenGivenTreatToday = PetInfo_HasBeenGivenTreatToday;
                _pet._petInfo.TrainedToday = PetInfo_TrainedToday;
                _pet._petInfo.BackgroundStyle_Index = PetInfo_BackgroundStyle_Index;
                _pet._petInfo.SkillMastery_level = PetInfo_SkillMastery_level;
                _pet._petInfo.SkillPerkChecklist = PetInfo_SkillPerkChecklist;

                _pet._petInfo.SkillMastery_level = new double[PetInfo_SkillMastery_level.Length];
                PetInfo_SkillMastery_level.AsSpan().CopyTo(_pet._petInfo.SkillMastery_level);

                _pet._petInfo.SkillPerkChecklist = new bool[PetInfo_SkillPerkChecklist.Length][];
                PetInfo_SkillPerkChecklist.AsSpan().CopyTo(_pet._petInfo.SkillPerkChecklist);

                _pet._petInfo.Energy = PetInfo_Energy;
                _pet._petInfo.MaxBaseEnergy = PetInfo_MaxBaseEnergy;

                _pet.AttackCooldown = SmartPet_AttackCooldown;
                _pet.petSearchCooldown = SmartPet_petSearchCooldown;
                _pet.petSearchPatienceTimer = SmartPet_petSearchPatienceTimer;

                _pet.foodSaturationTimer = SmartPet_foodSaturationTimer;
                _pet.emoteCooldown = SmartPet_emoteCooldown;
                _pet.isEmoting = SmartPet_IsEmoting;
                _pet.EmoteFading = SmartPet_EmoteIsFading;
                _pet.emoteInterval = SmartPet_EmoteInterval;
                _pet.currentEmoteFrame = SmartPet_currentEmoteFrame;
                _pet.currentEmote = SmartPet_currentEmote;

                _pet.OriginalPetInstance.flip = OriginalPet_flip;
                _pet.flip = SmartPet_flip;
                _pet.Sprite.CurrentFrame = SmartPet_CurrentFrame;
                if (_pet.OriginalPetInstance.Sprite is not null)
                {
                    _pet.OriginalPetInstance.Sprite.CurrentFrame = OriginalPet_CurrentFrame;
                }

                _pet.OriginalPetInstance.grantedFriendshipForPet.Set(newValue: OriginalPetGrantedFriendshipForPet);
                _pet.OriginalPetInstance.friendshipTowardFarmer.Set(newValue:OriginalPetFriendshipTowardFarmer);

                _info = _pet._petInfo;
            }
            private readonly int PetInfo_Age;
            private readonly bool PetInfo_HasBeenGivenTreatToday;
            private readonly bool PetInfo_TrainedToday;
            private readonly int PetInfo_BackgroundStyle_Index;
            private readonly double[] PetInfo_SkillMastery_level;
            private readonly bool[][] PetInfo_SkillPerkChecklist;
            private readonly int PetInfo_Energy;
            private readonly int PetInfo_MaxBaseEnergy;
        }
    }
}

