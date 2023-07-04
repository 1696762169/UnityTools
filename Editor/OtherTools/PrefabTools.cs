using Codice.CM.SEIDInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 预制体编辑器
/// </summary>
public class PrefabTools : EditorWindow
{
    private AssetsFilter assetsFilter;
    private const int SPACE_HEIGHT = 10;

    private Vector2 scrollPosition; // 滚动条位置
    private bool showHelpBoxes = false; // 是否显示帮助框

    [MenuItem("Tools/预制体批量编辑器")]
    public static void ShowWindow()
    {
        // 显示编辑器窗口
        GetWindow<PrefabTools>("预制体批量编辑器");
    }

    private void OnEnable()
    {
        if (assetsFilter == null)
            assetsFilter = CreateInstance<AssetsFilter>();
        assetsFilter.assetType = AssetsFilter.AssetType.Prefab;
    }

    private void OnGUI()
    {
        GUILayout.Label("预制体批量编辑器", EditorStyles.boldLabel);

        // 开始滚动视图
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 选择文件
        assetsFilter.ShowFunction();
        // 展示所有实际功能
        ShowFunctions();

        showHelpBoxes = EditorGUILayout.Toggle("显示帮助提示", showHelpBoxes);
        assetsFilter.showHelpBoxes = showHelpBoxes;

        // 结束滚动视图
        EditorGUILayout.EndScrollView();
    }

    private void ShowFunctions()
    {
        // 显示更名功能
        ShowChangeNameFunctions();

        // 显示检查子物体功能
        ShowCheckChildFunctions();
        // 显示添加子物体功能
        ShowAddChildFunctions();
        // 显示移除子物体功能
        ShowRemoveChildFunctions();

        // 显示检查组件功能
        ShowCheckComponentFunctions();
        // 显示添加组件功能
        ShowAddComponentFunctions();
        // 显示移除组件功能
        ShowRemoveComponentFunctions();

        // 显示组件值功能
        ShowComponentValueFunctions();
        // 在这里添加其他功能

        // ...
    }

    private void ForeachPrefabs(Action<GameObject> fucntion)
    {
        foreach (GameObject prefab in assetsFilter.FindAssetsInFolder<GameObject>(assetsFilter.folderPath, AssetsFilter.AssetType.Prefab))
        {
            fucntion(prefab);
        }
    }

    #region 预制体更名
    private bool showChangeNameFunctions = false;    // 是否显示更名功能

    private enum ChangeNameType
    {
        AddAtStart,       // 在名称开头添加
        AddAtEnd,         // 在名称结尾添加
        RemoveAtStart,    // 移除名称开头的部分
        RemoveAtEnd,      // 移除名称结尾的部分
        ReplaceEntireName // 替换整个名称
    }
    private ChangeNameType changeNameType;  // 更改名称的类型
    private string changeValue = "";    // 更改名称的值

    // 显示更名功能
    private void ShowChangeNameFunctions()
    {
        showChangeNameFunctions = EditorGUILayout.Foldout(showChangeNameFunctions, "名称替换");
        if (!showChangeNameFunctions)
        {
            GUILayout.Space(SPACE_HEIGHT);
            return;
        }

        ShowChangeNameTypeHelpBox();
        changeNameType = (ChangeNameType)EditorGUILayout.EnumPopup("更改类型", changeNameType);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("更改值:", GUILayout.Width(75));
        changeValue = EditorGUILayout.TextField(changeValue);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("执行替换", GUILayout.Width(100)))
        {
            ReplacePrefabNames();
        }

        GUILayout.Space(SPACE_HEIGHT);
    }
    private void ShowChangeNameTypeHelpBox()
    {
        if (showHelpBoxes)
        {
            EditorGUILayout.HelpBox("选择更改预制体名称的模式:\n" +
                "AddAtStart: 在名称开头添加所设值\n" +
                "AddAtEnd: 在名称结尾添加所设值\n" +
                "RemoveAtStart: 移除名称开头的所设值\n" +
                "RemoveAtEnd: 移除名称结尾的所设值\n" +
                "ReplaceEntireName: 替换整个名称为所设值", MessageType.Info);
        }
    }
    // 执行更名功能
    private void ReplacePrefabNames()
    {
        List<GameObject> prefabs = assetsFilter.FindAssetsInFolder<GameObject>(assetsFilter.folderPath, AssetsFilter.AssetType.Prefab);

        Dictionary<string, int> nameCount = new Dictionary<string, int>();

        foreach (GameObject prefab in prefabs)
        {

            string newName = prefab.name;

            switch (changeNameType)
            {
            case ChangeNameType.AddAtStart: // 在名称开头添加
                newName = changeValue + newName;
                break;
            case ChangeNameType.AddAtEnd:   // 在名称结尾添加
                newName += changeValue;
                break;
            case ChangeNameType.RemoveAtStart:  // 移除名称开头的部分
                if (newName.StartsWith(changeValue))
                {
                    newName = newName.Substring(changeValue.Length);
                }
                break;
            case ChangeNameType.RemoveAtEnd:    // 移除名称结尾的部分
                if (newName.EndsWith(changeValue))
                {
                    newName = newName.Substring(0, newName.Length - changeValue.Length);
                }
                break;
            case ChangeNameType.ReplaceEntireName:  // 替换整个名称
                if (!nameCount.ContainsKey(changeValue))
                {
                    nameCount[changeValue] = 0;
                }
                newName = changeValue + (nameCount[changeValue] > 0 ? $"_{nameCount[changeValue]}" : "");
                nameCount[changeValue]++;
                break;
            }

            string assetPath = AssetDatabase.GetAssetPath(prefab);
            AssetDatabase.RenameAsset(assetPath, newName);
        }
    }
    #endregion

    #region 子物体查找
    private bool showCheckChildFunctions = false;   // 是否显示检查子物体功能
    private string childPath = "";  // 子对象路径
    private GameObject foundChild;  // 查找到的子对象
    private void ShowCheckChildFunctions()
    {
        showCheckChildFunctions = EditorGUILayout.Foldout(showCheckChildFunctions, "检查子物体");
        if (!showCheckChildFunctions)
        {
            return;
        }

        ShowChildPathSettings();
        if (GUILayout.Button("检查子对象"))
            ForeachPrefabs(prefab => CheckChild(prefab));
    }
    private void ShowChildPathSettings()
    {
        ShowChildPathHelpBox();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("子对象路径:", GUILayout.Width(75));
        childPath = EditorGUILayout.TextField(childPath);
        EditorGUILayout.EndHorizontal();
    }
    private bool CheckChild(GameObject prefab)
    {
        if (string.IsNullOrEmpty(childPath))
        {
            foundChild = prefab;
            return true;
        }
        Transform child = prefab.transform.Find(childPath);
        if (child != null)
        {
            foundChild = child.gameObject;
            return true;
        }
        else
        {
            Debug.LogWarning($"在预制体【{prefab.name}】中未找到名为【{childPath}】的子对象");
            foundChild = null;
            return false;
        }
    }
    private void ShowChildPathHelpBox()
    {
        if (showHelpBoxes)
        {
            EditorGUILayout.HelpBox("按照Transform.Find的格式设置子物体路径\n" +
                "空值表示预制体根物体", MessageType.Info);
        }
    }
    #endregion
    #region 子物体添加
    private bool showAddChildFunctions = false;   // 是否显示添加子物体功能
    private string fatherPath;  // 待添加子物体的父物体路径
    private string childName;   // 待添加子物体的名称
    private Vector3 childPosition;  // 待添加子物体的位置
    private Vector3 childRotation;  // 待添加子物体的旋转
    private Vector3 childScale = new Vector3(1.0f, 1.0f, 1.0f); // 待添加子物体的缩放
    private GameObject sourcePrefab;    // 待添加子物体的源预制体
    private bool createEmptyObject; // 是否创建空对象
    private bool overwriteExistingChild;    // 是否覆盖已存在的子物体

    private void ShowAddChildFunctions()
    {
        showAddChildFunctions = EditorGUILayout.Foldout(showAddChildFunctions, "添加子物体");
        if (!showAddChildFunctions)
        {
            return;
        }

        ShowChildPathSettings();

        childPosition = EditorGUILayout.Vector3Field("位置:", childPosition);
        childRotation = EditorGUILayout.Vector3Field("旋转:", childRotation);
        childScale = EditorGUILayout.Vector3Field("缩放:", childScale);

        if (!createEmptyObject)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("源预制体:", GUILayout.Width(75));
            sourcePrefab = (GameObject)EditorGUILayout.ObjectField(sourcePrefab, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();
        }

        createEmptyObject = EditorGUILayout.Toggle("创建空对象:", createEmptyObject);
        overwriteExistingChild = EditorGUILayout.Toggle("已存在同名子物体时仍然添加:", overwriteExistingChild);

        if (GUILayout.Button("添加子物体"))
        {
            ForeachPrefabs((prefab) => AddChild(prefab));
        }
    }
    private void AddChild(GameObject prefab, bool forceEmpty = false)
    {
        // 调整路径
        string[] pathParts = childPath.Split('/');
        if (pathParts.Length > 0)
        {
            childName = pathParts[pathParts.Length - 1];

            if (pathParts.Length > 1)
                fatherPath = string.Join("/", pathParts, 0, pathParts.Length - 1);
            else
                fatherPath = "";
        }
        else
        {
            childName = "";
            fatherPath = "";
        }

        GameObject tempPrefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Transform parent = string.IsNullOrEmpty(fatherPath) ? tempPrefabInstance.transform : tempPrefabInstance.transform.Find(fatherPath);

        if (parent == null)
        {
            Debug.LogWarning($"在预制体 {prefab.name} 中未找到路径为 {fatherPath} 的父对象");
            DestroyImmediate(tempPrefabInstance);
            return;
        }

        if (!overwriteExistingChild && parent.Find(childName) != null)
        {
            Debug.LogWarning($"在预制体 {prefab.name} 的 {fatherPath} 中已存在名为 {childName} 的子对象");
            DestroyImmediate(tempPrefabInstance);
            return;
        }

        GameObject newChild;
        if (createEmptyObject || forceEmpty)
        {
            newChild = new GameObject(childName);
        }
        else
        {
            if (sourcePrefab == null)
            {
                Debug.LogWarning("需要一个源预制体以添加为子对象");
                DestroyImmediate(tempPrefabInstance);
                return;
            }
            newChild = (GameObject)PrefabUtility.InstantiatePrefab(sourcePrefab);
            newChild.name = childName;
        }

        newChild.transform.SetParent(parent, false);
        if (!forceEmpty)
        {
            newChild.transform.localPosition = childPosition;
            newChild.transform.localRotation = Quaternion.Euler(childRotation);
            newChild.transform.localScale = childScale;
        }

        Undo.RegisterCreatedObjectUndo(newChild, $"添加子对象 {childName} 到预制体 {prefab.name}");
        PrefabUtility.SaveAsPrefabAsset(tempPrefabInstance, AssetDatabase.GetAssetPath(prefab));
        DestroyImmediate(tempPrefabInstance);

        Debug.Log($"已成功添加子对象 {childName} 到预制体 {prefab.name} 的 {fatherPath}");
    }

    #endregion
    #region 子物体移除
    private bool showRemoveChildFunctions = false;   // 是否显示移除子物体功能
    private void ShowRemoveChildFunctions()
    {
        showRemoveChildFunctions = EditorGUILayout.Foldout(showRemoveChildFunctions, "移除子物体");
        if (!showRemoveChildFunctions)
        {
            GUILayout.Space(SPACE_HEIGHT);
            return;
        }

        ShowChildPathSettings();
        if (GUILayout.Button("移除子物体"))
            ForeachPrefabs(RemoveChild);

        GUILayout.Space(SPACE_HEIGHT);
    }
    private void RemoveChild(GameObject prefab)
    {
        if (string.IsNullOrEmpty(childPath))
            return;
        Transform child = prefab.transform.Find(childPath);
        if (child != null)
        {
            Undo.DestroyObjectImmediate(child.gameObject);
            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log($"已成功删除预制体 {prefab.name} 中名为 {childPath} 的子物体");
        }
        else
        {
            Debug.LogWarning($"在预制体 {prefab.name} 中未找到名为 {childPath} 的子物体");
        }
    }
    #endregion

    #region 检查组件
    private Component chosenComponent;  // 选择的组件
    private Component foundComponent;   // 找到的组件
    private bool showCheckComponentFunctions = false;   // 是否显示检查组件功能

    // 显示检查组件功能
    private void ShowCheckComponentFunctions()
    {
        showCheckComponentFunctions = EditorGUILayout.Foldout(showCheckComponentFunctions, "检查组件");
        if (!showCheckComponentFunctions)
        {
            return;
        }

        ShowComponentSettings();
        
        if (GUILayout.Button("检查组件是否存在", GUILayout.Width(200)))
            ForeachPrefabs((prefab) => CheckComponent(prefab, true));
        if (GUILayout.Button("检查组件是否不存在", GUILayout.Width(200)))
            ForeachPrefabs((prefab) => CheckComponent(prefab, false));
    }
    private void ShowComponentSettings()
    {
        ShowChildPathSettings();
        ShowComponentHelpBox();
        chosenComponent = EditorGUILayout.ObjectField("组件类型", chosenComponent, typeof(Component), true) as Component;
    }
    private void ShowComponentHelpBox()
    {
        if (showHelpBoxes)
        {
            EditorGUILayout.HelpBox("拖动物体/预制体上的组件到此处进行设置\n" +
                "大部分功能仅会检查并使用组件的类型，无需关心组件的预设值", MessageType.Info);
        }
    }
    // 检查组件是否存在
    private bool CheckComponent(GameObject prefab, bool shouldExist = true)
    {
        if (!CheckChild(prefab))
        {
            foundComponent = null;
            return false;
        }

        Transform targetTransform = foundChild.transform;
        foundComponent = targetTransform.GetComponent(chosenComponent.GetType());
        if (foundComponent == null && shouldExist)
        {
            Debug.LogWarning($"组件【{chosenComponent.GetType().Name}】在预制体【{prefab.name}】中不存在");
            return false;
        }
        else if (foundComponent != null && !shouldExist)
        {
            Debug.LogWarning($"组件【{chosenComponent.GetType().Name}】在预制体【{prefab.name}】中存在");
            return false;
        }
        return true;
    }
    #endregion
    #region 添加组件
    private bool showAddComponentFunctions = false; // 是否显示添加组件功能

    private bool addNewComponentIfSameExists = false;   // 如果存在相同组件，仍然添加新组件
    private bool removeOldComponentIfSameExists = false;    // 如果存在相同组件，移除旧组件
    private bool createChildIfNotFound = false; // 找不到子对象时创建子对象
    private bool copyChosenComponent = false;    // 是否复制组件

    // 显示添加组件功能
    private void ShowAddComponentFunctions()
    {
        showAddComponentFunctions = EditorGUILayout.Foldout(showAddComponentFunctions, "添加组件");
        if (!showAddComponentFunctions)
        {
            return;
        }

        ShowComponentSettings();

        addNewComponentIfSameExists = EditorGUILayout.Toggle("存在相同组件仍添加新组件", addNewComponentIfSameExists);
        removeOldComponentIfSameExists = EditorGUILayout.Toggle("存在相同组件时移除旧组件", removeOldComponentIfSameExists);
        createChildIfNotFound = EditorGUILayout.Toggle("找不到子对象时创建子对象", createChildIfNotFound);
        copyChosenComponent = EditorGUILayout.Toggle("复制设定组件的值", copyChosenComponent);

        if (GUILayout.Button("添加组件", GUILayout.Width(100)))
        {
            if (chosenComponent == null)
                Debug.LogError("请选择组件类型。");
            else
                ForeachPrefabs(AddComponent);
        }
    }
    // 添加组件到预制体
    private void AddComponent(GameObject prefab)
    {

        // 查找或创建子对象
        if (!CheckChild(prefab))
        {
            if (createChildIfNotFound)
            {
                
                AddChild(prefab, true);
            }
            else
            {
                return;
            }
        }
        Transform targetTransform = foundChild.transform;
        Type componentType = chosenComponent.GetType();

        // 添加组件
        if (targetTransform != null)
        {
            Component existingComponent = targetTransform.GetComponent(componentType);

            if (existingComponent != null)
            {
                if (removeOldComponentIfSameExists)
                    DestroyImmediate(existingComponent, true);
                else if (!addNewComponentIfSameExists)
                    return;
            }

            // 复制组件或添加新组件
            if (copyChosenComponent)
            {
                UnityEditorInternal.ComponentUtility.CopyComponent(chosenComponent);
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(targetTransform.gameObject);
            }
            else
            {
                targetTransform.gameObject.AddComponent(componentType);
            }

            PrefabUtility.SavePrefabAsset(prefab);
        }
    }
    #endregion
    #region 移除组件
    private bool showRemoveComponentFunctions = false;   // 是否显示移除组件功能
    // 显示移除组件功能
    private void ShowRemoveComponentFunctions()
    {
        showRemoveComponentFunctions = EditorGUILayout.Foldout(showRemoveComponentFunctions, "移除组件");
        if (!showRemoveComponentFunctions)
        {
            GUILayout.Space(SPACE_HEIGHT);
            return;
        }

        ShowComponentSettings();
        if (GUILayout.Button("移除组件", GUILayout.Width(100)))
            ForeachPrefabs(RemoveComponentToPrefab);

        GUILayout.Space(SPACE_HEIGHT);
    }
    // 移除组件
    private void RemoveComponentToPrefab(GameObject prefab)
    {
        if (!CheckComponent(prefab, true))
            return;

        GameObject tempPrefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        Transform targetTransform = string.IsNullOrEmpty(childPath) ? tempPrefabInstance.transform : tempPrefabInstance.transform.Find(childPath);
        Component component = targetTransform.GetComponent(chosenComponent.GetType());
        if (component != null)
        {
            Undo.DestroyObjectImmediate(component);
            PrefabUtility.SaveAsPrefabAsset(tempPrefabInstance, AssetDatabase.GetAssetPath(prefab));
            Debug.Log($"已成功删除预制体 {prefab.name} 中名为 {childPath} 的组件 {chosenComponent.GetType().Name}");
        }
        else
        {
            Debug.LogWarning($"在预制体 {prefab.name} 中未找到名为 {childPath} 的组件 {chosenComponent.GetType().Name}");
        }
        DestroyImmediate(tempPrefabInstance);
    }
    #endregion

    #region 设置组件值
    private bool showComponentValueFunctions = false;   // 是否显示组件中的特定值功能

    private string publicValueName; // 待检查的值名称
    private object publicValue; // 待检查的值

    private FieldInfo foundFieldInfo;   // 找到的字段信息
    private PropertyInfo foundPropertyInfo; // 找到的属性信息
    private bool isPublicValueTypeSupported;    // 是否支持检查的值类型
    private bool isFieldPublicAndSerializable;  // 找到的是否为公共且可序列化的字段
    private bool isPropertyReadable;    // 找到的是否为可读属性
    private bool isPropertyWritable;    // 找到的是否为可写属性
    // 显示组件中的特定值功能
    private void ShowComponentValueFunctions()
    {
        showComponentValueFunctions = EditorGUILayout.Foldout(showComponentValueFunctions, "组件值操作");
        if (!showComponentValueFunctions)
        {
            GUILayout.Space(SPACE_HEIGHT);
            return;
        }

        // 设置组件
        ShowComponentSettings();

        // 设置值
        if (!ShowValueSettings())
        {
            GUILayout.Space(SPACE_HEIGHT);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        if ((foundFieldInfo != null || isPropertyReadable) && GUILayout.Button($"检查值是否{(isPublicValueTypeSupported ? "相等" : "为空")}"))
            ForeachPrefabs((prefab) => CheckValueEqual(prefab, true));
        if ((foundFieldInfo != null || isPropertyReadable) && GUILayout.Button($"检查值是否不{(isPublicValueTypeSupported ? "相等" : "为空")}"))
            ForeachPrefabs((prefab) => CheckValueEqual(prefab, false));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if ((foundFieldInfo != null || isPropertyWritable) && isPublicValueTypeSupported && GUILayout.Button("设置为以上值"))
            ForeachPrefabs((prefab) => SetComponentValue(prefab, false));
        if ((foundFieldInfo != null || isPropertyWritable) && isPublicValueTypeSupported && GUILayout.Button("设置为以上值（如果原值是默认值）"))
            ForeachPrefabs((prefab) => SetComponentValue(prefab, true));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(SPACE_HEIGHT);
    }

    // 设置需要检查组件的值
    private bool ShowValueSettings()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("待检查值名称:", GUILayout.Width(75));
        publicValueName = EditorGUILayout.TextField(publicValueName);
        EditorGUILayout.EndHorizontal();

        // 如果没有找到变量 则不显示值类型选择
        if (string.IsNullOrEmpty(publicValueName) || !FindFieldOrProperty())
        {
            return false;
        }
        Type valueType = foundFieldInfo != null ? foundFieldInfo.FieldType : foundPropertyInfo.PropertyType;

        // 如果原来的值为null 或者 值类型不匹配 则重新设置默认值
        if (publicValue == null || publicValue.GetType() != valueType)
            publicValue = valueType.Default();

        if (valueType == typeof(int))
            publicValue = EditorGUILayout.IntField("值:", (int)publicValue);
        else if (valueType == typeof(float))
            publicValue = EditorGUILayout.FloatField("值:", (float)publicValue);
        else if (valueType == typeof(string))
            publicValue = EditorGUILayout.TextField("值:", (string)publicValue);
        else if (valueType == typeof(bool))
            publicValue = EditorGUILayout.Toggle("值:", (bool)publicValue);
        else if (valueType == typeof(Vector3))
            publicValue = EditorGUILayout.Vector3Field("值:", (Vector3)publicValue);
        else if (valueType == typeof(Vector2))
            publicValue = EditorGUILayout.Vector2Field("值:", (Vector2)publicValue);
        else if (valueType == typeof(Vector3Int))
            publicValue = EditorGUILayout.Vector3IntField("值:", (Vector3Int)publicValue);
        else if (valueType == typeof(Vector2Int))
            publicValue = EditorGUILayout.Vector2IntField("值:", (Vector2Int)publicValue);
        else if (valueType == typeof(Color))
            publicValue = EditorGUILayout.ColorField("值:", (Color)publicValue);
        else if (valueType == typeof(Rect))
            publicValue = EditorGUILayout.RectField("值:", (Rect)publicValue);
        else if (valueType == typeof(LayerMask))
            publicValue = EditorGUILayout.LayerField("值:", (LayerMask)publicValue);
        else if (valueType.IsEnum)
            publicValue = EditorGUILayout.EnumPopup("值:", publicValue as Enum);
        else if (valueType.IsNullable())
        {
            publicValue = null;
            isPublicValueTypeSupported = false;
            return true;
        }
        else
        {
            GUILayout.Label($"没有为类型为【{valueType}】的值提供的功能");
            return false;
        }

        GUILayout.Label($"检查类型为【{valueType}】的值是否匹配");
        isPublicValueTypeSupported = true;
        return true;
    }
    // 查找字段或属性 并保存在变量中
    private bool FindFieldOrProperty()
    {
        if (chosenComponent == null)
        {
            if (publicValueName != null)
                GUILayout.Label("指定的组件类型为空");
            return false;
        }
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
        foundFieldInfo = chosenComponent.GetType().GetField(publicValueName, flags);
        foundPropertyInfo = chosenComponent.GetType().GetProperty(publicValueName, flags);

        if (foundFieldInfo == null && foundPropertyInfo == null)
        {
            GUILayout.Label($"未找到名为【{publicValueName}】的序列化字段或属性");
            return false;
        }

        if (foundFieldInfo != null)
        {
            isFieldPublicAndSerializable = foundFieldInfo.IsPublic || foundFieldInfo.GetCustomAttribute<UnityEngine.SerializeField>() != null;
            if (!isFieldPublicAndSerializable)
            {
                GUILayout.Label($"字段【{publicValueName}】不是公共的且未标记为可序列化");
            }
        }

        if (foundPropertyInfo != null)
        {
            MethodInfo getMethod = foundPropertyInfo.GetGetMethod(nonPublic: true);
            MethodInfo setMethod = foundPropertyInfo.GetSetMethod(nonPublic: true);
            isPropertyReadable = getMethod != null;
            isPropertyWritable = setMethod != null;

            if (!isPropertyReadable)
            {
                GUILayout.Label($"属性【{publicValueName}】不可读（无get方法）");
            }

            if (!isPropertyWritable)
            {
                GUILayout.Label($"属性【{publicValueName}】不可写（无set方法）");
            }
        }

        return true;
    }

    // 检查值是否相等
    private bool CheckValueEqual(GameObject prefab, bool isEqualCheck)
    {
        if (!CheckComponent(prefab))
        {
            Debug.LogWarning($"预制体【{prefab.name}】中没有所要检查的组件");
            return false;
        }

        if (foundComponent == null)
            return false;

        object currentValue = null;
        if (foundFieldInfo != null)
        {
            currentValue = foundFieldInfo.GetValue(foundComponent);
        }
        else if (foundPropertyInfo != null && foundPropertyInfo.CanRead)
        {
            currentValue = foundPropertyInfo.GetValue(foundComponent, null);
        }

        bool isEqual = Equals(currentValue, publicValue);
        if (isEqualCheck && !isEqual)
        {
            Debug.LogWarning($"预制体【{prefab.name}】中的值与设置的值不相等");
            return false;
        }
        else if (!isEqualCheck && isEqual)
        {
            Debug.LogWarning($"预制体【{prefab.name}】中的值与设置的值相等");
            return false;
        }
        return true;
    }
    // 设置组件的值
    private void SetComponentValue(GameObject prefab, bool ignoreWhenNotDefault)
    {
        if (!CheckComponent(prefab))
        {
            Debug.LogWarning($"预制体【{prefab.name}】中没有所要检查的组件");
            return;
        }
        if (foundComponent == null)
            return;

        object currentValue;
        if (foundFieldInfo != null)
        {
            currentValue = foundFieldInfo.GetValue(foundComponent);
            if (ignoreWhenNotDefault && !Equals(currentValue, foundFieldInfo.FieldType.Default()) || Equals(currentValue, publicValue))
                return;
            foundFieldInfo.SetValue(foundComponent, publicValue);
        }
        else if (foundPropertyInfo != null && foundPropertyInfo.CanWrite)
        {
            currentValue = foundPropertyInfo.GetValue(foundComponent);
            if (ignoreWhenNotDefault && !Equals(currentValue, foundPropertyInfo.PropertyType.Default()) || Equals(currentValue, publicValue))
                return;
            foundPropertyInfo.SetValue(foundComponent, publicValue, null);
        }

        PrefabUtility.SavePrefabAsset(prefab);
        Debug.Log($"预制体 {prefab.name} 的值已被设置");
    }
    #endregion
}
