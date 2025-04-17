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

using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PX.Commerce.Shopify.API.REST
{
	/// <summary>
	/// The name of the tracking company.
	/// When creating a fulfillment for a supported carrier, send the tracking_company exactly as written in the list. 
	/// If the tracking company doesn't match one of the supported entries, then the shipping status might not be updated properly during the fulfillment process.
	/// </summary>
	[JsonConverter(typeof(StringEnumConverter))]
	public enum TrackingCompany
	{
		[EnumMember(Value = "4PX")]
		FourPX,

		[EnumMember(Value = "APC")]
		APC,

		[EnumMember(Value = "Amazon Logistics UK")]
		AmazonLogisticsUK,

		[EnumMember(Value = "Amazon Logistics US")]
		AmazonLogisticsUS,

		[EnumMember(Value = "Anjun Logistics")]
		AnjunLogistics,

		[EnumMember(Value = "Australia Post")]
		AustraliaPost,

		[EnumMember(Value = "Canada Post")]
		CanadaPost,

		[EnumMember(Value = "Canpar")]
		Canpar,

		[EnumMember(Value = "China Post")]
		ChinaPost,

		[EnumMember(Value = "Chukou1")]
		Chukou1,

		[EnumMember(Value = "Correios")]
		Correios,

		[EnumMember(Value = "DHL Express")]
		DHLExpress,

		[EnumMember(Value = "DHL eCommerce")]
		DHLeCommerce,

		[EnumMember(Value = "DHL eCommerce Asia")]
		DHLeCommerceAsia,

		[EnumMember(Value = "DPD")]
		DPD,

		[EnumMember(Value = "DPD Local")]
		DPDLocal,

		[EnumMember(Value = "DPD UK")]
		DPDUK,

		[EnumMember(Value = "Delhivery")]
		Delhivery,

		[EnumMember(Value = "Eagle")]
		Eagle,

		[EnumMember(Value = "FSC")]
		FSC,

		[EnumMember(Value = "FedEx")]
		FedEx,

		[EnumMember(Value = "GLS")]
		GLS,

		[EnumMember(Value = "GLS (US)")]
		GLSUS,

		[EnumMember(Value = "Globegistics")]
		Globegistics,

		[EnumMember(Value = "Japan Post (EN)")]
		JapanPostEN,

		[EnumMember(Value = "Japan Post (JA)")]
		JapanPostJA,

		[EnumMember(Value = "La Poste")]
		LaPoste,

		[EnumMember(Value = "New Zealand Post")]
		NewZealandPost,

		[EnumMember(Value = "Newgistics")]
		Newgistics,

		[EnumMember(Value = "PostNL")]
		PostNL,

		[EnumMember(Value = "PostNord")]
		PostNord,

		[EnumMember(Value = "Purolator")]
		Purolator,

		[EnumMember(Value = "Royal Mail")]
		RoyalMail,

		[EnumMember(Value = "SF Express")]
		SFExpress,

		[EnumMember(Value = "SFC Fulfillment")]
		SFCFulfillment,

		[EnumMember(Value = "Sagawa (EN)")]
		SagawaEN,

		[EnumMember(Value = "Sagawa (JA)")]
		SagawaJA,

		[EnumMember(Value = "Sendle")]
		Sendle,

		[EnumMember(Value = "Singapore Post")]
		SingaporePost,

		[EnumMember(Value = "TNT")]
		TNT,

		[EnumMember(Value = "UPS")]
		UPS,

		[EnumMember(Value = "USPS")]
		USPS,

		[EnumMember(Value = "Whistl")]
		Whistl,

		[EnumMember(Value = "Yamato (EN)")]
		YamatoEN,

		[EnumMember(Value = "Yamato (JA)")]
		YamatoJA,

		[EnumMember(Value = "YunExpress")]
		YunExpress,
	}
}
