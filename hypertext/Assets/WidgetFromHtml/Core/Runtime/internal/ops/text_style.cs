using System;
using System.Collections.Generic;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Values;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Color = Unity.UIWidgets.ui.Color;
using FontStyle = Unity.UIWidgets.ui.FontStyle;
using FontWeight = Unity.UIWidgets.ui.FontWeight;
using TextDecorationStyle = Unity.UIWidgets.ui.TextDecorationStyle;

namespace WidgetFromHtml.Core
{
    internal class TextStyleOps
    {
        public static TextStyleHtml color(TextStyleHtml p, Color color) =>
            p.copyWith(style: p.style.copyWith(color: color));

        public static TextStyleHtml fontFamily(TextStyleHtml p, List<String> list) =>
            p.copyWith
            (
                style: p.style.copyWith
                (
                    fontFamily: list.isNotEmpty() ? list.first() : null,
                    // fontFamilyFallback: list.skip(1).ToList() 
                    fontFamilyFallback: list.skip_toList(1)
                )
            );

        public static TextStyleHtml fontSize(TextStyleHtml p, ICssValue v) =>
            p.copyWith(style: p.style.copyWith(fontSize: _fontSizeTryParse(p, v)));

        public static TextStyleHtml fontSizeEm(TextStyleHtml p, float v) => p.copyWith
        (
            style: p.style.copyWith
            (
                fontSize: _fontSizeTryParseCssLength(p, new Length(v, Length.Unit.Em))
            )
        );

        public static TextStyleHtml fontSizeTerm(TextStyleHtml p, String v) => p.copyWith
        (
            style: p.style.copyWith(fontSize: _fontSizeTryParseTerm(p, v))
        );


        public static TextStyleHtml fontStyle(TextStyleHtml p, FontStyle fontStyle) =>
            p.copyWith(style: p.style.copyWith(fontStyle: fontStyle));

        public static TextStyleHtml fontWeight(TextStyleHtml p, FontWeight v) =>
            p.copyWith(style: p.style.copyWith(fontWeight: v));

        /// <summary>
        /// 返回一个委托!
        /// </summary>
        /// <param name="wf"></param>
        /// <returns></returns>
        public static Func<TextStyleHtml, ICssValue, TextStyleHtml> lineHeight(WidgetFactory wf)
        {
            return (p, v) => p.copyWith(height: _lineHeightTryParse(wf, p, v));
        }

        public static TextStyleHtml maxLines(TextStyleHtml p, int v) =>
            p.copyWith(maxLines: v);

        public static int? maxLinesTryParse(ICssValue expression)
        {
            // if (expression is css.LiteralTerm)
            if (expression.IsLiteralTerm())
            {
                // if (expression is css.NumberTerm)
                if (expression.IsNumberTerm())
                {
                    return expression.number().ceil();
                }

                // switch (expression.valueAsString)
                switch (expression.CssText)
                {
                    case Const.kCssMaxLinesNone:
                        return -1;
                }
            }

            return null;
        }

        public static TextStyleHtml textDeco(TextStyleHtml p, TextDeco v)
        {
            var pd = p.style.decoration;
            var lineThough = pd?.contains(TextDecoration.lineThrough) == true;
            var overline = pd?.contains(TextDecoration.overline) == true;
            var underline = pd?.contains(TextDecoration.underline) == true;

            //TODO 声明周期可控 可以池化!
            var list = new List<TextDecoration>(3);
            if (v.over == true || (overline && v.over != false))
            {
                list.Add(TextDecoration.overline);
            }

            if (v.strike == true || (lineThough && v.strike != false))
            {
                list.Add(TextDecoration.lineThrough);
            }

            if (v.under == true || (underline && v.under != false))
            {
                list.Add(TextDecoration.underline);
            }

            return p.copyWith(
                style: p.style.copyWith
                (
                    decoration: Helper.TextDecoration_combine(list),
                    decorationColor: v.color,
                    decorationStyle: v.style,
                    decorationThickness: v.thickness?.getValue(p)
                )
            );
        }

        public static TextStyleHtml textDirection(TextStyleHtml p, string v)
        {
            switch (v)
            {
                case Const.kCssDirectionLtr:
                    return p.copyWith(textDirection: TextDirection.ltr);
                case Const.kCssDirectionRtl:
                    return p.copyWith(textDirection: TextDirection.rtl);
            }

            return p;
        }

        public static TextStyleHtml textOverflow(TextStyleHtml p, TextOverflow v) =>
            p.copyWith(textOverflow: v);

        public static TextOverflow? textOverflowTryParse(string value)
        {
            switch (value)
            {
                case Const.kCssTextOverflowClip:
                    return TextOverflow.clip;
                case Const.kCssTextOverflowEllipsis:
                    return TextOverflow.ellipsis;
            }

            return null;
        }

        public static List<string> fontFamilyTryParse(List<ICssValue> expressions)
        {
            var list = new List<string>();

            foreach (var expression in expressions)
            {
                if (expression.IsLiteralTerm())
                {
                    var fontFamily = expression.CssText;
                    if (fontFamily.isNotEmpty()) list.Add(fontFamily);
                }
            }

            return list;
        }

        public static FontStyle? fontStyleTryParse(string value)
        {
            switch (value)
            {
                case Const.kCssFontStyleItalic:
                    return FontStyle.italic;
                case Const.kCssFontStyleNormal:
                    return FontStyle.normal;
            }

            return null;
        }


        public static FontWeight fontWeightTryParse(ICssValue expression)
        {
            if (expression.IsLiteralTerm())
            {
                // if (expression is css.NumberTerm)
                if (expression.IsNumberTerm())
                {
                    switch ((int)expression.AsDouble())
                    {
                        case 100:
                            return FontWeight.w100;
                        case 200:
                            return FontWeight.w200;
                        case 300:
                            return FontWeight.w300;
                        case 400:
                            return FontWeight.w400;
                        case 500:
                            return FontWeight.w500;
                        case 600:
                            return FontWeight.w600;
                        case 700:
                            return FontWeight.w700;
                        case 800:
                            return FontWeight.w800;
                        case 900:
                            return FontWeight.w900;
                    }
                }

                switch (expression.CssText)
                {
                    case Const.kCssFontWeightBold:
                        return FontWeight.bold;
                }
            }

            return null;
        }

        public static TextStyleHtml whitespace(TextStyleHtml p, Whitespace v) =>
            p.copyWith(whitespace: v);

        public static Whitespace? whitespaceTryParse(string value)
        {
            switch (value)
            {
                case Const.kCssWhitespacePre:
                    return Whitespace.Pre;
                case Const.kCssWhitespaceNormal:
                    return Whitespace.Normal;
            }

            return null;
        }

        static float? _fontSizeTryParse(TextStyleHtml p, ICssValue v)
        {
            Length? length = null;
            if (v is Length len)
            {
                length = len;
            }

            if (length != null)
            {
                var lengthValue = _fontSizeTryParseCssLength(p, length.Value);
                if (lengthValue != null) return lengthValue;
            }

            if (v is ICssSpecialValue specialValue)
            {
                return (float?)_fontSizeTryParseTerm(p, specialValue.CssText);
            }

            return null;
        }

        static float? _fontSizeTryParseCssLength(TextStyleHtml p, Length v) =>
            v.getValue
            (
                tsh: p,
                baseValue: p.parent?.style.fontSize,
                scaleFactor: p.getDependency<MediaQueryData>().textScaleFactor
            );

        static float? _fontSizeTryParseTerm(TextStyleHtml p, string v)
        {
            switch (v)
            {
                case Const.kCssFontSizeXxLarge:
                    return _fontSizeMultiplyRootWith(p, 2.0f);
                case Const.kCssFontSizeXLarge:
                    return _fontSizeMultiplyRootWith(p, 1.5f);
                case Const.kCssFontSizeLarge:
                    return _fontSizeMultiplyRootWith(p, 1.125f);
                case Const.kCssFontSizeMedium:
                    return _fontSizeMultiplyRootWith(p, 1f);
                case Const.kCssFontSizeSmall:
                    return _fontSizeMultiplyRootWith(p, .8125f);
                case Const.kCssFontSizeXSmall:
                    return _fontSizeMultiplyRootWith(p, .625f);
                case Const.kCssFontSizeXxSmall:
                    return _fontSizeMultiplyRootWith(p, .5625f);
                case Const.kCssFontSizeLarger:
                    return _fontSizeMultiplyWith(p.parent?.style.fontSize, 1.2f);
                case Const.kCssFontSizeSmaller:
                    return _fontSizeMultiplyWith(p.parent?.style.fontSize, 15f / 18f);
            }

            return null;
        }

        static float? _fontSizeMultiplyRootWith(TextStyleHtml tsh, float value)
        {
            var root = tsh;
            while (root.parent != null)
            {
                root = root.parent;
            }

            return _fontSizeMultiplyWith(root.style.fontSize, value);
        }

        static float? _fontSizeMultiplyWith(float? fontSize, float value) =>
            fontSize != null ? fontSize * value : null;

        static float? _lineHeightTryParse
        (
            WidgetFactory wf,
            TextStyleHtml p,
            ICssValue v
        )
        {
            if (v is ICssSpecialValue)
            {
                // if (v is css.NumberTerm)

                var number = v.AsDouble();
                if (number > 0) return (float)number;
                switch (v.CssText)
                {
                    case Const.kCssLineHeightNormal:
                        return -1f;
                }
            }

            var fontSize = p.style.fontSize;
            if (fontSize == null) return null;

            var length = core_parser.tryParseCssLength(v);
            if (length == null) return null;

            var lengthValue = length.Value.getValue
            (
                tsh: p,
                baseValue: fontSize,
                scaleFactor: p.getDependency<MediaQueryData>().textScaleFactor
            );
            if (lengthValue == null) return null;

            return lengthValue / fontSize;
        }
    }

    class TextDeco
    {
        public Color color;
        public bool? over;
        public bool? strike;
        public TextDecorationStyle? style;
        public Length? thickness;
        public bool? under;

        public TextDeco
        (
            Color color = null,
            bool? over = null,
            bool? strike = null,
            TextDecorationStyle? style = null,
            Length? thickness = null,
            bool? under = null
        )
        {
            this.color = color;
            this.over = over;
            this.strike = strike;
            this.style = style;
            this.thickness = thickness;
            this.under = under;
        }


        public static TextDeco tryParse(List<ICssValue> expressions)
        {
            foreach (var property in expressions)
            {
                var expression = property;
                // if (expression is !css.LiteralTerm) //文字
                // if (!(expression is ICssSpecialValue)) //比如 red black 这种常量型的cssValue
                if (!expression.IsLiteralTerm()) //比如 red black 这种常量型的cssValue
                {
                    continue;
                }

                switch (expression.CssText)
                {
                    case Const.kCssTextDecorationLineThrough:
                        return new TextDeco(strike: true);
                    case Const.kCssTextDecorationNone:
                        return new TextDeco(over: false, strike: false, under: false);
                    case Const.kCssTextDecorationOverline:
                        return new TextDeco(over: true);
                    case Const.kCssTextDecorationUnderline:
                        return new TextDeco(under: true);
                }
            }

            return null;
        }
    }
}