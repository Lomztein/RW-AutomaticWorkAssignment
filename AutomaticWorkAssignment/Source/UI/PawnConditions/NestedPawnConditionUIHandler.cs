using Lomzie.AutomaticWorkAssignment.Defs;
using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.UI.PawnFitness;
using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AutomaticWorkAssignment.Source.UI.PawnConditions
{
    public class NestedPawnConditionUIHandler : PawnConditionUIHandler<NestedPawnCondition>
    {
        private readonly float _addConditionButtonSize = 32;
        private readonly float _deleteInternalConditionSize = 24;

        public override float Handle(Vector2 position, float width, IPawnCondition pawnCondition)
        {
            NestedPawnCondition nested = pawnCondition as NestedPawnCondition;

            float y = base.Handle(position, width, pawnCondition);
            position.y += y;
            
            Vector2 innerPosition = position;

            innerPosition.x += 4;
            float innerWidth = width - 8;

            if (nested.InnerCondition != null)
            {
                Rect deleteButtonRect = new Rect(innerPosition.x + innerWidth - _deleteInternalConditionSize, position.y, _deleteInternalConditionSize, _deleteInternalConditionSize);
                if (Widgets.ButtonText(deleteButtonRect, "X"))
                {
                    nested.InnerCondition = null;
                }

                y += PawnConditionUIHandlers.Handle(innerPosition, innerWidth, nested.InnerCondition);
            }
            else
            {
                Rect addConditionButtonRect = new Rect(innerPosition, new Vector2(innerWidth, _addConditionButtonSize));
                if (Widgets.ButtonText(addConditionButtonRect, "Set condition"))
                {
                    FloatMenuUtility.MakeMenu(DefDatabase<PawnConditionDef>.AllDefs, x => x.label, x => () => nested.InnerCondition = (IPawnCondition)Activator.CreateInstance(x.defClass));
                }

                y += addConditionButtonRect.height;
            }

            return y;
        }
    }
}
