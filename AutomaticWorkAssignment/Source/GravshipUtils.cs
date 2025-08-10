using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public class GravshipUtils
    {
        public static bool LandingInProgress =>
            Find.GravshipController.LandingAreaConfirmationInProgress || WorldComponent_GravshipController.CutsceneInProgress;

        public static string GravshipConfigMigrationFileName;
        public static bool GravshipConfigMigrationFileExists()
            => (!string.IsNullOrEmpty(GravshipConfigMigrationFileName))
                && IO.GetFile(GravshipConfigMigrationFileName, IO.GetGravshipConfigDirectory()).Exists;
    }
}
