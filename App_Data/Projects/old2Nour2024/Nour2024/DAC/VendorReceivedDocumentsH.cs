﻿namespace MyMaintaince.LM
{
	using System;
	using PX.Data;
    using PX.Objects.PO;
	
	[System.SerializableAttribute()]
	public class VendorReceivedDocumentsH : PX.Data.IBqlTable
	{
		#region RefNbr
		public abstract class refNbr : PX.Data.IBqlField
		{
		}
		protected string _RefNbr;
        [PXDBString(15, IsUnicode = true, IsKey = true, InputMask = ">CCCCCCCCCCCCCCCC")]
		[PXDefault()]
		[PXUIField(DisplayName = "RefNbr")]
        [PXSelector(typeof(Search<VendorReceivedDocumentsH.refNbr>))]
        [LMOAutoNumbering(typeof(LMOSetup.autoNumbering), typeof(LMOSetup.lastRefNbr))]
		public virtual string RefNbr
		{
			get
			{
				return this._RefNbr;
			}
			set
			{
				this._RefNbr = value;
			}
		}
		#endregion
		#region released
		public abstract class Released : PX.Data.IBqlField
		{
		}
		protected bool? _released;
		[PXDBBool()]
		[PXUIField(DisplayName = "released")]
		public virtual bool? released
		{
			get
			{
				return this._released;
			}
			set
			{
				this._released = value;
			}
		}
		#endregion
        
        #region PONbr
        public abstract class pONbr : PX.Data.IBqlField
        {
        }
        protected string _PONbr;
        [PXString(300)]
        [PXUIField(DisplayName = "Purchase Order Nbr")]
        //[PXSelector(typeof(Search<POOrder.orderNbr, Where<POOrder.status, Equal<POOrderStatus.closed>>>)
        [PXSelector(typeof(Search<POOrder.orderNbr>)
                   , new Type[]{
                       typeof(POOrder.orderType),
                       typeof(POOrder.orderNbr),
                       typeof(POOrder.vendorID)
                   })]
        public virtual string PONbr
        {
            get
            {
                return this._PONbr;
            }
            set
            {
                this._PONbr = value;
            }
        }
        #endregion
        
	}
}
