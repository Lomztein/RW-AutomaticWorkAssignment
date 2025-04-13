using Lomzie.AutomaticWorkAssignment.PawnConditions;
using Lomzie.AutomaticWorkAssignment.PawnFitness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    public abstract class PawnSettingUIHandler<T> : IPawnSettingUIHandler where T : IPawnSetting
    {
        public virtual bool CanHandle(IPawnSetting pawnSetting)
            => typeof(T).IsInstanceOfType(pawnSetting);

        public float Handle(Vector2 position, float width, IPawnSetting pawnSetting) =>
            Handle(position, width, (T)pawnSetting);

        protected abstract float Handle(Vector2 position, float width, T pawnSetting);
    }
}
