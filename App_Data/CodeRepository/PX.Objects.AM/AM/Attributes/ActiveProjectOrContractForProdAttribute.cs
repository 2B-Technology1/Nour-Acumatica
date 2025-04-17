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

using PX.Data;
using PX.Objects.AM.CacheExtensions;
using PX.Objects.PM;

namespace PX.Objects.AM.Attributes
{
    /// <summary>
    /// Manufacturing active project attribute for visible in production projects
    /// </summary>
    [PXDBInt]
    [PXUIField(DisplayName = "Project", Visibility = PXUIVisibility.Visible)]
    [PXRestrictor(typeof(Where<PMProject.isActive, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PX.Objects.PM.Messages.InactiveContract, typeof(PMProject.contractCD))]
    [PXRestrictor(typeof(Where<PMProject.isCompleted, Equal<False>>), PX.Objects.PM.Messages.CompleteContract, typeof(PMProject.contractCD))]
    [PXRestrictor(typeof(Where<PMProject.isCancelled, Equal<False>>), PX.Objects.PM.Messages.CancelledContract, typeof(PMProject.contractCD))]
    [PXRestrictor(typeof(Where<IsNull<PMProjectExt.visibleInPROD, True>, Equal<True>, Or<PMProject.nonProject, Equal<True>>>), PX.Objects.PM.Messages.ProjectInvisibleInModule, typeof(PMProject.contractCD))]
    public class ActiveProjectOrContractForProdAttribute : ActiveProjectOrContractBaseAttribute
    {
        public ActiveProjectOrContractForProdAttribute() : base(null)
        {
            Filterable = true;
        }

        public override void CacheAttached(PXCache sender)
        {
            base.CacheAttached(sender);
            Visible = ProjectHelper.IsPMVisible(sender.Graph);
        }
    }
}
