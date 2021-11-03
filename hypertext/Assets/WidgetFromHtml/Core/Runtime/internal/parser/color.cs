using System;
using AngleSharp.Css.Dom;
using Unity.UIWidgets.ui;


namespace WidgetFromHtml.Core
{
    internal static partial class core_parser
    {
        private static readonly float pi = (float) Math.PI;

        public static Color tryParseColor(ICssValue v)
        {
            return new Color((uint) v.AsArgb());
        }

        // static double? _parseColorAlpha(ICssProperty v)
        // {
        //     if (float.TryParse(v.Value, out var fV))
        //     {
        //         return fV.clamp(0, 1.0f);
        //     }
        //
        //     if (v.Value.Contains(Const.kPercentage))
        //     {
        //         return ((float) (v.RawValue.AsPercent())).clamp(0, 1.0f);
        //     }
        //
        //     HLog.LogError($"_parseColorAlpha error  value={v.Value} return 0");
        //     return 0f;
        // }
        //
        //
        // static double _parseColorHue(float number, int? unit)
        // {
        //     HLog.LogError("TODO 不支持使用色相来指定颜色!!!");
        //     var v = number;
        //     float deg = 0;
        //     if (unit != null)
        //     {
        //         switch (unit)
        //         {
        //             // case css.TokenKind.UNIT_ANGLE_RAD:
        //             //     var rad = v;
        //             //     deg = rad * (180f / pi);
        //             //     break;
        //             // case css.TokenKind.UNIT_ANGLE_GRAD:
        //             //     var grad = v;
        //             //     deg = grad * 0.9f;
        //             //     break;
        //             // case css.TokenKind.UNIT_ANGLE_TURN:
        //             //     var turn = v;
        //             //     deg = turn * 360f;
        //             //     break;
        //             default:
        //                 deg = v;
        //                 break;
        //         }
        //     }
        //
        //     while (deg < 0f)
        //     {
        //         deg += 360;
        //     }
        //
        //     return deg % 360;
        // }
        //
        // static int? _parseColorRgbElement(ICssProperty v)
        // {
        //     if (float.TryParse(v.Value, out var fValue))
        //     {
        //         return MathUtils.ceil(fValue).clamp(0, 255);
        //     }
        //
        //     if (v.Value.Contains(Const.kPercentage))
        //     {
        //         var value = (int) (v.RawValue.AsPercent() * 255.0);
        //         return value.clamp(0, 255);
        //     }
        //
        //     HLog.LogError($"_parseColorRgbElement error  value={v.Value} return 0");
        //     return 0;
        //
        //     //
        //     // return (v is css.NumberTerm
        //     //         ? v.number.ceil()
        //     //         : v is css.PercentageTerm
        //     //             ? (v.valueAsDouble * 255.0).ceil()
        //     //             : null)
        //     //     ?.clamp(0, 255);
        // }
        //
        // static string _x2(string value)
        // {
        //     var sb = FsbPool.Get();
        //     for (var i = 0; i < value.Length; i++)
        //     {
        //         sb.Append((char) (value[i] * 2));
        //     }
        //
        //     var ret = sb.ToString();
        //     FsbPool.Release(sb);
        //     return ret;
        // }
    }
}