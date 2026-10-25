using HarmonyLib;

using UnityEngine;

namespace AllAttractionsAvailable
{
    /// <summary>
    /// Treats chosen research categories (all of them by default) as already researched in scenarios. Everything the
    /// game gates behind research asks <see cref="ResearchController"/>, so answering there covers the build menus,
    /// blueprints, and park stats at once, and keeps those items out of the research queue. The park's saved research is
    /// never changed, so turning a category off puts it back exactly as it was.
    /// </summary>
    public class AAA : AbstractMod, IModSettings
    {
        public const string VERSION_NUMBER = "261025";
        public const string IDENTIFIER = "AllAttractionsAvailable";

        private Harmony harmony;

        public override string getIdentifier() => IDENTIFIER;

        public override string getName() => "Everything Available";

        public override string getDescription() => @"Skips research for rides, shops, deco, and park statistics. Each category can be switched off in the mod settings.";

        public override string getVersionNumber() => VERSION_NUMBER;

        public override bool isMultiplayerModeCompatible() => true;

        public override bool isRequiredByAllPlayersInMultiplayerMode() => false;

        public override void onEnabled()
        {
            harmony = new Harmony(IDENTIFIER);
            harmony.PatchAll(typeof(AAA).Assembly);
        }

        public override void onDisabled()
        {
            harmony?.UnpatchAll(IDENTIFIER);
            harmony = null;
        }

        public void onDrawSettingsUI()
        {
            GUILayout.Label("Already researched:");
            foreach (AAASettings.Toggle toggle in AAASettings.All)
                toggle.Value = GUILayout.Toggle(toggle.Value, " " + toggle.Label);
        }

        public void onSettingsOpened() { }

        public void onSettingsClosed()
        {
            PlayerPrefs.Save();
        }
    }

    /// <summary>Per-player switch for each research category, all on by default, kept in PlayerPrefs.</summary>
    public static class AAASettings
    {
        public sealed class Toggle
        {
            private readonly string key;
            private bool? value;

            public ResearchCategoryType Category { get; }

            public string Label { get; }

            public Toggle(ResearchCategoryType category, string label)
            {
                key = "AAA_Unlock_" + category;
                Category = category;
                Label = label;
            }

            // Cached, since research checks run for every buildable object whenever a build menu refreshes.
            public bool Value
            {
                get
                {
                    if (value == null)
                        value = PlayerPrefs.GetInt(key, 1) == 1;
                    return value.Value;
                }
                set
                {
                    if (this.value == value)
                        return;

                    this.value = value;
                    PlayerPrefs.SetInt(key, value ? 1 : 0);
                }
            }
        }

        public static readonly Toggle[] All =
        {
            new Toggle(ResearchCategoryType.CALM_RIDE, "Calm rides"),
            new Toggle(ResearchCategoryType.THRILL_RIDE, "Thrill rides"),
            new Toggle(ResearchCategoryType.COASTER, "Coasters"),
            new Toggle(ResearchCategoryType.TRANSPORT_RIDE, "Transport rides"),
            new Toggle(ResearchCategoryType.WATER_RIDE, "Water rides"),
            new Toggle(ResearchCategoryType.SHOPS, "Shops"),
            new Toggle(ResearchCategoryType.DECO, "Deco and path attachments"),
            new Toggle(ResearchCategoryType.STATISTICS, "Park statistics"),
        };

        public static bool IsSkipped(ResearchCategoryType category)
        {
            foreach (Toggle toggle in All)
            {
                if (toggle.Category == category)
                    return toggle.Value;
            }
            return false;
        }
    }

    /// <summary>
    /// Items in skipped categories count as unlocked and stop being researchable. The game's own item list decides the
    /// category, and it only holds content from DLCs the player owns, so nothing outside that gets unlocked.
    /// </summary>
    public static class ResearchPatch
    {
        private static bool IsSkipped(ResearchController research, string referenceName)
        {
            ResearchItem item = research.getItem(referenceName);
            return item != null && AAASettings.IsSkipped(item.category);
        }

        [HarmonyPatch(typeof(ResearchController), nameof(ResearchController.isUnlocked))]
        public static class UnlockedPatch
        {
            private static void Postfix(ResearchController __instance, string referenceName, ref bool __result)
            {
                if (!__result && IsSkipped(__instance, referenceName))
                    __result = true;
            }
        }

        [HarmonyPatch(typeof(ResearchController), nameof(ResearchController.isResearchable))]
        public static class ResearchablePatch
        {
            private static void Postfix(ResearchController __instance, string referenceName, ref bool __result)
            {
                if (__result && IsSkipped(__instance, referenceName))
                    __result = false;
            }
        }
    }
}
