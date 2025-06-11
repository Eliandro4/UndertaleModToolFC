using System.Linq;
using System.Text;
using UndertaleModLib;
using UndertaleModLib.Scripting;
using UndertaleModLib.Util;
using UndertaleModLib.Decompiler;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Scripting;

Regex lang_id = new Regex(@"""([^""]+)"":");
ScriptMessage("Selecione um diretório");
//string selected_path = PromptChooseDirectory();
//if (string.IsNullOrWhiteSpace(selected_path)) { return; }
ScriptMessage("Selecione a lang japonesa");
string ja_lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(ja_lang_path)) { return; }
string[] ja_lang_content = File.ReadAllLines(ja_lang_path);
//var files = Directory.EnumerateFiles(selected_path, "*.gml", SearchOption.AllDirectories);
foreach (string line in ja_lang_content)
{
    Match lang_matches = lang_id.Match(line);
    Console.WriteLine(lang_matches.Groups[1].Value);
}