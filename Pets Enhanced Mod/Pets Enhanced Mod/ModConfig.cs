using System;
using StardewValley;
using System.Collections.Generic;
using StardewModdingAPI;
using System.Linq;
namespace Pets_Enhanced_Mod
{
    public class ModConfig
    {
        public enum controllerLayouts
        {
            PS4 = 0,
            Xbox360 = 1,
            SteamDeck = 2,
            Switch = 3,
            Generic = 4
        }
        public ModConfig() { }
        public ModConfig(bool doDefault)
        {
            if (doDefault)
            {
                KeyboardInteractionKey = SButton.None;
                KeyboardStatusTabHotkey = SButton.Q;
                KeyboardInventoryTabHotkey = SButton.F;
                KeyboardHatTabHotkey = SButton.D1;
                KeyboardAccessoryTabHotkey = SButton.D2;
                SubstituteDNumpadWithSymbols = false;
                CurrentGamepadLayout = controllerLayouts.Generic;
                GrabItemButtonSubstituteForGamepad = SButton.ControllerA;
                StackItemButtonSubstituteForGamepad = SButton.ControllerX;
                CloseUIButtonSubstituteForGamepad = SButton.RightTrigger;
                OpenInventoryHotkeySubstituteForGamepad = SButton.ControllerB;
                OpenStatusWindowHotkeySubstituteForGamepad = SButton.ControllerY;
                OpenHatSlotHotkeySubstituteForGamepad = SButton.LeftShoulder;
                OpenAccessorySlotHotkeySubstituteForGamepad = SButton.LeftTrigger;
                LShiftSubstituteForGamepad = SButton.LeftShoulder;
                UseL1andR1backgroundChangeShortcuts = true;
            }
        }
        public void Reset()
        {
            KeyboardInteractionKey = SButton.None;
            KeyboardStatusTabHotkey = SButton.Q;
            KeyboardInventoryTabHotkey = SButton.F;
            KeyboardHatTabHotkey = SButton.D1;
            KeyboardAccessoryTabHotkey = SButton.D2;
            SubstituteDNumpadWithSymbols = false;

            CurrentGamepadLayout = controllerLayouts.Generic;
            GrabItemButtonSubstituteForGamepad = SButton.ControllerA;
            StackItemButtonSubstituteForGamepad = SButton.ControllerX;
            CloseUIButtonSubstituteForGamepad = SButton.RightTrigger;
            OpenInventoryHotkeySubstituteForGamepad = SButton.ControllerB;
            OpenStatusWindowHotkeySubstituteForGamepad = SButton.ControllerY;
            OpenHatSlotHotkeySubstituteForGamepad = SButton.LeftShoulder;
            OpenAccessorySlotHotkeySubstituteForGamepad = SButton.LeftTrigger;
            LShiftSubstituteForGamepad = SButton.LeftShoulder;
            UseL1andR1backgroundChangeShortcuts = true;
        }
        public static controllerLayouts stringToControllerLayout(string name)
        {
            return name.Equals("PS4") ? controllerLayouts.PS4 : name.Equals("Xbox360") ? controllerLayouts.Xbox360 : name.Equals("SteamDeck") ? controllerLayouts.SteamDeck : name.Equals("Switch") ? controllerLayouts.Switch : controllerLayouts.Generic;
        }
        public static string[] getAllowedControllerLayoutList()
        {
            return new string[] { "PS4", "Xbox360", "SteamDeck", "Switch", "Generic" };
        }
        public static SButton stringToControllerSButton(string name)
        {
            return name.Equals("ControllerA") ? SButton.ControllerA : name.Equals("ControllerB") ? SButton.ControllerB : name.Equals("ControllerX") ? SButton.ControllerX : name.Equals("ControllerY") ? SButton.ControllerY :
                name.Equals("LeftShoulder") ? SButton.LeftShoulder : name.Equals("LeftTrigger") ? SButton.LeftTrigger : name.Equals("RightShoulder") ? SButton.RightShoulder : name.Equals("RightTrigger") ? SButton.RightTrigger :
                name.Equals("ControllerBack") ? SButton.ControllerBack : name.Equals("ControllerStart") ? SButton.ControllerStart : SButton.None;
        }
        public static string[] getAllowedControllerButtonList()
        {
            return new string[] { "ControllerA", "ControllerB", "ControllerX", "ControllerY", "LeftShoulder", "LeftTrigger", "RightShoulder", "RightTrigger", "ControllerBack", "ControllerStart" };
        }
        public SButton KeyboardInteractionKey { get; set; } = SButton.None;
        public SButton KeyboardStatusTabHotkey { get; set; } = SButton.Q;
        public SButton KeyboardInventoryTabHotkey { get; set; } = SButton.F;
        public SButton KeyboardHatTabHotkey { get; set; } = SButton.D1;
        public SButton KeyboardAccessoryTabHotkey { get; set; } = SButton.D2;
        public bool SubstituteDNumpadWithSymbols { get; set; } = false;
        public controllerLayouts CurrentGamepadLayout { get; set; } = controllerLayouts.Generic;
        public SButton GrabItemButtonSubstituteForGamepad { get; set; } = SButton.ControllerA;
        public SButton StackItemButtonSubstituteForGamepad { get; set; } = SButton.ControllerX;
        public SButton CloseUIButtonSubstituteForGamepad { get; set; } = SButton.RightTrigger;
        public SButton OpenInventoryHotkeySubstituteForGamepad { get; set; } = SButton.ControllerB;
        public SButton OpenStatusWindowHotkeySubstituteForGamepad { get; set; } = SButton.ControllerY;
        public SButton OpenHatSlotHotkeySubstituteForGamepad { get; set; } = SButton.LeftShoulder;
        public SButton OpenAccessorySlotHotkeySubstituteForGamepad { get; set; } = SButton.LeftTrigger;
        public SButton LShiftSubstituteForGamepad { get; set; } = SButton.LeftShoulder;
        public bool UseL1andR1backgroundChangeShortcuts { get; set; } = true;


    }



}