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

ScriptMessage("Selecione a lang do script");
string ja_lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(ja_lang_path)) { return; }

ScriptMessage("Selecione a lang do deltatranslate");
string delta_lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(delta_lang_path)) { return; }

ScriptMessage("Selecione um arquivo de saída");
string en_lang_path = PromptSaveFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(en_lang_path)) { return; }

string lang_entries = "{\n";
Dictionary<string, string> ja_lang_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ja_lang_path));
Dictionary<string, string> delta_lang_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(delta_lang_path));
List<string> ja_lang_keys = ja_lang_dict.Keys.ToList();
List<string> delta_lang_keys = delta_lang_dict.Keys.ToList();

for (int iteracoes = 0; iteracoes < delta_lang_keys.Count; iteracoes++)
{
    string ja_lang_line = delta_lang_keys[iteracoes];
    string result = null;
    int string_index = ja_lang_keys.IndexOf(ja_lang_line);
    if (string_index != -1) {
        result = ja_lang_dict[ja_lang_line];
    }
    lang_entries += $"    \"{ja_lang_line}\": {JsonConvert.SerializeObject(result)},\n";
    Console.WriteLine(ja_lang_line + " : " + result);
}
lang_entries += "}";
File.WriteAllText(en_lang_path, lang_entries);