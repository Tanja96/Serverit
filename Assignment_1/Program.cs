using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Assignment_1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ShowBikeCount("Sammonpuistikko","offline").Wait();
            }
            catch (ArgumentException e) 
            {
                Console.WriteLine("Invalid argument: " + e.Message);
            }   
        }

        static async Task ShowBikeCount(string input, string type)
        {
            bool ok = true;
            foreach(char c in input)
            {
                if(char.IsDigit(c)){
                    ok = false;
                }
            }
            if(ok == true)
            {
                ICityBikeDataFetcher fetcher;
                if(type == "offline")
                {
                    fetcher = new OfflineCityBikeDataFetcher();
                }
                else 
                {
                    fetcher = new RealTimeCityBikeDataFetcher();
                }
                try
                {
                    int count = await fetcher.GetBikeCountInStation(input);
                    Console.WriteLine(count);
                }
                catch(NotFoundException e)
                {
                    Console.WriteLine("Not Found: " + e);
                }
            }
            else
            {
                throw new ArgumentException (input);
            }
        }
    }

    class NotFoundException : Exception
    {
        public NotFoundException():base() { } 
        public NotFoundException (string message):base (message) {}
    }

    class BikeStations
    {
        public List<BikeStation> stations = null;
    }

    class BikeStation
    {
        public string name = "";
        public int bikesAvailable = 0;
    }

    public interface ICityBikeDataFetcher
    {
         Task<int> GetBikeCountInStation(string stationName);
    }

    class RealTimeCityBikeDataFetcher : ICityBikeDataFetcher
    {
        public async Task<int> GetBikeCountInStation(string stationName)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage message = await client.GetAsync("http://api.digitransit.fi/routing/v1/routers/hsl/bike_rental");
            byte[] array = message.Content.ReadAsByteArrayAsync().Result;
            string info = Encoding.UTF8.GetString(array);
            BikeStations bikes = new BikeStations();
            bikes = JsonConvert.DeserializeObject<BikeStations>(info);
            BikeStation station = bikes.stations.Find(st => st.name.Contains(stationName));
            if(station == null)
            {
                throw new NotFoundException (stationName);
            }
            else
            {
                return station.bikesAvailable;
            }
        }
    }

    class OfflineCityBikeDataFetcher : ICityBikeDataFetcher 
    {
        public async Task<int> GetBikeCountInStation(string stationName)
        {
            string text = await System.IO.File.ReadAllTextAsync(@"C:\Users\Tanja\Desktop\Koulu\Serverit\bikes.txt");
            if(text.Contains(stationName))
            {
                int index = text.IndexOf(stationName);
                string dig = "";
                bool found = false;
                for(int i = index; i < (index + stationName.Length + 5); i++)
                {
                    if(found == false && char.IsDigit(text[i]))
                    {
                        dig = dig + text[i];
                    }
                    else if(found == false && dig != "")
                    {
                        found = true;
                    }
                }
                return Int32.Parse(dig);
            }
            else
            {
                throw new NotFoundException (stationName);
            }
        }
    }
}
