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
	/// Represents a cost code.
	/// Cost codes are used to classify project revenues and costs in construction projects and
	/// can be associated with documents and document lines in which projects are referenced.
	/// The records of this type are created and edited through the Cost Codes (PM209500) form
	/// (which corresponds to the <see cref="CostCodeMaint"/> graph).
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("{CostCodeCD}({CostCodeID})-{Description}")]
	[PXCacheName(Messages.CostCode)]
	[PXPrimaryGraph(typeof(CostCodeMaint))]
	[Serializable]
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class PMCostCode : PX.Data.IBqlTable
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<PMCostCode>.By<costCodeID> 
		{
			public static PMCostCode Find(PXGraph graph, int? costCodeID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, costCodeID, options);
		}

		/// <summary>
		/// Unique Key
		/// </summary>
		public class UK : PrimaryKeyOf<PMCostCode>.By<costCodeCD>
		{
			public static PMCostCode Find(PXGraph graph, string costCodeCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, costCodeCD, options);
		}
		#endregion

		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
		protected bool? _Selected = false;

		/// <summary>
		/// Gets or sets whether the task is selected in the grid.
		/// </summary>
		[PXBool]
		[PXUnboundDefault(false)]
		[PXUIField(DisplayName = "Selected")]
		public virtual bool? Selected
		{
			get
			{
				return _Selected;
			}
			set
			{
				_Selected = value;
			}
		}
		#endregion
				
		#region CostCodeID
		public abstract class costCodeID : PX.Data.BQL.BqlInt.Field<costCodeID> { }
		protected Int32? _CostCodeID;
		/// <summary>
		/// Gets or sets unique identifier.
		/// </summary>
		[PXDBIdentity()]
		[PXSelector(typeof(PMCostCode.costCodeID))]
		[PXReferentialIntegrityCheck]
		public virtual Int32? CostCodeID
		{
			get
			{
				return this._CostCodeID;
			}
			set
			{
				this._CostCodeID = value;
			}
		}
		#endregion
		#region CostCodeCD
		public abstract class costCodeCD : PX.Data.BQL.BqlString.Field<costCodeCD>
		{
			public const string DimensionName = "COSTCODE";
		}
		protected String _CostCodeCD;
		/// <summary>
		/// Get or sets unique identifier.
		/// This is a segmented key and format is configured under segmented key maintenance screen in CS module.
		/// </summary>
		/// 
		[PXDimensionSelector(costCodeCD.DimensionName, typeof(Search<PMCostCode.costCodeCD>), typeof(PMCostCode.costCodeCD), DescriptionField = typeof(PMCostCode.description))]
		//[PXDimension(costCodeCD.DimensionName)]
		[PXDBString(IsUnicode = true, IsKey = true, InputMask = "")]
		[PXDefault()]
		[PXUIField(DisplayName = "Cost Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String CostCodeCD
		{
			get
			{
				return this._CostCodeCD;
			}
			set
			{
				this._CostCodeCD = value;
			}
		}
		#endregion


		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		protected bool? _IsDefault = false;

		/// <summary>
		/// Returns True for the Default Cost Code.
		/// </summary>
		/// 
		[PXUIField(DisplayName = "Default", Enabled = false, Visible = false)]
		[PXDBBool]
		[PXDefault(false)]
		public virtual bool? IsDefault
		{
			get
			{
				return _IsDefault;
			}
			set
			{
				_IsDefault = value;
			}
		}
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }

		/// <summary>
		/// A Boolean value that indicates (if set to <see langword="true" />) that the cost code is available for use.
		/// </summary>
		[PXUIField(DisplayName = "Active")]
		[PXDBBool]
		[PXDefault(true)]
		public virtual bool? IsActive { get; set; }
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		protected String _Description;
		/// <summary>
		/// Gets or sets description
		/// </summary>
		[PXDBLocalizableString(250, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
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

		#region IsProjectOverride
		public abstract class isProjectOverride : PX.Data.BQL.BqlBool.Field<isProjectOverride> { }
		
		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Used in Project", Visibility = PXUIVisibility.SelectorVisible)]
		public virtual bool? IsProjectOverride
		{
			get;
			set;
		}
		#endregion

		#region System Columns
		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
		protected Guid? _NoteID;
		[PXNote(DescriptionField = typeof(PMCostCode.costCodeCD))]
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
