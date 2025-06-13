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
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Underanalyzer.Decompiler;

EnsureDataLoaded();

Regex code_regex = new Regex(@"""((?:[^""\\\r\n]|\\.)*)""");
//Regex rnd_regex = new Regex(@"^[^\u3040-\u30FF\u4E00-\u9FFF\u31F0-\u31FF]+$");

ScriptMessage("Selecione a lang japonesa");
string ja_lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(ja_lang_path)) { return; }

ScriptMessage("Selecione um arquivo com a lista de scripts com texto");
string script_list_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(script_list_path)) { return; }

ScriptMessage("Selecione um arquivo de saída");
string en_lang_path = PromptSaveFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(en_lang_path)) { return; }

string[] script_list = await File.ReadAllLinesAsync(script_list_path);
Dictionary<string, string> lang_entries = [];
Dictionary<string, string> ja_lang_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ja_lang_path));
List<string> script_list_contents = [];
foreach (string CODIOS in script_list)
{
    script_list_contents.Add(GetDecompiledText(Data.Code.ByName(CODIOS.Trim())));
}
List<string> ja_lang_keys = ja_lang_dict.Keys.ToList();
List<string> ja_lang_values = ja_lang_dict.Values.ToList();

for (int iteracoes = 0; iteracoes < ja_lang_keys.Count; iteracoes++)
{
    string ja_lang_line = ja_lang_keys[iteracoes];
    string result = "null";
    int string_index = Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == ja_lang_line));
    if (string_index != -1) {
        bool is_nulo = true;
        if (string.IsNullOrWhiteSpace(ja_lang_line)) {
            result = ja_lang_dict[ja_lang_line];
        }
        else if (is_nulo) {
            bool encontrado = false;
            Parallel.For(0, script_list.Length, code_index =>
            {
                if (encontrado) { return; }
                string DecompiledCode = script_list_contents[code_index];
                List<Match> matchos = code_regex.Matches(DecompiledCode).Cast<Match>().ToList();
                Match match = matchos.FirstOrDefault(m => m.Groups.Count > 1 && m.Groups[1].Value == ja_lang_line);
                if (match?.Success == true)
                {
                    int match_index = matchos.IndexOf(match);
                    encontrado = true;
                    result = matchos[match_index - 1].Groups[1].Value;
                    return;
                }
            }
            );
        }
        else
        {
            result = Data.Strings[string_index + 1].Content;
        }
        lang_entries.Add(ja_lang_line, Regex.Unescape(result));
        Console.WriteLine(ja_lang_line + " : " + result);
    }
}

string json = JsonConvert.SerializeObject(lang_entries, Formatting.Indented);
File.WriteAllText(en_lang_path, json);