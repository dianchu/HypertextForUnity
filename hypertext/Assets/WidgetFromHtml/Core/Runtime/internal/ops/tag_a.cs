using System;
using System.Collections.Generic;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.widgets;
using Element = AngleSharp.Dom.Element;

namespace WidgetFromHtml.Core
{
    internal class TagA
    {
        public WidgetFactory wf;

        public TagA(WidgetFactory wf)
        {
            this.wf = wf;
        }


        public BuildOp buildOp =>
            new BuildOp
            (
                defaultStyles: DefaultTextStyles,
                onTree: ONTree,
                onWidgets: ONWidgets,
                onWidgetsIsOptional: true
            );

        private Dictionary<string, string> DefaultTextStyles(IElement element)
        {
            return new Dictionary<string, string>()
            {
                { Const.kCssTextDecoration, Const.kCssTextDecorationUnderline },
            };
        }

        private IEnumerable<Widget> ONWidgets(AbsBuildMetadata meta, IEnumerable<WidgetPlaceholder> widgets)
        {
            if (meta.willBuildSubtree == false) return widgets;

            var onTap = _gestureTapCallback(meta);
            if (onTap == null) return widgets;

            return Helper.listOrNull
            (
                wf.buildColumnPlaceholder(meta, widgets)?.wrapWith((context, child) =>
                {
                    return wf.buildGestureDetector(meta, child, onTap);
                })
            );
        }


        private void ONTree(AbsBuildMetadata meta, AbsBuildTree tree)
        {
            if (meta.willBuildSubtree == true) return;

            var onTap = _gestureTapCallback(meta);
            if (onTap == null) return;

            foreach (var bit in tree.bits_toList())
            {
                if (bit is WidgetBit widgetBit)
                {
                    widgetBit.child.wrapWith
                    (
                        (_, child) => wf.buildGestureDetector(meta, child, onTap)
                    );
                }
                // else if (   bit is! WhitespaceBit)
                else if (!(bit is WhitespaceBit)) //bit不是WhitespaceBit
                {
                    new _TagABit(bit.parent, bit.tsb, onTap).insertAfter(bit);
                }
            }
        }


        GestureTapCallback _gestureTapCallback(AbsBuildMetadata meta)
        {
            // var href = meta.element.attributes[kAttributeAHref];
            // var href = meta.element[Const.kAttributeAHref];
            var href = meta.element.GetAttribute(Const.kAttributeAHref);
            return href != null
                ? wf.gestureTapCallback(wf.urlFull(href) ?? href)
                : null;
        }
    }

    class _TagABit : BuildBit
    {
        public GestureTapCallback onTap;

        public _TagABit(AbsBuildTree parent, TextStyleBuilder tsb, GestureTapCallback onTap) : base(parent, tsb)
        {
            this.onTap = onTap;
        }

        public override bool? swallowWhitespace => null;

        public override object buildBit(object recognizer)
        {
            if (recognizer is TapGestureRecognizer recognizer1)
            {
                recognizer1.onTap = onTap;
                return recognizer;
            }

            var ret = new TapGestureRecognizer();
            ret.onTap = onTap;
            return ret;
        }

        public override Type buildBitInputTyp => Const.kTypNull;
        public override Type buildBitOutputTyp => Const.kTypString;

        public override BuildBit copyWith(AbsBuildTree parent = null, TextStyleBuilder tsb = null)
        {
            return new _TagABit(parent ?? this.parent, tsb ?? this.tsb, onTap);
        }

            
    }
}