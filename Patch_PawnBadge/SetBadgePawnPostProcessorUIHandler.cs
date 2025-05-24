using Lomzie.AutomaticWorkAssignment.UI;
using RimWorld;
using RR_PawnBadge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.Patches.PawnBadge
{
    public class SetBadgePawnPostProcessorUIHandler : PawnSettingUIHandler<SetBadgePawnPostProcessor>
    {
        private float _buttonSize = 32;

        protected override float Handle(Vector2 position, float width, SetBadgePawnPostProcessor pawnSetting)
        {
            Rect positionButtonRect = new Rect(position, new Vector2(width, _buttonSize));
            if (Widgets.ButtonText(positionButtonRect, pawnSetting.Position == 0 ? "AWA.PawnBadge.BadgeOne".Translate() : "AWA.PawnBadge.BadgeTwo".Translate()))
                pawnSetting.Position = pawnSetting.Position == 0 ? 1 : 0;

            Rect pickerButtonRect = new Rect(positionButtonRect);
            pickerButtonRect.y += _buttonSize;
            if (Widgets.ButtonText(pickerButtonRect, pawnSetting.BadgeDef?.LabelCap ?? "AWA.PawnBadge.None".Translate()))
            {
                IEnumerable<FloatMenuOption> options = GetFloatMenuOptions(pawnSetting);
                Find.WindowStack.Add(new FloatMenu(options.ToList()));
            }

            return _buttonSize * 2f;
        }

        private IEnumerable<FloatMenuOption> GetFloatMenuOptions (SetBadgePawnPostProcessor pawnSetting)
        {
            yield return new FloatMenuOption("AWA.PawnBadge.None".Translate(), () => pawnSetting.BadgeDef = null);
            var defs = DefDatabase<BadgeDef>.AllDefs;
            foreach (var def in defs)
            {
                yield return MakeOption(def, pawnSetting);
            }
        }

        private FloatMenuOption MakeOption(BadgeDef def, SetBadgePawnPostProcessor pawnSetting)
        {
            return new FloatMenuOption(def.defName, () => pawnSetting.BadgeDef = def, def.Symbol, Color.white);
        }
    }
}
