using Pathfinding;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GMEditor : Editor
{
    #region Fields

    public static long lastUpdateTick;
    public static string[] layerNames;
    public static List<int> layerNumbers;
    public static List<string> layers;
    public bool[] current;
    private GridManager GM;

    #endregion Fields

    #region Methods

    private GridGraph grid;

    public LayerMask LayerMaskField(string label, LayerMask selected, bool showSpecial)
    {
        //Unity 3.5 and up

        if (layers == null || (System.DateTime.Now.Ticks - lastUpdateTick > 10000000L && Event.current.type == EventType.Layout))
        {
            lastUpdateTick = System.DateTime.Now.Ticks;
            if (layers == null)
            {
                layers = new List<string>();
                layerNumbers = new List<int>();
                layerNames = new string[4];
            }
            else
            {
                layers.Clear();
                layerNumbers.Clear();
            }

            int emptyLayers = 0;
            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);

                if (layerName != "")
                {
                    for (; emptyLayers > 0; emptyLayers--)
                        layers.Add("Layer " + (i - emptyLayers));
                    layerNumbers.Add(i);
                    layers.Add(layerName);
                }
                else
                {
                    emptyLayers++;
                }
            }

            if (layerNames.Length != layers.Count)
            {
                layerNames = new string[layers.Count];
            }
            for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];
        }

        selected.value = EditorGUILayout.MaskField(label, selected.value, layerNames);

        return selected;
    }

    public override void OnInspectorGUI()
    {
        GM = (GridManager)target;

        EditorGUILayout.LabelField("Scanning: " + GridManager.isScanning);
        GM.DebugLvl = (GridManager.DebugLevel)EditorGUILayout.EnumPopup("Debug Level: ", GM.DebugLvl);

        GUILayout.Space(10f);

        GM.grid.name = EditorGUILayout.TextField("Name: ", GM.grid.name);
        GM.grid.center = EditorGUILayout.Vector3Field("Center", GM.grid.center);
        GM.grid.WorldSize = EditorGUILayout.Vector2Field("World Size", GM.grid.WorldSize);
        GM.grid.NodeRadius = EditorGUILayout.FloatField("Node Radius", GM.grid.NodeRadius);
        GM.grid.angleLimit = EditorGUILayout.IntSlider("Max Angle", GM.grid.angleLimit, 0, 90);
        GM.grid.WalkableMask = LayerMaskField("Walkable Layer(s):", GM.grid.WalkableMask, true);
        GUILayout.Space(10);
        if (GUILayout.Button("Scan Grid"))
        {
            GridManager.ScanGrid();
        }
        GUI.backgroundColor = Color.red;
        GUI.backgroundColor = Color.white;
        GUILayout.Space(10);
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    #endregion Methods
}