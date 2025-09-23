using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Dialogs
{
    public class Dialog_LoadConfigFileList : Dialog_ConfigFileList
    {
        public Dialog_LoadConfigFileList()
        {
            interactButLabel = "AWA.Load".Translate();
        }

        protected override void DoFileInteraction(string fileName)
        {
            IO.ImportFromFile(MapWorkManager.GetCurrentMapManager(), fileName, IO.GetConfigDirectory());
            Messages.Message("AWA.LoadMessage".Translate(), MessageTypeDefOf.SilentInput, false);
            WorkManagerWindow.ResetCurrentWorkSpecification();
            Close();
        }
    }
}
