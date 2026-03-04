using Microsoft.Xna.Framework.Input;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Characters;
using System.Text;
using Pets_Enhanced_Mod.Utilities.Custom_Classes;
using System.Threading.Tasks;
using Pets_Enhanced_Mod.Utilities;
using static Pets_Enhanced_Mod.Utilities.SunkenLaceUtilities;
using Pets_Enhanced_Mod.Multiplayer;
using StardewValley.Locations;

namespace Pets_Enhanced_Mod.Data
{
    internal class PetResponses
    {
        public static Response[] MainCommandPetOptions(Pet forPet, Farmer who)
        {
            if (forPet?.modData is null) { return new Response[1] { new Response("Nothing", I18n.NothingCommand()).SetHotKey(Keys.Escape) }; }

            if (!SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_CurrentPetObjective, out int currentPetObjective))
            {
                return new Response[1] { new Response("Nothing", I18n.NothingCommand()).SetHotKey(Keys.Escape) };
            }

            SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_LeaderUniqueMultiplayerID, out long? groupLeaderID);

            bool flag = false;
            if (!SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_Waiting_SkillMasteryLevel, out double waiting_SkillMasteryLevel)) { flag = true; }
            if (!SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_Following_SkillMasteryLevel, out double following_SkillMasteryLevel)) { flag = true; }
            if (!SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_Foraging_SkillMasteryLevel, out double foraging_SkillMasteryLevel)) { flag = true; }
            if (!SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_Fishing_SkillMasteryLevel, out double fishing_SkillMasteryLevel)) { flag = true; }
            if (!SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_Hunting_SkillMasteryLevel, out double hunting_SkillMasteryLevel)) { flag = true; }

            bool isSwimming = true;
            if (SynchronizationManager.CurrentClientInformation.TryGetValue(forPet.petId.Value, out var currentPetInformation))
            {
                isSwimming = SynchronizationManager.IsBitSet(2, currentPetInformation.Flags);
            }
            PE_Pet_Data petData = CachePetData.GetPetDataForPet(forPet);

            if (petData is null || flag) { return new Response[1] { new Response("Nothing", I18n.NothingCommand()).SetHotKey(Keys.Escape) }; }

            bool petManualInHand = who.CurrentItem is not null && who.CurrentItem.QualifiedItemId.Equals(SmartPet.PetManualQUID);

            List<Response> AllResponses = new();
            bool tooManyFollowers = groupLeaderID is not null && groupLeaderID.Value != who.UniqueMultiplayerID;
            bool objectiveWait = currentPetObjective == 2;
            string spaceForTreatIcon = "  ";
            switch (LocalizedContentManager.CurrentLanguageCode)
            {
                case LocalizedContentManager.LanguageCode.zh:
                    spaceForTreatIcon = "      ";
                    break;
                case LocalizedContentManager.LanguageCode.ko:
                    spaceForTreatIcon = "   ";
                    break;
                case LocalizedContentManager.LanguageCode.mod:
                    {
                        if (LocalizedContentManager.CurrentLanguageString == "vi")
                        {
                            spaceForTreatIcon = "      ";
                            break;
                        }
                        spaceForTreatIcon = "     ";
                    }
                    break;
            }
            if (!objectiveWait && petData.HasWaitSkill) 
            { 
                if (waiting_SkillMasteryLevel >= 1)
                {
                    AllResponses.Add(new Response("WaitCommand", I18n.WaitCommand()).SetHotKey(Keys.NumPad1));
                }
                else if (petManualInHand)
                {
                    AllResponses.Add(new Response("FreeUnlockWaitCommand", I18n.UnlockWaitCommandResponse()).SetHotKey(Keys.NumPad1));
                }
            }

            if (!objectiveWait && petData.HasFollowSkill && currentPetObjective != 1)
            {
                if (following_SkillMasteryLevel >= 1 && !tooManyFollowers)
                {
                    AllResponses.Add(new Response("FollowMeCommand", I18n.FollowMeCommand()).SetHotKey(Keys.NumPad2));
                }
                else if (petManualInHand && following_SkillMasteryLevel < 1)
                {
                    if (following_SkillMasteryLevel > 0f)
                    {
                        AllResponses.Add(new Response("UnlockFollowMeCommand", I18n.ProgressFollowMeCommandResponse(PetTreatIcon: spaceForTreatIcon, CommandPrice: 5 + (5 * (int)(4 * following_SkillMasteryLevel)), LessonProgress: following_SkillMasteryLevel * 100f)).SetHotKey(Keys.NumPad2));
                    }
                    else { AllResponses.Add(new Response("UnlockFollowMeCommand", I18n.UnlockFollowMeCommandResponse(PetTreatIcon: spaceForTreatIcon, CommandPrice: 5 + (5 * (int)(4 * following_SkillMasteryLevel)))).SetHotKey(Keys.NumPad2)); }
                }
                
            }

            if (!objectiveWait && petData.HasHuntSkill && currentPetObjective != 3)
            {
                if (hunting_SkillMasteryLevel >= 1 && !tooManyFollowers)
                {
                    AllResponses.Add(new Response("HuntCommand", I18n.HuntCommand()).SetHotKey(Keys.NumPad3));
                }
                else if (petManualInHand && hunting_SkillMasteryLevel < 1)
                {
                    if (hunting_SkillMasteryLevel > 0f)
                    {
                        AllResponses.Add(new Response("UnlockHuntCommand", I18n.ProgressHuntCommandResponse(PetTreatIcon: spaceForTreatIcon, CommandPrice: 15 + (15 * (int)(4 * hunting_SkillMasteryLevel)), LessonProgress: hunting_SkillMasteryLevel * 100f)).SetHotKey(Keys.NumPad3));
                    }
                    else { AllResponses.Add(new Response("UnlockHuntCommand", I18n.UnlockHuntCommandResponse(PetTreatIcon: spaceForTreatIcon, CommandPrice: 15 + (15 * (int)(4 * hunting_SkillMasteryLevel)))).SetHotKey(Keys.NumPad3)); }
                }
                
            }

            if (!objectiveWait && petData.HasForageSkill && currentPetObjective != 4) 
            {
                if (foraging_SkillMasteryLevel >= 1 && !tooManyFollowers)
                {
                    AllResponses.Add(new Response("ForageCommand", I18n.ForageCommand()).SetHotKey(Keys.NumPad4));
                }
                else if (petManualInHand && foraging_SkillMasteryLevel < 1)
                {
                    if (foraging_SkillMasteryLevel > 0f)
                    {
                        AllResponses.Add(new Response("UnlockForageCommand", I18n.ProgressForageCommandResponse(PetTreatIcon: spaceForTreatIcon, CommandPrice: 10 + (10 * (int)(4 * foraging_SkillMasteryLevel)), LessonProgress: foraging_SkillMasteryLevel * 100f)).SetHotKey(Keys.NumPad4));
                    }
                    else { AllResponses.Add(new Response("UnlockForageCommand", I18n.UnlockForageCommandResponse(PetTreatIcon: spaceForTreatIcon, CommandPrice: 10 + (10 * (int)(4 * foraging_SkillMasteryLevel)))).SetHotKey(Keys.NumPad4)); }
                }
            }

            if (!objectiveWait && petData.HasFishingSkill && currentPetObjective != 5)
            {
                if (fishing_SkillMasteryLevel >= 1 && !tooManyFollowers)
                {
                    AllResponses.Add(new Response("FishCommand", I18n.FishCommand()).SetHotKey(Keys.NumPad5));
                }
                else if (petManualInHand && fishing_SkillMasteryLevel < 1)
                {
                    if (fishing_SkillMasteryLevel > 0f)
                    {
                        AllResponses.Add(new Response("UnlockFishCommand", I18n.ProgressFishingCommandResponse(PetTreatIcon: spaceForTreatIcon, CommandPrice: 10 + (10 * (int)(4 * fishing_SkillMasteryLevel)), LessonProgress: fishing_SkillMasteryLevel * 100f)).SetHotKey(Keys.NumPad5));
                    }
                    else { AllResponses.Add(new Response("UnlockFishCommand", I18n.UnlockFishingCommandResponse(PetTreatIcon: spaceForTreatIcon, CommandPrice: 10 + (10 * (int)(4 * fishing_SkillMasteryLevel)))).SetHotKey(Keys.NumPad5)); }
                }
            }

            if (!objectiveWait && currentPetObjective != 0 && (who.currentLocation is Farm || who.currentLocation is FarmHouse) && !isSwimming) { AllResponses.Add(new Response("ReleaseCommand", I18n.ReleaseCommand()).SetHotKey(Keys.NumPad6)); }
            else if (!objectiveWait && currentPetObjective != 0 && !(who.currentLocation is Farm || who.currentLocation is FarmHouse)) { AllResponses.Add(new Response("GoHomeCommand", I18n.GoHomeCommand()).SetHotKey(Keys.NumPad6)); }

            if (objectiveWait)
            {
                AllResponses.Add(new Response("StopWaiting", I18n.StopWaitingCommand()).SetHotKey(Keys.NumPad1));
            }

            if (who.CurrentItem is not null && who.CurrentItem.QualifiedItemId.Equals("(O)ButterflyPowder")) { AllResponses.Add(new Response("ApplyButterflyPowderOption", I18n.ApplyButterflyPowderOption()).SetHotKey(Keys.NumPad8)); }

            AllResponses.Add(new Response("Nothing", I18n.NothingCommand()).SetHotKey(Keys.Escape));
            return AllResponses.ToArray();
        }

        public static Response[] CallPetOptions(Farmer who, List<Pet> _petCandidates)
        {
            if (who is null) { return Array.Empty<Response>(); }

            Response[] allResponses = new Response[_petCandidates.Count + 1];
            int numpadID = 13;
            for (int index = 0; index < _petCandidates.Count; index++)
            {
                allResponses[index] = new Response(_petCandidates[index].petId.Value.ToString(), _petCandidates[index].Name).SetHotKey((Keys)numpadID);
                if (numpadID == 13)
                {
                    numpadID = 97;
                }
                else if (numpadID < 106)
                {
                    numpadID++;
                }
            }
            allResponses[_petCandidates.Count] = new Response("Cancel", "..." + I18n.DontUnlockCommandsOption()).SetHotKey(Keys.Escape);
            return allResponses;
        }
    }
}
