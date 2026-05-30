using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Lomzie.AutomaticWorkAssignment.UI.Modular
{
    public class DraggableContainer<TItem> where TItem : class
    {
        public delegate Rect RenderItem(TItem item, Rect container, bool selected, int index);
        public delegate void MoveItem(int from, int to);

        private readonly RenderItem _renderItem;
        private readonly MoveItem _moveItem;

        private readonly Vector2 _direction;
        private Vector2 _scroll;
        private Vector2 _prevSize;
        private TItem? _dragging;

        private int _reorderableGroupID;

        public DraggableContainer(RenderItem renderItem, MoveItem moveItem, Vector2 direction)
        {
            if (direction.y != 0) // Invert vertical direction because of how GUILayout handles vertical layout (top to bottom instead of bottom to top)
                direction *= -1;

            _renderItem = renderItem;
            _moveItem = moveItem;
            if (direction != Vector2.down && direction != Vector2.up && direction != Vector2.left && direction != Vector2.right)
            {
                throw new ArgumentException("Should be a unit vector", nameof(direction));
            }
            _direction = direction;
        }

        public Rect Render(Rect container, List<TItem> items)
        {
            // Calculate sizes of containers/scroll area
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

            if (Event.current.type == EventType.Repaint)
            {
                _reorderableGroupID = ReorderableWidget.NewGroup(_moveItem.Invoke, _direction switch
                {
                    { x: var x } when x != 0 => ReorderableDirection.Horizontal,
                    { y: var y } when y != 0 => ReorderableDirection.Vertical,
                    _ => throw new ArgumentException("Should be a unit vector", nameof(_direction))
                }, container);
            }

            #region Scrollable content
            Widgets.BeginScrollView(container, ref _scroll, innerScrollRect);
            Widgets.BeginGroup(innerScrollRect);
            Vector2 currentPosition = innerScrollRect.position;

            foreach (var (item, index) in items.Select((item, index) => (item, index)))
            {
                var renderedRect = _renderItem(item, new Rect(currentPosition, itemSizeConstraint), item == _dragging, index);
                currentPosition += renderedRect.size * _direction;

                // Draggable behavior
                if (Event.current.rawType == EventType.MouseDown && Mouse.IsOver(renderedRect))
                {
                    _dragging = item;
                }

                // Null elements are not draggable, so that they can be used to represent invalid/empty slots.
                if (item != null && ReorderableWidget.Reorderable(_reorderableGroupID, renderedRect))
                {
                    Widgets.DrawRectFast(renderedRect, Widgets.WindowBGFillColor * new Color(1f, 1f, 1f, 0.5f));
                }
            }
            Widgets.EndGroup();
            Widgets.EndScrollView();
            #endregion

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
