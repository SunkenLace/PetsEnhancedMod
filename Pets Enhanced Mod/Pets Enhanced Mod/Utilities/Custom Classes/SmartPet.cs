using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pets_Enhanced_Mod.Data;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using System.Diagnostics;
using StardewValley.GameData.Pets;
using StardewValley.Locations;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using static Pets_Enhanced_Mod.Utilities.SunkenLaceUtilities;
using static Pets_Enhanced_Mod.ModEntry;
using System.Text.RegularExpressions;
using StardewValley.SpecialOrders;
using StardewValley.GameData.Locations;
using StardewValley.GameData;
using StardewValley.Internal;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StardewValley.GameData.WildTrees;
using System.Diagnostics.Metrics;
using Microsoft.Xna.Framework.Input;
using static Pets_Enhanced_Mod.Utilities.CachePetData;
using xTile.Tiles;
using xTile;
using xTile.Layers;
using System.Collections;
using StardewValley.ItemTypeDefinitions;
using System.IO;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework.Media;
using System.Threading;
using System.Text;
using static Pets_Enhanced_Mod.Utilities.Custom_Classes.SmartPet;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using Pets_Enhanced_Mod.Multiplayer;
using StardewValley.Network;
using StardewValley.Inventories;

namespace Pets_Enhanced_Mod.Utilities.Custom_Classes
{
    public class SmartPet
    {
        public enum PetObjective
        {
            None = 0,
            Follow = 1,
            Wait = 2,
            Hunt = 3,
            Forage = 4,
            Fishing = 5
        }
        public enum Directions
        {
            NonDefined = -1,
            North = 0,
            South = 1,
            East = 2,
            West = 3
        }
        public enum PetState
        {
            Sit,
            Walking,
            Sprint,
            Sleeping,
            Still,
            NonDefined,
            Resting,
            PreparingToJump,
            Jumping,
            Swimming
        }
        public enum SmartPetBehaviour
        {
            Sleeping,
            Sit,
            Sprint,
            Walk,
            NotDefined

        }
        public enum PetAction { Follow, Hunt, Forage, Wait, Fishing, None }

        public const float emoteBeginInterval = 20f;
        public const float emoteNormalInterval = 250f;

        public const int emptyCanEmote = 4;
        public const int questionMarkEmote = 8;
        public const int angryEmote = 12;
        public const int exclamationEmote = 16;
        public const int heartEmote = 20;
        public const int sleepEmote = 24;
        public const int sadEmote = 28;
        public const int happyEmote = 32;
        public const int xEmote = 36;
        public const int pauseEmote = 40;
        public const int videoGameEmote = 52;
        public const int musicNoteEmote = 56;
        public const int blushEmote = 60;

        public const int blockedIntervalBeforeEmote = 3000;
        public const int blockedIntervalBeforeSprint = 5000;

        public const string RoughCollarQUID = "(O)SunkenLace.PetsEnhancedMod.RoughCollar";
        public const string LuxuryCollarQUID = "(O)SunkenLace.PetsEnhancedMod.LuxuryCollar";
        public const string FloweryCollarQUID = "(O)SunkenLace.PetsEnhancedMod.FloweryCollar";
        public const string LightweightCollarQUID = "(O)SunkenLace.PetsEnhancedMod.LightweightCollar";
        public const string SeagoingCollarQUID = "(O)SunkenLace.PetsEnhancedMod.SeagoingCollar";
        public const string PetFluteQUID = "(T)SunkenLace.PetsEnhancedMod.PetFlute";
        public const string PetManualQUID = "(T)SunkenLace.PetsEnhancedMod.PetManual";
        public const string PetBackpackQUID = "(O)SunkenLace.PetsEnhancedMod.PetBackpack";

        public const int MaxBaseEnergyNoUpgrade = 270;
        public const int bedTime = 2000;
        public const int MaxDistanceBeforeTeleport = 15;
        public SmartPet(int xTile, int yTile, Pet _original, PetInfo petInfo)
        {
            this._petInfo = petInfo;
            this.OriginalPetInstance = _original;
            this.Sprite = new AnimatedSprite(_original.getPetTextureName(), 0, 32, 32);
            this.CurrentLocation = _original.currentLocation;
            this.Position = new Vector2(xTile, yTile) * 64f;
            this.petType = petInfo.PetType;
            this.animations = new(this);
            this.dummyNPCForCollisionChecking = new Character(new AnimatedSprite(_original.getPetTextureName(), 0, 32, 32), new Vector2(-5000, -5000), 0, "sunkenLace.dummyPetNPC"); //Name needs to have the "NPC" keyword in order to exit the farm
            this.Position = _original.Position + new Vector2(32, 0);
            this.facingDirection = _original.FacingDirection;
            this.CurrentPetObjective = SmartPet.PetObjective.None;
            this.Sprite.CurrentFrame = _original.Sprite.CurrentFrame;
            this.GlobalPetInventoryKey = SynchronizationManager.PetsEnhancedModPetInventoryStoragePasscode + petInfo.PetId;
            this.peData = CachePetData.GetPetDataForPet(_original);
        }
        public SmartPet() { }

        /// <summary>The original instance that was used on the creation of this one.</summary>
        public Pet OriginalPetInstance;


        public readonly string GlobalPetInventoryKey;

        public AnimatedSprite Sprite;
        public Texture2D SPTexture;
        public readonly PetAnimations animations;
        public PetAction CurrentPetAction = PetAction.None;
        public GameLocation CurrentLocation;
        public Vector2 Position;
        public float xVelocity;
        public float yVelocity;
        public PetInfo.Pet_Types petType;
        public PE_Pet_Data peData = new();
        public Vector2 Motion { get; set; } = new Vector2(0, 0);
        public bool Swimming = false;
        public bool BoardSinking = false;
        public int BoardBounceModifier = 0;
        public int defenceBreakCounter = 0;
        public bool setPetAttacking = false;
        public string WhichBreed => this.SmartPetBreed;
        public bool flip = false;
        public int AttackCooldown = 0;
        public Item PetGiftThatCouldntBeSavedOnItsInventory = null;
        public TerrainFeature targetObject;
        public readonly PetInfo _petInfo;
        public Character dummyNPCForCollisionChecking;
        public (int negative, int positive) foodSaturationLevel = (0, 0);
        public int foodSaturationTimer = 0;
        public readonly Stack<Point> pathToFollow = new();
        public Vector2 OldLeaderPosition;

        private bool PetLearnedANewSkill = false;
        public bool GetPetLearnedANewSkill() => this.PetLearnedANewSkill;
        public void SetPetLearnedANewSkill(bool value) => this.PetLearnedANewSkill = value;

        private bool NewItemHasBeenObtained = false;
        public bool GetNewItemHasBeenObtained() => this.NewItemHasBeenObtained;
        public void SetNewItemHasBeenObtained(bool value) => this.NewItemHasBeenObtained = value;

        public int emoteYOffset = 0;
        public bool isEmoting = false;
        public int currentEmote;
        public int currentEmoteFrame = 0;
        public float emoteInterval = 0f;
        public bool EmoteFading = false;
        public int emoteCooldown = 0;


        public Point CachedStandingPixel = new Point(0, 0);
        public Vector2 pixelPositionForCachedStandingPixel = new Vector2(0, 0);
        /// <summary>The pixel coordinates at the center of this character's bounding box, relativeto the top-left corner of the map.</summary>
        public Point StandingPixel
        {
            get
            {
                if (Position.X != pixelPositionForCachedStandingPixel.X || Position.Y != pixelPositionForCachedStandingPixel.Y)
                {
                    CachedStandingPixel = GetBoundingBox().Center;
                    pixelPositionForCachedStandingPixel = Position;
                }

                return CachedStandingPixel;
            }
        }
        public Rectangle GetBoundingBox()
        {
            Vector2 vector = this.Position;
            return ((this.petType == PetInfo.Pet_Types.EnhancedDog || this.petType == PetInfo.Pet_Types.LegacyDog) && this.CurrentPetState == PetState.Sleeping) ? new Rectangle((int)vector.X - 6, (int)vector.Y - 10, 64, 52) : new Rectangle((int)vector.X + 18, (int)vector.Y + 18, 28, 28);
        }
        public static Rectangle GetBoundingBoxFixed(int x, int y)
        {
            return new Rectangle(x + 18, y + 18, 28, 28);
        }

        /// <summary>The character's tile position within their current location.</summary>
        public Vector2 Tile
        {
            get
            {
                if (Position.X != pixelPositionForCachedTile.X || Position.Y != pixelPositionForCachedTile.Y)
                {
                    Point standingPixel = StandingPixel;
                    cachedTile = new Vector2(standingPixel.X / 64, standingPixel.Y / 64);
                    pixelPositionForCachedTile = Position;
                }

                return cachedTile;
            }
        }
        public Vector2 pixelPositionForCachedTile = new Vector2(0, 0);
        public Vector2 cachedTile = new Vector2(0, 0);
        public Vector2 pixelPositionForCachedTilePoint = new Vector2(0, 0);
        public Point cachedTilePoint = new Point(0, 0);
        public Point TilePoint
        {
            get
            {
                if (Position.X != pixelPositionForCachedTilePoint.X || Position.Y != pixelPositionForCachedTilePoint.Y)
                {
                    Vector2 tile = Tile;
                    cachedTilePoint = new Point((int)tile.X, (int)tile.Y);
                    pixelPositionForCachedTilePoint = Position;
                }

                return cachedTilePoint;
            }
        }
        public float DistanceFromGroupLeader
        {
            get
            {
                if (this.GroupLeader is not null && this.CurrentLocation.NameOrUniqueName.EqualsIgnoreCase(this.GroupLeader.currentLocation.NameOrUniqueName))
                {
                    return MathF.Max(0, Utility.distance(this.StandingPixel.X, this.GroupLeader.StandingPixel.X, this.StandingPixel.Y, this.GroupLeader.StandingPixel.Y));
                }

                return -1;
            }
        }

        public int petSearchPatienceTimer = 0;
        public int petSearchCooldown = 0;
        private int switchDirectionSpeed;
        public int ForageCooldownTimer = 0;

        public Character target = null;

        public Farmer GroupLeader = null;

        public SmartPet PetLeader = null;

        public Farmer HumanLeader = null;

        public Guid? Teammate1ID = null;
        public Guid? Teammate2ID = null;


        public Vector2? targetTile = null;

        public PetObjective CurrentPetObjective = PetObjective.None;

        public PetObjective PrevPetObjective = PetObjective.None;

        public PetState PrevPetState = PetState.NonDefined;

        public PetState CurrentPetState = PetState.NonDefined;

        public PetState ActualPetState
        {
            get
            {
                Dictionary<PetState, List<int>> Sprites = new Dictionary<PetState, List<int>>();
                if (this._petInfo.PetType == PetInfo.Pet_Types.EnhancedCat || this._petInfo.PetType == PetInfo.Pet_Types.LegacyCat)
                {
                    Sprites.Add(PetState.Sit, new() { 17, 18, 19, 20, 21, 22, 23, 32, 33, 34, 35, 36, 37, 38, 40, 41, 42, 43, 44, 45, 46, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58 });
                    Sprites.Add(PetState.Still, new() { 0, 2, 4, 6, 8, 10, 12, 14, 16 });
                    Sprites.Add(PetState.Sleeping, new() { 28, 29 });
                    Sprites.Add(PetState.Resting, new() { 24, 25, 26, 27 });
                    Sprites.Add(PetState.Walking, new() { 1, 3, 5, 7, 9, 11, 13, 15 });
                    Sprites.Add(PetState.PreparingToJump, new() { 30, 31 });
                    Sprites.Add(PetState.Swimming, new() { 59, 60, 61, 62, 63 });
                }
                else
                {
                    Sprites.Add(PetState.Sit, new() { 17, 18, 19, 21, 22, 23, 24, 25, 26, 27, 30, 31, 36, 37, 38, 40, 41 });
                    Sprites.Add(PetState.Sprint, new() { 32, 33, 34 });
                    Sprites.Add(PetState.Sleeping, new() { 28, 29 });
                    Sprites.Add(PetState.Still, new() { 0, 2, 4, 6, 8, 10, 12, 14, 16, 20 });
                    Sprites.Add(PetState.Walking, new() { 1, 3, 5, 7, 9, 11, 13, 15 });
                }

                foreach (var State in Sprites)
                {
                    foreach (int frame in State.Value)
                    {
                        if (this.Sprite.currentFrame == frame)
                        {
                            return State.Key;
                        }
                    }

                }
                return SmartPet.PetState.NonDefined;

            }
        }

        public SmartPetBehaviour CurrentBehavior = SmartPetBehaviour.Sleeping;

        public SmartPetBehaviour PrevBehavior = SmartPetBehaviour.NotDefined;

        public Directions ActualPetDirection
        {
            get
            {
                if (this.Sprite is not null)
                {
                    if (this._petInfo.PetType == PetInfo.Pet_Types.EnhancedCat || this._petInfo.PetType == PetInfo.Pet_Types.LegacyCat)
                    {
                        switch (this.Sprite.CurrentFrame)
                        {
                            case 8 or 9 or 10 or 11 or 59:
                                return Directions.North;
                            case 0 or 1 or 2 or 3 or 16 or 17 or 18 or 19 or 20 or 21 or 22 or 23 or 28 or 29 or 60 or 61:
                                return Directions.South;
                            case 12 or 13 or 14 or 15:
                                return this.flip ? Directions.East : Directions.West;
                            case 4 or 5 or 6 or 7 or 24 or 25 or 26 or 27 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 37 or 38 or 39 or 40 or 41 or 42 or 43 or 44 or 45 or 46 or 47 or 48 or 49 or 50 or 51 or 52 or 53 or 54 or 55 or 56 or 57 or 58 or 62 or 63:
                                return this.flip ? Directions.West : Directions.East;

                        }
                    }
                    else
                    {
                        switch (this.Sprite.CurrentFrame)
                        {
                            case 8 or 9 or 10 or 11:
                                return Directions.North;
                            case 0 or 1 or 2 or 3 or 16 or 17 or 18 or 19 or 27 or 28 or 29 or 36 or 37 or 38 or 42:
                                return Directions.South;
                            case 12 or 13 or 14 or 15:
                                return this.flip ? Directions.East : Directions.West;
                            case 4 or 5 or 6 or 7 or 20 or 21 or 22 or 23 or 24 or 25 or 26 or 30 or 31 or 32 or 33 or 34 or 40 or 41 or 43:
                                return this.flip ? Directions.West : Directions.East;

                        }
                    }


                    return Directions.NonDefined;
                }
                else
                {
                    return Directions.NonDefined;
                }
            }

        }

        public static int GetActualPetDirection(int frame, PetInfo.Pet_Types petType, bool flip)
        {
            if (petType == PetInfo.Pet_Types.EnhancedCat || petType == PetInfo.Pet_Types.LegacyCat)
            {
                switch (frame)
                {
                    case 8 or 9 or 10 or 11 or 59:
                        return 0;
                    case 0 or 1 or 2 or 3 or 16 or 17 or 18 or 19 or 20 or 21 or 22 or 23 or 28 or 29 or 60 or 61:
                        return 2;
                    case 12 or 13 or 14 or 15:
                        return flip ? 1 : 3;
                    case 4 or 5 or 6 or 7 or 24 or 25 or 26 or 27 or 30 or 31 or 32 or 33 or 34 or 35 or 36 or 37 or 38 or 39 or 40 or 41 or 42 or 43 or 44 or 45 or 46 or 47 or 48 or 49 or 50 or 51 or 52 or 53 or 54 or 55 or 56 or 57 or 58 or 62 or 63:
                        return flip ? 3 : 1;

                }
            }
            else
            {
                switch (frame)
                {
                    case 8 or 9 or 10 or 11:
                        return 0;
                    case 0 or 1 or 2 or 3 or 16 or 17 or 18 or 19 or 27 or 28 or 29 or 36 or 37 or 38 or 42:
                        return 2;
                    case 12 or 13 or 14 or 15:
                        return flip ? 1 : 3;
                    case 4 or 5 or 6 or 7 or 20 or 21 or 22 or 23 or 24 or 25 or 26 or 30 or 31 or 32 or 33 or 34 or 40 or 41 or 43:
                        return flip ? 3 : 1;

                }
            }
            return 2;
        }



        public Vector2 oldPetTile;
        public string SmartPetBreed
        {
            get
            {
                if (OriginalPetInstance is not null)
                {
                    return OriginalPetInstance.whichBreed.Value;
                }
                return "0";
            }
        }

        public int facingDirection;

        public int timer;



        public bool LockMovement = false;

        public bool movingOnPush = false;

        public int IdleTimer = 0;

        public bool readyToBeIddle = false;

        public void UnlockSkills()
        {
            if (this._petInfo.SkillMastery_level[0] < 0)
            {
                this._petInfo.SkillMastery_level[0] = 0;
            }
            if (this._petInfo.SkillMastery_level[1] < 0)
            {
                this._petInfo.SkillMastery_level[1] = 0;
            }
            if (this._petInfo.SkillMastery_level[2] < 0)
            {
                this._petInfo.SkillMastery_level[2] = 0;
            }
            if (this._petInfo.SkillMastery_level[3] < 0)
            {
                this._petInfo.SkillMastery_level[3] = 0;
            }
            if (this._petInfo.SkillMastery_level[4] < 0)
            {
                this._petInfo.SkillMastery_level[4] = 0;
            }
        }
        public virtual void faceDirection(int direction)
        {
            if (direction != -3)
            {
                Sprite?.faceDirection(direction);
            }
        }
        private void OnLeaderWarp(Farmer _player)
        {
            this.CurrentLocation = _player.currentLocation;
            this.Position = _player.getStandingPosition() - new Vector2(32, 32);
            this.facingDirection = _player.FacingDirection;
            this.faceDirection(this.facingDirection);
            this.Motion = Vector2.Zero;
            this.pathToFollow.Clear();
            ResetTargets();
            this.petSearchCooldown = petSearchPatienceTimer = BoardBounceModifier = 0;
            this.Swimming = this.BoardSinking = this.LockMovement = false;

        }
        private void OnLeaderWarp(int _facingDirection, Vector2 _position, string _locationNameOrUniqueName)
        {
            this.CurrentLocation = Game1.getLocationFromName(_locationNameOrUniqueName);
            this.Position = _position - new Vector2(32, 32);
            this.facingDirection = _facingDirection;
            this.faceDirection(this.facingDirection);
            this.Motion = Vector2.Zero;
            this.pathToFollow.Clear();
            ResetTargets();
            this.petSearchCooldown = petSearchPatienceTimer = BoardBounceModifier = 0;
            this.Swimming = this.BoardSinking = this.LockMovement = false;
        }
        public int GetFacingDirectionFromMovement(Vector2 movement, bool following)
        {
            int dir = this.facingDirection;
            if (!following)
            {
                if (Math.Abs(movement.X) > Math.Abs(movement.Y))
                {
                    dir = ((movement.X > 1f) ? 1 : (movement.X < -1f) ? 3 : this.facingDirection);
                }
                else if (Math.Abs(movement.X) < Math.Abs(movement.Y))
                {
                    dir = ((movement.Y > 1f) ? 2 : (movement.Y < -1f) ? 0 : this.facingDirection);
                }
            }
            else
            {
                if (Math.Abs(movement.X) > Math.Abs(movement.Y))
                {
                    dir = ((movement.X > 0f) ? 1 : 3);
                }
                else if (Math.Abs(movement.X) < Math.Abs(movement.Y))
                {
                    dir = ((movement.Y > 0f) ? 2 : 0);
                }
            }
            return dir;
        }
        private void AnimationSubUpdate(Vector2 velocity, GameTime time, bool following, int intervalOffset)
        {
            this.switchDirectionSpeed++;
            if ((!following && this.switchDirectionSpeed >= 10) || (following && this.switchDirectionSpeed >= 5))
            {
                this.facingDirection = this.GetFacingDirectionFromMovement(velocity, following);
                this.switchDirectionSpeed = 0;
            }
            if (!Swimming)
            {
                if ((int)velocity.X != 0 || (int)velocity.Y != 0)
                {
                    switch (this.facingDirection)
                    {
                        case 0:
                            this.Sprite.AnimateUp(time, intervalOffset);
                            break;
                        case 1:
                            this.Sprite.AnimateRight(time, intervalOffset);
                            break;
                        case 2:
                            this.Sprite.AnimateDown(time, intervalOffset);
                            break;
                        case 3:
                            this.Sprite.AnimateLeft(time, intervalOffset);
                            break;
                        default:
                            break;
                    }
                }
                else if ((int)velocity.X == 0 && (int)velocity.Y == 0)
                {
                    switch (this.facingDirection)
                    {
                        case 0:
                            this.Sprite.currentFrame = 8;
                            break;
                        case 1:
                            this.Sprite.currentFrame = 4;
                            break;
                        case 2:
                            this.Sprite.currentFrame = 0;
                            break;
                        case 3:
                            this.Sprite.currentFrame = 12;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                this.BoardBounceModifier = Math.Abs((int)velocity.X) > Math.Abs((int)velocity.Y) ? Math.Abs((int)velocity.X) : Math.Abs((int)velocity.Y);
            }

        }
        private void SetMovementDirectionAnimation(Directions dir, GameTime time, int intervalOffset)
        {
            switch (dir)
            {
                case Directions.North:
                    this.Sprite.AnimateUp(time, intervalOffset);
                    break;
                case Directions.East:
                    this.Sprite.AnimateRight(time, intervalOffset);
                    break;
                case Directions.South:
                    this.Sprite.AnimateDown(time, intervalOffset);
                    break;
                case Directions.West:
                    this.Sprite.AnimateLeft(time, intervalOffset);
                    break;

            }
        }
        public void doEmote(int whichEmote)
        {
            if (!isEmoting && !Game1.eventUp)
            {
                emoteYOffset = 0;
                isEmoting = true;
                currentEmote = whichEmote;
                currentEmoteFrame = 0;
                emoteInterval = 0f;
            }
        }
        private void ReachedForageSource(TerrainFeature target, ref int currentTick, ref int currentFrameIndex, ref double forageSkillProficiency, bool hasFloweryCollar, bool isLucky)
        {
            if (currentFrameIndex == 0)
            {
                currentFrameIndex = Game1.random.NextDouble() < ((hasFloweryCollar ? 0.35d : 0.2d) + (isLucky ? 0.07d : 0d)) ? -1 : 1;
                currentTick = 170;
                SetFacingRectangleStillPose(targetObject.getBoundingBox());
            }
            switch (currentTick)
            {

                case 190:
                    {
                        if (currentFrameIndex == -1)
                        {
                            if (isEmoting)
                            {
                                currentTick--;
                            }
                            else
                            {
                                this.doEmote(exclamationEmote);
                            }
                        }
                    }
                    break;
                case 210:
                    {
                        int energyConsumed = -6 + (this._petInfo.SkillMastery_level[0] >= 4 ? -1 : 0) + (this._petInfo.SkillMastery_level[1] >= 2 ? -1 : 0);
                        ChangeEnergyLevel(energyConsumed, this.GroupLeader);
                        IfClientSendMessageElseDoAction(this.GroupLeader, () => target.performUseAction(target.Tile), new KeyValuePair<string, Vector2>(this.CurrentLocation.NameOrUniqueName, target.Tile), "PerformUseActionOnTerrainFeature", new[] { this.GroupLeader.UniqueMultiplayerID });
                    }
                    break;
                case 220:
                    {
                        bool foraged = false;
                        if (currentFrameIndex == -1)
                        {
                            int luxuryCollarMultiplier = 0;
                            foreach (var collar in CachePetData.CachePetTeams.GetCollarsWornByTeam(this.GroupLeader.UniqueMultiplayerID))
                            {
                                if (!string.IsNullOrEmpty(collar) && collar.Equals(luxuryCollarMultiplier))
                                {
                                    luxuryCollarMultiplier++;
                                }
                            }
                            if (this.PickForageObject(ref forageSkillProficiency, hasFloweryCollar, luxuryCollarMultiplier, isLucky))
                            {
                                foraged = true;
                            }
                        }
                        if (!foraged) { currentTick = 280; }
                        CachePetData.CachePetTeams.AddTerrainFeatureToIgnoreList(this.targetObject.getBoundingBox());
                    }
                    break;

            }
        }
        private static bool CheckGenericFishRequirements(Item fish, Dictionary<string, string> allFishData, GameLocation location, double fishingSkillMastery, SpawnFishData spawn, int waterDepth, bool hasSeagoingCollar)
        {
            if (!fish.HasTypeObject() || !allFishData.TryGetValue(fish.ItemId, out var value))
            {
                return true;
            }

            string[] array = value.Split('/');
            if (ArgUtility.Get(array, 1) == "trap")
            {
                return true;
            }

            if (!spawn.IgnoreFishDataRequirements)
            {
                if (!ArgUtility.TryGet(array, 5, out var value4, out var error4, allowBlank: true, "string rawTimeSpans"))
                {
                    return false;
                }

                string[] array2 = ArgUtility.SplitBySpace(value4);
                bool flag2 = false;
                for (int i = 0; i < array2.Length; i += 2)
                {
                    if (!ArgUtility.TryGetInt(array2, i, out var value5, out error4, "int startTime") || !ArgUtility.TryGetInt(array2, i + 1, out var value6, out error4, "int endTime"))
                    {
                        return false;
                    }

                    if (Game1.timeOfDay >= value5 && Game1.timeOfDay < value6)
                    {
                        flag2 = true;
                        break;
                    }
                }

                if (!flag2)
                {
                    return false;
                }
                if (!ArgUtility.TryGet(array, 7, out var value7, out var error5, allowBlank: true, "string weather"))
                {
                    return false;
                }

                if (!(value7 == "rainy"))
                {
                    if (value7 == "sunny" && location.IsRainingHere())
                    {
                        return false;
                    }
                }
                else if (!location.IsRainingHere())
                {
                    return false;
                }

                if (!ArgUtility.TryGetInt(array, 12, out var value8, out var error6, "int minFishingLevel"))
                {
                    return false;
                }

                if ((fishingSkillMastery * 2) + (hasSeagoingCollar ? 2 : 0) < value8)
                {
                    return false;
                }

                if (!ArgUtility.TryGetInt(array, 9, out var value9, out var error7, "int maxDepth") || !ArgUtility.TryGetFloat(array, 10, out var value10, out error7, "float chance") || !ArgUtility.TryGetFloat(array, 11, out var value11, out error7, "float depthMultiplier"))
                {
                    return false;
                }

                float num = value11 * value10;
                value10 -= (float)Math.Max(0, value9 - waterDepth) * num;
                value10 += (float)(fishingSkillMastery + (hasSeagoingCollar ? 1 : 0)) / 25f;

                value10 = Math.Min(value10, 0.9f);

                if (spawn.ApplyDailyLuck)
                {
                    value10 += (float)Game1.player.DailyLuck;
                }

                List<QuantityModifier> chanceModifiers = spawn.ChanceModifiers;
                if (chanceModifiers != null && chanceModifiers.Count > 0)
                {
                    value10 = Utility.ApplyQuantityModifiers(value10, spawn.ChanceModifiers, spawn.ChanceModifierMode, location);
                }

                if (!Game1.random.NextBool(value10))
                {
                    return false;
                }
            }

            return true;
        }
        private static Item GetCrabPotFish(Vector2 tileLocation, int quantity, int quality, GameLocation location) //Memory allocated by code: 14,624 bytes
        {
            if (location is Caldera || location is VolcanoDungeon || location is MineShaft) { return null; }
            IList<string> targetAreas = location.GetCrabPotFishForTile(tileLocation);
            //CacheReciclerHelper.GetAllocatedMemoryStart();
            var fishContent = DataLoader.Fish(Game1.content);
            //CacheReciclerHelper.GetAllocatedMemoryEnd(true, false);
            foreach (KeyValuePair<string, string> v in fishContent)
            {
                if (!v.Value.Contains("trap"))
                {
                    continue;
                }
                string[] rawSplit = v.Value.Split('/');
                string[] array = ArgUtility.SplitBySpace(rawSplit[4]);
                bool found = false;
                string[] array2 = array;
                foreach (string crabPotArea in array2)
                {
                    foreach (string targetArea in targetAreas)
                    {
                        if (crabPotArea == targetArea)
                        {
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    continue;
                }
                double chanceForCatch = Convert.ToDouble(rawSplit[2]);
                if (!(Game1.random.NextDouble() < chanceForCatch))
                {
                    continue;
                }
                //CacheReciclerHelper.GetAllocatedMemoryStart();
                var _item = ItemRegistry.Create("(O)" + v.Key, quantity, quality);
                //CacheReciclerHelper.GetAllocatedMemoryEnd(true, false);
                return _item;
            }
            return null;
        }

        /// <param name="playerFishingLvl">Determines the chance to get trash</param>
        /// <param name="fishingCollarsOnParty">Increases chances to get better quality.</param>
        /// <param name="luckCollarsOnParty">Determines the chance to get double catch.</param>
        /// <param name="todaysLuckLvl">Determines the chance to get more or less trash.</param>
        /// <param name="playersLuckLvl">Determines the chance to get double catch.</param>
        /// <param name="petFishingProficiency">Determines the quality of the fish caught.</param>
        /// <returns></returns>
        private static Item GetFishFromLocationData(Dictionary<string, string> _allFishData, string locationName, Vector2 bobberTile, int waterDepth, Point fishermanTile, GameLocation location, double fishingSkillMastery, bool hasLuckyBuff, bool hasSeagoingCollar, Farmer groupLeader, bool ignoreLegendaryFish = true)
        {
            if (location == null)
            {
                location = Game1.getLocationFromName(locationName);
            }

            LocationData locationData = ((location != null) ? location.GetData() : GameLocation.GetData(locationName));
            Season seasonForLocation = Game1.GetSeasonForLocation(location);
            if (location == null || !location.TryGetFishAreaForTile(bobberTile, out var id, out var _))
            {
                id = null;
            }
            string text = null;
            bool flag2 = false;
            Point tilePoint = fishermanTile;

            IEnumerable<SpawnFishData> enumerable = Game1.locationData["Default"].Fish;
            if (locationData != null && locationData.Fish?.Count > 0)
            {
                enumerable = enumerable.Concat(locationData.Fish);
            }

            enumerable = from p in enumerable
                         orderby p.Precedence, Game1.random.Next()
                         select p;
            int num = 0;
            HashSet<string> ignoreQueryKeys = null;
            Item item = null;
            for (int i = 0; i < 2; i++)
            {
                foreach (SpawnFishData spawn in enumerable)
                {
                    if ((spawn.FishAreaId != null && id != spawn.FishAreaId) || (spawn.Season.HasValue && spawn.Season != seasonForLocation))
                    {
                        continue;
                    }

                    if (spawn.PlayerPosition.HasValue && !spawn.PlayerPosition.GetValueOrDefault().Contains(tilePoint.X, tilePoint.Y))
                    {
                        continue;
                    }

                    if ((spawn.BobberPosition.HasValue && !spawn.BobberPosition.GetValueOrDefault().Contains((int)bobberTile.X, (int)bobberTile.Y)) || (fishingSkillMastery * 2) + (hasSeagoingCollar ? 1 : 0) < spawn.MinFishingLevel || waterDepth < spawn.MinDistanceFromShore || (spawn.MaxDistanceFromShore > -1 && waterDepth > spawn.MaxDistanceFromShore) || (spawn.RequireMagicBait))
                    {
                        continue;
                    }

                    float chance = spawn.GetChance(hasLuckyBuff, groupLeader.DailyLuck, groupLeader.LuckLevel + (hasSeagoingCollar ? 1 : 0), (float value, IList<QuantityModifier> modifiers, QuantityModifier.QuantityModifierMode mode) => Utility.ApplyQuantityModifiers(value, modifiers, mode, location), false);
                    if (spawn.UseFishCaughtSeededRandom)
                    {
                        if (!Utility.CreateRandom(Game1.uniqueIDForThisGame, groupLeader.stats.Get("PreciseFishCaught") * 859).NextBool(chance))
                        {
                            continue;
                        }
                    }
                    else if (!Game1.random.NextBool(chance))
                    {
                        continue;
                    }

                    if (spawn.Condition != null && !GameStateQuery.CheckConditions(spawn.Condition, location, null, null, null, null, ignoreQueryKeys))
                    {
                        continue;
                    }

                    Item item2 = ItemQueryResolver.TryResolveRandomItem(spawn, null, avoidRepeat: false, null, (string query) => query.Replace("BOBBER_X", ((int)bobberTile.X).ToString()).Replace("BOBBER_Y", ((int)bobberTile.Y).ToString()).Replace("WATER_DEPTH", waterDepth.ToString()), null, delegate (string query, string error)
                    {
                    });
                    if (item2 == null || spawn.IsBossFish)
                    {
                        continue;
                    }
                    if (!string.IsNullOrWhiteSpace(spawn.SetFlagOnCatch))
                    {
                        item2.SetFlagOnPickup = spawn.SetFlagOnCatch;
                    }

                    Item item3 = item2;
                    if ((spawn.CatchLimit <= -1 || !groupLeader.fishCaught.TryGetValue(item3.QualifiedItemId, out var value2) || value2[0] < spawn.CatchLimit) && CheckGenericFishRequirements(item3, _allFishData, location, fishingSkillMastery, spawn, waterDepth, hasSeagoingCollar))
                    {
                        if (text == null || !(item3.QualifiedItemId != text) || num >= 2)
                        {
                            return item3;
                        }

                        if (item == null)
                        {
                            item = item3;
                        }

                        num++;
                    }
                }

                if (!flag2)
                {
                    i++;
                }
            }

            if (item is not null)
            {
                return item;
            }

            return ItemRegistry.Create("(O)145");
        }

        const int fishingBubblesID = 594652790;
        private void ReachedFishingSource(ref int timeOutTimer, ref int animationTimerStarted, Vector2 bubbleLocation, bool hasSeagoingCollar, bool isLucky)
        {
            if (animationTimerStarted >= 0)
            {
                animationTimerStarted = -100;
                timeOutTimer = 50;
                this.facingDirection = getGeneralDirectionTowards(bubbleLocation);
                this.faceDirection(this.facingDirection);
            }
            timeOutTimer = (timeOutTimer > 200 && animationTimerStarted == -105) || (timeOutTimer > 150 && animationTimerStarted == -100) ? 400 : timeOutTimer;
            switch (timeOutTimer)
            {
                case 70:
                    {
                        PetHelper.PlaySoundForAllPlayersAtFarmerLocation("swordswipe", this.Tile, this.GroupLeader);
                        Vector2 slashOffset = this.facingDirection == 0 ? new Vector2(-96, -192f) : this.facingDirection == 1 ? new Vector2(0f, -96) : this.facingDirection == 3 ? new Vector2(-192f, -96) : new Vector2(-96, 0);
                        Game1.Multiplayer.broadcastSprites(this.CurrentLocation, new[] { new TemporaryAnimatedSprite(15, this.StandingPixel.ToVector2() + slashOffset, Color.White, 3, flipped: false, 60f, 0, 128, 1f, 128, 0)
                            {
                                    scale = 1.5f,
                                    rotation = this.facingDirection == 0? 180 : this.facingDirection == 2? 90 : 0,
                                    flipped = this.facingDirection == 3,
                                    layerDepth = (float)(this.GetBoundingBox().Bottom + 1) / 10000f,
                                    verticalFlipped = this.facingDirection == 0
                             }});
                        int luxuryCollarMultiplier = 0;
                        foreach (var collar in CachePetData.CachePetTeams.GetCollarsWornByTeam(this.GroupLeader.UniqueMultiplayerID))
                        {
                            if (!string.IsNullOrEmpty(collar) && collar.Equals(luxuryCollarMultiplier))
                            {
                                luxuryCollarMultiplier++;
                            }
                        }
                        Point _bubbleLocationTile = (bubbleLocation / Game1.tileSize).ToPoint();
                        double chanceToGetFish = (0.3d + (hasSeagoingCollar ? 0.1d : 0d) + (isLucky ? 0.07d : 0d));
                        if (Game1.random.NextDouble() < chanceToGetFish)
                        {
                            GetSomeFish(ref animationTimerStarted, bubbleLocation, PetHelper.BobberPointDistanceToLand(_bubbleLocationTile.X, _bubbleLocationTile.Y, this.CurrentLocation), isLucky, luxuryCollarMultiplier, hasSeagoingCollar, chanceToGetFish);
                        }
                        else if (this._petInfo.SkillMastery_level[3] >= 3 && Game1.random.NextDouble() < 0.1d + (hasSeagoingCollar ? 0.05d : 0d) && !GetCrabPotFish(bubbleLocation / Game1.tileSize, 1, 0, this.CurrentLocation).IsNull(out var _crabPotFish))
                        {
                            GetSomeCrabPotFish(ref animationTimerStarted, bubbleLocation, isLucky, luxuryCollarMultiplier, hasSeagoingCollar, _crabPotFish);
                        }

                        this._petInfo.SkillMastery_level[3] = Math.Min(this._petInfo.SkillMastery_level[3] + CalculateExpForSkillMastery(0.4d, _petInfo.SkillMastery_level[3]), 5);

                        PetHelper.PlaySoundForAllPlayersAtFarmerLocation("pullItemFromWater", bubbleLocation / Game1.tileSize, this.GroupLeader);

                        Game1.Multiplayer.broadcastSprites(this.CurrentLocation, new[] { new TemporaryAnimatedSprite(28, 160f, 2, 1, new Vector2(bubbleLocation.X - 32f, bubbleLocation.Y - 32f), flicker: false, flipped: false) });
                        Game1.stats.TimesFished++;
                        this.CurrentLocation.TemporarySprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.id == fishingBubblesID && sprite.Position == bubbleLocation - new Vector2(32, 32));
                        ModEntry.AHelper.Multiplayer.SendMessage(new KeyValuePair<Vector2, int>(bubbleLocation - new Vector2(32, 32), fishingBubblesID), "AskClientToDeleteTempSpriteAtCurrentLocation", ModEntry.ModIDAsArray);

                        int energyConsumed = -6 + (this._petInfo.SkillMastery_level[0] >= 4 ? -1 : 0) + (this._petInfo.SkillMastery_level[1] >= 2 ? -1 : 0);
                        ChangeEnergyLevel(energyConsumed, this.GroupLeader);
                    }
                    break;
                case 80:
                    Game1.Multiplayer.broadcastSprites(this.CurrentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(320, 0, 64, 64), 150f, 3, 0, bubbleLocation - new Vector2(32, 32), flicker: false, flipped: Game1.random.NextBool())
                    {
                        layerDepth = (float)bubbleLocation.Y / 10000f - 0.002f,
                        scale = 0.9f,
                        alphaFade = 0.02f,
                        scaleChange = 0.005f,
                        motion = new Vector2(-0.1f, 0)
                    });
                    break;
                case 90:
                    if (animationTimerStarted == -105)
                    {
                        if (isEmoting) { timeOutTimer--; break; }

                        Game1.Multiplayer.broadcastSprites(this.CurrentLocation, new[] { new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(403, 496, 5, 14), 2500, 1, 1, position:this.StandingPixel.ToVector2() + new Vector2(-5, -116f),false, false)
                            {
                                layerDepth = (float)(this.GetBoundingBox().Bottom + 4) / 10000f,
                                motion = new Vector2(0, -0.33f),
                                scale = 3.2f,
                                alphaFade = 0.015f,
                            }});
                    }
                    break;
            }

        }
        private void GetSomeCrabPotFish(ref int animationTimerStarted, Vector2 bubbleLocation, bool hasLuckyBuff, int luxuryCollarMultiplier, bool hasSeagoingCollar, Item _crabPotFish) //Memory allocated by code: 37,968 bytes
        {
            var crabPotFishItem = _crabPotFish;
            if (_crabPotFish == null || ItemRegistry.GetDataOrErrorItem(_crabPotFish.QualifiedItemId).IsErrorItem)
            {
                crabPotFishItem = ItemRegistry.Create("(O)153");
            }
            string itemQUID = crabPotFishItem.QualifiedItemId;
            string itemID = crabPotFishItem.ItemId;

            double fishingSkillProficiency = this._petInfo.SkillMastery_level[3];
            double luckModifier = (hasLuckyBuff ? 0.07d : 0d);
            int quantity = Game1.random.NextDouble() < ((luxuryCollarMultiplier * 0.10d) + luckModifier) ? 2 : 1;
            bool rerollQualityChanges = Game1.random.NextDouble() < ((luxuryCollarMultiplier * 0.25d) + luckModifier);
            int itemQuality = 0;
            if (crabPotFishItem.Category == -4)
            {
                if (fishingSkillProficiency >= 5 && ((Game1.random.NextDouble() < 0.25d) || (rerollQualityChanges && Game1.random.NextDouble() < 0.25d)))
                {
                    itemQuality = 4;
                }
                else if (fishingSkillProficiency >= 4 && ((Game1.random.NextDouble() < 0.50d) || (rerollQualityChanges && Game1.random.NextDouble() < 0.50d)))
                {
                    itemQuality = 2;
                }
                else if (fishingSkillProficiency >= 2 && ((Game1.random.NextDouble() < 0.75d) || (rerollQualityChanges && Game1.random.NextDouble() < 0.75d)))
                {
                    itemQuality = 1;
                }
            }
            bool itemIsObject = crabPotFishItem.HasTypeObject();
            crabPotFishItem.Stack = quantity;
            crabPotFishItem.Quality = itemQuality;
            crabPotFishItem.FixQuality();
            crabPotFishItem.FixStackSize();

            Item itemLater = !SynchronizationManager.TryGetPetInventory(this.OriginalPetInstance).IsNull(out var _petInventory) ? SynchronizationManager.TryAddItemToPetBackpackOrPocketSlot(_petInventory, crabPotFishItem) : crabPotFishItem;

            if (itemLater?.Stack != crabPotFishItem.Stack)
            {
                animationTimerStarted = -105;
                this.GroupLeader.gainExperience(1, Math.Max(1, (itemQuality + 1) * 3 + 1 / 3));
                if (!Game1.isFestival() && itemIsObject && this.GroupLeader.team.specialOrders is not null)
                {
                    foreach (SpecialOrder specialOrder in this.GroupLeader.team.specialOrders)
                    {
                        specialOrder.onFishCaught?.Invoke(this.GroupLeader, _crabPotFish);
                    }
                }
                try
                {
                    if (DataLoader.Fish(Game1.content).TryGetValue(itemID, out var rawDataStr))
                    {
                        string[] rawData = rawDataStr.Split('/');
                        int minFishSize = 1;
                        if (rawData.Length > 5 && int.TryParse(rawData[5], out int minFishSizeTest))
                        {
                            minFishSize = minFishSizeTest;
                        }
                        int maxFishSize = 10;
                        if (rawData.Length > 6 && int.TryParse(rawData[6], out int maxFishSizeTest))
                        {
                            maxFishSize = maxFishSizeTest;
                        }
                        int randomSize = Game1.random.Next(minFishSize, maxFishSize + 1);
                        IfClientSendMessageElseDoAction(this.GroupLeader, () => this.GroupLeader.caughtFish(itemQUID, randomSize, from_fish_pond: false, crabPotFishItem.Stack), new KeyValuePair<string, KeyValuePair<int, int>>(itemQUID, new(randomSize, crabPotFishItem.Stack)), "PEMPFarmerCaughtFish", new[] { this.GroupLeader.UniqueMultiplayerID });
                    }
                }
                catch { IfClientSendMessageElseDoAction(this.GroupLeader, () => this.GroupLeader.caughtFish(itemQUID, 25, from_fish_pond: false, crabPotFishItem.Stack), new KeyValuePair<string, KeyValuePair<int, int>>(itemQUID, new(25, crabPotFishItem.Stack)), "PEMPFarmerCaughtFish", new[] { this.GroupLeader.UniqueMultiplayerID }); }
                Game1.Multiplayer.broadcastSprites(this.CurrentLocation, new[] { new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(653, 858, 1, 1), 9999f, 1, 1, bubbleLocation + new Vector2(Game1.random.Next(-3, 2) * 4, -32f), flicker: false, flipped: false, (float)bubbleLocation.Y / 10000f + 0.002f, 0.04f, Color.LightBlue, 5f, 0f, 0f, 0f) { acceleration = new Vector2(0f, 0.25f) } });
                DelayedAction.functionAfterDelay(() => PetHelper.PlaySoundForAllPlayersAtFarmerLocation("fishSlap", this.Tile, this.GroupLeader), 100);
                this.SetNewItemHasBeenObtained(true);
            }
            else { DelayedAction.functionAfterDelay(() => { this.doEmote(xEmote); PetHelper.PlaySoundForAllPlayersAtFarmerLocation("fallDown", this.Tile, this.GroupLeader); PetHelper.PlaySoundForAllPlayersAtFarmerLocation("fishSlap", this.Tile, this.GroupLeader); }, 100); }
        }
        private void GetSomeFish(ref int animationTimerStarted, Vector2 bubbleLocation, int _bobberPointDistanceToLand, bool hasLuckyBuff, int luxuryCollarMultiplier, bool hasSeagoingCollar, double chanceToGetFish)
        {
            var allFishData = DataLoader.Fish(Game1.content);
            Item item = GetFishFromLocationData(allFishData, this.CurrentLocation.Name, bubbleLocation / Game1.tileSize, _bobberPointDistanceToLand, this.Tile.ToPoint(), this.CurrentLocation, this._petInfo.SkillMastery_level[3], hasLuckyBuff, hasSeagoingCollar, this.GroupLeader) ?? ItemRegistry.Create("(O)168");

            if (item == null || ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId).IsErrorItem)
            {
                item = ItemRegistry.Create("(O)153");
            }

            string itemQUID = item.QualifiedItemId;
            string itemID = item.ItemId;

            double fishingSkillProficiency = this._petInfo.SkillMastery_level[3];
            double luckModifier = (hasLuckyBuff ? 0.07d : 0d);
            int quantity = Game1.random.NextDouble() < ((luxuryCollarMultiplier * 0.10d) + luckModifier) ? 2 : 1;
            bool rerollQualityChanges = Game1.random.NextDouble() < ((luxuryCollarMultiplier * 0.25d) + luckModifier);
            int itemQuality = 0;
            if (item.Category == -4)
            {
                if (fishingSkillProficiency >= 5 && ((Game1.random.NextDouble() < 0.25d) || (rerollQualityChanges && Game1.random.NextDouble() < 0.25d)))
                {
                    itemQuality = 4;
                }
                else if (fishingSkillProficiency >= 4 && ((Game1.random.NextDouble() < 0.50d) || (rerollQualityChanges && Game1.random.NextDouble() < 0.50d)))
                {
                    itemQuality = 2;
                }
                else if (fishingSkillProficiency >= 2 && ((Game1.random.NextDouble() < 0.75d) || (rerollQualityChanges && Game1.random.NextDouble() < 0.75d)))
                {
                    itemQuality = 1;
                }
            }
            bool itemIsObject = item.HasTypeObject();
            item.Stack = quantity;
            item.Quality = itemQuality;
            item.FixQuality();
            item.FixStackSize();

            Item itemLater = !SynchronizationManager.TryGetPetInventory(this.OriginalPetInstance).IsNull(out var _petInventory) ? SynchronizationManager.TryAddItemToPetBackpackOrPocketSlot(_petInventory, item) : item;

            if (itemLater?.Stack != item.Stack)
            {
                animationTimerStarted = -105;
                int experienceGainModifier = 1;
                this.GroupLeader.gainExperience(1, Math.Max(1, (itemQuality + 1) * 3 + experienceGainModifier / 3));
                if (!Game1.isFestival() && itemIsObject && this.GroupLeader.team.specialOrders is not null)
                {
                    foreach (SpecialOrder specialOrder in this.GroupLeader.team.specialOrders)
                    {
                        specialOrder.onFishCaught?.Invoke(this.GroupLeader, item);
                    }
                }
                try
                {
                    if (allFishData.TryGetValue(itemID, out var rawData))
                    {
                        string[] fields = rawData.Split('/');
                        int minFishSize = 1;
                        if (fields.Length > 3 && int.TryParse(fields[3], out int minFishSizeTest))
                        {
                            minFishSize = minFishSizeTest;
                        }
                        int randomSize = (int)(minFishSize * (1 + (0.5d * Game1.random.NextDouble())));
                        IfClientSendMessageElseDoAction(this.GroupLeader, () => this.GroupLeader.caughtFish(itemQUID, randomSize, from_fish_pond: false, item.Stack), new KeyValuePair<string, KeyValuePair<int, int>>(itemQUID, new(randomSize, item.Stack)), "PEMPFarmerCaughtFish", new[] { this.GroupLeader.UniqueMultiplayerID });
                    }
                }
                catch { IfClientSendMessageElseDoAction(this.GroupLeader, () => this.GroupLeader.caughtFish(itemQUID, 10, from_fish_pond: false, item.Stack), new KeyValuePair<string, KeyValuePair<int, int>>(itemQUID, new(10, item.Stack)), "PEMPFarmerCaughtFish", new[] { this.GroupLeader.UniqueMultiplayerID }); }
                Game1.Multiplayer.broadcastSprites(this.CurrentLocation, new[] { new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(653, 858, 1, 1), 9999f, 1, 1, bubbleLocation + new Vector2(Game1.random.Next(-3, 2) * 4, -32f), flicker: false, flipped: false, (float)bubbleLocation.Y / 10000f + 0.002f, 0.04f, Color.LightBlue, 5f, 0f, 0f, 0f) { acceleration = new Vector2(0f, 0.25f) } });
                DelayedAction.functionAfterDelay(() => PetHelper.PlaySoundForAllPlayersAtFarmerLocation("fishSlap", this.Tile, this.GroupLeader), 100);
                this.SetNewItemHasBeenObtained(true);
            }
            else { DelayedAction.functionAfterDelay(() => { this.doEmote(xEmote); PetHelper.PlaySoundForAllPlayersAtFarmerLocation("fallDown", this.Tile, this.GroupLeader); PetHelper.PlaySoundForAllPlayersAtFarmerLocation("fishSlap", this.Tile, this.GroupLeader); }, 100); }
        }
        public Item GetRandomForageAccordingToContext(int Quality, int Quantity, bool hasFloweryCollar)
        {
            (int weight, Item item)[] dropList = Array.Empty<(int, Item)>();
            if (targetObject is Tree t)
            {
                if (t.GetData() is WildTreeData data)
                {
                    dropList = new[]
                    {
                            (hasFloweryCollar? 3: 15,ItemRegistry.Create("(O)169", Quantity)), //Driftwood 15 - 3%)
                            (40,ItemRegistry.Create(data.SeedItemId, Game1.random.Next(1, 4) * Quantity, 0)), //That tree specific seed 40%
                            (25,ItemRegistry.Create(t.treeType.Value == "8"? "(O)709":"(O)388", t.treeType.Value == "8"? Game1.random.Next(2, 6) * Quantity :Game1.random.Next(6, 12) * Quantity)), // Wood 25%
                            (15,ItemRegistry.Create("(O)771", Game1.random.Next(3, 6) * Quantity)), // fiber 15%
                            (12,ItemRegistry.Create("(O)771", Game1.random.Next(4, 9) * Quantity)), // (O)Moss 10%
                            (8,ItemRegistry.Create("(O)404", Game1.random.Next(1, 3) * Quantity, Quality)), //Common Mushroom 5%
                            (8,ItemRegistry.Create("(O)281", Game1.random.Next(1, 3) * Quantity, Quality)), //Chanterelle Mushroom 5%
                            (8,ItemRegistry.Create("(O)257", Game1.random.Next(1, 3) * Quantity, Quality)), //Morel Mushroom 5%
                            (4,ItemRegistry.Create("(O)422", Quantity, Quality)), //Purple Mushroom 3%
                            (1,ItemRegistry.Create("(O)MysteryBox", Quantity)) // Mistery Box 1%
                    };

                }
            }
            else
            {
                switch (Game1.Date.Season)
                {
                    case Season.Winter:
                        {
                            dropList = new[]
                            {
                                (hasFloweryCollar? 2: 6,ItemRegistry.Create("(O)167", Quantity)), //Joja cola 6 - 2%)
                                (hasFloweryCollar? 3: 15,ItemRegistry.Create("(O)169", Quantity)), //Driftwood 15 - 3%)
                                (10,ItemRegistry.Create("(O)416", Quantity, Quality)), //Snow Yam 10%)
                                (15,ItemRegistry.Create("(O)282", Game1.random.Next(1, 3) * Quantity, Quality)), //Cranberry 15%
                                (15,ItemRegistry.Create("(O)414", Quantity, Quality)), //Crystal Fruit 15%
                                (15,ItemRegistry.Create("(O)412", Quantity, Quality)), //Winter Root 15%
                                (25,ItemRegistry.Create("(O)771", Game1.random.Next(1, 6) * Quantity)), // fiber 25%
                                (5,ItemRegistry.Create("(O)MysteryBox", Quantity)) // Mistery Box 5%
                            };
                        }
                        break;
                    case Season.Fall:
                        {
                            dropList = new[]
                            {
                                (hasFloweryCollar? 2: 6,ItemRegistry.Create("(O)167", Quantity)), //Joja cola 6 - 2%)
                                (hasFloweryCollar? 3: 15,ItemRegistry.Create("(O)169", Quantity)), //Driftwood 15 - 3%)
                                (10,ItemRegistry.Create("(O)259", Quantity, Quality)),//Fiddlehead fern 10%
                                (15,ItemRegistry.Create("(O)410", Game1.random.Next(1, 3) * Quantity, Quality)), //Blackberry 15%
                                (15,ItemRegistry.Create("(O)406", Quantity, Quality)), //Wild Plum 15%
                                (5,ItemRegistry.Create("(O)815", Quantity)), //Tea leaves 5%
                                (5,ItemRegistry.Create("(O)274", Quantity, Quality)), // Artichoke 5%
                                (5,ItemRegistry.Create("(O)300", Quantity, Quality)), // Amaranth 5%
                                (25,ItemRegistry.Create("(O)771", Game1.random.Next(1, 6) * Quantity)), // fiber 25%
                                (2,ItemRegistry.Create("(O)MysteryBox", Quantity)), // Mistery Box 2%
                                (3,ItemRegistry.Create("(O)444", Quantity)) // Duck Feather 3%
                            };
                        }
                        break;
                    case Season.Summer:
                        {
                            dropList = new[]
                            {
                                (hasFloweryCollar? 2: 6,ItemRegistry.Create("(O)167", Quantity)), //Joja cola 6 - 2%)
                                (hasFloweryCollar? 3: 15,ItemRegistry.Create("(O)169", Quantity)), //Driftwood 15 - 3%)
                                (10,ItemRegistry.Create("(O)259", Quantity, Quality)),//Fiddlehead fern 10%
                                (15,ItemRegistry.Create("(O)398", Game1.random.Next(1, 3) * Quantity, Quality)), //Grape 15%
                                (15,ItemRegistry.Create("(O)396", Quantity, Quality)), //Spice Berry 15%
                                (15,ItemRegistry.Create("(O)402", Quantity, Quality)), //Sweet Pea 15%
                                (5,ItemRegistry.Create("(O)815", Quantity)), //Tea leaves 5%
                                (5,ItemRegistry.Create("(O)304", Quantity, Quality)), // Hops 5%
                                (5,ItemRegistry.Create("(O)260", Quantity, Quality)), // Hot Pepper 5%
                                (25,ItemRegistry.Create("(O)771", Game1.random.Next(1, 6) * Quantity)), // fiber 25%
                                (2,ItemRegistry.Create("(O)MysteryBox", Quantity)), // Mistery Box 2%
                                (3,ItemRegistry.Create("(O)442", Quantity, Quality)) // Duck Egg 3%
                            };
                        }
                        break;
                    default:
                        {
                            dropList = new[]
                            {
                                (hasFloweryCollar? 2: 6,ItemRegistry.Create("(O)167", Quantity)), //Joja cola 6 - 2%)
                                (hasFloweryCollar? 3: 15,ItemRegistry.Create("(O)169", Quantity)), //Driftwood 15 - 3%)
                                (10,ItemRegistry.Create("(O)259", Quantity, Quality)),//Fiddlehead fern 10%
                                (15,ItemRegistry.Create("(O)18", Quantity, Quality)), //Daffodil 15%
                                (15,ItemRegistry.Create("(O)22", Quantity, Quality)), //Dandelion 15%
                                (15,ItemRegistry.Create("(O)296", Game1.random.Next(1, 3) * Quantity, Quality)), //Salmonberry 15%
                                (5,ItemRegistry.Create("(O)815", Quantity)), //Tea leaves 5%
                                (5,ItemRegistry.Create("(O)250", Quantity, Quality)), // Kale 5%
                                (5,ItemRegistry.Create("(O)188", Game1.random.Next(1, 4) * Quantity, Quality)), // Green Bean 5%
                                (25,ItemRegistry.Create("(O)771", Game1.random.Next(1, 6) * Quantity)), // fiber 25%
                                (1,ItemRegistry.Create("(O)222", Quantity)), // Rhubarb Pie. Just like the meme.
                                (2,ItemRegistry.Create("(O)MysteryBox", Quantity)), // Mistery Box 2%
                                (3,ItemRegistry.Create("(O)444", Quantity)) // Duck Feather 3%
                            };
                        }
                        break;
                }
            }
            return PetHelper.GetRandomItemFromWeightedList(dropList);
        }

        public bool PickForageObject(ref double forageSkillProficiency, bool hasFloweryCollar, int luxuryCollarMultiplier, bool isLucky)
        {
            bool foraged = false;
            double luckModifier = (isLucky ? 0.07d : 0d);
            int quantity = Game1.random.NextDouble() < ((luxuryCollarMultiplier * 0.10d) + luckModifier) ? 2 : 1;
            bool rerollQualityChanges = Game1.random.NextDouble() < ((luxuryCollarMultiplier * 0.25d) + luckModifier);
            int quality = 0;
            if (forageSkillProficiency >= 5 && ((Game1.random.NextDouble() < (hasFloweryCollar ? 0.35d : 0.25d)) || (rerollQualityChanges && Game1.random.NextDouble() < (hasFloweryCollar ? 0.35d : 0.25d))))
            {
                quality = 3;
            }
            else if (forageSkillProficiency >= 4 && ((Game1.random.NextDouble() < (hasFloweryCollar ? 0.60d : 0.50d)) || (rerollQualityChanges && Game1.random.NextDouble() < (hasFloweryCollar ? 0.60d : 0.50d))))
            {
                quality = 2;

            }
            else if (forageSkillProficiency >= 2 && ((Game1.random.NextDouble() < (hasFloweryCollar ? 0.85d : 0.75d)) || (rerollQualityChanges && Game1.random.NextDouble() < (hasFloweryCollar ? 0.85d : 0.75d))))
            {
                quality = 1;
            }
            Item item = GetRandomForageAccordingToContext(quality, quantity, hasFloweryCollar);

            Item resultItem = !SynchronizationManager.TryGetPetInventory(this.OriginalPetInstance).IsNull(out var _petInventory) ? SynchronizationManager.TryAddItemToPetBackpackOrPocketSlot(_petInventory, item) : item;
            if (resultItem?.Stack != item.Stack)
            {
                SetNewItemHasBeenObtained(true);
                PetHelper.PlaySoundForAllPlayersAtFarmerLocation("pickUpItem", this.Tile, this.GroupLeader);
                Game1.Multiplayer.broadcastSprites(this.CurrentLocation, new[] { new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(403, 496, 5, 14), 2500, 1, 1, position:this.StandingPixel.ToVector2() + new Vector2(-5, -116f),false, false)
                            {
                                layerDepth = (float)(this.GetBoundingBox().Bottom + 4) / 10000f,
                                motion = new Vector2(0, -0.33f),
                                scale = 3.2f,
                                alphaFade = 0.015f,
                            }});
                foraged = true;
            }
            else { DelayedAction.functionAfterDelay(() => { this.doEmote(xEmote); PetHelper.PlaySoundForAllPlayersAtFarmerLocation("fallDown", this.Tile, this.GroupLeader); }, 100); }
            forageSkillProficiency = Math.Min(forageSkillProficiency + CalculateExpForSkillMastery(0.5d, forageSkillProficiency), 5);
            return foraged;
        }

        public void PetGivenTreat(string treatQUID, int quality)
        {
            var _definedDietList = CachePetData.GetDietListFromID(peData.DietListID);
            if (!string.IsNullOrEmpty(treatQUID) && _definedDietList.TryGetValue(treatQUID, out var foundDietItem))
            {
                ChangeEnergyLevel((int)((float)foundDietItem.EnergyGain * (((float)quality * 0.15f) + 1f)), Game1.player);
                if (foundDietItem.EnergyGain >= 0)
                {
                    this.foodSaturationLevel.positive++;
                }
                else
                {
                    this.foodSaturationLevel.negative--;
                }
                if (!this._petInfo.KnownEdibleItems.Contains(treatQUID))
                {
                    var listT = new List<string>(this._petInfo.KnownEdibleItems);
                    listT.Add(treatQUID);
                    this._petInfo.KnownEdibleItems = listT.ToArray();
                }
            }
        }
        public static void ReceiveTreat(Farmer fromWho,Pet forPet, SynchronizationManager.PetInformation dynamicInformation, Inventory receivingInventory, NetMutex inventoryMutex)
        {
            if (fromWho?.CurrentItem is null || forPet is null) { return; }

            var _definedDietList = CachePetData.GetDietListFromID(CachePetData.GetPetDataForPet(forPet).DietListID);
            string currentItemQUID = fromWho.CurrentItem.QualifiedItemId;
            if (string.IsNullOrEmpty(currentItemQUID) || _definedDietList == null || _definedDietList.Count == 0 || !_definedDietList.TryGetValue(fromWho.CurrentItem.QualifiedItemId, out var foundDietItem)) { return; }

            Vector2 dynamicPosition = new Vector2(dynamicInformation.PetPositionX, dynamicInformation.PetPositionY);
            Vector2 dynamicTilePosition = dynamicPosition / Game1.tileSize;
            SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_Following_SkillMasteryLevel, out double petFollowingSkillMasteryLevel);
            int petMaxBaseEnergy = (SmartPet.MaxBaseEnergyNoUpgrade + (petFollowingSkillMasteryLevel >= 5 ? 80 : 0));

            SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_PetEnergy, out int currentEnergy);
            SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_CanBeGivenBadTreat, out bool canBeGivenBadTreat);
            SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_CanBeGivenGoodTreat, out bool canBeGivenGoodTreat);

            int energyAdded = (int)((float)foundDietItem.EnergyGain * (((float)fromWho.CurrentItem.Quality * 0.15f) + 1f));

            if (SynchronizationManager.TryParseModData(forPet.modData, SynchronizationManager.PetModDataKey_HasBeenGivenTreatToday, out bool hasBeenGivenTreatToday) && !hasBeenGivenTreatToday)
            {
                inventoryMutex.RequestLock(delegate
                {
                    try
                    {
                        if (fromWho.CurrentItem is not null && _definedDietList.ContainsKey(fromWho.CurrentItem.QualifiedItemId))
                        {
                            SynchronizationManager.TryAddItemListToInventory(receivingInventory, new List<Item>() { fromWho.CurrentItem },SynchronizationManager.PetsEnhancedModInventoryGiftedItemIndex);

                            fromWho.reduceActiveItemByOne();
                        }
                    }
                    finally { inventoryMutex.ReleaseLock(); }
                });
            }
            else if (foundDietItem.EnergyGain >= 0 && currentEnergy >= petMaxBaseEnergy)
            {
                Game1.drawObjectDialogue(Game1.parseText(I18n.PetIsFullAlert(PetName: forPet.Name)));
            }
            else if (!canBeGivenGoodTreat && foundDietItem.EnergyGain >= 0)
            {
                int rollDice = Game1.random.Next(2);
                Game1.drawObjectDialogue(Game1.parseText(rollDice == 1 ? I18n.PetHasBeenGivenTreatAlready1() : I18n.PetHasBeenGivenTreatAlready0()));
            }
            else if (!canBeGivenBadTreat && foundDietItem.EnergyGain < 0)
            {
                int rollDice = Game1.random.Next(2);
                Game1.drawObjectDialogue(Game1.parseText(rollDice == 1 ? I18n.PetDoesntWantMoreTreats0(forPet.Name) : I18n.PetDoesntWantMoreTreats1(forPet.Name)));
            }
            else if (energyAdded == 0 || (currentEnergy > 0 && energyAdded < 0) || (currentEnergy < petMaxBaseEnergy && energyAdded > 0))
            {
                inventoryMutex.RequestLock(delegate
                {
                    try
                    {
                        if (fromWho.CurrentItem is not null && _definedDietList.ContainsKey(fromWho.CurrentItem.QualifiedItemId))
                        {
                            SynchronizationManager.TryAddItemListToInventory(receivingInventory, new List<Item>() { fromWho.CurrentItem }, SynchronizationManager.PetsEnhancedModInventoryGiftedItemIndex);

                            fromWho.reduceActiveItemByOne();
                        }
                    }
                    finally { inventoryMutex.ReleaseLock(); }
                });
            }
        }
        public static void IfClientSendMessageElseDoAction<T>(Farmer who, Action _action, T _messageType, string _messageName, long[] _toPlayers = null)
        {
            if (!who.IsMainPlayer)
            {
                ModEntry.AHelper.Multiplayer.SendMessage<T>(_messageType, _messageName, ModEntry.ModIDAsArray, _toPlayers);
                return;
            }
            _action();
        }

        private int getGeneralDirectionTowards(Vector2 target, int yBias = 0, bool opposite = false, bool useTileCalculations = true)
        {
            int num = ((!opposite) ? 1 : (-1));
            Point standingPixel = StandingPixel;
            int num2;
            int num3;
            if (useTileCalculations)
            {
                Point tilePoint = this.TilePoint;
                num2 = ((int)(target.X / 64f) - tilePoint.X) * num;
                num3 = ((int)(target.Y / 64f) - tilePoint.Y) * num;
                if (num2 == 0 && num3 == 0)
                {
                    Vector2 vector = new Vector2(((float)(int)(target.X / 64f) + 0.5f) * 64f, ((float)(int)(target.Y / 64f) + 0.5f) * 64f);
                    num2 = (int)(vector.X - (float)standingPixel.X) * num;
                    num3 = (int)(vector.Y - (float)standingPixel.Y) * num;
                    yBias *= 64;
                }
            }
            else
            {
                num2 = (int)(target.X - (float)standingPixel.X) * num;
                num3 = (int)(target.Y - (float)standingPixel.Y) * num;
            }

            if (num2 > Math.Abs(num3) + yBias)
            {
                return 1;
            }

            if (Math.Abs(num2) > Math.Abs(num3) + yBias)
            {
                return 3;
            }

            if (num3 > 0 || ((float)standingPixel.Y - target.Y) * (float)num < 0f)
            {
                return 2;
            }

            return 0;
        }
        private void SetFacingRectangleStillPose(Rectangle rec)
        {
            Rectangle smallRect = new Rectangle(rec.Center.X - 16, rec.Center.Y - 16, 32, 32);
            if (!this.GetBoundingBox().Intersects(smallRect))
            {
                switch (this.getGeneralDirectionTowards(rec.Center.ToVector2(), 0, false, true))
                {
                    case 0:
                        {
                            this.Sprite.CurrentFrame = 8;
                            this.flip = false;
                            this.facingDirection = 0;
                        }
                        break;
                    case 1:
                        {
                            this.Sprite.CurrentFrame = 4;
                            this.flip = false;
                            this.facingDirection = 1;
                        }
                        break;
                    case 2:
                        {
                            this.Sprite.CurrentFrame = 0;
                            this.flip = false;
                            this.facingDirection = 2;
                        }
                        break;
                    case 3:
                        {
                            this.Sprite.CurrentFrame = 12;
                            this.flip = false;
                            this.facingDirection = 3;
                        }
                        break;
                    default:
                        {

                        }
                        break;
                }
            }
        }
        public static Vector2 GetKnockbackVelocity(Vector2 from, Vector2 to, float knockbackMultiplier)
        {
            return ((to - from) / 8) * knockbackMultiplier;
        }
        public static float CalculateHypotenuse(int a, int b)
        {
            return MathF.Sqrt(MathF.Pow(a, 2) + MathF.Pow(b, 2));
        }
        public Vector2 getLocalPosition(xTile.Dimensions.Rectangle viewport)
        {
            Vector2 vector = Position;
            return new Vector2(vector.X - viewport.X, vector.Y - viewport.Y);
        }
        //Point pointA = Point.Zero;
        //Point pointB = Point.Zero;
        public bool GoToSpot(GameTime time, Vector2? toPoint, out bool readyToBeIddle, Rectangle? collideWithRectangle = null, bool following = false, int stopAtDistance = 0)
        {
            readyToBeIddle = false;
            if (this.GroupLeader is null || toPoint is null || (this.HumanLeader is null && this.PetLeader is null) || !this.CurrentLocation.NameOrUniqueName.EqualsIgnoreCase(this.GroupLeader.currentLocation.NameOrUniqueName)) { return false; }
            float FollowerTargetDistance = PetHelper.GetDistance(this.StandingPixel.ToVector2(), toPoint.Value);
            float followerTargetDistanceMinusMinDistance = MathF.Max(0, FollowerTargetDistance - stopAtDistance);
            int DistanceInTilesForSpeedUp = 2 * 64;
            float distanceMultiplier = (MathF.Max(1, followerTargetDistanceMinusMinDistance) / DistanceInTilesForSpeedUp);
            float maxSpeed = (!following) ? (3 + MathF.Min(distanceMultiplier, 3)) : ((int)this.GroupLeader.getMovementSpeed() + distanceMultiplier);
            bool petIsNotCollidingWithRectangle = ((collideWithRectangle is not null && !this.GetBoundingBox().Intersects(collideWithRectangle.Value)) || collideWithRectangle is null);
            this.Motion = Vector2.Zero;

            if (PetHelper.GetDistance(this.StandingPixel, this.GroupLeader.StandingPixel) > (Game1.tileSize * MaxDistanceBeforeTeleport))
            {
                this.pathToFollow.Clear();
                this.Position = this.GroupLeader.getStandingPosition() - new Vector2(32, 32);
                this.ResetTargets();
                this.oldPetTile = this.Tile;
                return false;
            }
            //pointA = this.StandingPixel;
            //pointB = toPoint.Value.ToPoint();
            if (petIsNotCollidingWithRectangle && (int)FollowerTargetDistance > stopAtDistance)
            {
                var rentedHashset = CacheReciclerHelper.RentHSTIntInt();
                if (PetsEnhancedPathfindHelper.checkCollisionRay(this.StandingPixel.X, this.StandingPixel.Y, (int)toPoint.Value.X, (int)toPoint.Value.Y,this.CurrentLocation,this.GetBoundingBox(), rentedHashset))
                {
                    if (this.pathToFollow.Count == 0 || this.Tile != this.oldPetTile)
                    {
                        this.pathToFollow.Clear();
                        this.oldPetTile = this.Tile;
                        try { PetsEnhancedPathfindHelper.findPath(Utility.Vector2ToPoint(this.Tile), Utility.Vector2ToPoint(toPoint.Value / 64), this.CurrentLocation, pathToFollow, 500, 500, true, this.GetBoundingBox()); } catch { pathToFollow.Clear(); ModEntry.WriteMonitor($"Error while pathfinding.", LogLevel.Error); }

                        if (this.pathToFollow.Count > 0 && this.pathToFollow.Peek() == this.Tile.ToPoint())
                        {
                            this.pathToFollow.Pop();
                        }
                    }
                    if (this.pathToFollow?.Count > 0)
                    {
                        if (this.StandingPixel == PetsEnhancedPathfindHelper.ConvertTileToItsCenterPixel(this.pathToFollow.Peek())) { this.pathToFollow.Pop(); /*ModEntry.WriteMonitor($"Pop, count {this.pathToFollow.Count}", LogLevel.Error);*/ }
                        else
                        {
                            this.Motion = GetConstantMotionTowardPoint(this.StandingPixel, PetsEnhancedPathfindHelper.ConvertTileToItsCenterPixel(this.pathToFollow.Peek()), PetHelper.GetMovementSpeed(followerTargetDistanceMinusMinDistance, maxSpeed, 1, 96));
                        }
                    }
                }
                else
                {
                    this.pathToFollow.Clear();
                    this.oldPetTile = this.Tile;
                    this.Motion = GetConstantMotionTowardPoint(this.StandingPixel, toPoint.Value.ToPoint(), PetHelper.GetMovementSpeed(followerTargetDistanceMinusMinDistance, maxSpeed, 1, 96));
                }
                CacheReciclerHelper.Return(rentedHashset);
                if (this.Motion != Vector2.Zero)
                {
                    ApplyMotion(this.Motion, FollowerTargetDistance, stopAtDistance, time, following);
                }
            }
            readyToBeIddle = (int)FollowerTargetDistance <= stopAtDistance || this.StandingPixel.ToVector2() == toPoint.Value;
            return this.StandingPixel.ToVector2() == toPoint.Value;
        }

        public void ApplyMotion(Vector2 motion, float followerTargetDistance, int stopAtDistance, GameTime time, bool following, int intervalOffset = -50)
        {
            this.xVelocity = motion.X;
            this.yVelocity = motion.Y;
            this.ApplyVelocitySP();
            if (followerTargetDistance > stopAtDistance)
            {
                AnimationSubUpdate(motion, time, following, intervalOffset);
            }
        }
        public static Vector2 GetConstantMotionTowardPoint(Point from, Point to, float speed)
        {
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            var distance = MathF.Sqrt(dx * dx + dy * dy);
            if (distance == 0)
            {
                return Vector2.Zero;
            }
            float step = Math.Min(speed, distance);
            float x = dx / distance * step;
            float y = dy / distance * step;

            return new Vector2(x, y);
        }
        public void ApplyVelocitySP()
        {
            float speed = (float)Math.Sqrt((xVelocity * xVelocity) + (yVelocity * yVelocity));
            if (this.CurrentLocation.terrainFeatures.TryGetValue(this.Tile, out var feature) && feature is Grass grass) //Animate Grass when moving on top
            {
                if (grass.getBoundingBox().Intersects(this.GetBoundingBox()))
                {
                    var isRight = ModEntry.AHelper.Reflection.GetMethod(grass, "shake");
                    isRight?.Invoke(MathF.PI / 8f / Math.Min(1f, 5f / speed), MathF.PI / 80f / Math.Min(1f, 5f / speed), (float)this.StandingPixel.X > ((grass.Tile.X * Game1.tileSize) + 32f));
                }
            }

            Position.X += xVelocity;
            Position.Y += yVelocity;

            xVelocity = (int)(xVelocity - xVelocity / 2f);
            yVelocity = (int)(yVelocity - yVelocity / 2f);
        }
        private bool CheckLeaderRadius(GameTime time, PetObjective _objective)
        {
            if (this.GroupLeader is null || this.Swimming || _objective == PetObjective.Follow || _objective == PetObjective.Wait || _objective == PetObjective.None || this._petInfo.Energy <= 0) { return false; }
            float petGroupLeaderDistance = PetHelper.TargetPetDistance(this, this.GroupLeader.Tile);

            if (_objective == PetObjective.Forage)
            {
                if (petGroupLeaderDistance >= 11 || petSearchPatienceTimer >= 300 || (this.targetObject is not null && CachePetData.CachePetTeams.IsTargetTerrainFeatureAnotherPetTarget(this.targetObject.getBoundingBox(), this._petInfo.PetId)))
                {
                    if (targetObject is not null)
                    {
                        CachePetData.CachePetTeams.TryRemoveTargetTerrainFeatureToTargetedTerrainFeatures(targetObject.getBoundingBox());
                    }
                    ResetTargets();
                    this.petSearchPatienceTimer = petSearchCooldown = 0;
                    this.BubbleAnimationLocation = Point.Zero;
                    return false;
                }
                else
                {
                    if (targetObject is null || targetTile is null)
                    {
                        if (this.AcquireTargetTile() && targetObject is not null && CachePetData.CachePetTeams.TryAddTargetTerrainFeatureToTargetedTerrainFeatures(targetObject.getBoundingBox(), this._petInfo.PetId))
                        {
                            this.CurrentPetAction = PetAction.Forage;
                            this.petSearchPatienceTimer = petSearchCooldown = 0;
                            return true;
                        }
                    }
                    else
                    {

                        this.petSearchPatienceTimer++;
                        this.CurrentPetAction = PetAction.Forage;
                        if (GoToSpot(time, (targetTile.Value * Game1.tileSize) + new Vector2(32, 32), out _))
                        {
                            ReachedForageSource(targetObject, ref this.petSearchPatienceTimer, ref petSearchCooldown, ref this._petInfo.SkillMastery_level[2], SynchronizationManager.TryGetPetAccessory(this.OriginalPetInstance)?.QualifiedItemId == FloweryCollarQUID , this._petInfo.SkillMastery_level[0] >= 5);
                        }
                        return true;
                    }
                }

            }
            else if (_objective == PetObjective.Fishing)
            {
                if (petGroupLeaderDistance >= 12 || petSearchPatienceTimer >= 400)
                {
                    ResetTargets();
                    this.petSearchCooldown = this.petSearchPatienceTimer = 0;
                    this.BubbleAnimationLocation = Point.Zero;
                    return false;
                }
                else
                {
                    if (this.targetTile is null)
                    {
                        petSearchCooldown++;
                        if (petSearchCooldown >= 300)
                        {
                            if (Game1.random.NextDouble() < (this._petInfo.SkillMastery_level[1] >= 3 ? 0.85d : 0.75d) && this.AcquireTargetFishingTile())
                            {
                                this.CurrentPetAction = PetAction.Fishing;
                                petSearchCooldown = this.petSearchPatienceTimer = 0;
                                return true;
                            }
                            petSearchCooldown = 0;
                        }
                    }
                    else
                    {
                        this.CurrentPetAction = PetAction.Fishing;
                        PerformFishingAnimation(petSearchPatienceTimer, BubbleAnimationLocation.ToVector2());
                        this.petSearchPatienceTimer++;
                        if (petSearchPatienceTimer >= 50)
                        {
                            if (GoToSpot(time, (targetTile.Value * Game1.tileSize) + new Vector2(32, 32), out _))
                            {
                                ReachedFishingSource(ref petSearchPatienceTimer, ref petSearchCooldown, (BubbleAnimationLocation.ToVector2() * Game1.tileSize) + new Vector2(32, 32), SynchronizationManager.TryGetPetAccessory(this.OriginalPetInstance)?.QualifiedItemId == SeagoingCollarQUID , this._petInfo.SkillMastery_level[0] >= 5);
                            }
                        }
                        return true;
                    }
                }
                return false;
            }
            else if (_objective == PetObjective.Hunt)
            {
                Monster m = (this.target as Monster);
                if ((petGroupLeaderDistance >= 11) || (m is not null && !IsValidMonster(m)) || (CachePetData.CachePetTeams.IsTargetAnotherPetTarget(m) is not null && CachePetData.CachePetTeams.IsTargetAnotherPetTarget(m) != this._petInfo.PetId) || (m is not null && (!this.CurrentLocation.characters.Contains(m))))
                {
                    if (m is not null)
                    {
                        CachePetData.CachePetTeams.TryRemoveTargetToTargetedMonsters(m);
                    }
                    this.defenceBreakCounter = 0;
                    this.petSearchPatienceTimer = petSearchCooldown = 0;
                    ResetTargets();
                    this.BubbleAnimationLocation = Point.Zero;
                    return false;
                }
                else
                {
                    if (m is null)
                    {
                        Monster t = this.SearchForNewTarget();
                        if (t is not null && CachePetData.CachePetTeams.TryAddTargetToTargetedMonsters(t, this._petInfo.PetId))
                        {
                            doEmote(exclamationEmote);
                            this.CurrentPetAction = PetAction.Hunt;
                            this.target = t;
                            return true;
                        }
                    }
                    else
                    {
                        this.CurrentPetAction = PetAction.Hunt;
                        var accessoryOnUse = SynchronizationManager.TryGetPetAccessory(this.OriginalPetInstance);
                        DoDamage(ref m, this.GetBoundingBox(), ref this.AttackCooldown, this._petInfo.SkillMastery_level[4], this._petInfo.SkillMastery_level[1], accessoryOnUse?.QualifiedItemId == RoughCollarQUID, accessoryOnUse?.QualifiedItemId == LightweightCollarQUID, this._petInfo.SkillMastery_level[0] >= 5);
                        if (target is not null)
                        {
                            GoToSpot(time, this.target.StandingPixel.ToVector2(), out _, collideWithRectangle: this.target.GetBoundingBox());
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        public void PerformFishingAnimation(int elapsedTime, Vector2 splashPointTile)
        {
            if (elapsedTime > 10 && elapsedTime <= 15 && !isEmoting)
            {
                this.doEmote(exclamationEmote);
            }
            if (elapsedTime == 0)
            {
                Game1.Multiplayer.broadcastSprites(this.CurrentLocation, new[] {new TemporaryAnimatedSprite(51, new Vector2(splashPointTile.X * 64, splashPointTile.Y * 64), Color.White, 10, flipped: false, 100f, 4)
                {
                    layerDepth = (float)(splashPointTile.Y * 64 - 64 - 1) / 10000f,
                    id = fishingBubblesID
                } });
            }
        }
        public Point BubbleAnimationLocation = Point.Zero;
        public bool AcquireTargetFishingTile()
        {
            Stack<Point> checkPaths = CacheReciclerHelper.RentStackPoint();
            bool found = false;
            try
            {
                for (int u = 0; u < 20; u++)
                {
                    var rPoint = new Point((int)this.GroupLeader.Tile.X + Game1.random.Next(-8, 8), (int)this.GroupLeader.Tile.Y + Game1.random.Next(-8, 8));
                    var bubbleAnimationLocation = PetHelper.isTileFishable(this.CurrentLocation, rPoint.X, rPoint.Y + 2, false) ? new Point(rPoint.X, rPoint.Y + 2) : PetHelper.isTileFishable(this.CurrentLocation, rPoint.X, rPoint.Y - 2, false) ? new Point(rPoint.X, rPoint.Y - 2) : PetHelper.isTileFishable(this.CurrentLocation, rPoint.X + 2, rPoint.Y, false) ? new Point(rPoint.X + 2, rPoint.Y) : PetHelper.isTileFishable(this.CurrentLocation, rPoint.X - 2, rPoint.Y, false) ? new Point(rPoint.X - 2, rPoint.Y) : Point.Zero;
                    if (bubbleAnimationLocation != Point.Zero && !PetsEnhancedPathfindHelper.IsCollidingTilePosition(rPoint.X, rPoint.Y, this.CurrentLocation, false, false))
                    {
                        PetsEnhancedPathfindHelper.findPath(Utility.Vector2ToPoint(this.Tile), rPoint, this.CurrentLocation, checkPaths, 500, 500, false);

                        if (checkPaths?.Count > 0)
                        {
                            BubbleAnimationLocation = bubbleAnimationLocation;
                            this.targetTile = rPoint.ToVector2();
                            this.targetObject = null;
                            found = true;
                            break;

                        }
                    }
                }
            }
            finally { CacheReciclerHelper.Return(checkPaths); }
            return found;
        }
        public bool AcquireTargetTile()
        {
            Stack<Point> checkPaths = CacheReciclerHelper.RentStackPoint();
            bool found = false;
            try
            {
                foreach (var feature in this.CurrentLocation.largeTerrainFeatures)
                {
                    if (feature is Bush b && Game1.random.Next(10) < 2 && !CachePetData.CachePetTeams.IsTerrainFeatureOnTFIgnoreList(b.getBoundingBox()) && feature.isActionable() && IsInsideRadius(this.GroupLeader.Tile, feature.Tile, 7))
                    {
                        if (!CachePetData.CachePetTeams.IsTargetTerrainFeatureAnotherPetTarget(b.getBoundingBox(), this._petInfo.PetId) && !PetsEnhancedPathfindHelper.CheckAdjacentTilesForPassable(new Point(b.getBoundingBox().Center.X / 64, b.getBoundingBox().Center.Y / 64), this.CurrentLocation, 1).IsNull(out var whatTile))
                        {
                            PetsEnhancedPathfindHelper.findPath(Utility.Vector2ToPoint(this.Tile), whatTile.Value, this.CurrentLocation, checkPaths, 100, 30, false);
                            if (checkPaths?.Count > 0)
                            {
                                this.targetTile = whatTile.Value.ToVector2();
                                this.targetObject = b;
                                found = true;
                                break;
                            }
                        }
                    }
                }
                if (!found && this._petInfo.SkillMastery_level[2] >= 3)
                {
                    foreach (var feature in this.CurrentLocation.terrainFeatures.Values)
                    {
                        if (feature is Tree t && t.stump.Value != true && (t.treeType.Value == "1" || t.treeType.Value == "2" || t.treeType.Value == "3" || t.treeType.Value == "8") && t.GetData() is WildTreeData data && Game1.random.Next(10) <= 2 && !CachePetData.CachePetTeams.IsTerrainFeatureOnTFIgnoreList(t.getBoundingBox()) && feature.isActionable() && IsInsideRadius(this.GroupLeader.Tile, feature.Tile, 7))
                        {
                            if (!CachePetData.CachePetTeams.IsTargetTerrainFeatureAnotherPetTarget(t.getBoundingBox(), this._petInfo.PetId) && !PetsEnhancedPathfindHelper.CheckAdjacentTilesForPassable(t.Tile.ToPoint(), this.CurrentLocation, 1).IsNull(out var whatTile))
                            {
                                PetsEnhancedPathfindHelper.findPath(Utility.Vector2ToPoint(this.Tile), whatTile.Value, this.CurrentLocation, checkPaths, 100, 30, false);
                                if (checkPaths?.Count > 0)
                                {
                                    this.targetTile = whatTile.Value.ToVector2();
                                    this.targetObject = t;
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            finally { CacheReciclerHelper.Return(checkPaths); }
            return found;
        }
        public static bool IsValidMonster(Monster monster)
        {
            if (monster is null || ((NPC)monster).IsInvisible || monster.Health <= 0)
            {
                return false;
            }
            Bug bug = monster is Bug ? monster as Bug : null;
            if (bug is not null)
            {
                return bug.isArmoredBug is not null && (!bug.isArmoredBug.Value);
            }
            Mummy mummy = monster is Mummy ? monster as Mummy : null;
            if (mummy is not null)
            {
                return mummy.reviveTimer is not null && mummy.reviveTimer.Value <= 0;
            }
            return true;
        }

        private Monster SearchForNewTarget()
        {
            if (this.CurrentLocation is null || this.GroupLeader is null) { return null; }

            float searchRange = 10 * Game1.tileSize;
            float monsterAtClosestDistance = searchRange + 1;

            Monster result = null;

            Stack<Point> testList = CacheReciclerHelper.RentStackPoint();
            try
            {
                foreach (NPC character in this.CurrentLocation.characters)
                {
                    if (character is Monster monster && !(character is Spiker) && IsValidMonster(monster) && (CachePetData.CachePetTeams.IsTargetAnotherPetTarget(monster) is null || CachePetData.CachePetTeams.IsTargetAnotherPetTarget(monster) == this._petInfo.PetId))
                    {
                        float monsterDistance = Vector2.Distance(this.GroupLeader.StandingPixel.ToVector2(), character.getStandingPosition());
                        if (monsterDistance <= searchRange && monsterDistance < monsterAtClosestDistance)
                        {
                            PetsEnhancedPathfindHelper.findPath(Utility.Vector2ToPoint(this.Tile), Utility.Vector2ToPoint(monster.Tile), this.CurrentLocation, testList, 500, 500, false);
                            if (testList?.Count > 0)
                            {
                                result = monster;
                                monsterAtClosestDistance = monsterDistance;
                            }
                        }
                    }
                }
            }
            finally { CacheReciclerHelper.Return(testList); }

            return result;
        }
        private static (int minDamage, int maxDamage, float knockbackModifier, double critChance, float critMultiplier) CalculateMonsterDamage(int _minDamage, int _maxDamage, double _critChance, double huntingSkillMastery, bool hasRoughCollar, bool isLucky)
        {
            double CombatSkillMasteryDamageModifiers = hasRoughCollar ? 1.25d : 1;
            double CombatSkillMasteryCritChanceModifiers = ((int)huntingSkillMastery >= 3 ? 0.25d : 0) + (hasRoughCollar ? 0.10d : 0) + (isLucky ? 0.07d : 0);
            double MaxDamage = (int)huntingSkillMastery <= 1 ? _maxDamage : _maxDamage * (2.7d * (int)huntingSkillMastery);
            double MinDamage = (int)huntingSkillMastery <= 1 ? _minDamage : _minDamage * (2.7d * (int)huntingSkillMastery);

            return ((int)(MinDamage * CombatSkillMasteryDamageModifiers), (int)(MaxDamage * CombatSkillMasteryDamageModifiers), 1.25f, _critChance + CombatSkillMasteryCritChanceModifiers, 1.5f);
        }
        private void DoDamage(ref Monster enemy, Rectangle petBBox, ref int currentAttackCooldown, double petHuntingSkillMastery, double petFollowingSkillMastery, bool hasRoughCollar, bool hasLightweightCollar, bool isLucky)
        {

            if (enemy is null || currentAttackCooldown != 0)
            {
                return;
            }
            if (enemy.GetBoundingBox().Intersects(new Rectangle(petBBox.X - 64, petBBox.Y - 64, petBBox.Width + 128, petBBox.Height + 128)))
            {
                var (minDamage, maxDamage, knockbackModifier, critChance, critMultiplier) = CalculateMonsterDamage(this.peData.MinDamage, this.peData.MaxDamage, this.peData.CritChance, petHuntingSkillMastery, hasRoughCollar, isLucky);
                DamageMonster(ref enemy, minDamage, maxDamage, knockbackModifier, critChance, critMultiplier, this.peData.IsViciousType, this.peData.AttackEffect, this.facingDirection);

                double cooldownModifier = 1d + (petFollowingSkillMastery >= 3 ? -15 : 0) + (petHuntingSkillMastery >= 2 ? -10 : 0) + (petHuntingSkillMastery >= 4 ? -15 : 0) + (hasLightweightCollar ? -15 : 0);
                currentAttackCooldown = (int)(this.peData.BaseCooldownTime * (100 + cooldownModifier));

                int energyConsumed = -4 + (this._petInfo.SkillMastery_level[0] >= 4 ? -1 : 0) + (petFollowingSkillMastery >= 2 ? -1 : 0); //an attack takes less energy
                ChangeEnergyLevel(energyConsumed, this.GroupLeader);
            }
        }
        public void PerformBehavior(GameTime time, PetObjective behavior)
        {
            if (AttackCooldown > 0)
            {
                AttackCooldown--;
            }
            if (!LockMovement)
            {
                if (!this.CheckLeaderRadius(time, behavior))
                {
                    this.CurrentPetAction = PetAction.Follow;
                    if (this.HumanLeader is not null)
                    {
                        GoToSpot(time, this.HumanLeader.StandingPixel.ToVector2(), out this.readyToBeIddle, following: true, stopAtDistance: Game1.tileSize * 2);
                    }
                    else if (this.PetLeader is not null)
                    {
                        GoToSpot(time, this.PetLeader.StandingPixel.ToVector2(), out this.readyToBeIddle, following: true, stopAtDistance: Game1.tileSize * 2);
                    }
                }
            }
        }
        public bool DamageMonster(ref Monster monster, int minDamage, int maxDamage, float knockBackModifier, double critChance, float critMultiplier, bool isViciousType, string attackEffect, int currentFacingDirection)
        {
            Rectangle boundingBox = monster.GetBoundingBox();

            bool isCrit = false;
            int takenDamage = 0;
            if (currentFacingDirection == 3 || currentFacingDirection == 1)
            {
                this.setPetAttacking = true;
            }
            if (isViciousType)
            {
                if (monster is Grub mg)
                {
                    if (ModEntry.GetModHelper().Reflection.GetField<NetBool>(mg, "pupating").GetValue().Value == true)
                    {
                        ModEntry.GetModHelper().Reflection.GetField<NetBool>(mg, "pupating").SetValue(new NetBool(false));
                        ModEntry.GetModHelper().Reflection.GetField<int>(mg, "metamorphCounter").SetValue(4500);
                    }
                }
            }
            if (maxDamage >= 0)
            {
                int potential = Game1.random.Next(minDamage, maxDamage);
                if (Game1.random.NextDouble() < critChance)
                {
                    isCrit = true;
                    PetHelper.PlaySoundForAllPlayersAtFarmerLocation("crit", monster.Tile, this.GroupLeader);
                }
                int damage = Math.Max(1, (int)Math.Round((isCrit ? (potential * critMultiplier) : potential)));

                takenDamage = monster.takeDamage(damage, (int)GetKnockbackVelocity(this.StandingPixel.ToVector2(), monster.getStandingPosition(), knockBackModifier).X, (int)GetKnockbackVelocity(this.StandingPixel.ToVector2(), monster.getStandingPosition(), knockBackModifier).Y, false, 10, "shiny4");
                PetHelper.PlaySoundForAllPlayersAtFarmerLocation("hitEnemy", monster.Tile, this.GroupLeader);
                if (takenDamage == 0)
                {
                    if (monster is not RockCrab rc || (rc is not null && !isViciousType))
                    {
                        this.CurrentLocation.debris.Add(new Debris("Miss", 1, new Vector2(boundingBox.Center.X, (float)boundingBox.Center.Y), Color.LightGray, 1f, 0f));
                    }
                    else if (rc is not null && isViciousType)
                    {
                        this.CurrentLocation.debris.Add(new Debris("1", 1, new Vector2(boundingBox.Center.X, (float)boundingBox.Center.Y), new Color(128 + (30 * defenceBreakCounter), 128 + (30 * defenceBreakCounter), 128 + (30 * defenceBreakCounter)), 1f, 0f));
                        this.defenceBreakCounter++;
                        monster.shake(500);
                        if (defenceBreakCounter >= 5)
                        {
                            Point tilePoint = monster.TilePoint;
                            ModEntry.GetModHelper().Reflection.GetField<NetBool>(rc, "shellGone").SetValue(new NetBool(true));
                            PetHelper.PlaySoundForAllPlayersAtFarmerLocation("stoneCrack", monster.Tile, this.GroupLeader);
                            Game1.createRadialDebris(this.CurrentLocation, 14, tilePoint.X, tilePoint.Y, Game1.random.Next(2, 7), resource: false);
                            Game1.createRadialDebris(this.CurrentLocation, 14, tilePoint.X, tilePoint.Y, Game1.random.Next(2, 7), resource: false);
                            this.defenceBreakCounter = 0;
                        }
                    }
                }
                if (takenDamage > 0)
                {
                    if (attackEffect.EqualsIgnoreCase("Slash"))
                    {
                        PetHelper.PlaySoundForAllPlayersAtFarmerLocation("swordswipe", monster.Tile, this.GroupLeader);

                        Vector2 slashOffset = currentFacingDirection == 0 ? new Vector2(-96, -144f) : currentFacingDirection == 1 ? new Vector2(-16f, -96) : currentFacingDirection == 3 ? new Vector2(-160f, -96) : new Vector2(-96, -64);
                        Game1.Multiplayer.broadcastSprites(this.CurrentLocation, new[] { new TemporaryAnimatedSprite(15, this.StandingPixel.ToVector2() + slashOffset, Color.White, 3, flipped: false, 60f, 0, 128, 1f, 128, 0)
                            {
                                    scale = 1.3f,
                                    rotation = currentFacingDirection == 0? 180 : currentFacingDirection == 2? 90 : 0,
                                    flipped = currentFacingDirection == 3,
                                    layerDepth = (float)(this.GetBoundingBox().Bottom + 1) / 10000f,
                                    verticalFlipped = currentFacingDirection == 0
                             }});
                    }
                    else if (attackEffect.EqualsIgnoreCase("Bite"))
                    {
                        Game1.Multiplayer.broadcastSprites(monster.currentLocation, new[]
                        {
                            new TemporaryAnimatedSprite("Mods\\SunkenLace.PetsEnhancedMod\\Textures\\AttackSprites", new Rectangle(0,0,32,32), 50, 6, 1, position:new Vector2(monster.getStandingPosition().X, monster.getStandingPosition().Y + (float)monster.yJumpOffset) + monster.drawOffset + new Vector2(-64, -64),false, false)
                            {
                                layerDepth = (float)(this.GetBoundingBox().Bottom + 1) / 10000f,
                                scale = 4,
                                alphaFade = 0.01f,

                            }
                        });
                    }
                    if (this.defenceBreakCounter > 0)
                    {
                        this.defenceBreakCounter--;
                    }
                    monster.currentLocation.debris.Add(new Debris(damage, new Vector2(monster.StandingPixel.X + 8, monster.StandingPixel.Y), takenDamage > 5 ? Color.Wheat : Color.White, 1f, monster));
                    monster.setInvincibleCountdown(450 / 2);
                    monster.shedChunks(Game1.random.Next(1, 3));
                }

            }
            if (monster.Health <= 0)
            {
                if (!this.CurrentLocation.IsFarm)
                {
                    for (int i = GroupLeader.questLog.Count - 1; i >= 0; i--)
                    {
                        if (GroupLeader.questLog[i] != null && ((int)GroupLeader.questLog[i].questType.Value == 4))
                        {
                            GroupLeader.questLog[i].OnMonsterSlain(monster.currentLocation, monster, false, monster.currentLocation is SlimeHutch);
                        }
                    }
                }
                this.GroupLeader.currentLocation.monsterDrop(monster, (boundingBox).Center.X, (boundingBox).Center.Y, this.GroupLeader);
                this._petInfo.SkillMastery_level[4] = Math.Min(this._petInfo.SkillMastery_level[4] + CalculateExpForSkillMastery(1, _petInfo.SkillMastery_level[4]), 5);

                if (this._petInfo.SkillMastery_level[4] >= 5 && Game1.random.Next(10) < 3)
                {
                    IfClientSendMessageElseDoAction(this.GroupLeader, () => this.GroupLeader.health = Math.Min(this.GroupLeader.health + 20, this.GroupLeader.maxHealth), 20, "ClientHealPlayer20", new[] { this.GroupLeader.UniqueMultiplayerID });
                }
                this.GroupLeader.currentLocation.characters.Remove((NPC)(object)monster);
                Stats stats = Game1.stats;
                uint monstersKilled = stats.MonstersKilled + 1;
                stats.MonstersKilled = monstersKilled;
                Game1.stats.monsterKilled(((Character)monster).Name);
            }
            return false;
        }
        public static double CalculateExpForSkillMastery(double actionsPerformed, double currentMastery)
        {
            return currentMastery switch
            {
                < 2 => actionsPerformed / 0.5d,
                < 3 => actionsPerformed,
                < 4d => actionsPerformed / 2.5d,
                _ => actionsPerformed / 5d
            } * 0.01d;
        }
        private static void addSprinklesToLocation(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false, float layerDepth = 1f)
        {
            Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(sourceXTile - tilesWide / 2, sourceYTile - tilesHigh / 2, tilesWide, tilesHigh);
            Random random = Game1.random;
            int num = totalSprinkleDuration / millisecondsBetweenSprinkles;
            for (int i = 0; i < num; i++)
            {
                Vector2 vector = Utility.getRandomPositionInThisRectangle(r, random) * 64f;
                Game1.Multiplayer.broadcastSprites(l, new TemporaryAnimatedSprite(random.Next(10, 12), vector, sprinkleColor, 8, flipped: false, 50f)
                {
                    layerDepth = layerDepth,
                    delayBeforeAnimationStart = millisecondsBetweenSprinkles * i,
                    interval = 100f,
                    startSound = sound,
                    motion = (motionTowardCenter ? Utility.getVelocityTowardPoint(vector, new Vector2(sourceXTile, sourceYTile) * 64f, Vector2.Distance(new Vector2(sourceXTile, sourceYTile) * 64f, vector) / 64f) : Vector2.Zero),
                    xStopCoordinate = sourceXTile,
                    yStopCoordinate = sourceYTile
                });
            }
        }
        private static void addSmokePuff(GameLocation l, Vector2 v, int delay = 0, float baseScale = 2f, float scaleChange = 0.02f, float alpha = 0.75f, float alphaFade = 0.002f, float layerDepth = 1f)
        {
            TemporaryAnimatedSprite temporaryAnimatedSprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), v, flipped: false, alphaFade, Color.Gray);
            temporaryAnimatedSprite.alpha = alpha;
            temporaryAnimatedSprite.motion = new Vector2(0f, -0.5f);
            temporaryAnimatedSprite.acceleration = new Vector2(0.002f, 0f);
            temporaryAnimatedSprite.interval = 99999f;
            temporaryAnimatedSprite.layerDepth = layerDepth;
            temporaryAnimatedSprite.scale = baseScale;
            temporaryAnimatedSprite.scaleChange = scaleChange;
            temporaryAnimatedSprite.rotationChange = (float)Game1.random.Next(-5, 6) * MathF.PI / 256f;
            temporaryAnimatedSprite.delayBeforeAnimationStart = delay;
            Game1.Multiplayer.broadcastSprites(l, temporaryAnimatedSprite);
        }

        public void CommandPet(string responseKey, Farmer who, Guid _id)
        {
            bool followingAnotherLeader = GroupLeader is not null && GroupLeader.UniqueMultiplayerID != who.UniqueMultiplayerID;
            bool followingCurrentPlayer = GroupLeader is not null && GroupLeader.UniqueMultiplayerID == who.UniqueMultiplayerID;
            bool tooManyFollowers = CachePetData.CachePetTeams.IsTeamFull(who.UniqueMultiplayerID) && !CompareAndNullCheck(CachePetData.CachePetTeams.GetLeaderOfPetIfAny(this._petInfo.PetId), who.UniqueMultiplayerID);
            if (responseKey.Equals("GoHomeCommand"))
            {
                if (!followingCurrentPlayer && followingAnotherLeader)
                {
                    IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetAlreadyFollowingFarmerAlert(PetName: this._petInfo.Name, FarmerName: this.GroupLeader.Name))), new KeyValuePair<string, string>(this._petInfo.Name, this.GroupLeader.Name), "DOD_PetAlreadyFollowingFarmerAlert", new[] { who.UniqueMultiplayerID });
                }
                else if (followingCurrentPlayer && !followingAnotherLeader)
                {
                    float layerDepth = ((float)(this.GetBoundingBox().Center.Y + 4) + this.Position.X / 20000f) / 10000f;
                    var tile64Rounded = (new Vector2((int)MathF.Round(this.Tile.X), (int)MathF.Round(this.Tile.Y)) * 64);
                    addSprinklesToLocation(this.CurrentLocation, (int)MathF.Round(this.Tile.X), (int)MathF.Round(this.Tile.Y), 1, 1, 200, 40, Color.White, motionTowardCenter: true, layerDepth: layerDepth + 0.000002f);
                    addSmokePuff(this.CurrentLocation, tile64Rounded + new Vector2(-8, -8), baseScale: 5, alphaFade: 0.01f, layerDepth: layerDepth + 0.000002f);
                    addSmokePuff(this.CurrentLocation, tile64Rounded + new Vector2(32, -8), baseScale: 5, alphaFade: 0.01f, layerDepth: layerDepth + 0.000002f);
                    addSmokePuff(this.CurrentLocation, tile64Rounded + new Vector2(16, 24), baseScale: 5, alphaFade: 0.01f, layerDepth: layerDepth + 0.000002f);
                    PetHelper.PlaySoundForAllPlayersAtFarmerLocation("serpentDie", who.Tile, who);
                    PetHelper.PlaySoundForAllPlayersAtFarmerLocation("steam", who.Tile, who);
                    PetHelper.PlaySoundForAllPlayersAtFarmerLocation("yoba", who.Tile, who);
                    PetHelper.PlaySoundForAllPlayersAtFarmerLocation("fireball", who.Tile, who);
                    CachePetData.CachePetTeams.returnPetFromTeam(this.GroupLeader.UniqueMultiplayerID, _id);
                    IfClientSendMessageElseDoAction(who, () => Game1.addHUDMessage(HUDMessage.ForCornerTextbox(I18n.PetReturnedHomeMessage(this.OriginalPetInstance.Name))), this.OriginalPetInstance.Name, "FCTextboxPetHasReturnedHome", new[] { who.UniqueMultiplayerID });
                }
                return;
            }
            if (responseKey.Equals("StopWaiting"))
            {
                if (followingCurrentPlayer && !followingAnotherLeader)
                {
                    this.SetPetObjective(this.PrevPetObjective, who);
                }
                else if (!followingCurrentPlayer && followingAnotherLeader)
                {
                    if (!tooManyFollowers)
                    {
                        this.SetPetObjective(this.PrevPetObjective, who);
                    }
                    else
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.TooManyPetsOnYourTeamWarning())), "", "DOD_TooManyPetsOnYourTeamWarning", new[] { who.UniqueMultiplayerID });
                    }
                }
                else if (!followingCurrentPlayer && !followingAnotherLeader)
                {
                    if (this.CurrentPetObjective != PetObjective.None && (who.currentLocation is Farm || who.currentLocation is FarmHouse))
                    {
                        this.SetPetObjective(SmartPet.PetObjective.None, who);
                    }
                }
                return;
            }
            if (this.peData.HasWaitSkill)
            {
                if (responseKey.Equals("WaitCommand"))
                {
                    if (followingCurrentPlayer && !followingAnotherLeader)
                    {
                        this.SetPetObjective(SmartPet.PetObjective.Wait, who);
                    }
                    else if (!followingCurrentPlayer && followingAnotherLeader)
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetAlreadyFollowingFarmerAlert(PetName: this._petInfo.Name, FarmerName: this.GroupLeader.Name))), new KeyValuePair<string, string>(this._petInfo.Name, this.GroupLeader.Name), "DOD_PetAlreadyFollowingFarmerAlert", new[] { who.UniqueMultiplayerID });
                    }
                    else if (!followingCurrentPlayer && !followingAnotherLeader)
                    {
                        if (this.CurrentPetObjective != PetObjective.Wait)
                        {
                            this.SetPetObjective(SmartPet.PetObjective.Wait, who);
                        }
                    }
                    return;
                }
                if (responseKey.Equals("FreeUnlockWaitCommand"))
                {
                    if (this._petInfo.TrainedToday)
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetNeedsABreakAlert(PetName: this._petInfo.Name))), this._petInfo.Name, "DOD_PetNeedsABreakAlert", new[] { who.UniqueMultiplayerID });
                        return;
                    }
                    this._petInfo.SkillMastery_level[0] = 1f;

                    if (who.IsMainPlayer)
                    {
                        Game1.drawObjectDialogue(Game1.parseText(I18n.CommandUnlockedAlert(PetName: this._petInfo.Name, CommandName: I18n.WaitLessonName())));
                    }
                    else
                    {
                        ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, string>>(new(this._petInfo.Name, "Wait"), "DOD_CommandUnlockedAlert", ModEntry.ModIDAsArray, new[] { who.UniqueMultiplayerID });
                    }
                    PetHelper.PlaySoundForAllPlayersAtFarmerLocation("getNewSpecialItem", who.Tile, who);
                    PetLearnedANewSkill = true;
                    return;
                }
            }
            if (responseKey.Equals("ReleaseCommand"))
            {
                if (!followingCurrentPlayer && followingAnotherLeader)
                {
                    IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetAlreadyFollowingFarmerAlert(PetName: this._petInfo.Name, FarmerName: this.GroupLeader.Name))), new KeyValuePair<string, string>(this._petInfo.Name, this.GroupLeader.Name), "DOD_PetAlreadyFollowingFarmerAlert", new[] { who.UniqueMultiplayerID });
                }
                else if (followingCurrentPlayer && !followingAnotherLeader && this.CurrentPetObjective != PetObjective.None && (who.currentLocation is Farm || who.currentLocation is FarmHouse) && !this.Swimming)
                {
                    this.SetPetObjective(SmartPet.PetObjective.None, who);
                }
                return;

            }
            if (this.peData.HasFollowSkill)
            {
                if (responseKey.Equals("FollowMeCommand"))
                {
                    if (!tooManyFollowers)
                    {
                        if (followingCurrentPlayer && !followingAnotherLeader)
                        {
                            this.SetPetObjective(SmartPet.PetObjective.Follow, who);
                        }
                        else if (!followingCurrentPlayer && followingAnotherLeader)
                        {
                            IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetAlreadyFollowingFarmerAlert(PetName: this._petInfo.Name, FarmerName: this.GroupLeader.Name))), new KeyValuePair<string, string>(this._petInfo.Name, this.GroupLeader.Name), "DOD_PetAlreadyFollowingFarmerAlert", new[] { who.UniqueMultiplayerID });
                        }
                        else if (!followingCurrentPlayer && !followingAnotherLeader)
                        {
                            if (this.CurrentPetObjective != PetObjective.Follow)
                            {
                                this.SetPetObjective(SmartPet.PetObjective.Follow, who);
                            }
                        }
                    }
                    else
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.TooManyPetsOnYourTeamWarning())), "", "DOD_TooManyPetsOnYourTeamWarning", new[] { who.UniqueMultiplayerID });
                    }
                    return;
                }
                if (responseKey.Equals("UnlockFollowMeCommand"))
                {
                    if (this._petInfo.TrainedToday)
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetNeedsABreakAlert(PetName: this._petInfo.Name))), this._petInfo.Name, "DOD_PetNeedsABreakAlert", new[] { who.UniqueMultiplayerID });
                        return;
                    }
                    if (who.Items.ContainsId(this.peData.TrickLearningTreat) && who.Items.CountId(this.peData.TrickLearningTreat) >= 5 + (5 * (int)(4 * this._petInfo.SkillMastery_level[1])))
                    {

                        this._petInfo.TrainedToday = true;
                        if (who.IsMainPlayer)
                        {
                            who.Items.ReduceId(this.peData.TrickLearningTreat, 5 + (5 * (int)(4 * this._petInfo.SkillMastery_level[1])));
                        }
                        else
                        {
                            ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, int>>(new(this.peData.TrickLearningTreat, 5 + (5 * (int)(4 * this._petInfo.SkillMastery_level[1]))), "ReduceItemCall", ModEntry.ModIDAsArray, new[] { who.UniqueMultiplayerID });
                        }
                        this._petInfo.SkillMastery_level[1] += 0.25f;
                        if (this._petInfo.SkillMastery_level[1] >= 1)
                        {
                            if (who.IsMainPlayer)
                            {
                                Game1.drawObjectDialogue(Game1.parseText(I18n.CommandUnlockedAlert(PetName: this._petInfo.Name, CommandName: I18n.FollowMeLessonName())));
                            }
                            else
                            {
                                ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, string>>(new(this._petInfo.Name, "Follow"), "DOD_CommandUnlockedAlert", ModEntry.ModIDAsArray, new[] { who.UniqueMultiplayerID });
                            }
                            PetHelper.PlaySoundForAllPlayersAtFarmerLocation("getNewSpecialItem", who.Tile, who);
                            PetLearnedANewSkill = true;
                        }
                        else
                        {
                            int num = Game1.random.Next(3);
                            if (who.IsMainPlayer)
                            {
                                Game1.drawObjectDialogue(Game1.parseText(num > 1 ? I18n.PetProgressedWithLessonRandomAns3(PetName: this._petInfo.Name, LessonName: I18n.FollowMeLessonName()) : num > 0 ? I18n.PetProgressedWithLessonRandomAns2(PetName: this._petInfo.Name, LessonName: I18n.FollowMeLessonName()) : I18n.PetProgressedWithLessonRandomAns1(PetName: this._petInfo.Name, LessonName: I18n.FollowMeLessonName())));
                            }
                            else
                            {
                                ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, string>>(new(this._petInfo.Name, "Follow"), num > 1 ? "DOD_PetProgressedWithLessonRandomAns3" : num > 0 ? "DOD_PetProgressedWithLessonRandomAns2" : "DOD_PetProgressedWithLessonRandomAns1", ModEntry.ModIDAsArray, new[] { who.UniqueMultiplayerID });
                            }
                            PetHelper.PlaySoundForAllPlayersAtFarmerLocation("reward", who.Tile, who);
                        }
                    }
                    else
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.NotEnoughTreatsAlert())), this._petInfo.Name, "DOD_NotEnoughTreatsAlert", new[] { who.UniqueMultiplayerID });
                    }
                    return;
                }
            }
            if (this.peData.HasHuntSkill)
            {
                if (responseKey.Equals("HuntCommand"))
                {
                    if (!tooManyFollowers)
                    {
                        if (followingCurrentPlayer && !followingAnotherLeader)
                        {
                            this.SetPetObjective(SmartPet.PetObjective.Hunt, who);
                        }
                        else if (!followingCurrentPlayer && followingAnotherLeader)
                        {
                            IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetAlreadyFollowingFarmerAlert(PetName: this._petInfo.Name, FarmerName: this.GroupLeader.Name))), new KeyValuePair<string, string>(this._petInfo.Name, this.GroupLeader.Name), "DOD_PetAlreadyFollowingFarmerAlert", new[] { who.UniqueMultiplayerID });
                        }
                        else if (!followingCurrentPlayer && !followingAnotherLeader)
                        {
                            if (this.CurrentPetObjective != PetObjective.Hunt)
                            {
                                this.SetPetObjective(SmartPet.PetObjective.Hunt, who);
                            }
                        }
                    }
                    else
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.TooManyPetsOnYourTeamWarning())), "", "DOD_TooManyPetsOnYourTeamWarning", new[] { who.UniqueMultiplayerID });
                    }
                    return;
                }
                if (responseKey.Equals("UnlockHuntCommand"))
                {
                    if (this._petInfo.TrainedToday)
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetNeedsABreakAlert(PetName: this._petInfo.Name))), this._petInfo.Name, "DOD_PetNeedsABreakAlert", new[] { who.UniqueMultiplayerID });
                        return;
                    }
                    if (who.Items.ContainsId(this.peData.TrickLearningTreat) && who.Items.CountId(this.peData.TrickLearningTreat) >= 15 + (15 * (int)(4 * this._petInfo.SkillMastery_level[4])))
                    {
                        this._petInfo.TrainedToday = true;
                        if (who.IsMainPlayer)
                        {
                            who.Items.ReduceId(this.peData.TrickLearningTreat, 15 + (15 * (int)(4 * this._petInfo.SkillMastery_level[4])));
                        }
                        else
                        {
                            ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, int>>(new(this.peData.TrickLearningTreat, 15 + (15 * (int)(4 * this._petInfo.SkillMastery_level[4]))), "ReduceItemCall", ModEntry.ModIDAsArray, new[] { who.UniqueMultiplayerID });
                        }
                        this._petInfo.SkillMastery_level[4] += 0.25f;
                        if (this._petInfo.SkillMastery_level[4] >= 1)
                        {
                            if (who.IsMainPlayer)
                            {
                                Game1.drawObjectDialogue(Game1.parseText(I18n.CommandUnlockedAlert(PetName: this._petInfo.Name, CommandName: I18n.HuntLessonName())));
                            }
                            else
                            {
                                ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, string>>(new(this._petInfo.Name, "Hunt"), "DOD_CommandUnlockedAlert", ModEntry.ModIDAsArray, new[] { who.UniqueMultiplayerID });
                            }
                            PetHelper.PlaySoundForAllPlayersAtFarmerLocation("getNewSpecialItem", who.Tile, who);
                            PetLearnedANewSkill = true;
                        }
                        else
                        {
                            int num = Game1.random.Next(3);
                            if (who.IsMainPlayer)
                            {
                                Game1.drawObjectDialogue(Game1.parseText(num > 1 ? I18n.PetProgressedWithLessonRandomAns3(PetName: this._petInfo.Name, LessonName: I18n.HuntLessonName()) : num > 0 ? I18n.PetProgressedWithLessonRandomAns2(PetName: this._petInfo.Name, LessonName: I18n.HuntLessonName()) : I18n.PetProgressedWithLessonRandomAns1(PetName: this._petInfo.Name, LessonName: I18n.HuntLessonName())));
                            }
                            else
                            {
                                ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, string>>(new(this._petInfo.Name, "Hunt"), num > 1 ? "DOD_PetProgressedWithLessonRandomAns3" : num > 0 ? "DOD_PetProgressedWithLessonRandomAns2" : "DOD_PetProgressedWithLessonRandomAns1", ModEntry.ModIDAsArray, new[] { who.UniqueMultiplayerID });
                            }
                            PetHelper.PlaySoundForAllPlayersAtFarmerLocation("reward", who.Tile, who);
                        }
                    }
                    else
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.NotEnoughTreatsAlert())), this._petInfo.Name, "DOD_NotEnoughTreatsAlert", new[] { who.UniqueMultiplayerID });
                    }
                    return;
                }
            }
            if (this.peData.HasForageSkill)
            {
                if (responseKey.Equals("ForageCommand"))
                {
                    if (!tooManyFollowers)
                    {
                        if (followingCurrentPlayer && !followingAnotherLeader)
                        {
                            this.SetPetObjective(SmartPet.PetObjective.Forage, who);
                        }
                        else if (!followingCurrentPlayer && followingAnotherLeader)
                        {
                            IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetAlreadyFollowingFarmerAlert(PetName: this._petInfo.Name, FarmerName: this.GroupLeader.Name))), new KeyValuePair<string, string>(this._petInfo.Name, this.GroupLeader.Name), "DOD_PetAlreadyFollowingFarmerAlert", new[] { who.UniqueMultiplayerID });
                        }
                        else if (!followingCurrentPlayer && !followingAnotherLeader)
                        {
                            if (this.CurrentPetObjective != PetObjective.Forage)
                            {
                                this.SetPetObjective(SmartPet.PetObjective.Forage, who);
                            }
                        }
                    }
                    else
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.TooManyPetsOnYourTeamWarning())), "", "DOD_TooManyPetsOnYourTeamWarning", new[] { who.UniqueMultiplayerID });
                    }
                    return;
                }
                if (responseKey.Equals("UnlockForageCommand"))
                {
                    if (this._petInfo.TrainedToday)
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetNeedsABreakAlert(PetName: this._petInfo.Name))), this._petInfo.Name, "DOD_PetNeedsABreakAlert", new[] { who.UniqueMultiplayerID });
                        return;
                    }
                    if (who.Items.ContainsId(this.peData.TrickLearningTreat) && who.Items.CountId(this.peData.TrickLearningTreat) >= 10 + (10 * (int)(4 * this._petInfo.SkillMastery_level[2])))
                    {
                        this._petInfo.TrainedToday = true;
                        if (who.IsMainPlayer)
                        {
                            who.Items.ReduceId(this.peData.TrickLearningTreat, 10 + (10 * (int)(4 * this._petInfo.SkillMastery_level[2])));
                        }
                        else
                        {
                            ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, int>>(new(this.peData.TrickLearningTreat, 10 + (10 * (int)(4 * this._petInfo.SkillMastery_level[2]))), "ReduceItemCall", ModEntry.ModIDAsArray, new[] { who.UniqueMultiplayerID });
                        }
                        this._petInfo.SkillMastery_level[2] += 0.25f;
                        if (this._petInfo.SkillMastery_level[2] >= 1)
                        {
                            if (who.IsMainPlayer)
                            {
                                Game1.drawObjectDialogue(Game1.parseText(I18n.CommandUnlockedAlert(PetName: this._petInfo.Name, CommandName: I18n.ForageLessonName())));
                            }
                            else
                            {
                                ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, string>>(new(this._petInfo.Name, "Forage"), "DOD_CommandUnlockedAlert", ModEntry.ModIDAsArray, new[] { who.UniqueMultiplayerID });
                            }
                            PetHelper.PlaySoundForAllPlayersAtFarmerLocation("getNewSpecialItem", who.Tile, who);
                            PetLearnedANewSkill = true;
                        }
                        else
                        {
                            int num = Game1.random.Next(3);
                            if (who.IsMainPlayer)
                            {
                                Game1.drawObjectDialogue(Game1.parseText(num > 1 ? I18n.PetProgressedWithLessonRandomAns3(PetName: this._petInfo.Name, LessonName: I18n.ForageLessonName()) : num > 0 ? I18n.PetProgressedWithLessonRandomAns2(PetName: this._petInfo.Name, LessonName: I18n.ForageLessonName()) : I18n.PetProgressedWithLessonRandomAns1(PetName: this._petInfo.Name, LessonName: I18n.ForageLessonName())));
                            }
                            else
                            {
                                ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, string>>(new(this._petInfo.Name, "Forage"), num > 1 ? "DOD_PetProgressedWithLessonRandomAns3" : num > 0 ? "DOD_PetProgressedWithLessonRandomAns2" : "DOD_PetProgressedWithLessonRandomAns1", ModEntry.ModIDAsArray, new[] { who.UniqueMultiplayerID });
                            }
                            PetHelper.PlaySoundForAllPlayersAtFarmerLocation("reward", who.Tile, who);
                        }
                    }
                    else
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.NotEnoughTreatsAlert())), this._petInfo.Name, "DOD_NotEnoughTreatsAlert", new[] { who.UniqueMultiplayerID });
                    }
                    return;
                }
            }
            if (this.peData.HasFishingSkill)
            {
                if (responseKey.Equals("FishCommand"))
                {

                    if (!tooManyFollowers)
                    {
                        if (followingCurrentPlayer && !followingAnotherLeader)
                        {
                            this.SetPetObjective(SmartPet.PetObjective.Fishing, who);
                        }
                        else if (!followingCurrentPlayer && followingAnotherLeader)
                        {
                            IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetAlreadyFollowingFarmerAlert(PetName: this._petInfo.Name, FarmerName: this.GroupLeader.Name))), new KeyValuePair<string, string>(this._petInfo.Name, this.GroupLeader.Name), "DOD_PetAlreadyFollowingFarmerAlert", new[] { who.UniqueMultiplayerID });
                        }
                        else if (!followingCurrentPlayer && !followingAnotherLeader)
                        {
                            if (this.CurrentPetObjective != PetObjective.Fishing)
                            {
                                this.SetPetObjective(SmartPet.PetObjective.Fishing, who);
                            }
                        }
                        return;
                    }
                    else
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.TooManyPetsOnYourTeamWarning())), "", "DOD_TooManyPetsOnYourTeamWarning", new[] { who.UniqueMultiplayerID });
                    }
                    return;
                }
                if (responseKey.Equals("UnlockFishCommand"))
                {
                    if (this._petInfo.TrainedToday)
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.PetNeedsABreakAlert(PetName: this._petInfo.Name))), this._petInfo.Name, "DOD_PetNeedsABreakAlert", new[] { who.UniqueMultiplayerID });
                        return;
                    }
                    if (who.Items.ContainsId(this.peData.TrickLearningTreat) && who.Items.CountId(this.peData.TrickLearningTreat) >= 10 + (10 * (int)(4 * this._petInfo.SkillMastery_level[3])))
                    {
                        this._petInfo.TrainedToday = true;
                        if (who.IsMainPlayer)
                        {
                            who.Items.ReduceId(this.peData.TrickLearningTreat, 10 + (10 * (int)(4 * this._petInfo.SkillMastery_level[3])));
                        }
                        else
                        {
                            ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, int>>(new(this.peData.TrickLearningTreat, 10 + (10 * (int)(4 * this._petInfo.SkillMastery_level[3]))), "ReduceItemCall", new[] { ModEntry.ModID }, new[] { who.UniqueMultiplayerID });
                        }
                        this._petInfo.SkillMastery_level[3] += 0.25f;
                        if (this._petInfo.SkillMastery_level[3] >= 1)
                        {
                            if (who.IsMainPlayer)
                            {
                                Game1.drawObjectDialogue(Game1.parseText(I18n.CommandUnlockedAlert(PetName: this._petInfo.Name, CommandName: I18n.FishingLessonName())));
                            }
                            else
                            {
                                ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, string>>(new(this._petInfo.Name, "Fishing"), "DOD_CommandUnlockedAlert", new[] { ModEntry.ModID }, new[] { who.UniqueMultiplayerID });
                            }
                            PetHelper.PlaySoundForAllPlayersAtFarmerLocation("getNewSpecialItem", who.Tile, who);
                            PetLearnedANewSkill = true;
                        }
                        else
                        {
                            int num = Game1.random.Next(3);
                            if (who.IsMainPlayer)
                            {
                                Game1.drawObjectDialogue(Game1.parseText(num > 1 ? I18n.PetProgressedWithLessonRandomAns3(PetName: this._petInfo.Name, LessonName: I18n.FishingLessonName()) : num > 0 ? I18n.PetProgressedWithLessonRandomAns2(PetName: this._petInfo.Name, LessonName: I18n.FishingLessonName()) : I18n.PetProgressedWithLessonRandomAns1(PetName: this._petInfo.Name, LessonName: I18n.FishingLessonName())));
                            }
                            else
                            {
                                ModEntry.AHelper.Multiplayer.SendMessage<KeyValuePair<string, string>>(new(this._petInfo.Name, "Fishing"), num > 1 ? "DOD_PetProgressedWithLessonRandomAns3" : num > 0 ? "DOD_PetProgressedWithLessonRandomAns2" : "DOD_PetProgressedWithLessonRandomAns1", new[] { ModEntry.ModID }, new[] { who.UniqueMultiplayerID });
                            }
                            PetHelper.PlaySoundForAllPlayersAtFarmerLocation("reward", who.Tile, who);
                        }
                    }
                    else
                    {
                        IfClientSendMessageElseDoAction(who, () => Game1.drawObjectDialogue(Game1.parseText(I18n.NotEnoughTreatsAlert())), this._petInfo.Name, "DOD_NotEnoughTreatsAlert", new[] { who.UniqueMultiplayerID });
                    }
                    return;
                }
            }
            if (responseKey.Equals("ApplyButterflyPowderOption") && who.CurrentItem is not null && who.CurrentItem.QualifiedItemId.Equals("(O)ButterflyPowder"))
            {
                ButterflyPowderApply(who);
            }
        }
        public void ButterflyPowderApply(Farmer who)
        {
            GameLocation gameLocation = this.OriginalPetInstance.currentLocation;
            this.OriginalPetInstance.unassignPetBowl();
            gameLocation.characters.Remove(this.OriginalPetInstance);
            playContentSound(who);
            PetHelper.PlaySoundForAllPlayersAtFarmerLocation("fireball", this.Tile, who);
            Rectangle boundingBox = this.GetBoundingBox();
            boundingBox.Inflate(32, 32);
            boundingBox.X -= 32;
            boundingBox.Y -= 32;
            TemporaryAnimatedSpriteList temporarySprites = Utility.sparkleWithinArea(boundingBox, 6, Color.White, 50);
            temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) - new Vector2(32f), Color.White, 8, flipped: false, 50f));
            for (int i = 0; i < 8; i++)
            {
                temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), this.Position + new Vector2(32f) + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-32, 16)), flipped: false, 0.002f, Color.White)
                {
                    alphaFade = 0.00433333358f,
                    alpha = 0.75f,
                    motion = new Vector2((float)Game1.random.Next(-10, 11) / 20f, -1f),
                    acceleration = new Vector2(0f, 0f),
                    interval = 99999f,
                    layerDepth = 1f,
                    scale = 3f,
                    scaleChange = 0.01f,
                    rotationChange = (float)Game1.random.Next(-5, 6) * MathF.PI / 256f
                });
            }
            Game1.Multiplayer.broadcastSprites(gameLocation, temporarySprites);

            gameLocation.instantiateCrittersList();
            gameLocation.addCritter(new Butterfly(gameLocation, this.Tile + new Vector2(0f, 1f)));
            IfClientSendMessageElseDoAction(who, () => { if (Game1.player.CurrentItem is not null && Game1.player.CurrentItem.QualifiedItemId == "(O)ButterflyPowder") { who.reduceActiveItemByOne(); } }, "(O)ButterflyPowder", "ReduceActiveItemIdByOne", new[] { who.UniqueMultiplayerID });

            if (!SynchronizationManager.TryGetPetInventory(this.OriginalPetInstance).IsNull(out var _petInventory))
            {
                if (_petInventory.Count > SynchronizationManager.PetsEnhancedModInventoryHatSlotIndex && !_petInventory[SynchronizationManager.PetsEnhancedModInventoryHatSlotIndex].IsNull(out var _itemHat))
                {
                    Game1.createItemDebris(_itemHat, this.Position, -1, gameLocation);
                }
                if (_petInventory.Count > SynchronizationManager.PetsEnhancedModInventoryAccessorySlotIndex && !_petInventory[SynchronizationManager.PetsEnhancedModInventoryAccessorySlotIndex].IsNull(out var _itemAccessory))
                {
                    Game1.createItemDebris(_itemAccessory, this.Position, -1, gameLocation);
                }
                var backpackList = SynchronizationManager.TryGetItemListFromInventory(_petInventory, new List<Item>(6) { null, null, null, null, null, null }, SynchronizationManager.PetsEnhancedModInventoryBackpackStartIndex);
                foreach (Item itemDrop in backpackList)
                {
                    if (itemDrop is not null)
                    {
                        Game1.createItemDebris(itemDrop, this.Position, -1, gameLocation);
                    }
                }
                if (_petInventory.Count > SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex && !_petInventory[SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex].IsNull(out var _itemPocketSlot))
                {
                    Game1.createItemDebris(_itemPocketSlot, this.Position, -1, gameLocation);
                }
            }
            Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:ButterflyPowder_Goodbye", this.OriginalPetInstance.Name));
            if (this.GroupLeader is not null)
            {
                CachePetData.CachePetTeams.RemovePetFromTeam(this._petInfo.PetId, this.GroupLeader.UniqueMultiplayerID);
            }
            this.OriginalPetInstance = null;
            CachePetData.PetCache.Remove(_petInfo.PetId);
            ModConfig_Helper.ModDataLibrary.Remove(_petInfo.PetId);
        }
        public static void performPuffAnimation(Vector2 tile, GameLocation location)
        {
            Vector2 center = tile;
            int radius = 3;
            for (int x = (int)center.X - radius; (float)x < center.X + (float)radius; x++)
            {
                for (int y = (int)center.Y - radius; (float)y < center.Y + (float)radius; y++)
                {
                    if (Math.Round(Utility.distance(x, center.X, y, center.Y)) == (double)(radius - 1))
                    {
                        Game1.Multiplayer.broadcastSprites(location, Utility.getStarsAndSpirals(location, x, y, 1, 1, 100, 100, Color.White));
                    }
                }
            }
        }
        public void DoHappyReaction()
        {
            if (!isEmoting)
            {
                this.doEmote(heartEmote);
            }
            this.playContentSound(Game1.player);
        }
        public void DoAngryReaction()
        {
            if (!isEmoting)
            {
                this.doEmote(angryEmote);
            }
        }
        public static bool CustomCheckActionStatic(Farmer who, GameLocation l, Pet forPet, SynchronizationManager.PetInformation dynamicInformation)
        {
            if (forPet?.modData is null) { return false; }

            var petInventoryGlobal = SynchronizationManager.TryGetPetInventoryWithMutex(forPet, out NetMutex petInventoryGlobalMutex);

            if (petInventoryGlobal is null || petInventoryGlobalMutex is null) { return false; }

            Vector2 petDynamicPosition = new Vector2(dynamicInformation.PetPositionX, dynamicInformation.PetPositionY);

            bool petManualInHand = who.CurrentItem is not null && who.CurrentItem.QualifiedItemId.Equals(PetManualQUID);
            bool canBePet = (!forPet.lastPetDay.TryGetValue(who.UniqueMultiplayerID, out var value2) || value2 != Game1.Date.TotalDays);
            bool backpackUnlocked = petInventoryGlobal.Count > SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex && petInventoryGlobal[SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex]?.QualifiedItemId == PetBackpackQUID;

            if (canBePet)
            {
                forPet.lastPetDay[who.UniqueMultiplayerID] = Game1.Date.TotalDays;
                forPet.mutex.RequestLock(delegate
                {
                    if (!forPet.grantedFriendshipForPet.Value)
                    {
                        forPet.grantedFriendshipForPet.Set(newValue: true);
                        forPet.friendshipTowardFarmer.Set(Math.Min(1000, (int)forPet.friendshipTowardFarmer.Value + 12));

                        forPet.timesPet.Value++;
                    }

                    forPet.mutex.ReleaseLock();
                });
                petInventoryGlobalMutex.RequestLock(delegate
                {
                    if (petInventoryGlobal.Count > SynchronizationManager.PetsEnhancedModInventoryItemCouldntFitOnInventoryIndex && petInventoryGlobal[SynchronizationManager.PetsEnhancedModInventoryItemCouldntFitOnInventoryIndex] is not null)
                    {
                        Game1.createMultipleItemDebris(petInventoryGlobal[SynchronizationManager.PetsEnhancedModInventoryItemCouldntFitOnInventoryIndex], petDynamicPosition, -1, l, -1, flopFish: true);

                        petInventoryGlobal[SynchronizationManager.PetsEnhancedModInventoryItemCouldntFitOnInventoryIndex] = null;
                    }

                    petInventoryGlobalMutex.ReleaseLock();
                });

                if (isHostScreenId())
                {
                    if (CachePetData.PetCache.TryGetValue(forPet.petId.Value, out var petKit))
                    {
                        petKit?.Pet?.DoHappyReaction();
                    }
                }
                else
                {
                    ModEntry.AHelper.Multiplayer.SendMessage(forPet.petId.Value, "PEM_PetHappyReactionRequest", ModEntry.ModIDAsArray, new[] { MainPlayerID });
                }
                return true;
            }
            else
            {
                bool currentToolIndexValid = who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] is not null;
                if (currentToolIndexValid && who.Items[who.CurrentToolIndex] is Hat)
                {
                    petInventoryGlobalMutex.RequestLock(delegate
                    {
                        try
                        {
                            if (who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] is not null && who.Items[who.CurrentToolIndex] is Hat value)
                            {
                                if (currentToolIndexValid && who.Items[who.CurrentToolIndex].QualifiedItemId == value.QualifiedItemId)
                                {
                                    who.Items[who.CurrentToolIndex] = null;
                                }
                                else
                                {
                                    who.Items.ReduceId(value.QualifiedItemId, 1);
                                }

                                if (SynchronizationManager.GetItemFromInventoryAtIndex(petInventoryGlobal, SynchronizationManager.PetsEnhancedModInventoryHatSlotIndex, out Item itemToDrop))
                                {
                                    Game1.createItemDebris(itemToDrop, petDynamicPosition, -1, l);
                                }

                                Game1.sounds.PlayAll("dirtyHit", l, petDynamicPosition / Game1.tileSize, null, context: StardewValley.Audio.SoundContext.Default);
                                SynchronizationManager.TryAddItemListToInventory(petInventoryGlobal, new List<Item>() { value }, SynchronizationManager.PetsEnhancedModInventoryHatSlotIndex);
                            }
                        }
                        finally { petInventoryGlobalMutex.ReleaseLock(); }
                    });
                }
                else if (currentToolIndexValid && (who.CurrentItem.QualifiedItemId.Equals(LuxuryCollarQUID) || who.CurrentItem.QualifiedItemId.Equals(LightweightCollarQUID) || who.CurrentItem.QualifiedItemId.Equals(RoughCollarQUID) || who.CurrentItem.QualifiedItemId.Equals(FloweryCollarQUID) || who.CurrentItem.QualifiedItemId.Equals(SeagoingCollarQUID)))
                {
                    petInventoryGlobalMutex.RequestLock(delegate
                    {
                        try
                        {
                            if (who.CurrentItem is not null && (who.CurrentItem.QualifiedItemId.Equals(LuxuryCollarQUID) || who.CurrentItem.QualifiedItemId.Equals(LightweightCollarQUID) || who.CurrentItem.QualifiedItemId.Equals(RoughCollarQUID) || who.CurrentItem.QualifiedItemId.Equals(FloweryCollarQUID) || who.CurrentItem.QualifiedItemId.Equals(SeagoingCollarQUID)))
                            {
                                Item value = who.Items[who.CurrentToolIndex];

                                if (currentToolIndexValid && who.Items[who.CurrentToolIndex].QualifiedItemId == value.QualifiedItemId)
                                {
                                    who.Items[who.CurrentToolIndex] = null;
                                }
                                else
                                {
                                    who.Items.ReduceId(value.QualifiedItemId, 1);
                                }

                                if (SynchronizationManager.GetItemFromInventoryAtIndex(petInventoryGlobal, SynchronizationManager.PetsEnhancedModInventoryAccessorySlotIndex, out Item itemToDrop))
                                {
                                    Game1.createItemDebris(itemToDrop, petDynamicPosition, -1, l);
                                }

                                Game1.sounds.PlayAll("crit", l, petDynamicPosition / Game1.tileSize, null, context: StardewValley.Audio.SoundContext.Default);
                                SynchronizationManager.TryAddItemListToInventory(petInventoryGlobal, new List<Item>() { value }, SynchronizationManager.PetsEnhancedModInventoryAccessorySlotIndex);
                            }
                        }
                        finally { petInventoryGlobalMutex.ReleaseLock(); }
                    });
                }
                else if (currentToolIndexValid && !backpackUnlocked && who.CurrentItem.QualifiedItemId.Equals(PetBackpackQUID))
                {
                    petInventoryGlobalMutex.RequestLock(delegate
                    {
                        try
                        {
                            bool flag = petInventoryGlobal.Count > SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex && petInventoryGlobal[SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex]?.QualifiedItemId == PetBackpackQUID;

                            if (!flag && who.CurrentItem?.QualifiedItemId == PetBackpackQUID)
                            {
                                if (petInventoryGlobal.Count > SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex && !petInventoryGlobal[SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex].IsNull(out var movedItem))
                                {
                                    SynchronizationManager.TryAddItemListToInventory(petInventoryGlobal, new List<Item>() { movedItem }, SynchronizationManager.PetsEnhancedModInventoryBackpackStartIndex);
                                }

                                SynchronizationManager.TryAddItemListToInventory(petInventoryGlobal, new List<Item>() { who.CurrentItem.getOne() }, SynchronizationManager.PetsEnhancedModInventoryPetPocketSlotIndex);

                                PetHelper.PlaySoundForAllPlayersAtFarmerLocation("SunkenLace.PetsEnhancedMod.Sounds.BackpackEquiped", petDynamicPosition / Game1.tileSize, who);

                                who.reduceActiveItemByOne();
                            }
                        }
                        finally { petInventoryGlobalMutex.ReleaseLock(); }
                    });
                }
                else if (petManualInHand && !SynchronizationManager.SkillMasteryLevelStruct.AllSkillsUnlocked(forPet.modData))
                {
                    Game1.MusicDuckTimer = 2000f;
                    who.completelyStopAnimatingOrDoingAction();
                    who.freezePause = 3600;
                    who.canMove = false;

                    Vector2 tiledDynamicPosition = petDynamicPosition / Game1.tileSize;
                    DelayedAction.functionAfterDelay(delegate { PetHelper.PlaySoundForAllPlayersAtFarmerLocation("getNewSpecialItem", tiledDynamicPosition, who); Game1.drawObjectDialogue(Game1.parseText(I18n.PetTricksUnlockedAlert(PetName: forPet.Name))); }, 750);
                    PetHelper.PlaySoundForAllPlayersAtFarmerLocation("firework", tiledDynamicPosition, who);
                    performPuffAnimation(tiledDynamicPosition, l);

                    if (isHostScreenId())
                    {
                        if (CachePetData.PetCache.TryGetValue(forPet.petId.Value, out var petKit))
                        {
                            petKit?.Pet?.UnlockSkills();
                        }
                    }
                    else
                    {
                        ModEntry.AHelper.Multiplayer.SendMessage(forPet.petId.Value, "PEM_UnlockSkillsRequest", ModEntry.ModIDAsArray, new[] { MainPlayerID });
                    }
                }
                else if (who.CurrentItem is not null && CachePetData.GetDietListFromID(CachePetData.GetPetDataForPet(forPet).DietListID).ContainsKey(who.CurrentItem.QualifiedItemId))
                {
                    ReceiveTreat(who, forPet, dynamicInformation,petInventoryGlobal,petInventoryGlobalMutex);
                }
                else
                {
                    PetDialogueBox.CreateNewDialogueBoxClient(forPet, CommandPetStatic);
                }
            }
            return false;
        }
        public static void CommandPetStatic(string responseKey, Farmer who, Guid id)
        {
            if (isHostScreenId())
            {
                if (CachePetData.PetCache.TryGetValue(id, out var petKit))
                {
                    petKit?.Pet?.CommandPet(responseKey,who,id);
                }
            }
            else
            {
                ModEntry.AHelper.Multiplayer.SendMessage(new KeyValuePair<Guid, string>(id, responseKey), "CommandPetCall_client", ModEntry.ModIDAsArray, new[] { ModEntry.MainPlayerID });
            }
        }

        public Rectangle InteractiveBoundingBox
        {
            get
            {
                var rectangle = new Rectangle(GetBoundingBox().X - 32, GetBoundingBox().Y - 48, GetBoundingBox().Width + 64, GetBoundingBox().Height + 64);
                return rectangle;
            }
        }
        public static Rectangle GetInteractiveBoundingBoxFixed(int x,int y)
        {
            var bbox = GetBoundingBoxFixed(x, y);
            return new Rectangle(bbox.X - 32, bbox.Y - 48, bbox.Width + 64, bbox.Height + 64);
        }
        public virtual void playContentSound(Farmer whoAsked)
        {
            if (this.OriginalPetInstance is null) { return; }

            PetData petData = this.OriginalPetInstance.GetPetData();
            if (petData?.ContentSound is null)
            {
                return;
            }

            string contentSound = petData.ContentSound;
            PlaySound(contentSound, whoAsked, is_voice: true);
            if (petData.RepeatContentSoundAfter >= 0)
            {
                DelayedAction.functionAfterDelay(delegate
                {
                    PlaySound(contentSound, whoAsked, is_voice: true);
                }, petData.RepeatContentSoundAfter);
            }
        }
        public void ResetVariables()
        {
            this.Motion = Vector2.Zero;
            this.pathToFollow.Clear();
            ResetTargets();
            this.petSearchCooldown = petSearchPatienceTimer = BoardBounceModifier = 0;
            this.Swimming = this.BoardSinking = this.LockMovement = false;
        }
        public static (Vector2 _offset, int _direction, bool _drawHat, float _drawScale) getHatOffsetForPet(Dictionary<int, ModEntry.HatOffset_Simple> _dic, bool flip, int currentFrame, PetInfo.Pet_Types petType)
        {
            Vector2 AddedPos = Vector2.Zero;
            int dir = 2;
            bool _boolDrawHat = true;
            float _hatScale = 1.33333337f;
            if (_dic is not null && _dic.Count > 0)
            {
                if (_dic.TryGetValue(currentFrame, out var result2))
                {
                    dir = result2.Direction;
                    AddedPos += (new Vector2(result2.OffsetX, result2.OffsetY) * 4);
                    _hatScale *= result2.Scale;
                    _boolDrawHat = result2.DrawHat;
                }
            }
            else { _boolDrawHat = false; }

            if (petType == PetInfo.Pet_Types.LegacyCat || petType == PetInfo.Pet_Types.EnhancedCat)
            {
                if ((currentFrame >= 0 && currentFrame != 4 && currentFrame <= 23) || (currentFrame >= 59 && currentFrame <= 61))
                {
                    flip = false;
                }
            }
            else if (petType == PetInfo.Pet_Types.LegacyDog || petType == PetInfo.Pet_Types.EnhancedDog)
            {
                if ((currentFrame >= 0 && currentFrame != 4 && currentFrame < 20) || currentFrame == 27 || (currentFrame >= 36 && currentFrame < 39) || currentFrame == 42)
                {
                    flip = false;
                }
            }
            if (flip)
            {
                AddedPos.X = -(AddedPos.X + 4);
                switch (dir)
                {
                    case 1:
                        dir = 3;
                        break;
                    case 3:
                        dir = 1;
                        break;
                    default:
                        break;
                }
            }
            switch (dir)
            {
                case 0:
                    dir = 3;
                    break;
                case 2:
                    dir = 0;
                    break;
                case 3:
                    dir = 2;
                    break;
            }
            return (AddedPos, dir, _boolDrawHat, _hatScale);
        }
        public void SetOPetAtFarmPosition()
        {
            if (ModEntry.isHostScreenId())
            {
                if (!Game1.isRaining)
                {
                    PetBowl petBowl = this.OriginalPetInstance.GetPetBowl();
                    if (petBowl is not null)
                    {
                        faceDirection(2);
                        Game1.warpCharacter(this.OriginalPetInstance, petBowl.parentLocationName.Value, petBowl.GetPetSpot());
                        return;
                    }
                }
                this.OriginalPetInstance.warpToFarmHouse(Game1.MasterPlayer);
            }
        }
        public void UpdateEmote(GameTime time)
        {
            if (!isEmoting)
            {
                return;
            }

            emoteInterval += time.ElapsedGameTime.Milliseconds;
            if (EmoteFading && emoteInterval > 20f)
            {
                emoteInterval = 0f;
                currentEmoteFrame--;
                if (currentEmoteFrame < 0)
                {
                    EmoteFading = false;
                    isEmoting = false;
                }
            }
            else if (!EmoteFading && emoteInterval > 20f && currentEmoteFrame <= 3)
            {
                emoteInterval = 0f;
                currentEmoteFrame++;
                if (currentEmoteFrame == 4)
                {
                    currentEmoteFrame = currentEmote;
                }
            }
            else if (!EmoteFading && emoteInterval > 250f)
            {
                emoteInterval = 0f;
                currentEmoteFrame++;
                if (currentEmoteFrame >= currentEmote + 4)
                {
                    EmoteFading = true;
                    currentEmoteFrame = 3;
                }
            }
        }
        public bool ChangeEnergyLevel(int _amount, Farmer who)
        {
            if (this._petInfo.Energy > 0 && this._petInfo.Energy + _amount <= 0) { doEmote(sadEmote); DelayedAction.functionAfterDelay(() => PetHelper.PlaySoundForAllPlayersAtFarmerLocation("croak", this.Tile, who), 30); }

            bool e = _amount == 0 || (this._petInfo.Energy > 0 && _amount < 0) || (this._petInfo.Energy < this._petInfo.MaxBaseEnergy && _amount > 0);
            this._petInfo.Energy = Math.Clamp(this._petInfo.Energy + _amount, 0, this._petInfo.MaxBaseEnergy);
            return e;
        }
        public void DoWhenSwimming()
        {
            if ((this.CurrentLocation.doesTileHaveProperty((int)this.Tile.X, (int)this.Tile.Y, "TouchAction", "Back") == "PoolEntrance") || (this.Tile.X - (int)this.Tile.X > 0.5f && (this.CurrentLocation.doesTileHaveProperty((int)this.Tile.X + 1, (int)this.Tile.Y, "TouchAction", "Back") == "PoolEntrance")))
            {
                if ((float)this.StandingPixel.Y / 64 > (float)this.Tile.Y + 0.3f)
                {
                    if (!Swimming)
                    {
                        PetHelper.PlaySoundForAllPlayersAtFarmerLocation("pullItemFromWater", this.Tile, this.GroupLeader);
                        this.Swimming = true;
                        Game1.Multiplayer.broadcastSprites(this.CurrentLocation, new TemporaryAnimatedSprite(27, 100f, 4, 0, new Vector2(this.Position.X, this.StandingPixel.Y - 40), flicker: false, flipped: false)
                        {
                            layerDepth = 1f,
                            motion = new Vector2(0f, 2f)
                        });
                    }
                }
                else
                {
                    if (Swimming)
                    {
                        PetHelper.PlaySoundForAllPlayersAtFarmerLocation("dwop", this.Tile, this.GroupLeader);
                        PetHelper.PlaySoundForAllPlayersAtFarmerLocation("pullItemFromWater", this.Tile, this.GroupLeader);
                        this.Swimming = false;
                        this.BoardSinking = false;
                        this.BoardBounceModifier = 0;
                    }
                }
            }
            if (this.Swimming)
            {
                if (((int)this.xVelocity == 0 && (int)this.yVelocity == 0)) { this.BoardBounceModifier = 0; }
                animations.AnimatePetFloating(this._petInfo.PetType, this.facingDirection, true);
            }

        }
        public void OnWarp(Farmer _player)
        {
            if (this.GroupLeader is not null && this.GroupLeader.UniqueMultiplayerID == _player.UniqueMultiplayerID)
            {
                if (this.CurrentPetObjective != PetObjective.Wait)
                {
                    this.OnLeaderWarp(_player);
                    return;
                }

                if (this.CurrentPetObjective == PetObjective.Wait)
                {
                    if (!Game1.locations.Contains(this.CurrentLocation))
                    {
                        this.OnLeaderWarp(_player);
                    }
                }
            }
        }
        public void OnWarpFarmhand(long _farmhandUMID, int _facingDirection, Vector2 _position, string _locationNameOrUniqueName)
        {
            if (this.GroupLeader is not null && this.GroupLeader.UniqueMultiplayerID == _farmhandUMID)
            {
                if (this.CurrentPetObjective != PetObjective.Wait)
                {
                    this.OnLeaderWarp(_facingDirection, _position, _locationNameOrUniqueName);
                    return;
                }

                if (this.CurrentPetObjective == PetObjective.Wait)
                {
                    if (!Game1.locations.Contains(this.CurrentLocation))
                    {
                        this.OnLeaderWarp(_facingDirection, _position, _locationNameOrUniqueName);
                    }
                }
            }
        }
        private void UpdatefoodSaturationTimer()
        {
            if (++this.foodSaturationTimer >= 6000)
            {
                this.foodSaturationLevel.positive = Math.Clamp(this.foodSaturationLevel.positive - 1, 0, 6);
                this.foodSaturationLevel.negative = Math.Clamp(this.foodSaturationLevel.negative + 1, -5, 0);
                this.foodSaturationTimer = 0;
            }
        }
        private void UpdateIdleTimer()
        {
            if (this.readyToBeIddle && !this.Swimming)
            {
                if (++this.IdleTimer > 1000)
                {
                    bool _isTurtle = this.OriginalPetInstance.petType is not null && (this.OriginalPetInstance.petType.Contains("turtle") || this.OriginalPetInstance.petType.Contains("Turtle"));
                    AnimatePetWaiting(this._petInfo.PetType, _isTurtle);
                }
                return;
            }
            if (IdleTimer > 1000)
            {
                animations.resetAnimations();
            }
            this.IdleTimer = 0;
        }
        private void AnimatePetWaiting(PetInfo.Pet_Types type,bool isTurtle)
        {
            if (this.Swimming) { return; }
            switch (type)
            {
                case PetInfo.Pet_Types.EnhancedCat:
                    animations.AnimateCatWaiting(this.ActualPetDirection, ActualPetState);
                    break;
                case PetInfo.Pet_Types.LegacyCat:
                    animations.AnimateCatClassicWaiting(ActualPetState);
                    break;
                case PetInfo.Pet_Types.EnhancedDog:
                    animations.AnimateDogWaiting(this.ActualPetDirection, ActualPetState);
                    break;
                case PetInfo.Pet_Types.LegacyDog:
                    {
                        if (!isTurtle)
                        {
                            animations.AnimateDogClassicWaiting(this.ActualPetDirection, ActualPetState);
                        }
                        else { animations.AnimateTurtleWaiting(isTurtle && this.Sprite.CurrentFrame == 20? PetState.Sit : ActualPetState); }
                    }
                    break;
                default:
                    animations.AnimateDogClassicWaiting(this.ActualPetDirection, ActualPetState);
                    break;
            }
        }
        private void warpOPetIfOutOfFarm()
        {
            if (this.OriginalPetInstance.currentLocation is not Farm && this.OriginalPetInstance.currentLocation is not FarmHouse)
            {
                SetOPetAtFarmPosition();
            }
            if (this.CurrentPetObjective == PetObjective.None && this.OriginalPetInstance.currentLocation.IsOutOfBounds(this.OriginalPetInstance.GetBoundingBox()))
            {
                SetOPetAtFarmPosition();
            }
        }
        public bool AllSkillsLearned()
        {
            if (this.peData is null || this._petInfo is null) { return false; }
            bool _result = (this.peData.HasWaitSkill && this._petInfo.SkillMastery_level[0] < 1) || ((this.peData.HasFollowSkill && this._petInfo.SkillMastery_level[1] < 1) || !this.peData.HasFollowSkill) || (this.peData.HasForageSkill && this._petInfo.SkillMastery_level[2] < 1) || (this.peData.HasFishingSkill && this._petInfo.SkillMastery_level[3] < 1) || (this.peData.HasHuntSkill && this._petInfo.SkillMastery_level[4] < 1) ? false : true;
            return _result;
        }
        private void CheckInventoryForGiftedItem()
        {
            var inventory = SynchronizationManager.TryGetPetInventoryWithMutex(this.OriginalPetInstance,out var inventoryMutex);
            if (inventory is null || inventoryMutex is null) { return; }
            if (inventory.Count > SynchronizationManager.PetsEnhancedModInventoryGiftedItemIndex && inventory[SynchronizationManager.PetsEnhancedModInventoryGiftedItemIndex] is not null)
            {
                inventoryMutex.RequestLock(delegate
                {
                    try
                    {
                        if (inventory.Count > SynchronizationManager.PetsEnhancedModInventoryGiftedItemIndex && inventory[SynchronizationManager.PetsEnhancedModInventoryGiftedItemIndex] is not null)
                        {
                            var itemHere = inventory[SynchronizationManager.PetsEnhancedModInventoryGiftedItemIndex];
                            var _definedDietList = CachePetData.GetDietListFromID(this.peData.DietListID);
                            if (!string.IsNullOrEmpty(itemHere.QualifiedItemId) && _definedDietList is not null && _definedDietList.Count > 0 && _definedDietList.TryGetValue(itemHere.QualifiedItemId, out var foundDietItem))
                            {
                                bool firstTreatOfTheDay = false;
                                int friendshipAdded = foundDietItem.FriendshipGain;
                                if (!this._petInfo.HasBeenGivenTreatToday)
                                {
                                    firstTreatOfTheDay = true;
                                    this._petInfo.HasBeenGivenTreatToday = true;

                                    this.OriginalPetInstance.mutex.RequestLock(delegate
                                    {
                                        this.OriginalPetInstance.grantedFriendshipForPet.Set(newValue: true);
                                        this.OriginalPetInstance.friendshipTowardFarmer.Set(Math.Max(Math.Min(1000, (int)this.OriginalPetInstance.friendshipTowardFarmer.Value + friendshipAdded), 0));

                                        this.OriginalPetInstance.mutex.ReleaseLock();
                                    });
                                }

                                ChangeEnergyLevel((int)((float)foundDietItem.EnergyGain * (((float)itemHere.Quality * 0.15f) + 1f)), Game1.player);

                                bool val = foundDietItem.EnergyGain < 0;
                                this.doEmote((firstTreatOfTheDay && friendshipAdded < 0) || (!firstTreatOfTheDay && val) ? angryEmote : heartEmote);
                                Game1.sounds.PlayAll(val ? "cancel" : "give_gift", this.CurrentLocation, this.Tile, null, context: StardewValley.Audio.SoundContext.Default);
                                if (foundDietItem.EnergyGain >= 0)
                                {
                                    this.foodSaturationLevel.positive++;
                                }
                                else
                                {
                                    this.foodSaturationLevel.negative--;
                                }
                                if (friendshipAdded >= 0)
                                {
                                    this.playContentSound(Game1.player);
                                }

                                SynchronizationManager.TryAddKnownEdibleItemToInventoryTag(inventory, itemHere.QualifiedItemId);

                            }

                            inventory[SynchronizationManager.PetsEnhancedModInventoryGiftedItemIndex] = null;
                        }
                    }
                    finally
                    {
                        inventoryMutex.ReleaseLock();
                    }
                });
            }
        }
        private void UpdatePlayerMouse()
        {
            bool CanInteract = Game1.activeClickableMenu is null && !Game1.dialogueUp && ShouldTimePass && Game1.player.CanMove && Game1.CurrentEvent is null;
            if (!CanInteract) { return; }

            var mousePos = ModEntry.AHelper.Input.GetCursorPosition().AbsolutePixels;
            if (this.CurrentLocation.NameOrUniqueName.EqualsIgnoreCase(Game1.player.currentLocation.NameOrUniqueName) && this.InteractiveBoundingBox.Contains(mousePos))
            {
                Point psPixel = Game1.player.StandingPixel;
                Point sPixel = this.StandingPixel;
                Item item = Game1.player.CurrentItem;
                bool itemNotNull = item is not null;
                float distanceFromPlayer = Utility.distance(sPixel.X, psPixel.X, sPixel.Y, psPixel.Y);

                if (itemNotNull && CachePetData.GetDietListFromID(peData.DietListID).ContainsKey(item.QualifiedItemId))
                {
                    Game1.mouseCursor = Game1.cursor_gift;
                    Game1.mouseCursorTransparency = distanceFromPlayer <= 128 ? 1f : 0.5f;
                }
                else if (itemNotNull && (item.QualifiedItemId.Equals("(O)ButterflyPowder") || item is Hat))
                {
                    Game1.mouseCursor = Game1.cursor_harvest;
                    Game1.mouseCursorTransparency = distanceFromPlayer <= 128 ? 1f : 0.5f;
                }
                else
                {
                    Game1.mouseCursor = Game1.cursor_talk;
                    Game1.mouseCursorTransparency = distanceFromPlayer <= 128 ? 1f : 0.5f;
                }
            }
        }
        public void PetFollowingGainExp(bool isOneSecond)
        {
            if ((this._petInfo.Energy <= 0) || !isOneSecond) { return; }
            this._petInfo.SkillMastery_level[1] = Math.Min(this._petInfo.SkillMastery_level[1] + CalculateExpForSkillMastery(0.026d, this._petInfo.SkillMastery_level[1]), 5);
            if (this._petInfo.SkillMastery_level[1] >= 5)
            {
                this._petInfo.MaxBaseEnergy = MaxBaseEnergyNoUpgrade + 80;
            }
        }
        public void PetWaitingChargeEnergy(bool isOneSecond)
        {
            if ((this._petInfo.Energy >= this._petInfo.MaxBaseEnergy) || !isOneSecond) { return; }
            int energyGain = this._petInfo.SkillMastery_level[0] >= 3 ? 4 : this._petInfo.SkillMastery_level[0] >= 2 ? 3 : 2;
            this._petInfo.Energy = Math.Clamp(this._petInfo.Energy + energyGain, 0, this._petInfo.MaxBaseEnergy);
            this._petInfo.SkillMastery_level[0] = Math.Min(this._petInfo.SkillMastery_level[0] + CalculateExpForSkillMastery(0.05d, this._petInfo.SkillMastery_level[0]), 5);
        }
        private int currentTick = 0;
        public void Update(GameTime time)
        {
            bool hostPaused = true;
            if (currentTick < Game1.ticks)
            {
                currentTick = Game1.ticks;
                hostPaused = false;
            }
            this.readyToBeIddle = false;
            UpdatePetDataIfOPetTextureNotMatchSPet();
            if (!hostPaused)
            {
                UpdateEmote(time);
                this.DoWhenSwimming();
                this.emoteCooldown = this.emoteCooldown > 0 ? this.emoteCooldown - 1 : this.emoteCooldown;
                UpdatefoodSaturationTimer();
            }

            CheckInventoryForGiftedItem();
            warpOPetIfOutOfFarm();
            UpdatePlayerMouse();
            if (this.CurrentPetObjective == PetObjective.None)
            {
                this.CurrentPetAction = PetAction.None;
                this.CurrentLocation = this.OriginalPetInstance.currentLocation;
                this.Position = this.OriginalPetInstance.Position + new Vector2(32, 0f);
            }

            if (this.CurrentPetObjective != PetObjective.None)
            {
                this.OriginalPetInstance.Sprite.CurrentAnimation = null;
                this.OriginalPetInstance.Position = new Vector2(-5000, -5000);
                this.UpdateFlipStatus();
                if (this.OriginalPetInstance.currentLocation is not FarmHouse)
                {
                    Game1.warpCharacter(this.OriginalPetInstance, Utility.getHomeOfFarmer(Game1.player), new Vector2(-5000, 5000));
                }
                if (this.CurrentPetObjective == PetObjective.Wait && !hostPaused)
                {
                    PetWaitingChargeEnergy(time.TotalGameTime.Ticks % 70 == 0);
                    bool _isTurtle = this.OriginalPetInstance.petType is not null && (this.OriginalPetInstance.petType.Contains("turtle") || this.OriginalPetInstance.petType.Contains("Turtle"));
                    this.CurrentPetAction = PetAction.Wait;
                    AnimatePetWaiting(this._petInfo.PetType, _isTurtle);
                }
                else if (!hostPaused)
                {
                    if (this.CurrentPetObjective == PetObjective.Follow)
                    {
                        PetFollowingGainExp(time.TotalGameTime.Ticks % 70 == 0);
                    }
                    this.PerformBehavior(time, this.CurrentPetObjective);
                }
            }
            if (!hostPaused)
            {
                this.UpdateIdleTimer();
            }

        }
        private void UpdateFlipStatus()
        {
            if (petType == PetInfo.Pet_Types.LegacyCat || petType == PetInfo.Pet_Types.EnhancedCat)
            {
                if ((this.Sprite.CurrentFrame >= 0 && this.Sprite.CurrentFrame <= 23) || (this.Sprite.CurrentFrame >= 59 && this.Sprite.CurrentFrame <= 61))
                {
                    this.flip = false;
                }
            }
            else if (petType == PetInfo.Pet_Types.LegacyDog || petType == PetInfo.Pet_Types.EnhancedDog)
            {
                if ((this.Sprite.CurrentFrame >= 0 && this.Sprite.CurrentFrame < 20) || this.Sprite.CurrentFrame == 27 || (this.Sprite.CurrentFrame >= 36 && this.Sprite.CurrentFrame < 39) || this.Sprite.CurrentFrame == 42)
                {
                    this.flip = false;
                }
            }
        }
        private void UpdatePetDataIfOPetTextureNotMatchSPet()
        {
            if (this.OriginalPetInstance.Sprite is not null && this.Sprite.Texture != this.OriginalPetInstance.Sprite.Texture)
            {
                string petAssetPath = this.OriginalPetInstance.getPetTextureName();
                this.LoadSpriteTexture(petAssetPath);
                this.Sprite.sourceRect.Y = this.OriginalPetInstance.Sprite.sourceRect.Y;

                if (this.Sprite.Texture is not null)
                {
                    PetInfo.Pet_Types petType = PetHelper.GetPetTypeFromTextureHeightAndOpetType(this.OriginalPetInstance.petType?.Value, this.Sprite.Texture.Height);
                    this._petInfo.PetType = petType;
                    this.petType = petType;

                    if (PetHelper.CheckIfPetConfigExistAnValid(petType, petAssetPath, out var peData))
                    {
                        this.peData = peData;
                    }
                    else
                    {
                        this.peData = PetContent.getDefaultDataForClassicPet(this.OriginalPetInstance.petType?.Value);
                    }
                }
            }
        }

        private void LoadSpriteTexture(string textureName, bool syncTextureName = true)
        {
            if (this.Sprite is null) { return; }
            if (Game1.content.DoesAssetExist<Texture2D>(textureName))
            {
                if (syncTextureName)
                {
                    this.Sprite.textureName.Value = textureName;
                    this.Sprite.overrideTextureName = null;
                }
                else
                {
                    this.Sprite.overrideTextureName = textureName;
                }

                loadSpriteTexture();
            }
        }

        private void loadSpriteTexture()
        {
            string text = this.Sprite.overrideTextureName ?? this.Sprite.textureName.Value;
            if (!(this.Sprite.loadedTexture == text))
            {
                this.Sprite.spriteTexture = ((text != null) ? PetHelper.TryLoadTextureEfficiently(text) : null);
                this.Sprite.loadedTexture = text;
                if (this.Sprite.spriteTexture != null)
                {
                    this.Sprite.UpdateSourceRect();
                }
            }
        }

        public void Draw(SpriteBatch b, Vector2 _viewport)
        {
            try
            {
                this.Sprite.UpdateSourceRect();

                SynchronizationManager.PetInformation information = new(this);
                SynchronizationManager.SetFlag(4, this.CurrentPetObjective != SmartPet.PetObjective.None, ref information.Flags);
                var spriteInfo = new SynchronizationManager.PetSpriteInformation(this.OriginalPetInstance, information,false);

                spriteInfo.DrawSprite(b, _viewport);
            }
            catch (ArgumentNullException error)
            {
                ModEntry.WriteMonitor($"An error ocurred when drawing textures, details: {error}", LogLevel.Error);
            }
        }
        public static int GetBoardSpriteSrcYPosition(int BGindex)
        {
            return (BGindex == 1 || BGindex == 8 || BGindex == 14) ? 64 : (BGindex == 2 || BGindex == 4) ? 128 : (BGindex == 3 || BGindex == 6 || BGindex == 13) ? 32 : (BGindex == 15 || BGindex == 12) ? 96 : (BGindex == 5 || BGindex == 11 || BGindex == 10) ? 160 : 0;
        }
        public static bool IsInsideRadius(Vector2 center, Vector2 point, float radius)
        {
            float distance = Vector2.Distance(center, point);
            return distance <= radius;
        }
        public void PlaySound(string sound,Farmer whoAsked, bool is_voice)
        {
            if (string.IsNullOrEmpty(sound)) { return; }
            float num = 1f;
            PetBreed breedById = this.OriginalPetInstance?.GetPetData()?.GetBreedById(WhichBreed.ToString(), true);
            if (breedById is null) { return; }
            if (sound == "BARK")
            {
                sound = this.OriginalPetInstance.GetPetData().BarkSound;
                if (breedById.BarkOverride is not null)
                {
                    sound = breedById.BarkOverride;
                }
            }

            if (is_voice)
            {
                num = breedById.VoicePitch;
            }
            PetHelper.PlaySoundForAllPlayersAtFarmerLocation(sound, this.Tile, whoAsked, (int)(num * 1200));
        }
        public void resetOPetAnimation()
        {
            this.OriginalPetInstance.Sprite.CurrentAnimation = null;
            this.OriginalPetInstance.CurrentBehavior = "Walk";
        }
        public void ResetTargets()
        {
            if (this.target is not null)
            {
                CachePetData.CachePetTeams.TryRemoveTargetToTargetedMonsters(this.target as Monster);
            }
            this.targetObject = null;
            this.target = null;
            this.targetTile = null;
        }
        public void SetPetObjective(PetObjective objective, Farmer who)
        {
            this.PrevPetObjective = this.CurrentPetObjective;
            this.CurrentPetObjective = objective;
            this.Sprite.CurrentAnimation = null;
            ResetTargets();
            this.petSearchPatienceTimer = petSearchCooldown = 0;
            resetOPetAnimation();
            animations.resetAnimations();
            this.Sprite.UpdateSourceRect();
            this.OriginalPetInstance.Sprite.UpdateSourceRect();
            if ((int)objective > 0)
            {
                this.OriginalPetInstance.Position = new Vector2(-5000, -5000);
                if (objective != PetObjective.Wait)
                {
                    CachePetData.CachePetTeams.RelocatePetToTeamOtherwiseCreateNew(this._petInfo.PetId, who.UniqueMultiplayerID);
                }
            }
            else
            {
                this.Motion = Vector2.Zero;
                this.pathToFollow.Clear();
                this.PrevPetState = this.CurrentPetState;
                this.CurrentPetState = PetState.NonDefined;
                this.OriginalPetInstance.Sprite.CurrentFrame = this.ActualPetDirection == Directions.West ? 12 : this.ActualPetDirection == Directions.North ? 8 : this.ActualPetDirection == Directions.East ? 4 : 0;
                this.Sprite.CurrentFrame = this.OriginalPetInstance.Sprite.CurrentFrame;
                this.flip = this.OriginalPetInstance.flip = false;
                if ((this.CurrentLocation is Farm || this.CurrentLocation is FarmHouse) && !this.OriginalPetInstance.currentLocation.NameOrUniqueName.EqualsIgnoreCase(this.CurrentLocation.NameOrUniqueName))
                {
                    Game1.warpCharacter(this.OriginalPetInstance, this.CurrentLocation, this.Position - new Vector2(32, 0));
                }
                this.OriginalPetInstance.Position = this.Position - new Vector2(32, 0);
                if (this.GroupLeader is not null)
                {
                    CachePetData.CachePetTeams.RemovePetFromTeam(this._petInfo.PetId, this.GroupLeader.UniqueMultiplayerID);
                }
            }
        }
        public void SetSPetToCopyOPet()
        {
            this.Motion = Vector2.Zero;
            this.pathToFollow.Clear();
            this.PrevPetState = this.CurrentPetState;
            this.CurrentPetState = PetState.NonDefined;
            this.Sprite.CurrentFrame = this.OriginalPetInstance.Sprite.CurrentFrame;
            this.flip = this.OriginalPetInstance.flip = false;
            this.Position = this.OriginalPetInstance.Position + new Vector2(32, 0);
            this.CurrentPetAction = PetAction.None;
            this.CurrentLocation = this.OriginalPetInstance.currentLocation;
        }
    }

}
