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
using PX.Data.ReferentialIntegrity.Attributes;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information related to the Canadian annual tax form associated with a specific employee.
	/// </summary>
	[PXCacheName(Messages.PREmployeeTaxFormData)]
	[Serializable]
	public class PREmployeeTaxFormData : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREmployeeTaxFormData>.By<batchID, employeeID, formFileType>
		{
			public static PREmployeeTaxFormData Find(PXGraph graph, string batchID, int? employeeID, string formFileType, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, batchID, employeeID, formFileType, options);
		}

		public static class FK
		{
			public class TaxFormBatch : PRTaxFormBatch.PK.ForeignKeyOf<PREmployeeTaxFormData>.By<batchID> { }
			public class Employee : PREmployee.PK.ForeignKeyOf<PREmployeeTaxFormData>.By<employeeID> { }
		}
		#endregion

		#region BatchID
		public abstract class batchID : PX.Data.BQL.BqlString.Field<batchID> { }
		[PXDBString(15, IsKey = true, IsUnicode = true)]
		[PXUIField(DisplayName = "Batch ID")]
		[PXDBDefault(typeof(PRTaxFormBatch.batchID))]
		[PXParent(typeof(FK.TaxFormBatch))]
		public string BatchID { get; set; }
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		[PXDBInt(IsKey = true)]
		[PXForeignReference(typeof(Field<employeeID>.IsRelatedTo<PREmployee.bAccountID>))]
		public int? EmployeeID { get; set; }
		#endregion
		#region FormFileType
		public abstract class formFileType : PX.Data.BQL.BqlString.Field<formFileType> { }
		[PXDBString(5, IsKey = true)]
		[PXUIField(DisplayName = "Form File Type", Enabled = false)]
		[FormFileType.List]
		public string FormFileType { get; set; }
		#endregion
		#region FormData
		public abstract class formData : PX.Data.BQL.BqlString.Field<formData> { }
		[PXDBString]
		public virtual string FormData { get; set; }
		#endregion

		#region System Columns
		#region TStamp
		public abstract class tStamp : PX.Data.BQL.BqlByteArray.Field<tStamp> { }
		[PXDBTimestamp]
		public byte[] TStamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID]
		public Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID]
		public string CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime]
		public DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID]
		public Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID]
		public string LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime]
		public DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion
	}
}
