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

namespace PX.Objects.PM
{
	using System;
	using PX.Data;

	/// <summary>
	/// A rate definition that is defined for a combination of a rate table and a rate type.
	/// The rate definition includes the sequence number and the types of factors to which the rate is applicable.
	/// The records of this type are created and edited through the Rate Lookup Rules (PM205000) form
	/// (which corresponds to the <see cref="RateDefinitionMaint"/> graph).
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.PMRateDefinition)]
	[PXPrimaryGraph(typeof(RateMaint))]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMRateDefinition : PX.Data.IBqlTable
	{
		#region RateDefinitionID
		/// <inheritdoc cref="RateDefinitionID"/>
		public abstract class rateDefinitionID : PX.Data.BQL.BqlInt.Field<rateDefinitionID> { }
		/// <summary>
		/// The identifier of the rate definition.
		/// </summary>
		protected int? _RateDefinitionID;
		/// <summary>
		/// The identifier of the rate definition.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		public virtual int? RateDefinitionID
		{
			get
			{
				return this._RateDefinitionID;
			}
			set
			{
				this._RateDefinitionID = value;
			}
		}
		#endregion
		#region RateTableID
		/// <inheritdoc cref="RateTableID"/>
		public abstract class rateTableID : PX.Data.BQL.BqlString.Field<rateTableID> { }
		/// <summary>
		/// The rate table to which the rate definition belongs.
		/// </summary>
		protected String _RateTableID;
		/// <summary>
		/// The rate table to which the rate definition belongs.
		/// </summary>
		[PXDefault]
		[PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true)]
		public virtual String RateTableID
		{
			get
			{
				return this._RateTableID;
			}
			set
			{
				this._RateTableID = value;
			}
		}
		#endregion
		#region RateTypeID
		/// <inheritdoc cref="RateTypeID"/>
		public abstract class rateTypeID : PX.Data.BQL.BqlString.Field<rateTypeID> { }
		/// <summary>
		/// The rate type to which the rate definition belongs.
		/// </summary>
		protected String _RateTypeID;
		/// <summary>
		/// The rate type to which the rate definition belongs.
		/// </summary>
		[PXDefault]
		[PXDBString(PMRateType.rateTypeID.Length, IsUnicode = true)]
		public virtual String RateTypeID
		{
			get
			{
				return this._RateTypeID;
			}
			set
			{
				this._RateTypeID = value;
			}
		}
		#endregion
		#region Sequence
		/// <inheritdoc cref="Sequence"/>
		public abstract class sequence : PX.Data.BQL.BqlShort.Field<sequence> { }
		/// <summary>
		/// The sequence of the rate table.
		/// </summary>
		protected Int16? _Sequence;
		/// <summary>
		/// The sequence of the rate table.
		/// </summary>
        [PXDefault(TypeCode.Int16, "1")]
		[PXDBShort]
		[PXUIField(DisplayName = "Sequence")]
		public virtual Int16? Sequence
		{
			get
			{
				return this._Sequence;
			}
			set
			{
				this._Sequence = value;
			}
		}
		#endregion
		#region Description
		/// <inheritdoc cref="Description"/>
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		/// <summary>
		/// The description of the rate definition.
		/// </summary>
		protected String _Description;
		/// <summary>
		/// The description of the rate definition.
		/// </summary>
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Description
		{
			get
			{
				return this._Description;
			}
			set
			{
				this._Description = value;
			}
		}
		#endregion
		#region Project
		/// <inheritdoc cref="Project"/>
		public abstract class project : PX.Data.BQL.BqlBool.Field<project> { }
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the project is a factor that affects rate selection.
		/// </summary>
		protected Boolean? _Project;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the project is a factor that affects rate selection.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Project")]
		public virtual Boolean? Project
		{
			get
			{
				return this._Project;
			}
			set
			{
				this._Project = value;
			}
		}
		#endregion
		#region Task
		/// <inheritdoc cref="Task"/>
		public abstract class task : PX.Data.BQL.BqlBool.Field<task> { }
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the task is a factor that affects rate selection.
		/// </summary>
		protected Boolean? _Task;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the task is a factor that affects rate selection.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Project Task")]
		public virtual Boolean? Task
		{
			get
			{
				return this._Task;
			}
			set
			{
				this._Task = value;
			}
		}
		#endregion
		#region AccountGroup
		/// <inheritdoc cref="AccountGroup"/>
		public abstract class accountGroup : PX.Data.BQL.BqlBool.Field<accountGroup> { }
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the account group is a factor that affects rate selection.
		/// </summary>
		protected Boolean? _AccountGroup;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the account group is a factor that affects rate selection.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Account Group")]
		public virtual Boolean? AccountGroup
		{
			get
			{
				return this._AccountGroup;
			}
			set
			{
				this._AccountGroup = value;
			}
		}
		#endregion
		#region RateItem
		/// <inheritdoc cref="RateItem"/>
		public abstract class rateItem : PX.Data.BQL.BqlBool.Field<rateItem> { }
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the inventory is a factor that affects rate selection.
		/// </summary>
		protected Boolean? _RateItem;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the inventory item is a factor that affects rate selection.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Inventory")]
		public virtual Boolean? RateItem
		{
			get
			{
				return this._RateItem;
			}
			set
			{
				this._RateItem = value;
			}
		}
		#endregion
		#region Employee
		/// <inheritdoc cref="Employee"/>
		public abstract class employee : PX.Data.BQL.BqlBool.Field<employee> { }
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the employee is a factor that affects rate selection.
		/// </summary>
		protected Boolean? _Employee;
		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the employee is a factor that affects rate selection.
		/// </summary>
		[PXDBBool()]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Employee")]
		public virtual Boolean? Employee
		{
			get
			{
				return this._Employee;
			}
			set
			{
				this._Employee = value;
			}
		}
		#endregion
		
		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(PMTask.taskCD))]
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
