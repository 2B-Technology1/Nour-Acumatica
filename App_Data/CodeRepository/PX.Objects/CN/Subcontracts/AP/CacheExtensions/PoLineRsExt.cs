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

using System;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.CS;
using PX.Objects.PO;
using Messages = PX.Objects.CN.Subcontracts.AP.Descriptor.Messages.Subcontract;

namespace PX.Objects.CN.Subcontracts.AP.CacheExtensions
{
    public sealed class PoLineRsExt : PXCacheExtension<POLineRS>
    {
        [PXString]
        [PXUIField(DisplayName = Messages.SubcontractNumber)]
        public string SubcontractNbr => Base.OrderNbr;

        [PXDate]
        [PXUIField(DisplayName = Messages.SubcontractDate)]
        public DateTime? SubcontractDate => Base.OrderDate;

        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        public abstract class subcontractNbr : BqlString.Field<subcontractNbr>
        {
        }

        public abstract class subcontractDate : BqlDateTime.Field<subcontractDate>
        {
        }
    }
}
