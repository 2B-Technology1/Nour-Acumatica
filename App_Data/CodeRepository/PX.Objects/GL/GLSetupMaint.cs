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
using System.Linq;
using PX.Data;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.EP;

namespace PX.Objects.GL
{
	public class GLSetupMaint : PXGraph<GLSetupMaint>
	{
		public PXSelect<GLSetup> GLSetupRecord;
		public PXSave<GLSetup> Save;
		public PXCancel<GLSetup> Cancel;
		public PXSetup<Company> company;
		public PXSelect<GLSetupApproval> SetupApproval;

		public GLSetupMaint()
		{
			if (string.IsNullOrEmpty(company.Current.BaseCuryID))
			{
                throw new PXSetupNotEnteredException(ErrorMessages.SetupNotEntered, typeof(Company), PXMessages.LocalizeNoPrefix(CS.Messages.OrganizationMaint));
			}
        }

		#region Actions
		public PXAction<GLSetup> viewAssignmentMap;
		[PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
		[PXButton]
		public virtual IEnumerable ViewAssignmentMap(PXAdapter adapter)
		{
			if (SetupApproval.Current != null)
			{
				PXGraph graph = null;
				GLSetupApproval setupApproval = SetupApproval.Current;
				EPAssignmentMap map = (EPAssignmentMap)PXSelect<EPAssignmentMap,
					Where<EPAssignmentMap.assignmentMapID, Equal<Required<EPAssignmentMap.assignmentMapID>>>>.Select(this, setupApproval.AssignmentMapID).First();
				if (map.MapType == EPMapType.Approval)
				{
					graph = PXGraph.CreateInstance<EPApprovalMapMaint>();
				}
				else if (map.MapType == EPMapType.Assignment)
				{
					graph = PXGraph.CreateInstance<EPAssignmentMapMaint>();
				}
				else if (map.MapType == EPMapType.Legacy && map.AssignmentMapID > 0)
				{
					graph = PXGraph.CreateInstance<EPAssignmentMaint>();
				}
				else
				{
					graph = PXGraph.CreateInstance<EPAssignmentAndApprovalMapEnq>();
				}

				PXRedirectHelper.TryRedirect(graph, map, PXRedirectHelper.WindowMode.NewWindow);
			}
			return adapter.Get();
		}
		#endregion

        #region Events - GLSetup
        protected virtual void GLSetup_BegFiscalYear_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        {
            e.NewValue = Accessinfo.BusinessDate;
        }

        protected virtual void GLSetup_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
        {
            GLSetup OldRow = (GLSetup)PXSelectReadonly<GLSetup>.Select(this);
            GLSetup NewRow = (GLSetup)e.Row;
            if ((OldRow == null || OldRow.COAOrder != NewRow.COAOrder) && NewRow.COAOrder < 4)
            {
                for (short i = 0; i < 4; i++)
                {
                    PXDatabase.Update<Account>(new PXDataFieldAssign("COAOrder", Convert.ToInt32(AccountType.COAOrderOptions[(int)NewRow.COAOrder].Substring((int)i, 1))),
                                                                        new PXDataFieldRestrict("Type", AccountType.Literal(i)));
                    PXDatabase.Update<PM.PMAccountGroup>(new PXDataFieldAssign(typeof(PM.PMAccountGroup.sortOrder).Name, Convert.ToInt32(AccountType.COAOrderOptions[(int)NewRow.COAOrder].Substring((int)i, 1))),
                                                                        new PXDataFieldRestrict(typeof(PM.PMAccountGroup.type).Name, AccountType.Literal(i)));
                }
            }
        }

        protected virtual void GLSetup_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            if (e.Row == null) return;

            GLSetup setup = e.Row as GLSetup;
            bool hasHistory = GLUtility.IsAccountHistoryExist(this, setup.YtdNetIncAccountID);
            PXUIFieldAttribute.SetEnabled<GLSetup.ytdNetIncAccountID>(GLSetupRecord.Cache, setup, !hasHistory);
			bool retEarnHasHistory = GLUtility.IsAccountHistoryExist(this, setup.RetEarnAccountID);
			PXUIFieldAttribute.SetEnabled<GLSetup.retEarnAccountID>(GLSetupRecord.Cache, setup, !retEarnHasHistory);
        }

		protected virtual void GLSetup_AutoReleaseReclassBatch_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
		{
			if ((bool) e.NewValue == true )
			{
				foreach (GLSetupApproval appr in SetupApproval.Select())
				{
					if (appr.BatchType == BatchTypeCode.Reclassification && appr.IsActive == true)
					{
						throw new PXSetPropertyException(Messages.AutoReleaseOnReclassificationApprovalSelected);
					}
				}
			}
		}

		protected virtual void GLSetupApproval_RowPersisting(PXCache sender, PXRowPersistingEventArgs e)
		{
			GLSetupApproval row = (GLSetupApproval)e.Row;
			if (row.IsActive == true && row.BatchType == BatchTypeCode.Reclassification && GLSetupRecord.Current.AutoReleaseReclassBatch == true)
			{
				GLSetupRecord.Cache.RaiseExceptionHandling<GLSetup.autoReleaseReclassBatch>(GLSetupRecord.Current, GLSetupRecord.Current.AutoReleaseReclassBatch,
							new PXSetPropertyException(Messages.AutoReleaseOnReclassificationApprovalSelected, PXErrorLevel.Error));
				throw new PXSetPropertyException(Messages.AutoReleaseOnReclassificationApprovalSelected);
			}
		}

        protected virtual void GLSetup_YtdNetIncAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            GLSetup row = (GLSetup)e.Row;
            if (row == null) return;

            if (e.NewValue != null)
            {
                Account YtdAccNew = PXSelect<Account, Where<Account.accountID, Equal<Required<GLSetup.ytdNetIncAccountID>>>>.Select(this, e.NewValue);
                if ((int?) e.NewValue == row.RetEarnAccountID)
                {
                    Account YtdAcc = PXSelect<Account, Where<Account.accountID, Equal<Current<GLSetup.ytdNetIncAccountID>>>>.SelectSingleBound(this, new object[] {row});
                    Account REAcc = PXSelect<Account, Where<Account.accountID, Equal<Current<GLSetup.retEarnAccountID>>>>.SelectSingleBound(this, new object[] {row});
                    e.NewValue = YtdAcc == null ? null : YtdAcc.AccountCD;
                    throw new PXSetPropertyException(CS.Messages.Entry_NE, REAcc.AccountCD);
                }

				if (YtdAccNew.GLConsolAccountCD != null)
				{
					throw new PXSetPropertyException(Messages.AccountCannotBeSpecifiedAsTheYTDNetIncome);
				}

                if (GLUtility.IsAccountHistoryExist(this, (int?) e.NewValue) || GLUtility.IsAccountGLTranExist(this, (int?)e.NewValue))
                {
                    e.NewValue = YtdAccNew == null ? null : YtdAccNew.AccountCD;
                    throw new PXSetPropertyException(Messages.AccountExistsHistory2);
                }
            }
        }

        protected virtual void GLSetup_RetEarnAccountID_FieldVerifying(PXCache sender, PXFieldVerifyingEventArgs e)
        {
            GLSetup row = (GLSetup)e.Row;
            if (row == null) return;

            if (e.NewValue != null && (int?)e.NewValue == row.YtdNetIncAccountID)
            {
                Account YtdAcc = PXSelect<Account, Where<Account.accountID, Equal<Current<GLSetup.ytdNetIncAccountID>>>>.SelectSingleBound(this, new object[] { row });
                Account REAcc = PXSelect<Account, Where<Account.accountID, Equal<Current<GLSetup.retEarnAccountID>>>>.SelectSingleBound(this, new object[] { row });
                e.NewValue = REAcc == null ? null : REAcc.AccountCD;
                throw new PXSetPropertyException(CS.Messages.Entry_NE, YtdAcc.AccountCD);
            }

            if (e.NewValue != null && GLUtility.IsAccountHistoryExist(this, (int?)e.NewValue))
            {
                sender.RaiseExceptionHandling<GLSetup.retEarnAccountID>(e.Row, null, new PXSetPropertyException(Messages.AccountExistsHistory2, PXErrorLevel.Warning));
            }
        }
		#endregion

		public override void Persist()
        {
            base.Persist();
        }
    }
	
}
