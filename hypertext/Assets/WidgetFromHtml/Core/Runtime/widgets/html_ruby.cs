using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace WidgetFromHtml.Core
{
    public class HtmlRuby : MultiChildRenderObjectWidget
    {
        public HtmlRuby
        (
            Widget ruby,
            Widget rt,
            Key key = null
        ) : base(key, new List<Widget>(2) { ruby, rt })
        {
        }

        public override RenderObject createRenderObject(BuildContext context)
        {
            return new _RubyRenderObject();
        }
    }

    class _RubyParentData : ContainerBoxParentData<RenderBox>
    {
    }


    class _RubyRenderObject : RenderBoxContainerDefaultsMixin<RenderBox, _RubyParentData>
    {
        // @override
        // Size computeDryLayout(BoxConstraints constraints) =>
        // _performLayout(firstChild!, constraints, _performLayoutDry);


        public override float? computeDistanceToActualBaseline(TextBaseline baseline)
        {
            // return base.computeDistanceToActualBaseline(baseline);
            var ruby = firstChild;
            var rubyValue = ruby.getDistanceToActualBaseline(baseline) ?? 0f;

            var offset = (ruby.parentData as _RubyParentData).offset;
            return offset.dy + rubyValue;
        }


        protected internal override float computeMaxIntrinsicHeight(float width)
        {
            // return base.computeMaxIntrinsicHeight(width);
            var ruby = firstChild;
            var rubyValue = ruby.computeMaxIntrinsicHeight(width);
            var rt = (ruby.parentData as _RubyParentData).nextSibling;
            var rtValue = rt.computeMaxIntrinsicHeight(width);
            return rubyValue + rtValue;
        }

        protected internal override float computeMaxIntrinsicWidth(float height)
        {
            // return base.computeMaxIntrinsicWidth(height);

            var ruby = firstChild;
            var rubyValue = ruby.computeMaxIntrinsicWidth(height);

            var rt = (ruby.parentData as _RubyParentData).nextSibling;
            var rtValue = rt.computeMaxIntrinsicWidth(height);

            return Mathf.Max(rubyValue, rtValue);
        }

        protected internal override float computeMinIntrinsicHeight(float width)
        {
            // return base.computeMinIntrinsicHeight(width);
            var ruby = firstChild;
            var rubyValue = ruby.computeMinIntrinsicHeight(width);

            var rt = (ruby.parentData as _RubyParentData).nextSibling;
            var rtValue = rt.computeMinIntrinsicHeight(width);

            return rubyValue + rtValue;
        }

        protected internal override float computeMinIntrinsicWidth(float height)
        {
            // return base.computeMinIntrinsicWidth(height);
            var ruby = firstChild;
            var rubyValue = ruby.getMinIntrinsicWidth(height);
            var rt = (ruby.parentData as _RubyParentData).nextSibling;
            var rtValue = rt.getMinIntrinsicWidth(height);
            return Mathf.Min(rubyValue, rtValue);
        }

        public new Size getDryLayout(BoxConstraints constraints)
        {
            return _performLayout(firstChild, constraints, _performLayoutDry);
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null)
        {
            // return base.hitTestChildren(result, position);
            return defaultHitTestChildren(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset)
        {
            // base.paint(context, offset);
            defaultPaint(context, offset);
        }

        protected override void performLayout()
        {
            // base.performLayout();
            size = _performLayout(firstChild, constraints, _performLayoutLayouter);
        }


        public override void setupParentData(RenderObject child)
        {
            // base.setupParentData(child);

            if (!(child.parentData is _RubyParentData))
            {
                child.parentData = new _RubyParentData();
            }
        }


        static Size _performLayout
        (
            RenderBox ruby,
            BoxConstraints constraints,
            layouter layouter
        )
        {
            var rubyConstraints = constraints.loosen();
            var rubyData = ruby.parentData as _RubyParentData;
            var rubySize = layouter(ruby, rubyConstraints);

            var rt = rubyData.nextSibling;
            var rtConstraints = rubyConstraints.copyWith(
                maxHeight: rubyConstraints.maxHeight - rubySize.height);
            var rtData = rt.parentData as _RubyParentData;
            var rtSize = layouter(rt, rtConstraints);

            var height = rubySize.height + rtSize.height;
            var width = Mathf.Max(rubySize.width, rtSize.width);

            if (ruby.hasSize)
            {
                rubyData.offset = new Offset((width - rubySize.width) / 2, rtSize.height);
                rtData.offset = new Offset((width - rtSize.width) / 2, 0);
            }

            return constraints.constrain(new Size(width, height));
        }

        //TODO 暂时使用 getDryLayout暂代
        static Size _performLayoutDry
        (
            RenderBox renderBox,
            BoxConstraints constraints
        ) =>
            renderBox.getDryLayout(constraints);

        static Size _performLayoutLayouter
        (
            RenderBox renderBox,
            BoxConstraints constraints
        )
        {
            renderBox.layout(constraints, parentUsesSize: true);
            return renderBox.size;
        }
    }
}