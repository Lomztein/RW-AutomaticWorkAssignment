using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Dialogs
{
    public class Dialog_LoadConfigFileList : Dialog_ConfigFileList
    {
        public Dialog_LoadConfigFileList()
        {
            interactButLabel = "Load";
        }

        protected override void DoFileInteraction(string fileName)
        {
            IO.ImportFromFile(fileName);
            Messages.Message("Work manager configuration loaded..", MessageTypeDefOf.SilentInput, false);
            Close();
        }
    }
}
