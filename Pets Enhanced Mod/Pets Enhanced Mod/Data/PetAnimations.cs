using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using static StardewValley.Objects.BedFurniture;
using StardewModdingAPI.Framework;
using StardewValley.Characters;
using Pets_Enhanced_Mod.Utilities;
using Pets_Enhanced_Mod.Utilities.Custom_Classes;
using System.ComponentModel;
using static Pets_Enhanced_Mod.Utilities.Custom_Classes.SmartPet;
using StardewValley.Extensions;
using static Pets_Enhanced_Mod.Utilities.CachePetData;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Xna.Framework.Media;

namespace Pets_Enhanced_Mod.Data
{
    public class PetAnimations
    {
        private int ATimer = 0;
        private int BTimer = 0;
        private int CTimer = 0;
        private int DTimer = 0;
        private int ETimer = 0;

        public bool AnimatingPet { get; set; }
        public bool AnimatingSprite { get; set; }

        public bool ClosingOpeningEyes { get; set; } = false;
        public int animationChance { get; set; }

        public int animationChanceB { get; set; }

        public int CloseOpenEyesChance { get; set; }

        public bool FinishedAnimatingPetSittingDown = true;

        public int floatTimer = 0;

        public int blinkTimer = 0;
        public bool EyesClosed = false;
        public SmartPet pet { get; set; }

        public int frameIndex = 0;
        public int frameIndexB = 0;
        public int frameIndexB1 = 0;
        public int frameIndexC = 0;

        public int currentTick = 0;
        public int currentTickB = 0;

        /// <summary>
        /// A boolean for whether pet was sitting before or not
        /// </summary>
        public PetAnimations(SmartPet pet)
        {
            if (pet == null) { return; }

            this.pet = pet;
        }
        public void resetAnimations()
        {
            this.frameIndexC = 0;
            this.ATimer = 0;
            this.BTimer = 0;
            this.CTimer = 0;
            this.AnimatingPet = false;
            this.floatTimer = 0;
            this.blinkTimer = 0;
            this.EyesClosed = false;
            this.ClosingOpeningEyes = false;
            this.frameIndex = 0;
            this.currentTick = 0;
            this.frameIndexB = 0;
            this.frameIndexB1 = 0;
            this.currentTickB = 0;
            this.AnimatingSprite = false;

        }
        public void PlayAllInPetLocation(string _sound, int? pitch = null)
        {
            if (Game1.options.muteAnimalSounds) { return; }
            Game1.sounds.PlayAll(_sound, this.pet.CurrentLocation, this.pet.Tile, pitch, context: StardewValley.Audio.SoundContext.Default);
        }
        public void AnimateDogWaiting(SmartPet.Directions direction, SmartPet.PetState currentState)
        {
            if (pet == null) { return; }
            this.ATimer++;
            pet.Sprite.CurrentAnimation = null;
            this.blinkTimer++;
            if (this.blinkTimer > 400)
            {
                this.blinkTimer = 0;
                this.ClosingOpeningEyes = false;
            }
            if (!ClosingOpeningEyes)
            {
                this.CloseOpenEyesChance = Game1.random.Next(3);
                this.ClosingOpeningEyes = true;
            }
            if (direction == SmartPet.Directions.North || direction == SmartPet.Directions.South)
            {
                this.pet.flip = false;
                if (ATimer > 200)
                {
                    if (!AnimatingPet)
                    {
                        this.animationChance = Game1.random.Next(10);
                    }
                    switch (this.animationChance) //Do animation based on random chance.
                    {
                        case 4 or 6: //Pet panting slow
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 40;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 36 : 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 37 : 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 36 : 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 37 : 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 36 : 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0;}
                                }
                                if (frameIndex == 5)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }
                        case 3 or 7: //Pet panting faster
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 18;

                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 36 : 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 37 : 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 36 : 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 37 : 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 36 : 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 5)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 37 : 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 6)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 36 : 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0;}
                                }
                                if (frameIndex == 7)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }

                                break;
                            }
                        case 1://Pet panting animation
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 30;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 36 : 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 37 : 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 36 : 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 37 : 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 36 : 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }

                                }
                                if (frameIndex == 5)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 37 : 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 6)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 36 : 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0;}
                                }
                                if (frameIndex == 7)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }

                        default: //animationChance == 0
                            this.ATimer = 0;
                            pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 27 : 38;
                            break;
                    }
                }
                else if (currentState != PetState.Sit || !FinishedAnimatingPetSittingDown)
                {
                    FinishedAnimatingPetSittingDown = false;
                    this.AnimatingPet = true;
                    int currentFrameDuration = 20;
                    if (frameIndex == 0)
                    {
                        pet.Sprite.CurrentFrame = 17;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 1)
                    {
                        pet.Sprite.CurrentFrame = 38;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 2)
                    {
                        this.AnimatingPet = false;
                        currentTick = 0;
                        frameIndex = 0;
                        FinishedAnimatingPetSittingDown = true;
                    }

                }
                else { pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2? 27 : 38; } //Default frame sit still
            }
            if (direction == SmartPet.Directions.East || direction == SmartPet.Directions.West)
            {
                this.pet.flip = direction == SmartPet.Directions.West;
                if (ATimer > 200)
                {
                    if (!AnimatingPet)
                    {
                        this.animationChance = Game1.random.Next(8);
                    }
                    switch (this.animationChance) //Do animation based on random chance.
                    {
                        case 4 or 6: //Pet panting slow
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 40;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 25 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 24 : 41;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 25 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 24 : 41;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 25 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0;}
                                }
                                if (frameIndex == 5)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }
                        case 3 or 7: //Pet panting faster
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 18;

                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 25 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 24 : 41;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 25 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 24 : 41;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 25 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 5)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 24 : 41;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 6)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 25 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 7)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }

                                break;
                            }
                        case 1://Pet panting animation
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 30;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 25 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 24 : 41;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 25 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 24 : 41;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 25 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }

                                }
                                if (frameIndex == 5)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 24 : 41;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 6)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 25 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0;}
                                }
                                if (frameIndex == 7)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }

                        default: //animationChance == 0
                            this.ATimer = 0;
                            pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 30 : 23;
                            break;
                    }
                }
                else if (currentState != PetState.Sit || !FinishedAnimatingPetSittingDown)
                {
                    FinishedAnimatingPetSittingDown = false;
                    this.AnimatingPet = true;
                    int currentFrameDuration = 10;
                    if (frameIndex == 0)
                    {
                        pet.Sprite.CurrentFrame = 21;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 1)
                    {
                        pet.Sprite.CurrentFrame = 22;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 2)
                    {
                        pet.Sprite.CurrentFrame = 23;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 3)
                    {
                        this.AnimatingPet = false;
                        currentTick = 0;
                        frameIndex = 0;
                        FinishedAnimatingPetSittingDown = true;
                    }

                }
                else { pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 30 :23; } //Default frame sit still
            }

        }

        public void AnimateDogClassicWaiting(SmartPet.Directions direction, SmartPet.PetState currentState)
        {
            if (pet == null) { return; }
            this.ATimer++;
            pet.Sprite.CurrentAnimation = null;
            this.blinkTimer++;
            if (this.blinkTimer > 400)
            {
                this.blinkTimer = 0;
                this.ClosingOpeningEyes = false;
            }
            if (!ClosingOpeningEyes)
            {
                this.CloseOpenEyesChance = Game1.random.Next(3);
                this.ClosingOpeningEyes = true;
            }
            if (direction == SmartPet.Directions.North || direction == SmartPet.Directions.South)
            {
                this.pet.flip = false;
                if (ATimer > 200)
                {
                    if (!AnimatingPet)
                    {
                        this.animationChance = Game1.random.Next(10);
                    }
                    switch (this.animationChance) //Do animation based on random chance.
                    {
                        case 4 or 6: //Pet panting slow
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 40;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 5)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }
                        case 3 or 7: //Pet panting faster
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 18;

                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 5)
                                {
                                    pet.Sprite.CurrentFrame = 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 6)
                                {
                                    pet.Sprite.CurrentFrame = 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 7)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }

                                break;
                            }
                        case 1://Pet panting animation
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 30;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }

                                }
                                if (frameIndex == 5)
                                {
                                    pet.Sprite.CurrentFrame = 19;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 6)
                                {
                                    pet.Sprite.CurrentFrame = 18;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 7)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }

                        default: //animationChance == 0
                            this.ATimer = 0;
                            pet.Sprite.CurrentFrame = 18;
                            break;
                    }
                }
                else if (currentState != PetState.Sit || !FinishedAnimatingPetSittingDown)
                {
                    FinishedAnimatingPetSittingDown = false;
                    this.AnimatingPet = true;
                    int currentFrameDuration = 20;
                    if (frameIndex == 0)
                    {
                        pet.Sprite.CurrentFrame = 17;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 1)
                    {
                        pet.Sprite.CurrentFrame = 18;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 2)
                    {
                        this.AnimatingPet = false;
                        currentTick = 0;
                        frameIndex = 0;
                        FinishedAnimatingPetSittingDown = true;
                    }

                }
                else { pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 27 : 38; } //Default frame sit still
            }
            if (direction == SmartPet.Directions.East || direction == SmartPet.Directions.West)
            {
                this.pet.flip = direction == SmartPet.Directions.West;
                if (ATimer > 200)
                {
                    if (!AnimatingPet)
                    {
                        this.animationChance = Game1.random.Next(8);
                    }
                    switch (this.animationChance) //Do animation based on random chance.
                    {
                        case 4 or 6: //Pet panting slow
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 40;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = 25;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = 24;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = 25;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = 24;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = 25;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 5)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }
                        case 3 or 7: //Pet panting faster
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 18;

                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = 25;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = 24;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = 25;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = 24;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = 25;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 5)
                                {
                                    pet.Sprite.CurrentFrame = 24;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 6)
                                {
                                    pet.Sprite.CurrentFrame = 25;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 7)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }

                                break;
                            }
                        case 1://Pet panting animation
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 30;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = 25;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = 24;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = 25;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = 24;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = 25;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("dog_pant"); }

                                }
                                if (frameIndex == 5)
                                {
                                    pet.Sprite.CurrentFrame = 24;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 6)
                                {
                                    pet.Sprite.CurrentFrame = 25;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 7)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }

                        default: //animationChance == 0
                            this.ATimer = 0;
                            pet.Sprite.CurrentFrame = 23;
                            break;
                    }
                }
                else if (currentState != PetState.Sit || !FinishedAnimatingPetSittingDown)
                {
                    FinishedAnimatingPetSittingDown = false;
                    this.AnimatingPet = true;
                    int currentFrameDuration = 10;
                    if (frameIndex == 0)
                    {
                        pet.Sprite.CurrentFrame = 21;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 1)
                    {
                        pet.Sprite.CurrentFrame = 22;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 2)
                    {
                        pet.Sprite.CurrentFrame = 23;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 3)
                    {
                        this.AnimatingPet = false;
                        currentTick = 0;
                        frameIndex = 0;
                        FinishedAnimatingPetSittingDown = true;
                    }

                }
                else { pet.Sprite.CurrentFrame = 23; } //Default frame sit still
            }

        }

        public void AnimateTurtleWaiting(SmartPet.PetState currentState)
        {
            if (pet == null) { return; }
            pet.Sprite.CurrentAnimation = null;
            this.pet.flip = false;
            if (currentState != PetState.Sit || !FinishedAnimatingPetSittingDown)
            {
                FinishedAnimatingPetSittingDown = false;
                this.AnimatingPet = true;
                int currentFrameDuration = 20;
                if (frameIndex == 0)
                {
                    pet.Sprite.CurrentFrame = 16;
                    currentTick++;
                    if (currentTick > 10) { frameIndex++; currentTick = 0; }
                }
                if (frameIndex == 1)
                {
                    pet.Sprite.CurrentFrame = 17;
                    currentTick++;
                    if (currentTick > 15) { frameIndex++; currentTick = 0; }
                }
                if (frameIndex == 2)
                {
                    pet.Sprite.CurrentFrame = 18;
                    currentTick++;
                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                }
                if (frameIndex == 3)
                {
                    pet.Sprite.CurrentFrame = 19;
                    currentTick++;
                    if (currentTick > 15) { frameIndex++; currentTick = 0; }
                }
                if (frameIndex == 4)
                {
                    this.AnimatingPet = false;
                    currentTick = 0;
                    frameIndex = 0;
                    FinishedAnimatingPetSittingDown = true;
                    pet.Sprite.CurrentFrame = 20;
                }

            }
            else { pet.Sprite.CurrentFrame = 20; } //Default frame sit still

        }
        public void AnimateCatWaiting(SmartPet.Directions direction, SmartPet.PetState currentState)
        {
            if (pet == null) { return; }
            this.ATimer++;
            pet.Sprite.CurrentAnimation = null;
            this.blinkTimer++;
            if (this.blinkTimer > 400)
            {
                this.blinkTimer = 0;
                this.ClosingOpeningEyes = false;
            }
            if (!ClosingOpeningEyes)
            {
                this.CloseOpenEyesChance = Game1.random.Next(3);
                this.ClosingOpeningEyes = true;
            }
            if (direction == SmartPet.Directions.North || direction == SmartPet.Directions.South)
            {
                this.pet.flip = false;
                if (ATimer > 200)
                {
                    if (!AnimatingPet)
                    {
                        this.animationChance = Game1.random.Next(11);
                    }
                    switch (this.animationChance) //Do animation based on random chance.
                    {
                        case 1://Cat cleaning itself
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 15;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = 20;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = 21;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("Cowboy_Footstep"); }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = 22;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = 23;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = 21;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("Cowboy_Footstep"); }
                                }
                                if (frameIndex == 5)
                                {
                                    pet.Sprite.CurrentFrame = 22;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; currentTick = 0; }
                                }
                                if (frameIndex == 6)
                                {
                                    pet.Sprite.CurrentFrame = 23;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 7)
                                {
                                    pet.Sprite.CurrentFrame = 20;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }

                                }
                                if (frameIndex == 8)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }

                        default: 
                            this.ATimer = 0;
                            pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 19 : 18;
                            break;
                    }
                }
                else if (currentState != PetState.Sit || !FinishedAnimatingPetSittingDown)
                {
                    FinishedAnimatingPetSittingDown = false;
                    this.AnimatingPet = true;
                    int currentFrameDuration = 10;
                    if (frameIndex == 0)
                    {
                        pet.Sprite.CurrentFrame = 16;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 1)
                    {
                        pet.Sprite.CurrentFrame = 17;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 2)
                    {
                        pet.Sprite.CurrentFrame = 18;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 3)
                    {
                        this.AnimatingPet = false;
                        currentTick = 0;
                        frameIndex = 0;
                        FinishedAnimatingPetSittingDown = true;
                    }

                }
                else { pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 19 : 18; } //Default frame sit still
            }
            if (direction == SmartPet.Directions.East || direction == SmartPet.Directions.West)
            {
                this.pet.flip = direction == SmartPet.Directions.West;
                if (ATimer > 200)
                {
                    if (!AnimatingPet)
                    {
                        this.animationChance = Game1.random.Next(10);
                    }
                    switch (this.animationChance) //Do animation based on random chance.
                    {
                        case 5://Cat winging tail fast
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 20;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 51 : 34;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 52 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 53 : 41;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 54 : 42;
                                    currentTick++;
                                    if (currentTick > 200) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 55 : 43;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 5)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 56 : 44;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 6)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 57 : 45;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 7)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 58 : 46;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 8)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 50 : 38;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 9)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }
                        case 4://Cat winging tail slow
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 20;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 51 : 34;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 52 : 40;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 53 : 41;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 54 : 42;
                                    currentTick++;
                                    if (currentTick > 1000) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 55 : 43;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 5)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 56 : 44;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 6)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 57 : 45;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 7)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 58 : 46;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 8)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 50 : 38;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if(frameIndex == 9)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }
                        case 2 or 3 or 6 or 7://Cat winging tail slow
                            {
                                this.AnimatingPet = true;
                                int currentFrameDuration = 20;
                                if (frameIndex == 0)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 51 : 34;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 1)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 48 : 36;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 2)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 49 : 37;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 3)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 50 : 38;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 4)
                                {
                                    pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 51 : 34;
                                    currentTick++;
                                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                                }
                                if (frameIndex == 5)
                                {
                                    this.AnimatingPet = false;
                                    currentTick = 0;
                                    frameIndex = 0;
                                }
                                break;
                            }

                        default: 
                            this.ATimer = 0;
                            pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 51 : 34;
                            break;
                    }
                }

                else if (currentState != PetState.Sit || !FinishedAnimatingPetSittingDown)
                {
                    FinishedAnimatingPetSittingDown = false;
                    this.AnimatingPet = true;
                    int currentFrameDuration = 10;
                    if (frameIndex == 0)
                    {
                        pet.Sprite.CurrentFrame = 32;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 1)
                    {
                        pet.Sprite.CurrentFrame = 33;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 2)
                    {
                        pet.Sprite.CurrentFrame = 34;
                        currentTick++;
                        if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                    }
                    if (frameIndex == 3)
                    {
                        this.AnimatingPet = false;
                        currentTick = 0;
                        frameIndex = 0;
                        FinishedAnimatingPetSittingDown = true;
                    }

                }
                else { pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 51 : 34; } //Default frame sit still
            }

        }

        public void AnimateCatClassicWaiting(SmartPet.PetState currentState)
        {
            if (pet == null) { return; }
            this.ATimer++;
            pet.Sprite.CurrentAnimation = null;
            this.blinkTimer++;
            if (this.blinkTimer > 400)
            {
                this.blinkTimer = 0;
                this.ClosingOpeningEyes = false;
            }
            if (!ClosingOpeningEyes)
            {
                this.CloseOpenEyesChance = Game1.random.Next(3);
                this.ClosingOpeningEyes = true;
            }
            this.pet.flip = false;
            if (ATimer > 200)
            {
                if (!AnimatingPet)
                {
                    this.animationChance = Game1.random.Next(11);
                }
                switch (this.animationChance) //Do animation based on random chance.
                {
                    case 1://Cat cleaning itself
                        {
                            this.AnimatingPet = true;
                            int currentFrameDuration = 15;
                            if (frameIndex == 0)
                            {
                                pet.Sprite.CurrentFrame = 20;
                                currentTick++;
                                if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                            }
                            if (frameIndex == 1)
                            {
                                pet.Sprite.CurrentFrame = 21;
                                currentTick++;
                                if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("Cowboy_Footstep"); }
                            }
                            if (frameIndex == 2)
                            {
                                pet.Sprite.CurrentFrame = 22;
                                currentTick++;
                                if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                            }
                            if (frameIndex == 3)
                            {
                                pet.Sprite.CurrentFrame = 23;
                                currentTick++;
                                if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                            }
                            if (frameIndex == 4)
                            {
                                pet.Sprite.CurrentFrame = 21;
                                currentTick++;
                                if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; PlayAllInPetLocation("Cowboy_Footstep"); }
                            }
                            if (frameIndex == 5)
                            {
                                pet.Sprite.CurrentFrame = 22;
                                currentTick++;
                                if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; currentTick = 0; }
                            }
                            if (frameIndex == 6)
                            {
                                pet.Sprite.CurrentFrame = 23;
                                currentTick++;
                                if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                            }
                            if (frameIndex == 7)
                            {
                                pet.Sprite.CurrentFrame = 20;
                                currentTick++;
                                if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }

                            }
                            if (frameIndex == 8)
                            {
                                this.AnimatingPet = false;
                                currentTick = 0;
                                frameIndex = 0;
                            }
                            break;
                        }

                    default:
                        this.ATimer = 0;
                        pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 19 : 18;
                        break;
                }
            }
            else if (currentState != PetState.Sit || !FinishedAnimatingPetSittingDown)
            {
                FinishedAnimatingPetSittingDown = false;
                this.AnimatingPet = true;
                int currentFrameDuration = 10;
                if (frameIndex == 0)
                {
                    pet.Sprite.CurrentFrame = 16;
                    currentTick++;
                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                }
                if (frameIndex == 1)
                {
                    pet.Sprite.CurrentFrame = 17;
                    currentTick++;
                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                }
                if (frameIndex == 2)
                {
                    pet.Sprite.CurrentFrame = 18;
                    currentTick++;
                    if (currentTick > currentFrameDuration) { frameIndex++; currentTick = 0; }
                }
                if (frameIndex == 3)
                {
                    this.AnimatingPet = false;
                    currentTick = 0;
                    frameIndex = 0;
                    FinishedAnimatingPetSittingDown = true;
                }

            }
            else { pet.Sprite.CurrentFrame = this.CloseOpenEyesChance == 2 ? 19 : 18; } //Default frame sit still

        }
        public void AnimatePetFloating(PetInfo.Pet_Types petType,int direction, bool swimEffects)
        {
            if (direction < 0 || this.pet == null) { return; }

            this.pet.Sprite.CurrentAnimation = null;
            if (direction == 0)
            {
                this.blinkTimer = 0;
                this.EyesClosed = false;
            }
            else
            {
                this.blinkTimer++;
                if (blinkTimer >= 200)
                {
                    if (Game1.random.Next(3) == 2)
                    {
                        if (EyesClosed)
                        {
                            EyesClosed = false;
                        }
                        else
                        {
                            EyesClosed = true;
                        }
                    }
                    this.blinkTimer = 0;

                }
            }
            this.floatTimer += 1 + this.pet.BoardBounceModifier;
            if (this.floatTimer >= 50)
            {
                if (this.pet.BoardSinking)
                {
                    if (swimEffects)
                    {
                        Game1.Multiplayer.broadcastSprites(this.pet.CurrentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f - (Math.Abs(this.pet.xVelocity) + Math.Abs(this.pet.yVelocity)) * 3f, 7, 0, new Vector2(this.pet.Position.X - 4, this.pet.Position.Y - 12), flicker: false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
                    }
                    this.pet.BoardSinking = false;
                }
                else
                {
                    this.pet.BoardSinking = true;
                }
                this.floatTimer = 0;
            }
            switch (direction)
            {
                case 0:
                    this.pet.flip = false;
                    if (petType == PetInfo.Pet_Types.EnhancedCat)
                    {
                        this.pet.Sprite.CurrentFrame = 59;
                    }
                    else
                    {
                        this.pet.Sprite.CurrentFrame = 8;
                    }
                    break;
                case 2:
                    this.pet.flip = false;
                    if (petType == PetInfo.Pet_Types.EnhancedCat)
                    {
                        this.pet.Sprite.CurrentFrame = !this.EyesClosed? 60:61;
                    }
                    else if (petType == PetInfo.Pet_Types.EnhancedDog)
                    {
                        this.pet.Sprite.CurrentFrame = !this.EyesClosed? 0:42;
                    }
                    else
                    {
                        this.pet.Sprite.CurrentFrame = 0;
                    }
                        break;
                case 1:
                    this.pet.flip = false;
                    if (petType == PetInfo.Pet_Types.EnhancedCat)
                    {
                        this.pet.Sprite.CurrentFrame = !this.EyesClosed ? 62 : 63;
                    }
                    else if (petType == PetInfo.Pet_Types.EnhancedDog)
                    {
                        this.pet.Sprite.CurrentFrame = !this.EyesClosed ? 4 : 43;
                    }
                    else
                    {
                        this.pet.Sprite.CurrentFrame = 4;
                    }
                    break;
                case 3:
                    if (petType == PetInfo.Pet_Types.EnhancedCat)
                    {
                        this.pet.flip = true;
                        this.pet.Sprite.CurrentFrame = !this.EyesClosed ? 62 : 63;
                    }
                    else if (petType == PetInfo.Pet_Types.EnhancedDog)
                    {
                        this.pet.flip = true;
                        this.pet.Sprite.CurrentFrame = !this.EyesClosed ? 4 : 43;
                    }
                    else
                    {
                        this.pet.flip = true;
                        this.pet.Sprite.CurrentFrame = 4;
                    }
                    break;
            }
        }
    }
}
