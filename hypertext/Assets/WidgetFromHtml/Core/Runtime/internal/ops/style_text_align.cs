using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace WidgetFromHtml.Core
{
    internal class StyleTextAlign
    {
        public WidgetFactory wf;
        public string value;

        public StyleTextAlign(WidgetFactory wf, string value)
        {
            this.wf = wf;
            this.value = value;
        }

        public BuildOp op
        {
            get
            {
                return new BuildOp
                (
                    onTree: ONTree,
                    onWidgets: ONWidgets,
                    onWidgetsIsOptional: true,
                    priority: 4200
                );
            }
        }

        private IEnumerable<Widget> ONWidgets(AbsBuildMetadata meta, IEnumerable<WidgetPlaceholder> widgets)
        {
            return _onWidgets(widgets, value);
        }

        private void ONTree(AbsBuildMetadata meta, AbsBuildTree tree)
        {
            meta.tsb.enqueue(_tsb, value);
        }

        static IEnumerable<Widget> _onWidgets(IEnumerable<Widget> widgets, string value)
        {
            switch (value)
            {
                case Const.kCssTextAlignCenter:
                case Const.kCssTextAlignEnd:
                case Const.kCssTextAlignJustify:
                case Const.kCssTextAlignLeft:
                case Const.kCssTextAlignRight:
                    widgets = widgets.map(selector_alignRight);
                    break;
                case Const.kCssTextAlignMozCenter:
                case Const.kCssTextAlignWebkitCenter:
                    widgets = widgets.map(selector_AlignWebkitCenter);
                    break;
            }

            return widgets;
        }

        private static Widget selector_AlignWebkitCenter(Widget child)
        {
            return WidgetPlaceholder.lazy(child).wrapWith(_center);
        }

        private static Widget selector_alignRight(Widget child)
        {
            return WidgetPlaceholder.lazy(child).wrapWith(_block);
        }


        static Widget _block(BuildContext _, Widget child)
        {
            if (child is CssBlock)
            {
                return child;
            }
            else
            {
                return new _TextAlignBlock(child);
            }
        }

        static Widget _center(BuildContext _, Widget child) => new _TextAlignCenter(child);

        static TextStyleHtml _tsb(TextStyleHtml tsh, string value)
        {
            TextAlign? textAlign = null;

            switch (value)
            {
                case Const.kCssTextAlignCenter:
                case Const.kCssTextAlignMozCenter:
                case Const.kCssTextAlignWebkitCenter:
                    textAlign = TextAlign.center;
                    break;
                case Const.kCssTextAlignEnd:
                    textAlign = TextAlign.end;
                    break;
                case Const.kCssTextAlignJustify:
                    textAlign = TextAlign.justify;
                    break;
                case Const.kCssTextAlignLeft:
                    textAlign = TextAlign.left;
                    break;
                case Const.kCssTextAlignRight:
                    textAlign = TextAlign.right;
                    break;
                case Const.kCssTextAlignStart:
                    textAlign = TextAlign.start;
                    break;
            }

            return textAlign == null ? tsh : tsh.copyWith(textAlign: textAlign);
        }
    }

    class _TextAlignBlock : CssBlock
    {
        public _TextAlignBlock
        (
            Widget child,
            Key key = null
        ) : base(child: child, key: key)
        {
        }
    }

    class _TextAlignCenter : Center
    {
        public _TextAlignCenter
        (
            Widget child,
            Key key = null
        )
            : base(child: child, heightFactor: 1.0f, key: key)
        {
        }
    }
}