using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine.UI;
using System.Text.RegularExpressions;
public class EmojiTextManager : MonoBehaviour
{
    private Dictionary<string, Rect> emojiRects = new Dictionary<string, Rect>();
    private Dictionary<string, string> emojiCharToTags = new Dictionary<string, string>();
    private Dictionary<string, string> emojiTagToChars = new Dictionary<string, string>();
    //普通自定义表情图片 tag
    private List<Dictionary<string, Rect>> normalIcon = new List<Dictionary<string, Rect>>();
    private const char emSpace = '\u2001';
    private const string regexTag = @"{emoji:(?<id>\d*)}";

    #region  外部接口

    //外部接口 这里设置转换过后的的字符名字
    public void SetUITextThatHasEmoji(Text textToEdit, string inputString, Texture atlas)
    {
        StartCoroutine(mSetUITextThatHasEmojiByAtlas(textToEdit, inputString, atlas));
    }

    //传入的InputString含有"{emoji:(%d*)}"地方代表着表情
    public void SetChatTextEmoji(Text textToEdit, string inputString, string idStr, Texture atlas)
    {
        string[] idStrList = idStr.Split('|');
        for (int i = 0; i < idStrList.Length; i++)
        {
            if (string.IsNullOrEmpty(idStrList[i]))
            {
                break;
            }

            var replaceStr = "{emoji:" + idStrList[i] + "}";
            inputString = inputString.Replace(replaceStr, emojiCharToTags[replaceStr]);
        }
        SetUITextThatHasEmoji(textToEdit, inputString, atlas);
    }

    //普通图片解析以"{emoji:(?<id>/d*)}"
    public void SetUITextThatHasIconByAtlas(Text textToEdit, string inputString, Texture atlas, int atlasType = 0)
    {
        StartCoroutine(mSetUITextThatHasIconByAtlas(textToEdit, inputString, atlas, atlasType));
    }

    //填充普通icon map 格式为["textTag"] + "|" + ["rectX"]+ "|" + ["rectY"] + "|" + ["rectWidth"] + "|" + ["rectHeight"] + "|" + "0"
    //最后一个type默认为0，可以设置多张图集
    public void ParseNormalIcon(string inputString)
    {
        string[] split = inputString.Split('|');
        float x = float.Parse(split[1], System.Globalization.CultureInfo.InvariantCulture);
        float y = float.Parse(split[2], System.Globalization.CultureInfo.InvariantCulture);
        float width = float.Parse(split[3], System.Globalization.CultureInfo.InvariantCulture);
        float height = float.Parse(split[4], System.Globalization.CultureInfo.InvariantCulture);
        int atlasType = int.Parse(split[5], System.Globalization.CultureInfo.InvariantCulture);

        if (this.normalIcon.Count >= atlasType)
        {
            var count = atlasType - this.normalIcon.Count + 1;
            for (int i = 0; i < count; i++)
            {
                this.normalIcon.Add(new Dictionary<string, Rect>());
            }
        }
        if (this.normalIcon[atlasType] == null)
        {
            this.normalIcon[atlasType] = new Dictionary<string, Rect>();
        }
        this.normalIcon[atlasType][split[0]] = new Rect(x, y, width, height);
    }

    /// <summary>
    /// 填充数据emoji
    /// </summary>
    /// 格式为["emojiFileNames"] + "|" + ["rectX"]+ "|" + ["rectY"] + "|" + ["rectWidth"] + "|" + ["rectHeight"] +["textTag"] 
    /// <param name="inputString"></param>
    public void ParseEmojiInfo(string inputString)
    {
        string[] split = inputString.Split('|');
        float x = float.Parse(split[1], System.Globalization.CultureInfo.InvariantCulture);
        float y = float.Parse(split[2], System.Globalization.CultureInfo.InvariantCulture);
        float width = float.Parse(split[3], System.Globalization.CultureInfo.InvariantCulture);
        float height = float.Parse(split[4], System.Globalization.CultureInfo.InvariantCulture);
        var charName = GetConvertedString(split[0]);
        this.emojiRects[charName] = new Rect(x, y, width, height);
        this.emojiCharToTags[split[5]] = charName;
        this.emojiTagToChars[charName] = split[5];
    }

    //手机输入法输入emoji 转换{emoji:(?<id>/d*)}
    public string ConvertInputFieldText(string inputString)
    {

        StringBuilder sb = new StringBuilder();
        int i = 0;
        while (i < inputString.Length)
        {
            string singleChar = inputString.Substring(i, 1);
            string doubleChar = "";
            string fourChar = "";

            if (i < (inputString.Length - 1))
            {
                doubleChar = inputString.Substring(i, 2);
            }

            if (i < (inputString.Length - 3))
            {
                fourChar = inputString.Substring(i, 4);
            }

            if (this.emojiRects.ContainsKey(fourChar))
            {
                // Check 64 bit emojis first
                sb.Append(emojiTagToChars[fourChar]);
                i += 4;
            }
            else if (this.emojiRects.ContainsKey(doubleChar))
            {
                // Then check 32 bit emojis
                sb.Append(emojiTagToChars[doubleChar]);
                i += 2;
            }
            else if (this.emojiRects.ContainsKey(singleChar))
            {
                // Finally check 16 bit emojis
                sb.Append(emojiTagToChars[singleChar]);
                i++;
            }
            else
            {
                sb.Append(inputString[i]);
                i++;
            }
        }
        return sb.ToString();
    }
    #endregion

    private string GetConvertedString(string inputString)
    {
        string[] converted = inputString.Split('-');
        for (int j = 0; j < converted.Length; j++)
        {
            converted[j] = char.ConvertFromUtf32(Convert.ToInt32(converted[j], 16));
        }
        return string.Join(string.Empty, converted);
    }

    private IEnumerator mSetUITextThatHasEmojiByAtlas(Text textToEdit, string inputString, Texture emojiAtlas)
    {
        List<EmojiItem> emojiReplacements = new List<EmojiItem>();
        StringBuilder sb = new StringBuilder();
        EmojiEffect emojiEffect = textToEdit.GetComponent<EmojiEffect>();

        if (emojiEffect == null)
        {
            emojiEffect = textToEdit.gameObject.AddComponent<EmojiEffect>();
        }

        int i = 0;
        while (i < inputString.Length)
        {
            string singleChar = inputString.Substring(i, 1);
            string doubleChar = "";
            string fourChar = "";

            if (i < (inputString.Length - 1))
            {
                doubleChar = inputString.Substring(i, 2);
            }

            if (i < (inputString.Length - 3))
            {
                fourChar = inputString.Substring(i, 4);
            }

            if (this.emojiRects.ContainsKey(fourChar))
            {
                // Check 64 bit emojis first
                sb.Append(emSpace);
                emojiReplacements.Add(new EmojiItem(sb.Length, emojiRects[fourChar]));
                i += 4;
            }
            else if (this.emojiRects.ContainsKey(doubleChar))
            {
                // Then check 32 bit emojis
                sb.Append(emSpace);
                emojiReplacements.Add(new EmojiItem(sb.Length, emojiRects[doubleChar]));
                i += 2;
            }
            else if (this.emojiRects.ContainsKey(singleChar))
            {
                // Finally check 16 bit emojis
                sb.Append(emSpace);
                emojiReplacements.Add(new EmojiItem(sb.Length, emojiRects[singleChar]));
                i++;
            }
            else
            {
                sb.Append(inputString[i]);
                i++;
            }
        }

        // 设置文字
        textToEdit.text = sb.ToString();

        yield return null;

        emojiEffect.SetEmojiByEmojiRawImage(emojiAtlas, emojiReplacements);

    }

    private IEnumerator mSetUITextThatHasIconByAtlas(Text textToEdit, string inputString, Texture atlas, int normalAtlasType = 0)
    {
        var dataMap = normalIcon[normalAtlasType];
        if (dataMap == null)
        {
            Debug.LogError("没有填充Type为：" + normalAtlasType + "的Map");
            yield return 0;
        }
        Regex regex = new Regex(regexTag);
        var replaceEmojiList = new List<EmojiItem>();

        EmojiEffect emojiEffect = textToEdit.GetComponent<EmojiEffect>();

        if (emojiEffect == null)
        {
            emojiEffect = textToEdit.gameObject.AddComponent<EmojiEffect>();
        }
        int length = 0;
        int count = 0;
        foreach (Match match in regex.Matches(inputString))
        {
            var tagName = match.Value.ToString();
            var index = match.Index - length+count;
            length += tagName.Length;
            count++;
            replaceEmojiList.Add(new EmojiItem(index, dataMap[tagName]));
        }
        var res = regex.Replace(inputString, emSpace.ToString());
        textToEdit.text = res;

        yield return null;

        emojiEffect.SetEmojiByEmojiRawImage(atlas, replaceEmojiList);
    }

    #region 单例
    private static EmojiTextManager instance;

    public static EmojiTextManager Instance
    {

        get
        {

            if (!instance)
            {
                instance = FindObjectOfType<EmojiTextManager>();
                if (!instance)
                {
                    Debug.LogError("脚本并不在场景中激活！");
                }
            }
            return instance;
        }
    }

    #endregion

}


public class EmojiItem
{
    //这里的位置从1开始
    public int pos;
    public string emoji;
    public Rect atlasRect;
    public Sprite sprite;

    public EmojiItem(int p, string s)
    {
        this.pos = p;
        this.emoji = s;
    }
    public EmojiItem(int p, Rect r)
    {
        this.pos = p;
        this.atlasRect = r;
    }
    public EmojiItem(int p, Sprite s)
    {
        this.pos = p;
        this.sprite = s;
    }
}