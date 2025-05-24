using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            IO.ExportToFile(fileName);
            Messages.Message("AWA.SaveMessage", MessageTypeDefOf.SilentInput, false);
            Close();
        }
    }
}
