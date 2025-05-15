using System;
using System.IO;
using System.Linq;
using System.Text;

StringBuilder DataWinContent = new StringBuilder("{\r\n    \"Strings\": [\r\n");
const string
    prefix = "        ",
    suffix = ",\r\n";

EnsureDataLoaded();

foreach (string str in Data.Strings.Select(str => str.Content))
    DataWinContent.Append(
        prefix
        + JsonifyString(str)
        + suffix);
DataWinContent.Length -= suffix.Length;
DataWinContent.Append("\r\n    ]\r\n}");

string filePath2 = EnsureFileSelected();
if (!string.IsNullOrEmpty(filePath2))
{
    string[] file2Lines = File.ReadAllLines(filePath2);

    string[] DataWinLines = DataWinContent.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

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
else
{
    ScriptMessage("Select a strings_better to compare with the .win content");
}

static string JsonifyString(string str)
{
    StringBuilder sb = new StringBuilder();
    foreach (char ch in str)
    {
        // Characters that JSON requires escaping
        if (ch == '\"') { sb.Append("\\\""); continue; }
        if (ch == '\\') { sb.Append("\\\\"); continue; }
        if (ch == '\b') { sb.Append("\\b"); continue; }
        if (ch == '\f') { sb.Append("\\f"); continue; }
        if (ch == '\n') { sb.Append("\\n"); continue; }
        if (ch == '\r') { sb.Append("\\r"); continue; }
        if (ch == '\t') { sb.Append("\\t"); continue; }
        if (Char.IsControl(ch))
        {
            sb.Append("\\u" + Convert.ToByte(ch).ToString("x4"));
            continue;
        }

        sb.Append(ch);
    }
    return "\"" + sb.ToString() + "\"";
}

string EnsureFileSelected()
{
    OpenFileDialog openFileDialog = new OpenFileDialog();
    openFileDialog.Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*";

    if (openFileDialog.ShowDialog() == DialogResult.OK)
    {
        return openFileDialog.FileName;
    }
    else
    {
        Console.WriteLine("Operação cancelada.");
        return string.Empty;
    }
}

string GetDifferences(string[] lines1, string[] lines2)
{
    StringBuilder differences = new StringBuilder();

    int maxLength = Math.Max(lines1.Length, lines2.Length);
    for (int i = 0; i < maxLength; i++)
    {
        string line1 = (i < lines1.Length) ? lines1[i] : string.Empty;
        string line2 = (i < lines2.Length) ? lines2[i] : string.Empty;

        if (line1 != line2)
        {
            differences.AppendLine($"Original: {line1}");
            differences.AppendLine($"Modified: {line2}");
            differences.AppendLine();
        }
    }

    return differences.ToString();
}

void SaveDifferencesToFile(string differences)
{
    SaveFileDialog saveFileDialog = new SaveFileDialog();
    saveFileDialog.Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*";

    if (saveFileDialog.ShowDialog() == DialogResult.OK)
    {
        string resultFilePath = saveFileDialog.FileName;
        File.WriteAllText(resultFilePath, differences);

        Console.WriteLine($"Diferenças salvas em: {resultFilePath}");
    }
    else
    {
        Console.WriteLine("Operação cancelada. Nenhuma diferença foi salva.");
    }
}
