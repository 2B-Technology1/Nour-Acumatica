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

/// <summary>
/// VAT (MTD) API
/// https://developer.service.hmrc.gov.uk/api-documentation/docs/api/service/vat-api/1.0
/// </summary>
namespace PX.Objects.Localizations.GB.HMRC.Model
{
	[Serializable()]
	public class ObligationsRequest : RequestAuthorisation
	{
		/// <summary>
		/// Date from which to return obligations. Mandatory unless the status is O.
		/// For example: 2017-01-25
		/// </summary>
		public DateTime? from { get; set; }

		/// <summary>
		/// Date to which to return obligations. Mandatory unless the status is O.
		/// For example: 2017-01-25
		/// </summary>
		public DateTime? to { get; set; }

		/// <summary>
		/// Which obligation statuses to return (O=Open, F=Fulfilled)
		/// For example: F
		/// </summary>
		public string status { get; set; }
	}
}
