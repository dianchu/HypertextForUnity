namespace WidgetFromHtml.Core
{
    internal delegate BuildBit TreeAppend(AbsBuildTree parent);

    internal delegate BuildBit TreePrepend(AbsBuildTree parent);

    internal static class core_ops
    {
        public static void wrapTree
        (
            AbsBuildTree tree,
            TreeAppend append = null,
            TreePrepend prepend = null
        )
        {
            if (tree.isEmpty)
            {
                if (prepend != null)
                {
                    var prependBit = prepend(tree);
                    tree.add(prependBit);
                }

                if (append != null)
                {
                    var appendBit = append(tree);
                    tree.add(appendBit);
                }

                return;
            }

            if (prepend != null)
            {
                var first = tree.first;
                prepend(first.parent).insertBefore(first);
            }

            if (append != null)
            {
                var last = tree.last;
                append(last.parent).insertAfter(last);
            }
        }
    }
}