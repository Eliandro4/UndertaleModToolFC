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
List<string> embededos = new List<string>();
ScriptMessage("Selecione um JSON do TextureRepacker");
string path = PromptLoadFile("", "TXT files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*");
string json = File.ReadAllText(path);
List<TextureJson> ListaTexturas = JsonSerializer.Deserialize<List<TextureJson>>(json);
foreach (TextureJson algo in ListaTexturas)
{
    ScriptMessage(algo.Name);
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
    Data.EmbeddedTextures.Add(texture);
}