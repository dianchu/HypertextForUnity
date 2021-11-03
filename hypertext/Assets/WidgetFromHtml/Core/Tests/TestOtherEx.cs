using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace WidgetFromHtml.Core.Tests
{
    public class TestOtherEx
    {
        interface IA
        {
            
        }
        class A:IA
        {
        }

        class B : A
        {
        }

        class C : B
        {
        }


        IEnumerable<C> getList(int count)
        {
            var list = new List<C>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(new C());
            }

            return list;
        }



        [Test]
        public void 测试For循环()
        {
            
            int min = 0;
            int max = 0;

            for (; min <= max; min++) {
                break;
            }
            HLog.LogInfo($"min={min} max={max}");
            min = 0;
            max = 0;

            for (; min <= max; min++) {}
            HLog.LogInfo($"min={min} max={max}");
            
        }
        
        
        [Test]
        public void 测试TypeIs()
        {
            Assert.IsTrue(typeof(C).Is(typeof(A)));
            Assert.IsTrue(typeof(C).Is(typeof(IA)));
            Assert.IsTrue(typeof(B).Is(typeof(A)));
            Assert.IsTrue(typeof(B).Is(typeof(IA)));
            Assert.IsTrue(typeof(A).Is(typeof(IA)));
            Assert.IsTrue(typeof(A).Is(typeof(A)));
            Assert.IsTrue(typeof(B).Is(typeof(B)));
            Assert.IsTrue(typeof(C).Is(typeof(C)));
            Assert.IsTrue(typeof(C).Is(typeof(object)));
            
            Assert.IsFalse(typeof(A).Is(typeof(B)));
            Assert.IsFalse(typeof(int).Is(typeof(B)));
        }

        [Test]
        public void 测试RefrenceEqual()
        {
            A a = null;
            Assert.IsTrue(ReferenceEquals(a, null));
        }

        [TestCase("1234", 1, 2, "2")]
        [TestCase("1234", 0, 3, "123")]
        [TestCase("1234", 0, 1, "1")]
        public void 测试subString_startIndex_endIndex(string input, int startIndex, int endIndex, string result)
        {
            var ret = input.substring(startIndex, endIndex);
            Assert.IsTrue(result == ret);
        }


        [TestCase("1234", 1, "234")]
        [TestCase("1234", 0, "1234")]
        [TestCase("1234", 3, "4")]
        // [TestCase("1234", 0,   "1234")]
        public void 测试subString_startIndex(string input, int startIndex, string result)
        {
            var ret = input.substring(startIndex);
            Assert.IsTrue(result == ret);
        }

        [TestCase("1234", -1, 4)]
        [TestCase("1234", -1, 4)]
        [TestCase("1234", 0, -1)]
        public void 测试subString_error(string input, int startIndex, int endIndex)
        {
            Assert.Throws<Exception>(() => input.substring(startIndex, endIndex));
        }

        [TestCase(new int[] { 1, 1, 1, 1 }, 1, 4)]
        [TestCase(new int[] { 1, 1, 1, 1 }, 0, 0)]
        [TestCase(new int[] { 1, 1, 1, 1 }, 0, -1)]
        public void 测试List_filled(int[] results, int filledNum, int count)
        {
            var listT = ListEx.filled(count, filledNum);

            if (count < 0)
            {
                Assert.IsTrue(listT == null);
                return;
            }

            if (count == 0)
            {
                Assert.IsTrue(listT.Count == 0);
                return;
            }


            var listResults = new List<int>(results);
            var listTest = ListEx.filled(count, filledNum);
            for (int i = 0; i < listResults.Count; i++)
            {
                Assert.IsTrue(listResults[i] == listTest[i]);
            }
        }

        [Test]
        public void 测试List容器的强制类型转换()
        {
            int count = 10;
            List<A> listA = getList(count) as List<A>;
            Assert.IsTrue(listA == null);
            // Assert.IsTrue(listA.Count == count);
            // Assert.IsTrue(listA.First() != null);
        }

        [TestCase(new float[] { 1, 2, 3, 4, 5, 6, 7 }, 0, 4, 10)]
        public void 测试_getRange_sum(float[] nums, int startIndex, int endIndex, float sum)
        {
            var list = new List<float>(nums);
            var ret = list.getRange_sum(startIndex, endIndex);
            Assert.IsTrue(ret == sum);
        }


        [TestCase(" 123 456 789 ", 0, 1, 12, 13)]
        public void 测试_regExpSpaceTrailing__regExpSpaceTrailing
        (
            string str,
            int leadingStart,
            int leadingEnd,
            int trailingStart,
            int trailingEnd
        )
        {
            var leading = Const._regExpSpaceLeading.Match(str);
            var trailing = Const._regExpSpaceTrailing.Match(str);


            var ls = leading.start();
            var le = leading.end();
            var ts = trailing.start();
            var te = trailing.end();

            Assert.IsTrue(ls == leadingStart);
            Assert.IsTrue(le == leadingEnd);
            Assert.IsTrue(ts == trailingStart);
            Assert.IsTrue(te == trailingEnd);
        }


        [TestCase(new int[] { 1, 2, 3, 4, 5 }, 2, new int[] { 3, 4, 5 })]
        [TestCase(new int[] { 1, 2, 3, 4, 5 }, -1, new int[] { 1, 2, 3, 4, 5 })]
        [TestCase(new int[] { 1, 2, 3, 4, 5 }, 0, new int[] { 1, 2, 3, 4, 5 })]
        [TestCase(new int[] { 1, 2, 3, 4, 5 }, 5, new int[] { })]
        [TestCase(new int[] { 1, 2, 3, 4, 5 }, 6, null)]
        public void 测试skip_toList(int[] input, int count, int[] outPut)
        {
            if (count > input.Length)
            {
                Assert.IsTrue(new List<int>(input).skip_toList(count) == null);
                return;
            }

            var list1 = new List<int>(input);
            var list2 = new List<int>(outPut);
            var list3 = list1.skip_toList(count);
            for (int i = 0; i < list3.Count; i++)
            {
                Assert.IsTrue(list2[i] == list3[i]);
            }
        }
    }
}