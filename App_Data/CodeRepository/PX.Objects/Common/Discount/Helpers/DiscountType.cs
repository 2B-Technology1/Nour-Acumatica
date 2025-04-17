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

namespace PX.Objects.Common.Discount
{
	public static class DiscountType
	{
		public class ListAttribute : PXStringListAttribute
		{
			public ListAttribute() : base(
				new[]
				{
					Pair(Line, AR.Messages.Line),
					Pair(Group, AR.Messages.Group),
					Pair(Document, AR.Messages.Document),
					Pair(ExternalDocument, AR.Messages.ExternalDocument)
				}) { }
		}

		public const string Line = "L";
		public const string Group = "G";
		public const string Document = "D";
		public const string ExternalDocument = "B";
		public const string Flat = "F";

		public class LineDiscount : PX.Data.BQL.BqlString.Constant<LineDiscount> { public LineDiscount() : base(Line) { } }
		public class GroupDiscount : PX.Data.BQL.BqlString.Constant<GroupDiscount> { public GroupDiscount() : base(Group) { } }
		public class DocumentDiscount : PX.Data.BQL.BqlString.Constant<DocumentDiscount> { public DocumentDiscount() : base(Document) { } }
		public class ExternalDocumentDiscount : PX.Data.BQL.BqlString.Constant<ExternalDocumentDiscount> { public ExternalDocumentDiscount() : base(ExternalDocument) { } }
	}
}