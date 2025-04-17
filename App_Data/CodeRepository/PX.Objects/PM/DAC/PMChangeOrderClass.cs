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
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.PM
{
	/// <summary>
	/// A change order class.
	/// Change order classes are used in <see cref="PMChangeOrder">change orders</see> and define
	/// which project data (the revenue budget, the cost budget, or commitments) can be adjusted
	/// with a change order of this class.
	/// The records of this type are created and edited through the Change Order Classes (PM203000) form
	/// (which corresponds to the <see cref="ChangeOrderClassMaint"/> graph).
	/// </summary>
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	[PXCacheName(Messages.ChangeOrderClass)]
	[PXPrimaryGraph(typeof(ChangeOrderClassMaint))]
	[Serializable]
	public class PMChangeOrderClass : PX.Data.IBqlTable
	{

		#region Keys
		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<PMChangeOrderClass>.By<PMChangeOrderClass.classID>
		{
			public static PMChangeOrderClass Find(PXGraph graph, string classID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, classID, options);
		}


		#endregion
		#region ClassID
		/// <inheritdoc cref="ClassID"/>
		public abstract class classID : PX.Data.BQL.BqlString.Field<classID>
		{
			public const int Length = 15;
		}

		/// <summary>
		/// The identifier of the change order class.
		/// </summary>
		[PXReferentialIntegrityCheck]
		[PXDBString(classID.Length, IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Class ID", Visibility = PXUIVisibility.SelectorVisible)]
		[PXSelector(typeof(PMChangeOrderClass.classID), DescriptionField = typeof(PMChangeOrderClass.description))]
		public virtual String ClassID
		{
			get;
			set;
		}
		#endregion

		#region Description
		/// <inheritdoc cref="Description"/>
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// The description of the change order class.
		/// </summary>
		[PXDBString(256, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Description
		{
			get;
			set;
		}
		#endregion
		#region IsCostBudgetEnabled
		/// <inheritdoc cref="IsCostBudgetEnabled"/>
		public abstract class isCostBudgetEnabled : PX.Data.BQL.BqlBool.Field<isCostBudgetEnabled> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the user can modify existing cost budget lines and add new ones with change orders of this class.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Cost Budget")]
		public virtual bool? IsCostBudgetEnabled
		{
			get;
			set;
		}
		#endregion
		#region IsRevenueBudgetEnabled
		/// <inheritdoc cref="IsRevenueBudgetEnabled"/>
		public abstract class isRevenueBudgetEnabled : PX.Data.BQL.BqlBool.Field<isRevenueBudgetEnabled> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the user can modify existing revenue budget lines and add new ones with change orders of this class.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Revenue Budget")]
		public virtual bool? IsRevenueBudgetEnabled
		{
			get;
			set;
		}
		#endregion
		#region IsPurchaseOrderEnabled
		/// <inheritdoc cref="IsPurchaseOrderEnabled"/>
		public abstract class isPurchaseOrderEnabled : PX.Data.BQL.BqlBool.Field<isPurchaseOrderEnabled> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the user can modify existing commitments and add new ones with change orders of this class.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName ="Commitments")]
		public virtual bool? IsPurchaseOrderEnabled
		{
			get;
			set;
		}
		#endregion
		#region IsActive
		/// <inheritdoc cref="IsActive"/>
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the change order class is available for selection on the Change Orders (PM308000) form.
		/// </summary>
		[PXUIField(DisplayName ="Active")]
		[PXDBBool]
		[PXDefault(true)]
		public virtual bool? IsActive
		{
			get;
			set;
		}
		#endregion
		#region IsAdvance
		/// <inheritdoc cref="IsAdvance"/>
		public abstract class isAdvance : PX.Data.BQL.BqlBool.Field<isAdvance> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the change order class supports two-tier change management.
		/// </summary>
		[PXDBBool()]
		[PXUIField(DisplayName = "Two-Tier Change Management", FieldClass = nameof(CS.FeaturesSet.ChangeRequest))]
		[PXDefault(false)]
		public virtual Boolean? IsAdvance
		{
			get;
			set;
		}
		#endregion

		#region IncrementsProjectNumber
		/// <inheritdoc cref="IncrementsProjectNumber"/>
		public abstract class incrementsProjectNumber : PX.Data.BQL.BqlBool.Field<incrementsProjectNumber> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the <see cref="IsRevenueBudgetEnabled"/> field is <see langword="true"/>.
		/// </summary>
		///  <value>
		/// The value of this field is defined by the <see cref="IsRevenueBudgetEnabled"/> field.
		/// See the attributes of the <see cref="IsRevenueBudgetEnabled"/> field for details.
		/// </value> 
		[PXBool]
		public virtual bool? IncrementsProjectNumber
		{
			get { return IsRevenueBudgetEnabled == true;  }
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(PMChangeOrderClass.description))]
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
		[PXDBCreatedByID]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBCreatedDateTime]
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
		[PXDBLastModifiedByID]
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
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
		[PXDBLastModifiedDateTime]
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
		#endregion

	}
}
