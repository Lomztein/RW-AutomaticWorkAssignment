using System;
using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.UI
{
    public interface IPawnSettingUIHandler
    {
        Action? GetHelp { get; }
        bool CanHandle(IPawnSetting pawnCondition);

        float Handle(Vector2 position, float width, IPawnSetting pawnCondition);
    }
}
