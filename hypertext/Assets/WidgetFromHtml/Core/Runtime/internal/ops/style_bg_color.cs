using System.Collections.Generic;
using AngleSharp.Css.Dom;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Color = Unity.UIWidgets.ui.Color;

namespace WidgetFromHtml.Core
{
    internal class StyleBgColor
    {
        public readonly WidgetFactory wf;

        public StyleBgColor(WidgetFactory wf)
        {
            this.wf = wf;
        }

        public BuildOp buildOp
        {
            get
            {
                return new BuildOp(
                    onTree: ONTree,
                    onWidgets: ONWidgets,
                    onWidgetsIsOptional: true,
                    priority: 6100
                );
            }
        }

        private IEnumerable<Widget> ONWidgets(AbsBuildMetadata meta, IEnumerable<WidgetPlaceholder> widgets)
        {
            if (meta.willBuildSubtree == false) return widgets;

            var color = _parseColor(wf, meta);
            if (color == null) return null;
            return Helper.listOrNull
            (
                wf.buildColumnPlaceholder(meta, widgets)?.wrapWith
                (
                    (_, child) => wf.buildDecoratedBox(meta, child, color: color)
                )
            );
        }

        private void ONTree(AbsBuildMetadata meta, AbsBuildTree tree)
        {
            if (meta.willBuildSubtree == true) return;

            Color bgColor = _parseColor(wf, meta);
            if (bgColor == null) return;
            
            void OnBitsIterate(BuildBit bit) 
            {
                bit.tsb.enqueue(_tsb, bgColor);
            }
            
            tree.bits(OnBitsIterate);
            
            // foreach (var bit in tree.bits)
            // {
                // bit.tsb.enqueue(_tsb, bgColor);
            // }
        }

    


        Color _parseColor(WidgetFactory wf, AbsBuildMetadata meta)
        {
            Color color = null;
            foreach (var style in meta.styles)
            {
                // switch (style.property)
                switch (style.Name)
                {
                    case Const.kCssBackground:
                        HLog.LogError("不支持background标签,返回黑色");
                        color = Color.black;
                        // color = new Color((uint)style.RawValue.AsArgb());
                        break;
                    case Const.kCssBackgroundColor:
                        color = new Color((uint) style.RawValue.AsArgb());
                        break;
                }
            }

            return color;
        }

        public static TextStyleHtml _tsb(TextStyleHtml p, Color c)
        {
            // public static TextStyleHtml _tsb(object objTextStyleHtml, Color c)
            // {
            // TextStyleHtml p = objTextStyleHtml as TextStyleHtml;

            var paint = new Paint();
            paint.color = c;
            return p.copyWith
            (
                style: p.style.copyWith(background: paint)
            );
        }
    }
}