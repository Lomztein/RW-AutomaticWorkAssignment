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
            interactButLabel = "AWA.Load".Translate();
        }

        protected override void DoFileInteraction(string fileName)
        {
            IO.ImportFromFile(fileName);
            Messages.Message("AWA.LoadMessage", MessageTypeDefOf.SilentInput, false);
            Close();
        }
    }
}
