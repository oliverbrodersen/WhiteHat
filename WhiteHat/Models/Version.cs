using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WhiteHat.Models
{
    public class Version
    {
        public int Release { get; set; }
        public int Feature { get; set; }
        public int Subfeature { get; set; }

        public Version(int release)
        {
            Release = release;
            Feature = 0;
            Subfeature = 0;
        }
        public Version(int release, int feature)
        {
            Release = release;
            Feature = feature;
            Subfeature = 0;
        }

        public Version(int release, int feature, int subfeature)
        {
            Release = release;
            Feature = feature;
            Subfeature = subfeature;
        }

        public static explicit operator Version(string v)
        {
            string[] vList = v.Split('.');
            return new Version(int.Parse(vList[0]), int.Parse(vList[1]), int.Parse(vList[2]));
        }

        public static implicit operator string(Version v) => v.ToString();

        public static bool TryParse(string s)
        {
            try
            {
                Version v = (Version)s;
            }
            catch(Exception e){ 
                return false; 
            }
            return true;
        }

        public override string ToString() => string.Format("{0}.{1}.{2}", Release, Feature, Subfeature);


        public override bool Equals(object? obj)
        {
            return obj is Version version &&
                   Release == version.Release &&
                   Feature == version.Feature &&
                   Subfeature == version.Subfeature;
        }
    }
}
