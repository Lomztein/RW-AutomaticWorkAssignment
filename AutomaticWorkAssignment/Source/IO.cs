using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;
using Verse.Noise;

namespace Lomzie.AutomaticWorkAssignment
{
    public class IO
    {
        private const string CONFIG_SAVE_FOLDER = "AutomaticWorkAssignment/Configs";
        public static DirectoryInfo GetConfigDirectory ()
        {
            DirectoryInfo directory = new DirectoryInfo(Path.Combine(GenFilePaths.SaveDataFolderPath, CONFIG_SAVE_FOLDER));
            if (!directory.Exists)
            {
                directory.Create();
            }
            return directory;
        }

        public static FileInfo GetConfigFile(string fileName)
        {
            DirectoryInfo folder = GetConfigDirectory();
            FileInfo file = new FileInfo(Path.ChangeExtension(Path.Combine(folder.FullName, fileName), "xml"));
            return file;
        }

        public static IEnumerable<FileInfo> GetConfigFiles()
        {
            DirectoryInfo directory = GetConfigDirectory();
            return directory.GetFiles("*.xml");
        }

        public static void ExportToFile(string fileName)
        {
            try
            {
                FileInfo file = GetConfigFile(fileName);
                SafeSaver.Save(file.FullName, "workManagerConfig", () => {
                    ScribeMetaHeaderUtility.WriteMetaHeader();
                    Scribe_Deep.Look(ref WorkManager.Instance, "workManager");
                });
            }catch (Exception ex)
            {
                Log.Error(ex.Message + " - " + ex.StackTrace);
            }
        }

        public static void ImportFromFile(string fileName)
        {
            try
            {
                FileInfo file = GetConfigFile(fileName);
                Scribe.loader.InitLoading(file.FullName);
                ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.ModList, true);
                if (Scribe.EnterNode("workManager"))
                {
                    WorkManager.Instance.ExposeData();
                }
                else
                {
                    Log.Error("Unable to enter workManagerConfig node.");
                }
                Scribe.loader.FinalizeLoading();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + " - " + ex.StackTrace);
            }
        }

        public static void ImportFromSave(string saveName)
        {
            try
            {
                string saveFile = GenFilePaths.FilePathForSavedGame(saveName);
                Scribe.loader.InitLoading(saveFile);
                ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.ModList, true);
                Scribe.EnterNode("game");
                Scribe.EnterNode("components");
                var curNode = Scribe.loader.curXmlParent;
                bool found = false;
                foreach (XmlNode child in curNode.ChildNodes)
                {
                    XmlNode classNode = child.Attributes.GetNamedItem("Class");
                    if (classNode.InnerText == typeof(WorkManager).FullName)
                    {
                        Scribe.loader.curXmlParent = child;
                        WorkManager.Instance.ExposeData();
                        found = true;
                        break;
                    }
                }
                if (!found)
                    Messages.Message("No work manager configuration found in save game", MessageTypeDefOf.NegativeEvent);

                Scribe.loader.FinalizeLoading();
            }catch(Exception ex)
            {
                Log.Error(ex.Message + " - " + ex.StackTrace);
            }
        }
    }
}
