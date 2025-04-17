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
	/// Stores the information related to the possible work locations of a specific group of employees. The information will be displayed on the Employee Payroll Class (PR202000) form.
	/// </summary>
	[PXCacheName(Messages.PREmployeeClassWorkLocation)]
	[Serializable]
	public class PREmployeeClassWorkLocation : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREmployeeClassWorkLocation>.By<employeeClassID, locationID>
		{
			public static PREmployeeClassWorkLocation Find(PXGraph graph, string employeeClassID, int? locationID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, employeeClassID, locationID, options);
		}

		public static class FK
		{
			public class EmployeeClass : PREmployeeClass.PK.ForeignKeyOf<PREmployeeClassWorkLocation>.By<employeeClassID> { }
			public class Location : PRLocation.PK.ForeignKeyOf<PREmployeeClassWorkLocation>.By<locationID> { }
		}
		#endregion

		#region EmployeeClassID
		public abstract class employeeClassID : PX.Data.BQL.BqlString.Field<employeeClassID> { }
		[PXDBString(10, IsUnicode = true, IsKey = true)]
		[PXDBDefault(typeof(PREmployeeClass.employeeClassID))]
		[PXParent(typeof(Select<PREmployeeClass, Where<PREmployeeClass.employeeClassID, Equal<Current<employeeClassID>>>>))]
		public string EmployeeClassID { get; set; }
		#endregion
		#region LocationID
		public abstract class locationID : PX.Data.BQL.BqlInt.Field<locationID> { }
		[PXDBInt(IsKey = true)]
		[PXSelector(typeof(SelectFrom<PRLocation>
			.InnerJoin<Address>.On<Address.addressID.IsEqual<PRLocation.addressID>>
			.Where<Address.countryID.IsEqual<employeeClassCountryID.FromCurrent>>
			.SearchFor<PRLocation.locationID>), SubstituteKey = typeof(PRLocation.locationCD))]
		[PXUIField(DisplayName = "Location")]
		[PXDefault]
		[PXForeignReference(typeof(Field<locationID>.IsRelatedTo<PRLocation.locationID>))]
		[PXFormula(null, typeof(CountCalc<PREmployeeClass.workLocationCount>))]
		public int? LocationID { get; set; }
		#endregion
		#region IsDefault
		public abstract class isDefault : PX.Data.BQL.BqlBool.Field<isDefault> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Default")]
		public bool? IsDefault { get; set; }
		#endregion

		#region EmployeeClassCountryID
		[PXString]
		[PXUnboundDefault(typeof(Parent<PREmployeeClass.countryID>))]
		public string EmployeeClassCountryID { get; set; }
		public abstract class employeeClassCountryID : PX.Data.BQL.BqlString.Field<employeeClassCountryID> { }
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
