Auth.start(startApp);
const loginName = document.getElementById("loginName");
const loginPassword = document.getElementById("password");
const UserName = document.getElementById("username");
window.roomName="Global";
var messages;
function startApp() {
    Chat.send("connected");
    Chat.connect();
    console.log("1");
    loadRooms();
    joinR("");
    //console.log(window.location);
    if (!window.location.pathname.endsWith("hub.html")) {
        window.location.assign("Xamic/hub.html");
    }
}
function login() {
    const name = loginName.value;
    const password = loginPassword.value;
    const username = UserName.value;
    Auth.login(name, password, username, startApp);
}
function registration() {
    const name = loginName.value;
    const password = loginPassword.value;
    const username = UserName.value;
    Api.post("api/register", {name, password, login: username}, startApp);
}



Chat.receive("chat", function(t) {
    var convert = JSON.parse(t);
    if (window.roomName == convert.group) {
        showMessage(convert.user + ":" + convert.message);
    }
});
function doinging() {
    window.location.assign('t-rex-runner-gh-pages/index.html');
}

function sendMessage() {
    const text = document.getElementById("messageInput").value;  
    if (text==="") {
        alert("Сообщение");
        return;
    }
    Chat.send("chat", JSON.stringify({message:text, group:window.roomName}));
    document.getElementById("messageInput").value = "";   
}
function showMessage(text) {
    const messages = document.getElementById("messages");
    const block = document.createElement("div");
    block.className = "message";
    block.textContent = text;
    messages.appendChild(block);
    messages.scrollTop = messages.scrollHeight;
}




//Api.get("/api/rooms". showRooms);
const roomNameInput = document.getElementById("roomNameInput");
const roomsBlock = document.getElementById("rooms");
function showRooms(rooms) {
    // roomsBlock.textContent = "";
    // for (const room of rooms) {
    //     const item = document.createElement("li");
    //     item.classname = "list-group-item";
    //     item.textContent = room.name;
    //     roomsBlock.appendChild(item);
    // }
    roomsBlock.textContent = "";
    for (const room of rooms) {
        if (room == "") {
            const item = document.createElement("li")
            item.className = "list-group-item";
            item.textContent = "🌍 Глобальный чат";
            item.onclick = function() { joinR(room); };
            roomsBlock.appendChild(item);
        } else {
            const item = document.createElement("li");
            item.className = "list-group-item";
            item.textContent = room;
            item.onclick = function() { joinR(room); };
            roomsBlock.appendChild(item);
        }
    }
}
function loadRooms() {
    Api.get("api/rooms", showRooms);
}

function createRoom() {
    const name = roomNameInput.value;
    if (name === "") {
        return;
    }
    roomNameInput.value = "";
    Chat.send("createRoom", JSON.stringify( {RoomName: name}));
}


Chat.receive("Room created", function(roomName) {
    const item = document.createElement("li");
    item.className = "list-group-item";
    item.textContent = roomName;
    item.onclick = function () {
        joinR(roomName);
    };
    roomsBlock.appendChild(item);
});

function joinR(roomname) {
    window.roomName = roomname;
    Chat.send("joinRoom", JSON.stringify({RoomName: roomname}));
}
Chat.receive("I joined room", function(mess) {
        messages=JSON.parse(mess);
        const messagesHTML = document.getElementById("messages");
        messagesHTML.innerHTML ="";
        for (const message of messages) {
                showMessage(message.user + ": " + message.message);
        }
});
