using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnConditions
{
    public class CompareFitnessPawnConditionUIHandler : PawnSettingUIHandler<CompareFitnessPawnCondition>
    {
        private readonly float _labelSize = 24;
        private readonly float _buttonSize = 32;
        private Dictionary<CompareFitnessPawnCondition.Comparsion, string> _iconMap = new Dictionary<CompareFitnessPawnCondition.Comparsion, string>()
        {
            { CompareFitnessPawnCondition.Comparsion.Equals, "=" },
            { CompareFitnessPawnCondition.Comparsion.NotEquals, "!=" },
            { CompareFitnessPawnCondition.Comparsion.LessThan, "<" },
            { CompareFitnessPawnCondition.Comparsion.LessThanOrEqual, "<=" },
            { CompareFitnessPawnCondition.Comparsion.GreaterThan, ">" },
            { CompareFitnessPawnCondition.Comparsion.GreaterThanOrEqual, ">=" }
        };

        protected override float Handle(Vector2 position, float width, CompareFitnessPawnCondition pawnSetting)
        {
            float y = 0f;
            if (pawnSetting.InnerSettings[0] != null)
            {
                float height = DrawOperand(position, width, pawnSetting.InnerSettings[0], 0, pawnSetting);
                position.y += height;
                y += height;
            }
            else
            {
                float height = DrawAddOperandButton(position, width, "AWA.LeftOperandSet".Translate(), 0, pawnSetting);
                position.y += height;
                y += height;
            }

            Rect buttonRect = new Rect(position, new Vector2(width, _buttonSize));
            if (Widgets.ButtonText(buttonRect, _iconMap[pawnSetting.ComparisonType]))
            {
                Find.WindowStack.Add(new FloatMenu(GetFloatMenuOptions(pawnSetting).ToList()));
            }

            position.y += _buttonSize;
            y += _buttonSize;

            if (pawnSetting.InnerSettings[1] != null)
            {
                float height = DrawOperand(position, width, pawnSetting.InnerSettings[1], 1, pawnSetting);
                position.y += height;
                y += height;
            }
            else
            {
                float height = DrawAddOperandButton(position, width, "AWA.RightOperandSet".Translate(), 1, pawnSetting);
                position.y += height;
                y += height;
            }

            return y;
        }

        private float DrawAddOperandButton(Vector2 position, float width, string label, int operandIndex, CompareFitnessPawnCondition pawnSetting)
        {
            Rect rect = new Rect(position, new Vector2(width, _buttonSize));
            if (Widgets.ButtonText(rect, label))
            {
                Utils.MakeMenuForSettingDefs(PawnFitnessDef.GetSorted(), () => x => pawnSetting.InnerSettings[operandIndex] = PawnSetting.CreateFrom<IPawnFitness>(x));
            }
            return _buttonSize;
        }

        private float DrawOperand(Vector2 position, float width, IPawnFitness operand, int operandIndex, CompareFitnessPawnCondition pawnSetting)
        {
            float y = 0f;
            position.x += 4;
            width -= 4;

            Rect labelRect = new Rect(position, new Vector2(width, _labelSize));
            (Rect label, Rect button) = Utils.GetLabeledContentWithFixedLabelSize(labelRect, width - _labelSize);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(label, WorkManagerWindow.GetSettingLabel(operand));
            position.y += _labelSize;
            y += _labelSize;
            Text.Anchor = TextAnchor.UpperLeft;

            if (Widgets.ButtonText(button, "X"))
            {
                pawnSetting.InnerSettings[operandIndex] = null;
            }
            else
            {
                y += PawnSettingUIHandlers.Handle(position, width, operand);
            }

            return y;
        }

        private void SetComparison(CompareFitnessPawnCondition condition, CompareFitnessPawnCondition.Comparsion comparison)
        {
            condition.ComparisonType = comparison;
        }

        private IEnumerable<FloatMenuOption> GetFloatMenuOptions(CompareFitnessPawnCondition condition)
        {
            yield return GetOption(condition, CompareFitnessPawnCondition.Comparsion.Equals);
            yield return GetOption(condition, CompareFitnessPawnCondition.Comparsion.NotEquals);
            yield return GetOption(condition, CompareFitnessPawnCondition.Comparsion.LessThan);
            yield return GetOption(condition, CompareFitnessPawnCondition.Comparsion.LessThanOrEqual);
            yield return GetOption(condition, CompareFitnessPawnCondition.Comparsion.GreaterThan);
            yield return GetOption(condition, CompareFitnessPawnCondition.Comparsion.GreaterThanOrEqual);
        }

        private FloatMenuOption GetOption(CompareFitnessPawnCondition condition, CompareFitnessPawnCondition.Comparsion comparison)
            => new FloatMenuOption(_iconMap[comparison], () => SetComparison(condition, comparison));
    }
}
