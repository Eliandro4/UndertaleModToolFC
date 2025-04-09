using System.Linq;
using System.Text;
using UndertaleModLib;
using UndertaleModLib.Scripting;
using UndertaleModLib.Util;
using UndertaleModLib.Decompiler;
using System.Text.RegularExpressions;

EnsureDataLoaded();

bool UseID = true;
string patho = PromptChooseDirectory();
string extractedStrings = "{";
string[] codeArray = Data.Code.Select(c => c.Name.Content).ToArray();
UndertaleCode codo = Data.Code.ByName(codeArray[0]);
string corio;
int jay = 0;
Regex regex = new Regex(@"""((?:[^""\\]|\\.)*)""");
SetProgressBar(null, "Scanning Scripts", jay, Data.Scripts.Count);
StartProgressBarUpdater();
//foreach (string code in codeArray)
for (jay = 0; jay < codeArray.Length; jay++)
{
    SetProgressBar(null, "Scanning Scripts", jay, codeArray.Length);
    codo = Data.Code.ByName(codeArray[jay]);
    corio = GetDecompiledText(codo);
    MatchCollection matches = regex.Matches(corio);
    for (int i = 0; i < matches.Count; i++)
    {
        Match match = matches[i];
        string val = match.Groups[1].Value;
        if ((!extractedStrings.Contains(val)) && (!val.Contains("gml_GlobalScript")) && (!val.Contains("gml_Object")) && (!val.Contains("rm_")) && (!val.Contains("obj_")) && (!val.Contains("bg_")) && (!val.Contains("spr_")) && (!val.Contains("_sound")))
        {
            if ((Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == val)) != -1) && UseID)
            {
                extractedStrings += $"\n\t\"[{Data.Strings.IndexOf(Data.Strings.FirstOrDefault(e => e.Content == val))}]{codo.Name.ToString().Replace("\"", "")}_{i}\": \"{val}\",";
            }
            else if (!UseID)
            {
                extractedStrings += $"\n\t\"{codo.Name.ToString().Replace("\"", "")}_{i}\": \"{val}\",";
            }
        }
    }
}
StopProgressBarUpdater();
HideProgressBar();
extractedStrings += "\n}";
extractedStrings = extractedStrings.Replace("\",\n}", "\"\n}");
string exo = UseID ?  "StringsId" : "WStringsID";
File.WriteAllText(patho + $"\\exported_lang_{exo}.json", extractedStrings);
ScriptMessage($"\nLang file created sucessfully.\n\nLocation: {Environment.CurrentDirectory + $"\\exported_lang_{exo}.json"}");