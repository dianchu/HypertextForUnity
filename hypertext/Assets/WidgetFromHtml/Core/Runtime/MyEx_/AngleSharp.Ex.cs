using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Values;
using UnityEngine;

namespace WidgetFromHtml.Core
{
    internal static class AngleSharpLengthEx
    {
        internal static bool isPositive(this Length length)
        {
            return length.Value > 0.0;
        }

        internal static float number(this Length length)
        {
            return (float) length.Value;
        }


        
        internal static float? getValue
        (
            this Length length,
            TextStyleHtml tsh,
            double? baseValue = null,
            double? scaleFactor = null
        )
        {
            double value;
            // switch (unit)
            switch (length.Type)
            {
                case Length.Unit.None:
                    return null;
                case Length.Unit.Em:
                    // baseValue ??= tsh.style.fontSize;
                    if (baseValue == null)
                    {
                        baseValue = tsh.style.fontSize;
                    }

                    if (baseValue == null) return null;
                    value = baseValue.Value * length.Value;
                    scaleFactor = 1;
                    break;
                case Length.Unit.Percent:
                    if (baseValue == null) return null;
                    value = baseValue.Value * length.Value / 100;
                    scaleFactor = 1;
                    break;
                case Length.Unit.Pt:
                    value = length.Value * 96 / 72;
                    // value = length.To(Length.Unit.Pt,null,);
                    break;
                case Length.Unit.Px:
                    value = length.Value;
                    break;
                default:
                    HLog.LogError($"不支持该length单位={length.Type}");
                     throw new ArgumentOutOfRangeException();
                break;
            }

            if (scaleFactor != null) value *= scaleFactor.Value;

            return (float) value;
        }
    }

    internal static class AngleSharpICssPropertyEx
    {
        public static List<ICssValue> values(this ICssProperty cssProperty)
        {
            if (cssProperty is ICssMultipleValue cssMultipleValue)
            {
                Debug.Log(">>>多value的 property>>");
                return cssMultipleValue.ToList();
            }
            else
            {
                HLog.LogInfo("TODO border 加上color>>");
                return new List<ICssValue>(1) {cssProperty.RawValue};
            }
        }
    }
}