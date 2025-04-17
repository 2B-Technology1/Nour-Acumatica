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
using PX.Commerce.Core;
using PX.Data;
using PX.Data.BQL;
using PX.Objects.IN;

namespace PX.Commerce.Objects
{
	/// <summary>
	/// DAC extension of InventoryItem to add additional properties.
	/// </summary>
	[Serializable]
	public sealed class BCInventoryItem : PXCacheExtension<InventoryItem>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		#region Visibility
		/// <summary>
		/// Indicates the visibility for this inventory item.
		/// </summary>
		[PXDBString(1, IsUnicode = true)]
        [PXUIField(DisplayName = "Visibility")]
		[BCItemVisibility.List]
		[PXDefault(BCItemVisibility.StoreDefault)]
        public string Visibility { get; set; }
		/// <inheritdoc cref="Visibility"/>
        public abstract class visibility : PX.Data.BQL.BqlString.Field<visibility> { }
		#endregion
		#region Availability
		/// <summary>
		/// Indicates the availability setting for the inventory item.
		/// </summary>
		[PXDBString(1, IsUnicode = true)]
		[PXUIField(DisplayName = "Availability")]
		[BCItemAvailabilities.ListDef]
		[PXDefault(BCItemAvailabilities.StoreDefault)]
		public string Availability { get; set; }
		/// <inheritdoc cref="Availability"/>
		public abstract class availability : PX.Data.BQL.BqlString.Field<availability> { }
		#endregion
		#region NotAvailMode
		/// <summary>
		/// Indicates the Not Available Setting to use when an item has no more quantity available.
		/// </summary>
		[PXDBString(1, IsUnicode = true)]
		[PXUIField(DisplayName = "When Qty Unavailable")]
		[BCItemNotAvailModes.ListDef]
		[PXDefault(BCItemNotAvailModes.StoreDefault)]
		[PXUIEnabled(typeof(Where<BCInventoryItem.availability, Equal<BCItemAvailabilities.availableTrack>>))]
		[PXFormula(typeof(Default<BCInventoryItem.availability>))]
		public string NotAvailMode { get; set; }
		/// <inheritdoc cref="NotAvailMode"/>
		public abstract class notAvailMode : PX.Data.BQL.BqlString.Field<notAvailMode> { }
		#endregion

		#region CustomURL
		/// <summary>
		/// The URL to use for this inventory item.
		/// </summary>
		[PXDBString(100, IsUnicode = true)]
        [PXUIField(DisplayName = "Custom URL")]
        public string CustomURL { get; set; }
		/// <inheritdoc cref="CustomURL"/>
		public abstract class customURL : PX.Data.BQL.BqlString.Field<customURL> { }
        #endregion
        #region PageTitle
		/// <summary>
		/// The title to use on this item's page.
		/// </summary>
		[PXDBLocalizableString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Page Title")]
        public string PageTitle { get; set; }
		/// <inheritdoc cref="PageTitle"/>
		public abstract class pageTitle : PX.Data.BQL.BqlString.Field<pageTitle> { }
		#endregion
		#region MetaDescription
		/// <summary>
		/// The meta description for this item.
		/// </summary>
		[PXDBLocalizableString(1024, IsUnicode = true)]
		[PXUIField(DisplayName = "Meta Description")]
        public string MetaDescription { get; set; }
		/// <inheritdoc cref="MetaDescription"/>
		public abstract class metaDescription : PX.Data.BQL.BqlString.Field<metaDescription> { }
		#endregion
		#region MetaKeywords
		/// <summary>
		/// A comma-separated list of meta keywords for this item.
		/// </summary>
		[PXDBLocalizableString(1024, IsUnicode = true)]
		[PXUIField(DisplayName = "Meta Keywords")]
		public string MetaKeywords { get; set; }
		/// <inheritdoc cref="MetaKeywords"/>
		public abstract class metaKeywords : PX.Data.BQL.BqlString.Field<metaKeywords> { }
		#endregion
		#region SearchKeywords
		/// <summary>
		/// A comma-separated list of search keywords for this item.
		/// </summary>
		[PXDBLocalizableString(1024, IsUnicode = true)]
		[PXUIField(DisplayName = "Search Keywords")]
		public string SearchKeywords { get; set; }
		/// <inheritdoc cref="SearchKeywords"/>
		public abstract class searchKeywords : PX.Data.BQL.BqlString.Field<searchKeywords> { }
		#endregion
		#region ShortDescription
		/// <summary>
		/// A short description for this item.
		/// </summary>
		[PXDBLocalizableString(1024, IsUnicode = true)]
		[PXUIField(DisplayName = "Short Description", Visible = false)]
		public string ShortDescription { get; set; }
		/// <inheritdoc cref="ShortDescription"/>
		public abstract class shortDescription : PX.Data.BQL.BqlString.Field<shortDescription> { }
		#endregion
	}
}
