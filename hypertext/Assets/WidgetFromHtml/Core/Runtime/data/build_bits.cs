using System;
using System.Collections.Generic;
using System.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace WidgetFromHtml.Core
{
        
          

    internal abstract class BuildBit
    {
        /// <summary>
        /// The container tree.
        /// </summary>
        public AbsBuildTree parent;

        /// <summary>
        /// The associated [TextStyleBuilder].
        /// </summary>
        public TextStyleBuilder tsb;


        protected BuildBit
        (
            AbsBuildTree parent,
            TextStyleBuilder tsb
        )
        {
            this.parent = parent;
            this.tsb = tsb;
        }


        /// <summary>
        /// Returns true if this bit should be rendered inline.
        /// </summary>
        public virtual bool isInline => true;

        /// <summary>
        /// The next bit in the tree.
        ///
        /// Note: the next bit may not have the same parent or grandparent,
        /// it's only guaranteed to be within the same tree.
        /// </summary>
        /// <returns></returns>
        public BuildBit next
        {
            get
            {
                BuildBit x = this;

                while (x != null)
                {
                    var siblings = x.parent?._children;
                    var i = siblings?.IndexOf(x) ?? -1;
                    if (i == -1) return null;

                    for (var j = i + 1; j < siblings.Count; j++)
                    {
                        var candidate = siblings[j];
                        if (candidate is AbsBuildTree candidateAbstree)
                        {
                            var first = candidateAbstree.first;
                            if (first != null) return first;
                        }
                        else
                        {
                            return candidate;
                        }
                    }

                    x = x.parent;
                }

                return null;
            }
        }

        /// <summary>
        /// The previous bit in the tree.
        ///
        /// Note: the previous bit may not have the same parent or grandparent,
        /// it's only guaranteed to be within the same tree.
        /// </summary>
        BuildBit prev
        {
            get
            {
                BuildBit x = this;

                while (x != null)
                {
                    var siblings = x.parent?._children;
                    var i = siblings?.IndexOf(x) ?? -1;
                    if (i == -1) return null;

                    for (var j = i - 1; j > -1; j--)
                    {
                        var candidate = siblings[j];
                        if (candidate is AbsBuildTree candidateAbsBuildTree)
                        {
                            var last = candidateAbsBuildTree.last;
                            if (last != null) return last;
                        }
                        else
                        {
                            return candidate;
                        }
                    }

                    x = x.parent;
                }

                return null;
            }
        }


        /// <summary>
        ///   /// Controls whether to swallow following whitespaces.
        ///
        /// Returns `true` to swallow, `false` to accept whitespace.
        /// Returns `null` to use configuration from the previous bit.
        ///
        /// By default, do swallow if not [isInline].
        /// </summary>
        public virtual bool? swallowWhitespace => !isInline;

        /// <summary>
        /// Builds input into output.
        /// 
        /// Supported input types:
        /// - [BuildContext] (output must be `Widget`)
        /// - [GestureRecognizer]
        /// - [Null]
        /// - [TextStyleHtml] (output must be `InlineSpan`)
        /// 
        /// Supported output types:
        /// - [BuildTree]
        /// - [GestureRecognizer]
        /// - [InlineSpan]
        /// - [String]
        /// - [Widget]
        /// 
        /// Returning an unsupported type or `null` will not trigger any error.
        /// The output will be siliently ignored.
        /// </summary>
        /// <param name="input"></param>
        /// <typeparam name="InputType"></typeparam>
        /// <typeparam name="OutputType"></typeparam>
        /// <returns></returns>
        public abstract object buildBit(object input);


        

        
        /// <summary>
        /// 映射Dart中 BuildBit中的泛型...
        /// </summary>
        public  abstract Type buildBitInputTyp { get; }

        public  abstract Type buildBitOutputTyp { get; }

        public virtual bool buildBitInputIs<TInput>()
        {
            var t = typeof(TInput);
            return t.Is(buildBitInputTyp);
        }

        public virtual bool buildBitOutputIs<TOutput>()
        {
            var t = typeof(TOutput);
            return t.Is(buildBitOutputTyp);
        }


        /// Creates a copy with the given fields replaced with the new values.
        public abstract BuildBit copyWith(AbsBuildTree parent = null, TextStyleBuilder tsb = null);


        /// <summary>
        /// Removes self from [parent].
        /// </summary>
        /// <returns></returns>
        public bool detach() => parent?._children.Remove(this) ?? false;

        /// <summary>
        /// Inserts self after [another] in the tree.
        /// </summary>
        /// <param name="another"></param>
        /// <returns></returns>
        public bool insertAfter(BuildBit another)
        {
            if (parent == null) return false;

            D.assert(parent == another.parent);
            var siblings = parent._children;
            var i = siblings.IndexOf(another);
            if (i == -1)
            {
                return false;
            }

            siblings.Insert(i + 1, this);
            return true;
        }

        /// <summary>
        /// Inserts self before [another] in the tree.
        /// </summary>
        /// <param name="another"></param>
        /// <returns></returns>
        public bool insertBefore(BuildBit another)
        {
            if (parent == null) return false;

            D.assert(parent == another.parent);
            var siblings = parent._children;
            var i = siblings.IndexOf(another);
            if (i == -1) return false;

            siblings.Insert(i, this);
            return true;
        }

        public override string ToString()
        {
            return $"${this.GetType().Name} ${this.GetHashCode()} ${tsb}";
        }
    }


    internal abstract class AbsBuildTree : BuildBit
    {
        public List<BuildBit> _children { get; private set; } = new List<BuildBit>();

        StringBuilder _toStringBuffer = new StringBuilder();

        public AbsBuildTree(AbsBuildTree parent, TextStyleBuilder tsb) : base(parent, tsb)
        {
        }
        
        

        public void bits(Action<BuildBit> onIterate)
        {
            _bits(onIterate, _children);
        }

        /// <summary>
        /// 递归迭代BuildBit
        /// dart写法,
        ///   Iterable<BuildBit> get bits sync* {
        ///     for (final child in _children) {
        ///         if (child is BuildTree) {
        ///             yield* child.bits;
        ///         } else {
        ///             yield child;
        ///         }
        ///     }
        /// }
        /// </summary>
        void _bits(Action<BuildBit> onIterate, IEnumerable<BuildBit> childs)
        {
            foreach (var child in childs)
            {
                if (child is AbsBuildTree childTree)
                {
                    _bits(onIterate, childTree._children);
                }
                else
                {
                    onIterate(child);
                }
            }
        }

        public List<BuildBit> bits_toList()
        {
            var retList = new List<BuildBit>();

            void ONIterate(BuildBit bit)
            {
                retList.Add(bit);
            }

            bits(ONIterate);
            return retList;
        }

        /// <summary>
        ///The first bit (recursively).
        /// </summary>
        /// <returns></returns>
        public BuildBit first => _first(_children);


        /// <summary>
        ///The first bit (recursively).
        /// </summary>
        /// <returns></returns>
        BuildBit _first(List<BuildBit> bits)
        {
            for (int i = 0; i < bits.Count; i++)
            {
                var child = bits[i];
                if (child is AbsBuildTree childTree)
                {
                    return _first(childTree._children);
                }
                else
                {
                    return child;
                }
            }

            return null;
        }


        public bool isEmpty => _isEmpty(_children);

        bool _isEmpty(List<BuildBit> bits)
        {
            for (int i = 0; i < bits.Count; i++)
            {
                var child = bits[i];
                if (child is AbsBuildTree absBuildTree)
                {
                    if (!_isEmpty(absBuildTree._children))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public BuildBit last => _last(_children);

        BuildBit _last(List<BuildBit> bits)
        {
            for (int i = bits.Count - 1; i >= 0; i--)
            {
                var child = bits[i];
                if (child is AbsBuildTree absBuildTree)
                {
                    return _last(absBuildTree._children);
                }
                else
                {
                    return child;
                }
            }

            return null;
        }


        /// <summary>
        /// Adds [bit] as the last bit.
        /// </summary>
        /// <param name="bit"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T add<T>(T bit) where T : BuildBit
        {
            D.assert(bit.parent == this);
            _children.Add(bit);
            return bit;
        }


        /// <summary>
        /// Adds a new line.
        /// </summary>
        /// <returns></returns>
        public BuildBit addNewLine()
        {
            return add<_SwallowWhitespaceBit>(new _SwallowWhitespaceBit(this, 10));
        }


        /// <summary>
        /// Adds whitespace.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public BuildBit addWhitespace(string data)
        {
            return add<WhitespaceBit>(new WhitespaceBit(this, data));
        }


        public TextBit addText(string data)
        {
            return add<TextBit>(new TextBit(this, data));
        }

        /// <summary>
        /// Builds widgets from bits.
        /// </summary>
        /// <returns></returns>
        // IEnu <WidgetPlaceholder> build();
        public abstract IEnumerable<WidgetPlaceholder> build();

        public abstract List<Widget> getBuiltWidgetsOrNull { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input">传入null即可..</param>
        /// <typeparam name="InputType"></typeparam>
        /// <typeparam name="OutputType"></typeparam>
        /// <returns></returns>
        public override object buildBit(object input)
        {
            return build();
        }

        private static Type _typeIeWidgets = typeof(IEnumerable<Widget>);

        public override Type buildBitInputTyp => Const.kTypNull;
        public override Type buildBitOutputTyp => _typeIeWidgets;
        

        public override BuildBit copyWith(AbsBuildTree parent = null, TextStyleBuilder tsb = null)
        {
            var copied = sub(parent: parent ?? this.parent, tsb: tsb ?? this.tsb);
            foreach (var bit in _children)
            {
                copied.add(bit.copyWith(parent: copied));
            }

            return copied;
        }

        /// Replaces all children bits with [another].
        public void replaceWith(BuildBit another)
        {
            D.assert(another.parent == this);

            _children.Clear();
            _children.Add(another);
            // _children
            //         ..clear()
            //     ..add(another);
        }


        /// <summary>
        /// Creates a sub tree.
        ///
        /// Remember to call [add] to connect the new tree to a parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tsb"></param>
        /// <returns></returns>
        public abstract AbsBuildTree sub(AbsBuildTree parent = null, TextStyleBuilder tsb = null);

        public override string ToString()
        {
            if (_toStringBuffer.Length > 0) return $"{this.GetType().Name}#{this.GetHashCode()}";

            var sb = _toStringBuffer;
            sb.AppendLine($"{this.GetType().Name}#{this.GetHashCode()}");

            const string _indent = "    ";
            foreach (var child in
                _children)
            {
                var strChild = child.ToString().Replace("\n", $"\n{_indent}");
                sb.Append($"{_indent}${strChild}\n");
            }

            var str = sb.ToString().TrimEnd();
            sb.Clear();
            return str;
        }
    }


    internal class TextBit : BuildBit
    {
        public string data;

        public TextBit
        (
            AbsBuildTree parent,
            string data,
            TextStyleBuilder tsb = null
        )
            : base(parent, tsb ?? parent.tsb)
        {
            this.data = data;
        }

        public override object buildBit(object input)
        {
            return this.data;
        }

        public override Type buildBitInputTyp => Const.kTypNull;
        public override Type buildBitOutputTyp =>Const.kTypString;
        
        public override BuildBit copyWith(AbsBuildTree parent = null, TextStyleBuilder tsb = null)
        {
            return new TextBit(parent ?? this.parent, data, tsb ?? this.tsb);
        }

        public override string ToString()
        {
            return $"\"{data}\"";
        }

        private static Type tStr = typeof(string);
    }

    internal class WidgetBit : BuildBit
    {
        public PlaceholderAlignment? alignment;

        public TextBaseline? baseline;

        /// <summary>
        /// The widget to be rendered
        /// </summary>
        public WidgetPlaceholder child;

        public WidgetBit(AbsBuildTree parent, TextStyleBuilder tsb) : base(parent, tsb)
        {
        }


        public static WidgetBit _
        (
            AbsBuildTree parent,
            TextStyleBuilder tsb,
            WidgetPlaceholder child,
            PlaceholderAlignment? alignment = null, //参考dart这边的写法,  child alignment baseline 都是可选的构造参数
            TextBaseline? baseline = null
        )
        {
            var ret = new WidgetBit(parent, tsb)
            {
                child = child,
                alignment = alignment,
                baseline = baseline
            };
            return ret;
        }


        public static WidgetBit block
        (
            AbsBuildTree parent,
            Widget child,
            TextStyleBuilder tsb = null
        )
        {
            return _(parent, tsb ?? parent.tsb, WidgetPlaceholder.lazy(child));
        }

        public static WidgetBit inline
        (
            AbsBuildTree parent,
            Widget child,
            PlaceholderAlignment alignment = PlaceholderAlignment.baseline,
            TextBaseline baseline = TextBaseline.alphabetic,
            TextStyleBuilder tsb = null
        )
        {
            return _(parent, tsb ?? parent.tsb, WidgetPlaceholder.lazy(child), alignment, baseline);
        }


        public override bool isInline => alignment != null && baseline != null;

        public override object buildBit(object input)
        {
            if (isInline)
            {
                return new WidgetSpan(child, baseline.Value, alignment: alignment.Value);
            }
            else
            {
                return child;
            }
        }

        public override Type buildBitInputTyp => Const.kTypNull;
        public override Type buildBitOutputTyp => Const.kTypObject;

        public override BuildBit copyWith(AbsBuildTree parent = null, TextStyleBuilder tsb = null)
        {
            return _(parent ?? this.parent, tsb ?? this.tsb, child, alignment, baseline);
        }

        public override string ToString()
        {
            var str = isInline ? "inLine" : "block";
            return $"WidgetBit.{str} #{GetHashCode()} {child}";
        }
    }


    internal class WhitespaceBit : BuildBit
    {
        public string data;

        public WhitespaceBit
        (
            AbsBuildTree parent,
            string data,
            TextStyleBuilder tsb = null
        ) : base(parent, tsb ?? parent.tsb)
        {
            this.data = data;
        }


        public override bool? swallowWhitespace => true;
        public override object buildBit(object input) => data;
        public override Type buildBitInputTyp => Const.kTypNull;
        public override Type buildBitOutputTyp => Const.kTypString;

        public override BuildBit copyWith(AbsBuildTree parent = null, TextStyleBuilder tsb = null)
        {
            return new WhitespaceBit(parent ?? this.parent, this.data, tsb ?? this.tsb);
        }

        public override string ToString()
        {
            var codes = data.codeUnitsAndJoinSpaceStr();
            return $"Whitespace[' {codes} ']#{GetHashCode()}";
        }
    }

    internal class _SwallowWhitespaceBit : BuildBit
    {
        public int charCode;

        public _SwallowWhitespaceBit
        (
            AbsBuildTree parent,
            int charCode,
            TextStyleBuilder tsb = null
        ) : base(parent, tsb?? parent.tsb)
        {
            this.charCode = charCode;
        }

        public override bool? swallowWhitespace => true;

        public override object buildBit(object input)
        {
            return StringEx.fromCharCode(charCode);
        }

        public override Type buildBitInputTyp => Const.kTypNull;
        public override Type buildBitOutputTyp => Const.kTypString;

        public override BuildBit copyWith(AbsBuildTree parent = null, TextStyleBuilder tsb = null)
        {
            return new _SwallowWhitespaceBit(parent ?? this.parent, charCode, tsb ?? this.tsb);
        }

        public override string ToString()
        {
            return $"ASCII-{StringEx.fromCharCode(charCode)}";
        }
    }
}