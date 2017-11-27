#TextEmoji
写在前面，最近在写聊天系统，这里面就有一个很重要的环节就是表情与文字混排需要解决。前段时间有客户说微信登录，有的人微信名字含有emoji，这样直接使用会显示乱码。就一起解决了。

>**功能**


![](http://upload-images.jianshu.io/upload_images/3438059-602c771cf9bf1881.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

◆打包图集
◆图文混排
◆使用输入法直接输入emoji转化显示


>**怎么用？**

**打包图集**

点击Tool->Bake Emoji Atlas然后设置好各个选项点击生成即可。

**显示emoji**

    EmojiTextManager.Instance.SetChatTextEmoji(Emoji, "{emoji:54}Emoji表情{emoji:4}", "54|4|", emojiAtlas);
 
使用这个是已知emoji的id，用于聊天表情框选择等。

    EmojiTextManager.Instance.SetUITextThatHasEmoji(inputEmoji, "自行车: \U0001F6B4, 国旗: \U0001F1FA\U0001F1F8", emojiAtlas);

使用于用户直接用输入法输入emoji或者获得网络消息中含有emoji的Unicode的时候。

    EmojiTextManager.Instance.SetUITextThatHasIconByAtlas(qq, "{emoji:44}QQ{emoji:55}表情{emoji:4}", iconAtlas);

与第一个同样，区别在于这是使用自定义的图集。

    EmojiTextManager.Instance.ConvertInputFieldText(str);

用于用户使用输入法输入emoji，显示输入框中的表现。

**填充数据**

    EmojiTextManager.Instance.ParseEmojiInfo(resStr);
    EmojiTextManager.Instance.ParseNormalIcon(resStr);


这个需要在使用显示图片之前调用。

>**具体实现**

**打包图集**

![图集工具](http://upload-images.jianshu.io/upload_images/3438059-724150a95a0251dc.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

设置好上面的参数。生成会把图片文件夹中的图片遍历打到一个Texture2D中，然后把每张图片在图集的位置大小记录下来，然后把这些数据存入xml文件中。在生成每个表情时可以设置UV来确定哪一个图片。

**图文混排**

这个功能在网上看到大多是使用的富文本加重写Text组件来实现的。这次并没有使用这个方案，
使用的Git上面的这个[开源方案](https://github.com/mcraiha/Unity-UI-emoji)，但是这个方案有一个致命问题——只适用于一个分辨率。解决这个问题的方案是使用了NGUI中的使文字渐变的功能的原理。

    public class EmojiEffect : BaseMeshEffect{
           public override void ModifyMesh(VertexHelper vh){}
    }

继承与BaseMeshEffect抽象类，然后重写ModifyMesh方法。这个方法会在你修改文字时调用。值得注意的是这个方法在编辑器下也会调用。然后使用下面的代码获得文字的顶点。

    var vertexList=newList<UIVertex> ();
    vh.GetUIVertexStream(vertexList);

这里的每一个文字有6个顶点。第一个和最后一个顶点在一个位子，以顺时针增加，第三个和第四个顶点是在一起。如下示意图：

![顶点示意图](http://upload-images.jianshu.io/upload_images/3438059-62442687ca441984.png?imageMogr2/auto-orient/strip%7CimageView2/2/w/1240)

根据顶点位子来确定表情位置，在表情的位置使用了占位符替换。

    private const char emSpace = '\u2001';

根据字符串匹配判断表情在那个字符的后面。可以看看这篇文章使用[正则表达式匹配](http://www.jianshu.com/p/7654636c1df7)，当然也有使用直接遍历比对。




>**写在后面**

这个里面还有很多没有优化，一些东西也没有改，等后面有时间了再来修改吧。:-)
先写这么多吧。
嗯，一定要好好锻炼，好好减肥！
这个是[工程地址](https://coding.net/u/BanMing/p/TextEmoji/git)。