using System;
using System.Collections.Generic;
using System.Linq;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace WidgetFromHtml.Core
{
    internal class TagRuby
    {
        public BuildOp op;
        public AbsBuildMetadata rubyMeta;
        public WidgetFactory wf;
        public BuildOp _rtOp;

        public TagRuby(WidgetFactory wf, AbsBuildMetadata rubyMeta)
        {
            this.rubyMeta = rubyMeta;
            this.wf = wf;

            op = new BuildOp(onChild: onChild, onTree: onTree);
            _rtOp = new BuildOp(
                onTree: (rtMeta, rtTree) =>
                {
                    if (rtTree.isEmpty) return;
                    var rtBit = new _RtBit
                    (
                        rtTree,
                        rtTree.tsb,
                        rtMeta,
                        rtTree.copyWith() as BuildTree
                    );
                    rtTree.replaceWith(rtBit);
                }
            );
        }

        void onChild(AbsBuildMetadata childMeta)
        {
            var e = childMeta.element;
            if (e.Parent != rubyMeta.element) return;

            switch (e.LocalName)
            {
                case Const.kTagRp:
                    childMeta.AddOneCssPropertyStyle(Const.kCssDisplay, Const.kCssDisplayNone);
                    // childMeta[kCssDisplay] = kCssDisplayNone;
                    break;
                case Const.kTagRt:
                    childMeta.AddOneCssPropertyStyle(Const.kCssFontSize, "0.5em");
                    childMeta.register(_rtOp);
                    // childMeta
                    // ..[kCssFontSize] =
                    // "0.5em
                    // ..register(_rtOp);
                    break;
            }
        }

        void onTree(AbsBuildMetadata _, AbsBuildTree tree)
        {
            var rubyBits = new List<BuildBit>();
            // foreach (var bit in tree.bits.toList(growable: false))
            foreach (var bit1 in tree.bits_toList())
            {
                if (rubyBits.isEmpty() && bit1 is WhitespaceBit)
                {
                    // the first bit is whitespace, just ignore it
                    continue;
                }

                // if (bit is !_RtBit || rubyBits.isEmpty)
                if (!(bit1 is _RtBit) || rubyBits.isEmpty())
                {
                    rubyBits.Add(bit1);
                    continue;
                }

                var rtBit = bit1 as _RtBit;
                var rtTree = rtBit.tree;
                var rubyTree = tree.sub();
                var placeholder = new WidgetPlaceholder(new List<AbsBuildTree> {rubyTree, rtTree});
                placeholder.wrapWith((context, __) =>
                {
                    var tsh = rubyTree.tsb.build(context);
                    rubyTree.build();
                    var ruby = wf.buildColumnWidget
                    (
                        rubyMeta, tsh, rubyTree.getBuiltWidgetsOrNull
                    );
                    rtTree.build();
                    var rt = wf.buildColumnWidget
                    (
                        rtBit.meta, tsh, rtTree.getBuiltWidgetsOrNull
                    );

                    return new HtmlRuby(ruby ?? Helper.widget0, rt ?? Helper.widget0);
                });

                var anchor = rubyBits.first();
                WidgetBit.inline(anchor.parent, placeholder).insertBefore(anchor);

                foreach (var rubyBit in rubyBits)
                {
                    rubyTree.add(rubyBit.copyWith(parent: rubyTree));
                    rubyBit.detach();
                }

                rubyBits.Clear();
                rtBit.detach();
            }
        }
    }

    class _RtBit : BuildBit
    {
        public AbsBuildMetadata meta;
        public AbsBuildTree tree;


        public _RtBit
        (
            AbsBuildTree parent,
            TextStyleBuilder tsb,
            AbsBuildMetadata meta,
            AbsBuildTree tree
        ) : base(parent, tsb)
        {
            this.meta = meta;
            this.tree = tree;
        }

        public override object buildBit(object input)
        {
            return this.tree;
        }

        private static Type _typAbsBuildTree = typeof(AbsBuildTree);
        public override Type buildBitInputTyp => Const.kTypNull;
        public override Type buildBitOutputTyp => _typAbsBuildTree;

        public override BuildBit copyWith(AbsBuildTree parent = null, TextStyleBuilder tsb = null)
        {
            return new _RtBit(parent ?? this.parent, tsb ?? this.tsb, meta, tree);
        }
    }
}