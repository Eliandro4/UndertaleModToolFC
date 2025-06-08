using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using UndertaleModLib;
using UndertaleModLib.Models;
using UndertaleModLib.Util;

public class TextureJson
{
    public string Name { get; set; }
    public string Texture { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

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
    string EmbeddedImage = Path.Combine(Path.GetDirectoryName(path), $"{Embeddedoro}.png");
    UndertaleEmbeddedTexture texture = new UndertaleEmbeddedTexture();
    texture.Name = new UndertaleString($"Texture {Data.EmbeddedTextures.Count}");
    texture.TextureData.Image = GMImage.FromPng(File.ReadAllBytes(atlasName));
    embeddedos_id.Add(Data.EmbeddedTextures.Count);
    Data.EmbeddedTextures.Add(texture);
}

foreach (TextureJson sprite in ListaTexturas)
{
    Match frame_match = frame_regex.Match(sprite.Name);
    string name = sprite.Name.Replace($"_{frame_match.Groups[1].Value}", "");
    int sprite_index = Data.Sprites.IndexOf(Data.Sprites.FirstOrDefault(e => e.Name.Content == name));
    int pageitem_index = Data.TexturePageItems.IndexOf(Data.Sprites[sprite_index].Textures[frame_match.Groups[1].Value].Texture);
    Data.TexturePageItems[pageitem_index].ReplaceTexture(new MagickImage());
    Data.TexturePageItems[pageitem_index].TexturePage = Data.EmbeddedTextures[embeddedos_id[embededos.IndexOf(sprite.Texture)]];
    Data.TexturePageItems[pageitem_index].SourceHeight = sprite.Height;
    Data.TexturePageItems[pageitem_index].TargetHeight = sprite.Height;
    Data.TexturePageItems[pageitem_index].BoundingHeight = sprite.Height;
    Data.TexturePageItems[pageitem_index].SourceWidth = sprite.Width;
    Data.TexturePageItems[pageitem_index].TargetWidth = sprite.Width;
    Data.TexturePageItems[pageitem_index].BoundingWidth = sprite.Width;
    Data.TexturePageItems[pageitem_index].SourceX = sprite.X;
    Data.TexturePageItems[pageitem_index].TargetX = sprite.X;
    Data.TexturePageItems[pageitem_index].SourceY = sprite.Y;
    Data.TexturePageItems[pageitem_index].TargetY = sprite.Y;
}