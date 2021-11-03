using System.Collections.Generic;

namespace WidgetFromHtml.Core
{
    internal class ImageMetadata
    {
        /// The image alternative text.
        internal readonly string alt;

        /// The image sources.
        internal readonly IEnumerable<ImageSource> sources;

        /// The image title.
        internal readonly string title;

        public ImageMetadata
        (
            string alt = null,
            IEnumerable<ImageSource> sources = null,
            string title = null
        )
        {
            this.alt = alt;
            this.sources = sources;
            this.title = title;
        }
    }

    internal class ImageSource
    {
        /// The image height.
        internal readonly float? height;

        /// The image URL.
        internal readonly string url;

        /// The image width.
        internal readonly float? width;

        public ImageSource(
            string url,
            float? height = null,
            float? width = null)
        {
            this.height = height;
            this.url = url;
            this.width = width;
        }
    }
}