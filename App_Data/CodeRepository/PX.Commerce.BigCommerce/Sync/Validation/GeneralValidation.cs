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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Commerce.Core;
using PX.Objects.AR;
using PX.Objects.CS;

namespace PX.Commerce.BigCommerce
{
	public class GeneralValidator : BCBaseValidator, ISettingsValidator, IExternValidator
	{
		public int Priority { get { return int.MaxValue; } }

		public virtual void Validate(IProcessor processor)
		{
			if (PXAccess.FeatureInstalled<FeaturesSet.subItem>() == true)
				throw new PXException(BCMessages.FeatureNotSupported, PXMessages.LocalizeNoPrefix(BCCaptions.InventorySubitems));
			if (PXAccess.FeatureInstalled<FeaturesSet.financialStandard>() == false)
				throw new PXException(BCMessages.FeatureRequired, PXMessages.LocalizeNoPrefix(BCCaptions.StandardFinancials));
			if (PXAccess.FeatureInstalled<FeaturesSet.accountLocations>() == false)
				throw new PXException(BCMessages.FeatureRequired, PXMessages.LocalizeNoPrefix(BCCaptions.BusinessAccountsLocation));
			if (PXAccess.FeatureInstalled<FeaturesSet.distributionModule>() == false)
				throw new PXException(BCMessages.FeatureRequired, PXMessages.LocalizeNoPrefix(BCCaptions.Distribution));
		}

		public virtual void Validate(IProcessor processor, IExternEntity entity)
		{
			RunAttributesValidation(processor, entity);
		}
	}
}
