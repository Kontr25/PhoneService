using UnityEditor;
using UnityEngine;

/// <summary>
/// Окно редактирования <see cref="PhonePartsDatabase"/>.
/// </summary>
public sealed class PhonePartsDatabaseEditorWindow : EditorWindow
{
    private const string PrefsGuidKey = "PhoneService.PhonePartsDatabase.Guid";

    private SerializedObject _serializedDatabase;
    private PhonePartsDatabase _database;
    private Vector2 _scroll;

    [MenuItem("Window/Phone Service/Phone Parts Database")]
    private static void Open()
    {
        var window = GetWindow<PhonePartsDatabaseEditorWindow>();
        window.titleContent = new GUIContent("Phone Parts DB");
        window.minSize = new Vector2(420f, 280f);
        window.TryRestoreDatabase();
    }

    private void OnEnable()
    {
        TryRestoreDatabase();
    }

    private void TryRestoreDatabase()
    {
        var guid = EditorPrefs.GetString(PrefsGuidKey, string.Empty);
        if (!string.IsNullOrEmpty(guid))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (!string.IsNullOrEmpty(path))
                _database = AssetDatabase.LoadAssetAtPath<PhonePartsDatabase>(path);
        }

        if (_database == null)
            _database = PhonePartsDatabaseAccess.TryGetForEditor();

        RefreshSerializedObject();
    }

    private void RefreshSerializedObject()
    {
        _serializedDatabase = _database != null ? new SerializedObject(_database) : null;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("База: бренды с моделями + типы запчастей", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Для сборки игры положите asset в папку Resources с именем «PhonePartsDatabase» " +
            $"(см. {nameof(PhonePartsDatabase)}.{nameof(PhonePartsDatabase.ResourcesAssetName)}).",
            MessageType.Info);

        EditorGUILayout.Space(4f);
        EditorGUI.BeginChangeCheck();
        var newDb = (PhonePartsDatabase)EditorGUILayout.ObjectField("Database asset", _database, typeof(PhonePartsDatabase), false);
        if (EditorGUI.EndChangeCheck() && newDb != _database)
        {
            _database = newDb;
            if (_database != null)
                EditorPrefs.SetString(PrefsGuidKey, AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_database)));
            else
                EditorPrefs.DeleteKey(PrefsGuidKey);

            RefreshSerializedObject();
        }

        if (_database == null)
        {
            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Создать Phone Parts Database…"))
            {
                var path = EditorUtility.SaveFilePanelInProject(
                    "Создать Phone Parts Database",
                    PhonePartsDatabase.ResourcesAssetName,
                    "asset",
                    "Выберите путь для asset (рекомендуется Assets/Resources/).");

                if (!string.IsNullOrEmpty(path))
                {
                    var asset = CreateInstance<PhonePartsDatabase>();
                    AssetDatabase.CreateAsset(asset, path);
                    AssetDatabase.SaveAssets();
                    _database = asset;
                    EditorPrefs.SetString(PrefsGuidKey, AssetDatabase.AssetPathToGUID(path));
                    RefreshSerializedObject();
                    Selection.activeObject = asset;
                }
            }

            return;
        }

        if (_serializedDatabase == null || _serializedDatabase.targetObject != _database)
            RefreshSerializedObject();

        _serializedDatabase.Update();

        EditorGUILayout.Space(6f);
        var brandsProp = _serializedDatabase.FindProperty("_brands");
        var partTypesProp = _serializedDatabase.FindProperty("_partTypes");
        if (brandsProp == null || partTypesProp == null)
        {
            EditorGUILayout.HelpBox("Не найдены поля _brands / _partTypes.", MessageType.Error);
            return;
        }

        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUILayout.PropertyField(brandsProp, new GUIContent("Бренды и модели телефонов"), true);
        EditorGUILayout.Space(6f);
        EditorGUILayout.PropertyField(partTypesProp, new GUIContent("Типы запчастей (для всех телефонов)"), true);
        EditorGUILayout.EndScrollView();

        _serializedDatabase.ApplyModifiedProperties();

        EditorGUILayout.Space(8f);
        if (GUILayout.Button("Выбрать asset в Project"))
            Selection.activeObject = _database;
    }
}
