using System.Collections.Generic;

namespace WidgetFromHtml.Core
{
    internal class TagImg
    {
        public WidgetFactory wf;


        public TagImg(WidgetFactory wf)
        {
            this.wf = wf;
        }

        private static Dictionary<string, string> _imgDefaultStyle = new Dictionary<string, string>()
        {
            {Const.kCssMinWidth, "1px"},
            {Const.kCssMinHeight, "1px"},
        };
        Dictionary<string, string> getStyles()
        {
            return _imgDefaultStyle;
            // return new Dictionary<string, string>()
            // {
            //     // { Const.kCssHeight, "auto" },
            //     { Const.kCssMinWidth, "1px" },
            //     { Const.kCssMinHeight, "1px" },
            //     // { Const.kCssWidth, "auto" },
            // };
        }


        public BuildOp buildOp => new BuildOp
        (
            
            //TODO 屏蔽img的一些默认属性 似乎没啥用--!
            defaultStyles: element =>
            {
                // var attrs = element.Attributes;
                var styles = getStyles();
            
                // if (attrs.containsKey(Const.kAttributeImgHeight))
                // {
                //     // styles[Const.kCssHeight] = "${attrs[Const.kAttributeImgHeight]}px";
                //     styles[Const.kCssHeight] = $"{attrs[Const.kAttributeImgHeight].Value}px";
                // }
                //
                // if (attrs.containsKey(Const.kAttributeImgWidth))
                // {
                //     // styles[Const.kCssWidth] = "${attrs[Const.kAttributeImgWidth]}px";
                //     styles[Const.kCssWidth] = $"{attrs[Const.kAttributeImgWidth].Value}px";
                // }
                //
                return styles;
            },
            onTree: (meta, tree) =>
            {
                var data = _parse(meta);
                var built = wf.buildImage(meta, data);
                if (built == null)
                {
                    var imgText = data.alt ?? data.title ?? "";
                    if (imgText.isNotEmpty()) tree.addText(imgText);
                    return;
                }

                var placeholder =
                    new WidgetPlaceholder(data, child: built);

                tree.replaceWith
                (
                    meta.willBuildSubtree.Value
                        ? WidgetBit.block(tree, placeholder)
                        : WidgetBit.inline(tree, placeholder)
                );
            }
        );


        ImageMetadata _parse(AbsBuildMetadata meta)
        {
            var attrs = meta.element.Attributes;
            var u = "";

            var attr = attrs[Const.kAttributeImgSrc];
            if (attr != null)
            {
                u = attr.Value;
            }

            var url = wf.urlFull(u);

            // var url = wf.urlFull(attrs[Const.kAttributeImgSrc] ?? "");
            IEnumerable<ImageSource> sources = null;
            if (url != null)
            {
                sources = new List<ImageSource>(1)
                {
                    new ImageSource(
                        url,
                        height: Helper.tryParseDoubleFromMap(attrs, Const.kAttributeImgHeight),
                        width: Helper.tryParseDoubleFromMap(attrs, Const.kAttributeImgWidth)
                    ),
                };
            }

            else
            {
                sources = new List<ImageSource>(0);
            }

            return new ImageMetadata
            (
                alt: attrs[Const.kAttributeImgAlt]?.Value,
                sources: sources,
                title: attrs[Const.kAttributeImgTitle]?.Value
            );
        }
    }
}