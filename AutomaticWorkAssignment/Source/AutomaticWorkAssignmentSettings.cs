using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace Lomzie.AutomaticWorkAssignment
{
    public class AutomaticWorkAssignmentSettings : ModSettings
    {
        public static int MaxCommitment = 10;
        public static float MentalBreakHourThreshold = 0.5f;
        private static string _hourBuffer;
        public static bool IgnoreUnmanagedWorkTypes = true;
        public static float ReservationTimeoutDays = 1f;
        public static bool LogEnabled = false;
        private static string _defaultConfigurationFile = null;
        public static FileInfo DefaultConfigurationFile => string.IsNullOrEmpty(_defaultConfigurationFile) ? null : IO.GetConfigFile(_defaultConfigurationFile);

        private const float MANAGER_WINDOW_WIDTH_DEFAULT = 1400;
        private static string _managerWindowWidthBuffer;
        public static float ManagerWindowWidth = MANAGER_WINDOW_WIDTH_DEFAULT;
        private const float MANAGER_WINDOW_HEIGHT_DEFAULT = 400;
        private static string _managerWindowHeightBuffer;
        public static float ManagerWindowHeight = MANAGER_WINDOW_HEIGHT_DEFAULT;

        private const float MANAGER_LIST_SECTION_RATIO_DEFAULT = 0.15f;
        public static float ManagerListSectionRatio = MANAGER_LIST_SECTION_RATIO_DEFAULT;
        public static float ManagerListSectionRatioNormalized => ManagerListSectionRatio / (ManagerListSectionRatio + ManagerMainSectionRatio + ManagerSettingsSectionRatio);

        private const float MANAGER_MAIN_SECTION_RATIO_DEFAULT = 0.4f;
        public static float ManagerMainSectionRatio = MANAGER_MAIN_SECTION_RATIO_DEFAULT;
        public static float ManagerMainSectionRatioNormalized => ManagerMainSectionRatio / (ManagerListSectionRatio + ManagerMainSectionRatio + ManagerSettingsSectionRatio);

        private const float MANAGER_SETTINGS_SECTION_RATIO_DEFAULT = 0.45f;
        public static float ManagerSettingsSectionRatio = MANAGER_SETTINGS_SECTION_RATIO_DEFAULT;
        public static float ManagerSettingsSectionRatioNormalized => ManagerSettingsSectionRatio / (ManagerListSectionRatio + ManagerMainSectionRatio + ManagerSettingsSectionRatio);

        internal void DoWindow(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            DoListing(listingStandard);
            listingStandard.End();
        }

        private static void DoListing(Listing_Standard listing)
        {
            listing.Label("AWA.SettingsHeaderCore".Translate());
            MaxCommitment = (int)listing.SliderLabeled("AWA.SettingsMaxCommitment".Translate(MaxCommitment), MaxCommitment, 1, 10, tooltip: "AWA.SettingsMaxCommitmentTooltip".Translate());

            _hourBuffer = MentalBreakHourThreshold.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsMaxHoursForMentalBreak".Translate(), ref MentalBreakHourThreshold, ref _hourBuffer);
            MentalBreakHourThreshold = float.Parse(_hourBuffer);

            listing.CheckboxLabeled("AWA.SettingsIgnoreUnmanagedWork".Translate(), ref IgnoreUnmanagedWorkTypes);
            listing.CheckboxLabeled("AWA.SettingsAdditionalLogging".Translate(), ref LogEnabled, tooltip: "AWA.SettingsAdditionalLoggingTooltip".Translate());

            if (DefaultConfigurationFile != null && !DefaultConfigurationFile.Exists)
                _defaultConfigurationFile = null;

            if (listing.ButtonTextLabeled("AWA.SettingsDefaultConfigurationFile".Translate(), (string.IsNullOrEmpty(_defaultConfigurationFile) ? "AWA.SettingsDefaultConfigurationFileNoneSelected".Translate().RawText : _defaultConfigurationFile)))
            {
                OpenDefaultConfigurationMenu();
            }

            listing.Label("AWA.SettingsHeaderUI".Translate());

            _managerWindowWidthBuffer = ManagerWindowWidth.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsManagerWindowWidth".Translate(MANAGER_WINDOW_WIDTH_DEFAULT), ref ManagerWindowWidth, ref _managerWindowWidthBuffer, min: 100);
            ManagerWindowWidth = int.Parse(_managerWindowWidthBuffer);

            _managerWindowHeightBuffer = ManagerWindowHeight.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsManagerWindowHeight".Translate(MANAGER_WINDOW_HEIGHT_DEFAULT), ref ManagerWindowHeight, ref _managerWindowHeightBuffer, min: 100);
            ManagerWindowHeight = int.Parse(_managerWindowHeightBuffer);

            ManagerListSectionRatio = listing.SliderLabeled("AWA.SettingsManagerListSectionRatio".Translate(Mathf.Round(ManagerListSectionRatioNormalized * 100), MANAGER_LIST_SECTION_RATIO_DEFAULT * 100), ManagerListSectionRatio, 0f, 1f);
            ManagerListSectionRatio = Mathf.Round(ManagerListSectionRatio * 100f) / 100f;

            ManagerMainSectionRatio = listing.SliderLabeled("AWA.SettingsManagerMainSectionRatio".Translate(Mathf.Round(ManagerMainSectionRatioNormalized * 100), MANAGER_MAIN_SECTION_RATIO_DEFAULT * 100), ManagerMainSectionRatio, 0f, 1f);
            ManagerMainSectionRatio = Mathf.Round(ManagerMainSectionRatio * 100f) / 100f;

            ManagerSettingsSectionRatio = listing.SliderLabeled("AWA.SettingsManagerSettingsSectionRatio".Translate(Mathf.Round(ManagerSettingsSectionRatioNormalized * 100), MANAGER_SETTINGS_SECTION_RATIO_DEFAULT * 100), ManagerSettingsSectionRatio, 0f, 1f);
            ManagerSettingsSectionRatio = Mathf.Round(ManagerSettingsSectionRatio * 100f) / 100f;

            if (listing.ButtonText("AWA.SettingsResetWindowLayout".Translate()))
            {
                ManagerWindowWidth = MANAGER_WINDOW_WIDTH_DEFAULT;
                ManagerWindowHeight = MANAGER_WINDOW_HEIGHT_DEFAULT;
                ManagerListSectionRatio = MANAGER_LIST_SECTION_RATIO_DEFAULT;
                ManagerMainSectionRatio = MANAGER_MAIN_SECTION_RATIO_DEFAULT;
                ManagerSettingsSectionRatio = MANAGER_SETTINGS_SECTION_RATIO_DEFAULT;
            }
        }

        private static void OpenDefaultConfigurationMenu()
        {
            FloatMenuUtility.MakeMenu(Enumerable.Concat(new string[] { null }, IO.GetConfigFiles().Select(x => x.Name)), x => x ?? "AWA.SettingsDefaultConfigurationFileNoneSelected".Translate(), (x) => () => SelectDefaultConfiguration(x));
        }

        private static void SelectDefaultConfiguration(string fileName)
        {
            _defaultConfigurationFile = fileName;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref MaxCommitment, "maxCommitment", 10);
            Scribe_Values.Look(ref MentalBreakHourThreshold, "mentalBreakHourTreshold", 0.5f);
            Scribe_Values.Look(ref IgnoreUnmanagedWorkTypes, "ignoreUmanagedWorkTypes", true);
            Scribe_Values.Look(ref LogEnabled, "logEnabled", false);
            Scribe_Values.Look(ref _defaultConfigurationFile, "defaultConfigurationFile", null);
            Scribe_Values.Look(ref ManagerWindowWidth, "managerWindowWidth", MANAGER_WINDOW_WIDTH_DEFAULT);
            Scribe_Values.Look(ref ManagerWindowHeight, "managerWindowHeight", MANAGER_WINDOW_HEIGHT_DEFAULT);
            Scribe_Values.Look(ref ManagerListSectionRatio, "managerListSectionRatio", MANAGER_LIST_SECTION_RATIO_DEFAULT);
            Scribe_Values.Look(ref ManagerMainSectionRatio, "managerMainSectionRatio", MANAGER_MAIN_SECTION_RATIO_DEFAULT);
            Scribe_Values.Look(ref ManagerSettingsSectionRatio, "managerSettingsSectionRatio", MANAGER_SETTINGS_SECTION_RATIO_DEFAULT);
        }
    }
}
