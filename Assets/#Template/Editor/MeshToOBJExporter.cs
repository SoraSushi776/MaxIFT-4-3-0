using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class MeshAssetToOBJWindow : EditorWindow
{
    private Mesh meshAsset;
    private string exportFolder = "ExportedOBJ";

    [MenuItem("Tools/Mesh Asset To OBJ")]
    static void OpenWindow()
    {
        GetWindow<MeshAssetToOBJWindow>("Mesh → OBJ");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);

        meshAsset = (Mesh)EditorGUILayout.ObjectField("Mesh Asset", meshAsset, typeof(Mesh), false);
        exportFolder = EditorGUILayout.TextField("Export Folder", exportFolder);

        GUILayout.Space(20);

        if (GUILayout.Button("导出为 OBJ", GUILayout.Height(40)))
        {
            if (meshAsset == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择 Mesh", "OK");
                return;
            }

            ExportMesh(meshAsset);
        }
    }

    void ExportMesh(Mesh mesh)
{
    string folderPath = Path.Combine(Application.dataPath, exportFolder);
    if (!Directory.Exists(folderPath))
        Directory.CreateDirectory(folderPath);

    string filePath = Path.Combine(folderPath, mesh.name + ".obj");

    StringBuilder sb = new StringBuilder();
    sb.AppendLine("o " + mesh.name);

    Vector3[] vertices = mesh.vertices;
    Vector3[] normals = mesh.normals;
    Vector2[] uvs = mesh.uv;

    bool hasNormals = normals != null && normals.Length == vertices.Length;
    bool hasUV = uvs != null && uvs.Length == vertices.Length;

    // ===== 坐标系转换：翻转 X =====
    foreach (Vector3 v in vertices)
        sb.AppendLine($"v {-v.x} {v.y} {v.z}");

    sb.AppendLine();

    if (hasUV)
    {
        foreach (Vector2 uv in uvs)
            sb.AppendLine($"vt {uv.x} {uv.y}");
        sb.AppendLine();
    }

    if (hasNormals)
    {
        foreach (Vector3 n in normals)
            sb.AppendLine($"vn {-n.x} {n.y} {n.z}");
        sb.AppendLine();
    }

    // ===== 面顺序反转 =====
    for (int sub = 0; sub < mesh.subMeshCount; sub++)
    {
        int[] triangles = mesh.GetTriangles(sub);

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int a = triangles[i] + 1;
            int b = triangles[i + 1] + 1;
            int c = triangles[i + 2] + 1;

            // 反转 b 和 c
            if (hasUV && hasNormals)
                sb.AppendLine($"f {a}/{a}/{a} {c}/{c}/{c} {b}/{b}/{b}");
            else if (hasUV)
                sb.AppendLine($"f {a}/{a} {c}/{c} {b}/{b}");
            else if (hasNormals)
                sb.AppendLine($"f {a}//{a} {c}//{c} {b}//{b}");
            else
                sb.AppendLine($"f {a} {c} {b}");
        }
    }

    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    AssetDatabase.Refresh();

    EditorUtility.DisplayDialog("完成", "导出成功:\n" + filePath, "OK");
}
}