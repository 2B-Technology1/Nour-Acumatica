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
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.IN
{
	/// <summary>
	/// Represents a Auto-Incremental Value of a Lot/Serial Class.
	/// Auto-Incremental Value of a Lot/Serial Class are available only if the <see cref="FeaturesSet.LotSerialTracking">Lot/Serial Tracking</see> feature is enabled.
	/// The records of this type are created on the Lot/Serial Classes (IN207000) form
	/// (which corresponds to the <see cref="INLotSerClassMaint"/> graph)
	/// </summary>
	[Serializable]
	[PXPrimaryGraph(typeof(INLotSerClassMaint))]
	[PXCacheName(Messages.LotSerClassAutoIncrementalValue)]
	public class INLotSerClassLotSerNumVal : IBqlTable, ILotSerNumVal
	{
		#region Keys
		public class PK : PrimaryKeyOf<INLotSerClassLotSerNumVal>.By<lotSerClassID>
		{
			public static INLotSerClassLotSerNumVal Find(PXGraph graph, string lotSerClassID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, lotSerClassID, options);
			public static INLotSerClassLotSerNumVal FindDirty(PXGraph graph, string lotSerClassID)
				=> (INLotSerClassLotSerNumVal)PXSelect<INLotSerClassLotSerNumVal,
					Where<lotSerClassID, Equal<Required<lotSerClassID>>>>.SelectWindowed(graph, 0, 1, lotSerClassID);
		}
		public static class FK
		{
			public class LotSerialClass : INLotSerClass.PK.ForeignKeyOf<INLotSerClassLotSerNumVal>.By<lotSerClassID> { }
		}
		#endregion
		#region LotSerClassID
		public abstract class lotSerClassID : BqlString.Field<lotSerClassID> { }
		protected string _LotSerClassID;

		/// <summary>
		/// The <see cref="INLotSerClass">lot/serial class</see>, to which the item is assigned.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="INLotSerClass.LotSerClassID"/> field.
		/// </value>
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(INLotSerClass.lotSerClassID))]
		[PXParent(typeof(FK.LotSerialClass))]
		public virtual string LotSerClassID
		{
			get
			{
				return this._LotSerClassID;
			}
			set
			{
				this._LotSerClassID = value;
			}
		}
		#endregion
		#region LotSerNumVal
		public abstract class lotSerNumVal : BqlString.Field<lotSerNumVal> { }
		protected string _LotSerNumVal;
		[PXDBString(30, InputMask = "999999999999999999999999999999")]
		[PXUIField(DisplayName = "Auto-Incremental Value")]
		public virtual string LotSerNumVal
		{
			get
			{
				return this._LotSerNumVal;
			}
			set
			{
				this._LotSerNumVal = value;
			}
		}
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
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
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
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
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
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
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
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
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
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
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
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
		public abstract class Tstamp : BqlByteArray.Field<Tstamp> { }
		protected byte[] _tstamp;
		[PXDBTimestamp(VerifyTimestamp = VerifyTimestampOptions.FromRecord)]
		public virtual byte[] tstamp
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
