using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(NavToCollider))]
public class NavToColliderEditor : Editor
{
    private NavToCollider m_NavToCollider;
    private List<Vector3> m_SelectedVerts;
    private List<Collider> m_Colliders;
    private Collider m_SelectedCollider;
    private NavMeshTriangulation m_NavMeshTriangulation;
    private List<Vector3> m_RegionVerts;
    private static Vector3 regionCenter = new Vector3(30f, -20f, 30f);
    private static Vector3 regionSize = new Vector3(100f, 50f, 100f);
    private static int colliderLayer = 0;

    void OnEnable()
    {
        m_NavToCollider = target as NavToCollider;
        m_SelectedVerts = new List<Vector3>();
        m_RegionVerts = new List<Vector3>();
        m_Colliders = m_NavToCollider.GetComponentsInChildren<Collider>(true).ToList();
        m_NavMeshTriangulation = NavMesh.CalculateTriangulation();
        UnityEditor.Tools.hidden = true;
    }

    public void OnDisable()
    {
        UnityEditor.Tools.hidden = false;
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();
        regionCenter = EditorGUILayout.Vector3Field("Region Center", regionCenter);
        regionSize = EditorGUILayout.Vector3Field("Region Size", regionSize);
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
        colliderLayer = EditorGUILayout.LayerField("Collider Layer", colliderLayer);

        EditorGUILayout.Space();
        if (GUILayout.Button("Show Verticies from Selected Region"))
        {
            ShowSelectedRegion();
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Box from Selected Verticies[" + m_SelectedVerts.Count + "]"))
        {
            GenBoxCollider();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("Remove Box from Selected Collider[" + (m_SelectedCollider != null ? "1" : "0") + "]"))
        {
            RemoveSelectedCollider();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Finish"))
        {
            Undo.DestroyObjectImmediate(target);
        }
    }

    void OnSceneGUI()
    {
        OnShowRegion();
        OnShowVerts();
        CheckSelectedCollider();
        OnShowSelectedCollider();
    }

    private void OnShowRegion()
    {
        switch (UnityEditor.Tools.current)
        {
            case Tool.Move:
                DrawMoveHandle();
                break;
            case Tool.Rect:
                DrawRectHandle();
                break;
            case Tool.Scale:
                DrawScaleHandle();
                break;
        }

        Handles.color = Color.red;
        Handles.DrawWireCube(regionCenter, regionSize);
    }

    private void OnShowVerts()
    {
        Handles.color = Color.blue;
        for (int i = 0; i < m_RegionVerts.Count; i++)
        {
            var vertex = m_RegionVerts[i];
            if (m_SelectedVerts.Contains(vertex))
            {
                continue;
            }

            float pointHandleSize = HandleUtility.GetHandleSize(vertex) * 0.04f;
            float pointPickSize = pointHandleSize * 0.7f;

            if (Handles.Button(vertex, Quaternion.identity, pointHandleSize, pointPickSize, Handles.DotHandleCap))
            {
                m_SelectedVerts.Add(vertex);
                Repaint();
                return;
            }
        }

        Handles.color = Color.green;
        foreach (var vertex in m_SelectedVerts)
        {
            float pointHandleSize = HandleUtility.GetHandleSize(vertex) * 0.04f;
            float pointPickSize = pointHandleSize * 0.7f;

            Handles.DotHandleCap(0, vertex, Quaternion.identity, pointPickSize, EventType.repaint);
        }
    }

    private void CheckSelectedCollider()
    {
        foreach (var collider in m_Colliders)
        {
            if (!collider)
            {
                continue;
            }

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            if (collider.Raycast(ray, out hit, Mathf.Infinity))
            {
                m_SelectedCollider = collider;
                Repaint();
                break;
            }
        }
    }

    private void OnShowSelectedCollider()
    {
        if (!m_SelectedCollider)
        {
            return;
        }

        Transform tf = m_SelectedCollider.transform;
        BoxCollider bc = m_SelectedCollider as BoxCollider;
        if (!bc)
        {
            return;
        }
        Handles.color = Color.yellow;
        Handles.matrix = tf.localToWorldMatrix;
        Handles.DrawWireCube(bc.center, bc.size);
        Handles.matrix = Matrix4x4.identity;
    }

    void DrawMoveHandle()
    {
        regionCenter = Handles.PositionHandle(regionCenter, Quaternion.identity);
    }

    void DrawScaleHandle()
    {
        float s = Vector3.Distance(Camera.current.transform.position, regionCenter) / 5.0f;
        regionSize = Handles.ScaleHandle(regionSize, regionCenter, Quaternion.identity, s);
    }

    void DrawRectHandle()
    {
        float max = regionCenter.x + regionSize.x / 2;
        float may = regionCenter.y + regionSize.y / 2;
        float maz = regionCenter.z + regionSize.z / 2;
        float mix = regionCenter.x - regionSize.x / 2;
        float miy = regionCenter.y - regionSize.y / 2;
        float miz = regionCenter.z - regionSize.z / 2;

        Vector3 p1 = Handles.PositionHandle(new Vector3(max, may, maz), Quaternion.identity);
        max = p1.x; may = p1.y; maz = p1.z;
        Vector3 p2 = Handles.PositionHandle(new Vector3(max, may, miz), Quaternion.identity);
        max = p2.x; may = p2.y; miz = p2.z;
        Vector3 p3 = Handles.PositionHandle(new Vector3(max, miy, maz), Quaternion.identity);
        max = p3.x; miy = p3.y; maz = p3.z;
        Vector3 p4 = Handles.PositionHandle(new Vector3(mix, may, maz), Quaternion.identity);
        mix = p4.x; may = p4.y; maz = p4.z;
        Vector3 p5 = Handles.PositionHandle(new Vector3(mix, miy, miz), Quaternion.identity);
        mix = p5.x; miy = p5.y; miz = p5.z;
        Vector3 p6 = Handles.PositionHandle(new Vector3(mix, miy, maz), Quaternion.identity);
        mix = p6.x; miy = p6.y; maz = p6.z;
        Vector3 p7 = Handles.PositionHandle(new Vector3(mix, may, miz), Quaternion.identity);
        mix = p7.x; may = p7.y; miz = p7.z;
        Vector3 p8 = Handles.PositionHandle(new Vector3(max, miy, miz), Quaternion.identity);
        max = p8.x; miy = p8.y; miz = p8.z;

        regionCenter.x = (max + mix) / 2;
        regionCenter.y = (may + miy) / 2;
        regionCenter.z = (maz + miz) / 2;
        regionSize.x = (regionCenter.x - mix) * 2;
        regionSize.y = (regionCenter.y - miy) * 2;
        regionSize.z = (regionCenter.z - miz) * 2;
    }

    private void GenMeshCollider()
    {
        GameObject go = new GameObject("Mesh Collider");
        MeshCollider mc = go.AddComponent<MeshCollider>();
        go.transform.SetParent(m_NavToCollider.transform, false);
        Undo.RegisterCreatedObjectUndo(go, "GenMeshCollider");

        var ct = NavMesh.CalculateTriangulation();
        Mesh mesh = new Mesh();
        mesh.vertices = ct.vertices;
        mesh.triangles = ct.indices;
        mc.sharedMesh = mesh;
    }

    private void GenBoxCollider(bool xAxis = false)
    {
        if (m_SelectedVerts.Count <= 1)
        {
            return;
        }

        Transform tf = m_NavToCollider.transform;
        GameObject go = new GameObject("Box Collider");
        go.transform.SetParent(tf, false);
        Undo.RegisterCreatedObjectUndo(go, "GenBoxCollider");

        List<Vector3> vertList = new List<Vector3>();
        foreach (var vertex in m_SelectedVerts)
        {
            vertList.Add(vertex);
        }

        Transform tf2 = go.transform;
        // 三点先构建一个平面
        if (vertList.Count >= 3)
        {
            Plane plane = new Plane(vertList[0], vertList[1], vertList[2]);
            Quaternion rotation = Quaternion.FromToRotation(tf2.up, -plane.normal) * tf2.rotation;
            tf2.rotation = rotation;
        }

        vertList.Clear();
        foreach (var vertex in m_SelectedVerts)
        {
            vertList.Add(tf2.InverseTransformPoint(vertex));
        }

        Vector3 min = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        Vector3 max = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);
        foreach (var vertex in vertList)
        {
            min.x = Mathf.Min(vertex.x, min.x);
            min.y = Mathf.Min(vertex.y, min.y);
            min.z = Mathf.Min(vertex.z, min.z);

            max.x = Mathf.Max(vertex.x, max.x);
            max.y = Mathf.Max(vertex.y, max.y);
            max.z = Mathf.Max(vertex.z, max.z);
        }
        Vector3 size = max - min;
        Vector3 center = (max + min) / 2;

        BoxCollider bc = go.AddComponent<BoxCollider>();
        bc.size = size;
        bc.center = center;
        m_Colliders.Add(bc);
        m_SelectedVerts.Clear();
        go.layer = colliderLayer;
    }

    private void RemoveSelectedCollider()
    {
        if (m_SelectedCollider)
        {
            Undo.DestroyObjectImmediate(m_SelectedCollider.gameObject);
            m_SelectedCollider = null;
        }
    }

    private void ShowSelectedRegion()
    {
        Bounds bounds = new Bounds(regionCenter, regionSize);
        m_RegionVerts.Clear();
        foreach (var vertex in m_NavMeshTriangulation.vertices)
        {
            if (bounds.Contains(vertex))
            {
                m_RegionVerts.Add(vertex);
            }
        }
        SceneView.RepaintAll();
    }
}
