using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Extensions;
using System.Text;
using StardewValley.Characters;
using StardewValley.Menus;
using static StardewValley.Minigames.MineCart.Whale;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using StardewValley.GameData.Pets;
using System.Xml.Linq;
using StardewValley.Buildings;
using StardewModdingAPI.Events;
using Pets_Enhanced_Mod.Multiplayer;
using StardewModdingAPI;

namespace Pets_Enhanced_Mod.Utilities.Custom_Classes
{
    public class PetStatusWindow
    {
        public int xPos;
        public int yPos;
        public int width = 660;
        public int height = 536;
        public struct FontProperty
        {
            public int x;
            public int y;
            public int height;
            public int width;
            public float scale;
            public float leftMargin;
            public float rightMargin;
            public FontProperty(int _x, int _y, int _width, int _height, float _scale, float _rightMargin = 0, float _leftMargin = 0)
            {
                this.x = _x;
                this.y = _y;
                this.height = _height;
                this.width = _width;
                this.scale = _scale;
                this.rightMargin = _rightMargin;
                this.leftMargin = _leftMargin;
            }
        }
        /// <summary>
        /// Defines the item currently being focused.
        /// </summary><remarks>0 means no object is feing controlled.
        /// 1 means left bg change arrow.
        /// 2 means right bg change arrow. 
        /// 3 means both bg change arrow. 
        /// 4 means left direction change arrow. 
        /// 5 means right direction change arrow. 
        /// 6 means both direction change arrows.
        /// 7 means gift icon.
        /// 8 means up diet list scroll arrow.
        /// 9 means down diet list scroll arrow.
        /// 10 means close window button.</remarks>
        public int currentControlledObject = 0;
        private readonly Rectangle emptyHeartSrcRect = new(210, 18, 8, 7);
        private readonly Rectangle fullHeartSrcRect = new(201, 18, 8, 7);
        private readonly Rectangle hoveredSkillFrameSrcRect = new(281, 179, 14, 14);
        private readonly Rectangle ExclamationIconSrcRect = new(221, 26, 3, 8);
        private float exclamationIconAlpha1 = 0f;
        private float exclamationIconAlpha2 = 0f;
        private float exclamationIconAlpha3 = 0f;
        private float exclamationIconAlpha4 = 0f;
        private float exclamationIconAlpha5 = 0f;
        public readonly SpriteRectangle LeftBGChangeArrow;
        public readonly SpriteRectangle RightBGChangeArrow;
        public readonly SpriteRectangle RightRotationArrow;
        public readonly SpriteRectangle LeftRotationArrow;
        public readonly SpriteRectangle CloseStatusBarRectangle;
        private readonly SpriteRectangle WaitingSkillRectangle;
        private readonly SpriteRectangle FollowSkillRectangle;
        private readonly SpriteRectangle ForageSkillRectangle;
        private readonly SpriteRectangle FishingSkillRectangle;
        private readonly SpriteRectangle HuntingSkillRectangle;
        public bool MouseMoved = false;
        private readonly SpriteInfo fHeart1;
        private readonly SpriteInfo fHeart2;
        private readonly SpriteInfo fHeart3;
        private readonly SpriteInfo fHeart4;
        private readonly SpriteInfo fHeart5;
        private readonly StringSpriteInfo NameSprite;
        private SpriteInfo? hoveredSkillFrame;
        public FontProperty AgeText;
        public FontProperty DateText;
        public int DateNumber;
        public FontProperty StatTitle;
        public int StatUnderlineType;
        public FontProperty SkillTitle;
        public int SkillUnderlineType;
        public float ScrollButtonScale = 1f;
        public float ScrollUpButtonScale = 1f;
        public float ScrollDownButtonScale = 1f;
        public bool HasWaitSkill = false;
        public bool HasFollowSkill = false;
        public bool HasForageSkill = false;
        public bool HasFishingSkill = false;
        public bool HasHuntSkill = false;
        public Vector2 MousePos;
        /// <summary>Acronym for: Scroll Opening Unit Percentage</summary>
        public int soup = 0;
        public int soupStage = 0;
        public int elapsedTime = 0;
        public int currentScrollYIndex = 0;
        public Texture2D itemsOnScrollTexture;
        public Texture2D PetTexture;
        public double[] SkillMastery_level = new[] { -1d, -1d, -1d, -1d, -1d };
        public uint SkillPerkTierChecklist = 0;
        public bool HasAllThreeMainSkills;
        private string DmgStat = "";
        private string CooldownStat = "";
        private string CritStat = "";
        public PetInfo.Pet_Types PetType;
        public bool IsTurtle = false;
        public int petFacingDirection = 2;
        public float[] CurrentSkillPerkScale = new[] { 1f, 1f, 1f, 1f };
        private int HoveredSkillPerk = -1;
        private int animatingSkillPerkUnlock = -1;
        private int animatingSkillPerkUnlockElapsedTime = 0;
        private (Vector2 pos, Vector2 basePos, bool unlocked, float alpha, Vector2 randomVelocity)? skillPerkUnlockSpriteData;
        private float currentSkillPerkAlpha = 1f;

        private bool IsSkillPerkUnlocked(int sk, int skP) => sk >= 0 && skP >= 0 && SkillMastery_level[sk] >= skP + 2 && SynchronizationManager.GetBoolByte(this.SkillPerkTierChecklist, sk, skP);
        private bool IsSkillPerkReadyNotUnlocked(int sk, int skP) => sk >= 0 && skP >= 0 && SkillMastery_level[sk] >= skP + 2 && !SynchronizationManager.GetBoolByte(this.SkillPerkTierChecklist, sk, skP);
        private bool IsSkillPerkReadyToBeUnlocked(int sk, int skP) => sk >= 0 && skP >= 0 && SkillMastery_level[sk] >= skP + 2;
        public PetStatusWindow(Pet sourcePet, LocalizedContentManager.LanguageCode _language, Texture2D mainTexture, Texture2D petTexture,bool mouseMoved, string collarInUse, StardewValley.Inventories.Inventory inventoryRef)
        {
            bool isZh = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh;
            var pos = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
            this.xPos = (int)pos.X;
            this.yPos = (int)pos.Y;

            var petTempData = CachePetData.GetPetDataForPet(sourcePet);
            SynchronizationManager.TryParseModData(sourcePet.modData, SynchronizationManager.PetModDataKey_PetAge, out int _petTempAge);
            var skillMasteryTemp = new SynchronizationManager.SkillMasteryLevelStruct(sourcePet.modData);

            this.SetUpLanguageChanges(_language, _petTempAge);
            this.CreateItemScrollAtlasTexture(CachePetData.GetDietListFromID(petTempData.DietListID), mainTexture, inventoryRef,petTempData.TrickLearningTreat); //change this later
            this.SkillMastery_level = new double[5] { skillMasteryTemp.WaitingSkillMastery, skillMasteryTemp.FollowingSkillMastery, skillMasteryTemp.ForagingSkillMastery, skillMasteryTemp.FishingSkillMastery, skillMasteryTemp.HuntingSkillMastery };
            this.HasAllThreeMainSkills = (SkillMastery_level[2] >= 1 && petTempData.HasForageSkill) && (SkillMastery_level[3] >= 1 && petTempData.HasFishingSkill) && (SkillMastery_level[4] >= 1 && petTempData.HasHuntSkill);
            this.MouseMoved = mouseMoved;
            if (!MouseMoved) { this.currentControlledObject = 7; }

            SynchronizationManager.TryParseModData(sourcePet.modData, SynchronizationManager.PetModDataKey_SkillPerkTierChecklist, out SkillPerkTierChecklist);
            this.PetType = PetHelper.GetPetTypeFromTextureHeightAndOpetType(sourcePet.petType?.Value, sourcePet.Sprite.Texture.Height);
            SetupStatStrings(this.SkillMastery_level[4], this.SkillMastery_level[1], petTempData.BaseCooldownTime, petTempData.MaxDamage, petTempData.CritChance, collarInUse == SmartPet.RoughCollarQUID, collarInUse == SmartPet.LightweightCollarQUID, this.SkillMastery_level[0] >= 5);
            this.PetTexture = petTexture;
            this.CloseStatusBarRectangle = new(this.xPos + 624, this.yPos - 12, 4f, 208, 26, 12, 12);

            this.IsTurtle = sourcePet.petType is not null && (sourcePet.petType.Contains("turtle") || sourcePet.petType.Contains("Turtle"));
            this.LeftBGChangeArrow = new(xPos + 108, yPos + 76, 4f, 218, 17, 8, 8);
            this.RightBGChangeArrow = new(xPos + 204, yPos + 76, 4f, 218, 17, 8, 8);
            this.LeftRotationArrow = new(xPos + 32, yPos + 172, 4f, 212, 0, 12, 10);
            this.RightRotationArrow = new(xPos + 264, yPos + 172, 4f, 212, 0, 12, 10);

            this.HasWaitSkill = petTempData.HasWaitSkill;
            this.HasFollowSkill = petTempData.HasFollowSkill;
            this.HasForageSkill = petTempData.HasForageSkill;
            this.HasFishingSkill = petTempData.HasFishingSkill;
            this.HasHuntSkill = petTempData.HasHuntSkill;

            int yOffset = isZh ? 0 : -4;
            if (SkillMastery_level[0] >= 1 && petTempData.HasWaitSkill)
            {
                this.WaitingSkillRectangle = new(xPos + ((this.HasAllThreeMainSkills ? 86 : 91) * 4), yPos + 356 + yOffset, 4f, 473, 193, 14, 14);
            }
            if (SkillMastery_level[1] >= 1 && petTempData.HasFollowSkill)
            {
                this.FollowSkillRectangle = new(xPos + ((this.HasAllThreeMainSkills ? 144 : 139) * 4), yPos + 356 + yOffset, 4f, 487, 193, 14, 14);
            }
            if (SkillMastery_level[2] >= 1 && petTempData.HasForageSkill)
            {
                this.ForageSkillRectangle = new(xPos + ((this.HasAllThreeMainSkills ? 102 : 108) * 4), yPos + 352 + yOffset, 4f, 501, 193, 14, 14);
            }
            if (SkillMastery_level[3] >= 1 && petTempData.HasFishingSkill)
            {
                this.FishingSkillRectangle = new(xPos + ((this.HasAllThreeMainSkills ? 128 : this.SkillMastery_level[2] >= 1 ? 122 : 108) * 4), yPos + 352 + yOffset, 4f, 515, 193, 14, 14);
            }
            if (SkillMastery_level[4] >= 1 && petTempData.HasHuntSkill)
            {
                this.HuntingSkillRectangle = new(xPos + ((this.HasAllThreeMainSkills ? 115 : 122) * 4), yPos + 352 + yOffset, 4f, 529, 193, 14, 14);
            }

            int num = yPos + 356;
            fHeart1 = new(new Vector2(xPos + 76, num), sourcePet.friendshipTowardFarmer.Value >= 200 ? fullHeartSrcRect : emptyHeartSrcRect);
            fHeart2 = new(new Vector2(xPos + 116, num), sourcePet.friendshipTowardFarmer.Value >= 400 ? fullHeartSrcRect : emptyHeartSrcRect);
            fHeart3 = new(new Vector2(xPos + 156, num), sourcePet.friendshipTowardFarmer.Value >= 600 ? fullHeartSrcRect : emptyHeartSrcRect);
            fHeart4 = new(new Vector2(xPos + 196, num), sourcePet.friendshipTowardFarmer.Value >= 800 ? fullHeartSrcRect : emptyHeartSrcRect);
            fHeart5 = new(new Vector2(xPos + 236, num), sourcePet.friendshipTowardFarmer.Value >= 1000 ? fullHeartSrcRect : emptyHeartSrcRect);

            float scaleLang = 0.74f;
            int yLangOffset = 0;
            switch (LocalizedContentManager.CurrentLanguageCode)
            {
                case LocalizedContentManager.LanguageCode.zh: scaleLang = 0.8f; break;
                case LocalizedContentManager.LanguageCode.ko: scaleLang = 0.6f; break;
                case LocalizedContentManager.LanguageCode.ja: yLangOffset = 8; break;
                case LocalizedContentManager.LanguageCode.ru or LocalizedContentManager.LanguageCode.de: scaleLang = 1f; yLangOffset = 2; break;
            }
            NameSprite = new(new Vector2(xPos + 172 - ((Game1.dialogueFont.MeasureString(sourcePet.Name).X / 2) * scaleLang), yPos + 305 + yLangOffset), sourcePet.Name, new Color(113, 37, 24), scaleLang);
        }
        private void SetupStatStrings(double _HuntSkillPercentage, double _followSkillPercentage, float _baseCooldownTime, int _maxDamage, float _critChance, bool hasRoughCollar, bool hasLightweightCollar, bool isLucky)
        {
            int hPAsInt = (int)_HuntSkillPercentage;
            double cooldownModifier = 1d + (_followSkillPercentage >= 3 ? -15 : 0) + (hPAsInt >= 2 ? -10 : 0) + (hPAsInt >= 4 ? -15 : 0) + (hasLightweightCollar ? -15 : 0);
            int attackCooldown = (int)(_baseCooldownTime * (100 + cooldownModifier));
            this.CooldownStat = attackCooldown < 100 ? $"0.{attackCooldown}s" : attackCooldown.ToString().Insert($"{attackCooldown}".Length - 2, ".") + "s";

            double MaxDamage = hPAsInt <= 1 ? _maxDamage : _maxDamage * (2.14d * hPAsInt);
            string s = hasRoughCollar ? " + 25%" : "";
            this.DmgStat = (int)MaxDamage + s;

            int critChanceModifiers = (hPAsInt >= 3 ? 25 : 0) + (hasRoughCollar ? 10 : 0) + (isLucky ? 7 : 0);
            string c = critChanceModifiers > 0 ? $"% + {critChanceModifiers}%" : "%";
            this.CritStat = (int)(_critChance * 100) + c;
        }
        private void SetUpLanguageChanges(LocalizedContentManager.LanguageCode l, int _age)
        {
            int dateType = _age >= 224 ? 5 : _age >= 112 ? 4 : _age >= 56 ? 3 : _age >= 28 ? 2 : _age > 1 ? 1 : 0; //Years, year, Months, Month, Days, Day

            var ageText = new FontProperty(389, 1, 17, 10, 1);
            var statTitle = new FontProperty(239, 1, 74, 8, 1);
            var skillTitle = new FontProperty(314, 1, 74, 11, 1);
            var dateText = new FontProperty(407, 1, 14, 10, 1);
            int skillUnderlineType = 0;
            int statUnderlineType = 0;
            switch (l)
            {
                case LocalizedContentManager.LanguageCode.es:
                    {
                        ageText.width = 21;
                        ageText.y = dateText.y = 12;
                        dateText.x = dateType > 4 ? 495 : dateType > 3 ? 480 : dateType > 2 ? 456 : dateType > 1 ? 441 : dateType > 0 ? 424 : 411;
                        dateText.width = dateType == 5 ? 18 : dateType == 4 || dateType == 2 ? 14 : dateType == 3 ? 23 : dateType == 1 ? 16 : 12;
                        statTitle.y = 10;
                        skillTitle.y = 13;
                        statUnderlineType = 2;
                        skillUnderlineType = 2;
                    }
                    break;
                case LocalizedContentManager.LanguageCode.pt:
                    {
                        ageText.width = 26;
                        ageText.y = dateText.y = 23;
                        dateText.x = dateType > 4 ? 500 : dateType > 3 ? 485 : dateType > 2 ? 461 : dateType > 1 ? 446 : dateType > 0 ? 429 : 416;
                        dateText.width = dateType == 5 ? 18 : dateType == 4 || dateType == 2 ? 14 : dateType == 3 ? 23 : dateType == 1 ? 16 : 12;
                        statTitle.y = 19;
                        skillTitle.y = 25;
                        statUnderlineType = 2;
                        skillUnderlineType = 2;
                    }
                    break;
                case LocalizedContentManager.LanguageCode.de:
                    {
                        ageText.width = 22;
                        ageText.y = dateText.y = 34;
                        dateText.x = dateType > 4 ? 519 : dateType > 3 ? 500 : dateType > 2 ? 470 : dateType > 1 ? 445 : dateType > 0 ? 426 : 412;
                        dateText.width = dateType == 5 ? 23 : dateType == 4 ? 18 : dateType == 3 ? 29 : dateType == 2 ? 24 : dateType == 1 ? 18 : 13;
                        statTitle.y = 28;
                        skillTitle.y = 37;
                        statUnderlineType = 2;
                        skillUnderlineType = 1;
                    }
                    break;
                case LocalizedContentManager.LanguageCode.fr:
                    {
                        ageText.width = 17;
                        ageText.y = dateText.y = 45;
                        dateText.x = dateType > 4 ? 474 : dateType > 3 ? 464 : dateType > 2 ? 447 : dateType > 1 ? 447 : dateType > 0 ? 425 : 407;
                        dateText.width = dateType == 5 ? 13 : dateType == 4 ? 9 : dateType == 3 ? 16 : dateType == 2 ? 16 : dateType == 1 ? 21 : 17;
                        statTitle.y = 37;
                        skillTitle.y = 49;
                        statUnderlineType = 2;
                        skillUnderlineType = 2;
                    }
                    break;
                case LocalizedContentManager.LanguageCode.it:
                    {
                        ageText.width = 16;
                        ageText.y = dateText.y = 56;
                        dateText.x = dateType > 4 ? 512 : dateType > 3 ? 492 : dateType > 2 ? 475 : dateType > 1 ? 455 : dateType > 0 ? 432 : 406;
                        dateText.width = dateType == 5 ? 16 : dateType == 4 ? 19 : dateType == 3 ? 16 : dateType == 2 ? 19 : dateType == 1 ? 22 : 25;
                        statTitle.y = 46;
                        skillTitle.y = 61;
                        statUnderlineType = 2;
                    }
                    break;
                case LocalizedContentManager.LanguageCode.tr:
                    {
                        ageText.width = 17;
                        ageText.y = dateText.y = 67;
                        dateText.x = dateType > 4 ? 432 : dateType > 3 ? 432 : dateType > 2 ? 422 : dateType > 1 ? 422 : dateType > 0 ? 407 : 407;
                        dateText.width = dateType == 5 ? 9 : dateType == 4 ? 9 : dateType == 3 ? 9 : dateType == 2 ? 9 : dateType == 1 ? 14 : 14;
                        statTitle.y = 55;
                        skillTitle.y = 73;
                        statUnderlineType = 2;
                        skillUnderlineType = 1;
                    }
                    break;
                case LocalizedContentManager.LanguageCode.hu:
                    {
                        ageText.width = 16;
                        ageText.y = dateText.y = 78;
                        dateText.x = dateType > 4 ? 448 : dateType > 3 ? 448 : dateType > 2 ? 422 : dateType > 1 ? 422 : dateType > 0 ? 406 : 406;
                        dateText.width = dateType == 5 ? 10 : dateType == 4 ? 10 : dateType == 3 ? 25 : dateType == 2 ? 25 : dateType == 1 ? 15 : 15;
                        statTitle.y = 64;
                        skillTitle.y = 85;
                        statUnderlineType = 3;
                        skillUnderlineType = 1;
                    }
                    break;
                case LocalizedContentManager.LanguageCode.ru:
                    {
                        ageText.width = 33;
                        ageText.y = dateText.y = 89;
                        dateText.x = dateType > 4 ? 526 : dateType > 3 ? 513 : dateType > 2 ? 483 : dateType > 1 ? 458 : dateType > 0 ? 443 : 423;
                        dateText.width = dateType == 5 ? 18 : dateType == 4 ? 12 : dateType == 3 ? 29 : dateType == 2 ? 24 : dateType == 1 ? 14 : 19;
                        statTitle.y = 73;
                        skillTitle.y = 97;
                        statUnderlineType = 2;
                    }
                    break;
                case LocalizedContentManager.LanguageCode.ja:
                    {
                        ageText.width = 33;
                        ageText.y = dateText.y = 100;
                        dateText.x = dateType > 4 ? 443 : dateType > 3 ? 443 : dateType > 2 ? 430 : dateType > 1 ? 430 : dateType > 0 ? 423 : 423;
                        dateText.width = dateType == 5 ? 7 : dateType == 4 ? 7 : dateType == 3 ? 12 : dateType == 2 ? 12 : dateType == 1 ? 6 : 6;
                        statTitle.y = 82;
                        skillTitle.y = 109;
                        statUnderlineType = 1;
                    }
                    break;
                case LocalizedContentManager.LanguageCode.ko:
                    {
                        ageText.width = 16;
                        ageText.y = dateText.y = 111;
                        dateText.x = dateType > 4 ? 431 : dateType > 3 ? 431 : dateType > 2 ? 414 : dateType > 1 ? 414 : dateType > 0 ? 406 : 406;
                        dateText.width = dateType == 5 ? 8 : dateType == 4 ? 8 : dateType == 3 ? 16 : dateType == 2 ? 16 : dateType == 1 ? 7 : 7;
                        statTitle.y = 91;
                        skillTitle.y = 121;
                    }
                    break;
                case LocalizedContentManager.LanguageCode.zh:
                    {
                        ageText.width = 23;
                        ageText.y = dateText.y = 122;
                        dateText.x = dateType > 4 ? 435 : dateType > 3 ? 435 : dateType > 2 ? 421 : dateType > 1 ? 421 : dateType > 0 ? 413 : 413;
                        dateText.width = dateType == 5 ? 7 : dateType == 4 ? 7 : dateType == 3 ? 13 : dateType == 2 ? 13 : dateType == 1 ? 7 : 7;
                        statTitle.y = 100;
                        statTitle.height = 13;
                        statTitle.scale = 0.8076923076f; //Aka 10.5
                        skillTitle.y = 133;
                        statUnderlineType = 1;
                    }
                    break;
                default:
                    {
                        if (LocalizedContentManager.CurrentLanguageString == "th")
                        {
                            ageText.width = 18;
                            ageText.y = dateText.y = 133;
                            dateText.x = dateType > 4 ? 437 : dateType > 3 ? 437 : dateType > 2 ? 418 : dateType > 1 ? 418 : dateType > 0 ? 408 : 408;
                            dateText.width = dateType == 5 ? 5 : dateType == 4 ? 5 : dateType == 3 ? 18 : dateType == 2 ? 18 : dateType == 1 ? 9 : 9;
                            statTitle.y = 114;
                            skillTitle.y = 145;
                            statUnderlineType = 1;
                        }
                        else if (LocalizedContentManager.CurrentLanguageString == "vi")
                        {
                            ageText.width = 18;
                            ageText.y = dateText.y = 144;
                            dateText.x = dateType > 4 ? 453 : dateType > 3 ? 453 : dateType > 2 ? 428 : dateType > 1 ? 428 : dateType > 0 ? 408 : 408;
                            dateText.width = dateType == 5 ? 16 : dateType == 4 ? 16 : dateType == 3 ? 24 : dateType == 2 ? 24 : dateType == 1 ? 19 : 19;
                            statTitle.y = 123;
                            skillTitle.y = 157;
                            statUnderlineType = 2;
                        }
                        else
                        {
                            dateText.x = dateType > 4 ? 515 : dateType > 3 ? 495 : dateType > 2 ? 466 : dateType > 1 ? 441 : dateType > 0 ? 422 : 407;
                            dateText.width = dateType == 5 ? 23 : dateType == 4 ? 19 : dateType == 3 ? 28 : dateType == 2 ? 24 : dateType == 1 ? 18 : 14;
                        }
                    }
                    break;
            }
            this.DateNumber = dateType > 3 ? (int)_age / 112 : dateType > 1 ? (int)_age / 28 : _age;
            this.DateText = dateText;
            this.StatTitle = statTitle;
            this.StatUnderlineType = statUnderlineType;
            this.SkillTitle = skillTitle;
            this.SkillUnderlineType = skillUnderlineType;
            this.AgeText = ageText;
        }

        private void CreateItemScrollAtlasTexture(Dictionary<string, (int FriendshipGain, int EnergyGain)> _dietList, Texture2D texture, StardewValley.Inventories.Inventory inventoryRef, string trickLearningTreatQUID)
        {
            Texture2D texture1 = new(Game1.graphics.GraphicsDevice, 248, 68 + (68 * (int)(_dietList.Count / 4)));
            Color[] ogData = new Color[texture1.Width * texture1.Height];

            Color[] shadowData = new Color[12 * 8];
            texture.GetData(0, new(226, 19, 12, 8), shadowData, 0, shadowData.Length);
            var shadowCo = ScaleColorArray(shadowData, 12, 8, 36, 24);

            Color[] notKnownItemData = new Color[6 * 9];
            texture.GetData(0, new(201, 26, 6, 9), notKnownItemData, 0, notKnownItemData.Length);
            var notKnownItemCo = ScaleColorArray(notKnownItemData, 6, 9, 18, 27);

            Color[] minusEData = new Color[5 * 5];
            texture.GetData(0, new(213, 12, 5, 5), minusEData, 0, minusEData.Length);
            var minusECo = ScaleColorArray(minusEData, 5, 5, 20, 20);
            Color[] plusEData = new Color[5 * 5];
            texture.GetData(0, new(219, 12, 5, 5), plusEData, 0, plusEData.Length);
            var plusECo = ScaleColorArray(plusEData, 5, 5, 20, 20);
            Color[] favPData = new Color[5 * 5];
            texture.GetData(0, new(201, 12, 5, 5), favPData, 0, favPData.Length);
            var favPCo = ScaleColorArray(favPData, 5, 5, 20, 20);
            Color[] favNData = new Color[5 * 5];
            texture.GetData(0, new(207, 12, 5, 5), favNData, 0, favNData.Length);
            var favNCo = ScaleColorArray(favNData, 5, 5, 20, 20);

            var _dietListArray = _dietList.ToArray();
            int i = 0;
            while (i < _dietListArray.Length)
            {
                ogData = MergeColorArrays(ogData, shadowCo, texture1.Width, texture1.Height, 9 + (64 * (i % 4)), 44 + (66 * (int)(i / 4)), 36, 24);
                if (!string.IsNullOrEmpty(_dietListArray[i].Key) && ((_dietListArray[i].Key == trickLearningTreatQUID) || SynchronizationManager.IsItemQUIDKnownInInventoryBook(inventoryRef,_dietListArray[i].Key)))
                {
                    var it = ItemRegistry.GetDataOrErrorItem(_dietListArray[i].Key);
                    Rectangle s = it.GetSourceRect();
                    Color[] dataB = new Color[s.Width * s.Height];
                    it.GetTexture().GetData(0, s, dataB, 0, dataB.Length);
                    ogData = MergeColorArrays(ogData, ScaleColorArray(dataB, s.Width, s.Height, 48, 48), texture1.Width, texture1.Height, 4 + (64 * (i % 4)), 16 + (66 * (int)(i / 4)), 48, 48);

                    if (_dietListArray[i].Value.EnergyGain > 0)
                    {
                        ogData = MergeColorArrays(ogData, plusECo, texture1.Width, texture1.Height, 36 + (64 * (i % 4)), 44 + (66 * (int)(i / 4)), 20, 20);
                    }
                    else if (_dietListArray[i].Value.EnergyGain < 0)
                    {
                        ogData = MergeColorArrays(ogData, minusECo, texture1.Width, texture1.Height, 36 + (64 * (i % 4)), 44 + (66 * (int)(i / 4)), 20, 20);
                    }
                    if (_dietListArray[i].Value.FriendshipGain > 0)
                    {
                        ogData = MergeColorArrays(ogData, favPCo, texture1.Width, texture1.Height, (64 * (i % 4)), 12 + (66 * (int)(i / 4)), 20, 20);
                    }
                    else if (_dietListArray[i].Value.FriendshipGain < 0)
                    {
                        ogData = MergeColorArrays(ogData, favNCo, texture1.Width, texture1.Height, (64 * (i % 4)), 12 + (66 * (int)(i / 4)), 20, 20);
                    }
                }
                else
                {
                    ogData = MergeColorArrays(ogData, notKnownItemCo, texture1.Width, texture1.Height, 18 + (64 * (i % 4)), 14 + (66 * (int)(i / 4)), 18, 27);
                }

                i++;
            }
            texture1.SetData(0, new Rectangle(0, 0, texture1.Width, texture1.Height), ogData, 0, ogData.Length);
            itemsOnScrollTexture = texture1;
        }
        public Color[] MergeColorArrays(Color[] destination, Color[] source, int destWidth, int destHeight, int rectX, int rectY, int rectWidth, int rectHeight)
        {
            Color[] returnValue = destination;
            int y = 0;
            while (y < rectHeight)
            {
                int x = 0;
                while (x < rectWidth)
                {
                    int destIndex = (rectY + y) * destWidth + (rectX + x);
                    int sourceIndex = y * rectWidth + x;
                    var sI = source[sourceIndex];
                    if (sI.A > 0)
                    {
                        returnValue[destIndex] = sI;
                    }
                    x++;
                }
                y++;
            }
            return returnValue;
        }
        public static Color[] ScaleColorArray(Color[] original, int oWidth, int oHeight, int newWidth, int newHeight)
        {
            Color[] scaledPixels = new Color[newWidth * newHeight];
            int y = 0;
            while (y < newHeight)
            {
                int x = 0;
                while (x < newWidth)
                {
                    int origX = x * oWidth / newWidth;
                    int origY = y * oHeight / newHeight;
                    Color originalColor = original[origY * oWidth + origX];
                    scaledPixels[y * newWidth + x] = originalColor;
                    x++;
                }
                y++;
            }

            return scaledPixels;
        }
        private Texture2D CropTexture(Texture2D t, Rectangle s)
        {
            Texture2D croppedTexture = new(Game1.graphics.GraphicsDevice, s.Width, s.Height);
            Color[] data = new Color[s.Width * s.Height];
            t.GetData(0, s, data, 0, data.Length);
            croppedTexture.SetData(0, new Rectangle(0, 0, s.Width, s.Height), data, 0, data.Length);
            return croppedTexture;
        }
        public static double GetPulsatingScale(int deltaTime)
        {
            int normalizedDeltaTime = deltaTime + 1500;
            return (normalizedDeltaTime <= 1500? ((double)normalizedDeltaTime / 1500): 1.0d - (double)(normalizedDeltaTime - 1500) / 1500) * 0.055d;
        }
        private int HoveredSkill = -1;
        private Vector2 currentMouseLocation = Vector2.Zero;
        private bool hoveringWindowLeftSide = false;
        public void Hover(int _x, int _y)
        {
            currentMouseLocation = new(_x, _y);

            float giftIconY = yPos + 456 - this.soup;
            bool scrollIsFullyClosed = this.soup == 0;
            bool scrollIsNotAtTopOfList = (this.soup == 476 && this.currentScrollYIndex > 0);
            bool scrollIsNotAtBottom = (this.soup == 476 && this.itemsOnScrollTexture.Height > 476 && ((float)this.itemsOnScrollTexture.Height - (this.currentScrollYIndex + 490) > 0));
            bool scrollIsNotMoving = this.soup == 476 || this.soup == 0;
            hoveringWindowLeftSide = _x > xPos + 20 && _y > yPos + 20 && _x <= xPos + 324 && _y <= yPos + 516;
            int prevControlledObject = this.currentControlledObject;
            if (LeftBGChangeArrow.Hovering(_x, _y, scrollIsFullyClosed))
            {
                this.currentControlledObject = 1;
            }
            else if (RightBGChangeArrow.Hovering(_x, _y, scrollIsFullyClosed))
            {
                this.currentControlledObject = 2;
            }
            else if (LeftRotationArrow.Hovering(_x, _y, this.canChangeDirection && scrollIsFullyClosed))
            {
                this.currentControlledObject = 4;
            }
            else if (RightRotationArrow.Hovering(_x, _y, this.canChangeDirection && scrollIsFullyClosed))
            {
                this.currentControlledObject = 5;
            }
            else if (scrollIsNotMoving && _x > xPos + 148 && _y > giftIconY && _x <= xPos + 196 && _y <= giftIconY + 52)
            {
                this.currentControlledObject = 7; //Gift icon
            }
            else if (scrollIsNotAtTopOfList && _x > xPos + 148 && _y > yPos + 48 && _x <= xPos + 148 + 44 && _y <= yPos + 48 + 32)
            {
                this.currentControlledObject = 8; //Scroll up arrow
            }
            else if (scrollIsNotAtBottom && _x > xPos + 148 && _y > yPos + 468 && _x <= xPos + 148 + 44 && _y <= yPos + 468 + 32)
            {
                this.currentControlledObject = 9; //Scroll down arrow
            }
            else if (CloseStatusBarRectangle.Hovering(_x, _y))
            {
                this.currentControlledObject = 10;
            }
            else { this.currentControlledObject = 0; }
            
            int prevHoveredSkill = this.HoveredSkill;
            bool isZh = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh;
            if (animatingSkillPerkUnlock == -1)
            {
                if (WaitingSkillRectangle is not null && WaitingSkillRectangle.Hovering(_x, _y))
                {
                    this.HoveredSkill = 0;
                    hoveredSkillFrame = new(WaitingSkillRectangle.Position, hoveredSkillFrameSrcRect);
                }
                else if (ForageSkillRectangle is not null && ForageSkillRectangle.Hovering(_x, _y))
                {
                    this.HoveredSkill = 2;
                    hoveredSkillFrame = new(ForageSkillRectangle.Position, hoveredSkillFrameSrcRect);
                }
                else if (FishingSkillRectangle is not null && FishingSkillRectangle.Hovering(_x, _y))
                {
                    this.HoveredSkill = 3;
                    hoveredSkillFrame = new(FishingSkillRectangle.Position, hoveredSkillFrameSrcRect);
                }
                else if (HuntingSkillRectangle is not null && HuntingSkillRectangle.Hovering(_x, _y))
                {
                    this.HoveredSkill = 4;
                    hoveredSkillFrame = new(HuntingSkillRectangle.Position, hoveredSkillFrameSrcRect);
                }
                else if (FollowSkillRectangle is not null && FollowSkillRectangle.Hovering(_x, _y))
                {
                    this.HoveredSkill = 1;
                    hoveredSkillFrame = new(FollowSkillRectangle.Position, hoveredSkillFrameSrcRect);
                }
                else if (!CheckIfMouseInSkillAreaIsInNeutralZone(_x, _y, xPos, yPos + (isZh ? 0 : -4), this.SkillMastery_level[0] >= 1 && HasWaitSkill, this.SkillMastery_level[2] >= 1 && HasForageSkill, this.SkillMastery_level[4] >= 1 && HasHuntSkill, this.SkillMastery_level[3] >= 1 && HasFishingSkill, this.SkillMastery_level[1] >= 1 && HasFollowSkill, this.HasAllThreeMainSkills))
                {
                    this.HoveredSkill = -1;
                    hoveredSkillFrame = null;
                }
                int prevHoveredSkillPerk = this.HoveredSkillPerk;
                this.HoveredSkillPerk = this.HoveredSkill != -1 ? CheckIfHoveringSkillPerk(_x, _y, xPos, yPos + (isZh ? 4 : 0), HoveredSkill) : -1;
                if (this.HoveredSkillPerk != -1 && prevHoveredSkillPerk != this.HoveredSkillPerk)
                {
                    if (!SynchronizationManager.GetBoolByte(SkillPerkTierChecklist, HoveredSkill, HoveredSkillPerk)) { Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.SkillPerkLocked_select"); }
                    else { Game1.playSound("shiny4"); }
                }
            }
            if ((this.currentControlledObject != 0 && prevControlledObject != this.currentControlledObject) || (this.HoveredSkill != -1 && prevHoveredSkill != this.HoveredSkill))
            {
                Game1.playSound("shiny4");
            }
        }
        private int CheckIfHoveringSkillPerk(int _mousex, int _mousey, int xpos, int ypos, int hovSkill)
        {
            if (IsSkillPerkReadyToBeUnlocked(hovSkill, 0) && new Rectangle(xpos + 360, ypos + 452, 48, 48).Contains(_mousex, _mousey))
            {
                return 0;
            }
            if (IsSkillPerkReadyToBeUnlocked(hovSkill, 1) && new Rectangle(xpos + 428, ypos + 448, 48, 48).Contains(_mousex, _mousey))
            {
                return 1;
            }
            if (IsSkillPerkReadyToBeUnlocked(hovSkill, 2) && new Rectangle(xpos + 500, ypos + 448, 48, 48).Contains(_mousex, _mousey))
            {
                return 2;
            }
            if (IsSkillPerkReadyToBeUnlocked(hovSkill, 3) && new Rectangle(xpos + 568, ypos + 452, 48, 48).Contains(_mousex, _mousey))
            {
                return 3;
            }
            return -1;
        }
        public void resetTimer3and4()
        {
            this.timer3 = this.timer4 = 0;
        }
        private bool CheckIfMouseInSkillAreaIsInNeutralZone(int mouseX, int mouseY, int posX, int posY, bool hasWaitSkill, bool hasForageSkill, bool hasHuntSkill, bool hasFishingSkill, bool hasFollowSkill, bool hasAllThreeMainSkills)
        {
            bool isIt = false;
            if (hasWaitSkill)
            {
                if (hasAllThreeMainSkills)
                {
                    if (hasForageSkill)
                    {
                        isIt = new Rectangle(posX + 400, posY + 356, 8, 52).Contains(mouseX, mouseY);
                    }
                }
                else if (hasForageSkill || (!hasForageSkill && hasFishingSkill))
                {
                    isIt = new Rectangle(posX + 420, posY + 356, 12, 52).Contains(mouseX, mouseY);
                }
            }
            if (!isIt && hasFollowSkill)
            {
                if (hasAllThreeMainSkills)
                {
                    if (hasFishingSkill)
                    {
                        isIt = new Rectangle(posX + 568, posY + 356, 8, 52).Contains(mouseX, mouseY);
                    }
                }
                else if ((hasForageSkill && hasFishingSkill) || hasHuntSkill)
                {
                    isIt = new Rectangle(posX + 544, posY + 356, 12, 52).Contains(mouseX, mouseY);
                }
            }
            if (!isIt && new Rectangle(posX + 356, posY + 408, 264, 100).Contains(mouseX, mouseY))
            {
                isIt = true;
            }
            return isIt;
        }
        public void ActionClick(int _bgIndex, Guid _petID, int _buttonPressedForTotalOfTicks)
        {
            bool isFirstTick = _buttonPressedForTotalOfTicks == 1;
            bool isTickModulus20 = (_buttonPressedForTotalOfTicks % 20) == 1;
            if (isTickModulus20 && this.currentControlledObject == 1) { this.LeftBGChangeArrow.ResetScale(); PetHelper.ChangeBackgroundIndex(_petID, (_bgIndex + 15) % 16); Game1.playSound("pickUpItem"); }
            if (isTickModulus20 && this.currentControlledObject == 2) { this.RightBGChangeArrow.ResetScale(); PetHelper.ChangeBackgroundIndex(_petID, (_bgIndex + 1) % 16); Game1.playSound("pickUpItem"); }
            if (isTickModulus20 && canChangeDirection && ((this.currentControlledObject == 4) || (this.currentControlledObject == 5)))
            {
                if (this.currentControlledObject == 4) { this.LeftRotationArrow.ResetScale(); }
                else
                {
                    this.RightRotationArrow.ResetScale();
                }
                this.petFacingDirection = this.currentControlledObject == 5 ? (this.petFacingDirection + 1) % 4 : this.currentControlledObject == 4 ? (this.petFacingDirection + 3) % 4 : this.petFacingDirection;
                resetTimer3and4();
                flipSprite = false;
                Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.Rotation_arrowClick0" + Game1.random.Next(3));
            }

            if (this.soupStage == 0)
            {
                if (isFirstTick)
                {
                    if (this.currentControlledObject == 7 && this.soup == 476)
                    {
                        this.soupStage = -1;
                        Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.PetDietList_closing");
                    }
                    else if (this.currentControlledObject == 7 && this.soup == 0)
                    {
                        this.soupStage = 1;
                        Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.PetDietList_opening");
                    }
                    else if (this.currentControlledObject == 8)
                    {
                        this.soupStage = 2;
                        this.fromYIndex = this.currentScrollYIndex;
                        this.toYIndex = Math.Max(0, this.currentScrollYIndex - 66);
                        if (this.currentScrollYIndex > 0)
                        {
                            Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.PetDietList_scroll0" + Game1.random.Next(3));
                        }
                    }
                    else if (this.currentControlledObject == 9)
                    {
                        this.soupStage = -2;
                        this.fromYIndex = this.currentScrollYIndex;
                        this.toYIndex = Math.Min(this.currentScrollYIndex + 66, this.itemsOnScrollTexture.Height - 490);
                        if (this.currentScrollYIndex < (this.itemsOnScrollTexture.Height - 490))
                        {
                            Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.PetDietList_scroll0" + Game1.random.Next(3));
                        }
                    }
                }
                else if (_buttonPressedForTotalOfTicks > 10 && (this.currentControlledObject == 8 || this.currentControlledObject == 9))
                {
                    this.currentScrollYIndex = this.currentControlledObject == 8 ? Math.Max(0, this.currentScrollYIndex - 6) : this.currentControlledObject == 9 ? Math.Min(this.currentScrollYIndex + 6, this.itemsOnScrollTexture.Height - 490) : this.currentScrollYIndex;
                    this.toYIndex = this.currentScrollYIndex;
                    this.fromYIndex = this.toYIndex;
                }
                
            }
            if (this.soup == 476 && this.currentControlledObject == 9)
            {
                this.ScrollDownButtonScale = MathF.Max(1f, this.ScrollDownButtonScale - 0.075f);
            }
            else if (this.soup == 476 && this.currentControlledObject == 8)
            {
                this.ScrollUpButtonScale = MathF.Max(1f, this.ScrollUpButtonScale - 0.075f);
            }
            if (isFirstTick && HoveredSkill >= 0 && HoveredSkillPerk >= 0 && animatingSkillPerkUnlock == -1)
            {
                if (!SynchronizationManager.GetBoolByte(SkillPerkTierChecklist, HoveredSkill, HoveredSkillPerk))
                {
                    bool isZH = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh;
                    Vector2 offset = new(xPos + 340, yPos + 448 + (isZH ? 4 : 0));
                    Vector2 basePosition = HoveredSkillPerk == 0 ? new Vector2(offset.X + 20f, offset.Y + 4) : HoveredSkillPerk == 1 ? new Vector2(offset.X + 88f, offset.Y) : HoveredSkillPerk == 2 ? new Vector2(offset.X + 160f, offset.Y) : HoveredSkillPerk == 3 ? new Vector2(offset.X + 228f, offset.Y + 4) : Vector2.Zero;
                    currentSkillPerkAlpha = 0f;
                    animatingSkillPerkUnlockElapsedTime = 0;
                    animatingSkillPerkUnlock = HoveredSkillPerk;
                    PetHelper.MarkPetSkillPerkChecklist(_petID, HoveredSkill, HoveredSkillPerk);
                    skillPerkUnlockSpriteData = (basePosition, basePosition, false, 1f, new(Game1.random.Next(-14, 15), -Game1.random.Next(10, 25)));
                }
            }
        }

        private int MoveHoveredSkillPerk(int _currentHoveredSkillPerk, bool _moveRight) => !_moveRight ? _currentHoveredSkillPerk - 1 : (_currentHoveredSkillPerk < 3 && HoveredSkill < SkillMastery_level.Length && IsSkillPerkReadyToBeUnlocked(HoveredSkill, _currentHoveredSkillPerk + 1)) ? _currentHoveredSkillPerk + 1 : _currentHoveredSkillPerk;
        private int MoveHoveredSkill(int _currentHoveredSkill, bool _moveRight, bool lockMovement = false)
        {
            if (_moveRight)
            {
                if (_currentHoveredSkill == -1)
                {
                    if (SkillMastery_level[0] >= 1 && HasWaitSkill) { return 0; }
                    else if (SkillMastery_level[2] >= 1 && HasForageSkill) { return 2; }
                    else if (SkillMastery_level[3] >= 1 && HasFishingSkill && !HasAllThreeMainSkills) { return 3; }
                    else if (SkillMastery_level[4] >= 1 && HasHuntSkill) { return 4; }
                    else if (SkillMastery_level[1] >= 1 && HasFollowSkill) { return 1; }
                }
                if (_currentHoveredSkill == 0)
                {
                    if ( SkillMastery_level[2] >= 1 && HasForageSkill) { return 2; }
                    else if (SkillMastery_level[3] >= 1 && HasFishingSkill && !HasAllThreeMainSkills) { return 3; }
                    else if (SkillMastery_level[4] >= 1 && HasHuntSkill) { return 4; }
                    else if (SkillMastery_level[1] >= 1 && HasFollowSkill) { return 1; }
                }
                if (_currentHoveredSkill == 2)
                {
                    if (SkillMastery_level[3] >= 1 && HasFishingSkill && !HasAllThreeMainSkills) { return 3; }
                    else if (SkillMastery_level[4] >= 1 && HasHuntSkill) { return 4; }
                    else if (SkillMastery_level[1] >= 1 && HasFollowSkill) { return 1; }
                }
                if (_currentHoveredSkill == 3)
                {
                    if ((SkillMastery_level[4] >= 1 && HasHuntSkill) && !HasAllThreeMainSkills) { return 4; }
                    else if (SkillMastery_level[1] >= 1 && HasFollowSkill) { return 1; }
                }
                if (_currentHoveredSkill == 4)
                {
                    if ((SkillMastery_level[3] >= 1 && HasFishingSkill) && HasAllThreeMainSkills) { return 3; }
                    else if (SkillMastery_level[1] >= 1 && HasFollowSkill) { return 1; }
                }
            }
            else
            {
                if (_currentHoveredSkill == -1)
                {
                    if (SkillMastery_level[1] >= 1 && HasFollowSkill) { return 1; }
                    else if (SkillMastery_level[3] >= 1 && HasFishingSkill && (HasAllThreeMainSkills || SkillMastery_level[2] >= 1)) { return 3; }
                    else if (SkillMastery_level[4] >= 1 && HasHuntSkill) { return 4; }
                    else if (SkillMastery_level[3] >= 1 && HasFishingSkill) { return 3; }
                    else if (SkillMastery_level[2] >= 1 && HasForageSkill) { return 2; }
                    else if (SkillMastery_level[0] >= 1 && HasWaitSkill) { return 0; }
                }
                if (_currentHoveredSkill == 2)
                {
                    if (SkillMastery_level[0] >= 1 && HasWaitSkill) { return 0; }
                }
                if (_currentHoveredSkill == 3)
                {
                    if (SkillMastery_level[4] >= 1 && HasHuntSkill && HasAllThreeMainSkills) { return 4; }
                    else if (SkillMastery_level[2] >= 1 && HasForageSkill) { return 2; }
                    else if (SkillMastery_level[0] >= 1 && HasWaitSkill) { return 0; }
                }
                if (_currentHoveredSkill == 4)
                {
                    if (SkillMastery_level[3] >= 1 && HasFishingSkill && !HasAllThreeMainSkills) { return 3; }
                    else if (SkillMastery_level[2] >= 1 && HasForageSkill) { return 2; }
                    else if (SkillMastery_level[0] >= 1 && HasWaitSkill) { return 0; }
                }
                if (_currentHoveredSkill == 1)
                {
                    if (SkillMastery_level[3] >= 1 && HasFishingSkill && (HasAllThreeMainSkills || SkillMastery_level[2] >= 1)) { return 3; }
                    else if (SkillMastery_level[4] >= 1 && HasHuntSkill) { return 4; }
                    else if (SkillMastery_level[3] >= 1 && HasFishingSkill) { return 3; }
                    else if (SkillMastery_level[2] >= 1 && HasForageSkill) { return 2; }
                    else if (SkillMastery_level[0] >= 1 && HasWaitSkill) { return 0; }
                }
            }
                return !lockMovement ? -1 : _currentHoveredSkill;
        }
        public void PressedRight(int _buttonPressedForTotalOfTicks)
        {
            if ((_buttonPressedForTotalOfTicks % 10) != 1) { return; }
            int prevControlledObject = this.currentControlledObject;
            int prevHoveredSkill = this.HoveredSkill;
            if (this.currentControlledObject == 1)
            {
                this.currentControlledObject = 2;
            }
            else if (this.currentControlledObject == 3)
            {
                this.currentControlledObject = 2;
            }
            else if (this.currentControlledObject == 4)
            {
                this.currentControlledObject = 5;
            }
            else if (this.currentControlledObject == 5 || this.currentControlledObject == 7 || this.currentControlledObject == 8 || this.currentControlledObject == 9)
            {
                this.HoveredSkill = MoveHoveredSkill(-1, true);
                switch (HoveredSkill)
                {
                    case 0: { hoveredSkillFrame = new(WaitingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                    case 1: { hoveredSkillFrame = new(FollowSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                    case 2: { hoveredSkillFrame = new(ForageSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                    case 3: { hoveredSkillFrame = new(FishingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                    case 4: { hoveredSkillFrame = new(HuntingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                    default: { hoveredSkillFrame = null; break; }
                }
                this.currentControlledObject = HoveredSkill >= 0 ? 0 : 10;
            }
            else if (this.currentControlledObject == 6)
            {
                this.currentControlledObject = 5;
            }
            else if (this.currentControlledObject == 2)
            {
                this.currentControlledObject = 10;
            }
            else if (this.currentControlledObject == 0)
            {
                if (this.HoveredSkill < 0)
                {
                    this.HoveredSkill = MoveHoveredSkill(this.HoveredSkill, true);
                    switch (HoveredSkill)
                    {
                        case 0: { hoveredSkillFrame = new(WaitingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                        case 1: { hoveredSkillFrame = new(FollowSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                        case 2: { hoveredSkillFrame = new(ForageSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                        case 3: { hoveredSkillFrame = new(FishingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                        case 4: { hoveredSkillFrame = new(HuntingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                        default: { hoveredSkillFrame = null; this.currentControlledObject = 10; break; }
                    }
                }
                else
                {
                    if (HoveredSkillPerk < 0)
                    {
                        this.HoveredSkill = MoveHoveredSkill(this.HoveredSkill, true, true);
                        switch (HoveredSkill)
                        {
                            case 0: { hoveredSkillFrame = new(WaitingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                            case 1: { hoveredSkillFrame = new(FollowSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                            case 2: { hoveredSkillFrame = new(ForageSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                            case 3: { hoveredSkillFrame = new(FishingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                            case 4: { hoveredSkillFrame = new(HuntingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                        }
                    }
                    else if (animatingSkillPerkUnlock == -1)
                    {
                        int prevHoveredSkillPerk = this.HoveredSkillPerk;
                        this.HoveredSkillPerk = MoveHoveredSkillPerk(this.HoveredSkillPerk, true);
                        if (this.HoveredSkillPerk != -1 && prevHoveredSkillPerk != this.HoveredSkillPerk)
                        {
                            if (!SynchronizationManager.GetBoolByte(SkillPerkTierChecklist, HoveredSkill, HoveredSkillPerk)) { Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.SkillPerkLocked_select"); }
                            else { Game1.playSound("shiny4"); }
                        }

                    }
                }
            }
            if ((this.currentControlledObject != 0 && prevControlledObject != this.currentControlledObject) || (this.HoveredSkill != -1 && prevHoveredSkill != this.HoveredSkill))
            {
                Game1.playSound("shiny4");
            }
        }
        public void PressedLeft(int _buttonPressedForTotalOfTicks)
        {
            if ((_buttonPressedForTotalOfTicks % 10) != 1) { return; }
            int prevControlledObject = this.currentControlledObject;
            int prevHoveredSkill = this.HoveredSkill;
            if (this.currentControlledObject == 0)
            {
                if (HoveredSkill < 0)
                {
                    this.currentControlledObject = soup == 0? 6 : soup == 476 ? 7 : this.currentControlledObject;
                }
                else if (HoveredSkillPerk < 0)
                {
                    this.HoveredSkill = MoveHoveredSkill(this.HoveredSkill, false);
                    switch (HoveredSkill)
                    {
                        case 0: { hoveredSkillFrame = new(WaitingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                        case 1: { hoveredSkillFrame = new(FollowSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                        case 2: { hoveredSkillFrame = new(ForageSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                        case 3: { hoveredSkillFrame = new(FishingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                        case 4: { hoveredSkillFrame = new(HuntingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                        default: 
                            {
                                if (soup == 476 || soup == 0)
                                {
                                    hoveredSkillFrame = null;
                                    HoveredSkill = -1;
                                    this.currentControlledObject = soup == 476 ? 7 : 5;
                                }
                                else
                                {
                                    HoveredSkill = prevHoveredSkill;
                                }
                                break;
                            }
                    }
                }
                else if (animatingSkillPerkUnlock == -1)
                {
                    int prevHoveredSkillPerk = this.HoveredSkillPerk;
                    this.HoveredSkillPerk = MoveHoveredSkillPerk(this.HoveredSkillPerk, false);
                    if (this.HoveredSkillPerk != -1 && prevHoveredSkillPerk != this.HoveredSkillPerk)
                    {
                        if (!SynchronizationManager.GetBoolByte(SkillPerkTierChecklist, HoveredSkill, HoveredSkillPerk)) { Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.SkillPerkLocked_select"); }
                        else { Game1.playSound("shiny4"); }
                    }
                    if (HoveredSkillPerk < 0)
                    {
                        if (soup == 476 || soup == 0)
                        {
                            hoveredSkillFrame = null;
                            HoveredSkill = -1;
                            this.currentControlledObject = 7;
                        }
                        else { this.HoveredSkillPerk = prevHoveredSkillPerk; }
                    }
                }
            }
            else if (this.currentControlledObject == 2)
            {
                this.currentControlledObject = 1;
            }
            else if (this.currentControlledObject == 3)
            {
                this.currentControlledObject = 1;
            }
            else if (this.currentControlledObject == 5)
            {
                this.currentControlledObject = 4;
            }
            else if (this.currentControlledObject == 6)
            {
                this.currentControlledObject = 4;
            }
            else if (this.currentControlledObject == 10)
            {
                if (soup == 476)
                {
                    this.currentControlledObject = 7;
                }
                else if (soup == 0)
                {
                    this.currentControlledObject = 2;
                }
            }
            if ((this.currentControlledObject != 0 && prevControlledObject != this.currentControlledObject) || (this.HoveredSkill != -1 && prevHoveredSkill != this.HoveredSkill))
            {
                Game1.playSound("shiny4");
            }
        }
        public void PressedDown(int _buttonPressedForTotalOfTicks)
        {
            int prevControlledObject = this.currentControlledObject;
            int prevHoveredSkill = this.HoveredSkill;
            bool clickEveryOnceAndThen = (_buttonPressedForTotalOfTicks % 10) == 1;
            if (clickEveryOnceAndThen && this.currentControlledObject == 0)
            {
                if (HoveredSkill < 0)
                {
                    this.currentControlledObject = this.soup == 476 || this.soup == 0 ? 7 : this.currentControlledObject;
                }
                else if (HoveredSkillPerk < 0)
                {
                    int prevHoveredSkillPerk = this.HoveredSkillPerk;
                    this.HoveredSkillPerk = MoveHoveredSkillPerk(this.HoveredSkillPerk, true);
                    if (this.HoveredSkillPerk != -1 && prevHoveredSkillPerk != this.HoveredSkillPerk)
                    {
                        if (!SynchronizationManager.GetBoolByte(SkillPerkTierChecklist, HoveredSkill, HoveredSkillPerk)) { Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.SkillPerkLocked_select"); }
                        else { Game1.playSound("shiny4"); }
                    }
                }
            }
            else if (clickEveryOnceAndThen && this.currentControlledObject == 1)
            {
                this.currentControlledObject = 4;
            }
            else if (clickEveryOnceAndThen && this.currentControlledObject == 2)
            {
                this.currentControlledObject = 5;
            }
            else if (clickEveryOnceAndThen && this.currentControlledObject == 3)
            {
                this.currentControlledObject = 6;
            }
            else if (clickEveryOnceAndThen && this.currentControlledObject == 10)
            {
                this.HoveredSkill = MoveHoveredSkill(-1, false);
                switch (HoveredSkill)
                {
                    case 0: { hoveredSkillFrame = new(WaitingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                    case 1: { hoveredSkillFrame = new(FollowSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                    case 2: { hoveredSkillFrame = new(ForageSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                    case 3: { hoveredSkillFrame = new(FishingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                    case 4: { hoveredSkillFrame = new(HuntingSkillRectangle.Position, hoveredSkillFrameSrcRect); break; }
                    default: { hoveredSkillFrame = null; break; }
                }
                this.currentControlledObject = HoveredSkill >= 0 ? 0 : this.currentControlledObject;
            }
            else if (clickEveryOnceAndThen && (this.currentControlledObject == 4 || this.currentControlledObject == 5 || this.currentControlledObject == 6))
            {
                this.currentControlledObject = 7;
            }
            else if ((currentControlledObject == 7 || currentControlledObject == 8 || currentControlledObject == 9 || (hoveringWindowLeftSide && currentControlledObject == 0)) && this.soup == 476 && HoveredSkill < 0)
            {
                if (currentControlledObject == 8 || currentControlledObject == 9) { currentControlledObject = 7; }
                this.currentScrollYIndex = Math.Min(this.currentScrollYIndex + (int)Math.Min(6f * ((float)_buttonPressedForTotalOfTicks / 10f), 6f), this.itemsOnScrollTexture.Height - 490);
                this.ScrollDownButtonScale = MathF.Min(this.ScrollDownButtonScale + 0.05f, 1.10f);
            }

            if ((this.currentControlledObject != 0 && prevControlledObject != this.currentControlledObject) || (this.HoveredSkill != -1 && prevHoveredSkill != this.HoveredSkill))
            {
                Game1.playSound("shiny4");
            }
        }
        public void PressedUp(int _buttonPressedForTotalOfTicks)
        {
            int prevControlledObject = this.currentControlledObject;
            int prevHoveredSkill = this.HoveredSkill;
            bool clickEveryOnceAndThen = (_buttonPressedForTotalOfTicks % 10) == 1;

            if (clickEveryOnceAndThen && this.currentControlledObject == 0)
            {
                if (HoveredSkill < 0)
                {
                    this.currentControlledObject = this.soup == 0 ? 3 : this.soup == 476 ? 7 : this.currentControlledObject;
                }
                else if (HoveredSkillPerk < 0)
                {
                    this.currentControlledObject = 10;
                    hoveredSkillFrame = null;
                    
                    HoveredSkill = -1;
                }
                else if (animatingSkillPerkUnlock == -1)
                {
                    HoveredSkillPerk = -1;
                }
            }
            else if (clickEveryOnceAndThen && this.currentControlledObject == 4)
            {
                this.currentControlledObject = 1;
            }
            else if (clickEveryOnceAndThen && this.currentControlledObject == 5)
            {
                this.currentControlledObject = 2;
            }
            else if (clickEveryOnceAndThen && this.currentControlledObject == 6)
            {
                this.currentControlledObject = 3;
            }
            else if (clickEveryOnceAndThen && this.currentControlledObject == 7 && this.soup == 0)
            {
                this.currentControlledObject = 6;
            }
            else if ((currentControlledObject == 7 || currentControlledObject == 8 || currentControlledObject == 9 || (hoveringWindowLeftSide && currentControlledObject == 0)) && this.soup == 476 && HoveredSkill < 0)
            {
                if (currentControlledObject == 8 || currentControlledObject == 9) { currentControlledObject = 7; }
                this.currentScrollYIndex = Math.Max(0, this.currentScrollYIndex - (int)Math.Min(6f * ((float)_buttonPressedForTotalOfTicks / 10f), 6f));
                this.ScrollUpButtonScale = MathF.Min(this.ScrollUpButtonScale + 0.05f, 1.10f);
            }
            if ((this.currentControlledObject != 0 && prevControlledObject != this.currentControlledObject) || (this.HoveredSkill != -1 && prevHoveredSkill != this.HoveredSkill))
            {
                Game1.playSound("shiny4");
            }
        }
        int curTime = 0;
        int fromYIndex;
        int toYIndex;
        Vector2 newPerkIconOffset = Vector2.Zero;
        double PulsatingPerkScale = 0d;
        float pulsatingHotkeyAlpha = 0.5f;
        public void Update(GameTime time,int animationDeltaTime, uint _skPChecklist, bool[] hasPerksReadyToBeUnlocked, bool _mouseMoved)
        {
            double elapsedMs = time.ElapsedGameTime.Milliseconds * 0.001d;
            double elapsedMsFaster = elapsedMs * 4;
            MouseMoved = _mouseMoved;
            bool isZH = LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh;
            PulsatingPerkScale = GetPulsatingScale(animationDeltaTime);
            if (HoveredSkill != -1)
            {
                CurrentSkillPerkScale[0] = (float)Math.Clamp(CurrentSkillPerkScale[0] + (HoveredSkillPerk == 0 ? elapsedMs : elapsedMs * -2), 1, 1.12f);
                CurrentSkillPerkScale[1] = (float)Math.Clamp(CurrentSkillPerkScale[1] + (HoveredSkillPerk == 1 ? elapsedMs : elapsedMs * -2), 1, 1.12f);
                CurrentSkillPerkScale[2] = (float)Math.Clamp(CurrentSkillPerkScale[2] + (HoveredSkillPerk == 2 ? elapsedMs : elapsedMs * -2), 1, 1.12f);
                CurrentSkillPerkScale[3] = (float)Math.Clamp(CurrentSkillPerkScale[3] + (HoveredSkillPerk == 3 ? elapsedMs : elapsedMs * -2), 1, 1.12f);
            }
            else
            {
                CurrentSkillPerkScale[0] = CurrentSkillPerkScale[1] = CurrentSkillPerkScale[2] = CurrentSkillPerkScale[3] = 1f;
            }
            exclamationIconAlpha1 = hasPerksReadyToBeUnlocked[0] ? 1f : MathF.Max(exclamationIconAlpha1 - (float)elapsedMs, 0);
            exclamationIconAlpha2 = hasPerksReadyToBeUnlocked[1] ? 1f : MathF.Max(exclamationIconAlpha2 - (float)elapsedMs, 0);
            exclamationIconAlpha3 = hasPerksReadyToBeUnlocked[2] ? 1f : MathF.Max(exclamationIconAlpha3 - (float)elapsedMs, 0);
            exclamationIconAlpha4 = hasPerksReadyToBeUnlocked[3] ? 1f : MathF.Max(exclamationIconAlpha4 - (float)elapsedMs, 0);
            exclamationIconAlpha5 = hasPerksReadyToBeUnlocked[4] ? 1f : MathF.Max(exclamationIconAlpha5 - (float)elapsedMs, 0);

            newPerkIconOffset = new Vector2(22, -35 - YoffsetForEaseInOutLoop(animationDeltaTime, 3));
            this.SkillPerkTierChecklist = _skPChecklist;
            LeftBGChangeArrow.InflateIfHoveredDeflateOtherwise(4.4f, elapsedMsFaster, currentControlledObject == 1 || currentControlledObject == 3);
            RightBGChangeArrow.InflateIfHoveredDeflateOtherwise(4.4f, elapsedMsFaster, currentControlledObject == 2 || currentControlledObject == 3);
            LeftRotationArrow.InflateIfHoveredDeflateOtherwise(4.4f, elapsedMsFaster, currentControlledObject == 4 || currentControlledObject == 6);
            RightRotationArrow.InflateIfHoveredDeflateOtherwise(4.4f, elapsedMsFaster, currentControlledObject == 5 || currentControlledObject == 6);
            CloseStatusBarRectangle.InflateIfHoveredDeflateOtherwise(4.4f, elapsedMsFaster, currentControlledObject == 10);
            if (animatingSkillPerkUnlock != -1)
            {
                AnimateSkillPerkUnlock(ref animatingSkillPerkUnlockElapsedTime, time.ElapsedGameTime.Milliseconds, ref animatingSkillPerkUnlock, xPos + 340, yPos + 448 + (isZH ? 4 : 0));
            }
            switch (this.soupStage)
            {
                case 1: { if (EaseOut(0, 476, 100, ref curTime, ref this.soup)) { this.curTime = this.soupStage = 0; } } break;
                case -1: { if (EaseOut(476, 0, 100, ref curTime, ref this.soup)) { this.curTime = this.soupStage = 0; } } break;
                case 2 or -2: { if (EaseOut(fromYIndex, toYIndex, 30, ref curTime, ref this.currentScrollYIndex)) { this.curTime = this.soupStage = 0; } } break;
                default: break;
            }
            pulsatingHotkeyAlpha = Math.Clamp(0.2f + (1f * (MathF.Abs(animationDeltaTime) / 1500f)),0.5f, 0.9f);

            this.ScrollButtonScale = this.currentControlledObject == 7 && (soup == 476 || soup == 0) ? MathF.Min(this.ScrollButtonScale + 0.05f, 1.10f) : MathF.Max(1f, this.ScrollButtonScale - 0.025f);
            this.ScrollUpButtonScale = this.currentControlledObject == 8 ? MathF.Min(this.ScrollUpButtonScale + 0.05f, 1.10f) : MathF.Max(1f, this.ScrollUpButtonScale - 0.025f);
            this.ScrollDownButtonScale = this.currentControlledObject == 9 ? MathF.Min(this.ScrollDownButtonScale + 0.05f, 1.10f) : MathF.Max(1f, this.ScrollDownButtonScale - 0.025f);
        }
        private void AnimateSkillPerkUnlock(ref int elapsedT, int time, ref int whatSkillPerk, int _xPos, int _yPos)
        {
            if (elapsedT == 0)
            {
                Game1.playSound("SunkenLace.PetsEnhancedMod.Sounds.SkillPerkLock_opened");
            }
            float lockAlpha = skillPerkUnlockSpriteData.Value.alpha;
            if (elapsedT >= 750)
            {
                lockAlpha = (float)Math.Max(lockAlpha + (-0.002d * time), 0d);
            }
            if (elapsedT <= 500)
            {
                skillPerkUnlockSpriteData = (skillPerkUnlockSpriteData.Value.basePos + new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-2, 3)), skillPerkUnlockSpriteData.Value.basePos, false, lockAlpha, skillPerkUnlockSpriteData.Value.randomVelocity);
            }
            else if (elapsedT <= 530)
            {
                skillPerkUnlockSpriteData = (skillPerkUnlockSpriteData.Value.basePos, skillPerkUnlockSpriteData.Value.basePos, true, lockAlpha, skillPerkUnlockSpriteData.Value.randomVelocity);
            }
            else if (elapsedT > 530)
            {
                var elapsedTNew = elapsedT - 530;
                currentSkillPerkAlpha = (float)Math.Min(currentSkillPerkAlpha + (0.002d * (double)(time)), 1d);
                Vector2 position = skillPerkUnlockSpriteData.Value.pos + new Vector2((time * skillPerkUnlockSpriteData.Value.randomVelocity.X * 0.01f) / 1.01f, (time * skillPerkUnlockSpriteData.Value.randomVelocity.Y * 0.01f) + elapsedTNew * 0.02f);
                skillPerkUnlockSpriteData = (position, skillPerkUnlockSpriteData.Value.basePos, true, lockAlpha, skillPerkUnlockSpriteData.Value.randomVelocity);
            }
            if (lockAlpha == 0 || elapsedT >= 1500)
            {
                currentSkillPerkAlpha = 1f;
                skillPerkUnlockSpriteData = null;
                whatSkillPerk = -1;
                elapsedT = 0;
            }
            elapsedT += time;
        }
        public bool EaseOut(int _from, int _to, int _duration, ref int currentTime, ref int variable)
        {
            variable = (int)(_from + ((1 - (float)Math.Pow(1 - ((float)currentTime / _duration), 2)) * (_to - _from)));
            currentTime++;
            return (variable >= _to && _from < _to) || (variable <= _to && _from > _to) || (_from == _to && variable == _from);
        }
        public void Draw(SpriteBatch b, Texture2D _texture, int _bgIndex,bool _useL1andR1backgroundChangeShortcuts)
        {
            b.Draw(_texture, new Vector2(xPos + 108, yPos + 76), new Rectangle(_bgIndex * 33, 232, 32, 57), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8700f); //Pet Background
            DrawPet(b, PetTexture, xPos + 108, yPos + 140, this.PetType,this.IsTurtle);
            b.Draw(_texture, new Vector2(xPos, yPos), new Rectangle(0, 73, 165, 134), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8700f); //Window
            DrawWindowLeftSide(b, _texture, xPos, yPos, this.DateNumber, this.AgeText, this.DateText, _useL1andR1backgroundChangeShortcuts);
            DrawItemScroll(b, _texture, xPos, yPos, soup, this.ScrollButtonScale, this.currentScrollYIndex, this.itemsOnScrollTexture, this.ScrollUpButtonScale, this.ScrollDownButtonScale);
            DrawWindowRightSide(b, _texture, xPos, yPos, this.StatTitle, this.StatUnderlineType, this.SkillTitle, this.SkillUnderlineType, LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh, this.HasAllThreeMainSkills, this.SkillMastery_level[0], this.SkillMastery_level[1], this.SkillMastery_level[4], this.SkillMastery_level[2], this.SkillMastery_level[3], this.DmgStat, this.CooldownStat, this.CritStat, this.HoveredSkill);
        }
        private int timer1 = 0;
        private int timer2 = 0;
        private int timer3 = 0;
        private int timer4 = 0;
        private float pet_xPositionOffset = 128;
        public bool canChangeDirection = false;
        private int CurrentFrameIndex = 0;
        public bool flipSprite = false;
        public void DrawPet(SpriteBatch b, Texture2D _texture, int posX, int posY, PetInfo.Pet_Types petType,bool _isTurtle)
        {
            if (Game1.timeOfDay >= 2000)
            {
                pet_xPositionOffset = 0;
                timer1 = timer1 < 160 ? timer1 + 1 : 0;
                CurrentFrameIndex = _isTurtle ? 20 : timer1 < 80 ? 28 : 29;
                timer2 = timer4 = timer3 = timer4 = 0;
                flipSprite = false;
            }
            else if (pet_xPositionOffset > 0)
            {
                pet_xPositionOffset = pet_xPositionOffset > 0 ? pet_xPositionOffset - 2 : 0;
                AnimatePetWalking(3, ref timer2, ref CurrentFrameIndex);
                if (pet_xPositionOffset <= 0) { timer2 = 0; }

            }
            else if (!canChangeDirection)
            {
                AnimatePetSitingFront(ref timer3, ref CurrentFrameIndex, _isTurtle);
                if (timer3 >= 32) { canChangeDirection = true; timer3 = 300; }
            }
            else if (canChangeDirection && timer3 < 80)
            {
                timer3++;
                AnimatePetWalking(this.petFacingDirection, ref timer2, ref CurrentFrameIndex);
                if (timer3 >= 80)
                {
                    this.CurrentFrameIndex = this.petFacingDirection == 0 ? 8 : this.petFacingDirection == 1 ? 4 : this.petFacingDirection == 3 ? 12 : 0;
                }
            }
            else if (timer3 < 162)
            {
                timer3++;
                if (timer3 >= 130)
                {
                    switch (this.petFacingDirection)
                    {
                        case 2: AnimatePetSitingFront(ref timer4, ref CurrentFrameIndex, _isTurtle); break;
                        case 1 or 3: AnimatePetSitingSideways(petType, this.petFacingDirection, ref timer4, ref CurrentFrameIndex, ref flipSprite, _isTurtle); break;
                    }
                }
            }
            b.Draw(_texture, new Vector2(posX + pet_xPositionOffset, posY), new Rectangle((CurrentFrameIndex % 4) * 32, (int)(CurrentFrameIndex / 4) * 32, 32, 32), Color.White, 0f, Vector2.Zero, 4f, flipSprite ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.8700f);
        }
        private void AnimatePetSitingFront(ref int _timer, ref int currentFrameIndex,bool _isTurtle)
        {
            _timer++;
            currentFrameIndex = _isTurtle? 16 :_timer < 10 ? 0 : _timer < 18 ? 16 : _timer < 26 ? 17 : 18;
        }
        private void AnimatePetSitingSideways(PetInfo.Pet_Types petType, int _direction, ref int _timer, ref int currentFrameIndex, ref bool flip,bool _isTurtle)
        {
            if (petType == PetInfo.Pet_Types.LegacyCat || _isTurtle) return; //vanilla cat spritesheet
            flip = _direction == 3;
            _timer++;
            if (petType == PetInfo.Pet_Types.EnhancedCat) //If cat spriteSheet
            {
                currentFrameIndex = _timer < 10 ? 32 : _timer < 20 ? 33 : 34;
            }
            else //dog spritesheet
            {
                currentFrameIndex = _timer < 10 ? 20 : _timer < 18 ? 21 : _timer < 26 ? 22 : 23;
            }
        }
        private void AnimatePetWalking(int _direction, ref int _timer, ref int currentFrame)
        {
            _timer = _timer < 200 ? _timer + 5 : 0;
            if (_direction == 0)
            {
                currentFrame = _timer < 50 ? 8 : _timer < 100 ? 9 : _timer < 150 ? 10 : 11;
            }
            else if (_direction == 1)
            {
                currentFrame = _timer < 50 ? 4 : _timer < 100 ? 5 : _timer < 150 ? 6 : 7;
            }
            else if (_direction == 2)
            {
                currentFrame = _timer < 50 ? 0 : _timer < 100 ? 1 : _timer < 150 ? 2 : 3;
            }
            else if (_direction == 3)
            {
                currentFrame = _timer < 50 ? 12 : _timer < 100 ? 13 : _timer < 150 ? 14 : 15;
            }
        }
        private void DrawWindowLeftSide(SpriteBatch b, Texture2D texture, int posX, int posY, int _dateNumber, FontProperty _ageText, FontProperty _dateText,bool _useL1andR1backgroundChangeShortcuts)
        {
            DrawL1R1Shorcut(b, texture, posX, posY, Game1.options.gamepadControls && _useL1andR1backgroundChangeShortcuts, this.currentControlledObject, pulsatingHotkeyAlpha);

            LeftBGChangeArrow.Draw(b, texture, switchSourceRectPos: (this.currentControlledObject == 1 || this.currentControlledObject == 3) ? new(230, 39) : null);

            RightBGChangeArrow.Draw(b, texture, flipHorizontal:true, switchSourceRectPos: (this.currentControlledObject == 2 || this.currentControlledObject == 3) ? new(230, 39) : null);

            LeftRotationArrow.Draw(b, texture, switchSourceRectPos: (this.currentControlledObject == 4 || this.currentControlledObject == 6) ? new(226, 28) : null);

            RightRotationArrow.Draw(b, texture, flipHorizontal: true, switchSourceRectPos: (this.currentControlledObject == 5 || this.currentControlledObject == 6)? new(226, 28) : null);

            NameSprite.Draw(b, Game1.dialogueFont);

            fHeart1.Draw(b, texture);
            fHeart2.Draw(b, texture);
            fHeart3.Draw(b, texture);
            fHeart4.Draw(b, texture);
            fHeart5.Draw(b, texture);

            DrawPetAge(b, texture, posX, posY, _dateNumber, _ageText, _dateText);
        }
        private static void DrawPetAge(SpriteBatch b, Texture2D texture, int posX, int posY, int _dateNumber, FontProperty _ageText, FontProperty _dateText)
        {
            int yPos = posY + (97 * 4);
            float numXOffset = posX + ((43 - ((_ageText.width + 5 + GetAgeNumberFontMeasured(_dateNumber, out var numFontList) + numFontList.Count + _dateText.width) / 2)) * 4);
            float numXOffset2 = numXOffset + ((_ageText.width + 4) * 4);
            b.Draw(texture, new Vector2(numXOffset, yPos), new Rectangle(_ageText.x, _ageText.y, _ageText.width, _ageText.height), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.895f);
            int u = 0;
            while (u < numFontList.Count)
            {
                b.Draw(texture, new Vector2(numXOffset2, yPos + 12), new Rectangle(numFontList[u].x, numFontList[u].y, numFontList[u].width, numFontList[u].height), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.895f);
                numXOffset2 += ((numFontList[u].width + 1) * 4);
                u++;
            }
            b.Draw(texture, new Vector2(numXOffset2 + 4, yPos), new Rectangle(_dateText.x, _dateText.y, _dateText.width, _dateText.height), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.895f);
        }
        private static int GetAgeNumberFontMeasured(int _number, out List<FontProperty> _font)
        {
            int width = 0;
            List<FontProperty> fonts = new();
            var numStr = _number.ToString();
            int i = 0;
            while (i < numStr.Length)
            {
                char idx = numStr[i];
                int fontWidth = idx == '1' ? 3 : 4;
                fonts.Add(new(idx == '9' ? 433 : idx == '8' ? 428 : idx == '7' ? 423 : idx == '6' ? 418 : idx == '5' ? 413 : idx == '4' ? 408 : idx == '3' ? 403 : idx == '2' ? 398 : idx == '1' ? 394 : 389, 201, fontWidth, 6, 1));
                width += fontWidth;
                i++;
            }
            _font = fonts;
            return width;
        }
        private void DrawItemScroll(SpriteBatch b, Texture2D texture, int posX, int posY, int _soup, float scrollButtonScale, int _cSYIndex, Texture2D _itemsOnScrollTexture, float scrollUpButtonScale, float scrollDownButtonScale)
        {
            b.Draw(texture, new Rectangle(posX + 32, posY + 512 - _soup, 280, _soup), new Rectangle(166, 89, 70, 117), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.876f);
            b.Draw(texture, new Vector2(posX + 32, posY + 512 - _soup), new Rectangle(166, 88, 70, _soup > 3 ? 1 : 0), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.876f);
            b.Draw(texture, new Vector2(posX + 32, posY + 508), new Rectangle(166, 88, 70, 1), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.876f);

            b.Draw(texture, new Vector2(posX + 24, posY + 456 - _soup), new Rectangle(239, 148, 74, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.876f);

            Rectangle scrollButtonSourceRect = this.currentControlledObject == 7 && (_soup == 476 || _soup == 0) ? new Rectangle(300, 180, 12, 12) : new Rectangle(200, 0, 12, 12);
            b.Draw(texture, new Vector2(posX + 152 + 20 - (24 * scrollButtonScale), posY + 464 + 20 - _soup - (24 * scrollButtonScale)), scrollButtonSourceRect, Color.White, 0f, Vector2.Zero, 4f * scrollButtonScale, SpriteEffects.None, 0.878f);

            b.Draw(_itemsOnScrollTexture, new Vector2(posX + 48, posY + 512 - _soup), new Rectangle(0, _cSYIndex, _itemsOnScrollTexture.Width, (int)Math.Min(Math.Min(_soup, _itemsOnScrollTexture.Height), 476)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.876f);

            b.Draw(texture, new Vector2(posX + 32, posY + 452), new Rectangle(241, 163, 70, 15), _itemsOnScrollTexture.Height > 476 ? (Color.White * MathF.Min(((float)_itemsOnScrollTexture.Height - (_cSYIndex + 488)) / 68, 1f)) * ((float)_soup / 476) : Color.Transparent, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.876f);
            b.Draw(texture, new Vector2(posX + 32, posY + 512 - _soup), new Rectangle(241, 163, 70, 15), (Color.White * Math.Min((float)_cSYIndex / 68, 1f)) * ((float)_soup / 476), 0f, Vector2.Zero, 4f, SpriteEffects.FlipVertically, 0.876f);

            Vector2 a = SunkenLaceUtilities.OffsetByScale(44, 32, scrollUpButtonScale);
            Vector2 a2 = SunkenLaceUtilities.OffsetByScale(44, 32, scrollDownButtonScale);
            if (_soup == 476)
            {
                if (_cSYIndex > 0)
                {
                    b.Draw(texture, new Vector2(posX + 148 + 22 - a.X, posY + 48 + 16 - a.Y), new Rectangle(226, 1, 11, 8), Color.White, 0f, Vector2.Zero, 4f * scrollUpButtonScale, SpriteEffects.None, 0.876f);
                }
                if (_itemsOnScrollTexture.Height > 476 && ((float)_itemsOnScrollTexture.Height - (_cSYIndex + 490) > 0))
                {
                    b.Draw(texture, new Vector2(posX + 148 + 22 - a2.X, posY + 468 + 16 - a2.Y), new Rectangle(226, 10, 11, 8), Color.White, 0f, Vector2.Zero, 4f * scrollDownButtonScale, SpriteEffects.None, 0.876f);
                }
            }


        }
        private void DrawWindowRightSide(SpriteBatch b, Texture2D texture, int _posX, int _posY, FontProperty statTitle, int statUnderlineType, FontProperty skillTitle, int skillUnderlineType, bool isChinese, bool _hatms, double ws_p, double fs_p, double hs_p, double fos_p, double fis_p, string dmgStat, string cdStat, string critCStat, int hovSkill)
        {
            float newX = 340 + _posX;
            int newY = isChinese ? 68 + _posY : 64 + _posY;
            b.Draw(texture, new Vector2(isChinese ? newX + 148 - ((statTitle.width / 2) * (4f * statTitle.scale)) : newX, 28 + _posY), new Rectangle(statTitle.x, statTitle.y, statTitle.width, statTitle.height), Color.White, 0f, Vector2.Zero, 4f * statTitle.scale, SpriteEffects.None, 0.875f);

            DrawStats(b, texture, newX, newY + (isChinese ? 6 : 0), statUnderlineType, dmgStat, cdStat, critCStat);
            DrawSkills(b, texture, newX, newY + 220, skillTitle, skillUnderlineType, _hatms, ws_p, fs_p, hs_p, fos_p, fis_p, hovSkill);
            CloseStatusBarRectangle.Draw(b, texture);
        }
        private void DrawStats(SpriteBatch b, Texture2D texture, float posX, int posY, int statUnderlineType, string dmgStat, string cdStat, string critCStat)
        {
            b.Draw(texture, new Vector2(posX, posY), new Rectangle(389, 170 + (statUnderlineType * 2), 74, 1), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            float statPosX = posX + 84;

            b.Draw(texture, new Vector2(posX, posY + 16), new Rectangle(259, 179, 21, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            b.Draw(texture, new Vector2(statPosX, posY + 20), new Rectangle(259, 195, 53, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            GetStatStringFontMeasured(dmgStat, texture, out var fonts);
            loopDrawingStats(b, texture, statPosX, posY + 28, fonts);

            b.Draw(texture, new Vector2(posX, posY + 80), new Rectangle(237, 179, 21, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            b.Draw(texture, new Vector2(statPosX, posY + 84), new Rectangle(259, 195, 53, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            GetStatStringFontMeasured(cdStat, texture, out fonts);
            loopDrawingStats(b, texture, statPosX, posY + 92, fonts);

            b.Draw(texture, new Vector2(posX, posY + 144), new Rectangle(237, 193, 21, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            b.Draw(texture, new Vector2(statPosX, posY + 152), new Rectangle(259, 195, 53, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            GetStatStringFontMeasured(critCStat, texture, out fonts);
            loopDrawingStats(b, texture, statPosX, posY + 160, fonts);
        }
        private void loopDrawingStats(SpriteBatch b, Texture2D t, float posX, float posY, List<FontProperty> fonts)
        {
            float posB = posX + 24;
            int i = 0;
            while (i < fonts.Count)
            {
                var f = fonts[i];
                b.Draw(t, new Vector2(posB, posY), new Rectangle(f.x, f.y, f.width, f.height), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
                posB += ((f.width + f.rightMargin) * 4);
                i++;
            }
        }
        /// <summary>Returns the width of an string taking into acount the size in pixels of the font.</summary><remarks>Doesn't take into account character separation apart from spaces.</remarks>
        private static int GetStatStringFontMeasured(string _str, Texture2D texture, out List<FontProperty> _font)
        {
            int width = 0;
            List<FontProperty> fonts = new();
            int i = 0;
            while (i < _str.Length)
            {
                char c = _str[i];
                FontProperty font = new(c == '+' ? 459 : c == ' ' ? 394 : c == '%' ? 465 : c == '.' ? 455 : c == 's' ? 449 : c == '9' ? 443 : c == '8' ? 437 : c == '7' ? 431 : c == '6' ? 425 : c == '5' ? 419 : c == '4' ? 413 : c == '3' ? 407 : c == '2' ? 401 : c == '1' ? 395 : 389, 193, c == ' ' ? 1 : c == '%' ? 7 : c == '.' ? 3 : 5, 7, 1, c == ' ' ? 0 : 1);
                fonts.Add(font);
                width += font.width;
                i++;
            }
            _font = fonts;
            return width;
        }
        private void DrawSkills(SpriteBatch b, Texture2D texture, float _posX, int _posY, FontProperty skillTitle, int skillUnderlineType, bool hasAllThreeMainSkills, double ws_p, double fs_p, double hs_p, double fos_p, double fis_p, int hoveredSkill)
        {
            double currentMasteryPercentage = hoveredSkill == 0 ? (ws_p - 1) / 4 : hoveredSkill == 1 ? (fs_p - 1) / 4 : hoveredSkill == 2 ? (fos_p - 1) / 4 : hoveredSkill == 3 ? (fis_p - 1) / 4 : hoveredSkill == 4 ? (hs_p - 1) / 4 : -1;
            b.Draw(texture, new Vector2(_posX, _posY + 4), new Rectangle(skillTitle.x, skillTitle.y, skillTitle.width, skillTitle.height), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            b.Draw(texture, new Vector2(_posX, _posY + 44), new Rectangle(389, 178 + (skillUnderlineType * 5), 74, 4), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            b.Draw(texture, new Vector2(_posX, _posY + 60), new Rectangle(hasAllThreeMainSkills ? 464 : 314, hasAllThreeMainSkills ? 168 : 190, 74, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f); //Skill frame
            Rectangle skillExperienceBar = hoveredSkill < 0 ? new Rectangle(468, 186, 66, 6) : new Rectangle(397, 208, 66, 6);
            b.Draw(texture, new Vector2(_posX + 16, _posY + 132), skillExperienceBar, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            var skillPerkFrameAlt = new Rectangle(449, 215, 14, 15);
            var skillPerkFrame = new Rectangle(434, 215, 14, 15);
            bool condition1 = hoveredSkill >= 0;
            b.Draw(texture, new Vector2(_posX + 16, _posY + 160), condition1 && HoveredSkillPerk == 0 ? skillPerkFrameAlt : skillPerkFrame, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            b.Draw(texture, new Vector2(_posX + 84, _posY + 156), condition1 && HoveredSkillPerk == 1 ? skillPerkFrameAlt : skillPerkFrame, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            b.Draw(texture, new Vector2(_posX + 156, _posY + 156), condition1 && HoveredSkillPerk == 2 ? skillPerkFrameAlt : skillPerkFrame, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            b.Draw(texture, new Vector2(_posX + 224, _posY + 160), condition1 && HoveredSkillPerk == 3 ? skillPerkFrameAlt : skillPerkFrame, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            if (currentMasteryPercentage > 0)
            {
                b.Draw(texture, new Rectangle((int)_posX + 40, _posY + 136, Math.Clamp((int)(4 * (54 * currentMasteryPercentage)), 0, 216), 4), new Rectangle(296, 179 + (hoveredSkill), 1, 1), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.875f);
                if (currentMasteryPercentage < 1)
                {
                    b.Draw(texture, new Vector2((int)_posX + 40 + Math.Clamp((int)(4 * (54 * currentMasteryPercentage)) - 4, 0, 212), _posY + 136), new Rectangle(298, 179 + (hoveredSkill), 1, 1), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
                }
            }
            WaitingSkillRectangle?.Draw(b, texture);
            FollowSkillRectangle?.Draw(b, texture);
            ForageSkillRectangle?.Draw(b, texture);
            FishingSkillRectangle?.Draw(b, texture);
            HuntingSkillRectangle?.Draw(b, texture);

            hoveredSkillFrame?.Draw(b, texture);

            if (exclamationIconAlpha1 > 0 && WaitingSkillRectangle is not null)
            {
                b.Draw(texture, WaitingSkillRectangle.Position + newPerkIconOffset, ExclamationIconSrcRect, Color.White * exclamationIconAlpha1, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            }
            if (exclamationIconAlpha2 > 0 && FollowSkillRectangle is not null)
            {
                b.Draw(texture, FollowSkillRectangle.Position + newPerkIconOffset, ExclamationIconSrcRect, Color.White * exclamationIconAlpha2, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            }
            if (exclamationIconAlpha3 > 0 && ForageSkillRectangle is not null)
            {
                b.Draw(texture, ForageSkillRectangle.Position + newPerkIconOffset, ExclamationIconSrcRect, Color.White * exclamationIconAlpha3, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            }
            if (exclamationIconAlpha4 > 0 && FishingSkillRectangle is not null)
            {
                b.Draw(texture, FishingSkillRectangle.Position + newPerkIconOffset, ExclamationIconSrcRect, Color.White * exclamationIconAlpha4, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            }
            if (exclamationIconAlpha5 > 0 && HuntingSkillRectangle is not null)
            {
                b.Draw(texture, HuntingSkillRectangle.Position + newPerkIconOffset, ExclamationIconSrcRect, Color.White * exclamationIconAlpha5, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.875f);
            }
            DrawSkillPerksAnimations(b, texture, _posX, _posY + 164, hoveredSkill, CurrentSkillPerkScale);
            if (IsSkillPerkUnlocked(hoveredSkill, HoveredSkillPerk) && animatingSkillPerkUnlock == -1)
            {
                drawHoverSkillPerkDescription(b, hoveredSkill, HoveredSkillPerk, MouseMoved);
            }
        }
        public static float YoffsetForEaseInOutLoop(float time, float amplitude)
        {
            float duration = 1500;

            // Normalize time to range [0, 1] for each phase
            float normalizedTime = (time % duration) / duration;

            return amplitude * MathF.Sin((normalizedTime * MathF.PI * 2) - (MathF.PI / 2));
        }
        private void DrawL1R1Shorcut(SpriteBatch b, Texture2D texture, int _x, int _y,bool _controller, int currentControlledObject, float _iconDinamicOpacity)
        {
            if (!_controller) { return; }
            if (currentControlledObject == 1 || currentControlledObject == 2 || currentControlledObject == 3)
            {
                b.Draw(texture, new Vector2(_x + 108, _y + 28), new Rectangle(561,197,11,8), Color.White * _iconDinamicOpacity, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.875f);
                b.Draw(texture, new Vector2(_x + 203, _y + 28), new Rectangle(561, 188, 11, 8), Color.White * _iconDinamicOpacity, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.875f);
            }
            else if (currentControlledObject == 4 || currentControlledObject == 5 || currentControlledObject == 6)
            {
                b.Draw(texture, new Vector2(_x + 36, _y + 216), new Rectangle(561, 197, 11, 8), Color.White * _iconDinamicOpacity, 0f, Vector2.Zero, 3.4f, SpriteEffects.None, 0.875f);
                b.Draw(texture, new Vector2(_x + 271, _y + 216), new Rectangle(561, 188, 11, 8), Color.White * _iconDinamicOpacity, 0f, Vector2.Zero, 3.4f, SpriteEffects.None, 0.875f);
            }
        }
        private void drawHoverSkillPerkDescription(SpriteBatch b, int hoveredSkill, int hoveredSPerk, bool _mouseMoved)
        {
            string text = "";
            switch (hoveredSkill)
            {
                case 0:
                    {
                        text = hoveredSPerk == 2 ? I18n.SkillPerkDescription1() : hoveredSPerk == 3 ? I18n.SkillPerkDescription2() : I18n.SkillPerkDescription0();
                    }
                    break;
                case 1:
                    {
                        text = hoveredSPerk == 1 ? I18n.SkillPerkDescription3() : hoveredSPerk == 2 ? I18n.SkillPerkDescription4(I18n.FollowMeLessonName()) : hoveredSPerk == 3 ? I18n.SkillPerkDescription5() : I18n.SkillPerkDescription1();
                    }
                    break;
                case 2:
                    {
                        text = hoveredSPerk == 1 ? I18n.SkillPerkDescription9() : hoveredSPerk == 2 ? I18n.SkillPerkDescription7() : hoveredSPerk == 3 ? I18n.SkillPerkDescription8() : I18n.SkillPerkDescription6();
                    }
                    break;
                case 3:
                    {
                        text = hoveredSPerk == 1 ? I18n.SkillPerkDescription10() : hoveredSPerk == 2 ? I18n.SkillPerkDescription7() : hoveredSPerk == 3 ? I18n.SkillPerkDescription8() : I18n.SkillPerkDescription6();
                    }
                    break;
                case 4:
                    {
                        text = hoveredSPerk == 1 ? I18n.SkillPerkDescription12() : hoveredSPerk == 2 ? I18n.SkillPerkDescription13() : hoveredSPerk == 3 ? I18n.SkillPerkDescription14() : I18n.SkillPerkDescription11();
                    }
                    break;
                default: break;
            }
            Point overridePosition = !_mouseMoved ? new Point(xPos + 472 - (int)(Game1.smallFont.MeasureString(text).X * 0.5f), yPos + 540) : new Point(-1, -1);
            IClickableMenu.drawHoverText(b, text, Game1.smallFont, 0, 0, overrideX: overridePosition.X, overrideY: overridePosition.Y);
        }
        /// <summary>Example: (16 * newScale) / 2</summary>
        private Vector2 CalculateOffsetBasedOnScale(float sizeTimesNewScaleInHalf, float sizeTimesBaseScaleInHalf)
        {
            float offset = sizeTimesBaseScaleInHalf - sizeTimesNewScaleInHalf;
            return new Vector2(offset, offset);
        }
        private void DrawSkillPerksAnimations(SpriteBatch b, Texture2D texture, float _posX, float _posY, int hoveredSkill, float[] currentSkillPerkScale)
        {
            Rectangle lockRectangle = new(464, 208, 16, 16);
            float perk1Scale = 3f * (currentSkillPerkScale[0] + (HoveredSkillPerk != 0 && IsSkillPerkReadyNotUnlocked(hoveredSkill,0)? (float)PulsatingPerkScale: 0));
            float perk2Scale = 3f * (currentSkillPerkScale[1] + (HoveredSkillPerk != 1 && IsSkillPerkReadyNotUnlocked(hoveredSkill, 1) ? (float)PulsatingPerkScale : 0));
            float perk3Scale = 3f * (currentSkillPerkScale[2] + (HoveredSkillPerk != 2 && IsSkillPerkReadyNotUnlocked(hoveredSkill, 2) ? (float)PulsatingPerkScale : 0));
            float perk4Scale = 3f * (currentSkillPerkScale[3] + (HoveredSkillPerk != 3 && IsSkillPerkReadyNotUnlocked(hoveredSkill, 3) ? (float)PulsatingPerkScale : 0));
            Vector2 perk1Position = new Vector2(_posX + 20f, _posY + 4) + CalculateOffsetBasedOnScale(perk1Scale * 8, 24);
            Vector2 perk2Position = new Vector2(_posX + 88f, _posY) + CalculateOffsetBasedOnScale(perk2Scale * 8, 24);
            Vector2 perk3Position = new Vector2(_posX + 160f, _posY) + CalculateOffsetBasedOnScale(perk3Scale * 8, 24);
            Vector2 perk4Position = new Vector2(_posX + 228f, _posY + 4) + CalculateOffsetBasedOnScale(perk4Scale * 8, 24);

            switch (hoveredSkill)
            {
                case 0:
                    {
                        b.Draw(texture, perk1Position, IsSkillPerkUnlocked(hoveredSkill, 0) ? new Rectangle(544, 32, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 0 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk1Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk2Position, IsSkillPerkUnlocked(hoveredSkill, 1) ? new Rectangle(544, 32, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 1 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk2Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk3Position, IsSkillPerkUnlocked(hoveredSkill, 2) ? new Rectangle(544, 128, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 2 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk3Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk4Position, IsSkillPerkUnlocked(hoveredSkill, 3) ? new Rectangle(512, 208, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 3 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk4Scale, SpriteEffects.None, 0.875f);
                    }
                    break;
                case 1:
                    {
                        b.Draw(texture, perk1Position, IsSkillPerkUnlocked(hoveredSkill, 0) ? new Rectangle(544, 128, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 0 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk1Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk2Position, IsSkillPerkUnlocked(hoveredSkill, 1) ? new Rectangle(544, 192, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 1 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk2Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk3Position, IsSkillPerkUnlocked(hoveredSkill, 2) ? new Rectangle(544, 64, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 2 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk3Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk4Position, IsSkillPerkUnlocked(hoveredSkill, 3) ? new Rectangle(544, 0, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 3 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk4Scale, SpriteEffects.None, 0.875f);
                    }
                    break;
                case 2:
                    {
                        b.Draw(texture, perk1Position, IsSkillPerkUnlocked(hoveredSkill, 0) ? new Rectangle(544, 80, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 0 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk1Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk2Position, IsSkillPerkUnlocked(hoveredSkill, 1) ? new Rectangle(544, 16, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 1 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk2Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk3Position, IsSkillPerkUnlocked(hoveredSkill, 2) ? new Rectangle(544, 96, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 2 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk3Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk4Position, IsSkillPerkUnlocked(hoveredSkill, 3) ? new Rectangle(544, 112, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 3 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk4Scale, SpriteEffects.None, 0.875f);
                    }
                    break;
                case 3:
                    {
                        b.Draw(texture, perk1Position, IsSkillPerkUnlocked(hoveredSkill, 0) ? new Rectangle(544, 144, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 0 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk1Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk2Position, IsSkillPerkUnlocked(hoveredSkill, 1) ? new Rectangle(544, 48, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 1 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk2Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk3Position, IsSkillPerkUnlocked(hoveredSkill, 2) ? new Rectangle(544, 160, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 2 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk3Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk4Position, IsSkillPerkUnlocked(hoveredSkill, 3) ? new Rectangle(544, 176, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 3 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk4Scale, SpriteEffects.None, 0.875f);
                    }
                    break;
                case 4:
                    {
                        b.Draw(texture, perk1Position, IsSkillPerkUnlocked(hoveredSkill, 0) ? new Rectangle(528, 208, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 0 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk1Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk2Position, IsSkillPerkUnlocked(hoveredSkill, 1) ? new Rectangle(544, 208, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 1 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk2Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk3Position, IsSkillPerkUnlocked(hoveredSkill, 2) ? new Rectangle(528, 208, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 2 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk3Scale, SpriteEffects.None, 0.875f);
                        b.Draw(texture, perk4Position, IsSkillPerkUnlocked(hoveredSkill, 3) ? new Rectangle(496, 208, 16, 16) : lockRectangle, Color.White * (animatingSkillPerkUnlock == 3 ? currentSkillPerkAlpha : 1f), 0f, Vector2.Zero, perk4Scale, SpriteEffects.None, 0.875f);
                    }
                    break;
                default: break;
            }
            if (skillPerkUnlockSpriteData is not null)
            {
                (Vector2 pos, Vector2 basePos, bool unlocked, float alpha, Vector2 r) = skillPerkUnlockSpriteData.Value;
                b.Draw(texture, pos, !unlocked ? lockRectangle : new(480, 208, 16, 16), Color.White * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.875f);
            }
        }
    }

    public class SpriteRectangle
    {
        public float BaseX;
        public float BaseY;
        private float BaseHeight;
        private float BaseWidth;
        public Vector2 Position { get; private set; }
        public float Height { get; private set; }
        public float Width { get; private set; }
        public float BaseScale;
        public Rectangle SourceRect { get; private set; }
        public float CurrentScale { get; private set; }
        public SpriteRectangle(float x,float y, float baseScale, int sourceX, int sourceY, int sourceWidth, int sourceHeight)
        {
            this.Width = BaseWidth = sourceWidth * baseScale;
            this.Height = BaseHeight = sourceHeight * baseScale;
            this.BaseX = x;
            this.BaseY = y;
            this.Position = new Vector2(x,y);
            this.BaseScale = this.CurrentScale = baseScale;
            this.SourceRect = new(sourceX, sourceY, sourceWidth, sourceHeight);
        }
        public void ResetScale()
        {
            this.CurrentScale = this.BaseScale;
            this.Position = new(BaseX,BaseY);
            this.Width = this.BaseWidth;
            this.Height = this.BaseHeight;
        }
        public void UpdateSourceRect(Rectangle newSrcRect) => this.SourceRect = newSrcRect;
        public void UpdatePosition(float newX,float newY)
        {
            this.BaseX = newX;
            this.BaseY = newY;
            this.Position = new(newX,newY);
        }
        public bool Hovering(float x, float y,bool? condition = null)
        {
            if (condition is not null && !condition.Value) { return false; }
            bool response = (((this.BaseX <= x) && (x < (this.BaseX + this.BaseWidth))) && (this.BaseY <= y)) && (y < (this.BaseY + this.BaseHeight));
            return response;
        }
        public void InflateIfHoveredDeflateOtherwise(float maxScale, double deltaT, bool _beingHovered)
        {
            if (maxScale <= BaseScale) { return; }
            CurrentScale = (float)Math.Clamp(CurrentScale + (_beingHovered ? deltaT: -deltaT * 2), BaseScale, maxScale);
            float newScale = CurrentScale / BaseScale;
            this.Width = BaseWidth * newScale;
            this.Height = BaseHeight * newScale;
            this.Position = new(BaseX + ((BaseWidth - this.Width) * 0.5f), BaseY + ((BaseHeight - this.Height) * 0.5f));
        }
        /// <param name="switchSourceRect">Move the source rect to another position on the texture</param>
        public void Draw(SpriteBatch b,Texture2D texture,bool flipHorizontal = false,Vector2? customPos = null, Point? switchSourceRectPos = null)
        {
            b.Draw(texture, customPos is not null? customPos.Value: this.Position, switchSourceRectPos is not null? new Rectangle(switchSourceRectPos.Value.X, switchSourceRectPos.Value.Y, this.SourceRect.Width, this.SourceRect.Height) : this.SourceRect, Color.White, 0f, Vector2.Zero, this.CurrentScale, flipHorizontal? SpriteEffects.FlipHorizontally: SpriteEffects.None, 0.875f);
        }
    }

    public readonly struct SpriteInfo
    {
        public readonly Rectangle sourceRect;
        public readonly Vector2 position;
        public readonly float scale;

        public SpriteInfo(Vector2 pos, Rectangle srcRect, float _scale = 4f)
        {
            this.sourceRect = srcRect;
            this.position = pos;
            this.scale = _scale;
        }
        public void Draw(SpriteBatch b,Texture2D texture)
        {
            b.Draw(texture,  position, sourceRect, Color.White, 0f, Vector2.Zero, this.scale, SpriteEffects.None, 0.875f);
        }
    }

    public readonly struct StringSpriteInfo
    {
        public readonly Vector2 position;
        public readonly float scale;
        public readonly string text;
        public readonly Color color = Color.White;

        public StringSpriteInfo(Vector2 pos, string str, Color _color, float _scale = 4f)
        {
            this.position = pos;
            this.scale = _scale;
            this.color = _color;
            this.text = str;
        }
        public void Draw(SpriteBatch b, SpriteFont font)
        {
            b.DrawString(font, text, position, color, 0f, Vector2.Zero, this.scale, SpriteEffects.None, 0.875f);
        }
    }
}
