using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Util;
using UndertaleModLib.Decompiler;
using UndertaleModLib.Scripting;
using System.Linq;
using ImageMagick;
using System.Text.RegularExpressions;

public class TextureJson
{
    public string Name { get; set; }
    public string Texture { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }
    public ushort Width { get; set; }
    public ushort Height { get; set; }

    public TextureJson() {}
}
Regex frame_regex = new Regex(@"_(\d+).png");
List<string> embededos = new List<string>();
List<int> embeddedos_id = new List<int>();
ScriptMessage("Selecione um JSON do TextureRepacker");
string path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
string json = File.ReadAllText(path);
List<TextureJson> ListaTexturas = JsonSerializer.Deserialize<List<TextureJson>>(json);
foreach (TextureJson algo in ListaTexturas)
{
    //ScriptMessage(algo.Name);
    if (embededos.IndexOf(algo.Texture) == -1)
    {
        embededos.Add(algo.Texture);
    }
}

foreach (string Embeddedoro in embededos)
{
    int lastTextPage = Data.EmbeddedTextures.Count - 1;
    string EmbeddedImage = Path.Combine(Path.GetDirectoryName(path), $"{Embeddedoro}.png");
    UndertaleEmbeddedTexture texture = new UndertaleEmbeddedTexture();
    texture.Name = new UndertaleString($"Texture {++lastTextPage}");
    texture.TextureData.Image = GMImage.FromPng(File.ReadAllBytes(EmbeddedImage));
    embeddedos_id.Add(lastTextPage);
    Data.EmbeddedTextures.Add(texture);
}

foreach (TextureJson sprite in ListaTexturas)
{
    string name = sprite.Name.Replace(" uneconomical", "").Replace("  redimensioned", "").Replace("_uneconomical", "");
    Match frame_match = frame_regex.Match(name + ".png");
    int sprite_frame_index = frame_match.Success ? int.Parse(frame_match.Groups[1].Value) : 0;
    name = name.Replace($"_{sprite_frame_index}", "");
    int sprite_index = Data.Sprites.IndexOf(Data.Sprites.FirstOrDefault(e => e.Name.Content == name));
    if (sprite_index != -1)
    {
        if (sprite_frame_index < Data.Sprites[sprite_index].Textures.Count)
        {
            int pageitem_index = Data.TexturePageItems.IndexOf(Data.Sprites[sprite_index].Textures[sprite_frame_index].Texture);
            Data.TexturePageItems[pageitem_index].ReplaceTexture(new MagickImage(MagickColors.Transparent, 64, 64));
            Data.TexturePageItems[pageitem_index].TexturePage = Data.EmbeddedTextures[embeddedos_id[embededos.IndexOf(sprite.Texture)]];
            Data.TexturePageItems[pageitem_index].SourceHeight = sprite.Height;
            Data.TexturePageItems[pageitem_index].TargetHeight = sprite.Height;
            Data.TexturePageItems[pageitem_index].BoundingHeight = sprite.Height;
            Data.TexturePageItems[pageitem_index].SourceWidth = sprite.Width;
            Data.TexturePageItems[pageitem_index].TargetWidth = sprite.Width;
            Data.TexturePageItems[pageitem_index].BoundingWidth = sprite.Width;
            Data.TexturePageItems[pageitem_index].SourceX = sprite.X;
            Data.TexturePageItems[pageitem_index].TargetX = 0;
            Data.TexturePageItems[pageitem_index].SourceY = sprite.Y;
            Data.TexturePageItems[pageitem_index].TargetY = 0;
        }
        else
        {
            ScriptMessage($"O sprite {name} não tem tantos frames");
        }
    }
    else
    {
        ScriptMessage($"Data.Sprites não tem uma definição para {name}");
    }
}