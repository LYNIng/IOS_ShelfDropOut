using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.U2D;
using System.Linq;

public class UnityTestEditor
{

    [MenuItem("数据/删除本地数据")]
    public static void DeleteData()
    {
        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Assets/Export Sprites to PNG")]
    private static void ExportSpritesToPNG()
    {
        // 获取当前选中的对象
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected!");
            return;
        }

        foreach (Object selectedObject in selectedObjects)
        {
            if (selectedObject is Texture2D)
            {
                ExportSpriteSheetToPNG(selectedObject as Texture2D);
            }
            else
            {
                Debug.LogWarning($"Selected object {selectedObject.name} is not a Texture2D!");
            }
        }
    }

    private static void ExportSpriteSheetToPNG(Texture2D texture)
    {
        string texturePath = AssetDatabase.GetAssetPath(texture);
        TextureImporter textureImporter = AssetImporter.GetAtPath(texturePath) as TextureImporter;

        if (textureImporter == null || textureImporter.spriteImportMode != SpriteImportMode.Multiple)
        {
            Debug.LogWarning($"Texture {texture.name} is not a Sprite Sheet (Sprite Mode must be 'Multiple')!");
            return;
        }

        // 加载Sprite Sheet的所有Sprite
        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning($"No Sprites found in {texture.name}!");
            return;
        }

        // 创建导出目录
        string exportDir = Path.Combine(Application.dataPath, "ExportedSprites", texture.name);
        if (!Directory.Exists(exportDir))
        {
            Directory.CreateDirectory(exportDir);
        }

        // 遍历每个Sprite并导出为PNG
        foreach (Sprite sprite in sprites)
        {
            // 创建一个新的Texture2D来存储Sprite的像素数据
            Texture2D spriteTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, TextureFormat.RGBA32, false);
            spriteTexture.SetPixels(sprite.texture.GetPixels(
                (int)sprite.rect.x,
                (int)sprite.rect.y,
                (int)sprite.rect.width,
                (int)sprite.rect.height
            ));
            spriteTexture.Apply();

            // 转换为PNG字节数据
            byte[] pngData = spriteTexture.EncodeToPNG();

            // 保存PNG文件
            string filePath = Path.Combine(exportDir, $"{sprite.name}.png");
            File.WriteAllBytes(filePath, pngData);

            Debug.Log($"Exported: {filePath}");
        }

        AssetDatabase.Refresh();
        Debug.Log($"All Sprites from {texture.name} exported to {exportDir}");
    }
}