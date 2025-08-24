using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Source.UI.Windows
{
    public abstract class ListerWindow<T> : Window
    {
        public override Vector2 InitialSize => new Vector2(350, 512);

        private float _listHeight;
        private Vector2 _listPosition;

        private const float SCROLL_BAR_WIDTH = 24;
        private const float HEADER_SIZE = 32;
        public virtual float AddButtonSize => 48;
        public virtual float RowHeight => 32;

        public abstract string Header { get; }
        public abstract string AddButtonLabel { get; }

        public bool AllowAdd => true;
        public bool AllowRemove => true;

        private readonly Texture2D _removeButtonIcon = ContentFinder<Texture2D>.Get("UI/Widgets/CheckOff");

        public abstract IList<T> GetList();

        public override void DoWindowContents(Rect inRect)
        {
            (Rect header, Rect body) = Utils.SplitRectVerticalUpper(inRect, HEADER_SIZE);
            if (Widgets.CloseButtonFor(inRect))
            {
                Close();
            }

            inRect = Utils.GetSubRectFraction(inRect, new Vector2(0f, 0.075f), new Vector2(1f, 1f));

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(header, Header);
            Text.Anchor = TextAnchor.UpperLeft;

            var height = _listHeight;
            var scrollView = new Rect(0f, 0f, inRect.width, height);
            if (height > inRect.height)
                scrollView.width -= SCROLL_BAR_WIDTH;

            Widgets.BeginScrollView(inRect, ref _listPosition, scrollView);
            var scrollContent = scrollView;

            Widgets.BeginGroup(scrollContent);
            var cur = Vector2.zero;
            var i = 0;

            IList<T> list = GetList();
            foreach (T element in list)
            {
                Rect fullRow = new Rect(0, cur.y, inRect.width, RowHeight);
                Rect drawRow;
                if (AllowRemove)
                {
                    (Rect rowRect, Rect removeRect) = Utils.SplitRectHorizontalRight(fullRow, HEADER_SIZE);
                    removeRect = Utils.ShrinkByMargin(removeRect, 4);
                    if (Widgets.ButtonImage(removeRect, _removeButtonIcon))
                    {
                        DoAtEndOfFrame(() => RemoveButtonClicked(element));
                    }
                    drawRow = rowRect;
                }
                else
                {
                    drawRow = fullRow;
                }
                DrawRow(drawRow, element);

                if (i++ % 2 == 1) Widgets.DrawAltRect(fullRow);
                cur.y += fullRow.height;
            }

            if (AllowAdd)
            {
                // row for new pawn.
                var newRect = new Rect(0f, cur.y, inRect.width, AddButtonSize);
                newRect = Utils.ShrinkByMargin(newRect, 8f);

                Text.Anchor = TextAnchor.MiddleCenter;
                WorkTypeDef workDef = new WorkTypeDef();
                if (Widgets.ButtonText(newRect, AddButtonLabel))
                {
                    DoAtEndOfFrame(() => AddButtonClicked());
                }
                Text.Anchor = TextAnchor.UpperLeft;
                cur.y += AddButtonSize;
            }

            _listHeight = cur.y;

            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        private void DoAtEndOfFrame(Action action)
            => Find.Root.StartCoroutine(DoAtEndOfFrameEnumerator(action));

        private IEnumerator DoAtEndOfFrameEnumerator(Action action)
        {
            yield return new WaitForEndOfFrame();
            action();
        }

        protected abstract void DrawRow(Rect inRect, T element);

        protected abstract void AddButtonClicked();

        protected abstract void RemoveButtonClicked(T element);
    }
}
