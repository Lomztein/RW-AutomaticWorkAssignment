using Lomzie.AutomaticWorkAssignment.PawnConditions;
using RimWorld;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnConditions
{
    public class SkillLevelPawnConditionUIHandler : PawnSettingUIHandler<SkillLevelPawnCondition>
    {
        private float _menuButtonSize = 32;
        private float _rangeSelectionLabelSize = 24;
        private float _rangeSelectionInputSize = 24;

        private string _minBuffer;
        private string _maxBuffer;

        protected override float Handle(Vector2 position, float width, SkillLevelPawnCondition pawnCondition)
        {
            float y = 0f;
            Rect menuButtonRect = new Rect(position, new Vector2(width, _menuButtonSize));

            // TODO: Handle case where skill has not been selected properly, either by auto selecting a default, or supporting "Auto";
            string label = pawnCondition.SkillDef != null ? pawnCondition.SkillDef.LabelCap : "AWA.SkillSelect".Translate();

            // Skill selection
            if (Widgets.ButtonText(menuButtonRect, label))
            {
                var skillDefs = DefDatabase<SkillDef>.AllDefs;
                FloatMenuUtility.MakeMenu(skillDefs, x => x.skillLabel, x => () => pawnCondition.SkillDef = x);
            }
            y += menuButtonRect.height;

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

            _minBuffer = pawnCondition.MinLevel.ToString();
            _maxBuffer = pawnCondition.MaxLevel.ToString();

            Widgets.TextFieldNumeric(minRect, ref pawnCondition.MinLevel, ref _minBuffer);
            Widgets.TextFieldNumeric(maxRect, ref pawnCondition.MaxLevel, ref _maxBuffer);

            y += rangeInput.height;
            return y;
        }
    }
}
