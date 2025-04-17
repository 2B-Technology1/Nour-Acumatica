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
using PX.Objects.CS;
using PX.Data.BQL;
using PX.Objects.GL.Attributes;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.GL.DAC;

namespace PX.Objects.Localizations.CA
{
	/// <summary>
	/// The T5018 organization settings for reporting.
	/// </summary>
	[PXCacheName("T5018 Organization Settings")]
	public class T5018OrganizationSettings : IBqlTable
	{	
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.canadianLocalization>();
		}

		#region Keys
		public class PK : PrimaryKeyOf<T5018OrganizationSettings>.By<organizationID>
		{
			public static T5018OrganizationSettings Find(PXGraph graph, int? organizationID) => FindBy(graph, organizationID);
		}

		public static class FK
		{
			public class OrganizationFK : Organization.PK.ForeignKeyOf<T5018OrganizationSettings>.By<organizationID> { }
		}
		#endregion		

		#region OrganizationID
		public abstract class organizationID : BqlInt.Field<organizationID> { }
		/// <summary>
		/// The ID of the organization.
		/// The field is included in <see cref="FK.OrganizationFK"/>.
		/// </summary>
		[PXDBInt( IsKey = true)]
		[PXDBDefault(typeof(Organization.organizationID))]
		[PXParent(typeof(FK.OrganizationFK))]
		public int? OrganizationID
		{
			get;
			set;
		}
		#endregion

		#region T5018ReportingYear
		public abstract class t5018ReportingYear : BqlType<IBqlInt, int>.Field<t5018ReportingYear>
		{
			#region T5018ReportingYearConstants
			public const int CalendarYear = 1;
			public class calendarYear : BqlInt.Constant<calendarYear>
			{
				public calendarYear() :
					base(CalendarYear)
				{ }
			}

			public const int FiscalYear = 2;
			public class fiscalYear : BqlInt.Constant<fiscalYear>
			{
				public fiscalYear() :
					base(FiscalYear)
				{ }
			}
			#endregion
		}

		/// <summary>
		/// The T5018 reporting year type of the organization.
		/// </summary>
		[PXDBInt]
		[PXIntList(new int[] { t5018ReportingYear.CalendarYear, t5018ReportingYear.FiscalYear }, new string[] { Messages.T5018Messages.CalendarYear, Messages.T5018Messages.FiscalYear})]
		[PXDefault(t5018ReportingYear.FiscalYear)]
		[PXUIField(DisplayName = "Year Type")]
		public int? T5018ReportingYear
		{
			get;
			set;
		}
		#endregion

		#region ProgramNumber
		public abstract class programNumber : BqlString.Field<programNumber> { }
		/// <summary>
		/// The program number of the organization.
		/// </summary>
		[PXDBString(15)]
		[PXUIField(DisplayName = "Program Number")]
		public string ProgramNumber
		{
			get;
			set;
		}
		#endregion

		#region TransmitterNumber
		public abstract class transmitterNumber : BqlString.Field<transmitterNumber> { }
		/// <summary>
		/// The transmitter number of the organization.
		/// </summary>
		[PXDBString(8)]
		[PXUIField(DisplayName = "Transmitter Number")]
		[PXDefault("MM555555", PersistingCheck = PXPersistingCheck.Nothing)]
		public string TransmitterNumber
		{
			get;
			set;
		}
		#endregion
	}
}
