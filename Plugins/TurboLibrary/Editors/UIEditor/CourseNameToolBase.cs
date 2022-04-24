using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;
using CLMS;

namespace TurboLibrary
{
    public class CourseNameToolBase
    {
        protected MSBT MessageTable = null;
        protected string filePath = "";
        protected string currentLanguage = "";
        protected string currentLanguageName = "";
        protected string currentTrack => CourseUIEditor.currentTrack;
        protected int messageIndex = 0;
        protected bool edited = false;

        protected virtual Dictionary<string, string> LanguageKeys => new Dictionary<string, string>();

        public virtual void SaveMessage(bool hasDialog = true)
        {

        }

        public virtual void Render()
        {

        }

        public virtual void UpdateTrack()
        {

        }

        protected virtual void LoadMessageData()
        {

        }

        protected void DrawLanguageSelector()
        {
            //Language selector
            ImguiCustomWidgets.ComboScrollable("Language", currentLanguageName, ref currentLanguageName, LanguageKeys.Values, () =>
            {
                //Make sure to apply edits before switching (files are different and are reloaded)
                if (edited)
                {
                    int result = TinyFileDialog.MessageBoxInfoYesNo($"Unchanged edits. Do you want to save?");
                    if (result == 1)
                        SaveMessage(true);
                    edited = false;
                }

                var key = LanguageKeys.FirstOrDefault(x => x.Value == currentLanguageName).Key;
                currentLanguage = key;
                LoadMessageData();
            });
        }

        protected void MessageEntry(int id, string label)
        {
            var message = MessageTable.tryGetMessageByKey(id.ToString());
            if (ImGui.InputText($"{label}##{currentTrack}_{currentLanguage}", ref message.rawString, 0x100))
                edited = true;
        }

        protected void MessageEntryTagEdit(int id, string label, int tagIndex)
        {
            var message = MessageTable.tryGetMessageByKey(id.ToString());
            if (message.tags.Count == 0)
            {
                MessageEntry(id, label);
                return;
            }
            var prms = message.ToParams();
            var name = (string)prms[tagIndex];
            if (ImGui.InputText($"{label}##{currentTrack}_{currentLanguage}", ref name, 0x100))
            {
                prms[1] = name;
                message.edit(prms);
                edited = true;
            }
        }
    }
}
