using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley;
using System.Linq;
using StardewValley.Characters;
using StardewModdingAPI;
using StardewValley.Network;
using StardewValley.Inventories;
using StardewValley.Locations;
using System.Threading;
using StardewValley.Objects;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Pets_Enhanced_Mod.Utilities.Custom_Classes.PetDialogueBox;
using static Pets_Enhanced_Mod.ModEntry;
using StardewValley.Extensions;
using Force.DeepCloner;
using static System.Net.Mime.MediaTypeNames;
using StardewModdingAPI.Events;
using StardewValley.GameData.HomeRenovations;
using System.Runtime.CompilerServices;
using Pets_Enhanced_Mod.Data;
using Pets_Enhanced_Mod.Multiplayer;

namespace Pets_Enhanced_Mod.Utilities.Custom_Classes
{
    public class PetDialogueBox : IClickableMenu
    {
        public List<string> dialogues = new();

        public Response[] responses = Array.Empty<Response>();

        public delegate void AfterQuestionAction(string _answer, Farmer _player,Guid _id);

        public AfterQuestionAction actionAfterQuestion;

        public PetStatusWindow StatusWindow;

        public PetStorageMenu PlayerInventory;

        public PetStorageMenu PetBackpack;
        public PetStorageMenu PetHatSlot;
        public PetStorageMenu PetAccessorySlot;
        public PetStorageMenu PetPocketSlot;

        public int hoveringInventoryChestTimer = 0;

        public enum eCurrentInterface
        {
            None = 0,
            Hat = 1,
            Accessory = 2,
            InventoryOne = 3,
            InventoryMultiple = 4,
            StatusTab = 5
        }
        /// <summary>-1 means don't show confirmation text. 0 means show confirmation text. 1 means yes.</summary>
        private int DoConfirmation = -1;
        private SButton StatusTabHotkey = SButton.Q;
        private SButton InventoryTabHotkey = SButton.F;
        private SButton HatTabHotkey = SButton.NumPad1;
        private SButton AccessoryTabHotkey = SButton.NumPad2;
        private SButton GrabItemButtonSubstituteForGamepad = SButton.ControllerA;
        private SButton StackItemButtonSubstituteForGamepad = SButton.ControllerX;
        private SButton CloseUIButtonSubstituteForGamepad = SButton.RightTrigger;
        private SButton OpenInventoryHotkeySubstituteForGamepad = SButton.ControllerB;
        private SButton OpenStatusWindowHotkeySubstituteForGamepad = SButton.ControllerY;
        private SButton OpenHatSlotHotkeySubstituteForGamepad = SButton.LeftShoulder;
        private SButton OpenAccessorySlotHotkeySubstituteForGamepad = SButton.LeftTrigger;
        private SButton LShiftSubstituteForGamepad = SButton.LeftShoulder;
        private bool UseL1andR1backgroundChangeShortcuts = true;

        /// <summary>0 defines playstation. 1 defines xbox. 2 defines steamdeck. 3 defines switch. 4 defines other controllers.</summary>
        private int CurrentGamepadLayout = 6;
        public Point CurrentMousePosition = Point.Zero;
        private bool MouseMoved = false;
        public eCurrentInterface CurrentInterface = eCurrentInterface.None;

        public Rectangle okButtonBBox = new Rectangle(0, 0, 72, 72);
        public float okButtonTextureScale = 1f;

        public float HatIconScale = 0.9f;
        public float InventoryItemScale = 0.825f;
        public float AccessoryIconScale = 0.825f;
        public float PetIconScale = 0.825f;
        public ButtonPressedEventArgs buttonPressedEventArgs;
        public Rectangle PlayerInventorySpriteRec = Rectangle.Empty;

        public Rectangle EnergyFrameRec = Rectangle.Empty;
        public Rectangle HatFrameSpriteRec = Rectangle.Empty;
        public Rectangle AccFrameSpriteRec = Rectangle.Empty;
        public Rectangle PInventoryMFrameSpriteRec = Rectangle.Empty;
        public Rectangle PInventorySFrameSpriteRec = Rectangle.Empty;
        private bool isThereAnyPerkReadyToBeUnlocked = false;
        private bool doHandIconTap = false;
        public SpriteRectangle SkillMasteryRelatedNotificationIcon;
        public int questionFinishPauseTimer;
        private int MouseInMotion = 0;
        public bool hoveringOkayButton = false;

        /// <summary>
        /// For controller compatibility, it lets the mod know what item of the gui is being selected with a controller.
        /// </summary> <remarks> -1 means no object should be selected
        /// 0 means no item is being currently selected.
        /// 1 means pet status frame.
        /// 2 means energy bar.
        /// 3 means inventory frame.
        /// 4 means hat frame.
        /// 5 means pet collar frame.
        /// 6 and beyond means a dialogue option
        /// </remarks>
        public int CurrentControlledObject = 0;
        private int CurrentControlledArrowDelta = 0;

        protected bool _showedOptions;

        public List<ClickableComponent> responseCC;

        public int x;

        public int y;

        public int transitionX = -1;

        public int transitionY;

        public int transitionWidth;

        public int transitionHeight;

        public int safetyTimer = 750;

        public int heightForQuestions;


        public bool transitionInitialized;

        //
        // Summary:
        //     Whether to progressively type the dialogue text into the box. If false, the dialogue
        //     appears instantly instead.

        public bool transitioning = true;

        public bool transitioningBigger = true;

        public Texture2D UISprites_Texture;

        public Item HeldItem;

        public readonly Pet SelectedPet;

        public readonly Inventory PetInventory = null;

        public readonly NetMutex PetInventoryMutex = null;

        public bool NewItemObtained = false;

        public bool NewSkillLearned = false;

        public readonly List<Item> temporaryHatSlot = new(1) { null };
        public readonly List<Item> temporaryAccessorySlot = new(1) { null };
        public readonly List<Item> temporaryPocketItem = new(1) { null };
        public readonly List<Item> temporaryBackpack = new(6) { null, null, null, null, null, null };

        public int BackgroundStyleIndex = 0;

        public bool BackpackUnlocked = false;

        public static void CreateNewDialogueBoxClient(Pet forPet, AfterQuestionAction afterDialogueBehavior)
        {
            if (forPet?.modData is null || SynchronizationManager.TryGetPetInventoryWithMutex(forPet, out var mutex) is null || mutex is null) { return; }

            Game1.currentLocation.lastQuestionKey = null;
            Game1.currentLocation.afterQuestion = null;

            Game1.currentSpeaker = null;
            Game1.activeClickableMenu = new PetDialogueBox(I18n.CommandPetQuestion(PetName: forPet.Name), PetResponses.MainCommandPetOptions(forPet, Game1.player), afterDialogueBehavior, forPet);
            Game1.dialogueUp = true;
            Game1.player.CanMove = false;
            Game1.objectDialoguePortraitPerson = null;
        }
        public PetDialogueBox(string dialogue, Response[] responses, AfterQuestionAction _actionAfterQuestion, Pet selectedPet, int width = 1060)
        {
            CurrentMousePosition = new Point(Game1.getMouseX(), Game1.getMouseY());
            if (Game1.options.gamepadControls)
            {
                MouseMoved = false;
                CurrentControlledObject = 1;
            }
            else { MouseMoved = true; }

            this.actionAfterQuestion = _actionAfterQuestion;
            dialogues.Add(dialogue);
            this.responses = responses;
            base.width = width;
            setUpQuestions();
            height = heightForQuestions;
            x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width - 88, height).X;
            y = Game1.uiViewport.Height - height - 64;
            this.UISprites_Texture = PetHelper.TryLoadTextureEfficiently("Mods\\SunkenLace.PetsEnhancedMod\\Textures\\UI\\UISprites");
            setUpIcons();
            if (responses != null)
            {
                foreach (Response response in responses)
                {
                    response.responseText = Dialogue.applyGenderSwitch(Game1.player.Gender, response.responseText, altTokenOnly: true);
                }
            }

            this.SelectedPet = selectedPet;
            this.PetInventory = SynchronizationManager.TryGetPetInventoryWithMutex(selectedPet,out PetInventoryMutex);
            SynchronizationManager.TryParseModData(selectedPet.modData, SynchronizationManager.PetModDataKey_NewItemObtained, out NewItemObtained);
            SynchronizationManager.TryParseModData(selectedPet.modData, SynchronizationManager.PetModDataKey_PetLearnedNewSkill, out NewSkillLearned);
            SynchronizationManager.TryParseModData(selectedPet.modData, SynchronizationManager.PetModDataKey_BackgroundStyleIndex, out BackgroundStyleIndex);

            SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryHatSlot, SynchronizationManager.PetsEnhancedModInventoryHatSlotIndex);
            SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryAccessorySlot, SynchronizationManager.PetsEnhancedModInventoryAccessorySlotIndex);
            SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryPocketItem, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
            SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryBackpack, SynchronizationManager.PetsEnhancedModInventoryBackpackStartIndex);

            this.BackpackUnlocked = this.temporaryPocketItem is not null && this.temporaryPocketItem.Count > 0 && this.temporaryPocketItem[0] is not null && this.temporaryPocketItem[0].QualifiedItemId.Equals(SmartPet.PetBackpackQUID);

            SkillMasteryRelatedNotificationIcon = new SpriteRectangle(x - 78, y - 180, 4f, 230, 49, 7, 13);
            if (ModEntry.CurrentModConfig is not null)
            {
                StatusTabHotkey = ModEntry.CurrentModConfig.KeyboardStatusTabHotkey;
                InventoryTabHotkey = ModEntry.CurrentModConfig.KeyboardInventoryTabHotkey;
                HatTabHotkey = ModEntry.CurrentModConfig.KeyboardHatTabHotkey;
                AccessoryTabHotkey = ModEntry.CurrentModConfig.KeyboardAccessoryTabHotkey;
                CurrentGamepadLayout = (int)ModEntry.CurrentModConfig.CurrentGamepadLayout;

                GrabItemButtonSubstituteForGamepad = ModEntry.CurrentModConfig.GrabItemButtonSubstituteForGamepad;
                StackItemButtonSubstituteForGamepad = ModEntry.CurrentModConfig.StackItemButtonSubstituteForGamepad;
                CloseUIButtonSubstituteForGamepad = ModEntry.CurrentModConfig.CloseUIButtonSubstituteForGamepad;
                OpenInventoryHotkeySubstituteForGamepad = ModEntry.CurrentModConfig.OpenInventoryHotkeySubstituteForGamepad;
                OpenStatusWindowHotkeySubstituteForGamepad = ModEntry.CurrentModConfig.OpenStatusWindowHotkeySubstituteForGamepad;
                OpenHatSlotHotkeySubstituteForGamepad = ModEntry.CurrentModConfig.OpenHatSlotHotkeySubstituteForGamepad;
                OpenAccessorySlotHotkeySubstituteForGamepad = ModEntry.CurrentModConfig.OpenAccessorySlotHotkeySubstituteForGamepad;
                LShiftSubstituteForGamepad = ModEntry.CurrentModConfig.LShiftSubstituteForGamepad;
                UseL1andR1backgroundChangeShortcuts = ModEntry.CurrentModConfig.UseL1andR1backgroundChangeShortcuts;
                if (!ModEntry.CurrentModConfig.SubstituteDNumpadWithSymbols)
                {
                    InputTextureDictionary[SButton.D0] = InputTextureDictionary[SButton.NumPad0];
                    InputTextureDictionary[SButton.D1] = InputTextureDictionary[SButton.NumPad1];
                    InputTextureDictionary[SButton.D2] = InputTextureDictionary[SButton.NumPad2];
                    InputTextureDictionary[SButton.D3] = InputTextureDictionary[SButton.NumPad3];
                    InputTextureDictionary[SButton.D4] = InputTextureDictionary[SButton.NumPad4];
                    InputTextureDictionary[SButton.D5] = InputTextureDictionary[SButton.NumPad5];
                    InputTextureDictionary[SButton.D6] = InputTextureDictionary[SButton.NumPad6];
                    InputTextureDictionary[SButton.D7] = InputTextureDictionary[SButton.NumPad7];
                    InputTextureDictionary[SButton.D8] = InputTextureDictionary[SButton.NumPad8];
                    InputTextureDictionary[SButton.D9] = InputTextureDictionary[SButton.NumPad9];
                }
            }
        }

        public override void snapToDefaultClickableComponent()
        {
        }

        private void playOpeningSound()
        {
            Game1.playSound("breathin");
        }

        public override void setUpForGamePadMode()
        {
        }

        public void closeDialogue()
        {
            if (Game1.activeClickableMenu.Equals(this))
            {
                Game1.exitActiveMenu();
                Game1.dialogueUp = false;

                if (Game1.messagePause)
                {
                    Game1.pauseTime = 500f;
                }

                if (Game1.currentObjectDialogue.Count > 0)
                {
                    Game1.currentObjectDialogue.Dequeue();
                }

                Game1.currentDialogueCharacterIndex = 0;
                if (Game1.currentObjectDialogue.Count > 0)
                {
                    Game1.dialogueUp = true;
                    Game1.questionChoices.Clear();
                    Game1.dialogueTyping = true;
                }

                Game1.currentSpeaker = null;
                if (!Game1.eventUp)
                {
                    if (!Game1.isWarping)
                    {
                        Game1.player.CanMove = true;
                    }

                    Game1.player.movementDirections.Clear();
                }
                else if (Game1.currentLocation.currentEvent.CurrentCommand > 0 || Game1.currentLocation.currentEvent.specialEventVariable1)
                {
                    if (!Game1.isFestival() || !Game1.currentLocation.currentEvent.canMoveAfterDialogue())
                    {
                        Game1.currentLocation.currentEvent.CurrentCommand++;
                    }
                    else
                    {
                        Game1.player.CanMove = true;
                    }
                }

                Game1.questionChoices.Clear();
            }

            if (Game1.afterDialogues != null)
            {
                Game1.afterFadeFunction afterDialogues = Game1.afterDialogues;
                Game1.afterDialogues = null;
                afterDialogues();
            }
        }

        public void beginOutro()
        {
            transitioning = true;
            transitioningBigger = false;
            Game1.playSound("breathout");
        }
        private void tryOutro()
        {
            if (Game1.activeClickableMenu != null && Game1.activeClickableMenu.Equals(this))
            {
                beginOutro();
            }
        }
        public void ResetInventoryUIs()
        {
            this.PlayerInventory = null;
            this.PetPocketSlot = null;
            this.PetAccessorySlot = null;
            this.PetBackpack = null;
            this.PetHatSlot = null;
            this.StatusWindow = null;

            this.okButtonBBox = new Rectangle(0, 0, 72, 72);
            this.hoveringOkayButton = false;
            this.okButtonTextureScale = 1f;
        }
        public void CloseInventoryUI()
        {
            Game1.playSound("bigDeSelect");
            this.UpdateInventoryUI();
            this.RescueHeldItemOnExit();
            this.ResetInventoryUIs();

            this.CurrentInterface = eCurrentInterface.None;
        }
        public bool IsMouseInsideClickableInventoryUIArea(int mouseX, int mouseY)
        {
            if (this.CurrentInterface != eCurrentInterface.None)
            {
                if (SunkenLaceUtilities.Intercepts(new Vector2(mouseX, mouseY), PlayerInventorySpriteRec.X - 28, PlayerInventorySpriteRec.Y - 32, PlayerInventorySpriteRec.Right + 28, PlayerInventorySpriteRec.Bottom + 20))
                {
                    return true;
                }
                if (this.okButtonBBox.Contains(mouseX, mouseY))
                {
                    return true;
                }
                if (this.CurrentInterface == eCurrentInterface.Hat && HatFrameSpriteRec.Contains(mouseX, mouseY))
                {
                    return true;
                }
                else if (this.CurrentInterface == eCurrentInterface.Accessory && AccFrameSpriteRec.Contains(mouseX, mouseY))
                {
                    return true;
                }
                else if (this.CurrentInterface == eCurrentInterface.InventoryOne && PInventorySFrameSpriteRec.Contains(mouseX,mouseY))
                {
                    return true;
                }
                else if (this.CurrentInterface == eCurrentInterface.InventoryMultiple && PInventoryMFrameSpriteRec.Contains(mouseX, mouseY))
                {
                    return true;
                }
            }

            return false;
        }
        public void OpenGrabItemUISimple(eCurrentInterface _selType, bool sound = true)
        {
            this.CurrentInterface = _selType;
            if (_selType != eCurrentInterface.StatusTab)
            {
                SetUpPlayerInventory();
            }
            if (sound) { Game1.playSound("bigSelect"); }
            switch (this.CurrentInterface)
            {
                case eCurrentInterface.InventoryMultiple:
                    {
                        if (this.NewItemObtained)
                        {
                            PetHelper.TurnOffNewItemHasBeenObtained(this.SelectedPet.petId.Value);
                        }
                        this.PetBackpack = new PetStorageMenu(x + 10, y - (heightForQuestions - height) - 246, this.temporaryBackpack, 2, 3, 4, 4, new(60, 60), 0.825f);

                        this.PetPocketSlot = new PetStorageMenu(x + 154, y - (heightForQuestions - height) - 110, this.temporaryPocketItem, 1, 1, 0, 0, new(64, 64), 0.825f);
                        this.okButtonBBox.Location = new Point(this.PlayerInventory.xPositionOnScreen - 28 - this.okButtonBBox.Width - 16, this.PlayerInventory.yPositionOnScreen - 16);
                        if (!MouseMoved)
                        {
                            this.PetPocketSlot.HoveredSlotIndex = 0;
                            doHandIconTap = false;
                        }
                    }
                    break;
                case eCurrentInterface.Hat:
                    {
                        this.PetHatSlot = new PetStorageMenu(x - 98, y - (heightForQuestions - height) + 22, this.temporaryHatSlot, 1, 1, 0, 0, new(64, 64), 0.9f, _itemOffset: new Vector2(0, 2), _onlyAddThisTypeofItem: typeof(Hat));
                        this.okButtonBBox.Location = new Point(this.PlayerInventory.xPositionOnScreen - 28 - this.okButtonBBox.Width - 16, this.PlayerInventory.yPositionOnScreen + (this.PlayerInventory.inventoryHeight / 2) - (this.okButtonBBox.Height / 2));
                        this.PetHatSlot.ClickableItemList[0].itemScale = this.HatIconScale;
                        if (!MouseMoved)
                        {
                            this.PetHatSlot.HoveredSlotIndex = 0;
                            doHandIconTap = false;
                        }
                    }
                    break;
                case eCurrentInterface.Accessory:
                    {
                        this.PetAccessorySlot = new PetStorageMenu(x - 97, y - (heightForQuestions - height) + 24 + 62, this.temporaryAccessorySlot, 1, 1, 0, 0, new(64, 64), 0.825f, _onlyAddTheseIDs: new string[] { SmartPet.LuxuryCollarQUID, SmartPet.LightweightCollarQUID, SmartPet.RoughCollarQUID, SmartPet.FloweryCollarQUID, SmartPet.SeagoingCollarQUID});
                        this.okButtonBBox.Location = new Point(this.PlayerInventory.xPositionOnScreen - 28 - this.okButtonBBox.Width - 16, this.PlayerInventory.yPositionOnScreen + (this.PlayerInventory.inventoryHeight / 2) - (this.okButtonBBox.Height / 2));
                        this.PetAccessorySlot.ClickableItemList[0].itemScale = this.AccessoryIconScale;
                        if (!MouseMoved)
                        {
                            this.PetAccessorySlot.HoveredSlotIndex = 0;
                            doHandIconTap = false;
                        }
                    }
                    break;
                case eCurrentInterface.InventoryOne:
                    {
                        if (this.NewItemObtained)
                        {
                            PetHelper.TurnOffNewItemHasBeenObtained(this.SelectedPet.petId.Value);
                        }
                        this.PetPocketSlot = new PetStorageMenu(x + 154, y - (heightForQuestions - height) - 110, this.temporaryPocketItem, 1, 1, 0, 0, new(64, 64), 0.825f);
                        this.okButtonBBox.Location = new Point(this.PlayerInventory.xPositionOnScreen - 28 - this.okButtonBBox.Width - 16, this.PlayerInventory.yPositionOnScreen - 16);
                        if (!MouseMoved)
                        {
                            this.PetPocketSlot.HoveredSlotIndex = 0;
                            doHandIconTap = false;
                        }
                    }
                    break;
                case eCurrentInterface.StatusTab:
                    {
                        if (NewSkillLearned)
                        {
                            PetHelper.TurnOffPetLearnedANewSkillNotification(this.SelectedPet.petId.Value);
                        }
                        this.StatusWindow = new PetStatusWindow(this.SelectedPet, LocalizedContentManager.CurrentLanguageCode, this.UISprites_Texture, this.SelectedPet.Sprite.Texture, this.MouseMoved, this.temporaryAccessorySlot[0]?.QualifiedItemId, this.PetInventory);
                    }
                    break;

            }
            HatIconScale = 0.9f;
            InventoryItemScale = 0.825f;
            AccessoryIconScale = 0.825f;
            PetIconScale = 0.825f;
        }
        private bool RightClickForInventoryUI(int mouseX, int mouseY, int _buttonPressedForTotalOfTicks, bool isLeftShiftPressed)
        {
            bool isSinglePress = _buttonPressedForTotalOfTicks == 1;
            bool UIActive = false;
            if (CurrentInterface != eCurrentInterface.None)
            {
                if (isSinglePress || ((_buttonPressedForTotalOfTicks % 4) == 1 && _buttonPressedForTotalOfTicks >= 40))
                {
                    this.HeldItem = this.PlayerInventory?.RightClick(isLeftShiftPressed,this.HeldItem);
                    switch (CurrentInterface)
                    {
                        case eCurrentInterface.InventoryMultiple:
                            {
                                PetInventoryMutex.RequestLock(delegate
                                {
                                    try
                                    {
                                        SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryBackpack, SynchronizationManager.PetsEnhancedModInventoryBackpackStartIndex);
                                        this.PetBackpack.actualInventory = this.temporaryBackpack;
                                        SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryPocketItem, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
                                        this.PetPocketSlot.actualInventory = this.temporaryPocketItem;

                                        this.HeldItem = this.PetBackpack.RightClick(isLeftShiftPressed, this.HeldItem);
                                        this.HeldItem = this.PetPocketSlot.RightClick(isLeftShiftPressed, this.HeldItem);
                                        SynchronizationManager.TryAddItemListToInventory(this.PetInventory, (List<Item>)this.PetBackpack.actualInventory, SynchronizationManager.PetsEnhancedModInventoryBackpackStartIndex);
                                        SynchronizationManager.TryAddItemListToInventory(this.PetInventory, (List<Item>)this.PetPocketSlot.actualInventory, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
                                    }
                                    finally { PetInventoryMutex.ReleaseLock(); }
                                });
                            }
                            break;
                        case eCurrentInterface.Hat:
                            {
                                PetInventoryMutex.RequestLock(delegate
                                {
                                    try
                                    {
                                        SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryHatSlot, SynchronizationManager.PetsEnhancedModInventoryHatSlotIndex);
                                        this.PetHatSlot.actualInventory = this.temporaryHatSlot;

                                        this.HeldItem = this.PetHatSlot.RightClick(isLeftShiftPressed, this.HeldItem);
                                        SynchronizationManager.TryAddItemListToInventory(this.PetInventory, (List<Item>)this.PetHatSlot.actualInventory, SynchronizationManager.PetsEnhancedModInventoryHatSlotIndex);
                                    }
                                    finally { PetInventoryMutex.ReleaseLock(); }
                                });
                            }
                            break;
                        case eCurrentInterface.Accessory:
                            {
                                PetInventoryMutex.RequestLock(delegate
                                {
                                    try
                                    {
                                        SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryAccessorySlot, SynchronizationManager.PetsEnhancedModInventoryAccessorySlotIndex);
                                        this.PetAccessorySlot.actualInventory = this.temporaryAccessorySlot;

                                        this.HeldItem = this.PetAccessorySlot.RightClick(isLeftShiftPressed, this.HeldItem);
                                        SynchronizationManager.TryAddItemListToInventory(this.PetInventory, (List<Item>)this.PetAccessorySlot.actualInventory, SynchronizationManager.PetsEnhancedModInventoryAccessorySlotIndex);
                                    }
                                    finally { PetInventoryMutex.ReleaseLock(); }
                                });
                            }
                            break;
                        case eCurrentInterface.InventoryOne:
                            {
                                PetInventoryMutex.RequestLock(delegate
                                {
                                    try
                                    {
                                        SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryPocketItem, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
                                        this.PetPocketSlot.actualInventory = this.temporaryPocketItem;

                                        this.HeldItem = this.PetPocketSlot.RightClick(isLeftShiftPressed, this.HeldItem);
                                        SynchronizationManager.TryAddItemListToInventory(this.PetInventory, (List<Item>)this.PetPocketSlot.actualInventory, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
                                    }
                                    finally { PetInventoryMutex.ReleaseLock(); }
                                });
                            }
                            break;
                    }
                }
                UIActive = true;
            }
            else if (isSinglePress && (CurrentControlledObject == 1 || CurrentControlledObject == 3 || CurrentControlledObject == 4 || CurrentControlledObject == 5))
            {
                OpenGrabItemUISimple(CurrentControlledObject == 1 ? eCurrentInterface.StatusTab : CurrentControlledObject == 4 ? eCurrentInterface.Hat : CurrentControlledObject == 5 ? eCurrentInterface.Accessory : this.BackpackUnlocked ? eCurrentInterface.InventoryMultiple : eCurrentInterface.InventoryOne);
                UIActive = true;
            }
            return UIActive;
        }
        public override void receiveRightClick(int posX, int posY, bool playSound = true) { }
        public override void receiveLeftClick(int posX, int posY, bool playSound = true) { }
        public override void receiveKeyPress(Keys key) { }
        private bool LeftClickForInventoryUI(int mouseX, int mouseY, bool isLeftShiftPressed)
        {
            bool UIActive = false;
            if (CurrentInterface != eCurrentInterface.None)
            {
                bool playerInventoryHoveringItem = this.PlayerInventory?.HoveredSlotIndex != -1 && this.PlayerInventory.actualInventory.Count > this.PlayerInventory.HoveredSlotIndex && this.PlayerInventory.actualInventory[this.PlayerInventory.HoveredSlotIndex] is not null;
                if (!isLeftShiftPressed)
                {
                    this.HeldItem = this.PlayerInventory?.LeftClick(this.HeldItem);
                }
                switch (CurrentInterface)
                {
                    case eCurrentInterface.InventoryMultiple:
                        {
                            PetInventoryMutex.RequestLock(delegate
                            {
                                try
                                {
                                    SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryBackpack, SynchronizationManager.PetsEnhancedModInventoryBackpackStartIndex);
                                    this.PetBackpack.actualInventory = this.temporaryBackpack;
                                    SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryPocketItem, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
                                    this.PetPocketSlot.actualInventory = this.temporaryPocketItem;

                                    if (isLeftShiftPressed)
                                    {
                                        if (playerInventoryHoveringItem)
                                        {
                                            this.PlayerInventory.actualInventory[this.PlayerInventory.HoveredSlotIndex] = this.PetBackpack.TryAddItemSomewhereInsideTheInventory(this.PlayerInventory.actualInventory[this.PlayerInventory.HoveredSlotIndex]);
                                        }
                                        else if (this.PetBackpack?.HoveredSlotIndex != -1 && this.PetBackpack.actualInventory[this.PetBackpack.HoveredSlotIndex] is not null && this.PlayerInventory is not null)
                                        {
                                            int _maxPlayerItems = Game1.player?.maxItems is not null ? Game1.player.maxItems.Value : -1;
                                            this.PetBackpack.actualInventory[this.PetBackpack.HoveredSlotIndex] = this.PlayerInventory.TryAddItemSomewhereInsideTheInventory(this.PetBackpack.actualInventory[this.PetBackpack.HoveredSlotIndex], _maxPlayerItems);
                                        }
                                        else if (this.PetPocketSlot?.HoveredSlotIndex != -1 && !this.PetPocketSlot.DontMoveItems && this.PetPocketSlot.actualInventory[this.PetPocketSlot.HoveredSlotIndex] is not null && this.PlayerInventory is not null)
                                        {
                                            int _maxPlayerItems = Game1.player?.maxItems is not null ? Game1.player.maxItems.Value : -1;
                                            this.PetPocketSlot.actualInventory[this.PetPocketSlot.HoveredSlotIndex] = this.PlayerInventory.TryAddItemSomewhereInsideTheInventory(this.PetPocketSlot.actualInventory[this.PetPocketSlot.HoveredSlotIndex], _maxPlayerItems);
                                        }
                                    }
                                    else
                                    {
                                        this.HeldItem = this.PetBackpack.LeftClick(this.HeldItem);
                                        bool flag = this.HeldItem is null && this.PetPocketSlot.HoveredSlotIndex > -1 && this.PetPocketSlot.actualInventory[this.PetPocketSlot.HoveredSlotIndex].QualifiedItemId.Equals(SmartPet.PetBackpackQUID);
                                        this.HeldItem = this.PetPocketSlot.LeftClick(this.HeldItem, customPlaceItemSound: flag ? "SunkenLace.PetsEnhancedMod.Sounds.BackpackEquiped_inventory" : null, playSoundInLocation: flag, isPocketSlotItem: true);
                                    }
                                    SynchronizationManager.TryAddItemListToInventory(this.PetInventory, (List<Item>)this.PetBackpack.actualInventory, SynchronizationManager.PetsEnhancedModInventoryBackpackStartIndex);
                                    SynchronizationManager.TryAddItemListToInventory(this.PetInventory, (List<Item>)this.PetPocketSlot.actualInventory, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
                                }
                                finally { PetInventoryMutex.ReleaseLock(); }
                            });
                        }
                        break;
                    case eCurrentInterface.Hat:
                        {
                            PetInventoryMutex.RequestLock(delegate
                            {
                                try
                                {
                                    SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryHatSlot, SynchronizationManager.PetsEnhancedModInventoryHatSlotIndex);
                                    this.PetHatSlot.actualInventory = this.temporaryHatSlot;

                                    if (isLeftShiftPressed)
                                    {
                                        if (playerInventoryHoveringItem)
                                        {
                                            this.PlayerInventory.actualInventory[this.PlayerInventory.HoveredSlotIndex] = this.PetHatSlot.TryAddItemSomewhereInsideTheInventory(this.PlayerInventory.actualInventory[this.PlayerInventory.HoveredSlotIndex]);
                                        }
                                        else if (this.PetHatSlot?.HoveredSlotIndex != -1 && this.PetHatSlot.actualInventory[this.PetHatSlot.HoveredSlotIndex] is not null && this.PlayerInventory is not null)
                                        {
                                            int _maxPlayerItems = Game1.player?.maxItems is not null ? Game1.player.maxItems.Value : -1;
                                            this.PetHatSlot.actualInventory[this.PetHatSlot.HoveredSlotIndex] = this.PlayerInventory.TryAddItemSomewhereInsideTheInventory(this.PetHatSlot.actualInventory[this.PetHatSlot.HoveredSlotIndex], _maxPlayerItems);
                                        }
                                    }
                                    else
                                    {
                                        this.HeldItem = this.PetHatSlot.LeftClick(this.HeldItem);
                                    }
                                    SynchronizationManager.TryAddItemListToInventory(this.PetInventory, (List<Item>)this.PetHatSlot.actualInventory, SynchronizationManager.PetsEnhancedModInventoryHatSlotIndex);
                                }
                                finally { PetInventoryMutex.ReleaseLock(); }
                            });
                        }
                        break;
                    case eCurrentInterface.Accessory:
                        {
                            PetInventoryMutex.RequestLock(delegate
                            {
                                try
                                {
                                    SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryAccessorySlot, SynchronizationManager.PetsEnhancedModInventoryAccessorySlotIndex);
                                    this.PetAccessorySlot.actualInventory = this.temporaryAccessorySlot;

                                    if (isLeftShiftPressed)
                                    {
                                        if (playerInventoryHoveringItem)
                                        {
                                            this.PlayerInventory.actualInventory[this.PlayerInventory.HoveredSlotIndex] = this.PetAccessorySlot.TryAddItemSomewhereInsideTheInventory(this.PlayerInventory.actualInventory[this.PlayerInventory.HoveredSlotIndex], playCustomSound: "crit", playsoundAtLocation: true);
                                        }
                                        else if (this.PetAccessorySlot?.HoveredSlotIndex != -1 && this.PetAccessorySlot.actualInventory[this.PetAccessorySlot.HoveredSlotIndex] is not null && this.PlayerInventory is not null)
                                        {
                                            int _maxPlayerItems = Game1.player?.maxItems is not null ? Game1.player.maxItems.Value : -1;
                                            this.PetAccessorySlot.actualInventory[this.PetAccessorySlot.HoveredSlotIndex] = this.PlayerInventory.TryAddItemSomewhereInsideTheInventory(this.PetAccessorySlot.actualInventory[this.PetAccessorySlot.HoveredSlotIndex], _maxPlayerItems);
                                        }
                                    }
                                    else
                                    {
                                        this.HeldItem = this.PetAccessorySlot.LeftClick(this.HeldItem, customPlaceItemSound: "crit", playSoundInLocation: true);
                                    }
                                    SynchronizationManager.TryAddItemListToInventory(this.PetInventory, (List<Item>)this.PetAccessorySlot.actualInventory, SynchronizationManager.PetsEnhancedModInventoryAccessorySlotIndex);
                                }
                                finally { PetInventoryMutex.ReleaseLock(); }
                            });
                        }
                        break;
                    case eCurrentInterface.InventoryOne:
                        {
                            PetInventoryMutex.RequestLock(delegate
                            {
                                try
                                {
                                    SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryPocketItem, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
                                    this.PetPocketSlot.actualInventory = this.temporaryPocketItem;

                                    bool heldItemIsBackpack = this.HeldItem is not null && this.HeldItem.QualifiedItemId.Equals(SmartPet.PetBackpackQUID);
                                    if (isLeftShiftPressed)
                                    {
                                        if (playerInventoryHoveringItem)
                                        {
                                            bool hoveringBackpack = this.PlayerInventory.actualInventory[this.PlayerInventory.HoveredSlotIndex].QualifiedItemId.Equals(SmartPet.PetBackpackQUID);
                                            this.PlayerInventory.actualInventory[this.PlayerInventory.HoveredSlotIndex] = this.PetPocketSlot.TryAddItemSomewhereInsideTheInventory(this.PlayerInventory.actualInventory[this.PlayerInventory.HoveredSlotIndex], playCustomSound: hoveringBackpack ? "SunkenLace.PetsEnhancedMod.Sounds.BackpackEquiped_inventory" : null, playsoundAtLocation: hoveringBackpack, onlyTakeOne: hoveringBackpack);
                                        }
                                        else if (this.PetPocketSlot?.HoveredSlotIndex != -1 && this.PetPocketSlot.actualInventory[this.PetPocketSlot.HoveredSlotIndex] is not null && this.PlayerInventory is not null)
                                        {
                                            int _maxPlayerItems = Game1.player?.maxItems is not null ? Game1.player.maxItems.Value : -1;
                                            this.PetPocketSlot.actualInventory[this.PetPocketSlot.HoveredSlotIndex] = this.PlayerInventory.TryAddItemSomewhereInsideTheInventory(this.PetPocketSlot.actualInventory[this.PetPocketSlot.HoveredSlotIndex], _maxPlayerItems);
                                        }
                                    }
                                    else
                                    {
                                        this.HeldItem = this.PetPocketSlot.LeftClick(this.HeldItem, customPlaceItemSound: heldItemIsBackpack ? "SunkenLace.PetsEnhancedMod.Sounds.BackpackEquiped_inventory" : null, playSoundInLocation: heldItemIsBackpack, isPocketSlotItem: true);
                                    }
                                    SynchronizationManager.TryAddItemListToInventory(this.PetInventory, (List<Item>)this.PetPocketSlot.actualInventory, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
                                }
                                finally { PetInventoryMutex.ReleaseLock(); }
                            });
                        }
                        break;
                }
                if (MouseMoved && !IsMouseInsideClickableInventoryUIArea(mouseX, mouseY))
                {
                    DropHeldItem();
                }
                UIActive = true;
            }
            else if (CurrentControlledObject == 1 || CurrentControlledObject == 3 || CurrentControlledObject == 4 || CurrentControlledObject == 5)
            {
                OpenGrabItemUISimple(CurrentControlledObject == 1 ? eCurrentInterface.StatusTab : CurrentControlledObject == 4 ? eCurrentInterface.Hat : CurrentControlledObject == 5 ? eCurrentInterface.Accessory : this.BackpackUnlocked ? eCurrentInterface.InventoryMultiple : eCurrentInterface.InventoryOne);
                UIActive = true;
            }
            return UIActive;
        }
        private void PressedActionButton(int _buttonPressedForTotalOfTicks)
        {
            bool isSinglePress = _buttonPressedForTotalOfTicks == 1;
            if (this.CurrentInterface == eCurrentInterface.StatusTab)
            {
                this.StatusWindow?.ActionClick(this.BackgroundStyleIndex, this.SelectedPet.petId.Value, _buttonPressedForTotalOfTicks);
                if (isSinglePress && this.StatusWindow?.currentControlledObject == 10)
                {
                    CloseInventoryUI();
                }
                return;
            }
            bool leftShiftDown = buttonPressedEventArgs is not null && buttonPressedEventArgs.IsDown(LShiftSubstituteForGamepad) ? true : Game1.isOneOfTheseKeysDown(Game1.oldKBState, new InputButton[1] { new(Keys.LeftShift) });
            bool inventoryUIActive = RightClickForInventoryUI(CurrentMousePosition.X, CurrentMousePosition.Y, _buttonPressedForTotalOfTicks, leftShiftDown);
            if (this.CurrentInterface != eCurrentInterface.StatusTab && this.CurrentInterface != eCurrentInterface.None && _buttonPressedForTotalOfTicks >= 2 && this.HeldItem is null && hoveringOkayButton)
            {
                this.CloseInventoryUI();
                inventoryUIActive = false;
            }
            if (inventoryUIActive) { return; }
            if (safetyTimer > 0)
            {
                return;
            }
            if (!isSinglePress) { return; }
            if (CurrentControlledObject >= 6)
            {
                if (DoConfirmation == 0) { DoConfirmation = 1; }
                else if (DoConfirmation == -1 && this.responses[CurrentControlledObject - 6].responseKey.Equals("GoHomeCommand"))
                {
                    Game1.playSound("stoneStep");
                    DoConfirmation = 0;
                    setUpQuestions(I18n.PressAgainToConfirmText());
                }
                if (DoConfirmation == -1 || DoConfirmation == 1)
                {
                    Game1.playSound("smallSelect");
                    this.actionAfterQuestion?.Invoke(this.responses[CurrentControlledObject - 6].responseKey, Game1.player, this.SelectedPet.petId.Value);
                    tryOutro();
                }
            }
        }
        private void PressedUseToolButton(int _buttonPressedForTotalOfTicks)
        {
            bool isSinglePress = _buttonPressedForTotalOfTicks == 1;
            if (this.CurrentInterface == eCurrentInterface.StatusTab)
            {
                this.StatusWindow?.ActionClick(this.BackgroundStyleIndex, this.SelectedPet.petId.Value, _buttonPressedForTotalOfTicks);
                if (isSinglePress && this.StatusWindow?.currentControlledObject == 10)
                {
                    CloseInventoryUI();
                }
                return;
            }
            bool leftShiftDown = buttonPressedEventArgs is not null && buttonPressedEventArgs.IsDown(LShiftSubstituteForGamepad) ? true : Game1.isOneOfTheseKeysDown(Game1.oldKBState, new InputButton[1] { new(Keys.LeftShift) });
            bool inventoryUIActive = isSinglePress && LeftClickForInventoryUI(CurrentMousePosition.X, CurrentMousePosition.Y, leftShiftDown);
            if (this.CurrentInterface != eCurrentInterface.StatusTab && this.CurrentInterface != eCurrentInterface.None && _buttonPressedForTotalOfTicks >= 2 && this.HeldItem is null && hoveringOkayButton)
            {
                this.CloseInventoryUI();
                inventoryUIActive = false;
            }
            if (inventoryUIActive) { return; }
            if (safetyTimer > 0)
            {
                return;
            }
            if (!isSinglePress) { return; }
            if (CurrentControlledObject >= 6)
            {
                if (DoConfirmation == 0) { DoConfirmation = 1; }
                else if (DoConfirmation == -1 && this.responses[CurrentControlledObject - 6].responseKey.Equals("GoHomeCommand"))
                {
                    Game1.playSound("stoneStep");
                    DoConfirmation = 0;
                    setUpQuestions(I18n.PressAgainToConfirmText());
                }
                if (DoConfirmation == -1 || DoConfirmation == 1)
                {
                    Game1.playSound("smallSelect");
                    this.actionAfterQuestion?.Invoke(this.responses[CurrentControlledObject - 6].responseKey, Game1.player, this.SelectedPet.petId.Value);
                    tryOutro();
                }
            }
        }
        bool buttonBeenClickedOnce = false;
        public void ReceiveButtonPress(ButtonPressedEventArgs e, int _buttonPressedForTotalOfTicks)
        {
            bool pressedShoulders = e.Button == SButton.RightShoulder || e.Button == SButton.LeftShoulder;
            if (_buttonPressedForTotalOfTicks <= 0) { return; }
            if (_buttonPressedForTotalOfTicks == 1) { buttonBeenClickedOnce = false; }
            if (_buttonPressedForTotalOfTicks == 1 && (e.Button == SButton.Escape || e.Button == CloseUIButtonSubstituteForGamepad))
            {
                if (CurrentInterface == eCurrentInterface.None)
                {
                    Game1.playSound("smallSelect");
                    tryOutro();
                }
                else if (this.HeldItem is null)
                {
                    this.CloseInventoryUI();
                }
            }
            else if (e.Button == GrabItemButtonSubstituteForGamepad)
            {
                PressedUseToolButton(_buttonPressedForTotalOfTicks);
                doHandIconTap = true;
            }
            else if (e.Button == StackItemButtonSubstituteForGamepad)
            {
                PressedActionButton(_buttonPressedForTotalOfTicks);
                doHandIconTap = true;
            }
            else if (PetHelper.IsActionButton(e.Button))
            {
                PressedActionButton(_buttonPressedForTotalOfTicks);
                doHandIconTap = true;
            }
            else if (PetHelper.IsUseToolButton(e.Button))
            {
                PressedUseToolButton(_buttonPressedForTotalOfTicks);
                doHandIconTap = true;
            }
            else if (MouseInMotion == 0 && PressedArrowKeys(e, _buttonPressedForTotalOfTicks))
            {
                MouseMoved = false;
                if (DoConfirmation > -1)
                {
                    setUpQuestions();
                    DoConfirmation = -1;
                }
            }
            else if (MouseInMotion == 0 && CurrentInterface == eCurrentInterface.None && !buttonBeenClickedOnce && PressedHotkeys(e, _buttonPressedForTotalOfTicks))
            {
                buttonBeenClickedOnce = true;
                MouseMoved = false;
                if (DoConfirmation > -1)
                {
                    setUpQuestions();
                    DoConfirmation = -1;
                }
            }
            else if (CurrentInterface == eCurrentInterface.StatusTab && (_buttonPressedForTotalOfTicks % 20) == 1 && pressedShoulders && UseL1andR1backgroundChangeShortcuts)
            {
                if ((this.StatusWindow?.currentControlledObject == 1 || this.StatusWindow?.currentControlledObject == 2)) { this.StatusWindow.currentControlledObject = 3; }
                else if ((this.StatusWindow?.currentControlledObject == 4 || this.StatusWindow?.currentControlledObject == 5)) { this.StatusWindow.currentControlledObject = 6; }

                if (this.StatusWindow?.currentControlledObject == 3 && e.Button == SButton.LeftShoulder) { this.StatusWindow.LeftBGChangeArrow.ResetScale(); PetHelper.ChangeBackgroundIndex(this.SelectedPet.petId.Value, (this.BackgroundStyleIndex + 15) % 16); Game1.playSound("pickUpItem"); }
                else if (this.StatusWindow?.currentControlledObject == 3 && e.Button == SButton.RightShoulder) { this.StatusWindow.RightBGChangeArrow.ResetScale(); PetHelper.ChangeBackgroundIndex(this.SelectedPet.petId.Value, (this.BackgroundStyleIndex + 1) % 16); Game1.playSound("pickUpItem"); }
                else if (this.StatusWindow?.canChangeDirection == true && this.StatusWindow.currentControlledObject == 6)
                {
                    if (e.Button == SButton.LeftShoulder) { this.StatusWindow.LeftRotationArrow.ResetScale(); }
                    else
                    {
                        this.StatusWindow.RightRotationArrow.ResetScale();
                    }
                    this.StatusWindow.petFacingDirection = e.Button == SButton.RightShoulder ? (this.StatusWindow.petFacingDirection + 1) % 4 : (this.StatusWindow.petFacingDirection + 3) % 4;
                    this.StatusWindow.resetTimer3and4();
                    this.StatusWindow.flipSprite = false;
                    Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.Rotation_arrowClick0" + Game1.random.Next(3));
                }
            }
        }
        private bool PressedHotkeys(ButtonPressedEventArgs e, int _buttonPressedForTotalOfTicks)
        {
            int _cCO = CurrentControlledObject;
            bool flag = Game1.options.gamepadControls;
            if ((flag && e.Button == OpenStatusWindowHotkeySubstituteForGamepad) || (!flag && e.Button == StatusTabHotkey))
            {
                if (_cCO != 1) { CurrentControlledObject = 1; }
                else { PressedUseToolButton(_buttonPressedForTotalOfTicks); }
            }
            else if ((flag && e.Button == OpenInventoryHotkeySubstituteForGamepad) || (!flag && e.Button == InventoryTabHotkey))
            {
                if (_cCO != 3) { CurrentControlledObject = 3; }
                else { PressedUseToolButton(_buttonPressedForTotalOfTicks); }
            }
            else if ((flag && e.Button == OpenHatSlotHotkeySubstituteForGamepad) || (!flag && e.Button == HatTabHotkey))
            {
                if (_cCO != 4) { CurrentControlledObject = 4; }
                else { PressedUseToolButton(_buttonPressedForTotalOfTicks); }
            }
            else if ((flag && e.Button == OpenAccessorySlotHotkeySubstituteForGamepad) || (!flag && e.Button == AccessoryTabHotkey))
            {
                if (_cCO != 5) { CurrentControlledObject = 5; }
                else { PressedUseToolButton(_buttonPressedForTotalOfTicks); }
            }
            else { return false; }
            if (CurrentControlledObject == 3 && _cCO != 3 && this.BackpackUnlocked)
            {
                Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.OpeningBackpack_sound");
            }
            if (CurrentControlledObject != 3 && _cCO == 3 && this.BackpackUnlocked && this.hoveringInventoryChestTimer >= 225)
            {
                Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.ClosingBackpack_sound");
            }
            if (CurrentControlledObject > 0 && _cCO != this.CurrentControlledObject && CurrentControlledObject < 6 && (CurrentControlledObject != 3 || (CurrentControlledObject == 3 && !this.BackpackUnlocked)))
            {
                Game1.playSound("shiny4");
            }
            return true;
        }

        public static bool IsDownKey(SButton _button)
        {
            _button.TryGetKeyboard(out var _AsKey);
            return (Game1.options.doesInputListContain(Game1.options.moveDownButton, _AsKey) || _button == SButton.Down || _button == SButton.DPadDown || _button == SButton.LeftThumbstickDown);
        }
        public static bool IsUpKey(SButton _button)
        {
            _button.TryGetKeyboard(out var _AsKey);
            return (Game1.options.doesInputListContain(Game1.options.moveUpButton, _AsKey) || _button == SButton.Up || _button == SButton.DPadUp || _button == SButton.LeftThumbstickUp);
        }
        private bool PressedArrowKeys(ButtonPressedEventArgs e, int _buttonPressedForTotalOfTicks)
        {
            int _cCO = CurrentControlledObject;
            bool statusTab = CurrentInterface == eCurrentInterface.StatusTab;
            bool baseTab = CurrentInterface == eCurrentInterface.None;
            int buttonPressedTotalTicksModulus = _buttonPressedForTotalOfTicks % 10;
            e.Button.TryGetKeyboard(out var _AsKey);
            if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, _AsKey) || e.Button == SButton.Left || e.Button == SButton.DPadLeft || e.Button == SButton.LeftThumbstickLeft)
            {
                if (statusTab) { this.StatusWindow?.PressedLeft(_buttonPressedForTotalOfTicks); }
                else if (baseTab && buttonPressedTotalTicksModulus == 1)
                {
                    if (_cCO == 2 || _cCO == 3) { CurrentControlledObject--; }
                    else if (_cCO == 0) { CurrentControlledObject = 4; }
                    else if (_cCO > 5) { CurrentControlledObject = 5; }
                }
                else if (!baseTab && buttonPressedTotalTicksModulus == 1) { moveLeftInsideInventoryTab(); }
            }
            else if (IsUpKey(e.Button))
            {
                if (statusTab) { this.StatusWindow?.PressedUp(_buttonPressedForTotalOfTicks); }
                else if (baseTab && buttonPressedTotalTicksModulus == 1)
                {
                    if (_cCO == 4) { CurrentControlledObject = 1; }
                    else if (_cCO == 0) { CurrentControlledObject = 1; }
                    else if (_cCO == 5) { CurrentControlledObject = 4; }
                    else if (_cCO == 6) { CurrentControlledObject = 3; }
                    else if (_cCO > 6) { CurrentControlledObject--; }
                }
                else if (!baseTab && buttonPressedTotalTicksModulus == 1) { moveUpInsideInventoryTab(); }
            }
            else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, _AsKey) || e.Button == SButton.Right || e.Button == SButton.DPadRight || e.Button == SButton.LeftThumbstickRight)
            {
                if (statusTab) { this.StatusWindow?.PressedRight(_buttonPressedForTotalOfTicks); }
                else if (baseTab && buttonPressedTotalTicksModulus == 1)
                {
                    if (_cCO == 1 || _cCO == 2) { CurrentControlledObject++; }
                    else if (_cCO == 0) { CurrentControlledObject = 3; }
                    else if ((_cCO == 4 || _cCO == 5) && responses.Length > 0) { CurrentControlledObject = 6; }
                }
                else if (!baseTab && buttonPressedTotalTicksModulus == 1) { moveRightInsideInventoryTab(); }
            }
            else if (IsDownKey(e.Button))
            {
                if (statusTab) { this.StatusWindow?.PressedDown(_buttonPressedForTotalOfTicks); }
                else if (baseTab && buttonPressedTotalTicksModulus == 1)
                {
                    if (_cCO == 2 || _cCO == 3)
                    {
                        if (responses.Length > 0) { CurrentControlledObject = 6; }
                        else { CurrentControlledObject = 4; }
                    }
                    else if (_cCO == 0 && responses.Length > 0) { CurrentControlledObject = 6; }
                    else if (_cCO == 1) { CurrentControlledObject = 4; }
                    else if (_cCO == 4) { CurrentControlledObject = 5; }
                    else if (_cCO > 5 && (responses.Length + 5) > _cCO) { CurrentControlledObject++; }
                }
                else if (!baseTab && buttonPressedTotalTicksModulus == 1) { moveDownInsideInventoryTab(); }
            }
            else { return false; }

            if (CurrentControlledObject != _cCO && CurrentControlledObject > 5)
            {
                Game1.playSound("Cowboy_gunshot");
            }
            if (CurrentControlledObject == 3 && _cCO != 3 && this.BackpackUnlocked)
            {
                Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.OpeningBackpack_sound");
            }
            if (CurrentControlledObject != 3 && _cCO == 3 && this.BackpackUnlocked && this.hoveringInventoryChestTimer >= 225)
            {
                Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.ClosingBackpack_sound");
            }
            if (CurrentControlledObject > 0 && _cCO != this.CurrentControlledObject && CurrentControlledObject < 6 && (CurrentControlledObject != 3 || (CurrentControlledObject == 3 && !this.BackpackUnlocked)))
            {
                Game1.playSound("shiny4");
            }
            return true;
        }
        private void moveLeftInsideInventoryTab()
        {
            if (CurrentInterface == eCurrentInterface.Hat)
            {
                if (hoveringOkayButton) { hoveringOkayButton = false; this.PetHatSlot.HoveredSlotIndex = 0; Game1.playSound("shiny4"); }
                else if (this.PlayerInventory?.HoveredSlotIndex >= 0)
                {
                    if (this.PlayerInventory.MoveToLeftSlot()) { hoveringOkayButton = true; this.PlayerInventory.HoveredSlotIndex = -1; Game1.playSound("shiny4"); }
                }
                else if (this.PetHatSlot?.HoveredSlotIndex < 0) { this.PetHatSlot.HoveredSlotIndex = 0; Game1.playSound("shiny4"); }
            }
            else if (CurrentInterface == eCurrentInterface.Accessory)
            {
                if (hoveringOkayButton) { hoveringOkayButton = false; this.PetAccessorySlot.HoveredSlotIndex = 0; Game1.playSound("shiny4"); }
                else if (this.PlayerInventory?.HoveredSlotIndex >= 0)
                {
                    if (this.PlayerInventory.MoveToLeftSlot()) { hoveringOkayButton = true; this.PlayerInventory.HoveredSlotIndex = -1; Game1.playSound("shiny4"); }
                }
                else if (this.PetAccessorySlot?.HoveredSlotIndex < 0) { this.PetAccessorySlot.HoveredSlotIndex = 0; Game1.playSound("shiny4"); }
            }
            else if (CurrentInterface == eCurrentInterface.InventoryOne)
            {
                if (this.PlayerInventory?.HoveredSlotIndex >= 0)
                {
                    if (this.PlayerInventory.MoveToLeftSlot())
                    {
                        if (this.PlayerInventory.HoveredSlotIndex >= (this.PlayerInventory.capacity - this.PlayerInventory.columns)) { this.PetPocketSlot.HoveredSlotIndex = 0; }
                        else { hoveringOkayButton = true; }
                        this.PlayerInventory.HoveredSlotIndex = -1;
                        Game1.playSound("shiny4");
                    }
                }
                else if (!hoveringOkayButton && this.PetPocketSlot?.HoveredSlotIndex < 0) { this.PetPocketSlot.HoveredSlotIndex = 0; Game1.playSound("shiny4"); }
            }
            else if (CurrentInterface == eCurrentInterface.InventoryMultiple)
            {
                if (hoveringOkayButton) { hoveringOkayButton = false; this.PetBackpack.HoveredSlotIndex = 1; Game1.playSound("shiny4"); }
                else if (this.PlayerInventory?.HoveredSlotIndex >= 0)
                {
                    if (this.PlayerInventory.MoveToLeftSlot())
                    {
                        if (this.PlayerInventory.HoveredSlotIndex >= (this.PlayerInventory.capacity - this.PlayerInventory.columns)) { this.PetPocketSlot.HoveredSlotIndex = 0; }
                        else if (this.PlayerInventory.HoveredSlotIndex >= (this.PlayerInventory.capacity - (this.PlayerInventory.columns * 2))) { this.PetBackpack.HoveredSlotIndex = 3; }
                        else if (this.PlayerInventory?.rows > 3 && this.PlayerInventory.HoveredSlotIndex >= (this.PlayerInventory.capacity - (this.PlayerInventory.columns * 3)))
                        {
                            this.PetBackpack.HoveredSlotIndex = 1;
                        }
                        else { hoveringOkayButton = true; }
                        this.PlayerInventory.HoveredSlotIndex = -1;
                        Game1.playSound("shiny4");
                    }
                }
                else if (this.PetPocketSlot?.HoveredSlotIndex >= 0) { this.PetPocketSlot.HoveredSlotIndex = -1; this.PetBackpack.HoveredSlotIndex = this.PetBackpack.capacity - 1; Game1.playSound("shiny4"); }
                else if (this.PetBackpack?.HoveredSlotIndex >= 0) { this.PetBackpack.MoveToLeftSlot(); }
                else if (!hoveringOkayButton) { this.PetBackpack.HoveredSlotIndex = 0; Game1.playSound("shiny4"); }
            }
        }
        private void moveRightInsideInventoryTab()
        {
            if (CurrentInterface == eCurrentInterface.Hat)
            {
                if (hoveringOkayButton)
                {
                    hoveringOkayButton = false;
                    this.PlayerInventory.HoveredSlotIndex = 0;
                    Game1.playSound("shiny4");
                }
                else if (this.PetHatSlot?.HoveredSlotIndex >= 0)
                {
                    this.PetHatSlot.HoveredSlotIndex = -1; hoveringOkayButton = true; Game1.playSound("shiny4");
                }
                else if (this.PlayerInventory?.HoveredSlotIndex >= 0)
                {
                    this.PlayerInventory.MoveToRightSlot();
                }
                else { this.PlayerInventory.HoveredSlotIndex = 0; Game1.playSound("shiny4"); }
            }
            else if (CurrentInterface == eCurrentInterface.Accessory)
            {
                if (hoveringOkayButton)
                {
                    hoveringOkayButton = false;
                    this.PlayerInventory.HoveredSlotIndex = 0;
                    Game1.playSound("shiny4");
                }
                else if (this.PetAccessorySlot?.HoveredSlotIndex >= 0)
                {
                    this.PetAccessorySlot.HoveredSlotIndex = -1; hoveringOkayButton = true; Game1.playSound("shiny4");
                }
                else if (this.PlayerInventory?.HoveredSlotIndex >= 0)
                {
                    this.PlayerInventory.MoveToRightSlot();
                }
                else { this.PlayerInventory.HoveredSlotIndex = 0; Game1.playSound("shiny4"); }
            }
            else if (CurrentInterface == eCurrentInterface.InventoryOne || CurrentInterface == eCurrentInterface.InventoryMultiple)
            {
                if (hoveringOkayButton)
                {
                    hoveringOkayButton = false;
                    this.PlayerInventory.HoveredSlotIndex = 0;
                    Game1.playSound("shiny4");
                }
                else if (this.PetPocketSlot?.HoveredSlotIndex >= 0)
                {
                    this.PetPocketSlot.HoveredSlotIndex = -1;
                    this.PlayerInventory.HoveredSlotIndex = Math.Clamp(Game1.player.MaxItems - this.PlayerInventory.columns, 0, this.PlayerInventory.capacity - this.PlayerInventory.columns);
                    Game1.playSound("shiny4");
                }
                else if (CurrentInterface == eCurrentInterface.InventoryMultiple && this.PetBackpack?.HoveredSlotIndex >= 0)
                {
                    bool movedOutOfBounds = this.PetBackpack.MoveToRightSlot();
                    if (this.PetBackpack.HoveredSlotIndex == 1 && movedOutOfBounds)
                    {
                        this.PetBackpack.HoveredSlotIndex = -1;
                        if (this.PlayerInventory?.rows > 3)
                        {
                            this.PlayerInventory.HoveredSlotIndex = Math.Clamp(Game1.player.MaxItems - this.PlayerInventory.columns, 0, this.PlayerInventory.capacity - (this.PlayerInventory.columns * 3));
                        }
                        else { hoveringOkayButton = true; }
                        Game1.playSound("shiny4");
                    }
                    else if (this.PetBackpack.HoveredSlotIndex == 3 && movedOutOfBounds)
                    {
                        this.PetBackpack.HoveredSlotIndex = -1;
                        this.PlayerInventory.HoveredSlotIndex = Math.Clamp(Game1.player.MaxItems - this.PlayerInventory.columns, 0, this.PlayerInventory.capacity - (this.PlayerInventory.columns * 2));
                        Game1.playSound("shiny4");
                    }
                    else if (this.PetBackpack.HoveredSlotIndex == 5 && movedOutOfBounds)
                    {
                        this.PetBackpack.HoveredSlotIndex = -1; this.PetPocketSlot.HoveredSlotIndex = 0;
                        Game1.playSound("shiny4");
                    }
                }
                else if (this.PlayerInventory?.HoveredSlotIndex >= 0)
                {
                    this.PlayerInventory.MoveToRightSlot();
                }
                else { this.PlayerInventory.HoveredSlotIndex = 0; Game1.playSound("shiny4"); }
            }
        }
        private void moveUpInsideInventoryTab()
        {
            if (this.PlayerInventory?.HoveredSlotIndex >= 0)
            {
                this.PlayerInventory.MoveToUpSlot();
            }
            else if (CurrentInterface == eCurrentInterface.InventoryMultiple && this.PetBackpack?.HoveredSlotIndex >= 0)
            {
                this.PetBackpack.MoveToUpSlot();
            }
            else
            {
                if ((CurrentInterface == eCurrentInterface.InventoryOne || CurrentInterface == eCurrentInterface.InventoryMultiple) && this.PetPocketSlot?.HoveredSlotIndex >= 0)
                {
                    this.PetPocketSlot.HoveredSlotIndex = -1;
                }
                else if (CurrentInterface == eCurrentInterface.Hat && this.PetHatSlot?.HoveredSlotIndex >= 0) { this.PetHatSlot.HoveredSlotIndex = -1;}
                else if (CurrentInterface == eCurrentInterface.Accessory && this.PetAccessorySlot?.HoveredSlotIndex >= 0) { this.PetAccessorySlot.HoveredSlotIndex = -1;}
                if (!hoveringOkayButton) { Game1.playSound("shiny4"); }
                hoveringOkayButton = true;
            }
        }
        private void moveDownInsideInventoryTab()
        {
            if (this.PlayerInventory?.HoveredSlotIndex >= 0)
            {
                this.PlayerInventory.MoveToDownSlot(dontMoveBeyondMax:Game1.player.MaxItems);
            }
            else if (CurrentInterface == eCurrentInterface.InventoryMultiple && this.PetBackpack?.HoveredSlotIndex >= 0)
            {
                this.PetBackpack.MoveToDownSlot();
            }
            else
            {
                bool flag = false;
                if (CurrentInterface == eCurrentInterface.InventoryOne || CurrentInterface == eCurrentInterface.InventoryMultiple) { flag = this.PetPocketSlot.HoveredSlotIndex == 0; this.PetPocketSlot.HoveredSlotIndex = 0; }
                else if (CurrentInterface == eCurrentInterface.Hat) { flag = this.PetHatSlot.HoveredSlotIndex == 0; this.PetHatSlot.HoveredSlotIndex = 0; }
                else if (CurrentInterface == eCurrentInterface.Accessory) { flag = this.PetAccessorySlot.HoveredSlotIndex == 0; this.PetAccessorySlot.HoveredSlotIndex = 0;}
                if (hoveringOkayButton) { hoveringOkayButton = false; }
                if (!flag) { Game1.playSound("shiny4"); }
            }
        }
        private bool hoverInventoryUI(int mouseX, int mouseY)
        {
            bool UIActive = false;
            if (CurrentInterface != eCurrentInterface.None && CurrentInterface != eCurrentInterface.StatusTab)
            {
                switch (CurrentInterface)
                {
                    case eCurrentInterface.InventoryMultiple:
                        this.PetBackpack?.Hover(mouseX, mouseY);
                        this.PetPocketSlot?.Hover(mouseX, mouseY);
                        break;
                    case eCurrentInterface.Hat:
                        this.PetHatSlot?.Hover(mouseX, mouseY);
                        break;
                    case eCurrentInterface.Accessory:
                        this.PetAccessorySlot?.Hover(mouseX, mouseY);
                        break;
                    case eCurrentInterface.InventoryOne:
                        this.PetPocketSlot?.Hover(mouseX, mouseY);
                        break;
                    default: break;
                }
                this.hoveringOkayButton = this.okButtonBBox.Contains(mouseX, mouseY);
                this.PlayerInventory?.Hover(mouseX, mouseY);
                UIActive = true;
            }
            return UIActive;
        }
        private bool UpdateSelectedObject = false;
        public override void performHoverAction(int mouseX, int mouseY)
        {
            if (transitioning) { return; }
            MouseInMotion = Math.Max(MouseInMotion - 1, 0);
            if (CurrentMousePosition.X != mouseX || CurrentMousePosition.Y != mouseY)
            {
                CurrentMousePosition = new Point(mouseX, mouseY);
                MouseMoved = true;
                UpdateSelectedObject = true;
                MouseInMotion = 5;
            }
            if (this.CurrentInterface == eCurrentInterface.StatusTab && UpdateSelectedObject)
            {
                this.StatusWindow.Hover(mouseX, mouseY);
                UpdateSelectedObject = false;
                return;
            }
            if (UpdateSelectedObject && hoverInventoryUI(mouseX, mouseY)) { UpdateSelectedObject = false; return; } //Check if Inventory UI is active.

            if (this.CurrentInterface == eCurrentInterface.None && UpdateSelectedObject)
            {
                int prevControlledObject = CurrentControlledObject;

                int yCalculated = y - (heightForQuestions - height);
                int num2 = yCalculated + SpriteText.getHeightOfString(getCurrentString(), width - 16) + 48;
                int num3 = 8;

                Rectangle ItemBBox = this.BackpackUnlocked ? new Rectangle(x + 144, yCalculated + -68, 76, 52) : new Rectangle(x + 156, yCalculated + -76, 60, 60);
                Rectangle hatBBox = new Rectangle(x + -96, yCalculated + 20, 60, 60);
                Rectangle StatusTabBBox = new Rectangle(x - 108, yCalculated - 116, 88, 92);
                Rectangle AccessoryBBox = new Rectangle(x + -96, yCalculated + 88, 60, 60);
                Rectangle EnergyBBox = new Rectangle(x + 4, yCalculated - 56, 124, 28);

                CurrentControlledObject = StatusTabBBox.Contains(CurrentMousePosition)? 1: EnergyBBox.Contains(CurrentMousePosition) ? 2 : ItemBBox.Contains(CurrentMousePosition) ? 3 : hatBBox.Contains(CurrentMousePosition) ? 4 : AccessoryBBox.Contains(CurrentMousePosition) ? 5 : 0;

                if (CurrentControlledObject == 3 && prevControlledObject != 3 && this.BackpackUnlocked)
                {
                    Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.OpeningBackpack_sound");
                }
                if (CurrentControlledObject != 3 && prevControlledObject == 3 && this.BackpackUnlocked && this.hoveringInventoryChestTimer >= 225)
                {
                    Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.ClosingBackpack_sound");
                }

                if (CurrentControlledObject == 0)
                {
                    for (int i = 0; i < responses.Length; i++)
                    {
                        string responseTextIs = responses[i].responseKey.Equals("GoHomeCommand") && DoConfirmation == 0 ? I18n.PressAgainToConfirmText() : responses[i].responseText;
                        if (mouseY >= num2 - num3 && mouseY < num2 + SpriteText.getHeightOfString(responseTextIs, width) + num3 && mouseX >= this.x && mouseX < (this.x + this.width))
                        {
                            CurrentControlledObject = i + 6;
                            break;
                        }
                        num2 += SpriteText.getHeightOfString(responseTextIs, width) + 16;
                    }
                }

                if (CurrentControlledObject != prevControlledObject && DoConfirmation > -1)
                {
                    setUpQuestions();
                    DoConfirmation = -1;
                }
                if (CurrentControlledObject != prevControlledObject && CurrentControlledObject > 5)
                {
                    Game1.playSound("Cowboy_gunshot");
                }
                else if (CurrentControlledObject > 0 && prevControlledObject != this.CurrentControlledObject && CurrentControlledObject < 6 && (CurrentControlledObject != 3 || (CurrentControlledObject == 3 && !this.BackpackUnlocked)))
                {
                    Game1.playSound("shiny4");
                }
                UpdateSelectedObject = false;
            }
        }
        private void UpdateInventoryUI()
        {
            SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryHatSlot, SynchronizationManager.PetsEnhancedModInventoryHatSlotIndex);
            SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryAccessorySlot, SynchronizationManager.PetsEnhancedModInventoryAccessorySlotIndex);
            SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryPocketItem, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);
            SynchronizationManager.TryGetItemListFromInventory(this.PetInventory, this.temporaryBackpack, SynchronizationManager.PetsEnhancedModInventoryBackpackStartIndex);

            if (this.PetAccessorySlot is not null)
            {
                this.PetAccessorySlot.actualInventory = temporaryAccessorySlot;

                if (this.PetAccessorySlot.actualInventory.Count == 1 && this.PetAccessorySlot.ClickableItemList.Count == 1) { this.PetAccessorySlot.ClickableItemList[0].item = this.PetAccessorySlot.actualInventory[0]; }
            }
            if (this.PetHatSlot is not null)
            {
                this.PetHatSlot.actualInventory = temporaryHatSlot;

                if (this.PetHatSlot.actualInventory.Count == 1 && this.PetHatSlot.ClickableItemList.Count == 1) { this.PetHatSlot.ClickableItemList[0].item = this.PetHatSlot.actualInventory[0]; }
            }
            if (this.PetPocketSlot is not null)
            {
                this.PetPocketSlot.actualInventory = temporaryPocketItem;

                this.PetPocketSlot.DontMoveItems = false;
                foreach (var item in this.temporaryBackpack)
                {
                    if (item is not null)
                    {
                        this.PetPocketSlot.DontMoveItems = true;
                        break;
                    }
                }
                if (this.PetBackpack is null && this.BackpackUnlocked)
                {
                    this.PetBackpack = new PetStorageMenu(x + 10, y - (heightForQuestions - height) - 246, this.temporaryBackpack, 2, 3, 4, 4, new(60, 60), 0.825f);
                    this.CurrentInterface = eCurrentInterface.InventoryMultiple;
                }
            }
            if (this.PetBackpack is not null)
            {
                this.PetBackpack.actualInventory = temporaryBackpack;

                if (!this.BackpackUnlocked)
                {
                    this.CurrentInterface = eCurrentInterface.InventoryOne;
                    this.PetBackpack = null;
                }

            }
        }
        public static bool anyPerkReady(uint perks, int perkIndex, double sMastery)
        {
            return (sMastery >= 2 && !SynchronizationManager.GetBoolByte(perks, perkIndex, 0)) || (sMastery >= 3 && !SynchronizationManager.GetBoolByte(perks, perkIndex, 1)) || (sMastery >= 4 && !SynchronizationManager.GetBoolByte(perks, perkIndex, 2)) || (sMastery >= 5 && !SynchronizationManager.GetBoolByte(perks, perkIndex, 3));
        }
        public static bool anyPerkReady(bool[] perks, double sMastery)
        {
            return (sMastery >= 2 && !perks[0]) || (sMastery >= 3 && !perks[1]) || (sMastery >= 4 && !perks[2]) || (sMastery >= 5 && !perks[3]);
        }
        public bool[] hasPerksReadyToBeUnlocked = new[] { false, false, false, false, false }; 
        public int SameButtonBeingPressed = 0;
        private SButton LastPressedButton = SButton.None;
        public override void update(GameTime time)
        {
            base.update(time);
            SameButtonBeingPressed = buttonPressedEventArgs is not null && buttonPressedEventArgs.IsDown(buttonPressedEventArgs.Button) ? SameButtonBeingPressed + 1 : 0;
            if (LastPressedButton != buttonPressedEventArgs?.Button) { SameButtonBeingPressed = 0; LastPressedButton = buttonPressedEventArgs is not null ? buttonPressedEventArgs.Button : LastPressedButton; }
            doHandIconTap = false;
            if (Game1.options.SnappyMenus && !Game1.lastCursorMotionWasMouse)
            {
                Game1.mouseCursorTransparency = 0f;
            }
            else
            {
                Game1.mouseCursorTransparency = 1f;
            }
            this.animationDeltaTime = this.animationDeltaTime + time.ElapsedGameTime.Milliseconds >= 1500? -1500: this.animationDeltaTime + time.ElapsedGameTime.Milliseconds;

            this.BackpackUnlocked = this.temporaryPocketItem.Count > 0 && this.temporaryPocketItem[0] is not null && this.temporaryPocketItem[0].QualifiedItemId.Equals(SmartPet.PetBackpackQUID);
            this.UpdateInventoryUI();

            SynchronizationManager.TryParseModData(this.SelectedPet.modData, SynchronizationManager.PetModDataKey_NewItemObtained, out NewItemObtained);
            SynchronizationManager.TryParseModData(this.SelectedPet.modData, SynchronizationManager.PetModDataKey_PetLearnedNewSkill, out NewSkillLearned);
            SynchronizationManager.TryParseModData(this.SelectedPet.modData, SynchronizationManager.PetModDataKey_BackgroundStyleIndex, out BackgroundStyleIndex);

            CurrentControlledArrowDelta = CurrentControlledObject > 0 && CurrentControlledObject < 6 ? (CurrentControlledArrowDelta + 65) % 6000 : CurrentControlledArrowDelta;
            if (SynchronizationManager.TryParseModData(SelectedPet.modData, SynchronizationManager.PetModDataKey_SkillPerkTierChecklist, out uint skillPerkTierCheckListParsed))
            {
                var skillMasteryTemp = new SynchronizationManager.SkillMasteryLevelStruct(SelectedPet.modData);
                hasPerksReadyToBeUnlocked[0] = anyPerkReady(skillPerkTierCheckListParsed, 0, skillMasteryTemp.WaitingSkillMastery);
                hasPerksReadyToBeUnlocked[1] = anyPerkReady(skillPerkTierCheckListParsed, 1, skillMasteryTemp.FollowingSkillMastery);
                hasPerksReadyToBeUnlocked[2] = anyPerkReady(skillPerkTierCheckListParsed, 2, skillMasteryTemp.ForagingSkillMastery);
                hasPerksReadyToBeUnlocked[3] = anyPerkReady(skillPerkTierCheckListParsed, 3, skillMasteryTemp.FishingSkillMastery);
                hasPerksReadyToBeUnlocked[4] = anyPerkReady(skillPerkTierCheckListParsed, 4, skillMasteryTemp.HuntingSkillMastery);
            }
            this.isThereAnyPerkReadyToBeUnlocked = hasPerksReadyToBeUnlocked.Contains(true);

            if (this.CurrentInterface == eCurrentInterface.StatusTab)
            {
                this.StatusWindow?.Update(time, animationDeltaTime, skillPerkTierCheckListParsed, hasPerksReadyToBeUnlocked, MouseMoved);
            }
            if (safetyTimer > 0)
            {
                safetyTimer -= time.ElapsedGameTime.Milliseconds;
            }

            if (questionFinishPauseTimer > 0)
            {
                questionFinishPauseTimer -= time.ElapsedGameTime.Milliseconds;
                return;
            }

            if (transitioning)
            {
                if (!transitionInitialized)
                {
                    transitionInitialized = true;
                    transitionX = x + width / 2;
                    transitionY = y + height / 2;
                    transitionWidth = 0;
                    transitionHeight = 0;
                }

                if (transitioningBigger)
                {
                    int num2 = transitionWidth;
                    transitionX -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f);
                    transitionY -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)(heightForQuestions) / (float)width));
                    transitionX = Math.Max(x, transitionX);
                    transitionY = Math.Max((y + height - heightForQuestions), transitionY);
                    transitionWidth += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * 2f);
                    transitionHeight += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)(heightForQuestions) / (float)width) * 2f);
                    transitionWidth = Math.Min(width, transitionWidth);
                    transitionHeight = Math.Min(heightForQuestions, transitionHeight);
                    if (num2 == 0 && transitionWidth > 0)
                    {
                        playOpeningSound();
                    }

                    if (transitionX == x && transitionY == (y + height - heightForQuestions))
                    {
                        transitioning = false;
                        setUpIcons();
                        transitionX = x;
                        transitionY = y;
                        transitionWidth = width;
                        transitionHeight = height;
                    }
                }
                else
                {
                    transitionX += (int)((float)time.ElapsedGameTime.Milliseconds * 3f);
                    transitionY += (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)height / (float)width));
                    transitionX = Math.Min(x + width / 2, transitionX);
                    transitionY = Math.Min(y + height / 2, transitionY);
                    transitionWidth -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * 2f);
                    transitionHeight -= (int)((float)time.ElapsedGameTime.Milliseconds * 3f * ((float)height / (float)width) * 2f);
                    transitionWidth = Math.Max(0, transitionWidth);
                    transitionHeight = Math.Max(0, transitionHeight);
                    if (transitionWidth == 0 && transitionHeight == 0)
                    {
                        if (!this.BackpackUnlocked && this.NewItemObtained)
                        {
                            PetHelper.TurnOffNewItemHasBeenObtained(this.SelectedPet.petId.Value);
                        }
                        closeDialogue();
                    }
                }
            }
            else
            {
                if (buttonPressedEventArgs is not null) { ReceiveButtonPress(buttonPressedEventArgs, SameButtonBeingPressed); }

                this.PetIconScale = Math.Max(0.825f, this.PetIconScale - 0.025f);
                this.InventoryItemScale = Math.Max(0.825f, this.InventoryItemScale - 0.025f);
                this.AccessoryIconScale = Math.Max(0.825f, this.AccessoryIconScale - 0.025f);
                this.HatIconScale = Math.Max(0.9f, this.HatIconScale - 0.025f);

                this.PetIconScale = CurrentControlledObject == 1 ? Math.Min(this.PetIconScale + 0.05f, 0.925f) : this.PetIconScale;
                this.InventoryItemScale = CurrentControlledObject == 3 ? Math.Min(this.InventoryItemScale + 0.05f, 0.925f) : this.InventoryItemScale;
                this.AccessoryIconScale = CurrentControlledObject == 5 ? Math.Min(this.AccessoryIconScale + 0.05f, 0.925f) : this.AccessoryIconScale;
                this.HatIconScale = CurrentControlledObject == 4 ? Math.Min(this.HatIconScale + 0.05f, 1f) : this.HatIconScale;
                this.hoveringInventoryChestTimer = CurrentControlledObject == 3 ? Math.Min(300, this.hoveringInventoryChestTimer + 75) : Math.Max(0, this.hoveringInventoryChestTimer - 75);

                if (CurrentInterface != eCurrentInterface.StatusTab && CurrentInterface != eCurrentInterface.None)
                {
                    this.PlayerInventory?.Update(time);
                    this.okButtonTextureScale = MathF.Max(1, okButtonTextureScale - 0.025f);
                    if (hoveringOkayButton && this.HeldItem is null)
                    {
                        this.okButtonTextureScale = MathF.Min(okButtonTextureScale + 0.05f, 1.1f);
                    }
                }
                switch (CurrentInterface)
                {
                    case eCurrentInterface.InventoryMultiple:
                        this.PetBackpack?.Update(time);
                        this.PetPocketSlot?.Update(time);
                        break;
                    case eCurrentInterface.Hat:
                        this.PetHatSlot?.Update(time, this.HeldItem);
                        break;
                    case eCurrentInterface.Accessory:
                        this.PetAccessorySlot?.Update(time, this.HeldItem);
                        break;
                    case eCurrentInterface.InventoryOne:
                        this.PetPocketSlot?.Update(time);
                        break;
                }

            }
        }
        public void SetUpPlayerInventory()
        {
            this.PlayerInventory = new PetStorageMenu(x + width - 4, y - (heightForQuestions - height) - 56, Game1.player.Items,12, Math.Max((int)(Game1.player.MaxItems / 12f), 3), verticalGap: 4, _slotSize: new(64,64),_substractInventorySizeFromPosition:true);
        }
        private void setUpQuestions(string confirmationText = null)
        {
            int widthConstraint = width;
            heightForQuestions = SpriteText.getHeightOfString(getCurrentString(), widthConstraint);
            Response[] array = responses;
            foreach (Response response in array)
            {
                heightForQuestions += SpriteText.getHeightOfString(confirmationText is not null && response.responseKey.Equals("GoHomeCommand")? confirmationText: response.responseText, widthConstraint) + 16;
            }

            heightForQuestions += 40;
        }
        public string getCurrentString()
        {
            if (dialogues.Count > 0)
            {
                return dialogues[0].Trim().Replace(Environment.NewLine, "");
            }
            return "";
        }
        private void setUpIcons()
        {
            setUpForGamePadMode();
            if (getCurrentString() != null && getCurrentString().Length <= 20)
            {
                safetyTimer -= 200;
            }
        }
        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {

            width = 1060;
            height = heightForQuestions;
            x = (int)Utility.getTopLeftPositionForCenteringOnScreen(width - 88, height).X;
            y = Game1.uiViewport.Height - height - 64;
            setUpIcons();
            if (this.CurrentInterface != eCurrentInterface.None)
            {
                OpenGrabItemUISimple(this.CurrentInterface, false);
            }
        }
        public void drawBox(SpriteBatch b, int xPos, int yPos, int boxWidth, int boxHeight)
        {
            if (transitionInitialized)
            {
                b.Draw(this.UISprites_Texture, new Rectangle(xPos, yPos, boxWidth, boxHeight), new Rectangle(207, 54, 16, 16), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8700f); //paper wall
                b.Draw(this.UISprites_Texture, new Rectangle(xPos, yPos - 20, boxWidth, 20), new Rectangle(207, 49, 1, 5), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8700f); //top wall
                b.Draw(this.UISprites_Texture, new Rectangle(xPos, yPos + boxHeight, boxWidth, 28), new Rectangle(207, 70, 1, 7), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8700f); //bottom wall
                b.Draw(this.UISprites_Texture, new Rectangle(xPos - 24, yPos, 24, boxHeight), new Rectangle(201, 55, 6, 1), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8700f); //left wall
                b.Draw(this.UISprites_Texture, new Rectangle(xPos + boxWidth, yPos, 24, boxHeight), new Rectangle(223, 54, 6, 1), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8700f); //right wall
                b.Draw(this.UISprites_Texture, new Vector2(xPos - 24, yPos - 20), new Rectangle(201, 49, 6, 5), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8700f); //-x -y c
                b.Draw(this.UISprites_Texture, new Vector2(xPos + boxWidth, yPos - 20), new Rectangle(223, 49, 6, 5), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8700f); //+x -y c
                b.Draw(this.UISprites_Texture, new Vector2(xPos + boxWidth, yPos + boxHeight), new Rectangle(223, 70, 6, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8700f); //+x +y c
                b.Draw(this.UISprites_Texture, new Vector2(xPos - 24, yPos + boxHeight), new Rectangle(201, 70, 6, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8700f); //-x +y c

            }
            if (!transitioning)
            {
                bool aBool = this.temporaryHatSlot[0] is not null;
                bool bBool = this.temporaryAccessorySlot[0] is not null;
                b.Draw(this.UISprites_Texture, new Vector2(xPos - 120f, yPos - 20f), new Rectangle(aBool && bBool? 81 : aBool? 27 : bBool? 54: 0, 0, 27, 49), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8701f); //Pet Accessory UI sprite
                b.Draw(this.UISprites_Texture, new Vector2(xPos - 108, yPos - 116f), new Rectangle(this.BackgroundStyleIndex * 23, 208, 23, 24), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8701f); //Pet status tab window
                if (this.CurrentInterface != eCurrentInterface.InventoryMultiple)
                {
                    b.Draw(this.UISprites_Texture, new Vector2(xPos - 20, yPos - 64f), new Rectangle(201, 78, 38, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8701f); //Pet Energy bar
                    if (this.CurrentInterface != eCurrentInterface.InventoryOne)
                    {
                        int itemFrameOriginX = !this.BackpackUnlocked ? 60 : this.hoveringInventoryChestTimer >= 200 ? 150 : this.hoveringInventoryChestTimer >= 100 ? 129 : 108;
                        b.Draw(this.UISprites_Texture, new Vector2(xPos + 140f, !this.BackpackUnlocked ? yPos - 84f : yPos - 72), new Rectangle(itemFrameOriginX, this.BackpackUnlocked ? 0 : 49, !this.BackpackUnlocked ? 23 : 21, !this.BackpackUnlocked ? 18 : 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8701f); //Pet item slot UI sprite (backpack variant)
                    }
                }
            }
        }
        public void drawPlayerInventoryTab(SpriteBatch b)
        {
            this.PlayerInventorySpriteRec = new Rectangle(this.PlayerInventory.xPositionOnScreen, this.PlayerInventory.yPositionOnScreen, this.PlayerInventory.inventoryWidth, this.PlayerInventory.inventoryHeight + 4);

            int box_X = PlayerInventorySpriteRec.X; //PlayerInventorySpriteRec.X - 28, PlayerInventorySpriteRec.Y - 36, PlayerInventorySpriteRec.Right + 16, PlayerInventorySpriteRec.Bottom - 4
            int box_Y = PlayerInventorySpriteRec.Y;
            int box_Width = PlayerInventorySpriteRec.Width;
            int box_Height = PlayerInventorySpriteRec.Height;
            int box_Bottom = PlayerInventorySpriteRec.Bottom;
            int box_Right = PlayerInventorySpriteRec.Right;

            b.Draw(Game1.mouseCursors, new Rectangle(box_X - 8, box_Y - 8, box_Width + 16, box_Height + 8), new Rectangle(389, 379, 7, 7), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8820f); //Box BG
            b.Draw(Game1.mouseCursors, new Rectangle(box_X - 8, box_Y - 32, box_Width + 16, 24), new Rectangle(389, 373, 8, 6), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8820f); //Box Frame Top
            b.Draw(Game1.mouseCursors, new Rectangle(box_X - 8, box_Bottom, box_Width + 16, 20), new Rectangle(389, 386, 7, 5), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8820f); //Box Frame Bottom
            b.Draw(Game1.mouseCursors, new Rectangle(box_X - 28, box_Y - 8, 20, box_Height + 8), new Rectangle(384, 379, 5, 7), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8820f); //Box Frame Left
            b.Draw(Game1.mouseCursors, new Rectangle(box_Right + 8, box_Y - 8, 20, box_Height + 8), new Rectangle(397, 379, 5, 7), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.8820f); //Box Frame Right

            b.Draw(Game1.mouseCursors, new Vector2(box_X - 28, box_Y - 32), new Rectangle(384, 373, 5, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8820f); //Corner X-, Y-
            b.Draw(Game1.mouseCursors, new Vector2(box_Right + 8, box_Bottom), new Rectangle(397, 386, 5, 5), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8820f); //Corner X+, Y+
            b.Draw(Game1.mouseCursors, new Vector2(box_X - 28, box_Bottom), new Rectangle(384, 386, 5, 5), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8820f); //Corner X-, Y+
            b.Draw(Game1.mouseCursors, new Vector2(box_Right + 8, box_Y - 32), new Rectangle(397, 373, 5, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8820f); //Corner X+, Y-

            this.PlayerInventory?.draw(b, this.HeldItem, true, mouseMoved: MouseMoved, drawTooltip:false);
        }
        public void RescueHeldItemOnExit()
        {
            if (this.HeldItem != null)
            {
                if (this.HeldItem != null)
                {
                    this.HeldItem = Game1.player.addItemToInventory(this.HeldItem);
                }
                this.DropHeldItem();
            }
        }

        public void DropHeldItem()
        {
            if (this.HeldItem != null && this.HeldItem.canBeDropped())
            {
                Game1.playSound("throwDownITem");
                int drop_direction = Game1.player.FacingDirection;

                Game1.createItemDebris(this.HeldItem, Game1.player.getStandingPosition(), drop_direction);
                this.HeldItem = null;
            }
        }
        int animationDeltaTime = -1500;
        public void DrawMenuIcons(SpriteBatch b)
        {
            float yCalculated = y - (heightForQuestions - height);

            this.SelectedPet.GetPetIcon(out string petIconAssetName, out Rectangle petIconSrcRect);
            if (petIconAssetName is not null && !PetHelper.TryLoadTextureEfficiently(petIconAssetName).IsNull(out var PetIconTexture))
            {
                bool isTurtle = SelectedPet.petType is not null && (SelectedPet.petType.Contains("turtle") || SelectedPet.petType.Contains("Turtle"));
                b.Draw(PetIconTexture, position: new Vector2(x - 62 + (isTurtle ? -4 : 0), yCalculated - 68) - (SunkenLaceUtilities.OffsetByScale(petIconSrcRect.Width, petIconSrcRect.Height, this.PetIconScale) * 4), petIconSrcRect, Color.White, 0f, Vector2.Zero, this.PetIconScale * 4f, SpriteEffects.None, 0.8702f);
            }
            //Item shown slot
            if (this.CurrentInterface != eCurrentInterface.InventoryOne && this.CurrentInterface != eCurrentInterface.InventoryMultiple && !this.BackpackUnlocked && !this.temporaryPocketItem[0].IsNull(out var itemHD))
            {
                itemHD.drawInMenu(b, new Vector2(x + 154f, yCalculated - 78), this.InventoryItemScale, 1f, 0.8702f, StackDrawType.Draw, Color.White, false);
            }
            //Accessory draw slot
            if (this.CurrentInterface != eCurrentInterface.Accessory && !this.temporaryAccessorySlot[0].IsNull(out var itemAcc))
            {
                itemAcc.drawInMenu(b, new Vector2(x - 97f, yCalculated + 86f), this.AccessoryIconScale, 1f, 0.8702f, StackDrawType.Draw, Color.White, false);
            }
            //Hat icon
            if (this.CurrentInterface != eCurrentInterface.Hat && !this.temporaryHatSlot[0].IsNull(out var itemNotHat) && itemNotHat is Hat itemHat)
            {
                itemHat.draw(b, new Vector2(x - 82f, yCalculated + 36) - (SunkenLaceUtilities.OffsetByScale(10, 10, this.HatIconScale) * 3), this.HatIconScale, 1, 0.8702f, 2, true);
            }

            if (this.NewItemObtained && (this.CurrentInterface != eCurrentInterface.InventoryMultiple && this.CurrentInterface != eCurrentInterface.InventoryOne))
            {
                float addedX = this.CurrentControlledObject == 3 ? -22 : 0;
                Vector2 iconOffset = this.BackpackUnlocked ? new Vector2(-4, 16) : Vector2.Zero;
                b.Draw(PetHelper.TryLoadTextureEfficiently("LooseSprites\\Cursors"), position: new Vector2(x + 176f + addedX, yCalculated - 152 + YoffsetForEaseInOutLoop(animationDeltaTime, -6.5f)) + iconOffset, new Microsoft.Xna.Framework.Rectangle(403, 496, 5, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8705f);
            }
            if (this.NewSkillLearned || isThereAnyPerkReadyToBeUnlocked)
            {
                SkillMasteryRelatedNotificationIcon.Draw(b, UISprites_Texture,customPos:new Vector2(x - 78, yCalculated - 180 + YoffsetForEaseInOutLoop(animationDeltaTime, -4f)));
            }
        }
        public static float YoffsetForEaseInOutLoop(float time, float amplitude)
        {
            float duration = 1500;

            // Normalize time to range [0, 1] for each phase
            float normalizedTime = (time % duration) / duration;

            return amplitude * MathF.Sin((normalizedTime * MathF.PI * 2) - (MathF.PI / 2));
        }
        private void DrawEnergyBar(SpriteBatch b,Texture2D texture, int _x, int _y, float _energyPercentage)
        {
            b.Draw(texture, new Vector2(_x + 4, _y - 48), new Rectangle(171, _energyPercentage >= 1f? 84 : (int)(28 * _energyPercentage) * 3, 29, 3), _energyPercentage==0f?Color.Transparent:Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8702f);
        }
        private void DrawControllerArrowForSelection(SpriteBatch b, Texture2D texture, int _x, int _y, int _currentControlled, int _controllerArAnDelta)
        {
            if (_currentControlled == 0 || _currentControlled >= 6) { return; }

            Rectangle sRect = new Rectangle(201, 39, 9, 8);
            float addedP = 0;
            SpriteEffects flip = _controllerArAnDelta >= 4000? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 arrowPosition = _currentControlled == 4 ? new Vector2(-144, 68) : _currentControlled == 5 ? new Vector2(-144, 136) : _currentControlled == 2 ? new Vector2(46, -96) : _currentControlled == 1 ? new Vector2(-148, -50) : new Vector2(0, 0);
            Vector2 arrowFloatingAnimationOffset = _currentControlled != 2? new(0, YoffsetForEaseInOutLoop(animationDeltaTime, -4f)) : Vector2.Zero;
            switch (_currentControlled)
            {
                case 5: { arrowPosition = new(-148, 136); break; }
                case 4: { arrowPosition = new(-148, 68); break; }
                case 3: 
                    {
                        Vector2 iconOffset = this.BackpackUnlocked ? new Vector2(-4, 16) : Vector2.Zero;
                        float addedX = this.NewItemObtained ? + 16 : 0;
                        arrowPosition = new(168f + iconOffset.X + addedX, -126 + iconOffset.Y);
                        break;
                    }
                case 2: { arrowPosition = new(46, -96); break; }
                case 1: { arrowPosition = new(-152, -50); break; }
            }
            float doRotate = 0f;
            if ((_controllerArAnDelta >= 1000 && _controllerArAnDelta < 2000) || (_controllerArAnDelta >= 5000 && _controllerArAnDelta < 6000))
            {
                sRect.X = 211;
                sRect.Width = 7;
                addedP = 4;
            }
            else if ((_controllerArAnDelta >= 2000 && _controllerArAnDelta < 3000) || (_controllerArAnDelta >= 4000 && _controllerArAnDelta < 5000))
            {
                sRect.X = 219;
                sRect.Width = 5;
                addedP = 8;
            }
            else if (_controllerArAnDelta >= 3000 && _controllerArAnDelta < 4000)
            {
                sRect.X = 225;
                sRect.Width = 3;
                addedP = 12;
            }
            Vector2 addedPos = new(addedP, 0);
            if (_currentControlled == 1 || _currentControlled == 4 || _currentControlled == 5)
            {
                addedPos = new(0, -addedP);
                doRotate = -1.5708f; //90º
                arrowFloatingAnimationOffset = new(arrowFloatingAnimationOffset.Y, 0);
            }
            b.Draw(texture, new Vector2(_x + arrowPosition.X, _y + arrowPosition.Y) + addedPos + arrowFloatingAnimationOffset, sRect, Color.White, doRotate, Vector2.Zero, 4f, flip, 0.8702f);
        }
        public override void draw(SpriteBatch b)
        {
            if (width < 16 || height < 16)
            {
                DrawMouse(b);
                return;
            }
            if (transitioning)
            {
                drawBox(b, transitionX, transitionY, transitionWidth, transitionHeight);
                DrawMouse(b);
                return;
            }

            var thisPetData = CachePetData.GetPetDataForPet(this.SelectedPet);
            SynchronizationManager.TryParseModData(this.SelectedPet.modData, SynchronizationManager.PetModDataKey_PetEnergy, out int petEnergy);
            SynchronizationManager.TryParseModData(this.SelectedPet.modData, SynchronizationManager.PetModDataKey_Following_SkillMasteryLevel, out double petFollowingSkillMasteryLevel);
            int petMaxBaseEnergy = (SmartPet.MaxBaseEnergyNoUpgrade + (petFollowingSkillMasteryLevel >= 5 ? 80 : 0));

            if (this.CurrentInterface != eCurrentInterface.StatusTab) 
            {
                drawBox(b, x, y - (heightForQuestions - height), width, heightForQuestions);
                DrawMenuIcons(b);
                DrawEnergyBar(b, this.UISprites_Texture, x, y - (heightForQuestions - height), (float)petEnergy / (float)petMaxBaseEnergy);
            }
            if (this.CurrentInterface == eCurrentInterface.None)
            {
                int xOffsetForTreatIcon = 22;
                switch (LocalizedContentManager.CurrentLanguageCode)
                {
                    case LocalizedContentManager.LanguageCode.zh:
                        xOffsetForTreatIcon = 8;
                        break;
                    case LocalizedContentManager.LanguageCode.ja:
                        xOffsetForTreatIcon = 18;
                        break;
                    case LocalizedContentManager.LanguageCode.mod:
                        xOffsetForTreatIcon = 12;
                        break;
                }
                SpriteText.drawString(b, getCurrentString(), x + 8, y + 12 - (heightForQuestions - height), getCurrentString().Length, width - 16);

                int num = y - (heightForQuestions - height) + SpriteText.getHeightOfString(getCurrentString(), width - 16) + 48;
                for (int i = 0; i < responses.Length; i++)
                {
                    string responseTextIs = responses[i].responseKey.Equals("GoHomeCommand") && DoConfirmation == 0 ? I18n.PressAgainToConfirmText() : responses[i].responseText;
                    if (CurrentControlledObject > 5 && i == CurrentControlledObject - 6)
                    {
                        IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), x + 4, num - 8, width - 8, SpriteText.getHeightOfString(responseTextIs, width) + 16, Color.White, 4f, drawShadow: false);
                    }
                    SpriteText.drawString(b, responseTextIs, x + 8, num, 999999, width, 999999, (CurrentControlledObject > 5 && i == CurrentControlledObject - 6) ? 1f : 0.6f);
                    if (responses[i].responseKey.Contains("Unlock") && !responses[i].responseKey.Contains("FreeUnlock") && responses[i].responseText[0] == '[')
                    {
                        Item itTreat = ItemRegistry.Create(thisPetData.TrickLearningTreat, allowNull: true);
                        itTreat?.drawInMenu(b, new Vector2(x + xOffsetForTreatIcon, num - 12), 0.7f, 1f, 0.8702f, StackDrawType.Draw, Color.White, false);
                    }
                    num += SpriteText.getHeightOfString(responseTextIs, width) + 16;
                }

                if (CurrentControlledObject == 2)
                {
                    Point overridePosition = !MouseMoved? new Point(x, y - (heightForQuestions - height) - 24) : new Point(-1,-1);
                    IClickableMenu.drawHoverText(b, petEnergy + "/" + petMaxBaseEnergy, Game1.smallFont, 0, 0,overrideX: overridePosition.X,overrideY: overridePosition.Y);
                }
                DrawHotkeysForFrames(b, this.UISprites_Texture, x, y - (heightForQuestions - height));
                DrawControllerArrowForSelection(b, this.UISprites_Texture, x, y - (heightForQuestions - height), CurrentControlledObject, CurrentControlledArrowDelta);
            }
            if (this.CurrentInterface != eCurrentInterface.None)
            {
                b.Draw(texture: Game1.fadeToBlackRect, destinationRectangle: new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), sourceRectangle: null, color: Color.Black * 0.5f, rotation: 0f, origin: Vector2.Zero, effects: SpriteEffects.None, layerDepth: 0.8810f);
            }
            if (this.PlayerInventory is not null)
            {
                drawPlayerInventoryTab(b);
            }
            if (this.CurrentInterface != eCurrentInterface.None && this.CurrentInterface != eCurrentInterface.StatusTab)
            {
                b.Draw(Game1.mouseCursors, new Vector2(this.okButtonBBox.Location.X + 4, this.okButtonBBox.Location.Y + 4) - SunkenLaceUtilities.OffsetByScale(64, 64, this.okButtonTextureScale, true), new Rectangle(128, 256, 64, 64), Color.White, 0f, Vector2.Zero, this.okButtonTextureScale, SpriteEffects.None, 0.876f);
            }
            switch (CurrentInterface)
            {
                case eCurrentInterface.InventoryMultiple:
                    this.PInventoryMFrameSpriteRec = new Rectangle(x - 8, y - (heightForQuestions - height) - 260, 60 * 4, 58 * 4);
                    b.Draw(this.UISprites_Texture, new Vector2(PInventoryMFrameSpriteRec.X, PInventoryMFrameSpriteRec.Y), new Rectangle(108, 15, 60, 58), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8701f);
                    this.PetPocketSlot?.draw(b, this.HeldItem, mouseMoved:MouseMoved);
                    this.PetBackpack?.draw(b, this.HeldItem, mouseMoved: MouseMoved);
                    break;
                case eCurrentInterface.Hat:
                    this.HatFrameSpriteRec = new Rectangle(x - 104, y - (heightForQuestions - height) + 16, 20 * 4, 20 * 4);
                    b.Draw(this.UISprites_Texture, new Vector2(HatFrameSpriteRec.X, HatFrameSpriteRec.Y), new Rectangle(this.temporaryHatSlot[0] is not null? 40: 0, 49, 20, 20), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8701f);
                    this.PetHatSlot?.draw(b, this.HeldItem, mouseMoved: MouseMoved);
                    break;
                case eCurrentInterface.Accessory:
                    this.AccFrameSpriteRec = new Rectangle(x - 104, y - (heightForQuestions - height) + 22 + 58, 20 * 4, 20 * 4);
                    b.Draw(this.UISprites_Texture, new Vector2(AccFrameSpriteRec.X, AccFrameSpriteRec.Y), new Rectangle(this.temporaryAccessorySlot[0] is not null ? 40 : 20, 49, 20, 20), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8701f);
                    this.PetAccessorySlot?.draw(b, this.HeldItem, mouseMoved: MouseMoved);
                    break;
                case eCurrentInterface.InventoryOne:
                    this.PInventorySFrameSpriteRec = new Rectangle(x + 144, y - (heightForQuestions - height) - 116, 22 * 4, 22 * 4);
                    b.Draw(this.UISprites_Texture, new Vector2(PInventorySFrameSpriteRec.X, PInventorySFrameSpriteRec.Y), new Rectangle(83, 49, 22, 22), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8701f);
                    this.PetPocketSlot?.draw(b, this.HeldItem, mouseMoved: MouseMoved);
                    break;
                case eCurrentInterface.StatusTab:
                    this.StatusWindow?.Draw(b, this.UISprites_Texture, this.BackgroundStyleIndex, UseL1andR1backgroundChangeShortcuts);
                    break;
            }
            this.PlayerInventory?.DrawInventoryItemsTooltips(b, this.HeldItem, MouseMoved);
            DrawMouse(b);
        }
        private void DrawMouse(SpriteBatch b, bool ignore_transparency = false, int cursor = 0)
        {
            if (!Game1.options.hardwareCursor && MouseMoved)
            {
                this.HeldItem?.drawInMenu(b, new Vector2(Game1.getMouseX() + 16, Game1.getMouseY() + 16), 1f, 1f, 0.88f, StackDrawType.Draw);
                float num = Game1.mouseCursorTransparency;
                if (ignore_transparency)
                {
                    num = 1f;
                }
                b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, cursor, 16, 16), Color.White * num, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
            }
            else if (!MouseMoved && (CurrentInterface != eCurrentInterface.StatusTab && CurrentInterface != eCurrentInterface.None))
            {
                Point p = hoveringOkayButton? new(okButtonBBox.Right,okButtonBBox.Bottom): GetMousePositionBasedOnCurrentlySelectedSlotInInventories();
                if (p != Point.Zero)
                {
                    this.HeldItem?.drawInMenu(b, new Vector2(p.X + 12, p.Y + 8), 1f, 1f, 0.88f, StackDrawType.Draw);
                    DrawControllerSelectMouse(b, this.UISprites_Texture, p.X - 12, p.Y - 20);
                }
            }
        }
        private void DrawControllerSelectMouse(SpriteBatch b, Texture2D texture, int _cursorX, int _cursorY)
        {
            b.Draw(texture, new Vector2(_cursorX, _cursorY), new Rectangle(doHandIconTap ? 423 : 412,215,10,10), Color.White, 0f, Vector2.Zero, 4f + (0.25f * MathF.Abs(animationDeltaTime) / 1500f), SpriteEffects.None, 0.8702f);
        }
        private void DrawHotkeysForFrames(SpriteBatch b, Texture2D texture, int _x, int _y)
        {
            int currentControllerTest = Game1.options.gamepadControls ? CurrentGamepadLayout + 1: 0;
            SButton buttonSt = StatusTabHotkey;
            SButton buttonIv = InventoryTabHotkey;
            SButton buttonHs = HatTabHotkey;
            SButton buttonAs = AccessoryTabHotkey;
            if (currentControllerTest > 0)
            {
                buttonSt = OpenStatusWindowHotkeySubstituteForGamepad;
                buttonIv = OpenInventoryHotkeySubstituteForGamepad;
                buttonHs = OpenHatSlotHotkeySubstituteForGamepad;
                buttonAs = OpenAccessorySlotHotkeySubstituteForGamepad;
            }
            Vector2 iconOffset = this.BackpackUnlocked ? new Vector2(-4, 16) : Vector2.Zero;
            b.Draw(texture, new Vector2(_x - 44, _y - 125), GetSpriteForAssignedKey(currentControllerTest, buttonSt), Color.White, 0f, Vector2.Zero, 3.7f, SpriteEffects.None, 0.8702f); //Status frame
            b.Draw(texture, new Vector2(_x + 204, _y - 94) + iconOffset, GetSpriteForAssignedKey(currentControllerTest, buttonIv), Color.White, 0f, Vector2.Zero, 3.4f, SpriteEffects.None, 0.8702f); //Inventory frame
            b.Draw(texture, new Vector2(_x - 50, _y + 66), GetSpriteForAssignedKey(currentControllerTest, buttonHs), Color.White, 0f, Vector2.Zero, 3.4f, SpriteEffects.None, 0.8702f); //HatSlot frame
            b.Draw(texture, new Vector2(_x - 50, _y + 134), GetSpriteForAssignedKey(currentControllerTest, buttonAs), Color.White, 0f, Vector2.Zero, 3.4f, SpriteEffects.None, 0.8702f); //Accessory frame
        }
        /// <param name="_currentController">0 defines keyboard. 1 defines playstation. 2 defines xbox. 3 defines steamdeck. 4 defines switch. 5 defines other controllers.</param>
        private Rectangle GetSpriteForAssignedKey(int _currentController, SButton _key)
        {
            if (_currentController < 1)
            {
                if (InputTextureDictionary.TryGetValue(_key, out var _result))
                {
                    return _result;
                }
                return new Rectangle(561, 1, 10, 10); //Blank Icon
            }
            switch (_key)
            {
                case SButton.ControllerA: { return _currentController == 1 ? InputTextureDictionary[SButton.X] : _currentController == 4 ? InputTextureDictionary[SButton.B] : InputTextureDictionary[SButton.A]; }
                case SButton.ControllerB: { return _currentController == 1 ? new Rectangle(583, 111, 10, 10) /*|Circle|*/ : _currentController == 4 ? InputTextureDictionary[SButton.A] : InputTextureDictionary[SButton.B]; }
                case SButton.ControllerX: { return _currentController == 1 ? new Rectangle(561, 111, 10, 10) /*|Square|*/ : _currentController == 4 ? InputTextureDictionary[SButton.Y] : InputTextureDictionary[SButton.X]; }
                case SButton.ControllerY: { return _currentController == 1 ? new Rectangle(572, 111, 10, 10) /*|Triangle|*/: _currentController == 4 ? InputTextureDictionary[SButton.X] : InputTextureDictionary[SButton.Y]; }
                case SButton.ControllerStart: { return _currentController == 1 || _currentController == 3 ? new Rectangle(561, 133, 10, 10) /*|Options|*/ : _currentController == 4 ? new Rectangle(572, 122, 10, 10) /*|Plus|*/ : new Rectangle(594, 111, 10, 10); /*|Controller Start button|*/}
                case SButton.ControllerBack: { return _currentController == 1 || _currentController == 3 ? new Rectangle(583, 122, 10, 10) /*|Share|*/ : _currentController == 4 ? new Rectangle(561, 122, 10, 10) /*|Minus|*/ : new Rectangle(594, 122, 10, 10); /*|Controller Back button|*/ }
                case SButton.LeftShoulder: { return new Rectangle(561, 197, 11, 8); } //L1
                case SButton.LeftTrigger: { return new Rectangle(573, 197, 11, 8); } //L2
                case SButton.RightShoulder: { return new Rectangle(561, 188, 11, 8); } //R1
                case SButton.RightTrigger: { return new Rectangle(573, 188, 11, 8); } //R2
                default: { return new Rectangle(561, 1, 10, 10); } //Blank Icon
            }
        }
        private readonly Dictionary<SButton, Rectangle> InputTextureDictionary = new()
        {
            [SButton.A] = new Rectangle(572, 1, 10, 10),
            [SButton.B] = new Rectangle(583, 1, 10, 10),
            [SButton.C] = new Rectangle(594, 1, 10, 10),
            [SButton.D] = new Rectangle(561, 12, 10, 10),
            [SButton.E] = new Rectangle(572, 12, 10, 10),
            [SButton.F] = new Rectangle(583, 12, 10, 10),
            [SButton.G] = new Rectangle(594, 12, 10, 10),
            [SButton.H] = new Rectangle(561, 23, 10, 10),
            [SButton.I] = new Rectangle(572, 23, 10, 10),
            [SButton.J] = new Rectangle(583, 23, 10, 10),
            [SButton.K] = new Rectangle(594, 23, 10, 10),
            [SButton.L] = new Rectangle(561, 34, 10, 10),
            [SButton.M] = new Rectangle(572, 34, 10, 10),
            [SButton.N] = new Rectangle(583, 34, 10, 10),
            [SButton.O] = new Rectangle(594, 34, 10, 10),
            [SButton.P] = new Rectangle(561, 45, 10, 10),
            [SButton.Q] = new Rectangle(572, 45, 10, 10),
            [SButton.R] = new Rectangle(583, 45, 10, 10),
            [SButton.S] = new Rectangle(594, 45, 10, 10),
            [SButton.T] = new Rectangle(561, 56, 10, 10),
            [SButton.U] = new Rectangle(572, 56, 10, 10),
            [SButton.V] = new Rectangle(583, 56, 10, 10),
            [SButton.X] = new Rectangle(594, 56, 10, 10),
            [SButton.Y] = new Rectangle(561, 67, 10, 10),
            [SButton.Z] = new Rectangle(572, 67, 10, 10),
            [SButton.OemComma] = new Rectangle(583, 67, 10, 10), //Comma
            [SButton.OemQuotes] = new Rectangle(594, 67, 10, 10), //Apostrophe
            [SButton.NumPad0] = new Rectangle(561, 78, 10, 10), // 0
            [SButton.NumPad1] = new Rectangle(572, 78, 10, 10), // 1
            [SButton.NumPad2] = new Rectangle(583, 78, 10, 10), // 2
            [SButton.NumPad3] = new Rectangle(594, 78, 10, 10), // 3
            [SButton.NumPad4] = new Rectangle(561, 89, 10, 10), // 4
            [SButton.D4] = new Rectangle(561, 89, 10, 10), // 4
            [SButton.NumPad5] = new Rectangle(572, 89, 10, 10), // 5
            [SButton.NumPad6] = new Rectangle(583, 89, 10, 10), // 6
            [SButton.NumPad7] = new Rectangle(594, 89, 10, 10), // 7
            [SButton.D7] = new Rectangle(594, 89, 10, 10), // 7
            [SButton.NumPad8] = new Rectangle(561, 100, 10, 10), // 8
            [SButton.NumPad9] = new Rectangle(572, 100, 10, 10), // 9
            [SButton.OemSemicolon] = new Rectangle(583, 100, 10, 10), //Semicolon
            [SButton.Decimal] = new Rectangle(594, 100, 10, 10), //Decimal
            [SButton.OemPeriod] = new Rectangle(594, 100, 10, 10), //Decimal
            [SButton.OemMinus] = new Rectangle(561, 122, 10, 10), //Minus
            [SButton.Subtract] = new Rectangle(561, 122, 10, 10), //Minus
            [SButton.OemPlus] = new Rectangle(572, 122, 10, 10), //Plus
            [SButton.Add] = new Rectangle(572, 122, 10, 10), //Plus
            [SButton.OemTilde] = new Rectangle(594, 133, 10, 10), //Tilde
            [SButton.OemOpenBrackets] = new Rectangle(561, 144, 10, 10), //Open Brackets
            [SButton.OemCloseBrackets] = new Rectangle(572, 144, 10, 10), //Close Brackets
            [SButton.OemQuestion] = new Rectangle(583, 144, 10, 10), //Question
            [SButton.OemPipe] = new Rectangle(594, 144, 10, 10), //Pipe
            [SButton.D2] = new Rectangle(561, 155, 10, 10), //AtSign
            [SButton.D8] = new Rectangle(572, 155, 10, 10), //Asterisk
            [SButton.Multiply] = new Rectangle(572, 155, 10, 10), //Asterisk
            [SButton.D9] = new Rectangle(583, 155, 10, 10), //Open Parentheses
            [SButton.D0] = new Rectangle(594, 155, 10, 10), //Close Parentheses
            [SButton.D1] = new Rectangle(561, 166, 10, 10), //Exclamation
            [SButton.OemBackslash] = new Rectangle(572, 166, 10, 10), //Backslash
            [SButton.Divide] = new Rectangle(583, 166, 10, 10), //Slash
            [SButton.D6] = new Rectangle(594, 166, 10, 10), //Caret
            [SButton.D5] = new Rectangle(561, 177, 10, 10), //Percentage
            [SButton.D3] = new Rectangle(594, 177, 10, 10), //Hash
            [SButton.Delete] = new Rectangle(585, 188, 11, 8), //Del
            [SButton.Back] = new Rectangle(585, 188, 11, 8), //Del
            [SButton.Enter] = new Rectangle(585, 197, 11, 8), //Enter
            [SButton.End] = new Rectangle(585, 197, 11, 8), //Enter
            [SButton.LeftShift] = new Rectangle(561, 206, 11, 8), //Shift
            [SButton.RightShift] = new Rectangle(561, 206, 11, 8), //Shift
            [SButton.RightControl] = new Rectangle(573, 206, 11, 8), //CTRL
            [SButton.LeftControl] = new Rectangle(573, 206, 11, 8), //CTRL
            [SButton.Space] = new Rectangle(585, 206, 11, 8), //space
            [SButton.Tab] = new Rectangle(561, 215, 11, 8), //tab
            [SButton.LeftAlt] = new Rectangle(573, 215, 11, 8), //alt
            [SButton.RightAlt] = new Rectangle(573, 215, 11, 8), //alt
        };

        private Point GetMousePositionBasedOnCurrentlySelectedSlotInInventories()
        {
            switch (CurrentInterface)
            {
                case eCurrentInterface.Hat:
                    {
                        if (PetHatSlot?.HoveredSlotIndex > -1)
                        {
                            return PetHatSlot.ClickableItemList[PetHatSlot.HoveredSlotIndex].GetLowerRightCorner();
                        }
                        else if (PlayerInventory?.HoveredSlotIndex > -1)
                        {
                            return PlayerInventory.ClickableItemList[PlayerInventory.HoveredSlotIndex].GetLowerRightCorner();
                        }
                    }
                    break;
                case eCurrentInterface.Accessory:
                    {
                        if (PetAccessorySlot?.HoveredSlotIndex > -1)
                        {
                            return PetAccessorySlot.ClickableItemList[PetAccessorySlot.HoveredSlotIndex].GetLowerRightCorner();
                        }
                        else if (PlayerInventory?.HoveredSlotIndex > -1)
                        {
                            return PlayerInventory.ClickableItemList[PlayerInventory.HoveredSlotIndex].GetLowerRightCorner();
                        }
                    }
                    break;
                case eCurrentInterface.InventoryOne:
                    {
                        if (PetPocketSlot?.HoveredSlotIndex > -1)
                        {
                            return PetPocketSlot.ClickableItemList[PetPocketSlot.HoveredSlotIndex].GetLowerRightCorner();
                        }
                        else if (PlayerInventory?.HoveredSlotIndex > -1)
                        {
                            return PlayerInventory.ClickableItemList[PlayerInventory.HoveredSlotIndex].GetLowerRightCorner();
                        }
                    }
                    break;
                case eCurrentInterface.InventoryMultiple:
                    {
                        if (PetPocketSlot?.HoveredSlotIndex > -1)
                        {
                            return PetPocketSlot.ClickableItemList[PetPocketSlot.HoveredSlotIndex].GetLowerRightCorner();
                        }
                        else if (PetBackpack?.HoveredSlotIndex > -1)
                        {
                            return PetBackpack.ClickableItemList[PetBackpack.HoveredSlotIndex].GetLowerRightCorner();
                        }
                        else if (PlayerInventory?.HoveredSlotIndex > -1)
                        {
                            return PlayerInventory.ClickableItemList[PlayerInventory.HoveredSlotIndex].GetLowerRightCorner();
                        }
                    }
                    break;
            }
            return Point.Zero;
        }

    }
}
