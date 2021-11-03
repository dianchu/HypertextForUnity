using System;
using AngleSharp.Text;
using Unity.UIWidgets.gestures;

namespace WidgetFromHtml.Core
{
    internal class TagQ
    {
        public WidgetFactory wf;

        public TagQ(WidgetFactory wf)
        {
            this.wf = wf;
        }

        public BuildOp buildOp => new BuildOp
        (
            onTree: (_, tree) =>
                core_ops.wrapTree(tree, append: _TagQBit.closing, prepend: _TagQBit.opening)
        );
    }

    class _TagQBit : BuildBit
    {
        public bool isOpening;

        public _TagQBit
        (
            AbsBuildTree parent,
            TextStyleBuilder tsb,
            bool isOpening
        ) : base(parent, tsb)
        {
            this.isOpening = isOpening;
        }

        public override object buildBit(object input)
        {
            // isOpening ? '“' : '”';
            //  201c 左引号 201d右引号
            return
                isOpening
                    ? "\u201C"
                    : "\u201D"; //https://www.ltool.net/characters-to-unicode-charts-in-simplified-chinese.php?unicode=71
        }

        private static Type _typGestureRecognizer = typeof(GestureRecognizer);

        public override Type buildBitInputTyp => _typGestureRecognizer;
        public override Type buildBitOutputTyp => _typGestureRecognizer;

        public override BuildBit copyWith(AbsBuildTree parent = null, TextStyleBuilder tsb = null)
        {
            return new _TagQBit(parent ?? this.parent, tsb ?? this.tsb, isOpening);
        }

        public override string ToString()
        {
            // return  'QBit.${isOpening ? "opening" : "closing"}#$hashCode $tsb';
            var sb = StringBuilderPool.Obtain();
            sb.Append("QBit.");
            sb.Append(isOpening ? "opening" : "closing");
            sb.Append(" ");
            sb.Append(GetHashCode());
            sb.Append(" ");
            sb.Append(tsb);
            var retStr = sb.ToString();
            sb.ToPool();
            return retStr;
        }


        public static _TagQBit closing(AbsBuildTree parent)
        {
            return new _TagQBit(parent, parent.tsb, false);
        }


        public static _TagQBit opening(AbsBuildTree parent)
        {
            return new _TagQBit(parent, parent.tsb, true);
        }
    }
}