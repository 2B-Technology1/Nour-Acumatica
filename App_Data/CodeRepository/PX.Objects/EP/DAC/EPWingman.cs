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
using System.Collections.Generic;
using System.Text;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Data.SQLTree;
using PX.Objects.AP;
using PX.Objects.CS;
using PX.Objects.CR;

namespace PX.Objects.EP
{
	[Serializable]
	[PXCacheName(Messages.Wingman)]
	public partial class EPWingman : IBqlTable
	{
		#region Keys
		public class PK : PrimaryKeyOf<EPWingman>.By<recordID>
		{
			public static EPWingman Find(PXGraph graph, int recordID, PKFindOptions options = PKFindOptions.None) => FindBy(graph, recordID, options);
		}
		public static class FK
		{
		}
		#endregion

		#region RecordID
		public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
		protected Int32? _RecordID;
		[PXDBIdentity(IsKey = true)]
		public virtual Int32? RecordID
		{
			get
			{
				return _RecordID;
			}
			set
			{
				_RecordID = value;
			}
		}
		#endregion
		#region EmployeeID
		public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }
		protected Int32? _EmployeeID;
		[PXDBInt()]
		[PXDBDefault(typeof(EPEmployee.bAccountID))]
		[PXParent(typeof(Select<EPEmployee, Where<EPEmployee.bAccountID, Equal<Current<EPWingman.employeeID>>>>))]
		[PXEPEmployeeSelector]
		public Int32? EmployeeID
		{
			get
			{
				return _EmployeeID;
			}
			set
			{
				_EmployeeID = value;
			}
		}
		#endregion
		#region WingmanID
		public abstract class wingmanID : PX.Data.BQL.BqlInt.Field<wingmanID> { }
		protected Int32? _WingmanID;
		[PXDBInt()]
		[PXEPEmployeeSelector]
		[PXRestrictor(typeof(Where<EPEmployee.vStatus, Equal<VendorStatus.active>>), Messages.EmployeeIsInactive)]
		[PXCheckUnique(typeof(employeeID), typeof(delegationOf), Where = typeof(Where<EPWingman.delegationOf, Equal<EPDelegationOf.expenses>>))]
		[PXUIField(DisplayName = "Delegated To", Required = true)]
		[PXDefault(PersistingCheck = PXPersistingCheck.NullOrBlank)]
		public Int32? WingmanID
		{
			get
			{
				return _WingmanID;
			}
			set
			{
				_WingmanID = value;
			}
		}
		#endregion
		#region DelegationOf
		public abstract class delegationOf : PX.Data.BQL.BqlString.Field<delegationOf> { }
		/// <summary>
		/// Represents the type of the delegation.
		/// </summary>
		/// <value>
		/// The field can have one of the values listed in the <see cref="EPDelegationOf"/> class.
		/// </value>
		[PXDBString(1, IsFixed = true)]
		[EPDelegationOf.List]
		[PXUIField(DisplayName = "Delegation Of")]
		[PXDefault(typeof(IIf<Where<IsContractBasedAPI, Equal<True>>,
			EPDelegationOf.expenses,
			EPDelegationOf.approvals>))]
		public virtual string DelegationOf { get; set; }
		#endregion
		#region StartsOn
		public abstract class startsOn : PX.Data.BQL.BqlDateTime.Field<startsOn> { }
		/// <summary>
		/// Delegation start date
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Starts On")]
		public virtual DateTime? StartsOn { get; set; }
		#endregion
		#region ExpiresOn
		public abstract class expiresOn : PX.Data.BQL.BqlDateTime.Field<expiresOn> { }
		/// <summary>
		/// Delegation end date
		/// </summary>
		[PXDBDate]
		[PXUIField(DisplayName = "Expires On")]
		[EPVerifyEndDate(typeof(startsOn), AllowAutoChange = true, AutoChangeWarning = true)]
		public virtual DateTime? ExpiresOn { get; set; }
		#endregion
		#region IsActive
		public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
		/// <summary>
		/// If set to <see langword="true"/>, this field indicates that the delegation is active.
		/// </summary>
		[PXDBBool]
		[PXDefault(true)]
		[PXUIField(DisplayName = "Active")]
		public virtual bool? IsActive { get; set; }
		#endregion
		#region CreatedByID
		public abstract class createdByID : PX.Data.BQL.BqlGuid.Field<createdByID> { }
		protected Guid? _CreatedByID;
		[PXDBCreatedByID]
		public virtual Guid? CreatedByID
		{
			get
			{
				return this._CreatedByID;
			}
			set
			{
				this._CreatedByID = value;
			}
		}
		#endregion
		#region CreatedByScreenID
		public abstract class createdByScreenID : PX.Data.BQL.BqlString.Field<createdByScreenID> { }
		protected String _CreatedByScreenID;
		[PXDBCreatedByScreenID()]
		public virtual String CreatedByScreenID
		{
			get
			{
				return this._CreatedByScreenID;
			}
			set
			{
				this._CreatedByScreenID = value;
			}
		}
		#endregion
		#region CreatedDateTime
		public abstract class createdDateTime : PX.Data.BQL.BqlDateTime.Field<createdDateTime> { }
		protected DateTime? _CreatedDateTime;
		[PXDBCreatedDateTime()]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.CreatedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? CreatedDateTime
		{
			get
			{
				return this._CreatedDateTime;
			}
			set
			{
				this._CreatedDateTime = value;
			}
		}
		#endregion
		#region LastModifiedByID
		public abstract class lastModifiedByID : PX.Data.BQL.BqlGuid.Field<lastModifiedByID> { }
		protected Guid? _LastModifiedByID;
		[PXDBLastModifiedByID]
		public virtual Guid? LastModifiedByID
		{
			get
			{
				return this._LastModifiedByID;
			}
			set
			{
				this._LastModifiedByID = value;
			}
		}
		#endregion
		#region LastModifiedByScreenID
		public abstract class lastModifiedByScreenID : PX.Data.BQL.BqlString.Field<lastModifiedByScreenID> { }
		protected String _LastModifiedByScreenID;
		[PXDBLastModifiedByScreenID()]
		public virtual String LastModifiedByScreenID
		{
			get
			{
				return this._LastModifiedByScreenID;
			}
			set
			{
				this._LastModifiedByScreenID = value;
			}
		}
		#endregion
		#region LastModifiedDateTime
		public abstract class lastModifiedDateTime : PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime> { }
		protected DateTime? _LastModifiedDateTime;
		[PXDBLastModifiedDateTime()]
        [PXUIField(DisplayName = PXDBLastModifiedByIDAttribute.DisplayFieldNames.LastModifiedDateTime, Enabled = false, IsReadOnly = true)]
        public virtual DateTime? LastModifiedDateTime
		{
			get
			{
				return this._LastModifiedDateTime;
			}
			set
			{
				this._LastModifiedDateTime = value;
			}
		}
		#endregion
	}

	/// <exclude/>
	[PXHidden]
	[PXProjection(typeof(
		SelectFrom<EPWingman>
			.Where<
				EPWingman.delegationOf.IsEqual<EPDelegationOf.expenses>
				.And<EPWingman.isActive.IsEqual<True>>>
	))]
	public class EPWingmanForExpenses : EPWingman { }

	/// <exclude/>
	[PXHidden]
	[PXProjection(typeof(
		SelectFrom<EPWingman>
			.Where<
				EPWingman.delegationOf.IsEqual<EPDelegationOf.approvals>
				.And<EPWingman.isActive.IsEqual<True>>>
	))]
	public class EPWingmanForApprovals : EPWingman { }

	public class WingmanUser<UserID> : IBqlComparison
		where UserID : IBqlOperand, new()
	{
		private IBqlCreator _operand;
		private const string EMPLOYEERALIAS = "EMPLOYEERALIAS";
		public void Verify(PXCache cache, object item, List<object> pars, ref bool? result, ref object value)
		{
			result = null;
			value = null;
		}

		public bool AppendExpression(ref SQLExpression exp, PXGraph graph, BqlCommandInfo info, BqlCommand.Selection selection)
		{
			bool status = true;

			SQLExpression userID = null;
			status &= BqlCommand.AppendExpression<UserID>(ref userID, graph, info, selection, ref _operand);

			if (graph == null || !info.BuildExpression) return status;
			
			SimpleTable epEmployee = new SimpleTable<EPEmployee>(EMPLOYEERALIAS);

			exp = exp.In(new Query()
				.Select<EPWingman.employeeID>().From<EPWingman>()
					.InnerJoin(epEmployee)
					.On(new Column<EPEmployee.bAccountID>(epEmployee).EQ(new Column(typeof(EPWingman.wingmanID)))
						.And(new Column<EPEmployee.userID>(epEmployee).EQ(userID)))
					.Where(new Column<EPWingman.isActive>()
							.EQ(new SQLConst(1))
						.And(new Column<EPWingman.delegationOf>()
							.EQ(new SQLConst(EPDelegationOf.Expenses)))
					));

			return status;
		}
	}
}
