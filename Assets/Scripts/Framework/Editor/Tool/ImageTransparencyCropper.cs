using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class ImageTransparencyCropper : EditorWindow
{
    private Texture2D selectedTexture;
    private string savePath = "Assets/CroppedImages/";
    private bool includeSemiTransparent = true;
    private float alphaThreshold = 0.1f;
    private Vector2Int padding = Vector2Int.zero;

    [MenuItem("Tools/图片透明区域裁剪工具")]
    public static void ShowWindow()
    {
        GetWindow<ImageTransparencyCropper>("图片裁剪工具");
    }

    private void OnGUI()
    {
        GUILayout.Label("图片透明区域裁剪工具", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        selectedTexture = (Texture2D)EditorGUILayout.ObjectField("选择图片", selectedTexture, typeof(Texture2D), false);

        EditorGUILayout.Space();
        savePath = EditorGUILayout.TextField("保存路径", savePath);
        includeSemiTransparent = EditorGUILayout.Toggle("包含半透明区域", includeSemiTransparent);
        alphaThreshold = EditorGUILayout.Slider("透明度阈值", alphaThreshold, 0f, 1f);
        padding = EditorGUILayout.Vector2IntField("边距", padding);

        EditorGUILayout.Space();

        GUI.enabled = selectedTexture != null;
        if (GUILayout.Button("裁剪选中图片", GUILayout.Height(30)))
        {
            CropSelectedTexture();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("选择一张图片，工具会自动裁剪掉透明区域并保存为新图片。", MessageType.Info);
    }

    private void CropSelectedTexture()
    {
        if (selectedTexture == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择一张图片", "确定");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selectedTexture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (textureImporter == null)
        {
            EditorUtility.DisplayDialog("错误", "无法获取图片导入设置", "确定");
            return;
        }

        // 保存原始设置
        bool wasReadable = textureImporter.isReadable;
        TextureImporterCompression originalCompression = textureImporter.textureCompression;

        // 临时设置为可读
        if (!wasReadable)
        {
            textureImporter.isReadable = true;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            textureImporter.SaveAndReimport();
        }

        try
        {
            // 执行裁剪
            CropTexture(selectedTexture, assetPath);
        }
        finally
        {
            // 恢复原始设置
            if (!wasReadable)
            {
                textureImporter.isReadable = false;
                textureImporter.textureCompression = originalCompression;
                textureImporter.SaveAndReimport();
            }
        }
    }

    private void CropTexture(Texture2D texture, string originalPath)
    {
        // 获取图片边界
        Rect bounds = FindTextureBounds(texture);

        if (bounds.width <= 0 || bounds.height <= 0)
        {
            EditorUtility.DisplayDialog("错误", "无法找到有效的非透明区域", "确定");
            return;
        }

        // 创建新纹理
        int newWidth = Mathf.RoundToInt(bounds.width) + padding.x * 2;
        int newHeight = Mathf.RoundToInt(bounds.height) + padding.y * 2;

        Texture2D croppedTexture = new Texture2D(newWidth, newHeight, TextureFormat.RGBA32, false);

        // 复制像素数据
        for (int x = 0; x < newWidth; x++)
        {
            for (int y = 0; y < newHeight; y++)
            {
                int sourceX = Mathf.RoundToInt(bounds.x) + x - padding.x;
                int sourceY = Mathf.RoundToInt(bounds.y) + y - padding.y;

                Color pixel = Color.clear;

                if (sourceX >= 0 && sourceX < texture.width && sourceY >= 0 && sourceY < texture.height)
                {
                    pixel = texture.GetPixel(sourceX, sourceY);
                }

                croppedTexture.SetPixel(x, y, pixel);
            }
        }

        croppedTexture.Apply();

        // 保存图片
        SaveTexture(croppedTexture, originalPath);

        DestroyImmediate(croppedTexture);
    }

    private Rect FindTextureBounds(Texture2D texture)
    {
        int minX = texture.width;
        int minY = texture.height;
        int maxX = 0;
        int maxY = 0;

        // 遍历所有像素寻找边界
        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color pixel = texture.GetPixel(x, y);

                if (IsPixelVisible(pixel))
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }
        }

        // 如果没有找到可见像素
        if (minX > maxX || minY > maxY)
        {
            return new Rect(0, 0, 0, 0);
        }

        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private bool IsPixelVisible(Color pixel)
    {
        if (includeSemiTransparent)
        {
            return pixel.a > alphaThreshold;
        }
        else
        {
            return pixel.a >= 0.99f;
        }
    }

    private void SaveTexture(Texture2D texture, string originalPath)
    {
        // 确保保存目录存在
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        string originalFileName = Path.GetFileNameWithoutExtension(originalPath);
        string newFileName = $"{originalFileName}_Cropped.png";
        string fullPath = Path.Combine(savePath, newFileName);

        // 编码为PNG
        byte[] pngData = texture.EncodeToPNG();

        // 写入文件
        File.WriteAllBytes(fullPath, pngData);

        // 刷新资源数据库
        AssetDatabase.Refresh();

        // 设置导入设置
        TextureImporter importer = AssetImporter.GetAtPath(fullPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        EditorUtility.DisplayDialog("完成", $"图片已保存到: {fullPath}", "确定");

        // 在Project窗口中高亮显示新文件
        Object savedAsset = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
        Selection.activeObject = savedAsset;
        EditorGUIUtility.PingObject(savedAsset);
    }

    // 批量处理功能
    [MenuItem("Assets/批量裁剪图片透明区域", false, 300)]
    static void BatchCropSelectedTextures()
    {
        Object[] selectedObjects = Selection.objects;
        List<Texture2D> texturesToCrop = new List<Texture2D>();

        foreach (Object obj in selectedObjects)
        {
            if (obj is Texture2D texture)
            {
                texturesToCrop.Add(texture);
            }
        }

        if (texturesToCrop.Count == 0)
        {
            EditorUtility.DisplayDialog("错误", "请先选择要裁剪的图片", "确定");
            return;
        }

        int successCount = 0;
        foreach (Texture2D texture in texturesToCrop)
        {
            try
            {
                ImageTransparencyCropper cropper = CreateInstance<ImageTransparencyCropper>();
                cropper.selectedTexture = texture;
                cropper.savePath = "Assets/CroppedImages/";
                cropper.CropSelectedTexture();
                successCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"裁剪图片 {texture.name} 时出错: {e.Message}");
            }
        }

        EditorUtility.DisplayDialog("完成", $"成功裁剪 {successCount}/{texturesToCrop.Count} 张图片", "确定");
    }

    [MenuItem("Assets/批量裁剪图片透明区域", true)]
    static bool ValidateBatchCropSelectedTextures()
    {
        return Selection.activeObject is Texture2D;
    }
}