namespace surfm.tool.realtimedb {
    public class GeoHashUtils {

        private static readonly int BITS_PER_BASE32_CHAR = 5;
        private static readonly int MAX_PRECISION = 22;
        public static readonly string BASE32_CHARS = "0123456789bcdefghjkmnpqrstuvwxyz";
        private static string bucketUrl = ConstantRepo.getInstance().get<string>("Firebase.Storage.bucketUrl");

        private readonly string geoHash;

        public static string calcGeoHash(double latitude, double longitude, int precision) {
            if (precision < 1) {
                throw new System.Exception("Precision of GeoHash must be larger than zero!");
            }
            if (precision > MAX_PRECISION) {
                throw new System.Exception("Precision of a GeoHash must be less than " + (MAX_PRECISION + 1) + "!");
            }
            if (!coordinatesValid(latitude, longitude)) {
                throw new System.Exception(string.Format("Not valid location coordinates: [{0}, {1}]", latitude, longitude));
            }
            double[] longitudeRange = { -180, 180 };
            double[] latitudeRange = { -90, 90 };

            char[] buffer = new char[precision];

            for (int i = 0; i < precision; i++) {
                int hashValue = 0;
                for (int j = 0; j < BITS_PER_BASE32_CHAR; j++) {
                    bool even = (((i * BITS_PER_BASE32_CHAR) + j) % 2) == 0;
                    double val = even ? longitude : latitude;
                    double[] range = even ? longitudeRange : latitudeRange;
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

    }
}
