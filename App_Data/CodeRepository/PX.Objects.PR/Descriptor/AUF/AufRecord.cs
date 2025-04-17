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
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PX.Objects.PR.AUF
{
	public abstract class AufRecord
	{
		public char DelimiterCharacter;
		public string Endline;

		private AufRecordType _RecordType;

		public AufRecord(AufRecordType recordType, char delimiterCharacter = AufConstants.DefaultDelimiterCharacter, string endline = AufConstants.DefaultEndline)
		{
			DelimiterCharacter = delimiterCharacter;
			Endline = endline;
			_RecordType = recordType;
		}

		public abstract override string ToString();

		protected string FormatLine(params object[] lineData)
		{
			StringBuilder builder = new StringBuilder(AufConstants.RecordNames[_RecordType]);

			foreach (object data in lineData)
			{
				builder.Append(DelimiterCharacter);

				if (data is decimal)
				{
					builder.Append(((decimal)data).ToString("0.00"));
				}
				else if (data is DateTime)
				{
					builder.Append(((DateTime)data).ToString("MM/dd/yyyy"));
				}
				else if (data != null)
				{
					builder.Append(data.ToString());
				}
			}

			builder.Append(Endline);
			return builder.ToString();
		}

		protected static string FormatZipCode(string zipCode)
		{
			if (!string.IsNullOrEmpty(zipCode))
			{
				return new string(zipCode.ToCharArray().Where(x => x >= '0' && x <= '9').ToArray());
			}

			return null;
		}

		protected static string FormatPhoneNumber(string phoneNumber)
		{
			if (!string.IsNullOrEmpty(phoneNumber))
			{
				return new string(phoneNumber.ToCharArray().Where(x => x >= '0' && x <= '9').ToArray());
			}

			return null;
		}

		protected static string FormatEin(string ein)
		{
			if (!string.IsNullOrEmpty(ein) && Regex.IsMatch(ein, @"^\d{2}[ -]?\d{7}$"))
			{
				return ein;
			}

			throw new PXException(Messages.AatrixReportEinMissing);
		}

		protected static string FormatSsn(string ssn, string employeeID, bool required = true)
		{
			if (!string.IsNullOrEmpty(ssn))
			{
				string filtered = new string(ssn.ToCharArray().Where(x => x >= '0' && x <= '9').ToArray());
				if (!string.IsNullOrEmpty(filtered))
				{
					if (filtered.Length == 9)
					{
						return filtered;
					}
					else
					{
						throw new PXException(Messages.AatrixReportInvalidSsn, employeeID, ssn);
					}
				}
			}
			else if (required)
			{
				throw new PXException(Messages.AatrixReportSsnNotSet, employeeID);
			}

			return null;
		}
	}
}
