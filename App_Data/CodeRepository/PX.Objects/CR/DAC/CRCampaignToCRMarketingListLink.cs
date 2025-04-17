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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity;
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.CR
{
	/// <summary>
	/// CRMarketing list included into CRCampaign
	/// </summary>
	[PXCacheName(Messages.CRCampaignToCRMarketingListLink)]
	public partial class CRCampaignToCRMarketingListLink : PX.Data.IBqlTable
	{
		#region Keys
		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<CRCampaignToCRMarketingListLink>.By<campaignID, marketingListID>
		{
			public static CRCampaignToCRMarketingListLink Find(PXGraph graph, string campaignID, int marketingListID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, campaignID, marketingListID, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Campaign FK
			/// </summary>
			public class Campaign : CR.CRCampaign.PK.ForeignKeyOf<CRCampaign>.By<campaignID> { }

			/// <summary>
			/// MarketingList FK
			/// </summary>
			public class MarketingList : CR.CRMarketingList.PK.ForeignKeyOf<CRMarketingList>.By<marketingListID> { }
		}
		#endregion

		#region SelectedForCampaign
		public new abstract class selectedForCampaign : PX.Data.BQL.BqlBool.Field<selectedForCampaign> { }

		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public new virtual bool? SelectedForCampaign { get; set; }
		#endregion

		#region CampaignID
		public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }

		/// <summary>
		/// A reference to the <see cref="CRCampaign.campaignID"/>.
		/// </summary>
		[PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCC")]
		[PXDefault(typeof(Current<CRCampaign.campaignID>))]
		[PXUIField(DisplayName = Messages.CampaignID, Visibility = PXUIVisibility.SelectorVisible)]
		[PXParent(typeof(FK.Campaign))]
		public virtual String CampaignID { get; set; }
		#endregion

		#region MarketingListID
		public abstract class marketingListID : PX.Data.BQL.BqlInt.Field<marketingListID> { }

		/// <summary>
		/// A reference to the <see cref="CRMarketingList.marketingListID"/>.
		/// </summary>
		[PXDBInt(IsKey = true)]
		[PXForeignReference(typeof(FK.MarketingList), ReferenceBehavior.Restrict)]
		[PXUIField(Visible = true)]
		public virtual Int32? MarketingListID { get; set; }
		#endregion

		#region LastUpdateDate
		public abstract class lastUpdateDate : PX.Data.BQL.BqlDateTime.Field<lastUpdateDate> { }

		/// <summary>
		/// The date of the last update of the marketing campaign members from the current list.
		/// </summary>
		[PXDBDate(PreserveTime = true, InputMask = "g")]
		[PXUIField(DisplayName = "Last Updated On", Visible = true, Enabled = false)]
		public virtual DateTime? LastUpdateDate { get; set; }
		#endregion
	}

	/// <exclude/>
	[PXHidden]
	[PXProjection(typeof(
			Select2<
				CRMarketingList,
			LeftJoin<CRCampaignToCRMarketingListLink,
				On<CRMarketingList.marketingListID, Equal<CRCampaignToCRMarketingListLink.marketingListID>,
				And<CRCampaignToCRMarketingListLink.campaignID, Equal<CurrentValue<CRCampaign.campaignID>>>>>>),
		Persistent = false)]
	public class CRMarketingListWithLinkToCRCampaign : CRMarketingList
	{
		#region SelectedForCampaign
		public abstract class selectedForCampaign : PX.Data.BQL.BqlBool.Field<selectedForCampaign> { }

		[PXDBBool(BqlField = typeof(CRCampaignToCRMarketingListLink.selectedForCampaign))]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? SelectedForCampaign { get; set; }
		#endregion

		#region CampaignID
		public abstract class campaignID : PX.Data.BQL.BqlString.Field<campaignID> { }

		/// <summary>
		/// A reference to the <see cref="CRCampaign.campaignID"/>.
		/// </summary>
		[PXDBString(15, IsUnicode = true, InputMask = ">CCCCCCCCCCCCCCC", BqlField = typeof(CRCampaignToCRMarketingListLink.campaignID))]
		[PXDefault(typeof(Current<CRCampaign.campaignID>))]
		[PXUIField(DisplayName = Messages.CampaignID, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String CampaignID { get; set; }
		#endregion

		#region LastUpdateDate
		public abstract class lastUpdateDate : PX.Data.BQL.BqlDateTime.Field<lastUpdateDate> { }

		/// <summary>
		/// The date of the last update of the marketing campaign members from the current list.
		/// </summary>
		[PXDBDate(PreserveTime = true, InputMask = "g", BqlField = typeof(CRCampaignToCRMarketingListLink.lastUpdateDate))]
		[PXUIField(DisplayName = "Last Updated On", Visible = true, Enabled = false)]
		public virtual DateTime? LastUpdateDate { get; set; }
		#endregion
	}
}
