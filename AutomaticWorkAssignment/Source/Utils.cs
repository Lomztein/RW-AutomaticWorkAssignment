using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment
{
    public static class Utils
    {
        public static (Rect labelRect, Rect contentRect) GetLabeledContentWithFixedLabelSize(Rect inRect, int labelSize)
        {
            Rect labelRect = new Rect(inRect);
            labelRect.width = labelSize;
            Rect contentRect = new Rect(inRect);
            contentRect.width = inRect.width - labelRect.width;
            contentRect.x += labelRect.width;
            return (labelRect, contentRect);
        }

        public static Rect GetSubRectFraction(Rect inRect, Vector2 from, Vector2 to)
        {
            Rect newRect = new Rect(inRect);
            newRect.x += inRect.width * from.x;
            newRect.y += inRect.height * from.y;

            Vector2 diff = to - from;
            newRect.width = inRect.width * diff.x;
            newRect.height = inRect.height * diff.y;

            return newRect;
        }

        public static Rect ShrinkByMargin(Rect inRect, float margin)
        {
            Rect newRect = new Rect(inRect);
            newRect.x += margin;
            newRect.y += margin;
            newRect.width -= margin * 2;
            newRect.height -= margin * 2;
            return newRect;
        }

        public static void MoveElement<T>(IList<T> list, T element, int movement)
        {
            int currentIndex = list.IndexOf(element);
            int newIndex = currentIndex + movement;
            list.Remove(element);
            newIndex = Mathf.Clamp(newIndex, 0, list.Count);
            list.Insert(newIndex, element);
        }
    }
}
