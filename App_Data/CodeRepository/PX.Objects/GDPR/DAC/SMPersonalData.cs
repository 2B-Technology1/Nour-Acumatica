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

namespace PX.Objects.GDPR
{
	[Serializable]
	[PXHidden]
	public class SMPersonalData : IBqlTable
	{
		#region Selected
		public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Selected", Visibility = PXUIVisibility.Service)]
		public virtual bool? Selected { get; set; }
		#endregion

		#region Table
		public abstract class table : PX.Data.BQL.BqlString.Field<table> { }

		[PXDBString(100, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Table name")]
		public virtual String Table { get; set; }
		#endregion

		#region Field
		public abstract class field : PX.Data.BQL.BqlString.Field<field> { }

		[PXDBString(100, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Field name")]
		public virtual String Field { get; set; }
		#endregion

		#region EntityID
		public abstract class entityID : PX.Data.BQL.BqlGuid.Field<entityID> { }

		[PXDBGuid(IsKey = true)]
		public virtual Guid? EntityID { get; set; }
		#endregion

		#region TopParentNoteID
		public abstract class topParentNoteID : PX.Data.BQL.BqlGuid.Field<topParentNoteID> { }

		[PXDBGuid]
		public virtual Guid? TopParentNoteID { get; set; }
		#endregion

		#region Value
		public abstract class value : PX.Data.BQL.BqlString.Field<value> { }

		[PXDBString(100, IsUnicode = true)]
		[PXUIField(DisplayName = "Value")]
		public virtual String Value { get; set; }
		#endregion

		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }

		[PXDBCreatedDateTime]
		[PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
	}
}