using PX.Data;
using PX.Objects.BQLConstants;
using PX.Objects.CM;
using PX.Objects.GL;
using PX.Objects;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using System.Text;
using System;

namespace PX.Objects.GL
{
	public class GLHistoryEnquiryResultExt : PXCacheExtension<PX.Objects.GL.GLHistoryEnquiryResult>
	{
			#region UsrSubDescr
			[PXString(300)]
			[PXUIField(DisplayName="Sub Descr")]

			public virtual string UsrSubDescr { get; set; }
			public abstract class usrSubDescr : IBqlField { }
			#endregion
	}
}