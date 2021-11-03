using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AngleSharp.Css.Values;
using Unity.UIWidgets.widgets;
using Unity.UIWidgets.painting;

namespace WidgetFromHtml.Core
{
    internal class DisplayBlockOp : BuildOp
    {
        public DisplayBlockOp(WidgetFactory wf)
            : base
            (
                onWidgets: (meta, widgets) => { return ONWidgets(meta, widgets, wf); },
                priority: StyleSizing.kPriority7k + 1
            )
        {
        }

        private static IEnumerable<Widget> ONWidgets(AbsBuildMetadata meta, IEnumerable<WidgetPlaceholder> widgets,
            WidgetFactory wf)
        {
            var widgetPlaceholder = wf.buildColumnPlaceholder(meta, widgets)
                ?.wrapWith((_, w) => w is CssSizing ? w : new CssBlock(child: w));
            return Helper.listOrNull(widgetPlaceholder);
        }
    }


    internal class StyleSizing
    {
        public const int kPriority7k = 7000;

        public WidgetFactory wf;

        static ConditionalWeakTable<AbsBuildMetadata, BOOL> _treatHeightAsMinHeight =
            new ConditionalWeakTable<AbsBuildMetadata, BOOL>();

        public StyleSizing(WidgetFactory wf)
        {
            this.wf = wf;
        }

        public BuildOp buildOp =>
            new BuildOp(
                onTree: ONTree,
                onWidgets: ONWidgets,
                onWidgetsIsOptional: true,
                priority: kPriority7k
            );

        private IEnumerable<Widget> ONWidgets(AbsBuildMetadata meta, IEnumerable<WidgetPlaceholder> widgets)
        {
            if (meta.willBuildSubtree == false) return widgets;
            var input = _parse(meta);
            if (input == null) return widgets;
            return Helper.listOrNull(wf
                .buildColumnPlaceholder(meta, widgets)
                ?.wrapWith((c, w) => _build(c, w, input, meta.tsb)));
        }

        private void ONTree(AbsBuildMetadata meta, AbsBuildTree tree)
        {
            if (meta.willBuildSubtree == true) return;

            var input = _parse(meta);
            if (input == null) return;

            WidgetPlaceholder widget = null;
            bool isReturn = false;
            tree.bits(b =>
            {
                if (isReturn)
                {
                    return;
                }

                if (b is WidgetBit widgetBit)
                {
                    if (widget != null)
                    {
                        isReturn = true;
                    }

                    widget = widgetBit.child;
                }
                else
                {
                    isReturn = true;
                }
            });
            if (isReturn)
            {
                return;
            }
            //
            // for (final b in tree.bits) {
            //     if (b is WidgetBit) {
            //         if (widget != null) return;
            //         widget = b.child;
            //     } else {
            //         return;
            //     }
            // }

            widget?.wrapWith((c, w) => _build(c, w, input, meta.tsb));
        }


        _StyleSizingInput _parse(AbsBuildMetadata meta)
        {
            Length? maxHeight = null;
            Length? maxWidth = null;
            Length? minHeight = null;
            Length? minWidth = null;
            Length? preferredHeight = null;
            Length? preferredWidth = null;
            Axis? preferredAxis = null;


            foreach (var style in meta.styles)
            {
                var value = style.RawValue;
                if (value == null) continue;

                switch (style.Name)
                {
                    case Const.kCssHeight:
                        var parsedHeight = core_parser.tryParseCssLength(value);
                        if (parsedHeight != null)
                        {
                            if (_treatHeightAsMinHeight.TryGetValue(meta, out var bValue))
                            {
                                if (bValue)
                                {
                                    minHeight = parsedHeight;
                                }
                                else
                                {
                                    preferredAxis = Axis.vertical;
                                    preferredHeight = parsedHeight;
                                }
                            }
                            else
                            {
                                preferredAxis = Axis.vertical;
                                preferredHeight = parsedHeight;
                            }

                            // if (_treatHeightAsMinHeight[meta] == true)
                            // {
                            //     minHeight = parsedHeight;
                            // }
                            // else
                            // {
                            //     preferredAxis = Axis.vertical;
                            //     preferredHeight = parsedHeight;
                            // }
                        }

                        break;
                    case Const.kCssMaxHeight:
                        maxHeight = core_parser.tryParseCssLength(value) ?? maxHeight;
                        break;
                    case Const.kCssMaxWidth:
                        maxWidth = core_parser.tryParseCssLength(value) ?? maxWidth;
                        break;
                    case Const.kCssMinHeight:
                        minHeight = core_parser.tryParseCssLength(value) ?? minHeight;
                        break;
                    case Const.kCssMinWidth:
                        minWidth = core_parser.tryParseCssLength(value) ?? minWidth;
                        break;
                    case Const.kCssWidth:
                        var parsedWidth = core_parser.tryParseCssLength(value);
                        if (parsedWidth != null)
                        {
                            preferredAxis = Axis.horizontal;
                            preferredWidth = parsedWidth;
                        }

                        break;
                }
            }

            if (maxHeight == null &&
                maxWidth == null &&
                minHeight == null &&
                minWidth == null &&
                preferredHeight == null &&
                preferredWidth == null) return null;

            if (
                preferredWidth == null
                // && meta.buildOps.whereType<DisplayBlockOp>().isNotEmpty
                && meta.buildOps.Where(op => op is DisplayBlockOp).isNotEmpty()
            )
            {
                // `display: block` implies a 100% width
                // but it MUST NOT reset width value if specified
                // we need to keep track of block width to calculate contraints correctly
                preferredWidth = new Length(100, Length.Unit.Percent);
                // preferredAxis ??= Axis.horizontal;
                if (preferredAxis == null)
                {
                    preferredAxis = Axis.horizontal;
                }
            }

            return new _StyleSizingInput
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


        public static void treatHeightAsMinHeight(AbsBuildMetadata meta)
        {
            if (_treatHeightAsMinHeight.TryGetValue(meta, out var bValue))
            {
                bValue.Bool = true;
            }
            else
            {
                _treatHeightAsMinHeight.Add(meta, new BOOL(true));
            }

            // _treatHeightAsMinHeight[meta] = true;
        }


        static Widget _build
        (
            BuildContext context,
            Widget child,
            _StyleSizingInput input,
            TextStyleBuilder tsb
        )
        {
            var tsh = tsb.build(context);

            return new CssSizing
            (
                maxHeight: _getValue(input.maxHeight, tsh),
                maxWidth: _getValue(input.maxWidth, tsh),
                minHeight: _getValue(input.minHeight, tsh),
                minWidth: _getValue(input.minWidth, tsh),
                preferredAxis: input.preferredAxis,
                preferredHeight: _getValue(input.preferredHeight, tsh),
                preferredWidth: _getValue(input.preferredWidth, tsh),
                child: child
            );
        }

        static CssSizingValue _getValue(Length? length, TextStyleHtml tsh)
        {
            if (length == null) return null;

            var value = length.Value.getValue(tsh);
            if (value != null) return CssSizingValue.value(value.Value);

            switch (length.Value.Type)
            {
                case Length.Unit.None:
                    return CssSizingValue.auto();
                case Length.Unit.Percent:
                    return CssSizingValue.percentage(length.Value.number());
                default:
                    return null;
            }
        }
    }


    class _StyleSizingInput
    {
        public Length? maxHeight;
        public Length? maxWidth;
        public Length? minHeight;
        public Length? minWidth;
        public Length? preferredHeight;
        public Length? preferredWidth;
        public Axis? preferredAxis;

        public _StyleSizingInput
        (
            Length? maxHeight = null,
            Length? maxWidth = null,
            Length? minHeight = null,
            Length? minWidth = null,
            Length? preferredHeight = null,
            Length? preferredWidth = null,
            Axis? preferredAxis = null
        )
        {
            this.maxHeight = maxHeight;
            this.maxWidth = maxWidth;
            this.minHeight = minHeight;
            this.minWidth = minWidth;
            this.preferredHeight = preferredHeight;
            this.preferredWidth = preferredWidth;
            this.preferredAxis = preferredAxis;
        }
    }
}