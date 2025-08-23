﻿using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnFitness
{
    public class FormulaPawnFitnessUIHandler : PawnSettingUIHandler<FormulaPawnFitness>
    {
        protected override float Handle(Vector2 position, float width, FormulaPawnFitness pawnSetting)
        {
            const int inset = 8;
            var layout = new RectAggregator(new Rect(position.x, position.y, width, 0), GetHashCode(), new(8, 1));

            Rect rect = layout.NewRow(AutomaticWorkAssignmentSettings.UIInputSizeBase);
            var newFormula = Widgets.TextField(rect, pawnSetting.SourceString);
            pawnSetting.SourceString = newFormula;

            Rect buttonRect = layout.NewRow(AutomaticWorkAssignmentSettings.UILabelSizeBase);
            if (Widgets.ButtonText(buttonRect, "AWA.CommitSetting".Translate(), active: pawnSetting.LastException == null))
            {
                pawnSetting.Commit();
            }

            if (pawnSetting.LastException != null)
            {
                var curY = layout.Rect.yMax;
                Widgets.LongLabel(layout.Rect.x, layout.Rect.width, pawnSetting.LastException, ref curY);
                layout.NewRow(curY - layout.Rect.yMax);
            }

            if (pawnSetting.InnerFormula != null)
            {
                var bindingsLayout = new RectAggregator(layout.Rect.BottomPart(0).Pad(left: inset), GetHashCode(), new(1, 1));
                for (var i = 0; i < pawnSetting.InnerFormula.BindingNames.Length; i++)
                {
                    if (i > 0)
                    {
                        var lineRect = bindingsLayout.NewRow(9);
                        Widgets.DrawLineHorizontal(lineRect.Rect.x, lineRect.Rect.center.y, width);
                    }

                    var bindingName = pawnSetting.InnerFormula.BindingNames[i];

                    var labelRect = bindingsLayout.NewRow(AutomaticWorkAssignmentSettings.UILabelSizeBase);
                    Widgets.Label(labelRect, bindingName);

                    if (pawnSetting.bindingSettings.TryGetValue(bindingName, out var setting))
                    {
                        WorkManagerWindow.DoPawnSetting(
                            ref bindingsLayout,
                            setting: setting,
                            canMoveUp: false,
                            canMoveDown: false,
                            onMoveSetting: null,
                            onDeleteSetting: (x) => Find.Root.StartCoroutine(DelayedRemoveInnerSetting(bindingName, pawnSetting)));
                    }
                    else
                    {
                        Rect addConditionButtonRect = bindingsLayout.NewRow(AutomaticWorkAssignmentSettings.UIButtonSizeBase);
                        if (Widgets.ButtonText(addConditionButtonRect, "AWA.NestedSettingSelect".Translate()))
                        {
                            FloatMenuUtility.MakeMenu(
                                objects: GetDefs(),
                                labelGetter: def => def.LabelCap,
                                actionGetter: def => () => pawnSetting.bindingSettings.SetOrAdd(bindingName, PawnSetting.CreateFrom<IPawnFitness>(def)));
                        }
                    }
                }
                layout.NewRow(bindingsLayout.Rect.height);
            }

            return layout.Rect.height;
        }

        private IEnumerable<PawnFitnessDef> GetDefs()
            => DefDatabase<PawnFitnessDef>.AllDefs;
        private IEnumerator DelayedRemoveInnerSetting(string name, FormulaPawnFitness setting)
        {
            yield return new WaitForEndOfFrame();
            setting.bindingSettings.Remove(name);
        }
    }
}
