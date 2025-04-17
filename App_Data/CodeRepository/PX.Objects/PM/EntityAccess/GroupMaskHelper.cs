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

namespace PX.Objects.PM
{
	public static class GroupMaskHelper
	{
		public static bool IsIncluded(byte[] entityMask, byte[] groupMask)
		{
			if (entityMask == null || groupMask == null)
				return false;

			for (int i = 0; i < Math.Min(entityMask.Length, groupMask.Length); i++)
			{
				if (groupMask[i] != 0x00 && (entityMask[i] & groupMask[i]) == groupMask[i])
					return true;
			}

			return false;
		}

		public static byte[] UpdateMask(bool isIncluded, byte[] oldEntityMask, byte[] groupMask)
		{
			if (groupMask == null)
				return oldEntityMask;

			if (oldEntityMask == null && !isIncluded)
				return oldEntityMask;

			if (oldEntityMask == null)
			{
				oldEntityMask = new byte[groupMask.Length];
			}

			if (oldEntityMask.Length < groupMask.Length)
			{
				Array.Resize(ref oldEntityMask, groupMask.Length);
			}

			for (var i = 0; i < groupMask.Length; i++)
			{
				if (groupMask[i] == 0x00)
					continue;

				oldEntityMask[i] = isIncluded
					? (byte)(oldEntityMask[i] | groupMask[i])
					: (byte)(oldEntityMask[i] & ~groupMask[i]);
			}

			return oldEntityMask;
		}
	}
}
