using UnityEngine;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class ModularPawnSettingUIHandler<T> : PawnSettingUIHandler<T> where T : IPawnSetting
    {
        private IHandlerModule<T>[] _modules;

        public ModularPawnSettingUIHandler(params IHandlerModule<T>[] modules)
            => _modules = modules;

        protected override float Handle(Vector2 position, float width, T pawnSetting)
        {
            float y = 0;
            Vector2 currentPosition = position;
            foreach (var module in _modules)
            {
                float height = module.Handle(currentPosition, width, pawnSetting);
                currentPosition.y += height;
                y += height;
            }
            return y;
        }
    }
}
