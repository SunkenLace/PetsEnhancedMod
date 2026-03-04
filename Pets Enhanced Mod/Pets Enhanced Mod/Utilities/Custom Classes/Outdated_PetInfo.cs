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
using System.Linq;
using static Pets_Enhanced_Mod.Utilities.Custom_Classes.SmartPet;

namespace Pets_Enhanced_Mod.Utilities.Custom_Classes
{
    public class Outdated_PetInfo
    {
        public PetManual petManual = new PetManual(Array.Empty<PetManual.CommandsEnum>());
        public int ageInDays { get; set; } = 0;
        public bool petTricksUnlocked { get; set; } = false;
    }
    public struct PetManual
    {
        public enum CommandsEnum { Follow, Wait, Hunt, Search }
        public CommandsEnum[] CommandsList { get; set; } = Array.Empty<CommandsEnum>();
        public bool TricksUnlocked = false;
        public bool WaitTrickUnlocked => WaitTrickUnlockPercent >= 1;
        public double WaitTrickUnlockPercent = 0d;
        public bool FollowMeTrickUnlocked => FollowMeTrickUnlockPercent >= 1;
        public double FollowMeTrickUnlockPercent = 0d;
        public bool HuntTrickUnlocked => HuntTrickUnlockPercent >= 1;
        public double HuntTrickUnlockPercent = 0d;
        public bool SearchTrickUnlocked => SearchTrickUnlockPercent >= 1;
        public double SearchTrickUnlockPercent = 0d;
        public PetManual(PetManual.CommandsEnum[] commandList)
        {
            this.CommandsList = commandList;
        }
    }
}
