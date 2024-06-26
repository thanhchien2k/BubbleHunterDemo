using UnityEditor;
using UnityEngine;

public class ConfigBase<T> : ScriptableObject where T : ScriptableObject
{
    protected static T instance = null;

    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<T>("Config/" + typeof(T).Name);
            }
//#if UNITY_EDITOR
//            if (instance == null)
//            {
//                string configPath = string.Format("{0}/{1}", "Resources", "Config");
//                if (!System.IO.Directory.Exists(configPath))
//                    System.IO.Directory.CreateDirectory(configPath);

//                instance = CreateAsset(configPath, typeof(T).Name.ToString());
//            }
//#endif
            return instance;
        }
        
    }

    //public static T CreateAsset(string path, string fileName)
    //{
    //    string filePath = string.Format("{0}/{1}.asset", path, fileName);

    //    T asset = ScriptableObject.CreateInstance<T>();

    //    AssetDatabase.CreateAsset(asset, filePath);
    //    SaveAssetsDatabase();

    //    return AssetDatabase.LoadAssetAtPath(filePath, typeof(T)) as T;
    //}

    //public static void SaveAsset(ScriptableObject asset)
    //{
    //    EditorUtility.SetDirty(asset);
    //    SaveAssetsDatabase();
    //}

    //static void SaveAssetsDatabase()
    //{
    //    AssetDatabase.SaveAssets();
    //    AssetDatabase.Refresh();
    //}
}


