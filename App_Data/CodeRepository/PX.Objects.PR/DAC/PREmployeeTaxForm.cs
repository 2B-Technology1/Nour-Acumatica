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
using PX.Objects.CS;
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// Stores the information related to the Canadian annual tax form associated with a specific employee.
	/// </summary>
	[PXCacheName(Messages.PREmployeeTaxForm)]
	[Serializable]
	public class PREmployeeTaxForm : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PREmployeeTaxForm>.By<batchID, employeeID>
		{
			public static PREmployeeTaxForm Find(PXGraph graph, string batchID, int? employeeID, PKFindOptions options = PKFindOptions.None) =>
				FindBy(graph, batchID, employeeID, options);
		}

		public static class FK
		{
			public class TaxFormBatch : PRTaxFormBatch.PK.ForeignKeyOf<PREmployeeTaxForm>.By<batchID> { }
			public class Employee : PREmployee.PK.ForeignKeyOf<PREmployeeTaxForm>.By<employeeID> { }
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
		[PXUIField(Visible = false, Visibility = PXUIVisibility.Invisible)]
		[PXFormula(null, typeof(CountCalc<PRTaxFormBatch.numberOfEmployees>))]
		[PXUnboundFormula(typeof(int1.When<published.IsEqual<True>>.Else<int0>), typeof(SumCalc<PRTaxFormBatch.numberOfPublishedEmployees>))]
		[PXForeignReference(typeof(Field<employeeID>.IsRelatedTo<PREmployee.bAccountID>))]
		public int? EmployeeID { get; set; }
		#endregion
		#region Published
		public abstract class published : PX.Data.BQL.BqlBool.Field<published> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Published", Enabled = false)]
		public virtual bool? Published { get; set; }
		#endregion
		#region EverPublished
		public abstract class everPublished : PX.Data.BQL.BqlBool.Field<everPublished> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(Enabled = false)]
		public virtual bool? EverPublished { get; set; }
		#endregion
		#region NotPublished
		public abstract class notPublished : PX.Data.BQL.BqlBool.Field<notPublished> { }
		[PXBool]
		[PXFormula(typeof(Where<published.IsEqual<False>>))]
		public virtual bool? NotPublished { get; set; }
		#endregion
		#region PublishedFrom
		public abstract class publishedFrom : PX.Data.BQL.BqlBool.Field<publishedFrom> { }
		[PXString]
		[PXUIField(DisplayName = "Published From", Enabled = false)]
		public virtual string PublishedFrom { get; set; }
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
