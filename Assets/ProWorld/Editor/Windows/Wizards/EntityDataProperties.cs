using ProWorldEditor;
using ProWorldSDK;
using UnityEditor;
using UnityEngine;

public class EntityDataProperties : ScriptableWizard
{
    public EntityEditorData EntityOld;
    public EntityData EntityNew;
    private bool _close;
    private bool _isPicking;

    public static void CreateTDP(EntityEditorData entityData)
    {
        var tdp = DisplayWizard<EntityDataProperties>("Create Tree", "Apply");
        tdp.minSize = new Vector2(300, 174);
        tdp.maxSize = new Vector2(300, 174);
        tdp.EntityOld = entityData;
        tdp.EntityNew = new EntityData(entityData.Entity);
    }

// ReSharper disable UnusedMember.Local
    private void Update()
// ReSharper restore UnusedMember.Local
    {
        if (EditorApplication.isCompiling || (_close && !_isPicking))
        {
            IsRemove();
            Close();
        }
    }

// ReSharper disable UnusedMember.Local
    private void OnGUI()
// ReSharper restore UnusedMember.Local
    {
        if (Event.current.type == EventType.ExecuteCommand)
        {
            _isPicking = false;
            _close = false;
            Focus();
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Object: ", GUILayout.Width(100));
        var rect = GUILayoutUtility.GetRect(new GUIContent(""), EditorStyles.objectField);

        var mouse = Event.current.mousePosition;

        if (Event.current.type == EventType.MouseDown && rect.Contains(mouse))
        {
            _isPicking = true;
        }

        EntityNew.Prefab = EditorGUI.ObjectField(rect, EntityNew.Prefab, typeof (GameObject), false) as GameObject;
        //GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EntityNew.Radius[0] = EditorGUILayout.FloatField("Trunk/Shrub Radius", EntityNew.Radius[0]);
        EntityNew.Radius[1] = EditorGUILayout.FloatField("Canopy Radius", EntityNew.Radius[1]);

        if (EntityNew.Radius[0] >= EntityNew.Radius[1])
            EntityNew.Radius[1] = 0;

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Remove"))
        {
            Remove();
            Close();
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Apply"))
        {
            if (!IsRemove())
            {
                EntityOld.UpdateObject(EntityNew);
            }

            Close();
        }
        GUILayout.EndHorizontal();
    }
    private bool IsRemove()
    {
        if (EntityNew.Prefab == null)
        {
            Remove();
            return true;
        }
        return false;
    }
    private void Remove()
    {
        ProWorld.Data.Entities.Remove(EntityOld);
        ProWorld.Data.World.Entities.Remove(EntityOld.Entity);
    }
// ReSharper disable UnusedMember.Local
    private void OnDestroy()
// ReSharper restore UnusedMember.Local
    {
        IsRemove();
    }
// ReSharper disable UnusedMember.Local
    private void OnLostFocus()
// ReSharper restore UnusedMember.Local
    {
        // Can't close in here as it throws an error so we just set a flag
        // WindowLayouts are invalid. Please use 'Window -> Layouts -> Revert Factory Settings...' menu to fix it.
        _close = true;
    }
// ReSharper disable UnusedMember.Local
    private void OnFocus()
// ReSharper restore UnusedMember.Local
    {
        _close = false;
    }
}