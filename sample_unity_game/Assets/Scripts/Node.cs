using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using StackExchange.Redis;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

/// <summary>
/// Authors: Kushaal Rao, Sam Boyer (Case Western), Hailey Ho (Emory)
/// This is a hybrid implementation written by Kushaal, combining Sam's and Hailey's work
/// Date: 11/13/2024
/// 
/// This code is a c# implementation of a BRAND node described in 
/// https://github.com/BrainGateTeam/brand-core/lib/python/brand/node.py
/// 
/// This implementation differs from the traditional python implementation of the brand node in 2 ways:
/// 
/// 1. This implmentation contains a nested dictionary as an attribute of the BRANDNode class. The dictionary will 
/// be asycronously updated as it receives information from specified Redis stream. In your Unity Game, you should 
/// retrieve values from BRANDNode.StreamDataDict, not read from redis directly. This is to prevent overloading of the Update function.
/// More detials in the Docs folder of this game project. 
/// 
/// 2. Reading from and writing to redis streams are now functions defined in the BRANDNode class, whereas in 
/// the python implementation, we read and write from redis using an instrance of redis(r.xread, r.get, r.xrange, etc...), 
/// which is an attribute of the brand class. In your Unity script, you will want to create an instance of BRANDNode and invoke the 
/// read and write functions like so: node.WriteToStream("game_data", gameDataDict) or node.ReadFromStream("game_data"))
/// 
/// </summary>

namespace BRANDForUnity
{
    public class BRANDNode
    {
        protected string _serverSocket; //the socket (path) from command line. currently unimplemented
        protected string _name; //nickname of this node recieved from command line 
        protected string _serverIP; //redis IP from command line
        protected int _serverPort; //redis port from command line
        protected ConnectionMultiplexer _redis; //redis multiplexer. do not dispose, this object is expensive
        protected IDatabase _database; //data base for reading and writing to redis.
        public static Dictionary<string, object> _parameters; //this nodes parameters, as read from the supergraph
        public static Dictionary<string, Dictionary<string, string>> StreamDataDict; //dictionary of all the stream data 

        public BRANDNode(string[] args)
        {
            //initializing dictionaries
            StreamDataDict = new Dictionary<string, Dictionary<string, string>> ();
            _parameters = new Dictionary<string, object>();

            ParseRedisArguments(args);
            ConnectToRedis();
            //StartListenersFromGraph();
        }

        void ParseRedisArguments(string[] args)
        {            
            /// This function parses arguments that are provided when instantiating a BRANDNode object. 
            /// These arguments will be used to create a connection to Redis
            
            Debug.Log("Parsing arguments");
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Contains("-n") || args[i].Contains("--nickname"))
                {
                    _name = args[i + 1];
                }

                if (args[i].Contains("-i") || args[i].Contains("--redis_host"))
                {
                    _serverIP = args[i + 1];
                }

                if (args[i].Contains("-p") || args[i].Contains("--redis_port"))
                {
                    _serverPort = int.Parse(args[i + 1]);
                }

                if (args[i].Contains("-s") || args[i].Contains("--redis_socket"))
                {
                    _serverSocket = args[i + 1];
                }
            }

            if (_name == null || _serverIP == null || _serverPort == 0)
            {
                throw new ArgumentException("Missing required argument(s)");
            }
        }

        void ConnectToRedis()
        {
            /// This function is establishing a connection with Redis
            
            Log("Connecting to Redis...");
            ConfigurationOptions config = new ConfigurationOptions // creating a config object based on parsing of arguments
            {
                EndPoints =
                {
                    {(_serverSocket != null) ? _serverSocket : _serverIP, _serverPort }
                }
            };

            try
            {
                _redis = ConnectionMultiplexer.Connect(config); // connecting to redis
                _database = _redis.GetDatabase();
                _parameters = ParseGraphParameters(); // storing game params from supergraph
                Log("Connection Successful");
            }
            catch (RedisConnectionException ex)
            {
                throw new BRANDException("Failed to connect to Redis. Check if the Redis server was started.", ex);
            }

            // (optional) creating a stream with the name of the game and the status 
            var res = _database.StreamAdd( _name + "_state",
                new NameValueEntry[]
                { new NameValueEntry("code", 0), new NameValueEntry("status", "initialized") }
            );
        }

        private Dictionary<string, object> ParseGraphParameters()
        {
            /// This function parses the supergraph's JSON and 
            /// extracts parameters for the games node. 
            
            _database = _redis.GetDatabase();
            var values = new Dictionary<string, object>();
            var key = "supergraph_stream"; //at risk due to hardcode 
            var result =  _database.StreamRange(key, "-", "+", 1, Order.Descending);

            if (result.Any())
            {
                //painful extraction of the parameters from the SUPER jagged json string 
                string masterJsonString = result[0].Values[0].Value.ToString();
                JObject jobject = JObject.Parse(masterJsonString);
                Dictionary<string, object> dict = jobject.ToObject<Dictionary<string, object>>();
                var nodesString = dict["nodes"].ToString();
                var nodesDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(nodesString);
                if (nodesDict.ContainsKey(_name))
                {
                    var graphDict = nodesDict[_name];
                    values = JsonConvert.DeserializeObject<Dictionary<string, object>>(graphDict["parameters"].ToString());
                }
                else
                {
                    Debug.LogError("could not find this nodes parameters in the super graph");
                }
            }
            return values;
        }

        public async Task ReadFromStream(string channelName)
        {
            /// This function reads data from a specified redis stream and 
            /// appends data to StreamDataDict. This function should only be called
            /// once because the while loop constantly reads in data. The Start()
            /// function would be an appropriate place to call this function. 
            
            while (Application.isPlaying)
            { // Application is playing is basically a replacement for SIGINT
                await Task.Run(async () =>
                {
                    var result = await _database.StreamRangeAsync(channelName, "-", "+", 1, Order.Descending);
                    if (result.Any()) 
                    {
                        StreamDataDict[channelName] = ParseResult(result);
                    }
                    else
                    {
                        //Debug.LogWarning($"No entries found for stream: {channelName}");
                        return; 
                    }
                });
            }
            Debug.LogWarning("Application is not playing. Stopping reading from Redis stream.");
        }

        private void StartListenersFromGraph()
        {
            /// This function uses _parameters retrieved from ParseGraphParameters()
            /// and specifically looks for the key "input_streams" to determine what streams
            /// it should listen to. It then calls ReadFromStream() for each stream. If you 
            /// are running a graph from the BRAND GUI using supervisor, you probably want to use
            /// this function. If you are doing dev work and locally testing things, you can 
            /// manually call ReadFromStream().  
            var tasks = new List<Task>();
            foreach (KeyValuePair<string, object> kvp in _parameters)
            {
                Debug.Log($"param key: {kvp.Key} | param value: {kvp.Value}");
            }

            if (_parameters.ContainsKey("input_streams"))
            {
                var inputStreamObj = _parameters["input_streams"];
                try
                {
                    var inputStreamArray = JsonConvert.DeserializeObject<string[]>(inputStreamObj.ToString());
                    foreach (string inputStream in inputStreamArray)
                    {
                        Debug.Log($"adding {inputStream} to reading task list");
                        tasks.Add(ReadFromStream(inputStream));
                    }
                }
                catch (Exception ex)
                {
                    //Debug.LogWarning("could not load streams from graph. please declare input streams manually");
                    Debug.LogWarning(ex);
                }
            }
            else
            {
                Debug.Log("no input streams declared in graph");
            }
        }

        public async Task WriteToStream(string stream_name, Dictionary<string, string> entries)
        {
            /// This function parses writes an entry to a stream matching a specified key. 
            /// If a stream with the given key does not exist, it will automatically create one
            /// Unlike ReadFromStream() you will have to call this function each time you want to
            /// write to a stream. Essentially replaces LogStream() function in BGCommon
            
            // Convert the dictionary to an array of NameValueEntry
            // NameValueEntries are easier for Redis to handle than standard Dictionaries
            
            //var nameValueEntries = entries.Select(kvp => new NameValueEntry(kvp.Key, (float)(kvp.Value))).ToArray();
            var nameValueEntries = entries.Select(kvp => 
            {
                try
                {
                    string key = kvp.Key;
                    //float value = Convert.ToSingle(kvp.Value);
                    //float value = 5f;

                    byte[] byteArray = Encoding.UTF8.GetBytes(kvp.Value);
                    //Debug.Log($"Processing: Key = {key}, Value = {value}");
                    //_database.StreamAddAsync(key, field, value);
                    NameValueEntry namevalue = new NameValueEntry(key, byteArray);
                    //Debug.Log($"value: {namevalue.Value} type: {test.GetType()}");
                    return namevalue;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing entry: {ex.Message}");
                    return new NameValueEntry(kvp.Key, "0"); // or handle error as appropriate
                }
            }).ToArray();

            // //Debug.Log($"Processed {nameValueEntries.Length} entries");

            // // Add the entries to the stream
            // float value = 5f;
            // byte[] byteArray = BitConverter.GetBytes(value);
            
            await _database.StreamAddAsync(stream_name, nameValueEntries);
        }

        public Dictionary<string, string> ParseResult(StreamEntry[] result)
        {
            var parsedResult = new Dictionary<string, string>();

            if (result.Length > 0)
            {
                var entry = result[0]; // We're only dealing with the first (and likely only) entry
                // Parse the values
                foreach (var nameValue in entry.Values)
                {
                    string key = nameValue.Name.ToString();
                    RedisValue value = nameValue.Value;
                    parsedResult[key] = value.ToString();
                }
            }
            return parsedResult;
        }

        public void Log(string message)
        {
            UnityEngine.Debug.LogFormat("[{0}] {1}", _name, message);

        }


        public void PrintObjDictionary(Dictionary<string, object> dictionary)
        {
            try
            {
                foreach (var kvp in dictionary)
                {
                    Debug.Log($"Key: {kvp.Key}, Value: {kvp.Value}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
            }

        }
    }
}
