using System;
using PX.Data;

namespace SurveyCustomization
{
  [Serializable]
  [PXCacheName("survayanswers")]
  public class survayanswers : IBqlTable
  {
    #region QType
    [PXDBInt()]
    [PXUIField(DisplayName = "QType")]
    public virtual int? QType { get; set; }
    public abstract class qType : PX.Data.BQL.BqlInt.Field<qType> { }
    #endregion

    #region Answers
    [PXDBString(20, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Answers")]
    public virtual string Answers { get; set; }
    public abstract class answers : PX.Data.BQL.BqlString.Field<answers> { }
    #endregion

    #region Value
    [PXDBInt()]
    [PXUIField(DisplayName = "Value")]
    public virtual int? Value { get; set; }
    public abstract class value : PX.Data.BQL.BqlInt.Field<value> { }
    #endregion
  }
}