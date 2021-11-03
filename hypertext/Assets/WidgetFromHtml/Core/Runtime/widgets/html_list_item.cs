using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace WidgetFromHtml.Core
{
    internal class HtmlListItem : MultiChildRenderObjectWidget
    {
        public TextDirection textDirection;

        public HtmlListItem
        (
            Widget child,
            Widget marker,
            TextDirection textDirection,
            Key key = null
        ) : base(key, new List<Widget> { child, marker })
        {
            this.textDirection = textDirection;
        }

        public override RenderObject createRenderObject(BuildContext context)
        {
            return new _ListItemRenderObject(textDirection: textDirection);
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);
            properties.add
            (
                new DiagnosticsProperty<TextDirection>("textDirection", textDirection)
            );
        }

        public override void updateRenderObject(BuildContext context, RenderObject renderObject1)
        {
            // base.updateRenderObject(context, renderObject);
            if (renderObject1 is _ListItemRenderObject renderObject)
            {
                renderObject.textDirection = textDirection;
            }
            else
            {
                HLog.LogError(
                    "HtmlListItem :updateRenderObject renderObject1 is not  _ListItemRenderObject update property failed...");
            }
        }
    }

    class _ListItemData : ContainerBoxParentData<RenderBox>
    {
        //dart处没有任何实现..
    }


    /*
     * dart语言的 _ListItemRenderObject的继承关系如下, 
     * 
     * class _ListItemRenderObject extends RenderBox
    with
        ContainerRenderObjectMixin<RenderBox, _ListItemData>,
        RenderBoxContainerDefaultsMixin<RenderBox, _ListItemData>
     *
     *
     *c#
     * RenderBoxContainerDefaultsMixin  继承了RenderBox  实现了ContainerRenderObjectMixin接口
     * 所以 _ListItemRenderObject 直接继承 ContainerRenderObjectMixinRenderBox即可!
     */


    class _ListItemRenderObject : RenderBoxContainerDefaultsMixin<RenderBox, _ListItemData>
    {
        TextDirection _textDirection;

        public TextDirection textDirection
        {
            get => _textDirection;
            set
            {
                if (_textDirection == value) return;
                _textDirection = value;
                markNeedsLayout();
            }
        }

        public _ListItemRenderObject(TextDirection textDirection)
        {
        }

        public override float? computeDistanceToActualBaseline(TextBaseline baseline)
        {
            return defaultComputeDistanceToFirstActualBaseline(baseline);
        }

        protected internal override float computeMaxIntrinsicHeight(float width)
        {
            return firstChild.computeMaxIntrinsicHeight(width);
        }

        protected internal override float computeMaxIntrinsicWidth(float height)
        {
            return firstChild.computeMaxIntrinsicWidth(height);
        }

        protected internal override float computeMinIntrinsicHeight(float width)
        {
            return firstChild.computeMinIntrinsicHeight(width);
        }

        protected internal override float computeMinIntrinsicWidth(float height)
        {
            return firstChild.getMinIntrinsicWidth(height);
        }

        // public override Size computeDryLayout(BoxConstraints constraints)
        //TODO 这种重载了computeDryLayout 等待作者升级 使用getDryLayout暂代
        public new Size getDryLayout(BoxConstraints constraints)
        {
            var child = firstChild;
            var childConstraints = constraints;
            var childData = child.parentData as _ListItemData;
            var childSize = child.getDryLayout(childConstraints);

            var marker = childData.nextSibling;
            var markerConstraints = childConstraints.loosen();
            var markerSize = marker.getDryLayout(markerConstraints);

            return constraints.constrain
            (
                new Size
                (
                    childSize.width,
                    childSize.height > 0 ? childSize.height : markerSize.height
                )
            );
        }

        protected override bool hitTestChildren(BoxHitTestResult result, Offset position = null)
        {
            return defaultHitTestChildren(result, position: position);
        }

        public override void paint(PaintingContext context, Offset offset)
        {
            defaultPaint(context, offset);
        }

        protected override void performLayout()
        {
            var child = firstChild;
            var childConstraints = constraints;
            var childData = child.parentData as _ListItemData;
            child.layout(childConstraints, parentUsesSize: true);
            var childSize = child.size;

            var marker = childData.nextSibling;
            var markerConstraints = childConstraints.loosen();
            var markerData = marker.parentData as _ListItemData;
            marker.layout(markerConstraints, parentUsesSize: true);
            var markerSize = marker.size;

            size = constraints.constrain
            (
                new Size
                (
                    childSize.width,
                    childSize.height > 0 ? childSize.height : markerSize.height
                )
            );

            var baseline = TextBaseline.alphabetic;
            var markerDistance =
                marker.getDistanceToBaseline(baseline, onlyReal: true) ??
                markerSize.height;
            var childDistance =
                child.getDistanceToBaseline(baseline, onlyReal: true) ?? markerDistance;

            markerData.offset = new Offset
            (
                textDirection == TextDirection.ltr
                    ? -markerSize.width - Const._kGapVsMarker
                    : childSize.width + Const._kGapVsMarker,
                childDistance - markerDistance
            );
            // Debug.Log($"htmlListItem set offset={markerData.offset}");
        }

        public override void setupParentData(RenderObject child)
        {
            if (!(child.parentData is _ListItemData))
            {
                child.parentData = new _ListItemData();
            }
        }
    }
}