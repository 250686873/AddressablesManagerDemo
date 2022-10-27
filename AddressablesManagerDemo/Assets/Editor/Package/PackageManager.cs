using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.IO;
using UnityEditor;

public class PackageManager
{
    /*
        以 Assets/ASSETS_PACKAGE_PATH 目录下的所有文件夹为 AB 包分组，每个 文件夹 下的 子文件(包含子文件夹下的文件) 都会被分到 文件夹 对应的 AB 分组下
    */

    //资源 assets 根目录
    private const string ASSETS_PACKAGE_PATH = "AssetsPackage";

    [MenuItem("Tools/Package/设置 AA 分组")]
    public static void SetAddressableGroup()
    {
        AddressableAssetSettings settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
        Debug.Log(settings.name + " PackageManager 设置 aa 分组");

        //package 文件信息
        var packageDirection = new DirectoryInfo("Assets/" + ASSETS_PACKAGE_PATH);
        //文件夹 aa 包设置
        var packageFiles = packageDirection.GetDirectories();
        //遍历 ASSETS_PACKAGE_PATH 下所有子文件夹
        foreach (var file in packageFiles)
        {
            //文件夹名字作为 AA 分组名
            var groupName = file.Name;
            //文件夹目录
            var path = "Assets/" + ASSETS_PACKAGE_PATH + "/" + file.Name;
            //查询或创建分组
            AddressableAssetGroup tempGroup = settings.FindGroup(groupName);
            if (tempGroup == null)
            {
                tempGroup = settings.CreateGroup(groupName, false, false, true, GetCommonSchema());
            }
            //递归添加 entry
            AddAssetEntry(path, settings, file, tempGroup);
        }
        //检查失效的 group
        var allGroup = settings.groups;
        for (var i = allGroup.Count - 1; i >= 0; i--)
        {
            var curGroup = allGroup[i];
            if (curGroup.entries == null || curGroup.entries.Count <= 0 && !curGroup.IsDefaultGroup())
            {   // 没有等级 entry 并且不为默认分组的组要被移除
                settings.RemoveGroup(curGroup);
            }
        }
    }

    private static void AddAssetEntry(string path, AddressableAssetSettings settings, DirectoryInfo file, AddressableAssetGroup targetGroup)
    {
        //拿到所有子文件夹
        var allFiles = file.GetDirectories();
        foreach (var tempFile in allFiles)
        {
            //递归检查子目录
            AddAssetEntry(path + "/" + tempFile.Name, settings, tempFile, targetGroup);
            //文件登记 entry
            var chileFiles = tempFile.GetFiles();
            foreach (var chileFile in chileFiles)
            {
                //mate 文件不需要划分到 aa 分组之下
                var nameDate = chileFile.Name.Split('.');
                //跳过 meta 文件和 DS_Store 文件
                if (nameDate.Length > 1 && nameDate[nameDate.Length - 1] != "meta" && chileFile.Name != "DS_Store")
                {
                    //通过文件夹路径获得 guid
                    var childFilePath = path + "/" + tempFile.Name + "/" + chileFile.Name;
                    //Debug.Log("aa ChildFile Path: " + childFilePath);
                    var guid = AssetDatabase.AssetPathToGUID(childFilePath);
                    //通过 guid 获得 文件地址登记信息
                    var entry = settings.FindAssetEntry(guid);
                    //如果信息信息 存在 并且所属 aa 分组不为目标分组，等级分组信息
                    if (entry == null || entry.parentGroup != targetGroup)
                    {
                        settings.CreateOrMoveEntry(guid, targetGroup);
                    }
                }
            }
        }
    }

    //创建默认的 Schema
    private static List<AddressableAssetGroupSchema> GetCommonSchema(bool isLocal = true)
    {
        BundledAssetGroupSchema test = new BundledAssetGroupSchema();
        ContentUpdateGroupSchema updateMode = new ContentUpdateGroupSchema();
        List<AddressableAssetGroupSchema> commonSchema = new List<AddressableAssetGroupSchema>();
        //TODO:shangdengma,后续要可以设置远程加载还是本地加载，热更相关
        commonSchema.Add(test);
        //TODO:shangdengma,后续要拓展设置 增量更新 还是 全量更新
        commonSchema.Add(updateMode);
        return commonSchema;
    }
}
