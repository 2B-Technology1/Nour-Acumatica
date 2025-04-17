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
	public class CfbRecord : AufRecord
	{
		public CfbRecord(string benefitCode, char benefitType) : base(AufRecordType.Cfb)
		{
			BenefitName = benefitCode;
			BenefitID = PimRecord.GetPimIDFromName(benefitCode);
			BenefitType = benefitType;
		}

		public override string ToString()
		{
			object[] lineData =
			{
				BenefitID,
				BenefitType,
				BenefitName,
				VendorAddress,
				VendorCity,
				VendorState,
				VendorZipCode,
				FormatPhoneNumber(VendorPhone),
				AccountNumber,
				VendorContact,
				'E' // Payment method
			};

			return FormatLine(lineData);
		}

		public virtual int BenefitID { get; set; }
		public virtual char BenefitType { get; set; }
		public virtual string BenefitName { get; set; }
		public virtual string VendorAddress { get; set; }
		public virtual string VendorCity { get; set; }
		public virtual string VendorState { get; set; }
		public virtual string VendorZipCode { get; set; }
		public virtual string VendorPhone { get; set; }
		public virtual string AccountNumber { get; set; }
		public virtual string VendorContact { get; set; }
	}
}
