using System;
using System.Collections.Generic;
using AngleSharp;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using Unity.UIWidgets.widgets;

namespace WidgetFromHtml.Core
{
    internal abstract partial class AbsBuildMetadata
    {
        public readonly IElement element;

        public TextStyleBuilder tsb;

        public AbsBuildMetadata(IElement element, TextStyleBuilder tsb)
        {
            this.element = element;
            this.tsb = tsb;
        }

        public abstract IBrowsingContext browsingContext { get; }
        public abstract ICssProperty this[string propertyName] { get; }
        public abstract void AddOneCssPropertyStyle(string keyName, string value);
        public abstract IEnumerable<BuildOp> buildOps { get; }

        public abstract IEnumerable<BuildOp> parentOps { get; }

        public abstract List<ICssProperty> styles { get; }

        /// <summary>
        /// 如果返回true， subtree 将会被构建
        /// 
        /// May returns `null` if metadata is still being collected.
        /// There are a few things that may trigger subtree building:
        /// - Some [BuildOp] has a mandatory `onWidgets` callback
        /// - Inline style `display: block`
        ///
        /// See [BuildOp.onWidgetsIsOptional].
        /// </summary>
        internal abstract bool? willBuildSubtree { get; }

        public abstract void register(BuildOp op);

        public override string ToString()
        {
            return $"BuildMetadata(${element.OuterHtml})";
        }
    }

    internal delegate Dictionary<string, string> DefaultStylesHandle(IElement element);

    internal delegate void OnChildHandle(AbsBuildMetadata childMeta);

    internal delegate void OnTreeHandle(AbsBuildMetadata meta, AbsBuildTree tree);

    internal delegate IEnumerable<Widget> OnWidgetsHandle(AbsBuildMetadata meta,
        IEnumerable<WidgetPlaceholder> widgets);

    internal class BuildOp : IComparable<BuildOp>
    {
        /// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Number/MAX_SAFE_INTEGER
        public readonly int priority;

        public readonly DefaultStylesHandle defaultStyles;

        public readonly OnChildHandle onChild;

        public readonly OnTreeHandle onTree;

        public readonly OnWidgetsHandle onWidgets;

        public readonly bool onWidgetsIsOptional;

        public BuildOp
        (
            DefaultStylesHandle defaultStyles = null,
            OnChildHandle onChild = null,
            OnTreeHandle onTree = null,
            OnWidgetsHandle onWidgets = null,
            bool onWidgetsIsOptional = false,
            int priority = 10
        )
        {
            this.defaultStyles = defaultStyles;
            this.onChild = onChild;
            this.onWidgets = onWidgets;
            this.onTree = onTree;
            this.onWidgetsIsOptional = onWidgetsIsOptional;
            this.priority = priority;
        }


        public int CompareTo(BuildOp other)
        {
            if (other == null)
            {
                return -1;
            }


            if (object.ReferenceEquals(this, other)) return 0;

            var cmp = priority.CompareTo(other.priority);
            if (cmp == 0)
            {
                // if two ops have the same priority, they should not be considered equal
                // fallback to compare hash codes for stable sorting
                // while still provide pseudo random order across different runs
                return this.GetHashCode().CompareTo(other.GetHashCode());
            }
            else
            {
                return cmp;
            }

            return priority.CompareTo(other.priority);
        }
    }
}