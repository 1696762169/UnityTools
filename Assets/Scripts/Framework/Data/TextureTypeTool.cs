using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureTypeTool : MonoBehaviour
{
    [Tooltip("��Ŀ¼���᳢�Խ���Ŀ¼�е�����ͼƬ�޸�Ϊָ�����ͣ�")]
    public string root;
    [Tooltip("��Ҫ�޸�Ϊ������")]
    public TextureImporterType type;
    protected void Start()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { root });
        foreach (string guid in guids)
        {
            TextureImporter texture = AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as TextureImporter;
            if (texture.textureType != type)
            {
                texture.textureType = type;
                texture.SaveAndReimport();
            }
        }
    }
}
