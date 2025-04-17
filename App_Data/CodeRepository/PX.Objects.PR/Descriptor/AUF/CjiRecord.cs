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

namespace PX.Objects.PR.AUF
{
	public class CjiRecord : AufRecord
	{
		public CjiRecord(int jobID) : base(AufRecordType.Cji)
		{
			JobID = jobID;
		}

		public override string ToString()
		{
			bool isSubcontractor = IsSubcontractor == true;

			object[] lineData =
			{
				JobID,
				AufConstants.UnusedField,
				ProjectName,
				ProjectNumber,
				ProjectAddress,
				isSubcontractor ? 'S' : 'C',
				AwardingContractor,
				ACAddress,
				ACCity,
				ACStateAbbr,
				FormatZipCode(ACZipCode),
				FormatPhoneNumber(ACPhoneNumber),
				AwardDate,
				FederalProjectNumber,
				StateProjectNumber,
				LicenseNumber,
				ContractAmount,
				ProjectCounty,
				EstimatedCompletionDate,
				EstimatedPercentComplete,
				TypeOfWork,
				isSubcontractor ? PrimeContractor : null,
				isSubcontractor ? PCAddress : null,
				isSubcontractor ? PCCity : null,
				isSubcontractor ? PCStateAbbr : null,
				isSubcontractor ? FormatZipCode(PCZipCode) : null,
				isSubcontractor ? FormatPhoneNumber(PCPhoneNumber) : null,
				isSubcontractor ? PCEmail : null,
				ProjectCity,
				ProjectStateAbbr,
				FormatZipCode(ProjectZipCode)
			};

			return FormatLine(lineData);
		}

		public virtual int JobID { get; set; }
		public virtual string ProjectName { get; set; }
		public virtual string ProjectNumber { get; set; }
		public virtual string ProjectAddress { get; set; }
		public virtual bool? IsSubcontractor { get; set; }
		public virtual string AwardingContractor { get; set; }
		public virtual string ACAddress { get; set; }
		public virtual string ACCity { get; set; }
		public virtual string ACStateAbbr { get; set; }
		public virtual string ACZipCode { get; set; }
		public virtual string ACPhoneNumber { get; set; }
		public virtual DateTime? AwardDate { get; set; }
		public virtual string FederalProjectNumber { get; set; }
		public virtual string StateProjectNumber { get; set; }
		public virtual string LicenseNumber { get; set; }
		public virtual decimal? ContractAmount { get; set; }
		public virtual string ProjectCounty { get; set; }
		public virtual DateTime? EstimatedCompletionDate { get; set; }
		public virtual int? EstimatedPercentComplete { get; set; }
		public virtual string TypeOfWork { get; set; }
		public virtual string PrimeContractor { get; set; }
		public virtual string PCAddress { get; set; }
		public virtual string PCCity { get; set; }
		public virtual string PCStateAbbr { get; set; }
		public virtual string PCZipCode { get; set; }
		public virtual string PCPhoneNumber { get; set; }
		public virtual string PCEmail { get; set; }
		public virtual string ProjectCity { get; set; }
		public virtual string ProjectStateAbbr { get; set; }
		public virtual string ProjectZipCode { get; set; }
	}
}
