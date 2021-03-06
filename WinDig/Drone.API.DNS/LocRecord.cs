using System;
using System.Collections.Generic;
using System.Text;

namespace Drone.API.DNS
{
    /// <summary>
    /// Implementation Referecne RFC 1876
    /// </summary>
    class LocRecord : IRecordData
    {
        /// <summary>
        /// Create Location Record from Data Buffer
        /// </summary>
        /// <param name="buffer"></param>
        public LocRecord(DataBuffer buffer)
        {
            version = buffer.ReadShortInt();
            size = buffer.ReadShortInt();
            horzPrecision = buffer.ReadShortInt();
            vertPrecision = buffer.ReadShortInt();
            lattitude = buffer.ReadInt();
            longitude = buffer.ReadInt();
            altitude = buffer.ReadInt();
        }
        /// <summary>
        /// Converts this data record to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("Version:{0} Size:{1} Horz Precision:{2} Veret Precision:{3} Lattitude:{4} Longitude:{5} Altitude:{6}",
                version, size, horzPrecision, vertPrecision, lattitude, longitude, altitude);
        }

        private short version;
        /// <summary>
        /// Version of record 
        /// </summary>
        public short Version
        {
            get { return version; }
        }
        private short size;
        /// <summary>
        /// Size of location 
        /// </summary>
        public short Size
        {
            get { return size; }
        }
        private short horzPrecision;
        /// <summary>
        /// Horizontal Precision of location
        /// </summary>
        public short HorzPrecision
        {
            get { return horzPrecision; }
        }
        private short vertPrecision;
        /// <summary>
        /// Vertical Precision of location
        /// </summary>
        public short VertPrecision
        {
            get { return vertPrecision; }
        }
        private long lattitude;
        /// <summary>
        /// Lattitude of location
        /// </summary>
        public long Lattitude
        {
            get { return lattitude; }
        }
        private long longitude;
        /// <summary>
        /// Longitude of location
        /// </summary>
        public long Longitude
        {
            get { return longitude; }
        }
        private long altitude;
        /// <summary>
        /// Altitude of location
        /// </summary>
        public long Altitude
        {
            get { return altitude; }
        }
    }
}
