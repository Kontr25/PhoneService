#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Окно редактирования базы запчастей в режиме master-detail.
/// </summary>
public sealed class PhonePartsDatabaseEditorWindow : EditorWindow
{
    /// <summary>
    /// Ключ EditorPrefs для последнего выбранного asset.
    /// </summary>
    private const string PrefsGuidKey = "PhoneService.PhonePartsDatabase.Guid";

    /// <summary>
    /// Стандартная ширина одной списковой колонки.
    /// </summary>
    private const float ListColumnWidth = 130f;

    /// <summary>
    /// Стандартная ширина правой панели деталей.
    /// </summary>
    private const float DetailPanelWidth = 320f;

    /// <summary>
    /// Отступ между колонками.
    /// </summary>
    private const float ColumnSpacing = 1f;

    /// <summary>
    /// Высота области основных панелей.
    /// </summary>
    private const float PanelAreaHeight = 430f;

    /// <summary>
    /// Высота заголовка колонки (кнопки +/- и подпись) над списком.
    /// </summary>
    private const float ColumnChromeHeight = 48f;

    /// <summary>
    /// Суммарный горизонтальный отступ внутри ScrollView (padding box).
    /// </summary>
    private const float ScrollViewInnerPadding = 12f;

    /// <summary>
    /// Цвет фона кнопок добавления.
    /// </summary>
    private static readonly Color AddButtonColor = new Color(0.4f, 0.85f, 0.4f);

    /// <summary>
    /// Цвет фона кнопок удаления.
    /// </summary>
    private static readonly Color RemoveButtonColor = new Color(0.95f, 0.35f, 0.3f);

    /// <summary>
    /// Цвет фона выделенного элемента в списке.
    /// </summary>
    private static readonly Color SelectedItemColor = new Color(1f, 0.6f, 0.15f);

    /// <summary>
    /// Цвет фона кнопки сохранения.
    /// </summary>
    private static readonly Color SaveButtonColor = new Color(0.3f, 0.85f, 0.8f);

    /// <summary>
    /// Режим правой панели деталей.
    /// </summary>
    private enum DetailMode
    {
        /// <summary>
        /// Редактируется запись запчасти.
        /// </summary>
        PartRecord,

        /// <summary>
        /// Редактируется категория.
        /// </summary>
        Category,

        /// <summary>
        /// Редактируется телефон.
        /// </summary>
        Phone,

        /// <summary>
        /// Редактируется модель выбранного телефона.
        /// </summary>
        PhoneModel
    }

    /// <summary>
    /// Сериализованный объект базы.
    /// </summary>
    private SerializedObject _serializedDatabase;

    /// <summary>
    /// Активный asset базы.
    /// </summary>
    private PhonePartsDatabase _database;

    /// <summary>
    /// Текущий выбранный индекс категории.
    /// </summary>
    private int _selectedCategoryIndex = -1;

    /// <summary>
    /// Текущий выбранный индекс телефона.
    /// </summary>
    private int _selectedPhoneIndex = -1;

    /// <summary>
    /// Текущий выбранный индекс модели в списке моделей выбранного телефона.
    /// </summary>
    private int _selectedModelIndex = -1;

    /// <summary>
    /// Текущий выбранный индекс записи запчасти (в полном массиве _partRecords).
    /// </summary>
    private int _selectedPartRecordIndex = -1;

    /// <summary>
    /// Текущий режим правой панели.
    /// </summary>
    private DetailMode _detailMode = DetailMode.PartRecord;

    /// <summary>
    /// Признак несохраненных изменений.
    /// </summary>
    private bool _isDirty;

    /// <summary>
    /// Позиция прокрутки колонки категорий.
    /// </summary>
    private Vector2 _categoryScroll;

    /// <summary>
    /// Позиция прокрутки колонки телефонов.
    /// </summary>
    private Vector2 _phoneScroll;

    /// <summary>
    /// Позиция прокрутки колонки моделей телефона.
    /// </summary>
    private Vector2 _modelScroll;

    /// <summary>
    /// Позиция прокрутки колонки запчастей.
    /// </summary>
    private Vector2 _partScroll;

    /// <summary>
    /// Позиция прокрутки панели редактирования.
    /// </summary>
    private Vector2 _detailScroll;

    /// <summary>
    /// Позиция горизонтальной прокрутки общей строки панелей.
    /// </summary>
    private Vector2 _mainPanelsScroll;

    /// <summary>
    /// Открывает окно редактора базы.
    /// </summary>
    [MenuItem("Window/Phone Service/Phone Parts Database (NEW)")]
    private static void Open()
    {
        var window = GetWindow<PhonePartsDatabaseEditorWindow>();
        window.titleContent = new GUIContent("Phone Parts DB [NEW]");
        window.minSize = new Vector2(600f, 420f);
        window.TryRestoreDatabase();
    }

    /// <summary>
    /// Восстанавливает состояние окна при активации.
    /// </summary>
    private void OnEnable()
    {
        TryRestoreDatabase();
    }

    /// <summary>
    /// Отрисовывает интерфейс окна.
    /// </summary>
    private void OnGUI()
    {
        DrawHeaderAndAssetPicker();
        if (!EnsureDatabaseReady())
            return;

        _serializedDatabase.Update();
        var phoneCatalogProp = _serializedDatabase.FindProperty("_phoneCatalog");
        var partCategoriesProp = _serializedDatabase.FindProperty("_partCategories");
        var partRecordsProp = _serializedDatabase.FindProperty("_partRecords");
        var validMatProp = _serializedDatabase.FindProperty("_validSlotPreviewMaterial");
        var invalidMatProp = _serializedDatabase.FindProperty("_invalidSlotPreviewMaterial");
        if (phoneCatalogProp == null || partCategoriesProp == null || partRecordsProp == null ||
            validMatProp == null || invalidMatProp == null)
        {
            EditorGUILayout.HelpBox("Не найдены необходимые поля базы данных.", MessageType.Error);
            return;
        }

        DrawPreviewMaterials(validMatProp, invalidMatProp);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        DrawSaveButton();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(8f);
        DrawMainPanels(partCategoriesProp, phoneCatalogProp, partRecordsProp);

        RebuildRecordIds(partRecordsProp);
        if (_serializedDatabase.ApplyModifiedProperties())
            _isDirty = true;
    }

    /// <summary>
    /// Рисует пять панелей в одну строку внутри горизонтального ScrollView.
    /// </summary>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <param name="partRecordsProp">Массив записей.</param>
    private void DrawMainPanels(
        SerializedProperty partCategoriesProp,
        SerializedProperty phoneCatalogProp,
        SerializedProperty partRecordsProp)
    {
        var totalWidth = 4f * ListColumnWidth + DetailPanelWidth + 4f * ColumnSpacing;
        _mainPanelsScroll = EditorGUILayout.BeginScrollView(
            _mainPanelsScroll,
            true,
            false,
            GUILayout.Height(PanelAreaHeight + 18f));

        EditorGUILayout.BeginHorizontal(GUILayout.Height(PanelAreaHeight));
        DrawCategoryColumn(partCategoriesProp, partRecordsProp, ListColumnWidth, PanelAreaHeight - ColumnChromeHeight);
        GUILayout.Space(ColumnSpacing);
        DrawPhoneColumn(phoneCatalogProp, partRecordsProp, ListColumnWidth, PanelAreaHeight - ColumnChromeHeight);
        GUILayout.Space(ColumnSpacing);
        DrawModelColumn(phoneCatalogProp, ListColumnWidth, PanelAreaHeight - ColumnChromeHeight);
        GUILayout.Space(ColumnSpacing);
        DrawPartColumn(partRecordsProp, partCategoriesProp, phoneCatalogProp, ListColumnWidth, PanelAreaHeight - ColumnChromeHeight);
        GUILayout.Space(ColumnSpacing);
        DrawDetailPanel(partCategoriesProp, phoneCatalogProp, partRecordsProp, DetailPanelWidth);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// Восстанавливает сохраненный asset базы.
    /// </summary>
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

    /// <summary>
    /// Пересоздает сериализованную обертку базы.
    /// </summary>
    private void RefreshSerializedObject()
    {
        _serializedDatabase = _database != null ? new SerializedObject(_database) : null;
    }

    /// <summary>
    /// Рисует заголовок окна и выбор asset базы.
    /// </summary>
    private void DrawHeaderAndAssetPicker()
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("База: категории, телефоны и запчасти", EditorStyles.boldLabel);
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

            _selectedCategoryIndex = -1;
            _selectedPhoneIndex = -1;
            _selectedModelIndex = -1;
            _selectedPartRecordIndex = -1;
            _isDirty = false;
            RefreshSerializedObject();
        }
    }

    /// <summary>
    /// Проверяет, что база готова к редактированию.
    /// </summary>
    /// <returns>True, если база доступна.</returns>
    private bool EnsureDatabaseReady()
    {
        if (_database == null)
        {
            DrawCreateDatabaseSection();
            return false;
        }

        if (_serializedDatabase == null || _serializedDatabase.targetObject != _database)
            RefreshSerializedObject();

        return _serializedDatabase != null;
    }

    /// <summary>
    /// Рисует блок создания базы, если asset отсутствует.
    /// </summary>
    private void DrawCreateDatabaseSection()
    {
        EditorGUILayout.Space(8f);
        if (!GUILayout.Button("Создать Phone Parts Database..."))
            return;

        var path = EditorUtility.SaveFilePanelInProject(
            "Создать Phone Parts Database",
            PhonePartsDatabase.ResourcesAssetName,
            "asset",
            "Выберите путь для asset (рекомендуется Assets/Resources/).");
        if (string.IsNullOrEmpty(path))
            return;

        var asset = CreateInstance<PhonePartsDatabase>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        _database = asset;
        EditorPrefs.SetString(PrefsGuidKey, AssetDatabase.AssetPathToGUID(path));
        _isDirty = false;
        RefreshSerializedObject();
        Selection.activeObject = asset;
    }

    /// <summary>
    /// Рисует строку кнопок добавления и удаления над колонкой списка.
    /// </summary>
    /// <param name="add">Действие по кнопке «+».</param>
    /// <param name="remove">Действие по кнопке «-».</param>
    private void DrawColumnToolbar(System.Action add, System.Action remove)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
        if (DrawMiniToolbarButton("+", AddButtonColor))
            add?.Invoke();
        if (DrawMiniToolbarButton("-", RemoveButtonColor))
            remove?.Invoke();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Рисует компактную кнопку для строки над колонкой.
    /// </summary>
    /// <param name="label">Подпись.</param>
    /// <param name="color">Цвет фона.</param>
    /// <returns>True при нажатии.</returns>
    private static bool DrawMiniToolbarButton(string label, Color color)
    {
        var oldBg = GUI.backgroundColor;
        GUI.backgroundColor = color;
        var clicked = GUILayout.Button(label, GUILayout.Width(22f), GUILayout.Height(18f));
        GUI.backgroundColor = oldBg;
        return clicked;
    }

    /// <summary>
    /// Рисует кнопку сохранения с подсветкой dirty состояния.
    /// </summary>
    private void DrawSaveButton()
    {
        var oldBg = GUI.backgroundColor;
        GUI.backgroundColor = SaveButtonColor;
        if (GUILayout.Button("СОХРАНИТЬ", EditorStyles.toolbarButton, GUILayout.Width(110f)))
            SaveDatabase();
        GUI.backgroundColor = oldBg;
    }

    /// <summary>
    /// Рисует секцию материалов превью.
    /// </summary>
    /// <param name="validMatProp">Материал корректного превью.</param>
    /// <param name="invalidMatProp">Материал некорректного превью.</param>
    private static void DrawPreviewMaterials(SerializedProperty validMatProp, SerializedProperty invalidMatProp)
    {
        EditorGUILayout.LabelField("Материалы превью установки", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(validMatProp, new GUIContent("Подходит"));
        EditorGUILayout.PropertyField(invalidMatProp, new GUIContent("Не подходит"));
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// Рисует колонку категорий как ScrollView с жёсткой шириной.
    /// </summary>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    /// <param name="partRecordsProp">Массив записей запчастей.</param>
    /// <param name="width">Ширина колонки.</param>
    /// <param name="scrollHeight">Высота области прокрутки списка.</param>
    private void DrawCategoryColumn(
        SerializedProperty partCategoriesProp,
        SerializedProperty partRecordsProp,
        float width,
        float scrollHeight)
    {
        EditorGUILayout.BeginVertical(
            GUILayout.MinWidth(width),
            GUILayout.MaxWidth(width),
            GUILayout.Width(width),
            GUILayout.ExpandWidth(false));
        DrawColumnToolbar(
            () => AddCategory(partCategoriesProp),
            () => RemoveCategory(partCategoriesProp, partRecordsProp));
        EditorGUILayout.LabelField("Категории", EditorStyles.boldLabel, GUILayout.Width(width));
        _categoryScroll = EditorGUILayout.BeginScrollView(
            _categoryScroll, false, false,
            GUIStyle.none, GUI.skin.verticalScrollbar, "box",
            GUILayout.Width(width), GUILayout.Height(scrollHeight));
        var btnWidth = Mathf.Max(20f, width - ScrollViewInnerPadding);
        for (var i = 0; i < partCategoriesProp.arraySize; i++)
        {
            var entry = partCategoriesProp.GetArrayElementAtIndex(i);
            var idProp = entry.FindPropertyRelative("_categoryId");
            var name = idProp != null && !string.IsNullOrWhiteSpace(idProp.stringValue)
                ? idProp.stringValue
                : $"Категория {i + 1}";
            if (!DrawSelectionButton(name, _selectedCategoryIndex == i, btnWidth))
                continue;

            _selectedCategoryIndex = i;
            _selectedPhoneIndex = -1;
            _selectedModelIndex = -1;
            _selectedPartRecordIndex = -1;
            _detailMode = DetailMode.Category;
            Repaint();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Рисует колонку телефонов как ScrollView с жёсткой шириной.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <param name="partRecordsProp">Массив записей запчастей.</param>
    /// <param name="width">Ширина колонки.</param>
    /// <param name="scrollHeight">Высота области прокрутки списка.</param>
    private void DrawPhoneColumn(
        SerializedProperty phoneCatalogProp,
        SerializedProperty partRecordsProp,
        float width,
        float scrollHeight)
    {
        EditorGUILayout.BeginVertical(
            GUILayout.MinWidth(width),
            GUILayout.MaxWidth(width),
            GUILayout.Width(width),
            GUILayout.ExpandWidth(false));
        DrawColumnToolbar(
            () => AddPhone(phoneCatalogProp),
            () => RemovePhone(phoneCatalogProp, partRecordsProp));
        EditorGUILayout.LabelField("Телефоны", EditorStyles.boldLabel, GUILayout.Width(width));
        _phoneScroll = EditorGUILayout.BeginScrollView(
            _phoneScroll, false, false,
            GUIStyle.none, GUI.skin.verticalScrollbar, "box",
            GUILayout.Width(width), GUILayout.Height(scrollHeight));
        var btnWidth = Mathf.Max(20f, width - ScrollViewInnerPadding);
        for (var i = 0; i < phoneCatalogProp.arraySize; i++)
        {
            var entry = phoneCatalogProp.GetArrayElementAtIndex(i);
            var nameProp = entry.FindPropertyRelative("_phoneName");
            var name = nameProp != null && !string.IsNullOrWhiteSpace(nameProp.stringValue)
                ? nameProp.stringValue
                : $"Телефон {i + 1}";
            if (!DrawSelectionButton(name, _selectedPhoneIndex == i, btnWidth))
                continue;

            _selectedPhoneIndex = i;
            _selectedModelIndex = -1;
            _selectedPartRecordIndex = -1;
            _detailMode = DetailMode.Phone;
            Repaint();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Рисует колонку моделей выбранного телефона.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <param name="width">Ширина колонки.</param>
    /// <param name="scrollHeight">Высота области прокрутки списка.</param>
    private void DrawModelColumn(SerializedProperty phoneCatalogProp, float width, float scrollHeight)
    {
        EditorGUILayout.BeginVertical(
            GUILayout.MinWidth(width),
            GUILayout.MaxWidth(width),
            GUILayout.Width(width),
            GUILayout.ExpandWidth(false));
        DrawColumnToolbar(
            () => AddPhoneModel(phoneCatalogProp),
            () => RemovePhoneModel(phoneCatalogProp));
        EditorGUILayout.LabelField("Модели", EditorStyles.boldLabel, GUILayout.Width(width));
        _modelScroll = EditorGUILayout.BeginScrollView(
            _modelScroll, false, false,
            GUIStyle.none, GUI.skin.verticalScrollbar, "box",
            GUILayout.Width(width), GUILayout.Height(scrollHeight));
        var btnWidth = Mathf.Max(20f, width - ScrollViewInnerPadding);
        if (phoneCatalogProp.arraySize == 0 || _selectedPhoneIndex < 0)
        {
            EditorGUILayout.LabelField("Телефон не выбран", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            return;
        }

        _selectedPhoneIndex = Mathf.Clamp(_selectedPhoneIndex, 0, phoneCatalogProp.arraySize - 1);
        var phone = phoneCatalogProp.GetArrayElementAtIndex(_selectedPhoneIndex);
        var models = phone.FindPropertyRelative("_phoneModels");
        for (var i = 0; i < models.arraySize; i++)
        {
            var modelProp = models.GetArrayElementAtIndex(i);
            var modelNameProp = modelProp.FindPropertyRelative("_modelName");
            var label = modelNameProp != null && !string.IsNullOrWhiteSpace(modelNameProp.stringValue)
                ? modelNameProp.stringValue
                : $"Модель {i + 1}";
            if (!DrawSelectionButton(label, _selectedModelIndex == i, btnWidth))
                continue;

            _selectedModelIndex = i;
            _selectedPartRecordIndex = -1;
            _detailMode = DetailMode.PhoneModel;
            Repaint();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Рисует колонку запчастей как ScrollView с жёсткой шириной.
    /// </summary>
    /// <param name="partRecordsProp">Массив записей запчастей.</param>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <param name="width">Ширина колонки.</param>
    /// <param name="scrollHeight">Высота области прокрутки списка.</param>
    private void DrawPartColumn(
        SerializedProperty partRecordsProp,
        SerializedProperty partCategoriesProp,
        SerializedProperty phoneCatalogProp,
        float width,
        float scrollHeight)
    {
        var selectedCategoryId = GetSelectedCategoryId(partCategoriesProp);
        var selectedPhoneName = GetSelectedPhoneName(phoneCatalogProp);
        var selectedModelName = GetSelectedModelName(phoneCatalogProp);
        var hasModelPick = _selectedPhoneIndex >= 0 && _selectedModelIndex >= 0 &&
                           !string.IsNullOrWhiteSpace(selectedModelName);
        var filteredIndices = CollectFilteredPartRecordIndices(
            partRecordsProp,
            _selectedCategoryIndex >= 0,
            selectedCategoryId,
            _selectedPhoneIndex >= 0,
            selectedPhoneName,
            hasModelPick,
            selectedModelName);
        if (!filteredIndices.Contains(_selectedPartRecordIndex))
            _selectedPartRecordIndex = -1;

        EditorGUILayout.BeginVertical(
            GUILayout.MinWidth(width),
            GUILayout.MaxWidth(width),
            GUILayout.Width(width),
            GUILayout.ExpandWidth(false));
        DrawColumnToolbar(
            () => AddPartRecord(partRecordsProp, partCategoriesProp, phoneCatalogProp),
            () => RemovePartRecord(partRecordsProp));
        EditorGUILayout.LabelField("Запчасти", EditorStyles.boldLabel, GUILayout.Width(width));
        _partScroll = EditorGUILayout.BeginScrollView(
            _partScroll, false, false,
            GUIStyle.none, GUI.skin.verticalScrollbar, "box",
            GUILayout.Width(width), GUILayout.Height(scrollHeight));
        var btnWidth = Mathf.Max(20f, width - ScrollViewInnerPadding);
        for (var i = 0; i < filteredIndices.Count; i++)
        {
            var fullIndex = filteredIndices[i];
            var record = partRecordsProp.GetArrayElementAtIndex(fullIndex);
            var model = record.FindPropertyRelative("_phoneModelName")?.stringValue ?? string.Empty;
            var qualityIndex = record.FindPropertyRelative("_partQualityType")?.enumValueIndex ?? 0;
            var quality = ((PartQualityType)Mathf.Clamp(qualityIndex, 0, 2)).ToString();
            var label = $"{model} | {quality}";
            if (!DrawSelectionButton(label, _selectedPartRecordIndex == fullIndex, btnWidth))
                continue;

            _selectedPartRecordIndex = fullIndex;
            _detailMode = DetailMode.PartRecord;
            Repaint();
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Рисует кнопку списка с визуальным выделением и явной шириной.
    /// </summary>
    /// <param name="label">Подпись строки.</param>
    /// <param name="isSelected">Признак выбранной строки.</param>
    /// <param name="buttonWidth">Ширина кнопки.</param>
    /// <returns>True, если кнопка была нажата.</returns>
    private static bool DrawSelectionButton(string label, bool isSelected, float buttonWidth)
    {
        var oldColor = GUI.backgroundColor;
        if (isSelected)
            GUI.backgroundColor = SelectedItemColor;

        var clicked = GUILayout.Button(label, GUILayout.Height(20f), GUILayout.Width(buttonWidth));
        GUI.backgroundColor = oldColor;
        return clicked;
    }

    /// <summary>
    /// Рисует правую панель редактирования выбранного объекта как ScrollView.
    /// </summary>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <param name="partRecordsProp">Массив записей запчастей.</param>
    /// <param name="width">Ширина панели.</param>
    private void DrawDetailPanel(
        SerializedProperty partCategoriesProp,
        SerializedProperty phoneCatalogProp,
        SerializedProperty partRecordsProp, 
        float width)
    {
        EditorGUILayout.BeginVertical(
            GUILayout.MinWidth(width),
            GUILayout.MaxWidth(width),
            GUILayout.Width(width),
            GUILayout.ExpandWidth(false));

        _detailScroll = EditorGUILayout.BeginScrollView(
            _detailScroll, false, false,
            GUIStyle.none, GUI.skin.verticalScrollbar, "box",
            GUILayout.Width(width), GUILayout.Height(PanelAreaHeight));

        float innerWidth = width - 26f; // запас под скроллбар + внутренние отступы box

        EditorGUILayout.BeginVertical(GUILayout.Width(innerWidth), GUILayout.ExpandWidth(false));

        EditorGUILayout.LabelField(
            "Редактор выбранного элемента",
            EditorStyles.boldLabel,
            GUILayout.Width(innerWidth));

        switch (_detailMode)
        {
            case DetailMode.Category:
                DrawSelectedCategoryDetails(partCategoriesProp);
                break;
            case DetailMode.Phone:
                DrawSelectedPhoneDetails(phoneCatalogProp);
                break;
            case DetailMode.PhoneModel:
                DrawSelectedPhoneModelDetails(phoneCatalogProp);
                break;
            default:
                DrawSelectedPartRecordDetails(partRecordsProp, partCategoriesProp, phoneCatalogProp);
                break;
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// Рисует редактор выбранной категории.
    /// </summary>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    private void DrawSelectedCategoryDetails(SerializedProperty partCategoriesProp)
    {
        if (partCategoriesProp.arraySize == 0 || _selectedCategoryIndex < 0)
        {
            EditorGUILayout.HelpBox("Выберите категорию в первой колонке.", MessageType.Info);
            return;
        }

        _selectedCategoryIndex = Mathf.Clamp(_selectedCategoryIndex, 0, partCategoriesProp.arraySize - 1);
        var category = partCategoriesProp.GetArrayElementAtIndex(_selectedCategoryIndex);
        EditorGUILayout.PropertyField(category.FindPropertyRelative("_categoryId"), new GUIContent("Category Id"));
        EditorGUILayout.PropertyField(category.FindPropertyRelative("_displayName"), new GUIContent("Display Name"));
        EditorGUILayout.PropertyField(category.FindPropertyRelative("_icon"), new GUIContent("Иконка"));
    }

    /// <summary>
    /// Рисует редактор выбранного телефона.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    private void DrawSelectedPhoneDetails(SerializedProperty phoneCatalogProp)
    {
        if (phoneCatalogProp.arraySize == 0 || _selectedPhoneIndex < 0)
        {
            EditorGUILayout.HelpBox("Выберите телефон в колонке «Телефоны».", MessageType.Info);
            return;
        }

        _selectedPhoneIndex = Mathf.Clamp(_selectedPhoneIndex, 0, phoneCatalogProp.arraySize - 1);
        var phone = phoneCatalogProp.GetArrayElementAtIndex(_selectedPhoneIndex);
        EditorGUILayout.PropertyField(phone.FindPropertyRelative("_phoneName"), new GUIContent("Phone Name"));
        EditorGUILayout.PropertyField(phone.FindPropertyRelative("_displayName"), new GUIContent("Display Name"));
    }

    /// <summary>
    /// Рисует редактор выбранной модели телефона.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    private void DrawSelectedPhoneModelDetails(SerializedProperty phoneCatalogProp)
    {
        if (phoneCatalogProp.arraySize == 0 || _selectedPhoneIndex < 0 || _selectedModelIndex < 0)
        {
            EditorGUILayout.HelpBox("Выберите модель в колонке «Модели».", MessageType.Info);
            return;
        }

        _selectedPhoneIndex = Mathf.Clamp(_selectedPhoneIndex, 0, phoneCatalogProp.arraySize - 1);
        var phone = phoneCatalogProp.GetArrayElementAtIndex(_selectedPhoneIndex);
        var models = phone.FindPropertyRelative("_phoneModels");
        if (models.arraySize == 0)
        {
            EditorGUILayout.HelpBox("У телефона нет моделей. Добавьте строку кнопкой «+» над колонкой.", MessageType.Info);
            return;
        }

        _selectedModelIndex = Mathf.Clamp(_selectedModelIndex, 0, models.arraySize - 1);
        var model = models.GetArrayElementAtIndex(_selectedModelIndex);
        EditorGUILayout.PropertyField(model.FindPropertyRelative("_modelName"), new GUIContent("Model Name"));
        EditorGUILayout.PropertyField(model.FindPropertyRelative("_mesh"), new GUIContent("Меш"));
        EditorGUILayout.PropertyField(model.FindPropertyRelative("_material"), new GUIContent("Материал"));
    }

    /// <summary>
    /// Рисует редактор выбранной записи запчасти.
    /// </summary>
    /// <param name="partRecordsProp">Массив записей запчастей.</param>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    private void DrawSelectedPartRecordDetails(
        SerializedProperty partRecordsProp,
        SerializedProperty partCategoriesProp,
        SerializedProperty phoneCatalogProp)
    {
        if (_selectedPartRecordIndex < 0 || _selectedPartRecordIndex >= partRecordsProp.arraySize)
        {
            EditorGUILayout.HelpBox("Выберите запись запчасти в колонке «Запчасти».", MessageType.Info, true);
            return;
        }

        var record = partRecordsProp.GetArrayElementAtIndex(_selectedPartRecordIndex);
        var categoryIdProp = record.FindPropertyRelative("_partCategoryId");
        var phoneNameProp = record.FindPropertyRelative("_phoneName");
        var phoneModelProp = record.FindPropertyRelative("_phoneModelName");
        var qualityProp = record.FindPropertyRelative("_partQualityType");
        var costProp = record.FindPropertyRelative("_cost");
        var descriptionProp = record.FindPropertyRelative("_description");
        var prefabProp = record.FindPropertyRelative("_partPrefab");
        var meshProp = record.FindPropertyRelative("_partMesh");
        var materialProp = record.FindPropertyRelative("_partMaterial");
        var recordIdProp = record.FindPropertyRelative("_recordId");

        DrawStringPopup("Категория", CollectCategoryIds(partCategoriesProp), categoryIdProp);
        DrawStringPopup("Название телефона", CollectPhoneNames(phoneCatalogProp), phoneNameProp);
        DrawStringPopup("Модель", CollectModelsForPhone(phoneCatalogProp, phoneNameProp.stringValue), phoneModelProp);
        EditorGUILayout.PropertyField(qualityProp, new GUIContent("Тип запчасти"));
        EditorGUILayout.PropertyField(costProp, new GUIContent("Стоимость"));
        EditorGUILayout.PropertyField(descriptionProp, new GUIContent("Описание"));
        EditorGUILayout.PropertyField(prefabProp, new GUIContent("Префаб"));
        EditorGUILayout.PropertyField(meshProp, new GUIContent("Меш"));
        EditorGUILayout.PropertyField(materialProp, new GUIContent("Материал"));
        using (new EditorGUI.DisabledScope(true))
            EditorGUILayout.TextField("Record Id", recordIdProp.stringValue);
    }

    /// <summary>
    /// Добавляет новую категорию.
    /// </summary>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    private void AddCategory(SerializedProperty partCategoriesProp)
    {
        partCategoriesProp.arraySize++;
        var index = partCategoriesProp.arraySize - 1;
        var entry = partCategoriesProp.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("_categoryId").stringValue = BuildUniqueCategoryId(partCategoriesProp, "new_category");
        entry.FindPropertyRelative("_displayName").stringValue = "Новая категория";
        _selectedCategoryIndex = index;
        _selectedPartRecordIndex = -1;
        _detailMode = DetailMode.Category;
    }

    /// <summary>
    /// Удаляет выбранную категорию и связанные записи запчастей.
    /// </summary>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    /// <param name="partRecordsProp">Массив записей запчастей.</param>
    private void RemoveCategory(SerializedProperty partCategoriesProp, SerializedProperty partRecordsProp)
    {
        if (partCategoriesProp.arraySize == 0)
            return;

        _selectedCategoryIndex = Mathf.Clamp(_selectedCategoryIndex, 0, partCategoriesProp.arraySize - 1);
        var removedCategoryId = partCategoriesProp.GetArrayElementAtIndex(_selectedCategoryIndex)
            .FindPropertyRelative("_categoryId").stringValue;
        RemoveRecordsByField(partRecordsProp, "_partCategoryId", removedCategoryId);
        partCategoriesProp.DeleteArrayElementAtIndex(_selectedCategoryIndex);
        _selectedCategoryIndex = partCategoriesProp.arraySize > 0
            ? Mathf.Clamp(_selectedCategoryIndex - 1, 0, partCategoriesProp.arraySize - 1)
            : -1;
        _selectedPartRecordIndex = -1;
    }

    /// <summary>
    /// Добавляет новый телефон.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    private void AddPhone(SerializedProperty phoneCatalogProp)
    {
        phoneCatalogProp.arraySize++;
        var index = phoneCatalogProp.arraySize - 1;
        var entry = phoneCatalogProp.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("_phoneName").stringValue = BuildUniquePhoneName(phoneCatalogProp, "new_phone");
        entry.FindPropertyRelative("_displayName").stringValue = "Новый телефон";
        var models = entry.FindPropertyRelative("_phoneModels");
        models.arraySize = 1;
        var firstModel = models.GetArrayElementAtIndex(0);
        firstModel.FindPropertyRelative("_modelName").stringValue = "Model1";
        _selectedPhoneIndex = index;
        _selectedModelIndex = 0;
        _selectedPartRecordIndex = -1;
        _detailMode = DetailMode.Phone;
    }

    /// <summary>
    /// Удаляет выбранный телефон и связанные записи запчастей.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <param name="partRecordsProp">Массив записей запчастей.</param>
    private void RemovePhone(SerializedProperty phoneCatalogProp, SerializedProperty partRecordsProp)
    {
        if (phoneCatalogProp.arraySize == 0)
            return;

        _selectedPhoneIndex = Mathf.Clamp(_selectedPhoneIndex, 0, phoneCatalogProp.arraySize - 1);
        var removedPhoneName = phoneCatalogProp.GetArrayElementAtIndex(_selectedPhoneIndex)
            .FindPropertyRelative("_phoneName").stringValue;
        RemoveRecordsByField(partRecordsProp, "_phoneName", removedPhoneName);
        phoneCatalogProp.DeleteArrayElementAtIndex(_selectedPhoneIndex);
        _selectedPhoneIndex = phoneCatalogProp.arraySize > 0
            ? Mathf.Clamp(_selectedPhoneIndex - 1, 0, phoneCatalogProp.arraySize - 1)
            : -1;
        _selectedModelIndex = -1;
        _selectedPartRecordIndex = -1;
    }

    /// <summary>
    /// Добавляет модель к выбранному телефону.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    private void AddPhoneModel(SerializedProperty phoneCatalogProp)
    {
        if (phoneCatalogProp.arraySize == 0 || _selectedPhoneIndex < 0)
            return;

        _selectedPhoneIndex = Mathf.Clamp(_selectedPhoneIndex, 0, phoneCatalogProp.arraySize - 1);
        var phone = phoneCatalogProp.GetArrayElementAtIndex(_selectedPhoneIndex);
        var models = phone.FindPropertyRelative("_phoneModels");
        models.arraySize++;
        var newIndex = models.arraySize - 1;
        var el = models.GetArrayElementAtIndex(newIndex);
        el.FindPropertyRelative("_modelName").stringValue = BuildUniqueModelName(models, "model");
        _selectedModelIndex = newIndex;
        _selectedPartRecordIndex = -1;
        _detailMode = DetailMode.PhoneModel;
    }

    /// <summary>
    /// Удаляет выбранную модель у выбранного телефона.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    private void RemovePhoneModel(SerializedProperty phoneCatalogProp)
    {
        if (phoneCatalogProp.arraySize == 0 || _selectedPhoneIndex < 0)
            return;

        _selectedPhoneIndex = Mathf.Clamp(_selectedPhoneIndex, 0, phoneCatalogProp.arraySize - 1);
        var phone = phoneCatalogProp.GetArrayElementAtIndex(_selectedPhoneIndex);
        var models = phone.FindPropertyRelative("_phoneModels");
        if (models.arraySize == 0)
            return;

        _selectedModelIndex = Mathf.Clamp(_selectedModelIndex, 0, models.arraySize - 1);
        models.DeleteArrayElementAtIndex(_selectedModelIndex);

        _selectedModelIndex = models.arraySize > 0
            ? Mathf.Clamp(_selectedModelIndex - 1, 0, models.arraySize - 1)
            : -1;
        _selectedPartRecordIndex = -1;
        _detailMode = _selectedModelIndex >= 0 ? DetailMode.PhoneModel : DetailMode.Phone;
    }

    /// <summary>
    /// Добавляет запись запчасти для выбранных категории и телефона.
    /// </summary>
    /// <param name="partRecordsProp">Массив записей запчастей.</param>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    private void AddPartRecord(
        SerializedProperty partRecordsProp,
        SerializedProperty partCategoriesProp,
        SerializedProperty phoneCatalogProp)
    {
        if (partCategoriesProp.arraySize == 0 || phoneCatalogProp.arraySize == 0)
            return;

        if (_selectedCategoryIndex < 0)
            _selectedCategoryIndex = 0;
        if (_selectedPhoneIndex < 0)
            _selectedPhoneIndex = 0;

        var categoryId = GetSelectedCategoryId(partCategoriesProp);
        var phoneName = GetSelectedPhoneName(phoneCatalogProp);
        var model = GetDefaultModelForNewPart(phoneCatalogProp, phoneName);

        partRecordsProp.arraySize++;
        var index = partRecordsProp.arraySize - 1;
        var record = partRecordsProp.GetArrayElementAtIndex(index);
        record.FindPropertyRelative("_partCategoryId").stringValue = categoryId;
        record.FindPropertyRelative("_phoneName").stringValue = phoneName;
        record.FindPropertyRelative("_phoneModelName").stringValue = model;
        record.FindPropertyRelative("_partQualityType").enumValueIndex = (int)PartQualityType.Original;
        record.FindPropertyRelative("_cost").intValue = 0;
        record.FindPropertyRelative("_description").stringValue = string.Empty;
        _selectedPartRecordIndex = index;
        _detailMode = DetailMode.PartRecord;
    }

    /// <summary>
    /// Удаляет выбранную запись запчасти.
    /// </summary>
    /// <param name="partRecordsProp">Массив записей запчастей.</param>
    private void RemovePartRecord(SerializedProperty partRecordsProp)
    {
        if (_selectedPartRecordIndex < 0 || _selectedPartRecordIndex >= partRecordsProp.arraySize)
            return;

        partRecordsProp.DeleteArrayElementAtIndex(_selectedPartRecordIndex);
        _selectedPartRecordIndex = -1;
    }

    /// <summary>
    /// Сохраняет базу на диск.
    /// </summary>
    private void SaveDatabase()
    {
        _serializedDatabase.ApplyModifiedProperties();
        EditorUtility.SetDirty(_database);
        AssetDatabase.SaveAssets();
        _isDirty = false;
    }

    /// <summary>
    /// Удаляет записи запчастей по совпадающему строковому полю.
    /// </summary>
    /// <param name="partRecordsProp">Массив записей.</param>
    /// <param name="fieldName">Имя сериализованного поля записи.</param>
    /// <param name="fieldValue">Значение для удаления.</param>
    private static void RemoveRecordsByField(SerializedProperty partRecordsProp, string fieldName, string fieldValue)
    {
        for (var i = partRecordsProp.arraySize - 1; i >= 0; i--)
        {
            var value = partRecordsProp.GetArrayElementAtIndex(i).FindPropertyRelative(fieldName).stringValue;
            if (!string.Equals(value, fieldValue))
                continue;

            partRecordsProp.DeleteArrayElementAtIndex(i);
        }
    }

    /// <summary>
    /// Возвращает id выбранной категории.
    /// </summary>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    /// <returns>Id категории или пустая строка.</returns>
    private string GetSelectedCategoryId(SerializedProperty partCategoriesProp)
    {
        if (partCategoriesProp.arraySize == 0 || _selectedCategoryIndex < 0)
            return string.Empty;

        _selectedCategoryIndex = Mathf.Clamp(_selectedCategoryIndex, 0, partCategoriesProp.arraySize - 1);
        return partCategoriesProp.GetArrayElementAtIndex(_selectedCategoryIndex).FindPropertyRelative("_categoryId").stringValue;
    }

    /// <summary>
    /// Возвращает имя выбранного телефона.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <returns>Имя телефона или пустая строка.</returns>
    private string GetSelectedPhoneName(SerializedProperty phoneCatalogProp)
    {
        if (phoneCatalogProp.arraySize == 0 || _selectedPhoneIndex < 0)
            return string.Empty;

        _selectedPhoneIndex = Mathf.Clamp(_selectedPhoneIndex, 0, phoneCatalogProp.arraySize - 1);
        return phoneCatalogProp.GetArrayElementAtIndex(_selectedPhoneIndex).FindPropertyRelative("_phoneName").stringValue;
    }

    /// <summary>
    /// Возвращает имя выбранной в колонке модели (если выбраны телефон и строка модели).
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <returns>Имя модели или пустая строка.</returns>
    private string GetSelectedModelName(SerializedProperty phoneCatalogProp)
    {
        if (phoneCatalogProp.arraySize == 0 || _selectedPhoneIndex < 0 || _selectedModelIndex < 0)
            return string.Empty;

        _selectedPhoneIndex = Mathf.Clamp(_selectedPhoneIndex, 0, phoneCatalogProp.arraySize - 1);
        var phone = phoneCatalogProp.GetArrayElementAtIndex(_selectedPhoneIndex);
        var models = phone.FindPropertyRelative("_phoneModels");
        if (models.arraySize == 0)
            return string.Empty;

        _selectedModelIndex = Mathf.Clamp(_selectedModelIndex, 0, models.arraySize - 1);
        var nameProp = models.GetArrayElementAtIndex(_selectedModelIndex).FindPropertyRelative("_modelName");
        return nameProp != null ? nameProp.stringValue : string.Empty;
    }

    /// <summary>
    /// Возвращает модель по умолчанию для новой записи запчасти: выбранная в колонке или первая у телефона.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <param name="phoneName">Имя телефона.</param>
    /// <returns>Имя модели.</returns>
    private string GetDefaultModelForNewPart(SerializedProperty phoneCatalogProp, string phoneName)
    {
        var selected = GetSelectedModelName(phoneCatalogProp);
        if (!string.IsNullOrWhiteSpace(selected))
            return selected.Trim();

        return GetFirstModelForPhone(phoneCatalogProp, phoneName);
    }

    /// <summary>
    /// Возвращает первую модель для телефона.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <param name="phoneName">Имя телефона.</param>
    /// <returns>Имя модели или пустая строка.</returns>
    private static string GetFirstModelForPhone(SerializedProperty phoneCatalogProp, string phoneName)
    {
        for (var i = 0; i < phoneCatalogProp.arraySize; i++)
        {
            var phone = phoneCatalogProp.GetArrayElementAtIndex(i);
            if (!string.Equals(phone.FindPropertyRelative("_phoneName").stringValue, phoneName))
                continue;

            var models = phone.FindPropertyRelative("_phoneModels");
            if (models.arraySize == 0)
                return string.Empty;

            var nameProp = models.GetArrayElementAtIndex(0).FindPropertyRelative("_modelName");
            return nameProp != null ? nameProp.stringValue : string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// Собирает список индексов записей, подходящих под фильтр категории и телефона.
    /// </summary>
    /// <param name="partRecordsProp">Массив записей запчастей.</param>
    /// <param name="categoryId">Id категории.</param>
    /// <param name="phoneName">Имя телефона.</param>
    /// <param name="hasModelFilter">Фильтровать по модели телефона.</param>
    /// <param name="phoneModelName">Имя модели для фильтра.</param>
    /// <returns>Список индексов записей.</returns>
    private static List<int> CollectFilteredPartRecordIndices(
        SerializedProperty partRecordsProp,
        bool hasCategoryFilter,
        string categoryId,
        bool hasPhoneFilter,
        string phoneName,
        bool hasModelFilter,
        string phoneModelName)
    {
        var result = new List<int>();
        var modelKey = string.IsNullOrWhiteSpace(phoneModelName) ? string.Empty : phoneModelName.Trim();
        for (var i = 0; i < partRecordsProp.arraySize; i++)
        {
            var record = partRecordsProp.GetArrayElementAtIndex(i);
            var recordCategory = record.FindPropertyRelative("_partCategoryId").stringValue;
            var recordPhone = record.FindPropertyRelative("_phoneName").stringValue;
            if (hasCategoryFilter && !string.Equals(recordCategory, categoryId))
                continue;

            if (hasPhoneFilter && !string.Equals(recordPhone, phoneName))
                continue;

            if (hasModelFilter)
            {
                var recordModel = record.FindPropertyRelative("_phoneModelName").stringValue;
                if (!string.Equals(
                        string.IsNullOrWhiteSpace(recordModel) ? string.Empty : recordModel.Trim(),
                        modelKey,
                        StringComparison.Ordinal))
                    continue;
            }

            result.Add(i);
        }

        return result;
    }

    /// <summary>
    /// Рисует popup для строкового значения.
    /// </summary>
    /// <param name="label">Подпись поля.</param>
    /// <param name="items">Список значений.</param>
    /// <param name="targetProp">Целевое свойство.</param>
    private static void DrawStringPopup(string label, IReadOnlyList<string> items, SerializedProperty targetProp)
    {
        if (items.Count == 0)
        {
            targetProp.stringValue = string.Empty;
            EditorGUILayout.TextField(label, string.Empty);
            return;
        }

        var currentIndex = 0;
        for (var i = 0; i < items.Count; i++)
        {
            if (!string.Equals(items[i], targetProp.stringValue))
                continue;

            currentIndex = i;
            break;
        }

        var newIndex = EditorGUILayout.Popup(label, currentIndex, ToDisplayOptions(items));
        targetProp.stringValue = items[Mathf.Clamp(newIndex, 0, items.Count - 1)];
    }

    /// <summary>
    /// Собирает список id категорий.
    /// </summary>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    /// <returns>Список id категорий.</returns>
    private static List<string> CollectCategoryIds(SerializedProperty partCategoriesProp)
    {
        var result = new List<string>();
        for (var i = 0; i < partCategoriesProp.arraySize; i++)
        {
            var value = partCategoriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("_categoryId").stringValue;
            if (string.IsNullOrWhiteSpace(value))
                continue;

            result.Add(value.Trim());
        }

        if (result.Count == 0)
            result.Add(string.Empty);

        return result;
    }

    /// <summary>
    /// Собирает список имен телефонов.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <returns>Список имен телефонов.</returns>
    private static List<string> CollectPhoneNames(SerializedProperty phoneCatalogProp)
    {
        var result = new List<string>();
        for (var i = 0; i < phoneCatalogProp.arraySize; i++)
        {
            var value = phoneCatalogProp.GetArrayElementAtIndex(i).FindPropertyRelative("_phoneName").stringValue;
            if (string.IsNullOrWhiteSpace(value))
                continue;

            result.Add(value.Trim());
        }

        if (result.Count == 0)
            result.Add(string.Empty);

        return result;
    }

    /// <summary>
    /// Собирает модели выбранного телефона.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <param name="phoneName">Имя телефона.</param>
    /// <returns>Список моделей.</returns>
    private static List<string> CollectModelsForPhone(SerializedProperty phoneCatalogProp, string phoneName)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(phoneName))
        {
            result.Add(string.Empty);
            return result;
        }

        for (var i = 0; i < phoneCatalogProp.arraySize; i++)
        {
            var phone = phoneCatalogProp.GetArrayElementAtIndex(i);
            var name = phone.FindPropertyRelative("_phoneName").stringValue;
            if (!string.Equals(name, phoneName))
                continue;

            var models = phone.FindPropertyRelative("_phoneModels");
            for (var m = 0; m < models.arraySize; m++)
            {
                var modelProp = models.GetArrayElementAtIndex(m).FindPropertyRelative("_modelName");
                var model = modelProp != null ? modelProp.stringValue : string.Empty;
                if (!string.IsNullOrWhiteSpace(model))
                    result.Add(model.Trim());
            }

            break;
        }

        if (result.Count == 0)
            result.Add(string.Empty);

        return result;
    }

    /// <summary>
    /// Строит уникальный id категории.
    /// </summary>
    /// <param name="partCategoriesProp">Массив категорий.</param>
    /// <param name="baseValue">Базовое значение.</param>
    /// <returns>Уникальный id.</returns>
    private static string BuildUniqueCategoryId(SerializedProperty partCategoriesProp, string baseValue)
    {
        var used = new HashSet<string>();
        for (var i = 0; i < partCategoriesProp.arraySize; i++)
        {
            var value = partCategoriesProp.GetArrayElementAtIndex(i).FindPropertyRelative("_categoryId").stringValue;
            if (!string.IsNullOrWhiteSpace(value))
                used.Add(value.Trim());
        }

        return BuildUniqueString(used, baseValue);
    }

    /// <summary>
    /// Строит уникальное имя телефона.
    /// </summary>
    /// <param name="phoneCatalogProp">Массив телефонов.</param>
    /// <param name="baseValue">Базовое значение.</param>
    /// <returns>Уникальное имя.</returns>
    private static string BuildUniquePhoneName(SerializedProperty phoneCatalogProp, string baseValue)
    {
        var used = new HashSet<string>();
        for (var i = 0; i < phoneCatalogProp.arraySize; i++)
        {
            var value = phoneCatalogProp.GetArrayElementAtIndex(i).FindPropertyRelative("_phoneName").stringValue;
            if (!string.IsNullOrWhiteSpace(value))
                used.Add(value.Trim());
        }

        return BuildUniqueString(used, baseValue);
    }

    /// <summary>
    /// Строит уникальное имя модели в пределах списка моделей телефона.
    /// </summary>
    /// <param name="modelsProp">Массив моделей телефона.</param>
    /// <param name="baseValue">Базовое значение.</param>
    /// <returns>Уникальное имя модели.</returns>
    private static string BuildUniqueModelName(SerializedProperty modelsProp, string baseValue)
    {
        var used = new HashSet<string>();
        for (var i = 0; i < modelsProp.arraySize; i++)
        {
            var value = modelsProp.GetArrayElementAtIndex(i).FindPropertyRelative("_modelName").stringValue;
            if (!string.IsNullOrWhiteSpace(value))
                used.Add(value.Trim());
        }

        return BuildUniqueString(used, baseValue);
    }

    /// <summary>
    /// Возвращает уникальную строку относительно набора занятых значений.
    /// </summary>
    /// <param name="used">Набор уже занятых значений.</param>
    /// <param name="baseValue">Базовое значение.</param>
    /// <returns>Уникальное значение.</returns>
    private static string BuildUniqueString(HashSet<string> used, string baseValue)
    {
        var candidate = baseValue;
        var suffix = 2;
        while (!used.Add(candidate))
        {
            candidate = $"{baseValue}_{suffix}";
            suffix++;
        }

        return candidate;
    }

    /// <summary>
    /// Пересчитывает и гарантирует уникальность id записей запчастей.
    /// </summary>
    /// <param name="partRecordsProp">Массив записей.</param>
    private static void RebuildRecordIds(SerializedProperty partRecordsProp)
    {
        var used = new HashSet<string>();
        for (var i = 0; i < partRecordsProp.arraySize; i++)
        {
            var record = partRecordsProp.GetArrayElementAtIndex(i);
            var recordIdProp = record.FindPropertyRelative("_recordId");
            var categoryId = record.FindPropertyRelative("_partCategoryId").stringValue;
            var phoneName = record.FindPropertyRelative("_phoneName").stringValue;
            var phoneModel = record.FindPropertyRelative("_phoneModelName").stringValue;
            var qualityIndex = record.FindPropertyRelative("_partQualityType").enumValueIndex;
            var quality = ((PartQualityType)Mathf.Clamp(qualityIndex, 0, 2)).ToString();
            var baseId = BuildBaseRecordId(categoryId, phoneName, phoneModel, quality);
            var candidate = baseId;
            var suffix = 2;
            while (!used.Add(candidate))
            {
                candidate = $"{baseId}_{suffix}";
                suffix++;
            }

            recordIdProp.stringValue = candidate;
        }
    }

    /// <summary>
    /// Формирует базовый id записи запчасти.
    /// </summary>
    /// <param name="categoryId">Id категории.</param>
    /// <param name="phoneName">Имя телефона.</param>
    /// <param name="phoneModel">Модель.</param>
    /// <param name="qualityName">Качество.</param>
    /// <returns>Базовый id.</returns>
    private static string BuildBaseRecordId(string categoryId, string phoneName, string phoneModel, string qualityName)
    {
        var category = SanitizeIdPart(categoryId);
        var phone = SanitizeIdPart(phoneName);
        var model = SanitizeIdPart(phoneModel);
        var quality = SanitizeIdPart(qualityName);
        var combined = $"{category}_{phone}_{model}_{quality}".Trim('_');
        return string.IsNullOrWhiteSpace(combined) ? "part_record" : combined;
    }

    /// <summary>
    /// Нормализует часть id в безопасный ascii-токен.
    /// </summary>
    /// <param name="value">Исходное значение.</param>
    /// <returns>Нормализованное значение.</returns>
    private static string SanitizeIdPart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "empty";

        var source = value.Trim().ToLowerInvariant();
        var sb = new StringBuilder(source.Length);
        var prevUnderscore = false;
        for (var i = 0; i < source.Length; i++)
        {
            var ch = source[i];
            var isLetter = ch is >= 'a' and <= 'z';
            var isDigit = ch is >= '0' and <= '9';
            if (isLetter || isDigit)
            {
                sb.Append(ch);
                prevUnderscore = false;
                continue;
            }

            if (prevUnderscore)
                continue;

            sb.Append('_');
            prevUnderscore = true;
        }

        var normalized = sb.ToString().Trim('_');
        return string.IsNullOrEmpty(normalized) ? "empty" : normalized;
    }

    /// <summary>
    /// Формирует массив подписей для popup.
    /// </summary>
    /// <param name="items">Список значений.</param>
    /// <returns>Массив подписей.</returns>
    private static string[] ToDisplayOptions(IReadOnlyList<string> items)
    {
        var options = new string[items.Count];
        for (var i = 0; i < items.Count; i++)
            options[i] = string.IsNullOrWhiteSpace(items[i]) ? "(пусто)" : items[i];

        return options;
    }
}
#endif
