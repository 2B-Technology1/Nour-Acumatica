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
using PX.Payroll.Data;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information displayed in the application coming from the web service.
	/// </summary>
	[PXCacheName(Messages.PRTaxWebServiceData)]
	[Serializable]
	public class PRTaxWebServiceData : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRTaxWebServiceData>.By<countryID>
		{
			public static PRTaxWebServiceData Find(PXGraph graph, string countryID, PKFindOptions options = PKFindOptions.None) => 
				FindBy(graph, countryID, options);
		}
		#endregion

		#region CountryID
		[PXDBString(2, IsFixed = true, IsKey = true)]
		[PXDefault]
		public virtual string CountryID { get; set; }
		public abstract class countryID : BqlString.Field<countryID> { }
		#endregion
		#region TaxSettings
		[PXDBString]
		public virtual string TaxSettings { get; set; }
		public abstract class taxSettings : BqlString.Field<taxSettings> { }
		#endregion
		#region DeductionTypes
		[PXDBString]
		public virtual string DeductionTypes { get; set; }
		public abstract class deductionTypes : BqlString.Field<deductionTypes> { }
		#endregion
		#region WageTypes
		[PXDBString]
		public virtual string WageTypes { get; set; }
		public abstract class wageTypes : BqlString.Field<wageTypes> { }
		#endregion
		#region ReportingTypes
		[PXDBString]
		public virtual string ReportingTypes { get; set; }
		public abstract class reportingTypes : BqlString.Field<reportingTypes> { }
		#endregion
		#region QuebecReportingTypes
		/// <summary>
		/// The Quebec reporting types.
		/// </summary>
		[PXDBString]
		public virtual string QuebecReportingTypes { get; set; }
		public abstract class quebecReportingTypes : BqlString.Field<quebecReportingTypes> { }
		#endregion
		#region States
		[PXDBString]
		public virtual string States { get; set; }
		public abstract class states : BqlString.Field<states> { }
		#endregion

		#region System Columns
		#region TStamp
		public class tStamp : IBqlField { }
		[PXDBTimestamp()]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public class createdByID : IBqlField { }
		[PXDBCreatedByID()]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public class createdByScreenID : IBqlField { }
		[PXDBCreatedByScreenID()]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public class createdDateTime : IBqlField { }
		[PXDBCreatedDateTime()]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public class lastModifiedByID : IBqlField { }
		[PXDBLastModifiedByID()]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public class lastModifiedByScreenID : IBqlField { }
		[PXDBLastModifiedByScreenID()]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public class lastModifiedDateTime : IBqlField { }
		[PXDBLastModifiedDateTime()]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
