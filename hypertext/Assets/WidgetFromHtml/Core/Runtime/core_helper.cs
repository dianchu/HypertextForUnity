using System.Collections.Generic;
using System.Text.RegularExpressions;
using AngleSharp.Css.Parser;
using AngleSharp.Dom;
using AngleSharp.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace WidgetFromHtml.Core
{
    public delegate Dictionary<string, string> CustomStylesBuilder(IElement element);

    public delegate Widget CustomWidgetBuilder(IElement element);


    internal class RebuildTriggers
    {
        public List<object> _values;

        public RebuildTriggers(List<object> values)
        {
            _values = values;
        }

        public override int GetHashCode()
        {
            return _values.Count;
        }

        static bool _Equal(RebuildTriggers v1, RebuildTriggers v2)
        {
            if (ReferenceEquals(v2, v1))
            {
                return true;
            }

            if (!ReferenceEquals(v1, null) && ReferenceEquals(v2, null))
            {
                return false;
            }

            if (!ReferenceEquals(v2, null) && ReferenceEquals(v1, null))
            {
                return false;
            }


            var list1 = v1._values;
            var list2 = v2._values;

            if (list1.Count != list2.Count)
            {
                return false;
            }

            for (int i = 0; i < list1.Count; i++)
            {
                var o1 = list1[i];
                var o2 = list2[i];
                if (o1 != o2)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool operator ==(RebuildTriggers v1, RebuildTriggers v2)
        {
            return _Equal(v1, v2);
        }

        public static bool operator !=(RebuildTriggers v1, RebuildTriggers v2)
        {
            return !_Equal(v1, v2);
        }
    }

    internal class INT
    {
        public int IntNumber { get; set; }

        public INT(int i)
        {
            IntNumber = i;
        }

        public static implicit operator int(INT i)
        {
            return i.IntNumber;
        }
    }

    internal class BOOL
    {
        public bool Bool { get; set; }

        public BOOL(bool b)
        {
            Bool = b;
        }

        public static implicit operator bool(BOOL b)
        {
            return b.Bool;
        }
    }

    public static class Helper
    {
        public static float? tryParseDoubleFromMap(INamedNodeMap map, string key)
        {
            var item = map.GetNamedItem(key);
            if (item == null)
            {
                return null;
            }
            else
            {
                if (float.TryParse(item.Value, out var f))
                {
                    return f;
                }
                else
                {
                    return null;
                }
            }
        }

        public static int? tryParseIntFromMap(INamedNodeMap map, string key)
        {
            var item = map.GetNamedItem(key);
            if (item == null)
            {
                return null;
            }
            else
            {
                if (int.TryParse(item.Value, out var f))
                {
                    return f;
                }
                else
                {
                    return null;
                }
            }
        }

        /// Parses [key] from [map] as an double literal and return its value.
        // double? tryParseDoubleFromMap(Map<dynamic, String> map, String key) =>
        // map.containsKey(key) ? double.tryParse(map[key]!) : null;
        public static TextDecoration TextDecoration_combine(List<TextDecoration> decorations)
        {
            var mask = 0;
            foreach (var decoration in decorations)
            {
                mask |= decoration._mask;
            }

            return new TextDecoration(_mask: mask);
        }

        readonly static Widget _widget0 = SizedBox.shrink();
        public static Widget widget0 => _widget0;

        public static IEnumerable<T> listOrNull<T>(T item)
        {
            if (item == null)
            {
                return null;
            }
            else
            {
                return new List<T>(1) {item};
            }
        }

        public static CssParser GetCssParser()
        {
            return new CssParser(new CssParserOptions()
            {
                IsIncludingUnknownDeclarations = true,
                IsIncludingUnknownRules = true,
                IsToleratingInvalidSelectors = true,
            });
        }

        public static string GetCssText(string cssPropertyKey, string cssPropertyValue)
        {
            var sb = StringBuilderPool.Obtain();
            sb.Append('*');
            sb.Append('{');
            sb.Append(cssPropertyKey);
            sb.Append(':');
            sb.Append(' ');
            sb.Append(cssPropertyValue);
            sb.Append('}');
            var ret = sb.ToString();
            return ret;
        }
    }


    internal class WidgetPlaceholder : StatelessWidget
    {
        /// <summary>
        /// The origin of this widget.
        /// </summary>
        public readonly object generator;

        public delegate Widget WidgetBuilder(BuildContext context, Widget widget);

        private readonly List<WidgetBuilder> _builders = new List<WidgetBuilder>();

        private readonly Widget _firstChild;

        public WidgetPlaceholder(object objGeneratorForDebug, Widget child = null)
        {
            this.generator = objGeneratorForDebug;
            _firstChild = child;
        }

        public override Widget build(BuildContext context)
        {
            return callBuilders(context, _firstChild);
        }

        public Widget callBuilders(BuildContext context, Widget child)
        {
            var build = child ?? Helper.widget0;

            foreach (var builder in _builders)
            {
                var builderRet = builder(context, build);
                if (builderRet == null)
                {
                    HLog.LogError($"WidgetPlaceholder::callBuilders builder return null {builder}");
                    build = Helper.widget0;
                }
                else
                {
                    build = builderRet;
                }
            }

            return build;
        }


        public override void debugFillProperties(DiagnosticPropertiesBuilder properties)
        {
            base.debugFillProperties(properties);
            properties.add(
                new DiagnosticsProperty<object>(
                    $"generator{generator.GetType().Name}",
                    generator,
                    showName: false
                ));
        }

        public virtual WidgetPlaceholder wrapWith(WidgetBuilder builder)
        {
            _builders.Add(builder);
            return this;
        }


        public static WidgetPlaceholder lazy(Widget child) =>
            child is WidgetPlaceholder placeholder ? placeholder : new WidgetPlaceholder(child);

        public override string toString(DiagnosticLevel minLevel = DiagnosticLevel.info)
        {
            return generator != null
                ? $"WidgetPlaceholder{generator}"
                : this.GetType().Name;
        }
    }
}