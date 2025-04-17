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
using System;

namespace PX.Objects.PR
{
	/// <summary>
	/// The rate of a fringe benefit which an organization is required to pay its employees who work on certified projects.
	/// </summary>
	[PXCacheName(Messages.PRProjectFringeBenefitRate)]
	[Serializable]
	public class PRProjectFringeBenefitRate : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<PRProjectFringeBenefitRate>.By<recordID>
		{
			public static PRProjectFringeBenefitRate Find(PXGraph graph, int? recordID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, recordID, options);
		}

		public static class FK
		{
			public class Project : PMProject.PK.ForeignKeyOf<PRProjectFringeBenefitRate>.By<projectID> { }
			public class LaborItem : InventoryItem.PK.ForeignKeyOf<PRProjectFringeBenefitRate>.By<laborItemID> { }
			public class ProjectTask : PMTask.PK.ForeignKeyOf<PRProjectFringeBenefitRate>.By<projectID, projectTaskID> { }
		}
		#endregion

		#region RecordID
		public abstract class recordID : BqlInt.Field<recordID> { }
		/// <summary>
		/// The unique identifier of the fringe benefit rate.
		/// </summary>
		[PXDBIdentity(IsKey = true)]
		public virtual int? RecordID { get; set; }
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
		[PXDBInt]
		[PXDBDefault(typeof(PMProject.contractID))]
		[PXParent(typeof(Select<PMProject, Where<PMProject.contractID, Equal<Current<projectID>>>>))]
		public virtual int? ProjectID { get; set; }
		#endregion
		#region LaborItemID
		public abstract class laborItemID : BqlInt.Field<laborItemID> { }
		/// <summary>
		/// The unique identifier of the labor item associated with the project pay rate.
		/// The field is included in <see cref="FK.LaborItem"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="InventoryItem.InventoryID"/> field.
		/// </value>
		[PMLaborItem(typeof(projectID), null, null)]
		[PXDefault]
		[PXForeignReference(typeof(Field<laborItemID>.IsRelatedTo<InventoryItem.inventoryID>))]
		public virtual int? LaborItemID { get; set; }
		#endregion
		#region ProjectTaskID
		public abstract class projectTaskID : BqlInt.Field<projectTaskID> { }
		/// <summary>
		/// The unique identifier of the project task associated with the project pay rate.
		/// The field is included in <see cref="FK.ProjectTask"/>.
		/// </summary>
		/// <value>
		/// The value of this field corresponds to the value of the <see cref="PMTask.TaskID"/> field.
		/// </value>
		[ProjectTask(typeof(projectID), AllowNull = true)]
		public virtual int? ProjectTaskID { get; set; }
		#endregion
		#region Rate
		public abstract class rate : BqlDecimal.Field<rate> { }
		/// <summary>
		/// The pay rate for the combination of the labor item and project task.
		/// </summary>
		[PRCurrency(MinValue = 0)]
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Rate")]
		public virtual decimal? Rate { get; set; }
		#endregion
		#region EffectiveDate
		public abstract class effectiveDate : BqlDateTime.Field<effectiveDate> { }
		/// <summary>
		/// The date when the pay rate becomes effective.
		/// </summary>
		[PXDefault]
		[PXDBDate]
		[PXUIField(DisplayName = "Effective Date")]
		[PXCheckUnique(typeof(projectID), typeof(laborItemID), typeof(projectTaskID), ClearOnDuplicate = false)]
		public virtual DateTime? EffectiveDate { get; set; }
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
