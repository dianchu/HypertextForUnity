using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;

namespace WidgetFromHtml.Core
{
    public static class DomElementExtension
    {
        private static ConditionalWeakTable<IElement, List<ICssProperty>> _expando =
            new ConditionalWeakTable<IElement, List<ICssProperty>>();


        /// <summary>
        /// Returns CSS declarations from the element's `style` attribute.
        ///
        /// This is different from [BuildMetadata.styles] as it doesn't include
        /// runtime additions from [WidgetFactory] or [BuildOp]s.
        ///
        /// Parsing CSS is a non-trivial task but the result is cached for each element
        /// so it's safe to call this again and again.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static List<ICssProperty> styles(this IElement element)
        {
            if (_expando.TryGetValue(element, out var retList))
            {
                return retList;
            }
            else
            {
                var addList = new List<ICssProperty>(16); //16 经验值 我也不知道是多少--!
                addList.CollectAllStyleFromSheetAndInline(element);
                _expando.Add(element, addList); //弱引用表,当element和list没人使用之后, 会自动被gc掉!

                return addList;
            }
        }
    }

    #region 并不需要翻译的css扩展代码

    /// <summary>
    /// 对应dart中的CssDeclarationExtension, values value term 使用ICssProperty.RawValue.AsXXX , 可以快速解析, 不需要这个扩展了.
    /// </summary>
    public static class CssDeclarationExtension
    {
    }


    /// <summary>
    /// ICssProperty.RawValue.AsArgb ..
    /// </summary>
    public static class CssFunctionTermExtension
    {
    }

    /// <summary>
    /// ICssProperty.RawValue.AsArgb ..
    /// </summary>
    public static class CssLiteralTermExtension
    {
    }

    #endregion
}