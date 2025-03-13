using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

[System.Serializable]
[CanEditMultipleObjects]
public class NoteReader : EditorWindow
{

    public string file;

    public float speed = 10;

    public bool makeRoad;
    public float roadWidth = 1;
    public GameObject road;

    public bool autoPlay;
    public GameObject autoPlayTrigger;

    public Vector3 forward1 = new Vector3(1, 0, 0);
    public Vector3 forward2 = new Vector3(0, 0, 1);


    [MenuItem("Tools/DLFM/NoteReader")]
    public static void Init()
    {
        GetWindow<NoteReader>().Show();
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical();

        GUILayout.Label("File: " + file);

        if (GUILayout.Button("Load File"))
        {
            string s = loadFile();
            if (s != "") file = s;
        }

        //铺路选项
        makeRoad = EditorGUILayout.BeginToggleGroup("Make Road", makeRoad);
        roadWidth = EditorGUILayout.FloatField("Road Width", roadWidth);
        road = (GameObject)EditorGUILayout.ObjectField("Road", road, typeof(GameObject));
        EditorGUILayout.EndToggleGroup();

        //自动选项
        autoPlay = EditorGUILayout.BeginToggleGroup("Auto Play", autoPlay);
        autoPlayTrigger = (GameObject)EditorGUILayout.ObjectField("Trigger", autoPlayTrigger, typeof(GameObject));
        EditorGUILayout.EndToggleGroup();

        //空
        EditorGUILayout.Space();

        //输入线速
        speed = EditorGUILayout.FloatField("Speed", speed);

        //输入方向
        forward1 = EditorGUILayout.Vector3Field("Forward 1", forward1, GUILayout.MaxWidth(250));
        forward2 = EditorGUILayout.Vector3Field("Forward 2", forward2, GUILayout.MaxWidth(250));

        if (GUILayout.Button("Spawn Objects"))
        {
            bool reading = false;
            Vector3 lastPostion = new Vector3(2, 0, 0);
            Vector3 thisPostion;
            float lastTime = 0;
            Vector3 thisForward = forward1;


            GameObject roadPar = new GameObject();
            roadPar.name = "Road";
            if (!makeRoad) DestroyImmediate(roadPar);

            GameObject autoPlayPar = new GameObject();
            autoPlayPar.name = "Auto Play Triggers";
            if (!autoPlay) DestroyImmediate(autoPlayPar);

            StreamReader stream = null;
            try
            {
                stream = File.OpenText(file);
                string line;
                while ((line = stream.ReadLine()) != null)
                {
                    if (line == "[HitObjects]" && (!reading))
                    {
                        reading = true;
                    }
                    else if (reading)
                    {
                        if (line == null) break;
                        string[] array = line.Split(',');
                        //Debug.Log(array[2]);

                        if (Convert.ToSingle(array[2]) == lastTime) continue;

                        thisPostion = lastPostion + thisForward * speed * (Convert.ToSingle(array[2]) - lastTime) / 1000;

                        //铺路
                        if (makeRoad)
                        {
                            GameObject roadCr = Instantiate(road, (thisPostion + lastPostion) / 2, new Quaternion());
                            roadCr.transform.parent = roadPar.transform;
                            roadCr.transform.localScale = new Vector3(Mathf.Abs(thisPostion.x - lastPostion.x) + roadWidth, 1, Mathf.Abs(thisPostion.z - lastPostion.z) + roadWidth);
                        }

                        //自动
                        if (autoPlay)
                        {
                            GameObject Tri = Instantiate(autoPlayTrigger, thisPostion + thisForward / 2, new Quaternion());
                            Tri.transform.parent = autoPlayPar.transform;
                        }

                        //转向
                        if (thisForward == forward1)
                        {
                            thisForward = forward2;
                        }
                        else thisForward = forward1;

                        lastTime = Convert.ToSingle(array[2]);
                        lastPostion = thisPostion;

                    }
                }
                //Debug.Log(arrayData.Count);
                stream.Close();
                stream.Dispose();
            }
            catch
            {
                Debug.LogError("[NoteReader] File Has been lost!");
            }
            if (roadPar) roadPar.transform.position = new Vector3(0, -1, 0);
        }

        GUILayout.EndVertical();

        if (GUI.changed)
        {
            if (UnityEngine.Random.Range(0, 10) == 0) Debug.Log("[NoteReader] 感谢使用，来支持下子智君呗 https://space.bilibili.com/426181974");
        }
    }

    public static string loadFile()
    {
        string bgImagePath = "";
        //加载图片的对话框，是在编辑模式下的
        string extion = "osu";
        string path = "";
#if UNITY_EDITOR
        // Editor specific code here
        path = UnityEditor.EditorUtility.OpenFilePanel("Choose music", UnityEngine.Application.dataPath, extion);
#endif
        //WWW ww = new WWW("file:///" + path);
        //print(ww.url);
        if (path != "")//load image as texture
        {
            //StartCoroutine(WaitLoad(path));
            Debug.Log("[NoteReader] Get music successfully: " + path);
            bgImagePath = path;
        }
        return bgImagePath;
    }
}