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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.BQL;
using PX.Data.BQL.Fluent;
using PX.Objects.CR;
using SP.Objects.CR;

namespace SP.Objects.SP
{
	/// <exclude/>
	public class SPCaseStatusInquiry : PXGraph<SPCaseStatusInquiry>
	{
		[Serializable]
		[PXHidden]
		public class CaseStatusFilter : IBqlTable
		{
			#region Status
			public abstract class status : BqlString.Field<status> { }
			[PXString]
			[PXStringList(new string[0], new string[0], BqlField = typeof(CRCase.status))]
			[PXUIField(DisplayName = "Status", Required = true, Visibility = PXUIVisibility.SelectorVisible)]
			public virtual string Status { get; set; }
			#endregion
		}

		public PXFilter<CaseStatusFilter> Filter;

		[PXFilterable]
		public
			SelectFrom<CRCase>
			.LeftJoin<CRCaseClass>.On<CRCaseClass.caseClassID.IsEqual<CRCase.caseClassID>>
			.Where<CRCase.status.IsEqual<CaseStatusFilter.status.FromCurrent>
				.And<MatchWithBAccountNotNull<CRCase.customerID>>
				.And<Brackets<CRCaseClass.isInternal.IsEqual<False>.Or<CRCaseClass.isInternal.IsNull>>>>
			.View.ReadOnly
			FilteredItems;

		#region Actions

		public PXAction<CaseStatusFilter> ViewCase;
		[PXUIField(DisplayName = "View Case", Visible = false)]
		[PXButton]
		public virtual IEnumerable viewCase(PXAdapter adapter)
		{
			if (FilteredItems.Current != null)
			{
				CRCaseMaint graph = CreateInstance<CRCaseMaint>();
				PXResult result = graph.Case.Search<CRCase.caseCD>(FilteredItems.Current.CaseCD);
				CRCase @case = result[typeof(CRCase)] as CRCase;
				graph.Case.Current = @case;
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.Same);
			}
			return adapter.Get();
		}
		#endregion
	}
}
