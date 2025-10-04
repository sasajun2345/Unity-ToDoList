using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using UnityEditorInternal;

public class TodoListEditor : EditorWindow
{
    // 语言枚举
    private enum Language { Chinese, English }
    private Language currentLanguage = Language.Chinese;

    // 语言资源
    private Dictionary<string, string> languageDict = new Dictionary<string, string>();

    // 任务类
    [System.Serializable]
    public class Task
    {
        public string taskName;
        public string description;
        public DateTime dueDate;
        public int priority; // 0-小, 1-中, 2-大
        public int workload; // 0-小, 1-中, 2-大
        public bool isCompleted;
        public bool isExpanded;
        public string category;
        public DateTime creationDate = DateTime.Now;
        public DateTime lastModifiedDate = DateTime.Now;
    }

    private List<Task> tasks = new List<Task>();
    private Vector2 scrollPosition;
    private string newCategory = "";
    private List<string> categories = new List<string>();
    private string searchQuery = "";
    private bool showCompleted = true;
    private bool showIncomplete = true;
    private int sortOption = 0;
    private bool ascendingOrder = false;
    private bool showStatistics = false;
    private bool showExportOptions = false;
    private Texture2D priorityIconLow;
    private Texture2D priorityIconMedium;
    private Texture2D priorityIconHigh;
    private Texture2D workloadIconLow;
    private Texture2D workloadIconMedium;
    private Texture2D workloadIconHigh;

    // 多语言下拉选项
    private string[] PriorityOptions => currentLanguage == Language.Chinese ?
        new string[] { "低", "中", "高" } :
        new string[] { "Low", "Medium", "High" };

    private string[] WorkloadOptions => currentLanguage == Language.Chinese ?
        new string[] { "小", "中", "大" } :
        new string[] { "Small", "Medium", "Large" };

    private string[] SortOptions => currentLanguage == Language.Chinese ?
        new string[] { "默认", "优先级", "截止日期", "创建日期" } :
        new string[] { "Default", "Priority", "Due Date", "Created Date" };

    private readonly string[] exportOptions = { "CSV", "JSON", "Markdown" };

    private ReorderableList reorderableList;

    [MenuItem("Window/Todo List Editor")]
    public static void ShowWindow()
    {
        GetWindow<TodoListEditor>("Todo List");
    }

    private void OnEnable()
    {
        // 初始化语言
        LoadLanguage();

        // 初始化任务
        if (tasks == null)
            tasks = new List<Task>();

        // 添加一些示例分类
        categories = new List<string> { GetText("Work"), GetText("Personal"), GetText("Study"), GetText("GameDev") };

        // 创建图标
        CreateIcons();

        // 初始化 ReorderableList
        reorderableList = new ReorderableList(tasks, typeof(Task), true, true, true, true)
        {
            drawElementCallback = DrawTaskElement,
            elementHeightCallback = CalculateTaskHeight,
            onAddCallback = (ReorderableList list) => {
                tasks.Add(new Task()
                {
                    taskName = GetText("NewTask"),
                    dueDate = DateTime.Today.AddDays(7),
                    category = categories.Count > 0 ? categories[0] : ""
                });
                Repaint();
            },
            onRemoveCallback = (ReorderableList list) => {
                if (EditorUtility.DisplayDialog(
                    GetText("DeleteTask"),
                    GetText("ConfirmDeleteTask"),
                    GetText("Yes"),
                    GetText("No")))
                {
                    tasks.RemoveAt(list.index);
                    Repaint();
                }
            },
            onReorderCallback = (ReorderableList list) => Repaint(),
            drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, GetText("TaskList"));
            }
        };
    }

    // 加载语言资源
    private void LoadLanguage()
    {
        languageDict.Clear();

        if (currentLanguage == Language.Chinese)
        {
            // 中文资源
            languageDict.Add("TodoList", "待办事项列表");
            languageDict.Add("NewTask", "新任务");
            languageDict.Add("TaskName", "任务名称");
            languageDict.Add("Description", "描述");
            languageDict.Add("DueDate", "截止日期 (YYYY-MM-DD)");
            languageDict.Add("Category", "分类");
            languageDict.Add("Priority", "优先级");
            languageDict.Add("Workload", "工作量");
            languageDict.Add("Completed", "已完成");
            languageDict.Add("Created", "创建于");
            languageDict.Add("Modified", "修改于");
            languageDict.Add("DeleteTask", "删除任务");
            languageDict.Add("ConfirmDeleteTask", "确定要删除这个任务吗？");
            languageDict.Add("Yes", "是");
            languageDict.Add("No", "否");
            languageDict.Add("TaskList", "任务列表");
            languageDict.Add("Work", "工作");
            languageDict.Add("Personal", "个人");
            languageDict.Add("Study", "学习");
            languageDict.Add("GameDev", "游戏开发");
            languageDict.Add("AddNewCategory", "添加新分类:");
            languageDict.Add("Add", "添加");
            languageDict.Add("CategoryExists", "分类已存在");
            languageDict.Add("CategoryExistsMsg", "该分类名称已经存在!");
            languageDict.Add("CurrentCategories", "当前分类:");
            languageDict.Add("DeleteCategory", "删除分类");
            languageDict.Add("ConfirmDeleteCategory", "确定要删除分类 '{0}' 吗?");
            languageDict.Add("Search", "搜索...");
            languageDict.Add("Show", "显示:");
            languageDict.Add("ShowCompleted", "已完成");
            languageDict.Add("ShowIncomplete", "未完成");
            languageDict.Add("Sort", "排序:");
            languageDict.Add("Ascending", "升序");
            languageDict.Add("Statistics", "统计");
            languageDict.Add("Export", "导出");
            languageDict.Add("TaskStatistics", "任务统计");
            languageDict.Add("TotalTasks", "总任务数");
            languageDict.Add("CompletedTasks", "已完成");
            languageDict.Add("IncompleteTasks", "未完成");
            languageDict.Add("HighPriority", "高优先级");
            languageDict.Add("Overdue", "已逾期");
            languageDict.Add("CompletionProgress", "完成进度");
            languageDict.Add("ExportOptions", "导出选项");
            languageDict.Add("ExportSuccess", "导出成功");
            languageDict.Add("ExportSuccessMsg", "任务已成功导出为 {0} 格式");
            languageDict.Add("ExportError", "导出错误");
            languageDict.Add("ExportErrorMsg", "导出失败: {0}");
            languageDict.Add("NoTasksFound", "没有找到匹配的任务");
            languageDict.Add("Total", "总数");
            languageDict.Add("Showing", "显示");
            languageDict.Add("AddNewTask", "添加新任务");
            languageDict.Add("Language", "语言");
            languageDict.Add("Chinese", "中文");
            languageDict.Add("English", "英文");
        }
        else
        {
            // English resources
            languageDict.Add("TodoList", "Todo List");
            languageDict.Add("NewTask", "New Task");
            languageDict.Add("TaskName", "Task Name");
            languageDict.Add("Description", "Description");
            languageDict.Add("DueDate", "Due Date (YYYY-MM-DD)");
            languageDict.Add("Category", "Category");
            languageDict.Add("Priority", "Priority");
            languageDict.Add("Workload", "Workload");
            languageDict.Add("Completed", "Completed");
            languageDict.Add("Created", "Created");
            languageDict.Add("Modified", "Modified");
            languageDict.Add("DeleteTask", "Delete Task");
            languageDict.Add("ConfirmDeleteTask", "Are you sure you want to delete this task?");
            languageDict.Add("Yes", "Yes");
            languageDict.Add("No", "No");
            languageDict.Add("TaskList", "Task List");
            languageDict.Add("Work", "Work");
            languageDict.Add("Personal", "Personal");
            languageDict.Add("Study", "Study");
            languageDict.Add("GameDev", "Game Development");
            languageDict.Add("AddNewCategory", "Add New Category:");
            languageDict.Add("Add", "Add");
            languageDict.Add("CategoryExists", "Category Exists");
            languageDict.Add("CategoryExistsMsg", "This category name already exists!");
            languageDict.Add("CurrentCategories", "Current Categories:");
            languageDict.Add("DeleteCategory", "Delete Category");
            languageDict.Add("ConfirmDeleteCategory", "Are you sure you want to delete category '{0}'?");
            languageDict.Add("Search", "Search...");
            languageDict.Add("Show", "Show:");
            languageDict.Add("ShowCompleted", "Completed");
            languageDict.Add("ShowIncomplete", "Incomplete");
            languageDict.Add("Sort", "Sort:");
            languageDict.Add("Ascending", "Ascending");
            languageDict.Add("Statistics", "Statistics");
            languageDict.Add("Export", "Export");
            languageDict.Add("TaskStatistics", "Task Statistics");
            languageDict.Add("TotalTasks", "Total Tasks");
            languageDict.Add("CompletedTasks", "Completed");
            languageDict.Add("IncompleteTasks", "Incomplete");
            languageDict.Add("HighPriority", "High Priority");
            languageDict.Add("Overdue", "Overdue");
            languageDict.Add("CompletionProgress", "Completion Progress");
            languageDict.Add("ExportOptions", "Export Options");
            languageDict.Add("ExportSuccess", "Export Success");
            languageDict.Add("ExportSuccessMsg", "Tasks exported successfully to {0} format");
            languageDict.Add("ExportError", "Export Error");
            languageDict.Add("ExportErrorMsg", "Export failed: {0}");
            languageDict.Add("NoTasksFound", "No matching tasks found");
            languageDict.Add("Total", "Total");
            languageDict.Add("Showing", "Showing");
            languageDict.Add("AddNewTask", "Add New Task");
            languageDict.Add("Language", "Language");
            languageDict.Add("Chinese", "中文");
            languageDict.Add("English", "English");
        }
    }

    // 获取本地化文本
    private string GetText(string key)
    {
        if (languageDict.TryGetValue(key, out string value))
        {
            return value;
        }
        return key; // 如果找不到，返回key
    }

    // 格式化带参数的文本
    private string FormatText(string key, params object[] args)
    {
        if (languageDict.TryGetValue(key, out string format))
        {
            return string.Format(format, args);
        }
        return key; // 如果找不到，返回key
    }

    private void CreateIcons()
    {
        priorityIconLow = CreateColorTexture(new Color(0.4f, 0.8f, 0.4f)); // 绿色
        priorityIconMedium = CreateColorTexture(new Color(0.9f, 0.8f, 0.2f)); // 黄色
        priorityIconHigh = CreateColorTexture(new Color(0.9f, 0.4f, 0.3f)); // 红色

        workloadIconLow = CreateColorTexture(new Color(0.6f, 0.9f, 1f)); // 浅蓝
        workloadIconMedium = CreateColorTexture(new Color(0.4f, 0.7f, 1f)); // 蓝色
        workloadIconHigh = CreateColorTexture(new Color(0.2f, 0.4f, 0.8f)); // 深蓝
    }

    private Texture2D CreateColorTexture(Color color)
    {
        var texture = new Texture2D(16, 16);
        var pixels = Enumerable.Repeat(color, 16 * 16).ToArray();
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    private float CalculateTaskHeight(int index)
    {
        Task task = tasks[index];

        if (!task.isExpanded)
            return EditorGUIUtility.singleLineHeight + 8f;

        // 增加展开时的高度计算（原9行→11行）
        return EditorGUIUtility.singleLineHeight * 11 + 12f; // 增加额外空间
    }

    private void DrawTaskElement(Rect rect, int index, bool isActive, bool isFocused)
    {
        Task task = tasks[index];

        float originalY = rect.y;
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float verticalSpacing = 2f;

        // 背景色
        if (index % 2 == 0)
        {
            EditorGUI.DrawRect(new Rect(rect.x - 10, rect.y, rect.width + 20, rect.height),
                             task.isCompleted ? new Color(0.1f, 0.4f, 0.1f, 0.1f) : new Color(0.2f, 0.2f, 0.2f, 0.1f));
        }

        // 1. 折叠标题行 (带优先级图标)
        Rect headerRect = new Rect(rect.x, rect.y, rect.width - 60, lineHeight);
        Rect iconRect = new Rect(rect.x + rect.width - 55, rect.y, 50, lineHeight);

        // 绘制优先级图标
        Texture2D priorityIcon = task.priority == 0 ? priorityIconLow :
                                task.priority == 1 ? priorityIconMedium : priorityIconHigh;
        GUI.DrawTexture(iconRect, priorityIcon);

        // 折叠标题
        task.isExpanded = EditorGUI.Foldout(headerRect, task.isExpanded,
                                          $"{task.taskName} {(task.isCompleted ? $"({GetText("Completed")})" : "")}",
                                          true);
        rect.y += lineHeight + verticalSpacing;

        if (task.isExpanded)
        {
            // 2. 任务名称
            Rect nameRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            task.taskName = EditorGUI.TextField(nameRect, GetText("TaskName"), task.taskName);
            rect.y += lineHeight + verticalSpacing;

            // 3. 任务描述 (多行文本)
            Rect descRect = new Rect(rect.x, rect.y, rect.width, lineHeight * 2);
            EditorGUI.LabelField(descRect, GetText("Description"));
            rect.y += lineHeight;
            task.description = EditorGUI.TextArea(descRect, task.description);
            rect.y += lineHeight * 2 + verticalSpacing;

            // 4. 截止日期
            string dueDateString = task.dueDate.ToString("yyyy-MM-dd");
            Rect dateRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            dueDateString = EditorGUI.TextField(dateRect, GetText("DueDate"), dueDateString);
            if (DateTime.TryParse(dueDateString, out DateTime parsedDate))
            {
                task.dueDate = parsedDate;
            }
            rect.y += lineHeight + verticalSpacing;

            // 5. 分类选择
            Rect catRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            int catIndex = Mathf.Clamp(categories.IndexOf(task.category), 0, categories.Count - 1);
            if (catIndex < 0 && categories.Count > 0) catIndex = 0;

            if (categories.Count > 0)
            {
                catIndex = EditorGUI.Popup(catRect, GetText("Category"), catIndex, categories.ToArray());
                task.category = categories[catIndex];
            }
            else
            {
                task.category = EditorGUI.TextField(catRect, GetText("Category"), task.category);
            }
            rect.y += lineHeight + verticalSpacing;

            // 6. 优先级 (带图标和标签)
            Rect priorityRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            Rect priorityLabelRect = new Rect(priorityRect.x, priorityRect.y, EditorGUIUtility.labelWidth, lineHeight);
            EditorGUI.LabelField(priorityLabelRect, GetText("Priority"));

            Rect priorityIconRect = new Rect(priorityRect.x + EditorGUIUtility.labelWidth, priorityRect.y, 20, lineHeight);
            GUI.DrawTexture(priorityIconRect, priorityIcon);

            task.priority = EditorGUI.Popup(
                new Rect(priorityIconRect.x + 25, priorityRect.y,
                         priorityRect.width - EditorGUIUtility.labelWidth - 25, lineHeight),
                "",
                task.priority,
                PriorityOptions);

            rect.y += lineHeight + verticalSpacing;

            // 7. 工作量 (带图标和标签) - 优化布局
            Rect workloadRect = new Rect(rect.x, rect.y, rect.width, lineHeight * 1.5f); // 增加高度
            Rect workloadLabelRect = new Rect(workloadRect.x, workloadRect.y, EditorGUIUtility.labelWidth, lineHeight);
            EditorGUI.LabelField(workloadLabelRect, GetText("Workload"));

            Texture2D workloadIcon = task.workload == 0 ? workloadIconLow :
                                   task.workload == 1 ? workloadIconMedium : workloadIconHigh;

            // 垂直居中图标
            Rect workloadIconRect = new Rect(
                workloadRect.x + EditorGUIUtility.labelWidth,
                workloadRect.y + (workloadRect.height - lineHeight) / 2,
                20,
                lineHeight
            );
            GUI.DrawTexture(workloadIconRect, workloadIcon);

            task.workload = EditorGUI.Popup(
                new Rect(workloadIconRect.x + 25, workloadRect.y,
                         workloadRect.width - EditorGUIUtility.labelWidth - 25, lineHeight),
                "",
                task.workload,
                WorkloadOptions);

            rect.y += workloadRect.height + verticalSpacing; // 使用实际高度

            // 8. 完成状态 - 增加间距
            Rect completeRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            task.isCompleted = EditorGUI.Toggle(completeRect, GetText("Completed"), task.isCompleted);
            rect.y += lineHeight + verticalSpacing + 4f; // 额外增加4px间距

            // 9. 日期信息
            Rect dateInfoRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            GUIStyle dateStyle = new GUIStyle(EditorStyles.miniLabel);
            dateStyle.normal.textColor = Color.gray;
            EditorGUI.LabelField(dateInfoRect,
                               $"{GetText("Created")}: {task.creationDate:yyyy-MM-dd} | {GetText("Modified")}: {task.lastModifiedDate:yyyy-MM-dd HH:mm}",
                               dateStyle);
        }
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        // 语言切换按钮
        if (GUILayout.Button($"{GetText("Language")}: {GetText(currentLanguage.ToString())}",
                           EditorStyles.toolbarButton, GUILayout.Width(120)))
        {
            // 切换语言
            currentLanguage = currentLanguage == Language.Chinese ? Language.English : Language.Chinese;
            LoadLanguage();
            Repaint();
        }

        GUILayout.Space(10);

        // 搜索框
        string newSearch = EditorGUILayout.TextField(searchQuery, EditorStyles.toolbarSearchField,
                                                    GUILayout.Width(200), GUILayout.Height(20));
        if (newSearch != searchQuery)
        {
            searchQuery = newSearch;
            Repaint();
        }

        // 显示选项
        GUILayout.Space(10);
        GUILayout.Label(GetText("Show"), EditorStyles.miniBoldLabel, GUILayout.Width(40));
        bool newShowCompleted = EditorGUILayout.ToggleLeft(GetText("ShowCompleted"), showCompleted, GUILayout.Width(80));
        bool newShowIncomplete = EditorGUILayout.ToggleLeft(GetText("ShowIncomplete"), showIncomplete, GUILayout.Width(80));
        if (newShowCompleted != showCompleted || newShowIncomplete != showIncomplete)
        {
            showCompleted = newShowCompleted;
            showIncomplete = newShowIncomplete;
            Repaint();
        }

        // 排序选项
        GUILayout.Space(10);
        GUILayout.Label(GetText("Sort"), EditorStyles.miniBoldLabel, GUILayout.Width(40));
        int newSortOption = EditorGUILayout.Popup(sortOption, SortOptions, GUILayout.Width(90));
        if (newSortOption != sortOption)
        {
            sortOption = newSortOption;
            Repaint();
        }
        bool newAscending = EditorGUILayout.ToggleLeft(GetText("Ascending"), ascendingOrder, GUILayout.Width(70));
        if (newAscending != ascendingOrder)
        {
            ascendingOrder = newAscending;
            Repaint();
        }

        GUILayout.FlexibleSpace();

        // 统计按钮
        if (GUILayout.Button(GetText("Statistics"), EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            showStatistics = !showStatistics;
        }

        // 导出按钮
        if (GUILayout.Button(GetText("Export"), EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            showExportOptions = !showExportOptions;
        }

        GUILayout.EndHorizontal();
    }

    private void DrawCategoryManagement()
    {
        GUILayout.BeginHorizontal();
        newCategory = EditorGUILayout.TextField(GetText("AddNewCategory"), newCategory);
        if (GUILayout.Button(GetText("Add"), GUILayout.Width(60)))
        {
            if (!string.IsNullOrEmpty(newCategory))
            {
                if (!categories.Contains(newCategory))
                {
                    categories.Add(newCategory);
                    newCategory = "";
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        GetText("CategoryExists"),
                        GetText("CategoryExistsMsg"),
                        "OK");
                }
            }
        }
        GUILayout.EndHorizontal();

        // 显示所有分类
        GUILayout.BeginHorizontal();
        GUILayout.Label(GetText("CurrentCategories"), GUILayout.Width(100));
        for (int i = 0; i < categories.Count; i++)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(categories[i]);
            if (GUILayout.Button("×", GUILayout.Width(20)))
            {
                if (EditorUtility.DisplayDialog(
                    GetText("DeleteCategory"),
                    FormatText("ConfirmDeleteCategory", categories[i]),
                    GetText("Yes"),
                    GetText("No")))
                {
                    categories.RemoveAt(i);
                    i--;
                }
            }
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
        GUILayout.EndHorizontal();
    }

    private void DrawStatisticsPanel()
    {
        if (!showStatistics) return;

        GUILayout.Space(10);
        EditorGUILayout.LabelField(GetText("TaskStatistics"), EditorStyles.boldLabel);

        int totalTasks = tasks.Count;
        int completedTasks = tasks.Count(t => t.isCompleted);
        int highPriorityTasks = tasks.Count(t => t.priority == 2);
        int overdueTasks = tasks.Count(t => !t.isCompleted && t.dueDate < DateTime.Today);

        float completionRate = totalTasks > 0 ? (float)completedTasks / totalTasks : 0f;

        GUILayout.BeginVertical("Box");
        EditorGUILayout.LabelField($"{GetText("TotalTasks")}: {totalTasks}");
        EditorGUILayout.LabelField($"{GetText("CompletedTasks")}: {completedTasks} ({completionRate:P0})");
        EditorGUILayout.LabelField($"{GetText("IncompleteTasks")}: {totalTasks - completedTasks}");
        EditorGUILayout.LabelField($"{GetText("HighPriority")}: {highPriorityTasks}");
        EditorGUILayout.LabelField($"{GetText("Overdue")}: {overdueTasks}");
        GUILayout.EndVertical();

        // 进度条
        Rect progressRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));
        EditorGUI.ProgressBar(progressRect, completionRate,
                             $"{GetText("CompletionProgress")}: {completedTasks}/{totalTasks} ({completionRate:P0})");

        GUILayout.Space(10);
    }

    private void DrawExportPanel()
    {
        if (!showExportOptions) return;

        GUILayout.Space(10);
        EditorGUILayout.LabelField(GetText("ExportOptions"), EditorStyles.boldLabel);

        GUILayout.BeginVertical("Box");
        int exportChoice = GUILayout.SelectionGrid(-1, exportOptions, 3);
        if (exportChoice >= 0)
        {
            string path = EditorUtility.SaveFilePanel(GetText("Export"), "", "tasks", exportOptions[exportChoice].ToLower());
            if (!string.IsNullOrEmpty(path))
            {
                ExportTasks(path, exportOptions[exportChoice]);
            }
            showExportOptions = false;
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
    }

    private void ExportTasks(string path, string format)
    {
        try
        {
            string content = "";

            switch (format)
            {
                case "CSV":
                    content = $"{GetText("TaskName")},{GetText("Description")},{GetText("DueDate")},{GetText("Category")},{GetText("Priority")},{GetText("Workload")},{GetText("Completed")}\n";
                    foreach (var task in tasks)
                    {
                        content += $"\"{task.taskName}\",\"{task.description}\",{task.dueDate:yyyy-MM-dd},{task.category},{PriorityOptions[task.priority]},{WorkloadOptions[task.workload]},{(task.isCompleted ? GetText("Completed") : GetText("IncompleteTasks"))}\n";
                    }
                    break;

                case "JSON":
                    content = "[\n";
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        var task = tasks[i];
                        content += "  {\n";
                        content += $"    \"name\": \"{task.taskName}\",\n";
                        content += $"    \"description\": \"{task.description}\",\n";
                        content += $"    \"dueDate\": \"{task.dueDate:yyyy-MM-dd}\",\n";
                        content += $"    \"category\": \"{task.category}\",\n";
                        content += $"    \"priority\": \"{PriorityOptions[task.priority]}\",\n";
                        content += $"    \"workload\": \"{WorkloadOptions[task.workload]}\",\n";
                        content += $"    \"status\": \"{(task.isCompleted ? "completed" : "pending")}\",\n";
                        content += $"    \"created\": \"{task.creationDate:yyyy-MM-dd}\"\n";
                        content += "  }" + (i < tasks.Count - 1 ? "," : "") + "\n";
                    }
                    content += "]";
                    break;

                case "Markdown":
                    content = $"# {GetText("TaskList")}\n\n";
                    content += $"| {GetText("TaskName")} | {GetText("Description")} | {GetText("DueDate")} | {GetText("Category")} | {GetText("Priority")} | {GetText("Workload")} | {GetText("Completed")} |\n";
                    content += "|----------|------|----------|------|--------|--------|------|\n";
                    foreach (var task in tasks)
                    {
                        content += $"| {task.taskName} | {task.description} | {task.dueDate:yyyy-MM-dd} | {task.category} | {PriorityOptions[task.priority]} | {WorkloadOptions[task.workload]} | {(task.isCompleted ? "✅" : "❌")} |\n";
                    }
                    break;
            }

            System.IO.File.WriteAllText(path, content);
            EditorUtility.DisplayDialog(
                GetText("ExportSuccess"),
                FormatText("ExportSuccessMsg", format),
                "OK");
        }
        catch (Exception ex)
        {
            EditorUtility.DisplayDialog(
                GetText("ExportError"),
                FormatText("ExportErrorMsg", ex.Message),
                "OK");
        }
    }

    private void OnGUI()
    {
        // 绘制工具栏
        DrawToolbar();

        // 绘制分类管理
        DrawCategoryManagement();

        // 绘制统计面板
        DrawStatisticsPanel();

        // 绘制导出面板
        DrawExportPanel();

        // 过滤和排序任务
        var filteredTasks = tasks
            .Where(t => (showCompleted && t.isCompleted) || (showIncomplete && !t.isCompleted))
            .Where(t => string.IsNullOrEmpty(searchQuery) ||
                      t.taskName.Contains(searchQuery) ||
                      t.description.Contains(searchQuery) ||
                      t.category.Contains(searchQuery))
            .ToList();

        switch (sortOption)
        {
            case 1: // 优先级
                filteredTasks = ascendingOrder ?
                    filteredTasks.OrderBy(t => t.priority).ToList() :
                    filteredTasks.OrderByDescending(t => t.priority).ToList();
                break;
            case 2: // 截止日期
                filteredTasks = ascendingOrder ?
                    filteredTasks.OrderBy(t => t.dueDate).ToList() :
                    filteredTasks.OrderByDescending(t => t.dueDate).ToList();
                break;
            case 3: // 创建日期
                filteredTasks = ascendingOrder ?
                    filteredTasks.OrderBy(t => t.creationDate).ToList() :
                    filteredTasks.OrderByDescending(t => t.creationDate).ToList();
                break;
        }

        // 更新列表
        reorderableList.list = filteredTasks;

        // 绘制任务列表
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        {
            if (filteredTasks.Count == 0)
            {
                GUILayout.Label(GetText("NoTasksFound"), EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                // 确保列表高度正确计算
                reorderableList.DoLayoutList();
            }
        }
        EditorGUILayout.EndScrollView();

        // 绘制底部信息
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.Label($"{GetText("Total")}: {tasks.Count} | {GetText("Showing")}: {filteredTasks.Count}");
        if (GUILayout.Button(GetText("AddNewTask"), GUILayout.Width(120)))
        {
            tasks.Add(new Task()
            {
                taskName = GetText("NewTask"),
                dueDate = DateTime.Today.AddDays(7),
                category = categories.Count > 0 ? categories[0] : ""
            });
        }
        GUILayout.EndHorizontal();
    }
}