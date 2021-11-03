using System.Collections.Generic;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Values;

namespace WidgetFromHtml.Core
{
    internal static partial class core_parser
    {
        public static Length? tryParseCssLength(ICssValue expression)
        {
            if (expression is Constant<Length> length)
            {
                return length.Value;
            }
            else if (expression is Length length2)
            {
                return length2;
            }

            HLog.LogInfo($"TODO core_parser tryParseCssLength error value={expression.CssText}");
            // return Length.Thick;
            return null;
        }

        static CssLengthBox _parseCssLengthBoxOne
        (
            CssLengthBox existing,
            string suffix,
            ICssValue expression
        )
        {
            var parsed = tryParseCssLength(expression);
            if (parsed == null) return existing;

            if (existing == null)
            {
                existing = new CssLengthBox();
            }

            switch (suffix)
            {
                case Const.kSuffixBottom:
                case Const.kSuffixBlockEnd:
                    return existing.copyWith(bottom: parsed);
                case Const.kSuffixInlineEnd:
                    return existing.copyWith(inlineEnd: parsed);
                case Const.kSuffixInlineStart:
                    return existing.copyWith(inlineStart: parsed);
                case Const.kSuffixLeft:
                    return existing.copyWith(left: parsed);
                case Const.kSuffixRight:
                    return existing.copyWith(right: parsed);
                case Const.kSuffixTop:
                case Const.kSuffixBlockStart:
                    return existing.copyWith(top: parsed);
            }

            return existing;
        }


        static CssLengthBox _parseCssLengthBoxAll(List<ICssValue> expressions)
        {
            switch (expressions.Count)
            {
                case 4:
                    return new CssLengthBox(
                        top: tryParseCssLength(expressions[0]),
                        inlineEnd: tryParseCssLength(expressions[1]),
                        bottom: tryParseCssLength(expressions[2]),
                        inlineStart: tryParseCssLength(expressions[3])
                    );
                case 2:
                    var topBottom = tryParseCssLength(expressions[0]);
                    var leftRight = tryParseCssLength(expressions[1]);
                    return new CssLengthBox(
                        top: topBottom,
                        inlineEnd: leftRight,
                        bottom: topBottom,
                        inlineStart: leftRight
                    );
                case 1:
                    var all = tryParseCssLength(expressions[0]);
                    return new CssLengthBox(
                        top: all,
                        inlineEnd: all,
                        bottom: all,
                        inlineStart: all
                    );
            }

            return null;
        }


        public static CssLengthBox tryParseCssLengthBox(AbsBuildMetadata meta, string prefix)
        {
            CssLengthBox output = null;

            foreach (var style in meta.styles)
            {
                var key = style.Name;
                if (!key.StartsWith(prefix)) continue;

                var suffix = key.substring(prefix.Length);
                if (suffix.isEmpty())
                {
                    output = _parseCssLengthBoxAll(style.values());
                }
                else
                {
                    var expression = style.RawValue;
                    if (expression != null)
                    {
                        output = _parseCssLengthBoxOne(output, suffix, expression);
                    }
                }
            }

            return output;
        }
    }
}