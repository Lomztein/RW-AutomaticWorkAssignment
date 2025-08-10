using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public interface IHandlerModule<T> where T : IPawnSetting
    {
        float Handle(Vector2 position, float width, T pawnSetting);
    }
}
