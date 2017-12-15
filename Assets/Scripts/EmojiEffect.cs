using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class EmojiEffect : BaseMeshEffect {
    private List<UIVertex> vertexList;
    private Text thisText;
    private const string EmojiName = "emoji";

    public override void ModifyMesh (VertexHelper vh) {
        if (!Application.isPlaying || vertexList == null) {
            return;
        }
        vertexList.Clear ();
        vh.GetUIVertexStream (vertexList);
        //StartCoroutine(PrintVertexList());
    }
    /// <summary>
    /// 这里是设置Emoji使用emoji图集来 EmojiItem中有rect数据
    /// </summary>
    public void SetEmojiByEmojiRawImage (Texture emojiAtlas, List<EmojiItem> emojiItemList) {

        if (transform == null) { return; }

        //清空之前有生成的emoji
        var childEmojiTransList = new List<Transform> ();
        foreach (Transform item in transform) {
            if (item.name == EmojiName) {
                item.gameObject.SetActive (false);
                childEmojiTransList.Add (item);
            }
        }

        if (emojiAtlas == null || emojiItemList.Count < 1) { return; }

        //生成每一个emoji
        for (int i = 0; i < emojiItemList.Count; i++) {

            var emojiIndex = emojiItemList[i].pos;

            var index = Mathf.Clamp (emojiIndex * 6 - 4, 2, int.MaxValue);
            var imagePos = new Vector3 (vertexList[index].position.x, vertexList[index].position.y, 0);

            //不显示没有渲染的
            if (vertexList.Count <= index || (i>0 && IsOutTextArea(emojiItemList[i-1].pos,emojiIndex))) {
                return;
            }

            Transform emojiTrans;
            if (i > childEmojiTransList.Count - 1) {
                emojiTrans = NewEmojiRwaImage (emojiAtlas);
            } else {
                emojiTrans = childEmojiTransList[i];
            }
            emojiTrans.gameObject.SetActive (true);
            emojiTrans.localPosition = imagePos;

            //赋值
            var ri = emojiTrans.GetComponent<RawImage> ();
            ri.uvRect = emojiItemList[i].atlasRect;
        }

    }

    private Transform NewEmojiRwaImage (Texture emojiAtlas) {
        GameObject newRawImage = new GameObject ("emoji");
        var rawComponent = newRawImage.AddComponent<RawImage> ();
        rawComponent.texture = emojiAtlas;
        newRawImage.transform.SetParent (transform);
        newRawImage.transform.localScale = Vector3.one;
        rawComponent.rectTransform.sizeDelta = new Vector2 (thisText.fontSize, thisText.fontSize);
        rawComponent.rectTransform.pivot = new Vector2 (0, 0.1f);
        return newRawImage.transform;
    }

    protected override void Start () {
        base.Start ();
        vertexList = new List<UIVertex> ();
        thisText = GetComponent<Text> ();
    }

    /// <summary>
    /// 生成顶点测试
    /// </summary>
    private IEnumerator PrintVertexList () {
        yield return null;
        int k = 0;
        foreach (var item in vertexList) {
            GameObject go = new GameObject (k.ToString ());
            k++;
            go.transform.SetParent (transform);
            var image = go.AddComponent<RawImage> ();
            image.color = Color.red;
            go.GetComponent<RectTransform> ().sizeDelta = new Vector2 (1, 1);
            go.transform.localScale = Vector3.one;
            Debug.Log ("before:" + item.position.ToString ());
            go.transform.localPosition = item.position;
        }
    }
    private bool IsOutTextArea (int lastPos, int curPos) {

        var lastIndex = Mathf.Clamp (lastPos * 6 - 4, 2, int.MaxValue);
        var lastImagePos = new Vector3 (vertexList[lastIndex].position.x, vertexList[lastIndex].position.y, 0);

        var curIndex = Mathf.Clamp (curPos * 6 - 4, 2, int.MaxValue);
        var curImagePos = new Vector3 (vertexList[curIndex].position.x, vertexList[curIndex].position.y, 0);
        // if (lastImagePos.x > curImagePos.x && lastImagePos.y - thisText.fontSize != curImagePos.y) { }
        // Debug.Log("lastImagePos.y - thisText.fontSize/2:"+(lastImagePos.y - thisText.fontSize/2).ToString());
        // Debug.Log("curImagePos.y:"+(curImagePos.y).ToString());
        return lastImagePos.x > curImagePos.x && lastImagePos.y - thisText.fontSize/2 < curImagePos.y;
    }
}