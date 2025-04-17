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
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CR;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the work locations in which the employee can work. The information will be displayed on the Employee Payroll Settings (PR203000) form.
	/// </summary>
	[PXCacheName(Messages.PREmployeeWorkLocation)]
	[Serializable]
	public class PREmployeeWorkLocation : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREmployeeWorkLocation>.By<employeeID, locationID>
		{
			public static PREmployeeWorkLocation Find(PXGraph graph, int? employeeID, int? locationID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, employeeID, locationID, options);
		}

		public static class FK
		{
			public class Employee : PREmployee.PK.ForeignKeyOf<PREmployeeWorkLocation>.By<employeeID> { }
			public class Location : PRLocation.PK.ForeignKeyOf<PREmployeeWorkLocation>.By<locationID> { }
		}
		#endregion

		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[PXDBInt(IsKey = true)]
		[PXDBDefault(typeof(PREmployee.bAccountID))]
		[PXParent(typeof(Select<PREmployee, Where<PREmployee.bAccountID, Equal<Current<employeeID>>>>))]
		public int? EmployeeID { get; set; }
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(SelectFrom<PRLocation>
			.InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>
			.Where<Address.countryID.IsEqual<employeeCountryID.FromCurrent>>
			.SearchFor<PRLocation.locationID>), SubstituteKey = typeof(PRLocation.locationCD))]
		[PXRestrictor(typeof(Where<PRLocation.isActive.IsEqual<True>>), Messages.LocationIsInactive, typeof(PRLocation.locationID))]
		[PXUIField(DisplayName = "Location")]
		[PXDefault]
		[PXForeignReference(typeof(Field<locationID>.IsRelatedTo<PRLocation.locationID>))]
		public int? LocationID { get; set; }
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default")]
		public bool? IsDefault { get; set; }
		#endregion

		#region EmployeeCountryID
		[PXString]
		[PXUnboundDefault(typeof(Parent<PREmployee.countryID>))]
		public string EmployeeCountryID { get; set; }
		public abstract class employeeCountryID : PX.Data.BQL.BqlString.Field<employeeCountryID> { }
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
