using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace WidgetFromHtml.Core
{
    public class CharArrayPool
    {
        private int _defaultSize = 0;

        CharArrayPool()
        {
        }

        public CharArrayPool(int defaultSize)
        {
            _defaultSize = defaultSize;
        }

        //池中一般也只有一个..
        private List<char[]> _list = new List<char[]>();
        // private LinkedList<char[]> _list = new LinkedList<char[]>();

        private object _lockObj = new object();

        public char[] Rent(int size)
        {
            lock (_lockObj)
            {
                if (size < 0)
                {
                    HLog.LogError($"CharArrayPool error rent args needSize={size}");
                    return null;
                }

                for (int i = 0; i < _list.Count; i++)
                {
                    var arr = _list[i];
                    if (arr != null && arr.Length >= size)
                    {
                        _list.RemoveAt(i);
                        return arr;
                    }
                }

                return new char[Math.Max(size, _defaultSize)];
            }
        }

        public void Recyle(char[] arrChar)
        {
            lock (_lockObj)
            {
                if (arrChar == null || arrChar.Length == 0)
                {
                    return;
                }

                _list.Add(arrChar);
            }
        }
    }

    public static partial class StringEx
    {
        private const int CharArrSize = 1024; //字符串很少有能够这么长的.
        public static CharArrayPool ArrayPool { get; } = new CharArrayPool(CharArrSize);

        public static List<int> codeUnits(this string str)
        {
            var retList = new List<int>(str.Length);

            foreach (var c in str)
            {
                retList.Add(c);
            }

            return retList;
        }

        public static List<int> codeUnitsAndJoinSpace(this string str)
        {
            var retList = new List<int>(str.Length);

            foreach (var c in str)
            {
                retList.Add(c);
                retList.Add(' ');
            }

            return retList;
        }

        public static string codeUnitsAndJoinSpaceStr(this string str)
        {
            var sb = new StringBuilder();

            foreach (var c in str)
            {
                sb.Append(c);
                sb.Append(' ');
            }

            return sb.ToString();
        }


        public static string fromCharCode(int charCode)
        {
            return $"{(char) charCode}";
        }
    }

    public static partial class StringEx
    {
        // private static char[] CharArray = new char[CharArrSize];

        private static readonly char[] splitCharArr = new char[1] {SpaceChar};
        public const char SpaceChar = ' ';
        public const char StrEndChar = '\0';
        public const string StrR = "\r";
        public const string StrN = "\n";
        public const string StrRN = "\r\n";
        public const char CharR = '\r';
        public const char CharN = '\n';
        public const string StrEmpty = "";
        public const string spaceStr = " ";
        public static readonly char[] RNArr = new[] {'\r', '\n'};
        public static readonly uint CharSize = sizeof(char);


        public static void AppendLineWithN(this StringBuilder sb, string subStr)
        {
            sb.Append(subStr);
            if (!(subStr.EndWithN() || subStr.EndWithRN()))
            {
                sb.Append('\n');
            }


            // if (!(subStr.EndsWith(StrN) || subStr.EndsWith(StrRN)))
            // {
            //     sb.Append('\n');
            // }
        }

        /// <summary>
        /// 指示index所在的是否是指定的char
        /// </summary>
        /// <param name="str"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static unsafe bool IndexOfIsChar(this string str, char c, int index)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (index < 0 || index >= str.Length)
            {
                HLog.LogInfo($"IndexOfIsChar 传入的index非法index={index}");
                return false;
            }


            fixed (char* pStr = str)
            {
                return pStr[index] == c;
            }
        }

        public static unsafe bool IndexOfIsChar(this string str, char[] cArr, int index)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (cArr == null || cArr.Length == 0)
            {
                return false;
            }

            if (index < 0 || (index + cArr.Length) > str.Length)
            {
                HLog.LogError($"IndexOfIsChar 传入的index越界 index={index} cArr.Length={cArr.Length}");
                return false;
            }


            fixed (char* pStr = str)
            {
                for (int i = 0; i < cArr.Length; i++)
                {
                    var c = cArr[i];
                    if (pStr[index + i] != c)
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        public static bool EndWithRN(this string str)
        {
            if (str.Length <= 1)
            {
                return false;
            }

            return str.IndexOfIsChar(RNArr, str.Length - 2);
        }

        public static bool StartWithRN(this string str)
        {
            if (str.Length <= 1)
            {
                return false;
            }

            return str.IndexOfIsChar(RNArr, 0);
        }

        public static bool EndWithN(this string str)
        {
            if (str.Length == 0)
            {
                return false;
            }

            return str.IndexOfIsChar(CharN, str.Length - 1);
        }

        public static bool StartWithN(this string str)
        {
            return str.IndexOfIsChar(CharN, 0);
        }


        public static string RemoveRChar(string strInput)
        {
            if (strInput.Contains(StrR))
            {
                strInput = strInput.Replace(StrR, StrEmpty);
            }

            return strInput;
        }

        public static string ReverseWordsSlowly(string input)
        {
            string[] words = input.Split(splitCharArr);
            Array.Reverse(words);
            var retStr = string.Join(spaceStr, words);
            return retStr;
        }

        public static string ReverseWordsAndTrimSlowly(string input)
        {
            return ReverseWordsSlowly(input).Trim();
        }

        // static void EnsureBufferEnough(int needSize)
        // {
        //     if (CharArray.Length < needSize)
        //     {
        //         CharArray = new char[needSize];
        //     }
        // }

            

    

        

        public static unsafe void Trim(char[] buffer, int originStrLen, out int strBebinIndex, out int strEndIndex)
        {
            strBebinIndex = 0;
            strEndIndex = 0;
            if (buffer == null || buffer.Length == 0)
            {
                return;
            }


            fixed (char* p = buffer)
            {
                int startIndex = 0; //正序遍历
                while (startIndex < originStrLen && char.IsWhiteSpace(*(p + startIndex)))
                {
                    ++startIndex;
                }

                int endIndex = originStrLen - 1; //逆序遍历
                while (endIndex >= startIndex && char.IsWhiteSpace(*(p + endIndex)))
                {
                    --endIndex;
                }

                strBebinIndex = startIndex;
                strEndIndex = endIndex;
            }
        }

            

        
            
    }
}