using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NeonSwarm.EditorTools
{
    internal static class Ser
    {
        public static void Set(Object target, string field, object value)
        {
            var serialized = new SerializedObject(target);
            var property = serialized.FindProperty(field);
            if (property == null)
            {
                throw new InvalidOperationException($"{target.GetType().Name}.{field} is not a serialized field");
            }
            Assign(property, value);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void Set(Object target, params (string field, object value)[] values)
        {
            foreach (var (field, value) in values)
            {
                Set(target, field, value);
            }
        }

        private static void Assign(SerializedProperty property, object value)
        {
            switch (value)
            {
                case null:
                    property.objectReferenceValue = null;
                    break;
                case Object reference:
                    property.objectReferenceValue = reference;
                    break;
                case Enum enumValue:
                    property.intValue = Convert.ToInt32(enumValue);
                    break;
                case int number:
                    property.intValue = number;
                    break;
                case float number:
                    property.floatValue = number;
                    break;
                case bool flag:
                    property.boolValue = flag;
                    break;
                case string text:
                    property.stringValue = text;
                    break;
                case Color color:
                    property.colorValue = color;
                    break;
                case Vector2 vector:
                    property.vector2Value = vector;
                    break;
                case Object[] references:
                    property.arraySize = references.Length;
                    for (var i = 0; i < references.Length; i++)
                    {
                        property.GetArrayElementAtIndex(i).objectReferenceValue = references[i];
                    }
                    break;
                default:
                    throw new NotSupportedException($"Unsupported value type {value.GetType().Name} for {property.propertyPath}");
            }
        }
    }
}
