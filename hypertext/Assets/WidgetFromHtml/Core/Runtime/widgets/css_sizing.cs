using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using UnityEngine;

namespace WidgetFromHtml.Core
{
    internal class CssBlock : CssSizing
    {
        public CssBlock
        (
            Widget child,
            Key key = null
        ) : base(child, key)
        {
        }

        public override RenderObject createRenderObject(BuildContext context)
        {
            return new _RenderCssSizing(preferredWidth: CssSizingValue.percentage(100));
        }

        public override void updateRenderObject
        (
            BuildContext context,
            RenderObject renderObject1
        ) //dart中  RenderObject 被声明成covariant (协变)
        {
            if (renderObject1 is _RenderCssSizing renderObject)
            {
                renderObject.setPreferredSize(null, CssSizingValue.percentage(100), null);
            }
            else
            {
                HLog.LogError("CssBlock updateRenderObject 协变_RenderCssSizing 错误!");
            }
        }
    }

    internal class CssSizing : SingleChildRenderObjectWidget
    {
        public CssSizingValue maxHeight;
        public CssSizingValue maxWidth;
        public CssSizingValue minHeight;
        public CssSizingValue minWidth;
        public CssSizingValue preferredHeight;
        public CssSizingValue preferredWidth;
        public Axis? preferredAxis;


        public CssSizing
        (
            Widget child,
            Key key = null,
            CssSizingValue maxHeight = null,
            CssSizingValue maxWidth = null,
            CssSizingValue minHeight = null,
            CssSizingValue minWidth = null,
            CssSizingValue preferredHeight = null,
            CssSizingValue preferredWidth = null,
            Axis? preferredAxis = null
        ) : base(key: key, child: child)
        {
            this.maxHeight = maxHeight;
            this.maxWidth = maxWidth;
            this.minHeight = minHeight;
            this.minWidth = minWidth;
            this.preferredHeight = preferredHeight;
            this.preferredWidth = preferredWidth;
            this.preferredAxis = preferredAxis;
        }

        public override RenderObject createRenderObject(BuildContext context)
        {
            return new _RenderCssSizing
            (
                maxHeight: maxHeight,
                maxWidth: maxWidth,
                minHeight: minHeight,
                minWidth: minWidth,
                preferredAxis: preferredAxis,
                preferredHeight: preferredHeight,
                preferredWidth: preferredWidth
            );
        }

        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);
            _debugFillProperty(properties, "maxHeight", maxHeight);
            _debugFillProperty(properties, "maxWidth", maxWidth);
            _debugFillProperty(properties, "minHeight", minHeight);
            _debugFillProperty(properties, "minWidth", minWidth);
            _debugFillProperty(
                properties,
                "preferredHeight" +
                (preferredHeight != null &&
                 preferredWidth != null &&
                 preferredAxis == Axis.vertical
                    ? "*"
                    : ""),
                preferredHeight);
            _debugFillProperty(
                properties,
                "preferredWidth" +
                (preferredHeight != null &&
                 preferredWidth != null &&
                 preferredAxis != Axis.vertical
                    ? "*"
                    : ""),
                preferredWidth);
        }


        void _debugFillProperty(DiagnosticPropertiesBuilder properties, string name,
            CssSizingValue value)
        {
            if (value == null) return;
            properties.add(new DiagnosticsProperty<CssSizingValue>(name, value));
        }


        public override void updateRenderObject
        (
            BuildContext context,
            RenderObject renderObject1
        ) //dart中  RenderObject 被声明成covariant (协变)
        {
            if (renderObject1 is _RenderCssSizing renderObject)
            {
                renderObject.setConstraints(
                    maxHeight: maxHeight,
                    maxWidth: maxWidth,
                    minHeight: minHeight,
                    minWidth: minWidth
                );
                renderObject.setPreferredSize(
                    preferredAxis, preferredWidth, preferredHeight);
            }
            else
            {
                HLog.LogError("CssSizing updateRenderObject 协变_RenderCssSizing 错误!");
            }
        }
    }


    class _RenderCssSizing : RenderProxyBox
    {
        CssSizingValue _maxHeight;
        CssSizingValue _maxWidth;
        CssSizingValue _minHeight;
        CssSizingValue _minWidth;
        CssSizingValue _preferredHeight;
        CssSizingValue _preferredWidth;
        Axis? _preferredAxis;

        public _RenderCssSizing
        (
            RenderBox child = null,
            CssSizingValue maxHeight = null,
            CssSizingValue maxWidth = null,
            CssSizingValue minHeight = null,
            CssSizingValue minWidth = null,
            Axis? preferredAxis = null,
            CssSizingValue preferredHeight = null,
            CssSizingValue preferredWidth = null
        ) : base(child)
        {
            _maxHeight = maxHeight;
            _maxWidth = maxWidth;
            _minHeight = minHeight;
            _minWidth = minWidth;
            _preferredHeight = preferredHeight;
            _preferredWidth = preferredWidth;
            _preferredAxis = preferredAxis;
        }


        public void setConstraints
        (
            CssSizingValue maxHeight = null,
            CssSizingValue maxWidth = null,
            CssSizingValue minHeight = null,
            CssSizingValue minWidth = null
        )
        {
            if (maxHeight == _maxHeight &&
                maxWidth == _maxWidth &&
                minHeight == _minHeight &&
                minWidth == _minWidth)
            {
                return;
            }

            _maxHeight = maxHeight;
            _maxWidth = maxWidth;
            _minHeight = minHeight;
            _minWidth = minWidth;
            markNeedsLayout();
        }


        public void setPreferredSize
        (
            Axis? axis,
            CssSizingValue width,
            CssSizingValue height
        )
        {
            if
            (
                axis == _preferredAxis &&
                height == _preferredHeight &&
                width == _preferredWidth
            )
            {
                return;
            }

            _preferredAxis = axis;
            _preferredHeight = height;
            _preferredWidth = width;
            markNeedsLayout();
        }


        //TODO UIWidget没有这个重载,就不搬过来了..
        // public Size computeDryLayout(BoxConstraints constraints)
        // {
        //     var cc = _applyContraints(constraints);
        //     var childSize = child.getDryLayout(cc);
        //     return constraints.constrain(childSize);
        // }


        public new Size getDryLayout(BoxConstraints constraints)
        {
            var cc = _applyContraints(constraints);
            var childSize = child.getDryLayout(cc);
            return constraints.constrain(childSize);
        }

        protected override void performLayout()
        {
            /*
             *             if (child != null) {
                child.layout(constraints, parentUsesSize: true);
                size = child.size;
            } else {
                performResize();
            }
             */
            // base.performLayout();

            var cc = _applyContraints(constraints);
            child.layout(cc, parentUsesSize: true);
            size = constraints.constrain(child.size);
        }


        BoxConstraints _applyContraints(BoxConstraints c)
        {
            var maxHeight =
                Mathf.Min(c.maxHeight, _maxHeight?.clamp(0f, c.maxHeight) ?? c.maxHeight);
            var maxWidth =
                Mathf.Min(c.maxWidth, _maxWidth?.clamp(0f, c.maxWidth) ?? c.maxWidth);
            var minHeight =
                Mathf.Min(maxHeight, _minHeight?.clamp(0f, c.maxHeight) ?? c.minHeight);
            var minWidth =
                Mathf.Min(maxWidth, _minWidth?.clamp(0f, c.maxWidth) ?? c.minWidth);

            var effectiveMinHeight =
                c.hasTightHeight && _minHeight == null ? 0f : minHeight;
            var effectiveMinWidth =
                c.hasTightWidth && _minWidth == null ? 0f : minWidth;
            var __preferredHeight =
                _preferredHeight?.clamp(effectiveMinHeight, maxHeight);
            var __preferredWidth =
                _preferredWidth?.clamp(effectiveMinWidth, maxWidth);

            // ignore preferred value if it's infinite
            var preferredHeight =
                __preferredHeight?.isFinite() == true ? __preferredHeight : null;
            var preferredWidth =
                __preferredWidth?.isFinite() == true ? __preferredWidth : null;

            var stableChildSize = (preferredHeight != null && preferredWidth != null)
                ? _guessChildSize
                (
                    maxHeight: maxHeight,
                    maxWidth: maxWidth,
                    preferredHeight: preferredHeight.Value,
                    preferredWidth: preferredWidth.Value
                )
                : null;

            var cc = new BoxConstraints(
                maxHeight: stableChildSize?.height ?? preferredHeight ?? maxHeight,
                maxWidth: stableChildSize?.width ?? preferredWidth ?? maxWidth,
                minHeight: stableChildSize?.height ?? preferredHeight ?? minHeight,
                minWidth: stableChildSize?.width ?? preferredWidth ?? minWidth
            );

            return cc;
        }


        Size _guessChildSize
        (
            float maxHeight,
            float maxWidth,
            float preferredHeight,
            float preferredWidth
        )
        {
            //TODO 因为dryLayout在当前版本的UIWidget还没有实现, 世界使用perferred来作为size
            return new Size
            (
                Mathf.Min(preferredWidth, maxWidth),
                Mathf.Min(preferredHeight, maxHeight)
            );
            var ccHeight = new BoxConstraints
            (
                maxWidth: float.PositiveInfinity,
                maxHeight: preferredHeight,
                minWidth: 0f,
                minHeight: preferredHeight
            );

            var sizeHeight = child.getDryLayout(ccHeight);

            var ccWidth = new BoxConstraints
            (
                maxWidth: preferredWidth,
                maxHeight: float.PositiveInfinity,
                minWidth: preferredWidth,
                minHeight: 0
            );
            var sizeWidth = child.getDryLayout(ccWidth);

            var childAspectRatio = sizeWidth.width / sizeWidth.height;
            var epsilon = 0.01f;
            if ((childAspectRatio - sizeHeight.width / sizeHeight.height).abs() >
                epsilon)
            {
                return null;
            }

            // child appears to have a stable aspect ratio
            float? childWidth, childHeight;
            if (_preferredAxis == Axis.vertical)
            {
                childHeight = preferredHeight;
                childWidth = childHeight * childAspectRatio;
            }
            else
            {
                childWidth = preferredWidth;
                childHeight = childWidth / childAspectRatio;
            }

            if (childWidth > maxWidth)
            {
                childWidth = maxWidth;
                childHeight = childWidth / childAspectRatio;
            }

            if (childHeight > maxHeight)
            {
                childHeight = maxHeight;
                childWidth = childHeight * childAspectRatio;
            }

            return new Size(childWidth.Value, childHeight.Value);
        }
    }


    /// A [CssSizing] value.
    public abstract class CssSizingValue
    {
        public abstract float? clamp(float min, float max);

        public static CssSizingValue auto() => new _CssSizingAuto();

        public static CssSizingValue percentage(float fV) => new _CssSizingPercentage(fV);

        public static CssSizingValue value(float fv) => new _CssSizingValue(fv);
    }


    class _CssSizingAuto : CssSizingValue
    {
        public override float? clamp(float min, float max)
        {
            return null;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        static bool _Equal(_CssSizingAuto value1, CssSizingValue value2)
        {
            if (ReferenceEquals(value1, value2))
            {
                return true;
            }

            if (ReferenceEquals(value1, null) && !ReferenceEquals(value2, null))
            {
                return false;
            }

            if (ReferenceEquals(value2, null) && !ReferenceEquals(value1, null))
            {
                return false;
            }

            return value2 is _CssSizingAuto;
        }


        public static bool operator ==(_CssSizingAuto value1, CssSizingValue value2)
        {
            return _Equal(value1, value2);
        }

        public static bool operator !=(_CssSizingAuto value1, CssSizingValue value2)
        {
            return !_Equal(value1, value2);
        }

        public override string ToString()
        {
            return "auto";
        }
    }

    class _CssSizingPercentage : CssSizingValue
    {
        public float percentage;

        public _CssSizingPercentage(float percentage)
        {
            this.percentage = percentage;
        }


        public override float? clamp(float min, float max)
        {
            return (max * percentage).clamp(min, max);
        }

        public override int GetHashCode()
        {
            return percentage.GetHashCode();
        }

        public static bool operator ==(_CssSizingPercentage value1, _CssSizingPercentage value2)
        {
            return value1.percentage == value2.percentage;
        }

        public static bool operator !=(_CssSizingPercentage value1, _CssSizingPercentage value2)
        {
            return value1.percentage != value2.percentage;
        }

        public override string ToString()
        {
            var strP = percentage.toStringAsFixed1();
            return $"{strP}%";
        }
    }


    class _CssSizingValue : CssSizingValue
    {
        public float value;


        public _CssSizingValue(float value)
        {
            this.value = value;
        }

        public override float? clamp(float min, float max)
        {
            return value.clamp(min, max);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public static bool operator ==(_CssSizingValue value1, _CssSizingValue value2)
        {
            return value1.value == value2.value;
        }

        public static bool operator !=(_CssSizingValue value1, _CssSizingValue value2)
        {
            return value1.value != value2.value;
        }

        public override string ToString()
        {
            return value.toStringAsFixed1();
        }
    }
}