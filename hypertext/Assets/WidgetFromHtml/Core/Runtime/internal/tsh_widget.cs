using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace WidgetFromHtml.Core
{
    internal class TshWidget : InheritedWidget
    {
        public readonly TextStyleHtml textStyleHtml;

        public TshWidget(Widget child, TextStyleHtml tsh, Key key = null) : base(key: key, child)
        {
            textStyleHtml = tsh;
        }

        public override bool updateShouldNotify(InheritedWidget oldWidget)
        {
            return textStyleHtml == null || textStyleHtml != (oldWidget as TshWidget).textStyleHtml;
        }
    }
}