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

using HtmlAgilityPack;
using PX.Objects.CN.Common.Descriptor;

namespace PX.Objects.CN.Common.Helpers
{
	public static class RichTextEditHelper
	{
		public static string GetInnerText(string htmlText)
		{
			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(htmlText);
			var htmlBody = htmlDocument.DocumentNode.SelectSingleNode(Constants.HtmlParser.HtmlBodyXpath);
			return htmlBody.InnerText.Replace(Constants.HtmlParser.HtmlSpace, string.Empty)
				.Replace(Constants.HtmlParser.InnerSpace, string.Empty);
		}
	}
}