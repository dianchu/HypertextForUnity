using System;
using System.Linq;
using AngleSharp.Css.Values;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Color = Unity.UIWidgets.ui.Color;

namespace WidgetFromHtml.Core
{
    internal class CssBorder
    {
        internal readonly bool inherit;
        readonly CssBorderSide _all;
        readonly CssBorderSide _bottom;
        readonly CssBorderSide _inlineEnd;
        readonly CssBorderSide _inlineStart;
        readonly CssBorderSide _left;
        readonly CssBorderSide _right;
        readonly CssBorderSide _top;

        public CssBorder
        (
            CssBorderSide all = null,
            CssBorderSide bottom = null,
            bool inherit = false,
            CssBorderSide inlineEnd = null,
            CssBorderSide inlineStart = null,
            CssBorderSide left = null,
            CssBorderSide right = null,
            CssBorderSide top = null
        )
        {
            _all = all;
            _bottom = bottom;
            this.inherit = inherit;
            _inlineEnd = inlineEnd;
            _inlineStart = inlineStart;
            _left = left;
            _right = right;
            _top = top;
        }

        internal bool isNone =>
            (_all == null || _all == CssBorderSide.none) &&
            (_bottom == null || _bottom == CssBorderSide.none) &&
            (_inlineEnd == null || _inlineEnd == CssBorderSide.none) &&
            (_inlineStart == null || _inlineStart == CssBorderSide.none) &&
            (_left == null || _left == CssBorderSide.none) &&
            (_right == null || _right == CssBorderSide.none) &&
            (_top == null || _top == CssBorderSide.none);


        /// Creates a copy of this border with the sides from [other].
        public CssBorder copyFrom(CssBorder other) => copyWith(
            bottom: other._bottom,
            inlineEnd: other._inlineEnd,
            inlineStart: other._inlineStart,
            left: other._left,
            right: other._right,
            top: other._top
        );

        internal CssBorder copyWith
        (
            CssBorderSide bottom = null,
            CssBorderSide inlineEnd = null,
            CssBorderSide inlineStart = null,
            CssBorderSide left = null,
            CssBorderSide right = null,
            CssBorderSide top = null
        )
        {
            return new CssBorder
            (
                all: _all,
                bottom: CssBorderSide._copyWith(_bottom, bottom),
                inherit: inherit,
                inlineEnd: CssBorderSide._copyWith(_inlineEnd, inlineEnd),
                inlineStart: CssBorderSide._copyWith(_inlineStart, inlineStart),
                left: CssBorderSide._copyWith(_left, left),
                right: CssBorderSide._copyWith(_right, right),
                top: CssBorderSide._copyWith(_top, top)
            );
        }

        internal Border getValue(TextStyleHtml tsh)
        {
            var bottom = CssBorderSide._copyWith(_all, _bottom)?._getValue(tsh);
            var left = CssBorderSide._copyWith(
                    _all,
                    _left ??
                    (tsh.textDirection == TextDirection.ltr
                        ? _inlineStart
                        : _inlineEnd))
                ?._getValue(tsh);
            var right = CssBorderSide._copyWith(
                    _all,
                    _right ??
                    (tsh.textDirection == TextDirection.ltr
                        ? _inlineEnd
                        : _inlineStart))
                ?._getValue(tsh);
            var top = CssBorderSide._copyWith(_all, _top)?._getValue(tsh);
            if (bottom == null && left == null && right == null && top == null)
            {
                return null;
            }

            return new Border(
                bottom: bottom ?? BorderSide.none,
                left: left ?? BorderSide.none,
                right: right ?? BorderSide.none,
                top: top ?? BorderSide.none
            );
        }
    }

    /// <summary>
    /// A side of a border of a box.
    /// </summary>
    internal class CssBorderSide
    {
        /// <summary>
        /// The color of this side of the border.
        /// </summary>
        internal Color color;

        /// <summary>
        /// The style of this side of the border.
        /// </summary>
        internal TextDecorationStyle? style;

        /// <summary>
        /// The width of this side of the border.
        /// </summary>
        internal Length? width;


        public CssBorderSide(Color color, TextDecorationStyle? style, Length? width)
        {
            this.color = color;
            this.style = style;
            this.width = width;
        }

        internal static CssBorderSide none = new CssBorderSide(null, null, null);

        internal BorderSide _getValue(TextStyleHtml tsh)
        {
            return
                ReferenceEquals(this, none)
                    ? null
                    : new BorderSide
                    (
                        color: color ?? tsh.style.color ?? new BorderSide().color,
                        style: style != null ? BorderStyle.solid : BorderStyle.none,
                        width: (float?) (width?.getValue(tsh)) ?? 0.0f
                    );
        }

        internal static CssBorderSide _copyWith(CssBorderSide baseV, CssBorderSide value)
        {
            return
                baseV == null || ReferenceEquals(value, none)
                    ? value
                    : value == null
                        ? baseV
                        : new CssBorderSide(
                            color: value.color ?? baseV.color,
                            style: value.style ?? baseV.style,
                            width: value.width ?? baseV.width
                        );
        }
    }


    /// <summary>
    /// /// A length measurement.
    /// </summary>
    // internal class CssLength
    // {
    //     /// <summary>
    //     /// The measurement number.
    //     /// </summary>
    //     internal readonly double number;
    //
    //     /// <summary>
    //     /// The measurement unit.
    //     /// </summary>
    //     internal readonly CssLengthUnit unit;
    //
    //
    //     /// <summary>
    //     /// Creates a measurement.
    //     /// </summary>
    //     /// <param name="number"></param>
    //     /// <param name="unit"></param>
    //     internal CssLength(double number, CssLengthUnit unit = CssLengthUnit.px)
    //     {
    //         this.number = number;
    //         this.unit = unit;
    //     }
    //
    //     /// <summary>
    //     /// /// Returns `true` if value is larger than zero.
    //     /// </summary>
    //     internal bool isPositive => this.number > 0.0;
    //
    //
    //     internal double? getValue
    //     (
    //         TextStyleHtml tsh,
    //         double? baseValue = null,
    //         double? scaleFactor = null
    //     )
    //     {
    //         double value;
    //         switch (unit)
    //         {
    //             case CssLengthUnit.auto:
    //                 return null;
    //             case CssLengthUnit.em:
    //                 // baseValue ??= tsh.style.fontSize;
    //                 if (baseValue == null)
    //                 {
    //                     baseValue = tsh.style.fontSize;
    //                 }
    //
    //                 if (baseValue == null) return null;
    //                 value = baseValue.Value * number;
    //                 scaleFactor = 1;
    //                 break;
    //             case CssLengthUnit.percentage:
    //                 if (baseValue == null) return null;
    //                 value = baseValue.Value * number / 100;
    //                 scaleFactor = 1;
    //                 break;
    //             case CssLengthUnit.pt:
    //                 value = number * 96 / 72;
    //                 break;
    //             case CssLengthUnit.px:
    //                 value = number;
    //                 break;
    //             default:
    //                 throw new ArgumentOutOfRangeException();
    //         }
    //
    //         if (scaleFactor != null) value *= scaleFactor.Value;
    //
    //         return value;
    //     }
    //
    //     public override string ToString()
    //     {
    //         return $"{number.ToString()}{unit.ToString().Replace("CssLengthUnit.", "")}";
    //     }
    // }
    internal class CssLengthBox
    {
        internal readonly Length? bottom;
        internal readonly Length? _inlineEnd;
        internal readonly Length? _inlineStart;
        internal readonly Length? _left;
        internal readonly Length? _right;
        internal readonly Length? top;

        internal CssLengthBox
        (
            Length? bottom = null,
            Length? top = null,
            Length? inlineEnd = null,
            Length? inlineStart = null,
            Length? left = null,
            Length? right = null
        )
        {
            this.bottom = bottom;
            this.top = top;
            this._inlineEnd = inlineEnd;
            this._inlineStart = inlineStart;
            this._left = left;
            this._right = right;
        }

        /// <summary>
        /// /// Creates a copy with the given measurements replaced with the new values.
        /// </summary>
        /// <param name="bottom"></param>
        /// <param name="inlineEnd"></param>
        /// <param name="inlineStart"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <returns></returns>
        internal CssLengthBox copyWith
        (
            Length? bottom = null,
            Length? inlineEnd = null,
            Length? inlineStart = null,
            Length? left = null,
            Length? right = null,
            Length? top = null
        )
        {
            return new CssLengthBox
            (
                bottom ?? this.bottom,
                top ?? this.top,
                inlineEnd ?? _inlineEnd,
                inlineStart ?? _inlineStart,
                left ?? _left,
                right ?? _right
            );
        }

        /// <summary>
        /// Returns `true` if any of the left, right, inline measurements is set.
        /// </summary>
        internal bool hasPositiveLeftOrRight =>
            _inlineEnd?.isPositive() == true ||
            _inlineStart?.isPositive() == true ||
            _left?.isPositive() == true ||
            _right?.isPositive() == true;

        /// <summary>
        /// Calculates the left value taking text direction into account.
        /// </summary>
        /// <param name="tsh"></param>
        /// <returns></returns>
        internal float? getValueLeft(TextStyleHtml tsh)
        {
            return
            (
                _left ??
                ((tsh.textDirection == TextDirection.ltr ? _inlineStart : _inlineEnd))
            )?.getValue(tsh);
        }


        internal float? getValueRight(TextStyleHtml tsh)
        {
            return
            (
                _right ??
                (tsh.textDirection == TextDirection.ltr ? _inlineEnd : _inlineStart)
            )?.getValue(tsh);
        }

        public override string ToString()
        {
            const string _null = "null";
            var left = (_left ?? _inlineStart)?.ToString() ?? _null;
            var top = this.top?.ToString() ?? _null;
            var right = (_right ?? _inlineEnd)?.ToString() ?? _null;
            var bottom = this.bottom?.ToString() ?? _null;
            if (left == right && right == top && top == bottom)
            {
                return $"CssLengthBox.all({left})";
            }

            //一般也就debug用用,就不考虑gc了.
            var values = new String[] {left, top, right, bottom};

            if (values.Where((v) => v == _null).Count() == 3)
            {
                if (left != _null)
                {
                    if (_left != null)
                    {
                        return $"CssLengthBox(left={_left})";
                    }
                    else
                    {
                        return $"CssLengthBox(inline-start={_inlineStart})";
                    }
                }

                if (top != _null) return $"CssLengthBox(top={top})";
                if (right != _null)
                {
                    if (_right != null)
                    {
                        return $"CssLengthBox(right={_right})";
                    }
                    else
                    {
                        return $"CssLengthBox(inline-end={_inlineEnd})";
                    }
                }

                if (bottom != _null) return $"CssLengthBox(bottom={bottom})";
            }

            return $"CssLengthBox({left}, {top}, {right}, {bottom})";
        }
    }


    /// Length measurement units.
    // enum CssLengthUnit
    // {
    //     /// Special value: auto.
    //     auto,
    //
    //     /// Relative unit: em.
    //     em,
    //
    //     /// Relative unit: percentage.
    //     percentage,
    //
    //     /// Absolute unit: points, 1pt = 1/72th of 1in.
    //     pt,
    //
    //     /// Absolute unit: pixels, 1px = 1/96th of 1in.
    //     px,
    // }


    /// The whitespace behavior.
    // enum CssWhitespace
    // {
    //     /// Sequences of white space are collapsed.
    //     /// Newline characters in the source are handled the same as other white space.
    //     /// Lines are broken as necessary to fill line boxes.
    //     normal,
    //
    //     /// Sequences of white space are preserved.
    //     /// Lines are only broken at newline characters in the source and at <br> elements.
    //     pre,
    // }
}