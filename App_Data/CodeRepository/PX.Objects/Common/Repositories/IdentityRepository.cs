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

namespace PX.Objects.Common.Repositories
{
	/// <summary>
	/// Repository providing basic methods for tables that have a unique ID field.
	/// </summary>
	public class IdentityRepository<TNode, TIdentityField> : RepositoryBase<TNode>
		where TNode : class, IBqlTable, new()
		where TIdentityField : IBqlField
	{
		public IdentityRepository(PXGraph graph) : base(graph)
		{ }

		public TNode FindByID(object id) => SelectSingle<Where<TIdentityField, Equal<Required<TIdentityField>>>>(id);

		public TNode GetByID(object id) => ForceNotNull(FindByID(id));
	}
}
