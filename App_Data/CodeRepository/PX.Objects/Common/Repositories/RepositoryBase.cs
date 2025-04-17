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

using PX.Data;

namespace PX.Objects.Common
{
	/// <summary>
	/// A base class for repository objects capable of retrieving entities 
	/// from the database according to different restrictions. Optionally, 
	/// uses a <see cref="PXGraph"/> object to leverage Acumatica platform's 
	/// caching mechanisms.
	/// </summary>
	/// <typeparam name="TNode">The type of objects retrieved from the database.</typeparam>
	public class RepositoryBase<TNode>
		where TNode: class, IBqlTable, new()
	{
		protected readonly PXGraph _graph;
		protected string _itemName;

		public RepositoryBase(PXGraph graph)
		{
			_graph = graph ?? new PXGraph();
			_itemName = PXUIFieldAttribute.GetItemName(_graph.Caches[typeof(TNode)]);
		}

		protected TNode ForceNotNull(TNode record, string valueDescription = null)
		{
			if (record == null)
			{
				if (valueDescription == null)
				{
					throw new PXException(ErrorMessages.ElementDoesntExist, _itemName);
				}
				else
				{
					throw new PXException(ErrorMessages.ValueDoesntExist, _itemName, valueDescription);
				}
			}

			return record;
		}

		public PXResultset<TNode> Select<Where>(params object[] parameters)
			where Where : IBqlWhere, new()
			=> PXSelect<TNode, Where>.Select(_graph, parameters);

		public TNode SelectSingle<Where>(params object[] parameters)
			where Where : IBqlWhere, new()
			=> PXSelect<TNode, Where>.SelectWindowed(_graph, 0, 1, parameters);

		public TNode SelectSingle<Where, OrderBy>(params object[] parameters)
			where Where : IBqlWhere, new()
			where OrderBy : IBqlOrderBy, new()
			=> PXSelect<TNode, Where, OrderBy>.SelectWindowed(_graph, 0, 1, parameters);
	}
}
