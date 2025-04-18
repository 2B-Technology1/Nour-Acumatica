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
using System.Runtime.Serialization;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.CM;
using PX.Objects.CR;
using PX.Objects.CS;
using PX.Objects.GL.Attributes;
using PX.Objects.PM;

namespace PX.Objects.GL.Reclassification.Common
{
	[PXBreakInheritance]
	public class GLTranForReclassification : GLTran
	{
		public GLTranForReclassification()
		{
			FieldsErrorForInvalidFromValues = new Dictionary<string, ExceptionAndErrorValuesTriple>(4);
		}

		public new abstract class branchID : PX.Data.BQL.BqlInt.Field<branchID> { }

		#region SplittedIcon

		public abstract class splittedIcon : PX.Data.BQL.BqlString.Field<splittedIcon> { }

        [PXUIField(DisplayName = "", IsReadOnly = true, Visible = false)]
        [PXImage]
        public virtual string SplittedIcon { get; set; }
        #endregion
        #region DebitAmt

        [PXDBBaseCury(typeof(GLTran.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? DebitAmt
		{
			get
			{
				return this._DebitAmt;
			}
			set
			{
				this._DebitAmt = value;
			}
		}
		#endregion
		#region CreditAmt
		[PXDBBaseCury(typeof(GLTran.ledgerID))]
		[PXDefault(TypeCode.Decimal, "0.0")]
		public override Decimal? CreditAmt
		{
			get
			{
				return this._CreditAmt;
			}
			set
			{
				this._CreditAmt = value;
			}
		}
		#endregion
		#region CuryDebitAmt
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Debit Amount", Visibility = PXUIVisibility.Visible)]
		[PXDBCurrency(typeof(GLTran.curyInfoID), typeof(GLTran.debitAmt))]
		public override Decimal? CuryDebitAmt
		{
			get
			{
				return this._CuryDebitAmt;
			}
			set
			{
				this._CuryDebitAmt = value;
			}
		}
		#endregion
		#region CuryCreditAmt
		[PXDefault(TypeCode.Decimal, "0.0")]
		[PXUIField(DisplayName = "Credit Amount", Visibility = PXUIVisibility.Visible)]
		[PXDBCurrency(typeof(GLTran.curyInfoID), typeof(GLTran.creditAmt))]
		public override Decimal? CuryCreditAmt
		{
			get
			{
				return this._CuryCreditAmt;
			}
			set
			{
				this._CuryCreditAmt = value;
			}
		}
		#endregion
		#region LedgerID

		[PXDBInt]
		[PXDefault]
		public override Int32? LedgerID
		{
			get
			{
				return this._LedgerID;
			}
			set
			{
				this._LedgerID = value;
			}
		}

		#endregion
		#region ReferenceID
		public new abstract class referenceID : PX.Data.BQL.BqlInt.Field<referenceID> { }

		[PXDBInt()]
		[PXSelector(typeof(Search<BAccountR.bAccountID>),
			 typeof(BAccountR.bAccountID),
			 typeof(BAccountR.acctName),
			 typeof(BAccountR.type),
			SubstituteKey = typeof(BAccountR.acctCD))]
		[CustomerVendorRestrictor]
		[PXUIField(DisplayName = "Customer/Vendor", Enabled = false, Visible = false)]
		public override Int32? ReferenceID
		{
			get
			{
				return this._ReferenceID;
			}
			set
			{
				this._ReferenceID = value;
			}
		}
		#endregion
		#region NewBranchID

		public abstract class newBranchID : PX.Data.BQL.BqlInt.Field<newBranchID> { }

		protected Int32? _NewBranchID;

		[Branch(sourceType: typeof(GLTranForReclassification.branchID),  useDefaulting: false, DisplayName = "To Branch", IsDBField = false)]
		public virtual Int32? NewBranchID
		{
			get { return this._NewBranchID; }
			set { this._NewBranchID = value; }
		}

		#endregion
		#region NewAccountID

		public abstract class newAccountID : PX.Data.BQL.BqlInt.Field<newAccountID> { }

		protected Int32? _NewAccountID;

		[Account(typeof(newBranchID),
			LedgerID = typeof(ledgerID),
			DescriptionField = typeof(Account.description),
			DisplayName = "To Account", IsDBField = false,
			AvoidControlAccounts = true)]
		public virtual Int32? NewAccountID
		{
			get { return this._NewAccountID; }
			set { this._NewAccountID = value; }
		}

		#endregion
		#region NewSubID

		public abstract class newSubID : PX.Data.BQL.BqlInt.Field<newSubID> { }

		protected Int32? _NewSubID;

		[SubAccount(typeof(GLTranForReclassification.newAccountID), typeof(GLTranForReclassification.newBranchID),
			DisplayName = "To Subaccount", IsDBField = false)]
		public virtual Int32? NewSubID
		{
			get { return this._NewSubID; }
			set { this._NewSubID = value; }
		}

        #endregion
        #region NewSubID
        public virtual string NewSubCD { get; set; }
        #endregion
        #region NewTranDate

        public abstract class newTranDate : PX.Data.BQL.BqlDateTime.Field<newTranDate> { }

		protected DateTime? _NewTranDate;

		[PXDate]
		[PXUIField(DisplayName = "New Tran. Date")]
		public virtual DateTime? NewTranDate
		{
			get { return this._NewTranDate; }
			set { this._NewTranDate = value; }
		}

		#endregion
		#region NewFinPeriodID

		public abstract class newFinPeriodID : PX.Data.BQL.BqlString.Field<newFinPeriodID> { }

		protected String _NewFinPeriodID;

		//Used only for validation
		[OpenPeriod(null, 
		    typeof(GLTranForReclassification.newTranDate), 
		    typeof(GLTranForReclassification.newBranchID), 
		    IsDBField = false, 
			RedefaultOrRevalidateOnOrganizationSourceUpdated = false,
		    RedefaultOnDateChanged = false)]
		public virtual String NewFinPeriodID
		{
			get { return this._NewFinPeriodID; }
			set { this._NewFinPeriodID = value; }
		}

		#endregion
		#region NewTranDesc
		public abstract class newTranDesc : PX.Data.BQL.BqlString.Field<newTranDesc> { }
		protected String _NewTranDesc;

		[PXString(256, IsUnicode = true)]
		[PXUIField(DisplayName = "New Transaction Description")]
		public virtual String NewTranDesc
		{
			get
			{
				return this._NewTranDesc;
			}
			set
			{
				this._NewTranDesc = value;
			}
		}
		#endregion

		#region NewProjectID
		public abstract class newProjectID : PX.Data.BQL.BqlInt.Field<newProjectID> { }

		[GLProjectDefault(typeof(GLTran.ledgerID), AccountType = typeof(newAccountID), PersistingCheck = PXPersistingCheck.Nothing)]
		[ActiveProject(AccountFieldType = typeof(newAccountID), IsDBField = false, DisplayName = "To Project")]
		public virtual int? NewProjectID
		{
			get;
			set;
		}
		#endregion
		#region NewTaskID
		public abstract class newTaskID : PX.Data.BQL.BqlInt.Field<newTaskID> { }

		[PXDefault(typeof(Search<PMTask.taskID, Where<PMTask.projectID, Equal<Current<newProjectID>>, And<PMTask.isDefault, Equal<True>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
		[BaseProjectTask(typeof(newProjectID), BatchModule.GL, IsDBField = false, DisplayName = "To Project Task", AllowInactive = false)]
		[PXUIEnabled(typeof(Where<FeatureInstalled<FeaturesSet.projectAccounting>>))]
		[PXUIVisible(typeof(Where<FeatureInstalled<FeaturesSet.projectAccounting>>))]
		public virtual int? NewTaskID
		{
			get;
			set;
		}
		#endregion
		#region NewCostCodeID
		public abstract class newCostCodeID : PX.Data.BQL.BqlInt.Field<newCostCodeID> { }

		[PXForeignReference(typeof(Field<costCodeID>.IsRelatedTo<PMCostCode.costCodeID>))]
		[CostCode(typeof(newAccountID), typeof(newTaskID), ReleasedField = typeof(released), DisplayName = "To Cost Code", IsDBField = false,
			Visibility = PXUIVisibility.SelectorVisible, DescriptionField = typeof(PMCostCode.description))]
		[PXUIEnabled(typeof(Where<FeatureInstalled<FeaturesSet.costCodes>>))]
		[PXUIVisible(typeof(Where<FeatureInstalled<FeaturesSet.costCodes>>))]
		public virtual int? NewCostCodeID
		{
			get;
			set;
		}
		#endregion

		#region  CuryNewAmt
		public abstract class curyNewAmt : PX.Data.BQL.BqlDecimal.Field<curyNewAmt> { }

        [PXDefault]
        [PXCurrency(typeof(GLTran.curyInfoID), typeof(newAmt))]
        [PXUIField(DisplayName = "New Amount", Visible = false)]
        public virtual Decimal? CuryNewAmt { get; set; }
        #endregion
        #region  NewAmt
        public abstract class newAmt : PX.Data.BQL.BqlDecimal.Field<newAmt> { }

        [PXBaseCury(typeof(GLTran.ledgerID))]
        public virtual Decimal? NewAmt { get; set; }
        #endregion
        #region CuryID
        public abstract class curyID : PX.Data.BQL.BqlString.Field<curyID> { }
		protected String _CuryID;

		[PXString]
		[PXUIField(DisplayName = "Currency")]
		public virtual String CuryID
		{
			get
			{
				return this._CuryID;
			}
			set
			{
				this._CuryID = value;
			}
		}
        #endregion
        #region SortOrder
        public abstract class sortOrder : PX.Data.BQL.BqlInt.Field<sortOrder> { }

        [PXInt]
        [PXDefault]
        public virtual int? SortOrder
        {
            get;
            set;
        }
        #endregion
        #region SourceCuryDebitAmt
        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? SourceCuryDebitAmt { get; set; }
        #endregion
        #region SourceCuryCreditAmt
        [PXDecimal]
        [PXDefault(TypeCode.Decimal, "0.0")]
        public virtual decimal? SourceCuryCreditAmt { get; set; }
        #endregion

        public virtual ReclassRowTypes ReclassRowType { get; set; }
		public virtual int? EditingPairReclassifyingLineNbr { get; set; }
		
		public virtual bool VerifyingForFromValuesInvoked { get; set; }
		public virtual Dictionary<string, ExceptionAndErrorValuesTriple> FieldsErrorForInvalidFromValues { get; set; }

        public virtual GLTranKey ParentKey { get; set; }

        public bool IsSplitting => ParentKey != null;

        /// <summary>
        /// This field is used for UI only.
        /// </summary>
        public bool IsSplitted { get; set; }

		public class ExceptionAndErrorValuesTriple
		{
			public Exception Error;
			public object ErrorValue;
			public object ErrorUIValue;
		}
    }
}
