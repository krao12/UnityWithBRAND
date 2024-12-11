# Basic Usage
The central object in `BRAND.CSharp` is the `BRANDNode` class in the `BRAND.CSharp` namespace. This object is essentially a replica of the Python [`BRANDNode`](https://github.com/brandbci/brand/blob/main/lib/python/brand/node.py). 

Ideally, `BRANDNode` should be treated as a singleton object. `BRANDNode` uses the `ConnectionMultiplexer` class from `StackExchange.Redis`, which is designed to be *shared and reused*. Therefore, to avoid unnecessary overhead, `BRANDNODE` should be initialized once in your entire project.

Let's first create the `BRANDNode` object. `BRANDNode` requires info about the graph in order to connect to your `Redis` server and acquire relevant node parameters.
```
using BRAND.CSharp; # if you are using Unity, replace this with BRAND.Unity
...
BRANDNode Node = new BRANDNode("YOUR NODE NAME", "localhost", 6379);
```

Alternatively, if your end goal is to run the BRAND node as an executable program, you can also pass the parameters via the command line, such as:
```
myapp.bin --nickname YOUR_NODE_NAME --redis-host localhost --redis-port 6379
```
Then, your code will only look like this:
```
using BRAND.CSharp; # if you are using Unity, replace this with BRAND.Unity
...
BRANDNode Node = new BRANDNode();
```

Once you have a `BRANDNode`, there are 2 things you might want to do:
- Reading from a `Redis` stream
- Writing to a `Redis` stream

## Reading from a `Redis` stream

<!-- First, to prevent your Redis requests from running endlessly in case of an error, we'll set a timeout for those requests.
```
var tokenSource = new CancellationTokenSource();
var token = tokenSource.Token;

tokenSource.
``` -->
To read from a `Redis` stream, we'll use the `StreamRangeAsync` function. `StackExchange.Redis` provides a synchronous and asynchronous version of any `Redis` commands. But if your node performs a lot of simultaneous reads, it is recommended to use the async functions. This example assumes you are making multiple read requests.
```
var latestId = "-"
var readTask1 = Task.Run(async() => {
    var res = await node.Db.StreamRangeAsync(
        "YOUR_STREAM_NAME",
        minId: latestId,
        maxId: "+",
        messageOrder: Order.Descending
    );

    if (res.Any())
    {
        var streamEntry = res[0]; # obtain the most recent stream entry
        var latestId = streamEntry.Id; # store the latest entry ID for next request
        var parsed = BRANDNode.DeserializeStreamEntry(streamEntry);
    }
});

var readTask2 = Task.Run(...); # same as above

await Task.WhenAll(readTask1, readTask2);
```
`StreamRangeAsync` returns a list of stream entries matching your request. This specific implementation of `StreamRangeAsync` is exactly similar to `XREVRANGE`, where the latest entry is returned first.

Each stream entry has an entry ID. Feel free to store the latest entry ID to optimize your request next time. 

`BRANDNode.DeserializeStreamEntry`, provided with `BRAND.CSharp`, reformats your stream entry into a dictionary-like object (`JObject`), including the stream keys. Note that this dictionary object will not contain your entry ID, so you'd need to store the ID separately if needed. Using this function is entirely optional. Feel free to create your own parse function!

An example of another parse function:
```
Dictionary<string, string> ParseResult(StreamEntry entry) => entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString());
```

## Writing to a `Redis` stream

To write to a `Redis` stream, we will use the `StreamWriteAsync` function. Again, note that there is also a synchronous version of this function.

Before we write, we need to serialize the data. There are multiple ways to serialize data:
- Simplest form --
```
var serialized = new NameValueEntry[]
{
    new NameValueEntry("key1", "hello world"),
    new NameValueEntry("key2", jobject.ToString())
};
```
- Using our function `SerializeStreamEntry` -- This function is the exact opposite of `DeserializeStreamEntry`. It takes in a `JObject` assuming that the stream keys (e.g., "key1" and "key2" from the above example) are already in JSON struct.
```
var serialized = BRANDNode.SerializeStreamEntry(jobject);
```

After you serialized your data, writing to a `Redis` stream is very straightforward:
```
var writeTask1 = Task.Run(async() => {
    var res = await node.Db.StreamAddAsync(
        "YOUR_STREAM_NAME",
        serialized
    );
});

var writeTask2 = Task.Run(...); # same as above

await Task.WhenAll(writeTask1, writeTask2);
```