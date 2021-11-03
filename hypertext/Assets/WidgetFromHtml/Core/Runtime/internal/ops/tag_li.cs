using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Dom;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using WidgetFromHtml.Core;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace WidgetFromHtml.Core
{
    internal class TagLi
    {
        public AbsBuildMetadata listMeta;
        public BuildOp op;
        public WidgetFactory wf;

        List<AbsBuildMetadata> _itemMetas = new List<AbsBuildMetadata>();
        BuildOp _itemOp;
        List<WidgetPlaceholder> _itemWidgets = new List<WidgetPlaceholder>();
        _ListConfig _config;


        public TagLi
        (
            WidgetFactory wf,
            AbsBuildMetadata listMeta
        )
        {
            this.listMeta = listMeta;
            this.wf = wf;

            this.op = new _TagLiListOp(this);
            _itemOp = new BuildOp
            (
                onWidgets: (itemMeta, widgets) =>
                {
                    var column = wf.buildColumnPlaceholder(itemMeta, widgets) ??
                                 new WidgetPlaceholder(itemMeta);
                    var i = _itemMetas.length();
                    _itemMetas.Add(itemMeta);
                    _itemWidgets.Add(column);
                    var retList = new List<Widget>(1);
                    retList.Add(column.wrapWith((c, w) => _buildItem(c, w, i)));
                    // return [column.wrapWith((c, w) => _buildItem(c, w, i))]
                    return retList;
                }
            );
        }

        public _ListConfig config
        {
            get
            {
                if (_config == null)
                {
                    _config = _ListConfig.fromBuildMetadata(listMeta);
                }

                return _config;
            }
        }

        public Dictionary<string, string> defaultStyles(IElement element)
        {
            var attrs = element.Attributes;
            // var depth = listMeta.parentOps.whereType<_TagLiListOp>().length;
            var depth = listMeta.parentOps.whereType_length<BuildOp, _TagLiListOp>();

            var styles = new Dictionary<string, string>(2);

            var strListStyleType = element.LocalName == Const.kTagOrderedList
                ? _ListConfig.listStyleTypeFromAttributeType(attrs[Const.kAttributeLiType]?.Value ?? "")
                  ?? Const.kCssListStyleTypeDecimal
                : depth == 0
                    ? Const.kCssListStyleTypeDisc
                    : depth == 1
                        ? Const.kCssListStyleTypeCircle
                        : Const.kCssListStyleTypeSquare;

            styles.Add("padding-inline-start", "40px");
            styles.Add(Const.kCssListStyleType, strListStyleType);


            if (depth == 0)
            {
                // styles[Const.kCssMargin] = "1em 0";
                styles.Add(Const.kCssMargin, "1em 0");
            }

            return styles;
        }


        public void onChild(AbsBuildMetadata childMeta)
        {
            var e = childMeta.element;
            if (e.LocalName != Const.kTagLi) return;
            if (e.Parent != listMeta.element) return;
            childMeta.register(_itemOp);
        }

        Widget _buildItem(BuildContext context, Widget child, int i)
        {
            var meta = _itemMetas[i];
            var listStyleType = _ListConfig.listStyleTypeFromBuildMetadata(meta) ??
                                config.listStyleType;
            var markerIndex = config.markerReversed
                ? (config.markerStart ?? _itemWidgets.length()) - i
                : (config.markerStart ?? 1) + i;
            var tsh = meta.tsb.build(context);

            var markerText = wf.getListStyleMarker(listStyleType, markerIndex);
            var marker = _buildMarker(tsh, listStyleType, markerText);

            return new HtmlListItem
            (
                marker: marker,
                textDirection: tsh.textDirection,
                child: child
            );
        }

        Widget _buildMarker(TextStyleHtml tsh, string type, string text)
        {
            var style = tsh.styleWithHeight;
            return text.isNotEmpty()
                ? new RichText
                (
                    maxLines: 1,
                    overflow: TextOverflow.clip,
                    softWrap: false,
                    text: new TextSpan(style: style, text: text),
                    textDirection: tsh.textDirection
                )
                : type == Const.kCssListStyleTypeCircle
                    ? (Widget)new _ListMarkerCircle(style)
                    : type == Const.kCssListStyleTypeSquare
                        ? (Widget)new _ListMarkerSquare(style)
                        : (Widget)new _ListMarkerDisc(style);
        }
    }


    class _ListConfig
    {
        public string listStyleType;
        public bool markerReversed;
        public int? markerStart;

        public _ListConfig
        (
            string listStyleType,
            bool markerReversed,
            int? markerStart = null
        )
        {
            this.listStyleType = listStyleType;
            this.markerReversed = markerReversed;
            this.markerStart = markerStart;
        }

        public static _ListConfig fromBuildMetadata(AbsBuildMetadata meta)
        {
            var attrs = meta.element.Attributes;
            return new _ListConfig
            (
                listStyleType: meta[Const.kCssListStyleType]?.Value ?? Const.kCssListStyleTypeDisc,
                markerReversed: attrs.containsKey(Const.kAttributeOlReversed),
                markerStart: Helper.tryParseIntFromMap(attrs, Const.kAttributeOlStart)
            );
        }

        public static string listStyleTypeFromBuildMetadata(AbsBuildMetadata meta)
        {
            var listStyleType = meta[Const.kCssListStyleType]?.Value;
            if (listStyleType != null) return listStyleType;
            return listStyleTypeFromAttributeType
            (
                meta.element.Attributes[Const.kAttributeLiType]?.Value ?? ""
            );
        }

        public static string listStyleTypeFromAttributeType(string type)
        {
            switch (type)
            {
                case Const.kAttributeLiTypeAlphaLower:
                    return Const.kCssListStyleTypeAlphaLower;
                case Const.kAttributeLiTypeAlphaUpper:
                    return Const.kCssListStyleTypeAlphaUpper;
                case Const.kAttributeLiTypeDecimal:
                    return Const.kCssListStyleTypeDecimal;
                case Const.kAttributeLiTypeRomanLower:
                    return Const.kCssListStyleTypeRomanLower;
                case Const.kAttributeLiTypeRomanUpper:
                    return Const.kCssListStyleTypeRomanUpper;
            }

            return null;
        }
    }


    class _ListMarkerCircle : _ListMarker
    {
        public _ListMarkerCircle
        (
            TextStyle textStyle,
            Key key = null,
            Widget child = null
        ) : base(_ListMarkerType.circle, textStyle, key, child)
        {
        }
    }


    class _ListMarkerDisc : _ListMarker
    {
        public _ListMarkerDisc
        (
            TextStyle textStyle,
            Key key = null,
            Widget child = null
        ) : base(_ListMarkerType.disc, textStyle, key, child)
        {
        }
    }


    class _ListMarkerSquare : _ListMarker
    {
        public _ListMarkerSquare
        (
            TextStyle textStyle,
            Key key = null,
            Widget child = null
        ) : base(_ListMarkerType.square, textStyle, key, child)
        {
        }
    }

    class _ListMarker : SingleChildRenderObjectWidget
    {
        _ListMarkerType markerType;
        public TextStyle textStyle;

        public _ListMarker
        (
            _ListMarkerType markerType,
            TextStyle textStyle,
            Key key = null,
            Widget child = null
        ) : base(key, child)
        {
            this.markerType = markerType;
            this.textStyle = textStyle;
        }

        public override RenderObject createRenderObject(BuildContext context)
        {
            return new _ListMarkerRenderObject(markerType, textStyle);
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject1)
        {
            if (renderObject1 is _ListMarkerRenderObject renderObject)
            {
                renderObject.markerType = this.markerType;
                renderObject.textStyle = this.textStyle;
            }
            else
            {
                HLog.LogError("_ListMarker 类型不是_ListMarkerRenderObject");
            }
        }
    }


    class _ListMarkerRenderObject : RenderBox
    {
        _ListMarkerType _markerType;
        TextStyle _textStyle;

        public _ListMarkerRenderObject
        (
            _ListMarkerType markerType,
            TextStyle textStyle
        )
        {
            _markerType = markerType;
            _textStyle = textStyle;
        }

        public _ListMarkerType markerType
        {
            get { return _markerType; }
            set
            {
                if (value == _markerType) return;
                _markerType = value;
                markNeedsLayout();
            }
        }

        TextPainter __textPainter;

        public TextPainter _textPainter
        {
            get
            {
                if (__textPainter == null)
                {
                    __textPainter = new TextPainter
                    (
                        text: new TextSpan(style: _textStyle, text: "1."), //这个字符串看不懂啥意思--!
                        textDirection: TextDirection.ltr
                    );
                    __textPainter
                        .layout(); //此处的layout只会调用1次, dart那侧的写法已经验证过 https://xmdc.yuque.com/staff-nt39y2/xh7p4o/tm7i8r#GAjDr
                }

                return __textPainter;
            }
        }

        public TextStyle textStyle
        {
            get { return _textStyle; }
            set
            {
                if (value == _textStyle) return;
                __textPainter = null;
                _textStyle = value;
                markNeedsLayout();
            }
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline)
        {
            // return base.computeDistanceToActualBaseline(baseline);
            return _textPainter.computeDistanceToActualBaseline(baseline);
        }


        //作者暂时没有支持dryLayout相关的特性 issue
        // @override
        //     Size computeDryLayout(BoxConstraints constraints) =>
        //     constraints.constrain(_textPainter.size);

        //TODO 等待作者升级
        public new Size getDryLayout(BoxConstraints constraints)
        {
            return constraints.constrain(_textPainter.size);
        }

        public override void paint(PaintingContext context, Offset offset)
        {
            var canvas = context.canvas;
            var lineMetrics = new List<LineMetrics>();
            try
            {
                lineMetrics = _textPainter.computeLineMetricsEx();
                // ignore: empty_catches
            }
            catch (Exception e)
            {
                HLog.LogError($"_TagLiListOp paint error msg={e.Message} \n{e.StackTrace}");
            }


            var m = lineMetrics.isNotEmpty() ? lineMetrics.First() : null;
            var center = offset +
                         new Offset(
                             size.width / 2f,
                             (m?.descent.isFinite() == true && m?.unscaledAscent.isFinite() == true)
                                 ? size.height -
                                   m.descent -
                                   m.unscaledAscent +
                                   m.unscaledAscent * 0.7f
                                 : size.height / 2f
                         );
            // Debug.Log($"center={center} offset={offset}");
            var color = _textStyle.color;
            var fontSize = _textStyle.fontSize;
            if (color == null || fontSize == null) return;

            var radius = fontSize * 0.2f;
            switch (_markerType)
            {
                case _ListMarkerType.circle:
                    var paintC = new Paint();
                    paintC.color = color;
                    paintC.strokeWidth = 1;
                    paintC.style = PaintingStyle.stroke;
                    canvas.drawCircle
                    (
                        center,
                        radius.Value * 0.9f,
                        paintC
                    );
                    break;
                case _ListMarkerType.disc:
                    var paintD = new Paint();
                    paintD.color = color;
                    canvas.drawCircle
                    (
                        center,
                        radius.Value,
                        paintD
                    );
                    break;
                case _ListMarkerType.square:
                    var paintS = new Paint();
                    paintS.color = color;
                    canvas.drawRect
                    (
                        Rect.fromCircle(center: center, radius: radius.Value * 0.8f),
                        paintS
                    );
                    break;
            }
        }

        protected override void performLayout()
        {
            // size = computeDryLayout(constraints);
            size = getDryLayout(constraints);
        }
    }

    enum _ListMarkerType
    {
        circle,
        disc,
        square,
    }


    class _TagLiListOp : BuildOp
    {
        public _TagLiListOp(TagLi tagLi)
            : base(
                defaultStyles: tagLi.defaultStyles,
                onChild: tagLi.onChild,
                onWidgets: _onWidgetsPassThrough
            )
        {
        }

        static IEnumerable<Widget> _onWidgetsPassThrough(
            AbsBuildMetadata _, IEnumerable<Widget> widgets) =>
            widgets;
    }
}