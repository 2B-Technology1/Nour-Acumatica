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
using PX.Payroll.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PX.Objects.PR.AUF
{
	public class EmpRecord : AufRecord
	{
		public EmpRecord(string employeeID) : base(AufRecordType.Emp)
		{
			EmployeeID = employeeID.TrimEnd();
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(FirstName))
			{
				throw new PXException(Messages.AatrixReportFirstNameMissing, EmployeeID);
			}

			if (string.IsNullOrEmpty(LastName))
			{
				throw new PXException(Messages.AatrixReportLastNameMissing, EmployeeID);
			}

			bool isUS = LocationConstants.USCountryCode == CountryAbbr;
			bool isCanada = LocationConstants.CanadaCountryCode == CountryAbbr;
			bool isAlaska = isUS && LocationConstants.AlaskaStateAbbr == StateAbbr;

			object[] lineData =
			{
				FirstName,
				MiddleName,
				LastName,
				NameSuffix,
				isUS ? FormatSsn(SocialSecurityNumber, EmployeeID) : null,
				AddressLine1,
				City,
				County,
				CountyCode,
				isUS ? StateAbbr : null,
				isUS ? FormatZipCode(ZipCode) : null,
				Country,
				isUS ? null : CountryAbbr,
				isUS ? null : NonUSPostalCode,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				AufConstants.UnusedField,
				IsFemale == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				IsDisabled == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				HireDate,
				RehireDate == null || RehireDate < FireDate ? FireDate : null,
				MedicalCoverageDate,
				BirthDate,
				PayRate,
				FederalExemptions,
				IsHourlyPay == true ? 'H' : 'S',
				IsFullTime == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				Title,
				StateOfHireAbbr,
				WorkType,
				AufConstants.UnusedField,
				HasHealthBenefits == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				FormatPhoneNumber(PhoneNumber),
				IsSeasonal == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				WorkersCompClass,
				WorkersCompSubclass,
				AufConstants.UnusedField,
				MaritalStatus,
				EmployeeID,
				IsStatutoryEmployee == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				HasRetirementPlan == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				HasThirdPartySickPay == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				HasDirectDeposit == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				AddressLine2,
				AufConstants.UnusedField,
				Email,
				HasElectronicW2 == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				isUS ? null : NonUSState,
				RehireDate,
				isUS || isCanada ? EmploymentCode : null,
				StandardOccupationalClassification,
				isAlaska ? GeographicCode : null,
				PensionDate,
				isUS ? null : NonUSNationalID,
				AufConstants.UnusedField,
				isCanada && CppExempt == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				isCanada && EmploymentInsuranceExempt == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				isCanada && ProvincialParentalInsurancePlanExempt == true ? AufConstants.SelectedBox : AufConstants.NotSelectedBox,
				StateExemptions,
				Ethnicity,
				AufConstants.ManualInput, // Sort Text
				EEOJobCategory,
				EEOEthnicity
			};

			StringBuilder builder = new StringBuilder(FormatLine(lineData));
			GenList?.ForEach(gen => builder.Append(gen.ToString()));
			EffList?.ForEach(eff => builder.Append(eff.ToString()));
			GenList?.SelectMany(x => x.EjwList ?? new List<EjwRecord>()).Aggregate(GenList).ToList().ForEach(ejw => builder.Append(ejw.ToString()));
			EfbList?.ForEach(efb => builder.Append(efb.ToString()));

			if (Ecv != null)
			{
				builder.Append(Ecv.ToString());
			}

			if (Eci != null)
			{
				builder.Append(Eci.ToString());
			}

			return builder.ToString();
		}

		#region Data
		public virtual string FirstName { get; set; }
		public virtual string MiddleName { get; set; }
		public virtual string LastName { get; set; }
		public virtual string NameSuffix { get; set; }
		public virtual string SocialSecurityNumber { get; set; }
		public virtual string AddressLine1 { get; set; }
		public virtual string City { get; set; }
		public virtual string County { get; set; }
		public virtual string CountyCode { get; set; }
		public virtual string StateAbbr { get; set; }
		public virtual string ZipCode { get; set; }
		public virtual string Country { get; set; }
		public virtual string CountryAbbr { get; set; }
		public virtual string NonUSPostalCode { get; set; }
		public virtual bool? IsFemale { get; set; }
		public virtual bool? IsDisabled { get; set; }
		public virtual DateTime? HireDate { get; set; }
		public virtual DateTime? FireDate { get; set; }
		public virtual DateTime? MedicalCoverageDate { get; set; }
		public virtual DateTime? BirthDate { get; set; }
		public virtual decimal? PayRate { get; set; }
		public virtual int? FederalExemptions { get; set; }
		public virtual bool? IsHourlyPay { get; set; }
		public virtual bool? IsFullTime { get; set; }
		public virtual string Title { get; set; }
		public virtual string StateOfHireAbbr { get; set; }
		public virtual string WorkType { get; set; }
		public virtual bool? HasHealthBenefits { get; set; }
		public virtual string PhoneNumber { get; set; }
		public virtual bool? IsSeasonal { get; set; }
		public virtual string WorkersCompClass { get; set; }
		public virtual string WorkersCompSubclass { get; set; }
		public virtual string MaritalStatus { get; set; }
		public virtual string EmployeeID { get; set; }
		public virtual bool? IsStatutoryEmployee { get; set; }
		public virtual bool? HasRetirementPlan { get; set; }
		public virtual bool? HasThirdPartySickPay { get; set; }
		public virtual bool? HasDirectDeposit { get; set; }
		public virtual string AddressLine2 { get; set; }
		public virtual string Email { get; set; }
		public virtual bool? HasElectronicW2 { get; set; }
		public virtual string NonUSState { get; set; }
		public virtual DateTime? RehireDate { get; set; }
		public virtual string EmploymentCode { get; set; }
		public virtual string StandardOccupationalClassification { get; set; }
		public virtual string GeographicCode { get; set; }
		public virtual DateTime? PensionDate { get; set; }
		public virtual string NonUSNationalID { get; set; }
		public virtual bool? CppExempt { get; set; }
		public virtual bool? EmploymentInsuranceExempt { get; set; }
		public virtual bool? ProvincialParentalInsurancePlanExempt { get; set; }
		public virtual int? StateExemptions { get; set; }
		public virtual string Ethnicity { get; set; }
		public virtual int? EEOJobCategory { get; set; }
		public virtual string EEOEthnicity { get; set; }
		#endregion

		#region Children records
		public List<GenRecord> GenList { get; set; }
		public List<EffRecord> EffList { get; set; }
		public List<EfbRecord> EfbList { private get; set; }
		public EcvRecord Ecv { private get; set; }
		public EciRecord Eci { private get; set; }
		#endregion

		#region Obsolete data
		[Obsolete(Common.Messages.ItemIsObsoleteAndWillBeRemoved2023R2)]
		public virtual string FullOccupationalTitle { get; set; }
		#endregion
	}
}
