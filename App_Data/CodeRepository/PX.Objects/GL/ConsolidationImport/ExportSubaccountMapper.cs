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

using PX.Data;
using PX.Objects.CS;

namespace PX.Objects.GL.ConsolidationImport
{
	public class ExportSubaccountMapper : IExportSubaccountMapper
	{
		protected readonly SegmentWithMappingInfo[] OrderedSegmentWithMappingInfos;

		public ExportSubaccountMapper(IReadOnlyCollection<Segment> segments,
			IEnumerable<SegmentValue> segmentValues)
		{
			var segmentIdsNeedingMappedValues = segments
				.Where(segment => segment.Validate == true)
				.Select(segment => segment.SegmentID)
				.ToArray();

			var segmentValuesGroupsBySegmentID = segmentValues
				.Where(segmentValue => segmentIdsNeedingMappedValues.Contains(segmentValue.SegmentID))
				.GroupBy(segmentValue => segmentValue.SegmentID)
				.ToDictionary(group => group.Key, group => group);

			OrderedSegmentWithMappingInfos = segments
				.Select(segment => new SegmentWithMappingInfo(segment,
					segment.Validate == true
						? segmentValuesGroupsBySegmentID[segment.SegmentID]
						: Enumerable.Empty<SegmentValue>()))
				.OrderBy(segment => segment.Segment.SegmentID)
				.ToArray();
		}

		public string GetMappedSubaccountCD(Sub subaccount)
		{
			if (subaccount == null)
				throw new ArgumentNullException("subaccount");

			int startIndex = 0;
			var resultSegmentValues = new SortedList<short, string>();

			foreach (var segmentInfo in OrderedSegmentWithMappingInfos)
			{
				var segment = segmentInfo.Segment;

				if (segment.ConsolNumChar <= 0)
				{
					startIndex += segment.Length.Value;
					continue;
				}

				string sourceValue = subaccount.SubCD.Substring(startIndex, segment.Length.Value);

				string mappedValue = null;
				if (segment.Validate == true)
				{
					if (segmentInfo.SegmentValueMap.ContainsKey(sourceValue))
					{
						mappedValue = segmentInfo.SegmentValueMap[sourceValue]?.PadRight(segmentInfo.Segment.ConsolNumChar ?? 0, ' ');
					}
					else if (segmentInfo.SegmentValueMap.ContainsKey(sourceValue.TrimEnd()))
					{
						mappedValue = segmentInfo.SegmentValueMap[sourceValue.TrimEnd()]?.PadRight(segmentInfo.Segment.ConsolNumChar ?? 0, ' ');
					}
				}
				else
				{
					mappedValue = sourceValue;
				}

				if (mappedValue != null && mappedValue.Length == segment.ConsolNumChar)
				{
					resultSegmentValues[segment.ConsolOrder.Value] = mappedValue;
				}
				else
				{
					return string.Empty;
				}

				startIndex += segment.Length.Value;
			}

			return string.Join(string.Empty, resultSegmentValues.Values);
		}

		protected class SegmentWithMappingInfo
		{
			public readonly Segment Segment;
			public readonly Dictionary<string, string> SegmentValueMap;

			public SegmentWithMappingInfo(Segment segment, IEnumerable<SegmentValue> segmentValues)
			{
				Segment = segment;
				SegmentValueMap = segmentValues.ToDictionary(
					segmentValue => segmentValue.Value,
					segmentValue => segmentValue.MappedSegValue);
			}
		}
	}
}
