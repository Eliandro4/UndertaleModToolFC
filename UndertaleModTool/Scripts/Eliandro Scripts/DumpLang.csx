using System.Linq;
using System.Text;
using UndertaleModLib;
using UndertaleModLib.Scripting;
using UndertaleModLib.Util;
using UndertaleModLib.Decompiler;
using System.Text.RegularExpressions;

EnsureDataLoaded();

bool id = ScriptQuestion("Use IDs?");
string patho = PromptChooseDirectory();
string extractedStrings = "{";
string[] codeArray = Data.Code.Select(c => c.Name.Content).ToArray();
UndertaleCode codo = Data.Code.ByName(codeArray[0]);
string corio;
Regex regex = new Regex(@"""((?:[^""\\]|\\.)*)""");
SetProgressBar(null, "Scanning Scripts", 0, Data.Scripts.Count);
StartProgressBarUpdater();
//foreach (string code in codeArray)
for (int jay = 0; jay < codeArray.Length; jay++)
{
    codo = Data.Code.ByName(codeArray[jay]);
    corio = GetDecompiledText(codo);
    MatchCollection matches = regex.Matches(corio);
    for (int i = 0; i < matches.Count; i++)
    {
        Match match = matches[i];
        string val = match.Groups[1].Value;
        if ((!extractedStrings.Contains("\"" + val + "\"")) && (!val.Contains("gml_GlobalScript")) && (!val.Contains("gml_Object")) && (!val.Contains("rm_")) && (!val.Contains("obj_")) && (!val.Contains("bg_")) && (!val.Contains("spr_")) && (!val.Contains("_sound")))
        {
            int string_id = (Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == val)));
            if ((string_id) != -1)
            {
                extractedStrings += $"\n\t\"{(id ? $"[{string_id}]" : String.Empty)}{codo.Name.ToString().Replace("\"", "")}_{i}\": \"{val}\",";
            }
        }
    }
    IncrementProgress();
}
StopProgressBarUpdater();
HideProgressBar();
extractedStrings += "\n}";
extractedStrings = extractedStrings.Replace("\",\n}", "\"\n}");
string resultpath = Path.Combine(patho, "exported_lang_StringsId.json");
File.WriteAllText(resultpath, extractedStrings);
ScriptMessage($"\nLang file created sucessfully.\n\nLocation: {resultpath}");