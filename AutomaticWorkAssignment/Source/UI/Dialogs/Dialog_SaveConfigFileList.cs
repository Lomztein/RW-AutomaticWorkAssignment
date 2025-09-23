using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Dialogs
{
    public class Dialog_SaveConfigFileList : Dialog_ConfigFileList
    {
        public Dialog_SaveConfigFileList()
        {
            interactButLabel = "AWA.Save".Translate();
        }

        protected override bool ShouldDoTypeInField => true;

        protected override void DoFileInteraction(string fileName)
        {
            fileName = GenFile.SanitizedFileName(fileName);
            IO.ExportToFile(MapWorkManager.GetCurrentMapManager(), fileName, IO.GetConfigDirectory());
            Messages.Message("AWA.SaveMessage".Translate(), MessageTypeDefOf.SilentInput, false);
            WorkManagerWindow.ResetCurrentWorkSpecification();
            Close();
        }
    }
}
