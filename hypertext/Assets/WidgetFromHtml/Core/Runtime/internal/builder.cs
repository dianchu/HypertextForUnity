using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Text;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using UnityEngine;
using UnityEngine.UIElements;

namespace WidgetFromHtml.Core
{
    /// <summary>
    /// int _compareBuildOps(BuildOp a, BuildOp b) {
    ///  if (identical(a, b)) return 0;
    ///
    ///  final cmp = a.priority.compareTo(b.priority);
    ///  if (cmp == 0) {
    ///    // if two ops have the same priority, they should not be considered equal
    ///    // fallback to compare hash codes for stable sorting
    ///    // while still provide pseudo random order across different runs
    ///    return a.hashCode.compareTo(b.hashCode);
    ///  } else {
    ///    return cmp;
    ///  }
    ///}
    /// </summary>
    internal class BuildOpComparer : IComparer<BuildOp>
    {
        public int Compare(BuildOp a, BuildOp b)
        {
            if (object.ReferenceEquals(a, b)) return 0;

            var cmp = a.priority.CompareTo(b.priority);
            if (cmp == 0)
            {
                // if two ops have the same priority, they should not be considered equal
                // fallback to compare hash codes for stable sorting
                // while still provide pseudo random order across different runs
                return a.GetHashCode().CompareTo(b.GetHashCode());
            }
            else
            {
                return cmp;
            }
        }
    }

    internal class BuildMetadata : AbsBuildMetadata
    {
        private IEnumerable<BuildOp> _parentOps;
        public SortedSet<BuildOp> _buildOps;
        public bool _buildOpsIsLocked = false;
        private List<ICssProperty> _styles;
        public bool _stylesIsLocked = false;
        public bool? _willBuildSubtree;

        private IBrowsingContext _browsingContext;
        public override IBrowsingContext browsingContext => _browsingContext;


        //自定义添加..
        private CssParser _cssParser;

        public BuildMetadata
        (
            IElement element,
            TextStyleBuilder tsb,
            IBrowsingContext browsingContext,
            IEnumerable<BuildOp> parentOps = null
        ) : base(element, tsb)
        {
            if (parentOps == null)
            {
                _parentOps = new List<BuildOp>();
            }

            _browsingContext = browsingContext;
            _parentOps = parentOps;
            _cssParser = Helper.GetCssParser();
        }

        public override IEnumerable<BuildOp> buildOps
        {
            get
            {
                if (_buildOps == null)
                {
                    _buildOps = new SortedSet<BuildOp>();
                }

                return _buildOps;
            }
        }

        public override IEnumerable<BuildOp> parentOps
        {
            get { return _parentOps; }
        }

        public override List<ICssProperty> styles
        {
            get
            {
                D.assert(_stylesIsLocked);
                if (_styles == null)
                {
                    // _styles = _cssParser.ParseDeclaration("");
                    _styles = new List<ICssProperty>(16);
                }

                return _styles;
            }
        }

        internal override bool? willBuildSubtree => _willBuildSubtree;


        // /// <summary>
        // /// []=
        // /// </summary>
        // /// <param name="cssPropertyKey"></param>
        // /// <param name="cssPropertyValue"></param>
        // public void AddCssProperty(string cssPropertyKey, string cssPropertyValue)
        // {
        //     var strCssText = Helper.GetCssText(cssPropertyKey, cssPropertyValue);
        //     styles.Update(strCssText);
        // }

        ///  /// Adds an inline style.  对应dart语法中的 operator []=(String key, String value);
        public override void AddOneCssPropertyStyle(string keyName, string value)
        {
            // D.assert(!_stylesIsLocked, () => "Metadata can no longer be changed.");
            styles.SetProperty(_browsingContext, keyName, value);
        }

        public override void register(BuildOp op)
        {
            D.assert(!_buildOpsIsLocked, () => "Metadata can no longer be changed.");
            if (_buildOps == null)
            {
                _buildOps = new SortedSet<BuildOp>(new BuildOpComparer());
            }

            _buildOps.Add(op);
        }

        /// <summary>
        /// Gets a styling declaration by `property`.
        /// </summary>
        /// <param name="propertyName"></param>
        public override ICssProperty this[string propertyName]
        {
            get
            {
                // foreach (var style in styles)
                // {
                //     if (style.Name == propertyName)
                //     {
                //         return style;
                //     }
                // }

                //dart中是逆序遍历
                // foreach (var property in styles.Reverse())
                // {
                // }

                for (int i = styles.Count - 1; i >= 0; i--)
                {
                    var style = styles[i];
                    if (style.Name.Isi(propertyName))
                    {
                        return style;
                    }
                }

                return null;

                // var count = styles.Count; //逆序遍历 样式就近原则  styles加入是按照远的先加入.
                // for (int i = count - 1; i >= 0; i--)
                // {
                //     var style = styles.GetProperty(propertyName);
                //     if (style.Name == propertyName)
                //     {
                //         return style;
                //     }
                // }
                // return null;
            }
        }
    }

    internal class BuildTree : AbsBuildTree
    {
        public CustomStylesBuilder customStylesBuilder;
        public CustomWidgetBuilder customWidgetBuilder;
        public AbsBuildMetadata parentMeta;
        public IEnumerable<BuildOp> parentOps;
        public WidgetFactory wf;

        private CssParser _cssParser;

        List<WidgetPlaceholder> _built = new List<WidgetPlaceholder>();

        public BuildTree
        (
            AbsBuildMetadata parentMeta,
            TextStyleBuilder tsb,
            WidgetFactory wf,
            AbsBuildTree parent = null,
            CustomStylesBuilder customStylesBuilder = null,
            CustomWidgetBuilder customWidgetBuilder = null,
            IEnumerable<BuildOp> parentOps = null
        ) : base(parent, tsb)
        {
            this.customStylesBuilder = customStylesBuilder;
            this.customWidgetBuilder = customWidgetBuilder;
            this.parentMeta = parentMeta;
            this.parentOps = parentOps ?? new List<BuildOp>();
            this.wf = wf;
            _cssParser = Helper.GetCssParser();
        }

        public override T add<T>(T bit)
        {
            D.assert(_built.isEmpty(), () => "Built tree shouldn't be altered.");
            return base.add(bit);
        }

        public void addBitsFromNodes(INodeList domNodes)
        {
            foreach (var domNode in domNodes)
            {
                _addBitsFromNode(domNode);
            }

            foreach (var op in parentMeta.buildOps)
            {
                op.onTree?.Invoke(parentMeta, this);
            }
        }


        public override IEnumerable<WidgetPlaceholder> build()
        {
            if (_built.isNotEmpty())
            {
                return _built;
            }

            var widgets = wf.flatten(parentMeta, this);
            foreach (var op in parentMeta.buildOps)
            {
                widgets = op.onWidgets
                              ?.Invoke(parentMeta, widgets)
                              ?.map(WidgetPlaceholder.lazy)
                              .ToList() ??
                          widgets;
            }

            // _built.addAll(widgets);
            _built.AddRange(widgets);
            return _built;
        }

        public override List<Widget> getBuiltWidgetsOrNull
        {
            get
            {
                if (_built.isEmpty()) return null;
                var retList = new List<Widget>(_built.Count);
                for (int i = 0; i < _built.Count; i++)
                {
                    retList.Add(_built[i]);
                }

                return retList;
            }
        }

        /* 这边写了两个sub, 用于重载
         dart的语法规定, 重载的方法允许和父类的参数列表不一致!
  @override
  BuildTree sub({
    core_data.BuildTree? parent,
    BuildMetadata? parentMeta,
    Iterable<BuildOp> parentOps = const [],
    TextStyleBuilder? tsb,
  }) ...
  */
        public override AbsBuildTree sub
        (
            AbsBuildTree parent = null,
            TextStyleBuilder tsb = null
        )
        {
            return new BuildTree(
                customStylesBuilder: customStylesBuilder,
                customWidgetBuilder: customWidgetBuilder,
                parent: parent ?? this,
                parentMeta: parentMeta ?? this.parentMeta,
                parentOps: new List<BuildOp>(),
                tsb: tsb ?? this.tsb.sub(),
                wf: wf
            );
        }

        public new BuildTree sub
        (
            AbsBuildTree parent = null,
            TextStyleBuilder tsb = null,
            BuildMetadata parentMeta = null,
            IEnumerable<BuildOp> parentOps = null
        )
        {
            parentOps = parentOps ?? new List<BuildOp>();
            return new BuildTree(
                customStylesBuilder: customStylesBuilder,
                customWidgetBuilder: customWidgetBuilder,
                parent: parent ?? this,
                parentMeta: parentMeta ?? this.parentMeta,
                parentOps: parentOps ?? new List<BuildOp>(),
                tsb: tsb ?? this.tsb.sub(),
                wf: wf
            );
        }


        void _addBitsFromNode(INode domNode)
        {
            // if (domNode.nodeType == dom.Node.TEXT_NODE)
            if (domNode.NodeType == NodeType.Text)
            {
                // var text = domNode as IText ;
                // return _addText(text.data);
                _addText(domNode.TextContent);
                return;
            }

            // if (domNode.nodeType != dom.Node.ELEMENT_NODE) return;
            if (domNode.NodeType != NodeType.Element) return;

            var element = domNode as IElement;
            var customWidget = customWidgetBuilder?.Invoke(element);
            if (customWidget != null)
            {
                add(WidgetBit.block(this, customWidget));
                // skip further processing if a custom widget found
                return;
            }

            var meta = new BuildMetadata
            (
                element,
                parentMeta.tsb.sub(),
                parentMeta.browsingContext,
                parentOps
            );
            _collectMetadata(meta);

            var subTree = sub(
                parentMeta: meta,
                parentOps: _prepareParentOps(parentOps, meta),
                tsb: meta.tsb
            );
            add(subTree);

            // subTree.addBitsFromNodes(element.nodes);
            subTree.addBitsFromNodes(element.ChildNodes);

            if (meta.willBuildSubtree == true)
            {
                foreach (var widget in subTree.build())
                {
                    add(WidgetBit.block(this, widget));
                }

                subTree.detach();
            }
        }

        void _addText(string data)
        {
            //Match (首次命中)  Searches an input string for a substring that matches a regular expression pattern and returns the first occurrence as a single Match object.
            var leading = Const._regExpSpaceLeading.Match(data);
            var trailing = Const._regExpSpaceTrailing.Match(data);
            // final start = leading == null ? 0 : leading.end;
            // final end = trailing == null ? data.length : trailing.start;

            // var start = leading == null ? 0 : leading.end();
            // var end = trailing == null ? data.Length : trailing.start();

            var start = !leading.Success ? 0 : leading.end();
            var end = !trailing.Success ? data.Length : trailing.start();

            if (end <= start)
            {
                // the string contains all spaces
                addWhitespace(data);

                return;
            }

            // if (start > 0) addWhitespace(leading.group(0));
            if (start > 0) addWhitespace(leading.group_0());
            var contents = data.substring(start, end);
            // var spaces = Const._regExpSpaces.allMatches(contents);
            // var spaces = Const._regExpSpaces.Matches(contents);
            var spaces = Const._regExpSpaces.Matches(contents);
            var offset = 0;
            var tempList = new List<Match>();
            tempList.AddRenderMatchs(spaces);
            tempList.Add(null); //加一个null进去.. 作为标兵..
            // foreach (var space in[  ...spaces, null])
            foreach (var space in tempList)
            {
                if (space == null)
                {
                    // reaches end of string
                    var text = contents.substring(offset);
                    if (text.isNotEmpty())
                    {
                        addText(text);
                    }

                    break;
                }
                else
                {
                    // var spaceData = space.group(0);
                    var spaceData = space.group_0();
                    if (spaceData == " ") //空格!
                    {
                        // micro optimization: ignore single space (ASCII 32)
                        continue;
                    }

                    var text = contents.substring(offset, space.start());
                    addText(text);

                    addWhitespace(spaceData);
                    offset = space.end();
                }
            }

            if (end < data.Length) addWhitespace(trailing.group_0());
        }


        void _collectMetadata(BuildMetadata meta)
        {
            meta._stylesIsLocked = true;
            wf.parse(meta);

            foreach (var op in meta.parentOps)
            {
                op.onChild?.Invoke(meta);
            }


            // stylings, step 1: get default styles from tag-based build ops
            // foreach (var op in meta.buildOps)
            // {
            //     var map = op.defaultStyles?.Invoke(meta.element);
            //     if (map == null) continue;
            //
            //     // var str = map.entries.map((e) => '${e.key}: ${e.value}').join(';');
            //     // var styleSheet = css.parse('*{$str}');
            //     var strStyleSheet = map.Map_entries_map_join_to_style_sheet_(';');
            //     meta.styles.Update(strStyleSheet);
            // }

            foreach (var op in meta.buildOps)
            {
                var map = op.defaultStyles?.Invoke(meta.element);
                if (map == null) continue;

                // var str = map.entries.map((e) => '${e.key}: ${e.value}').join(';');
                // var styleSheet = css.parse('*{$str}');
                // var strStyleSheet = map.Map_entries_map_join_to_style_sheet_(';');
                foreach (var kv in map)
                {
                    meta.styles.SetProperty(parentMeta.browsingContext, kv.Key, kv.Value);
                }

                // meta.styles.Update(strStyleSheet);
            }

            _customStylesBuilder(meta);

            // stylings, step 2: get styles from `style` attribute
            // foreach (var declaration in meta.element.styles)
            // {
            //     meta._styles.add(declaration);
            // }

            //收集StyleSheet和InlineStyle 2021年8月17日14:16:13


            //收集收集所有属性! 
            meta.styles.CollectAllStyleFromSheetAndInline(meta.element);

            foreach (var style in meta.styles)
            {
                wf.parseStyle(meta, style);
            }

            wf.parseStyleDisplay(meta, meta[Const.kCssDisplay]?.Value);

            meta._willBuildSubtree = meta[Const.kCssDisplay]?.Value == Const.kCssDisplayBlock
                                     || meta._buildOps?.Where(_opRequiresBuildingSubtree).isNotEmpty() == true;
            meta._buildOpsIsLocked = true;
            meta._stylesIsLocked = true; //styles锁住
        }


        void _customStylesBuilder(AbsBuildMetadata meta)
        {
            var map = customStylesBuilder?.Invoke(meta.element);
            if (map == null) return;

            foreach (var pair in map)
            {
                meta.AddOneCssPropertyStyle(pair.Key, pair.Value);
                // meta[pair.Key] = pair.Value;
            }

            // foreach (var pair in map.entries)
            // {
            //     meta[pair.key] = pair.value;
            // }
        }


        bool _opRequiresBuildingSubtree(BuildOp op) =>
            op.onWidgets != null && !op.onWidgetsIsOptional;


        IEnumerable<BuildOp> _prepareParentOps(IEnumerable<BuildOp> ops, BuildMetadata meta)
        {
            // try to reuse existing list if possible
            var withOnChild = meta.buildOps.Where((op) => op.onChild != null).ToList();

            var isEmpty = withOnChild.isEmpty();
            if (isEmpty)
            {
                return ops;
            }
            else
            {
                var retList = new List<BuildOp>();
                retList.AddRange(ops);
                retList.AddRange(withOnChild);
                return retList;
            }

            // return withOnChild.isEmpty()
            //     ? ops
            //     : List.unmodifiable([...ops, ...withOnChild]);
        }
    }
}