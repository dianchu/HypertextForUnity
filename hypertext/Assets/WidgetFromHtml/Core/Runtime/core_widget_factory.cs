using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Css.Dom;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
// using static Unity.UIWidgets.ui.Color;
using Color = Unity.UIWidgets.ui.Color;
using FontStyle = Unity.UIWidgets.ui.FontStyle;
using FontWeight = Unity.UIWidgets.ui.FontWeight;
using TextDecorationStyle = Unity.UIWidgets.ui.TextDecorationStyle;

namespace WidgetFromHtml.Core
{
    /// <summary>
    /// 构建widgets的工厂类
    /// </summary>
    internal class WidgetFactory : IDisposable
    {
        private readonly Dictionary<string, GlobalKey> _anchors = new Dictionary<string, GlobalKey>();
        private readonly Flattener _flattener = new Flattener();

        BuildOp _styleBgColor;
        BuildOp _styleBlock;
        BuildOp _styleBorder;
        BuildOp _styleDisplayNone;
        BuildOp _styleMargin;
        BuildOp _stylePadding;
        BuildOp _styleSizing;
        BuildOp _styleTextDecoration;
        BuildOp _styleVerticalAlign;

        BuildOp _tagA;

        // TextStyleHtml Function(TextStyleHtml, NULL)? _tagAColor;
        Func<TextStyleHtml, object, TextStyleHtml> _tagAColor;

        BuildOp _tagBr;
        BuildOp _tagFont;
        BuildOp _tagHr;
        BuildOp _tagImg;
        BuildOp _tagPre;

        BuildOp _tagQ;

        // TextStyleHtml Function(TextStyleHtml, css.Expression)? _tsbLineHeight;
        Func<TextStyleHtml, ICssValue, TextStyleHtml> _tsbLineHeight;

        HtmlWidget _widget;


        public void parseStyle(BuildMetadata meta, ICssProperty style)
        {
            var key = style.Name;
            switch (key)
            {
                case Const.kCssBackground:
                case Const.kCssBackgroundColor:
                    if (_styleBgColor == null)
                    {
                        _styleBgColor = new StyleBgColor(this).buildOp;
                    }

                    meta.register(_styleBgColor);
                    break;

                case Const.kCssColor:
                    var color = core_parser.tryParseColor(style.RawValue);
                    if (color != null) meta.tsb.enqueue(TextStyleOps.color, color);
                    break;

                case Const.kCssDirection:
                    meta.tsb.enqueue(TextStyleOps.textDirection, style.Value);
                    break;

                case Const.kCssFontFamily:
                    var list = TextStyleOps.fontFamilyTryParse(style.values());
                    meta.tsb.enqueue(TextStyleOps.fontFamily, list);
                    break;

                case Const.kCssFontSize:
                    meta.tsb.enqueue(TextStyleOps.fontSize, style.RawValue);
                    break;

                case Const.kCssFontStyle:
                    // var term = style.term;
                    var termtermFontStyle = style.Value;
                    var fontStyle =
                        termtermFontStyle != null ? TextStyleOps.fontStyleTryParse(termtermFontStyle) : null;
                    if (fontStyle != null)
                    {
                        meta.tsb.enqueue(TextStyleOps.fontStyle, fontStyle.Value);
                    }

                    break;

                case Const.kCssFontWeight:
                    // var value = style.value;
                    var value = style.Value;
                    var fontWeight =
                        value != null ? TextStyleOps.fontWeightTryParse(style.RawValue) : null;
                    if (fontWeight != null)
                    {
                        meta.tsb.enqueue(TextStyleOps.fontWeight, fontWeight);
                    }

                    break;

                case Const.kCssHeight:
                case Const.kCssMaxHeight:
                case Const.kCssMaxWidth:
                case Const.kCssMinHeight:
                case Const.kCssMinWidth:
                case Const.kCssWidth:
                    if (_styleSizing == null)
                    {
                        _styleSizing = new StyleSizing(this).buildOp;
                    }

                    // _styleSizing ??= StyleSizing(this).buildOp;
                    meta.register(_styleSizing);
                    break;

                case Const.kCssLineHeight:
                    if (_tsbLineHeight == null)
                    {
                        _tsbLineHeight = TextStyleOps.lineHeight(this);
                    }

                    meta.tsb.enqueue(_tsbLineHeight, style.RawValue);
                    break;

                case Const.kCssMaxLines:
                case Const.kCssMaxLinesWebkitLineClamp:
                    var maxLines = TextStyleOps.maxLinesTryParse(style.RawValue);
                    if (maxLines != null) meta.tsb.enqueue(TextStyleOps.maxLines, maxLines.Value);
                    break;

                case Const.kCssTextAlign:
                    // var termTextAlign = style.term;
                    var termTextAlign = style.Value;
                    if (termTextAlign != null)
                    {
                        meta.register(new StyleTextAlign(this, termTextAlign).op);
                    }

                    break;

                case Const.kCssTextDecoration:
                    if (_styleTextDecoration == null)
                    {
                        _styleTextDecoration = new BuildOp
                        (
                            onTree: (metaFromCallBack, _) =>
                            {
                                foreach (var styleLoop in metaFromCallBack.styles)
                                {
                                    if (styleLoop.Name == Const.kCssTextDecoration)
                                    {
                                        var textDeco = TextDeco.tryParse(styleLoop.values());
                                        if (textDeco != null)
                                        {
                                            metaFromCallBack.tsb.enqueue(TextStyleOps.textDeco, textDeco);
                                        }
                                    }
                                }
                            }
                        );
                    }

                    meta.register(_styleTextDecoration);
                    break;
                case Const.kCssTextOverflow:
                    // var termTextOverflow = style.term;
                    var termTextOverflow = style.Value;
                    var textOverflow =
                        termTextOverflow != null ? TextStyleOps.textOverflowTryParse(termTextOverflow) : null;
                    if (textOverflow != null)
                    {
                        meta.tsb.enqueue(TextStyleOps.textOverflow, textOverflow.Value);
                    }

                    break;

                case Const.kCssVerticalAlign:
                    if (_styleVerticalAlign == null)
                    {
                        _styleVerticalAlign = new StyleVerticalAlign(this).buildOp;
                    }

                    meta.register(_styleVerticalAlign);
                    break;

                case Const.kCssWhitespace:
                    // var termWhitespace = style.term;
                    var termWhitespace = style.Value;
                    var whitespace =
                        termWhitespace != null ? TextStyleOps.whitespaceTryParse(termWhitespace) : null;
                    if (whitespace != null)
                    {
                        meta.tsb.enqueue(TextStyleOps.whitespace, whitespace.Value);
                    }

                    break;
            }

            if (key.StartsWith(Const.kCssBorder))
            {
                if (_styleBorder == null)
                {
                    _styleBorder = new StyleBorder(this).buildOp;
                }

                // _styleBorder =  _styleBorder ?? 
                meta.register(_styleBorder);
            }

            if (key.StartsWith(Const.kCssMargin))
            {
                if (_styleMargin == null)
                {
                    _styleMargin = new StyleMargin(this).buildOp;
                }

                meta.register(_styleMargin);
            }

            if (key.StartsWith(Const.kCssPadding))
            {
                if (_stylePadding == null)
                {
                    _stylePadding = new StylePadding(this).buildOp;
                }

                meta.register(_stylePadding);
            }
        }


        /// Parses display inline style.
        public void parseStyleDisplay(BuildMetadata meta, string value)
        {
            switch (value)
            {
                case Const.kCssDisplayBlock:
                    // _styleBlock ??= DisplayBlockOp(this);
                    if (_styleBlock == null)
                    {
                        _styleBlock = new DisplayBlockOp(this);
                    }

                    meta.register(_styleBlock);
                    break;
                case Const.kCssDisplayNone:
                    _styleDisplayNone = _styleDisplayNone ?? new BuildOp
                    (
                        onTree: (_, tree) =>
                        {
                            // foreach (var bit in tree.bits.toList())
                            foreach (var bit in tree.bits_toList())
                            {
                                bit.detach();
                            }
                        },
                        priority: 0
                    );
                    meta.register(_styleDisplayNone);
                    break;
                case Const.kCssDisplayTable:
                    meta.register(new TagTable(this, meta).op);
                    break;
            }
        }


        /// <summary>
        /// Parses [meta] for build ops and text styles.
        /// </summary>
        /// <param name="meta"></param>
        internal void parse(AbsBuildMetadata meta)
        {
            // var attrs = meta.element.attributes;
            var attrs = meta.element.Attributes;

            switch (meta.element.LocalName)
            {
                case Const.kTagA:
                    // _tagA ??= TagA(this).buildOp;
                    if (_tagA == null)
                    {
                        _tagA = new TagA(this).buildOp;
                    }

                    meta.register(_tagA);
                    if (_tagAColor == null)
                    {
                        _tagAColor = (tsh, _) => tsh.copyWith
                        (
                            style: tsh.style.copyWith
                            (
                                color: _widget?.hyperlinkColor ??
                                       tsh.getDependency<ThemeData>().accentColor
                            )
                        );
                    }

                    //TODO 这边加入了一个null
                    meta.tsb.enqueue(_tagAColor, null);

                    var name = attrs[Const.kAttributeAName];
                    if (name != null) meta.register(_anchorOp(name.Name));
                    break;

                case "abbr":
                case "acronym":
                    meta.tsb.enqueue(
                        TextStyleOps.textDeco,
                        new TextDeco(style: TextDecorationStyle.dotted, under: true)
                    );
                    break;

                case "address":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.tsb.enqueue(TextStyleOps.fontStyle, FontStyle.italic);
                    // meta
                    //     ..[Const.kCssDisplay] =  kCssDisplayBlock
                    //     ..tsb.enqueue(TextStyleOps.fontStyle, FontStyle.italic);
                    break;

                case "article":
                case "aside":
                case "div":
                case "figcaption":
                case "footer":
                case "header":
                case "main":
                case "nav":
                case "section":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    break;

                case "blockquote":
                case "figure":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssMargin, "1em 40px");
                    // meta
                    //     ..[kCssDisplay] =
                    // kCssDisplayBlock
                    //     ..[kCssMargin] =
                    // "1em 40px";
                    break;

                case "b":
                case "strong":
                    meta.tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    break;

                case "big":
                    meta.tsb.enqueue(TextStyleOps.fontSizeTerm, Const.kCssFontSizeLarger);
                    break;
                case "small":
                    meta.tsb.enqueue(TextStyleOps.fontSizeTerm, Const.kCssFontSizeSmaller);
                    break;

                case "br":
                    if (_tagBr == null)
                    {
                        _tagBr = new BuildOp(onTree: (_, tree) => tree.addNewLine());
                    }

                    // _tagBr ??= BuildOp(onTree: (_, tree) => tree.addNewLine());
                    meta.register(_tagBr);
                    break;

                case Const.kTagCenter:
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssTextAlign, Const.kCssTextAlignWebkitCenter);
                    // meta
                    //     ..[kCssDisplay] =
                    // kCssDisplayBlock
                    //     ..[kCssTextAlign] =
                    // kCssTextAlignWebkitCenter;
                    break;

                case "cite":
                case "dfn":
                case "em":
                case "i":
                case "var":
                    meta.tsb.enqueue(TextStyleOps.fontStyle, FontStyle.italic);
                    break;

                case Const.kTagCode:
                case Const.kTagKbd:
                case Const.kTagSamp:
                case Const.kTagTt:
                    meta.tsb.enqueue
                    (
                        // TextStyleOps.fontFamily,  const [kTagCodeFont1, kTagCodeFont2])
                        TextStyleOps.fontFamily, new List<string>(2) {Const.kTagCodeFont1, Const.kTagCodeFont2}
                    );
                    break;
                case Const.kTagPre:
                    if (_tagPre == null)
                    {
                        _tagPre = new BuildOp
                        (
                            onWidgets: (metaFromCallBack, widgets) => Helper.listOrNull
                            (
                                buildColumnPlaceholder(metaFromCallBack, widgets)
                                    ?.wrapWith
                                    (
                                        (_, w) => buildHorizontalScrollView(metaFromCallBack, w)
                                    )
                            )
                        );
                    }

                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssWhitespace, Const.kCssWhitespacePre);
                    meta.tsb.enqueue
                    (
                        TextStyleOps.fontFamily,
                        new List<string>(2) {Const.kTagCodeFont1, Const.kTagCodeFont2}
                    );
                    meta.register(_tagPre);
                    // meta
                    //     ..[Const.kCssDisplay] =
                    // kCssDisplayBlock
                    //     ..[Const.kCssWhitespace] =
                    // Const.kCssWhitespacePre
                    // ..tsb.enqueue(
                    //     TextStyleOps.fontFamily,  const [Const.kTagCodeFont1, Const.kTagCodeFont2])
                    // ..register(_tagPre!);
                    break;

                case "dd":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssMargin, "0 0 1em 40px");
                    // meta
                    // ..[kCssDisplay] =
                    // kCssDisplayBlock
                    // ..[kCssMargin] =
                    // "0 0 1em 40px";
                    break;
                case "dl":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    // meta[kCssDisplay] = kCssDisplayBlock;
                    break;
                case "dt":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    // meta
                    //     ..[kCssDisplay] =
                    // kCssDisplayBlock
                    //     ..tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    break;

                case "del":
                case "s":
                case "strike":
                    meta.tsb.enqueue(TextStyleOps.textDeco, new TextDeco(strike: true));
                    break;

                case Const.kTagFont:
                    if (_tagFont == null)
                    {
                        _tagFont = new BuildOp(
                            defaultStyles: (elementFromCallBack) =>
                            {
                                var attrsFromCallBack = elementFromCallBack.Attributes;
                                var color = attrs[Const.kAttributeFontColor];
                                var fontFace = attrs[Const.kAttributeFontFace];
                                var key = "";
                                var nameItem = attrs.GetNamedItem(Const.kAttributeFontSize);
                                if (nameItem != null)
                                {
                                    key = nameItem.Name;
                                }

                                //等价于final fontSize = kCssFontSizes[attrs[kAttributeFontSize] ?? ''];
                                var fontSize = Const.kCssFontSizes[key];
                                var retDic = new Dictionary<string, string>(3);
                                if (color != null)
                                    retDic.Add(Const.kCssColor, color.Value);
                                if (fontFace != null)
                                    retDic.Add(Const.kCssFontFamily, fontFace.Value);
                                if (fontSize != null)
                                    retDic.Add(Const.kCssFontSize, fontSize);
                                return retDic;

                                // return {
                                //     if (color != null)
                                //         kCssColor:
                                //     color,
                                //     if (fontFace != null)
                                //         kCssFontFamily:
                                //     fontFace,
                                //     if (fontSize != null)
                                //         kCssFontSize:
                                //     fontSize,
                                // }
                                // ;
                            }
                        );
                    }


                    meta.register(_tagFont);
                    break;

                case "hr":
                    if (_tagHr == null)
                    {
                        _tagHr = new BuildOp
                        (
                            onWidgets: (metaFromCallBack, _) => Helper.listOrNull(buildDivider(metaFromCallBack))
                        );
                    }

                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssMargin + Const.kSuffixBottom, "1em");
                    meta.register(_tagHr);
                    // meta
                    // ..[kCssDisplay] =
                    // kCssDisplayBlock
                    // ..[kCssMargin
                    // +kSuffixBottom] = "1em"
                    // ..register(_tagHr!);
                    break;
                case "h1":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssMargin, "0.67em 0");
                    meta.tsb.enqueue(TextStyleOps.fontSizeEm, 2.0f);
                    meta.tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    // meta
                    // ..[kCssDisplay] =
                    // kCssDisplayBlock
                    // ..[kCssMargin] =
                    // "0.67em 0"
                    // ..tsb.enqueue(TextStyleOps.fontSizeEm, 2.0)
                    // ..tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    break;
                case "h2":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssMargin, "0.83em 0");
                    meta.tsb.enqueue(TextStyleOps.fontSizeEm, 1.5f);
                    meta.tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    // meta
                    // ..[kCssDisplay] =
                    // kCssDisplayBlock
                    // ..[kCssMargin] =
                    // "0.83em 0"
                    // ..tsb.enqueue(TextStyleOps.fontSizeEm, 1.5)
                    // ..tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    break;
                case "h3":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssMargin, "1em 0");
                    meta.tsb.enqueue(TextStyleOps.fontSizeEm, 1.17f);
                    meta.tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    // meta
                    //     ..[kCssDisplay] =
                    // kCssDisplayBlock
                    //     ..[kCssMargin] =
                    // "1em 0"
                    //         ..tsb.enqueue(TextStyleOps.fontSizeEm, 1.17)
                    //     ..tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    break;
                case "h4":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssMargin, "1.33em 0");
                    meta.tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    // meta
                    //     ..[kCssDisplay] =
                    // kCssDisplayBlock
                    //     ..[kCssMargin] =
                    // "1.33em 0"
                    //     ..tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    break;
                case "h5":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssMargin, "1.67em 0");
                    meta.tsb.enqueue(TextStyleOps.fontSizeEm, 0.83f);
                    meta.tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    // meta
                    //     ..[kCssDisplay] =
                    // kCssDisplayBlock
                    //     ..[kCssMargin] =
                    // "1.67em 0"
                    //         ..tsb.enqueue(TextStyleOps.fontSizeEm, .83)
                    //     ..tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    break;
                case "h6":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssMargin, "2.33em 0");
                    meta.tsb.enqueue(TextStyleOps.fontSizeEm, 0.67f);
                    meta.tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    // meta
                    //     ..[kCssDisplay] =
                    // kCssDisplayBlock
                    //     ..[kCssMargin] =
                    // "2.33em 0"
                    //         ..tsb.enqueue(TextStyleOps.fontSizeEm, .67)
                    //     ..tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    break;

                case Const.kTagImg:
                    if (_tagImg == null)
                    {
                        _tagImg = new TagImg(this).buildOp;
                    }

                    meta.register(_tagImg);
                    break;

                case "ins":
                case "u":
                    meta.tsb.enqueue(TextStyleOps.textDeco, new TextDeco(under: true));
                    break;

                case Const.kTagOrderedList:
                case Const.kTagUnorderedList:
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.register(new TagLi(this, meta).op);
                    // meta
                    //     ..[kCssDisplay] =
                    // kCssDisplayBlock
                    // ..register(TagLi(this, meta).op);
                    break;

                case "mark":
                    meta.AddOneCssPropertyStyle(Const.kCssBackgroundColor, "#ff0");
                    meta.AddOneCssPropertyStyle(Const.kCssColor, "#000");
                    // meta
                    // ..[kCssBackgroundColor] =
                    // "#ff0"
                    //     ..[kCssColor] =
                    // "#000";
                    break;
                case "p":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayBlock);
                    meta.AddOneCssPropertyStyle(Const.kCssMargin, "1em 0");
                    // meta
                    // ..[kCssDisplay] =
                    // kCssDisplayBlock
                    //     ..[kCssMargin] =
                    // "1em 0";
                    break;

                case Const.kTagQ:
                    if (_tagQ == null)
                    {
                        _tagQ = new TagQ(this).buildOp;
                    }

                    // _tagQ ??= TagQ(this).buildOp;
                    meta.register(_tagQ);
                    break;

                case Const.kTagRuby:
                    meta.register(new TagRuby(this, meta).op);
                    break;

                case "script":
                case "style":
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayNone);
                    // meta[Const.kCssDisplay] = Const.kCssDisplayNone;
                    break;

                case "sub":
                    meta.AddOneCssPropertyStyle(Const.kCssVerticalAlign, Const.kCssVerticalAlignSub);
                    meta.tsb.enqueue(TextStyleOps.fontSizeTerm, Const.kCssFontSizeSmaller);
                    // meta
                    //     ..[Const.kCssVerticalAlign] =
                    // Const.kCssVerticalAlignSub
                    // ..tsb.enqueue(TextStyleOps.fontSizeTerm, Const.kCssFontSizeSmaller);
                    break;
                case "sup":
                    meta.AddOneCssPropertyStyle(Const.kCssVerticalAlign, Const.kCssVerticalAlignSuper);
                    meta.tsb.enqueue(TextStyleOps.fontSizeTerm, Const.kCssFontSizeSmaller);
                    // meta
                    //     ..[Const.kCssVerticalAlign] =
                    // Const.kCssVerticalAlignSuper
                    // ..tsb.enqueue(TextStyleOps.fontSizeTerm, Const.kCssFontSizeSmaller);
                    break;

                case Const.kTagTable:
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayTable);
                    meta.register
                    (
                        TagTable.borderOp
                        (
                            Helper.tryParseDoubleFromMap(attrs, Const.kAttributeBorder) ?? 0.0f,
                            Helper.tryParseDoubleFromMap(attrs, Const.kAttributeCellSpacing) ?? 2.0f
                        )
                    );
                    meta.register
                    (
                        TagTable.cellPaddingOp
                        (
                            Helper.tryParseDoubleFromMap(attrs, Const.kAttributeCellPadding) ?? 1.0f
                        )
                    );
                    // meta
                    //     ..[Const.kCssDisplay] =
                    // Const.kCssDisplayTable
                    // ..register(TagTable.borderOp(
                    //     Helper.tryParseDoubleFromMap(attrs, kAttributeBorder) ?? 0.0,
                    //     Helper.tryParseDoubleFromMap(attrs, kAttributeCellSpacing) ?? 2.0,
                    // ))
                    // ..register(TagTable.cellPaddingOp(
                    //     Helper.tryParseDoubleFromMap(attrs, kAttributeCellPadding) ?? 1.0));
                    break;
                case Const.kTagTableCell:
                    meta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssVerticalAlignMiddle);
                    // meta[Const.kCssVerticalAlign] = Const.kCssVerticalAlignMiddle;
                    break;
                case Const.kTagTableHeaderCell:
                    meta.AddOneCssPropertyStyle(Const.kCssVerticalAlign, Const.kCssVerticalAlignMiddle);
                    meta.tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    // meta
                    //     ..[Const.kCssVerticalAlign] =
                    // Const.kCssVerticalAlignMiddle
                    // ..tsb.enqueue(TextStyleOps.fontWeight, FontWeight.bold);
                    break;
                case Const.kTagTableCaption:
                    meta.AddOneCssPropertyStyle(Const.kCssTextAlign, Const.kCssTextAlignCenter);
                    // meta[Const.kCssTextAlign] = Const.kCssTextAlignCenter;
                    break;
            }

            foreach (var attribute in attrs)
            {
                // switch (attribute.key)
                switch (attribute.Name)
                {
                    case Const.kAttributeAlign:
                        // meta[Const.kCssTextAlign] = attribute.value;
                        meta.AddOneCssPropertyStyle(Const.kCssTextAlign, attribute.Value);
                        break;
                    case Const.kAttributeDir:
                        meta.AddOneCssPropertyStyle(Const.kCssDirection, attribute.Value);
                        // meta[Const.kCssDirection] = attribute.value;
                        break;
                    case Const.kAttributeId:
                        meta.register(_anchorOp(attribute.Value));
                        break;
                }
            }
        }


        public ImageProvider imageProviderFromAsset(String url)
        {
            throw new NotImplementedException("ab包加载的形式, 这边应该抽象给上层,让上层处理");
            // var uri = new Uri(url);
            // var assetName = uri.path;
            // if (assetName.isEmpty) return null;
            //
            // final package = uri.queryParameters.containsKey('package') == true
            //     ? uri.queryParameters['package']
            //     : null;
            //
            // return new AssetImage(assetName, package: package);
        }

        /// Returns a [MemoryImage].
        public ImageProvider imageProviderFromDataUri(String dataUri)
        {
            throw new NotImplementedException("内存流加载的形式, 这边应该抽象给上层,让上层处理");
            // var bytes = bytesFromDataUri(dataUri);
            // if (bytes == null) return null;
            //
            // return new MemoryImage(bytes);
        }

        /// Returns a [FileImage].
        public ImageProvider imageProviderFromFileUri(String url)
        {
            throw new NotImplementedException("文件加载的形式, 这边应该抽象给上层,让上层处理");
            // var filePath = Uri.parse(url).toFilePath();
            // var filePath = Uri.parse(url).toFilePath();
            // if (filePath.isEmpty) return null;
            //
            // return new fileImageProvider(filePath);
        }

        /// Returns a [NetworkImage].
        ImageProvider imageProviderFromNetwork(String url)
        {
            // HLog.LogInfo("imageProviderFromNetwork 从网路加载图片,release应该取消掉!");
            return url.isNotEmpty() ? new NetworkImage(url) : null;
        }


        BuildOp _anchorOp(string id)
        {
            // var anchor = GlobalKey(debugLabel: id);
            var anchor = GlobalKey.key(debugLabel: id);
            _anchors[id] = anchor;

            return new BuildOp
            (
                onTree: (meta, tree) =>
                {
                    if (meta.willBuildSubtree == true) return;
                    var widget = new WidgetPlaceholder($"#{id}")
                        .wrapWith
                        (
                            (context, _) => new SizedBox(
                                height: meta.tsb.build(context).style.fontSize,
                                key: anchor
                            )
                        );
                    var bit = tree.first;
                    if (bit == null)
                    {
                        // most likely an A[name]
                        tree.add(WidgetBit.inline(tree, widget));
                    }
                    else
                    {
                        // most likely a SPAN[id]
                        WidgetBit.inline(bit.parent, widget).insertBefore(bit);
                    }
                },
                onWidgets:
                (meta, widgets) =>
                {
                    if (meta.willBuildSubtree == false) return widgets;
                    return Helper.listOrNull
                    (
                        buildColumnPlaceholder(meta, widgets)
                            ?.wrapWith
                            (
                                (context, child) => new SizedBox(key: anchor, child: child)
                            )
                    );
                },
                onWidgetsIsOptional: true
            );
        }

        /// <summary>
        /// Builds [Tooltip].
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="child"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Widget buildTooltip(AbsBuildMetadata meta, Widget child, String message) =>
            new Tooltip(message: message, child: child);


        /// Builds [Image].
        public Widget buildImageWidget
        (
            AbsBuildMetadata meta,
            string url,
            string semanticLabel = null
        )
        {
            ImageProvider provider = null;
            if (url.StartsWith("asset:"))
            {
                provider = imageProviderFromAsset(url);
            }
            else if (url.StartsWith("data:image/"))
            {
                provider = imageProviderFromDataUri(url);
            }
            else if (url.StartsWith("file:"))
            {
                provider = imageProviderFromFileUri(url);
            }
            else
            {
                provider = imageProviderFromNetwork(url);
            }

            if (provider == null) return null;

            return new Unity.UIWidgets.widgets.Image
            (
                errorBuilder: (_, error, __) =>
                {
                    HLog.LogError($"{provider} error: {error}");
                    var text = semanticLabel ?? "❌";
                    return new Text(text);
                },
                // excludeFromSemantics: semanticLabel == null,//UIWidgets 没有搬运该功能..
                fit:
                BoxFit.fill,
                image:
                provider
                // semanticLabel: semanticLabel
            );
        }

        /// Builds [AspectRatio].
        Widget buildAspectRatio(
            AbsBuildMetadata meta, Widget child, float aspectRatio) =>
            new AspectRatio(aspectRatio: aspectRatio, child: child);


        /// <summary>
        /// Builds image widget from an [ImageMetadata].
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public Widget buildImage(AbsBuildMetadata meta, ImageMetadata data)
        {
            var src = data.sources.isNotEmpty() ? data.sources.First() : null;
            if (src == null) return null;

            var built = buildImageWidget
            (
                meta,
                semanticLabel: data.alt ?? data.title,
                url: src.url
            );

            var title = data.title;
            if (built != null && title != null)
            {
                built = buildTooltip(meta, built, title);
            }

            if (built != null
                && src.height?.isNegative() == false
                && src.width?.isNegative() == false
                && src.height != null
                && src.height.Value != 0)
            {
                built = buildAspectRatio(meta, built, src.width.Value / src.height.Value);
            }

            if (_widget?.onTapImage != null && built != null)
            {
                built = buildGestureDetector(
                    meta, built, () => _widget?.onTapImage?.Invoke(data));
            }

            return built;
        }


        /// <summary>
        /// Builds 1-pixel-height divider.
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public Widget buildDivider(AbsBuildMetadata meta) => new DecoratedBox
        (
            decoration: new BoxDecoration(color: Color.fromRGBO(0, 0, 0, 1)),
            child: new SizedBox(height: 1)
        );

        public Widget buildHorizontalScrollView(AbsBuildMetadata meta, Widget child) =>
            new SingleChildScrollView(scrollDirection: Axis.horizontal, child: child);

        public Widget buildColumnWidget
        (
            AbsBuildMetadata meta,
            TextStyleHtml tsh,
            List<Widget> children
        )
        {
            if (children.isEmpty()) return null;
            if (children.Count == 1) return children.First();

            return new Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                textDirection: tsh.textDirection,
                children: children
            );
        }

        /// Builds column placeholder.
        public WidgetPlaceholder buildColumnPlaceholder
        (
            AbsBuildMetadata meta,
            IEnumerable<WidgetPlaceholder> children,
            bool trimMarginVertical = false
        )
        {
            if (children.isEmpty())
            {
                return null;
            }

            if (children.length() == 1)
            {
                var child = children.First();
                if (child is ColumnPlaceholder childColumnPlaceholder)
                {
                    if (childColumnPlaceholder.trimMarginVertical == trimMarginVertical)
                    {
                        return child;
                    }
                }
                else
                {
                    return child;
                }
            }

            return new ColumnPlaceholder
            (
                children,
                meta,
                trimMarginVertical,
                this
            );
        }


        /// Builds [DecoratedBox].
        public Widget buildDecoratedBox
        (
            AbsBuildMetadata meta,
            Widget child,
            Color color
        )
        {
            return new DecoratedBox(
                decoration: new BoxDecoration(color: color),
                child: child
            );
        }


        /// <summary>
        ///        /// Builds [border] with [Container] or [DecoratedBox].
        ///
        /// See https://developer.mozilla.org/en-US/docs/Web/CSS/box-sizing
        /// for more information regarding `content-box` (the default)
        /// and `border-box` (set [isBorderBox] to use).
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="child"></param>
        /// <param name="border"></param>
        /// <param name="???"></param>
        /// <returns></returns>
        public Widget buildBorder
        (
            AbsBuildMetadata meta,
            Widget child,
            Border border,
            bool isBorderBox = false
        )
        {
            if (isBorderBox)
            {
                return new DecoratedBox
                (
                    decoration: new BoxDecoration(border: border),
                    child: child
                );
            }
            else
            {
                return new Container
                (
                    decoration: new BoxDecoration(border: border),
                    child: child
                );
            }
        }

        /// Builds [Padding].
        public Widget buildPadding
        (
            AbsBuildMetadata meta, Widget child, EdgeInsetsGeometry padding)
        {
            return (padding == EdgeInsets.zero)
                ? child
                : new Padding(padding: padding, child: child);
        }


        public Widget buildAlign(
            AbsBuildMetadata meta,
            Widget child,
            AlignmentGeometry alignment,
            float? heightFactor = null,
            float? widthFactor = null
        )
        {
            return new Align(
                alignment: alignment,
                heightFactor: heightFactor,
                widthFactor: widthFactor,
                child: child
            );
        }


        public Widget buildGestureDetector
        (
            AbsBuildMetadata meta,
            Widget child,
            GestureTapCallback onTap
        )
        {
            return new GestureDetector(onTap: onTap, child: child);
        }


        /// Builds [RichText].
        public Widget buildText(AbsBuildMetadata meta, TextStyleHtml tsh, InlineSpan text)
        {
            return new RichText
            (
                overflow: tsh.textOverflow ?? TextOverflow.clip,
                text: text,
                textAlign: tsh.textAlign ?? TextAlign.start,
                textDirection: tsh.textDirection,

                // TODO: calculate max lines automatically for ellipsis if needed //TODO处理...省略号符号
                // currently it only renders 1 line with ellipsis
                maxLines: tsh.maxLines == -1 ? null : tsh.maxLines
            );
        }


        public GestureTapCallback gestureTapCallback(string url) => () => onTapUrl(url);


        Future<bool> onTapAnchor(string id, BuildContext anchorContext)
        {
            HLog.LogError($"onTapAnchor 尚未实现!");
            return new SynchronousFuture<bool>(false);
        }

        // Future<bool> onTapAnchor(string id, BuildContext anchorContext) async
        // {
        //     if (anchorContext == null) return false;
        //
        //     var renderObject = anchorContext.findRenderObject();
        //     if (renderObject == null) return false;
        //
        //     var offsetToReveal = RenderAbstractViewport.of(renderObject)
        //         ?.getOffsetToReveal(renderObject, 0.0)
        //         .offset;
        //     var position = Scrollable.of(anchorContext)?.position;
        //     if (offsetToReveal == null || position == null) return false;
        //
        //     await position.ensureVisible(
        //         renderObject,
        //         duration: new TimeSpan(0, 0, 0, 0, 100),
        //         curve: Curves.easeIn
        //     );
        //     return true;
        // }

        // public Future<bool> onTapUrl(string url)
        // {
        //     var handledViaCallback = await onTapCallback(url);
        //     if (handledViaCallback) return true;
        //
        //     if (url.StartsWith("#"))
        //     {
        //         var id = url.Substring(1);
        //         var anchorContext = _anchors[id]?.currentContext;
        //         var handledViaAnchor = await onTapAnchor(id, anchorContext);
        //         if (handledViaAnchor)
        //         {
        //             return true;
        //         }
        //     }
        //
        //     return false;
        // }
        public bool onTapUrl(string url)
        {
            HLog.LogError("暂时使用同步的方式来触发回调!");
            var handledViaCallback = onTapCallback(url);
            if (handledViaCallback) return true;

            if (url.StartsWith("#"))
            {
                HLog.LogError("尚未实现! url.StartsWith(\"#\")");
                // var id = url.Substring(1);
                // var anchorContext = _anchors[id]?.currentContext;
                // var handledViaAnchor =  onTapAnchor(id, anchorContext);
                // if (handledViaAnchor)
                // {
                //     return true;
                // }
            }

            return false;
        }

        // public Future<bool> onTapCallback(string url) async
        // {
        //     var callback = _widget?.onTapUrl;
        //     if (callback == null) return false;
        //
        //     var result = await Future.value(callback(url));
        //     return result != false;
        // }
        //
        public bool onTapCallback(string url)
        {
            var callback = _widget?.onTapUrl;
            if (callback == null)
            {
                HLog.LogError($"没有注册onTap事件回调! _widget.html ={_widget.html}  ");
                return false;
            }
            else
            {
                callback(url);
                return true;
            }

            // if (callback == null) return new SynchronousFuture<bool>(false);

            // var result = Future.value(callback(url));
            // var result = Future.value(FutureOr.value(callback(url)));
            // var result = Future.value(FutureOr.value(callback(url)));
            // return result.then(o => );
        }

        /// Prepares the root [TextStyleBuilder].
        public void onRoot(TextStyleBuilder rootTsb)
        {
        } //dart源码处, 这个onRoot 啥都没实现..


        /// <summary>
        /// Resolves full URL with [HtmlWidget.baseUrl] if available.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public string urlFull(string url)
        {
            
            if (!url.StartsWith("http"))
            {
                // HLog.LogError($"暂时只支持http开头的url={url}");
                return url;
            }
            if (string.IsNullOrEmpty(url)) return null;
            if (url.StartsWith("data:")) return url;

            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                HLog.LogInfo($"TryCreate uri failed url={url}");
                return null;
            }


            // var uri = new Uri(url);
            if (uri.hasScheme()) return url;

            var baseUrl = _widget?.baseUrl;
            if (baseUrl == null) return null;

            // return baseUrl.resolveUri(uri).toString();
            return baseUrl.resolveUri(uri).ToString();
        }

        /// <summary>
        /// Returns marker for the specified [type] at index [i].
        ///
        /// Note: `circle`, `disc` and `square` type won't trigger this method
        /// </summary>
        /// <param name="type"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public string getListStyleMarker(string type, int i)
        {
            switch (type)
            {
                case Const.kCssListStyleTypeAlphaLower:
                case Const.kCssListStyleTypeAlphaLatinLower:
                    if (i >= 1 && i <= 26)
                    {
                        // the specs said it"s unspecified after the 26th item
                        // TODO: generate something like aa, ab, etc. when needed
                        // return "${String.fromCharCode(96 + i)}.";
                        return $"{StringEx.fromCharCode(96 + i)}.";
                    }

                    return "";
                case Const.kCssListStyleTypeAlphaUpper:
                case Const.kCssListStyleTypeAlphaLatinUpper:
                    if (i >= 1 && i <= 26)
                    {
                        // the specs said it"s unspecified after the 26th item
                        // TODO: generate something like AA, AB, etc. when needed
                        // return "${String.fromCharCode(64 + i)}.";
                        return $"{StringEx.fromCharCode(64 + i)}.";
                    }

                    return "";
                case Const.kCssListStyleTypeDecimal:
                    return $"{i}.";
                case Const.kCssListStyleTypeRomanLower:
                    // var roman = _getListStyleMarkerRoman(i)?.toLowerCase();
                    var roman1 = _getListStyleMarkerRoman(i)?.ToLower();
                    // return roman1 != null ? "$roman." : "";
                    return roman1 != null ? $"{roman1}." : "";
                case Const.kCssListStyleTypeRomanUpper:
                    var roman2 = _getListStyleMarkerRoman(i);
                    // return roman2 != null ? "$roman." : "";
                    return roman2 != null ? $"{roman2}." : "";
            }

            return "";
        }

        private static Dictionary<int, string> _dicRomanNum = new Dictionary<int, string>()
        {
            {1, "I"},
            {2, "II"},
            {3, "III"},
            {4, "IV"},
            {5, "V"},
            {6, "VI"},
            {7, "VII"},
            {8, "VIII"},
            {9, "IX"},
            {10, "X"},
        };

        string _getListStyleMarkerRoman(int i)
        {
            // TODO: find some lib to generate programatically
            // var map = new Dictionary<int, string>()
            // {
            //     { 1, "I" },
            //     { 2, "II" },
            //     { 3, "III" },
            //     { 4, "IV" },
            //     { 5, "V" },
            //     { 6, "VI" },
            //     { 7, "VII" },
            //     { 8, "VIII" },
            //     { 9, "IX" },
            //     { 10, "X" },
            // };
            // return map[i];

            if (_dicRomanNum.TryGetValue(i, out var v))
            {
                return v;
            }
            else
            {
                HLog.LogError($"_getListStyleMarkerRoman don't contain key = {i}");
                return "";
            }
        }


        public IEnumerable<object> getDependencies(BuildContext context)
        {
            var retList = new List<object>();
            retList.Add(MediaQuery.of(context));
            retList.Add(Directionality.of(context));
            retList.Add(DefaultTextStyle.of(context).style);
            retList.Add(Theme.of(context));
            return retList;

            // [
            //     MediaQuery.of(context),
            //     Directionality.of(context),
            //     DefaultTextStyle.of(context).style,
            // Theme.of(context),
            //     ];
        }

        /// <summary>
        ///  Resets for a new build.
        /// </summary>
        /// <returns></returns>
        public void reset(State state)
        {
            _anchors.Clear();
            _flattener.reset();

            var widget = state.widget;
            if (widget is HtmlWidget htmlWidget)
            {
                _widget = htmlWidget;
            }
            else
            {
                HLog.LogError("WidegtFactory reset widget is not HtmlWidget");
                widget = null;
            }

            // _widget = widget is HtmlWidget ? widget : null;
        }


        /// <summary>
        /// Flattens a [BuildTree] into widgets.
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="tree"></param>
        /// <returns></returns>
        public IEnumerable<WidgetPlaceholder> flatten(AbsBuildMetadata meta, BuildTree tree)
        {
            var widgets = new List<WidgetPlaceholder>();

            var flattens = _flattener.flatten(tree);
            foreach (var flattened in flattens)
            {
                if (flattened.widget != null)
                {
                    widgets.Add(WidgetPlaceholder.lazy(flattened.widget));
                    continue;
                }

                if (flattened.widgetBuilder != null)
                {
                    widgets.Add(
                        new WidgetPlaceholder(tree)
                            .wrapWith
                            (
                                (context, _) => flattened.widgetBuilder(context)
                            ));
                    continue;
                }

                if (flattened.spanBuilder == null) continue;
                widgets.Add
                (
                    new WidgetPlaceholder(tree)
                        .wrapWith
                        (
                            (context, _) =>
                            {
                                var tsh = tree.tsb.build(context);
                                var span = flattened.spanBuilder(context, tsh.whitespace);
                                if (span == null || (!(span is InlineSpan))) return Helper.widget0;

                                var textAlign = tsh.textAlign ?? TextAlign.start;

                                /*if (span is WidgetSpan &&
                                span.alignment == PlaceholderAlignment.baseline &&
                                    textAlign == TextAlign.start)*/
                                if (span is WidgetSpan widgetSpan)
                                {
                                    if (widgetSpan.alignment == PlaceholderAlignment.baseline
                                        && textAlign == TextAlign.start)
                                    {
                                        return widgetSpan.child;
                                    }
                                }

                                return buildText(meta, tsh, span);
                            }
                        )
                );
            }

            return widgets;
        }

        /// Builds primary column (body).
        public WidgetPlaceholder buildBody(
            BuildMetadata meta, IEnumerable<WidgetPlaceholder> children)
        {
            return buildColumnPlaceholder(meta, children, trimMarginVertical: true);
        }


        public void Dispose()
        {
            _flattener.Dispose();
        }
    }
}