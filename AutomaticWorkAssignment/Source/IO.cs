using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Verse;
using Verse.Noise;

namespace Lomzie.AutomaticWorkAssignment
{
    public class IO
    {
        private const string CONFIG_SAVE_FOLDER = "AutomaticWorkAssignment/Configs";
        private const string GRAVSHIP_CONFIG_SAVE_FOLDER = "AutomaticWorkAssignment/GravshipConfigs";
        private const string CLIPBOARD_SAVE_FOLDER = "AutomaticWorkAssignment/Clipboard";

        public static DirectoryInfo GetConfigDirectory ()
            => GetOrCreateDirectory (CONFIG_SAVE_FOLDER);

        public static DirectoryInfo GetClipboardDirectory()
            => GetOrCreateDirectory(CLIPBOARD_SAVE_FOLDER);

        public static DirectoryInfo GetGravshipConfigDirectory()
            => GetOrCreateDirectory(GRAVSHIP_CONFIG_SAVE_FOLDER);

        private static DirectoryInfo GetOrCreateDirectory(string saveDataSubfolder)
        {
            DirectoryInfo directory = new DirectoryInfo(Path.Combine(GenFilePaths.SaveDataFolderPath, saveDataSubfolder));
            if (!directory.Exists)
            {
                directory.Create();
            }
            return directory;
        }

        public static FileInfo GetFile(string fileName, DirectoryInfo directory)
        {
            FileInfo file = new FileInfo(Path.ChangeExtension(Path.Combine(directory.FullName, fileName), "xml"));
            return file;
        }

        public static IEnumerable<FileInfo> GetConfigFiles()
        {
            DirectoryInfo directory = GetConfigDirectory();
            return directory.GetFiles("*.xml");
        }

        public static void ExportToFile(IExposable exposable, string fileName, DirectoryInfo directory)
        {
            try
            {
                FileInfo file = GetFile(fileName, directory);
                SafeSaver.Save(file.FullName, "workManagerConfig", () => {
                    ScribeMetaHeaderUtility.WriteMetaHeader();
                    Scribe_Deep.Look(ref exposable, "workManager");
                });
            }catch (Exception ex)
            {
                Log.Error(ex.Message + " - " + ex.StackTrace);
            }
        }

        public static void ImportFromFile(IExposable exposable, string fileName, DirectoryInfo directory)
        {
            try
            {
                FileInfo file = GetFile(fileName, directory);
                Scribe.loader.InitLoading(file.FullName);
                ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.ModList, true);
                if (Scribe.EnterNode("workManager"))
                {
                    exposable.ExposeData();
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

        public static void ImportFromSave(MapWorkManager toManager, string saveName, int mapId)
        {
            try
            {
                string saveFile = GenFilePaths.FilePathForSavedGame(saveName);
                Scribe.loader.InitLoading(saveFile);
                ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.ModList, true);
                Scribe.EnterNode("game");
                var curNode = Scribe.loader.curXmlParent;
                XmlNode mapNode = FindMapNodeWithWorldObjectId(curNode, mapId);
                XmlNode components = mapNode.SelectSingleNode("components");

                bool found = false;
                foreach (XmlNode child in components.ChildNodes)
                {
                    XmlNode classNode = child.Attributes.GetNamedItem("Class");
                    if (classNode.InnerText == typeof(MapWorkManager).FullName)
                    {
                        Scribe.loader.curXmlParent = child;
                        toManager.ExposeData();
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    // Try legacy
                    Scribe.EnterNode("components");
                    curNode = Scribe.loader.curXmlParent;
                    foreach (XmlNode child in curNode.ChildNodes)
                    {
                        XmlNode classNode = child.Attributes.GetNamedItem("Class");
                        if (classNode.InnerText == typeof(WorkManager).FullName)
                        {
                            Scribe.loader.curXmlParent = child;
                            toManager.ExposeData();
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        Messages.Message("No work manager configuration found in save game", MessageTypeDefOf.NegativeEvent);
                }

                Scribe.loader.FinalizeLoading();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message + " - " + ex.StackTrace);
            }
            finally
            {
                if (Scribe.mode != LoadSaveMode.Inactive)
                    Scribe.loader.FinalizeLoading();
            }
        }

        public static XmlNode FindMapNodeWithWorldObjectId(XmlNode gameNode, int worldObjectId)
        {
            XmlNode mapNode = gameNode.SelectSingleNode("maps");
            foreach (XmlNode child in mapNode.ChildNodes)
            {
                XmlNode mapRefNode = child.SelectSingleNode("mapInfo/parent");
                if (mapRefNode == null) continue;
                string mapRef = mapRefNode.InnerText;

                int mapId = int.Parse(mapRef.Substring("WorldObject_".Length));
                if (mapId == worldObjectId)
                    return child;
            }

            return null;
        }

        public static IEnumerable<Tuple<XmlNode, XmlNode>> FindWorldObjectsWithMaps (XmlNode gameNode)
        {
            XmlNode mapNode = gameNode.SelectSingleNode("maps");
            XmlNode worldObjects = gameNode.SelectSingleNode("world/worldObjects/worldObjects");

            foreach (XmlNode child in mapNode.ChildNodes)
            {
                XmlNode mapRefNode = child.SelectSingleNode("mapInfo/parent");
                if (mapRefNode == null) continue;
                string mapRef = mapRefNode.InnerText;
                int mapId = int.Parse(mapRef.Substring("WorldObject_".Length));

                foreach (XmlNode worldObj in worldObjects.ChildNodes)
                {
                    XmlNode idNode = worldObj.SelectSingleNode("ID");
                    int id = int.Parse(idNode.InnerText);
                    if (id == mapId)
                        yield return new Tuple<XmlNode, XmlNode>(worldObj, child);
                }
            }
        }
    }
}
