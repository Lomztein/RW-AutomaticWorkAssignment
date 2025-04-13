using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.UI.PawnFitness;
using RimWorld;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnConditions
{
    public class SkillLevelPawnConditionUIHandler : PawnConditionUIHandler<SkillLevelPawnCondition>
    {
        private float _menuButtonSize = 32;
        private float _rangeSelectionLabelSize = 24;
        private float _rangeSelectionInputSize = 24;

        private string _minBuffer;
        private string _maxBuffer;

        public override float Handle(Vector2 position, float width, IPawnCondition pawnCondition)
        {
            SkillLevelPawnCondition skillLevelCondition = pawnCondition as SkillLevelPawnCondition;
            float y = base.Handle(position, width, pawnCondition);

            Rect menuButtonRect = new Rect(position, new Vector2(width, _menuButtonSize));
            menuButtonRect.y += y;

            // TODO: Handle case where skill has not been selected properly, either by auto selecting a default, or supporting "Auto";
            string label = skillLevelCondition.SkillDef != null ? skillLevelCondition.SkillDef.skillLabel : "Select skill";

            // Skill selection
            if (Widgets.ButtonText(menuButtonRect, label))
            {
                var skillDefs = DefDatabase<SkillDef>.AllDefs;
                FloatMenuUtility.MakeMenu(skillDefs, x => x.skillLabel, x => () => skillLevelCondition.SkillDef = x);
            }
            y += menuButtonRect.height;

            // Min/max labels.
            Rect labelsRect = new Rect(position, new Vector2(width, _rangeSelectionLabelSize));
            labelsRect.y += y;

            Rect minRect = Utils.GetSubRectFraction(labelsRect, Vector2.zero, new Vector2(0.5f, 1f));
            Rect maxRect = Utils.GetSubRectFraction(labelsRect, new Vector2(0.5f, 0f), Vector2.one);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(minRect, "Min");
            Widgets.Label(maxRect, "Max");

            y += labelsRect.height;

            Rect rangeInput = new Rect(position, new Vector2(width, _rangeSelectionInputSize));
            rangeInput.y += y;

            minRect = Utils.GetSubRectFraction(rangeInput, Vector2.zero, new Vector2(0.5f, 1f));
            maxRect = Utils.GetSubRectFraction(rangeInput, new Vector2(0.5f, 0f), Vector2.one);

            _minBuffer = skillLevelCondition.MinLevel.ToString();
            _maxBuffer = skillLevelCondition.MaxLevel.ToString();

            Widgets.TextFieldNumeric(minRect, ref skillLevelCondition.MinLevel, ref _minBuffer);
            Widgets.TextFieldNumeric(maxRect, ref skillLevelCondition.MaxLevel, ref _maxBuffer);

            y += rangeInput.height;
            return y;
        }
    }
}
