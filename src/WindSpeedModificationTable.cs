//  Authors:  Robert M. Scheller

using Landis.Utilities;

namespace Landis.Extension.Hurricane
{

    public interface IWindSpeedModificationTable
    {
        
        //IgnitionType Type {get;set;}
        double RangeMaximum{get;set;}
        double FractionWindReduction{get;set;}
    }

    /// <summary>
    /// </summary>
    public class WindSpeedModificationTable
        : IWindSpeedModificationTable
    {
        
        private double rangeMaximum;
        private double fractionWindReduction;
        
        
        //---------------------------------------------------------------------

        public WindSpeedModificationTable()
        {
            rangeMaximum = 0.0;
            fractionWindReduction = 0.0;
        }
        //---------------------------------------------------------------------

        public double RangeMaximum
        {
            get {
                return rangeMaximum;
            }
            set {
                    if (value < 0.0)
                        throw new InputValueException(value.ToString(), "Value must be > 0");
                rangeMaximum = value;
            }
        }
        //---------------------------------------------------------------------
        public double FractionWindReduction
        {
            get {
                return fractionWindReduction;
            }
            set {
                if (value < 0.0)
                    throw new InputValueException(value.ToString(), "Value must be > 0");
                fractionWindReduction = value;
            }
        }

    }
}
