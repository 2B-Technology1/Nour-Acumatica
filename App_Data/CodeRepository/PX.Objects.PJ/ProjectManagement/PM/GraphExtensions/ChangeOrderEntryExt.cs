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

using PX.Objects.PJ.ProjectManagement.Descriptor;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CN.Common.Extensions;
using PX.Objects.CS;
using PX.Objects.PM;

namespace PX.Objects.PJ.ProjectManagement.PM.GraphExtensions
{
    public class ChangeOrderEntryExt : PXGraphExtension<ChangeOrderEntry>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.constructionProjectManagement>();
        }

        public virtual void _(Events.RowSelected<PMChangeOrder> args)
        {
            if (Base.IsMobile && Base.Setup.Current.CostCommitmentTracking == false)
            {
                Base.Details.Cache.Enable(false);
            }
        }

        public virtual void _(Events.FieldSelecting<PMChangeOrder.classID> args)
        {
            if (Base.IsMobile && args.Row is PMChangeOrder changeOrder &&
                IsTwoTierLevelManagementEnabled(changeOrder.ClassID))
            {
                args.Cache.RaiseException<PMChangeOrder.classID>(
                    changeOrder, ProjectManagementMessages.ChangeOrderClassIsTwoTier, null, PXErrorLevel.RowWarning);
            }
        }

        private PMChangeOrderClass GetChangeOrderClass(string classId)
        {
            return SelectFrom<PMChangeOrderClass>
                .Where<PMChangeOrderClass.classID.IsEqual<P.AsString>>.View.Select(Base, classId);
        }

        private bool IsTwoTierLevelManagementEnabled(string classId)
        {
            return GetChangeOrderClass(classId)?.IsAdvance == true;
        }
    }
}
