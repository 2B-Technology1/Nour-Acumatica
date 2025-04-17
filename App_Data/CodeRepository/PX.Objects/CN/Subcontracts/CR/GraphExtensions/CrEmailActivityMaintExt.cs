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
using PX.Data.EP;
using PX.Objects.CN.Subcontracts.CR.Helpers;
using PX.Objects.CR;
using PX.Objects.CS;

namespace PX.Objects.CN.Subcontracts.CR.GraphExtensions
{
    public class CrEmailActivityMaintExt : PXGraphExtension<CREmailActivityMaint>
    {
        public static bool IsActive()
        {
            return PXAccess.FeatureInstalled<FeaturesSet.construction>();
        }

        protected virtual void CrSmEmail_RowSelected(PXCache cache, PXRowSelectedEventArgs args,
            PXRowSelected baseHandler)
        {
            baseHandler(cache, args);
            if (args.Row is CRSMEmail email)
            {
                UpdateEntityDescriptionIfRequired(email);
            }
        }

        private void UpdateEntityDescriptionIfRequired(CRSMEmail email)
        {
            var entityDescription = SubcontractEntityDescriptionHelper.GetDescription(email, Base);
            if (entityDescription != null)
            {
                email.EntityDescription =
                    string.Concat(CacheUtility.GetErrorDescription(email.Exception), entityDescription);
            }
        }
    }
}
