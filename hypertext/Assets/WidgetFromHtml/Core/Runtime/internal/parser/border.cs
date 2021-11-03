using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AngleSharp.Common;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Values;
using AngleSharp.Dom;
using Unity.UIWidgets.foundation;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextDecorationStyle = Unity.UIWidgets.ui.TextDecorationStyle;

namespace WidgetFromHtml.Core
{
    internal static partial class core_parser
    {
        static readonly ConditionalWeakTable<IElement, CssBorder> _elementBorder
            = new ConditionalWeakTable<IElement, CssBorder>();


        internal static CssBorder tryParseBorder(AbsBuildMetadata meta)
        {
            _elementBorder.TryGetValue(meta.element, out var existing);
            if (existing != null) return existing;
            var border = new CssBorder(all:new CssBorderSide(color:Color.black,style:TextDecorationStyle.solid,width: Length.Half));
            // Debug.Log("暂时屏蔽border的解析...");
            // foreach (var style in meta.styles)
            // {
            //     // var key = style.property;
            //     var key = style.Name;
            //     if (!key.StartsWith(Const.kCssBorder)) continue;
            //     
            //     Debug.Log($"border 属性name={key}");
            //     var suffix = key.substring(Const.kCssBorder.Length);
            //     // if (suffix.isEmpty && style.term == Const.kCssBorderInherit)
            //     if (string.IsNullOrEmpty(suffix) && style.Value == Const.kCssBorderInherit)
            //     {
            //         border = new CssBorder(inherit: true);
            //         continue;
            //     }
            //     var borderSide = _tryParseBorderSide(style.values());
            //     if (string.IsNullOrEmpty(suffix))
            //     {
            //         border = new CssBorder(all: borderSide);
            //     }
            //     else
            //     {
            //         switch (suffix)
            //         {
            //             case Const.kSuffixBottom:
            //             case Const.kSuffixBlockEnd:
            //                 border = border.copyWith(bottom: borderSide);
            //                 break;
            //             case Const.kSuffixInlineEnd:
            //                 border = border.copyWith(inlineEnd: borderSide);
            //                 break;
            //             case Const.kSuffixInlineStart:
            //                 border = border.copyWith(inlineStart: borderSide);
            //                 break;
            //             case Const.kSuffixLeft:
            //                 border = border.copyWith(left: borderSide);
            //                 break;
            //             case Const.kSuffixRight:
            //                 border = border.copyWith(right: borderSide);
            //                 break;
            //             case Const.kSuffixTop:
            //             case Const.kSuffixBlockStart:
            //                 border = border.copyWith(top: borderSide);
            //                 break;
            //         }
            //     }
            // }

            _elementBorder.Add(meta.element, border);
            return border;
        }


        static CssBorderSide _tryParseBorderSide(List<ICssValue> expressions)
        {
            var width = expressions.isNotEmpty() ? tryParseCssLength(expressions[0]) : null;
            if (width == null || width.Value.number() <= 0) return CssBorderSide.none;

            return new CssBorderSide(
                color: expressions.Count >= 3 ? tryParseColor(expressions[2]) : null,
                style: expressions.Count >= 2
                    ? _tryParseTextDecorationStyle(expressions[1])
                    : null,
                width: width
            );
        }


        static TextDecorationStyle? _tryParseTextDecorationStyle(ICssValue expression)
        {
            // var value = expression is css.LiteralTerm ? expression.valueAsString : null;
            var value = expression.CssText;
            switch (value)
            {
                case Const.kCssBorderStyleDotted:
                    return TextDecorationStyle.dotted;
                case Const.kCssBorderStyleDashed:
                    return TextDecorationStyle.dashed;
                case Const.kCssBorderStyleDouble:
                    return TextDecorationStyle.doubleLine;
                case Const.kCssBorderStyleSolid:
                    return TextDecorationStyle.solid;
            }

            return null;
        }
    }
}