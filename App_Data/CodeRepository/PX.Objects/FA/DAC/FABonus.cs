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
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;

namespace PX.Objects.FA
{
	[Serializable]
	[PXPrimaryGraph(typeof(BonusMaint))]
	[PXCacheName(Messages.Bonus)]
	public partial class FABonus : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<FABonus>.By<bonusID>
		{
			public static FABonus Find(PXGraph graph, Int32? bonusID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, bonusID, options);
		}
		#endregion
		#region BonusID
		public abstract class bonusID : PX.Data.BQL.BqlInt.Field<bonusID> { }
		protected Int32? _BonusID;
		[PXDBIdentity]
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXReferentialIntegrityCheck]
		public virtual Int32? BonusID
		{
			get
			{
				return _BonusID;
			}
			set
			{
				_BonusID = value;
			}
		}
		#endregion
		#region BonusCD
		public abstract class bonusCD : PX.Data.BQL.BqlString.Field<bonusCD> { }
		protected string _BonusCD;
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Bonus ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXDefault]
		[PXSelector(typeof(Search<bonusCD>))]
		[PX.Data.EP.PXFieldDescription]
		public virtual string BonusCD
		{
			get
			{
				return _BonusCD;
			}
			set
			{
				_BonusCD = value;
			}
		}
		#endregion
		#region State
		public abstract class state : PX.Data.BQL.BqlString.Field<state> { }
		protected string _State;
		[PXDBString(2, IsFixed = true)]
		[PXUIField(DisplayName = "State", Visible = false, Visibility = PXUIVisibility.Invisible)]
		public virtual string State
		{
			get
			{
				return _State;
			}
			set
			{
				_State = value;
			}
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected string _Description;
		[PXDBString(60, IsUnicode = true)]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PX.Data.EP.PXFieldDescription]
		public virtual string Description
		{
			get
			{
				return _Description;
			}
			set
			{
				_Description = value;
			}
		}
		#endregion

		#region NoteID

		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		[PXNote(DescriptionField = typeof(FABonus.bonusCD))]
		public virtual Guid? NoteID { get; set; }

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
	}
}
