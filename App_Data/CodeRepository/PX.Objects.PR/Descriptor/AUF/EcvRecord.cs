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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class EcvRecord : AufRecord
	{
		public EcvRecord() : base(AufRecordType.Ecv) { }

		public override string ToString()
		{
			List<object> lineData = new List<object>()
			{
				ElectronicOnly == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox
			};

			for (int i = 0; i < 12; i++)
			{
				lineData.Add(string.IsNullOrEmpty(OfferOfCoverageCode[i]) ? AcaOfferOfCoverage.GetDefault() : OfferOfCoverageCode[i]);
				lineData.Add(MinimumIndividualContribution[i] == 0 ? new decimal?() : MinimumIndividualContribution[i]);
				lineData.Add(SafeHarborCode[i]);
			}

			lineData.Add(PolicyOriginCode);
			lineData.Add(SelfInsuredEmployee == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox);
			lineData.Add(PlanStartMonth);

			return FormatLine(lineData.ToArray());
		}

		public virtual bool? ElectronicOnly { get; set; }
		public virtual string[] OfferOfCoverageCode { get; set; } = new string[12];
		public virtual decimal?[] MinimumIndividualContribution { get; set; } = new decimal?[12];
		public virtual string[] SafeHarborCode { get; set; } = new string[12];
		public virtual char? PolicyOriginCode { get; set; }
		public virtual bool? SelfInsuredEmployee { get; set; }
		public virtual int? PlanStartMonth { get; set; }
	}
}
