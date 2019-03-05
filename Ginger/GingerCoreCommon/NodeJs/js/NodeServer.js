var WebSocketServer=require("ws").Server;

var Jimp=require("Jimp");



handleMessage("")





var wss = new WebSocketServer({port:2999});

wss.on("connection",function(ws){

    console.log("Client connected");

    ws.send("Connected to Ginger Node Js server");
    ws.on("message",function(message)
    {
ws.send(handleMessage(message))
    });

});


function handleMessage(message)
{
  var base64string= Jimp.read(message).
  then(image=>{
   return image.getBase64Async(mime);
  });



   
  console.log(base64string);

  return base64string;
}