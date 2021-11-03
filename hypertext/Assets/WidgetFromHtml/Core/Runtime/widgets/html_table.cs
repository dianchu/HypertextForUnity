using System;
using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace WidgetFromHtml.Core
{
    public delegate Size layouter(RenderBox renderBox, BoxConstraints constraints);

    internal class HtmlTable : MultiChildRenderObjectWidget
    {
        /// <summary>
        /// The table border sides.
        /// </summary>
        public Border border;

        /// <summary>
        /// Controls whether to collapse borders.
        ///
        /// Default: `false`.
        /// </summary>
        public bool borderCollapse;

        /// <summary>
        /// The gap between borders.
        ///
        /// Default: `0.0`.
        /// </summary>
        public float borderSpacing;

        /// <summary>
        /// The companion data for table.
        /// </summary>
        public HtmlTableCompanion companion;


        public HtmlTable
        (
            Border border,
            List<Widget> children,
            HtmlTableCompanion companion,
            bool borderCollapse = false,
            float borderSpacing = 0f,
            Key key = null
        ) : base(key, children)
        {
            this.border = border;
            this.borderCollapse = borderCollapse;
            this.borderSpacing = borderSpacing;
            this.companion = companion;
        }


        public override RenderObject createRenderObject(BuildContext context)
        {
            return new _TableRenderObject(border, borderCollapse, borderSpacing, companion);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);
            var floatProperty = new FlagProperty
            ("borderCollapse",
                value: borderCollapse,
                defaultValue: false,
                ifTrue: "borderCollapse: true"
            );

            properties.add(new DiagnosticsProperty<Border>("border", border, defaultValue: null));
            properties.add(floatProperty);
            properties.add(new FloatProperty("borderSpacing", borderSpacing, defaultValue: 0.0f));
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject1)
        {
            // base.updateRenderObject(context, renderObject);

            if (renderObject1 is _TableRenderObject renderObject)
            {
                renderObject.border = this.border;
                renderObject.borderCollapse = this.borderCollapse;
                renderObject.borderSpacing = this.borderSpacing;
                renderObject.companion = this.companion;
            }
            else
            {
                HLog.LogError("HtmlTable::updateRenderObject renderObject1 is not _TableRenderObject");
            }
        }
    }

    internal class HtmlTableCompanion
    {
        public Dictionary<int, List<_ValignBaselineRenderObject>> _baselines =
            new Dictionary<int, List<_ValignBaselineRenderObject>>();
    }

    internal class HtmlTableCell : ParentDataWidget<_TableCellData>
    {
        /// <summary>
        /// The cell border sides.
        /// </summary>
        public Border border;

        /// <summary>
        /// The number of columns this cell should span.
        /// </summary>
        public int columnSpan;

        /// <summary>
        /// The column index this cell should start.
        /// </summary>
        public int columnStart;

        /// <summary>
        /// The number of rows this cell should span.
        /// </summary>
        public int rowSpan;

        /// <summary>
        /// The row index this cell should start.
        /// </summary>
        public int rowStart;

        public HtmlTableCell
        (
            Border border,
            Widget child,
            int columnStart,
            int rowStart,
            int columnSpan = 1,
            Key key = null,
            int rowSpan = 1
        ) : base(key, child)
        {
            this.border = border;
            this.columnSpan = columnSpan;
            this.columnStart = columnStart;
            this.rowSpan = rowSpan;
            this.rowStart = rowStart;
            D.assert(this.columnSpan >= 1);
            D.assert(this.columnStart >= 0);
            D.assert(this.rowSpan >= 1);
            D.assert(this.rowStart >= 0);
        }


        public override void applyParentData(RenderObject renderObject)
        {
            var data = renderObject.parentData as _TableCellData;
            var needsLayout = false;

            if (data.border != border)
            {
                data.border = border;
                needsLayout = true;
            }

            if (data.columnSpan != columnSpan)
            {
                data.columnSpan = columnSpan;
                needsLayout = true;
            }

            if (data.columnStart != columnStart)
            {
                data.columnStart = columnStart;
                needsLayout = true;
            }

            if (data.rowStart != rowStart)
            {
                data.rowStart = rowStart;
                needsLayout = true;
            }

            if (data.rowSpan != rowSpan)
            {
                data.rowSpan = rowSpan;
                needsLayout = true;
            }

            if (needsLayout)
            {
                var parent = renderObject.parent;
                // if (parent is RenderObject) parent.markNeedsLayout();
                if (parent is RenderObject ro) ro.markNeedsLayout();
            }
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);
            properties.add(new DiagnosticsProperty<Border>("border", border, defaultValue: null));
            properties.add(new IntProperty("columnSpan", columnSpan, defaultValue: 1));
            properties.add(new IntProperty("columnStart", columnStart));
            properties.add(new IntProperty("rowSpan", rowSpan, defaultValue: 1));
            properties.add(new IntProperty("rowStart", rowStart));
        }

        public override Type debugTypicalAncestorWidgetClass => typeof(HtmlTable);
    }


    class HtmlTableValignBaseline : SingleChildRenderObjectWidget
    {
        /// <summary>
        ///  The table's companion data.
        /// </summary>
        public HtmlTableCompanion companion;

        /// <summary>
        ///  The cell's row index.
        /// </summary>
        public int row;


        public HtmlTableValignBaseline
        (
            HtmlTableCompanion companion,
            int row,
            Key key = null,
            Widget child = null
        ) : base(key, child)
        {
            this.companion = companion;
            this.row = row;
        }

        public override RenderObject createRenderObject(BuildContext context)
        {
            return new _ValignBaselineRenderObject(companion, row);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);
            properties.add(new IntProperty("row", row));
        }


        public override void updateRenderObject(BuildContext context, RenderObject renderObject1)
        {
            // base.updateRenderObject(context, renderObject);

            if (renderObject1 is _ValignBaselineRenderObject renderObject)
            {
                renderObject.companion = this.companion;
                renderObject.row = this.row;
            }
            else
            {
                HLog.LogError(
                    "HtmlTableValignBaseline::updateRenderObject error renderObject1 is not _ValignBaselineRenderObject");
            }
        }
    }

    class _TableCellData : ContainerBoxParentData<RenderBox>
    {
        public Border border;
        public int columnSpan = 1;
        public int columnStart = -1;
        public int rowSpan = 1;
        public int rowStart = -1;

        public float calculateHeight(_TableRenderObject tro, List<float> heights)
        {
            var gaps = (rowSpan - 1) * tro.rowGap;
            // return heights.getRange(rowStart, rowStart + rowSpan).sum + gaps;
            var ret = heights.getRange_sum(rowStart, rowStart + rowSpan) + gaps;
            return ret;
        }

        public float calculateWidth(_TableRenderObject tro, List<float> widths)
        {
            var gaps = (columnSpan - 1) * tro.columnGap;
            // return widths.getRange(columnStart, columnStart + columnSpan).sum + gaps;
            var ret = widths.getRange_sum(columnStart, columnStart + columnSpan) + gaps;
            return ret;
        }

        public float calculateX(_TableRenderObject tro, List<float> widths)
        {
            var padding = tro._border?.left.width ?? 0.0f;
            var gaps = (columnStart + 1) * tro.columnGap;
            // return padding + widths.getRange(0, columnStart).sum + gaps;
            return padding + widths.getRange_sum(0, columnStart) + gaps;
        }

        public float calculateY(_TableRenderObject tro, List<float> heights)
        {
            var padding = tro._border?.top.width ?? 0.0f;
            var gaps = (rowStart + 1) * tro.rowGap;
            // return padding + heights.getRange(0, rowStart).sum + gaps;
            return padding + heights.getRange_sum(0, rowStart) + gaps;
        }
    }

    class _TableRenderObject : RenderBoxContainerDefaultsMixin<RenderBox, _TableCellData>
    {
        float? _calculatedHeight;
        float? _calculatedWidth;

        public Border _border;
        bool _borderCollapse;
        float _borderSpacing;
        HtmlTableCompanion _companion;

        public _TableRenderObject
        (
            Border border,
            bool borderCollapse,
            float borderSpacing,
            HtmlTableCompanion companion
        )
        {
            _border = border;
            _borderCollapse = borderCollapse;
            _borderSpacing = borderSpacing;
            _companion = companion;
        }

        public Border border
        {
            get => _border;
            set
            {
                if (value == _border) return;
                _border = value;
                markNeedsLayout();
            }
        }

        public bool borderCollapse
        {
            get => _borderCollapse;
            set
            {
                if (value == _borderCollapse) return;
                _borderCollapse = value;
                markNeedsLayout();
            }
        }

        public HtmlTableCompanion companion
        {
            get => _companion;
            set
            {
                if (value == _companion) return;
                _companion = value;
                markNeedsLayout();
            }
        }

        public float borderSpacing
        {
            get => _borderSpacing;
            set
            {
                if (value == _borderSpacing) return;
                _borderSpacing = value;
                markNeedsLayout();
            }
        }

        public float columnGap =>
            (_border != null && _borderCollapse)
                ? (_border.left.width * -1.0f)
                : _borderSpacing;


        public float paddingBottom => _border?.bottom.width ?? 0f;

        public float paddingLeft => _border?.left.width ?? 0f;

        public float paddingRight => _border?.right.width ?? 0f;

        public float paddingTop => _border?.top.width ?? 0f;

        public float rowGap =>
            (_border != null && _borderCollapse)
                ? (_border.top.width * -1.0f)
                : _borderSpacing;

        // @override
        // Size computeDryLayout(BoxConstraints constraints) =>
        // _performLayout(this, firstChild!, constraints, _performLayoutDry);


        public override float? computeDistanceToActualBaseline(TextBaseline baseline)
        {
            // return base.computeDistanceToActualBaseline(baseline);//仅仅做了一个断言

            D.assert(!debugNeedsLayout);
            float? result = null;

            var child = firstChild;
            while (child != null)
            {
                var data = child.parentData as _TableCellData;
                // only compute cells in the first row
                if (data.rowStart != 0) continue;

                var candidate = child.getDistanceToActualBaseline(baseline);
                if (candidate != null)
                {
                    candidate += data.offset.dy;
                    if (result != null)
                    {
                        // result = min(result, candidate);
                        result = Mathf.Min(result.Value, candidate.Value);
                    }
                    else
                    {
                        result = candidate;
                    }
                }

                child = data.nextSibling;
            }

            return result;
        }

        public new Size getDryLayout(BoxConstraints constraints)
        {
            return _performLayout(this, firstChild, constraints, _performLayoutDry);
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null)
        {
            // return base.hitTestChildren(result, position);
            return defaultHitTestChildren(result, position: position);
        }


        public override void paint(PaintingContext context, Offset offset)
        {
            // base.paint(context, offset);
            _companion._baselines.Clear();

            D.assert(_calculatedHeight != null);
            D.assert(_calculatedWidth != null);

            _border?.paint(
                context.canvas,
                Rect.fromLTWH(offset.dx, offset.dy, _calculatedWidth ?? 0f,
                    _calculatedHeight ?? 0f));

            var child = firstChild;  
            while (child != null)
            {
                var data = child.parentData as _TableCellData;
                var childOffset = data.offset + offset;
                var childSize = child.size;
                // UnityEngine.Debug.Log($"table cell childoffset ={offset}");
                context.paintChild(child, childOffset);

                data.border?.paint
                (
                    context.canvas,
                    Rect.fromLTWH
                    (
                        childOffset.dx,
                        childOffset.dy,
                        childSize.width,
                        childSize.height
                    )
                );

                child = data.nextSibling;
            }
        }

        protected override void performLayout()
        {
            // base.performLayout(); //dart中没有调用super. base中只是一个断言而已
            var calculatedSize = _performLayout(this, firstChild, constraints, _performLayoutLayouter);
            _calculatedHeight = calculatedSize.height;
            _calculatedWidth = calculatedSize.width;
            size = constraints.constrain(calculatedSize);
        }

        public override void setupParentData(RenderObject child)
        {
            if (!(child.parentData is _TableCellData))
            {
                child.parentData = new _TableCellData();
            }
        }


        static Size _performLayout
        (
            _TableRenderObject tro,
            RenderBox firstChild,
            BoxConstraints constraints,
            layouter layouter
        )
        {
            var children = new List<RenderBox>();
            var cells = new List<_TableCellData>();

            RenderBox child = firstChild;
            var columnCount = 0;
            var rowCount = 0;
            while (child != null)
            {
                var data = child.parentData as _TableCellData;
                children.Add(child);
                cells.Add(data);

                columnCount = Mathf.Max(columnCount, data.columnStart + data.columnSpan);
                rowCount = Mathf.Max(rowCount, data.rowStart + data.rowSpan);

                child = data.nextSibling;
            }

            var columnGaps = (columnCount + 1) * tro.columnGap;
            var rowGaps = (rowCount + 1) * tro.rowGap;
            var width0 = (constraints.maxWidth -
                          tro.paddingLeft -
                          tro.paddingRight -
                          columnGaps) /
                         columnCount;
            var childSizes = ListEx.filled(children.length(), Size.zero);
            var columnWidths = ListEx.filled(columnCount, 0f);
            var rowHeights = ListEx.filled(rowCount, 0f);
            for (var i = 0; i < children.length(); i++)
            {
                var childInLoop = children[i];
                var data = cells[i];

                // assume even distribution of column widths if width is finite
                var childColumnGaps = (data.columnSpan - 1) * tro.columnGap;
                float? childWidth = null;

                if (width0.isFinite())
                {
                    childWidth = width0 * data.columnSpan + childColumnGaps;
                }

                var cc = new BoxConstraints(
                    // maxWidth: childWidth ?? double.infinity,
                    maxWidth: childWidth ?? float.PositiveInfinity,
                    minWidth: childWidth ?? 0.0f
                );
                var childSize = childSizes[i] = layouter(childInLoop, cc);

                // distribute cell width across spanned columns
                var columnWidth = (childSize.width - childColumnGaps) / data.columnSpan;
                for (var c = 0; c < data.columnSpan; c++)
                {
                    var column = data.columnStart + c;
                    columnWidths[column] = Mathf.Max(columnWidths[column], columnWidth);
                }

                // distribute cell height across spanned rows
                var childRowGaps = (data.rowSpan - 1) * tro.rowGap;
                var rowHeight = (childSize.height - childRowGaps) / data.rowSpan;
                for (var r = 0; r < data.rowSpan; r++)
                {
                    var row = data.rowStart + r;
                    rowHeights[row] = Mathf.Max(rowHeights[row], rowHeight);
                }
            }

            // we now know all the widths and heights, let's position cells
            // sometime we have to relayout child, e.g. stretch its height for rowspan
            var calculatedHeight =
                tro.paddingTop + rowHeights.sum() + rowGaps + tro.paddingBottom;
            var constraintedHeight = constraints.constrainHeight(calculatedHeight);
            var deltaHeight =
                Mathf.Max(0, (constraintedHeight - calculatedHeight) / rowCount);
            var calculatedWidth =
                tro.paddingLeft + columnWidths.sum() + columnGaps + tro.paddingRight;
            var constraintedWidth = constraints.constrainWidth(calculatedWidth);
            var deltaWidth = (constraintedWidth - calculatedWidth) / columnCount;
            for (var i = 0; i < children.length(); i++)
            {
                var childInLoop = children[i];
                var data = cells[i];
                var childSize = childSizes[i];

                var childHeight = data.calculateHeight(tro, rowHeights) + deltaHeight;
                var childWidth = data.calculateWidth(tro, columnWidths) + deltaWidth;
                if (childSize.height != childHeight || childSize.width != childWidth)
                {
                    var cc2 = BoxConstraints.tight(new Size(childWidth, childHeight));
                    layouter(childInLoop, cc2);
                }

                if (childInLoop.hasSize)
                {
                    data.offset = new Offset(
                        data.calculateX(tro, columnWidths),
                        data.calculateY(tro, rowHeights)
                    );
                    // Debug.Log($"htmlListTable set offset={data.offset}");
                }
            }

            return new Size(calculatedWidth, calculatedHeight);
        }

        static Size _performLayoutDry
        (
            RenderBox renderBox,
            BoxConstraints constraints
        )
        {
            return renderBox.getDryLayout(constraints);
        }

        static Size _performLayoutLayouter
        (
            RenderBox renderBox,
            BoxConstraints constraints
        )
        {
            renderBox.layout(constraints, parentUsesSize: true);
            return renderBox.size;
        }


        // 2021年8月13日17:28:37 跑去搬运 _TableCellData
    }

    class _ValignBaselineRenderObject : RenderProxyBox
    {
        HtmlTableCompanion _companion;
        int _row;
        float? _baselineWithOffset;
        float _paddingTop = 0f;

        public HtmlTableCompanion companion
        {
            get { return _companion; }
            set
            {
                if (value == _companion) return;
                _companion = value;
                markNeedsLayout();
            }
        }

        public int row
        {
            get => _row;
            set
            {
                if (value == _row) return;
                _row = value;
                markNeedsLayout();
            }
        }


        public _ValignBaselineRenderObject
        (
            HtmlTableCompanion companion,
            int row,
            RenderBox child = null
        ) : base(child)
        {
            _companion = companion;
            _row = row;
        }

        //TODO 取代 override computeDryLayout
        public new Size getDryLayout(BoxConstraints constraints)
        {
            return _performLayout(child, _paddingTop, constraints, _performLayoutDry);
        }

        float? GetMaxBaselineValue(List<_ValignBaselineRenderObject> list)
        {
            float maxValue = float.MinValue;
            foreach (var renderObject in list)
            {
                maxValue = Mathf.Max(maxValue, renderObject._baselineWithOffset.Value);
            }

            return maxValue;
        }

        public override void paint(PaintingContext context, Offset offset)
        {
            // base.paint(context, offset);//dart语言中,没有调用基类的paint
            offset = offset.translate(0, _paddingTop);

            var child = this.child;
            if (child == null) return;

            var baselineWithOffset
                = _baselineWithOffset
                    =
                    offset.dy +
                    (child.getDistanceToBaseline(TextBaseline.alphabetic) ?? 0.0f);

            var siblings = _companion._baselines;
            if (siblings.ContainsKey(_row))
            {
                // var rowBaseline = siblings[_row]
                //     .map((e) => e._baselineWithOffset)
                //      .reduce((v, e) => Mathf.Max(v, e)); //避免linq迭代多次, 故调整.
                var rowBaseline = GetMaxBaselineValue(siblings[_row]);
                siblings[_row].Add(this);
                if (rowBaseline > baselineWithOffset)
                {
                    var offsetY = rowBaseline - baselineWithOffset;
                    if (size.height - child.size.height >= offsetY)
                    {
                        // paint child with additional offset
                        context.paintChild(child, offset.translate(0, offsetY.Value));
                        return;
                    }
                    else
                    {
                        // skip painting this frame, wait for the correct padding
                        _paddingTop += offsetY.Value;
                        _baselineWithOffset = rowBaseline;
                        WidgetsBinding.instance
                            ?.addPostFrameCallback((_) => markNeedsLayout());
                        return;
                    }
                }
                else if (rowBaseline < baselineWithOffset)
                {
                    foreach (var sibling in siblings[_row])
                    {
                        if (sibling == this) continue;

                        var offsetY = baselineWithOffset - sibling._baselineWithOffset;
                        if (offsetY != 0.0)
                        {
                            sibling._paddingTop += offsetY.Value;
                            sibling._baselineWithOffset = baselineWithOffset;
                            WidgetsBinding.instance
                                ?.addPostFrameCallback((_) => sibling.markNeedsLayout());
                        }
                    }
                }
            }
            else
            {
                siblings[_row] = new List<_ValignBaselineRenderObject>(1) {this};
            }

            context.paintChild(child, offset);
        }

        protected override void performLayout()
        {
            size = _performLayout
            (
                child,
                _paddingTop,
                constraints,
                _performLayoutLayouter
            );
        }

        delegate Size layouter(RenderBox renderBox, BoxConstraints constraints);


        static Size _performLayout
        (
            RenderBox child,
            float paddingTop,
            BoxConstraints constraints,
            layouter layouter
        )
        {
            var cc = constraints.loosen().deflate(EdgeInsets.only(top: paddingTop));
            var childSize = layouter(child, cc) ?? Size.zero;
            return constraints.constrain(childSize + new Offset(0, paddingTop));
        }

        static Size _performLayoutDry
        (
            RenderBox renderBox,
            BoxConstraints constraints
        )
        {
            return renderBox?.getDryLayout(constraints);
        }

        static Size _performLayoutLayouter
        (
            RenderBox renderBox, BoxConstraints constraints
        )
        {
            renderBox.layout(constraints, parentUsesSize: true);
            return renderBox.size;
        }
    }
}