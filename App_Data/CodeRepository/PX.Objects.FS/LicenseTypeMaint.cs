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

namespace PX.Objects.FS
{
    public class LicenseTypeMaint : PXGraph<LicenseTypeMaint, FSLicenseType>
    {
        [PXImport(typeof(FSLicenseType))]
        public PXSelect<FSLicenseType> LicenseTypeRecords;

		protected virtual void _(Events.RowDeleting<FSLicenseType> e)
		{
			if (e.Row == null)
				return;

			FSLicense license = PXSelect<FSLicense, Where<FSLicense.licenseTypeID, Equal<Required<FSLicenseType.licenseTypeID>>>>.SelectWindowed(this, 0, 1, e.Row.LicenseTypeID);
			FSServiceLicenseType service = PXSelect<FSServiceLicenseType, Where<FSServiceLicenseType.licenseTypeID, Equal<Required<FSLicenseType.licenseTypeID>>>>.SelectWindowed(this, 0, 1, e.Row.LicenseTypeID);
			if (license != null || service != null)
			{
				throw new PXException(TX.Error.RecordIsReferencedAtLicense);
			}

		}
	}
}
