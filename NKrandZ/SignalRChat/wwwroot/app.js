
function loadRooms(){
    loadListR("/api/rooms");
}
function loadUsers(){
    loadList("/api/users");
}
function loadChatUsers(){
    loadListC("/api/chat-users");
}
function loadRoomUsers(roomName){
    loadListC("/api/rooms/" + roomName + "/users");
}
function loadListR(url){
    fetch(url)
        .then(function (response) {
            return response.json();
        })
        .then(function (response) {
            showListR(response);
        });
}
function loadListC(url){
    fetch(url)
        .then(function (response) {
            return response.json();
        })
        .then(function (response) {
            showListC(response);
        });
}
function loadList(url){
    fetch(url)
        .then(function (response) {
            return response.json();
        })
        .then(function (response) {
            showList(response);
        });
}
function showListR(items){
    const result = document.getElementById("roomResult");
    result.innerHTML = "";

    for (const item of items) {
        const block = document.createElement("option");
        block.value = item;
        block.textContent = item;
        result.appendChild(block);
    }
}
function showListC(items){
    const result = document.getElementById("chatResult");
    result.innerHTML = "";

    for (const item of items) {
        const block = document.createElement("option");
        block.value = item;
        block.textContent = item;
        result.appendChild(block);
    }
}
function showList(items){
    const result = document.getElementById("result");
    result.innerHTML = "";

    for (const item of items) {
        const block = document.createElement("div");
        block.textContent = item;
        result.appendChild(block);
    }
}

Chat.receive("historyFirst", function(text){
    const message = document.getElementById("messages");
    message.innerHTML="";

});

Chat.receive("messageHistory", function(json){
    const message = JSON.parse(json);
    showMessage(message.group + ": " + message.user + ": " + message.message);
});

Chat.receive("loginResult", function (text) {
    if (text == "Login Complete") {
        document.getElementById("login").hidden = true;
        document.getElementById("bd").hidden = false;
        loadRooms(); 
        joinRoom('General');
        loadRoomUsers('General');
    }
});

Chat.receive("chat", function (json) {
    try {
        const message = JSON.parse(json);
        showMessage(message.group + ": " + message.user + ": " + message.message);
    } catch (error) {
        showMessage("Failed to load Json");
    }
});

Chat.connect();

function register() {
    const username = document.getElementById("nameInput").value.trim();
    const login = document.getElementById("loginInput").value.trim();
    const password = document.getElementById("passwordInput").value.trim();
    const loginMessage = {
        username: username,
        login: login,
        password: password
    }
    const json = JSON.stringify(loginMessage);
    Chat.send("register", json);
}

function loginlogin() {
    const login = document.getElementById("loginInput").value.trim();
    const password = document.getElementById("passwordInput").value.trim();
    const loginMessage = {
        username: "",
        login: login,
        password: password
    }
    const json = JSON.stringify(loginMessage);
    Chat.send("login", json);
}

function joinRoom(rm) {
    const message = {
        message: "",
        user: document.getElementById("nameInput").value.trim(),
        dt: new Date().toLocaleTimeString(),
        group: rm
    };
    
    const json = JSON.stringify(message);
    Chat.send("joinRoom", json);
}

function newRoom() {
    const element = document.getElementById('newRoomProp');
    element.hidden = !element.hidden;
    const button = document.getElementById('newRoom');
    if (button.innerText === '✖️') {
        button.innerText = '➕';
    } else {
        button.innerText = '✖️';
    }
}

function appendRoom() {
    const name = document.getElementById("roomNameInput").value.trim();
    document.getElementById("roomNameInput").value = "";
    const message = {
        message: "",
        user: document.getElementById("nameInput").value.trim(),
        dt: new Date().toLocaleTimeString(),
        group: name
    };
    
    const json = JSON.stringify(message);
    Chat.send("newRoom", json);
    loadRooms();
}

function sendMessage() {
    const message = {
        message: document.getElementById("messageInput").value,
        user: document.getElementById("nameInput").value.trim(),
        dt: new Date().toLocaleTimeString(),
        group: document.getElementById('roomResult').value
    };
    if (message.message == ""){
        return;
    }
    const json = JSON.stringify(message);
    Chat.send("chat", json)
    document.getElementById("messageInput").value = "";
}

function showMessage(text) {
    const messages = document.getElementById("messages");
    const block = document.createElement("div");
    block.className = "message";
    block.id = "message";
    block.textContent = text;
    messages.appendChild(block);
}
