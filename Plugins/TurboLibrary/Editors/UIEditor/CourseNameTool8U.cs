using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MapStudio.UI;
using UIFramework;
using CLMS;
using ImGuiNET;

namespace TurboLibrary
{
    public class CourseNameTool8U : CourseNameToolBase
    {
        public CourseNameTool8U()
        {

        }

        public override void SaveMessage(bool useDialog = true)
        {
            if (MessageTable == null)
                return;

            //Save the file either in the mod directory or original path
            string modPath = $"{GlobalSettings.ModOutputPath}\\ui\\{ currentLanguage}\\Common.msbt";
            if (!string.IsNullOrEmpty(GlobalSettings.ModOutputPath))
            {
                //Create directory if does not exist
                string dir = System.IO.Path.GetDirectoryName(modPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                //Save file
                File.WriteAllBytes(modPath, MessageTable.Save());
            }
            else //Save file
                File.WriteAllBytes(filePath, MessageTable.Save());

            if (useDialog)
                TinyFileDialog.MessageBoxInfoOk(string.Format("Saved file to {0}!", modPath));
        }

        public override void Render()
        {
            if (ImGui.CollapsingHeader("Message", ImGuiTreeNodeFlags.DefaultOpen))
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

                if (MessageTable == null)
                {
                    if (!string.IsNullOrEmpty(currentLanguage))
                        ImGui.TextColored(ThemeHandler.Theme.Error, $"Failed to find file for 'ui\\{ currentLanguage}\\Common.msbt'. Make sure your game path is configured!");
                    return;
                }

                if (!MessageTable.Messages.ContainsKey((1401 + messageIndex).ToString()))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ThemeHandler.Theme.Error);
                    ImGui.Text($"Cannot find message ID! You may need to provide the latest update in path settings!");
                    ImGui.PopStyleColor();
                    return;
                }
                if (CourseUIEditor.TrackList.Contains(currentTrack))
                {
                    //Determine the index of the track
                    MessageEntry(1401 + messageIndex, "Opening Name");
                    MessageEntryTagEdit(1501 + messageIndex, "Menu Name", 1);
                    MessageEntry(1601 + messageIndex, "Menu Online Name");
                }
                else if (CourseUIEditor.TrackListRetro.Contains(currentTrack))
                { 
                    //Determine the index of the track
                    MessageEntry(1441 + messageIndex, "Opening Name");
                    MessageEntryTagEdit(1541 + messageIndex, "Menu Name", 1);
                    MessageEntry(1641 + messageIndex, "Menu Online Name");
                }
                else if (CourseUIEditor.TrackListDLC.Contains(currentTrack))
                {
                    MessageEntry(1481 + messageIndex, "Opening Name");
                    MessageEntryTagEdit(1581 + messageIndex, "Menu Name", 1);
                    MessageEntry(1681 + messageIndex, "Menu Online Name");
                }
            }
        }

        public override void UpdateTrack()
        {
            if (CourseUIEditor.TrackList.Contains(currentTrack))
                messageIndex = CourseUIEditor.TrackList.IndexOf(currentTrack);
            if (CourseUIEditor.TrackListRetro.Contains(currentTrack))
                messageIndex = CourseUIEditor.TrackListRetro.IndexOf(currentTrack);
            if (CourseUIEditor.TrackListDLC.Contains(currentTrack))
                messageIndex = CourseUIEditor.TrackListDLC.IndexOf(currentTrack);
        }

        protected override void LoadMessageData()
        {
            string path = GlobalSettings.GetContentPath($"ui\\{currentLanguage}\\Common.msbt");
            if (File.Exists(path))
            {
                filePath = path;
                MessageTable = new MSBT(File.ReadAllBytes(path));
                Console.WriteLine();
            }
            else
            {
                MessageTable = null;
            }
        }

        protected override Dictionary<string, string> LanguageKeys => new Dictionary<string, string>()
        {
            { "ue", "English (US)" },
            { "uf", "French (Western)" },
            { "us", "Spanish (Western)" },
            { "ed", "Dutch" },
            { "ee", "English (Eastern)" },
            { "ef", "French (Eastern)" },
            { "eg", "German" },
            { "ei", "Italian" },
            { "ep", "Portuguese" },
            { "er", "Russian" },
            { "es", "Spanish (Eastern)" },
            { "jp", "Japanese" },
        };
    }
}
