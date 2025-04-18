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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using PX.Common;
using PX.Data;
using PX.Data.EP;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Data.SQLTree;

namespace SP.Objects.CR
{
	public sealed class MatchWithBAccount<Field, Parameter> : MatchWithBAccountBase, IBqlUnary, IBqlPortalRestrictor
        where Field : IBqlOperand
        where Parameter : IBqlParameter, new()
    {
        IBqlParameter _parameter;
		private IBqlCreator _operand;

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
        {
            result = true;
        }

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection) 
		{
			bool status = true;

			if (graph != null)
			{
				if (_parameter == null) _parameter = new Parameter();

				object val = null;
				if (_parameter.HasDefault) 
				{
					Type ft = _parameter.GetReferencedType();
					if (ft.IsNested) 
					{
						Type ct = BqlCommand.GetItemType(ft);
						PXCache cache = graph.Caches[ct];
						if (cache.Current != null) 
							val = cache.GetValue(cache.Current, ft.Name);
					}
				}

				SQLExpression fieldExpression = null;
				if (!typeof(IBqlCreator).IsAssignableFrom(typeof(Field))) 
				{
					fieldExpression = SPCommand.GetSingleField(typeof(Field), graph, info.Tables, PXDBOperation.Select);
				}
				else 
				{
					if (_operand == null) _operand = Activator.CreateInstance<Field>() as IBqlCreator;
					if (_operand == null) 
						throw new PXArgumentException("Field", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);

					status &= _operand.AppendExpression(ref fieldExpression, graph, info, selection);
				}
				exp = (fieldExpression ?? SQLExpression.None()).IsNull();

				List<int> baccounts = val != null ? GetBaccounts(graph) : null;

				if (PXContext.PXIdentity.User.IsInRole(PXAccess.GetAdministratorRole())) {
					exp = exp.Or(new SQLConst(1).EQ(1));
				}
				else if (baccounts != null && baccounts.Count > 0) {
					SQLExpression left = null;
					if (!typeof(IBqlCreator).IsAssignableFrom(typeof(Field))) {
						left = SPCommand.GetSingleField(typeof(Field), graph, info.Tables, PXDBOperation.Select);
					}
					else {
						if (_operand == null) _operand = Activator.CreateInstance<Field>() as IBqlCreator;
						if (_operand == null) {
							throw new PXArgumentException("Field", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
						}
						status &= _operand.AppendExpression(ref left, graph, info, selection);
					}

					var seq = SQLExpression.None();
					for (int i = 0; i < baccounts.Count; i++) {
						seq=seq.Seq(baccounts[i]);
					}
					var ins = (left ?? SQLExpression.None()).In(seq);

					exp = exp.Or(ins);
				}
			}
			else if (typeof(IBqlCreator).IsAssignableFrom(typeof(Field)))
			{
				if (_operand == null) _operand = Activator.CreateInstance<Field>() as IBqlCreator;
				if (_operand == null)
				{
					throw new PXArgumentException("Field", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
				}
				SQLExpression left = null;
				status &= _operand.AppendExpression(ref left, graph, info, selection);
				exp = (left ?? SQLExpression.None()).IsNull();
			}

			return status;
		}
	}

	public class MatchWithBAccountBase
	{
		private const string _TYPEID = "Warning";

		protected static List<int> GetBaccounts(PXGraph graph)
		{

			var userID = graph?.Accessinfo?.UserID;

			if (userID == null)
				return Enumerable.Empty<int>().ToList();

			var res = PortalBAccountVisibilityProvider.Definition.GetVisibleBAccountIds((Guid)userID).ToList();

			if (res.Count == 0 && HttpContext.Current != null)
			{
				if (PXGraph.ProxyIsActive)
				{
					return new List<int> { -1 };
				}

				var url = string.Format("~/Frames/Error.aspx?exceptionID={0}&typeID={1}&errorcode={2}&HideScript=On",
					HttpUtility.UrlEncode(PXMessages.LocalizeNoPrefix(Objects.Messages.GetBaccountsErrorMessage)),
					HttpUtility.UrlEncode(_TYPEID),
					HttpUtility.UrlEncode(PXMessages.LocalizeNoPrefix(Objects.Messages.ConfigurationError)));

				var page = HttpContext.Current.CurrentHandler as Page;

				if (page != null && page.IsCallback)
					throw new PXRedirectToUrlException(url, "");
				else
					HttpContext.Current.Response.Redirect(url);
			}

			return res;
		}


		public static bool IsParentChild(int? parent, int? child)
		{
			return parent is { } parentId
				&& (parentId == child
				|| PortalBAccountVisibilityProvider
					.Definition
					.GetChildrenBAccountsIds(parentId, onlyDirectChildren: false)
					.Any(childId => childId == child));
		}
	}

	public sealed class MatchWithBAccountNotNull<Field> : MatchWithBAccountBase, IBqlUnary, IBqlPortalRestrictor
		where Field : IBqlOperand
	{
		private IBqlCreator _operand;

		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			List<int> baccounts = GetBaccounts(cache?.Graph);

			if (System.Web.Security.Roles.IsUserInRole(PXAccess.GetUserName(), PXAccess.GetAdministratorRole()))
			{
				result = true;
			}
			else if (baccounts != null && baccounts.Count > 0)
			{
				result =
					value == null || value is int intValue && GetBaccounts(cache?.Graph).Contains(intValue);
			}
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			bool status = true;

			if (graph == null)
				return status;

			List<int> baccounts = GetBaccounts(graph);

			if (PXContext.PXIdentity.User.IsInRole(PXAccess.GetAdministratorRole()))
			{
				exp = new SQLConst(1).EQ(1);
			}
			else if (baccounts != null && baccounts.Count > 0)
			{
				SQLExpression left = null;

				if (!typeof(IBqlCreator).IsAssignableFrom(typeof(Field)))
				{
					left = SPCommand.GetSingleField(typeof(Field), graph, info.Tables, PXDBOperation.Select);
				}
				else
				{
					if (_operand == null) _operand = Activator.CreateInstance<Field>() as IBqlCreator;
					if (_operand == null)
					{
						throw new PXArgumentException("Field", ErrorMessages.OperandNotClassFieldAndNotIBqlCreator);
					}
					status &= _operand.AppendExpression(ref left, graph, info, selection);
				}
				
				var seq = SQLExpression.None();
				for (int i = 0; i < baccounts.Count; i++)
				{
					seq = seq.Seq(baccounts[i]);
				}

				exp = (left ?? SQLExpression.None()).In(seq);
			}

			return status;
		}
	}

	public abstract class SPCommand : BqlCommand
    {
		internal static SQLExpression GetSingleField(Type field, PXGraph graph, List<Type> tables, PXDBOperation operation) {
			Type table0 = PX.Data.BqlCommand.GetItemType(field);

			PXCache cache = graph.Caches[table0];
			PXCommandPreparingEventArgs.FieldDescription description;
			Type table = table0;
			if (tables != null && tables.Count > 0) {
				if (tables[0].IsSubclassOf(table0)) {
					table = tables[0];
				}
				else if (!typeof(IBqlTable).IsAssignableFrom(table)) {
					Type cust = table;
					table = null;
					for (int i = 0; i < tables.Count; i++) {
						if (cust.IsAssignableFrom(tables[i])
							&& (table == null
								|| tables[i].IsAssignableFrom(table))) {
							table = tables[i];
						}
					}
					table = table ?? cust;
				}
			}
			cache.RaiseCommandPreparing(field.Name, null, null, operation, table, out description);
			return description.Expr;
		}
    }
}
