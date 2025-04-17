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
using PX.Objects.CA;
using PX.Objects.Common;
using PX.Objects.CS;
using System;
using System.Linq;

namespace PX.Objects.PR
{
	/// <summary>
	/// Payroll Module's extension of the CATran DAC.
	/// </summary>
	[PXPrimaryGraph(new Type[] {
						typeof(PRPayChecksAndAdjustments),
						typeof(PRDirectDepositBatchEntry),
						},
					new Type[] {
						typeof(Select<PRPayment, Where<PRPayment.caTranID, Equal<Current<CATran.tranID>>>>),
						typeof(Select<PRCABatch, Where<PRCABatch.origModule, Equal<Current<CATran.origModule>>,
								And<PRCABatch.batchNbr, Equal<Current<CATran.origRefNbr>>,
								And<PRCABatch.origModule, Equal<GL.BatchModule.modulePR>>>>>)
					})]
	public class PRxCATran : PXCacheExtension<CATran>
	{
		public static bool IsActive()
		{
			return PXAccess.FeatureInstalled<FeaturesSet.payrollModule>();
		}

		#region OrigTranType
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXDBString(3, IsFixed = true)]
		[PXRemoveBaseAttribute(typeof(CAAPARTranType.ListByModuleAttribute))]
		[PRCATranType.ListByModule(typeof(CATran.origModule))]
		public virtual string OrigTranType { get; set; }
		#endregion

		#region OrigTranTypeUI
		/// <summary>
		/// The CATran type.
		/// </summary>
		/// <value>
		/// The field can have one of the values described in <see cref="PRCATranType.ListAttribute"/>.
		/// </value>
		[PXMergeAttributes(Method = MergeMethod.Merge)]
		[PXString]
		[PXRemoveBaseAttribute(typeof(CAAPARTranType.ListByModuleUIAttribute))]
		[PRCATranType.ListByModuleUI(typeof(CATran.origModule))]
		public virtual string OrigTranTypeUI { get; set; }
		#endregion
	}

	public class PRCATranType
	{
		public class ListByModuleAttribute : CAAPARTranType.ListByModuleAttribute
		{
			private readonly ValueLabelList _listPRRestricted;
			private readonly ValueLabelList _listPRFull;

			public ListByModuleAttribute(Type moduleField) : base(moduleField)
			{
				_listPRRestricted = new ValueLabelList
				{
					{PayrollType.Regular, Messages.Regular},
					{PayrollType.Special, Messages.Special},
					{PayrollType.Adjustment, Messages.Adjustment},
					{CATranType.CABatch, Messages.DirectDepositBatch},
				};

				_listPRFull = new ValueLabelList(_listPRRestricted) {{PayrollType.VoidCheck, Messages.VoidCheck}};
				_listAll = new ValueLabelList(_listAll) {_listPRRestricted};
			}

			public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			{
				string module = (string)sender.GetValue(e.Row, _ModuleField.Name);
				if (module == GL.BatchModule.PR && module != lastModule && sender.Graph.GetType() != typeof(PXGraph))
				{
					SetNewList(sender, _listPRFull);
					lastModule = module;
				}

				base.FieldSelecting(sender, e);
			}

			protected override void TryLocalize(PXCache sender)
			{
				base.TryLocalize(sender);
				RipDynamicLabels(_listPRFull.Select(pair => pair.Label).ToArray(), sender);
			}
		}

		public class ListByModuleUIAttribute : CAAPARTranType.ListByModuleUIAttribute
		{
			private readonly ValueLabelList _listPRFull;

			public ListByModuleUIAttribute(Type moduleField) : base(moduleField)
			{
				_listPRFull = new ValueLabelList
				{
					{PayrollType.Regular, Messages.Regular},
					{PayrollType.Special, Messages.Special},
					{PayrollType.Adjustment, Messages.Adjustment},
					{CATranType.CABatch, Messages.DirectDepositBatch},
					{PayrollType.VoidPayment, Messages.VoidCheck}
				};
			}

			public override void FieldSelecting(PXCache sender, PXFieldSelectingEventArgs e)
			{
				string module = (string)sender.GetValue(e.Row, _ModuleField.Name);
				if (module == GL.BatchModule.PR && module != lastModule && sender.Graph.GetType() != typeof(PXGraph))
				{
					SetNewList(sender, _listPRFull);
					lastModule = module;
				}
				base.FieldSelecting(sender, e);
			}

			protected override void TryLocalize(PXCache sender)
			{
				base.TryLocalize(sender);
				RipDynamicLabels(_listPRFull.Select(pair => pair.Label).ToArray(), sender);
			}
		}
	}
}
