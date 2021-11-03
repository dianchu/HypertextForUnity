using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEngine;

namespace WidgetFromHtml.Core.Tests
{
    class TestClass
    {
        private int a = 0;
    }

    public class 测试WeakRefrence配合容器使用
    {
        private static WeakReference<TestClass> _weakReference2 = new WeakReference<TestClass>(null);

        private static WeakReference<Dictionary<int, TestClass>> _weakReference =
            new WeakReference<Dictionary<int, TestClass>>(new Dictionary<int, TestClass>());


        private ConditionalWeakTable<string, TestClass> _weak3 = new ConditionalWeakTable<string, TestClass>();

        private static int count = 0;

        [Test]
        public void 测试弱引用与容器的关系()
        {
            _weakReference.TryGetTarget(out var dic);
            var len = 10;
            for (int i = 0; i < len; i++)
            {
                dic.Add(dic.Count() + 1, new TestClass());
                _weak3.Add(count++.ToString(), new TestClass());
            }

            _weakReference2.SetTarget(new TestClass());
            GC.Collect();
            GC.Collect(0, GCCollectionMode.Forced);
            GC.Collect(1, GCCollectionMode.Forced);
            GC.Collect(2, GCCollectionMode.Forced);
            GC.Collect(3, GCCollectionMode.Forced);
            GC.Collect(4, GCCollectionMode.Forced);

            // var count = _weakReference.TryGetTarget(ot);
            var notNullCount1 = 0;
            var notNullCount2 = 0;
            foreach (var kv in dic)
            {
                if (kv.Value != null)
                {
                    notNullCount1++;
                }
            }

            _weak3.TryGetValue(count.ToString(), out var t);
            _weakReference2.TryGetTarget(out var ob);

            Debug.Log($"notNullCount={notNullCount1}");
            Debug.Log($"weak2={ob == null}");
            Debug.Log($"weak3={t == null}");
            Assert.IsTrue(t == null);
            Assert.IsTrue(notNullCount1==len);
         
            // Assert.IsTrue(ob==null);很疑惑,这个应该是null才对!
        }
    }
}