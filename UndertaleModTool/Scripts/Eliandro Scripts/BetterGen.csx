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

ScriptMessage("Selecione a lang do script");
string lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(lang_path)) { return; }

ScriptMessage("Selecione o better de destino");
string en_lang_path = PromptSaveFile("lang_en.json", "JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(en_lang_path)) { return; }

Dictionary<string, string> lang_dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(lang_path));
List<string> lang_keys = lang_dict.Keys.ToList();

for (int iteracoes = 0; iteracoes < lang_keys.Count; iteracoes++)
{
    string ja_lang_line = lang_keys[iteracoes];
    if (lang_dict[ja_lang_line] != null)
    {
        int string_index = Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == ja_lang_line));
        if (string_index != -1) {
            Data.Strings[string_index + 1].Content = lang_dict[ja_lang_line];
        }
    }
}

LangEntry lang_entries = new();
lang_entries.Strings = Data.Strings.Select(e => e.Content).ToList();

string json = JsonConvert.SerializeObject(lang_entries, Formatting.Indented);
File.WriteAllText(en_lang_path, json);

class LangEntry
{
    public List<string> Strings { get; set; }
}