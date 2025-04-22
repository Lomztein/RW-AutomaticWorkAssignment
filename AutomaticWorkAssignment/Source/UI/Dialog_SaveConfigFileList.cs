using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    public class Dialog_SaveConfigFileList : Dialog_ConfigFileList
    {
        public Dialog_SaveConfigFileList()
        {
            interactButLabel = "Save";
        }

        protected override bool ShouldDoTypeInField => true;

        protected override void DoFileInteraction(string fileName)
        {
            fileName = GenFile.SanitizedFileName(fileName);
            IO.ExportToFile(fileName);
            Messages.Message("Work manager configuration saved..", MessageTypeDefOf.SilentInput, false);
            Close();
        }
    }
}
