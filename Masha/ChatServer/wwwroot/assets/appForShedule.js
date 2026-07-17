const timesBlock = document.getElementById("times");
const timeInput = document.getElementById("timeInput");
const tasksBlock = document.getElementById("tasks");
const tasksInput = document.getElementById("tasksInput");

Chat.receive("elementOfTable", function (text){
    console.log(text);
    showPair(text);
});
Chat.receive("system", function (text){

});

Chat.connect();

// Chat.receive("chat", function (task){
//     showTasks(task);
// });
// Chat.receive("system", function (task){
//     showTasks(task);
// });

// Chat.connect();

function sendPair(){
    const elementOfTable = {
        time: timeInput.value, 
        task: tasksInput.value
    }
    const time = timeInput.value;
    if (time === ""){
        return;
    }
    const timeForAlert=timeInput.value;
    const ListTimeForAlert=timeForAlert.split(":");
    const date = new Date();
    date.setHours(ListTimeForAlert[0]);
    date.setMinutes(ListTimeForAlert[1]);
    // const invalidDate = new Date('invalid date');
    // if (isNaN(invalid))
    if (isNaN(date.getTime())){
        alert("Некорректный формат времени. Введите заново");
        timeInput.value = "";
        return;
    }
    const text = JSON.stringify(elementOfTable);
    allTasks.push(text);
    Chat.send("elementOfTable", text);
    tasksInput.value = "";
    timeInput.value = "";
}

    
var allTasks = [];



// function showTime(time){
//     const timesTemplate = document
//     .getElementById("timesTemplate");
//     const timeTempContent = timesTemplate.content;
//     const timeForAdd = timeTempContent.cloneNode(true);
//     timeForAdd.querySelector('.time-item').textContent = time;
//     document.getElementById("times").appendChild(timeForAdd);
// }

function showPair(text){
    const restoredElement = JSON.parse(text);
    const timesTemplate = document
    .getElementById("timesTemplate");
    const tasksTemplate=document
    .getElementById("tasksTemplate");
    const timeTempContent = timesTemplate.content;
    const taskTempContent = tasksTemplate.content;
    const timeForAdd = timeTempContent.cloneNode(true);
    const taskForAdd = taskTempContent.cloneNode(true);
    timeForAdd.querySelector('.time-item').textContent = restoredElement.time;
    taskForAdd.querySelector('.task-item').textContent = restoredElement.task;
    document.getElementById("times").appendChild(timeForAdd);
    document.getElementById("tasks").appendChild(taskForAdd);
    setTimeout(alertForUsers, 300000, restoredElement);
    
}


function alertForUsers(restoredElement){
    const currentDate = new Date();
    listRETime = restoredElement.time.split(":");
    if (currentDate.getHours() < Number(listRETime[0])){
        if (currentDate.getMinutes() < Number(listRETime[1])){
            if (currentDate.getMinutes() + 5 >= Number(listRETime[1])){
                alert("Ваша задача:" + restoredElement.task + "в" + restoredElement.time);
            } else{
                return;
            }
            
        }
    }
}

timeInput.addEventListener(
    "keydown",
    function (event) {
        if (event.key === "Enter"){
            sendPair();
        }
    }
); 
tasksInput.addEventListener(
    "keydown",
    function (event) {
        if (event.key === "Enter"){
            sendPair();
        }
    }
);