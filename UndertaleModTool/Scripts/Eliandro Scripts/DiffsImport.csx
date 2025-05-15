using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

EnsureDataLoaded();

// Primeiro, obtendo o conteúdo do arquivo1 (armazenado na variável 'jsonString')
StringBuilder json = new StringBuilder("{\r\n    \"Strings\": [\r\n");
const string
    prefix = "        ",
    suffix = ",\r\n";
foreach (string str in Data.Strings.Select(str => str.Content))
    json.Append(
        prefix
        + JsonifyString(str)
        + suffix);
json.Length -= suffix.Length;
json.Append("\r\n    ]\r\n}");

string jsonString = json.ToString();
MessageBox.Show("Successfully obtained JSON data", "String export");

// Agora, abrindo o arquivo2 para processar substituições
string inputFilePath = "";
using (OpenFileDialog openFileDialog = new OpenFileDialog())
{
    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
    openFileDialog.Title = "Select the input file (file2)";

    if (openFileDialog.ShowDialog() == DialogResult.OK)
    {
        inputFilePath = openFileDialog.FileName;
    }
}

if (!string.IsNullOrEmpty(inputFilePath))
{
    string[] lines = File.ReadAllLines(inputFilePath);

    foreach (string line in lines)
    {
        if (line.StartsWith("Original:"))
        {
            string originalString = line.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[1];
            string modifiedString = lines.FirstOrDefault(l => l.StartsWith("Modified:") && l.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[1] == originalString);

            if (modifiedString != null)
            {
                string searchString = $"\"{originalString}\"";
                string replaceString = $"\"{modifiedString.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)[1]}\"";
                jsonString = jsonString.Replace(searchString, replaceString);
            }
        }
    }

    MessageBox.Show("Content modified", "Modification");

    // Salvando o conteúdo modificado em um novo arquivo
    string outputFilePath = "";
    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
    {
        saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
        saveFileDialog.Title = "Save the modified JSON file";

        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            outputFilePath = saveFileDialog.FileName;
        }
    }

    if (!string.IsNullOrEmpty(outputFilePath))
    {
        File.WriteAllText(outputFilePath, jsonString);
        MessageBox.Show($"Successfully saved modified JSON to\n{outputFilePath}", "Save File");
    }
}
// Resto do código permanece o mesmo
// ...

static string JsonifyString(string str)
{
    StringBuilder sb = new StringBuilder();
    foreach (char ch in str)
    {    // Characters that JSON requires escaping
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

