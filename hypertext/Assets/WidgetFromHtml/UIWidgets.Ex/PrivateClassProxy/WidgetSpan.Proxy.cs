using System;
using System.Collections.Generic;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace Unity.UIWidgets.widgets.proxyFromHtmlCore
{
    internal class WidgetSpan : PlaceholderSpan, IEquatable<WidgetSpan>
    {
        public WidgetSpan(
            Widget child,
            TextBaseline baseline = TextBaseline.alphabetic,
            TextStyle style = null,
            PlaceholderAlignment alignment = PlaceholderAlignment.bottom
        ) : base(
            alignment: alignment,
            baseline: baseline,
            style: style
        )
        {
            _widgetSpan = new widgets.WidgetSpan(
                child,
                baseline: baseline,
                style: style,
                alignment: alignment
            );
        }

        public readonly Unity.UIWidgets.widgets.WidgetSpan _widgetSpan;

        public override void build(ParagraphBuilder builder, float textScaleFactor = 1,
            List<PlaceholderDimensions> dimensions = null)
        {
            _widgetSpan.build(builder, textScaleFactor, dimensions);
        }

        public override bool visitChildren(InlineSpanVisitor visitor)
        {
            return _widgetSpan.visitChildren(visitor);
        }

        protected override InlineSpan getSpanForPositionVisitor(TextPosition position, Accumulator offset)
        {
            if (position.offset == offset.value)
            {
                return this;
            }

            offset.increment(1);
            return null;
        }

        protected override int? codeUnitAtVisitor(int index, Accumulator offset)
        {
            return null;
        }

        public override RenderComparison compareTo(InlineSpan other)
        {
            return _widgetSpan.compareTo(other);
        }

        public override int GetHashCode()
        {
            return _widgetSpan.GetHashCode();
        }

        public bool DebugAssertIsValid()
        {
            return true;
        }

        public bool Equals(WidgetSpan other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this._widgetSpan.Equals(other._widgetSpan);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((WidgetSpan) obj);
        }

        public static bool operator ==(WidgetSpan left, WidgetSpan right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(WidgetSpan left, WidgetSpan right)
        {
            return !Equals(left, right);
        }

        public override InlineSpan getSpanForPosition(TextPosition position)
        {
            return _widgetSpan.getSpanForPosition(position);
        }

        public override bool debugAssertIsValid()
        {
            return _widgetSpan.debugAssertIsValid();
        }

        public static implicit operator Unity.UIWidgets.widgets.WidgetSpan(WidgetSpan ws)
        {
            return ws._widgetSpan;
        }

        // public static explicit operator Unity.UIWidgets.widgets.WidgetSpan(WidgetSpan ws)
        // {
        //     return ws._widgetSpan;
        // }
    }
}