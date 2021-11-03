using System;
using System.Collections.Generic;
using System.Linq;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Values;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace WidgetFromHtml.Core
{
    internal class Flattened
    {
        public readonly SpanBuilder spanBuilder;
        public readonly Widget widget;
        public readonly WidgetBuilder widgetBuilder;


        public Flattened
        (
            SpanBuilder spanBuilder = null,
            Widget widget = null,
            WidgetBuilder widgetBuilder = null
        )
        {
            this.spanBuilder = spanBuilder;
            this.widget = widget;
            this.widgetBuilder = widgetBuilder;
        }

        public static Flattened _
        (
            SpanBuilder spanBuilder = null,
            Widget widget = null,
            WidgetBuilder widgetBuilder = null
        )
        {
            return new Flattened
            (
                spanBuilder: spanBuilder,
                widget: widget,
                widgetBuilder: widgetBuilder
            );
        }
    }


    internal delegate InlineSpan SpanBuilder(BuildContext context, Whitespace whitespace);


    internal class Flattener : IDisposable
    {
        private List<GestureRecognizer> _recognizers = new List<GestureRecognizer>();
        private List<Flattened> _flattened;

        private _Recognizer _recognizer, _prevRecognizer;

        private List<SpanBuilder> _spans;

        private List<_String> _strings, _prevStrings;
        private bool _swallowWhitespace;

        private TextStyleBuilder _tsb;
        private TextStyleBuilder _prevTsb;


        public void Dispose()
        {
            _reset();
        }

        public void reset()
        {
            _reset();
        }

        public List<Flattened> flatten(AbsBuildTree tree)
        {
            _flattened = new List<Flattened>();
            // 不复用 因为是返回给外部的,外部不知道会不会执行什么操作
            // if (_flattened == null)
            // {
            //     _flattened = new List<Flattened>();
            // }
            // else
            // {
            //     _flattened.Clear();
            // }

            _resetLoop(tree.tsb);
            var bits = tree.bits_toList();
            int min = 0;
            int max = bits.Count - 1;
           
            for (; min <= max; min++)
            {
               
                if (!(bits[min] is WhitespaceBit))
                {
                   
                    break;
                }
            }
           

            for (; max >= min; max--)
            {
               
                if (!(bits[max] is WhitespaceBit))
                {
                    break;
                }
            }
           

            for (var i = min; i <= max; i++)
            {
                _loop(bits[i]);
            }

            _completeLoop();

            return _flattened;
        }


        private void _reset()
        {
            if (_recognizers != null)
            {
                foreach (var recognizer in _recognizers)
                {
                    recognizer.dispose();
                }

                _recognizers.Clear();
            }
        }

        void _resetLoop(TextStyleBuilder tsb)
        {
            _recognizer = new _Recognizer();
            _spans = new List<SpanBuilder>();
            _strings = new List<_String>();
            _swallowWhitespace = true;
            _tsb = tsb;
            _prevRecognizer = _recognizer;
            _prevStrings = _strings;
            _prevTsb = _tsb;
        }


        void _loop(BuildBit bit)
        {
            var thisTsb = _getBitTsb(bit);
            if (_spans == null)
            {
                _resetLoop(thisTsb);
            }
            if (!thisTsb.hasSameStyleWith(_prevTsb))
            {
                _saveSpan();
            }

            object built = null;

            //判定bit的是哪种类型的BuildBit
            // if (bit is BuildBit<Null, dynamic>)
            if (bit.buildBitInputIs<Null>()) //dynamic在dart中,代表任意类型,
            {
                built = bit.buildBit(null);
            }
            // else if (bit is BuildBit<BuildContext, Widget>)
            else if (bit.buildBitInputIs<BuildContext>() && bit.buildBitOutputIs<Widget>())
            {
                // ignore: omit_local_variable_types
                WidgetBuilder widgetBuilder = (c) => (Widget) bit.buildBit(c);
                built = widgetBuilder;
            }
            // else if (bit is BuildBit<GestureRecognizer?, dynamic>)
            else if (bit.buildBitInputIs<GestureRecognizer>())
            {
                built = bit.buildBit(_prevRecognizer.value);
            }
            // else if (bit is BuildBit<TextStyleHtml, InlineSpan>)
            else if (bit.buildBitInputIs<TextStyleHtml>() && bit.buildBitOutputIs<InlineSpan>())
            {
                SpanBuilder spanBuilder = (c, _) => (InlineSpan) bit.buildBit(thisTsb.build(c));
                built = spanBuilder;
            }

            //判定built是哪种类型?
            if (built is GestureRecognizer builtGestureRecognizer)
            {
                _prevRecognizer.value = builtGestureRecognizer;
            }
            else if (built is InlineSpan builtInlineSpan)
            {
                _saveSpan();
                _spans.Add((_, __) => builtInlineSpan);
            }
            else if (built is SpanBuilder builtSpanBuilder )
            {
                _saveSpan();
                _spans.Add(builtSpanBuilder);
            }
            else if (built is string builtstring)
            {
                if (bit is WhitespaceBit)
                {
                    if (!_loopShouldSwallowWhitespace(bit))
                    {
                        _prevStrings.Add(new _String(builtstring, isWhitespace: true));
                    }
                }
                else
                {
                    _prevStrings.Add(new _String(builtstring));
                }
            }
            else if (built is Widget builtWidget)
            {
                _completeLoop();
                _flattened.Add(Flattened._(widget: builtWidget));
            }
            else if (built is WidgetBuilder builtWidgetBuilder)
            {
                _completeLoop();
                _flattened.Add(Flattened._(widgetBuilder: builtWidgetBuilder));
            }

            _prevTsb = thisTsb;
            _swallowWhitespace = bit.swallowWhitespace ?? _swallowWhitespace;

            if (built is AbsBuildTree builtAbsBuildTree )
            {
                builtAbsBuildTree.bits(subBit => { _loop(subBit); });
                // foreach (var subBit in builtAbsBuildTree.bits_toList())
                // {
                // _loop(subBit);
                // }
            }
        }

        bool _loopShouldSwallowWhitespace(BuildBit bit)
        {
            // special handling for whitespaces
            if (_swallowWhitespace) return true;

            var next = nextNonWhitespace(bit);
            if (next == null)
            {
                // skip trailing whitespace
                return true;
            }
            else if (!next.isInline)
            {
                // skip whitespace before a new block
                return true;
            }

            return false;
        }

        void _saveSpan()
        {
            if (_prevStrings != _strings && _prevStrings.isNotEmpty())
            {
                var scopedRecognizer = _prevRecognizer.value;
                var scopedTsb = _prevTsb;
                var scopedStrings = _prevStrings;

                if (scopedRecognizer != null) _recognizers.Add(scopedRecognizer);

                _spans.Add((context, whitespace) => new TextSpan
                (
                    recognizer: scopedRecognizer,
                    style: scopedTsb.build(context).styleWithHeight,
                    text: scopedStrings.toText(whitespace: whitespace)
                ));
            }

            _prevStrings = new List<_String>();
            _prevRecognizer = new _Recognizer();
        }

        void _completeLoop()
        {
            _saveSpan();

            var scopedSpans = _spans;
            if (scopedSpans == null) return;

            _spans = null;
            if (scopedSpans.isEmpty() && _strings.isEmpty()) return;
            var scopedRecognizer = _recognizer.value;
            var scopedTsb = _tsb;
            var scopedStrings = _strings;

            if (scopedRecognizer != null) _recognizers.Add(scopedRecognizer);

            if (scopedSpans.isEmpty() &&
                scopedStrings.Count == 1 &&
                scopedStrings[0].isNewLine)
            {
                // special handling for paragraph with only one line break
                _flattened.Add
                (
                    Flattened._
                    (
                        widget: new HeightPlaceholder(new Length(1, Length.Unit.Em), scopedTsb)
                    )
                );
                return;
            }

            _flattened.Add
            (Flattened._
                (spanBuilder: (context, whitespace) =>
                    {
                        var text = scopedStrings.toText
                        (
                            dropNewLine: scopedSpans.isEmpty(),
                            whitespace: whitespace
                        );

                        // var children = scopedSpans
                        //     .map((s) => s(context, whitespace))
                        //     .whereType<InlineSpan>()
                        //     .toList(growable: false);
                        var children = toListInLineSpan
                        (
                            scopedSpans,
                            context,
                            whitespace
                        );
                        if (text.isEmpty())
                        {
                            if (children.isEmpty()) return null;
                            if (children.length() == 1) return children.first();
                        }

                        return new TextSpan
                        (
                            children: children,
                            recognizer: scopedRecognizer,
                            style: scopedTsb.build(context).styleWithHeight,
                            text: text
                        );
                    }
                )
            );
        }


        TextStyleBuilder _getBitTsb(BuildBit bit)
        {
            if (!(bit is WhitespaceBit)) return bit.tsb;

            // the below code will find the best style for this whitespace bit
            // easy case: whitespace at the beginning of a tag, use the previous style
            var parent = bit.parent;
            if (parent == null || bit == parent.first)
            {
                return _prevTsb;
            }

            // complicated: whitespace at the end of a tag, try to merge with the next
            // unless it has unrelated style (e.g. next bit is a sibling)
            if (bit == parent.last)
            {
                var next = nextNonWhitespace(bit);
                if (next != null)
                {
                    var tree = parent;
                    while (true)
                    {
                        var bitsParentLast = tree.parent?.last;
                        if (bitsParentLast != bit) break;
                        tree = tree.parent;
                    }

                    if (tree.parent == next.parent)
                    {
                        return next.tsb;
                    }
                    else
                    {
                        return _prevTsb;
                    }
                }
            }

            // fallback to default (style from parent)
            return parent.tsb;
        }

        static BuildBit nextNonWhitespace(BuildBit bit)
        {
            var next = bit.next;
            while (next != null && next is WhitespaceBit)
            {
                next = next.next;
            }

            return next;
        }

        static List<InlineSpan> toListInLineSpan
        (
            List<SpanBuilder> listSpanBuilders,
            BuildContext context,
            Whitespace whitespace
        )
        {
            // var children = scopedSpans
            // .map((s) => s(context, whitespace))
            // .whereType<InlineSpan>()
            // .toList(growable: false);
            var retList = new List<InlineSpan>(listSpanBuilders.Count);

            for (int i = 0; i < listSpanBuilders.Count; i++)
            {
                var span = listSpanBuilders[i]?.Invoke(context, whitespace);
                if (span is InlineSpan inlineSpan)
                {
                    retList.Add(inlineSpan);
                }
            }

            return retList;
        }
    }

    internal class _Recognizer
    {
        public GestureRecognizer value;
    }


    internal class _String
    {
        public string data { get; set; }
        public bool isWhitespace { get; set; }

        public _String(string data, bool isWhitespace = false)
        {
            this.data = data;
            this.isWhitespace = isWhitespace;
        }

        public bool isNewLine => data == "\n";
    }


    //Other.Ex2 _StringListToText
    // extension _StringListToText on List<_String>
}