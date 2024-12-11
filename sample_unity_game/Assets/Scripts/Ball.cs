using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using BRANDForUnity;
using StackExchange.Redis;
using Newtonsoft.Json.Linq;
using System.Text;
using System;

public class Ball : MonoBehaviour
{
    private BRANDNode node;
    private Dictionary<string, string> gameDataDict;
    
    void Awake()
    {
        Debug.Log("creating an instance of BRAND node");
        string args = "-n sample_unity_game -i localhost -p 6379"; // change ip address to match BRAND PC (or wherever redis is hosted)
        node = new BRANDNode(args.Split(' '));
    }

    async void Start()
    {
        //initializing a dictionary of sample game data
        gameDataDict = new Dictionary<string, string>
        {
            { "cursor_x", "10" },
            { "cursor_y", "15" },
            { "click", "1" }
        };

        // Writing gameDataDict to a redis stream with key "game_data"
        // A new stream will be created bc this is the first time we are calling it
        node.WriteToStream("game_data", gameDataDict); 
        node.Log("writing to stream game_data");


        node.ReadFromStream("game_data"); // only needs to be called once, so don't call it in the Update()
        
    }

    async void Update()
    {
        node.WriteToStream("game_data", gameDataDict);
        node.Log("writing to stream game_data");
        if (BRANDNode.StreamDataDict.ContainsKey("game_data"))
        {
            PrintDictionary(BRANDNode.StreamDataDict["game_data"]);
        }         
        
    }

    public void PrintDictionary(Dictionary<string, string> dictionary)
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
