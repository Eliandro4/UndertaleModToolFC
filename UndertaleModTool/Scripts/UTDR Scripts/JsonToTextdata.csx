using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using UndertaleModLib.Util;
using Underanalyzer.Decompiler;
using Underanalyzer.Decompiler.AST;

public string ApplyPatterns(string text, string[] patterns, string[] replacements)
{
    for (int i = 0; i < patterns.Length; i++)
    {
        text = Regex.Replace(text, patterns[i], replacements[i]);
    }
    return text;
}


string jsonFilePath = "";
ScriptMessage("Put the JSON path: ");
jsonFilePath = PromptLoadFile("json", "JSON files (.json)|*.json|All files|*");
if (jsonFilePath == null)
	throw new ScriptException("The JSON path was not set.");

ScriptMessage("JSON file loaded: " + jsonFilePath);

String lang = SimpleTextInput("Write the language to save [en,es,ru,etc]: ", "Language value to global.lang", "", true);
String langName = SimpleTextInput("Write the language NAME to settings menu: ", "Language NAME in settings Menu", "", true);

string txtFilePath = Path.Combine(ExePath + Path.DirectorySeparatorChar, "gml_Script_textdata_" + lang + ".gml");

using (StreamReader jsonFile = new StreamReader(jsonFilePath))
{
	string jsonContent = jsonFile.ReadToEnd();
	Dictionary<string, string> data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);


	string[] patterns = new string[]
	{
		"\""
	};
	string[] replacements = new string[]
	{
		"\"+ '\"'+\""
	};

	using (StreamWriter txtFile = new StreamWriter(txtFilePath, false))
	{
		txtFile.WriteLine("global.text_data_" + lang + " = ds_map_create()");
		foreach (KeyValuePair<string, string> pair in data)
		{
			string modifiedValue = ApplyPatterns(pair.Value, patterns, replacements);
			string line = $"ds_map_add(global.text_data_{lang}, \"{pair.Key}\", \"{modifiedValue}\")";
			txtFile.WriteLine(line);
		}
		txtFile.WriteLine($"ds_map_add(global.text_data_{lang}, \"settings_language_{lang}\", \"{langName}\")");
	}
}

ScriptMessage($"File saved in {txtFilePath}");

bool AutoImport = ScriptQuestion("Import generated File?");

if(AutoImport)
{
	bool stopOnError = ScriptQuestion("Stop importing on error?");
	SetProgressBar(null, "Files", 0, 1);
	StartProgressBarUpdater();
	SyncBinding("Strings, Code, CodeLocals, Scripts, GlobalInitScripts, GameObjects, Functions, Variables", true);
	await Task.Run(() => {
		IncrementProgress();
		ImportGMLFile(txtFilePath);
	});
	DisableAllSyncBindings();
	await StopProgressBarUpdater();
	HideProgressBar();
	ScriptMessage("File successfully imported.");

	if (Data?.GeneralInfo?.DisplayName?.Content.ToLower() == "deltarune chapter 1 & 2")
	{
		ScriptError("Error 0: Incompatible with the new Deltarune Chapter 1 & 2 demo");
		return;
	}
	else if (Data?.GeneralInfo?.DisplayName?.Content.ToLower() == "deltarune chapter 1&2")
	{
		ScriptError("Error 1: Incompatible with the new Deltarune Chapter 1 & 2 demo");
		return;
	}

	GlobalDecompileContext globalDecompileContext = new(Data);
	IDecompileSettings decompilerSettings = new DecompileSettings();
	UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data, globalDecompileContext, decompilerSettings);
	string obj_time_code = @"
	var i;
	if (os_type == os_windows)
		global.osflavor = 1
	else if (os_type == os_ps4 || os_type == os_psvita)
		global.osflavor = 4
	else
		global.osflavor = 2
	global.locale = ((os_get_language() + ""_"") + string_upper(os_get_region()))
	if (global.osflavor >= 3)
	{
		application_surface_enable(true)
		application_surface_draw_enable(false)
	}
	global.savedata_async_id = -1
	global.savedata_async_load = 0
	global.savedata_error = 0
	global.savedata_debuginfo = """"
	global.disable_os_pause = 0
	paused = false
	idle = 0
	idle_time = 0
	up = 0
	down = 0
	left = 0
	right = 0
	quit = 0
	try_up = 0
	try_down = 0
	try_left = 0
	try_right = 0
	canquit = 1
	h_skip = 0
	j_xpos = 0
	j_ypos = 0
	j_dir = 0
	j_fr = 0
	j_fl = 0
	j_fu = 0
	j_fd = 0
	j_fr_p = 0
	j_fl_p = 0
	j_fu_p = 0
	j_fd_p = 0
	for (i = 0; i < 12; i += 1)
	{
		j_prev[i] = 0
		j_on[i] = 0
	}
	global.button0 = 2
	global.button1 = 1
	global.button2 = 4
	global.analog_sense = 0.15
	global.analog_sense_sense = 0.01
	global.joy_dir = 0
	if (os_type == os_ps4 || os_type == os_psvita)
	{
		if (substr(global.locale, 1, 2) == ""ja"")
		{
			global.button0 = gp_face2
			global.button1 = gp_face1
		}
		else
		{
			global.button0 = gp_face1
			global.button1 = gp_face2
		}
		global.button2 = gp_face4
	}
	global.default_button0 = global.button0
	global.default_button1 = global.button1
	global.default_button2 = global.button2
	global.default_analog_sense = global.analog_sense
	global.default_analog_sense_sense = global.analog_sense_sense
	global.default_joy_dir = global.joy_dir
	global.screen_border_id = 0
	global.screen_border_active = false
	global.screen_border_alpha = 1
	global.screen_border_state = 0
	global.screen_border_dynamic_fade_id = 0
	global.screen_border_dynamic_fade_level = 0
	global.screen_border_activate_on_game_over = 0
	debug_r = 0
	debug_f = 0
	j1 = 0
	j2 = 0
	ja = 0
	j_ch = 0
	jt = 0
	if (global.osflavor >= 4)
	{
		j_ch = 1
		for (i = 0; i < gamepad_get_device_count(); i++)
		{
			if gamepad_is_connected(i)
				j_ch = (i + 1)
		}
	}
	spec_rtimer = 0
	global.endsong_loaded = 0
	control_init()
	ossafe_ini_open(""config.ini"")
	global.language = ini_read_string(""General"", ""lang"", substr(global.locale, 1, 2))
	ossafe_ini_close()
	scr_kanatype_init()
	script_execute(textdata_en)
	if (!(variable_global_exists((""text_data_"" + global.language))))
	{
		script_loaded = scr_assetGet(""textdata"")
		if (script_loaded != -1)
			script_execute(script_loaded)
		else
			global.language = ""en""
	}
	if (!variable_global_exists(""trophy_init_complete""))
	{
		global.trophy_init_complete = 0
		trophy_ts = -1
	}";

	List<string> langsToAdd = new List<string>();
	foreach (UndertaleScript scr in Data.Scripts)
    {
        if (scr.Code == null)
            continue;
		string scrName = scr.Name.Content;
        if (scrName.Contains("textdata_"))
        {
            langsToAdd.Add(scrName.Replace("textdata_",""));
        }
    }

	Data.Functions.EnsureDefined("get_integer", Data.Strings);

	for(var i = 0; i < langsToAdd.Count;i++)
		obj_time_code += $"\nglobal.lang_list[{i}] = \"{langsToAdd[i]}\"";

	importGroup.QueueReplace("gml_Object_obj_time_Create_0", obj_time_code);
	importGroup.Import();

	ScriptMessage("Patched!");
}

void ImportGMLFile(string txtFilePath)
{
	UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data);
	string code = File.ReadAllText(txtFilePath);
	string codeName = Path.GetFileNameWithoutExtension(txtFilePath);
	importGroup.QueueReplace(codeName, code);
	importGroup.Import();
}