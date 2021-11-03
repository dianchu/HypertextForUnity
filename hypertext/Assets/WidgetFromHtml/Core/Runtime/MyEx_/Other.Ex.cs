using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Values;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace WidgetFromHtml.Core
{
    public static partial class OtherEx
    {
        public static int length<T>(this List<T> list)
        {
            return list.Count;
        }

        public static float clamp(this float f, float min, float max)
        {
            return Mathf.Clamp(f, min, max);
        }

        public static string toStringAsFixed1(this float f)
        {
            return f.ToString("F1");
        }


        /// <summary>
        /// TODO 等待UIWidgets2.0更新! 更新dry布局相关的东西!
        /// </summary>
        /// <param name="renderBox"></param>
        /// <param name="boxConstraints"></param>
        /// <returns></returns>
        public static Size getDryLayout(this RenderBox renderBox, BoxConstraints boxConstraints)
        {
       

            if (renderBox is  _ListItemRenderObject listItemRenderObject )
            {
                return listItemRenderObject.getDryLayout(boxConstraints);
            }

            if (renderBox is  _RubyRenderObject rubyRenderObject)
            {
                return rubyRenderObject.getDryLayout(boxConstraints);
            }

            if (renderBox is _ListMarkerRenderObject listMarkerRenderObject)
            {
                return listMarkerRenderObject.getDryLayout(boxConstraints);
            }

            if (renderBox is _TableRenderObject tableRenderObject)
            {
                return tableRenderObject.getDryLayout(boxConstraints);
            }
            if (renderBox is _ValignBaselineRenderObject valignBaselineRenderObject)
            {
                return valignBaselineRenderObject.getDryLayout(boxConstraints);
            }
            if (renderBox is _RenderCssSizing renderCssSizing)
            {
                return renderCssSizing.getDryLayout(boxConstraints);
            }
            
            HLog.LogInfo("TODO 等UIWidget2.0 更新dryLayout ,这个方法就可以删除了.");
            return new Size
            (
                // width: renderBox.computeMaxIntrinsicHeight(boxConstraints.maxWidth),
                // height: renderBox.computeMaxIntrinsicHeight(boxConstraints.maxHeight)
                width: renderBox.computeMinIntrinsicWidth(boxConstraints.maxWidth),
                height: renderBox.computeMinIntrinsicHeight(boxConstraints.maxHeight)
            );
        }


        public static bool hasScheme(this Uri uri)
        {
            return !string.IsNullOrEmpty(uri.Scheme);
        }

        /// <summary>
        /// 制作相对路径! 参考 https://docs.microsoft.com/en-us/dotnet/api/system.uri.makerelativeuri?view=net-5.0
        /// 制作相对路径! https://api.dart.dev/stable/2.13.4/dart-core/Uri/resolveUri.html
        /// Resolve [reference] as an URI relative to `this`.
        ///
        /// Returns the resolved URI.
        ///
        /// The algorithm "Transform Reference" for resolving a reference is described
        /// in [RFC-3986 Section 5](http://tools.ietf.org/html/rfc3986#section-5 "RFC-1123").
        ///
        /// Updated to handle the case where the base URI is just a relative path -
        /// that is: when it has no scheme and no authority and the path does not start
        /// with a slash.
        /// In that case, the paths are combined without removing leading "..", and
        /// an empty path is not converted to "/".
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="reference"></param>
        /// <returns></returns>
        public static Uri resolveUri(this Uri uri, Uri reference)
        {
            return uri.MakeRelativeUri(reference);
        }


        /// <summary>
        /// $"{e.key}: {e.value}"
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="joinChar"></param>
        /// <returns></returns>
        public static string Map_entries_map_join_to_style_sheet_(this Dictionary<string, string> dic, char joinChar)
        {
            var sb = StringBuilderPool.Obtain();
            var count = 0;
            var totalCount = dic.Count;
            sb.Append("*{");
            foreach (var kv in dic)
            {
                sb.Append(kv.Key);
                sb.Append(':');
                sb.Append(' ');
                sb.Append(kv.Value);
                if (count != totalCount - 1)
                {
                    sb.Append(joinChar);
                }

                count++;
            }

            sb.Append('}');
            var ret = sb.ToString();
            sb.ToPool();
            return ret;
        }


        public static void AddRenderMatchs(this List<Match> list, MatchCollection matchCollection)
        {
            // if (list == null)
            // {
            // HLog.LogError("AddRenderMatchs error list = null");
            // }

            var count = matchCollection.Count;
            for (int i = 0; i < count; i++)
            {
                var m = matchCollection[i];
                list.Add(m);
            }
        }


        public static bool IsLiteralTerm(this ICssValue cssValue)
        {
            return cssValue is ICssSpecialValue;
        }

        public static bool IsNumberTerm(this ICssValue cssValue)
        {
            return cssValue is Length;
        }


        public static float number(this ICssValue cssValue)
        {
            return (float) cssValue.AsDouble();
        }

        public static bool containsKey(this INamedNodeMap nodeMap, string key)
        {
            return nodeMap.GetNamedItem(key) != null;
        }

        private static MethodInfo _miComputeLineMetricsEx = typeof(TextPainter)
            .GetMethod("computeLineMetrics",
                BindingFlags.Instance | BindingFlags.NonPublic);

        private static object[] emptyObjs = new object[] { };

        /// <summary>
        /// TextPainter.computeLineMetrics 被作者声明成私有的,dart的成员方法,只要不是下划线开头的,就是public , 作者估计粗心了--! 
        /// </summary>
        /// <param name="textPainter"></param>
        /// <returns></returns>
        public static List<LineMetrics> computeLineMetricsEx(this TextPainter textPainter)
        {
            return _miComputeLineMetricsEx.Invoke(textPainter, emptyObjs) as List<LineMetrics>;
        }

        internal static List<Widget> to_list_widget(this IEnumerable<WidgetPlaceholder> ieWidgetPlaceholders)
        {
            var list = new List<Widget>(ieWidgetPlaceholders.Count());
            ieWidgetPlaceholders.GetEnumerator().Reset();
            foreach (var widgetPlaceholder in ieWidgetPlaceholders)
            {
                list.Add(widgetPlaceholder);
            }

            return list;
        }

        internal static string toText
        (
            this List<_String> listStrs,
            Whitespace whitespace,
            bool dropNewLine = false
        )
        {
            if (listStrs.Count == 0)
            {
                return "";
            }

            if (dropNewLine && listStrs.last().isNewLine)
            {
                listStrs.removeLast();
                // removeLast();
                if (listStrs.isEmpty())
                {
                    return "";
                }
            }

            var buffer = StringBuilderPool.Obtain();

            foreach (var str in listStrs)
            {
                if (str.isWhitespace)
                {
                    if (whitespace == Whitespace.Pre)
                    {
                        buffer.Append(str.data);
                    }
                    else
                    {
                        buffer.Append(' ');
                    }
                }
                else
                {
                    buffer.Append(str.data);
                }
            }

            buffer.ToPool();
            var ret = buffer.ToString();
            return ret;
        }


        public static void CollectAllStyleFromSheetAndInline(this List<ICssProperty> listProperties, IElement element)
        {
            //收集styleSheet的属性!
            listProperties._CollectStyleSheetStyles(element);
            //收集行内属性
            listProperties._CollectInLineStyles(element);
        }

        static void _CollectStyleSheetStyles
        (
            this List<ICssProperty> listProperties,
            IElement element
        )
        {
            var document = element?.Owner;
            if (document == null) return;
            foreach (var styleSheet in document.StyleSheets)
            {
                var cssStyleSheet = (ICssStyleSheet) styleSheet;
                if (cssStyleSheet == null) continue;
                foreach (var cssRule in cssStyleSheet.Rules)
                {
                    var cssStyleRule = (ICssStyleRule) cssRule;
                    if (cssStyleRule.Selector.Match(element))
                    {
                        listProperties._CollectStyles(cssStyleRule.Style, element);
                    }
                }
            }
        }

        static void _CollectInLineStyles
        (
            this List<ICssProperty> listProperties,
            IElement element
        )
        {
            if (element == null)
            {
                HLog.LogError("CollectInLineStyles error element=null");
            }

            listProperties._CollectStyles(element.GetStyle(), element);
        }

        static void _CollectStyles
        (
            this List<ICssProperty> listProperties,
            ICssStyleDeclaration cssStyleDeclaration,
            IElement element = null
        )
        {
            if (cssStyleDeclaration != null)
            {
                foreach (var inlineStyle in cssStyleDeclaration)
                {
                    listProperties.Add(inlineStyle);
                }
            }
            else
            {
                HLog.LogError($" _CollectStyles error html = {element?.OuterHtml}");
            }
        }


        /// <summary>
        /// 对照CssStyleDeclaration.SetProperty接口实现
        /// </summary>
        /// <param name="listProperties"></param>
        /// <param name="keyName"></param>
        /// <param name="value"></param>
        public static void SetProperty
        (
            this List<ICssProperty> listProperties,
            IBrowsingContext browsingContext,
            string propertyName,
            string propertyValue
        )
        {
            if (!string.IsNullOrEmpty(propertyValue))
            {
                var property = browsingContext.CreateProperty(propertyName);
                if (property != null)
                {
                    property.Value = propertyValue;

                    if (property.RawValue != null)
                    {
                        _SetProperty(property, browsingContext, listProperties); //
                    }
                }
            }
            else
            {
                for (int i = listProperties.Count - 1; i >= 0; i--)
                {
                    var p = listProperties[i];
                    if (p.Name.Isi(propertyName))
                    {
                        listProperties.RemoveAt(i);
                    }
                }

                // RemoveProperty(propertyName);
            }
        }

        static void _SetProperty(ICssProperty property, IBrowsingContext context, List<ICssProperty> properties)
        {
            if (property.IsShorthand)
            {
                _SetShorthand(property, context, properties);
            }
            else
            {
                _SetLonghand(property, properties);
            }
        }


        /// <summary>
        /// 速记写法
        /// </summary>
        /// <param name="shorthand"></param>
        /// <param name="context"></param>
        static void _SetShorthand(ICssProperty shorthand, IBrowsingContext context, List<ICssProperty> properties)
        {
            var createNewProperties = context.CreateLonghands(shorthand);

            if (createNewProperties != null)
            {
                foreach (var property in createNewProperties)
                {
                    _SetProperty(property, context, properties);
                }
            }
        }

        static void _SetLonghand(ICssProperty property, List<ICssProperty> _declarations)
        {
            for (var i = 0; i < _declarations.Count; i++)
            {
                var declaration = _declarations[i];

                if (declaration.Name.Is(property.Name))
                {
                    _declarations[i] = property;
                    return;
                }
            }

            _declarations.Add(property);
        }

        // public static ICssProperty GetProperty(this List<ICssProperty> listProperties, string keyName)
        // {
        // }
        //

        // public static string (this IHtmlElement htmlElement) this []
        // {
        // }
    }
}