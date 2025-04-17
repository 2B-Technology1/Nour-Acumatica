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
using System;

namespace PX.Objects.CR.Extensions.CRCreateSalesOrder
{
	/// <exclude/>
	[PXHidden]
	public class Document : PXMappedCacheExtension
	{
		#region OpportunityID
		public abstract class opportunityID : IBqlField { }
		public virtual string OpportunityID { get; set; }
		#endregion

		#region QuoteID
		public abstract class quoteID : IBqlField { }
		public virtual Guid? QuoteID { get; set; }
		#endregion

		#region Subject
		public abstract class subject : IBqlField { }
		public virtual string Subject { get; set; }
		#endregion

		#region BAccountID
		public abstract class bAccountID : IBqlField { }
		public virtual int? BAccountID { get; set; }
		#endregion

		#region LocationID
		public abstract class locationID : IBqlField { }
		public virtual int? LocationID { get; set; }
		#endregion

		#region ContactID
		public abstract class contactID : IBqlField { }
		public virtual int? ContactID { get; set; }
		#endregion

		#region TaxZoneID
		public abstract class taxZoneID : IBqlField { }
		public virtual string TaxZoneID { get; set; }
		#endregion

		#region TaxCalcMode
		public abstract class taxCalcMode : IBqlField { }
		public virtual string TaxCalcMode { get; set; }
		#endregion

		#region ManualTotalEntry
		public abstract class manualTotalEntry : IBqlField { }
		public virtual bool? ManualTotalEntry { get; set; }
		#endregion

		#region CuryID
		public abstract class curyID : IBqlField { }
		public virtual string CuryID { get; set; }
		#endregion

		#region CuryInfoID
		public abstract class curyInfoID : IBqlField { }
		public virtual Int64? CuryInfoID { get; set; }
		#endregion

		#region CuryDiscTot
		public abstract class curyDiscTot : IBqlField { }
		public virtual decimal? CuryDiscTot { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : IBqlField { }
		public virtual int? ProjectID { get; set; }
		#endregion

		#region BranchID
		public abstract class branchID : IBqlField { }
		public virtual int? BranchID { get; set; }
		#endregion

		#region NoteID
		public abstract class noteID : IBqlField { }
		public virtual Guid? NoteID { get; set; }
		#endregion

		#region TermsID
		public abstract class termsID : IBqlField { }
		public virtual string TermsID { get; set; }
		#endregion

		#region ExternalTaxExemptionNumber
		public abstract class externalTaxExemptionNumber : IBqlField { }
		public virtual string ExternalTaxExemptionNumber { get; set; }
		#endregion

		#region AvalaraCustomerUsageType
		public abstract class avalaraCustomerUsageType : IBqlField { }
		public virtual string AvalaraCustomerUsageType { get; set; }
		#endregion

		#region IsPrimary
		public abstract class isPrimary : IBqlField { }
		public virtual bool? IsPrimary { get; set; }
		#endregion

		#region SiteID
		public abstract class siteID : IBqlField { }
		public virtual Int32? SiteID { get; set; }
		#endregion
		#region CarrierID
		public abstract class carrierID : IBqlField { }
		public virtual String CarrierID { get; set; }
		#endregion
		#region ShipTermsID
		public abstract class shipTermsID : IBqlField { }
		public virtual String ShipTermsID { get; set; }
		#endregion
		#region ShipZoneID
		public abstract class shipZoneID : IBqlField { }
		public virtual String ShipZoneID { get; set; }
		#endregion
		#region FOBPointID
		public abstract class fOBPointID : IBqlField { }
		public virtual String FOBPointID { get; set; }
		#endregion
		#region Resedential
		public abstract class resedential : IBqlField { }
		public virtual Boolean? Resedential { get; set; }
		#endregion
		#region SaturdayDelivery
		public abstract class saturdayDelivery : IBqlField { }
		public virtual Boolean? SaturdayDelivery { get; set; }
		#endregion
		#region Insurance
		public abstract class insurance : IBqlField { }
		public virtual Boolean? Insurance { get; set; }
		#endregion
		#region ShipComplete
		public abstract class shipComplete : IBqlField { }
		public virtual String ShipComplete { get; set; }
		#endregion
	}
}
