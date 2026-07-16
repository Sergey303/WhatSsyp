Chat.connect();

Chat.receive("chat", (text) => {
    //alert(text);
    showMessage(text);
});

function showMessage(text){
    const messages = document.getElementById("messages");
    const message = document.createElement("div");
    const mes = JSON.parse(text);
    message.innerHTML = mes.user + ": " + mes.message;
    messages.appendChild(message);
}

function sendBtn() {
    const name = document.getElementById("nameInput").value;
    const text = document.getElementById("messageInput").value;
    const group = document.getElementById("groupInput").value;
    const messageJson = "{\"user\": "+"\""+ name+"\"" + ", \"group\": " + "\""+ group +"\"" + 
    ", \"message\": "+"\""+ text+"\"" + ", \"dt\": " + "\""+ "" +"\"}"; 
    //alert(messageJson);
    Chat.send("chat", messageJson);
}