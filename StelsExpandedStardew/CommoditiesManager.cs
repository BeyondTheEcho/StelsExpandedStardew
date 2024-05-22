using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Objects;
using StardewValley.TerrainFeatures;

using static StelsExpandedStardew.SMAPI_Utils;

namespace StelsExpandedStardew
{
    internal static class CommoditiesManager
    {
        //ID to Price
        public static Dictionary<int, int> m_CommoditiesPriceDict = new();
        static Dictionary<int, int> m_CommoditiesDefaultPriceDict = new();

        public static IModHelper? m_Helper;

        private const float m_MinPriceAdjustment = 0.15f;
        private const float m_MaxPriceAdjustment = 2.5f;

        public static void UpdateCommoditiesPriceDict()
        {
            if (m_CommoditiesPriceDict.Count > 0) return;

            if (Game1.season == Season.Spring)
            {
                AdjustPricesForSeason(typeof(SpringCrops));
            }
            else if (Game1.season == Season.Summer)
            {
                AdjustPricesForSeason(typeof(SummerCrops));
            }
            else if (Game1.season == Season.Fall)
            {
                AdjustPricesForSeason(typeof(FallCrops));
            }
        }

        public static void AdjustPricesForSeason(Type enumType)
        {
            Array enumValues = Enum.GetValues(enumType);
            Array shuffledValues = ShuffleArray(enumValues);
            Array crops = shuffledValues.Cast<object>().Take(3).ToArray();

            Random random = new Random();

            foreach (object crop in crops)
            {
                int cropId = Convert.ToInt32(crop);

                if (m_CommoditiesDefaultPriceDict.TryGetValue(cropId, out int defaultPrice))
                {
                    float randomPriceMultiplier = m_MinPriceAdjustment + (float)(random.NextDouble() * (m_MaxPriceAdjustment - m_MinPriceAdjustment));

                    int finalPrice = (int)Math.Round(defaultPrice * randomPriceMultiplier);

                    m_CommoditiesPriceDict.Add(cropId, finalPrice);
                    Log($"Random Value: {crop} Price Percent: {randomPriceMultiplier} Price: {finalPrice}");
                }
            }

            //Invalidates cache to force reloading with the correct new values.
            m_Helper?.GameContent.InvalidateCache("Data/Objects");

            //Updates the value of all relevant items in all inventories.
            Utility.ForEachItem(item =>
            {
                if (item is StardewValley.Object objItem && TryExtractIDFromQualified(objItem.QualifiedItemId, out int itemID))
                {
                    if (m_CommoditiesPriceDict.ContainsKey(itemID))
                     {
                        objItem.Price = m_CommoditiesPriceDict[itemID];

                        Log($"Adjusted Value of Existing: {objItem.name} Price: {objItem.Price}");
                     }
                }

                return true;
            });

        }

        private static Array ShuffleArray(Array array)
        {
            Random random = new Random();

            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                object temp = array.GetValue(i);
                array.SetValue(array.GetValue(j), i);
                array.SetValue(temp, j);
            }

            return array;
        }

        public static void BuildDefaultPriceDict(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, ObjectData>().Data;

                    foreach (var item in data)
                    {
                        if (Int32.TryParse(item.Key, out int id) && Enum.IsDefined(typeof(Crops), id))
                        {
                            if (m_CommoditiesDefaultPriceDict.TryAdd(id, item.Value.Price))
                            Log($"ADDED TO DEFAULT: {item.Value.Name} (ID: {item.Key}) (Price: {item.Value.Price})");
                        }
                    }
                });
            }
        }
    }
}