//// Example usage of RpcServer and RpcClient
//// This file demonstrates how to use the enhanced RPC classes

///*
// * SERVER EXAMPLE
// */
//using LIB_Define;

//// Create and configure server
//var server = new RpcServer("./logs", index: 0);

//// Wire up event handlers
//server.ActionOnLog += (index, message) => 
//{
//    Console.WriteLine($"[Server {index}] {message}");
//};

//server.ActionOnClientConnected += (index, clientId) => 
//{
//    Console.WriteLine($"[Server {index}] Client connected: {clientId}");
//};

//server.ActionOnClientDisconnected += (index, clientId) => 
//{
//    Console.WriteLine($"[Server {index}] Client disconnected: {clientId}");
//};

//// Configure and start
//server.UpdateConfig("localhost", 50051);
//await server.StartAsync();
//Console.WriteLine("Server started on localhost:50051");

//// Example 1: Send FlowChartOBJ
//var flowChart = new FlowChartOBJ
//{
//    ID = 1,
//    Location_X = 100,
//    Location_Y = 200,
//    Type = "Process",
//    Caption = "Sample Process",
//    AddressName = "Process_001"
//};

//// Add work items
//flowChart.WorkList.Add("Initialize");
//flowChart.WorkList.Add("Process Data");
//flowChart.WorkList.Add("Finalize");

//// Add colors (they will be converted to ARGB for JSON)
//flowChart.WorkList_Color.Add(Color.Red);
//flowChart.WorkList_Color.Add(Color.Green);
//flowChart.WorkList_Color.Add(Color.Blue);

//// Broadcast with acknowledgment
//var result = await server.BroadcastFlowChartAsync(flowChart, useAckMode: true);
//Console.WriteLine($"FlowChart broadcast: Success={result.Success}, Clients={result.ClientsReached}");

//// Example 2: Send custom object
//var customData = new 
//{
//    MessageId = 123,
//    Timestamp = DateTime.Now,
//    Data = new { Status = "OK", Progress = 75 }
//};

//var result2 = await server.BroadcastObjectAsync("custom_event", customData, useAckMode: true);
//Console.WriteLine($"Custom object broadcast: Success={result2.Success}");

//// Example 3: Send image by path
//var imagePath = @"C:\temp\flowchart.png";
//if (File.Exists(imagePath))
//{
//    var result3 = await server.BroadcastImageByPathAsync(
//        ShowPictureType.Flow,
//        imagePath,
//        useAckMode: true
//    );
//    Console.WriteLine($"Image by path broadcast: Success={result3.Success}");
//}

//// Example 4: Send image by Image object
//using (var bitmap = new Bitmap(400, 300))
//using (var graphics = Graphics.FromImage(bitmap))
//{
//    graphics.Clear(Color.White);
//    graphics.DrawString("Test Image", 
//        new Font("Arial", 16), 
//        Brushes.Black, 
//        new PointF(10, 10));
    
//    var result4 = await server.BroadcastImageAsync(
//        ShowPictureType.Map,
//        bitmap,
//        fileName: "test_map.png",
//        useAckMode: true
//    );
//    Console.WriteLine($"Image by object broadcast: Success={result4.Success}");
//}

//// Get statistics
//var stats = server.GetStatistics();
//Console.WriteLine($"Stats: Requests={stats.TotalRequests}, Bytes={stats.TotalBytes}, Runtime={stats.Runtime}");

//// Keep running
//Console.WriteLine("Press any key to stop server...");
//Console.ReadKey();

//await server.StopAsync();

///*
// * CLIENT EXAMPLE
// */
//using LIB_Define;

//// Create client with config file path
//var client = new RpcClient(@"C:\config\client_config.json", index: 0);

//// Wire up event handlers
//client.ActionOnLog += (index, message) => 
//{
//    Console.WriteLine($"[Client {index}] {message}");
//};

//client.ActionOnConnectionError += (index, error) => 
//{
//    Console.WriteLine($"[Client {index}] Connection error: {error}");
//};

//// Handle incoming JSON messages from server
//client.ActionOnServerJson += (index, jsonMsg) =>
//{
//    Console.WriteLine($"[Client {index}] Received: type={jsonMsg.Type}, id={jsonMsg.Id}");
    
//    // Handle FlowChartOBJ
//    if (jsonMsg.Type == "flowchart")
//    {
//        var flowChart = client.DeserializeJsonMessage<FlowChartOBJ>(jsonMsg);
//        if (flowChart != null)
//        {
//            Console.WriteLine($"  FlowChart: ID={flowChart.ID}, Type={flowChart.Type}, Caption={flowChart.Caption}");
//            Console.WriteLine($"  Location: ({flowChart.Location_X}, {flowChart.Location_Y})");
//            Console.WriteLine($"  WorkList items: {flowChart.WorkList.Count}");
            
//            // Colors are automatically synchronized from ARGB
//            foreach (var color in flowChart.WorkList_Color)
//            {
//                Console.WriteLine($"    Color: R={color.R}, G={color.G}, B={color.B}");
//            }
//        }
//    }
    
//    // Note: "image" type is automatically handled by HandleServerImageMessage
//    // But you can also process it here if needed
//};

//// Handle images received from server
//client.ActionOnServerImage += (index, pictureType, image) =>
//{
//    Console.WriteLine($"[Client {index}] Received image: Type={pictureType}, Size={image.Width}x{image.Height}");
    
//    // Save to file or display in UI
//    var savePath = $@"C:\temp\received_{pictureType}_{DateTime.Now.Ticks}.png";
//    image.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
//    Console.WriteLine($"  Saved to: {savePath}");
//};

//client.ActionOnServerImagePath += (index, pictureType, path) =>
//{
//    Console.WriteLine($"[Client {index}] Received image path: Type={pictureType}, Path={path}");
    
//    // Load and use the image
//    if (File.Exists(path))
//    {
//        using var image = Image.FromFile(path);
//        Console.WriteLine($"  Loaded image: {image.Width}x{image.Height}");
//        // Process the image...
//    }
//};

//// Connect to server
//await client.StartConnect();
//Console.WriteLine("Client connected to server");

//// Example: Send FlowChartOBJ back to server
//var clientFlowChart = new FlowChartOBJ
//{
//    ID = 999,
//    Location_X = 500,
//    Location_Y = 600,
//    Type = "Response",
//    Caption = "Client Response"
//};

//var ack = await client.SendFlowChartAsync(clientFlowChart);
//Console.WriteLine($"Sent FlowChart to server: Success={ack.Success}");

//// Example: Send custom object
//var clientData = new 
//{
//    ClientId = "Client_001",
//    Status = "Ready",
//    Timestamp = DateTime.Now
//};

//var ack2 = await client.SendObjectAsJsonAsync("client_status", clientData);
//Console.WriteLine($"Sent status to server: Success={ack2.Success}");

//// Keep running
//Console.WriteLine("Press any key to disconnect...");
//Console.ReadKey();
