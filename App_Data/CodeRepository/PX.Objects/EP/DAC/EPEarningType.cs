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
using PX.Data;
using PX.Data.EP;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.PM;

namespace PX.Objects.EP
{
	/// <summary>
	/// A code that is used to categorize the type of work done by an employee. The information will be visible on the Earning Types (EP102000) form.
	/// </summary>
	[PXCacheName(Messages.EarningType)]
	[Serializable]
	public partial class EPEarningType : IBqlTable
	{
		#region Keys

		/// <summary>
		/// Primary Key
		/// </summary>
		public class PK : PrimaryKeyOf<EPEarningType>.By<typeCD>
		{
			public static EPEarningType Find(PXGraph graph, string typeCD, PKFindOptions options = PKFindOptions.None) => FindBy(graph, typeCD, options);
		}

		/// <summary>
		/// Foreign Keys
		/// </summary>
		public static class FK
		{
			/// <summary>
			/// Default Project
			/// </summary>
			public class DefaultProject : PMProject.PK.ForeignKeyOf<EPEarningType>.By<projectID> { }

			/// <summary>
			/// Default Project Task
			/// </summary>
			public class DefaultProjectTask : PMTask.PK.ForeignKeyOf<EPEarningType>.By<projectID, taskID> { }
		}
		#endregion

		#region TypeCD
		public abstract class typeCD : PX.Data.BQL.BqlString.Field<typeCD>
		{
			public const int Length = 15;
			public const string InputMask = ">CCCCCCCCCCCCCCC";
		}
		[PXDefault]
		[PXDBString(typeCD.Length, IsUnicode = true, IsKey = true, InputMask = typeCD.InputMask)]
		[PXUIField(DisplayName = "Code", Visibility = PXUIVisibility.SelectorVisible)]
		[PXReferentialIntegrityCheck]
		public virtual String TypeCD { get; set; }
		#endregion

		#region Description
		public abstract class description : PX.Data.BQL.BqlString.Field<description> { }
		[PXDBString(60, IsUnicode = true)]
		[PXDefault()]
		[PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
		[PXFieldDescription]
		public virtual String Description { get; set; }
		#endregion

		#region isActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Active")]
		public virtual Boolean? IsActive { get; set; }
		#endregion

		#region isOvertime
		public abstract class isOvertime : PX.Data.BQL.BqlBool.Field<isOvertime> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Overtime")]
		public virtual Boolean? IsOvertime { get; set; }
		#endregion

		#region isBillable
		public abstract class isbillable : PX.Data.BQL.BqlBool.Field<isbillable> { }
		[PXDBBool]
		[PXDefault(false)]
		[PXUIField(DisplayName = "Billable")]
		public virtual Boolean? isBillable { get; set; }
		#endregion

		#region ProjectID
		public abstract class projectID : PX.Data.BQL.BqlInt.Field<projectID> { }
		[Project(typeof(Where<PMProject.baseType, Equal<CT.CTPRType.project>>), DisplayName = "Default Project")]
		public virtual Int32? ProjectID { get; set; }
		#endregion

		#region TaskID
		public abstract class taskID : PX.Data.BQL.BqlInt.Field<taskID> { }
		[PM.ProjectTask(typeof(EPEarningType.projectID), DisplayName = "Default Project Task", AllowNull = true)]
		public virtual Int32? TaskID { get; set; }
		#endregion

		#region OvertimeMultiplier
		public abstract class overtimeMultiplier : PX.Data.BQL.BqlDecimal.Field<overtimeMultiplier> { }
		[PXDBDecimal(2, MinValue = 0.01, MaxValue = 99.99)]
		[PXUIEnabled(typeof(isOvertime))]
		[PXFormula(typeof(Switch<Case<Where<isOvertime, NotEqual<True>>, CS.decimal1>, overtimeMultiplier>))]
		[PXUIField(DisplayName = "Multiplier")]
		public virtual Decimal? OvertimeMultiplier { get; set; }
		#endregion


		#region System Columns
		#region tstamp
		public abstract class Tstamp : PX.Data.BQL.BqlByteArray.Field<Tstamp> { }
		[PXDBTimestamp()]
		public virtual Byte[] tstamp { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		[PXDBCreatedByID()]
		public virtual Guid? CreatedByID { get; set; }
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID { get; set; }
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		[PXDBCreatedDateTime()]
		public virtual DateTime? CreatedDateTime { get; set; }
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		[PXDBLastModifiedByID()]
		public virtual Guid? LastModifiedByID { get; set; }
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID { get; set; }
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		[PXDBLastModifiedDateTime()]
		public virtual DateTime? LastModifiedDateTime { get; set; }
		#endregion
		#endregion

	}
}
