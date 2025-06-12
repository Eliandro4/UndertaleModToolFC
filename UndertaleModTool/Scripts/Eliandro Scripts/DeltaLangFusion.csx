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

string[] script_list_content = await File.ReadAllLinesAsync(script_list_path);
Dictionary<string, string> lang_entries = [];
Dictionary<string, string> ja_lang_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ja_lang_path));
foreach (string ja_lang_line in ja_lang_dict.Keys)
{
    int string_index = Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == ja_lang_line));
    if (string_index != -1) {
        bool is_nulo = (
            Data.Strings[string_index + 1].Content.Contains("gml_") ||
            Data.Strings[string_index + 1].Content.Contains("obj_") ||
            Data.Strings[string_index + 1].Content.Contains("DEVICE_")
        );
        if (string.IsNullOrWhiteSpace(ja_lang_dict[ja_lang_line])) {
            lang_entries.Add(ja_lang_line, ja_lang_dict[ja_lang_line]);
            Console.WriteLine(ja_lang_line + " : " + ja_lang_dict[ja_lang_line].Trim());
        }
        else if (is_nulo) {
            bool encontrado = false;
            for (int code_index = 0; code_index < script_list_content.Length; code_index++)
            {
                if (encontrado) { break; }
                string DecompiledCode = GetDecompiledText(Data.Code.ByName(script_list_content[code_index].Trim()));
                if (DecompiledCode.Contains("\"" + ja_lang_line + "\""))
                {
                    MatchCollection matchos = code_regex.Matches(DecompiledCode);
                    for (int i = 0; i < matchos.Count(); i++)
                    {
                        if (matchos[i].Groups[1].Value == ja_lang_line)
                        {
                            encontrado = true;
                            string script_string = matchos[i - 1].Groups[1].Value;
                            lang_entries.Add(ja_lang_line, string.IsNullOrWhiteSpace(matchos[i - 1].Groups[1].Value) ? "null" : matchos[i - 1].Groups[1].Value);
                            Console.WriteLine(ja_lang_line + " : " + (string.IsNullOrWhiteSpace(matchos[i - 1].Groups[1].Value) ? "null" : matchos[i - 1].Groups[1].Value.Trim()));
                            break;
                        }
                        else if ((i == (matchos.Count() - 1)) && (code_index == script_list_content.Length))
                        {
                            Console.WriteLine(ja_lang_line + " : " + "null");
                        }
                    }
                }
            }
        }
        else {
            string exp_lang_string = Data.Strings[string_index + 1].Content;
            lang_entries.Add(ja_lang_line, exp_lang_string);
            Console.WriteLine(ja_lang_line + " : " + exp_lang_string);
        }
    }
}

string json = JsonConvert.SerializeObject(lang_entries, Formatting.Indented);
File.WriteAllText(en_lang_path, json);