using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureTypeTool : MonoBehaviour
{
    [Tooltip("根目录（会尝试将此目录中的所有图片修改为指定类型）")]
    public string root;
    [Tooltip("需要修改为的类型")]
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
