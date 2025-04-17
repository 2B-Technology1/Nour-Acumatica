using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nour20230314V1.InspectionForm.helpers
{
    public static class InspectionStatesConstants
    {
        public const string canceled = "C";
        public const string jobOrder = "J";
        public const string open = "O";
    }

    public static class InspectionTypeConstants
    {
        public const string FullInspection = "F";
        public const string SpecificInspection = "S";

    }
    
    public static class InspectionClass
    {
        public const string MalfunctionCheck = "MC";
        public const string SuspensionSystem = "SS";
        public const string RoadTest = "RT";
        public const string Motor = "M";
        public const string CarOutside = "CO";

    }


}
