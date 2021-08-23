using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CarConstructorV2.Scripts.Additional;
using Plugins.GoogleSheetWindow;
using UnityEditor;
using UnityEngine;

namespace mitaywalle.Plugins.Editor.GoogleSheetWindow
{
    public class GoogleSheetWindow : EditorWindowBase<GoogleSheetWindow>
    {
        public const int maxparsedRows = 5000;
        public bool autoUpdate = true;

        private const string LocalizeLink = "12bGrrM300Jf5bQCF1EJHTTv36711aA7AzDAm1tKJXVM";
        public string[] DocumentsKeys;
        public string[] DocumentsTitles;
        private const string SaveFilePathLocal = "/Resources/";
        public string SaveFilePathDownloadable = "/Resources/";
        public string SaveFilePath = SaveFilePathLocal;
        private string LinksPath = "/SpreadsheetsLinks.txt";
        public bool Updated = false;
        private List<string>[] _documentGIDs;
        private List<string>[] _documentTitles;
        private List<string[][]> all_Cells;

        private int lastSelectedDocument = -1;
        private int lastSelectedSpreadsheet = -1;
        bool spreadSheetsNeedToUpdate = true;
        private string filename = "";

        int currentSpreadsheet = 0;
        int currentDocument = 0;


        private Dictionary<VarType, SpreadsheetType> plugins = new Dictionary<VarType, SpreadsheetType>()
        {
            {VarType._Vector2, new SpreadsheetVector2()},
            {VarType._Vector3, new SpreadsheetVector3()},
            {VarType._bool, new SpreadsheetBool()},
            {VarType._int, new SpreadsheetInt()},
            {VarType._float, new SpreadsheetFloat()},
            {VarType._string, new SpreadsheetString()},
        };

        private string[] all_SheetNames;
        private string[] all_SheetJson;

        private string[] columnNames =
            ("A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,AA,AB,AC,AD,AE,AF,AG,AH,AI,AJ,AK,AL,AM,AN,AO,AP,AQ,AR,AS,AT,AU,AV,AW,AX,AY,AZ,BA,BB,BC,BD,BE,BF,BG,BH,BI,BJ,BK,BL,BM,BN,BO,BP,BQ,BR,BS,BT,BU,BV,BW,BX,BY,BZ,CA,CB,CC,CD,CE,CF,CG,CH,CI,CJ,CK,CL,CM,CN,CO,CP,CQ,CR,CS,CT,CU,CV,CW,CX,CY,CZ"
            ).Split(new char[] {','});

        public new const string Title = "Таблицы";

        [MenuItem(BasePath + Title)]
        public new static void OpenWindow()
        {
            var window = GetWindow<GoogleSheetWindow>(Title);

            if (!window) window = CreateInstance<GoogleSheetWindow>();
            window.ShowUtility();
        }

        private string GetDocumentURL(string key)
        {
            return string.Format(
                @"https://spreadsheets.google.com/feeds/worksheets/{0}/public/basic?alt=json-in-script", key);
        }


        public void OnEnable()
        {
            if (autoUpdate) UpdateDocuments();
            titleContent = new GUIContent("Spreadsheets");
        }

        private void LoadSpreadsheetData(int index)
        {
            _documentGIDs[index] = new List<string>();
            _documentTitles[index] = new List<string>();

            var request = new WWW(GetDocumentURL(DocumentsKeys[index]));
            while (!request.isDone)
            {
            }

            var json = JSON.Parse(request.text);
            int sheetsCount = int.Parse(json["feed"]["openSearch$totalResults"]["$t"].Value);
            DocumentsTitles[index] = json["feed"]["title"]["$t"].Value;
            for (int i = 0; i < sheetsCount; i++)
            {
                string title = json["feed"]["entry"][i]["title"]["$t"].Value;
                if (title.IndexOf("//") != 0)
                {
                    string gid = json["feed"]["entry"][i]["id"]["$t"].Value;
                    gid = gid.Substring(gid.LastIndexOf("/") + 1);
                    _documentGIDs[index].Add(gid);
                    _documentTitles[index].Add(title);
                }
            }

//LoadSheets(index);
        }

        private void LoadSheets(int index, int listIndex = -1)
        {
            string[] sheetsJson = new string[_documentGIDs[index].Count];

            if (listIndex < 0 || listIndex >= _documentGIDs[index].Count)
            {
                for (int i = 0; i < _documentGIDs[index].Count; i++)
                {
//Debug.Log($"загрузка данных из таблицы: {index} лист: {i}");

                    string url = "https://spreadsheets.google.com/feeds/cells/" + DocumentsKeys[index] + "/" +
                                 _documentGIDs[index][i] + "/public/basic?alt=json-in-script";
                    WWW request = new WWW(url);
                    while (!request.isDone)
                    {
                        EditorUtility.DisplayProgressBar("загрузка..", "загрузка данных из таблицы",
                            i * 1f / _documentGIDs[index].Count);
                    }

                    sheetsJson[i] = RemoveBrackets(request.text);
                }
            }
            else
            {
                Debug.Log($"загрузка данных из таблицы: {index} лист: {listIndex}");

                string url = "https://spreadsheets.google.com/feeds/cells/" + DocumentsKeys[index] + "/" +
                             _documentGIDs[index][listIndex] + "/public/basic?alt=json-in-script";
                WWW request = new WWW(url);
                while (!request.isDone)
                {
                    EditorUtility.DisplayProgressBar("загрузка..", "загрузка данных из таблицы",
                        listIndex * 1f / _documentGIDs[index].Count);
                }

                sheetsJson[listIndex] = RemoveBrackets(request.text);
            }

            EditorUtility.ClearProgressBar();
            ParseSheets(sheetsJson);
        }

        private void ParseSheets(string[] docs)
        {
            all_SheetNames = new string[docs.Length];
            all_SheetJson = new string[docs.Length];
            all_Cells = new List<string[][]>();

            for (int i = 0; i < docs.Length; i++)
            {
                EditorUtility.DisplayProgressBar("парсинг..", "парсинг документов", i * 1f / docs.Length);

                if (string.IsNullOrEmpty(docs[i])) continue;

                var json = JSON.Parse(docs[i]);
                string title = json["feed"]["title"]["$t"].Value;
                var entries = json["feed"]["entry"];
                int maxCols = 1;
                int maxRows = 1;
                string[][] cells = new string[maxparsedRows][];
                for (int j = 0; j < cells.Length; j++)
                {
                    cells[j] = new string[columnNames.Length];
                }

                for (int j = 0; j < entries.Count; j++)
                {
                    string cellId = entries[j]["title"]["$t"].Value; //A1, B1, A2,...
                    string cellContent = entries[j]["content"]["$t"].Value; //The cell value
                    int x, y;
                    GetColumnAndRow(cellId, out x, out y);
                    maxCols = Mathf.Max(maxCols, x + 1);
                    maxRows = Mathf.Max(maxRows, y + 1);
                    if (y < cells.Length && x < cells[0].Length)
                    {
                        //Fits inside an constructor array
                        cells[y][x] = cellContent; //Assign value
                    }
                }

                maxCols = Mathf.Min(maxCols, cells[0].Length);
                maxRows = Mathf.Min(maxRows, cells.Length);
                string[][] cellsTrimmed = new string[maxRows][];
                for (int j = 0; j < maxRows; j++)
                {
                    cellsTrimmed[j] = new string[maxCols];
                    Array.Copy(cells[j], cellsTrimmed[j], maxCols);
                }

                cells = cellsTrimmed;
                List<int> keepColumns = new List<int>();
                for (int j = 0; j < cells[0].Length; j++)
                {
                    if (cells[0][j] != null)
                    {
                        if (cells[0][j].IndexOf("//") != 0)
                        {
                            keepColumns.Add(j);
                        }
                    }
                }

                if (keepColumns.Count != cells[0].Length)
                {
                    for (int j = 0; j < cells.Length; j++)
                    {
                        List<string> row = new List<string>();
                        for (int k = 0; k < keepColumns.Count; k++)
                        {
                            row.Add(cells[j][keepColumns[k]]);
                        }

                        cells[j] = row.ToArray();
                    }
                }

                List<int> keepRows = new List<int>();
                for (int j = 0; j < cells.Length; j++)
                {
                    if (cells[j][0] == null || cells[j][0] == "")
                    {
                        int count = 0;
                        for (int k = 0; k < cells[j].Length; k++)
                        {
                            if (cells[j][k] == null || cells[j][k] == "")
                            {
                                count++;
                            }
                        }

                        if (count >= cells[j].Length)
                        {
                            continue;
                        }
                    }

                    if (cells[j].Length > 0 && cells[j][0] != null && cells[j][0].IndexOf("//") != 0)
                    {
                        keepRows.Add(j);
                    }
                }

                if (keepRows.Count != cells.Length)
                {
                    string[][] cellsCopy = new string[keepRows.Count][];
                    for (int j = 0; j < keepRows.Count; j++)
                    {
                        cellsCopy[j] = cells[keepRows[j]];
                    }

                    cells = cellsCopy;
                }

                for (int j = 0; j < cells.Length; j++)
                {
                    for (int k = 0; k < cells[j].Length; k++)
                    {
                        if (cells[j][k] == null)
                        {
                            cells[j][k] = "";
                        }
                    }
                }

                all_Cells.Add(cells);
                all_SheetNames[i] = GetCamelCase(title);
            }

            EditorUtility.ClearProgressBar();

            for (int i = 0; i < all_SheetNames.Length; i++)
            {
                if (string.IsNullOrEmpty(all_SheetNames[i])) continue;

                EditorUtility.DisplayProgressBar("парсинг..", "парсинг листов в json",
                    i * 1f / all_SheetNames.Length);

                string[][] cells = all_Cells[i];
                if (cells.Length == 0)
                {
                    Debug.Log($"пустая таблица на листе {all_SheetNames[i]}");
                    EditorUtility.ClearProgressBar();
                    continue;
                }
                else if (cells.Length == 1)
                {
                    Debug.Log($"заполнена всего одна ячейка на листе {all_SheetNames[i]}");
                    EditorUtility.ClearProgressBar();
                    continue;
                }
                else
                {
                    int maxCols = cells[0].Length;
                    int maxRows = cells.Length;
                    var json = JSON.Parse(docs[i]);
                    string updated = json["feed"]["updated"]["$t"].Value;
                    string title = all_SheetNames[i];
                    VarType[] typesCols = new VarType[maxCols];
                    for (var j = 0; j < maxCols; j++)
                    {
                        string[] colArray = new string[maxRows - 1];
                        for (var k = 0; k < colArray.Length; k++)
                        {
                            colArray[k] = cells[k + 1][j];
                        }

                        typesCols[j]
                            = GetTypeColumn(title, cells[0][j], colArray);
                    }

                    all_SheetJson[i] = "";

                    all_SheetJson[i] += "{\n";
                    for (int k = 1; k < maxRows; k++)
                    {
                        all_SheetJson[i] += "\t\"" + cells[k][0] + "\":{\n\t\t";
                        for (int l = 1; l < maxCols; l++)
                        {
                            all_SheetJson[i] += "\"" + cells[0][l] + "\": " +
                                                plugins[typesCols[l]].GetValue(cells[k][l], cells[0][l]) + ",\n\t\t";
                        }

                        all_SheetJson[i] = all_SheetJson[i].Substring(0, all_SheetJson[i].Length - 4);
                        all_SheetJson[i] += "\n\t},\n";
                    }

                    all_SheetJson[i] = all_SheetJson[i].Substring(0, all_SheetJson[i].Length - 2);
                    all_SheetJson[i] += "\n}";
                }

                Updated = true;
            }


            EditorUtility.ClearProgressBar();
        }

        private string RemoveBrackets(string str)
        {
            int first = str.IndexOf("{");
            int last = str.LastIndexOf("}");
            if (first != -1 && last != -1)
            {
                return str.Substring(first, last - first + 1);
            }
            else
                return str;
        }

        private void GetColumnAndRow(string id, out int x, out int y)
        {
            string A = Regex.Replace(id, @"[\d]", ""); //Alphabetical part
            string B = Regex.Replace(id, @"[^\d]", ""); //Row index
            x = Array.IndexOf<string>(columnNames, A);
            y = Convert.ToInt32(B) - 1; //Starts at 0
        }

        public string GetCamelCase(string inp)
        {
            inp = inp.Trim();
            if (string.IsNullOrEmpty(inp))
            {
                return "";
            }

            string[] words = inp.Split(' ');
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < words.Length; i++)
            {
                string s = words[i];
                if (s.Length > 0)
                {
                    string firstLetter = s.Substring(0, 1);
                    string rest = s.Substring(1, s.Length - 1);
//if (i == 0)
//{
// sb.Append(firstLetter.ToLower() + rest);//DON'T MODIFY FIRST CHARACTER
//}
//else
//{
                    sb.Append(firstLetter.ToUpper() + rest);
// }
                    sb.Append(" ");
                }
            }

            return (sb.ToString().Substring(0, sb.ToString().Length - 1)).Replace(" ", "");
        }

        public VarType GetPluginType(string cell)
        {
            foreach (KeyValuePair<VarType, SpreadsheetType> plugin in plugins)
            {
                if (plugin.Value.IsValid(cell))
                {
                    return plugin.Key;
                }
            }

            return VarType._string;
        }

        public VarType GetTypeColumn(string sheetName, string columnName, string[] cells)
        {
            string[] columnNameArray = columnName.Split(new char[] {' '});
            if (columnNameArray.Length >= 2)
            {
                string compareColumnType = GetCamelCase(columnNameArray[0]).ToLower();
                foreach (KeyValuePair<VarType, SpreadsheetType> pair in plugins)
                {
                    if (compareColumnType == pair.Value.columnKeyWord.ToLower())
                    {
                        return pair.Key;
                    }
                }
            }

            Dictionary<VarType, int> typeCount = new Dictionary<VarType, int>();
            int countTypeEmpty = 0;
            foreach (VarType type in plugins.Keys)
            {
                typeCount[type] = 0;
            }

            foreach (string cell in cells)
            {
                if (cell != "")
                {
                    typeCount[GetPluginType(cell)]++;
                }
                else
                {
                    countTypeEmpty++;
                }
            }

            List<string> output = new List<string>();
            foreach (KeyValuePair<VarType, int> set in typeCount)
            {
                if (set.Value > 0)
                {
                    output.Add(set.Key + " = " + set.Value);
                }
            }

            int countMaxCount = 0;
            VarType countMaxType = VarType._string;
            foreach (KeyValuePair<VarType, int> set in typeCount)
            {
                if (set.Value > countMaxCount)
                {
                    countMaxCount = set.Value;
                    countMaxType = set.Key;
                }

                if (set.Value == (cells.Length - countTypeEmpty) && set.Value > 0)
                {
                    //COULD MAP CLEARLY
                    return set.Key;
                }
            }

            if (countMaxType == VarType._float)
            {
                if (typeCount[VarType._string] == 0 && typeCount[VarType._bool] == 0)
                {
                    return VarType._float;
                }
            }
            else if (countMaxType == VarType._int)
            {
                if (typeCount[VarType._string] == 0 && typeCount[VarType._bool] == 0)
                {
                    if (typeCount[VarType._float] > 0)
                    {
                        //ONE OF THEM IS A FLOAT, CONVERT ALL INTEGERS TO FLOATS
                        return VarType._float;
                    }
                    else
                    {
                        return VarType._int;
                    }
                }
            }

            return VarType._string;
        }

        private string ToJsonString(VarType type, string cell)
        {
            switch (type)
            {
                case VarType._bool:
                    return bool.Parse(cell.ToLower()) ? "true" : "false";
                case VarType._float:
                    return float.Parse(cell).ToString();
                case VarType._int:
                    return int.Parse(cell).ToString();
                case VarType._string:
                    return "\"" + cell + "\"";
                default:
                    return "null";
            }
        }

        private void UpdateDocuments()
        {
            UpdateDocumentList();

            for (int i = 0;
                i <
                DocumentsKeys.Length;
                i++)
            {
                LoadSpreadsheetData(i);
            }
        }

        private void UpdateDocumentList()
        {
            DocumentsKeys = File.ReadAllLines(Application.dataPath + LinksPath);
            DocumentsTitles = new string[DocumentsKeys.Length];
            _documentGIDs = new List<string>[DocumentsKeys.Length];
            _documentTitles = new List<string>[DocumentsKeys.Length];
        }


        public static void OpenLink()
        {
            Application.OpenURL($"https://docs.google.com/spreadsheets/d/{LocalizeLink}/edit#gid=0");
        }
        
        private new void OnGUI()
        {
            base.OnGUI();

            EditorGUILayout.HelpBox($"Внимание! максимум можно спарсить {maxparsedRows} строк", MessageType.Warning);
            GUILayout.Label("Документы", EditorStyles.boldLabel);
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60f;
            currentDocument = EditorGUILayout.Popup("текущий", currentDocument, DocumentsTitles);
            EditorGUIUtility.labelWidth = oldLabelWidth;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Открыть файл ссылки"))
            {
                System.Diagnostics.Process.Start(Application.dataPath + LinksPath);
            }

            if (GUILayout.Button("Обновить файл ссылки"))
            {
                UpdateDocuments();
            }

            if (GUILayout.Button("Перейти по ссылке"))
            {
                Application.OpenURL(string.Format(@"https://docs.google.com/spreadsheets/d/{0}/edit#gid=0",
                    DocumentsKeys[currentDocument]));
            }

            GUILayout.EndHorizontal();
            if (currentDocument != lastSelectedDocument)
                spreadSheetsNeedToUpdate = true;
            lastSelectedDocument = currentDocument;

            if (spreadSheetsNeedToUpdate)
            {
                LoadSheets(currentDocument);
                spreadSheetsNeedToUpdate = false;
                lastSelectedSpreadsheet = -1;
                currentSpreadsheet = 0;
            }

            EditorGUILayout.Space();
            GUILayout.Label("Листы", EditorStyles.boldLabel);
            oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60f;
            currentSpreadsheet = EditorGUILayout.Popup("Текущий", currentSpreadsheet, all_SheetNames);
            OnChangeList(all_SheetNames[currentSpreadsheet]);
            EditorGUIUtility.labelWidth = oldLabelWidth;

            if (GUILayout.Button("подгрузить данные всех листов"))
            {
                LoadSheets(currentDocument);
                spreadSheetsNeedToUpdate = false;
                lastSelectedSpreadsheet = -1;
                currentSpreadsheet = 0;
            }

            if (currentSpreadsheet != lastSelectedSpreadsheet)
                filename = all_SheetNames[currentSpreadsheet] + ".json";
            lastSelectedSpreadsheet = currentSpreadsheet;
            EditorGUILayout.Space();
            GUILayout.Label("Настройки файла", EditorStyles.boldLabel);
            oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 60f;
            SaveFilePath = EditorGUILayout.TextField("Путь", SaveFilePath);
            EditorGUIUtility.labelWidth = oldLabelWidth;

            oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100f;
            filename = EditorGUILayout.TextField("Имя json-файла", filename);
            EditorGUIUtility.labelWidth = oldLabelWidth;

            try
            {
                EditorGUILayout.ObjectField(
                    AssetDatabase.LoadAssetAtPath<TextAsset>("Assets" + SaveFilePath + filename), typeof(TextAsset),
                    true);
            }
            catch
            {
            }

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Открыть json-файл "))
            {
                var path = SaveFilePath;
                if (!path.StartsWith("/"))
                    path = "/" + path;
                if (!path.EndsWith("/"))
                    path += "/";

                if (File.Exists(Application.dataPath + path + filename))
                    System.Diagnostics.Process.Start(Application.dataPath + path + filename);
                else
                    Debug.Log("этот файл еще не создан!");
            }

            if (GUILayout.Button("сохранить в Json")) SaveJson();

            GUILayout.EndHorizontal();
        }

        public void SaveJson(bool skipDialog = false)
        {
            var path = SaveFilePath;
            if (!path.StartsWith("/"))
                path = "/" + path;
            if (!path.EndsWith("/"))
                path += "/";

            path = "./Assets" + path;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log(string.Format("Directory {0} created", path));
            }

            path = path + filename;
            if (File.Exists(path))
            {
                if (skipDialog || EditorUtility.DisplayDialog("Сохранение файла..",
                    string.Format("Файл {0} уже существует.Перезаписать ? ", filename), "да", "нет"))
                {
                    File.WriteAllText(path, all_SheetJson[currentSpreadsheet]);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    AssetDatabase.ImportAsset(path);
                }
            }
            else
            {
                File.WriteAllText(path, all_SheetJson[currentSpreadsheet]);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                AssetDatabase.ImportAsset(path);
            }
        }

        public void ReloadSheets()
        {
            LoadSheets(currentDocument);
            spreadSheetsNeedToUpdate = false;
            lastSelectedSpreadsheet = -1;
            currentSpreadsheet = 0;
        }

// Custom funcs
        public static void ReimportLocalization()
        {
            var window = GetWindow(typeof(GoogleSheetWindow), false, "Таблицы") as GoogleSheetWindow;
            window.autoUpdate = false;
            window.lastSelectedDocument = 0;
            window.spreadSheetsNeedToUpdate = false;
            window.UpdateDocumentList();
            window.LoadSpreadsheetData(0);
            window.LoadSheets(0, 0);
            window.filename = window.all_SheetNames[0] + ".json";

//EditorApplication.delayCall += () =>
            {
                window.SaveJson(true);
                window.Close();
            }
        }

        private string lastListName;

        private void OnChangeList(string listName)
        {
            if (lastListName == listName)
                return;

            lastListName = listName;

            SaveFilePath = SaveFilePathDownloadable;
        }
    }
}