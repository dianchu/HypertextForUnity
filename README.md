# HypertextForUnity

基于Unity UIWidgets的超文本组件，可用于游戏UI中邮件，公告和聊天

# 简介

- html->widgets的映射抽象方法是从 [flutter_widget_from_html]() 0.6.1+4版本 移植而来
- 这是一个基于[UIWidgets2.0](https://github.com/Unity-Technologies/com.unity.uiwidgets)(flutter)和[AngleSharp](https://github.com/AngleSharp/AngleSharp)的富文本插件
- 使用html描述富文本内容,主要用于图文混排

  

# 优劣势

**优势:**

1. 用Html+样式表让用户非常方便完成图文混排的样式
2. 光栅化缓存. 当界面不动时, 只有一张纹理需要渲染. 如果内容中有动图，可以把不变的部分做光栅化缓存，获得“动静分离”的优化效果
3. 基于采用flutter的抽象和Skia引擎

**劣势:**

1. 需要了解较多的技术细节，AngleSharp+UIWidgets2.0+Skia引擎, 3个库的代码量还是非常大的.
2. 不支持Unity ShaderLab， 这部分官方想通过在unity完成绘制，然后传递一张纹理给skia来解决
3. 增加的包体代码尺寸，但是AAB的技术使得代码尺寸不再是一个问题

# 效果截图

​	![](doc.img/效果图.gif)

## TODO:

- 利用此图文混排的功能完善聊天组件，聊天组件的设计有待完善

  
