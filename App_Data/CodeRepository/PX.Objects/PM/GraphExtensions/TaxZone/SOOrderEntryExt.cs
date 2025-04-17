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
using PX.Objects.CS;
using PX.Objects.SO;
using PX.Objects.CR;
using CRLocation = PX.Objects.CR.Standalone.Location;
using System;

namespace PX.Objects.PM.TaxZoneExtension
{
	public class SOOrderEntryExt : ProjectRevenueTaxZoneExtension<SOOrderEntry>
	{
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXFormula(typeof(Default<SOOrder.projectID>))]
		protected virtual void _(Events.CacheAttached<SOOrder.taxZoneID> e) { }

		[PXMergeAttributes(Method = MergeMethod.Replace)]
		[PXDBInt()]
		[SOShippingAddress2(typeof(
					Select2<Address,
						InnerJoin<CRLocation,
				  On<CRLocation.bAccountID, Equal<Address.bAccountID>,
				 And<Address.addressID, Equal<CRLocation.defAddressID>,
					 And<CRLocation.bAccountID, Equal<Current<SOOrder.customerID>>,
							And<CRLocation.locationID, Equal<Current<SOOrder.customerLocationID>>>>>>,
						LeftJoin<SOShippingAddress,
							On<SOShippingAddress.customerID, Equal<Address.bAccountID>,
							And<SOShippingAddress.customerAddressID, Equal<Address.addressID>,
							And<SOShippingAddress.revisionID, Equal<Address.revisionID>,
							And<SOShippingAddress.isDefaultAddress, Equal<True>>>>>>>,
						Where<True, Equal<True>>>))]
		protected virtual void _(Events.CacheAttached<SOOrder.shipAddressID> e)
		{
		}

		public static bool IsActive()
		{
			if (!PXAccess.FeatureInstalled<FeaturesSet.projectAccounting>())
				return false;

			ProjectSettingsManager settings = new ProjectSettingsManager();
			return settings.CalculateProjectSpecificTaxes;
		}

		protected override DocumentMapping GetDocumentMapping()
		{
			return new DocumentMapping(typeof(SOOrder))
			{
				ProjectID = typeof(SOOrder.projectID)
			};
		}

		protected override void SetDefaultShipToAddress(PXCache sender, Document row)
		{
			SOShippingAddress2Attribute.DefaultRecord<SOOrder.shipAddressID>(sender, row);
		}

		[PXOverride]
		public virtual string GetDefaultTaxZone(SOOrder row,
			Func<SOOrder, string> baseMethod)
		{
			//Do not redefault if value exists and overide flag is ON:
			if (row != null && row.OverrideTaxZone == true)
			{
				return row.TaxZoneID;
			}

			PMProject project = PMProject.PK.Find(Base, row?.ProjectID);
			if (project != null && !string.IsNullOrEmpty(project.RevenueTaxZoneID))
			{
				return project.RevenueTaxZoneID;
			}
			else
			{
				return baseMethod(row);
			}
		}
	}

}
