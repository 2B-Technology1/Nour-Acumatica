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

namespace PX.Objects.CR.Extensions.CRCreateActions
{
	/// <exclude/>
	[PXHidden]
	public partial class DocumentContactMethod : PXMappedCacheExtension
	{
		#region Method
		public abstract class method : PX.Data.BQL.BqlString.Field<method> { }
		public virtual String Method { get; set; }
		#endregion

		#region NoFax
		public abstract class noFax : PX.Data.BQL.BqlBool.Field<noFax> { }
		public virtual bool? NoFax { get; set; }
		#endregion

		#region NoMail
		public abstract class noMail : PX.Data.BQL.BqlBool.Field<noMail> { }
		public virtual bool? NoMail { get; set; }
		#endregion

		#region NoMarketing
		public abstract class noMarketing : PX.Data.BQL.BqlBool.Field<noMarketing> { }
		public virtual bool? NoMarketing { get; set; }
		#endregion

		#region NoCall
		public abstract class noCall : PX.Data.BQL.BqlBool.Field<noCall> { }
		public virtual bool? NoCall { get; set; }
		#endregion

		#region NoEMail
		public abstract class noEMail : PX.Data.BQL.BqlBool.Field<noEMail> { }
		public virtual bool? NoEMail { get; set; }
		#endregion

		#region NoMassMail
		public abstract class noMassMail : PX.Data.BQL.BqlBool.Field<noMassMail> { }
		public virtual bool? NoMassMail { get; set; }
		#endregion
	}
}
