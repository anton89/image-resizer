// https://github.com/tar-bin/VirtualizingWrapPanel/blob/master/VirtualizingWrapPanel/VirtualizingWrapPanel.cs
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;


namespace ImageResizer.CustomControl.Views
{
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        #region OnItemsChanged

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ResetAll();
                    break;
            }
        }

        private void ResetAll()
        {
            _offset.X = 0;
            _offset.Y = 0;
            _extent.Width = 0;
            _extent.Height = 0;
            CanVerticallyScroll = false;
            CanHorizontallyScroll = false;
        }

        #endregion

        #region ItemSize

        #region ItemWidth

        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(
                "ItemWidth",
                typeof(double),
                typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(
                    double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsMeasure
                ),
                IsWidthHeightValid
            );

        [TypeConverter(typeof(LengthConverter))]
        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        #endregion

        #region ItemHeight

        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(
                "ItemHeight",
                typeof(double),
                typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(
                    double.NaN,
                    FrameworkPropertyMetadataOptions.AffectsMeasure
                ),
                IsWidthHeightValid
            );

        [TypeConverter(typeof(LengthConverter))]
        [Category("共通")]
        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        #endregion

        #region IsWidthHeightValid

        private static bool IsWidthHeightValid(object value)
        {
            var d = (double)value;
            return double.IsNaN(d) || d >= 0 && !double.IsPositiveInfinity(d);
        }

        #endregion

        #endregion

        #region Orientation

        public static readonly DependencyProperty OrientationProperty =
            WrapPanel.OrientationProperty.AddOwner(
                typeof(VirtualizingWrapPanel),
                new FrameworkPropertyMetadata(
                    Orientation.Horizontal,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnOrientationChanged
                )
            );

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = d as VirtualizingWrapPanel;
            if (panel == null)
            {
                return;
            }

            panel._offset = default(Point);
            panel.InvalidateMeasure();
        }

        #endregion

        #region MeasureOverride, ArrangeOverride

        private readonly Dictionary<int, Rect> _containerLayouts = new Dictionary<int, Rect>();

        protected override Size MeasureOverride(Size availableSize)
        {
            _containerLayouts.Clear();

            var isAutoWidth = double.IsNaN(ItemWidth);
            var isAutoHeight = double.IsNaN(ItemHeight);
            var childAvailable = new Size(isAutoWidth ? double.PositiveInfinity : ItemWidth,
                isAutoHeight ? double.PositiveInfinity : ItemHeight);
            var isHorizontal = Orientation == Orientation.Horizontal;

            var childrenCount = InternalChildren.Count;

            var itemsControl = ItemsControl.GetItemsOwner(this);
            if (itemsControl != null)
                childrenCount = itemsControl.Items.Count;

            var generator = new ChildGenerator(this);

            var x = 0.0;
            var y = 0.0;
            var lineSize = default(Size);
            var maxSize = default(Size);

            for (var i = 0; i < childrenCount; i++)
            {
                var childSize = ContainerSizeForIndex(i);

                var isWrapped = isHorizontal
                    ? lineSize.Width + childSize.Width > availableSize.Width
                    : lineSize.Height + childSize.Height > availableSize.Height;
                if (isWrapped)
                {
                    x = isHorizontal ? 0 : x + lineSize.Width;
                    y = isHorizontal ? y + lineSize.Height : 0;
                }

                var itemRect = new Rect(x, y, childSize.Width, childSize.Height);
                var viewportRect = new Rect(_offset, availableSize);
                if (itemRect.IntersectsWith(viewportRect))
                {
                    var child = generator.GetOrCreateChild(i);
                    child.Measure(childAvailable);
                    childSize = ContainerSizeForIndex(i);
                }

                _containerLayouts[i] = new Rect(x, y, childSize.Width, childSize.Height);

                isWrapped = isHorizontal
                    ? lineSize.Width + childSize.Width > availableSize.Width
                    : lineSize.Height + childSize.Height > availableSize.Height;
                if (isWrapped)
                {
                    maxSize.Width = isHorizontal
                        ? Math.Max(lineSize.Width, maxSize.Width)
                        : maxSize.Width + lineSize.Width;
                    maxSize.Height = isHorizontal
                        ? maxSize.Height + lineSize.Height
                        : Math.Max(lineSize.Height, maxSize.Height);
                    lineSize = childSize;

                    isWrapped = isHorizontal
                        ? childSize.Width > availableSize.Width
                        : childSize.Height > availableSize.Height;
                    if (isWrapped)
                    {
                        maxSize.Width = isHorizontal
                            ? Math.Max(childSize.Width, maxSize.Width)
                            : maxSize.Width + childSize.Width;
                        maxSize.Height = isHorizontal
                            ? maxSize.Height + childSize.Height
                            : Math.Max(childSize.Height, maxSize.Height);
                        lineSize = default(Size);
                    }
                }
                else
                {
                    lineSize.Width = isHorizontal
                        ? lineSize.Width + childSize.Width
                        : Math.Max(childSize.Width, lineSize.Width);
                    lineSize.Height = isHorizontal
                        ? Math.Max(childSize.Height, lineSize.Height)
                        : lineSize.Height + childSize.Height;
                }

                x = isHorizontal ? lineSize.Width : maxSize.Width;
                y = isHorizontal ? maxSize.Height : lineSize.Height;
            }

            maxSize.Width = isHorizontal ? Math.Max(lineSize.Width, maxSize.Width) : maxSize.Width + lineSize.Width;
            maxSize.Height = isHorizontal
                ? maxSize.Height + lineSize.Height
                : Math.Max(lineSize.Height, maxSize.Height);

            _extent = maxSize;
            _viewport = availableSize;

            generator.CleanupChildren();
            generator.Dispose();

            ScrollOwner?.InvalidateScrollInfo();

            return maxSize;
        }

        #region ChildGenerator

        private class ChildGenerator : IDisposable
        {
            #region CleanupChildren

            public void CleanupChildren()
            {
                if (_generator == null)
                    return;

                var children = _owner.InternalChildren;

                for (var i = children.Count - 1; i >= 0; i--)
                {
                    var childPos = new GeneratorPosition(i, 0);
                    var index = _generator.IndexFromGeneratorPosition(childPos);
                    if (index >= _firstGeneratedIndex && index <= _lastGeneratedIndex)
                    {
                        continue;
                    }

                    _generator.Remove(childPos, 1);
                    _owner.RemoveInternalChildRange(i, 1);
                }
            }

            #endregion

            #region fields

            private readonly VirtualizingWrapPanel _owner;

            private readonly IItemContainerGenerator _generator;

            private IDisposable _generatorTracker;

            private int _firstGeneratedIndex;

            private int _lastGeneratedIndex;

            private int _currentGenerateIndex;

            #endregion

            #region _ctor

            public ChildGenerator(VirtualizingWrapPanel owner)
            {
                this._owner = owner;

                var childrenCount = owner.InternalChildren.Count;
                _generator = owner.ItemContainerGenerator;
            }

            ~ChildGenerator()
            {
                Dispose();
            }

            public void Dispose()
            {
                _generatorTracker?.Dispose();
            }

            #endregion

            #region GetOrCreateChild

            private void BeginGenerate(int index)
            {
                _firstGeneratedIndex = index;
                var startPos = _generator.GeneratorPositionFromIndex(index);
                _currentGenerateIndex = startPos.Offset == 0 ? startPos.Index : startPos.Index + 1;
                _generatorTracker = _generator.StartAt(startPos, GeneratorDirection.Forward, true);
            }

            public UIElement GetOrCreateChild(int index)
            {
                if (_generator == null)
                    return _owner.InternalChildren[index];

                if (_generatorTracker == null)
                    BeginGenerate(index);

                bool newlyRealized;
                var child = _generator.GenerateNext(out newlyRealized) as UIElement;
                if (newlyRealized)
                {
                    if (_currentGenerateIndex >= _owner.InternalChildren.Count)
                    {
                        if (child != null)
                        {
                            _owner.AddInternalChild(child);
                        }
                    }
                    else if (child != null)
                    {
                        _owner.InsertInternalChild(_currentGenerateIndex, child);
                    }

                    _generator.PrepareItemContainer(child);
                }

                _lastGeneratedIndex = index;
                _currentGenerateIndex++;

                return child;
            }

            #endregion
        }

        #endregion

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                var gen = ItemContainerGenerator as ItemContainerGenerator;
                var index = gen?.IndexFromContainer(child) ?? InternalChildren.IndexOf(child);
                if (!_containerLayouts.ContainsKey(index))
                {
                    continue;
                }

                var layout = _containerLayouts[index];
                layout.Offset(_offset.X * -1, _offset.Y * -1);
                child.Arrange(layout);
            }

            return finalSize;
        }

        #endregion

        #region ContainerSizeForIndex

        private Size _prevSize = new Size(16, 16);

        private Size ContainerSizeForIndex(int index)
        {
            var getSize = new Func<int, Size>(idx =>
            {
                UIElement item = null;
                var itemsOwner = ItemsControl.GetItemsOwner(this);
                var generator = ItemContainerGenerator as ItemContainerGenerator;

                if (itemsOwner == null || generator == null)
                {
                    if (InternalChildren.Count > idx)
                        item = InternalChildren[idx];
                }
                else
                {
                    if (generator.ContainerFromIndex(idx) != null)
                        item = generator.ContainerFromIndex(idx) as UIElement;
                    else if (itemsOwner.Items.Count > idx)
                        item = itemsOwner.Items[idx] as UIElement;
                }

                if (item != null)
                {
                    if (item.IsMeasureValid)
                        return item.DesiredSize;

                    if (item is FrameworkElement i)
                        return new Size(i.Width, i.Height);
                }

                if (_containerLayouts.ContainsKey(idx))
                    return _containerLayouts[idx].Size;

                return _prevSize;
            });

            var size = getSize(index);

            if (!double.IsNaN(ItemWidth))
                size.Width = ItemWidth;
            if (!double.IsNaN(ItemHeight))
                size.Height = ItemHeight;

            return _prevSize = size;
        }

        #endregion

        #region IScrollInfo Members

        #region Extent

        private Size _extent = default(Size);

        public double ExtentHeight => _extent.Height;

        public double ExtentWidth => _extent.Width;

        #endregion Extent

        #region Viewport

        private Size _viewport = default(Size);

        public double ViewportHeight => _viewport.Height;

        public double ViewportWidth => _viewport.Width;

        #endregion

        #region Offset

        private Point _offset;

        public double HorizontalOffset => _offset.X;

        public double VerticalOffset => _offset.Y;

        #endregion

        #region ScrollOwner

        public ScrollViewer ScrollOwner { get; set; }

        #endregion

        #region CanHorizontallyScroll

        public bool CanHorizontallyScroll { get; set; }

        #endregion

        #region CanVerticallyScroll

        public bool CanVerticallyScroll { get; set; }

        #endregion

        #region LineUp

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - SystemParameters.ScrollHeight);
        }

        #endregion

        #region LineDown

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + SystemParameters.ScrollHeight);
        }

        #endregion

        #region LineLeft

        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - SystemParameters.ScrollWidth);
        }

        #endregion

        #region LineRight

        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + SystemParameters.ScrollWidth);
        }

        #endregion

        #region PageUp

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - _viewport.Height);
        }

        #endregion

        #region PageDown

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + _viewport.Height);
        }

        #endregion

        #region PageLeft

        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - _viewport.Width);
        }

        #endregion

        #region PageRight

        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + _viewport.Width);
        }

        #endregion

        #region MouseWheelUp

        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - SystemParameters.ScrollHeight * SystemParameters.WheelScrollLines * 5);
        }

        #endregion

        #region MouseWheelDown

        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + SystemParameters.ScrollHeight * SystemParameters.WheelScrollLines * 5);
        }

        #endregion

        #region MouseWheelLeft

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - SystemParameters.ScrollWidth * SystemParameters.WheelScrollLines * 5);
        }

        #endregion

        #region MouseWheelRight

        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + SystemParameters.ScrollWidth * SystemParameters.WheelScrollLines * 5);
        }

        #endregion

        #region MakeVisible

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var idx = InternalChildren.IndexOf(visual as UIElement);

            var generator = ItemContainerGenerator;
            if (generator != null)
            {
                var pos = new GeneratorPosition(idx, 0);
                idx = generator.IndexFromGeneratorPosition(pos);
            }

            if (idx < 0)
                return Rect.Empty;

            if (!_containerLayouts.ContainsKey(idx))
                return Rect.Empty;

            var layout = _containerLayouts[idx];

            if (HorizontalOffset + ViewportWidth < layout.X + layout.Width)
                SetHorizontalOffset(layout.X + layout.Width - ViewportWidth);
            if (layout.X < HorizontalOffset)
                SetHorizontalOffset(layout.X);

            if (VerticalOffset + ViewportHeight < layout.Y + layout.Height)
                SetVerticalOffset(layout.Y + layout.Height - ViewportHeight);
            if (layout.Y < VerticalOffset)
                SetVerticalOffset(layout.Y);

            layout.Width = Math.Min(ViewportWidth, layout.Width);
            layout.Height = Math.Min(ViewportHeight, layout.Height);

            return layout;
        }

        #endregion

        #region SetHorizontalOffset

        public void SetHorizontalOffset(double offset)
        {
            if (offset < 0 || ViewportWidth >= ExtentWidth)
            {
                offset = 0;
            }
            else
            {
                if (offset + ViewportWidth >= ExtentWidth)
                    offset = ExtentWidth - ViewportWidth;
            }

            _offset.X = offset;

            ScrollOwner?.InvalidateScrollInfo();

            InvalidateMeasure();
        }

        #endregion

        #region SetVerticalOffset

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || ViewportHeight >= ExtentHeight)
            {
                offset = 0;
            }
            else
            {
                if (offset + ViewportHeight >= ExtentHeight)
                    offset = ExtentHeight - ViewportHeight;
            }

            _offset.Y = offset;

            ScrollOwner?.InvalidateScrollInfo();

            InvalidateMeasure();
        }

        #endregion

        #endregion
    }
}
