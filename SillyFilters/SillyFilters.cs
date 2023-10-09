#pragma warning disable IDE0051 // Remove unused private members; Unity will call Awake for us.
using HarmonyLib;
using OWML.Common;
using OWML.Common.Menus;
using OWML.ModHelper;
using System.Reflection;

namespace SillyFilters
{
    /// <summary>
    /// Entry to the mod.
    /// </summary>
    public class SillyFilters : ModBehaviour
    {
        public static SillyFilters instance;

        /// <summary>The currently selected image file name.</summary>
        public string imageFileName;

        private void Awake()
        {
            // patch
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            instance = this;
        }

        public override void Configure(IModConfig config)
        {
            // change the filter when the user changes the settings
            imageFileName = config.GetSettingsValue<string>("Filter") + ".png";
        }
    }
}