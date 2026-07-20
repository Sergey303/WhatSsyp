const signInName = document.getElementById("nameInp");
const signInLogin = document.getElementById("loginInp");
const signInPassword = document.getElementById("passwordInp");
const roomNameInput = document.getElementById("roomNameInput");
const roomsBlock = document.getElementById("rooms");
const messagesBlock = document.getElementById("messages");

function clearMessages() {
    messagesBlock.innerHTML = '';
}

function showMessage(text) {
    const block = document.createElement("div");
    block.className = "message";
    block.textContent = text;
    messagesBlock.appendChild(block);
    messagesBlock.scrollTop = messagesBlock.scrollHeight;
}

function showSystemMessage(text) {
    const block = document.createElement("div");
    block.className = "message system-message";
    block.textContent = text;
    messagesBlock.appendChild(block);
    messagesBlock.scrollTop = messagesBlock.scrollHeight;
}

Chat.receive("chat", function (text) {
    showMessage(text);
});

Chat.receive("system", function (text) {
    showSystemMessage(text);
});

Chat.receive("file", function (json) {
    var fileData = JSON.parse(json);
    showFile(fileData.filePath, fileData.fileName);
});

function sendMessage() {
    const text = document.getElementById("messageInput").value.trim();
    if (text === "") {
        return;
    }
    if (text === "/roll") {
        rollDice();
        document.getElementById("messageInput").value = "";
        return;
    }
    Chat.send("chat", text);
    document.getElementById("messageInput").value = "";
}

function rollDice() {
    const num1 = Math.floor(Math.random() * 1000) + 1;
    const num2 = Math.floor(Math.random() * 1000) + 1;
    const num3 = Math.floor(Math.random() * 3) + 1;
    if (num3 == 1) {
        Chat.send("chat", "начал(а) игру, сколько будет " + num1 + " + " + num2 + "?");
    }
    if (num3 == 2) {
        var num4 = Math.floor(Math.random() * 20) + 1;
        var num5 = Math.floor(Math.random() * 20) + 1;
        Chat.send("chat", "начал(а) игру, сколько будет " + num4 + " * " + num5 + "?");
    }
    if (num3 == 3) {
        if (num1 >= num2) {
            Chat.send("chat", "начал(а) игру, сколько будет " + num1 + " - " + num2 + "?");
        } else {
            Chat.send("chat", "начал(а) игру, сколько будет " + num2 + " - " + num1 + "?");
        }
    }
}

function showFile(filePath, fileName) {
    const messages = document.getElementById("messages");
    const fileUrl = "/" + filePath;
    const type = getFileType(filePath);
    
    if (!fileName) {
        fileName = filePath.split("/").pop().split("\\").pop();
    }
    
    const container = document.createElement("div");
    container.style.margin = "5px 0";
    
    if (type === '.tpl-img') {
        const img = document.createElement("img");
        img.src = fileUrl;
        img.style.maxWidth = "300px";
        img.style.maxHeight = "200px";
        img.style.display = "block";
        img.style.borderRadius = "8px";
        container.appendChild(img);
        
        const downloadRow = document.createElement("div");
        downloadRow.style.marginTop = "5px";
        const link = document.createElement("a");
        link.href = fileUrl;
        link.textContent = "Скачать " + fileName;
        link.download = fileName;
        link.style.color = "#2563eb";
        link.style.fontSize = "13px";
        downloadRow.appendChild(link);
        container.appendChild(downloadRow);
    } else if (type === '.tpl-video') {
        const video = document.createElement("video");
        video.src = fileUrl;
        video.controls = true;
        video.style.maxWidth = "400px";
        video.style.display = "block";
        video.style.borderRadius = "8px";
        container.appendChild(video);
        
        const downloadRow = document.createElement("div");
        downloadRow.style.marginTop = "5px";
        const link = document.createElement("a");
        link.href = fileUrl;
        link.textContent = "Скачать " + fileName;
        link.download = fileName;
        link.style.color = "#2563eb";
        link.style.fontSize = "13px";
        downloadRow.appendChild(link);
        container.appendChild(downloadRow);
    } else if (type === '.tpl-audio') {
        const audio = document.createElement("audio");
        audio.src = fileUrl;
        audio.controls = true;
        audio.style.display = "block";
        container.appendChild(audio);
        
        const downloadRow = document.createElement("div");
        downloadRow.style.marginTop = "5px";
        const link = document.createElement("a");
        link.href = fileUrl;
        link.textContent = "Скачать " + fileName;
        link.download = fileName;
        link.style.color = "#2563eb";
        link.style.fontSize = "13px";
        downloadRow.appendChild(link);
        container.appendChild(downloadRow);
    } else {
        const fileContainer = document.createElement("div");
        fileContainer.style.display = "flex";
        fileContainer.style.alignItems = "center";
        fileContainer.style.gap = "10px";
        fileContainer.style.padding = "10px";
        fileContainer.style.background = "#f3f4f6";
        fileContainer.style.borderRadius = "8px";
        
        const icon = document.createElement("span");
        icon.textContent = "📎";
        icon.style.fontSize = "24px";
        
        const info = document.createElement("div");
        info.style.flex = "1";
        
        const nameEl = document.createElement("div");
        nameEl.textContent = fileName;
        nameEl.style.fontWeight = "bold";
        nameEl.style.fontSize = "14px";
        
        const link = document.createElement("a");
        link.href = fileUrl;
        link.textContent = "Скачать";
        link.download = fileName;
        link.style.color = "#2563eb";
        link.style.fontSize = "13px";
        
        info.appendChild(nameEl);
        info.appendChild(link);
        fileContainer.appendChild(icon);
        fileContainer.appendChild(info);
        container.appendChild(fileContainer);
    }
    
    messages.appendChild(container);
    messages.scrollTop = messages.scrollHeight;
}

function doinging() {
    window.location.assign('/t-rex-runner-gh-pages/index.html');
}

function showRooms(rooms) {
    roomsBlock.innerHTML = '';
    
    var generalItem = document.createElement("li");
    generalItem.className = "list-group-item";
    generalItem.textContent = "🌍 Глобальный чат";
    generalItem.style.fontWeight = "bold";
    generalItem.onclick = function() { joinRoom("Общий"); };
    roomsBlock.appendChild(generalItem);
    
    for (const room of rooms) {
        const item = document.createElement("li");
        item.className = "list-group-item";
        item.textContent = room.name;
        item.onclick = function() { joinRoom(room.name); };
        roomsBlock.appendChild(item);
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
    const name = roomNameInput.value.trim();
    if (name === "") {
        return;
    }
    Api.post("/api/rooms", {name: name}, roomCreated);
}

Chat.receive("roomMembers", function(text) {
    const members = JSON.parse(text); 
    console.log("Участники комнаты:", members);
});

function joinRoom(roomName) {
    clearMessages();
    const data = {RoomName: roomName, UserName: ""};
    Chat.send("joinRoom", JSON.stringify(data));
}

function accountReg(name, login, password) {
    if (login.length < 3 || login.length > 20) {
        alert("Login must be at least 3 characters and not exceed 20 characters!");
        return false;
    }
    if (name.length > 16) {
        alert("Name must not exceed 16 characters!");
        return false;
    }
    if (password.length > 20) {
        alert("Password must not exceed 20 characters!");
        return false;
    }
    return true;
}

function regin() {
    const name_ = signInName.value.trim();
    const login_ = signInLogin.value.trim();
    const password_ = signInPassword.value.trim();
    if (!accountReg(name_, login_, password_)) {
        return;
    }
    Auth.regin(name_, login_, password_, function() {
        window.location.assign('/index.html');
    });
}

function login() {
    const login_ = signInLogin.value.trim();
    const password_ = signInPassword.value.trim();
    Auth.login(login_, password_, function() {
        window.location.assign('/index.html');
    });
}

function signOut() {
    Auth.logout();
}

function startApp() {
    Chat.connect();
    loadRooms();
}

document.getElementById("sendBtn").onclick = sendMessage;
document.getElementById("messageInput").addEventListener("keypress", function(e) {
    if (e.key === "Enter") {
        sendMessage();
    }
});
//document.getElementById("gameBtn").onclick = rollDice;
document.getElementById("fileBtn").onclick = function() {
    document.getElementById("fileInp").click();
};

Auth.start(startApp);