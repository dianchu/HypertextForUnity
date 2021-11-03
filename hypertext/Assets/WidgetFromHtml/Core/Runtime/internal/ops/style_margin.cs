using System;
using System.Collections.Generic;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace WidgetFromHtml.Core
{
    internal class StyleMargin
    {
        Widget _marginHorizontalBuilder
        (
            Widget w,
            CssLengthBox b,
            TextStyleHtml tsh
        )
        {
            return new Padding(
                padding: EdgeInsets.only(
                    left: Mathf.Max(b.getValueLeft(tsh) ?? 0f, 0f),
                    right: Mathf.Max(b.getValueRight(tsh) ?? 0f, 0f)
                ),
                child: w
            );
        }

        const int kPriorityBoxModel9k = 9000;

        public WidgetFactory wf;

        public StyleMargin(WidgetFactory wf)
        {
            this.wf = wf;
        }


        public BuildOp buildOp =>
            new BuildOp(
                onTree: ONTree,
                onWidgets: ONWidgets,
                onWidgetsIsOptional: true,
                priority: kPriorityBoxModel9k
            );

        private void ONTree(AbsBuildMetadata meta, AbsBuildTree tree)
        {
            if (meta.willBuildSubtree == true) return;
            var m = core_parser.tryParseCssLengthBox(meta, Const.kCssMargin);
            if (m == null || !m.hasPositiveLeftOrRight) return;

            core_ops.wrapTree
            (
                tree,
                append: (p) => WidgetBit.inline(p, style_padding._paddingInlineAfter(p.tsb, m)),
                prepend: (p) => WidgetBit.inline(p, style_padding._paddingInlineBefore(p.tsb, m))
            );
        }

        private IEnumerable<Widget> ONWidgets(AbsBuildMetadata meta, IEnumerable<WidgetPlaceholder> widgets)
        {
            if (meta.willBuildSubtree == false) return widgets;
            if (widgets.isEmpty()) return widgets;

            var m = core_parser.tryParseCssLengthBox(meta, Const.kCssMargin);
            if (m == null) return null;
            var tsb = meta.tsb;

            var retList = new List<Widget>();


            if (m.top != null && m.top.Value.isPositive())
            {
                retList.Add(new HeightPlaceholder(m.top.Value, tsb));
            }

            foreach (var widget in widgets)
            {
                if (m.hasPositiveLeftOrRight)
                {
                    var widgetPlaceholder = widget.wrapWith(
                        (c, w) => _marginHorizontalBuilder(w, m, tsb.build(c)));
                    retList.Add(widgetPlaceholder);
                }

                else
                {
                    retList.Add(widget);
                }
            }

            if (m.bottom != null && m.bottom.Value.isPositive())
            {
                retList.Add(new HeightPlaceholder(m.bottom.Value, tsb));
            }


            return retList;
        }
    }
}