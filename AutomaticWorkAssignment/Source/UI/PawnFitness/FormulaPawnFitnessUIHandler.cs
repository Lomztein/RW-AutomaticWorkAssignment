using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
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
        private static readonly float _inputFieldSize = 32;
        private static readonly float _buttonSize = 16;
        private static readonly float _labelSize = 24;

        protected override float Handle(Vector2 position, float width, FormulaPawnFitness pawnSetting)
        {
            var localPosition = position;
            Rect rect = new Rect(localPosition, new Vector2(width, _inputFieldSize));
            var newFormula = Widgets.TextField(rect, pawnSetting.sourceString);
            localPosition.y += rect.height;
            if (newFormula != pawnSetting.sourceString)
            {
                pawnSetting.sourceString = newFormula;
            }
            Rect buttonRect = new Rect(localPosition, new Vector2(width, _buttonSize));
            if (Widgets.ButtonText(buttonRect, "Commit"))
            {
                pawnSetting.Commit();
            }
            localPosition.y += _buttonSize;
            if (pawnSetting.InnerFormula != null)
            {
                localPosition.x += 8;
                width -= 8;
                for (var i = 0; i < pawnSetting.InnerFormula.BindingNames.Length; i++)
                {
                    if (i > 0) {
                        localPosition.y += 4;
                        Widgets.DrawLineHorizontal(localPosition.x, localPosition.y, width);
                        localPosition.y += 5;
                    }

                    var bindingName = pawnSetting.InnerFormula.BindingNames[i];

                    Rect labelRect = new Rect(localPosition, new Vector2(width, _labelSize));
                    localPosition.y += _labelSize;
                    Widgets.Label(labelRect, bindingName);

                    if (pawnSetting.bindingSettings.TryGetValue(bindingName, out var setting))
                    {
                        localPosition.y += WorkManagerWindow.DoPawnSetting(
                            position: localPosition,
                            width: width,
                            setting: setting,
                            onMoveSetting: null,
                            onDeleteSetting: (x) => Find.Root.StartCoroutine(DelayedRemoveInnerSetting(bindingName, pawnSetting)));
                    }
                    else
                    {
                        Rect addConditionButtonRect = new Rect(localPosition, new Vector2(width, _inputFieldSize));
                        localPosition.y += _inputFieldSize;
                        if (Widgets.ButtonText(addConditionButtonRect, "AWA.NestedSettingSelect".Translate()))
                        {
                            FloatMenuUtility.MakeMenu(
                                objects: GetDefs(),
                                labelGetter: def => def.LabelCap,
                                actionGetter: def => () => pawnSetting.bindingSettings.SetOrAdd(bindingName, PawnSetting.CreateFrom<IPawnFitness>(def)));
                        }
                    }
                }
            }
            return localPosition.y - position.y;
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
