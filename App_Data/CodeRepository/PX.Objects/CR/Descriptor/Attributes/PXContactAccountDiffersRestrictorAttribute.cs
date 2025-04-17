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
using System.Linq;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.IN;
using PX.Objects.IN.Attributes;

namespace PX.Objects.CR
{
	public class PXContactAccountDiffersRestrictorAttribute : RestrictorWithParametersAttribute
	{
		public PXContactAccountDiffersRestrictorAttribute(Type where, params Type[] messageParameters)
			: base(where, "", messageParameters) { }

		public override object[] GetMessageParameters(PXCache sender, object itemres, object row)
		{
			var contact = PXResult.Unwrap<Contact>(itemres);

			if (contact != null && contact.ContactType == ContactTypesAttribute.Employee)
			{
				_Message = Messages.ContactBAccountDifferForEmployee;
			}
			else
			{
				_Message = Messages.ContractBAccountDiffer;
			}

			return base.GetMessageParameters(sender, itemres, row);
		}
	}
}
