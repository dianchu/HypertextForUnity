using System.Collections.Generic;
using System.Linq;
using AngleSharp.Css.Values;
using Unity.UIWidgets.widgets;

namespace WidgetFromHtml.Core
{
    internal class HeightPlaceholder : WidgetPlaceholder
    {
        public readonly TextStyleBuilder tsb;

        readonly List<Length> _heights = new List<Length>();

        public HeightPlaceholder(Length height, TextStyleBuilder tsb) : base(tsb)
        {
            base.wrapWith((context, widget) => { return _build(context, widget, height, tsb); });
            this.tsb = tsb;
            _heights.Add(height);
        }

        public Length height => _heights.First();


        public void mergeWith(HeightPlaceholder other)
        {
            var height = other.height;
            _heights.Add(height);

            base.wrapWith((c, w) => _build(c, w, height, other.tsb));
        }


        public override WidgetPlaceholder wrapWith(WidgetBuilder builder)
        {
            return this;
        }

        public static Widget _build
        (
            BuildContext context,
            Widget child,
            Length height,
            TextStyleBuilder tsb
        )
        {
            // var existing = (child is SizedBox ? child.height : null) ?? 0.0;
            float existing = 0f;
            if (child is SizedBox sizedBox)
            {
                if (sizedBox.height != null) existing = sizedBox.height.Value;
            }

            float? value = (float?) (height.getValue(tsb.build(context)));
            if (value != null && value > existing) return new SizedBox(height: value);
            return child;
        }
    }
}