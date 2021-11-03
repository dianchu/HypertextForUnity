using System.Collections.Generic;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace WidgetFromHtml.Core
{
    internal static class style_padding
    {
        public static WidgetPlaceholder _paddingInlineAfter(TextStyleBuilder tsb, CssLengthBox box)
        {
            Widget Builder(BuildContext context, Widget widget)
            {
                return _paddingInlineSizedBox(box.getValueRight(tsb.build(context)));
            }

            return new WidgetPlaceholder(box).wrapWith(Builder);
        }

        public static WidgetPlaceholder _paddingInlineBefore(TextStyleBuilder tsb, CssLengthBox b)
        {
            Widget Builder(BuildContext context, Widget widget)
            {
                return _paddingInlineSizedBox(b.getValueLeft(tsb.build(context)));
            }

            return new WidgetPlaceholder(b).wrapWith(Builder);
        }

        public static Widget _paddingInlineSizedBox(float? width)
        {
            return (width != null && width > 0)
                ? new SizedBox(width: width)
                : Helper.widget0;
        }
    }

    internal class StylePadding
    {
        const int kPriorityBoxModel3k = 3000;
        public WidgetFactory wf;

        public StylePadding(WidgetFactory wf)
        {
            this.wf = wf;
        }


        public BuildOp buildOp =>
            new BuildOp
            (
                onTree: ONTree,
                onWidgets: ONWidgets,
                onWidgetsIsOptional: true,
                priority: kPriorityBoxModel3k
            );

        private IEnumerable<Widget> ONWidgets(AbsBuildMetadata meta, IEnumerable<WidgetPlaceholder> widgets)
        {
            if (meta.willBuildSubtree == false) return widgets;
            if (widgets.isEmpty()) return widgets;

            var padding = core_parser.tryParseCssLengthBox(meta, Const.kCssPadding);
            if (padding == null) return null;

            return new[]
            {
                new WidgetPlaceholder
                (
                    padding,
                    wf.buildColumnPlaceholder(meta, widgets)
                ).wrapWith((context, child) => _build(meta, context, child, padding))
            };
        }

        private void ONTree(AbsBuildMetadata meta, AbsBuildTree tree)
        {
            if (meta.willBuildSubtree == true) return;
            var padding = core_parser.tryParseCssLengthBox(meta, Const.kCssPadding);
            if (padding == null || !padding.hasPositiveLeftOrRight) return;

            core_ops.wrapTree(
                tree,
                append: (p) =>
                    WidgetBit.inline(p, style_padding._paddingInlineAfter(p.tsb, padding)),
                prepend: (p) =>
                    WidgetBit.inline(p, style_padding._paddingInlineBefore(p.tsb, padding))
            );
        }

        Widget _build
        (
            AbsBuildMetadata meta,
            BuildContext context,
            Widget child,
            CssLengthBox padding
        )
        {
            var tsh = meta.tsb.build(context);
            return wf.buildPadding
            (
                meta,
                child,
                EdgeInsets.fromLTRB
                (
                    Mathf.Max(padding.getValueLeft(tsh) ?? 0f, 0f),
                    Mathf.Max(padding.top?.getValue(tsh) ?? 0f, 0f),
                    Mathf.Max(padding.getValueRight(tsh) ?? 0f, 0f),
                    Mathf.Max(padding.bottom?.getValue(tsh) ?? 0f, 0f)
                )
            );
        }
    }
}