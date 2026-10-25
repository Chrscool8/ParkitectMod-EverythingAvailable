using HarmonyLib;

using System.IO;

using UnityEngine;

namespace AllAttractionsAvailable
{
    public class AAA : AbstractMod, IModSettings
    {
        public const string VERSION_NUMBER = "241017";

        public override string getIdentifier() => "AllAttractionsAvailable";

        public override string getName() => "All Attractions Available";

        public override string getDescription() => @"Forces all attractions to be considered available.";

        public override string getVersionNumber() => VERSION_NUMBER;

        public override bool isMultiplayerModeCompatible() => true;

        public override bool isRequiredByAllPlayersInMultiplayerMode() => false;

        public GameObject go { get; private set; }
        public static AAA Instance;
        private Harmony _harmony;

        private static bool debug_mode = false;
        public static bool MOD_ENABLED = false;

        public static string _local_mods_directory = "";



        public static void AAADebug(string debug_string, bool always_show = false)
        {
            if (debug_mode || always_show)
            {
                Debug.LogWarning("AllAttractionsAvailable: " + debug_string);

                if (_local_mods_directory != "")
                    File.AppendAllText(_local_mods_directory + "/AllAttractionsAvailable.txt", "AllAttractionsAvailable: " + debug_string + "\n");
            }

            if (File.Exists(_local_mods_directory + "/AAA_debug"))
            {
                debug_mode = true;
                //AAADebug("Found debug flag file.");
            }
        }

        public AAA()
        {
            _local_mods_directory = GameController.modsPath;


        }

        public override void onEnabled()
        {
            Instance = this;

            if (!MOD_ENABLED)
            {
                _harmony = new Harmony(getIdentifier());
                _harmony.PatchAll();
                MOD_ENABLED = true;
                AAADebug(debug_string: "ENABLING AAA", always_show: true);
            }

            AAADebug("Modspath: " + GameController.modsPath, always_show: true);
            AAADebug("ModspathRel: " + GameController.modsPathRelative, always_show: true);

            go = new GameObject();
            go.name = "AAA GameObject";
        }
        public override void onDisabled()
        {
            UnityEngine.Object.DestroyImmediate(go);

            if (MOD_ENABLED)
            {
                _harmony.UnpatchAll(getIdentifier());
                MOD_ENABLED = false;
                AAADebug(debug_string: "DISABLING AAA", always_show: true);
            }
        }

        public void onDrawSettingsUI()
        {
        }

        public void onSettingsClosed()
        {
        }

        public void onSettingsOpened()
        {
        }

        [HarmonyPatch(typeof(Attraction))] // Target the Attraction class
        [HarmonyPatch("isAvailable")]      // Target the method isAvailable
        class AttractionIsAvailablePatch
        {
            static bool Prefix(ref bool __result)
            {
                AAADebug("Setting Available.");
                __result = true; // Always return true
                return false;    // Skip the original method
            }
        }
    }
}