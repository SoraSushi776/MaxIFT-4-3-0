using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System.IO;

public class MeshCombineWithColor : EditorWindow
{
    public GameObject root;
    public string saveFolder = "DLFM_Combine";

    [MenuItem("Tools/ModelCombine")]
    static void Open()
    {
        GetWindow<MeshCombineWithColor>("Color Atlas Combiner");
    }

    void OnGUI()
    {
        GUILayout.Space(10);

        root = (GameObject)EditorGUILayout.ObjectField(
            "父集合",
            root,
            typeof(GameObject),
            true);

        saveFolder = EditorGUILayout.TextField(
            "保存的位置 (在 Assets 文件下)",
            saveFolder);

        GUILayout.Space(10);

        if (GUILayout.Button("合并"))
        {
            Combine();
        }
    }

    void Combine()
    {
        if (root == null)
        {
            Debug.LogError("父集合为空");
            return;
        }

        string rootName = root.name;

        string folderPath = Path.Combine("Assets", saveFolder);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        MeshFilter[] filters =
            root.GetComponentsInChildren<MeshFilter>(true);

        Dictionary<Material, int> matIndex =
            new Dictionary<Material, int>();

        List<Material> materials =
            new List<Material>();

        // ===== 收集唯一材质 =====
        foreach (var f in filters)
        {
            var renderer = f.GetComponent<MeshRenderer>();
            if (!renderer) continue;

            foreach (var m in renderer.sharedMaterials)
            {
                if (m == null) continue;

                if (!matIndex.ContainsKey(m))
                {
                    matIndex[m] = materials.Count;
                    materials.Add(m);
                }
            }
        }

        if (materials.Count == 0)
        {
            Debug.LogError("无材质");
            return;
        }

        // ===== 创建颜色 Atlas =====
        int count = materials.Count;
        int size = Mathf.CeilToInt(Mathf.Sqrt(count));

        // 强制 sRGB
        Texture2D atlas = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false,
            false
        );

        atlas.filterMode = FilterMode.Point;

        for (int i = 0; i < count; i++)
        {
            Color col = Color.white;

            if (materials[i].HasProperty("_Color"))
                col = materials[i].GetColor("_Color");

            int x = i % size;
            int y = i / size;

            atlas.SetPixel(x, y, col);
        }

        atlas.Apply();

        string texPath =
            Path.Combine(folderPath,
            rootName + "_ColorAtlas.png");

        File.WriteAllBytes(texPath, atlas.EncodeToPNG());
        AssetDatabase.Refresh();

        // 强制 sRGB 导入
        TextureImporter importer =
            (TextureImporter)TextureImporter.GetAtPath(texPath);

        importer.sRGBTexture = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression =
            TextureImporterCompression.Uncompressed;

        importer.SaveAndReimport();

        Texture2D savedAtlas =
            AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

        // ===== 重写 UV + 合并 Mesh =====
        List<CombineInstance> combines =
            new List<CombineInstance>();

        foreach (var f in filters)
        {
            var renderer = f.GetComponent<MeshRenderer>();
            if (!renderer || f.sharedMesh == null)
                continue;

            Mesh mesh = f.sharedMesh;
            Material[] mats = renderer.sharedMaterials;

            for (int sub = 0; sub < mesh.subMeshCount; sub++)
            {
                if (sub >= mats.Length) continue;

                Material mat = mats[sub];
                if (mat == null) continue;

                int index = matIndex[mat];

                int x = index % size;
                int y = index / size;

                float uvX = (x + 0.5f) / size;
                float uvY = (y + 0.5f) / size;

                Mesh subMesh = new Mesh();
                subMesh.vertices = mesh.vertices;
                subMesh.normals = mesh.normals;
                subMesh.tangents = mesh.tangents;
                subMesh.triangles = mesh.GetTriangles(sub);

                Vector2[] uvs =
                    new Vector2[subMesh.vertexCount];

                for (int i = 0; i < uvs.Length; i++)
                    uvs[i] = new Vector2(uvX, uvY);

                subMesh.uv = uvs;

                CombineInstance ci = new CombineInstance();
                ci.mesh = subMesh;
                ci.transform = f.transform.localToWorldMatrix;

                combines.Add(ci);
            }
        }

        Mesh finalMesh = new Mesh();
        finalMesh.indexFormat = IndexFormat.UInt32;
        finalMesh.CombineMeshes(
            combines.ToArray(),
            true,
            true);

        finalMesh.RecalculateBounds();

        string meshPath =
            Path.Combine(folderPath,
            rootName + "_Mesh.asset");

        AssetDatabase.CreateAsset(finalMesh, meshPath);

        // ===== 创建材质 =====
        Material newMat =
            new Material(Shader.Find("Standard"));

        newMat.SetTexture("_MainTex", savedAtlas);
        newMat.SetFloat("_Metallic", 0f);
        newMat.SetFloat("_Glossiness", 0f);//0.5f

        string matPath =
            Path.Combine(folderPath,
            rootName + "_Mat.mat");

        AssetDatabase.CreateAsset(newMat, matPath);

        // ===== 创建 Prefab =====
        GameObject go =
            new GameObject(rootName + "_Combined");

        go.AddComponent<MeshFilter>().sharedMesh =
            finalMesh;

        go.AddComponent<MeshRenderer>().sharedMaterial =
            newMat;

        string prefabPath =
            Path.Combine(folderPath,
            rootName + ".prefab");

        PrefabUtility.SaveAsPrefabAssetAndConnect(
            go,
            prefabPath,
            InteractionMode.UserAction);

        Debug.Log("合并成功: " + rootName);
    }
}