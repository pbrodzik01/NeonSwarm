using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace NeonSwarm.EditorTools
{
    internal static class MixerFactory
    {
        public const string MixerPath = "Assets/Audio/MainMixer.mixer";

        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static AudioMixer Create()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (existing != null && existing.FindMatchingGroups("SFX").Length > 0)
            {
                return existing;
            }
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(MixerPath);
            }

            try
            {
                var editorAssembly = typeof(Editor).Assembly;
                var controllerType = editorAssembly.GetType("UnityEditor.Audio.AudioMixerController", true);
                var groupType = editorAssembly.GetType("UnityEditor.Audio.AudioMixerGroupController", true);
                var groupPathType = editorAssembly.GetType("UnityEditor.Audio.AudioGroupParameterPath", true);

                var mixer = controllerType.GetMethod("CreateMixerControllerAtPath", Flags).Invoke(null, new object[] { MixerPath });
                var master = controllerType.GetProperty("masterGroup", Flags).GetValue(mixer);
                var createGroup = controllerType.GetMethod("CreateNewGroup", Flags);
                var addChild = controllerType.GetMethod("AddChildToParent", Flags);

                var music = createGroup.Invoke(mixer, new object[] { "Music", false });
                addChild.Invoke(mixer, new[] { music, master });
                var sfx = createGroup.Invoke(mixer, new object[] { "SFX", false });
                addChild.Invoke(mixer, new[] { sfx, master });

                Expose(mixer, controllerType, groupType, groupPathType, master, AudioManager.MasterParam);
                Expose(mixer, controllerType, groupType, groupPathType, music, AudioManager.MusicParam);
                Expose(mixer, controllerType, groupType, groupPathType, sfx, AudioManager.SfxParam);

                try
                {
                    var guids = new[] { master, music, sfx }.Select(g => groupType.GetProperty("groupID", Flags).GetValue(g)).ToArray();
                    var guidArray = Array.CreateInstance(typeof(GUID), guids.Length);
                    for (var i = 0; i < guids.Length; i++)
                    {
                        guidArray.SetValue(guids[i], i);
                    }
                    controllerType.GetMethod("SetCurrentViewVisibility", Flags)?.Invoke(mixer, new object[] { guidArray });
                }
                catch (Exception viewException)
                {
                    Debug.Log($"[NeonSwarm] Mixer view visibility skipped: {viewException.GetBaseException().Message}");
                }

                EditorUtility.SetDirty((UnityEngine.Object)mixer);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(MixerPath, ImportAssetOptions.ForceSynchronousImport);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[NeonSwarm] AudioMixer could not be generated automatically: {exception.GetBaseException()}");
                return AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            }

            var result = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            Debug.Log($"[NeonSwarm] AudioMixer created with groups: {string.Join(", ", result.FindMatchingGroups(string.Empty).Select(g => g.name))}");
            return result;
        }

        private static void Expose(object mixer, Type controllerType, Type groupType, Type groupPathType, object group, string parameterName)
        {
            var volumeGuid = groupType.GetMethod("GetGUIDForVolume", Flags).Invoke(group, null);
            var path = Activator.CreateInstance(groupPathType, Flags, null, new[] { group, volumeGuid }, null);
            controllerType.GetMethod("AddExposedParameter", Flags).Invoke(mixer, new[] { path });

            var property = controllerType.GetProperty("exposedParameters", Flags);
            var parameters = (Array)property.GetValue(mixer);
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters.GetValue(i);
                var parameterType = parameter.GetType();
                var guid = parameterType.GetField("guid", Flags).GetValue(parameter);
                if (guid.Equals(volumeGuid))
                {
                    parameterType.GetField("name", Flags).SetValue(parameter, parameterName);
                    parameters.SetValue(parameter, i);
                }
            }
            property.SetValue(mixer, parameters);
        }
    }
}
