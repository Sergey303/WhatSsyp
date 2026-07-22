// const timesBlock = document.getElementById("times");
const timeInput = document.getElementById("timeInput");
// const tasksBlock = document.getElementById("tasks");
const tasksInput = document.getElementById("tasksInput");
const timeInputToDelete=document.getElementById("timeInputForDelete");
const tasksInputToDelete=document.getElementById("tasksInputForDelete");

Chat.receive("elementOfTable", function (text){
    console.log(text);
    showPairFromBase(JSON.parse(text));
});

// Chat.receive("elementOfTable1", function (text){
//     console.log(text);
//     showPairFromBase(text);
// });
Chat.receive("system", function (text){

});


Chat.receive("timer1", function (alert){
    const lstAlertFiveMinutes = JSON.parse(alert);
    alertForUsers(lstAlertFiveMinutes);
});


Chat.receive("timer2", function (alertOverdue){
    const lstAlertFinished = JSON.parse(alertOverdue);
    alertForUsersFinished(lstAlertFinished);
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
        Time: timeInput.value, 
        Task: tasksInput.value
    }
    const time = timeInput.value;
    if (time === ""){
        return;
    }
    const task = tasksInput.value;
    if (task === ""){
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
    Chat.send("elementOfTable", text);
    tasksInput.value = "";
    timeInput.value = "";
}

function sendPairForDelete(event){
    // const elementOfTable1 = {
    //     Time: timeInputToDelete.value, 
    //     Task: tasksInputToDelete.value
    // }
    // const timeToDelete = timeInputToDelete.value;
    // if (timeToDelete === ""){
    //     return;
    // }
    // const timeForAlertDel=timeInputToDelete.value;
    // const ListTimeForAlert=timeForAlertDel.split(":");
    // const date = new Date();
    // date.setHours(ListTimeForAlert[0]);
    // date.setMinutes(ListTimeForAlert[1]);
    // // const invalidDate = new Date('invalid date');
    // // if (isNaN(invalid))
    // if (isNaN(date.getTime())){
    //     alert("Некорректный формат времени. Введите заново");
    //     timeInputToDelete.value = "";
    //     return;
    // }
    const clickedIndex = event.target.dataset.index;
    Api.get("api/MyTasks", (result1) => JSON.parse(result1));
    const index=result1.indexOf(clickedIndex);
    // const textToDelete = JSON.stringify(elementOfTable1);
    Chat.send("elementOfTable1", index);
    tasksInputToDelete.value = "";
    timeInputToDelete.value = "";
}

Api.get("api/MyTasks", (result) => showPairFromBase(JSON.parse(result)));


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
    timeForAdd.querySelector('.time-item').textContent = restoredElement.Time;
    taskForAdd.querySelector('.task-item').textContent = restoredElement.Task;
    document.getElementById("times").appendChild(timeForAdd);
    document.getElementById("tasks").appendChild(taskForAdd);
    setTimeout(alertForUsers, 300000, restoredElement);
    
}

function showPairFromBase(result){
    for (const res of result){
        const timesTemplate = document
        .getElementById("timesTemplate");
        const tasksTemplate=document
        .getElementById("tasksTemplate");
        const timeTempContent = timesTemplate.content;
        const taskTempContent = tasksTemplate.content;
        const timeForAdd = timeTempContent.cloneNode(true);
        const taskForAdd = taskTempContent.cloneNode(true);
        timeForAdd.querySelector('.time-item').textContent = res.Time;
        taskForAdd.querySelector('.task-item').textContent = res.Task;
        const buttonDelete = taskForAdd.querySelector('.trash');
        buttonDelete.dataset.index = result.indexOf(res);
        document.getElementById("times").appendChild(timeForAdd);
        document.getElementById("tasks").appendChild(taskForAdd);
    }   
}

// function alertForUsers(restoredElement){
//     const currentDate = new Date();
//     listRETime = restoredElement.time.split(":");
//     if (currentDate.getHours() < Number(listRETime[0])){
//         if (currentDate.getMinutes() < Number(listRETime[1])){
//             if (currentDate.getMinutes() + 5 >= Number(listRETime[1])){
//                 alert("Ваша задача:" + restoredElement.task + "в" + restoredElement.time);
//             } else{
//                 return;
//             }
            
//         }
//     }
// }
function alertForUsers(lstAlertFiveMinutes){
    for (const alertFive of lstAlertFiveMinutes){
        alert("Ваша задача " + alertFive.Task +" в " + alertFive.Time);
    }
}
function alertForUsersFinished(lstAlertFinished){
    for (const alertFinished of lstAlertFinished){
        alert("Есть просроченная задача! Ваша задача " + alertFinished.Task +" в " + alertFinished.Time);
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