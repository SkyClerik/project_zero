using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Reflection;
using System.Text;


//В Unity в верхнем меню появится Tools → Show Project Structure.
public class ProjectStructureCollector : EditorWindow
{
    private Vector2 scrollPos;
    private string output = "";

    [MenuItem("Tools/Show Project Structure")]
    public static void ShowWindow()
    {
        ProjectStructureCollector window = GetWindow<ProjectStructureCollector>("Project Structure");
        window.RefreshStructure();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Обновить структуру проекта"))
        {
            RefreshStructure();
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.TextArea(output, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void RefreshStructure()
    {
        output = "";
        string assetsPath = Application.dataPath;
        output += "Структура проекта (папка Assets):\n";
        CollectDirectoryStructure(new DirectoryInfo(assetsPath), 0);

        output += "\n\nКлассы из сборок проекта:\n";
        //CollectClassesFromAssemblies();

        Debug.Log("Project structure collected, see 'Project Structure' window.");
    }

    private void CollectDirectoryStructure(DirectoryInfo dir, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 3);
        output += $"{indent}|- {dir.Name}\n";

        // Файлы (исключая метафайлы)
        FileInfo[] files = dir.GetFiles();
        foreach (var file in files)
        {
            if (file.Extension == ".meta") continue;
            output += $"{indent}   |- {file.Name}\n";
        }

        // Подпапки
        DirectoryInfo[] subDirs = dir.GetDirectories();
        foreach (var subDir in subDirs)
        {
            CollectDirectoryStructure(subDir, indentLevel + 1);
        }
    }

    private void CollectClassesFromAssemblies()
    {
        // Ограничимся основными сборками, чтобы не выводить все системные.
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            // Отфильтруем системные, оставим сборки Unity и пользовательские
            if (!IsUserAssembly(assembly))
                continue;

            output += $"\nAssembly: {assembly.GetName().Name}\n";

            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types;
            }

            if (types == null) continue;

            foreach (var type in types)
            {
                if (type == null) continue;
                if (!type.IsClass) continue;

                output += GetTypeInfo(type, 1);
            }
        }
    }

    private bool IsUserAssembly(Assembly assembly)
    {
        string name = assembly.GetName().Name;
        // Игнорируем стандартные системные сборки
        if (name.StartsWith("System") || name.StartsWith("Unity") || name.StartsWith("mscorlib") ||
            name.StartsWith("netstandard") || name.StartsWith("Newtonsoft"))
            return false;
        // Оставляем все остальные — пользовательские.
        return true;
    }

    private string GetTypeInfo(Type type, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 3);
        StringBuilder sb = new StringBuilder();

        sb.AppendLine($"{indent}Class: {type.FullName}");

        // Поля
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

        foreach (var field in fields)
        {
            string access = field.IsPublic ? "public" : (field.IsPrivate ? "private" : "protected/internal");
            sb.AppendLine($"{indent}   |- {access} {field.FieldType.Name} {field.Name}");
        }

        // Свойства (по желанию можно добавить)
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        foreach (var prop in properties)
        {
            sb.AppendLine($"{indent}   |- Property: {prop.PropertyType.Name} {prop.Name}");
        }

        // Вложенные классы
        Type[] nestedTypes = type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var nested in nestedTypes)
        {
            sb.Append(GetTypeInfo(nested, indentLevel + 1));
        }

        return sb.ToString();
    }
}
