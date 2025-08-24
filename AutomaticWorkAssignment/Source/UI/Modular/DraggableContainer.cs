using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class DraggableContainer<TItem>
    {
        public delegate Rect RenderItem(TItem item, Rect container, int index);
        private readonly RenderItem _renderItem;
        private readonly Vector2 _direction;
        private Vector2 _scroll;
        private Vector2 _prevSize;

        public DraggableContainer(RenderItem renderItem, Vector2 direction)
        {
            Logger.Message($"Build {nameof(DraggableContainer<TItem>)}");
            direction = direction * -1;
            _renderItem = renderItem;
            if (direction != Vector2.down && direction != Vector2.up && direction != Vector2.left && direction != Vector2.right)
            {
                throw new ArgumentException("Should be a unit vector", nameof(direction));
            }
            _direction = direction;
        }

        public Rect Render(Rect container, IEnumerable<TItem> items)
        {
            var scrollbarSizes = new Vector2(
                _prevSize.y > container.height ? GUI.skin.verticalScrollbar.fixedWidth + 1 : 0,
                _prevSize.x > container.width ? GUI.skin.horizontalScrollbar.fixedHeight + 1 : 0
            );
            Rect innerScrollRect = new(Vector2.zero, _prevSize - scrollbarSizes);
            Vector2 itemSizeConstraint = _direction switch
            {
                { x: var x } when x != 0 => new Vector2(float.MaxValue, container.height - scrollbarSizes.y),
                { y: var y } when y != 0 => new Vector2(container.width - scrollbarSizes.x, float.MaxValue),
                _ => throw new ArgumentException("Should be a unit vector", nameof(_direction))
            };
            Widgets.BeginScrollView(container, ref _scroll, innerScrollRect);
            Widgets.BeginGroup(innerScrollRect);
            Vector2 currentPosition = innerScrollRect.position;
            foreach (var (item, index) in items.Select((item, index) => (item, index)))
            {
                var renderedRect = _renderItem(item, new Rect(currentPosition, itemSizeConstraint), index);
                currentPosition += renderedRect.size * _direction;
            }
            Widgets.EndGroup();
            Widgets.EndScrollView();
            _prevSize = _direction switch
            {
                { x: var x } when x != 0 => new Vector2(currentPosition.x, container.height),
                { y: var y } when y != 0 => new Vector2(container.width, currentPosition.y),
                _ => throw new ArgumentException("Should be a unit vector", nameof(_direction))
            };
            return container;
        }
    }
}
