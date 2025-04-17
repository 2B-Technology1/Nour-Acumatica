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
using PX.Data;
using PX.Objects.CS;
using System.Collections.Generic;
using PX.Objects.PM;

namespace PX.Objects.AR.DAC.ReportParameters
{
	public class ARRetainageReportParameters : IBqlTable
	{
		#region Format
		public abstract class format : PX.Data.BQL.BqlString.Field<format>
		{
			public class ListAttribute : PXStringListAttribute
			{
				public const string Summary = "S";
				public const string Details = "D";
				public const string DetailsRetainage = "DR";

				public ListAttribute() : base(GetAllowedValues(), GetAllowedLabels()) { }

				public static string[] GetAllowedValues()
				{
					List<string> allowedValues = new List<string>
					{
						Summary,
						Details
					};

					if (PXAccess.FeatureInstalled<FeaturesSet.retainage>())
					{
						allowedValues.Add(DetailsRetainage);
					};

					return allowedValues.ToArray();
				}

				public static string[] GetAllowedLabels()
				{
					List<string> allowedLabels = new List<string>
					{
						AP.Messages.Summary,
						AP.Messages.Details
					};

					if (PXAccess.FeatureInstalled<FeaturesSet.retainage>())
					{
						allowedLabels.Add(AP.Messages.DetailsWithRetainage);
					};

					return allowedLabels.ToArray();
				}
			}
		}

		[format.List()]
		[PXDBString(2)]
		[PXUIField(DisplayName = AP.Messages.Format, Visibility = PXUIVisibility.SelectorVisible)]
		public String Format { get; set; }
		#endregion

		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }

		[ARActiveProjectAttribute]
		public Int32? ProjectID { get; set; }
	}
}
