using System.Linq;
using System.Text;
using UndertaleModLib;
using UndertaleModLib.Scripting;
using UndertaleModLib.Util;
using UndertaleModLib.Decompiler;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Newtonsoft.Json;

Regex lang_regex = new Regex(@"""([^""]+)"":");
ScriptMessage("Selecione a lang japonesa");
string ja_lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(ja_lang_path)) { return; }
ScriptMessage("Selecione um arquivo de saída");
string en_lang_path = PromptSaveFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(en_lang_path)) { return; }
List<string> ja_lang_content = File.ReadAllLines(ja_lang_path).ToList();
Dictionary<string, string> lang_entries = [];
foreach (string ja_lang_line in ja_lang_content)
{
    Match lang_match = lang_regex.Match(ja_lang_line);
    if (lang_match.Success)
    {
        int string_index = Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == lang_match.Groups[1].Value));
        if (string_index != -1) {
            string exp_lang_string = Data.Strings[string_index + 1].Content;
            lang_entries.Add(lang_match.Groups[1].Value, exp_lang_string);
            Console.WriteLine(lang_match.Groups[1].Value + " : " + exp_lang_string);
        }
    }
}

string json = JsonConvert.SerializeObject(lang_entries, Formatting.Indented);
File.WriteAllText(en_lang_path, json);