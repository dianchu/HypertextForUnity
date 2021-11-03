# HypertextForUnity

基于Unity UIWidgets的超文本组件，可用于游戏UI中邮件，公告和聊天

# 简介

- html->widgets的映射抽象方法是从 [flutter_widget_from_html]() 0.6.1+4版本 移植而来
- 这是一个基于[UIWidgets2.0](https://github.com/Unity-Technologies/com.unity.uiwidgets)(flutter)和[AngleSharp](https://github.com/AngleSharp/AngleSharp)的富文本插件
- 使用html描述富文本内容,主要用于图文混排
- 渲染目标是一张RenderTexture

# 优劣势

​	**优势:**

1. 使用RT实现了类似光栅化缓存的效果. 当界面不动时, rt作为一张普通的纹理进行渲染.可以有效减少drawCall 和SetPassCall

2. uiwidget2.0也实现了局部的光栅化缓存

   **劣势:**

   1. rt对内存的占用
   2. AngleSharp+UIWidgets2.0(+引擎代码), 3个库的代码量还是非常大的.
   3. 项目结构复杂,当原生层代码出现问题需要调整时,uiwidgets的构建工具链可能会产生一些麻烦,官方提供的bee似乎还依赖了unity内部员工vpn.
   4. overDraw上升?

# 效果截图

​	![](doc.img/效果图.gif)

## TODO:

- demo--->package
  - 距离可用还有一段距离
  - 目前能成功运行helloworld.html
  - 在移植过程中, 对一些细节的理解不够清楚,导致一些样式有误,甚至报错.
- 扩展层
  - 自定义资源加载器
  - 事件回调(点击,抬起,按下...)
- 功能类型封装,接口定义
  - 告知外部布局信息(比如方便聊天ListView的布局和ChatMsgItem的布局)
  - 品质(比如控制gif,序列帧是否进行刷新,在一些低端机上,动图就不需要动了--!)
  - 设置html模板中的属性.
  - 提供生命周期函数,便于组件封装.
- css样式(仅id选择器)
  - 缓存css样式,避免重复解析
- html template
  - 使用html模板, 节省html重复解析的消耗
