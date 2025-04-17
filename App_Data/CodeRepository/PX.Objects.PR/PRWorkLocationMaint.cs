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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL;
using PX.Payroll.Data;

namespace PX.Objects.PR
{
	public class WorkLocationsMaint : PXGraph<WorkLocationsMaint, PRLocation>
	{
		#region Views
		public PXSelect<PRLocation> Locations;

		public PXSelect<Address, Where<Address.addressID, Equal<Current<PRLocation.addressID>>>> Address;

		public SelectFrom<PREmployeeWorkLocation>
			.Where<PREmployeeWorkLocation.locationID.IsEqual<PRLocation.locationID.FromCurrent>>.View EmployeeWorkLocations;

		public SelectFrom<PREmployeeClassWorkLocation>
			.Where<PREmployeeClassWorkLocation.locationID.IsEqual<PRLocation.locationID.FromCurrent>>.View EmployeeClassWorkLocations;
		#endregion

		#region CacheAttached
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual void _(Events.CacheAttached<Address.postalCode> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual void _(Events.CacheAttached<Address.addressLine1> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual void _(Events.CacheAttached<Address.city> e) { }

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public virtual void _(Events.CacheAttached<Address.state> e) { }
		#endregion CacheAttached

		#region Events
		protected virtual void Address_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			Address row = (Address)e.Row;
			if (row != null)
			{
				bool allowAddressEditing = (Locations.Current.BranchID == null);
				PXUIFieldAttribute.SetEnabled<Address.addressLine1>(sender, row, allowAddressEditing);
				PXUIFieldAttribute.SetEnabled<Address.addressLine2>(sender, row, allowAddressEditing);
				PXUIFieldAttribute.SetEnabled<Address.city>(sender, row, allowAddressEditing);
				PXUIFieldAttribute.SetEnabled<Address.countryID>(sender, row, allowAddressEditing);
				PXUIFieldAttribute.SetEnabled<Address.state>(sender, row, allowAddressEditing);
				PXUIFieldAttribute.SetEnabled<Address.postalCode>(sender, row, allowAddressEditing);
			}
		}

		protected virtual void PRLocation_RowInserted(PXCache cache, PXRowInsertedEventArgs e)
		{
			// No-op, but leave there to avoid breaking change
		}

		protected virtual void Address_RowInserted(PXCache sender, PXRowInsertedEventArgs e)
		{
			Address addr = e.Row as Address;
			if (addr != null)
			{
				Locations.Current.AddressID = addr.AddressID;
			}
		}

		protected virtual void Address_CountryID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			Address addr = (Address)e.Row;
			if ((string)e.OldValue != addr.CountryID)
			{
				addr.State = null;
			}
		}

		protected virtual void PRLocation_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var row = e.Row as PRLocation;
			if (row.BranchID == null)
			{
				row.AddressID = null;
			}
			else
			{
				BAccount rec = PXSelectJoin<BAccountR,
					InnerJoin<Branch, 
						On<Branch.bAccountID, Equal<BAccountR.bAccountID>>>,
					Where<Branch.branchID, Equal<Required<Branch.branchID>>>>.Select(this, row.BranchID);
				row.AddressID = rec?.DefAddressID;
			}
		}

		protected virtual void _(Events.RowPersisting<Address> e)
		{
			TaxLocationHelpers.AddressPersisting(e);
		}

		protected virtual void _(Events.RowPersisting<PRLocation> e)
		{
			if (e.Row.BranchID.HasValue && !Equals(e.Row.BranchID, e.Cache.GetValueOriginal<PRLocation.branchID>(e.Row)))
			{
				if (this.IsContractBasedAPI)
				{
					Address.Current = Address.SelectSingle();
				}
				TaxLocationHelpers.UpdateAddressLocationCode(e.Cache.Graph, Address.Current);
				Address.Cache.PersistUpdated(Address.Current);
			}
		}

		protected virtual void _(Events.FieldVerifying<Address.countryID> e)
		{
			if (EmployeeWorkLocations.SelectSingle() != null || EmployeeClassWorkLocations.SelectSingle() != null)
			{
				throw new PXSetPropertyException<Address.countryID>(Messages.WorkLocationInUse);
			}
		}
		#endregion

		#region Buttons
		public PXAction<PRLocation> ViewOnMap;
		[PXUIField(DisplayName = "View On Map", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable viewOnMap(PXAdapter adapter)
		{
			BAccountUtility.ViewOnMap(this.Address.Current);
			return adapter.Get();
		}
		#endregion

		#region Address Lookup Extension
		/// <exclude/>
		public class WorkLocationsMaintAddressLookupExtension : CR.Extensions.AddressLookupExtension<WorkLocationsMaint, PRLocation, Address>
		{
			protected override string AddressView => nameof(Base.Address);
		}
		#endregion
	}
}
