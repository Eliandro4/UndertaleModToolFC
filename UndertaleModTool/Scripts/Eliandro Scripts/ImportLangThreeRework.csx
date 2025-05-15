using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using UndertaleModLib;
using UndertaleModLib.Models;

EnsureDataLoaded();
Regex StringsIdRegex = new Regex(@"""\[(.*?)\][^""]*"":");
Regex LangsStringsRegex = new Regex(@": ""((?:[^""\\]|\\.)*)""");
string path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");

if (File.Exists(path))
{
    string[] lines = File.ReadAllLines(path);
    SetProgressBar(null, "Strings", 0, lines.Length);
    StartProgressBarUpdater();
    await TaskImportLang(lines);
    await StopProgressBarUpdater();
    HideProgressBar();
}

async Task TaskImportLang(string[] lines)
{
    await Task.Run(() => Parallel.ForEach(lines, line => {ImportLang(line);}));
}

void ImportLang(string line)
{
    Match strings_id_match = StringsIdRegex.Match(line);
    Match lang_strings_match = LangsStringsRegex.Match(line);
    if (strings_id_match.Groups[1].Value != String.Empty)
    {
        Data.Strings[int.Parse(strings_id_match.Groups[1].Value)].Content = lang_strings_match.Groups[1].Value;
    }
    IncrementProgressParallel();
}