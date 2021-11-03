using System;
using System.Collections.Generic;
using AngleSharp.Css.Dom;
using JetBrains.Annotations;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace WidgetFromHtml.Core
{
    internal class TextStyleHtml
    {
        private readonly IEnumerable<object> _deps;

        //The line height
        public float? height;

        //The number of max lines that should be rendered 
        public readonly int? maxLines;

        //The parent style
        internal readonly TextStyleHtml parent;

        //The input [TextStyle]
        public readonly TextStyle style;

        //The text alignment
        public readonly TextAlign? textAlign;

        //The text direction
        public readonly TextDirection textDirection;
        public readonly TextOverflow? textOverflow;
        public readonly Whitespace whitespace;

        TextStyleHtml
        (
            IEnumerable<object> deps,
            TextStyle style,
            TextDirection textDirection,
            Whitespace whitespace,
            float? height = null,
            int? maxLines = null,
            TextStyleHtml parent = null,
            TextAlign? textAlign = null,
            TextOverflow? textOverflow = null
        )
        {
            _deps = deps;
            this.height = height;
            this.maxLines = maxLines;
            this.parent = parent;
            this.style = style;
            this.textAlign = textAlign;
            this.textDirection = textDirection;
            this.textOverflow = textOverflow;
            this.whitespace = whitespace;
        }

        public static TextStyleHtml _
        (
            IEnumerable<object> deps,
            Whitespace whitespace,
            TextDirection textDirection,
            TextStyle style,
            float? height = null,
            int? maxLines = null,
            TextStyleHtml parent = null,
            TextAlign? textAlign = null,
            TextOverflow? textOverflow = null
        )
        {
            return new TextStyleHtml
            (
                deps,
                style,
                textDirection,
                whitespace,
                height,
                maxLines,
                parent,
                textAlign,
                textOverflow
            );
        }

        public static TextStyleHtml root(IEnumerable<object> deps, TextStyle widgetStyle)
        {
            var style = _getDependency<TextStyle>(deps);
            if (widgetStyle != null)
            {
                style = widgetStyle.inherit ? style.merge(widgetStyle) : widgetStyle;
            }

            var mqd = _getDependency<MediaQueryData>(deps);
            var tsf = mqd.textScaleFactor;
            var fontSize = style.fontSize;
            if (tsf != 1 && fontSize != null)
            {
                style = style.copyWith(fontSize: fontSize * tsf);
            }

            return new TextStyleHtml
            (
                deps,
                style: style,
                textDirection: _getDependency<TextDirection>(deps),
                whitespace: Whitespace.Normal
            );
        }

        public TextStyle styleWithHeight =>
            height != null && height.Value >= 0 ? style.copyWith(height: height) : style;


        // public TextStyleHtml copyWith(TextStyleHtml other)
        // {
        //     return new TextStyleHtml(
        //         deps: _deps,
        //         style: other.style ?? this.style,
        //         textDirection: textDirection,
        //         whitespace: this.whitespace,
        //         height: other.height ?? this.height,
        //         maxLines: other.maxLines ?? this.maxLines,
        //         parent: other.parent ?? this.parent,
        //         textAlign: other.textAlign ?? this.textAlign,
        //         textOverflow: other.textOverflow ?? this.textOverflow
        //     );
        // }

        public TextStyleHtml copyWith
        (
            float? height = null,
            int? maxLines = null,
            TextStyleHtml parent = null,
            TextStyle style = null,
            TextAlign? textAlign = null,
            TextDirection? textDirection = null,
            TextOverflow? textOverflow = null,
            Whitespace? whitespace = null
        )
        {
            return TextStyleHtml._(
                _deps,
                whitespace ?? this.whitespace,
                textDirection ?? this.textDirection,
                style ?? this.style,
                height: height ?? this.height,
                maxLines: maxLines ?? this.maxLines,
                parent: parent ?? this.parent,
                textAlign: textAlign ?? this.textAlign,
                textOverflow: textOverflow ?? this.textOverflow
            );
        }

        public T getDependency<T>() => _getDependency<T>(_deps);

        static T _getDependency<T>(IEnumerable<object> deps)
        {
            foreach (var o in deps)
            {
                if (o is T instance)
                    return instance;
            }

            throw new Exception($"The {typeof(T)} dependency could not be found");
        }
    }

    internal delegate TextStyleHtml TextStyleHtmlBuilderDelegate<in T>(TextStyleHtml tsh, T input);

    // internal interface ITextStyleBuilder
    // {
    //     TextStyleHtml build(BuildContext context);
    //     bool HasBuilders { get; }
    //     ITextStyleBuilder parent { get; }
    //
    //     void enqueue<TInput>(Func<TextStyleHtml,TInput,TextStyleHtml> builder, TInput input);
    // }

    // A text styling builder
    internal class TextStyleBuilder
    {
        public readonly TextStyleBuilder parent;

        // private List<TextStyleHtmlBuilderDelegate<TextStyleHtml>> _builders = new List<TextStyleHtmlBuilderDelegate<TextStyleHtml>>();
        private List<Delegate> _builders;

        //sdart处, 该容器为dynamic类型, dart允许一个泛型容器不接受类型安全检查,并且容器中的每个元素类型可以不一致!https://xmdc.yuque.com/staff-nt39y2/xh7p4o/tm7i8r#HCKFr
        private List<object> _inputs;

        private TextStyleHtml _parentOutput;
        private TextStyleHtml _output;

        protected TextStyleBuilder(TextStyleBuilder parent = null)
        {
            this.parent = parent;
        }


        public void enqueue<TInput>(Func<TextStyleHtml, TInput, TextStyleHtml> builder, TInput input)
        {
            D.assert(() => _output == null, () => "Cannot add builder after beging build");

            if (_builders == null)
            {
                _builders = new List<Delegate>();
            }

            // if (_builders == null) _builders = new List<TextStyleHtmlBuilderDelegate<TextStyleHtml>>();
            _builders?.Add(builder);

            if (_inputs == null)
            {
                _inputs = new List<object>();
            }

            _inputs?.Add(input);
        }

        // bool HasBuilders => _builders != null && _builders.Count > 0;


        // Builds a [TextStyleHtml] by calling queued callbacks
        public virtual TextStyleHtml build(BuildContext context)
        {
            var parentOutput = parent?.build(context);
            if (parentOutput == null || parentOutput != _parentOutput)
            {
                _parentOutput = parentOutput;
                _output = null;
            }

            if (_output != null) return _output;
            if (_builders == null) return _output = _parentOutput;

            _output = _parentOutput?.copyWith(parent: _parentOutput);

            var l = _builders?.Count;
            for (var i = 0; i < l; i++)
            {
                var builder = _builders[i];
                _output = (TextStyleHtml)builder.DynamicInvoke(_output, _inputs[i]); //装箱拆箱!
                D.assert(_output?.parent == _parentOutput);
            }

            return _output;
        }

        /// <summary>
        ///  Returns `true` if this shares same styling with [other].
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal bool hasSameStyleWith(TextStyleBuilder other)
        {
            if (other == null) return false;
            TextStyleBuilder thisWithBuilder = this;
            while (thisWithBuilder._builders == null)
            {
                var thisParent = thisWithBuilder.parent;
                if (thisParent == null)
                {
                    break;
                }
                else
                {
                    thisWithBuilder = thisParent;
                }
            }

            var otherWithBuilder = other;
            while (otherWithBuilder._builders == null)
            {
                var otherParent = otherWithBuilder.parent;
                if (otherParent == null)
                {
                    break;
                }
                else
                {
                    otherWithBuilder = otherParent;
                }
            }

            return thisWithBuilder == otherWithBuilder;
        }

        internal TextStyleBuilder sub() => new TextStyleBuilder(this);

        public override string ToString()
        {
            return $"tsb#{GetHashCode()}" + (parent != null ? $"(parent=#{parent.GetHashCode()}" : "");
        }
    }
}