using UnityEngine;

namespace surfm.tool.realtimedb {
    public class GeoHashUtils {

        private static GeoHashUtils _instance;

        private static readonly int BITS_PER_BASE32_CHAR = 5;
        private static readonly int MAX_PRECISION = 22;
        public static readonly string BASE32_CHARS = "0123456789bcdefghjkmnpqrstuvwxyz";
        private int PRECISION = (int)ConstantRepo.getInstance().get<float>("Firebase.Geo.Precision");
        private float VECTOR_SCALE = ConstantRepo.getInstance().get<float>("Firebase.Geo.Vector.Scale");

        private GeoHashUtils() { }

        public string calcVectorHash(Vector2 v2)  {
            if (VECTOR_SCALE > 1 || VECTOR_SCALE <= 0) throw new System.Exception(VECTOR_SCALE+ " VECTOR_SCALE is not vaild");
            v2 *= VECTOR_SCALE;
            v2 += new Vector2(20,-45);
            return calcGeoHash(v2.x,v2.y);
        }

        public string calcGeoHash(double x,double y) {
            return calcGeoHash(
            x,
            y,
            PRECISION
            );
        }

        public static string calcGeoHash(
            double x,
            double y,
            int precision 
            ) {
            if (precision < 1) {
                throw new System.Exception("Precision of GeoHash must be larger than zero!");
            }
            if (precision > MAX_PRECISION) {
                throw new System.Exception("Precision of a GeoHash must be less than " + (MAX_PRECISION + 1) + "!");
            }
            if (!coordinatesValid(x, y)) {
                throw new System.Exception(string.Format("Not valid location coordinates: [{0}, {1}]", x, y));
            }
            double[] xRange = { -90, 90 };
            double[] yRange = { -180, 180 };

            char[] buffer = new char[precision];

            for (int i = 0; i < precision; i++) {
                int hashValue = 0;
                for (int j = 0; j < BITS_PER_BASE32_CHAR; j++) {
                    bool even = (((i * BITS_PER_BASE32_CHAR) + j) % 2) == 0;
                    double val = even ? y : x;
                    double[] range = even ? yRange : xRange;
                    double mid = (range[0] + range[1]) / 2;
                    if (val > mid) {
                        hashValue = (hashValue << 1) + 1;
                        range[0] = mid;
                    } else {
                        hashValue = (hashValue << 1);
                        range[1] = mid;
                    }
                }
                buffer[i] = valueToBase32Char(hashValue);
            }
            return new string(buffer);
        }

        public static bool coordinatesValid(double latitude, double longitude) {
            return (latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180);
        }

        public static char valueToBase32Char(int value) {
            if (value < 0 || value >= BASE32_CHARS.Length) {
                throw new System.Exception("Not a valid base32 value: " + value);
            }
            return BASE32_CHARS[(value)];
        }

        public static GeoHashUtils instance {
            get {
                if (_instance == null) {
                    _instance = new GeoHashUtils();
                }
                return _instance;
            }
        }

    }
}
