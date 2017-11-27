using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Xml;
using System.Text;

public class TestShow : MonoBehaviour
{
    public Text qq;
    public Text Emoji;
    public Text  inputEmoji;
    public Texture iconAtlas;
    public Texture emojiAtlas;
    void Start()
    {
        ReadEmojiXml();
        ReadIconXml();

        EmojiTextManager.Instance.SetUITextThatHasIconByAtlas(qq, "{emoji:44}QQ{emoji:55}表情{emoji:4}", iconAtlas);
        EmojiTextManager.Instance.SetChatTextEmoji(Emoji, "{emoji:54}Emoji表情{emoji:4}", "54|4|", emojiAtlas);
        EmojiTextManager.Instance.SetUITextThatHasEmoji(inputEmoji, "自行车: \U0001F6B4, 国旗: \U0001F1FA\U0001F1F8", emojiAtlas);

    }

    private void ReadIconXml()
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(Application.dataPath + "/Xml/QQIconXml.xml");
        XmlNodeList nodeList = XmlDoc.SelectSingleNode("root").ChildNodes;
        foreach (XmlElement xe in nodeList)
        {
            var resStr = string.Empty;
            resStr = xe["textTag"].InnerText + "|" + xe["rectX"].InnerText + "|" + xe["rectY"].InnerText + "|" + xe["rectWidth"].InnerText + "|" + xe["rectHeight"].InnerText + "|" + "0";
            EmojiTextManager.Instance.ParseNormalIcon(resStr);
        }
    }

    private void ReadEmojiXml()
    {
        XmlDocument XmlDoc = new XmlDocument();
        XmlDoc.Load(Application.dataPath + "/Xml/EmojiXml.xml");
        XmlNodeList nodeList = XmlDoc.SelectSingleNode("root").ChildNodes;
        foreach (XmlElement xe in nodeList)
        {
            var resStr = string.Empty;
            resStr = xe["emojiFileNames"].InnerText + "|" + xe["rectX"].InnerText + "|" + xe["rectY"].InnerText + "|" + xe["rectWidth"].InnerText + "|" + xe["rectHeight"].InnerText + "|" + xe["textTag"].InnerText;
            EmojiTextManager.Instance.ParseEmojiInfo(resStr);
        }
    }

}
