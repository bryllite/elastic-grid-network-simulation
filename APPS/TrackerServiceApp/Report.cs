using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrackerServiceApp
{
    public class Report
    {
        private int _id;
        private int _peers;
        private int _size;

        private List<double> _travel_times = new List<double>();

        public bool ReceivedAll
        {
            get
            {
                return (_travel_times.Count >= _peers);
            }
        }

        public int AvgTravelTime
        {
            get
            {
                return _travel_times.Count > 0 ? (int)(_travel_times.Sum() / _travel_times.Count) : 0;
            }
        }

        public int MaxTravelTime
        {
            get
            {
                return (int)_travel_times.Max();
            }
        }

        public int MinTravelTime
        {
            get
            {
                return (int)_travel_times.Min();
            }
        }

        public double[] TravelTimes
        {
            get
            {
                return _travel_times.ToArray();
            }
        }

        public int ReceivedCount
        {
            get
            {
                return _travel_times.Count;
            }
        }

        public Report(int id, int nPeers, int size)
        {
            _id = id;
            _peers = nPeers;
            _size = size;
        }

        public void Add(double latency)
        {
            _travel_times.Add(latency);
        }

        public override string ToString()
        {
            return $"id:{_id}, size:{_size}, peers:{_peers}, received:{_travel_times.Count}, latency:[{MinTravelTime},{AvgTravelTime},{MaxTravelTime}](ms)";
        }
    }
}
