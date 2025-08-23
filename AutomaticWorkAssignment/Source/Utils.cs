﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment
{
    public static class Utils
    {
        /// <summary>
        /// A bit hacky, but margin is private for some reason
        /// </summary>
        public static Vector2 GetMargin(this ref RectDivider rect)
        {
            var sourceRect = rect.Rect;
            var row = rect.NewRow(0);
            var col = rect.NewCol(0);
            var margin = new Vector2(rect.Rect.x - sourceRect.x, col.Rect.y - sourceRect.y);
            rect = new RectDivider(sourceRect, rect.GetHashCode(), margin);
            return margin;
        }
        public static RectDivider[] SplitIn(this ref RectDivider rect, int x = 1, int y = 1)
        {
            if (x < 1)
                throw new ArgumentOutOfRangeException(nameof(x), x, "Should be greater or equal to 1");
            if (y < 1)
                throw new ArgumentOutOfRangeException(nameof(y), y, "Should be greater or equal to 1");
            var margin = rect.GetMargin();
            var usableWidth = (rect.Rect.width - (margin.x * (x - 1))) / x;
            var usableHeight = (rect.Rect.height - (margin.y * (y - 1))) / y;
            var outRect = new List<RectDivider>();
            while (y-- > 0)
            {
                var row = rect.NewRow(usableHeight);
                var locX = x;
                while (locX-- > 0)
                {
                    outRect.Add(row.NewCol(usableWidth));
                }
            }
            return outRect.ToArray();
        }
        public static Rect Pad(this Rect inRect, float right = 0, float left = 0, float top = 0, float bottom = 0)
        {
            inRect.x += left;
            inRect.y += top;
            inRect.width -= left + right;
            inRect.height -= top + bottom;
            return inRect;
        }

        public static Rect PadX(this Rect inRect, float x)
        {
            inRect.x += x;
            inRect.width -= x * 2;
            return inRect;
        }

        public static Rect PadY(this Rect inRect, float y)
        {
            inRect.y += y;
            inRect.height -= y * 2;
            return inRect;
        }

        public static Rect PadXY(this Rect inRect, float xy) => inRect.Pad(left: xy, right: xy, top: xy, bottom: xy);
        public static (Rect labelRect, Rect contentRect) GetLabeledContentWithFixedLabelSize(Rect inRect, float labelSize)
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

        public static (Rect left, Rect right) SplitRectHorizontalLeft(Rect inRect, float leftWidth)
        {
            Rect left = new Rect(inRect);
            left.width = leftWidth;
            Rect right = new Rect(inRect);
            right.x += leftWidth;
            right.width = inRect.width - leftWidth;
            return (left, right);
        }

        public static (Rect left, Rect right) SplitRectHorizontalRight(Rect inRect, float rightWidth)
            => SplitRectHorizontalLeft(inRect, inRect.width - rightWidth);

        public static (Rect left, Rect right) SplitRectVerticalUpper(Rect inRect, float height)
        {
            Rect lower = new Rect(inRect);
            lower.height = height;
            Rect upper = new Rect(inRect);
            upper.y += height;
            upper.height = inRect.height - height;
            return (lower, upper);
        }

        public static (Rect upper, Rect lower) SplitRectVerticalLower(Rect inRect, float height)
            => SplitRectVerticalUpper(inRect, inRect.height - height);

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

        // I would very much like an explanation as to why these utility functions are internal.
        private static MethodInfo _unclip = typeof(GUIContent).Assembly.GetType("UnityEngine.GUIClip").GetMethod("Unclip", new[] { typeof(Vector2) });
        public static void RotateAroundPivot(float angle, Vector2 pivotPoint)
        {
            Matrix4x4 matrix = GUI.matrix;
            GUI.matrix = Matrix4x4.identity;
            Vector2 vector = (Vector2)_unclip.Invoke(null, new object[] { pivotPoint });
            vector *= Prefs.UIScale;
            Matrix4x4 matrix4x = Matrix4x4.TRS(vector, Quaternion.Euler(0f, 0f, angle), Vector3.one) * Matrix4x4.TRS(-vector, Quaternion.identity, Vector3.one);
            GUI.matrix = matrix4x * matrix;
        }

        public static bool IsValidAfterLoad(this IPawnSetting setting)
        {
            if (setting == null)
                return false;
            if (setting is PawnSetting pawnSetting && pawnSetting.Def == null)
                return false;
            return true;
        }

        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public static bool HasPassionIn(Pawn pawn, SkillDef skillDef)
        {
            if (pawn != null && pawn.skills != null)
            {
                var skill = pawn.skills.GetSkill(skillDef);
                bool passion = skill.passion != Passion.None;
                float learnRate = skill.LearnRateFactor(true);
                float threshold = AutomaticWorkAssignmentSettings.PassionLearnRateThreshold;
                return passion && learnRate >= threshold;
            }
            return false;
        }
    }
}
