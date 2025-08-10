using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Dialogs
{
    public class Dialog_ImportMapConfigFileList : Window
    {
        private string _fileName;
        private Dictionary<int, string> maps = new Dictionary<int, string>();

        public override Vector2 InitialSize => new Vector2(620f, 700f) / 2f;

        public Dialog_ImportMapConfigFileList()
        {
            ReloadMaps();
        }

        protected void ReloadMaps()
        {
            maps.Clear();

            _fileName = Dialog_ImportSaveConfigFileList.SelectedFileName;

            try
            {
                string saveFile = GenFilePaths.FilePathForSavedGame(_fileName);
                Scribe.loader.InitLoading(saveFile);
                ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.ModList, true);
                Scribe.EnterNode("game");
                var curNode = Scribe.loader.curXmlParent;

                var tuples = IO.FindWorldObjectsWithMaps(curNode);

                foreach (var tuple in tuples)
                {
                    string id = tuple.Item1.SelectSingleNode("ID")?.InnerText ?? null;
                    string name = tuple.Item1.SelectSingleNode("nameInt")?.InnerText ?? "[Unnamed colony]";

                    if (id == null)
                        continue;

                    maps.Add(int.Parse(id), name);
                }

                Scribe.loader.FinalizeLoading();
            }
            catch (Exception exc)
            {
                Log.Error(exc.Message + ": " + exc.StackTrace);
            }
            finally
            {
                if (Scribe.mode != LoadSaveMode.Inactive)
                    Scribe.loader.FinalizeLoading();
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (Widgets.CloseButtonFor(inRect))
                Close();

            Listing_Standard listingStandard = new Listing_Standard();

            Rect listingRect = new Rect(inRect);
            listingRect.height -= 40;
            listingRect.y += 40;

            listingStandard.Begin(listingRect);

            foreach (var map in maps)
            {
                if (listingStandard.ButtonText(map.Value))
                    DoFileInteraction(map.Key);
            }

            listingStandard.End();
        }

        protected void DoFileInteraction(int mapId)
        {
            IO.ImportFromSave(MapWorkManager.GetCurrentMapManager(), _fileName, mapId);
            Messages.Message("AWA.ImportMessage".Translate(), MessageTypeDefOf.SilentInput, false);
            Close();
        }

        public override void PostClose()
        {
        }
    }
}
