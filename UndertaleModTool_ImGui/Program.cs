using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using ImGuiNET;
using System.Numerics;

namespace UndertaleModTool
{
    static class Program
    {
        static void Main()
        {
            // Cria janela + device
            VeldridStartup.CreateWindowAndGraphicsDevice(
                new WindowCreateInfo(100, 100, 1000, 700, WindowState.Normal, "UndertaleModTool"),
                new GraphicsDeviceOptions(true, null, true),
                out Sdl2Window window,
                out GraphicsDevice gd);

            var cl = gd.ResourceFactory.CreateCommandList();

            var imguiRenderer = new ImGuiRenderer(
                gd,
                gd.MainSwapchain.Framebuffer.OutputDescription,
                window.Width,
                window.Height);

            window.Resized += () =>
            {
                gd.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
                imguiRenderer.WindowResized(window.Width, window.Height);
            };

            while (window.Exists)
            {
                var snapshot = window.PumpEvents();
                if (!window.Exists) break;

                imguiRenderer.Update(1f / 60f, snapshot);

                // Define tamanho da janela principal
                ImGui.SetNextWindowSize(new Vector2(window.Width, window.Height));
                ImGui.SetNextWindowPos(Vector2.Zero);
                ImGui.Begin("UndertaleModTool", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.MenuBar);

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        if (ImGui.MenuItem("New", "Ctrl+N")) { /* lógica de novo arquivo */ }
                        if (ImGui.MenuItem("Open", "Ctrl+O")) { /* lógica de abrir */ }
                        if (ImGui.MenuItem("Save", "Ctrl+S", false, false)) { /* salvar desativado */ }

                        ImGui.Separator();

                        if (ImGui.MenuItem("Temp run game", "F5", false, false)) { }
                        if (ImGui.MenuItem("Run game with other runner", "Alt+F5", false, false)) { }

                        ImGui.Separator();

                        if (ImGui.MenuItem("Generate offset map")) { }
                        if (ImGui.MenuItem("Settings", "F4")) { }
                        if (ImGui.MenuItem("Close", "Ctrl+Q")) { Environment.Exit(0); }

                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Scripts"))
                    {
                        // Adicione os subitens aqui se necessário
                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Find"))
                    {
                        if (ImGui.MenuItem("Search in code", "Ctrl+Shift+F")) { }
                        if (ImGui.MenuItem("Find unreferenced assets", "", false, false)) { }
                        ImGui.EndMenu();
                    }

                    if (ImGui.BeginMenu("Help"))
                    {
                        if (ImGui.MenuItem("GitHub")) { /* abrir link ou info */ }
                        if (ImGui.MenuItem("About")) { }
                        ImGui.EndMenu();
                    }

                    ImGui.EndMenuBar();
                }

                // Define duas colunas
                ImGui.Columns(2, "MainColumns", true);

                // Painel da esquerda (navegação)
                ImGui.BeginChild("LeftPanel", new Vector2(250, window.Height - 30), true);
                ImGui.Text("Data");

                if (ImGui.TreeNode("General info")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Global init")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Audio groups")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Sounds")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Sprites")) { ImGui.Text("Player"); ImGui.TreePop(); }
                if (ImGui.TreeNode("Backgrounds & Tile sets")) { ImGui.Text("Player"); ImGui.TreePop(); }
                if (ImGui.TreeNode("Paths")) { ImGui.Text("Player"); ImGui.TreePop(); }
                if (ImGui.TreeNode("Scripts"))
                {
                    /*
                    if (ImGui.TreeNode("Variables")) { ImGui.Text("var_player_hp"); ImGui.TreePop(); }
                    if (ImGui.TreeNode("Functions")) { ImGui.Text("func_attack"); ImGui.TreePop(); }
                    */
                    ImGui.TreePop();
                }
                if (ImGui.TreeNode("Shaders")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Fonts")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Timelines")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Game objects")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Rooms")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Extensions")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Texture page items")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Code")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Variables")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Functions")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Code locals")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Strings")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Embedded textures")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Embedded audio")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Texture group info")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Embedded images")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Particles system")) { ImGui.TreePop(); }
                if (ImGui.TreeNode("Particles system emitters")) { ImGui.TreePop(); }

                ImGui.EndChild();

                ImGui.NextColumn();

                // Painel da direita (conteúdo)
                ImGui.BeginChild("RightPanel", new Vector2(0, window.Height - 30), true);

                ImGui.Text("Welcome to UndertaleModTool!");
                ImGui.Separator();
                ImGui.TextWrapped("Open a data.win file to get started, then double click on the items on the left to view them.");

                ImGui.EndChild();

                ImGui.Columns(1);
                ImGui.End(); // End janela principal

                // Renderização
                cl.Begin();
                cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
                cl.ClearColorTarget(0, RgbaFloat.Black);
                imguiRenderer.Render(gd, cl);
                cl.End();
                gd.SubmitCommands(cl);
                gd.SwapBuffers(gd.MainSwapchain);
            }
        }
    }
}
