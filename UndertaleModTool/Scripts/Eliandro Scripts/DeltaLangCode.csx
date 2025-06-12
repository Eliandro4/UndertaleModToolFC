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
string script_list = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(script_list)) { return; }

ScriptMessage("Selecione um arquivo de saída");
string en_lang_path = PromptSaveFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(en_lang_path)) { return; }

string[] ja_lang_content = File.ReadAllLines(ja_lang_path);
string[] GameCodes = File.ReadAllLines(script_list);
Dictionary<string, string> lang_entries = [];
foreach (string ja_lang_line in ja_lang_content)
{
    bool encontrado = false;
    Match lang_match = lang_regex.Match(ja_lang_line);
    if (lang_match.Success)
    {
        if (Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == lang_match.Groups[1].Value)) != -1)
        {
            encontrado = false;
            string searching_for = lang_match.Groups[1].Value;
            foreach (string Code in GameCodes)
            {
                if (encontrado)
                {
                    encontrado = false;
                    break;
                }
                string DecompiledCode = GetDecompiledText(Data.Code.ByName(Code.Trim()));
                if (DecompiledCode.Contains("\"" + searching_for + "\""))
                {
                    MatchCollection matchos = code_regex.Matches(DecompiledCode);
                    for (int i = 0; i < matchos.Count(); i++)
                    {
                        if (matchos[i].Groups[1].Value == searching_for)
                        {
                            encontrado = true;
                            lang_entries.Add(searching_for, matchos[i - 1].Groups[1].Value);
                            Console.WriteLine(matchos[i - 1].Groups[1].Value.Trim());
                            break;
                        }
                    }
                }
            }
        }
    }
}

string json = JsonConvert.SerializeObject(lang_entries, Formatting.Indented);
File.WriteAllText(en_lang_path, json);