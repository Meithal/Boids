using UnityEngine;
using UnityEditor;

public class FindRunningScripts : EditorWindow
{
    [MenuItem("Tools/Find Running Scripts")]
    static void FindScripts()
    {
        MonoBehaviour[] scripts = GameObject.FindObjectsOfType<MonoBehaviour>();
        
        foreach (MonoBehaviour script in scripts)
        {
            Debug.Log($"Script: {script.GetType().Name} is attached to GameObject: {script.gameObject.name}");
        }
    }
}