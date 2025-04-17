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

using System.Collections;
using PX.Data;
using PX.Objects.CS;
using PX.Data.BQL.Fluent;

namespace PX.Objects.CR.Extensions.CRDuplicateEntities
{
	/// <exclude/>
	public class CRDuplicateEntitiesDummyLead : CRDuplicateEntitiesDummy<LeadMaint, CRLead>
	{
		public static bool IsActive()
		{
			return !PXAccess.FeatureInstalled<FeaturesSet.contactDuplicate>();
		}
	}

	/// <exclude/>
	public class CRDuplicateEntitiesDummyContact : CRDuplicateEntitiesDummy<ContactMaint, Contact>
	{
		public static bool IsActive()
		{
			return !PXAccess.FeatureInstalled<FeaturesSet.contactDuplicate>();
		}
	}

	/// <exclude/>
	public class CRDuplicateEntitiesDummyAccount : CRDuplicateEntitiesDummy<BusinessAccountMaint, BAccount>
	{
		public static bool IsActive()
		{
			return !PXAccess.FeatureInstalled<FeaturesSet.contactDuplicate>();
		}
	}
	/// <summary>
	/// Extension that is used when ordinary <see cref="CRDuplicateEntities"/> extension is switched off. This extension is made as a stub to avoid "View/Action doesn't exist" error.
	/// </summary>
	public abstract class CRDuplicateEntitiesDummy<TGraph, TMain> : PXGraphExtension<TGraph>
		where TGraph : PXGraph
		where TMain : class, IBqlTable, new()
	{
		[PXHidden]
		[PXCopyPasteHiddenView]
		// overridden in CRDuplicateEntities extension
		public SelectFrom<
				CRDuplicateRecord>
			.LeftJoin<Contact>
				.On<Contact.contactID.IsEqual<CRDuplicateRecord.contactID>>
			.LeftJoin<DuplicateContact>
				.On<DuplicateContact.contactID.IsEqual<CRDuplicateRecord.duplicateContactID>>
			.LeftJoin<BAccountR>
				.On<BAccountR.bAccountID.IsEqual<DuplicateContact.bAccountID>>
			.LeftJoin<Standalone.CRLead>
				.On<Standalone.CRLead.contactID.IsEqual<CRDuplicateRecord.contactID>>
			.OrderBy<
				Asc<DuplicateContact.contactPriority>,
				Asc<DuplicateContact.contactID>>
			.View Duplicates;
		protected virtual IEnumerable duplicates()
		{
			yield break;
		}

		#region Actions

		public PXAction<TMain> CheckForDuplicates;
		[PXUIField(DisplayName = Messages.CheckForDuplicates, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXButton(ImageKey = PX.Web.UI.Sprite.Main.Process, DisplayOnMainToolbar = false)]
		public virtual IEnumerable checkForDuplicates(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<TMain> DuplicateMerge;
		[PXUIField(DisplayName = Messages.Merge, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable duplicateMerge(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<TMain> DuplicateAttach;
		[PXUIField(DisplayName = Messages.LinkToEntity, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable duplicateAttach(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<TMain> ViewDuplicate;
		[PXUIField(DisplayName = Messages.View, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable viewDuplicate(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<TMain> ViewDuplicateRefContact;
		[PXUIField(DisplayName = Messages.View, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable viewDuplicateRefContact(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<TMain> MarkAsValidated;
		[PXUIField(DisplayName = Messages.MarkAsValidated, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable markAsValidated(PXAdapter adapter)
		{
			return adapter.Get();
		}

		public PXAction<TMain> CloseAsDuplicate;
		[PXUIField(DisplayName = Messages.CloseAsDuplicate, MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Update, Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXButton(DisplayOnMainToolbar = false)]
		public virtual IEnumerable closeAsDuplicate(PXAdapter adapter)
		{
			return adapter.Get();
		}

		#endregion
	}
}
