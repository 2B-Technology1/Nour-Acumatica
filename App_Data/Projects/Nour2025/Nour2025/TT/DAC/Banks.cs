using System;
using PX.Data;

namespace Maintenance
{
  [Serializable]
  public class Banks: IBqlTable
  {
    

    #region Bankid   
    

    [PXDBString(15, IsKey = true, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Bankid")]
    public string Bankid { get; set; }

    public class bankID : IBqlField{}

    #endregion


    #region BankName

    [PXDBString(100, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Bank Name")]
    public string BankName { get; set; }

    public class bankName : IBqlField{}

    #endregion

  }
}