using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;
using StardewValley.Menus;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StardewValley.Menus.StorageContainer;
using StardewValley.Inventories;
using System.ComponentModel;
using System.Security.Cryptography;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Security.Cryptography.X509Certificates;
using StardewValley.Buffs;
using StardewValley.Characters;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework.Media;

namespace Pets_Enhanced_Mod.Utilities.Custom_Classes
{
    public class PetStorageMenu
    {

        public List<ClickableItemRect> ClickableItemList = new List<ClickableItemRect>();

        public IList<Item> actualInventory;

        public int capacity 
        {
            get
            {
                return columns * rows;
            } 
        }
        public int xPositionOnScreen;

        public int yPositionOnScreen;

        public int rows = 0;

        public int inventoryWidth;
        public int inventoryHeight;

        public int columns = 0;

        public int horizontalGap;

        public int verticalGap;

        public Point slotSize = new(64, 64);

        public Vector2 itemOffset = new(0, 0);

        public float itemScaleM = 1f;

        public Type ExclusiveItemType;

        public bool DontMoveItems = false;

        public string[] ExclusiveForIds = null;

        public int HoveredSlotIndex = -1;


        public PetStorageMenu(int xPosition, int yPosition, IList<Item> actualInventory, int columns = 1, int rows = 1, int horizontalGap = 0, int verticalGap = 0, Point? _slotSize = null, float _itemScale = 1f, Vector2? _itemOffset = null, Type _onlyAddThisTypeofItem = null, string[] _onlyAddTheseIDs = null, bool _substractInventorySizeFromPosition = false)
        {
            this.xPositionOnScreen = xPosition;
            this.yPositionOnScreen = yPosition;
            this.horizontalGap = horizontalGap;
            this.verticalGap = verticalGap;
            this.rows = rows;
            this.columns = columns;
            if (_slotSize is not null)
            {
                this.slotSize = _slotSize.Value;
            }
            this.inventoryWidth = ((columns * this.slotSize.X) + (columns * this.horizontalGap));
            this.inventoryHeight = ((rows * this.slotSize.Y) + (rows * this.verticalGap));
            if (_substractInventorySizeFromPosition)
            {
                xPositionOnScreen = xPositionOnScreen - inventoryWidth;
                yPositionOnScreen = yPositionOnScreen - inventoryHeight;
            }
            if (_itemOffset is not null)
            {
                this.itemOffset = _itemOffset.Value;
            }
            this.itemScaleM = _itemScale;
            this.actualInventory = actualInventory;

            for (int i = 0; i < Game1.player.maxItems.Value; i++)
            {
                if (Game1.player.Items.Count <= i)
                {
                    Game1.player.Items.Add(null);
                }
            }
            this.ExclusiveItemType = _onlyAddThisTypeofItem;
            this.ExclusiveForIds = _onlyAddTheseIDs;

            for (int j = 0; j < this.capacity; j++)
            {
                float itemXPos = 0 + j % columns * this.slotSize.X + (j % (columns) * this.horizontalGap);
                float itemYPos = 0 + j / columns * this.slotSize.Y + (j / (columns) * this.verticalGap);
                Vector2 positionC = new Vector2(xPositionOnScreen + itemXPos, yPositionOnScreen + itemYPos);
                Item itemT = this.actualInventory.Count > j? this.actualInventory[j] : null;

                ClickableItemList.Add(new ClickableItemRect(new Rectangle(positionC.ToPoint(), this.slotSize), itemT, this.itemScaleM));
            }
        }
        public void draw(SpriteBatch b, Item _heldItem, bool isPlayerInventory = false, bool mouseMoved = true, bool drawTooltip = true)
        {
            if (isPlayerInventory)
            {
                Texture2D texture = Game1.menuTexture;
                for (int j = 0; j < capacity; j++)
                {
                    float itemXPos = 0 + j % columns * this.slotSize.X + (j % (columns) * this.horizontalGap);
                    float itemYPos = 0 + j / columns * this.slotSize.Y + (j / (columns) * this.verticalGap);
                    Vector2 positionC = new(xPositionOnScreen + itemXPos, yPositionOnScreen + itemYPos);
                    b.Draw(texture, positionC, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f);
                    if (j >= Game1.player.maxItems.Value)
                    {
                        b.Draw(texture, positionC, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 57), Color.White * 0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.9f);
                    }
                }
            }
            for (int l = 0; l < capacity; l++)
            {

                if (ClickableItemList.Count > l && ClickableItemList[l] != null && ClickableItemList[l].item != null)
                {
                    ClickableItemList[l].Draw(b, offsetX: this.itemOffset.X, offsetY: this.itemOffset.Y);
                }
            }
            
            if (drawTooltip) { DrawInventoryItemsTooltips(b, _heldItem, mouseMoved); }
        }
        public void DrawInventoryItemsTooltips(SpriteBatch b,Item _heldItem, bool mouseMoved)
        {
            if (this.HoveredSlotIndex != -1 && this.HoveredSlotIndex < actualInventory.Count && actualInventory[HoveredSlotIndex] is not null)
            {
                var lrCorner = ClickableItemList[HoveredSlotIndex].GetLowerRightCorner();
                int addedPosX = _heldItem is not null ? ClickableItemList[HoveredSlotIndex].bounds.Width + 4 : 20;
                int addedPosY = _heldItem is not null ? ClickableItemList[HoveredSlotIndex].bounds.Height : 16;
                Point overridePos = mouseMoved ? new(-1, -1) : new(lrCorner.X + addedPosX, lrCorner.Y + addedPosY);
                DrawToolTip(b, actualInventory[HoveredSlotIndex].getHoverBoxText(_heldItem) ?? actualInventory[HoveredSlotIndex].getDescription(), actualInventory[HoveredSlotIndex].DisplayName, this.actualInventory[this.HoveredSlotIndex], _heldItem != null, _overrideX: overridePos.X, _overrideY: overridePos.Y);
            }
        }
        public Item RightClick(bool isLeftShiftPressed, Item _heldItem = null, bool playSound = true)
        {
            if (this.HoveredSlotIndex != -1 && !DontMoveItems && actualInventory is not null && actualInventory.Count > HoveredSlotIndex && actualInventory[HoveredSlotIndex] is not null)
            {
                if (actualInventory[HoveredSlotIndex] is Tool tool && (_heldItem == null || _heldItem is StardewValley.Object) && tool.canThisBeAttached((StardewValley.Object)_heldItem))
                {
                    return tool.attach((StardewValley.Object)_heldItem);
                }
                if (_heldItem is null)
                {
                    if (actualInventory[HoveredSlotIndex].maximumStackSize() != -1)
                    {

                        Item one = actualInventory[HoveredSlotIndex].getOne();
                        if (actualInventory[HoveredSlotIndex].Stack > 1 && isLeftShiftPressed)
                        {
                            one.Stack = (int)Math.Ceiling((double)actualInventory[HoveredSlotIndex].Stack / 2.0);
                            actualInventory[HoveredSlotIndex].Stack = actualInventory[HoveredSlotIndex].Stack / 2;
                        }
                        else if (actualInventory[HoveredSlotIndex].Stack == 1)
                        {
                            actualInventory[HoveredSlotIndex] = null;
                        }
                        else
                        {
                            actualInventory[HoveredSlotIndex].Stack--;
                        }

                        if (actualInventory[HoveredSlotIndex] != null && actualInventory[HoveredSlotIndex].Stack <= 0)
                        {
                            actualInventory[HoveredSlotIndex] = null;
                            ClickableItemList[HoveredSlotIndex].itemScale = itemScaleM;
                        }

                        if (playSound)
                        {
                            Game1.playSound("dwop");
                        }
                        return one;
                    }
                }
                else
                {
                    Item itemMultiple = _heldItem;
                    if (!actualInventory[HoveredSlotIndex].canStackWith(_heldItem) || _heldItem.Stack >= _heldItem.maximumStackSize())
                    {
                        return _heldItem;
                    }

                    if (isLeftShiftPressed)
                    {
                        int val = (int)Math.Ceiling((double)actualInventory[HoveredSlotIndex].Stack / 2.0);
                        val = Math.Min(itemMultiple.maximumStackSize() - itemMultiple.Stack, val);
                        itemMultiple.Stack += val;
                        actualInventory[HoveredSlotIndex].Stack -= val;
                    }
                    else
                    {

                        itemMultiple.Stack++;
                        actualInventory[HoveredSlotIndex].Stack--;
                    }

                    if (playSound)
                    {
                        Game1.playSound("dwop");
                    }

                    if (actualInventory[HoveredSlotIndex].Stack <= 0)
                    {
                        actualInventory[HoveredSlotIndex] = null;
                        ClickableItemList[HoveredSlotIndex].itemScale = itemScaleM;
                    }
                    return itemMultiple;
                }
            }

            return _heldItem;
        }
        public Item LeftClick(Item _heldItem = null, bool playSound = true, string customPlaceItemSound = null, bool playSoundInLocation = false, bool isPocketSlotItem = false)
        {

            if (HoveredSlotIndex != -1 && !DontMoveItems && actualInventory is not null && actualInventory.Count > HoveredSlotIndex)
            {
                bool isExclusiveType = (this.ExclusiveItemType is not null && _heldItem is not null && _heldItem.GetType() == this.ExclusiveItemType) || this.ExclusiveItemType is null;
                bool isExclusiveID = (this.ExclusiveForIds is not null && _heldItem is not null && this.ExclusiveForIds.Length > 0 && this.ExclusiveForIds.Contains(_heldItem.QualifiedItemId)) || this.ExclusiveForIds is null;
                if (actualInventory[HoveredSlotIndex] != null)
                {
                    if (_heldItem is not null && isExclusiveType && isExclusiveID && !((actualInventory[HoveredSlotIndex].QualifiedItemId.Equals("(O)SunkenLace.PetsEnhancedMod.PetBackpack") || _heldItem.Stack > 1) && isPocketSlotItem && _heldItem.QualifiedItemId.Equals("(O)SunkenLace.PetsEnhancedMod.PetBackpack")))
                    {
                        if (playSound)
                        {
                            if (playSoundInLocation)
                            {
                                Game1.sounds.PlayAll(customPlaceItemSound ?? "stoneStep", Game1.player.currentLocation, Game1.player.Tile, null, context: StardewValley.Audio.SoundContext.Default);
                            }
                            else
                            {
                                Game1.playSound(customPlaceItemSound ?? "stoneStep");
                            }
                        }
                        Item itemR = Utility.addItemToInventory(_heldItem, HoveredSlotIndex, actualInventory);
                        return itemR;
                    }
                    if (_heldItem is null)
                    {
                        if (playSound)
                        {
                            if (playSoundInLocation)
                            {
                                Game1.sounds.PlayAll("dwop", Game1.player.currentLocation, Game1.player.Tile, null, context: StardewValley.Audio.SoundContext.Default);
                            }
                            else
                            {
                                Game1.playSound("dwop");
                            }
                        }
                        Item itemR2 = Utility.removeItemFromInventory(HoveredSlotIndex, actualInventory);
                        ClickableItemList[HoveredSlotIndex].itemScale = itemScaleM;
                        return itemR2;
                    }
                }

                else if (_heldItem is not null && isExclusiveType && isExclusiveID)
                {
                    bool isBackpack = _heldItem.QualifiedItemId.Equals(SmartPet.PetBackpackQUID) && isPocketSlotItem;
                    if (playSound)
                    {
                        if (playSoundInLocation)
                        {
                            Game1.sounds.PlayAll(customPlaceItemSound ?? "stoneStep", Game1.player.currentLocation, Game1.player.Tile, null, context: StardewValley.Audio.SoundContext.Default);
                        }
                        else
                        {
                            Game1.playSound(customPlaceItemSound ?? "stoneStep");
                        }
                    }
                    if (isBackpack)
                    {
                        actualInventory[HoveredSlotIndex] = ItemRegistry.Create(SmartPet.PetBackpackQUID, 1, 0);
                        ClickableItemList[HoveredSlotIndex].itemScale = itemScaleM;
                        return _heldItem.ConsumeStack(1);
                    }
                    Item itemR3 = Utility.addItemToInventory(_heldItem, HoveredSlotIndex, actualInventory);
                    ClickableItemList[HoveredSlotIndex].itemScale = itemScaleM;
                    return itemR3;
                }
            }
            return _heldItem;
        }

        public void Update(GameTime time,Item _heldItem = null)
        {
            for (int i = 0; i < this.ClickableItemList.Count; i++)
            {
                if (this.ClickableItemList[i] is not null)
                {
                    if (i < this.actualInventory.Count)
                    {
                        ClickableItemList[i].item = this.actualInventory[i];
                    }
                    this.ClickableItemList[i].itemScale = Math.Max(this.itemScaleM, this.ClickableItemList[i].itemScale - 0.025f);
                }
            }
            bool isExclusiveType = (this.ExclusiveItemType is not null && _heldItem is not null && _heldItem.GetType() == this.ExclusiveItemType) || this.ExclusiveItemType is null;
            bool isExclusiveID = (this.ExclusiveForIds is not null && _heldItem is not null && this.ExclusiveForIds.Length > 0 && this.ExclusiveForIds.Contains(_heldItem.QualifiedItemId)) || this.ExclusiveForIds is null;
            if (!this.DontMoveItems && this.HoveredSlotIndex > -1 && (_heldItem is null || (isExclusiveType && isExclusiveID)))
            {
                this.ClickableItemList[HoveredSlotIndex].itemScale = Math.Min(this.ClickableItemList[HoveredSlotIndex].itemScale + 0.05f, this.itemScaleM + 0.1f);
            }
        }
        public void Hover(int x, int y)
        {
            this.HoveredSlotIndex = -1;

            for (int i = 0; i < this.ClickableItemList.Count; i++)
            {
                if (this.ClickableItemList[i] is not null && this.ClickableItemList[i].bounds.Contains(x, y))
                {
                    this.HoveredSlotIndex = i;
                }
            }
        }

        /// <returns>Whether position went out of bounds</returns>
        public bool MoveToRightSlot(bool playSound = true, bool playSoundWhenOutOfBounds = false)
        {
            int prevValue = HoveredSlotIndex;
            int newValue = HoveredSlotIndex + 1 < ClickableItemList.Count && (HoveredSlotIndex % columns) + 1 < columns ? HoveredSlotIndex + 1 : HoveredSlotIndex;
            if (playSound)
            {
                if ((newValue == prevValue && playSoundWhenOutOfBounds) || newValue != prevValue) { Game1.playSound("shiny4"); }
            }
            HoveredSlotIndex = newValue;
            return newValue == prevValue;
        }
        /// <returns>Whether position went out of bounds</returns>
        public bool MoveToUpSlot(bool playSound = true, bool playSoundWhenOutOfBounds = false)
        {
            int prevValue = HoveredSlotIndex;
            int newValue = HoveredSlotIndex >= columns && ClickableItemList.Count > HoveredSlotIndex ? Math.Max(HoveredSlotIndex - columns, 0) : HoveredSlotIndex;
            if (playSound)
            {
                if ((newValue == prevValue && playSoundWhenOutOfBounds) || newValue != prevValue) { Game1.playSound("shiny4"); }
            }
            HoveredSlotIndex = newValue;
            return newValue == prevValue;
        }
        /// <returns>Whether position went out of bounds</returns>
        public bool MoveToLeftSlot(bool playSound = true, bool playSoundWhenOutOfBounds = false)
        {
            int prevValue = HoveredSlotIndex;
            int newValue = (HoveredSlotIndex % columns) - 1 >= 0 && ClickableItemList.Count > HoveredSlotIndex ? HoveredSlotIndex - 1 : HoveredSlotIndex;
            if (playSound)
            {
                if ((newValue == prevValue && playSoundWhenOutOfBounds) || newValue != prevValue) { Game1.playSound("shiny4"); }
            }
            HoveredSlotIndex = newValue;
            return newValue == prevValue;
        }
        /// <returns>Whether position went out of bounds</returns>
        public bool MoveToDownSlot(bool playSound = true, bool playSoundWhenOutOfBounds = false, int dontMoveBeyondMax = -1)
        {
            int prevValue = HoveredSlotIndex;
            int newValue = HoveredSlotIndex + columns < ClickableItemList.Count ? Math.Min(HoveredSlotIndex + columns, ClickableItemList.Count - 1) : HoveredSlotIndex;
            
            if (dontMoveBeyondMax < 0 || (dontMoveBeyondMax >= 0 && newValue < dontMoveBeyondMax))
            {
                if (playSound)
                {
                    if ((newValue == prevValue && playSoundWhenOutOfBounds) || newValue != prevValue) { Game1.playSound("shiny4"); }
                }
                HoveredSlotIndex = newValue;
            }
            return newValue == prevValue;
        }

        public Item TryAddItemSomewhereInsideTheInventory(Item _item,int listMaxSpace = -1, string playCustomSound = null,bool playsoundAtLocation = false, bool onlyTakeOne = false)
        {
            if (_item is null) { return _item; }
            bool isExclusiveType = (this.ExclusiveItemType is not null && _item.GetType() == this.ExclusiveItemType) || this.ExclusiveItemType is null;
            bool isExclusiveID = (this.ExclusiveForIds is not null && this.ExclusiveForIds.Length > 0 && this.ExclusiveForIds.Contains(_item.QualifiedItemId)) || this.ExclusiveForIds is null;
            if (!DontMoveItems && isExclusiveType && isExclusiveID)
            {
                bool itemAdded = false;
                Item result = null;
                if (onlyTakeOne)
                {
                    result = Utility.addItemToThisInventoryList(_item.getOne(), this.actualInventory, listMaxSpace);
                    if (result is null) { _item.ConsumeStack(1); itemAdded = true; }
                    result = _item;
                }
                else
                {
                    int itemStack = _item.Stack;
                    result = Utility.addItemToThisInventoryList(_item, this.actualInventory, listMaxSpace);
                    itemAdded = result is null || result.Stack != itemStack;
                }
                if (itemAdded)
                {
                    if (playsoundAtLocation) { PetHelper.PlaySoundForAllPlayersAtFarmerLocation(playCustomSound ?? "stoneStep", Game1.player.Tile, Game1.player); }
                    else { Game1.playSound(playCustomSound ?? "stoneStep"); }
                }
                return result;
            }
            return _item;
        }

        public static void DrawToolTip(SpriteBatch b, string hoverText, string hoverTitle, Item hoveredItem, bool heldItem = false, int healAmountToDisplay = -1, int currencySymbol = 0, string extraItemToShowIndex = null, int extraItemToShowAmount = -1, CraftingRecipe craftingIngredients = null, int moneyAmountToShowAtBottom = -1, IList<Item> additionalCraftMaterials = null, int _overrideX = -1, int _overrideY = -1)
        {
            bool flag = hoveredItem is StardewValley.Object @object && @object.Edibility != -300;
            string[] array = null;
            if (flag && Game1.objectData.TryGetValue(hoveredItem.ItemId, out var value))
            {
                BuffEffects buffEffects = new BuffEffects();
                int num = int.MinValue;
                foreach (Buff item in StardewValley.Object.TryCreateBuffsFromData(value, hoveredItem.Name, hoveredItem.DisplayName, 1f, hoveredItem.ModifyItemBuffs))
                {
                    buffEffects.Add(item.effects);
                    if (item.millisecondsDuration == -2 || (item.millisecondsDuration > num && num != -2))
                    {
                        num = item.millisecondsDuration;
                    }
                }

                if (buffEffects.HasAnyValue())
                {
                    array = buffEffects.ToLegacyAttributeFormat();
                    if (num != -2)
                    {
                        array[12] = " " + Utility.getMinutesSecondsStringFromMilliseconds(num);
                    }
                }
            }

            IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont, heldItem ? 40 : 0, heldItem ? 40 : 0, moneyAmountToShowAtBottom, hoverTitle, flag ? (hoveredItem as StardewValley.Object).Edibility : (-1), array, hoveredItem, currencySymbol, extraItemToShowIndex, extraItemToShowAmount, _overrideX, _overrideY, 1f, craftingIngredients, additionalCraftMaterials);
        }
    }

    public class ClickableItemRect
    {
        public Item item;

        public Rectangle bounds;

        public float itemScale;

        public ClickableItemRect(Rectangle _bounds, Item item = null, float itemScale = 1f)
        {
            this.item = item;
            this.bounds = _bounds;
            this.itemScale = itemScale;
        }
        public void Draw(SpriteBatch b, float offsetX = 0, float offsetY = 0, float _alpha = 1f, float layerDepth = 0.865f, StackDrawType drawType = StackDrawType.Draw, bool drawShadow = false)
        {
            if (item is not null)
            {
                item.drawInMenu(b, new Vector2(bounds.X + offsetX, bounds.Y + offsetY), itemScale , _alpha, layerDepth, drawType, Color.White, drawShadow);
            }
        }
        public Point GetLowerRightCorner() => bounds.Location + bounds.Size;
    }
}
