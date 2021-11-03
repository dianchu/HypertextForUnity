using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace WidgetFromHtml.Core
{
    public static partial class OtherEx
    {
        public static int start(this Match match)
        {
            return match.Index;
        }

        public static int end(this Match match)
        {
            return match.Index + match.Length;
            // return match.Index + match.Length-1;
        }

        /// <summary>
        /// 对等于dart中的 group(0)
        /// 以下是group(0的介绍)
        /// 
        /// The string matched by the given [group].
        ///
        /// If [group] is 0, returns the entire match of the pattern.
        ///
        /// The result may be `null` if the pattern didn't assign a value to it
        /// as part of this match.
        ///
        /// </summary>
        /// <param name="match"></param>
        /// <returns></returns>
        public static string group_0(this Match match)
        {
            return match.Value;
        }

        public static List<T> skip_toList<T>(this List<T> list, int count)
        {
            if (count <= 0)
            {
                return new List<T>(list);
            }

            if (list.Count < count)
            {
                HLog.LogError($"skip_toList error args count={count} > listLen={list.Count}");
                return null;
            }

            var retList = new List<T>(list.Count - count);
            for (int i = count; i < list.Count; i++)
            {
                retList.Add(list[i]);
            }

            return retList;
        }

        public static bool isNegative(this float f)
        {
            return f <= 0f;
        }

        public static int whereType_length<TOrigin, TTarget>(this IEnumerable<TOrigin> ie)
        {
            var count = 0;
            foreach (var t in ie)
            {
                if (t is TTarget)
                {
                    count++;
                }
            }

            return count;
        }

        public static float? reduce_max_float(this IEnumerable<float?> ie)
        {
            float? max = float.MinValue;
            var ietor = ie.GetEnumerator();
            bool hasValue = false;
            while (ietor.MoveNext())
            {
                if (ietor.Current != null)
                {
                    max = Math.Max(ietor.Current.Value, max.Value);
                    hasValue = true;
                }
            }

            if (!hasValue)
            {
                return null;
            }

            return max;
        }

        public static float sum(this IEnumerable<float> ie)
        {
            var ietor = ie.GetEnumerator();
            float sum = 0;
            while (ietor.MoveNext())
            {
                sum += ietor.Current;
            }

            return sum;
        }


        /// <summary>
        /// 左闭右开
        /// [startIndex,endIndex)
        /// The substring of this string from [start],inclusive, to [end], exclusive.
        /// 
        /// Example:
        /// ```dart
        /// var string = 'dartlang';
        /// string.substring(1);    // 'artlang'
        /// string.substring(1, 4); // 'art'
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public static string substring(this string str, int startIndex, int endIndex)
        {
            if (startIndex >= str.Length)
            {
                HLog.Ex($"substring error input startIndex >= str>Length index={startIndex}  len = {str.Length}");
                return "";
            }

            var len = endIndex - startIndex;
            if (len <= 0 || len > str.Length)
            {
                HLog.Ex($"substring err args startIndex={startIndex} endIndex={endIndex}");
                return "";
            }

            return str.Substring(startIndex, len);
        }

        /// <summary>
        /// 按照dart的subString写的..
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string substring(this string str, int startIndex)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }

            if (startIndex >= str.Length)
            {
                HLog.Ex($"substring error input startIndex >= str>Length index={startIndex}  len = {str.Length}");
                return "";
            }

            return str.substring(startIndex, str.Length);
        }


        /// <summary>
        /// [startIndex,endIndex] //左闭右闭
        /// </summary>
        /// <param name="fList"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public static float getRange_sum(this IEnumerable<float> fList, int startIndex, int endIndex)
        {
            // if (fList == null)
            // {
            //     HLog.LogError("getRange_sum error  fList is unll");
            //     return 0;
            // }

            float count = 0f;
            var ietor = fList.GetEnumerator();
            int index = 0;
            while (ietor.MoveNext())
            {
                if (index >= startIndex && index < endIndex)
                {
                    count += ietor.Current;
                  
                }
                index++;
            }

            return count;
        }

        public static bool isNewLine(this string str)
        {
            return str == "\n";
        }


        public static bool Is(this Type type, Type other)
        {
            if (other == null)
            {
                return false;
            }

            if (other == type)
            {
                return true;
            }

            // if (type.IsAssignableFrom(other))
            if (other.IsAssignableFrom(type))
            {
                return true;
            }

            return false;
        }


        public static bool Is<TOtherTyp>(this Type type)
        {
            var other = typeof(TOtherTyp);
            if (other == type)
            {
                return true;
            }

            if (type.IsAssignableFrom(other))
            {
                return true;
            }

            return false;
        }
    }

    public static class ListEx
    {
        /// <summary>
        /// 构造一个list , 使用fill进行填充
        /// </summary>
        /// <param name="len"></param>
        /// <param name="fill"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> filled<T>(int len, T fill)
        {
            if (len < 0)
            {
                return null;
            }

            var retList = new List<T>(len);

            for (int i = 0; i < len; i++)
            {
                retList.Add(fill);
            }

            return retList;
        }
    }
}