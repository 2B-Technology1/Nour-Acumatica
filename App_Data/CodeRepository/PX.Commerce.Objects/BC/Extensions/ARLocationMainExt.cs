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
using PX.Commerce.Core;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AR;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.SM;

namespace PX.Commerce.Objects
{
	public class BCLocationMaintExt : PXGraphExtension<CustomerLocationMaint>
	{
		public static bool IsActive() { return CommerceFeaturesHelper.CommerceEdition; }

		public bool SkipBAccountUpdate { get; set; }
		public bool ClearCache { get; set; }

		//Dimension Key Generator
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[BCCustomNumbering(LocationIDAttribute.DimensionName, typeof(BCBindingExt.locationTemplate), typeof(BCBindingExt.locationNumberingID),
			typeof(Select2<BCBindingExt,
				InnerJoin<BCBinding, On<BCBinding.bindingID, Equal<BCBindingExt.bindingID>>>,
				Where<BCBinding.connectorType, Equal<Required<BCBinding.connectorType>>,
					And<BCBinding.bindingID, Equal<Required<BCBinding.bindingID>>>>>))]
		public void _(Events.CacheAttached<Location.locationCD> e) { }

		//Sync Time 
		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PX.Commerce.Core.BCSyncExactTime()]
		public void Location_LastModifiedDateTime_CacheAttached(PXCache sender) { }
		[PXCopyPasteHiddenView]
		public PXSelect<Override.BAccount> Account;

		public virtual void Location_RowPersisted(PXCache sender, PXRowPersistedEventArgs e)
		{
			Location row = e.Row as Location;
			var activeLocation = PXSelect<BCEntity, Where<BCEntity.entityType, Equal<Required<BCEntity.entityType>>, And<BCEntity.isActive, Equal<True>>>>.Select(Base, BCEntitiesAttribute.Address)?.RowCast<BCEntity>()?.ToList();
			//TODO :CHeck if location active
			if (row != null && row.BAccountID != null && activeLocation?.Count() > 0 && !SkipBAccountUpdate)
			{
				Override.BAccount item = Override.BAccount.PK.Find(Base, row.BAccountID);
				BAccount bAccount = BAccount.PK.Find(Base, row.BAccountID);

				if (item != null && sender.Graph.Caches[typeof(BAccount)]?.GetStatus(bAccount) != PXEntryStatus.Updated)
				{
					Account.Cache.SetStatus(item, PXEntryStatus.Updated);
				}
			}
		}
		protected virtual void Location_RowPersisting(PXCache sender, PXRowPersistingEventArgs e, PXRowPersisting baseHandler)
		{
			if (ClearCache)
			{
				Base.Caches[typeof(BAccount)].Clear();
				Base.Caches[typeof(BAccount)].ClearQueryCache();
				Base.Caches[typeof(Location)].Clear();
				Base.Caches[typeof(Location)].ClearQueryCache();
				Account.Cache.Clear();
				Account.Cache.ClearQueryCache();
			}
			baseHandler?.Invoke(sender, e);
		}

	}
}

namespace PX.Commerce.Objects.Override
{
	[PXHidden]
	public class BAccount : PX.Data.IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<BAccount>.By<bAccountID>
		{
			public static BAccount Find(PXGraph graph, int? bAccountID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bAccountID, options);

		}
		#endregion

		#region BAccountID
		public abstract class bAccountID : PX.Data.BQL.BqlInt.Field<bAccountID> { }
		protected Int32? _BAccountID;
		[PXDBIdentity()]
		[PXUIField(DisplayName = "Account ID", Visibility = PXUIVisibility.Visible, Visible = false)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? BAccountID
		{
			get
			{
				return this._BAccountID;
			}
			set
			{
				this._BAccountID = value;
			}
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote]
		public virtual Guid? NoteID
		{
			get
			{
				return this._NoteID;
			}
			set
			{
				this._NoteID = value;
			}
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion

		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		protected Byte[] _tstamp;
		[PXDBTimestamp()]
		public virtual Byte[] tstamp
		{
			get
			{
				return this._tstamp;
			}
			set
			{
				this._tstamp = value;
			}
		}
		#endregion
	}
}
