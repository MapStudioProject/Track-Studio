using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurboLibrary;
using GLFrameworkEngine;
using Toolbox.Core.ViewModels;

namespace TurboLibrary.MuuntEditor
{
    public class PathWrapper : NodeBase
    {
        public override string Header
        {
            get
            {
                return $"Path{Index}";
            }
        }

        public override bool IsSelected
        { 
            get => base.IsSelected;
            set
            {
                if (base.IsSelected != value) {
                    base.IsSelected = value;
                }
            }
        }

        public PathWrapper(UnitObject unitObject) 
        {
            HasCheckBox = true;
            Tag = unitObject;
        }
    }
}
