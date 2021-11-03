using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace WidgetFromHtml.Core
{
    internal class StyleBorder
    {
        public const int kPriorityBoxModel5k = 5000;
        internal readonly WidgetFactory wf;

        //static final _skipBuilding = Expando<bool>();
        static readonly ConditionalWeakTable<AbsBuildMetadata, BOOL> _skipBuilding =
            new ConditionalWeakTable<AbsBuildMetadata, BOOL>();

        public StyleBorder(WidgetFactory wf)
        {
            this.wf = wf;
        }

        public BuildOp buildOp =>
            new BuildOp(
                onTree: ONTree,
                onWidgets: ONWidgets,
                onWidgetsIsOptional: true,
                priority: kPriorityBoxModel5k
            );

        private IEnumerable<Widget> ONWidgets(AbsBuildMetadata meta, IEnumerable<WidgetPlaceholder> widgets)
        {
            if (widgets.isEmpty())
            {
                return widgets;
            }

            if (_skipBuilding.TryGetValue(meta, out var vBool))
            {
                if (vBool == true)
                {
                    return widgets;
                }
            }
            else
            {
                vBool = new BOOL(true);
                _skipBuilding.Add(meta,vBool);
            }
                
            
            
            var border = core_parser.tryParseBorder(meta);
            if (border.isNone) return widgets;
           
            return new[]
            {
                new WidgetPlaceholder
                (
                    border,
                    wf.buildColumnPlaceholder(meta, widgets)
                ).wrapWith((context, child) => _buildBorder(meta, context, child: child, border))
            };
        }

        private void ONTree(AbsBuildMetadata meta, AbsBuildTree tree)
        {
            if (meta.willBuildSubtree == true) return;
            var border = core_parser.tryParseBorder(meta);
            if (border.isNone) return;

            // _skipBuilding[meta] = true;
            if (_skipBuilding.TryGetValue(meta, out var v))
            {
                v.Bool = true;
            }
            else
            {
                _skipBuilding.Add(meta, new BOOL(true));
            }

            var copied = tree.copyWith() as AbsBuildTree;
            var built = wf
                .buildColumnPlaceholder(meta, copied.build())
                ?.wrapWith((context, child) =>
                    _buildBorder(meta, context, child, border));
            if (built == null) return;

            tree.replaceWith(WidgetBit.inline(tree, built));
        }


        public Widget _buildBorder
        (
            AbsBuildMetadata meta,
            BuildContext context,
            Widget child,
            CssBorder border
        )
        {
            var tsh = meta.tsb.build(context);
            var borderValue = border.getValue(tsh);
            if (borderValue == null) return child;
            return wf.buildBorder
            (
                meta,
                child,
                borderValue,
                // isBorderBox: meta[Const.kCssBoxSizing]?.term == kCssBoxSizingBorderBox //处理BuildMetadata的重写[]运算符
                isBorderBox: meta[Const.kCssBoxSizing]?.Name == Const.kCssBoxSizingBorderBox //处理BuildMetadata的重写[]运算符
            );
        }


        public static void skip(AbsBuildMetadata meta)
        {
            if (_skipBuilding.TryGetValue(meta, out var bValue))
            {
#if UNITY_EDITOR
                if (bValue == true)
                {
                    HLog.LogInfo("TODO >>> 底下的断言应该通过的! 有空再查查...");
                }
#endif
               
                // D.assert(bValue != true, () => $"Built ${meta.element} already");
                bValue.Bool = true;
            }
            else
            {
                _skipBuilding.Add(meta, new BOOL(true));
            }
        }
    }
}