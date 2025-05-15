using System.Linq;
using System.Text;
using UndertaleModLib;
using UndertaleModLib.Scripting;
using UndertaleModLib.Util;
using UndertaleModLib.Decompiler;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.CodeAnalysis.Scripting;

EnsureDataLoaded();

Regex frame_regex = new Regex(@"_(\d+).png");
Regex filename = new Regex(@"^(?:[a-zA-Z_\d]+?)(?=(?:_[1-9]?\d)?(?: \w+\.png$|\.png$))");
string path = PromptChooseDirectory();
var files = Directory.EnumerateFiles(path, "*.png", SearchOption.AllDirectories);
List<string> images_files = new List<string>();
List<string> images = new List<string>();
List<string> frames = new List<string>();
bool Log = ScriptQuestion("Enable Logging?");
string LogExeptions = String.Empty;
string RandomLog = "SPRITE;TEXTURE;TEXTURE INDEX;PAGE ITEM\n";
string Fulllog = String.Empty;
string UneconomicaLog = String.Empty;
string WarnLog = String.Empty;
string RegexFileErrorLog = String.Empty;
bool BoolRandomLog = false;
bool WarningLog = false;

foreach (string file in files) {CreateList(file);}

SetProgressBar(null, "Sprites", 0, images.Count);
StartProgressBarUpdater();

await ReplacePageItems();
CreateLogs();

await StopProgressBarUpdater();
HideProgressBar();

/*
async Task CreateLists()
{
    await Task.Run(() => Parallel.ForEach(files, file => {CreateList(file);}));
}
*/

async Task ReplacePageItems()
{
    await Task.Run(() => Parallel.For(0, images.Count, i => {ReplacePageItem(i);}));
}

void CreateList(string file)
{
    string filo = Path.GetFileName(file);
    filo = filo.Replace(" uneconomical", "").Replace("  redimensioned", "").Replace("_uneconomical", "");
    Match frame_match = frame_regex.Match(file);
    Match filename_match = filename.Match(filo);
    {
        if (!filename_match.Success)
        {
            RegexFileErrorLog += $"\"{file}\" coudn't pass the regex for some reason\n\tpattern 1: \"{filo.Split("/").Last()}\"\n\tpattern 2: \"{filo.Split("\\").Last()}\"\n";
            return;
        }
    }
    frames.Add(frame_match.Success ? frame_match.Groups[1].Value : "0");
    images.Add(filename_match.Value);
    images_files.Add(file);
    if ((!frame_match.Success) && (Log) && (WarningLog))
    {
        WarnLog += $"file:{file} doesn't have and texture index. Assuming 0.\n";
    }
    return;
}

void ReplacePageItem(int i)
{
    //Console.WriteLine($"{sprite} : Data.Sprites[{Data.Sprites.IndexOf(Data.Sprites.FirstOrDefault(e => e.Name.Content == sprite))}]");
    int sprite_index = Data.Sprites.IndexOf(Data.Sprites.FirstOrDefault(e => e.Name.Content == images[i]));
    if (sprite_index != -1)
    {
        int pageitem_index = Data.TexturePageItems.IndexOf(Data.Sprites[sprite_index].Textures[int.Parse(frames[i])].Texture);
        if (pageitem_index != -1)
        {
            RandomLog = BoolRandomLog ? (RandomLog + $"\"{images[i]}[{frames[i]}]\";\"Data.Sprites[{sprite_index}].Textures[{frames[i]}]\";\"{frames[i]}\";\"Data.TexturePageItems[{pageitem_index}]\"\n") : String.Empty;
            //Console.WriteLine(Data.Sprites[Data.Sprites.IndexOf(Data.Sprites.FirstOrDefault(e => e.Name.Content == images[i]))].Name.Content);
            using MagickImage idk = TextureWorker.ReadBGRAImageFromFile(images_files[i]);
            if ((idk.Width == Data.TexturePageItems[pageitem_index].TargetWidth) && (idk.Height == Data.TexturePageItems[pageitem_index].TargetHeight))
            {
                Data.TexturePageItems[pageitem_index].ReplaceTexture(idk);
            }
            else if (Log)
            {
                string Exceptchones = String.Empty;
                Exceptchones += $"Data.TexturePageItems[{pageitem_index}] and {images_files[i]} have diferent sizes\n";
                Exceptchones += $"Data.TexturePageItems[{pageitem_index}.TargetWidth = {Data.TexturePageItems[pageitem_index].TargetWidth}] | image.Width = {idk.Width}\n";
                Exceptchones += $"Data.TexturePageItems[{pageitem_index}.TargetHeight = {Data.TexturePageItems[pageitem_index].TargetHeight}] | image.Height = {idk.Height}\n";
                if (!images_files[i].Contains("uneconomical"))
                {
                    LogExeptions += Exceptchones;
                }
                else
                {
                    UneconomicaLog += Exceptchones;
                }
            }
        }
        else if (Log)
        {
            LogExeptions += $"Data.TexturePageItems doesn't have a definition for \"{images[i]}[{frames[i]}]\"\n";
        }
    }
    else if (Log)
    {
        LogExeptions += $"Data.Sprites doesn't have a definition for \"{images[i]}\"\n";
    }
    IncrementProgressParallel();
    return;
}

void CreateLogs()
{
    if (LogExeptions != String.Empty)
    {
        Fulllog += "--------------------------------------------------\n";
        Fulllog += "-------------------EXCEPTIONS!--------------------\n";
        Fulllog += "--------------------------------------------------\n";
        Fulllog += LogExeptions;
    }

    if (RegexFileErrorLog != String.Empty)
    {
        Fulllog += "--------------------------------------------------\n";
        Fulllog += "-------------------REGEX_ERROR!--------------------\n";
        Fulllog += "--------------------------------------------------\n";
        Fulllog += RegexFileErrorLog;
    }

    if (BoolRandomLog)
    {
        File.WriteAllText(Path.Combine(path, "RandomLog.csv"), RandomLog);
    }

    if (WarningLog)
    {
        Fulllog += "--------------------------------------------------\n";
        Fulllog += "--------------------WARNINGS!---------------------\n";
        Fulllog += "--------------------------------------------------\n";
        Fulllog += WarnLog;
    }

    if (UneconomicaLog != String.Empty)
    {
        Fulllog += "--------------------------------------------------\n";
        Fulllog += "-------------------UNECONOMICAL!------------------\n";
        Fulllog += "--------------------------------------------------\n";
        Fulllog += UneconomicaLog;
    }

    if (!(Fulllog == String.Empty))
    {
        File.WriteAllText(Path.Combine(path, "Log.txt"), Fulllog);
    }
}