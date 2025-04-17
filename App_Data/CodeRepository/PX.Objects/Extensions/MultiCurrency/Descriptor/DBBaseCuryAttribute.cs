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
using CommonServiceLocator;

namespace PX.Objects.CM.Extensions
{
	/// <summary>
	/// Extends <see cref="PXDBDecimalAttribute"/> by defaulting the precision property.
	/// Precision is taken from Base Currency that is configured on the Company level.
	/// </summary>
	public class PXDBBaseCuryAttribute : PXDBDecimalAttribute
	{
		public virtual bool TrySetPrecisionWithBranch { get; set; } = true;
		private string _BranchID = "BranchID";
		protected Type branchID;

		public PXDBBaseCuryAttribute() : base() { }

		public PXDBBaseCuryAttribute(Type branchID):base()
		{
			this.branchID = branchID;
		}

		protected override void _ensurePrecision(PXCache sender, object row)
		{
			if (branchID != null)
			{
				_Precision = CurrencyCollection.GetCurrency(
						PXAccess.GetBranch(
							(int?)GetSourceID(sender, row, branchID))?.BaseCuryID)?.DecimalPlaces;
			}

			if (branchID == null && TrySetPrecisionWithBranch)
			{
				int? dacBranchID = sender.GetValue(row, _BranchID) as int?;
				if (dacBranchID != null)
				{
					_Precision = CurrencyCollection.GetCurrency(
						PXAccess.GetBranch(dacBranchID)?.BaseCuryID)?.DecimalPlaces;
				}

				if(dacBranchID == null || _Precision == null)
				{
					_Precision = CurrencyCollection.GetCurrency(sender.Graph.Accessinfo.BaseCuryID)?.DecimalPlaces;
				}
			}

			if ( _Precision == null || (branchID == null && !TrySetPrecisionWithBranch))
			{
				_Precision = ServiceLocator.Current.GetInstance<Func<PXGraph, IPXCurrencyService>>()(sender.Graph).BaseDecimalPlaces();
			}
		}
		public override void CacheAttached(PXCache sender)
		{
			sender.SetAltered(_FieldName, true);
			base.CacheAttached(sender);
		}

		private object GetSourceID(PXCache sender, object row, Type field)
		{
			if (field == null) return null;
			if (field.DeclaringType == sender.GetItemType())
				return sender.GetValue(row, field.Name);
			PXCache source = sender.Graph.Caches[field.DeclaringType];
			return source.GetValue(source.Current, field.Name);
		}
	}
}
