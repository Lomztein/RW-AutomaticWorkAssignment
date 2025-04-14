using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.GenericPawnSettings;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.UI.Generic;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static RimWorld.PsychicRitualRoleDef;

namespace Lomzie.AutomaticWorkAssignment.UI.PawnConditions
{
    public class CompareFitnessPawnConditionUIHandler : CompositePawnSettingsUIHandler<CompareFitnessPawnCondition, PawnFitnessDef>
    {
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

        public CompareFitnessPawnConditionUIHandler(string newSettingLabel, bool allowMoveSetting) : base(newSettingLabel, allowMoveSetting)
        {
        }

        protected override float Handle(Vector2 position, float width, CompositePawnSetting pawnSetting)
        {
            Rect buttonRect = new Rect(position, new Vector2(width, _buttonSize));
            CompareFitnessPawnCondition compareCondition = pawnSetting as CompareFitnessPawnCondition;
            if (Widgets.ButtonText(buttonRect, _iconMap[compareCondition.ComparisonType]))
            {
                Find.WindowStack.Add(new FloatMenu(GetFloatMenuOptions(compareCondition).ToList()));
            }
            position.y += _buttonSize;
            return base.Handle(position, width, pawnSetting) + _buttonSize;
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
