Auth.start(startApp);
const loginName = document.getElementById("loginName");
const loginPassword = document.getElementById("password");
const UserName = document.getElementById("username");
var roomName="Global";
var messages;
function startApp() {
    Chat.send("connected");
    Chat.connect();
    console.log("1");
    loadRooms();
    joinR("Global");
    //console.log(window.location);
    if (window.location.pathname != "/hub.html") {
        window.location.assign("/hub.html");
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
    Api.post("/olele", {Name:name, Password:password, UserName: username}, startApp);
}



Chat.receive("chat", function(t) {
    var convert = JSON.parse(t);
    if (roomName == convert.room) {
        showMessage(convert.name + ":" + convert.text);
    }
});
function doinging() {
    window.location.assign('/t-rex-runner-gh-pages/index.html');
}

function sendMessage() {
    const text = document.getElementById("messageInput").value;  
    if (text==="") {
        alert("Сообщение");
        return;
    }
    Chat.send("chat", JSON.stringify({text:text, room:roomName}));
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
        if (room.name == "Global") {
            const item = document.createElement("li")
            item.className = "list-group-item";
            item.textContent = "🌍 Глобальный чат";
            item.onclick = function() { joinR(room.name); };
            roomsBlock.appendChild(item);
        } else {
            const item = document.createElement("li");
            item.className = "list-group-item";
            item.textContent = room.name;
            item.onclick = function() { joinR(room.name); };
            roomsBlock.appendChild(item);
        }
    }
}
function loadRooms() {
    Api.get("/api/rooms", showRooms);
}
function roomCreated() {
    roomNameInput.value = "";
    loadRooms();
}
function createRoom() {
    const name = roomNameInput.value;
    Api.get("api/rooms",()=>{});
    if (name === "") {
        return;
    }
    Api.post("/api/rooms", {name: name, Members:[]}, roomCreated);
}
Chat.receive("roomMembers", function(text){const members = JSON.parse(text); console.log(members);});

function joinR(roomname) {
    roomName = roomname;

    //const name = document.getElementById("nameInput").value.trim();
    Api.get("/messages", (mess)=>{
        messages=JSON.parse(mess);
        const data = {RoomName:roomname};
        const messagesHTML = document.getElementById("messages");
        messagesHTML.innerHTML ="";
        for (const message of messages) {
            if (message.room == roomname) {
                showMessage(message.name + ":" + message.text);
            }
        }
        Chat.send("joinRoom", JSON.stringify(data));
    });
}
