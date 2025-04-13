using Lomzie.AutomaticWorkAssignment.PawnConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    public interface IPawnSettingUIHandler
    {
        bool CanHandle(IPawnSetting pawnCondition);

        float Handle(Vector2 position, float width, IPawnSetting pawnCondition);
    }
}
