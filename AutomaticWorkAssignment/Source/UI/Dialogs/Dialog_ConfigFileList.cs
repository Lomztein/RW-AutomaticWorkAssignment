using RimWorld;
using System.Collections.Generic;
using System.IO;

namespace Lomzie.AutomaticWorkAssignment.UI.Dialogs
{
    public abstract class Dialog_ConfigFileList : Dialog_FileList
    {
        protected override void ReloadFiles()
        {
            files.Clear();
            IEnumerable<FileInfo> currentFiles = IO.GetConfigFiles();
            foreach (var file in currentFiles)
            {
                files.Add(new Verse.SaveFileInfo(file));
            }
        }
    }
}
