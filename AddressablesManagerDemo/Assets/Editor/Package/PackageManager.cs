using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.IO;
using UnityEditor;
public class PackageManager
{

    private const string ASSETS_PACKAGE_PATH = "AssetsPackage";

    //public static LoadImageMgr Instance { get; private set; } = new LoadImageMgr();

    [MenuItem("Tools/Package/设置 AA 分组")]
    public static void SetAddressableGroup()
    {
        AddressableAssetSettings settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
        Debug.Log(settings.name + " PackageManager 设置 aa 分组");

        //package 文件信息
        var packageDirection = new DirectoryInfo("Assets/" + ASSETS_PACKAGE_PATH);
        //文件夹 aa 包设置
        var packageFiles = packageDirection.GetDirectories();
        foreach (var file in packageFiles)
        {
            var groupName = file.Name;
            var path = "Assets/" + ASSETS_PACKAGE_PATH + "/" + file.Name;
            AddressableAssetGroup tempGroup = settings.FindGroup(groupName);
            if (tempGroup == null)
            {
                tempGroup = settings.CreateGroup(groupName, false, false, true, GetCommonSchema());
            }
            AddOneAssetEntry(path, settings, tempGroup);
        }
        //检查失效的 group
        var allGroup = settings.groups;
        for (var i = allGroup.Count - 1; i >= 0; i--)
        {
            var curGroup = allGroup[i];
            if (curGroup.entries == null || curGroup.entries.Count <= 0 && !curGroup.IsDefaultGroup())
            {
                settings.RemoveGroup(curGroup);
            }
        }
    }

    private static void AddOneAssetEntry(string path, AddressableAssetSettings settings, AddressableAssetGroup targetGroup)
    {   
        //通过文件夹路径获得 guid
        var guid = AssetDatabase.AssetPathToGUID(path);
        //通过 guid 获得 文件地址登记信息
        var entry = settings.FindAssetEntry(guid);
        //如果信息信息 存在 并且所属 aa 分组不为目标分组，等级分组信息
        if (entry == null || entry.parentGroup != targetGroup)
        {   
            settings.CreateOrMoveEntry(guid, targetGroup);
        }
    }

    private static List<AddressableAssetGroupSchema> GetCommonSchema(bool isLocal = true)
    {
        List<AddressableAssetGroupSchema> commonSchema = new List<AddressableAssetGroupSchema>();
        BundledAssetGroupSchema test = new BundledAssetGroupSchema();
        ContentUpdateGroupSchema updateMode = new ContentUpdateGroupSchema();
        //TODO:wangxiangtian,后续要可以设置远程加载还是本地加载，热更相关
        commonSchema.Add(test);
        //TODO:wangxiangtian,后续要拓展设置 增量更新 还是 全量更新
        commonSchema.Add(updateMode);
        return commonSchema;
    }
}
