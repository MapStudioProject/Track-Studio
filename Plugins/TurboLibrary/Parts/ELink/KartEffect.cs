using Collada141;
using EffectLibrary;
using EffectLibrary.WiiU;
using MapStudio.UI;
using Octokit;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Core.ViewModels;
using TurboLibrary.Parts;
using TurboLibrary.Parts.Kart;

namespace TurboLibrary.EffectLinkU
{
    public class KartEffect
    {
        private static PTCL CommonPtcl;

        private NodeBase EffectFolder = new NodeBase("Effects");

        public void Init(FileEditor editor, KartVehicle kart)
        {
            if (CommonPtcl != null)
                return;

            editor.Root.AddChild(EffectFolder);

            //Init elink
            string filePath = GlobalSettings.GetContentPath(System.IO.Path.Combine("common", "effect", "elink.bin"));
            EffectManager.Instance.LoadElink(filePath);

            string ptclPath = GlobalSettings.GetContentPath(System.IO.Path.Combine("common", "effect", "common.ptcl"));
            EffectManager.Instance.LoadElink(filePath);

            CommonPtcl = new PTCL();
            CommonPtcl.FileInfo = new Toolbox.Core.File_Info();
            CommonPtcl.Load(File.OpenRead(ptclPath));

            EffectManager.AddPtclResource(CommonPtcl.FileHeader);

            EffectRenderer effectRenderer = new EffectRenderer();
            Workspace.ActiveWorkspace.StudioSystem.AddActor(effectRenderer); 
          //  editor.AddRender(effectRenderer);

            kart.OnKartTransform += delegate
            {
                foreach (EffectNode effect in this.EffectFolder.Children)
                    effect.UpdateMatrix(kart);
            };
        }

        public void Prepare( FileEditor editor, KartVehicle kart, EBodyID body)
        {
            editor.Root.AddChild(EffectFolder);

            EffectFolder.Children.Clear();
            /*
            var userData = EffectManager.ELink.Effects[body.ToString()];
            foreach (var container in userData.EffectResources.Values)
            {
                AddEffect(userData, kart, container.Name);
            }*/
        }

        private void AddEffect(UserHeader userHeader, KartVehicle kart, string resourceName)
        {
            EffectNode node = new EffectNode(userHeader.Name, resourceName);
            node.OnSelected += delegate
            {
                foreach (EffectNode effect in this.EffectFolder.Children)
                {
                    if (!effect.IsSelected)
                        effect.Dispose();
                }

                var mat = kart.GetKartMatrix();
                EffectManager.Instance.EmitDirect(ref node.Handle, userHeader.Name, resourceName, mat);
            };
            EffectFolder.AddChild(node);
        }

        class EffectNode : NodeBase
        {
            public Handle Handle = null;

            public string UserAssetName;

            public EffectNode(string userName, string resourceName)
            {
                UserAssetName = userName;
                Header = resourceName;
            }

            public void Dispose()
            {
                EffectManager.Instance.KillEmitterHandle(ref Handle);
                Handle = null;
            }

            public void UpdateMatrix(KartVehicle kart)
            {
                if (Handle == null)
                    return;

                Dispose();

                var mat = kart.GetKartMatrix();
                EffectManager.Instance.EmitDirect(ref Handle, UserAssetName, Header, mat);
            }
        }
    }
}
