using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using Microsoft.Xna.Framework;
using Pets_Enhanced_Mod.Utilities.Custom_Classes;
using StardewValley.Objects;
using StardewValley.Extensions;
using StardewValley.GameData.Pets;
using static Pets_Enhanced_Mod.Utilities.CachePetData;
using StardewValley.Locations;
using StardewModdingAPI;
using Microsoft.Xna.Framework.Graphics;
using Pets_Enhanced_Mod.Data;

namespace Pets_Enhanced_Mod.Utilities
{
    public class CachePetData
    {
        public static PetTeams CachePetTeams = new();

        public static Dictionary<Guid, PetInfoKit> PetCache = new();

        public static Dictionary<long, Vector2> SendPetToPlayerCache = new();

        public static Dictionary<string,Texture2D> TextureCache = new();

        private readonly static Dictionary<string, Dictionary<int,ModEntry.HatOffset_Simple>> HatOffsetCache = new();

        private readonly static Dictionary<string, Dictionary<string, (int FriendshipGain, int EnergyGain)>> DietListCache = new();

        public readonly static Dictionary<string, PE_Pet_Data> PetDataCached = new();
        public static void Clear()
        {
            CachePetTeams.Clear();
            PetCache.Clear();
            SendPetToPlayerCache.Clear();
            TextureCache.Clear();
            HatOffsetCache.Clear();
            DietListCache.Clear();
            PetDataCached.Clear();
        }
        public class PetInfoKit
        {
            private readonly PetInfo info;
            private readonly SmartPet pet;
            public PetInfoKit(PetInfo info, SmartPet pet)
            {
                this.info = info;
                this.pet = pet;
            }
            public PetInfo Info => info;
            public SmartPet Pet => pet;
        }

        public static Dictionary<int, ModEntry.HatOffset_Simple> GetHatOffsetFromID(string petTextureAssetPath,string hatoffsetID, string OpetType, string OpetBreed)
        {
            if (string.IsNullOrEmpty(petTextureAssetPath) || string.IsNullOrEmpty(hatoffsetID)) { return null; }

            if (HatOffsetCache.TryGetValue(petTextureAssetPath, out var result))
            {
                return result;
            }
            if (PetHelper.CheckIfSymaHatsOnPetsPlusHatConfigExistAnValid(OpetType, OpetBreed, out var _resultNewPetHatOffsetDic))
            {
                result = _resultNewPetHatOffsetDic;
            }
            else if (PetContent.CheckIfPetHatOffsetExistAndValid(hatoffsetID, out var hatOffsetResult1))
            {
                result = hatOffsetResult1;
            }
            else { result = PetContent.getDefaultHatOffsetForClassicPet(OpetType); }

            HatOffsetCache.TryAdd(petTextureAssetPath, result);

            return result;

        }
        public static Dictionary<string, (int FriendshipGain, int EnergyGain)> GetDietListFromID(string _dietListID)
        {
            if (string.IsNullOrEmpty(_dietListID)) { return new(); }

            if (!DietListCache.ContainsKey(_dietListID))
            {
                if (PetContent.CheckIfPetDietListExistAnValid(_dietListID, out var result))
                {
                    DietListCache.TryAdd(_dietListID, result);
                }
                else
                {
                    DietListCache.TryAdd(_dietListID, new());
                }
            }
            return DietListCache[_dietListID];
        }
        public static PE_Pet_Data GetPetDataForPet(StardewValley.Characters.Pet _pet)
        {
            if (_pet is null) { return null; }

            var path = _pet.getPetTextureName();
            if (PetDataCached.TryGetValue(path, out var result))
            {
                return result;
            }
            var petType = PetHelper.GetPetTypeFromTextureHeightAndOpetType(_pet.petType?.Value, _pet.Sprite.Texture.Height);
            if (PetHelper.CheckIfPetConfigExistAnValid(petType, path, out var peData1))
            {
                result = peData1;
            }
            else
            {
                result = PetContent.getDefaultDataForClassicPet(_pet.petType?.Value);
            }
            PetDataCached.TryAdd(path, result);

            return result;
        }
    }
}
