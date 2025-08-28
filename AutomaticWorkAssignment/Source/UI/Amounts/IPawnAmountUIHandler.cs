using Lomzie.AutomaticWorkAssignment.Amounts;
using System;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.UI.Amounts
{
    public interface IPawnAmountUIHandler
    {
        Action? HelpHandler(IPawnAmount pawnAmount);
        bool CanHandle(IPawnAmount pawnAmount);

        void Handle(Rect inRect, IPawnAmount pawnAmount);
    }
}
