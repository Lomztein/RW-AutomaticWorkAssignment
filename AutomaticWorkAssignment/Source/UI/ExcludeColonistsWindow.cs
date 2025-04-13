using Lomzie.AutomaticWorkAssignment.Defs;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    public class ExcludeColonistsWindow : Window
    {
        public override Vector2 InitialSize => new Vector2(350, 512);

        private float _listHeight;
        private Vector2 _listPosition;

        private float ListScrollBarWidth = 24;
        private float AddPawnButtonSize = 48;
        private float RowHeight = 32;

        public override void DoWindowContents(Rect inRect)
        {
            if (Widgets.CloseButtonFor(inRect))
            {
                Close();
            }

            Rect headerRect = Utils.GetSubRectFraction(inRect, Vector2.zero, new Vector2(1f, 0.075f));
            inRect = Utils.GetSubRectFraction(inRect, new Vector2(0f, 0.075f), new Vector2(1f, 1f));
            
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(headerRect, "Excluded Pawns");
            Text.Anchor = TextAnchor.UpperLeft;

            WorkManager manager = WorkManager.Instance;

            var excluded = manager.ExcludePawns;

            var height = _listHeight;
            var scrollView = new Rect(0f, 0f, inRect.width, height);
            if (height > inRect.height)
                scrollView.width -= ListScrollBarWidth;

            Widgets.BeginScrollView(inRect, ref _listPosition, scrollView);
            var scrollContent = scrollView;

            Widgets.BeginGroup(scrollContent);
            var cur = Vector2.zero;
            var i = 0;

            foreach (Pawn pawn in excluded)
            {
                var row = new Rect(0, cur.y, inRect.width, RowHeight);
                Rect labelRect = Utils.GetSubRectFraction(row, new Vector2(0f, 0f), new Vector2(0.8f, 1f));
                Rect removeRect = Utils.GetSubRectFraction(row, new Vector2(0.8f, 0f), new Vector2(1f, 1f));

                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(labelRect, pawn.Name.ToString());
                if (Widgets.ButtonText(removeRect, "X"))
                    Find.Root.StartCoroutine(RemovePawn(pawn));
                Text.Anchor = TextAnchor.UpperLeft;

                Widgets.DrawHighlightIfMouseover(row);
                if (i++ % 2 == 1) Widgets.DrawAltRect(row);

                cur.y += row.height;
            }

            if (manager.ExcludePawns.Count != manager.GetPawnCount())
            {
                // row for new pawn.
                var newRect = new Rect(0f, cur.y, inRect.width, AddPawnButtonSize);
                newRect = Utils.ShrinkByMargin(newRect, 8f);
            
                Text.Anchor = TextAnchor.MiddleCenter;
                WorkTypeDef workDef = new WorkTypeDef();
                if (Widgets.ButtonText(newRect, "Exclude pawn"))
                {
                    var pawns = WorkManager.Instance.GetAllPawns();
                    FloatMenuUtility.MakeMenu(pawns.Where(x => !WorkManager.Instance.ExcludePawns.Contains(x)), x => x.Name.ToString(), x => () => Find.Root.StartCoroutine(AddPawn(x)));
                }
                Text.Anchor = TextAnchor.UpperLeft;
                cur.y += AddPawnButtonSize;
            }

            _listHeight = cur.y;

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private IEnumerator AddPawn (Pawn pawn)
        {
            yield return new WaitForEndOfFrame();
            WorkManager.Instance.ExcludePawns.Add(pawn);
        }

        private IEnumerator RemovePawn(Pawn pawn)
        {
            yield return new WaitForEndOfFrame();
            WorkManager.Instance.ExcludePawns.Remove(pawn);
        }
    }
}
