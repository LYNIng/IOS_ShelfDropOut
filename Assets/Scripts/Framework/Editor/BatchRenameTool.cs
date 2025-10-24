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

        // �ļ���·��
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

        // �ļ�ɸѡ
        searchPattern = EditorGUILayout.TextField("File Filter (e.g. *.png)", searchPattern);
        includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);

        // �޸ķ�ʽ
        EditorGUILayout.Space();
        GUILayout.Label("Rename Options", EditorStyles.boldLabel);
        prefix = EditorGUILayout.TextField("Add Prefix", prefix);
        suffix = EditorGUILayout.TextField("Add Suffix (before extension)", suffix);

        EditorGUILayout.Space();
        findStr = EditorGUILayout.TextField("Find", findStr);
        replaceStr = EditorGUILayout.TextField("Replace With", replaceStr);

        // Ԥ����ť
        EditorGUILayout.Space();
        if (GUILayout.Button("Preview Changes"))
        {
            PreviewChanges();
        }

        // Ԥ�����
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

        // ִ�а�ť
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

            // Ӧ���޸�
            string newName = fileName;

            // �滻�ַ���
            if (!string.IsNullOrEmpty(findStr))
            {
                newName = newName.Replace(findStr, replaceStr);
            }

            // ���ǰ׺�ͺ�׺
            newName = prefix + newName + suffix + extension;

            // ��ӵ�Ԥ���б�
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

            // Ӧ���޸�
            string newName = fileName;

            // �滻�ַ���
            if (!string.IsNullOrEmpty(findStr))
            {
                newName = newName.Replace(findStr, replaceStr);
            }

            // ���ǰ׺�ͺ�׺
            newName = prefix + newName + suffix;

            // ������·��
            string newPath = Path.Combine(directory, newName + extension);

            // ����ļ���û�б仯������
            if (filePath == newPath) continue;

            // ȷ�����ļ�����Ψһ��
            newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

            // �������ļ�
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