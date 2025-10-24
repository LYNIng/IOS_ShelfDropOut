using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BatchRenameTool : EditorWindow
{
    private string folderPath = "Assets/";
    private string searchPattern = "*.*";
    private string prefix = "";
    private string suffix = "";
    private string findStr = "";
    private string replaceStr = "";
    private bool includeSubfolders = false;
    private bool showPreview = false;
    private List<string> previewList = new List<string>();

    [MenuItem("Tools/Batch Rename Tool")]
    public static void ShowWindow()
    {
        GetWindow<BatchRenameTool>("Batch Rename");
    }

    private void OnGUI()
    {
        GUILayout.Label("Batch Rename Settings", EditorStyles.boldLabel);

        // 文件夹路径
        EditorGUILayout.BeginHorizontal();
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(80)))
        {
            folderPath = EditorUtility.OpenFolderPanel("Select Folder", folderPath, "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                folderPath = "Assets" + folderPath.Replace(Application.dataPath, "");
            }
        }
        EditorGUILayout.EndHorizontal();

        // 文件筛选
        searchPattern = EditorGUILayout.TextField("File Filter (e.g. *.png)", searchPattern);
        includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);

        // 修改方式
        EditorGUILayout.Space();
        GUILayout.Label("Rename Options", EditorStyles.boldLabel);
        prefix = EditorGUILayout.TextField("Add Prefix", prefix);
        suffix = EditorGUILayout.TextField("Add Suffix (before extension)", suffix);

        EditorGUILayout.Space();
        findStr = EditorGUILayout.TextField("Find", findStr);
        replaceStr = EditorGUILayout.TextField("Replace With", replaceStr);

        // 预览按钮
        EditorGUILayout.Space();
        if (GUILayout.Button("Preview Changes"))
        {
            PreviewChanges();
        }

        // 预览结果
        if (showPreview && previewList.Count > 0)
        {
            EditorGUILayout.Space();
            GUILayout.Label("Preview Changes:", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");
            foreach (var item in previewList)
            {
                EditorGUILayout.LabelField(item);
            }
            EditorGUILayout.EndVertical();
        }

        // 执行按钮
        EditorGUILayout.Space();
        if (GUILayout.Button("Execute Rename"))
        {
            if (EditorUtility.DisplayDialog("Confirm Rename",
                $"Are you sure you want to rename {previewList.Count} files?",
                "Yes", "No"))
            {
                ExecuteRename();
            }
        }
    }

    private void PreviewChanges()
    {
        previewList.Clear();
        showPreview = false;

        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("Folder does not exist: " + folderPath);
            return;
        }

        SearchOption searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        string[] files = Directory.GetFiles(folderPath, searchPattern, searchOption);

        foreach (string filePath in files)
        {
            if (filePath.EndsWith(".meta")) continue;

            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            // 应用修改
            string newName = fileName;

            // 替换字符串
            if (!string.IsNullOrEmpty(findStr))
            {
                newName = newName.Replace(findStr, replaceStr);
            }

            // 添加前缀和后缀
            newName = prefix + newName + suffix + extension;

            // 添加到预览列表
            string oldName = Path.GetFileName(filePath);
            previewList.Add($"{oldName}  ->  {newName}");
        }

        showPreview = true;
    }

    private void ExecuteRename()
    {
        if (previewList.Count == 0)
        {
            Debug.LogWarning("No files to rename. Please preview first.");
            return;
        }

        SearchOption searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        string[] files = Directory.GetFiles(folderPath, searchPattern, searchOption);

        int renamedCount = 0;

        foreach (string filePath in files)
        {
            if (filePath.EndsWith(".meta")) continue;

            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            // 应用修改
            string newName = fileName;

            // 替换字符串
            if (!string.IsNullOrEmpty(findStr))
            {
                newName = newName.Replace(findStr, replaceStr);
            }

            // 添加前缀和后缀
            newName = prefix + newName + suffix;

            // 构建新路径
            string newPath = Path.Combine(directory, newName + extension);

            // 如果文件名没有变化，跳过
            if (filePath == newPath) continue;

            // 确保新文件名是唯一的
            newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

            // 重命名文件
            string error = AssetDatabase.MoveAsset(filePath, newPath);
            if (string.IsNullOrEmpty(error))
            {
                renamedCount++;
            }
            else
            {
                Debug.LogError($"Failed to rename {filePath}: {error}");
            }
        }

        AssetDatabase.Refresh();
        Debug.Log($"Renamed {renamedCount} files successfully.");
        previewList.Clear();
        showPreview = false;
    }
}