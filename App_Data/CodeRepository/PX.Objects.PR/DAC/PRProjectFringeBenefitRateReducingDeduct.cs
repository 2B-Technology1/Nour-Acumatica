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
using PX.Data.BQL;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.IN;
using PX.Objects.PM;
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// A benefit that reduces the fringe benefit rates to be payed.
	/// </summary>
	[PXCacheName(Messages.PRProjectFringeBenefitRateReducingDeduct)]
	[Serializable]
	public class PRProjectFringeBenefitRateReducingDeduct : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRProjectFringeBenefitRateReducingDeduct>.By<projectID, deductCodeID>
		{
			public static PRProjectFringeBenefitRateReducingDeduct Find(PXGraph graph, int? projectID, int? deductCodeID, PKFindOptions options = PKFindOptions.None) => 
				FindBy(graph, projectID, deductCodeID, options);
		}

		public static class FK
		{
			public class Project : PMProject.PK.ForeignKeyOf<PRProjectFringeBenefitRateReducingDeduct>.By<projectID> { }
			public class DeductionCode : PRDeductCode.PK.ForeignKeyOf<PRProjectFringeBenefitRateReducingDeduct>.By<deductCodeID> { }
		}
		#endregion

		#region ProjectID
		public abstract class projectID : BqlInt.Field<projectID> { }
		/// <summary>
		/// The unique identifier of the certified project.
		/// The field is included in <see cref="FK.Project"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMProject.ContractID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXParent(typeof(FK.Project))]
		public virtual int? ProjectID { get; set; }
		#endregion
		#region DeductCodeID
		public abstract class deductCodeID : BqlInt.Field<deductCodeID> { }
		/// <summary>
		/// The unique identifier of a benefit code which employer contribution settings the system uses to offset the fringe benefit rate.
		/// The field is included in <see cref="FK.DeductionCode"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PRDeductCode.CodeID"/> field.
		/// </value>
		[PXDBInt(IsKey = true)]
		[PXDefault]
		[PXUIField(DisplayName = "Benefit Code")]
		[DeductionActiveSelector(
			typeof(Where<PRDeductCode.contribType.IsNotEqual<ContributionTypeListAttribute.employeeDeduction>
				.And<PRDeductCode.certifiedReportType.IsNotNull>>),
			typeof(countryUS))]
		[PXForeignReference(typeof(Field<deductCodeID>.IsRelatedTo<PRDeductCode.codeID>))]
		public virtual int? DeductCodeID { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : BqlBool.Field<isActive> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the system uses the employer contribution to offset the fringe benefit rate the company is supposed to pay to employees working on the certified project.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion
		#region AnnualizationException
		public abstract class annualizationException : BqlBool.Field<annualizationException> { }
		/// <summary>
		/// A boolean value that specifies (if set to <see langword="true" />) that the system uses hours worked on the project to calculate the employer contribution rate instead of using the total hours worked during the pay period.
		/// </summary>
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Annualization Exception")]
		public virtual bool? AnnualizationException { get; set; }
		#endregion

		#region CountryUS
		[PXString(2)]
		[PXUnboundDefault(typeof(BQLLocationConstants.CountryUS))]
		public string CountryUS { get; set; }
		public abstract class countryUS : BqlString.Field<countryUS> { }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
