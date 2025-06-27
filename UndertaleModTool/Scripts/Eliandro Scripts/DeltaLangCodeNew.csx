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
using Underanalyzer.Decompiler.AST;
using Underanalyzer.Compiler.Nodes;

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
    CheckChildren(DecompiledCode);
}

Console.WriteLine($"Total de IStatements processados: {algodeveriaacontecer}");
Console.WriteLine($"Total de entradas encontradas: {achado}");
string json = JsonConvert.SerializeObject(lang_entries, Formatting.Indented);
File.WriteAllText(en_lang_path, json);

void do_smt(FunctionCallNode funcCall)
{
    achado++;
    StringNode valueString = (StringNode)funcCall.Arguments.First();
    StringNode keyString = (StringNode)funcCall.Arguments.Last();
    lang_entries[keyString.Value.Content] = valueString.Value.Content;
}

void do_smt_msgsetloc(FunctionCallNode funcCall)
{
    achado++;
    StringNode valueString = (StringNode)funcCall.Arguments[1];
    StringNode keyString = (StringNode)funcCall.Arguments.Last();
    lang_entries[keyString.Value.Content] = valueString.Value.Content;
}

void do_find(FunctionCallNode funcCall)
{
    if (
        funcCall.Function.Name.Content == "gml_Script_c_msgnextloc" ||
        funcCall.Function.Name.Content == "gml_Script_c_msgnextsubloc" ||
        funcCall.Function.Name.Content == "gml_Script_msgnextloc" ||
        funcCall.Function.Name.Content == "gml_Script_msgnextsubloc" ||
        funcCall.Function.Name.Content == "gml_Script_stringsetloc" ||
        funcCall.Function.Name.Content == "gml_Script_stringsetsubloc"
        )
    {
        do_smt(funcCall);
    }
    else if (
        funcCall.Function.Name.Content == "gml_Script_c_msgsetloc" ||
        funcCall.Function.Name.Content == "gml_Script_c_msgsetsubloc" ||
        funcCall.Function.Name.Content == "gml_Script_msgsetloc" ||
        funcCall.Function.Name.Content == "gml_Script_msgsetsubloc"
        )
    {
        do_smt_msgsetloc(funcCall);
    }
}

void CheckChildren(BlockNode block)
{
    foreach (IStatementNode stmt in block.Children)
    {
        if (stmt is FunctionCallNode funcCall)
        {
            do_find(funcCall);
        }
        else if (stmt is IfNode ifNode)
        {
            if (ifNode.TrueBlock != null)
                CheckChildren(ifNode.TrueBlock);
            if (ifNode.ElseBlock != null)
                CheckChildren(ifNode.ElseBlock);
        }
        else if (stmt is ForLoopNode forloop)
        {
            CheckChildren(forloop.Body);
        }
        else if (stmt is SwitchNode switchcase)
        {
            CheckChildren(switchcase.Body);
        }
        else if (stmt is WhileLoopNode whileLoop)
        {
            CheckChildren(whileLoop.Body);
        }
        else if (stmt is WithLoopNode withLoop)
        {
            CheckChildren(withLoop.Body);
        }
        else if (stmt is RepeatLoopNode RepeatLoop)
        {
            CheckChildren(RepeatLoop.Body);
        }
        else if (stmt is StructNode structNode)
        {
            CheckChildren(structNode.Body);
        }
        else if (stmt is DoUntilLoopNode doUntilLoopNode)
        {
            CheckChildren(doUntilLoopNode.Body);
        }
        else if (stmt is StaticInitNode staticInitNode)
        {
            CheckChildren(staticInitNode.Body);
        }
        else if (stmt is BlockNode blockNode)
        {
            CheckChildren(blockNode);
        }
        else if (stmt is TryCatchNode tryCatchNode)
        {
            CheckChildren(tryCatchNode.Try);
            if (tryCatchNode.Catch != null)
                CheckChildren(tryCatchNode.Catch);
            if (tryCatchNode.Finally != null)
                CheckChildren(tryCatchNode.Finally);
        }
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