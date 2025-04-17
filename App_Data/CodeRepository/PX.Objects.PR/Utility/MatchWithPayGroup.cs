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

using PX.Common;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Data.SQLTree;
using PX.SM;
using System.Collections.Generic;
using System.Linq;

namespace PX.Objects.PR
{
	public sealed class MatchWithPayGroup<Field> : Data.BQL.BqlChainableConditionLite<MatchWithPayGroup<Field>>, IBqlUnary
			where Field : IBqlOperand
	{
		/// <exclude/>
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			string payGroupID;
			if (!typeof(IBqlField).IsAssignableFrom(typeof(Field)))
			{
				throw new PXArgumentException("Operand", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
			}

			if (cache.GetItemType() == typeof(Field).DeclaringType ||
				typeof(Field).DeclaringType.IsAssignableFrom(cache.GetItemType()))
			{
				payGroupID = (string) cache.GetValue(item, typeof(Field).Name);
			}
			else
			{
				payGroupID = null;
			}

			if (payGroupID == null)
			{
				result = true;
				return;
			}

			string[] userPayGroupIDs = MatchWithPayGroupHelper.GetUserPayGroupIDs(cache.Graph);

			if (userPayGroupIDs != null && userPayGroupIDs.Length > 0)
			{
				result = userPayGroupIDs.Any(userPayGroupID => userPayGroupID == payGroupID);
			}
			else
			{
				result = false;
			}
		}

		/// <exclude />
		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			bool status = true;

			if (graph == null || !info.BuildExpression)
			{
				return status;
			}

			if (!typeof(IBqlField).IsAssignableFrom(typeof(Field)))
			{
				exp = new SQLConst(1).EQ(1);
				return status;
			}

			SQLExpression fieldExpression = BqlCommand.GetSingleExpression(typeof(Field), graph, info.Tables, selection, BqlCommand.FieldPlace.Condition);
			exp = fieldExpression.IsNull();

			string[] userPayGroupIDs = MatchWithPayGroupHelper.GetUserPayGroupIDs(graph);
			if (userPayGroupIDs.Length > 0)
			{
				SQLExpression innerExpression = null;
				foreach (string userPayGroupID in userPayGroupIDs)
				{
					innerExpression = innerExpression == null ? new SQLConst(userPayGroupID) : innerExpression.Seq(userPayGroupID);
				}
				exp = exp.Or(fieldExpression.In(innerExpression)).Embrace();
			}
			return status;
		}
	}

	public class MatchWithPayGroupHelper
	{
		private const string UserPayGroupIDsKey = "UserPayGroupIDs";

		public static string[] GetUserPayGroupIDs(PXGraph graph)
		{
			Dictionary<string, string[]> cachedPayGroupIds = PXContext.GetSlot<Dictionary<string, string[]>>(UserPayGroupIDsKey) ?? new Dictionary<string, string[]>();

			string userName = graph.Accessinfo.UserName;

			if (!cachedPayGroupIds.TryGetValue(userName, out string[] userPayGroupIDs))
			{
				if (graph is PRPayStubInq || graph.Accessinfo.ScreenID == "PR.64.10.00")
				{
					userPayGroupIDs = SelectFrom<PRPayGroup>
					.LeftJoin<UsersInRoles>.On<PRPayGroup.roleName.IsEqual<UsersInRoles.rolename>>
					.Where<MatchUser>.View
					.Select(graph, userName).FirstTableItems.Select(item => item.PayGroupID).ToArray();
				}
				else
				{
					userPayGroupIDs = SelectFrom<PRPayGroup>
					.LeftJoin<UsersInRoles>.On<PRPayGroup.roleName.IsEqual<UsersInRoles.rolename>>
					.Where<PRPayGroup.roleName.IsNull.Or<UsersInRoles.username.IsEqual<P.AsString>>>.View
					.Select(graph, userName).FirstTableItems.Select(item => item.PayGroupID).ToArray();
				}

				cachedPayGroupIds[userName] = userPayGroupIDs;
				PXContext.SetSlot(UserPayGroupIDsKey, cachedPayGroupIds);
			}

			return userPayGroupIDs;
		}

		public static void ClearUserPayGroupIDsSlot()
		{
			PXContext.SetSlot(UserPayGroupIDsKey, null);
		}
	}
}
