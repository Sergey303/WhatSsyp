const message = {
    name: "Система",
    text: "Новый пользователь подключился к серверу!",
}

const json = JSON.stringify(message);
//Chat.send("chat", json);

function sendMessage() {
    const name = document.getElementById("nameInput").value.trim();
    const text = document.getElementById("messageInput").value.trim();
    if (text === "" || name === "") {
        alert("Нужно имя и сообщение");
        return;
    }
    if (text === "/roll") {
        Chat.send(text);
        return;
    }

    Chat.send("chat", name + ": " + text);
    //Chat.send("chat", JSON.stringify(message));
    document.getElementById("messageInput").value = "";
    
    //showMessage(name + ": " + text);
    text.value = "";
}

function showMessage(text) {
    const messages = document.getElementById("messages");
    const block = document.createElement("div");
    block.className = "message";
    block.textContent = text;
    messages.appendChild(block);
}

function showPct(text) {
    const messages = document.getElementById("messages");
    const block = document.createElement("img");
    block.className = "image";
    block.src = text;
    block.width = 300;
    block.height = 200;
    block.title = text;
    messages.appendChild(block);
}

Chat.receive("chat", function (json) {
    showMessage(json);
    //try {
    //    const message = JSON.parse(json);
    //    showMessage(message.name, message.text);
    //} catch (error) {
    //    showMessage("Система", "Пришёл неправильный JSON")
    //}

});

Chat.receive("pct", function (json) {
    //debugger;
    showPct(json);
});

function rollDice() {
    const name = document.getElementById("nameInput").value.trim();
    //const number = Math.floor(Math.random() * 6) + 1;
    //Chat.send("roll", name + " выбросил " + number);
    //showMessage(name + " выбросил " + number);
    const num1 = Math.floor(Math.random() * 1000) + 1;
    const num2 = Math.floor(Math.random() * 1000) + 1;
    const num3 = Math.floor(Math.random() * 3) + 1;
    if (num3==1) {
        Chat.send("chat", name + " начал(а) игру, сколько будет " + num1 + " + " + num2 + "?");
    }
    if (num3==2) {
        num4 = Math.floor(Math.random() * 20) + 1;
        num5 = Math.floor(Math.random() * 20) + 1;
        Chat.send("chat", name + " начал(а) игру, сколько будет " + num4 + " * " + num5 + "?");
    }
    if (num3==3) {
        if (num1>=num2) {
            Chat.send("chat", name + " начал(а) игру, сколько будет " + num1 + " - " + num2 + "?");
        } else {
            Chat.send("chat", name + " начал(а) игру, сколько будет " + num2 + " - " + num1 + "?");
        }
    }
}

function sendPct() {
    const name = document.getElementById("nameInput").value.trim();
    const text = document.getElementById("messageInput").value.trim();
    const block = document.createElement("image");
    block.className = "image";
    block.src = text;
    block.width = 300;
    block.height = 200;
    //Chat.send("pct", name + ": " + text);
    Chat.send("pct", text);
}

Chat.connect();
Chat.send("chat", "Пользователь подключился к серверу!");
var b = document.getElementById("sendBtn");
var d = document.getElementById("fileBtn");
var g = document.getElementById("logining")
b.onclick=sendMessage;
d.onclick=sendPct;