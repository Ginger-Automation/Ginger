var WebSocketServer=require("ws").Server;
var wss = new WebSocketServer({port:3000});

wss.on("connection",function(ws){

    console.log("Client connected");
    ws.send("Connected to Ginger Node Js server");
});