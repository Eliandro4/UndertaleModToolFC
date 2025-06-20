using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UndertaleModLib;
using UndertaleModLib.Scripting;
using UndertaleModLib.Util;
using UndertaleModLib.Decompiler;
using Newtonsoft.Json;

EnsureDataLoaded();

List<string> DataWinContent = Data.Strings.Select(str => str.Content).ToList();

string filePath2 = PromptLoadFile("", "TXT and JSON files (.txt, .json)|*.txt;*.json|All files (*.*)|*.*");
if (string.IsNullOrEmpty(filePath2))
{
    ScriptMessage("No file selected.");
    return;
}
if (!string.IsNullOrEmpty(filePath2))
{
    DS file2Lines0 = JsonConvert.DeserializeObject<DS>(File.ReadAllText(filePath2));
    string[] file2Lines = file2Lines0.Strings.ToArray();

    string[] DataWinLines = DataWinContent.ToArray();

    string differences = GetDifferences(DataWinLines, file2Lines);

    if (!string.IsNullOrEmpty(differences))
    {
        SaveDifferencesToFile(differences);
        ScriptMessage("Differences saved to file.");
    }
    else
    {
        ScriptMessage("No differences found.");
    }
}

string GetDifferences(string[] lines1, string[] lines2)
{
    Dictionary<string, string> differencies = [];

    int maxLength = Math.Max(lines1.Length, lines2.Length);
    for (int i = 0; i < maxLength; i++)
    {
        string line1 = (i < lines1.Length) ? lines1[i] : string.Empty;
        string line2 = (i < lines2.Length) ? lines2[i] : string.Empty;

        if (line1 != line2)
        {
            differencies.Add(line1, line2);
        }
    }

    return JsonConvert.SerializeObject(differencies, Formatting.Indented);
}

void SaveDifferencesToFile(string differences)
{
    string save_path = PromptChooseDirectory();

    if (!string.IsNullOrEmpty(save_path))
    {
        string resultFilePath = Path.Combine(save_path, "diferencas.json");
        File.WriteAllText(resultFilePath, differences);

        ScriptMessage($"Diferenças salvas em: {resultFilePath}");
    }
    else
    {
        ScriptMessage("Operação cancelada. Nenhuma diferença foi salva.");
    }
}

struct DS
{
    public List<string> Strings { get; set; }
}