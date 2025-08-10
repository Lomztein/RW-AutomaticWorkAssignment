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
                Logger.Message($"[AWA:core] New text: {newFormula}");
                pawnSetting.sourceString = newFormula;
            }
            Rect buttonRect = new Rect(localPosition, new Vector2(width, _buttonSize));
            if (Widgets.ButtonText(buttonRect, "Commit"))
            {
                Logger.Message($"[AWA:core] Commit: {pawnSetting.sourceString}");
                pawnSetting.Commit();
            }
            localPosition.y += _buttonSize;
            if (pawnSetting.InnerFormula != null)
            {
                localPosition.x += 4;
                width -= 4;
                foreach (var bindingName in pawnSetting.InnerFormula.BindingNames)
                {
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
