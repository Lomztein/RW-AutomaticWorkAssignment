using System;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    public interface IPawnSettingUIHandler
    {
        Action? HelpHandler(IPawnSetting pawnCondition);
        bool CanHandle(IPawnSetting pawnCondition);

        float Handle(Vector2 position, float width, IPawnSetting pawnCondition);
    }
}
