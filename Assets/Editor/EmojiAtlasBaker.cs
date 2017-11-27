using UnityEngine;

using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
public class EmojiAtlasBaker : EditorWindow
{
    //private Vector2 atlasSize = Vector2.zero;
    private string iconFilesPath;
    private string atlasFilePath;
    private string xmlFilePath;
    private string xmlName;
    private string atlasName;
    private const string EditorPrefsStr = "Bake Emoji Atlas Tool";
    private void BakeEmojiAtlas()
    {
        //string pathToEmojis = Application.dataPath + "/Resources/ForAssetBundles/QiLe/UIAtlas/EmojiInfo/Textures/Emojis";
        //string pathToEmojiInfo = Application.dataPath + "/StreamingAssets/Data/EmojiInfo.xml";
        //string pathToBakedAtlas = Application.dataPath + "/Resources/ForAssetBundles/QiLe/UIAtlas/EmojiInfo/Textures/Baked/BakedEmojis.png";
        string pathToEmojis = iconFilesPath;
        string pathToEmojiInfo = xmlFilePath + "/" + xmlName+".xml";
        string pathToBakedAtlas = atlasFilePath + "/" + atlasName + ".png";
        string[] emojiFileNames = Directory.GetFiles(pathToEmojis, "*.png");

        Texture2D[] emojiTextures = new Texture2D[emojiFileNames.Length];

        for (int i = 0; i < emojiFileNames.Length; i++)
        {
            // 创建每个emoji
            Texture2D tex = new Texture2D(2, 2);
            if (!tex.LoadImage(File.ReadAllBytes(emojiFileNames[i])))
            {
                Debug.LogError("无法加载 " + emojiFileNames[i] + " 图片!!!");
                return;
            }
            emojiTextures[i] = tex;
        }

        // 创建图集
        Texture2D atlas = new Texture2D(2048, 2048);

        Rect[] rects = atlas.PackTextures(emojiTextures, 1, 2048);

        // 生成图集
        byte[] atlasBytes = atlas.EncodeToPNG();
        File.WriteAllBytes(pathToBakedAtlas, atlasBytes);

        // 把每张emoji信息存在xml中
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<root>");
        var spece = "    ";
        for (int i = 0; i < emojiFileNames.Length; i++)
        {
            sb.AppendLine(spece + "<emoji>");
            sb.AppendLine(spece + spece + "<id>" + i + "</id>");
            sb.AppendLine(spece + spece + "<textTag>{emoji:" + i + "}</textTag>");
            sb.AppendLine(spece + spece + "<emojiFileNames>" + Path.GetFileNameWithoutExtension(emojiFileNames[i]) + "</emojiFileNames>");
            sb.AppendLine(spece + spece + "<rectX>" + rects[i].x + "</rectX>");
            sb.AppendLine(spece + spece + "<rectY>" + rects[i].y + "</rectY>");
            sb.AppendLine(spece + spece + "<rectWidth>" + rects[i].width + "</rectWidth>");
            sb.AppendLine(spece + spece + "<rectHeight>" + rects[i].height + "</rectHeight>");
            sb.AppendLine(spece + "</emoji>");
        }
        sb.AppendLine("</root>");

        File.WriteAllText(pathToEmojiInfo, sb.ToString());

        AssetDatabase.Refresh();

        var index = pathToBakedAtlas.IndexOf("Assets/");
        var path = pathToBakedAtlas.Substring(index);
        var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
        textureImporter.alphaIsTransparency = true;
        textureImporter.SaveAndReimport();
        AssetDatabase.ImportAsset(path);

        Debug.Log("Baked " + emojiFileNames.Length + " emojis into " + pathToBakedAtlas);
    }

    [MenuItem("Tools/Bake Emoji Atlas")]
    private static void OpenBakeEmojiAtlasPanel()
    {
        var myWindow = GetWindow<EmojiAtlasBaker>();

        myWindow.iconFilesPath = EditorPrefs.GetString(EditorPrefsStr + "IconFilesPath", Application.dataPath);
        myWindow.atlasFilePath = EditorPrefs.GetString(EditorPrefsStr + "AtlasFilePath", Application.dataPath);
        myWindow.xmlFilePath = EditorPrefs.GetString(EditorPrefsStr + "XmlFilePath", Application.dataPath);
        myWindow.xmlName = EditorPrefs.GetString(EditorPrefsStr + "XmlName", "xmlName");
        myWindow.atlasName = EditorPrefs.GetString(EditorPrefsStr + "AtlasName", "atlasName");
    }
    private void OnGUI()
    {
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Icon源文件目录：",GUILayout.Width(90));
            iconFilesPath = EditorGUILayout.DelayedTextField(iconFilesPath);
            if (GUILayout.Button("选择", GUILayout.Width(70)))
            {
                 iconFilesPath = EditorUtility.OpenFolderPanel("Icon源文件目录", iconFilesPath, "");
            }
            EditorPrefs.SetString(EditorPrefsStr + "IconFilesPath", iconFilesPath);
            GUILayout.EndHorizontal();
        }

        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("图集生成路径：", GUILayout.Width(90));
            atlasFilePath = EditorGUILayout.DelayedTextField(atlasFilePath);
            if (GUILayout.Button("选择", GUILayout.Width(70)))
            {
                atlasFilePath = EditorUtility.OpenFolderPanel("图集生成路径", atlasFilePath, "");
            }
            EditorPrefs.SetString(EditorPrefsStr + "AtlasFilePath", atlasFilePath);
            GUILayout.EndHorizontal();
        }

        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Xml生成路径：", GUILayout.Width(90));
            xmlFilePath = EditorGUILayout.DelayedTextField(xmlFilePath);
            if (GUILayout.Button("选择", GUILayout.Width(70)))
            {
                xmlFilePath = EditorUtility.OpenFolderPanel("Xml生成路径", xmlFilePath, "");
            }
            EditorPrefs.SetString(EditorPrefsStr + "XmlFilePath", xmlFilePath);
            GUILayout.EndHorizontal();
        }

        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("图集名字：");
            atlasName = EditorGUILayout.DelayedTextField(atlasName);
            EditorPrefs.SetString(EditorPrefsStr + "AtlasName", atlasName);
            GUILayout.EndHorizontal();
        }

        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Xml名字：");
            xmlName= EditorGUILayout.DelayedTextField(xmlName);
            EditorPrefs.SetString(EditorPrefsStr + "XmlName", xmlName);
            GUILayout.EndHorizontal();
        }
        if (GUILayout.Button("生成"))
        {
            BakeEmojiAtlas();
            Close();
       }
    }
}
#endif
