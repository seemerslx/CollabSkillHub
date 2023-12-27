import * as signalR from '@microsoft/signalr';

const connection = new signalR.HubConnectionBuilder()
    .withUrl('api/chatHub', {
        accessTokenFactory: () => localStorage.getItem('bearer_token')
    })
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.start().then(() => console.log('SignalR Connected!'));

const signalRService = {
    onReceiveMessage: (callback) => {
        connection.on('broadcastMessage', (message) => {
            callback(message);
        });
    },
    sendMessage: async (message, id) => {
        await connection.invoke('Send', {message, id});
    },
};

export default signalRService;