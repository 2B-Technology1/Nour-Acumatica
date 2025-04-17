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
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Data.SQLTree;
using PX.TM;

namespace PX.Objects.EP
{
	public class Approver<ContactID> : IBqlComparison
		where ContactID : IBqlOperand, new()
	{
		private IBqlCreator _operand;

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			result = null;
			value = null;
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			bool status = true;

			SQLExpression contactID1 = null;
			status &= BqlCommand.AppendExpression<ContactID>(ref contactID1, graph, info, selection, ref _operand);
			SQLExpression contactID2 = null;
			status &= BqlCommand.AppendExpression<ContactID>(ref contactID2, graph, info, selection, ref _operand);

			if (graph == null || !info.BuildExpression) return status;
			
			exp = exp.In(new Query()
					.Select<EPApproval.refNoteID>()
					.From<EPApproval>()
					.Where(typeof(EPApproval.ownerID).EQ(contactID1)
						.Or(typeof(EPApproval.workgroupID)
							.In(new Query()
								.Select<EPCompanyTreeH.workGroupID>()
								.From<EPCompanyTreeH>()
								.Join<EPCompanyTreeMember>()
									.On(typeof(EPCompanyTreeH.parentWGID).EQ(typeof(EPCompanyTreeMember.workGroupID))
										.And(typeof(EPCompanyTreeH.parentWGID).NE(typeof(EPCompanyTreeH.workGroupID)))
										.And(typeof(EPCompanyTreeMember.active).EQ(true))
										.And(typeof(EPCompanyTreeMember.contactID).EQ(contactID2)))
									.Where(new SQLConst(1)
										.EQ(new SQLConst(1)))))));
			return status;
		}
	}

}
