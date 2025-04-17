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

using PX.Api;
using PX.Api.ContractBased;
using PX.Api.ContractBased.Models;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.SO;
using System.Globalization;
using System.Linq;

namespace PX.Commerce.Objects
{
	[PXVersion("20.200.001", "eCommerce")]
	[PXVersion("22.200.001", "eCommerce")]
	[PXVersion("23.200.001", "eCommerce")]
	public class CommerceEndpointImpl20
	{
		[FieldsProcessed(new[] {
			"Freight"
		})]
		protected virtual void Totals_Insert(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			Totals_Handler(graph, targetEntity);
		}
		[FieldsProcessed(new[] {
			"Freight"
		})]
		protected virtual void Totals_Update(PXGraph graph, EntityImpl entity, EntityImpl targetEntity)
		{
			Totals_Handler(graph, targetEntity);
		}
		protected virtual void Totals_Handler(PXGraph graph, EntityImpl targetEntity)
		{
			SOOrderEntry sograph = graph as SOOrderEntry;

			decimal? freight = null;
			EntityValueField freightField = targetEntity.Fields.SingleOrDefault(f => f.Name == "Freight") as EntityValueField;
			if (freightField?.Value != null)
				freight = decimal.Parse(freightField.Value, CultureInfo.InvariantCulture);
			if (freight != null && sograph.Document.Current != null)
			{
				SOOrder order = sograph.Document.Current;
				ShipTerms shipTerms = order.ShipTermsID != null ? PXSelectorAttribute.Select<SOOrder.shipTermsID>(sograph.Document.Cache, order) as ShipTerms : null;

				bool saveToFreight = order.OverrideFreightAmount == true || shipTerms == null || shipTerms.FreightAmountSource == FreightAmountSourceAttribute.OrderBased;
				if (saveToFreight)
				{
					if (order.OverrideFreightAmount != true)
						order.OverrideFreightAmount = true;
					order.CuryFreightAmt = freight;
				}
				else
					order.CuryPremiumFreightAmt = freight;

				order = sograph.Document.Update(order);
			}
		}
	}
}
