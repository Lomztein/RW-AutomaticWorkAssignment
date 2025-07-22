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
        public static FileInfo DefaultConfigurationFile => string.IsNullOrEmpty(_defaultConfigurationFile) ? null : IO.GetFile(_defaultConfigurationFile, IO.GetConfigDirectory());

        private const float MANAGER_WINDOW_HEIGHT_DEFAULT = 500;
        private static string _managerWindowHeightBuffer;
        public static float ManagerWindowHeight = MANAGER_WINDOW_HEIGHT_DEFAULT;

        private const float MANAGER_LIST_SECTION_WIDTH_DEFAULT = 300f;
        public static float ManagerListSectionWidth = MANAGER_LIST_SECTION_WIDTH_DEFAULT;
        private static string _managerListSectionWidthBuffer;

        private const float MANAGER_MAIN_SECTION_WIDTH_DEFAULT = 600f;
        public static float ManagerMainSectionWidth = MANAGER_MAIN_SECTION_WIDTH_DEFAULT;
        private static string _managerMainSectionWidthBuffer;

        private const float MANAGER_SETTINGS_SECTION_WIDTH_DEFAULT = 800f;
        public static float ManagerSettingsSectionWidth = MANAGER_SETTINGS_SECTION_WIDTH_DEFAULT;
        private static string _managerSettingsSectionWidthBuffer;

        public static float UIButtonSizeBase = 32;
        public static float UIHalfButtonSize => UIButtonSizeBase / 2;
        public static float UIInputSizeBase = 24;

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

            _managerWindowHeightBuffer = ManagerWindowHeight.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsManagerWindowHeight".Translate(MANAGER_WINDOW_HEIGHT_DEFAULT), ref ManagerWindowHeight, ref _managerWindowHeightBuffer, min: 100);
            ManagerWindowHeight = int.Parse(_managerWindowHeightBuffer);

            _managerListSectionWidthBuffer = ManagerListSectionWidth.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsManagerListSectionWidth".Translate(MANAGER_LIST_SECTION_WIDTH_DEFAULT), ref ManagerListSectionWidth, ref _managerListSectionWidthBuffer, min: 100);
            ManagerListSectionWidth = int.Parse(_managerListSectionWidthBuffer);

            _managerMainSectionWidthBuffer = ManagerMainSectionWidth.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsManagerMainSectionWidth".Translate(MANAGER_MAIN_SECTION_WIDTH_DEFAULT), ref ManagerMainSectionWidth, ref _managerMainSectionWidthBuffer, min: 100);
            ManagerMainSectionWidth = int.Parse(_managerMainSectionWidthBuffer);

            _managerSettingsSectionWidthBuffer = ManagerSettingsSectionWidth.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsManagerSettingsSectionWidth".Translate(MANAGER_SETTINGS_SECTION_WIDTH_DEFAULT), ref ManagerSettingsSectionWidth, ref _managerSettingsSectionWidthBuffer, min: 100);
            ManagerSettingsSectionWidth = int.Parse(_managerSettingsSectionWidthBuffer);

            if (listing.ButtonText("AWA.SettingsResetWindowLayout".Translate()))
            {
                ManagerWindowHeight = MANAGER_WINDOW_HEIGHT_DEFAULT;
                ManagerListSectionWidth = MANAGER_LIST_SECTION_WIDTH_DEFAULT;
                ManagerMainSectionWidth = MANAGER_MAIN_SECTION_WIDTH_DEFAULT;
                ManagerSettingsSectionWidth = MANAGER_SETTINGS_SECTION_WIDTH_DEFAULT;
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
            Scribe_Values.Look(ref ManagerWindowHeight, "managerWindowHeight", MANAGER_WINDOW_HEIGHT_DEFAULT);
            Scribe_Values.Look(ref ManagerListSectionWidth, "managerListSectionWidth", MANAGER_LIST_SECTION_WIDTH_DEFAULT);
            Scribe_Values.Look(ref ManagerMainSectionWidth, "managerMainSectionWidth", MANAGER_MAIN_SECTION_WIDTH_DEFAULT);
            Scribe_Values.Look(ref ManagerSettingsSectionWidth, "managerSettingsSectionWidth", MANAGER_SETTINGS_SECTION_WIDTH_DEFAULT);
        }
    }
}
