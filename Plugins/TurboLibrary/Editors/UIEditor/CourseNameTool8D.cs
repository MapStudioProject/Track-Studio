using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MapStudio.UI;
using UIFramework;
using CLMS;
using ImGuiNET;
using CafeLibrary;

namespace TurboLibrary
{
    public class CourseNameTool8D : CourseNameToolBase
    {
        SarcData MessageAchiveFile;

        public CourseNameTool8D()
        {
        }

        public override void SaveMessage(bool hasDialog = true)
        {
            if (MessageTable == null || !edited)
                return;

            MessageAchiveFile.Files["Common.msbt"] = MessageTable.Save();
            var sarcFile = SARC_Parser.PackN(MessageAchiveFile).Item2;

            string modPath = $"{GlobalSettings.ModOutputPath}\\UI\\{currentLanguage}\\message.sarc";
            if (!string.IsNullOrEmpty(GlobalSettings.ModOutputPath))
            {
                string dir = System.IO.Path.GetDirectoryName(modPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.WriteAllBytes(modPath, sarcFile);
            }
            else
                File.WriteAllBytes(filePath, sarcFile);

            if (hasDialog)
                TinyFileDialog.MessageBoxInfoOk(string.Format("Saved file to {0}!", modPath));

            edited = false;
        }

        public override void Render()
        {
            if (ImGui.CollapsingHeader("Message", ImGuiTreeNodeFlags.DefaultOpen))
            {
                DrawLanguageSelector();

                if (MessageTable == null)
                    return;

                if (!MessageTable.Messages.ContainsKey((1401 + messageIndex).ToString()))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ThemeHandler.Theme.Error);
                    ImGui.Text($"Cannot find message ID! You may need to provide the latest update in path settings!");
                    ImGui.PopStyleColor();
                    return;
                }
                //Determine the index of the track
                if (CourseUIEditor.TrackList.Contains(currentTrack))
                {
                    MessageEntry(1401 + messageIndex, "Opening Name");
                    MessageEntryTagEdit(1501 + messageIndex, "Menu Name", 1);
                }
                else if (CourseUIEditor.TrackListRetro.Contains(currentTrack))
                {
                    MessageEntry(1441 + messageIndex, "Opening Name");
                    MessageEntryTagEdit(1541 + messageIndex, "Menu Name", 1);

                }
                else if (CourseUIEditor.TrackListDLC.Contains(currentTrack))
                {
                    MessageEntry(1481 + messageIndex, "Opening Name");
                    MessageEntryTagEdit(1581 + messageIndex, "Menu Name", 1);
                }
                else if (CourseUIEditor.TrackListBattleDLC.Contains(currentTrack))
                {
                    MessageEntry(11401 + messageIndex, "Opening Name");
                    MessageEntryTagEdit(11501 + messageIndex, "Menu Name", 1);
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
            if (CourseUIEditor.TrackListBattleDLC.Contains(currentTrack))
                messageIndex = CourseUIEditor.TrackListBattleDLC.IndexOf(currentTrack);
        }

        protected override void LoadMessageData()
        {
            string path = GlobalSettings.GetContentPath($"UI\\{currentLanguage}\\message.sarc");
            if (File.Exists(path))
            {
                filePath = path;
                MessageAchiveFile = SARC_Parser.UnpackRamN(File.ReadAllBytes(path));
                MessageTable = new MSBT(MessageAchiveFile.Files["Common.msbt"]);
            }
            else
            {
                MessageTable = null;
            }
        }

        protected override Dictionary<string, string> LanguageKeys => new Dictionary<string, string>()
        {
            { "USen", "English (US)" },
            { "USfr", "French (Western)" },
            { "USes", "Spanish (Western)" },
            { "EUde", "German" },
            { "EUen", "English (Eastern)" },
            { "EUfr", "French (Eastern)" },
            { "EUnl", "Dutch" },
            { "EUit", "Italian" },
            { "EUpt", "Portuguese" },
            { "EUru", "Russian" },
            { "EUes", "Spanish (Eastern)" },
            { "JPja", "Japanese" },
        };
    }
}
