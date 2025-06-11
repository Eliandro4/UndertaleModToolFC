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
Regex code_regex = new Regex(@"""((?:[^""\\\r\n]|\\"")*)""");
ScriptMessage("Selecione a lang japonesa");
string ja_lang_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(ja_lang_path)) { return; }
/*
ScriptMessage("Selecione um arquivo de saída");
string en_lang_path = PromptSaveFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(en_lang_path)) { return; }
*/
string[] ja_lang_content = File.ReadAllLines(ja_lang_path);
List<UndertaleCode> GameCodes = Data.Code.Where(c => c.ParentEntry is null).ToList();
Dictionary<string, string> lang_entries = [];
List<string> strings_capturadas = new List<string>();
foreach (string ja_lang_line in ja_lang_content)
{
    bool encontrado = false;
    Match lang_match = lang_regex.Match(ja_lang_line);
    if (lang_match.Success)
    {
        encontrado = false;
        string searching_for = lang_match.Groups[1].Value;
        foreach (UndertaleCode Code in GameCodes)
        {
            string DecompiledCode = GetDecompiledText(Code);
            MatchCollection matchos = code_regex.Matches(DecompiledCode);
            foreach (Match matcho in matchos)
            {
                strings_capturadas.Add(matcho.Groups[1].Value);
                //Console.WriteLine(matcho.Groups[1].Value);
            }
            /*
            if (better_line.Content == searching_for) {
                string exp_lang_string = Data.Strings[Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == better_line.Content)) + 1].Content;
                lang_entries.Add(searching_for, exp_lang_string);
                Console.WriteLine(searching_for + " : " + exp_lang_string);
                encontrado = true;
                break;
            }
            */
        }
    }
}

/*
string json = JsonSerializer.Serialize(lang_entries);
File.WriteAllText(en_lang_path, json);
*/