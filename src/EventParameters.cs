//  Authors:    Robert M. Scheller, James B. Domingo

using Landis.Utilities;

namespace Landis.Extension.BaseHurricane
{
    public class EventParameters
        : IEventParameters
    {
        private double maxSize;
        private double meanSize;
        private double minSize;
        private int rotationPeriod;

        //---------------------------------------------------------------------

        /// <summary>
        /// Maximum event size (hectares).
        /// </summary>
        public double MaxSize
        {
            get {
                return maxSize;
            }
            set {
                if (value < 0)
                    throw new InputValueException(value.ToString(), "Value must be = or > 0.");
                if (meanSize > 0.0 && value < meanSize)
                    throw new InputValueException(value.ToString(), "Value must be = or > MeanSize.");
                if (minSize > 0.0 && value < minSize)
                    throw new InputValueException(value.ToString(),"Value must be = or > MinSize.");
                maxSize = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Mean event size (hectares).
        /// </summary>
        public double MeanSize
        {
            get {
                return meanSize;
            }
            set {
                if (value < 0)
                    throw new InputValueException(value.ToString(), "Value must be = or > 0.");
                if (maxSize > 0.0 && value > maxSize)
                    throw new InputValueException(value.ToString(), "Value must be < or = MaxSize.");
                if (minSize > 0.0 && value < minSize)
                    throw new InputValueException(value.ToString(), "Value must be = or > MinSize.");
                meanSize = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Minimum event size (hectares).
        /// </summary>
        public double MinSize
        {
            get {
                return minSize;
            }
            set {
                if (value < 0)
                    throw new InputValueException(value.ToString(), "Value must be = or > 0.");
                if (meanSize > 0.0 && value > meanSize)
                    throw new InputValueException(value.ToString(), "Value must be < or = MeanSize.");
                if (maxSize > 0.0 && value > maxSize)
                    throw new InputValueException(value.ToString(), "Value must be < or = MaxSize.");
                minSize = value;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Wind rotation period (years).
        /// </summary>
        public int RotationPeriod
        {
            get {
                return rotationPeriod;
            }
            set {
                if (value < 0)
                    throw new InputValueException(value.ToString(), "Value must be = or > 0.");
                rotationPeriod = value;
            }
        }

        //---------------------------------------------------------------------

        public EventParameters()
        {
        }
    }
}
