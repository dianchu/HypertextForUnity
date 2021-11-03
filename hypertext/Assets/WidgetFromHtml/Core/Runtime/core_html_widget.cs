using System;
using System.Collections.Generic;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace WidgetFromHtml.Core
{
    internal class HtmlWidget : StatefulWidget
    {
        public Uri baseUrl;


        /// <summary>
        /// Controls whether the widget tree is built asynchronously.
        ///
        /// If not set, async build will be enabled automatically if the
        /// [html] has at least [kShouldBuildAsync] characters.
        /// </summary>
        //TODO 暂时不支持异步构建, 缺少了一些类型
        // public bool? buildAsync;

        /// The callback to handle async build snapshot.
        ///
        /// By default, a [CircularProgressIndicator] will be shown until
        /// the widget tree is ready.
        /// This default builder doesn't do any error handling
        /// (it will just ignore any errors).
        //TODO 暂时不支持异步构建, 缺少了一些类型
        // public AsyncWidgetBuilder<Widget>? buildAsyncBuilder;


        /// <summary>
        /// The callback to specify custom stylings.
        /// </summary>
        /// <returns></returns>
        public CustomStylesBuilder customStylesBuilder;


        /// <summary>
        /// The callback to render a custom widget.
        /// </summary>
        public CustomWidgetBuilder customWidgetBuilder;

        /// <summary>
        /// Controls whether the built widget tree is cached between rebuilds.
        ///
        /// Default: `true` if [buildAsync] is off, `false` otherwise.
        /// </summary>
        /// <returns></returns>
        public bool? enableCaching;

        /// <summary>
        /// The input string.
        /// </summary>
        public string html;

        /// <summary>
        ///   /// The text color for link elements.
        ///
        /// Default: [ThemeData.accentColor].
        /// </summary>
        public Color hyperlinkColor;

        /// <summary>
        /// The custom [WidgetFactory] builder.
        /// </summary>
        /// <returns></returns>
        public Func<WidgetFactory> factoryBuilder;

        /// <summary>
        /// The callback when user taps an image.
        /// </summary>
        public Action<ImageMetadata> onTapImage;


        /// <summary>
        /// /// The callback when user taps a link.
        ///
        /// Returns `false` to fallback to the built-in handler.
        /// Returns `true` (or anything that is not `false`) to skip default behaviors.
        /// Returning a `Future` is supported.
        /// </summary>
        /// <returns></returns>
        public Func<string, object> onTapUrl;

        RebuildTriggers _rebuildTriggers;

        public RebuildTriggers rebuildTriggers
        {
            get
            {
                var list = new List<object>()
                {
                    html,
                    baseUrl,
                    // buildAsync,
                    enableCaching,
                    hyperlinkColor,
                };
                if (_rebuildTriggers != null)
                {
                    list.Add(_rebuildTriggers);
                }

                return new RebuildTriggers(list);
            }
        }

        /// The default styling for text elements.
        public TextStyle textStyle;

        /// <summary>
        ///  browsingContext如果传入null, 那么内部将会构造一个context,
        /// </summary>
        private IBrowsingContext _browsingContext = null;

        /// <summary>
        /// 如果外部的业务逻辑有大量的htmlWidget,比如要做一个聊天组件, 那么务必传入一个browsingContext,并且外部自己处理browsingContext的生命周期
        /// </summary>
        /// <param name="html"></param>
        /// <param name="browsingContext"></param>
        /// <param name="baseUrl"></param>
        /// <param name="customStylesBuilder"></param>
        /// <param name="customWidgetBuilder"></param>
        /// <param name="enableCaching"></param>
        /// <param name="hyperlinkColor"></param>
        /// <param name="factoryBuilder"></param>
        /// <param name="onTapImage"></param>
        /// <param name="onTapUrl"></param>
        /// <param name="rebuildTriggers"></param>
        /// <param name="textStyle"></param>
        /// <param name="key"></param>
        public HtmlWidget
        (
            string html,
            IBrowsingContext browsingContext = null,
            Uri baseUrl = null,
            CustomStylesBuilder customStylesBuilder = null,
            CustomWidgetBuilder customWidgetBuilder = null,
            bool? enableCaching = true,
            Color hyperlinkColor = null,
            Func<WidgetFactory> factoryBuilder = null,
            Action<ImageMetadata> onTapImage = null,
            Func<string, object> onTapUrl = null,
            RebuildTriggers rebuildTriggers = null,
            TextStyle textStyle = null,
            Key key = null) : base(key)
        {
            this._browsingContext = browsingContext;
            this.baseUrl = baseUrl;
            this.customStylesBuilder = customStylesBuilder;
            this.customWidgetBuilder = customWidgetBuilder;
            this.enableCaching = enableCaching;
            this.html = html;
            this.hyperlinkColor = hyperlinkColor;
            this.factoryBuilder = factoryBuilder;
            this.onTapImage = onTapImage;
            this.onTapUrl = onTapUrl;
            _rebuildTriggers = rebuildTriggers;
            this.textStyle = textStyle ?? new TextStyle();
        }

        public override State createState()
        {
            return new _HtmlWidgetState
            (
                html,
                _browsingContext
            );
        }
    }

    class _HtmlWidgetState : State<HtmlWidget>
    {
        internal Widget _cache;

        // internal Future<Widget> _future;//TODO 暂时不支持异步>>
        internal BuildMetadata _rootMeta;
        internal _RootTsb _rootTsb;
        internal WidgetFactory _wf;


        private string _strHtml;
        private IHtmlParser _parser;


        // public bool buildAsync => widget.buildAsync ?? widget.html.length() > Const.kShouldBuildAsync;
        // public bool buildAsync => widget.html.length() > Const.kShouldBuildAsync;
        public bool buildAsync => false; //直接写死, dart中使用10000字符作为异步的阈值

        public bool enableCaching =>
            widget.enableCaching != null && widget.enableCaching.Value; // widget.enableCaching ?? !buildAsync;

        private IBrowsingContext _browsingContext;
        private bool _browsingContextFromExternal;

        /// <summary>
        /// browsingContext如果传入null, 那么内部将会构造一个context,
        /// 
        /// </summary>
        /// <param name="strHtml"></param>
        /// <param name="browsingContext"></param>
        public _HtmlWidgetState
        (
            string strHtml,
            IBrowsingContext browsingContext = null
        )
        {
            _strHtml = strHtml;
            _browsingContextFromExternal = browsingContext != null;
            _browsingContext = browsingContext ?? BrowsingContext.New(AngleSharp.Configuration.Default.WithCss());
            _parser = _browsingContext.GetService<IHtmlParser>();
        }

        /// <summary>
        /// 等价于 dom.Element.tag('root'), 创建一个临时的给metadata
        /// </summary>
        /// <returns></returns>
        HtmlElement GetHtmlTagElement()
        {
            // var emptyHtmlDocument = _parser.ParseDocument("");
            var emptyHtmlDocument = _parser.ParseDocument("");
            return new HtmlElement(emptyHtmlDocument as Document, "root");
        }


        public override void initState()
        {
            base.initState();
            _rootTsb = new _RootTsb(this);
            // _rootMeta = new BuildMetadata(dom.Element.tag('root'), _rootTsb);
            // var element = _parser.ParseDocument(_strHtml).Body;

            _rootMeta = new BuildMetadata(GetHtmlTagElement(), _rootTsb,_browsingContext);
            _wf = widget.factoryBuilder?.Invoke() ?? new WidgetFactory();
            _wf.onRoot(_rootTsb);
            if (buildAsync) //false 暂时不支持异步
            {
                HLog.LogError(" 暂时不支持异步");
                // _future = _buildAsync();
            }
        }

        public override void dispose()
        {
            //自己构造出来的
            if (!_browsingContextFromExternal)
            {
                _browsingContext.Dispose();
            }

            _wf.Dispose();
            base.dispose();
        }

        public override void didChangeDependencies()
        {
            base.didChangeDependencies();
            _rootTsb.reset();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget1)
        {
            base.didUpdateWidget(oldWidget1);

            HtmlWidget oldWidget = oldWidget1 as HtmlWidget;
            if (oldWidget == null)
            {
                HLog.LogError("_HtmlWidgetState oldWidget1不是HtmlWidget 直接return");
                return;
            }

            bool needsRebuild = widget.rebuildTriggers != oldWidget.rebuildTriggers;

            if (widget.textStyle != oldWidget.textStyle)
            {
                _rootTsb.reset();
                needsRebuild = true;
            }

            if (needsRebuild)
            {
                _cache = null;
                // _future = buildAsync ? _buildAsync() : null;
                // _future = null; //TODO 暂时不支持异步解析!
            }
        }

        public override Widget build(BuildContext context)
        {
            //TODO  暂时不支持异步解析! 就不使用future来做了..
            // if (_future != null) {
            //     return new FutureBuilder<Widget>(
            //         builder: widget.buildAsyncBuilder ?? _buildAsyncBuilder,
            //         future: _future!.then(_tshWidget),
            //     );
            // }

            if (!enableCaching || _cache == null)
            {
                _cache = _buildSync();
            }


            return _tshWidget(_cache);
        }


        //TODO 暂时不支持异步解析!
        // Future<Widget> _buildAsync() async {
        //     final domNodes = await compute(_parseHtml, widget.html);
        //
        //     Timeline.startSync('Build $widget (async)');
        //     final built = _buildBody(this, domNodes);
        //     Timeline.finishSync();
        //
        //     return built;
        // }


        Widget _buildSync()
        {
            //似乎是给chrome 内核的tracing用的?? 不需要搬运.
            // Timeline.startSync($"Build {widget} (sync)"); 

            var domNodes = core_html_widget._parseHtml(widget.html, _parser);
            var built = core_html_widget._buildBody(this, domNodes);

            // Timeline.finishSync();

            return built;
        }

        Widget _tshWidget(Widget child)
        {
            return new TshWidget(tsh: _rootTsb._output, child: child);
        }
    }

    class _RootTsb : TextStyleBuilder
    {
        public TextStyleHtml _output;

        public _RootTsb(_HtmlWidgetState state)
        {
            enqueue(builder, state);
        }


        public override TextStyleHtml build(BuildContext context)
        {
            context.dependOnInheritedWidgetOfExactType<TshWidget>();
            return base.build(context);
        }

        TextStyleHtml builder(TextStyleHtml _, _HtmlWidgetState state)
        {
            if (_output != null) return _output;
            return _output = TextStyleHtml.root
            (
                state._wf.getDependencies(state.context),
                state.widget.textStyle
            );
        }

        public void reset() => _output = null;
    }


    internal static class core_html_widget
    {
        //TODO 因为不需要异步, 所以就跳过
        // public static Widget _buildAsyncBuilder(
        //     BuildContext context, AsyncSnapshot<Widget> snapshot)
        // {
        //     return snapshot.data ??
        //            new Center(
        //                child: new Padding(
        //                    padding: EdgeInsets.all(8),
        //                    child: Theme.of(context).platform == TargetPlatform.iOS
        //                        ? CupertinoActivityIndicator()
        //                        : CircularProgressIndicator(),
        //                )
        //            );
        // }


        public static Widget _buildBody
        (
            _HtmlWidgetState state,
            INodeList nodeList
        )
        {
            var rootMeta = state._rootMeta;
            var wf = state._wf;
            wf.reset(state);

            var tree = new BuildTree(
                rootMeta,
                rootMeta.tsb,
                wf,
                customStylesBuilder: state.widget.customStylesBuilder,
                customWidgetBuilder: state.widget.customWidgetBuilder
            );
            tree.addBitsFromNodes(nodeList);
            var children = tree.build();
            var retWidget = wf.buildBody(rootMeta, children);
            if (retWidget == null)
            {
                HLog.LogError($">> ret widget = null{retWidget}");
            }

            return retWidget ?? Helper.widget0;
        }

        public static INodeList _parseHtml(string html, IHtmlParser parser)
        {
            return parser.ParseDocument(html).Body.ChildNodes; //返回Body的即可!
            // return parser.HtmlParser(
            //     html,
            //     generateSpans: false,
            //     parseMeta: false
            // ).parseFragment().nodes; //TODO 这边为什么使用parseFragment??
        }

        // public static HtmlParser GetHtmlParser()
        // {
        //     return new HtmlParser(options: new HtmlParserOptions()
        //     {
        //         IsScripting = false,
        //         IsStrictMode = false,
        //         IsNotSupportingFrames = false,
        //         IsEmbedded = true,
        //     });
        // }
    }

    // /// A widget that builds widget tree from INodeList
    // public class HtmlWidget : StatefulWidget
    // {
    //     /// The default styling for text elements.
    //     private readonly TextStyle textStyle;
    //
    //     private IHtmlElement element;
    //
    //     /// Controls whether the built widget tree is cached between rebuilds.
    //     /// 
    //     /// Default: `true` if [buildAsync] is off, `false` otherwise.
    //     private bool enableCaching;
    //
    //     private INodeList nodeList;
    //
    //     /// Creates a widget that builds Flutter widget tree from html.
    //     /// 
    //     /// The [html] argument must not be null.
    //     public HtmlWidget(HtmlElement element)
    //     {
    //         this.element = element;
    //     }
    //
    //     public override State createState()
    //     {
    //         return new _HtmlWidgetState();
    //     }
    //
    //
    //     private class _HtmlWidgetState : State<HtmlWidget>
    //     {
    //         private Widget _cache;
    //         private AbsBuildMetadata _rootMeta;
    //         private _RootTsb _rootTsb;
    //         private WidgetFactory _wf;
    //
    //
    //         public override Widget build(BuildContext context)
    //         {
    //             return new Container();
    //         }
    //     }
    //
    //     class _RootTsb : TextStyleBuilder
    //     {
    //         private TextStyleHtml _output;
    //
    //         public _RootTsb(_HtmlWidgetState state)
    //         {
    //             // enqueue(builder, state);
    //         }
    //
    //         // private TextStyleHtml builder(TextStyleHtml _, _HtmlWidgetState state)
    //         // {
    //         //     if (_output != null) return _output;
    //         //     _output = TextStyleHtml.root(state._wf.getDependencies(state.context),
    //         //         state.widget.textStyle);
    //         // }
    //     }
    // }
}