using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using AGraphicsLibrary;
using System.ComponentModel;
using GLFrameworkEngine;
using MapStudio.UI;

namespace TurboLibrary.LightingEditor
{
    public class ColorCorrectionWindow
    {
        public bool IsActive;

        float _saturation;
        float _brightness;
        float _gamma;
        float _hue;

        public ColorCorrectionWindow()
        {

        }

        public bool Render(GLContext context)
        {
            var colorCorrection = LightingEngine.LightSettings.Resources.ColorCorrectionFiles.FirstOrDefault().Value;

            _saturation = colorCorrection.Saturation;
            _brightness = colorCorrection.Brightness;
            _gamma = colorCorrection.Gamma;
            _hue = colorCorrection.Hue;

            bool propertyChanged = false;

            propertyChanged |= SliderDefault("Hue", ref _hue, 0.0f, 360, 0.0f);
            propertyChanged |= SliderDefault("Saturation", ref _saturation, 0.0f, 5.0f, 1.0f);
            propertyChanged |= SliderDefault("Brightness", ref _brightness, 0.0f, 5.0f, 1.0f);
            propertyChanged |= SliderDefault("Gamma", ref _gamma, 0.0f, 5.2f, 1.0f);

            if (propertyChanged)
                PropertyChanged(context);

            return propertyChanged;
        }

        bool SliderDefault(string label, ref float value, float min, float max, float defaultValue)
        {
            bool ret = ImGui.SliderFloat(label, ref value, min, max);
            if (ImGui.BeginPopupContextItem(label))
            {
                if (ImGui.Button("Reset Default"))
                {
                    value = defaultValue;
                    ret = true;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }
            return ret;
        }

        void PropertyChanged(GLContext context)
        {
            var colorCorrection = LightingEngine.LightSettings.Resources.ColorCorrectionFiles.FirstOrDefault().Value;

            colorCorrection.Saturation = _saturation;
            colorCorrection.Gamma = _gamma;
            colorCorrection.Brightness = _brightness;
            colorCorrection.Hue = _hue;

            LightingEngine.LightSettings.UpdateColorCorrectionTable();
            context.UpdateViewport = true;
        }
    }
}
