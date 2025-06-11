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
using System.Text.Json;

Regex lang_regex = new Regex(@"""([^""]+)"":");
Regex string_regex = new Regex(@"""([^""]+)""");
ScriptMessage("Selecione um diretório");
string selected_path = PromptChooseDirectory();
if (string.IsNullOrWhiteSpace(selected_path)) { return; }
ScriptMessage("Selecione a lang japonesa");
string ja_lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(ja_lang_path)) { return; }
ScriptMessage("Selecione um arquivo de saída");
string en_lang_path = PromptSaveFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(en_lang_path)) { return; }
string[] ja_lang_content = File.ReadAllLines(ja_lang_path);
var gml_files = Directory.EnumerateFiles(selected_path, "*.gml", SearchOption.AllDirectories);
Dictionary<string, string> lang_entries = [];
foreach (string ja_lang_line in ja_lang_content)
{
    Match lang_match = lang_regex.Match(ja_lang_line);
    if (lang_match.Success)
    {
        bool encontrado = false;
        string searching_for = lang_match.Groups[1].Value;
        foreach (string gml_file in gml_files)
        {
            if (encontrado) { break; }
            string[] gml_file_content = File.ReadAllLines(gml_file);
            foreach (string line in gml_file_content)
            {
                MatchCollection script_strings = string_regex.Matches(gml_file);
                if (script_strings.Count() != 2) { continue; }
                if (script_strings[1].Groups[1].Value == searching_for)
                {
                    lang_entries.Add(searching_for, script_strings[0].Groups[1].Value);
                    encontrado = true;
                    break;
                }
            }
        }
    }
}

string json = JsonSerializer.Serialize(lang_entries);
File.WriteAllText(en_lang_path, json);