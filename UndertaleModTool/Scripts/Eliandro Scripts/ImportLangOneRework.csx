using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Scripting;
using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Compiler;
using UndertaleModLib.Decompiler;

EnsureDataLoaded();
Regex script_names = new Regex(@"""([^""]+)_\d+"":");
Regex lang_strings = new Regex(@": \s*""([^""\\]*(\\.[^""\\]*)*)""");
Regex strings_id = new Regex(@"_(\d+)""\s*:");
Regex script_strings = new Regex(@"""((?:[^""\\]|\\.)*)""");
List<string> arustringos = Data.Strings.Where(f => f is not null).Select(f => f.ToString().Replace("\"", "")).ToList();
string path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
string[] lines = File.ReadAllLines(path);

SetProgressBar(null, "Importing Lang", 0, lines.Length);
StartProgressBarUpdater();

foreach (string line in lines) { TaskImportLang(line); }

await StopProgressBarUpdater();
HideProgressBar();
ScriptMessage("\nLang imported succesfully\n");

void TaskImportLang(string line)
{
    Match scripto_namos = script_names.Match(line);
    Match strings_langos = lang_strings.Match(line);
    Match stringos_id = strings_id.Match(line);
    if (scripto_namos.Groups[1].Value != "")
    {
        UndertaleCode codo = Data.Code.ByName(scripto_namos.Groups[1].Value);
        //Console.WriteLine(scripto_namos.Groups[1].Value);
        string corio = GetDecompiledText(codo);
        MatchCollection scripts_stringos = script_strings.Matches(corio);
        for (int i = 0; i < scripts_stringos.Count; i++)
        {
            //Console.WriteLine(stringos_id.Groups[1].Value + " " + i);
            if (stringos_id.Groups[1].Value == $"{i}")
            {
                //Console.WriteLine("EQUALIDADE");
                int aoi = Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == scripts_stringos[i].Groups[1].Value));
                if (aoi != -1)
                {
                    Data.Strings[aoi].Content = strings_langos.Groups[1].Value;
                }
            }
        }
    }
    IncrementProgress();
}