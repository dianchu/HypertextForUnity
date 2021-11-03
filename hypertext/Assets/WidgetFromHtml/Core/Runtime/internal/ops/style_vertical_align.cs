using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace WidgetFromHtml.Core
{
    internal class StyleVerticalAlign
    {
        
        public WidgetFactory wf;

        static ConditionalWeakTable<AbsBuildMetadata, BOOL> _skipBuilding
            = new ConditionalWeakTable<AbsBuildMetadata, BOOL>();

        public StyleVerticalAlign(WidgetFactory wf)
        {
            this.wf = wf;
        }


        public BuildOp buildOp =>
            new BuildOp
            (
                onTree: ONTree,
                onWidgets: ONWidgets,
                onWidgetsIsOptional: true,
                priority: Const.kPriority4k3
            );

        private IEnumerable<Widget> ONWidgets(AbsBuildMetadata meta, IEnumerable<WidgetPlaceholder> widgets)
        {
            // if (_skipBuilding[meta] == true || widgets.isEmpty) {
            //     return widgets;
            // }
            if (widgets.isEmpty())
            {
                return widgets;
            }

            if (_skipBuilding.TryGetValue(meta, out var b))
            {
                if (b)
                {
                    return widgets;
                }
            }


            var v = meta[Const.kCssVerticalAlign]?.Value;
            if (v == null) return widgets;

            // _skipBuilding[meta] = true;
            if (_skipBuilding.TryGetValue(meta, out var vBool))
            {
                vBool.Bool = true;
            }
            else
            {
                _skipBuilding.Add(meta, new BOOL(true));
            }

            return Helper.listOrNull(wf
                .buildColumnPlaceholder(meta, widgets)
                ?.wrapWith((context, child) =>
                {
                    var tsh = meta.tsb.build(context);
                    var alignment = style_vertical_align._tryParseAlignmentGeometry(tsh.textDirection, v);
                    if (alignment == null) return child;
                    return wf.buildAlign(meta, child, alignment);
                }));
        }

        private void ONTree(AbsBuildMetadata meta, AbsBuildTree tree)
        {
            if (meta.willBuildSubtree == true) return;

            var v = meta[Const.kCssVerticalAlign]?.Value;
            if (v == null || v == Const.kCssVerticalAlignBaseline) return;

            var alignment = style_vertical_align._tryParsePlaceholderAlignment(v);
            if (alignment == null) return;

            // _skipBuilding[meta] = true;
            if (_skipBuilding.TryGetValue(meta, out var vBool))
            {
                vBool.Bool = true;
            }
            else
            {
                _skipBuilding.Add(meta, new BOOL(true));
            }

            var built = _buildTree(meta, tree);
            if (built == null) return;

            if (v == Const.kCssVerticalAlignSub || v == Const.kCssVerticalAlignSuper)
            {
                built.wrapWith(
                    (context, child) => _buildPaddedAlign(
                        context,
                        meta,
                        child,
                        EdgeInsets.only(
                            bottom: v == Const.kCssVerticalAlignSuper ? 0.4f : 0,
                            top: v == Const.kCssVerticalAlignSub ? 0.4f : 0
                        )
                    )
                );
            }

            tree.replaceWith(WidgetBit.inline(tree, built, alignment: alignment.Value));
        }


        WidgetPlaceholder _buildTree(AbsBuildMetadata meta, AbsBuildTree tree)
        {
            var bits = tree.bits_toList();
            if (bits.length() == 1)
            {
                var firstBit = bits.first();
                if (firstBit is WidgetBit widgetBit)
                {
                    // use the first widget if possible
                    // and avoid creating a redundant `RichText`
                    return widgetBit.child;
                }
            }

            var copied = tree.copyWith() as AbsBuildTree;
            return wf.buildColumnPlaceholder(meta, copied.build());
        }


        Widget _buildPaddedAlign(BuildContext context, AbsBuildMetadata meta,
            Widget child, EdgeInsets padding)
        {
            var tsh = meta.tsb.build(context);
            var fontSize = tsh.style.fontSize;
            if (fontSize == null) return child;

            var withPadding = wf.buildPadding(
                meta,
                child,
                EdgeInsets.only
                (
                    bottom: fontSize.Value * padding.bottom,
                    top: fontSize.Value * padding.top
                )
            );
            if (withPadding == null) return child;

            return wf.buildAlign(
                meta,
                withPadding,
                padding.bottom > 0 ? Alignment.topCenter : Alignment.bottomCenter,
                widthFactor: 1.0f
            );
        }
    }

    public static class style_vertical_align
    {
        public static AlignmentGeometry _tryParseAlignmentGeometry(TextDirection dir, string value)
        {
            var isLtr = dir != TextDirection.rtl;
            switch (value)
            {
                case Const.kCssVerticalAlignTop:
                case Const.kCssVerticalAlignSuper:
                    return isLtr ? Alignment.topLeft : Alignment.topRight;
                case Const.kCssVerticalAlignMiddle:
                    return isLtr ? Alignment.centerLeft : Alignment.centerRight;
                case Const.kCssVerticalAlignBottom:
                case Const.kCssVerticalAlignSub:
                    return isLtr ? Alignment.bottomLeft : Alignment.bottomRight;
            }

            return null;
        }

        public static PlaceholderAlignment? _tryParsePlaceholderAlignment(string value)
        {
            switch (value)
            {
                case Const.kCssVerticalAlignTop:
                case Const.kCssVerticalAlignSub:
                    return PlaceholderAlignment.top;
                case Const.kCssVerticalAlignSuper:
                case Const.kCssVerticalAlignBottom:
                    return PlaceholderAlignment.bottom;
                case Const.kCssVerticalAlignMiddle:
                    return PlaceholderAlignment.middle;
            }

            return null;
        }
    }
}