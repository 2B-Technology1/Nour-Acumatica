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
using PX.Data.WorkflowAPI;

namespace PX.Objects.Common
{
	/// <summary>
	/// Commonly used menu action categories.
	/// </summary>
	public static class CommonActionCategories
	{
		public const string ProcessingCategoryID = "Processing Category";
		public const string ApprovalCategoryID = "Approval Category";
		public const string PrintingAndEmailingCategoryID = "Printing and Emailing Category";
		public const string IntercompanyCategoryID = "Intercompany Category";
		public const string OtherCategoryID = "Other Category";

		[PXLocalizable]
		public static class DisplayNames
		{
			public const string Processing = "Processing";
			public const string Approval = "Approval";
			public const string PrintingAndEmailing = "Printing and Emailing";
			public const string Intercompany = "Intercompany";
			public const string Other = "Other";
		}

		public static Categories<TGraph, TPrimary> Get<TGraph, TPrimary>(WorkflowContext<TGraph, TPrimary> context)
			where TGraph : PXGraph
			where TPrimary : class, IBqlTable, new()
			=> new Categories<TGraph, TPrimary>(context);

		public struct Categories<TGraph, TPrimary>
			where TGraph : PXGraph
			where TPrimary : class, IBqlTable, new()
		{
			private readonly WorkflowContext<TGraph, TPrimary> _context;
			public Categories(WorkflowContext<TGraph, TPrimary> context) => _context = context;

			public BoundedTo<TGraph, TPrimary>.ActionCategory.IConfigured Processing =>
				_context.Categories.Get(ProcessingCategoryID) ??
				_context.Categories.CreateNew(ProcessingCategoryID, c => c.DisplayName(DisplayNames.Processing));

			public BoundedTo<TGraph, TPrimary>.ActionCategory.IConfigured Approval =>
				_context.Categories.Get(ApprovalCategoryID) ??
				_context.Categories.CreateNew(ApprovalCategoryID, c => c.DisplayName(DisplayNames.Approval));

			public BoundedTo<TGraph, TPrimary>.ActionCategory.IConfigured PrintingAndEmailing =>
				_context.Categories.Get(PrintingAndEmailingCategoryID) ??
				_context.Categories.CreateNew(PrintingAndEmailingCategoryID, c => c.DisplayName(DisplayNames.PrintingAndEmailing));

			public BoundedTo<TGraph, TPrimary>.ActionCategory.IConfigured Intercompany =>
				_context.Categories.Get(IntercompanyCategoryID) ??
				_context.Categories.CreateNew(IntercompanyCategoryID, c => c.DisplayName(DisplayNames.Intercompany));

			public BoundedTo<TGraph, TPrimary>.ActionCategory.IConfigured Other =>
				_context.Categories.Get(OtherCategoryID) ??
				_context.Categories.CreateNew(OtherCategoryID, c => c.DisplayName(DisplayNames.Other));
		}
	}
}
