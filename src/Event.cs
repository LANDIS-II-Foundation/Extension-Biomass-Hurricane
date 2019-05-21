//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.AgeOnlyCohorts;

using System.Collections.Generic;

namespace Landis.Extension.BaseHurricane
{
    public class Event
        : ICohortDisturbance
    {
        private static RelativeLocation[] neighborhood;
        private static IEventParameters[] hurricaneEventParms;
        private static List<ISeverity> severities;

        private ActiveSite initiationSite;
        private double intensity;
        private double sizeHectares;
        private int size;   // in # of sites
        private int sitesDamaged;
        private int cohortsKilled;
        private double severity;

        private ActiveSite currentSite; // current site where cohorts are being damaged
        private byte siteSeverity;      // used to compute maximum cohort severity at a site

        //---------------------------------------------------------------------

        static Event()
        {
            neighborhood = new RelativeLocation[] {
                new RelativeLocation(-1,  0),   // north
                new RelativeLocation(-1,  1),   // northeast
                new RelativeLocation( 0,  1),   // east
                new RelativeLocation( 1,  1),   // southeast
                new RelativeLocation( 1,  0),   // south
                new RelativeLocation( 1,  -1),  // southwest
                new RelativeLocation( 0, -1),   // west
                new RelativeLocation( -1, -1),  // northwest
            };

        }

        //---------------------------------------------------------------------

        public Location StartLocation
        {
            get {
                return initiationSite.Location;
            }
        }

        //---------------------------------------------------------------------

        public double Intensity
        {
            get {
                return intensity;
            }
        }

        //---------------------------------------------------------------------

        public double SizeHectares
        {
            get {
                return sizeHectares;
            }
        }

        //---------------------------------------------------------------------

        public int Size
        {
            get {
                return size;
            }
        }

        //---------------------------------------------------------------------

        public int SitesDamaged
        {
            get {
                return sitesDamaged;
            }
        }

        //---------------------------------------------------------------------

        public int CohortsKilled
        {
            get {
                return cohortsKilled;
            }
        }

        //---------------------------------------------------------------------

        public double Severity
        {
            get {
                return severity;
            }
        }

        //---------------------------------------------------------------------

        ExtensionType IDisturbance.Type
        {
            get {
                return PlugIn.ExtType;
            }
        }
        //---------------------------------------------------------------------

        ActiveSite IDisturbance.CurrentSite
        {
            get {
                return currentSite;
            }
        }
        //---------------------------------------------------------------------

        public static void Initialize(IEventParameters[] eventParameters,
                                      List<ISeverity>        severities)
        {
            hurricaneEventParms = eventParameters;
            Event.severities = severities;

        }

        //---------------------------------------------------------------------

        public static Event Initiate(ActiveSite site,
                                     int windTimestep)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            IEventParameters eventParms = hurricaneEventParms[ecoregion.Index];
            double eventProbability = (windTimestep * PlugIn.ModelCore.CellArea) /
                                      (eventParms.RotationPeriod * eventParms.MeanSize);
            
            if (PlugIn.ModelCore.GenerateUniform() <= eventProbability) {
                Event windEvent = new Event(site, ComputeSizeHectares(eventParms));
                windEvent.Spread(PlugIn.ModelCore.CurrentTime);
                return windEvent;
            }
            else
                return null;
        }

        //---------------------------------------------------------------------

        public static double ComputeSizeHectares(IEventParameters eventParms)
        {
            // double sizeGenerated = PlugIn.ModelCore.GenerateExponential(eventParms.MeanSize);
            PlugIn.ModelCore.ExponentialDistribution.Lambda = 1.0 / eventParms.MeanSize;
            double sizeGenerated = PlugIn.ModelCore.ExponentialDistribution.NextDouble();

            if (sizeGenerated < eventParms.MinSize)
                return eventParms.MinSize;
            else if (sizeGenerated > eventParms.MaxSize)
                return eventParms.MaxSize;
            else
                return sizeGenerated;
        }

        //---------------------------------------------------------------------

        private Event(ActiveSite initiationSite,
                      double     sizeInHectares)
        {
            this.initiationSite = initiationSite;
            this.sizeHectares = sizeInHectares;
            this.size = (int)(sizeInHectares / PlugIn.ModelCore.CellArea);
                // Round up?
            this.intensity = PlugIn.ModelCore.GenerateUniform();  //intensity ~ hurricane wind speed
            this.sitesDamaged = 0;
            this.cohortsKilled = 0;

        }

        //---------------------------------------------------------------------

        private void Spread(int currentTime)
        {
            int windDirection = (int) (PlugIn.ModelCore.GenerateUniform() * 8);
            int sitesInEvent = 0;
            long totalSiteSeverities = 0;

            Queue<Site> sitesToConsider = new Queue<Site>();
            sitesToConsider.Enqueue(initiationSite);
            while (sitesToConsider.Count > 0 && sitesInEvent < size) {
                Site site = sitesToConsider.Dequeue();
                SiteVars.Event[site] = this;
                sitesInEvent++;

                currentSite = (ActiveSite) site; // as ActiveSite;

                if (currentSite) {
                    KillSiteCohorts(currentSite);
                    if (siteSeverity > 0)
                    {
                        sitesDamaged++;
                        totalSiteSeverities += siteSeverity;
                        //UI.WriteLine("  site severity: {0}", siteSeverity);
                        SiteVars.Disturbed[currentSite] = true;
                        SiteVars.TimeOfLastEvent[currentSite] = currentTime;
                        // SiteVars.LastSeverity[currentSite] = siteSeverity;
                    }
                    SiteVars.Severity[currentSite] = siteSeverity;
                }

                if (sitesInEvent < size) {
                    //  Add site's neighbors in random order to the list of
                    //  sites to consider.  The neighbors cannot be part of
                    //  any other wind event in the current timestep, and
                    //  cannot already be on the list.
                    
                    List<Site> neighbors = GetNeighbors(site, windDirection);

                    if (neighbors.Count > 0)
                    {
                        neighbors = PlugIn.ModelCore.shuffle(neighbors);
                        foreach (Site neighbor in neighbors)
                        {
                            if (!neighbor.IsActive)
                                continue;
                            if (SiteVars.Event[neighbor] != null)
                                continue;
                            if (sitesToConsider.Contains(neighbor))
                                continue;
                            sitesToConsider.Enqueue(neighbor);
                        }
                    }
                }
            }

            if (sitesDamaged == 0)
                severity = 0;
            else
                severity = ((double) totalSiteSeverities) / sitesDamaged;
        }

        //---------------------------------------------------------------------

        private List<Site> GetNeighbors(Site site, int windDirection)
        {

            double mod_intensity = 1.0 - this.intensity;
            if(windDirection > 7) windDirection = 7;
            double[] windProbs =
            {
            (((4.0 - mod_intensity)/8.0) * (1+mod_intensity)), //Primary direction
            (((4.0 - mod_intensity)/8.0) * (1+mod_intensity)),
            (((4.0 - mod_intensity)/8.0)),
            (((4.0 - mod_intensity)/8.0) * (1-mod_intensity)),
            (((4.0 - mod_intensity)/8.0) * (1-mod_intensity)), //Opposite of primary direction
            (((4.0 - mod_intensity)/8.0) * (1-mod_intensity)),
            (((4.0 - mod_intensity)/8.0)),
            (((4.0 - mod_intensity)/8.0) * (1+mod_intensity)),
            };

            double windProb = 0.0;
            int index = 0;
            List<Site> neighbors = new List<Site>(9);
            foreach (RelativeLocation relativeLoc in neighborhood)
            {
                Site neighbor = site.GetNeighbor(relativeLoc);
                if(index + windDirection > 7)
                    windProb = windProbs[index + windDirection - 8];
                else
                    windProb = windProbs[index + windDirection];
                if (neighbor != null && PlugIn.ModelCore.GenerateUniform() < windProb)
                    neighbors.Add(neighbor);
                index++;
            }

            //Next, add the 9th neighbor, a neighbor one cell beyond the
            //8 nearest neighbors.
            //array index 0 = north; 1 = northeast, 2 = east,...,8 = northwest
            int[] vertical  ={2,2,0,-2,-2,-2,0,2};
            int[] horizontal={0,2,2,2,0,-2,-2,-2};

            RelativeLocation relativeLoc9 =
                new RelativeLocation(vertical[windDirection], horizontal[windDirection]);
            Site neighbor9 = site.GetNeighbor(relativeLoc9);
            if (neighbor9 != null && PlugIn.ModelCore.GenerateUniform() < this.intensity)
                neighbors.Add(neighbor9);
            return neighbors;
        }

        //---------------------------------------------------------------------

        private void KillSiteCohorts(ActiveSite site)
        {
            SiteVars.Cohorts[site].RemoveMarkedCohorts(this);
        }

        //---------------------------------------------------------------------

        bool ICohortDisturbance.MarkCohortForDeath(ICohort cohort)
        {
            float ageAsPercent = cohort.Age / (float) cohort.Species.Longevity;
            foreach (ISeverity severity in severities)
            {
                if(ageAsPercent >= severity.MinAge && ageAsPercent <= severity.MaxAge)
                {
                    if (intensity < severity.MortalityProbability) {
                        cohortsKilled++;
                        if (severity.Number > siteSeverity)
                            siteSeverity = severity.Number;
                        //UI.WriteLine("  cohort {0}:{1} killed, severity {2}", cohort.Species.Name, cohort.Age, severity.Number);
                        return true;
                    }
                    break;  // No need to search further in the table
                }
            }
            return false;
        }
    }
}
