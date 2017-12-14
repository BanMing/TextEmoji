using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using UnityEngine.UI;

public class EmojiEffect : BaseMeshEffect
{
    private List<UIVertex> vertexList;
    private Text thisText;
    private const string EmojiName = "emoji";

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!Application.isPlaying)
        {
            return;
        }
        vertexList.Clear();
        vh.GetUIVertexStream(vertexList);
        //StartCoroutine(PrintVertexList());
    }
    /// <summary>
    /// 这里是设置Emoji使用emoji图集来 EmojiItem中有rect数据
    /// </summary>
    public void SetEmojiByEmojiRawImage(Texture emojiAtlas, List<EmojiItem> emojiItemList)
    {

        //清空之前有生成的emoji
        var childEmojiTransList = new List<Transform>();
        foreach (Transform item in transform)
        {
            if (item.name == EmojiName)
            {
                item.gameObject.SetActive(false);
                childEmojiTransList.Add(item);
            }
        }

        //生成每一个emoji
        for (int i = 0; i < emojiItemList.Count; i++)
        {
            var emojiIndex = emojiItemList[i].pos;
            var index = Mathf.Clamp(emojiIndex * 6 - 4, 2, int.MaxValue);
            //取消被截断的文字显示emoji
            if(index>vertexList.Count){
                break;
            }
            Transform emojiTrans;
            if (i > childEmojiTransList.Count - 1)
            {
                emojiTrans = NewEmojiRwaImage(emojiAtlas);
            }
            else
            {
                emojiTrans = childEmojiTransList[i];
            }
            emojiTrans.gameObject.SetActive(true);           
            var imagePos = new Vector3(vertexList[index].position.x, vertexList[index].position.y, 0);
            emojiTrans.localPosition = imagePos;

            //赋值
            var ri = emojiTrans.GetComponent<RawImage>();
            ri.uvRect = emojiItemList[i].atlasRect;
        }

    }

    private Transform NewEmojiRwaImage(Texture emojiAtlas)
    {
        GameObject newRawImage = new GameObject("emoji");
        var rawComponent = newRawImage.AddComponent<RawImage>();
        rawComponent.texture = emojiAtlas;
        newRawImage.transform.SetParent(transform);
        newRawImage.transform.localScale = Vector3.one;
        rawComponent.rectTransform.sizeDelta = new Vector2(thisText.fontSize, thisText.fontSize);
        rawComponent.rectTransform.pivot = Vector2.zero;
        return newRawImage.transform;
    }

    protected override void Start()
    {
        base.Start();
        vertexList = new List<UIVertex>();
        thisText = GetComponent<Text>();
    }

    /// <summary>
    /// 生成顶点测试
    /// </summary>
    private IEnumerator PrintVertexList()
    {
        yield return null;
        int k = 0;
        foreach (var item in vertexList)
        {
            GameObject go = new GameObject(k.ToString());
            k++;
            go.transform.SetParent(transform);
            var image = go.AddComponent<RawImage>();
            image.color = Color.red;
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
            go.transform.localScale = Vector3.one;
            Debug.Log("before:" + item.position.ToString());
            go.transform.localPosition = item.position;
        }
    }
}
