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
int ValFuncCount = 0;

for (int code_index = 0; code_index < script_list.Count; code_index++)
{
    UndertaleCode code = Data.Code.ByName(script_list[code_index].Trim());
    var context = new DecompileContext(globalDecompileContext, code, decompilerSettings);
    BlockNode DecompiledCode = (BlockNode)context.DecompileToAST();
    //Console.WriteLine($"Processando script {code_index + 1}/{script_list.Count}: {script_list[code_index]}");
    CheckChildren(DecompiledCode);
}

Console.WriteLine($"Total de IStatements processados: {algodeveriaacontecer}");
Console.WriteLine($"Total de entradas encontradas: {achado}");
Console.WriteLine($"Total de valores retornados de funções sendo atribuido como valor de variável: {ValFuncCount}");
string json = JsonConvert.SerializeObject(lang_entries, Formatting.Indented);
File.WriteAllText(en_lang_path, json);

void do_smt(List<IExpressionNode> Arguments)
{
    achado++;
    StringNode valueString = (StringNode)Arguments.First();
    StringNode keyString = (StringNode)Arguments.Last();
    lang_entries[keyString.Value.Content] = valueString.Value.Content;
}

void do_smt_msgsetloc(List<IExpressionNode> Arguments)
{
    achado++;
    StringNode valueString = (StringNode)Arguments[1];
    StringNode keyString = (StringNode)Arguments.Last();
    lang_entries[keyString.Value.Content] = valueString.Value.Content;
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
        do_smt_msgsetloc(Arguments);
    }
    else
    {
        CheckChildren_multi_istate(Arguments.Select(x => x as IStatementNode).ToList());
    }
}

void CheckChildren(BlockNode block)
{
    foreach (IStatementNode stmt in block.Children)
    {
        CheckChildren_single_istate(stmt);
    }
}

void CheckChildren_multi_istate(List<IStatementNode> block)
{
    foreach (IStatementNode stmt in block)
    {
        CheckChildren_single_istate(stmt);
    }
}

void CheckChildren_single_istate(IStatementNode stmt)
{
    if (stmt is FunctionCallNode funcCall)
    {
        do_find(funcCall.Function.Name.Content, funcCall.Arguments);
    }
    else if (stmt is FunctionDeclNode FuncDel)
    {
        CheckChildren(FuncDel.Body);
    }
    else if (stmt is VariableCallNode variableCallNode)
    {
        List<IExpressionNode> Argumentoos1 = [variableCallNode.Function];
        Argumentoos1.AddRange(variableCallNode.Arguments);
        if (variableCallNode.Instance != null)
        {
            Argumentoos1.Add(variableCallNode.Instance);
        }
        CheckChildren_multi_istate(Argumentoos1.Select(x => x as IStatementNode).ToList());
        ValFuncCount++;
    }
    else if (stmt is IfNode ifNode)
    {
        if (ifNode.Condition is ShortCircuitNode ifsc)
        {
            CheckChildren_multi_istate(ifsc.Conditions.Select(x => x as IStatementNode).ToList());
        }
        else if (ifNode.Condition is BinaryNode binarynode)
        {
            List<IExpressionNode> Argumenturos = [binarynode.Left, binarynode.Right];
            CheckChildren_multi_istate(Argumenturos.Select(x => x as IStatementNode).ToList());
        }
        /*
        else if (ifNode.Condition is VariableNode variaburonode)
        {
            CheckChildren_single_istate((IStatementNode)variaburonode.Left);
        }
        */
        //Console.WriteLine("Encontrado IfNode com FunctionCall: " + iffuncall.Function.Name.Content);
        //Console.WriteLine(typeof(IfNode).Name + " " + ifNode.Condition.GetType().Name);
        if (ifNode.TrueBlock != null)
            CheckChildren(ifNode.TrueBlock);
        if (ifNode.ElseBlock != null)
            CheckChildren(ifNode.ElseBlock);
    }
    else if (stmt is ForLoopNode forloop)
    {
        CheckChildren(forloop.Body);
        if (forloop.Incrementor != null)
            CheckChildren(forloop.Incrementor);
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
    else if (stmt is AssignNode Assino)
    {
        List<IExpressionNode> Argumentos1 = [Assino.Variable, Assino.Value];
        CheckChildren_multi_istate(Argumentos1.Select(x => x as IStatementNode).ToList());
    }
    else if (stmt is ConditionalNode conditionalNode)
    {
        List<IExpressionNode> Argumentos2 = [conditionalNode.Condition, conditionalNode.True, conditionalNode.False];
        CheckChildren_multi_istate(Argumentos2.Select(x => x as IStatementNode).ToList());
    }
    else if (stmt is NewObjectNode newObjectNode)
    {
        List<IExpressionNode> Argumentos3 = [newObjectNode.Function];
        Argumentos3.AddRange(newObjectNode.Arguments);
        CheckChildren_multi_istate(Argumentos3.Select(x => x as IStatementNode).ToList());
    }
    else if (stmt is ArrayInitNode arrayInitNode)
    {
        CheckChildren_multi_istate(arrayInitNode.Elements.Select(x => x as IStatementNode).ToList());
    }
    else if (stmt is NullishCoalesceNode nullishCoalesceNode)
    {
        List<IExpressionNode> Argumentos4 = [nullishCoalesceNode.Left, nullishCoalesceNode.Right];
        CheckChildren_multi_istate(Argumentos4.Select(x => x as IStatementNode).ToList());
    }
    else if (stmt is VariableNode variableNode)
    {
        List<IExpressionNode> Argumentos5 = [variableNode.Left];
        Argumentos5.AddRange(variableNode.ArrayIndices);
        CheckChildren_multi_istate(Argumentos5.Select(x => x as IStatementNode).ToList());
    }
    algodeveriaacontecer++;
}

class LISTAS
{
    public List<string> LISTA_CH2 { get; set; }
    public List<string> LISTA_CH3 { get; set; }
    public List<string> LISTA_CH4 { get; set; }
    public LISTAS() {}
}