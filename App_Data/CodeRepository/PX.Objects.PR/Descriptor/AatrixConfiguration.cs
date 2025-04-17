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

using Microsoft.Extensions.Options;
using PX.Data;

namespace PX.Objects.PR
{
	public class AatrixConfiguration
	{
		public static string WebformsUrl { get; set; }
		public static string VendorID { get; set; }
		public static int TransactionTimeoutMs { get; set; }

		private static string _GetAvailableFormsListEndpoint;
		private static string _UploadAufEndpoint;

		public static string GetEndpoint(AatrixOperation op)
		{
			switch (op)
			{
				case AatrixOperation.GetAvailableFormsList:
					return _GetAvailableFormsListEndpoint;
				case AatrixOperation.UploadAuf:
					return _UploadAufEndpoint;
			}

			throw new PXException(Messages.CantFindAatrixEndpoint);
		}

		public class Options
		{
			public string WebformsUrl { get; set; }
			public string VendorID { get; set; }
			public int TransactionTimeoutMs { get; set; }
			public string GetAvailableFormsListEndpoint { get; set; }
			public string UploadAufEndpoint { get; set; }
		}

		public class Initializer
		{
			private Options _Options;

			public Initializer(IOptions<Options> options) => _Options = options.Value;

			public void Initialize()
			{

				AatrixConfiguration.WebformsUrl = _Options.WebformsUrl;
				AatrixConfiguration.VendorID = _Options.VendorID;
				AatrixConfiguration.TransactionTimeoutMs = _Options.TransactionTimeoutMs;
				AatrixConfiguration._GetAvailableFormsListEndpoint = _Options.GetAvailableFormsListEndpoint;
				AatrixConfiguration._UploadAufEndpoint = _Options.UploadAufEndpoint;
			}
		}
	}	

	public enum AatrixOperation
	{
		GetAvailableFormsList,
		UploadAuf
	}
}
