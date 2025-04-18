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
using System.Collections;
using PX.Data;
using PX.TM;
using PX.Data.EP;

namespace PX.Objects.EP
{
	public class ExpenseClaimMaint : PXGraph<ExpenseClaimMaint>
	{
		public class ExpenseClaimMaintReceiptExt : ExpenseClaimDetailGraphExtBase<ExpenseClaimMaint>
		{
			public override PXSelectBase<EPExpenseClaimDetails> Receipts => Base.Details;

			public override PXSelectBase<EPExpenseClaim> Claim => Base.Claim;
		}

		[PXSelector(typeof(PX.SM.Users.pKID), SubstituteKey = typeof(PX.SM.Users.fullName), DescriptionField = typeof(PX.SM.Users.fullName), CacheGlobal = true)]
		[PXDBGuid(false)]
		[PXUIField(DisplayName = "Created By", Enabled = false)]
		protected virtual void EPExpenseClaim_CreatedByID_CacheAttached(PXCache sender) { }

		#region Select

		public PXFilter<ExpenseClaimFilter> Filter;
		[PXFilterable]
		public PXSelectJoin<EPExpenseClaim,
							LeftJoin<EPEmployee, On<EPEmployee.bAccountID, Equal<EPExpenseClaim.employeeID>>>,
								Where<Current2<ExpenseClaimFilter.employeeID>, IsNotNull,
									And<EPExpenseClaim.employeeID, Equal<Current2<ExpenseClaimFilter.employeeID>>,
									Or<Current2<ExpenseClaimFilter.employeeID>, IsNull,
									And<Where<EPEmployee.defContactID, Equal<Current<AccessInfo.contactID>>,
									Or<EPExpenseClaim.createdByID, Equal<Current<AccessInfo.userID>>,
									Or<EPExpenseClaim.employeeID, WingmanUser<Current<AccessInfo.userID>>,
									Or<EPEmployee.defContactID, IsSubordinateOfContact<Current<AccessInfo.contactID>>,
									Or<EPExpenseClaim.noteID, Approver<Current<AccessInfo.contactID>>>>>>>>>>>,
								OrderBy<Desc<EPExpenseClaim.refNbr>>> Claim;
		public PXSelect<EPExpenseClaimDetails, Where<EPExpenseClaimDetails.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>> Details;
		#endregion

		public ExpenseClaimMaint()
		{
			Claim.View.IsReadOnly = true;
			PXUIFieldAttribute.SetVisible<EPExpenseClaim.branchID>(Claim.Cache, null, false);
		}

		#region Action

		public PXSave<EPExpenseClaim> Save;

		public PXAction<ExpenseClaimFilter> createNew;
		[PXInsertButton]
		[PXUIField(DisplayName = "")]
		[PXEntryScreenRights(typeof(EPExpenseClaim), nameof(ExpenseClaimEntry.Insert))]
		protected virtual void CreateNew()
		{
			using (new PXPreserveScope())
			{
				ExpenseClaimEntry graph = (ExpenseClaimEntry)PXGraph.CreateInstance(typeof(ExpenseClaimEntry));
				graph.Clear(PXClearOption.ClearAll);
				EPExpenseClaim claim = (EPExpenseClaim)graph.ExpenseClaim.Cache.CreateInstance();
				graph.ExpenseClaim.Insert(claim);
				graph.ExpenseClaim.Cache.IsDirty = false;
				PXRedirectHelper.TryRedirect(graph, PXRedirectHelper.WindowMode.InlineWindow);
			}
		}

		public PXAction<ExpenseClaimFilter> EditDetail;
		[PXEditDetailButton]
		[PXUIField(DisplayName = "", MapEnableRights = PXCacheRights.Select)]
		protected virtual void editDetail()
		{
			if (Claim.Current == null) return;
			EPExpenseClaim row = PXSelect<EPExpenseClaim, Where<EPExpenseClaim.refNbr, Equal<Required<EPExpenseClaim.refNbr>>>>.SelectSingleBound(this, null, Claim.Current.RefNbr);
			PXRedirectHelper.TryRedirect(this, row, PXRedirectHelper.WindowMode.InlineWindow);
		}

		public PXAction<ExpenseClaimFilter> delete;
		[PXDeleteButton]
		[PXUIField(DisplayName = "")]
		[PXEntryScreenRights(typeof(EPExpenseClaim))]
		protected void Delete()
		{
			if (Claim.Current == null) return;

			if (Claim.Current.Released == true)
				throw new PXException(Messages.ReleasedDocumentMayNotBeDeleted);

			ExpenseClaimEntry graph = (ExpenseClaimEntry)PXGraph.CreateInstance(typeof(ExpenseClaimEntry));
			graph.Clear(PXClearOption.ClearAll);
			graph.ExpenseClaim.Current = graph.ExpenseClaim.Search<EPExpenseClaim.refNbr>(Claim.Current.RefNbr);
			graph.Delete.Press();
		}

		public PXAction<ExpenseClaimFilter> submit;
		[PXButton]
		[PXUIField(DisplayName = Messages.Submit)]
		[PXEntryScreenRights(typeof(EPExpenseClaim))]
		protected void Submit()
		{
			if (Claim.Current != null)
			{
				ExpenseClaimEntry graph = (ExpenseClaimEntry)PXGraph.CreateInstance(typeof(ExpenseClaimEntry));
				graph.Clear(PXClearOption.ClearAll);
				graph.ExpenseClaim.Current = graph.ExpenseClaim.Search<EPExpenseClaim.refNbr>(Claim.Current.RefNbr);
				graph.submit.Press();
			}
		}
		#endregion

		[Serializable]
		[PXHidden]
		public partial class ExpenseClaimFilter : IBqlTable
		{
			private int? _employeeId;

			#region EmployeeID
			public abstract class employeeID : PX.Data.BQL.BqlInt.Field<employeeID> { }

			[PXDBInt]
			[PXUIField(DisplayName = "Employee")]
			[PXDefault(typeof(Search<EPEmployee.bAccountID, Where<EPEmployee.userID, Equal<Current<AccessInfo.userID>>>>), PersistingCheck = PXPersistingCheck.Nothing)]
			[PXSubordinateAndWingmenSelector()]
			[PXFieldDescription]
			public virtual Int32? EmployeeID
			{
				get { return _employeeId; }
				set { _employeeId = value; }
			}

			#endregion
		}

		protected virtual void EPExpenseClaimDetails_RowDeleting(PXCache cache, PXRowDeletingEventArgs e)
		{
			EPExpenseClaimDetails detail = e.Row as EPExpenseClaimDetails;
			if (detail != null)
			{
				FindImplementation<ExpenseClaimMaintReceiptExt>().RemoveReceipt(Details.Cache, detail);
				e.Cancel = true;
			}
		}
	}
}