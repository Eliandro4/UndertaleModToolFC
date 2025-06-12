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

EnsureDataLoaded();

Regex lang_regex = new Regex(@"""([^""]+)"":");
Regex code_regex = new Regex(@"""((?:[^""\\\r\n]|\\.)*)""");

ScriptMessage("Selecione a lang japonesa");
string ja_lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(ja_lang_path)) { return; }

ScriptMessage("Selecione um arquivo com a lista de scripts com texto");
string script_list_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(script_list_path)) { return; }

ScriptMessage("Selecione um arquivo de saída");
string en_lang_path = PromptSaveFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(en_lang_path)) { return; }

string ja_lang_content = File.ReadAllText(ja_lang_path);
string[] script_list_content = await File.ReadAllLinesAsync(script_list_path);
Dictionary<string, string> lang_entries = [];
MatchCollection Matchez = lang_regex.Matches(ja_lang_content);
foreach (Match ja_lang_line in Matchez)
{
    int string_index = Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == ja_lang_line.Groups[1].Value));
    if (string_index != -1) {
        bool is_nulo = (
            Data.Strings[string_index + 1].Content.Contains("gml_") ||
            Data.Strings[string_index + 1].Content.Contains("obj_") ||
            Data.Strings[string_index + 1].Content.Contains("DEVICE_")
        );
        if (is_nulo) {
            bool encontrado = false;
            foreach (string Code in script_list_content)
            {
                if (encontrado) { break; }
                string DecompiledCode = GetDecompiledText(Data.Code.ByName(Code.Trim()));
                if (DecompiledCode.Contains("\"" + ja_lang_line.Groups[1].Value + "\""))
                {
                    MatchCollection matchos = code_regex.Matches(DecompiledCode);
                    for (int i = 0; i < matchos.Count(); i++)
                    {
                        if (matchos[i].Groups[1].Value == ja_lang_line.Groups[1].Value)
                        {
                            encontrado = true;
                            string script_string = matchos[i - 1].Groups[1].Value;
                            lang_entries.Add(ja_lang_line.Groups[1].Value, string.IsNullOrWhiteSpace(matchos[i - 1].Groups[1].Value.Trim()) ? "null" : matchos[i - 1].Groups[1].Value.Trim());
                            Console.WriteLine(ja_lang_line.Groups[1].Value + " : " + (string.IsNullOrWhiteSpace(matchos[i - 1].Groups[1].Value.Trim()) ? "null" : matchos[i - 1].Groups[1].Value.Trim()));
                        }
                    }
                }
            }
        }
        else {
            string exp_lang_string = Data.Strings[string_index + 1].Content;
            lang_entries.Add(ja_lang_line.Groups[1].Value, exp_lang_string);
            Console.WriteLine(ja_lang_line.Groups[1].Value + " : " + exp_lang_string);
        }
    }
}

string json = JsonConvert.SerializeObject(lang_entries, Formatting.Indented);
File.WriteAllText(en_lang_path, json);