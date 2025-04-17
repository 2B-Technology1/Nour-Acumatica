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
	/// A combination of rate table, rate type, rate code, and rate sequence for which <see cref="PMRate">rates</see> can be set.
	/// The records of this type are created and edited through the Rate Tables (PM206000) form
	/// (which corresponds to the <see cref="RateMaint"/> graph).
	/// </summary>
	[System.SerializableAttribute()]
	[PXCacheName(Messages.PMRateSequence)]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public partial class PMRateSequence : PX.Data.IBqlTable
	{
		#region RateTableID
		/// <inheritdoc cref="RateTableID"/>
		public abstract class rateTableID : PX.Data.BQL.BqlString.Field<rateTableID> { }
		/// <summary>
		/// The identifier of the <see cref="PMRateTable">rate table</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMRateTable.rateTableID"/> field.
		/// </value>
		protected String _RateTableID;
		/// <summary>
		/// The identifier of the <see cref="PMRateTable">rate table</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMRateTable.rateTableID"/> field.
		/// </value>
		[PXDefault]
		[PXDBString(PMRateTable.rateTableID.Length, IsUnicode = true, IsKey = true)]
		[PXUIField(DisplayName = "Rate Table")]
		[PXSelector(typeof(PMRateTable.rateTableID), DescriptionField = typeof(PMRateTable.description))]
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
		/// The identifier of the <see cref="PMRateTable">rate type</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMRateTable.rateTableID"/> field.
		/// </value>
		protected String _RateTypeID;
		/// <summary>
		/// The identifier of the <see cref="PMRateTable">rate type</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMRateTable.rateTableID"/> field.
		/// </value>
		[PXDefault]
		[PXDBString(PMRateType.rateTypeID.Length, IsUnicode = true, IsKey = true)]
		[PXSelector(typeof(PMRateType.rateTypeID), DescriptionField = typeof(PMRateType.description))]
		[PXUIField(DisplayName = "Rate Type")]
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
		public abstract class sequence : PX.Data.BQL.BqlInt.Field<sequence> { }
		/// <summary>
		/// The identifier of the <see cref="PMRateDefinition">sequence</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMRateDefinition.RateDefinitionID"/> field.
		/// </value>
		protected int? _Sequence;
		/// <summary>
		/// The identifier of the <see cref="PMRateDefinition">sequence</see>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMRateDefinition.RateDefinitionID"/> field.
		/// </value>
		[PXDefault]
		[PXDBInt(IsKey = true)]
		[PXUIField(DisplayName = "Sequence")]
		public virtual int? Sequence
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
		#region RateCodeID
		/// <inheritdoc cref="RateCodeID"/>
		public abstract class rateCodeID : PX.Data.BQL.BqlString.Field<rateCodeID> { }
		/// <summary>
		/// The identifier of the rate code.
		/// </summary>
		protected String _RateCodeID;
		/// <summary>
		/// The identifier of the rate code.
		/// </summary>
		[PXSelector(typeof(Search<PMRateSequence.rateCodeID, Where<PMRateSequence.rateTableID, Equal<Current<PMRateSequence.rateTableID>>, 
			And<PMRateSequence.rateTypeID, Equal<Current<PMRateSequence.rateTypeID>>, 
			And<PMRateSequence.sequence, Equal<Current<PMRateSequence.sequence>>>>>>), DescriptionField = typeof(PMRateSequence.description))]
		[PXUIField(DisplayName = "Rate Code")]
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault]
		public virtual String RateCodeID
		{
			get
			{
				return this._RateCodeID;
			}
			set
			{
				this._RateCodeID = value;
			}
		}
		#endregion
		#region Description
		/// <inheritdoc cref="Description"/>
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		/// <summary>
		/// The description of the rate code.
		/// </summary>
		protected String _Description;
		/// <summary>
		/// The description of the rate code.
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

		#region RateDefinitionID
		/// <inheritdoc cref="RateDefinitionID"/>
		public abstract class rateDefinitionID : PX.Data.BQL.BqlInt.Field<rateDefinitionID> { }
		/// <summary>
		/// The rate definition ID of the rate sequence.
		/// </summary>
		/// <value>
		/// The value of this field is set by the <see cref="PMRateDefinition.rateDefinitionID"/> DAC
		/// </value> 
		protected int? _RateDefinitionID;
		/// <summary>
		/// The rate definition ID of the rate sequence.
		/// </summary>
		/// <value>
		/// The value of this field is defined by <see cref="PMRateDefinition.rateDefinitionID"/> of the parent DAC.
		/// </value> 
		[PXParent(typeof(Select<PMRateDefinition, Where<PMRateDefinition.rateTableID, Equal<Current<PMRateSequence.rateTableID>>,
			And<PMRateDefinition.rateTypeID, Equal<Current<PMRateSequence.rateTypeID>>,
			And<PMRateDefinition.sequence, Equal<Current<PMRateSequence.sequence>>>>>>))]
		[PXInt]
		[PXFormula(typeof(Parent<PMRateDefinition.rateDefinitionID>))]
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
		#region LineCntr
		/// <inheritdoc cref="LineCntr"/>
		public abstract class lineCntr : PX.Data.BQL.BqlInt.Field<lineCntr> { }
		protected Int32? _LineCntr;
		[PXDBInt()]
		[PXDefault(0)]
		public virtual Int32? LineCntr
		{
			get
			{
				return this._LineCntr;
			}
			set
			{
				this._LineCntr = value;
			}
		}
		#endregion

		#region System Columns
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
