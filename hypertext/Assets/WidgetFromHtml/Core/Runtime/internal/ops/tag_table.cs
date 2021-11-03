using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace WidgetFromHtml.Core
{
    internal class TagTable
    {
        public HtmlTableCompanion companion = new HtmlTableCompanion();
        public AbsBuildMetadata tableMeta;
        public WidgetFactory wf;
        public List<AbsBuildTree> _captions = new List<AbsBuildTree>();
        public _TagTableData _data = new _TagTableData();
        BuildOp _tableOp;


        public TagTable(WidgetFactory wf, AbsBuildMetadata tableMeta)
        {
            this.tableMeta = tableMeta;
            this.wf = wf;
        }

        public BuildOp op
        {
            get
            {
                if (_tableOp == null)
                {
                    _tableOp = new BuildOp(
                        onChild: onChild,
                        onTree: onTree,
                        onWidgets: onWidgets,
                        priority: 0
                    );
                }

                return _tableOp;
            }
        }


        void onChild(AbsBuildMetadata childMeta)
        {
            if (childMeta.element.Parent != tableMeta.element) return;

            var which = _getCssDisplayValue(childMeta);
            _TagTableDataGroup latestGroup = null;
            switch (which)
            {
                case Const.kCssDisplayTableRow:
                    if (latestGroup == null)
                    {
                        latestGroup = _data.body;
                    }

                    var row = new _TagTableDataRow();
                    latestGroup.rows.Add(row);
                    childMeta.register(new _TagTableRow(this, childMeta, row).op);
                    break;
                case Const.kCssDisplayTableHeaderGroup:
                case Const.kCssDisplayTableRowGroup:
                case Const.kCssDisplayTableFooterGroup:
                    var rows = which == Const.kCssDisplayTableHeaderGroup
                        ? _data.header.rows
                        : which == Const.kCssDisplayTableRowGroup
                            ? _data.body.rows
                            : _data.footer.rows;
                    childMeta.register(new _TagTableRowGroup(this, rows, childMeta).op);
                    latestGroup = null;
                    break;
                case Const.kCssDisplayTableCaption:
                    childMeta.register(new BuildOp(onTree: (_, tree) => _captions.Add(tree)));
                    break;
            }
        }

        void onTree(AbsBuildMetadata __, AbsBuildTree tree)
        {
            StyleBorder.skip(tableMeta);
            StyleSizing.treatHeightAsMinHeight(tableMeta);

            foreach (var caption in _captions)
            {
                var built = wf
                    .buildColumnPlaceholder(tableMeta, caption.build())
                    ?.wrapWith
                    (
                        (_, child) => new _TableCaption(child)
                    );
                if (built != null)
                {
                    WidgetBit.block(tree.parent, built).insertBefore(tree);
                }

                caption.detach();
            }
        }

        IEnumerable<Widget> onWidgets(AbsBuildMetadata ___, IEnumerable<WidgetPlaceholder> __)
        {
            var builders = new List<_HtmlTableCellBuilder>();

            // var occupations = <int, Map<int, bool> >{ } ;
            var occupations = new Dictionary<int, Dictionary<int, bool>>();
            _prepareHtmlTableCellBuilders(_data.header, occupations, builders);
            foreach (var body in _data.bodies)
            {
                _prepareHtmlTableCellBuilders(body, occupations, builders);
            }

            _prepareHtmlTableCellBuilders(_data.footer, occupations, builders);
            // if (builders.isEmpty()) return [] ;
            if (builders.isEmpty()) new List<Widget>(0);

            var border = core_parser.tryParseBorder(tableMeta);
            var borderCollapse = tableMeta[Const.kCssBorderCollapse]?.Value;
            var borderSpacingExpression = tableMeta[Const.kCssBorderSpacing]?.RawValue;
            var borderSpacing = borderSpacingExpression != null
                ? core_parser.tryParseCssLength(borderSpacingExpression)
                : null;


            var palceHolder = new WidgetPlaceholder(tableMeta)
                .wrapWith
                (
                    (context, _) =>
                    {
                        var tsh = tableMeta.tsb.build(context);
                        var children = new List<Widget>(builders.Count);
                        for (int i = 0; i < builders.Count; i++)
                        {
                            var builder = builders[i];
                            var w = builder(context) as Widget;
                            if (w != null)
                            {
                                children.Add(w);
                            }
                        }
                    
                        return new HtmlTable(
                            border: border.getValue(tsh),
                            borderCollapse: borderCollapse == Const.kCssBorderCollapseCollapse,
                            borderSpacing: borderSpacing?.getValue(tsh) ?? 0.0f,
                            companion: companion,
                            children: children
                            // builders
                            //     .map((f) => f(context) as Widget)
                            //     .Where((element) => element != null).ToList()
                        );
                    }
                );
            return new List<Widget>(1)
            {
                palceHolder
            };
        }


        void _prepareHtmlTableCellBuilders
        (
            _TagTableDataGroup group,
            Dictionary<int, Dictionary<int, bool>> occupations,
            List<_HtmlTableCellBuilder> builders
        )
        {
            // var rowStart = occupations.keys.length - 1;
            var rowStart = occupations.Keys.length() - 1;
            var rowSpanMax = group.rows.length();
            foreach (var row in group.rows)
            {
                rowStart++;
                
                // occupations[rowStart] ??=  { } ;
                if (!occupations.ContainsKey(rowStart))
                {
                    occupations.Add(rowStart, new Dictionary<int, bool>());
                }
    
                foreach (var cell in row.cells)
                {
                    var columnStart = 0;
                    while (occupations[rowStart].ContainsKey(columnStart))
                    {
                        columnStart++;
                    }

                    var columnSpan = cell.colspan > 0 ? cell.colspan : 1;
                    var rowSpan = Mathf.Min(
                        rowSpanMax,
                        cell.rowspan > 0
                            ? cell.rowspan
                            : cell.rowspan == 0
                                ? group.rows.length()
                                : 1);
                    for (var r = 0; r < rowSpan; r++)
                    {
                        var rowInSpan = rowStart + r;
                        // occupations[rowInSpan] ??=  { } ;
                        if (!occupations.ContainsKey(rowInSpan))
                        {
                            occupations.Add(rowInSpan, new Dictionary<int, bool>());
                        }

                        for (var c = 0; c < columnSpan; c++)
                        {
                            occupations[rowInSpan][columnStart + c] = true;
                        }
                    }

                    var cellMeta = cell.meta;
                    cellMeta.row = new INT(rowStart);


                    var cssBorderParsed = core_parser.tryParseBorder(cellMeta);
                    var cssBorder = cssBorderParsed.inherit
                        ? core_parser.tryParseBorder(tableMeta).copyFrom(cssBorderParsed)
                        : cssBorderParsed;

                    builders.Add(context =>
                        {
                            Widget child = cell.child;

                            var border = cssBorder.getValue(cellMeta.tsb.build(context));
                            if (border != null)
                            {
                                child = wf.buildPadding(cellMeta, cell.child, border.dimensions);
                            }

                            if (child == null) return null;
                            return new HtmlTableCell
                            (
                                border: border,
                                columnSpan: columnSpan,
                                columnStart: columnStart,
                                rowSpan: rowSpan,
                                rowStart: cellMeta.row,
                                child: child
                            );
                        }
                    );
                }
            }
        }

        private static Dictionary<string, string> getDefaultStyles(float border, float borderSpacing)
        {
            return new Dictionary<string, string>()
            {
                {Const.kCssBorder, $"{border}px solid black"},
                {Const.kCssBorderCollapse, Const.kCssBorderCollapseSeparate},
                {Const.kCssBorderSpacing, $"{borderSpacing}px"},
            };
        }


        public static BuildOp cellPaddingOp(float px)
        {
            return new BuildOp
            (
                onChild: meta =>
                {
                    if (meta.element.LocalName == "td" || meta.element.LocalName == "th")
                    {
                        meta.AddOneCssPropertyStyle(Const.kCssPadding, $"{px}px");
                    }
                }
            );
        }

        public static BuildOp borderOp(float border, float borderSpacing)
        {
            void ONChild_BuildOp(AbsBuildMetadata meta)
            {
                switch (meta.element.LocalName)
                {
                    case Const.kTagTableCell:
                    case Const.kTagTableHeaderCell:
                        // meta[Const.kCssBorder] = Const.kCssBorderInherit;
                        meta.AddOneCssPropertyStyle(Const.kCssBorder, Const.kCssBorderInherit);
                        break;
                }
            }

            if (border > 0)
            {
                return new BuildOp
                (
                    defaultStyles: _ => getDefaultStyles(border, borderSpacing),
                    onChild: ONChild_BuildOp
                );
            }
            else
            {
                return new BuildOp
                (
                    defaultStyles: _ => getDefaultStyles(border, borderSpacing)
                );
            }
        }


        public static string _getCssDisplayValue(AbsBuildMetadata meta)
        {
            // for (final style in meta.element.styles.reversed) 
            // {
            //     if (style.property == Const.kCssDisplay)
            //     {
            //         var term = style.term;
            //         if (term != null) return term;
            //     }
            // }

            //逆序遍历
            var styles = meta.element.styles();
            for (int i = styles.Count - 1; i >= 0; i--)
            {
                var style = styles[i];
                if (style.Name == Const.kCssDisplay)
                {
                    var term = style.Value;
                    if (term != null) return term;
                }
            }

            switch (meta.element.LocalName)
            {
                case Const.kTagTableRow:
                    return Const.kCssDisplayTableRow;
                case Const.kTagTableHeaderGroup:
                    return Const.kCssDisplayTableHeaderGroup;
                case Const.kTagTableRowGroup:
                    return Const.kCssDisplayTableRowGroup;
                case Const.kTagTableFooterGroup:
                    return Const.kCssDisplayTableFooterGroup;
                case Const.kTagTableHeaderCell:
                case Const.kTagTableCell:
                    return Const.kCssDisplayTableCell;
                case Const.kTagTableCaption:
                    return Const.kCssDisplayTableCaption;
            }

            return null;
        }
    }


    class _TableCaption : SingleChildRenderObjectWidget
    {
        public _TableCaption
        (
            Widget child,
            Key key = null
        ) : base(key, child)
        {
        }

        public override RenderObject createRenderObject(BuildContext context)
        {
            return new RenderProxyBox();
        }
    }


    //这边的抽象类扩展,直接给搬到 AbsBuildMetadata 里边去了. 就是row的get set属性
    // extension _BuildMetadataExtension on BuildMetadata {
    // static final _rows = Expando<int>();
    //
    // set row(int v) => _rows[this] = v;
    // int get row => _rows[this] ?? -1;
    // }
    internal abstract partial class AbsBuildMetadata
    {
        private static ConditionalWeakTable<AbsBuildMetadata, INT> _rows =
            new ConditionalWeakTable<AbsBuildMetadata, INT>();

        public INT row
        {
            get
            {
                if (_rows.TryGetValue(this, out var i))
                {
                    return i;
                }
                else
                {
                    return new INT(-1);
                }
            }
            set
            {
                if (_rows.TryGetValue(this, out var i))
                {
                    i.IntNumber = value;
                }
                else
                {
                    _rows.Add(this, new INT(value));
                }
            }
        }
    }


    internal delegate HtmlTableCell _HtmlTableCellBuilder(BuildContext context);

    class _TagTableRow
    {
        public BuildOp op;
        public TagTable parent;
        public _TagTableDataRow row;
        public AbsBuildMetadata rowMeta;
        public BuildOp _cellOp;
        public BuildOp _valignBaselineOp;

        public _TagTableRow
        (
            TagTable parent,
            AbsBuildMetadata rowMeta,
            _TagTableDataRow row
        )
        {
            this.parent = parent;
            this.row = row;
            this.rowMeta = rowMeta;

            op = new BuildOp(onChild: onChild);

            _cellOp = new BuildOp
            (
                onWidgets: ONWidgets_cell,
                priority: Const.kPriorityMax
            );

            _valignBaselineOp = new BuildOp
            (
                onWidgets: ONWidgets_valignBaselineOp,
                priority: Const.kPriority4k3
            );
        }

        private IEnumerable<Widget> ONWidgets_valignBaselineOp(AbsBuildMetadata cellMeta,
            IEnumerable<WidgetPlaceholder> widgets)
        {
            var v = cellMeta[Const.kCssVerticalAlign]?.Value;
            if (v != Const.kCssVerticalAlignBaseline) return widgets;
            return Helper.listOrNull
            (parent.wf
                .buildColumnPlaceholder(cellMeta, widgets)
                ?.wrapWith((_, child) =>
                {
                    var row = cellMeta.row.IntNumber;

                    return new HtmlTableValignBaseline
                    (
                        companion: parent.companion,
                        row: row,
                        child: child
                    );
                }));
        }

        private IEnumerable<Widget> ONWidgets_cell(AbsBuildMetadata cellMeta, IEnumerable<WidgetPlaceholder> widgets)
        {
            var child =
                parent.wf.buildColumnPlaceholder(cellMeta, widgets) ?? Helper.widget0;

            var attributes = cellMeta.element.Attributes;
            row.cells.Add(new _TagTableDataCell
            (
                meta: cellMeta,
                child: child,
                colspan: Helper.tryParseIntFromMap(attributes, Const.kAttributeColspan) ?? 1,
                rowspan: Helper.tryParseIntFromMap(attributes, Const.kAttributeRowspan) ?? 1
            ));

            // return [child]
            return new List<Widget>(1) {child};
        }

        void onChild(AbsBuildMetadata childMeta)
        {
            if (childMeta.element.Parent != rowMeta.element) return;
            if (TagTable._getCssDisplayValue(childMeta) != Const.kCssDisplayTableCell)
            {
                return;
            }

            var attrs = childMeta.element.Attributes;
            if (attrs.containsKey(Const.kAttributeValign))
            {
                childMeta.AddOneCssPropertyStyle(Const.kCssVerticalAlign, attrs[Const.kAttributeValign].Value);
                // childMeta[kCssVerticalAlign] = attrs[kAttributeValign]!;
            }

            childMeta.register(_cellOp);
            StyleBorder.skip(childMeta);
            StyleSizing.treatHeightAsMinHeight(childMeta);
            childMeta.register(_valignBaselineOp);
        }
    }

    class _TagTableRowGroup
    {
        public TagTable parent;
        public List<_TagTableDataRow> rows;
        public AbsBuildMetadata groupMeta;
        public BuildOp op;

        public _TagTableRowGroup(TagTable parent, List<_TagTableDataRow> rows, AbsBuildMetadata groupMeta)
        {
            this.parent = parent;
            this.rows = rows;
            this.groupMeta = groupMeta;
            op = new BuildOp(onChild: onChild);
        }

        void onChild(AbsBuildMetadata childMeta)
        {
            if (childMeta.element.Parent != groupMeta.element) return;
            if (TagTable._getCssDisplayValue(childMeta) != Const.kCssDisplayTableRow)
            {
                return;
            }

            var row = new _TagTableDataRow();
            rows.Add(row);
            childMeta.register(new _TagTableRow(parent, childMeta, row).op);
        }
    }

    class _TagTableData
    {
        public List<_TagTableDataGroup> bodies = new List<_TagTableDataGroup>();
        public _TagTableDataGroup footer = new _TagTableDataGroup();
        public _TagTableDataGroup header = new _TagTableDataGroup();

        public _TagTableDataGroup body
        {
            get
            {
                var body = new _TagTableDataGroup();
                bodies.Add(body);
                return body;
            }
        }
    }

    class _TagTableDataGroup
    {
        public List<_TagTableDataRow> rows = new List<_TagTableDataRow>();
    }


    class _TagTableDataRow
    {
        public List<_TagTableDataCell> cells = new List<_TagTableDataCell>();
    }

    class _TagTableDataCell
    {
        public Widget child;
        public int colspan;
        public AbsBuildMetadata meta;
        public int rowspan;

        public _TagTableDataCell
        (
            AbsBuildMetadata meta,
            Widget child,
            int colspan,
            int rowspan
        )
        {
            this.child = child;
            this.colspan = colspan;
            this.meta = meta;
            this.rowspan = rowspan;
        }
    }
}