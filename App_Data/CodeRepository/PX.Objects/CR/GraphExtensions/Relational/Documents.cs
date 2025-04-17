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
using System;

namespace PX.Objects.CR.Extensions.Relational
{
	[PXHidden]
	public abstract class Document<T> : PXMappedCacheExtension
		where T : PXGraphExtension
	{
		#region RelatedID
		public abstract class relatedID : PX.Data.BQL.BqlInt.Field<relatedID> { }
		public virtual int? RelatedID { get; set; }
		#endregion

		#region ChildID
		public abstract class childID : PX.Data.BQL.BqlInt.Field<childID> { }
		public virtual int? ChildID { get; set; }
		#endregion

		#region IsOverrideRelated
		public abstract class isOverrideRelated : PX.Data.BQL.BqlBool.Field<isOverrideRelated> { }
		public virtual bool? IsOverrideRelated { get; set; }
		#endregion
	}

	[PXHidden]
	public abstract class Related<T> : PXMappedCacheExtension
		where T : PXGraphExtension
	{
		#region RelatedID
		public abstract class relatedID : PX.Data.BQL.BqlInt.Field<relatedID> { }
		public virtual int? RelatedID { get; set; }
		#endregion

		#region ChildID
		public abstract class childID : PX.Data.BQL.BqlInt.Field<childID> { }
		public virtual int? ChildID { get; set; }
		#endregion
	}

	[PXHidden]
	public abstract class Child<T> : PXMappedCacheExtension
		where T : PXGraphExtension
	{
		#region ChildID
		public abstract class childID : PX.Data.BQL.BqlInt.Field<childID> { }
		public virtual int? ChildID { get; set; }
		#endregion

		#region RelatedID
		public abstract class relatedID : PX.Data.BQL.BqlInt.Field<relatedID> { }
		public virtual int? RelatedID { get; set; }
		#endregion
	}
}
