/****************************** Module Header ******************************\
Module Name:  <TDR_TrackExport>
Project:      TRACK BUILDER
Copyright (c) Mad Cow.
Version         0.0.5

This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.Build.Pipeline;
using UnityEditor;
#endif

[ExecuteInEditMode]
public class TDR_TrackExport : MonoBehaviour
{
    [SerializeField] private UnityEngine.Object TrackScene;

    [SerializeField] private string m_trackPath;
    [SerializeField] private string m_trackName;
    [SerializeField] private Tracks trackData;
    [SerializeField] private GameObject TrackMap;
    [SerializeField] private Material mapMaterial;
    [SerializeField] private Vector2 mapOffset;

    [SerializeField] private Texture2D TrackIcon;
    [SerializeField] private Texture2D TrackSplash;
    [SerializeField] private Texture2D TrackBack;
    [SerializeField] private Texture2D TrackMapImage;


    [Serializable]
    public class Tracks
    {
        public int version = 2;
        public string UUID;
        public string leaderboardName;
        public int[] allowCategory;
        public string[] leaderboardCategory;
        public string displayName;
        public string fileName;
        public string levelName;
        public string url;
        public string imagePath;
        public TrackExtraInfo extraInfo = new TrackExtraInfo();
    }

    [Serializable]
    public class TrackExtraInfo
    {
        public float lat = 41.5684837f;
        public float lon = 2.2573012f;
        public int[] date = { 2021, 14, 0, 17, 8 };
        public float[] dashPosition = new float[3] { 0f, 0f, 0f };
        public float[] dashRotation = new float[3] { 0f, 0f, 0f };
        public float[] mapPosition = new float[3] { 0f, 0f, 0f };
        public float[] mapRotation = new float[3] { 0f, 0f, 0f };
        public float mapScale = 1f;
        public int maxPitPosition = 9;
        public float extraSteer = 0f;
        public bool pysicallySky = false;
        public SoilData[] soilDatas = new SoilData[8];
    }

    [Serializable]
    public class SoilData
    {
        public string label = "";
        public int undergroundID = -1;
        public float ColorErosion = 0.33f;
        public float DepthErosion = 0.01f;
        public float DepthPenetrating = 0f;
        public float Resistance = 0.001f;
        public float LongGrip = 0.94f;
        public float LatGrip = 1.1f;
        public float MudFx = 0.0f;
        public float DirtFx = 0.05f;
    }

    public bool[] soilDrop = new bool[8] { false, false, false, false, false, false, false, false };

    public bool CheckFormat(Texture2D source)
    {
        return source.format == TextureFormat.RGBA32 || source.format == TextureFormat.RGB24;
    }

    /// <summary>
    /// EDITOR
    /// </summary>
#if UNITY_EDITOR
    [CustomEditor(typeof(TDR_TrackExport))]
    public class LevelScriptEditor : Editor
    {

        private static Texture2D TextureField(string name, Texture2D texture)
        {
            GUILayout.BeginVertical();
            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperCenter;
            style.fixedWidth = 70;
            GUILayout.Label(name, style);
            var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(70), GUILayout.Height(70));
            GUILayout.EndVertical();
            return result;
        }

        public override void OnInspectorGUI()
        {
            TDR_TrackExport myTarget = (TDR_TrackExport)target;
            EditorGUI.BeginChangeCheck();
            //Header
            if (myTarget.trackData == null)
            {
                myTarget.trackData = new Tracks();
            }

            if (myTarget.TrackScene == null)
            {
                var scenes = AssetDatabase.LoadAssetAtPath(SceneManager.GetActiveScene().path, typeof(UnityEngine.Object));
                if (scenes != null)
                {
                    myTarget.TrackScene = scenes;
                }
                else
                {
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("TDR TRACK BUILDER", EditorStyles.boldLabel);
                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("WARNING: SCENE IS NOT SAVED", EditorStyles.boldLabel);
                    return;
                }
            }
            else
            {
                var scenes = AssetDatabase.LoadAssetAtPath(SceneManager.GetActiveScene().path, typeof(UnityEngine.Object));
                if (scenes.name != myTarget.TrackScene.name)
                {
                    myTarget.TrackScene = scenes;
                }
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("TDR TRACK BUILDER", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Scene Asset " + myTarget.TrackScene.name, EditorStyles.boldLabel);//, typeof(UnityEngine.Object), false);
            EditorGUILayout.Space(5);

            GUILayout.BeginHorizontal();
            myTarget.TrackIcon = TextureField("Menu Icon ", myTarget.TrackIcon);
            myTarget.TrackSplash = TextureField("Menu Splash ", myTarget.TrackSplash);
            myTarget.TrackBack = TextureField("Menu Back ", myTarget.TrackBack);
            myTarget.TrackMapImage = TextureField("Ingame Map ", myTarget.TrackMapImage);

            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("TRACK INFO ", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);
            myTarget.m_trackName = EditorGUILayout.TextField("track name ", myTarget.m_trackName);
            myTarget.trackData.extraInfo.lat = EditorGUILayout.FloatField("Latitude ", myTarget.trackData.extraInfo.lat);
            myTarget.trackData.extraInfo.lon = EditorGUILayout.FloatField("Longitude ", myTarget.trackData.extraInfo.lon);
            myTarget.trackData.extraInfo.mapScale = EditorGUILayout.FloatField("MapWidth ", myTarget.trackData.extraInfo.mapScale);
            myTarget.mapOffset = EditorGUILayout.Vector2Field("Offset ", myTarget.mapOffset);


            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("TERRAIN EROSION DATA", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);
            bool invalidUnderground = false;
            if (myTarget.trackData.extraInfo.soilDatas != null && myTarget.trackData.extraInfo.soilDatas.Length >= 8)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (myTarget.trackData.extraInfo.soilDatas[i] == null)
                    {
                        myTarget.trackData.extraInfo.soilDatas[i] = new SoilData();
                    }
                    var labelUP = myTarget.trackData.extraInfo.soilDatas[i].label.ToUpper();
                    if (string.IsNullOrEmpty(labelUP))
                    {
                        labelUP = "(" + (i) + ") UNNAMED";
                    }
                    else
                    {
                        labelUP = "(" + (i) + ") " + labelUP;
                    }
                    var underInfo = "";
                    var uID = myTarget.trackData.extraInfo.soilDatas[i].undergroundID;
                    if (uID != -1 && uID < myTarget.trackData.extraInfo.soilDatas.Length)
                    {
                        var underG = myTarget.trackData.extraInfo.soilDatas[uID].label.ToUpper();
                        if (string.IsNullOrEmpty(underG))
                        {
                            underG = "UNNAMED";
                        }

                        underInfo += " is above " + "(" + uID + ") " + underG;
                    }
                    else if (uID == -1)
                    {
                        underInfo += "has no Underground";
                    }
                    else
                    {
                        underInfo += "Warning: INVALID Underground";
                        invalidUnderground = true;
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(labelUP + " " + (!myTarget.soilDrop[i] ? underInfo : ""), EditorStyles.whiteLabel);

                    if (GUILayout.Button(myTarget.soilDrop[i] ? "-" : "+"))
                    {
                        myTarget.soilDrop[i] = !myTarget.soilDrop[i];
                    }
                    EditorGUILayout.EndHorizontal();
                    if (myTarget.soilDrop[i])
                    {
                        EditorGUILayout.BeginVertical();
                        myTarget.trackData.extraInfo.soilDatas[i].label = EditorGUILayout.TextField("  Name ", myTarget.trackData.extraInfo.soilDatas[i].label);
                        myTarget.trackData.extraInfo.soilDatas[i].undergroundID = EditorGUILayout.IntField("  Under ID ", myTarget.trackData.extraInfo.soilDatas[i].undergroundID);
                        myTarget.trackData.extraInfo.soilDatas[i].ColorErosion = EditorGUILayout.FloatField("   Color erosion ", myTarget.trackData.extraInfo.soilDatas[i].ColorErosion);
                        myTarget.trackData.extraInfo.soilDatas[i].DepthErosion = EditorGUILayout.FloatField("   Height erosion ", myTarget.trackData.extraInfo.soilDatas[i].DepthErosion);
                        myTarget.trackData.extraInfo.soilDatas[i].DepthPenetrating = EditorGUILayout.FloatField("   Soil penetrating factor ", myTarget.trackData.extraInfo.soilDatas[i].DepthPenetrating);
                        myTarget.trackData.extraInfo.soilDatas[i].Resistance = EditorGUILayout.FloatField("   Roll resistance PHX Terrain ", myTarget.trackData.extraInfo.soilDatas[i].Resistance);
                        myTarget.trackData.extraInfo.soilDatas[i].LongGrip = EditorGUILayout.FloatField("   Longitudinal grip", myTarget.trackData.extraInfo.soilDatas[i].LongGrip);
                        myTarget.trackData.extraInfo.soilDatas[i].LatGrip = EditorGUILayout.FloatField("    Lateral grip ", myTarget.trackData.extraInfo.soilDatas[i].LatGrip);
                        myTarget.trackData.extraInfo.soilDatas[i].MudFx = EditorGUILayout.FloatField("   Mud FX", myTarget.trackData.extraInfo.soilDatas[i].MudFx);
                        myTarget.trackData.extraInfo.soilDatas[i].DirtFx = EditorGUILayout.FloatField("    Dirt FX ", myTarget.trackData.extraInfo.soilDatas[i].DirtFx);
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.Space(2);
                }
            }

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("SAVE TRACK", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("Game path ", myTarget.m_trackPath);
            if (GUILayout.Button("..", GUILayout.Width(30)))
            {
                myTarget.m_trackPath = EditorUtility.OpenFolderPanel("Select bike game path", "", "");
            }
            EditorGUILayout.EndHorizontal();

            //CHECK FOR ALLOW SAVE
            EditorGUILayout.Space(10);
            bool disableCheck = false;

            if (invalidUnderground)
            {
                EditorGUILayout.LabelField("WARNING: Found some invalid Underground", EditorStyles.boldLabel);
                disableCheck = true;
            }

            if (myTarget.TrackScene == null)
            {
                EditorGUILayout.LabelField("WARNING: Missing scene object", EditorStyles.boldLabel);
                disableCheck = true;
            }
            if (myTarget.TrackMap == null)
            {
                EditorGUILayout.LabelField("WARNING: Missing map object", EditorStyles.boldLabel);
                disableCheck = true;
            }
            if (String.IsNullOrEmpty(myTarget.m_trackName))
            {
                EditorGUILayout.LabelField("WARNING: Missing trackname", EditorStyles.boldLabel);
                disableCheck = true;
            }
            if (String.IsNullOrEmpty(myTarget.m_trackPath))
            {
                EditorGUILayout.LabelField("WARNING: Missing path", EditorStyles.boldLabel);
                disableCheck = true;
            }


            if (myTarget.TrackIcon == null)
            {
                EditorGUILayout.LabelField("WARNING: Missing menu icon", EditorStyles.boldLabel);
                disableCheck = true;
            }
            else
            {
                if (!myTarget.TrackIcon.isReadable || !myTarget.CheckFormat(myTarget.TrackIcon))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("WARNING: menu icon is ReadOnly or compressed", EditorStyles.boldLabel);
                    if (GUILayout.Button("FIX"))
                    {
                        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(myTarget.TrackIcon));
                        importer.isReadable = true;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SaveAndReimport();
                    }
                    EditorGUILayout.EndHorizontal();
                    disableCheck = true;
                }
            }

            if (myTarget.TrackSplash == null)
            {
                EditorGUILayout.LabelField("WARNING: Missing menu splash", EditorStyles.miniLabel);
                disableCheck = true;
            }
            else
            {
                if (!myTarget.TrackSplash.isReadable || !myTarget.CheckFormat(myTarget.TrackSplash))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("WARNING: splash is ReadOnly or compressed", EditorStyles.boldLabel);
                    if (GUILayout.Button("FIX"))
                    {
                        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(myTarget.TrackSplash));
                        importer.isReadable = true;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SaveAndReimport();
                    }
                    EditorGUILayout.EndHorizontal();
                    disableCheck = true;
                }
            }

            if (myTarget.TrackBack == null)
            {
                EditorGUILayout.LabelField("WARNING: Missing menu back", EditorStyles.boldLabel);
                disableCheck = true;
            }
            else
            {
                if (!myTarget.TrackBack.isReadable || !myTarget.CheckFormat(myTarget.TrackBack))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("WARNING: back is ReadOnly or compressed", EditorStyles.boldLabel);
                    if (GUILayout.Button("FIX"))
                    {
                        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(myTarget.TrackBack));
                        importer.isReadable = true;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SaveAndReimport();
                    }
                    EditorGUILayout.EndHorizontal();
                    disableCheck = true;
                }
            }

            if (myTarget.TrackMapImage == null)
            {
                EditorGUILayout.LabelField("WARNING: Missing menu map", EditorStyles.boldLabel);
                disableCheck = true;
            }
            else
            {
                if (!myTarget.TrackMapImage.isReadable || !myTarget.CheckFormat(myTarget.TrackMapImage))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("WARNING: map is ReadOnly or compressed", EditorStyles.boldLabel);
                    if (GUILayout.Button("FIX"))
                    {
                        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(myTarget.TrackMapImage));
                        importer.isReadable = true;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.SaveAndReimport();
                    }
                    EditorGUILayout.EndHorizontal();
                    disableCheck = true;
                }
            }

            if (EditorUtility.IsDirty(myTarget))
            {
                EditorGUILayout.LabelField("WARNING: SAVE UNITY SCENE FIRST ", EditorStyles.boldLabel);
                disableCheck = true;
            }

            EditorGUILayout.Space(5);

            if (myTarget.TrackMap == null)
            {
                var oldMap = GameObject.Find("#trackmap");
                if (oldMap != null)
                {
                    Destroy(oldMap);
                }

                myTarget.TrackMap = GameObject.CreatePrimitive(PrimitiveType.Quad);
                myTarget.TrackMap.name = "#trackmap";
                myTarget.TrackMap.transform.SetParent(myTarget.transform);
                myTarget.TrackMap.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                myTarget.mapMaterial = new Material(Resources.Load<Material>("Materials/GameMap"));
                myTarget.mapMaterial.name = "#MAPMATERIAL";
                myTarget.TrackMap.GetComponent<Renderer>().sharedMaterial = myTarget.mapMaterial;
            }
            else
            {
                myTarget.TrackMap.transform.localScale = Vector3.one * myTarget.trackData.extraInfo.mapScale;
                myTarget.TrackMap.transform.position = new Vector3(myTarget.mapOffset.x, 700f, myTarget.mapOffset.y);
                if (myTarget.TrackMapImage != null)
                {
                    myTarget.mapMaterial.SetTexture("_BaseColorMap", myTarget.TrackMapImage);
                }
            }


            //Commands
            EditorGUI.BeginDisabledGroup(disableCheck);
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save " + myTarget.m_trackName))
            {
                myTarget.SaveBike();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            var changed = EditorGUI.EndChangeCheck();
            if (changed && myTarget != null)
            {
                EditorUtility.SetDirty(myTarget);

            }


        }
    }
#endif

    public string FileName
    {
        get
        {
            return m_trackName.Replace(" ", "");
        }
    }

    public string LeaderboardName
    {
        get
        {
            return m_trackName.Replace(" ", "");
        }
    }

    public string UUID_Name
    {
        get
        {
            return "#" + m_trackName.Replace(" ", "");
        }
    }

    void SaveBike()
    {
#if UNITY_EDITOR

        string fileContainer = m_trackPath + "/";
        //Directory.CreateDirectory(fileContainer);
        if (!Directory.Exists(fileContainer))
        {
            m_trackPath = "";
            return;
        }
        string jsonObject = "";
        trackData.version = 2;
        trackData.fileName = FileName;
        trackData.leaderboardName = LeaderboardName;
        trackData.displayName = m_trackName;
        trackData.UUID = UUID_Name;
        trackData.levelName = TrackScene.name;

        byte[] bytes;
        bytes = TrackMapImage.EncodeToPNG();
        File.WriteAllBytes(fileContainer + FileName + "-map.png", bytes);

        bytes = TrackIcon.EncodeToPNG();
        File.WriteAllBytes(fileContainer + FileName + "-menu.png", bytes);

        bytes = TrackSplash.EncodeToPNG();
        File.WriteAllBytes(fileContainer + FileName + "-splash.png", bytes);

        bytes = TrackBack.EncodeToPNG();
        File.WriteAllBytes(fileContainer + FileName + "-back.png", bytes);

        var dashMan = GameObject.Find("#dashboard_man");
        if (dashMan != null)
        {
            trackData.extraInfo.dashPosition = new float[] { dashMan.transform.position.x, dashMan.transform.position.y, dashMan.transform.position.z };
            trackData.extraInfo.dashRotation = new float[] { dashMan.transform.eulerAngles.x, dashMan.transform.eulerAngles.y, dashMan.transform.eulerAngles.z };
        }


        trackData.extraInfo.mapPosition = new float[] { TrackMap.transform.position.x, 0f, TrackMap.transform.position.z };
        trackData.extraInfo.mapRotation = new float[] { 0f, -180f, 0f };
        trackData.extraInfo.mapScale = TrackMap.transform.localScale.x * 0.1f;

        jsonObject = EditorJsonUtility.ToJson(trackData, true);
        File.WriteAllText(fileContainer + FileName + "_info.tdr", jsonObject);

        UnityEngine.Object[] prefabList = new UnityEngine.Object[1]
        {
            TrackScene,
        };
        foreach (var item in prefabList)
        {
            string assetPath = AssetDatabase.GetAssetPath(item);
            Debug.Log("#ASSIGH TAG " + assetPath + " " + FileName);
            AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant(FileName, "");
        }
        var manifest = CompatibilityBuildPipeline.BuildAssetBundles(fileContainer, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows64);
        foreach (var item in prefabList)
        {
            string assetPath = AssetDatabase.GetAssetPath(item);
            Debug.Log("#REMOVE TAG " + assetPath);
            AssetImporter.GetAtPath(assetPath).SetAssetBundleNameAndVariant("", "");
        }

#endif
    }
}
