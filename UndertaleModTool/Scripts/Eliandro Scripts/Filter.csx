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

EnsureDataLoaded();

Regex code_regex = new Regex(@"""((?:[^""\\\r\n]|\\.)*)""");
//Regex rnd_regex = new Regex(@"^[^\u3040-\u30FF\u4E00-\u9FFF\u31F0-\u31FF]+$");

ScriptMessage("Selecione a lang do script");
string ja_lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(ja_lang_path)) { return; }

ScriptMessage("Selecione a lang em ingrês");
string eno_lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(eno_lang_path)) { return; }

ScriptMessage("Selecione a lang em português");
string delta_lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(delta_lang_path)) { return; }

ScriptMessage("Selecione um arquivo de saída");
string en_lang_path = PromptSaveFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(en_lang_path)) { return; }

Dictionary<string, string> lang_entries = [];
Dictionary<string, string> ja_lang_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(ja_lang_path));
Dictionary<string, string> en_lang_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(eno_lang_path));
Dictionary<string, string> delta_lang_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(delta_lang_path));
List<string> ja_lang_keys = ja_lang_dict.Keys.ToList();
List<string> delta_lang_keys = delta_lang_dict.Keys.ToList();

for (int iteracoes = 0; iteracoes < ja_lang_keys.Count; iteracoes++)
{
    string ja_lang_line = ja_lang_keys[iteracoes];
    string result = null;
    if (ja_lang_dict[ja_lang_line] != null)
    {
        int string_index = delta_lang_keys.IndexOf(ja_lang_line);
        if (string_index != -1) {
            if (ja_lang_dict[ja_lang_line] == en_lang_dict[ja_lang_line])
            {
                result = delta_lang_dict[ja_lang_line];
            }
            else
            {
                result = ja_lang_dict[ja_lang_line];
            }
        }
        else
        {
            result = ja_lang_dict[ja_lang_line];
        }
    }
    lang_entries.Add(ja_lang_line, result);
    Console.WriteLine(ja_lang_line + " : " + result);
}

string json = JsonConvert.SerializeObject(lang_entries, Formatting.Indented);
File.WriteAllText(en_lang_path, json);