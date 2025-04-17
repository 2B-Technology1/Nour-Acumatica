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
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Objects.GL;
using PX.Objects.GL.DAC;

namespace PX.Objects.CM
{
	public class CMSetupMaint : PXGraph<CMSetupMaint>
	{
		public PXSelect<CMSetup> cmsetup;
		public PXSave<CMSetup> Save;
		public PXCancel<CMSetup> Cancel;
		public PXSelectJoin<Currency, 
			InnerJoin<Organization, On<Organization.baseCuryID, Equal<Currency.curyID>>,
				InnerJoin<Branch, On<Branch.organizationID, Equal<Organization.organizationID>>>>,
			Where<Branch.branchID, Equal<Current<AccessInfo.branchID>>>> basecurrency;
		public PXSetup<Company> company;
		public PXSelect<TranslDef, Where<TranslDef.translDefId, Equal<Current<CMSetup.translDefId>>>> baseTranslDef;

		public CMSetupMaint()
		{
			Company setup = company.Current;
			if (string.IsNullOrEmpty(setup.BaseCuryID))
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Company), PXMessages.LocalizeNoPrefix(CS.Messages.BranchMaint));
			}
		}
	}
}
