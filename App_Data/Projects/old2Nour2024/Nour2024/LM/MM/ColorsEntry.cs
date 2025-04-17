using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;
using PX.Data;


namespace MyMaintaince
{
    public class ColorsEntry : PXGraph<ColorsEntry>
    {
        public PXSave<Colors> Save;
        public PXCancel<Colors> Cancel;
        [PXImport(typeof(Colors))]
        public PXSelect<Colors> colors;
    }
}