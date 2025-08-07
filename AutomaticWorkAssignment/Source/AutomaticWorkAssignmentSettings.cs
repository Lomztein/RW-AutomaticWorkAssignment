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
        public static float PassionLearnRateThreshold = 0.5f;
        public static bool IgnoreUnmanagedWorkTypes = true;
        public static bool AutoMigrateOnGravshipJump = true;
        public static float ReservationTimeoutDays = 1f;
        public static bool LogEnabled = false;
        private static string _defaultConfigurationFile = null;
        public static bool RedCriticalAlert = true;

        public static FileInfo DefaultConfigurationFile => string.IsNullOrEmpty(_defaultConfigurationFile) ? null : IO.GetFile(_defaultConfigurationFile, IO.GetConfigDirectory());

        private const float MANAGER_WINDOW_HEIGHT_DEFAULT = 500;
        public static float ManagerWindowHeight = MANAGER_WINDOW_HEIGHT_DEFAULT;

        private const float MANAGER_LIST_SECTION_WIDTH_DEFAULT = 300f;
        public static float ManagerListSectionWidth = MANAGER_LIST_SECTION_WIDTH_DEFAULT;

        private const float MANAGER_MAIN_SECTION_WIDTH_DEFAULT = 600f;
        public static float ManagerMainSectionWidth = MANAGER_MAIN_SECTION_WIDTH_DEFAULT;

        private const float MANAGER_SETTINGS_SECTION_WIDTH_DEFAULT = 800f;
        public static float ManagerSettingsSectionWidth = MANAGER_SETTINGS_SECTION_WIDTH_DEFAULT;

        public static float UIButtonSizeBase = 32;
        public static float UIHalfButtonSize => UIButtonSizeBase / 2;
        public static float UIInputSizeBase = 24;

        private static string _inputBuffer;

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

            _inputBuffer = MentalBreakHourThreshold.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsMaxHoursForMentalBreak".Translate(), ref MentalBreakHourThreshold, ref _inputBuffer);
            MentalBreakHourThreshold = float.Parse(_inputBuffer);

            _inputBuffer = PassionLearnRateThreshold.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsPassionLearnRateThreshold".Translate(), ref PassionLearnRateThreshold, ref _inputBuffer);
            PassionLearnRateThreshold = float.Parse(_inputBuffer);

            listing.CheckboxLabeled("AWA.SettingsIgnoreUnmanagedWork".Translate(), ref IgnoreUnmanagedWorkTypes);
            listing.CheckboxLabeled("AWA.SettingsAdditionalLogging".Translate(), ref LogEnabled, tooltip: "AWA.SettingsAdditionalLoggingTooltip".Translate());
            listing.CheckboxLabeled("AWA.SettingsRedCriticalAlert".Translate(), ref RedCriticalAlert, tooltip: "AWA.SettingsRedCriticalAlertTooltip".Translate());

            if (ModsConfig.OdysseyActive)
                listing.CheckboxLabeled("AWA.SettingsAutoMigrateOnGravshipJump".Translate(), ref AutoMigrateOnGravshipJump, tooltip: "AWA.SettingsAutoMigrateOnGravshipJumpTooltip".Translate());

            if (DefaultConfigurationFile != null && !DefaultConfigurationFile.Exists)
                _defaultConfigurationFile = null;

            if (listing.ButtonTextLabeled("AWA.SettingsDefaultConfigurationFile".Translate(), (string.IsNullOrEmpty(_defaultConfigurationFile) ? "AWA.SettingsDefaultConfigurationFileNoneSelected".Translate().RawText : _defaultConfigurationFile)))
            {
                OpenDefaultConfigurationMenu();
            }

            listing.Label("AWA.SettingsHeaderUI".Translate());

            _inputBuffer = ManagerWindowHeight.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsManagerWindowHeight".Translate(MANAGER_WINDOW_HEIGHT_DEFAULT), ref ManagerWindowHeight, ref _inputBuffer, min: 100);
            ManagerWindowHeight = int.Parse(_inputBuffer);

            _inputBuffer = ManagerListSectionWidth.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsManagerListSectionWidth".Translate(MANAGER_LIST_SECTION_WIDTH_DEFAULT), ref ManagerListSectionWidth, ref _inputBuffer, min: 100);
            ManagerListSectionWidth = int.Parse(_inputBuffer);

            _inputBuffer = ManagerMainSectionWidth.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsManagerMainSectionWidth".Translate(MANAGER_MAIN_SECTION_WIDTH_DEFAULT), ref ManagerMainSectionWidth, ref _inputBuffer, min: 100);
            ManagerMainSectionWidth = int.Parse(_inputBuffer);

            _inputBuffer = ManagerSettingsSectionWidth.ToString();
            listing.TextFieldNumericLabeled("AWA.SettingsManagerSettingsSectionWidth".Translate(MANAGER_SETTINGS_SECTION_WIDTH_DEFAULT), ref ManagerSettingsSectionWidth, ref _inputBuffer, min: 100);
            ManagerSettingsSectionWidth = int.Parse(_inputBuffer);

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
            Scribe_Values.Look(ref PassionLearnRateThreshold, "passionLearnRateThreshold", 0.5f);
            Scribe_Values.Look(ref IgnoreUnmanagedWorkTypes, "ignoreUmanagedWorkTypes", true);
            Scribe_Values.Look(ref LogEnabled, "logEnabled", false);
            Scribe_Values.Look(ref RedCriticalAlert, "redCriticalAlert", true);
            Scribe_Values.Look(ref AutoMigrateOnGravshipJump, "autoMigrateOnGravshipJump", true);
            Scribe_Values.Look(ref _defaultConfigurationFile, "defaultConfigurationFile", null);
            Scribe_Values.Look(ref ManagerWindowHeight, "managerWindowHeight", MANAGER_WINDOW_HEIGHT_DEFAULT);
            Scribe_Values.Look(ref ManagerListSectionWidth, "managerListSectionWidth", MANAGER_LIST_SECTION_WIDTH_DEFAULT);
            Scribe_Values.Look(ref ManagerMainSectionWidth, "managerMainSectionWidth", MANAGER_MAIN_SECTION_WIDTH_DEFAULT);
            Scribe_Values.Look(ref ManagerSettingsSectionWidth, "managerSettingsSectionWidth", MANAGER_SETTINGS_SECTION_WIDTH_DEFAULT);
        }
    }
}
