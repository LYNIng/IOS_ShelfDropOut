using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureGrayscaleConverter : EditorWindow
{
    private Texture2D sourceTexture;
    private Texture2D grayscaleTexture;
    private string savePath = "Assets/";

    [MenuItem("Tools/Texture Grayscale Converter")]
    public static void ShowWindow()
    {
        GetWindow<TextureGrayscaleConverter>("Grayscale Converter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Convert Texture to Grayscale", EditorStyles.boldLabel);

        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", sourceTexture, typeof(Texture2D), false);

        if (GUILayout.Button("Convert to Grayscale") && sourceTexture != null)
        {
            ConvertToGrayscale();
        }

        if (grayscaleTexture != null)
        {
            GUILayout.Label("Grayscale Result:");
            EditorGUILayout.ObjectField(grayscaleTexture, typeof(Texture2D), false);

            GUILayout.Label("Save Path:");
            savePath = EditorGUILayout.TextField(savePath);

            if (GUILayout.Button("Save Grayscale Texture"))
            {
                SaveTexture();
            }
        }
    }

    private void ConvertToGrayscale()
    {
        // Create a copy of the source texture
        string path = AssetDatabase.GetAssetPath(sourceTexture);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.isReadable = true;
        AssetDatabase.ImportAsset(path);

        Color[] pixels = sourceTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            float grayValue = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;
            pixels[i] = new Color(grayValue, grayValue, grayValue, pixels[i].a);
        }

        grayscaleTexture = new Texture2D(sourceTexture.width, sourceTexture.height);
        grayscaleTexture.SetPixels(pixels);
        grayscaleTexture.Apply();
    }

    private void SaveTexture()
    {
        byte[] bytes = grayscaleTexture.EncodeToPNG();
        string fullPath = Path.Combine(savePath, sourceTexture.name + "_Grayscale.png");
        File.WriteAllBytes(fullPath, bytes);
        AssetDatabase.Refresh();

        Debug.Log("Grayscale texture saved to: " + fullPath);
    }
}