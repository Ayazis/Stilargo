using System;
using UnityEditor;

namespace Stilargo.Editor.World
{
    /// <summary>Sets private [SerializeField]s from editor code, the same way the inspector does.</summary>
    public static class SerializedFields
    {
        public static void Assign(UnityEngine.Object target, string field, UnityEngine.Object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), $"Nothing to assign to {target.GetType().Name}.{field}.");
            }

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.GetType().Name} has no serialized field '{field}'.");
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            if (property.objectReferenceValue != value)
            {
                throw new InvalidOperationException($"Could not assign {value} to {target.GetType().Name}.{field}.");
            }
        }

        public static void AssignArray(UnityEngine.Object target, string field, UnityEngine.Object[] values)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null || !property.isArray)
            {
                throw new InvalidOperationException($"{target.GetType().Name} has no serialized array '{field}'.");
            }

            property.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
