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
using Underanalyzer;
using Underanalyzer.Decompiler;
using Underanalyzer.Decompiler.AST;
using Underanalyzer.Compiler.Nodes;
using System.Runtime.Serialization;
using Internal;

EnsureDataLoaded();

GlobalDecompileContext globalDecompileContext = new(Data);
IDecompileSettings decompilerSettings = new DecompileSettings();

ScriptMessage("Selecione um arquivo com a lista de scripts com texto");
string script_list_path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(script_list_path)) { return; }

ScriptMessage("Selecione um arquivo de saída");
string en_lang_path = PromptSaveFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
if (string.IsNullOrWhiteSpace(en_lang_path)) { return; }

LISTAS script_list_obj = JsonConvert.DeserializeObject<LISTAS>(File.ReadAllText(script_list_path));
Dictionary<string, string> lang_entries = [];
List<string> script_list = [];
switch (Data.GeneralInfo.DisplayName.ToString().Replace("\"", ""))
{
    case "DELTARUNE Chapter 2":
        script_list = script_list_obj.LISTA_CH2;
        break;
    case "DELTARUNE Chapter 3":
        script_list = script_list_obj.LISTA_CH3;
        break;
    case "DELTARUNE Chapter 4":
        script_list = script_list_obj.LISTA_CH4;
        break;
    case "DELTARUNE Capítulo 4":
        script_list = script_list_obj.LISTA_CH4;
        break;
    default:
        ScriptMessage("Isso não é o Deltarune");
        return;
}

int achado = 0;
int algodeveriaacontecer = 0;

for (int code_index = 0; code_index < script_list.Count; code_index++)
{
    UndertaleCode code = Data.Code.ByName(script_list[code_index].Trim());
    var context = new DecompileContext(globalDecompileContext, code, decompilerSettings);
    BlockNode DecompiledCode = (BlockNode)context.DecompileToAST();
    Console.WriteLine($"Processando script {code_index + 1}/{script_list.Count}: {script_list[code_index]}");
    CheckChildren(DecompiledCode.EnumerateChildren());
}

Console.WriteLine($"Total de IStatements processados: {algodeveriaacontecer}");
Console.WriteLine($"Total de entradas encontradas: {achado}");
string json = JsonConvert.SerializeObject(lang_entries, Formatting.Indented);
File.WriteAllText(en_lang_path, json);

void do_smt(List<IExpressionNode> Arguments, bool msgsetloc_style = false)
{
    if (Arguments.Last() is StringNode keyString)
    {
        StringNode valueString = msgsetloc_style ?
            (StringNode)Arguments[1]:
            (StringNode)Arguments.First();
        if (!lang_entries.ContainsKey(keyString.Value.Content))
        {
            lang_entries.Add(keyString.Value.Content, valueString.Value.Content);
            achado++;
        }
    }
}

void do_find(string Name, List<IExpressionNode> Arguments)
{
    if (
        Name == "gml_Script_c_msgnextloc" ||
        Name == "gml_Script_c_msgnextsubloc" ||
        Name == "gml_Script_msgnextloc" ||
        Name == "gml_Script_msgnextsubloc" ||
        Name == "gml_Script_stringsetloc" ||
        Name == "gml_Script_stringsetsubloc"
        )
    {
        do_smt(Arguments);
    }
    else if (
        Name == "gml_Script_c_msgsetloc" ||
        Name == "gml_Script_c_msgsetsubloc" ||
        Name == "gml_Script_msgsetloc" ||
        Name == "gml_Script_msgsetsubloc"
        )
    {
        do_smt(Arguments, true);
    }
}

void CheckChildren(IEnumerable<IBaseASTNode> block)
{
    foreach (IBaseASTNode stmt in block)
    {
        if (stmt is FunctionCallNode funcCall)
        {
            do_find(funcCall.Function.Name.Content, funcCall.Arguments);
        }
        CheckChildren(stmt.EnumerateChildren());
        algodeveriaacontecer++;
    }
}

class LISTAS
{
    public List<string> LISTA_CH2 { get; set; }
    public List<string> LISTA_CH3 { get; set; }
    public List<string> LISTA_CH4 { get; set; }
    public LISTAS() {}
}