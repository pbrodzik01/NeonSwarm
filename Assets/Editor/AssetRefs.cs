using System.Reflection;
using UnityEditor;
using Object = UnityEngine.Object;

namespace NeonSwarm.EditorTools
{
    internal static class AssetRefs
    {
        public static T Fresh<T>(T asset) where T : Object
        {
            if (ReferenceEquals(asset, null) || asset != null)
            {
                return asset;
            }
            return EditorUtility.InstanceIDToObject(asset.GetInstanceID()) as T;
        }

        public static void Refresh(object holder)
        {
            foreach (var field in holder.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (typeof(Object).IsAssignableFrom(field.FieldType) && field.GetValue(holder) is Object value)
                {
                    field.SetValue(holder, Fresh(value));
                }
            }
        }

        public static void Refresh<T>(T[] assets) where T : Object
        {
            for (var i = 0; i < assets.Length; i++)
            {
                assets[i] = Fresh(assets[i]);
            }
        }
    }
}
