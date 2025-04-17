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
using PX.Objects.AR;
using PX.Objects.CN.Common.Services;
using PX.Objects.CN.ProjectAccounting.Descriptor;
using PX.Objects.CN.ProjectAccounting.PM.Descriptor;
using PX.Objects.CS;
using PX.Objects.PM;
using System;
using System.Collections;

namespace PX.Objects.CN.ProjectAccounting.AR.GraphExtensions
{
	public class ArInvoiceEntryExt : PXGraphExtension<ARInvoiceEntry>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.construction>() && !SiteMapExtension.IsInvoicesScreenId();
		}

		[PXMergeAttributes(Method = MergeMethod.Append)]
		[PXRestrictor(typeof(Where<PMTask.type, NotEqual<ProjectTaskType.cost>>),
			ProjectAccountingMessages.TaskTypeIsNotAvailable, typeof(PMTask.type))]
		protected virtual void _(Events.CacheAttached<ARTran.taskID> e)
		{
		}

		protected virtual void _(Events.RowUpdated<ARTran> args)
		{
			if (args.Row is ARTran line)
			{
				object taskId = line.TaskID;
				try
				{
					args.Cache.RaiseFieldVerifying<ARTran.taskID>(line, ref taskId);
				}
				catch (PXSetPropertyException)
				{
					line.TaskID = null;
				}
			}
		}

		protected virtual PMProforma GetPMProformaOfCurrentARInvoice()
			=> PXSelect<PMProforma, Where<PMProforma.aRInvoiceDocType, Equal<Current<ARInvoice.docType>>,
									And<PMProforma.aRInvoiceRefNbr, Equal<Current<ARInvoice.refNbr>>>>>.Select(Base);

		[PXOverride]
		public virtual IEnumerable PayInvoice(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
		{
			if (Base.Document.Current != null && Base.Document.Current.Released == true)
			{
				if (Base.Document.Current.ProformaExists == true)
				{
					PMProforma proforma = GetPMProformaOfCurrentARInvoice();

					if (proforma != null && proforma.Corrected == true)
					{
						throw new PXSetPropertyException(PX.Objects.PM.Messages.CannotPreparePayment, Base.Document.Current.RefNbr, proforma.RefNbr);
					}
				}
			}

			return baseHandler(adapter);
		}

		[PXOverride]
		public virtual IEnumerable ReverseInvoice(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
		{
			if (Base.Document.Current != null)
			{
				if (Base.Document.Current.ProformaExists == true)
				{
					PMProforma proforma = GetPMProformaOfCurrentARInvoice();

					if (!AskUserApprovalIfProformaExistAndClosed(proforma, Base.Document.Current))
					{
						return adapter.Get();
					}

					if (proforma != null && proforma.Corrected == true)
					{
						if (proforma.Status == ProformaStatus.Closed)
						{
							if (Base.AskUserApprovalIfReversingDocumentAlreadyExists(Base.Document.Current))
							{
								return baseHandler(adapter);
							}
							else
							{
								return adapter.Get();
							}
						}

						throw new PXSetPropertyException(PX.Objects.PM.Messages.CannotReverseInvoice, Base.Document.Current.RefNbr, proforma.RefNbr);
					}
				}
			}

			return baseHandler(adapter);
		}

		[PXOverride]
		public virtual IEnumerable ReverseInvoiceAndApplyToMemo(PXAdapter adapter, Func<PXAdapter, IEnumerable> baseHandler)
		{
			if (Base.Document.Current != null)
			{
				if (Base.Document.Current.ProformaExists == true)
				{
					PMProforma proforma = GetPMProformaOfCurrentARInvoice();

					if (proforma != null && proforma.Corrected == true)
					{
						throw new PXSetPropertyException(PX.Objects.PM.Messages.CannotReverseInvoice, Base.Document.Current.RefNbr, proforma.RefNbr);
					}
					else
					{
						if (!AskUserApprovalIfProformaExistAndClosed(proforma, Base.Document.Current))
						{
							return adapter.Get();
						}
					}
				}
			}

			return baseHandler(adapter);
		}

		protected virtual bool AskUserApprovalIfProformaExistAndClosed(PMProforma proforma, ARInvoice invoice)
		{
			if (proforma?.Status != ProformaStatus.Closed || invoice.Released != true)
			{
				return true;
			}

			string localizedHeader = PX.Objects.PM.Messages.InvoiceWithProformaReverseDialogHeader;

			string localizedMessage = PXMessages.LocalizeFormatNoPrefix(
				PX.Objects.PM.Messages.InvoiceWithProformaReverseWarning, proforma.RefNbr);

			return Base.Document.View.Ask(null, localizedHeader, localizedMessage, MessageButtons.YesNo, MessageIcon.Warning) == WebDialogResult.Yes;
		}
	}
}
