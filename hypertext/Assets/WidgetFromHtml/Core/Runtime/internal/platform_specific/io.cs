using Unity.UIWidgets.painting;

namespace WidgetFromHtml.Core
{
    public delegate ImageProvider ImgProvider(string arg);

    // public delegate ImageProvider ImgProvider2(string arg1,string arg2);
    // public delegate ImageProvider ImgProvider3(string[] args);

    /// <summary>
    /// 图片io抽象层..
    /// </summary>
    public static partial class AbsLayer
    {
        public static ImgProvider fileImageProvider = defalutFileImageProvider;

        private static ImageProvider defalutFileImageProvider(string arg)
        {
            return new FileImage(arg);
        }
    }
}