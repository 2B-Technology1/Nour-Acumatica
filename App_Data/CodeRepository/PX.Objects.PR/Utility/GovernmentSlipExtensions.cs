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

using PX.Payroll.GovernmentSlips;
using System;
using System.Linq;

namespace PX.Objects.PR
{
	public static class GovernmentSlipExtensions
	{
		public static PRGovernmentSlip GetPRGovernmentSlip(this SlipInfo slipInfo, int year)
		{
			PRGovernmentSlip governmentSlip = new PRGovernmentSlip();
			governmentSlip.SlipName = slipInfo.SlipName;
			governmentSlip.Year = year;
			governmentSlip.Timestamp = slipInfo.Timestamp;
			governmentSlip.SlipData = Convert.ToBase64String(slipInfo.SlipData);

			return governmentSlip;
		}

		public static PRGovernmentSlipField GetPRGovernmentSlipField(this SlipField slipField, string slipName, int year)
		{
			PRGovernmentSlipField governmentSlip = new PRGovernmentSlipField();
			governmentSlip.SlipName = slipName;
			governmentSlip.Year = year;
			governmentSlip.Page = slipField.Page;
			governmentSlip.FieldName = slipField.FieldName;
			governmentSlip.FieldCode = slipField.FieldCode;
			governmentSlip.Fillable = slipField.Fillable;
			governmentSlip.FontName = slipField.FontName;
			governmentSlip.FontSize = slipField.FontSize;
			governmentSlip.Color = slipField.Color;
			governmentSlip.Alignment = slipField.Alignment.ToString();
			governmentSlip.DataType = slipField.DataType.ToString();
			governmentSlip.Multiline = slipField.Multiline;
			governmentSlip.LeftX = slipField.LeftX;
			governmentSlip.TopY = slipField.TopY;
			governmentSlip.Width = slipField.Width;
			governmentSlip.Height = slipField.Height;

			return governmentSlip;
		}

		public static SlipInfo GetSlipInfo(PRGovernmentSlip slip, PRGovernmentSlipField[] dbSlipFields)
		{
			SlipField[] slipFields = dbSlipFields.Select(f =>
				new SlipField(f.Page.Value, f.FieldName, f.FieldCode, (SlipFieldType)Enum.Parse(typeof(SlipFieldType), f.DataType),
					f.Fillable.Value, f.Multiline.Value, f.FontName, f.FontSize, f.Color,
					Enum.TryParse(f.Alignment, out SlipFieldAlignment slipFieldAlignment) ? slipFieldAlignment : (SlipFieldAlignment?)null,
					f.LeftX, f.TopY, f.Width, f.Height)).ToArray();

			return new SlipInfo(slip.SlipName, slip.Year.Value, slip.Timestamp.Value, Convert.FromBase64String(slip.SlipData), slipFields);
		}
	}
}
