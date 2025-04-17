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
using System.Collections.Generic;
using System.Text;
using PX.Common;
using PX.Data;
using PX.Objects.CS;
using PX.Objects.GL.Consolidation;
using System.Threading.Tasks;

namespace PX.Objects.GL
{
	public class GLConsolSetupMaint : PXGraph<GLConsolSetupMaint>
	{
		public PXSelect<GLSetup> GLSetupRecord;
		public PXSave<GLSetup> Save;
		public PXCancel<GLSetup> Cancel;

		public PXFilteredProcessing<GLConsolSetup, GLSetup> ConsolSetupRecords;
		public PXSelect<Account, Where<Account.gLConsolAccountCD, Equal<Required<Account.gLConsolAccountCD>>>> Accounts;
		public PXSelect<GLConsolLedger, Where<GLConsolLedger.setupID, Equal<Required<GLConsolSetup.setupID>>>> ConsolLedgers;
		public PXSelect<GLConsolBranch, Where<GLConsolBranch.setupID, Equal<Required<GLConsolBranch.setupID>>>> ConsolBranches;

		public PXSetup<GLSetup> glsetup;

		public GLConsolSetupMaint()
		{
			GLSetup setup = glsetup.Current;

			ConsolSetupRecords.SetProcessCaption(Messages.ProcSynchronize);
			ConsolSetupRecords.SetProcessAllCaption(Messages.ProcSynchronizeAll);
			ConsolSetupRecords.SetProcessDelegate<GLConsolSetupMaint>(Synchronize);
			PXUIFieldAttribute.SetEnabled(ConsolSetupRecords.Cache, null, true);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.lastPostPeriod>(ConsolSetupRecords.Cache, null, false);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.lastConsDate>(ConsolSetupRecords.Cache, null, false);
			ConsolSetupRecords.SetAutoPersist(true);
			ConsolSetupRecords.Cache.AllowDelete = true;
			ConsolSetupRecords.Cache.AllowInsert = true;

			PXUIFieldAttribute.SetRequired<GLConsolSetup.segmentValue>(ConsolSetupRecords.Cache, true);
			PXUIFieldAttribute.SetRequired<GLConsolSetup.sourceLedgerCD>(ConsolSetupRecords.Cache, true);

			Save.StateSelectingEvents += new PXFieldSelecting(delegate(PXCache sender, PXFieldSelectingEventArgs e)
			{
				e.ReturnState = PXButtonState.CreateInstance(e.ReturnState, null, null, null, null, null, false,
					PXConfirmationType.Unspecified, null, null, null, null, null, null, null, null, null, null, null, null);
				((PXButtonState)e.ReturnState).Enabled = !PXLongOperation.Exists(this.UID);
			});
		}

		protected static void Synchronize(GLConsolSetupMaint graph, GLConsolSetup consolSetup)
		{
			using (ConsolidationClient scope = new ConsolidationClient(consolSetup.Url, consolSetup.Login, consolSetup.Password, consolSetup.HttpClientTimeout))
			{
				Task<IEnumerable<ConsolAccountAPI>> accountsTask;
				Task<IEnumerable<OrganizationAPI>> organizationsTask;
				Task<IEnumerable<BranchAPI>> branchesTask;
				Task<IEnumerable<LedgerAPI>> ledgersTask;

				try
				{
					accountsTask = Task.Run(() => scope.GetConsolAccounts());
					organizationsTask = Task.Run(() => scope.GetOrganizations());
					branchesTask = Task.Run(() => scope.GetBranches());
					ledgersTask = Task.Run(() => scope.GetLedgers());
					Task.WaitAll(accountsTask, branchesTask, ledgersTask);
				}
				catch(Exception ex)
				{
					var error = ConsolidationClient.GetServerError(ex);
					throw new PXException(error);
				}

				var remoteConsolAccounts = accountsTask.Result;
				var remoteOrganizations = organizationsTask.Result;
				var remoteBranches = branchesTask.Result;
				var remoteLedgers = ledgersTask.Result;

				PXSelect<Account> select = new PXSelect<Account>(graph);
				List<Account> localAccounts = new List<Account>();
				foreach (Account acct in select.Select())
				{
					localAccounts.Add(acct);
				}

				List<GLConsolLedger> localConsLedgers = new List<GLConsolLedger>();
				foreach (GLConsolLedger ledger in graph.ConsolLedgers.Select(consolSetup.SetupID))
				{
					localConsLedgers.Add(ledger);
				}

				List<GLConsolBranch> localConsBranches = new List<GLConsolBranch>();
				foreach (GLConsolBranch branch in graph.ConsolBranches.Select(consolSetup.SetupID))
				{
					localConsBranches.Add(branch);
				}

				foreach (ConsolAccountAPI remoteAccount in remoteConsolAccounts)
				{
					Account acct = new Account();
					acct.AccountCD = remoteAccount.AccountCD;
					object value = acct.AccountCD;
					select.Cache.RaiseFieldUpdating<Account.accountCD>(acct, ref value);
					acct.AccountCD = (string)value;

					acct = select.Locate(acct);
					if (acct == null)
					{
						try
						{
							Task.Run(() => scope.DeleteConsolAccount(remoteAccount)).Wait();
						}
						catch(Exception ex)
						{
							var error = ConsolidationClient.GetServerError(ex);
							throw new PXException(error);
						}
					}
					else
					{
						localAccounts.Remove(acct);
						if (acct.Description != remoteAccount.Description)
						{
							remoteAccount.Description = acct.Description;
							try
							{
								Task.Run(() => scope.UpdateConsolAccount(remoteAccount)).Wait();
							}
							catch (Exception ex)
							{
								var error = ConsolidationClient.GetServerError(ex);
								throw new PXException(error);
							}
						}
					}
				}

				foreach (LedgerAPI remoteLedger in remoteLedgers)
				{
					GLConsolLedger ledger = new GLConsolLedger();
					ledger.SetupID = consolSetup.SetupID;
					ledger.LedgerCD = remoteLedger.LedgerCD;
					ledger = graph.ConsolLedgers.Locate(ledger);
					if (ledger != null)
					{
						localConsLedgers.Remove(ledger);
						if (ledger.Description != remoteLedger.Descr || ledger.BalanceType != remoteLedger.BalanceType)
						{
							ledger.Description = remoteLedger.Descr;
							ledger.BalanceType = remoteLedger.BalanceType;
							graph.ConsolLedgers.Cache.SetStatus(ledger, PXEntryStatus.Updated);
						}
					}
					else
					{
						ledger = new GLConsolLedger();
						ledger.SetupID = consolSetup.SetupID;
						ledger.LedgerCD = remoteLedger.LedgerCD;
						ledger.Description = remoteLedger.Descr;
						ledger.BalanceType = remoteLedger.BalanceType;
						graph.ConsolLedgers.Insert(ledger);
					}
				}

				foreach (GLConsolLedger ledger in localConsLedgers)
				{
					graph.ConsolLedgers.Delete(ledger);
					if (consolSetup.SourceLedgerCD == ledger.LedgerCD)
					{
						consolSetup.SourceLedgerCD = null;
						graph.ConsolSetupRecords.Update(consolSetup);
					}
				}

				foreach (OrganizationAPI org in remoteOrganizations)
				{
					GLConsolBranch localBranch = new GLConsolBranch();
					localBranch.SetupID = consolSetup.SetupID;
					localBranch.BranchCD = org.OrganizationCD;
					localBranch = graph.ConsolBranches.Locate(localBranch);
					if (localBranch != null)
					{
						localConsBranches.Remove(localBranch);

						if (localBranch.Description != org.OrganizationName
							|| localBranch.LedgerCD != org.LedgerCD
							|| localBranch.OrganizationCD != org.OrganizationCD
							|| localBranch.IsOrganization != true)
						{
							localBranch.OrganizationCD = org.OrganizationCD;
							localBranch.LedgerCD = org.LedgerCD;
							localBranch.Description = org.OrganizationName;
							localBranch.IsOrganization = true;
							graph.ConsolBranches.Cache.SetStatus(localBranch, PXEntryStatus.Updated);
						}
					}
					else
					{
						localBranch = new GLConsolBranch();
						localBranch.SetupID = consolSetup.SetupID;
						localBranch.BranchCD = org.OrganizationCD;
						localBranch.OrganizationCD = org.OrganizationCD;
						localBranch.LedgerCD = org.LedgerCD;
						localBranch.Description = org.OrganizationName;
						localBranch.IsOrganization = true;
						graph.ConsolBranches.Insert(localBranch);
					}
				}

				foreach (BranchAPI branch in remoteBranches)
				{
					GLConsolBranch localBranch = new GLConsolBranch();
					localBranch.SetupID = consolSetup.SetupID;
					localBranch.BranchCD = branch.BranchCD;
					localBranch = graph.ConsolBranches.Locate(localBranch);
					if (localBranch != null)
					{
						localConsBranches.Remove(localBranch);

						if (localBranch.Description != branch.AcctName || localBranch.LedgerCD != branch.LedgerCD || localBranch.OrganizationCD != branch.OrganizationCD)
						{
							localBranch.OrganizationCD = branch.OrganizationCD;
							localBranch.LedgerCD = branch.LedgerCD;
							localBranch.Description = branch.AcctName;
							graph.ConsolBranches.Cache.SetStatus(localBranch, PXEntryStatus.Updated);
						}
					}
					else
					{
						localBranch = new GLConsolBranch();
						localBranch.SetupID = consolSetup.SetupID;
						localBranch.BranchCD = branch.BranchCD;
						localBranch.OrganizationCD = branch.OrganizationCD;
						localBranch.LedgerCD = branch.LedgerCD;
						localBranch.Description = branch.AcctName;
						graph.ConsolBranches.Insert(localBranch);
					}
				}

				foreach (GLConsolBranch branch in localConsBranches)
				{
					graph.ConsolBranches.Delete(branch);
					if (consolSetup.SourceBranchCD == branch.BranchCD)
					{
						consolSetup.SourceBranchCD = null;
						graph.ConsolSetupRecords.Update(consolSetup);
					}
				}

				foreach (Account acct in localAccounts)
				{
					var consol = new ConsolAccountAPI();
					consol.AccountCD = acct.AccountCD;
					consol.Description = acct.Description;
					try
					{
						Task.Run(() => scope.InsertConsolAccount(consol)).Wait();
					}
					catch (Exception ex)
					{
						var error = ConsolidationClient.GetServerError(ex);
						throw new PXException(error);
					}
				}
			}

			graph.Save.Press();
		}

		#region GLSetupEventHandlers

		protected virtual void GLSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			if (e.Row == null) 
				return;

			GLSetup setup = (GLSetup)e.Row;

			var consColVisible = setup.ConsolSegmentId != null;

			PXUIFieldAttribute.SetEnabled<GLConsolSetup.segmentValue>(ConsolSetupRecords.Cache, null, consColVisible);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.pasteFlag>(ConsolSetupRecords.Cache, null, consColVisible);

			PXUIFieldAttribute.SetVisible<GLConsolSetup.segmentValue>(ConsolSetupRecords.Cache, null, consColVisible);
			PXUIFieldAttribute.SetVisible<GLConsolSetup.pasteFlag>(ConsolSetupRecords.Cache, null, consColVisible);

			PXDefaultAttribute.SetPersistingCheck<GLConsolSetup.segmentValue>(ConsolSetupRecords.Cache, null,
				consColVisible ? PXPersistingCheck.NullOrBlank : PXPersistingCheck.Nothing);
		}

		protected virtual void GLSetup_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			foreach (GLConsolSetup detail in ConsolSetupRecords.Select())
			{
				ConsolSetupRecords.Cache.MarkUpdated(detail);
			}
		}

		protected virtual void GLSetup_ConsolSegmentId_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			var glSetup = (GLSetup) e.Row;

			if (glSetup.ConsolSegmentId == null)
			{
				var glConsolSetupRows = ConsolSetupRecords
					.Select()
					.RowCast<GLConsolSetup>();

				glConsolSetupRows.ForEach(row =>
				{
					row.PasteFlag = false;
				});
			}
		}

		#endregion
		#region GLConsolSetupEbentHandlers
		protected virtual void GLConsolSetup_BranchID_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			sender.SetDefaultExt<GLConsolSetup.ledgerId>(e.Row);
		}

		protected virtual void GLConsolSetup_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
		{
			GLConsolSetup setup = (GLConsolSetup)e.Row;
			if (setup != null)
			{
				PXUIFieldAttribute.SetEnabled<GLConsolSetup.selected>(sender, setup, setup.IsActive == true);
			}

			GLConsolLedger coledger = PXSelect<GLConsolLedger, Where<GLConsolLedger.setupID, Equal<Current<GLConsolSetup.setupID>>, And<GLConsolLedger.ledgerCD, Equal<Current<GLConsolSetup.sourceLedgerCD>>>>>.Select(this);
			PXUIFieldAttribute.SetEnabled<GLConsolSetup.sourceBranchCD>(sender, e.Row, coledger != null);
		}

		protected virtual void GLConsolSetup_RowUpdated(PXCache sender, PXRowUpdatedEventArgs e)
		{
			GLConsolSetup setup = (GLConsolSetup)e.Row;
			if (!sender.ObjectsEqual<GLConsolSetup.sourceLedgerCD>(e.Row, e.OldRow))
			{
				string oldbranchid = (string)sender.GetValue<GLConsolSetup.sourceBranchCD>(e.Row);
				sender.SetValue<GLConsolSetup.sourceBranchCD>(e.Row, null);

				sender.SetValueExt<GLConsolSetup.sourceBranchCD>(e.Row, oldbranchid);
			}
		}

		protected virtual void GLConsolSetup_RowUpdating(PXCache sender, PXRowUpdatingEventArgs e)
		{
			var setup = (GLConsolSetup)e.NewRow;
			
			if (setup.PasteFlag == true && string.IsNullOrEmpty(setup.SegmentValue))
			{
				sender.RaiseExceptionHandling<GLConsolSetup.segmentValue>(e.NewRow, setup.SegmentValue,
					new PXSetPropertyException(Messages.ConsolidationSegmentValueMayNotBeEmpty));
				e.Cancel = true;
			}
		}

		protected virtual void GLConsolSetup_IsActive_FieldUpdated(PXCache sender, PXFieldUpdatedEventArgs e)
		{
			GLConsolSetup setup = (GLConsolSetup)e.Row;
			if (setup != null && setup.IsActive == false)
			{
				setup.Selected = false;
			}
		}

		protected virtual void GLConsolSetup_Url_FieldUpdating(PXCache sender, PXFieldUpdatingEventArgs e)
		{
			string value = e.NewValue as string;
			if (!String.IsNullOrEmpty(value))
			{
				string url = PXUrl.IngoreAllQueryParameters(value);
				string mainPagePath = PXUrl.MainPagePath.TrimStart('~');
				if (url.EndsWith(mainPagePath, StringComparison.OrdinalIgnoreCase))
					e.NewValue = url.Substring(0, url.Length - mainPagePath.Length);
			}
		}
		#endregion
	}
}
