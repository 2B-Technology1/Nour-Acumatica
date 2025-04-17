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
using System.Linq;
using System.Text;

using PX.Data;
using PX.Data.SQLTree;
using PX.Objects.CS;

namespace PX.Objects.GL.BQL
{
	/// <summary>
	/// A BQL predicate returning <c>true</c> if and only if there exists a 
	/// <see cref="GLVoucher">journal voucher</see> referencing the entity's
	/// note ID field by its <see cref="GLVoucher.RefNoteID"/> field.
	/// </summary>
	public class ExistsJournalVoucher<TNoteIDField> : IBqlUnary
		where TNoteIDField : IBqlField
	{
		private readonly IBqlCreator exists = new Exists<Select<
			GLVoucher,
			Where2<
				FeatureInstalled<FeaturesSet.gLWorkBooks>,
				And<TNoteIDField, Equal<GLVoucher.refNoteID>>>>>();

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
			=> exists.AppendExpression(ref exp, graph, info, selection);
	
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			Guid? noteID = cache.GetValue<TNoteIDField>(item) as Guid?;

			value = result = PXSelect<
				GLVoucher,
				Where2<
					FeatureInstalled<FeaturesSet.gLWorkBooks>,
					And<GLVoucher.refNoteID, Equal<Required<GLVoucher.refNoteID>>>>>
				.SelectWindowed(cache.Graph, 0, 1, noteID)
				.RowCast<GLVoucher>()
				.Any();
		}
	}
}
