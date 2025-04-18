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

namespace PX.Objects.RUTROT
{
	[Serializable]
	[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2021R2)]
	public class RUTROTWorkType : IBqlTable
	{
		#region RUTROTType
		public abstract class rUTROTType : PX.Data.BQL.BqlString.Field<rUTROTType> { }

		/// <summary>
		/// The type of deduction for a ROT and RUT deductible document.
		/// </summary>
		/// <value>
		/// Allowed values are:
		/// <c>"U"</c> - RUT,
		/// <c>"O"</c> - ROT.
		/// Defaults to RUT (<c>"U"</c>).
		/// </value>
		[PXDBString(1, IsKey = true)]
		[RUTROTTypes.List]
		[PXDefault(RUTROTTypes.RUT)]
		[PXUIField(DisplayName = "Deduction Type", Enabled = true, Visible = true, FieldClass = RUTROTMessages.FieldClass)]
		public virtual string RUTROTType
		{
			get;
			set;
		}
		#endregion
		#region WorkTypeID
		public abstract class workTypeID : PX.Data.BQL.BqlInt.Field<workTypeID> { }

		/// <summary>
		/// Database identity.
		/// The unique identifier of the RUTROTWorkType.
		/// </summary>
		[PXDBIdentity]
		[PXUIField(DisplayName = "Work Type ID", Visibility = PXUIVisibility.Invisible, Visible = false, FieldClass = RUTROTMessages.FieldClass)]
		public virtual int? WorkTypeID
		{
			get;
			set;
		}
		#endregion
		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }

		/// <summary>
		/// The user-friendly description of the Work Type.
		/// </summary>
		[PXDBString(255, IsUnicode = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Title", Enabled = true, Visible = true, Visibility = PXUIVisibility.Visible, FieldClass = RUTROTMessages.FieldClass)]
		[PX.Data.EP.PXFieldDescription]
		public virtual string Description
		{
			get;
			set;
		}
		#endregion
		#region XMLTag
		public abstract class xmlTag : PX.Data.BQL.BqlString.Field<xmlTag> { }

		/// <summary>
		/// The XML tag that should be included in the exported XML file.
		/// </summary>
		[PXDBString(50, IsUnicode = true, IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "XML Tag", Enabled = true, Visible = true, Visibility = PXUIVisibility.Visible, FieldClass = RUTROTMessages.FieldClass)]
		public virtual string XMLTag
		{
			get;
			set;
		}
		#endregion

		#region StartDate
		public abstract class startDate : PX.Data.BQL.BqlDateTime.Field<startDate> { }
		/// <summary>
		/// A work type is eligible starting from this date.
		/// </summary>
		/// <value>
		/// The value of this field cannot be null.
		/// </value>
		[PXDBDate]
		[PXDefault(typeof(AccessInfo.businessDate))]
		[PXUIField(DisplayName = "Start Date", Enabled = true, Visible = true, Visibility = PXUIVisibility.Visible, FieldClass = RUTROTMessages.FieldClass)]
		public virtual DateTime? StartDate
		{
			get;
			set;
		}
		#endregion
		#region EndDate
		public abstract class endDate : PX.Data.BQL.BqlDateTime.Field<endDate> { }
		/// <summary>
		/// The date a work type is eligible up to.
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "End Date", Enabled = true, Visible = true, Visibility = PXUIVisibility.Visible, FieldClass = RUTROTMessages.FieldClass)]
		public virtual DateTime? EndDate
		{
			get;
			set;
		}
		#endregion
		#region Position
		public abstract class position : PX.Data.BQL.BqlInt.Field<position> { }

		/// <summary>
		/// The current position of the work type among other work types.
		/// The list of work types is ordered by the value of this field on the 
		/// Work Types (AR203000) form and in the generated XML file.
		/// </summary>
		/// <value>
		/// The value of this field is auto-generated by the <see cref="RUTROTWorkTypesMaint"/> graph 
		/// and is unique among all work types with the same <see cref="RUTROTType"/>.
		/// </value>
		[PXDBInt]
		[PXCheckUnique(IgnoreNulls = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Position", Visibility = PXUIVisibility.Invisible, Visible = false, FieldClass = RUTROTMessages.FieldClass)]
		public virtual int? Position
		{
			get;
			set;
		}
		#endregion

		#region NoteID
		public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }

		/// <summary>
		/// Identifier of the <see cref="PX.Data.Note">Note</see> object, associated with the item.
		/// </summary>
		/// <value>
		/// Corresponds to the <see cref="PX.Data.Note.NoteID">Note.NoteID</see> field. 
		/// </value>
		[PXNote]
		public virtual Guid? NoteID
		{
			get;
			set;
		}
		#endregion

		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get;
			set;
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public virtual string CreatedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public virtual DateTime? CreatedDateTime
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public virtual string LastModifiedByScreenID
		{
			get;
			set;
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public virtual DateTime? LastModifiedDateTime
		{
			get;
			set;
		}
		#endregion
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp]
		public virtual byte[] tstamp
		{
			get;
			set;
		}
		#endregion
	}
}
