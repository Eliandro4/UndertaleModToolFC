using System.Linq;
using System.Text;
using UndertaleModLib;
using UndertaleModLib.Scripting;
using UndertaleModLib.Util;
using UndertaleModLib.Decompiler;
using System.Text.RegularExpressions;
using ImageMagick;

EnsureDataLoaded();

ReplaceTexturesNew();

public void ReplaceTexturesNew()
    {
        Regex frame_regex = new Regex(@"_(\d+).png");
        Regex filename = new Regex(@"^(?:[a-zA-Z_]+?)(?=(?:_\d+)?(?: \w+\.png|\.png$))");
        string path = PromptChooseDirectory();
        var files = Directory.EnumerateFiles(path, "*.png", SearchOption.AllDirectories);
        List<string> images_files = new List<string>();
        List<string> images = new List<string>();
        List<string> frames = new List<string>();
        bool Log = ScriptQuestion("Enable Logging?");
        string LogExeptions = String.Empty;
        string RandomLog = String.Empty;
        string Fulllog = String.Empty;
        string UneconomicaLog = String.Empty;
        string WarnLog = String.Empty
        foreach (string file in files)
        {
            Match frame_match = frame_regex.Match(file);
            Match filename_match = filename.Match(file);
            frames.Add(frame_match.Success ? frame_match.Groups[1].Value : "0");
            images.Add(filename_match.Groups[1].Value);
            images_files.Add(file);
            if ((!frame_match.Success) && (Log))
            {
                WarnLog += $"file:{file} doesn't have and texture index. Assuming 0.\n";
            }
        }

        for (int i = 0; i < images.Count; i++)
        {
            //Console.WriteLine($"{sprite} : Data.Sprites[{Data.Sprites.IndexOf(Data.Sprites.FirstOrDefault(e => e.Name.Content == sprite))}]");
            int sprite_index = Data.Sprites.IndexOf(Data.Sprites.FirstOrDefault(e => e.Name.Content == images[i]));
            if (sprite_index != -1)
            {
                int pageitem_index = Data.TexturePageItems.IndexOf(Data.Sprites[sprite_index].Textures[int.Parse(frames[i])].Texture);
                if (pageitem_index != -1)
                {
                    RandomLog = Log ? (RandomLog + $"{images[i]}[{frames[i]}] : Data.Sprites[{sprite_index}] \"frame[{frames[i]}]\" : Data.TexturePageItems[{pageitem_index}]") : String.Empty;
                    //Console.WriteLine(Data.Sprites[Data.Sprites.IndexOf(Data.Sprites.FirstOrDefault(e => e.Name.Content == images[i]))].Name.Content);
                    using MagickImage idk = TextureWorker.ReadBGRAImageFromFile(images_files[i]);
                    if ((idk.Width == Data.TexturePageItems[pageitem_index].TargetWidth) && (idk.Height == Data.TexturePageItems[pageitem_index].TargetHeight))
                    {
                        Data.TexturePageItems[pageitem_index].ReplaceTexture(idk);
                    }
                    else if (Log)
                    {
                        string Exceptchones = new String.Empty;
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
            else if (log)
            {
                LogExeptions += $"Data.Sprites doesn't have a definition for \"{images[i]}\"\n";
            }
        }

        if (!(LogExeptions == String.Empty))
        {
            Fulllog += "--------------------------------------------------\n";
            Fulllog += "-------------------EXCEPTIONS!--------------------\n";
            Fulllog += "--------------------------------------------------\n";
            Fulllog += LogExeptions;
        }

        if (!(RandomLog == String.Empty))
        {
            Fulllog += "\n--------------------------------------------------\n";
            Fulllog += "-------------------RANDOM_LOG!--------------------\n";
            Fulllog += "--------------------------------------------------\n";
            Fulllog += RandomLog;
        }

        if (!(WarnLog == String.Empty))
        {
            Fulllog += "--------------------------------------------------\n";
            Fulllog += "-------------------WARNINGS!--------------------\n";
            Fulllog += "--------------------------------------------------\n";
            Fulllog += WarnLog;
        }

        if (!(UneconomicaLog == String.Empty))
        {
            Fulllog += "\n--------------------------------------------------\n";
            Fulllog += "-------------------UNECONOMICAL!------------------\n";
            Fulllog += "--------------------------------------------------\n";
            Fulllog += UneconomicaLog;
        }

        if (!(Fulllog == String.Empty))
        {
        File.WriteAllText(Path.Combine(path, "Log.txt"), Fulllog);
        }
    }