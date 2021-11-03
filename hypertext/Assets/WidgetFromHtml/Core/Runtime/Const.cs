using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace WidgetFromHtml.Core
{
    /// <summary>
    /// 单纯只是为了映射dart中的 Null 类型
    /// </summary>
    internal class Null
    {
        public static Null _uninstantiable()
        {
            throw new Exception("class Null cannot be instantiated");
        }

        public override int GetHashCode()
        {
            return GetHashCode();
        }

        public override string ToString() => "Null class from dart language...";
    }

    public static class Const
    {
        public static readonly Type kTypString = typeof(string);
        public static readonly Type kTypNull = typeof(Null);
        public static readonly Type kTypObject = typeof(object);

        public const int kShouldBuildAsync = 10000;

        //background
        public const string kCssBackground = "background";
        public const string kCssBackgroundColor = "background-color";

        //box
        public const string kCssBoxSizing = "box-sizing";
        public const string kCssBoxSizingContentBox = "content-box";
        public const string kCssBoxSizingBorderBox = "border-box";

        //border
        public const string kCssBorder = "border";
        public const string kCssBorderInherit = "inherit";
        public const string kCssBorderStyleDotted = "dotted";
        public const string kCssBorderStyleDashed = "dashed";
        public const string kCssBorderStyleDouble = "double";
        public const string kCssBorderStyleSolid = "solid";


        //kSuffix
        public const string kSuffixBlockEnd = "-block-end";
        public const string kSuffixBlockStart = "-block-start";
        public const string kSuffixBottom = "-bottom";
        public const string kSuffixInlineEnd = "-inline-end";
        public const string kSuffixInlineStart = "-inline-start";
        public const string kSuffixLeft = "-left";
        public const string kSuffixRight = "-right";
        public const string kSuffixTop = "-top";

        public const string kCssMargin = "margin";


        public const string kAttributeId = "id";

        public const string kTagCode = "code";
        public const string kTagCodeFont1 = "Courier";
        public const string kTagCodeFont2 = "monospace";
        public const string kTagKbd = "kbd";
        public const string kTagPre = "pre";
        public const string kTagSamp = "samp";
        public const string kTagTt = "tt";

        public const string kTagFont = "font";
        public const string kAttributeFontColor = "color";
        public const string kAttributeFontFace = "face";
        public const string kAttributeFontSize = "size";

        public const string kCssDisplay = "display";
        public const string kCssDisplayBlock = "block";
        public const string kCssDisplayInline = "inline";
        public const string kCssDisplayInlineBlock = "inline-block";
        public const string kCssDisplayNone = "none";

        public const string kCssMaxLines = "max-lines";
        public const string kCssMaxLinesNone = "none";
        public const string kCssMaxLinesWebkitLineClamp = "-webkit-line-clamp";

        public const string kCssTextOverflow = "text-overflow";
        public const string kCssTextOverflowClip = "clip";
        public const string kCssTextOverflowEllipsis = "ellipsis";

        public const string kCssWhitespace = "white-space";
        public const string kCssWhitespacePre = "pre";
        public const string kCssWhitespaceNormal = "normal";

        public const string kCssPadding = "padding";

        public const string kCssHeight = "height";
        public const string kCssMaxHeight = "max-height";
        public const string kCssMaxWidth = "max-width";
        public const string kCssMinHeight = "min-height";
        public const string kCssMinWidth = "min-width";
        public const string kCssWidth = "width";


        public const string kCssColor = "color";

        public const string kAttributeAlign = "align";

        public const string kCssTextAlign = "text-align";
        public const string kCssTextAlignCenter = "center";
        public const string kCssTextAlignEnd = "end";
        public const string kCssTextAlignJustify = "justify";
        public const string kCssTextAlignLeft = "left";
        public const string kCssTextAlignMozCenter = "-moz-center";
        public const string kCssTextAlignRight = "right";
        public const string kCssTextAlignStart = "start";
        public const string kCssTextAlignWebkitCenter = "-webkit-center";

        public const string kTagCenter = "center";


        public const string kCssVerticalAlign = "vertical-align";
        public const string kCssVerticalAlignBaseline = "baseline";
        public const string kCssVerticalAlignTop = "top";
        public const string kCssVerticalAlignBottom = "bottom";
        public const string kCssVerticalAlignMiddle = "middle";
        public const string kCssVerticalAlignSub = "sub";
        public const string kCssVerticalAlignSuper = "super";


        public const string kAttributeAHref = "href";
        public const string kAttributeAName = "name";
        public const string kTagA = "a";


        public const string kCssDirection = "direction";
        public const string kCssDirectionLtr = "ltr";
        public const string kCssDirectionRtl = "rtl";
        public const string kAttributeDir = "dir";

        public const string kCssFontFamily = "font-family";

        public const string kCssFontSize = "font-size";
        public const string kCssFontSizeXxLarge = "xx-large";
        public const string kCssFontSizeXLarge = "x-large";
        public const string kCssFontSizeLarge = "large";
        public const string kCssFontSizeMedium = "medium";
        public const string kCssFontSizeSmall = "small";
        public const string kCssFontSizeXSmall = "x-small";
        public const string kCssFontSizeXxSmall = "xx-small";
        public const string kCssFontSizeLarger = "larger";
        public const string kCssFontSizeSmaller = "smaller";

        public const string kCssFontStyle = "font-style";
        public const string kCssFontStyleItalic = "italic";
        public const string kCssFontStyleNormal = "normal";

        public const string kCssFontWeight = "font-weight";
        public const string kCssFontWeightBold = "bold";

        public const string kCssLineHeight = "line-height";
        public const string kCssLineHeightNormal = "normal";

        public const string kCssTextDecoration = "text-decoration";
        public const string kCssTextDecorationLineThrough = "line-through";
        public const string kCssTextDecorationNone = "none";
        public const string kCssTextDecorationOverline = "overline";
        public const string kCssTextDecorationUnderline = "underline";


        public static readonly Dictionary<string, string> kCssFontSizes = new Dictionary<string, string>
        {
            {"1", kCssFontSizeXxSmall},
            {"2", kCssFontSizeXSmall},
            {"3", kCssFontSizeSmall},
            {"4", kCssFontSizeMedium},
            {"5", kCssFontSizeLarge},
            {"6", kCssFontSizeXLarge},
            {"7", kCssFontSizeXxLarge}
        };


        public const string kAttributeImgAlt = "alt";
        public const string kAttributeImgHeight = "height";
        public const string kAttributeImgSrc = "src";
        public const string kAttributeImgTitle = "title";
        public const string kAttributeImgWidth = "width";

        public const string kTagImg = "img";


        public const string kTagLi = "li";
        public const string kTagOrderedList = "ol";
        public const string kTagUnorderedList = "ul";
        public const string kAttributeLiType = "type";
        public const string kAttributeLiTypeAlphaLower = "a";
        public const string kAttributeLiTypeAlphaUpper = "A";
        public const string kAttributeLiTypeDecimal = "1";
        public const string kAttributeLiTypeRomanLower = "i";
        public const string kAttributeLiTypeRomanUpper = "I";
        public const string kAttributeOlReversed = "reversed";
        public const string kAttributeOlStart = "start";
        public const string kCssListStyleType = "list-style-type";
        public const string kCssListStyleTypeAlphaLower = "lower-alpha";
        public const string kCssListStyleTypeAlphaUpper = "upper-alpha";
        public const string kCssListStyleTypeAlphaLatinLower = "lower-latin";
        public const string kCssListStyleTypeAlphaLatinUpper = "upper-latin";
        public const string kCssListStyleTypeCircle = "circle";
        public const string kCssListStyleTypeDecimal = "decimal";
        public const string kCssListStyleTypeDisc = "disc";
        public const string kCssListStyleTypeRomanLower = "lower-roman";
        public const string kCssListStyleTypeRomanUpper = "upper-roman";
        public const string kCssListStyleTypeSquare = "square";


        public const string kTagTable = "table";
        public const string kTagTableRow = "tr";
        public const string kTagTableHeaderGroup = "thead";
        public const string kTagTableRowGroup = "tbody";
        public const string kTagTableFooterGroup = "tfoot";
        public const string kTagTableHeaderCell = "th";
        public const string kTagTableCell = "td";
        public const string kTagTableCaption = "caption";

        public const string kAttributeBorder = "border";
        public const string kAttributeCellPadding = "cellpadding";
        public const string kAttributeColspan = "colspan";
        public const string kAttributeCellSpacing = "cellspacing";
        public const string kAttributeRowspan = "rowspan";
        public const string kAttributeValign = "valign";

        public const string kCssBorderCollapse = "border-collapse";
        public const string kCssBorderCollapseCollapse = "collapse";
        public const string kCssBorderCollapseSeparate = "separate";
        public const string kCssBorderSpacing = "border-spacing";

        public const string kCssDisplayTable = "table";
        public const string kCssDisplayTableRow = "table-row";
        public const string kCssDisplayTableHeaderGroup = "table-header-group";
        public const string kCssDisplayTableRowGroup = "table-row-group";
        public const string kCssDisplayTableFooterGroup = "table-footer-group";
        public const string kCssDisplayTableCell = "table-cell";
        public const string kCssDisplayTableCaption = "table-caption";


        public const float _kGapVsMarker = 5.0f;

        public static readonly int kPriorityMax = int.MaxValue - 1;
        public const int kPriority4k3 = 4300;

        public const string kTagQ = "q";

        public const string kTagRuby = "ruby";
        public const string kTagRp = "rp";
        public const string kTagRt = "rt";

        //TODO 
        public const string _asciiWhitespace = "[\u0009\u000A\u000C\u000D\u0020]";
        public static readonly Regex _regExpSpaceLeading = new Regex($"^{_asciiWhitespace}+", RegexOptions.Compiled);
        public static readonly Regex _regExpSpaceTrailing = new Regex($@"{_asciiWhitespace}+$", RegexOptions.Compiled);
        public static readonly Regex _regExpSpaces = new Regex($@"{_asciiWhitespace}+", RegexOptions.Compiled);
    }
}