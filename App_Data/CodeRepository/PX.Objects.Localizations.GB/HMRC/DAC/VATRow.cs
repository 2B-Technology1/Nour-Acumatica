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

namespace PX.Objects.Localizations.GB.HMRC.DAC
{
	[Serializable()]
	[PXHidden]
	public class VATRow : IBqlTable
	{
		#region taxBoxNbr
		public abstract class taxBoxNbr : PX.Data.BQL.BqlString.Field<taxBoxNbr> { }

		[PXString(12, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = Messages.TaxBoxNumber, Visibility = PXUIVisibility.Visible)]
		public virtual String TaxBoxNbr { get; set; }
		#endregion
		#region taxBoxCode
		public abstract class taxBoxCode : PX.Data.BQL.BqlString.Field<taxBoxCode> { }

		[PXString(15, IsUnicode = true)]
		[PXUIField(DisplayName = Messages.TaxBoxCode, Visibility = PXUIVisibility.Visible)]
		public virtual String TaxBoxCode { get; set; }
		#endregion
		#region Descr
		public abstract class descr : PX.Data.BQL.BqlString.Field<descr> { }

		[PXDBString(128, IsUnicode = true)]
		[PXUIField(DisplayName = Messages.Description, Visibility = PXUIVisibility.SelectorVisible)]
		public virtual String Descr { get; set; }
		#endregion
		#region Amt
		public abstract class amt : PX.Data.BQL.BqlDecimal.Field<amt> { }

		[PXDecimal(2)]
		[PXDefault(TypeCode.Decimal, "0.0", PersistingCheck = PXPersistingCheck.Nothing)]
		[PXUIField(DisplayName = Messages.Amount, Visibility = PXUIVisibility.Visible, Enabled = false)]
		public virtual Decimal? Amt { get; set; }
		#endregion
	}
}
