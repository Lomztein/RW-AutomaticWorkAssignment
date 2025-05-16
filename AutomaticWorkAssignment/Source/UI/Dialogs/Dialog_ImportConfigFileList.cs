using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Dialogs
{
    public class Dialog_ImportConfigFileList : Dialog_SaveFileList_Load
    {
        public Dialog_ImportConfigFileList()
        {
            interactButLabel = "Import";
        }

        protected override void DoFileInteraction(string fileName)
        {
            IO.ImportFromSave(fileName);
            Messages.Message("Work manager configuration imported..", MessageTypeDefOf.SilentInput, false);
            Close();
        }

        public override void PostClose()
        {
        }
    }
}
