using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

var builder = 
    WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
// добавляет возможности SignalR в приложение.

var app = 
    builder.Build();

app.UseDefaultFiles(); // помогает открыть index.html по адресу /
app.UseStaticFiles(); //  разрешает серверу отдавать файлы из wwwroot.

app.MapHub<ChatHub> (
    "/chatHub");  // Связываем вход под названием "/chatHub"  с SignalR (есть основной вход, second вход, а есть вход для SignalR) (название придумываем сами)
// MapHub - особая функция. если MapGet имеет вход, то она пересылает сообщение (указанное такое как "Server vorks" или "Вы вошли во второго входа" в зависимости от того какой вход мы используем. а вот MapHub принимает сообщение и пересылает его всем. Это другое)

// app.MapGet(
//     "/",  // основной вход пустой
//     () => "Server vorks");

app.MapGet(
    "/second",
    () => "Вы вошли во второго входа"); //текст который получит пользователь со второго хода 
 
app.Run(
    "http://0.0.0.0:5002"); // Основной адрес