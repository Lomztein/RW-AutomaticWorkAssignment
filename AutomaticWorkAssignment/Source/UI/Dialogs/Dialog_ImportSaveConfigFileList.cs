using RimWorld;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Dialogs
{
    public class Dialog_ImportSaveConfigFileList : Dialog_SaveFileList_Load
    {
        public static string SelectedFileName { get; private set; }

        public Dialog_ImportSaveConfigFileList()
        {
            interactButLabel = "AWA.SelectSave".Translate();
        }

        protected override void DoFileInteraction(string fileName)
        {
            SelectedFileName = fileName;
            Find.WindowStack.Add(new Dialog_ImportMapConfigFileList());
            Close();
        }

        public override void PostClose()
        {
        }
    }
}
