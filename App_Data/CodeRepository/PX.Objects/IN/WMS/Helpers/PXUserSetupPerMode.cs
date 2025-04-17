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
using PX.Objects.Extensions;

namespace PX.Objects.IN
{
	public class PXUserSetupPerMode<TSelf, TGraph, THeader, TSetup, TUserIDField, TModeField, TModeValueField>
		: PXSetupBase<TSelf, TGraph, THeader, TSetup,
			Where<TUserIDField, Equal<Current<AccessInfo.userID>>, And<TModeField, Equal<Current<TModeField>>>>>
		where TSelf : PXUserSetupPerMode<TSelf, TGraph, THeader, TSetup, TUserIDField, TModeField, TModeValueField>
		where TGraph : PXGraph
		where THeader : class, IBqlTable, new()
		where TSetup : class, IBqlTable, new()
		where TUserIDField : IBqlField
		where TModeField : class, IBqlField
		where TModeValueField : IConstant, IBqlOperand, new()
	{
		public virtual void _(Events.FieldDefaulting<TSetup, TModeField> e) => e.NewValue = new TModeValueField().Value;
	}
}
