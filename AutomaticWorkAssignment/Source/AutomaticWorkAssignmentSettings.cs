using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class AutomaticWorkAssignmentSettings : ModSettings
    {
        public static int MaxCommitment = 10;
        public static float MentalBreakHourThreshold = 0.5f;
        private static string _hourBuffer;
        public static bool IgnoreUnmanagedWorkTypes = true;
        public static float ReservationTimeoutDays = 1f;

        internal void DoWindow(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            DoListing(listingStandard);
            listingStandard.End();
        }

        private static void DoListing(Listing_Standard listing)
        {
            listing.Label("Core settings");
            MaxCommitment = (int)listing.SliderLabeled($"Max commitment: {MaxCommitment}", MaxCommitment, 1, 10, tooltip: "The maximum work commitment that a pawn may have.");

            _hourBuffer = MentalBreakHourThreshold.ToString();
            listing.TextFieldNumericLabeled("Hour treshold for severe mental break", ref MentalBreakHourThreshold, ref _hourBuffer);
            MentalBreakHourThreshold = float.Parse(_hourBuffer);

            listing.CheckboxLabeled("Ignore unmanged work types", ref IgnoreUnmanagedWorkTypes);
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref MaxCommitment, "maxCommitment", 10);
            Scribe_Values.Look(ref MentalBreakHourThreshold, "mentalBreakHourTreshold", 0.5f);
            Scribe_Values.Look(ref IgnoreUnmanagedWorkTypes, "ignoreUmanagedWorkTypes", true);
        }
    }
}
