using UnityEngine;
using UnityEditor;

public class WebGLOptimizer : EditorWindow
{
    [MenuItem("Tools/Optimiser pour WebGL")]
    public static void Optimize()
    {
        int texturesChanged = 0;
        int modelsChanged = 0;
        int audiosChanged = 0;

        AssetDatabase.StartAssetEditing();
        try
        {
            // 1. Textures
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" });
            foreach (string guid in textureGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    TextureImporterPlatformSettings webglSettings = importer.GetPlatformTextureSettings("WebGL");
                    if (!webglSettings.overridden || webglSettings.maxTextureSize > 1024)
                    {
                        webglSettings.overridden = true;
                        webglSettings.maxTextureSize = 1024;
                        webglSettings.format = TextureImporterFormat.Automatic;
                        importer.SetPlatformTextureSettings(webglSettings);
                        importer.SaveAndReimport();
                        texturesChanged++;
                    }
                }
            }

            // 2. Modèles
            string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets" });
            foreach (string guid in modelGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer && importer.meshCompression == ModelImporterMeshCompression.Off)
                {
                    importer.meshCompression = ModelImporterMeshCompression.Medium;
                    importer.SaveAndReimport();
                    modelsChanged++;
                }
            }

            // 3. Audio
            string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });
            foreach (string guid in audioGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
                if (importer)
                {
                    AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                    if (settings.loadType == AudioClipLoadType.DecompressOnLoad)
                    {
                        settings.loadType = AudioClipLoadType.CompressedInMemory;
                        importer.defaultSampleSettings = settings;
                        importer.SaveAndReimport();
                        audiosChanged++;
                    }
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        Debug.Log($"Optimisation terminée ! Textures modifiées: {texturesChanged}, Modèles 3D modifiés: {modelsChanged}, Audios modifiés: {audiosChanged}");
    }
}
