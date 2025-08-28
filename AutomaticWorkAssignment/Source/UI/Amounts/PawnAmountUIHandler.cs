using Lomzie.AutomaticWorkAssignment.Amounts;
using System;
using UnityEngine;
using static Verse.AI.ThingCountTracker;

namespace Lomzie.AutomaticWorkAssignment.UI.Amounts
{
    public abstract class PawnAmountUIHandler<T> : IPawnAmountUIHandler where T : IPawnAmount
    {
        public virtual Action? HelpHandler(IPawnAmount setting) => null;

        public virtual bool CanHandle(IPawnAmount pawnAmount)
            => typeof(T).IsInstanceOfType(pawnAmount);

        public void Handle(Rect inRect, IPawnAmount pawnAmount) =>
            Handle(inRect, (T)pawnAmount);

        protected abstract void Handle(Rect inRect, T pawnAmount);
    }
}
