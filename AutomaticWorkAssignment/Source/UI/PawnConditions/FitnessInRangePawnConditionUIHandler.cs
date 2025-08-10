using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using RimWorld;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnConditions
{
    public class FitnessInRangePawnConditionUIHandler : PawnSettingUIHandler<FitnessInRangePawnCondition>
    {
        private float _menuButtonSize = 32;
        private float _labelSize = 24f;
        private float _rangeSelectionLabelSize = 24;
        private float _rangeSelectionInputSize = 24;

        private string _minBuffer;
        private string _maxBuffer;

        protected override float Handle(Vector2 position, float width, FitnessInRangePawnCondition pawnCondition)
        {
            float y = 0f;
            // TODO: Handle case where skill has not been selected properly, either by auto selecting a default, or supporting "Auto";

            position.x += 4;
            width -= 4;

            if (pawnCondition.Fitness != null)
            {
                Vector3 fitnessPosition = position;
                fitnessPosition.y += y;

                Rect labelRect = new Rect(fitnessPosition, new Vector2(width, _labelSize));
                (Rect label, Rect button) = Utils.GetLabeledContentWithFixedLabelSize(labelRect, width - _labelSize);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(label, WorkManagerWindow.GetSettingLabel(pawnCondition.Fitness));
                fitnessPosition.y += _labelSize;
                y += _labelSize;
                Text.Anchor = TextAnchor.UpperLeft;

                if (Widgets.ButtonText(button, "X"))
                {
                    pawnCondition.Fitness = null;
                }
                else
                {
                    y += PawnSettingUIHandlers.Handle(fitnessPosition, width, pawnCondition.Fitness);
                }
            }
            else
            {
                Vector3 buttonPosition = position;
                buttonPosition.y += y;

                Rect menuButtonRect = new Rect(position, new Vector2(width, _menuButtonSize));
                if (Widgets.ButtonText(menuButtonRect, "AWA.FunctionSelect".Translate()))
                {
                    FloatMenuUtility.MakeMenu(DefDatabase<PawnFitnessDef>.AllDefs, x => x.LabelCap, x => () => pawnCondition.Fitness = PawnSetting.CreateFrom<IPawnFitness>(x));
                }

                y += menuButtonRect.height;
            }

            // Min/max labels.
            Rect labelsRect = new Rect(position, new Vector2(width, _rangeSelectionLabelSize));
            labelsRect.y += y;

            Rect minRect = Utils.GetSubRectFraction(labelsRect, Vector2.zero, new Vector2(0.5f, 1f));
            Rect maxRect = Utils.GetSubRectFraction(labelsRect, new Vector2(0.5f, 0f), Vector2.one);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(minRect, "AWA.MinValue".Translate());
            Widgets.Label(maxRect, "AWA.MaxValue".Translate());

            y += labelsRect.height;

            Rect rangeInput = new Rect(position, new Vector2(width, _rangeSelectionInputSize));
            rangeInput.y += y;

            minRect = Utils.GetSubRectFraction(rangeInput, Vector2.zero, new Vector2(0.5f, 1f));
            maxRect = Utils.GetSubRectFraction(rangeInput, new Vector2(0.5f, 0f), Vector2.one);

            _minBuffer = pawnCondition.Min.ToString();
            _maxBuffer = pawnCondition.Max.ToString();

            Widgets.TextFieldNumeric(minRect, ref pawnCondition.Min, ref _minBuffer);
            Widgets.TextFieldNumeric(maxRect, ref pawnCondition.Max, ref _maxBuffer);

            y += rangeInput.height;
            return y;
        }
    }
}
