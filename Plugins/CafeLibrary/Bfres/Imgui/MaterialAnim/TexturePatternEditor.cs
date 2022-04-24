using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using CafeLibrary.Rendering;
using MapStudio.UI;
using ImGuiNET;

namespace CafeLibrary
{
    public class TexturePatternEditor
    {
        BfresMaterialAnim.SamplerTrack SamplerTrack;
        BfresMaterialAnim.MaterialAnimGroup Material;
        BfresMaterialAnim MaterialAnim;

        public TexturePatternEditor(BfresMaterialAnim anim) {
            MaterialAnim = anim;

            var material = anim.AnimGroups.FirstOrDefault();

        }

        public void Render()
        {
            ImGui.Columns(2);
            RenderKeyList();
            ImGui.NextColumn();

            ImGui.NextColumn();
            ImGui.Columns(1);
        }

        private void RenderKeyList()
        {
            if (SamplerTrack == null)
                return;

            var textureList = MaterialAnim.TextureList;
            foreach (var key in SamplerTrack.KeyFrames) {
                string textureName = textureList[(int)key.Value];
                RenderTextureIcon(textureName);
            }
        }

        private void RenderTextureIcon(string name)
        {
            int icon = GetTextureIcon(name);
            ImGui.Image((IntPtr)icon, new Vector2());
        }

        private int GetTextureIcon(string name)
        {
            if (!IconManager.HasIcon(name))
            {
                foreach (var cache in GLFrameworkEngine.DataCache.ModelCache.Values)
                {
                    if (cache.Textures.ContainsKey(name))
                        IconManager.AddIcon(name, cache.Textures[name].RenderTexture.ID);
                }
            }
            if (!IconManager.HasIcon(name))
                return IconManager.GetTextureIcon("TEXTURE");
            return IconManager.GetTextureIcon(name);
        }
    }
}
