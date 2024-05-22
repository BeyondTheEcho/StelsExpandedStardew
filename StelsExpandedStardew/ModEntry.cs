using System;
using Microsoft.Xna.Framework;
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
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod
    {
        public IModHelper? m_Helper;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            m_Helper = helper;
            CommoditiesManager.m_Helper = helper;
            m_Monitor = Monitor;

            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Content.AssetRequested += CommoditiesManager.BuildDefaultPriceDict;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            CommoditiesManager.UpdateCommoditiesPriceDict();
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady) return;

            // print button presses to the console window
            //Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

            if (e.Button == SButton.MouseLeft)
            {
                if (TryGetTileProperty(e, out string property) && property.Contains("Commodities"))
                {
                    Monitor.Log("FOUND", LogLevel.Debug);
                }
            }
        }

        private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (CommoditiesManager.m_CommoditiesPriceDict.Count == 0) return;

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, ObjectData>().Data;

                    foreach (var item in CommoditiesManager.m_CommoditiesPriceDict) 
                    {
                        // Directly access and modify the relevant entries
                        if (data.TryGetValue(item.Key.ToString(), out ObjectData? itemData))
                        {
                            itemData.Price = item.Value;
                            Monitor.Log($"UPDATING CACHE - ID: {item.Key}, Name: {itemData.Name}, Price: {itemData.Price}", LogLevel.Debug);
                        }
                        else
                        {
                            Monitor.Log($"Item with ID {item.Key} not found", LogLevel.Warn);
                        }
                    }         
                });
            }
        }

    }
}