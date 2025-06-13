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
Regex rnd_regex = new Regex(@"^[^\u3040-\u30FF\u4E00-\u9FFF\u31F0-\u31FF]+$");

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
Dictionary<string, string> ja_lang_dict_org = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ja_lang_path));
Dictionary<string, string> ja_lang_dict = [];
foreach (KeyValuePair<string, string> kvp in ja_lang_dict_org)
{
    int tmp_string_index = Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == kvp.Key));
    if (tmp_string_index != -1) {
        ja_lang_dict.Add(kvp.Key, kvp.Value);
    }
}
List<string> ja_lang_keys = ja_lang_dict.Keys.ToList();
List<string> ja_lang_values = ja_lang_dict.Values.ToList();

for (int iteracoes = 0; iteracoes < ja_lang_keys.Count; iteracoes++)
{
    string ja_lang_line = ja_lang_keys[iteracoes];
    int tmp_index = ja_lang_values.IndexOf(ja_lang_dict[ja_lang_line]);
    string result = "null";
    int string_index = Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == ja_lang_line));
    if (string_index != -1) {
        bool is_nulo = (
            Data.Strings[string_index + 1].Content.Contains("gml_") ||
            Data.Strings[string_index + 1].Content.Contains("obj_") ||
            Data.Strings[string_index + 1].Content.Contains("DEVICE_") ||
            ((Data.Strings[string_index + 1].Content.Split(" ").Length == 1) &&
            ((Data.Strings[string_index + 1].Content.ToLower() == Data.Strings[string_index + 1].Content) ||
            (Data.Strings[string_index + 1].Content.ToUpper() == Data.Strings[string_index + 1].Content)
            ))
        );
        if (rnd_regex.Match(ja_lang_dict[ja_lang_line]).Success) {
            result = ja_lang_dict[ja_lang_line];
        }
        else if ((tmp_index != -1) && (tmp_index < (iteracoes - 1))) {
            result = lang_entries[ja_lang_keys[tmp_index]];
        }
        else if (is_nulo) {
            bool encontrado = false;
            for (int code_index = 0; code_index < script_list_content.Length; code_index++)
            {
                if (encontrado) { break; }
                string DecompiledCode = GetDecompiledText(Data.Code.ByName(script_list_content[code_index].Trim()));
                List<Match> matchos = code_regex.Matches(DecompiledCode).Cast<Match>().ToList();
                Match match = matchos.FirstOrDefault(m => m.Groups.Count > 1 && m.Groups[1].Value == ja_lang_line);
                if (match?.Success == true)
                {
                    int match_index = matchos.IndexOf(match);
                    encontrado = true;
                    result = matchos[match_index - 1].Groups[1].Value;
                    break;
                }
            }
        }
        else
        {
            result = Data.Strings[string_index + 1].Content;
        }
        lang_entries.Add(ja_lang_line, UnescapeText(result));
        Console.WriteLine(ja_lang_line + " : " + result);
    }
}

string json = JsonConvert.SerializeObject(lang_entries, Formatting.Indented);
File.WriteAllText(en_lang_path, json);

public static string UnescapeText(string text)
{
    // TODO: optimize this? seems like a very whacky thing to do... why do they have escaped text in the first place?
    return text.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
}