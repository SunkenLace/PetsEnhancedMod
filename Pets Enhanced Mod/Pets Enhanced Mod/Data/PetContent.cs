using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pets_Enhanced_Mod.Utilities.Custom_Classes;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Tools;
using StardewValley.GameData.Shops;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Pets_Enhanced_Mod.Utilities;
using StardewValley;
using System.Text.RegularExpressions;

namespace Pets_Enhanced_Mod.Data
{
    public class PetContent
    {
        public static string DefaultCatTrickLearningTreat = "(O)SunkenLace.PetsEnhancedMod.CatTreats";
        public static string DefaultDogTrickLearningTreat = "(O)SunkenLace.PetsEnhancedMod.DogTreats";
        public static string DefaultGenericTrickLearningTreat = "(O)SunkenLace.PetsEnhancedMod.CrunchyTreats";
        public static string[] DefaultDogNewEdibleItemsList = new string[130]
        {
            "[(O)SunkenLace.PetsEnhancedMod.DogTreats] : 20p 25e", // *Self explanatory -
            "[(O)SunkenLace.PetsEnhancedMod.CrunchyTreats] : 15p 20e", //crunchy treats
      "[(O)580] : 30p 25e", //Prehistoric Tibia -
      "[(O)176] : 0p 0e", //Egg -
      "[(O)180] : 0p 0e", //Egg -
      "[(O)174] : 0p 0e", //Large Egg -
      "[(O)182] : 0p 0e", //Large Egg -
      "[(O)24] : 10p 25e", //Parsnip -
      "[(O)192] : 0p 15e", //Potato -
      "[(O)250] : 0p 15e", //Kale -
      "[(O)188] : 12p 15e", //Green Bean -
      "[(O)190] : 0p 25e", //Cauliflower -
      "[(O)256] : 10p 25e", //Tomato -
      "[(O)258] : 15p 20e", //Blueberry -
      "[(O)280] : 10p 25e", //Yam -
      "[(O)278] : 0p 20e", //Bok Choy -
      "[(O)304] : 0p 25e", //Hops -
      "[(O)424] : 10p 30e", //Cheese -
      "[(O)426] : 14p 34e", //Goat Cheese -
      "[(O)16] : -10p -30e", //Wild Horseradish -
      "[(O)20] : -10p -30e", //Leek -
      "[(O)399] : 4p -30e", //Spring Onion -
      "[(O)296] : 12p 20e", //Salmonberry -
      "[(O)410] : 12p 20e", //Blackberry -
      "[(O)396] : -10p 20e", //Spice Berry -
      "[(O)402] : 10p 0e", //Sweet Pea -
      "[(O)408] : 15p 20e", //Hazelnut -
      "[(O)406] : 16p 25e", //Wild plum -
      "[(O)404] : -10p -20e", //Common mushroom -
      "[(O)403] : 15p 30e", //Field Snack -
      "[(O)194] : 10p 20e", //Fried Egg -
      "[(O)195] : 14p 25e", //Omelet -
      "[(O)216] : 12p 15e", //Bread -
      "[(O)196] : 0p 30e", //Salad -
      "[(O)206] : 16p 30e", //Pizza -
      "[(O)224] : 10p 30e", //Spaghetti -
      "[(O)229] : 12p 20e", //Tortilla, -
      "[(O)228] : 15p 20e", //Maki Roll
      "[(O)223] : 15p 15e", //Cookie -
      "[(O)210] : 16p 20e", //Hashbrowns -
      "[(O)211] : 16p 25e", //Pancakes -
      "[(O)139] : 10p 25e", //Salmon -
      "[(O)130] : 10p 30e", //Tuna -
      "[(O)260] : -15p -30e", //Hot pepper -
      "[(O)398] : 12p -20e", //Grape -
      "[(O)254] : 16p 35e", //Melon -
      "[(O)282] : 12p 20e", //Cranberries -
      "[(O)634] : 16p 25e", //Apricot -
      "[(O)638] : 15p 30e", //Cherry -
      "[(O)635] : 15p 30e", //Orange -
      "[(O)636] : 15p 25e", //Peach -
      "[(O)613] : 12p 30e", //Apple -
      "[(O)637] : 15p 30e", //Pomegranate -
      "[(O)412] : 10p 25e", //Winter root -
      "[(O)416] : 10p 25e", //Snow Yam -
      "[(O)414] : 15p 25e", //Crystal Fruit -
      "[(O)431] : 7p 10e", //Sunflower Seeds -
      "[(O)Carrot] : 16p 25e", // *Self explanatory -
      "[(O)851] : 18p -40e", //Magma Cap -
      "[(O)422] : 18p -40e", //Purple mushroom -
      "[(O)281] : -10p -20e", //Chanterelle mushroom -
      "[(O)257] : -10p -20e", //Morel mushroom -
      "[(O)78] : 10p 25e", //Cave carrot -
      "[(O)248] : -14p -40e", //Garlic -
      "[(O)284] : 10p 15e", //Beet -
      "[(O)252] : -14p -30e", //Rhubarb -
      "[(O)268] : 14p 25e", //Starfruit -
      "[(O)266] : 10p 25e", //Red cabbage -
      "[(O)433] : -20p -30e", //Coffee bean -
      "[(O)830] : 0p 20e", //Taro root -
      "[(O)829] : -15p 25e", //Ginger -
      "[(O)91] : 8p 30e", //Banana -
      "[(O)834] : 18p 30e", //Mango -
      "[(O)832] : 18p 35e", //Pineapple -
      "[(O)400] : 17p 25e", //Strawberry, -
      "[(O)Powdermelon] : 16p 35e", // *Self explanatory -
      "[(O)456] : 0p 20e", //Algae Soup -
      "[(O)457] : 0p 25e", //Pale Broth -
      "[(O)MossSoup] : 0p 15e", //Moss Soup -
      "[(O)198] : 10p -30e", //Baked Fish -
      "[(O)199] : 20p 30e", //Parnsnip soup -
      "[(O)200] : 10p 35e", //Vegetable Medley -
      "[(O)202] : 15p 25e", //Fried calamary -
      "[(O)204] : 15p 25e", //Lucky Lunch -
      "[(O)205] : 10p -40e", //Fried mushroom -
      "[(O)207] : 10p 35e", //Bean hotpot -
      "[(O)208] : 14p 30e", //Glazed Yams - 
      "[(O)209] : 0p -30e", //Carp Surprise -
      "[(O)212] : 20p 35e", //Salmon Dinner -
      "[(O)213] : 20p 30e", //Fish Taco -
      "[(O)214] : 10p 25e", //Crispy Bass -
      "[(O)215] : -20p -40e", //Pepper Poppers -
      "[(O)218] : 10p -30e", //Tom Kha Soup -
      "[(O)222] : 15p -30e", //Rhubarb pie -
      "[(O)225] : 15p 25e", //Fried Eel -
      "[(O)226] : -20p -40e", //Spicy Eel -
      "[(O)230] : 14p 35e", //Red Plate -
      "[(O)231] : 10p 30e", //Eggplant Parmesan -
      "[(O)232] : 15p 35e", //Rice Pudding -
      "[(O)233] : 18p 25e", //Ice cream -
      "[(O)234] : 20p 35e", //Blueberry Tart -
      "[(O)235] : 12p 40e", //Autumm Bounty -
      "[(O)237] : 18p 35e", //Super Meal -
      "[(O)239] : 20p 30e", //Stuffing -
      "[(O)240] : 24p 40e", //Farmers lunch -
      "[(O)241] : 16p 30e", //Survival Burgah -
      "[(O)244] : 16p 30e", //Roots platter -
      "[(O)197] : 20p 35e", //Cheese Cauliflower -
      "[(O)604] : 20p 35e", //Plum Pudding -
      "[(O)605] : 10p 25e", //Artichoke Dip -
      "[(O)606] : 18p 40e", //Stir Fry -
      "[(O)607] : 15p 30e", //Roasted Hazelnuts -
      "[(O)608] : 16p 40e", //Pumpkin pie -
      "[(O)609] : 16p 35e", //Radish Salad -
      "[(O)610] : 20p 35e", //Fruit salad -
      "[(O)611] : 20p 35e", //Blackberry Cobbler -
      "[(O)618] : 18p 30e", //Bruschetta -
      "[(O)648] : -10p 35e", //Coleslaw -
      "[(O)649] : -15p -40e", //Fiddlehead Risotto -
      "[(O)651] : 10p 30e", //Poppyseed Muffin -
      "[(O)727] : 15p 30e", //Chowder -
      "[(O)729] : 10p -40e", //Escargot -
      "[(O)730] : 15p 35e", //Lobster Bisque -
      "[(O)731] : 10p 25e", //Maple Bar -
      "[(O)732] : 15p 40e", //Crab Cakes -
      "[(O)236] : 16p 40e", //Pumpkin soup -
      "[(O)201] : 24p 40e", //Complete Breakfast -
      "[(O)905] : 25p 40e", //Mango sticky rice -
      "[(O)906] : 10p 25e", //Poi -
      "[(O)907] : -20p 28e", //Tropical Curry -
        };
        public static string[] DefaultTurtleNewEdibleItemsList = new string[45]
        {
            "[(O)SunkenLace.PetsEnhancedMod.CrunchyTreats] : 20p 25e", //crunchy treats
            "[(O)153] : 10p 20e", // Green algae -
            "[(O)157] : 10p 20e", // White algae -
            "[(O)152] : 12p 20e", // Seaweed -
            "[(O)24] : 17p 25e", //Parsnip -
            "[(O)250] : 15p 15e", //Kale -
            "[(O)188] : 12p 15e", //Green Bean -
            "[(O)190] : 15p 25e", //Cauliflower -
            "[(O)256] : 10p 25e", //Tomato -
            "[(O)258] : 15p 20e", //Blueberry -
            "[(O)278] : 15p 20e", //Bok Choy -
            "[(O)304] : 15p 25e", //Hops -
            "[(O)16] : -10p -30e", //Wild Horseradish -
            "[(O)20] : -10p -30e", //Leek -
            "[(O)296] : 12p 20e", //Salmonberry -
            "[(O)410] : 12p 20e", //Blackberry -
            "[(O)396] : 10p 20e", //Spice Berry -
            "[(O)402] : 10p 15e", //Sweet Pea -
            "[(O)406] : 16p 25e", //Wild plum -
            "[(O)404] : -10p -20e", //Common mushroom -
            "[(O)196] : 20p 30e", //Salad -
            "[(O)398] : 12p 20e", //Grape -
            "[(O)254] : 16p 35e", //Melon -
            "[(O)282] : 12p 20e", //Cranberries -
            "[(O)634] : 16p 25e", //Apricot -
            "[(O)638] : 15p 30e", //Cherry -
            "[(O)635] : 15p 30e", //Orange -
            "[(O)636] : 15p 25e", //Peach -
            "[(O)613] : 12p 30e", //Apple -
            "[(O)637] : 15p 30e", //Pomegranate -
            "[(O)414] : 15p 25e", //Crystal Fruit -
            "[(O)Carrot] : 16p 25e", // *Self explanatory -
            "[(O)851] : 18p -40e", //Magma Cap -
            "[(O)422] : 18p -40e", //Purple mushroom -
            "[(O)281] : -10p -20e", //Chanterelle mushroom -
            "[(O)257] : -10p -20e", //Morel mushroom -
            "[(O)78] : 10p 25e", //Cave carrot -
            "[(O)268] : 14p 25e", //Starfruit -
            "[(O)266] : 10p 25e", //Red cabbage -
            "[(O)91] : 15p 30e", //Banana -
            "[(O)834] : 18p 30e", //Mango -
            "[(O)832] : 18p 35e", //Pineapple -
            "[(O)400] : 17p 25e", //Strawberry, -
            "[(O)Powdermelon] : 16p 35e", // *Self explanatory -
            "[(O)MossSoup] : 10p 15e", //Moss Soup -
        };
        public static string[] DefaultCatNewEdibleItemsList = new string[130] {
            "[(O)SunkenLace.PetsEnhancedMod.CatTreats] : 20p 25e", //cat treats
            "[(O)SunkenLace.PetsEnhancedMod.CrunchyTreats] : 15p 20e", //crunchy treats
            "[(O)176] : 0p 0e", //Egg -
            "[(O)180] : 0p 0e", //Egg -
            "[(O)174] : 0p 0e", //Large Egg -
            "[(O)182] : 0p 0e", //Large Egg -
            "[(O)184] : 20p 20e", //Milk
            "[(O)186] : 20p 25e", //Large Milk
            "[(O)192] : 0p -30e", //Potato -
            "[(O)188] : 12p 12e", //Green Bean
            "[(O)142] : 5p 10e", //Carp
            "[(O)132] : 12p 20e", //Bream
            "[(O)702] : 10p 20e", //Chub
            "[(O)129] : 6p 10e", //Anchovy
            "[(O)147] : 12p 20e", //Herring
            "[(O)137] : 12p 20e", //Smallmouth Bass
            "[(O)145] : 12p 20e", //Sunfish
            "[(O)716] : 15p 25e", //Crayfish
            "[(O)720] : 12p 14e", //Shrimp
            "[(O)717] : 15p 25e", //Crab
            "[(O)296] : 12p 20e", //Salmonberry -
            "[(O)410] : 12p 20e", //Blackberry -
            "[(O)396] : -10p -20e", //Spice Berry -
            "[(O)402] : 18p 0e", //Sweet Pea -
            "[(O)404] : -10p -20e", //Common mushroom -
            "[(O)684] : 6p 10e", //Bug Meat -  
            "[(O)424] : 20p 30e", //Cheese -
            "[(O)426] : 20p 34e", //Goat Cheese -
            "[(O)190] : -10p -20e", //Cauliflower -
            "[(O)258] : 15p 20e", //Blueberry -
            "[(O)256] : 10p 20e", //Tomato -
            "[(O)254] : 20p 35e", //Melon -
            "[(O)260] : -15p -30e", //Hot pepper -
            "[(O)398] : 12p -20e", //Grape -
            "[(O)282] : 12p 20e", //Cranberries -
            "[(O)634] : 16p 25e", //Apricot -
            "[(O)638] : 15p 30e", //Cherry -
            "[(O)635] : -20p -30e", //Orange -
            "[(O)613] : 12p 30e", //Apple -
            "[(O)637] : 15p 30e", //Pomegranate -
            "[(O)834] : 18p 30e", //Mango -
            "[(O)91] : 8p 30e", //Banana -
            "[(O)400] : 17p 25e", //Strawberry, -
            "[(O)408] : 15p 0e", //Hazelnut -
            "[(O)406] : 16p 25e", //Wild plum -
            "[(O)131] : 12p 20e", //Sardine
            "[(O)130] : 20p 30e", //Tuna
            "[(O)139] : 20p 25e", //Salmon
            "[(O)140] : 20p 28e", //Walleye
            "[(O)144] : 12p 30e",//Pike
            "[(O)136] : 15p 30e", //Largemouth Bass
            "[(O)700] : 12p 30e", //Bullhead
            "[(O)701] : 12p 25e", //Tilapia
            "[(O)705] : 20p 30e", //Albacore
            "[(O)706] : 15p 25e", //Shad
            "[(O)708] : 12p 28e", //Halibut
            "[(O)138] : 12p 25e", //Rainbow Trout
            "[(O)141] : 12p 30e", //Perch
            "[(O)146] : 12p 15e", //Red Mullet
            "[(O)150] : 20p 20e", //Red Snapper
            "[(O)267] : 12p 25e", //Flounder
            "[(O)154] : -30p -40e", // Sea Cucumber-
            "[(O)431] : 7p 10e", //Sunflower Seeds -
            "[(O)436] : 20p 24e", //Goat Milk
            "[(O)438] : 20p 28e", //L. Goat Milk
            "[(O)194] : 10p 20e", //Fried Egg -
            "[(O)195] : 14p 25e", //Omelet -
            "[(O)216] : 12p 15e", //Bread -
            "[(O)227] : 15p 20e", //Sashimi
            "[(O)196] : -10p 20e", //Salad -
            "[(O)206] : 16p 25e", //Pizza -
            "[(O)224] : 10p 25e", //Spaghetti -
            "[(O)229] : 12p 20e", //Tortilla, -
            "[(O)228] : 15p 25e", //Maki Roll
            "[(O)210] : 16p 20e", //Hashbrowns -
            "[(O)211] : 16p 25e", //Pancakes -
            "[(O)197] : 10p 20e", //Cheese Cauliflower -
            "[(O)200] : 10p 20e", //Vegetable Medley -
            "[(O)219] : 15p 28e", //Trout Soup
            "[(O)456] : 0p 15e", //Algae Soup -
            "[(O)457] : 0p 15e", //Pale Broth -
            "[(O)MossSoup] : -10p -10e", //Moss Soup -
            "[(O)248] : -14p -40e", //Garlic -
            "[(O)266] : -10p -20e", //Red cabbage -
            "[(O)268] : 14p 25e", //Starfruit -
            "[(O)433] : -20p -30e", //Coffee bean -
            "[(O)257] : -10p -20e", //Morel mushroom -  
            "[(O)281] : -10p -20e", //Chanterelle mushroom -
            "[(O)422] : -15p -40e", //Purple mushroom -
            "[(O)414] : 15p 25e", //Crystal Fruit -
            "[(O)156] : 5p 25e", //Ghostfish
            "[(O)Goby] : 12p 25e", //Goby
            "[(O)164] : 15p 25e", //Sandfish
            "[(O)715] : 15p 25e", //Lobster
            "[(O)269] : 10p 15e", //Midnight Carp
            "[(O)699] : 12p 25e", //Tiger Trout
            "[(O)707] : 10p 25e", //Lingcod
            "[(O)734] : 20p 25e", //Woodskip
            "[(O)704] : 12p 30e", //Dorado
            "[(O)698] : 20p 35e", //Sturgeon
            "[(O)155] : -30p -40e", // Super Cucumber-
            "[(O)214] : 20p 30e", //Crispy Bass -
            "[(O)213] : 20p 30e", //Fish Taco -
            "[(O)218] : 10p -30e", //Tom Kha Soup -
            "[(O)225] : 20p 25e", //Fried Eel -
            "[(O)226] : -20p -40e", //Spicy Eel -
            "[(O)212] : 20p 35e", //Salmon Dinner -
            "[(O)198] : 20p 35e", //Baked Fish -
            "[(O)231] : 10p 30e", //Eggplant Parmesan -
            "[(O)232] : 15p 35e", //Rice Pudding -
            "[(O)237] : 18p 35e", //Super Meal -
            "[(O)215] : -20p -40e", //Pepper Poppers -
            "[(O)205] : 10p -40e", //Fried mushroom -
            "[(O)202] : 15p 25e", //Fried calamary -
            "[(O)241] : 20p 30e", //Survival Burgah -
            "[(O)240] : 24p 40e", //Farmers lunch -
            "[(O)230] : -10p -20e", //Red Plate -
            "[(O)236] : 16p 40e", //Pumpkin Soup
            "[(O)610] : 10p 30e", //Fruit salad -
            "[(O)618] : 18p 25e", //Bruschetta -
            "[(O)727] : 15p 30e", //Chowder -
            "[(O)728] : 16p 40e", // Fish Stew -
            "[(O)729] : 15p 30e", //Escargot -
            "[(O)730] : 15p 35e", //Lobster Bisque -
            "[(O)732] : 20p 40e", //Crab Cakes -
            "[(O)161] : 20p 30e", //Ice Pip
            "[(O)796] : -20p -30e", //Slimejack
            "[(O)905] : 20p 30e", //Mango sticky rice -
            "[(O)445] : 15p 25e", //Caviar
            "[(O)921] : 15p 35e", //Squid Ink Ravioli -
        };
        public static string[] DefaultGenericPetNewEdibleItemsList = new string[8]
        {
            "[(O)SunkenLace.PetsEnhancedMod.CrunchyTreats] : 20p 25e", //crunchy treats
            "[(O)613] : 15p 25e", //Apple
            "[(O)400] : 17p 25e", //Strawberry
            "[(O)91] : 10p 30e", //Banana
            "[(O)258] : 15p 20e", //Blueberry
            "[(O)296] : 12p 20e", //Salmonberry -
            "[(O)410] : 12p 20e", //Blackberry -
            "[(O)254] : 16p 35e", //Melon -
        };

        public static Dictionary<string, float[]> defaultEnhancedTurtleHatOffset = new()
        {
            ["0,2"] = new float[] { 0f, 4f, 2f },
            ["1,3"] = new float[] { 0f, 5f, 2f },
            ["4,6"] = new float[] { 6.5f, 1f, 1f },
            ["5,7"] = new float[] { 6.5f, 2f, 1f },
            ["8,10"] = new float[] { 0f, -5f, 0f },
            ["9,11"] = new float[] { 0f, -4f, 0f },
            ["12,14"] = new float[] { -7.5f, 1f, 3f },
            ["13,15"] = new float[] { -7.5f, 2f, 3f },
            ["16,17"] = new float[] { 0f, 4f, 2f },
            ["18"] = new float[] { 0f, 3f, 2f },
            ["19"] = new float[] { 0f, 2f, 2f },
            ["20,21,22,23"] = new float[] { 0f, 1f, 2f },
            ["24,25,26,27"] = new float[] { 0f, 5f, 2f },
            ["28,29"] = new float[] { 0f, 3f, 2f },
            ["30,33"] = new float[] { 6.5f, 2f, 1f },
            ["31,32"] = new float[] { 6.5f, 1f, 1f },
        };
        public static Dictionary<string, float[]> defaultVanillaCatHatOffset = new()
        {
            ["0,2"] = new float[] { 0f, 0f, 2f },
            ["1,3"] = new float[] { 0f, 1f, 2f },
            ["4,6"] = new float[] { 5.5f, -2f, 1f },
            ["5,7"] = new float[] { 5.5f, -1f, 1f },
            ["8,10"] = new float[] { 0f, -6f, 0f },
            ["9,11"] = new float[] { 0f, -5f, 0f },
            ["12,14"] = new float[] { -5.5f, -2f, 3f },
            ["13,15"] = new float[] { -5.5f, -1f, 3f },
            ["16"] = new float[] { 0f, -2f, 2f },
            ["17"] = new float[] { 0f, -4f, 2f },
            ["21,23"] = new float[] { 0f, -3f, 2f },
            ["18,19"] = new float[] { 0f, -5f, 2f },
            ["20,22"] = new float[] { 0f, -4f, 2f },
            ["24"] = new float[] { 5.5f, 0f, 1f },
            ["25"] = new float[] { 5.5f, 3f, 1f },
            ["26"] = new float[] { 5.5f, 4f, 1f },
            ["27"] = new float[] { 6.5f, 4f, 1f },
            ["28,29"] = new float[] { -0.5f, 4f, 3f },
            ["30, 31"] = new float[] { 5.5f, 0f, 1f },
            ["32"] = new float[] { 5.5f, -3f, 1f },
            ["33"] = new float[] { 4.5f, -4f, 1f },
            ["34,36,37,38,40,41,42,43,44,45,46,48,49,50,51,52,53,54,55,56,57,58"] = new float[] { 2.5f, -4f, 1f },
            ["47"] = new float[] { 4.5f, -4f, 1f },
            ["60,61"] = new float[] { 0f, -7f, 2f },
            ["62,63"] = new float[] { 4.5f, -8f, 1f },
            ["59"] = new float[] { 0f, -6f, 0f },
            ["35,39"] = new float[] { 0f, 0f, 2f }
        };
        public static Dictionary<string, float[]> defaultEnhancedCatHatOffset = new()
        {
            ["0,2"] = new float[] { 0f, 0f, 2f },
            ["1,3"] = new float[] { 0f, 1f, 2f },
            ["4,6"] = new float[] { 6.5f, -2f, 1f },
            ["5,7"] = new float[] { 6.5f, -1f, 1f },
            ["8,10"] = new float[] { 0f, -6f, 0f },
            ["9,11"] = new float[] { 0f, -5f, 0f },
            ["12,14"] = new float[] { -6.5f, -2f, 3f },
            ["13,15"] = new float[] { -6.5f, -1f, 3f },
            ["16"] = new float[] { 0f, -1f, 2f },
            ["17,21,23"] = new float[] { 0f, -3f, 2f },
            ["18,19,20,22"] = new float[] { 0f, -4f, 2f },
            ["24"] = new float[] { 6.5f, 0f, 1f },
            ["25"] = new float[] { 6.5f, 3f, 1f },
            ["26"] = new float[] { 6.5f, 4f, 1f },
            ["27"] = new float[] { 7.5f, 4f, 1f },
            ["28,29"] = new float[] { -0.5f, 4f, 3f },
            ["30, 31"] = new float[] { 5.5f, 0f, 1f },
            ["32"] = new float[] { 5.5f, -3f, 1f },
            ["33"] = new float[] { 4.5f, -4f, 1f },
            ["34,36,37,38,40,41,42,43,44,45,46,48,49,50,51,52,53,54,55,56,57,58"] = new float[] { 2.5f, -4f, 1f },
            ["47"] = new float[] { 4.5f, -4f, 1f },
            ["60,61"] = new float[] { 0f, -7f, 2f },
            ["62,63"] = new float[] { 4.5f, -8f, 1f },
            ["59"] = new float[] { 0f, -6f, 0f },
            ["35,39"] = new float[] { 0f, 0f, 2f }
        };

        public static Dictionary<string, float[]> defaultEnhancedDog0HatOffset = new()
        {
            ["0,2,42"] = new float[] { 0f, -4.0f, 2f },
            ["1,3"] = new float[] { 0f, -3.0f, 2f },
            ["4,6,43"] = new float[] { 5.5f, -6.0f, 1f },
            ["5,7"] = new float[] { 5.5f, -5f, 1f },
            ["8,10"] = new float[] { 0f, -12f, 0f },
            ["9,11"] = new float[] { 0f, -11f, 0f },
            ["12,14"] = new float[] { -6.5f, -6f, 3f },
            ["13,15"] = new float[] { -6.5f, -5f, 3f },
            ["16"] = new float[] { 0f, -6f, 2f },
            ["17"] = new float[] { 0f, -8f, 2f },
            ["18,19,27,36,37,38"] = new float[] { 0f, -9f, 2f },
            ["20"] = new float[] { 5.5f, -7f, 1f },
            ["21"] = new float[] { 4.5f, -8f, 1f },
            ["22"] = new float[] { 3.5f, -9f, 1f },
            ["23,24,25,30,31,40,41"] = new float[] { 3.5f, -10f, 1f },
            ["26"] = new float[] { 3.5f, -11f, 1f },
            ["28,29"] = new float[] { 5f, 0f, 2f },
            ["32"] = new float[] { 5.5f, -8f, 1f },
            ["33"] = new float[] { 5.5f, -7f, 1f },
            ["34"] = new float[] { 5.5f, -5f, 1f },
            ["35,39"] = new float[] { 0f, 0f, 2f }
        };
        public static Dictionary<string, float[]> defaultEnhancedDog1HatOffset = new()
        {
            ["0,2,42"] = new float[] { 0f, -4.0f, 2f },
            ["1,3"] = new float[] { 0f, -3.0f, 2f },
            ["4,6,43"] = new float[] { 5.5f, -6.0f, 1f },
            ["5,7"] = new float[] { 5.5f, -5f, 1f },
            ["8,10"] = new float[] { 0f, -12f, 0f },
            ["9,11"] = new float[] { 0f, -11f, 0f },
            ["12,14"] = new float[] { -6.5f, -6f, 3f },
            ["13,15"] = new float[] { -6.5f, -5f, 3f },
            ["16"] = new float[] { 0f, -6f, 2f },
            ["17"] = new float[] { 0f, -9f, 2f },
            ["18,19,27,36,37,38"] = new float[] { 0f, -10f, 2f },
            ["20"] = new float[] { 5.5f, -7, 1f },
            ["21"] = new float[] { 4.5f, -7, 1f },
            ["22"] = new float[] { 3.5f, -11, 1f },
            ["23,24,25,30,31,40,41"] = new float[] { 3.5f, -11f, 1f },
            ["26"] = new float[] { 3.5f, -12f, 1f },
            ["28,29"] = new float[] { 5f, -1f, 2f },
            ["32"] = new float[] { 6.5f, -9f, 1f },
            ["33"] = new float[] { 6.5f, -8f, 1f },
            ["34"] = new float[] { 6.5f, -6f, 1f },
            ["35,39"] = new float[] { 0f, 0f, 2f }
        };
        public static Dictionary<string, float[]> defaultEnhancedDog2HatOffset = new()
        {
            ["0,2,42"] = new float[] { 0f, -3f, 2f },
            ["1,3"] = new float[] { 0f, -2f, 2f },
            ["4,6,43"] = new float[] { 5.5f, -4f, 1f },
            ["5,7"] = new float[] { 5.5f, -3f, 1f },
            ["8,10"] = new float[] { 0f, -11f, 0f },
            ["9,11"] = new float[] { 0f, -10f, 0f },
            ["12,14"] = new float[] { -6.5f, -4f, 3f },
            ["13,15"] = new float[] { -6.5f, -3f, 3f },
            ["16"] = new float[] { 0f, -6f, 2f },
            ["17"] = new float[] { 0f, -8f, 2f },
            ["18,19,27,36,37,38"] = new float[] { 0f, -9f, 2f },
            ["20"] = new float[] { 5.5f, -5f, 1f },
            ["21"] = new float[] { 4.5f, -7f, 1f },
            ["22"] = new float[] { 3.5f, -8f, 1f },
            ["23,24,25,30,31,40,41"] = new float[] { 3.5f, -8f, 1f },
            ["26"] = new float[] { 3.5f, -9f, 1f },
            ["28,29"] = new float[] { 5f, 0f, 2f },
            ["32"] = new float[] { 5.5f, -7f, 1f },
            ["33"] = new float[] { 5.5f, -6f, 1f },
            ["34"] = new float[] { 5.5f, -4f, 1f },
            ["35,39"] = new float[] { 0f, 0f, 2f }
        };
        public static Dictionary<string, float[]> defaultEnhancedDog3HatOffset = new()
        {
            ["0,2,42"] = new float[] { 0f, -4.0f, 2f },
            ["1,3"] = new float[] { 0f, -3.0f, 2f },
            ["4,6,43"] = new float[] { 5.5f, -6.0f, 1f },
            ["5,7"] = new float[] { 5.5f, -5f, 1f },
            ["8,10"] = new float[] { 0f, -12f, 0f },
            ["9,11"] = new float[] { 0f, -11f, 0f },
            ["12,14"] = new float[] { -6.5f, -6f, 3f },
            ["13,15"] = new float[] { -6.5f, -5f, 3f },
            ["16"] = new float[] { 0f, -6f, 2f },
            ["17"] = new float[] { 0f, -8f, 2f },
            ["18,19,27,36,37,38"] = new float[] { 0f, -9f, 2f },
            ["20"] = new float[] { 5.5f, -7f, 1f },
            ["21"] = new float[] { 4.5f, -8f, 1f },
            ["22"] = new float[] { 3.5f, -9f, 1f },
            ["23,24,25,30,31,40,41"] = new float[] { 3.5f, -10f, 1f },
            ["26"] = new float[] { 3.5f, -11f, 1f },
            ["28,29"] = new float[] { 5f, 0f, 2f },
            ["32"] = new float[] { 5.5f, -8f, 1f },
            ["33"] = new float[] { 5.5f, -7f, 1f },
            ["34"] = new float[] { 5.5f, -5f, 1f },
            ["35,39"] = new float[] { 0f, 0f, 2f }
        };
        public static Dictionary<string, float[]> defaultEnhancedDog4HatOffset = new()
        {
            ["0,2,42"] = new float[] { 0f, -4.0f, 2f },
            ["1,3"] = new float[] { 0f, -3.0f, 2f },
            ["4,6,43"] = new float[] { 5.5f, -6.0f, 1f },
            ["5,7"] = new float[] { 5.5f, -5f, 1f },
            ["8,10"] = new float[] { 0f, -12f, 0f },
            ["9,11"] = new float[] { 0f, -11f, 0f },
            ["12,14"] = new float[] { -6.5f, -6f, 3f },
            ["13,15"] = new float[] { -6.5f, -5f, 3f },
            ["16"] = new float[] { 0f, -6f, 2f },
            ["17"] = new float[] { 0f, -8f, 2f },
            ["18,19,27,36,37,38"] = new float[] { 0f, -9f, 2f },
            ["20"] = new float[] { 5.5f, -7f, 1f },
            ["21"] = new float[] { 4.5f, -8f, 1f },
            ["22"] = new float[] { 3.5f, -9f, 1f },
            ["23,24,25,30,31,40,41"] = new float[] { 3.5f, -10f, 1f },
            ["26"] = new float[] { 3.5f, -11f, 1f },
            ["28,29"] = new float[] { 5f, 0f, 2f },
            ["32"] = new float[] { 5.5f, -8f, 1f },
            ["33"] = new float[] { 5.5f, -7f, 1f },
            ["34"] = new float[] { 5.5f, -5f, 1f },
            ["35,39"] = new float[] { 0f, 0f, 2f }
        };
        public static Dictionary<string, float[]> defaultEnhancedDog5HatOffset = new()
        {
            ["0,2,42"] = new float[] { 0f, -4.0f, 2f },
            ["1,3"] = new float[] { 0f, -3.0f, 2f },
            ["4,6,43"] = new float[] { 5.5f, -6.0f, 1f },
            ["5,7"] = new float[] { 5.5f, -5f, 1f },
            ["8,10"] = new float[] { 0f, -12f, 0f },
            ["9,11"] = new float[] { 0f, -11f, 0f },
            ["12,14"] = new float[] { -6.5f, -6f, 3f },
            ["13,15"] = new float[] { -6.5f, -5f, 3f },
            ["16"] = new float[] { 0f, -6f, 2f },
            ["17"] = new float[] { 0f, -8f, 2f },
            ["18,19,27,36,37,38"] = new float[] { 0f, -9f, 2f },
            ["20"] = new float[] { 5.5f, -7f, 1f },
            ["21"] = new float[] { 4.5f, -8f, 1f },
            ["22"] = new float[] { 3.5f, -9f, 1f },
            ["23,24,25,30,31,40,41"] = new float[] { 3.5f, -10f, 1f },
            ["26"] = new float[] { 3.5f, -11f, 1f },
            ["28,29"] = new float[] { 5f, 0f, 2f },
            ["32"] = new float[] { 5.5f, -8f, 1f },
            ["33"] = new float[] { 5.5f, -7f, 1f },
            ["34"] = new float[] { 5.5f, -5f, 1f },
            ["35,39"] = new float[] { 0f, 0f, 2f }
        };

        public static PetConfigData catPetData_config = new()
        {
            BaseCooldownTime = 1.6f,
            MinDamage = 5,
            MaxDamage = 10,
            CritChance = 0.25f,
            IsViciousType = true,
            AttackEffect = "Slash",
            HatOffsetID = "cat",
            HasWaitSkill = true,
            HasFollowSkill = true,
            HasHuntSkill = true,
            HasForageSkill = false,
            HasFishingSkill = true,
            DietListID = "cat",
            TrickLearningTreat = DefaultCatTrickLearningTreat,
        };
        public static PetConfigData dog0PetData_config = new()
        {
            BaseCooldownTime = 1.9f,
            MinDamage = 8,
            MaxDamage = 16,
            CritChance = 0.15f,
            IsViciousType = false,
            AttackEffect = "Bite",
            HatOffsetID = "dog",
            HasWaitSkill = true,
            HasFollowSkill = true,
            HasHuntSkill = true,
            HasForageSkill = true,
            HasFishingSkill = false,
            DietListID = "dog",
            TrickLearningTreat = DefaultDogTrickLearningTreat,
        };
        public static PetConfigData dog1PetData_config = new()
        {
            BaseCooldownTime = 1.9f,
            MinDamage = 8,
            MaxDamage = 16,
            CritChance = 0.15f,
            IsViciousType = false,
            AttackEffect = "Bite",
            HatOffsetID = "dog1",
            HasWaitSkill = true,
            HasFollowSkill = true,
            HasHuntSkill = true,
            HasForageSkill = true,
            HasFishingSkill = false,
            DietListID = "dog",
            TrickLearningTreat = DefaultDogTrickLearningTreat,
        };
        public static PetConfigData dog2PetData_config = new()
        {
            BaseCooldownTime = 1.9f,
            MinDamage = 8,
            MaxDamage = 16,
            CritChance = 0.15f,
            IsViciousType = false,
            AttackEffect = "Bite",
            HatOffsetID = "dog2",
            HasWaitSkill = true,
            HasFollowSkill = true,
            HasHuntSkill = true,
            HasForageSkill = true,
            HasFishingSkill = false,
            DietListID = "dog",
            TrickLearningTreat = DefaultDogTrickLearningTreat,
        };
        public static PetConfigData dog3PetData_config = new()
        {
            BaseCooldownTime = 1.9f,
            MinDamage = 8,
            MaxDamage = 16,
            CritChance = 0.15f,
            IsViciousType = false,
            AttackEffect = "Bite",
            HatOffsetID = "dog3",
            HasWaitSkill = true,
            HasFollowSkill = true,
            HasHuntSkill = true,
            HasForageSkill = true,
            HasFishingSkill = false,
            DietListID = "dog",
            TrickLearningTreat = DefaultDogTrickLearningTreat,
        };
        public static PetConfigData dog4PetData_config = new()
        {
            BaseCooldownTime = 1.9f,
            MinDamage = 8,
            MaxDamage = 16,
            CritChance = 0.15f,
            IsViciousType = false,
            AttackEffect = "Bite",
            HatOffsetID = "dog4",
            HasWaitSkill = true,
            HasFollowSkill = true,
            HasHuntSkill = true,
            HasForageSkill = true,
            HasFishingSkill = false,
            DietListID = "dog",
            TrickLearningTreat = DefaultDogTrickLearningTreat,
        };
        public static PetConfigData dog5PetData_config = new()
        {
            BaseCooldownTime = 1.9f,
            MinDamage = 8,
            MaxDamage = 16,
            CritChance = 0.15f,
            IsViciousType = false,
            AttackEffect = "Bite",
            HatOffsetID = "dog5",
            HasWaitSkill = true,
            HasFollowSkill = true,
            HasHuntSkill = true,
            HasForageSkill = true,
            HasFishingSkill = false,
            DietListID = "dog",
            TrickLearningTreat = DefaultDogTrickLearningTreat,
        };
        public static PetConfigData turtlePetData_config = new()
        {
            BaseCooldownTime = 3.2f,
            MinDamage = 12,
            MaxDamage = 22,
            CritChance = 0.10f,
            IsViciousType = true,
            AttackEffect = "Bite",
            HatOffsetID = "turtle",
            HasWaitSkill = true,
            HasFollowSkill = true,
            HasHuntSkill = true,
            HasForageSkill = false,
            HasFishingSkill = false,
            DietListID = "turtle",
            TrickLearningTreat = DefaultGenericTrickLearningTreat, //Crunchy treats
        };
        public static PetConfigData nonRegisteredPetData_config = new()
        {
            BaseCooldownTime = 1.9f,
            MinDamage = 8,
            MaxDamage = 16,
            CritChance = 0.15f,
            IsViciousType = false,
            AttackEffect = "Bite",
            HatOffsetID = "default",
            HasWaitSkill = true,
            HasFollowSkill = true,
            HasHuntSkill = true,
            HasForageSkill = false,
            HasFishingSkill = false,
            DietListID = "generic",
            TrickLearningTreat = DefaultGenericTrickLearningTreat, //Crunchy treats
        };
        public const int DefaultInvisibleFrameDog = 35;
        public const int DefaultInvisibleFrameCat = 55;
        public static void WriteConfigFile()
        {
            ModEntry.GetModHelper().Data.WriteJsonFile(Path.Combine("ConfigData", "reference.pet.config.json"), new Dictionary<string, PetConfigData>() { 
                ["Animals/dog"] = PetContent.dog0PetData_config,
                ["Animals/dog1"] = PetContent.dog1PetData_config,
                ["Animals/dog2"] = PetContent.dog2PetData_config,
                ["Animals/dog3"] = PetContent.dog3PetData_config,
                ["Animals/dog4"] = PetContent.dog4PetData_config,
                ["Animals/dog5"] = PetContent.dog5PetData_config,
                ["Animals/cat"] = PetContent.catPetData_config,
                ["Animals/cat1"] = PetContent.catPetData_config,
                ["Animals/cat2"] = PetContent.catPetData_config,
                ["Animals/cat3"] = PetContent.catPetData_config,
                ["Animals/cat4"] = PetContent.catPetData_config,
                ["Animals/cat5"] = PetContent.catPetData_config,
                ["Animals/turtle"] = PetContent.turtlePetData_config,
                ["Animals/turtle1"] = PetContent.turtlePetData_config,
            });
            ModEntry.GetModHelper().Data.WriteJsonFile(Path.Combine("ConfigData", "reference.pet.diet.json"), new Dictionary<string, string[]>()
            {
                ["cat"] = PetContent.DefaultCatNewEdibleItemsList,
                ["dog"] = PetContent.DefaultDogNewEdibleItemsList,
                ["turtle"] = PetContent.DefaultTurtleNewEdibleItemsList,
                ["generic"] = PetContent.DefaultGenericPetNewEdibleItemsList,
            });
            ModEntry.GetModHelper().Data.WriteJsonFile(Path.Combine("ConfigData", "reference.pet.hatoffset.json"), new Dictionary<string, Dictionary<string,float[]>>()
            {
                ["cat"] = PetContent.defaultEnhancedCatHatOffset,
                ["dog"] = PetContent.defaultEnhancedDog0HatOffset,
                ["dog1"] = PetContent.defaultEnhancedDog1HatOffset,
                ["dog2"] = PetContent.defaultEnhancedDog2HatOffset,
                ["dog3"] = PetContent.defaultEnhancedDog3HatOffset,
                ["dog4"] = PetContent.defaultEnhancedDog4HatOffset,
                ["dog5"] = PetContent.defaultEnhancedDog5HatOffset,
                ["turtle"] = PetContent.defaultEnhancedTurtleHatOffset
            });
        }

        public static PE_Pet_Data getDefaultDataForClassicPet(string _petType)
        {
            if (_petType == "cat" || _petType == "Cat")
            {
                return new PE_Pet_Data(catPetData_config.HatOffsetID, catPetData_config.BaseCooldownTime.Value, catPetData_config.MinDamage.Value, catPetData_config.MaxDamage.Value, catPetData_config.CritChance.Value, catPetData_config.AttackEffect, catPetData_config.HasWaitSkill.Value, catPetData_config.HasFollowSkill.Value, catPetData_config.HasHuntSkill.Value, catPetData_config.HasForageSkill.Value, catPetData_config.HasFishingSkill.Value, catPetData_config.DietListID, catPetData_config.TrickLearningTreat, catPetData_config.IsViciousType.Value);
            }
            else if (_petType == "dog" || _petType == "Dog")
            {
                return new PE_Pet_Data(dog0PetData_config.HatOffsetID, dog0PetData_config.BaseCooldownTime.Value, dog0PetData_config.MinDamage.Value, dog0PetData_config.MaxDamage.Value, dog0PetData_config.CritChance.Value, dog0PetData_config.AttackEffect, dog0PetData_config.HasWaitSkill.Value, dog0PetData_config.HasFollowSkill.Value, dog0PetData_config.HasHuntSkill.Value, dog0PetData_config.HasForageSkill.Value, dog0PetData_config.HasFishingSkill.Value, dog0PetData_config.DietListID, dog0PetData_config.TrickLearningTreat, dog0PetData_config.IsViciousType.Value);
            }
            else if (_petType == "turtle" || _petType == "Turtle")
            {
                return new PE_Pet_Data(turtlePetData_config.HatOffsetID, turtlePetData_config.BaseCooldownTime.Value, turtlePetData_config.MinDamage.Value, turtlePetData_config.MaxDamage.Value, turtlePetData_config.CritChance.Value, turtlePetData_config.AttackEffect, turtlePetData_config.HasWaitSkill.Value, turtlePetData_config.HasFollowSkill.Value, turtlePetData_config.HasHuntSkill.Value, turtlePetData_config.HasForageSkill.Value, turtlePetData_config.HasFishingSkill.Value, turtlePetData_config.DietListID, turtlePetData_config.TrickLearningTreat, turtlePetData_config.IsViciousType.Value);
            }
            else
            {
                return new PE_Pet_Data(nonRegisteredPetData_config.HatOffsetID, nonRegisteredPetData_config.BaseCooldownTime.Value, nonRegisteredPetData_config.MinDamage.Value, nonRegisteredPetData_config.MaxDamage.Value, nonRegisteredPetData_config.CritChance.Value, nonRegisteredPetData_config.AttackEffect, nonRegisteredPetData_config.HasWaitSkill.Value, nonRegisteredPetData_config.HasFollowSkill.Value, nonRegisteredPetData_config.HasHuntSkill.Value, nonRegisteredPetData_config.HasForageSkill.Value, nonRegisteredPetData_config.HasFishingSkill.Value, nonRegisteredPetData_config.DietListID, nonRegisteredPetData_config.TrickLearningTreat, nonRegisteredPetData_config.IsViciousType.Value);
            }
        }
        public static Dictionary<int, ModEntry.HatOffset_Simple> getDefaultHatOffsetForClassicPet(string _petType)
        {
            if (_petType == "cat" || _petType == "Cat")
            {
                return cookHatOffset(defaultEnhancedCatHatOffset);
            }
            else if (_petType == "dog" || _petType == "Dog")
            {
                return cookHatOffset(defaultEnhancedDog0HatOffset);
            }
            else if (_petType == "turtle" || _petType == "Turtle")
            {
                return cookHatOffset(defaultEnhancedTurtleHatOffset);
            }
            else { return new(); }
        }
        public static Dictionary<int, ModEntry.HatOffset_Simple> cookHatOffset(Dictionary<string, float[]> dic)
        {
            Dictionary<int, ModEntry.HatOffset_Simple> hatOffset = new();

            foreach (var hOset in dic)
            {
                var u = 0;
                var frameSplited = hOset.Key.Split(',');
                while (u < frameSplited.Length)
                {
                    if (!frameSplited[u].IsNull(out var sF) && !int.Parse(sF).IsNull(out var frame) && !hatOffset.ContainsKey(frame))
                    {
                        int tempDir = 0;
                        if (hOset.Value.Length > 2)
                        {
                            tempDir = (int)hOset.Value[2];
                        }
                        float tempX = 0;
                        if (hOset.Value.Length > 0)
                        {
                            tempX = hOset.Value[0];
                        }
                        float tempY = 0;
                        if (hOset.Value.Length > 1)
                        {
                            tempY = hOset.Value[1];
                        }
                        bool tempDrawHat = true;
                        if (hOset.Value.Length > 3)
                        {
                            int _binaryBool = Math.Clamp((int)hOset.Value[3], 0, 1);
                            tempDrawHat = _binaryBool == 1;
                        }
                        float tempScale = 1f;
                        if (hOset.Value.Length > 4)
                        {
                            tempScale = hOset.Value[4];
                        }
                        hatOffset.Add(frame, new(frame,tempX, tempY,tempDir, tempDrawHat, tempScale));
                    }
                    u++;
                }
            }
            return hatOffset;
        }

        public static Dictionary<string, (int FriendshipGain, int EnergyGain)> cookEdibleList(string[] _edibleItemList)
        {
            var dictionaryResult = new Dictionary<string, (int, int)>();
            if (_edibleItemList is null || _edibleItemList.Length == 0) { return dictionaryResult; }

            foreach (string _edibleItem in _edibleItemList)
            {
                if (!string.IsNullOrEmpty(_edibleItem))
                {
                    try
                    {
                        string _edibleItemQUID = null;
                        Match matchQuid = Regex.Match(_edibleItem, @"\[(.*?)\]");
                        if (matchQuid.Success)
                        {
                            if (matchQuid.Groups.Count > 1)
                            {
                                _edibleItemQUID = matchQuid.Groups[1].Value;
                            }
                        }

                        int _flags = 0;

                        int _friendshipGain = 0;
                        int _energyGain = 0;
                        Match match = Regex.Match(_edibleItem, @"(-?\d+)p\s+(-?\d+)e");
                        if (match.Success)
                        {
                            if (match.Groups.Count > 1 && int.TryParse(match.Groups[1].Value, out int tryValue1))
                            {
                                _friendshipGain = tryValue1;
                                _flags += 1;
                            }
                            if (match.Groups.Count > 2 && int.TryParse(match.Groups[2].Value, out int tryValue2))
                            {
                                _energyGain = tryValue2;
                                _flags += 1;
                            }
                        }
                        if (_flags >= 2 && !string.IsNullOrEmpty(_edibleItemQUID))
                        {
                            dictionaryResult.TryAdd(_edibleItemQUID, (_friendshipGain, _energyGain));
                        }
                    }
                    catch { }
                }
            }

            return dictionaryResult;
        }

        public static bool CheckIfPetDietListExistAnValid(string _dietListID, out Dictionary<string, (int FriendshipGain, int EnergyGain)> dietList)
        {
            dietList = null;
            if (string.IsNullOrEmpty(_dietListID)) { return false; }
            try
            {
                if (Game1.content.DoesAssetExist<Dictionary<string, string[]>>("Mods\\SunkenLace.PetsEnhancedMod\\PetDiet"))
                {
                    var dic = Game1.content.Load<Dictionary<string, string[]>>("Mods\\SunkenLace.PetsEnhancedMod\\PetDiet");
                    if (dic is not null && dic.TryGetValue(_dietListID, out var dietListRaw) && dietListRaw is not null)
                    {
                        dietList = cookEdibleList(dietListRaw);
                        return dietList is not null;
                    }
                }
            }
            catch { }
            return false;
        }
        public static bool CheckIfPetHatOffsetExistAndValid(string _hatOffsetID, out Dictionary<int,ModEntry.HatOffset_Simple> hatOffsetResult)
        {
            hatOffsetResult = null;
            if (string.IsNullOrEmpty(_hatOffsetID)) { return false; }
            try
            {
                if (Game1.content.DoesAssetExist<Dictionary<string, Dictionary<string, float[]>>>("Mods\\SunkenLace.PetsEnhancedMod\\PetHatOffset"))
                {
                    var dic = Game1.content.Load<Dictionary<string, Dictionary<string, float[]>>>("Mods\\SunkenLace.PetsEnhancedMod\\PetHatOffset");
                    if (dic is not null && dic.TryGetValue(_hatOffsetID, out var hatOffsetRaw) && hatOffsetRaw is not null)
                    {
                        hatOffsetResult = cookHatOffset(hatOffsetRaw);
                        return hatOffsetResult is not null;
                    }
                }
            }
            catch { }
            return false;
        }

    }

    public class PetConfigData
    {
        public string HatOffsetID { get; set; }


        public float? BaseCooldownTime { get; set; }


        public int? MinDamage { get; set; }



        public int? MaxDamage { get; set; }



        public float? CritChance { get; set; }



        public bool? IsViciousType { get; set; }



        public string AttackEffect { get; set; }



        public bool? HasWaitSkill { get; set; }


        public bool? HasFollowSkill { get; set; }


        public bool? HasForageSkill { get; set; }


        public bool? HasHuntSkill { get; set; }


        public bool? HasFishingSkill { get; set; }


        public string DietListID { get; set; }



        public string TrickLearningTreat { get; set; }


    }
}
