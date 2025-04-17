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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using PX.Api;
using PX.Common;
using PX.Data;
using PX.DataSync;
using PX.DataSync.ACH;
using StringReader = System.IO.StringReader;

namespace PX.Objects.Localizations.GB
{
	/// <summary>
	/// Data provider class for the BACS export scenario specific to the Lloyds bank in the UK.
	/// </summary>
	public class BACSLloydsProvider : ACHProvider
	{
		/// <summary>
		/// Export parameters
		/// </summary>
		public class Params
		{
			public const string FileName = "FileName";
			public const string BatchNbr = "BatchNbr";
		}

		protected override List<PXStringState> FillParameters()
		{
			List<PXStringState> list = base.FillParameters();

			PXStringState fileName = list.FirstOrDefault(x => x.Name == Params.FileName);
			if (fileName != null)
			{
				fileName.Value = "Lloyds.csv";
			}
			return list;
		}

		public override string ProviderName
		{
			get
			{
				return PXMessages.Localize(Messages.BACSLloyds.ProviderName);
			}
		}
		public string CurrentDate()
		{
			DateTime date = PXContext.GetBusinessDate() ?? PXTimeZoneInfo.Today;
			return $"{date:yyyy}{date:MM}{date:dd}";
		}
		public new string FormatDate(DateTime date)
		{
			return $"{date:yyyy}{date:MM}{date:dd}";
		}
		public virtual string GetAmount(decimal amount)
		{
			int aMaxPrecision = 2;
			int aMaxDigits = 18;

			decimal fraction = Math.Abs(amount - decimal.Round(amount));
			if (fraction != 0)
			{
				int fractionLength = fraction.ToString().Trim('0').Length - 2;
				if (fractionLength > aMaxPrecision + 1)
				{
					throw new PXException(PX.DataSync.Messages.AmountCantConvertedWithoutLossPrecision, amount);
				}

				aMaxDigits = 18 - fractionLength;
			}

			if (aMaxDigits < decimal.Round(amount).ToString().Length)
			{
				throw new OverflowException();
			}

			return amount.ToString().TrimEnd('0');
		}
		public virtual string GetSeqNumber(string seqNbr)
		{
			int nbr;

			if (!int.TryParse(seqNbr, out nbr))
			{
				throw new PXException(Messages.BACSLloyds.FileCreationNumberInvalid, seqNbr);
			}

			if (nbr < 1 || nbr > 999999)
			{
				throw new PXException(Messages.BACSLloyds.FileCreationNumberInvalid, seqNbr);
			}

			return nbr.ToString("000000");
		}

		public string DebitAccRef(string refNbr)
		{
			if (string.IsNullOrEmpty(refNbr))
				return "";

			string res = Regex.Replace(refNbr.Trim(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");

			if (res.Length > 18)
				return res.Substring(0, 18);
			else
				return res;
		}

		public string FormatRef(string externalPaymentRef)
		{
			if (string.IsNullOrEmpty(externalPaymentRef))
				return "";

			string res = Regex.Replace(externalPaymentRef.Trim(), @"(\s+|@|&|'|\(|\)|<|>|#)", "");

			if (res.Length > 18)
				res = res.Substring(0, 18);

			res = "," + res;

			return res;
		}

		#region Export
		protected override Byte[] InternalExport(String objectName, PXSYTable table)
		{
			string fileName = GetParameter(Params.FileName);

			//from ACHSYProvider
			notes.Clear();

			Int32 noteIndex = table.Columns.IndexOf(NoteID);
			if (noteIndex >= 0) notes.AddRange(table.Select(r => r[noteIndex]).Distinct());

			table = TrimTable(table, new String[] { NoteID, FileID }, startRow, endRow);

			//from FixWidthSYProvider
			List<SchemaFieldInfo> fields = InternalGetSchemaFields(objectName);

			String[][] array = SortTable(table, fields.Select(f => f.Name));
			fill = new FillContext(' ', ' ', ' ', 0, 0, false); // reset needsColPadding
			FillSchema(array);

			Byte[] data = null;
			Encoding encoding = GetEncoding();
			using (BACSWriter writer = new BACSWriter(encoding))
			{
				String result = schema.GetText();
				using (StringReader reader = new StringReader(result))
				{
					String s = reader.ReadLine();
					while (s != null)
					{
						string nextS = reader.ReadLine();

						if (nextS == null)
						{
							writer.Write(s);
						}
						else
						{
							writer.WriteLine(s);
						}

						s = nextS;
					}
				}
				data = writer.BinData;
			}
			return data;
		}
		#endregion
	}
}
