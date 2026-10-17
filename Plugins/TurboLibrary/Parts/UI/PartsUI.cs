using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using MapStudio.UI;
using PartsEditor;
using System.Numerics;
using GLFrameworkEngine;
using CafeLibrary;
using Discord;
using static Toolbox.Core.Runtime;
using TurboLibrary.Parts.Kart;
using TurboLibrary.Parts;

namespace TurboLibrary
{
    public class PartsUI
    {
        public EventHandler OnKartChanged;

        bool overrideTireParams = true;

        public void RenderBodyParams(KartVehicle kart, KartParameter parameters)
        {
            RenderCommon(kart, parameters);

            bool update = false;

            var body = parameters.BodyPart;

            if (ImGui.CollapsingHeader("Body", ImGuiTreeNodeFlags.DefaultOpen))
            {
                StartColumns();

                update |= DrawFloat("Seat Angle", body, "SeatAngle");
                update |= DrawFloat("Max Steering Angle", body, "MaxSteeringAngle");

                update |= DrawVector2("Glider Offset", body, "GliderOffset");
                update |= DrawVector2("Antenna Offset", body, "AntennaOffset");
                update |= DrawVector3("Balloon Offset", body, "BalloonOffset");
                update |= DrawFloat("Packun Offset", body, "PackunOffsetZ");

                update |= DrawFloat("LOD0 Threshold", body, "LOD0Threshold");
                update |= DrawFloat("AnimMoveSpeedScale", body, "AnimMoveSpeedScale");

                update |= DrawVector2("Param Unknown 1", body, "ParamBG");
                update |= DrawVector2("Param Unknown 2", body, "ParamBI");

                EndColumns();
            }

            if (ImGui.CollapsingHeader("Lights", ImGuiTreeNodeFlags.DefaultOpen))
            {
                StartColumns();
                update |= DrawVector3("Headlight Offset", body, "FrontLightOffset");
                update |= DrawFloat("Headlight Angle", body, "HeadlightAngle");
                EndColumns();
            }

            if (ImGui.CollapsingHeader("Body Flags", ImGuiTreeNodeFlags.DefaultOpen))
            {
                StartColumns();

                update |= DrawBool("Is CoverLeg", body, "IsCoverLeg");
                update |= DrawBool("IsHeadLightSingle", body, "IsHeadLightSingle");
                update |= DrawBool("IsNormalArmWhenAntiG", body, "IsNormalArmWhenAntiG");
                update |= DrawBool("IsExternalHeadLight", body, "IsExternalHeadLight");
                update |= DrawBool("IsEmissionRythm", body, "IsEmissionRythm");
                update |= DrawBool("IsHeadHandleLocal", body, "IsHeadHandleLocal");

                EndColumns();
            }

            if (ImGui.CollapsingHeader("Sus", ImGuiTreeNodeFlags.DefaultOpen))
            {
                StartColumns();
                update |= DrawByte("Type Front", body, "SusTypeFront");
                update |= DrawByte("Type Front", body, "SusTypeBack");

                update |= DrawFloat("Length Back", body, "SusLengthFront");
                update |= DrawFloat("Length Front", body, "SusLengthBack");

                update |= DrawVector3("Sus Body Offset Front", body, "SusBodyOffsetFront");
                update |= DrawVector3("Sus Body Offset Back", body, "SusBodyOffsetBack");
                EndColumns();
            }

            if (update)
            {
                body.UpdateSetters();
                kart.ReloadParameters();
                GLContext.ActiveContext.UpdateViewport = true;
            }
        }

        public void RenderShaftParams(KartVehicle kart, KartParameter parameters)
        {
            bool update = false;

            RenderCommon(kart, parameters);

            var body = parameters.BodyPart;
            var bodyTire = parameters.BodyTire;

            if (ImGui.CollapsingHeader("Tire Shaft/Arm", ImGuiTreeNodeFlags.DefaultOpen))
            {
                StartColumns();

                ImGui.Text("Display Body Tire Override Params");
                ImGui.NextColumn();
                ImGui.Checkbox("##overrideTireParams", ref overrideTireParams);
                ImGui.NextColumn();

                if (overrideTireParams)
                {
                    update |= DrawFloat("Front Scale X", bodyTire.TireShaftTransform, "FrontScaleX");
                    update |= DrawFloat("Front Scale YZ", bodyTire.TireShaftTransform, "FrontScaleYZ");
                    update |= DrawFloat("Back Scale X", bodyTire.TireShaftTransform, "BackScaleX");
                    update |= DrawFloat("Back Scale YZ", bodyTire.TireShaftTransform, "BackScaleYZ");

                    update |= DrawVector3("Front Offset", bodyTire.TireShaftTransform, "FrontOffset");
                    update |= DrawVector3("Back Offset", bodyTire.TireShaftTransform, "BackOffset");

                    update |= DrawVector3("Front Anti G Offset", bodyTire.TireShaftTransform, "FrontAntiGravityOffset");
                    update |= DrawVector3("Back Anti G Offset", bodyTire.TireShaftTransform, "BackAntiGravityOffset");

                    update |= DrawVector3("Unknown1", bodyTire.TireShaftTransform, "Unknown1");
                    update |= DrawVector3("Unknown2", bodyTire.TireShaftTransform, "Unknown2");
                }
                else
                {
                    update |= DrawFloat("Front Scale X", body.TireShaftTransform, "FrontScaleX");
                    update |= DrawFloat("Front Scale YZ", body.TireShaftTransform, "FrontScaleYZ");
                    update |= DrawFloat("Back Scale X", body.TireShaftTransform, "BackScaleX");
                    update |= DrawFloat("Back Scale YZ", body.TireShaftTransform, "BackScaleYZ");

                    update |= DrawVector3("Front Offset", body.TireShaftTransform, "FrontOffset");
                    update |= DrawVector3("Back Offset", body.TireShaftTransform, "BackOffset");

                    update |= DrawVector3("Front Anti G Offset", body.TireShaftTransform, "FrontAntiGravityOffset");
                    update |= DrawVector3("Back Anti G Offset", body.TireShaftTransform, "BackAntiGravityOffset");

                    update |= DrawVector3("Unknown1", body.TireShaftTransform, "Unknown1");
                    update |= DrawVector3("Unknown2", body.TireShaftTransform, "Unknown2");
                }
                EndColumns();
            }

            if (update)
            {
                body.UpdateSetters();
                bodyTire.UpdateSetters();
                kart.ReloadParameters();
                GLContext.ActiveContext.UpdateViewport = true;
            }
        }

        public void RenderWaterPropellerParams(KartVehicle kart, KartParameter parameters)
        {
            RenderCommon(kart, parameters);

            bool update = false;
            var body = parameters.BodyPart;

            if (ImGui.CollapsingHeader("Water Propeller", ImGuiTreeNodeFlags.DefaultOpen))
            {
                StartColumns();

                update |= DrawFloat("Screw Scale", body.WaterPropellerTransform, "ScewScale");
                update |= DrawVector2("Screw Offset (XZ)", body.WaterPropellerTransform, "ScewOffsetYZ");

                update |= DrawFloat("Fan Scale", body.WaterPropellerTransform, "FanScale");
                update |= DrawVector2("Fan Offset (XZ)", body.WaterPropellerTransform, "FanOffsetYZ");

                EndColumns();
            }

            if (update)
            {
                body.UpdateSetters();
                kart.ReloadParameters();
                GLContext.ActiveContext.UpdateViewport = true;
            }
        }

        public void RenderTireParams(KartVehicle kart, KartParameter parameters)
        {
            bool update = false;

            RenderCommon(kart, parameters);

            var body = parameters.BodyPart;
            var bodyTire = parameters.BodyTire;
            var tire = parameters.TirePart;
            var tireBody = parameters.TireBody;

            ImGui.BeginTabBar("Menu3");

            if (ImguiCustomWidgets.BeginTab("Menu3", "Body Tire"))
            {
                if (ImGui.CollapsingHeader("Kart Tire", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();
                    update |= DrawFloat("Scale X", bodyTire, "TireScaleX");
                    update |= DrawFloat("Scale YZ", bodyTire, "TireScaleYZ");
                    EndColumns();
                }

                if (ImGui.CollapsingHeader("Tire Flags", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();
                    update |= DrawBool("UseTireCamberRateFront", bodyTire, "UseTireCamberRateFront");
                    update |= DrawBool("UseTireCamberRateFront", bodyTire, "UseTireCamberRateBack");
                    update |= DrawBool("UseTireCamberRateAntiG", bodyTire, "UseTireCamberRateAntiG");
                    EndColumns();
                }

                if (ImGui.CollapsingHeader("Body Flags", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();
                    update |= DrawBool("Has Gearbox", bodyTire, "HasGearbox");
                    update |= DrawBool("Has Fender", bodyTire, "HasFender");
                    update |= DrawBool("Use NormalArm When AntiG", bodyTire, "IsNormalArmWhenAntiG");
                    EndColumns();
                }

                if (ImGui.CollapsingHeader("Override Flags", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();
                    update |= DrawBool("UseBodyTireShaftFrontScale", bodyTire, "UseBodyTireShaftFrontScale");
                    update |= DrawBool("UseBodyTireShaftBackScale", bodyTire, "UseBodyTireShaftBackScale");
                    update |= DrawBool("UseBodyTireShaftFrontOffset", bodyTire, "UseBodyTireShaftFrontOffset");
                    update |= DrawBool("UseBodyTireShaftBackOffset", bodyTire, "UseBodyTireShaftBackOffset");
                    update |= DrawBool("UseBodyTireParamBG", bodyTire, "UseBodyTireParamBG");
                    update |= DrawBool("UseBodyTireParamBI", bodyTire, "UseBodyTireParamBI");
                    update |= DrawBool("UseBodyTireSusBackOffset", bodyTire, "UseBodyTireSusBackOffset");
                    update |= DrawBool("UseBodyTireSusFrontOffset", bodyTire, "UseBodyTireSusFrontOffset");
                    update |= DrawBool("UseBodyTireKartFrontTransform", bodyTire, "UseBodyTireKartFrontTransform");
                    update |= DrawBool("UseBodyTireKartBackTransform", bodyTire, "UseBodyTireKartBackTransform");
                    update |= DrawBool("UseBodyTireKartAntiGFrontTransform", bodyTire, "UseBodyTireKartAntiGFrontTransform");
                    update |= DrawBool("UseBodyTireKartAntiGBackTransform", bodyTire, "UseBodyTireKartAntiGBackTransform");
                    EndColumns();
                }

                if (ImGui.CollapsingHeader("Shaft Tire Transform", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();

                    if (bodyTire.UseBodyTireShaftFrontScale)
                    {
                        update |= DrawFloat("Front ScaleX", bodyTire.TireShaftTransform, "FrontScaleX");
                        update |= DrawFloat("Front ScaleYZ", bodyTire.TireShaftTransform, "FrontScaleYZ");
                    }
                    if (bodyTire.UseBodyTireShaftBackScale)
                    {
                        update |= DrawFloat("Back ScaleX", bodyTire.TireShaftTransform, "BackScaleX");
                        update |= DrawFloat("Back ScaleYZ", bodyTire.TireShaftTransform, "BackScaleYZ");
                    }

                    if (bodyTire.UseBodyTireShaftFrontOffset)
                        update |= DrawVector3("Front Offset", bodyTire.TireKartTransform, "FrontOffset");
                    if (bodyTire.UseBodyTireShaftBackOffset)
                        update |= DrawVector3("Back Offset", bodyTire.TireKartTransform, "BackOffset");

                    if (bodyTire.UseBodyTireShaftFrontOffset)
                        update |= DrawVector3("Front Anti G Offset", bodyTire.TireKartTransform, "FrontAntiGravityOffset");
                    if (bodyTire.UseBodyTireShaftBackOffset)
                        update |= DrawVector3("Back Anti G Offset", bodyTire.TireKartTransform, "BackAntiGravityOffset");

                    EndColumns();
                }

                if (ImGui.CollapsingHeader("Kart Tire Transform", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();

                    if (bodyTire.UseBodyTireKartFrontTransform)
                        update |= DrawVector3("Front Offset", bodyTire.TireKartTransform, "FrontOffset");
                    if (bodyTire.UseBodyTireKartBackTransform)
                        update |= DrawVector3("Back Offset", bodyTire.TireKartTransform, "BackOffset");

                    if (bodyTire.UseBodyTireKartAntiGFrontTransform)
                        update |= DrawVector3("Front Anti G Offset", bodyTire.TireKartTransform, "FrontAntiGravityOffset");
                    if (bodyTire.UseBodyTireKartAntiGFrontTransform)
                        update |= DrawVector3("Back Anti G Offset", bodyTire.TireKartTransform, "BackAntiGravityOffset");

                    EndColumns();
                }
                if (ImGui.CollapsingHeader("Kart Tire Camber", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();

                    if (bodyTire.UseTireCamberRateFront)
                        update |= DrawFloat("Front Rotation", bodyTire, "TireCamberRateFront");
                    if (bodyTire.UseBodyTireKartBackTransform)
                        update |= DrawFloat("Back Rotation", bodyTire, "TireCamberRateBack");
                    if (bodyTire.UseTireCamberRateAntiG)
                        update |= DrawFloat("Anti G Rotation", bodyTire, "TireCamberRateAntiG");

                    EndColumns();
                }
                ImGui.EndTabItem();
            }
            if (ImguiCustomWidgets.BeginTab("Menu3", "Tire Body Settings"))
            {
                if (ImGui.CollapsingHeader("Flags", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();

                    update |= DrawBool("UseTireBodyArmFront", tireBody, "UseTireBodyArmFront");
                    update |= DrawBool("UseTireBodyArmBack", tireBody, "UseTireBodyArmBack");
                    update |= DrawBool("UseTireBodySusOffsetFront", tireBody, "UseTireBodySusOffsetFront");
                    update |= DrawBool("UseTireBodySusOffsetBack", tireBody, "UseTireBodySusOffsetBack");

                    EndColumns();
                }
                if (ImGui.CollapsingHeader("Tire Arm Points", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();

                    if (tireBody.UseTireBodyArmFront)
                    {
             
                    }

                    if (tireBody.UseTireBodyArmBack)
                    {
                    
                    }

                    update |= DrawVector3("Arm Front Point Pos", tireBody.ArmFront, "ArmPointPos");
                    update |= DrawVector3("Arm Front Anti G Point Pos", tireBody.ArmFront, "ArmPointAntiGPos");

                    update |= DrawVector3("Arm Back Point Pos", tireBody.ArmBack, "ArmPointPos");
                    update |= DrawVector3("Arm Back Anti G Point Pos", tireBody.ArmBack, "ArmPointAntiGPos");

                    EndColumns();
                }
                if (ImGui.CollapsingHeader("Sus", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();

                    if (tireBody.UseTireBodySusOffsetFront)
                        update |= DrawVector3("Sus Arm Offset Front", tireBody, "SusArmOffsetFront");
                    if (tireBody.UseTireBodySusOffsetBack)
                        update |= DrawVector3("Sus Arm Offset Back", tireBody, "SusArmOffsetBack");

                    EndColumns();
                }
                ImGui.EndTabItem();
            }
            if (ImguiCustomWidgets.BeginTab("Menu3", "Tire Settings"))
            {
                ImGuiHelper.BoldText("These parameters affect all karts!");

                if (ImGui.CollapsingHeader("Tire Parameters", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();

                    update |= DrawFloat("Collision Width (Half)", tire, "TireWidthHalf");
                    update |= DrawFloat("Collision Radius (Affects Body Height)", tire, "TireColRadius");

                    update |= DrawFloat("Tire Spin Speed", tire, "TireRevolutionMax");
                    update |= DrawFloat("Tire Offset X", tire, "TireRotOffsetX");
                    update |= DrawFloat("Anti Gravity Offset X", tire, "TireRotOffsetXAntiG");
                    update |= DrawFloat("Anti Gravity Collision Radius", tire, "TireCollRadiusHoverPlus");
                    update |= DrawVector2("Unknown Param", tire, "UnknownParam");

                    EndColumns();
                }

                if (ImGui.CollapsingHeader("Tire Arm Points", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();

                    update |= DrawVector3("Arm Back Point Pos", tire.ArmBack, "ArmPointPos");
                    update |= DrawVector3("Arm Back Anti G Point Pos", tire.ArmBack, "ArmPointAntiGPos");

                    update |= DrawVector3("Arm Front Point Pos", tire.ArmFront, "ArmPointPos");
                    update |= DrawVector3("Arm Front Anti G Point Pos", tire.ArmFront, "ArmPointAntiGPos");

                    EndColumns();
                }

                if (ImGui.CollapsingHeader("Tire Camber Rate", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();

                    update |= DrawFloat("Front Rotation", tire, "TireCamberRateFront");
                    update |= DrawFloat("Back Rotation", tire, "TireCamberRateBack");
                    update |= DrawFloat("Anti Gravity Rotation", tire, "TireCamberRateAntiG");

                    EndColumns();
                }

                if (ImGui.CollapsingHeader("Sus", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    StartColumns();

                    update |= DrawVector3("Sus Arm Offset Front", tire, "SusArmOffsetFront");
                    update |= DrawVector3("Sus Arm Offset Back", tire, "SusArmOffsetBack");

                    EndColumns();
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();


            if (update)
            {
                body.UpdateSetters();
                bodyTire.UpdateSetters();
                tire.UpdateSetters();

                kart.ReloadParameters();
                GLContext.ActiveContext.UpdateViewport = true;
            }
        }

        public void RenderGearParams(KartVehicle kart, KartParameter parameters)
        {
            RenderCommon(kart, parameters);

            bool update = false;
            var body = parameters.BodyPart;

            if (ImGui.CollapsingHeader("Gear Box", ImGuiTreeNodeFlags.DefaultOpen))
            {
                StartColumns();

                update |= DrawVector3("Scale", body.GearBoxTransform, "Scale");
                update |= DrawVector3("Front Offset", body.GearBoxTransform, "FrontOffset");
                update |= DrawVector3("Back Offset", body.GearBoxTransform, "BackOffset");
                EndColumns();
            }

            if (update)
            {
                body.UpdateSetters();
                kart.ReloadParameters();
                GLContext.ActiveContext.UpdateViewport = true;
            }
        }

        private bool dialog_open = false;

        public void RenderCommon(KartVehicle kart, KartParameter parameters)
        {
            if (kart.Parameters == null)
                return;

            if (ImGui.Button("Save", new Vector2(ImGui.GetWindowWidth(), 30)))
                parameters.SavePartsBin();

            if (dialog_open)
            {
               if (PartSelectionDialog.Render(parameters.BodyID.ToString(), ref dialog_open))
                {
                    //ensure it changed
                    if (PartSelectionDialog.OutputName != parameters.BodyID.ToString())
                    {
                        var idx = (int)Enum.Parse(typeof(EBodyID), PartSelectionDialog.OutputName);
                        //Reload body parameters
                        parameters.BodyID = (EBodyID)idx;
                        parameters.SetupParts();
                        //Reload source
                        kart.ReloadBody(parameters);
                        kart.ReloadTire(parameters);
                        kart.Setup(parameters);

                        OnKartChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }

            ImGui.BeginColumns("kartSelectorClm", 2);

            ImGui.SetColumnWidth(0, 82);

            ImGui.Text("Body");
            ImGui.NextColumn();

            ImGui.PushItemWidth(ImGui.GetColumnWidth() - 10);

            if (ImGui.Button($"Change Body ({parameters.BodyID})"))
            {
                dialog_open = true;
            }

            ImGui.PopItemWidth();

            ImGui.NextColumn();

            ImGui.Text("Tire");
            ImGui.NextColumn();

            ImGui.PushItemWidth(ImGui.GetColumnWidth() - 10);

            bool IDNames = false;

            if (ImGui.BeginCombo("##Tire", parameters.TireID.ToString()))
            {
                for (int i = 0; i < MushResParts.TireNameList.Count; i++)
                {
                    string key = ((ETireID)i).ToString();
                    string text = IDNames ? key : MushResParts.TireNameList[i];

                    bool select = text == parameters.TireID.ToString();
                    if (ImGui.Selectable(text, select))
                    {
                        var idx = (int)Enum.Parse(typeof(ETireID), key);
                        //Reload body parameters
                        parameters.TireID = (ETireID)idx;
                        parameters.SetupParts();
                        //Reload source
                        kart.ReloadTire(parameters);
                        kart.Setup(parameters);

                        OnKartChanged?.Invoke(this, EventArgs.Empty);
                    }
                    if (select)
                        ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            ImGui.PopItemWidth();

            ImGui.NextColumn();
            ImGui.EndColumns();

            if (ImGui.SliderFloat("AntiGravityRatio", ref kart.AntiGravityRatio, 0, 1.0f))
            {
                kart.SetParam(kart.Parameters);
                GLContext.ActiveContext.UpdateViewport = true;
            }

            ImGui.Text($"Body ID: {parameters.BodyID}");
            ImGui.Text($"Tire ID: {parameters.TireID}");
            ImGui.Text($"Sus Front Type: {(SusType)parameters.SusFrontType}");
            ImGui.Text($"Sus Back Type: {(SusType)parameters.SusBackType}");
        }

        private void StartColumns()
        {
            ImGui.Columns(2);

            var width = Math.Max(ImGui.GetWindowWidth() - 150, 1);

          //  ImGui.SetColumnWidth(0, 150);
           // ImGui.SetColumnWidth(1, width);

        }

        private void EndColumns()
        {
            ImGui.Columns(1);
        }

        private bool DrawBool(string label, object obj, string property)
        {
            bool update = false;

            ImGui.Text(label);
            ImGui.NextColumn();

            var type = obj.GetType();
            var input = type.GetProperty(property);
            var v = (bool)input.GetValue(obj);

            ImGui.PushItemWidth(ImGui.GetColumnWidth() - 1);

            if (ImGui.Checkbox($"##{type}{label}", ref v))
            {
                input.SetValue(obj, v);
                update = true;
            }
            ImGui.PopItemWidth();

            ImGui.NextColumn();

            return update;
        }

        private bool DrawByte(string label, object obj, string property)
        {
            bool update = false;

            ImGui.Text(label);
            ImGui.NextColumn();

            var type = obj.GetType();
            var input = type.GetProperty(property);
            var v = (int)((byte)input.GetValue(obj));

            ImGui.PushItemWidth(ImGui.GetColumnWidth() - 1);

            if (ImGui.DragInt($"##{type}{label}", ref v, 0, 0, 255))
            {
                input.SetValue(obj, (byte)v);
                update = true;
            }
            ImGui.PopItemWidth();

            ImGui.NextColumn();

            return update;
        }

        private bool DrawInt(string label, object obj, string property)
        {
            bool update = false;

            ImGui.Text(label);
            ImGui.NextColumn();

            var type = obj.GetType();
            var input = type.GetProperty(property);
            var v = (int)input.GetValue(obj);

            ImGui.PushItemWidth(ImGui.GetColumnWidth() - 1);

            if (ImGui.DragInt($"##{type}{label}", ref v))
            {
                input.SetValue(obj, v);
                update = true;
            }
            ImGui.PopItemWidth();

            ImGui.NextColumn();

            return update;
        }

        private bool DrawFloat(string label, ref float v)
        {
            bool update = false;

            ImGui.Text(label);
            ImGui.NextColumn();

            ImGui.PushItemWidth(ImGui.GetColumnWidth() - 1);

            if (ImGui.DragFloat($"##{label}", ref v))
            {
                update = true;
            }
            ImGui.PopItemWidth();

            ImGui.NextColumn();

            return update;
        }

        private bool DrawFloat(string label, object obj, string property)
        {
            bool update = false;    

            ImGui.Text(label);
            ImGui.NextColumn();

            var type = obj.GetType();
            var input = type.GetProperty(property);
            var v = (float)input.GetValue(obj);

            ImGui.PushItemWidth(ImGui.GetColumnWidth() - 1);

            if (ImGui.DragFloat($"##{type}{label}", ref v))
            {
                input.SetValue(obj, v);
                update = true;
            }
            ImGui.PopItemWidth();

            ImGui.NextColumn();

            return update;
        }

        private bool DrawVector2(string label, object obj, string property)
        {
            bool update = false;

            ImGui.Text(label);
            ImGui.NextColumn();

            var type = obj.GetType();
            var input = type.GetProperty(property);
            var vec2 = (Vector2F)input.GetValue(obj);

            ImGui.PushItemWidth(ImGui.GetColumnWidth() - 1);

            Vector2 v = new Vector2(vec2.X, vec2.Y);
            if (ImGui.DragFloat2($"##{type}{label}", ref v))
            {
                input.SetValue(obj, new Vector2F(v.X, v.Y));
                update = true;
            }
            ImGui.PopItemWidth();

            ImGui.NextColumn();

            return update;
        }

        private bool DrawVector3(string label, object obj, string property)
        {
            bool update = false;

            ImGui.Text(label);
            ImGui.NextColumn();

            var type = obj.GetType();
            var input = type.GetProperty(property);
            var vec3 = (Vector3F)input.GetValue(obj);

            ImGui.PushItemWidth(ImGui.GetColumnWidth() - 1);

            Vector3 v = new Vector3(vec3.X, vec3.Y, vec3.Z);
            if (ImGui.DragFloat3($"##{type}{label}", ref v))
            {
                input.SetValue(obj, new Vector3F(v.X, v.Y, v.Z));
                update = true;
            }
            ImGui.PopItemWidth();

            ImGui.NextColumn();

            return update;
        }
    }
}
