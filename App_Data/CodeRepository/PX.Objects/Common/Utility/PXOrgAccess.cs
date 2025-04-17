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

namespace PX.Objects.Common
{
    public static class PXOrgAccess
    {
        public static string GetBaseCuryID(string bAccountCD)
        {
            var cd = bAccountCD?.Trim();

            return string.IsNullOrEmpty(cd)
                ? null
                : PXAccess.GetBranch(PXAccess.GetBranchID(cd))?.BaseCuryID ??
                  PXAccess.GetOrganizationByID(PXAccess.GetOrganizationID(cd))?.BaseCuryID;
        }
        
        public static string GetBaseCuryID(int? bAccountID) =>
            bAccountID == null
                ? null
                : PXAccess.GetBranchByBAccountID(bAccountID)?.BaseCuryID ??
                  PXAccess.GetOrganizationByBAccountID(bAccountID)?.BaseCuryID;

        public static string GetCD(int? bAccountID) =>
            bAccountID == null
                ? null
                : PXAccess.GetBranchByBAccountID(bAccountID)?.BranchCD ??
                  PXAccess.GetOrganizationByBAccountID(bAccountID)?.OrganizationCD;
    }
}