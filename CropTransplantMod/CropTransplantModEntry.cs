﻿using Harmony;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace CropTransplantMod
{
    /// <summary>The mod entry class loaded by SMAPI.</summary>
    public class CropTransplantModEntry : Mod
    {
        public static IMonitor ModMonitor;
        public static IModEvents Events;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            ModMonitor = Monitor;
            Events = helper.Events;

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.GameLoop.Saving += OnSaving;
        }

        
        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the game is launched, right before the first update tick. This happens once per game session (unrelated to loading saves). All mods are loaded and initialised at this point, so this is a good time to set up mod integrations.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            new DataLoader(Helper);

            var harmony = HarmonyInstance.Create("Digus.CustomCrystalariumMod");

            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.tryToPlaceItem)),
                prefix: new HarmonyMethod(typeof(TransplantOverrides), nameof(TransplantOverrides.TryToPlaceItem))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.canGrabSomethingFromHere)),
                prefix: new HarmonyMethod(typeof(TransplantOverrides), nameof(TransplantOverrides.CanGrabSomethingFromHere))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.playerCanPlaceItemHere)),
                prefix: new HarmonyMethod(typeof(TransplantOverrides), nameof(TransplantOverrides.PlayerCanPlaceItemHere))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(HoeDirt), nameof(HoeDirt.performUseAction)),
                prefix: new HarmonyMethod(typeof(TransplantOverrides), nameof(TransplantOverrides.PerformUseAction))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(FruitTree), nameof(FruitTree.performUseAction)),
                prefix: new HarmonyMethod(typeof(TransplantOverrides), nameof(TransplantOverrides.FruitTreePerformUseAction))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.pressUseToolButton)),
                prefix: new HarmonyMethod(typeof(TransplantOverrides), nameof(TransplantOverrides.PressUseToolButton))
            );

            if (DataLoader.ModConfig.EnableSoilTileUnderTrees)
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(Tree), nameof(Tree.draw)),
                    prefix: new HarmonyMethod(typeof(TransplantOverrides), nameof(TransplantOverrides.PreTreeDraw))
                );
            }
        }

        /// <summary>Raised before the game begins writes data to the save file (except the initial save creation).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (Game1.player.ActiveObject is HeldIndoorPot)
            {
                Game1.player.ActiveObject = TransplantOverrides.RegularPotObject;
                TransplantOverrides.CurrentHeldIndoorPot = null;
            }
        }
    }
}
