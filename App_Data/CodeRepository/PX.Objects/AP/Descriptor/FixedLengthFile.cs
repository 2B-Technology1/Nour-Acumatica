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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using PX.Data;

namespace PX.Objects.AP
{
	public class FixedLengthFile
	{
		public void WriteToFile(List<object> records, StreamWriter writer)
		{
			if (records.Count > 0)
			{
				foreach (object record in records)
				{
					string dataRow = string.Empty;

					Type recordtype = record.GetType();
					PropertyInfo[] propertyInfo = recordtype.GetProperties();

					foreach (PropertyInfo info in propertyInfo)
					{
						try
						{
							FixedLengthAttribute[] attributes = info.GetCustomAttributes(typeof(FixedLengthAttribute), false).Cast<FixedLengthAttribute>().ToArray();
							FixedLengthAttribute ffa = attributes.FirstOrDefault();
							if (ffa == null) continue;

							string outputString = Convert.ToString(info.GetValue(record, null));

							if (info.PropertyType == typeof(decimal))
								outputString = outputString.Replace(".", "");

							//Remove Special Characters
							if (!string.IsNullOrEmpty(ffa.RegexReplacePattern))
							{
								outputString = Regex.Replace(@outputString, ffa.RegexReplacePattern, string.Empty);
							}

							//Read value as per length and read position
							outputString = outputString.Length > ffa.FieldLength ? outputString.Substring(0, ffa.FieldLength) : outputString;

							switch (ffa.AlphaCharacterCaseStyle)
							{
								case AlphaCharacterCaseEnum.Lower:
									outputString = outputString.ToLower();
									break;
								case AlphaCharacterCaseEnum.Upper:
									outputString = outputString.ToUpper();
									break;
							}

							if (outputString.Length < ffa.FieldLength)
							{
								outputString = ffa.PaddingStyle == PaddingEnum.Left ? outputString.PadLeft(ffa.FieldLength, ffa.PaddingChar) : outputString.PadRight(ffa.FieldLength, ffa.PaddingChar);
							}

							if (outputString.Length == ffa.FieldLength)
							{
								if (dataRow.Length < ffa.StartPosition)
								{
									dataRow += outputString;
								}
								else
								{
									// current field falls inside the middle of the existing output string.
									// split based on start position and then concatenate
									string leftSide = dataRow.Substring(0, ffa.StartPosition - 1);
									string rightSide = dataRow.Substring(ffa.StartPosition, dataRow.Length);
									dataRow = leftSide + outputString + rightSide;
								}
							}
						}
						catch (Exception ex)
						{
							throw new PXException(PXMessages.LocalizeFormatNoPrefixNLA(Messages.WrittingError, record.GetType().Name, info.Name, ex.Message));
						}
					}

					// write to file
					writer.WriteLine(dataRow);
				}
			}
		}
	}
}