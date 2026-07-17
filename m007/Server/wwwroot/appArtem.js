Chat.connect();

Chat.receive("chat", (text) => {
    const chatBox = document.getElementById("messages-box");
    const messageTemp = document.getElementById("message-template");
    const message = messageTemp.content.cloneNode(true);
    message.querySelector(".text").innerText = text;
    chatBox.appendChild(message);
});

function sayHello() {
    alert("Hello!");
}

function sendChat() {
    const text = document.getElementById("InputID").value;
    Chat.send("chat", text);
}
