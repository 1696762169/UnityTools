using Codice.CM.SEIDInfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Ԥ����༭��
/// </summary>
public class PrefabTools : EditorWindow
{
    private AssetsFilter assetsFilter;
    private const int SPACE_HEIGHT = 10;

    private Vector2 scrollPosition; // ������λ��
    private bool showHelpBoxes = false; // �Ƿ���ʾ������

    [MenuItem("Tools/Ԥ���������༭��")]
    public static void ShowWindow()
    {
        // ��ʾ�༭������
        GetWindow<PrefabTools>("Ԥ���������༭��");
    }

    private void OnEnable()
    {
        if (assetsFilter == null)
            assetsFilter = CreateInstance<AssetsFilter>();
        assetsFilter.assetType = AssetsFilter.AssetType.Prefab;
    }

    private void OnGUI()
    {
        GUILayout.Label("Ԥ���������༭��", EditorStyles.boldLabel);

        // ��ʼ������ͼ
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // ѡ���ļ�
        assetsFilter.ShowFunction();
        // չʾ����ʵ�ʹ���
        ShowFunctions();

        showHelpBoxes = EditorGUILayout.Toggle("��ʾ������ʾ", showHelpBoxes);
        assetsFilter.showHelpBoxes = showHelpBoxes;

        // ����������ͼ
        EditorGUILayout.EndScrollView();
    }

    private void ShowFunctions()
    {
        // ��ʾ��������
        ShowChangeNameFunctions();

        // ��ʾ��������幦��
        ShowCheckChildFunctions();
        // ��ʾ��������幦��
        ShowAddChildFunctions();
        // ��ʾ�Ƴ������幦��
        ShowRemoveChildFunctions();

        // ��ʾ����������
        ShowCheckComponentFunctions();
        // ��ʾ����������
        ShowAddComponentFunctions();
        // ��ʾ�Ƴ��������
        ShowRemoveComponentFunctions();

        // ��ʾ���ֵ����
        ShowComponentValueFunctions();
        // �����������������

        // ...
    }

    private void ForeachPrefabs(Action<GameObject> fucntion)
    {
        foreach (GameObject prefab in assetsFilter.FindAssetsInFolder<GameObject>(assetsFilter.folderPath, AssetsFilter.AssetType.Prefab))
        {
            fucntion(prefab);
        }
    }

    #region Ԥ�������
    private bool showChangeNameFunctions = false;    // �Ƿ���ʾ��������

    private enum ChangeNameType
    {
        AddAtStart,       // �����ƿ�ͷ���
        AddAtEnd,         // �����ƽ�β���
        RemoveAtStart,    // �Ƴ����ƿ�ͷ�Ĳ���
        RemoveAtEnd,      // �Ƴ����ƽ�β�Ĳ���
        ReplaceEntireName // �滻��������
    }
    private ChangeNameType changeNameType;  // �������Ƶ�����
    private string changeValue = "";    // �������Ƶ�ֵ

    // ��ʾ��������
    private void ShowChangeNameFunctions()
    {
        showChangeNameFunctions = EditorGUILayout.Foldout(showChangeNameFunctions, "�����滻");
        if (!showChangeNameFunctions)
        {
            GUILayout.Space(SPACE_HEIGHT);
            return;
        }

        ShowChangeNameTypeHelpBox();
        changeNameType = (ChangeNameType)EditorGUILayout.EnumPopup("��������", changeNameType);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("����ֵ:", GUILayout.Width(75));
        changeValue = EditorGUILayout.TextField(changeValue);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("ִ���滻", GUILayout.Width(100)))
        {
            ReplacePrefabNames();
        }

        GUILayout.Space(SPACE_HEIGHT);
    }
    private void ShowChangeNameTypeHelpBox()
    {
        if (showHelpBoxes)
        {
            EditorGUILayout.HelpBox("ѡ�����Ԥ�������Ƶ�ģʽ:\n" +
                "AddAtStart: �����ƿ�ͷ�������ֵ\n" +
                "AddAtEnd: �����ƽ�β�������ֵ\n" +
                "RemoveAtStart: �Ƴ����ƿ�ͷ������ֵ\n" +
                "RemoveAtEnd: �Ƴ����ƽ�β������ֵ\n" +
                "ReplaceEntireName: �滻��������Ϊ����ֵ", MessageType.Info);
        }
    }
    // ִ�и�������
    private void ReplacePrefabNames()
    {
        List<GameObject> prefabs = assetsFilter.FindAssetsInFolder<GameObject>(assetsFilter.folderPath, AssetsFilter.AssetType.Prefab);

        Dictionary<string, int> nameCount = new Dictionary<string, int>();

        foreach (GameObject prefab in prefabs)
        {

            string newName = prefab.name;

            switch (changeNameType)
            {
            case ChangeNameType.AddAtStart: // �����ƿ�ͷ���
                newName = changeValue + newName;
                break;
            case ChangeNameType.AddAtEnd:   // �����ƽ�β���
                newName += changeValue;
                break;
            case ChangeNameType.RemoveAtStart:  // �Ƴ����ƿ�ͷ�Ĳ���
                if (newName.StartsWith(changeValue))
                {
                    newName = newName.Substring(changeValue.Length);
                }
                break;
            case ChangeNameType.RemoveAtEnd:    // �Ƴ����ƽ�β�Ĳ���
                if (newName.EndsWith(changeValue))
                {
                    newName = newName.Substring(0, newName.Length - changeValue.Length);
                }
                break;
            case ChangeNameType.ReplaceEntireName:  // �滻��������
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

    #region ���������
    private bool showCheckChildFunctions = false;   // �Ƿ���ʾ��������幦��
    private string childPath = "";  // �Ӷ���·��
    private GameObject foundChild;  // ���ҵ����Ӷ���
    private void ShowCheckChildFunctions()
    {
        showCheckChildFunctions = EditorGUILayout.Foldout(showCheckChildFunctions, "���������");
        if (!showCheckChildFunctions)
        {
            return;
        }

        ShowChildPathSettings();
        if (GUILayout.Button("����Ӷ���"))
            ForeachPrefabs(prefab => CheckChild(prefab));
    }
    private void ShowChildPathSettings()
    {
        ShowChildPathHelpBox();
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("�Ӷ���·��:", GUILayout.Width(75));
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
            Debug.LogWarning($"��Ԥ���塾{prefab.name}����δ�ҵ���Ϊ��{childPath}�����Ӷ���");
            foundChild = null;
            return false;
        }
    }
    private void ShowChildPathHelpBox()
    {
        if (showHelpBoxes)
        {
            EditorGUILayout.HelpBox("����Transform.Find�ĸ�ʽ����������·��\n" +
                "��ֵ��ʾԤ���������", MessageType.Info);
        }
    }
    #endregion
    #region ���������
    private bool showAddChildFunctions = false;   // �Ƿ���ʾ��������幦��
    private string fatherPath;  // �����������ĸ�����·��
    private string childName;   // ����������������
    private Vector3 childPosition;  // ������������λ��
    private Vector3 childRotation;  // ��������������ת
    private Vector3 childScale = new Vector3(1.0f, 1.0f, 1.0f); // ����������������
    private GameObject sourcePrefab;    // ������������ԴԤ����
    private bool createEmptyObject; // �Ƿ񴴽��ն���
    private bool overwriteExistingChild;    // �Ƿ񸲸��Ѵ��ڵ�������

    private void ShowAddChildFunctions()
    {
        showAddChildFunctions = EditorGUILayout.Foldout(showAddChildFunctions, "���������");
        if (!showAddChildFunctions)
        {
            return;
        }

        ShowChildPathSettings();

        childPosition = EditorGUILayout.Vector3Field("λ��:", childPosition);
        childRotation = EditorGUILayout.Vector3Field("��ת:", childRotation);
        childScale = EditorGUILayout.Vector3Field("����:", childScale);

        if (!createEmptyObject)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("ԴԤ����:", GUILayout.Width(75));
            sourcePrefab = (GameObject)EditorGUILayout.ObjectField(sourcePrefab, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();
        }

        createEmptyObject = EditorGUILayout.Toggle("�����ն���:", createEmptyObject);
        overwriteExistingChild = EditorGUILayout.Toggle("�Ѵ���ͬ��������ʱ��Ȼ���:", overwriteExistingChild);

        if (GUILayout.Button("���������"))
        {
            ForeachPrefabs((prefab) => AddChild(prefab));
        }
    }
    private void AddChild(GameObject prefab, bool forceEmpty = false)
    {
        // ����·��
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
            Debug.LogWarning($"��Ԥ���� {prefab.name} ��δ�ҵ�·��Ϊ {fatherPath} �ĸ�����");
            DestroyImmediate(tempPrefabInstance);
            return;
        }

        if (!overwriteExistingChild && parent.Find(childName) != null)
        {
            Debug.LogWarning($"��Ԥ���� {prefab.name} �� {fatherPath} ���Ѵ�����Ϊ {childName} ���Ӷ���");
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
                Debug.LogWarning("��Ҫһ��ԴԤ���������Ϊ�Ӷ���");
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

        Undo.RegisterCreatedObjectUndo(newChild, $"����Ӷ��� {childName} ��Ԥ���� {prefab.name}");
        PrefabUtility.SaveAsPrefabAsset(tempPrefabInstance, AssetDatabase.GetAssetPath(prefab));
        DestroyImmediate(tempPrefabInstance);

        Debug.Log($"�ѳɹ�����Ӷ��� {childName} ��Ԥ���� {prefab.name} �� {fatherPath}");
    }

    #endregion
    #region �������Ƴ�
    private bool showRemoveChildFunctions = false;   // �Ƿ���ʾ�Ƴ������幦��
    private void ShowRemoveChildFunctions()
    {
        showRemoveChildFunctions = EditorGUILayout.Foldout(showRemoveChildFunctions, "�Ƴ�������");
        if (!showRemoveChildFunctions)
        {
            GUILayout.Space(SPACE_HEIGHT);
            return;
        }

        ShowChildPathSettings();
        if (GUILayout.Button("�Ƴ�������"))
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
            Debug.Log($"�ѳɹ�ɾ��Ԥ���� {prefab.name} ����Ϊ {childPath} ��������");
        }
        else
        {
            Debug.LogWarning($"��Ԥ���� {prefab.name} ��δ�ҵ���Ϊ {childPath} ��������");
        }
    }
    #endregion

    #region ������
    private Component chosenComponent;  // ѡ������
    private Component foundComponent;   // �ҵ������
    private bool showCheckComponentFunctions = false;   // �Ƿ���ʾ����������

    // ��ʾ����������
    private void ShowCheckComponentFunctions()
    {
        showCheckComponentFunctions = EditorGUILayout.Foldout(showCheckComponentFunctions, "������");
        if (!showCheckComponentFunctions)
        {
            return;
        }

        ShowComponentSettings();
        
        if (GUILayout.Button("�������Ƿ����", GUILayout.Width(200)))
            ForeachPrefabs((prefab) => CheckComponent(prefab, true));
        if (GUILayout.Button("�������Ƿ񲻴���", GUILayout.Width(200)))
            ForeachPrefabs((prefab) => CheckComponent(prefab, false));
    }
    private void ShowComponentSettings()
    {
        ShowChildPathSettings();
        ShowComponentHelpBox();
        chosenComponent = EditorGUILayout.ObjectField("�������", chosenComponent, typeof(Component), true) as Component;
    }
    private void ShowComponentHelpBox()
    {
        if (showHelpBoxes)
        {
            EditorGUILayout.HelpBox("�϶�����/Ԥ�����ϵ�������˴���������\n" +
                "�󲿷ֹ��ܽ����鲢ʹ����������ͣ�������������Ԥ��ֵ", MessageType.Info);
        }
    }
    // �������Ƿ����
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
            Debug.LogWarning($"�����{chosenComponent.GetType().Name}����Ԥ���塾{prefab.name}���в�����");
            return false;
        }
        else if (foundComponent != null && !shouldExist)
        {
            Debug.LogWarning($"�����{chosenComponent.GetType().Name}����Ԥ���塾{prefab.name}���д���");
            return false;
        }
        return true;
    }
    #endregion
    #region ������
    private bool showAddComponentFunctions = false; // �Ƿ���ʾ����������

    private bool addNewComponentIfSameExists = false;   // ���������ͬ�������Ȼ��������
    private bool removeOldComponentIfSameExists = false;    // ���������ͬ������Ƴ������
    private bool createChildIfNotFound = false; // �Ҳ����Ӷ���ʱ�����Ӷ���
    private bool copyChosenComponent = false;    // �Ƿ������

    // ��ʾ����������
    private void ShowAddComponentFunctions()
    {
        showAddComponentFunctions = EditorGUILayout.Foldout(showAddComponentFunctions, "������");
        if (!showAddComponentFunctions)
        {
            return;
        }

        ShowComponentSettings();

        addNewComponentIfSameExists = EditorGUILayout.Toggle("������ͬ�������������", addNewComponentIfSameExists);
        removeOldComponentIfSameExists = EditorGUILayout.Toggle("������ͬ���ʱ�Ƴ������", removeOldComponentIfSameExists);
        createChildIfNotFound = EditorGUILayout.Toggle("�Ҳ����Ӷ���ʱ�����Ӷ���", createChildIfNotFound);
        copyChosenComponent = EditorGUILayout.Toggle("�����趨�����ֵ", copyChosenComponent);

        if (GUILayout.Button("������", GUILayout.Width(100)))
        {
            if (chosenComponent == null)
                Debug.LogError("��ѡ��������͡�");
            else
                ForeachPrefabs(AddComponent);
        }
    }
    // ��������Ԥ����
    private void AddComponent(GameObject prefab)
    {

        // ���һ򴴽��Ӷ���
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

        // ������
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

            // �����������������
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
    #region �Ƴ����
    private bool showRemoveComponentFunctions = false;   // �Ƿ���ʾ�Ƴ��������
    // ��ʾ�Ƴ��������
    private void ShowRemoveComponentFunctions()
    {
        showRemoveComponentFunctions = EditorGUILayout.Foldout(showRemoveComponentFunctions, "�Ƴ����");
        if (!showRemoveComponentFunctions)
        {
            GUILayout.Space(SPACE_HEIGHT);
            return;
        }

        ShowComponentSettings();
        if (GUILayout.Button("�Ƴ����", GUILayout.Width(100)))
            ForeachPrefabs(RemoveComponentToPrefab);

        GUILayout.Space(SPACE_HEIGHT);
    }
    // �Ƴ����
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
            Debug.Log($"�ѳɹ�ɾ��Ԥ���� {prefab.name} ����Ϊ {childPath} ����� {chosenComponent.GetType().Name}");
        }
        else
        {
            Debug.LogWarning($"��Ԥ���� {prefab.name} ��δ�ҵ���Ϊ {childPath} ����� {chosenComponent.GetType().Name}");
        }
        DestroyImmediate(tempPrefabInstance);
    }
    #endregion

    #region �������ֵ
    private bool showComponentValueFunctions = false;   // �Ƿ���ʾ����е��ض�ֵ����

    private string publicValueName; // ������ֵ����
    private object publicValue; // ������ֵ

    private FieldInfo foundFieldInfo;   // �ҵ����ֶ���Ϣ
    private PropertyInfo foundPropertyInfo; // �ҵ���������Ϣ
    private bool isPublicValueTypeSupported;    // �Ƿ�֧�ּ���ֵ����
    private bool isFieldPublicAndSerializable;  // �ҵ����Ƿ�Ϊ�����ҿ����л����ֶ�
    private bool isPropertyReadable;    // �ҵ����Ƿ�Ϊ�ɶ�����
    private bool isPropertyWritable;    // �ҵ����Ƿ�Ϊ��д����
    // ��ʾ����е��ض�ֵ����
    private void ShowComponentValueFunctions()
    {
        showComponentValueFunctions = EditorGUILayout.Foldout(showComponentValueFunctions, "���ֵ����");
        if (!showComponentValueFunctions)
        {
            GUILayout.Space(SPACE_HEIGHT);
            return;
        }

        // �������
        ShowComponentSettings();

        // ����ֵ
        if (!ShowValueSettings())
        {
            GUILayout.Space(SPACE_HEIGHT);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        if ((foundFieldInfo != null || isPropertyReadable) && GUILayout.Button($"���ֵ�Ƿ�{(isPublicValueTypeSupported ? "���" : "Ϊ��")}"))
            ForeachPrefabs((prefab) => CheckValueEqual(prefab, true));
        if ((foundFieldInfo != null || isPropertyReadable) && GUILayout.Button($"���ֵ�Ƿ�{(isPublicValueTypeSupported ? "���" : "Ϊ��")}"))
            ForeachPrefabs((prefab) => CheckValueEqual(prefab, false));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if ((foundFieldInfo != null || isPropertyWritable) && isPublicValueTypeSupported && GUILayout.Button("����Ϊ����ֵ"))
            ForeachPrefabs((prefab) => SetComponentValue(prefab, false));
        if ((foundFieldInfo != null || isPropertyWritable) && isPublicValueTypeSupported && GUILayout.Button("����Ϊ����ֵ�����ԭֵ��Ĭ��ֵ��"))
            ForeachPrefabs((prefab) => SetComponentValue(prefab, true));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(SPACE_HEIGHT);
    }

    // ������Ҫ��������ֵ
    private bool ShowValueSettings()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("�����ֵ����:", GUILayout.Width(75));
        publicValueName = EditorGUILayout.TextField(publicValueName);
        EditorGUILayout.EndHorizontal();

        // ���û���ҵ����� ����ʾֵ����ѡ��
        if (string.IsNullOrEmpty(publicValueName) || !FindFieldOrProperty())
        {
            return false;
        }
        Type valueType = foundFieldInfo != null ? foundFieldInfo.FieldType : foundPropertyInfo.PropertyType;

        // ���ԭ����ֵΪnull ���� ֵ���Ͳ�ƥ�� ����������Ĭ��ֵ
        if (publicValue == null || publicValue.GetType() != valueType)
            publicValue = valueType.Default();

        if (valueType == typeof(int))
            publicValue = EditorGUILayout.IntField("ֵ:", (int)publicValue);
        else if (valueType == typeof(float))
            publicValue = EditorGUILayout.FloatField("ֵ:", (float)publicValue);
        else if (valueType == typeof(string))
            publicValue = EditorGUILayout.TextField("ֵ:", (string)publicValue);
        else if (valueType == typeof(bool))
            publicValue = EditorGUILayout.Toggle("ֵ:", (bool)publicValue);
        else if (valueType == typeof(Vector3))
            publicValue = EditorGUILayout.Vector3Field("ֵ:", (Vector3)publicValue);
        else if (valueType == typeof(Vector2))
            publicValue = EditorGUILayout.Vector2Field("ֵ:", (Vector2)publicValue);
        else if (valueType == typeof(Vector3Int))
            publicValue = EditorGUILayout.Vector3IntField("ֵ:", (Vector3Int)publicValue);
        else if (valueType == typeof(Vector2Int))
            publicValue = EditorGUILayout.Vector2IntField("ֵ:", (Vector2Int)publicValue);
        else if (valueType == typeof(Color))
            publicValue = EditorGUILayout.ColorField("ֵ:", (Color)publicValue);
        else if (valueType == typeof(Rect))
            publicValue = EditorGUILayout.RectField("ֵ:", (Rect)publicValue);
        else if (valueType == typeof(LayerMask))
            publicValue = EditorGUILayout.LayerField("ֵ:", (LayerMask)publicValue);
        else if (valueType.IsEnum)
            publicValue = EditorGUILayout.EnumPopup("ֵ:", publicValue as Enum);
        else if (valueType.IsNullable())
        {
            publicValue = null;
            isPublicValueTypeSupported = false;
            return true;
        }
        else
        {
            GUILayout.Label($"û��Ϊ����Ϊ��{valueType}����ֵ�ṩ�Ĺ���");
            return false;
        }

        GUILayout.Label($"�������Ϊ��{valueType}����ֵ�Ƿ�ƥ��");
        isPublicValueTypeSupported = true;
        return true;
    }
    // �����ֶλ����� �������ڱ�����
    private bool FindFieldOrProperty()
    {
        if (chosenComponent == null)
        {
            if (publicValueName != null)
                GUILayout.Label("ָ�����������Ϊ��");
            return false;
        }
        BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;
        foundFieldInfo = chosenComponent.GetType().GetField(publicValueName, flags);
        foundPropertyInfo = chosenComponent.GetType().GetProperty(publicValueName, flags);

        if (foundFieldInfo == null && foundPropertyInfo == null)
        {
            GUILayout.Label($"δ�ҵ���Ϊ��{publicValueName}�������л��ֶλ�����");
            return false;
        }

        if (foundFieldInfo != null)
        {
            isFieldPublicAndSerializable = foundFieldInfo.IsPublic || foundFieldInfo.GetCustomAttribute<UnityEngine.SerializeField>() != null;
            if (!isFieldPublicAndSerializable)
            {
                GUILayout.Label($"�ֶΡ�{publicValueName}�����ǹ�������δ���Ϊ�����л�");
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
                GUILayout.Label($"���ԡ�{publicValueName}�����ɶ�����get������");
            }

            if (!isPropertyWritable)
            {
                GUILayout.Label($"���ԡ�{publicValueName}������д����set������");
            }
        }

        return true;
    }

    // ���ֵ�Ƿ����
    private bool CheckValueEqual(GameObject prefab, bool isEqualCheck)
    {
        if (!CheckComponent(prefab))
        {
            Debug.LogWarning($"Ԥ���塾{prefab.name}����û����Ҫ�������");
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
            Debug.LogWarning($"Ԥ���塾{prefab.name}���е�ֵ�����õ�ֵ�����");
            return false;
        }
        else if (!isEqualCheck && isEqual)
        {
            Debug.LogWarning($"Ԥ���塾{prefab.name}���е�ֵ�����õ�ֵ���");
            return false;
        }
        return true;
    }
    // ���������ֵ
    private void SetComponentValue(GameObject prefab, bool ignoreWhenNotDefault)
    {
        if (!CheckComponent(prefab))
        {
            Debug.LogWarning($"Ԥ���塾{prefab.name}����û����Ҫ�������");
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
        Debug.Log($"Ԥ���� {prefab.name} ��ֵ�ѱ�����");
    }
    #endregion
}
