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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PX.Objects.CA
{
	[PXHidden]
	public class PlugInFilter : IBqlTable
	{
		#region PaymentMethodID
		public abstract class paymentMethodID : PX.Data.BQL.BqlString.Field<paymentMethodID> { }

		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDefault(typeof(PaymentMethod.paymentMethodID))]
		public virtual string PaymentMethodID
		{
			get;
			set;
		}
		#endregion
		#region PlugInTypeName
		public abstract class plugInTypeName : PX.Data.BQL.BqlGuid.Field<plugInTypeName> { }

		[PXDBString(255, IsKey = true)]
		[PXDefault(typeof(PaymentMethod.aPBatchExportPlugInTypeName))]
		public virtual string PlugInTypeName
		{
			get;
			set;
		}
		#endregion
		#region ShowAllSettings
		public abstract class showAllSettings : PX.Data.BQL.BqlBool.Field<showAllSettings> { }

		[PXDBBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Show All Settings")]
		public virtual bool? ShowAllSettings
		{
			get;
			set;
		}
		#endregion
		#region ShowOffsetSettings
		public abstract class showOffsetSettings : PX.Data.BQL.BqlBool.Field<showOffsetSettings> { }

		[PXBool]
		[PXDefault(false, PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = "Show Offset Settings", Visible = false)]
		public virtual bool? ShowOffsetSettings
		{
			get;
			set;
		}
		#endregion
	}
}
