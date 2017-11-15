using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NavToColliderCreater
{
    [MenuItem("Tool/NavToCollider Creater")]
    static void Init()
    {
        NavToCollider navToCollider = GameObject.FindObjectOfType<NavToCollider>();
        if (navToCollider == null)
        {
            GameObject go = GameObject.Find("[NavToColliderManager]");
            if (go)
            {
                navToCollider = Undo.AddComponent<NavToCollider>(go);
            }
            else
            {
                navToCollider = new GameObject("[NavToColliderManager]").AddComponent<NavToCollider>();
                Undo.RegisterCreatedObjectUndo(navToCollider.gameObject, "Create object");
            }
        }

        Selection.activeGameObject = navToCollider.gameObject;
        EditorGUIUtility.PingObject(navToCollider.gameObject);
    }
}
