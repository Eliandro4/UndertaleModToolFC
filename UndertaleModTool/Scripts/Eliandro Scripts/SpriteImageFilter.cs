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
List<SpriteF> images_files = new List<SpriteF>();
List<SpriteE> errors = new List<SpriteE>();
bool Log = ScriptQuestion("Enable Logging?");

foreach (string file in files) {CreateInitialList(file);}
for (int i = 0; i < images_files.Count(); i++) { Filter(i); }

CreateLogs();

void CreateInitialList(string file)
{
    string filo = Path.GetFileName(file);
    filo = filo.Replace(" uneconomical", "").Replace("  redimensioned", "").Replace("_uneconomical", "").Replace(" redimensioned", "");
    Match frame_match = frame_regex.Match(file);
    Match filename_match = filename.Match(filo);
    {
        if (!filename_match.Success)
        {
            errors.Add(new SpriteE(file, $"\"{file}\" coudn't pass the regex for some reason"));
            return;
        }
    }
    SpriteF sprito= new();
    sprito.Frame = frame_match.Success ? int.Parse(frame_match.Groups[1].Value) : 0;
    sprito.Nome = filename_match.Value;
    sprito.Path = file;
    images_files.Add(sprito);
}

void Filter(int i)
{
    int sprite_index = Data.Sprites.IndexOf(Data.Sprites.FirstOrDefault(e => e.Name.Content == images_files[i]));
    if (sprite_index != -1)
    {
        if (int.Parse(frames[i]) < Data.Sprites[sprite_index].Textures.Count)
        {
            int pageitem_index = Data.TexturePageItems.IndexOf(Data.Sprites[sprite_index].Textures[int.Parse(frames[i])].Texture);
            if (pageitem_index != -1)
            {
                using MagickImage idk = TextureWorker.ReadBGRAImageFromFile(images_files[i]);
                if ((idk.Width == Data.TexturePageItems[pageitem_index].TargetWidth) && (idk.Height == Data.TexturePageItems[pageitem_index].TargetHeight))
                {
                    Data.TexturePageItems[pageitem_index].ReplaceTexture(idk);
                }
                else if ((idk.Width == Data.TexturePageItems[pageitem_index].BoundingWidth) && (idk.Height == Data.TexturePageItems[pageitem_index].BoundingHeight))
                {
                    bool import_padding = ScriptQuestion($"it look's like {images[i]} was exported with padding, try importing?");
                    if (import_padding)
                    {
                        MagickGeometry rectangle = new MagickGeometry(Data.TexturePageItems[pageitem_index].TargetX, Data.TexturePageItems[pageitem_index].TargetY, Data.TexturePageItems[pageitem_index].TargetWidth, Data.TexturePageItems[pageitem_index].TargetHeight);
                        idk.Crop(rectangle);
                        Data.TexturePageItems[pageitem_index].ReplaceTexture(idk);
                    }
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
                errors.Add(new SpriteE(images_files[i].Path, $"Data.TexturePageItems doesn't have a definition for \"{images_files[i].Nome}[{images_files[i].Frame}]\"\n"));
            }
        }
        else
        {
            errors.Add(new SpriteE(images_files[i].Path, $"Frame: {int.Parse(images_files[i].Frame)} fora do range de {images_files[i].Nome}"));
        }
    }
    else if (Log)
    {
        errors.Add(new SpriteE(images_files[i].Path, $"Data.Sprites doesn't have a definition for \"{images_files[i].Nome}\""));
    }
}

void CreateLogs()
{
}

public class SpriteF
{
    public string Nome { get; set; }
    public string Path { get; set; }
    public int Frame { get; set; }
}

public class SpriteP
{
    public string Path { get; set; }
    public int PageItem { get; set; }
}

public class SpriteE(string Path, string Error)
{
    public string Path { get; set; }
    public string Error { get; set; }
}

public class SpriteGroup
{
    public List<SpriteP> Tamanho_Igual {get; set;}
    public List<SpriteP> Tamanho_Dfrnt {get; set;}
    public List<SpriteE> Error { get; set; }
}