/* ---------------------------------------------------------------------*
*                             Acumatica Inc.                            *

*              Copyright (c) 2005-2023 All rights reserved.             *

*                                                                       *

*                                                                       *

* This file and its contents are protected by United States and         *

* International copyright laws.  Unauthorized reproduction and/or       *

* distribution of all or any portion of the code contained herein       *

* is strictly prohibited and will result in severe civil and criminal   *

* penalties.  Any violations of this copyright will be prosecuted       *

* to the fullest extent possible under law.                             *

*                                                                       *

* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *

* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *

* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY ACUMATICA PRODUCT.       *

*                                                                       *

* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *

* --------------------------------------------------------------------- */

using Autofac;
using PX.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Compilation;

namespace PX.Objects.IN.Matrix.GraphExtensions
{
    public class ExtensionSorting : Module
    {
        protected override void Load(ContainerBuilder builder) => builder.RunOnApplicationStart(() =>
            PXBuildManager.SortExtensions += list => PXBuildManager.PartialSort(list, _order)
            );

        private static readonly Dictionary<Type, int> _order = new Dictionary<Type, int>
        {
            {typeof(CreateMatrixItemsTabExt), 1 },
            {typeof(ApplyToMatrixItemsExt), 2 }
        };
    }
}
