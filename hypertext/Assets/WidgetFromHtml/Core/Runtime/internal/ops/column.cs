using System.Collections.Generic;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace WidgetFromHtml.Core
{
    internal class ColumnPlaceholder : WidgetPlaceholder
    {
        public readonly AbsBuildMetadata meta;
        public readonly bool trimMarginVertical;
        public readonly WidgetFactory wf;

        private IEnumerable<Widget> _children;

        public ColumnPlaceholder
        (
            IEnumerable<Widget> children,
            AbsBuildMetadata meta,
            bool trimMarginVertical,
            WidgetFactory wf
            
        ) : base(meta)
        {
            this.meta = meta;
            this.trimMarginVertical = trimMarginVertical;
            this.wf = wf;
            _children = children;
        }

        public override Widget build(BuildContext context)
        {
            var tsh = meta.tsb.build(context);
            return wf.buildColumnWidget(meta, tsh, getChildren(context))?? Helper.widget0;
        }

        public List<Widget> getChildren(BuildContext context)
        {
            //TODO 池化这个list
            var contents = new List<Widget>();

            HeightPlaceholder marginBottom = null;
            HeightPlaceholder marginTop = null;
            Widget prev = null;
            var state = 0;

            var ieChildren = _getIterable(context);
            while (ieChildren.MoveNext())
            {
                var child = ieChildren.Current;
                if (state == 0)
                {
                    if (child is HeightPlaceholder childHeightPlaceholder)
                    {
                        if (!trimMarginVertical)
                        {
                            if (marginTop != null)
                            {
                                marginTop.mergeWith(childHeightPlaceholder);
                            }
                            else
                            {
                                marginTop = childHeightPlaceholder;
                            }
                        }
                    }
                    else
                    {
                        state++;
                    }
                }

                if (state == 1)
                {
                    if (child is HeightPlaceholder childHeightPlaceholder
                        && prev is HeightPlaceholder prevHeightPlaceholder)
                    {
                        prevHeightPlaceholder.mergeWith(childHeightPlaceholder);
                        continue;
                    }

                    contents.Add(child);
                    prev = child;
                }
            }


            if (contents.isNotEmpty())
            {
                var lastWidget = contents.last();
                if (lastWidget is HeightPlaceholder lastWidgetHeightPlaceholder)
                {
                    contents.removeLast();
                    if (!trimMarginVertical)
                    {
                        marginBottom = lastWidgetHeightPlaceholder;
                    }
                }
            }

            var tsh = meta.tsb.build(context);
            var column = wf.buildColumnWidget(meta, tsh, contents);

            var retList = new List<Widget>(3);
            if (marginTop != null) retList.Add(marginTop);
            if (column != null) retList.Add(callBuilders(context, column));
            if (marginBottom != null) retList.Add(marginBottom);
            return retList;
        }


        IEnumerator<Widget> _getIterable(BuildContext context)
        {
            foreach (var child in _children)
            {
                if (child == Helper.widget0) continue;

                if (child is ColumnPlaceholder columnPlaceholder)
                {
                    foreach (var grandChild in columnPlaceholder.getChildren(context))
                    {
                        yield return grandChild;
                    }

                    continue;
                }

                yield return child;
            }
        }
    }
}