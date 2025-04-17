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
using System.Web.Compilation;
using PX.Data;

namespace PX.Objects.EP
{
	/// <summary>
	/// Formula that defines a UI-friendly type of the entity to be approved.
	/// Uses the detailed record-level type (if it is not empty), otherwise,
	/// uses the friendly name of the source item's cache.
	/// </summary>
	/// <typeparam name="EntityTypeName">
	/// Field containing the type name of the source item, e.g.
	/// <see cref="EPApprovalProcess.EPOwned.entityType"/>. From
	/// this type name, the friendly cache name will be deduced if
	/// the record-level entity type field returns <c>null</c>.
	/// </typeparam>
	/// <typeparam name="SourceItemType">
	/// Field containing the detailed, record-level source item type, e.g.
	/// <see cref="EPApproval.sourceItemType"/>. If this field does not
	/// contain <c>null</c>, its value will be returned by this formula.
	/// </typeparam>
	public class ApprovalDocType<EntityTypeName, SourceItemType> : BqlFormulaEvaluator<EntityTypeName, SourceItemType>
		where EntityTypeName : IBqlOperand
		where SourceItemType : IBqlOperand
	{
		public override object Evaluate(PXCache cache, object item, Dictionary<Type, object> pars)
		{
			string sourceItemType = (string)pars[typeof(SourceItemType)];

			if (!string.IsNullOrEmpty(sourceItemType))
			{
				return PXMessages.LocalizeNoPrefix(sourceItemType);
			}

			string entityType = (string)pars[typeof(EntityTypeName)];
			if (!string.IsNullOrEmpty(entityType))
			{
				return PXMessages.LocalizeNoPrefix(EntityHelper.GetFriendlyEntityName(PXBuildManager.GetType(entityType, false, true)));
			}
			return null;
		}
	}
}
