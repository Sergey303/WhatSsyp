const name = "";

function sendMessage(filePath) {
    const inpMessage = document.getElementById("messageInp");
    const text = inpMessage.value.trim();
    const jsonString = JSON.stringify(
        { message: text, filePath: filePath || "", group: ""});
    console.log(filePath);
    Chat.send("chat", jsonString);
    document.getElementById("messageInp").value = "";
    document.getElementById('sendBtn').disabled = true;
    // alert(name + ": " + text);
}
// Chat.receive("chat", function (text) {
//     const msgObj = JSON.parse(text);
//     if (msgObj.text) {
//     }
//     if (msgObj.filePath) {
//         msgObj.filePath = "http://172.16.47.22:8080/api/MLfile?filePath=" + msgObj.filePath;
//     }
//     console.log("1");
//     try {
//         activateMedia(msgObj.filePath, msgObj.text, msgObj.name, msgObj.date);
//     }
//     catch(error) {
//         console.error(error);
//     }
//     console.log(msgObj.fileUrl);
// });

Chat.receive("chat", function (text) {
    const msgObj = JSON.parse(text);
    try {
        showMessage(msgObj)
    } catch(error) {
        console.error("Error processing message:", error);
    }
});



function showMessage(msgObj) {
    const time = new Date().toLocaleString('ru-RU', {
        hour: '2-digit',
        minute: '2-digit'
    });
    const date = new Date().toLocaleString('ru-RU', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    });

    activateMedia(msgObj.filePath || "", msgObj.message || "", msgObj.user || "User", msgObj.dt || "-");
}

// document.getElementById("fileInp").addEventListener("change", function (event) {
//     const file = document.getElementById("fileInp").file;
    
//     if (file) {
//         const reader = new FileReader();
        
//         reader.onload = function(e) {
//             try {
//                 const jsonData = JSON.parse(e.target.result);
//                 console.log('JSON data:', jsonData);
//             } catch (error) {
//                 console.error('Invalid JSON file:', error);
//             }
//         };
        
//         reader.readAsText(file);
//     }

//     console.log(fileJSON);
// });

const signInName = document.getElementById("nameInp");

const signInLogin = document.getElementById("loginInp");

const signInPassword = document.getElementById("passwordInp");

Auth.start(startApp);

function startApp() {
    Chat.connect();
    Api.get("api/messages/", (messages) => {
        for (_m of messages) {
            showMessage(_m);
        }
    });
}
function regin() {
    const name_ = signInName.value;
    const login_ = signInLogin.value;
    const password_ = signInPassword.value;
    Auth.regin(name_, login_, password_, login)
}

function login() {
    const login = signInLogin.value;
    const password = signInPassword.value;

    Auth.login(login, password, startApp);
}

function signOut() {
    Auth.logout();
}