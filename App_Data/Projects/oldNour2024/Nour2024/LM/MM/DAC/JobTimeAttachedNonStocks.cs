using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMaintaince
{

    using PX.Data;
    using PX.SM;
    using PX.Objects;
    using PX.Objects.CS;
    using PX.Objects.IN;

    [System.SerializableAttribute()]
    public class JobTimeAttachedNonStocks : PX.Data.IBqlTable
    {

        #region JobTimeID
        public abstract class jobTimeID : PX.Data.IBqlField
        {
        }
        protected string _JobTimeID;
        [PXDBString(50, IsUnicode = true)]
        [PXDBDefault(typeof(JobTime.Code))]
        [PXParent(typeof(
        Select<JobTime,
        Where<JobTime.Code,
        Equal<Current<JobTimeAttachedNonStocks.jobTimeID>>>>))]
        public virtual string JobTimeID
        {
            get
            {
                return this._JobTimeID;
            }
            set
            {
                this._JobTimeID = value;
            }
        }
        #endregion

        #region LineID
        public abstract class lineID : PX.Data.IBqlField
        {
        }
        protected int? _LineID;
        [PXDBIdentity(IsKey = true)]
        public virtual int? LineID
        {
            get
            {
                return this._LineID;
            }
            set
            {
                this._LineID = value;
            }
        }
        #endregion

        #region NonStockID
        public abstract class nonStockID : PX.Data.IBqlField
        {
        }
        protected int? _NonStockID;
        [NonStockItem(DisplayName = "Inventory ID")]
        [PXUIField(DisplayName = "Inventory ID")]
        [PXDefault()]
        public virtual int? NonStockID
        {
            get
            {
                return this._NonStockID;
            }
            set
            {
                this._NonStockID = value;
            }
        }
        #endregion

        #region NonStockName
        public abstract class nonStockName : PX.Data.IBqlField
        {
        }
        protected string _NonStockName;
        [PXDBString(255, IsUnicode = true)]
        [PXDefault("")]
        [PXUIField(DisplayName = "Inventory Desc")]
        public virtual string NonStockName
        {
            get
            {
                return this._NonStockName;
            }
            set
            {
                this._NonStockName = value;
            }
        }
        #endregion

        #region StandardQty
        public abstract class standardQty : PX.Data.IBqlField
        {
        }
        protected float? _StandardQty;
        [PXDBFloat(4)]
        [PXUIField(DisplayName="Standard Time")]
        public virtual float? StandardQty
        {
            get
            {
                return this._StandardQty;
            }
            set
            {
                this._StandardQty = value;
            }
        }
        #endregion
    }
}
