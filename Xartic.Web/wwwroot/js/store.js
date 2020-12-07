let connected = false;
let connection;

function startSignalR(data) {
    if (isNullOrEmpty(store.state.host)) {
        setTimeout(() => startSignalR(data), 5000)
        return;
    }

    connection = new signalR.HubConnectionBuilder()
        .withUrl("https://api-xartic.azurewebsites.net/Xartic?username=" + store.state.host + "&roomName=MVPconf")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.start().then(function () {
        console.assert(connection.state === signalR.HubConnectionState.Connected);
        connected = true;
        console.log("Xartic hub Connected.");
        subscribeRoomMessages(data);
    }).catch(function (err) {
        console.assert(connection.state === signalR.HubConnectionState.Disconnected);
        console.log(err);
        setTimeout(() => startSignalR(data), 5000)
    });
};

function subscribeRoomMessages(data) {
    connection.on('Message', (username, message) => {
        if (username === store.state.host)
            return;

        const newAnswer = {
            username: username,
            message: message,
        };

        data.pushAnswer(newAnswer);
    });

    connection.on('ResponseResult', (message) => {

        const newAnswer = {
            message: message,
            isServerResponse: true,
            won: message.includes("acertou")
        };

        data.pushAnswer(newAnswer);
    });
}

function isNullOrEmpty(variable) {
    return variable === undefined ||
        variable === null ||
        variable === "null" ||
        variable === "" ||
        variable === " " ||
        variable === "\n";
}

const store = new Vuex.Store({
    state: {
        users: [],
        host: '',
        rooms: [],
        roomDetail: {},
        isPlaying: false,
        canvasStroke: {}
    },
    mutations: {
        setUsers(state, payload) {
            state.users = payload
        },
        getRooms(state, payload) {
            state.rooms = payload
        },
        updateRooms(state, data) {
            state.rooms = data
        },
        roomDetail(state, data) {
            state.roomDetail = data
            console.log(state.roomDetail)
        },
        startGame(state) {
            state.isPlaying = true
        },
        canvasStroke(state, data) {
            state.canvasStroke = data
        },
        setHost(state, data) {
            state.host = data;
        }
    },
    actions: {
        login(context, payload) {
            console.log('start login store');
            //socket.emit('user-login', payload)
        },
        updateUsers(context, payload) {
            //socket.on('user-data', (data) => {
                context.commit('setUsers', data)
            //})
        },
        setHost(context, data) {
            context.commit('setHost', data);
        },
        updateRooms(context) {
            //socket.on('updated-rooms', (data) => {
                //console.log(data, 'kiriman rooms dari update rooms')
                //context.commit('updateRooms', data)
            //})
        },
        getRooms(context) {
            console.log('start get rooms');
            //socket.on('get-rooms', (data) => {
                //console.log(data, 'data dari get rooms')
                //context.commit('getRooms', data)
            //})
        },
        createRoom(context, payload) {
            console.log(`dalam store.index create room`)
            //socket.emit('create-room', payload)
        },
        joinRoom(context, payload) {
            console.log('join room');
            //socket.emit('join-room', payload)
        },
        roomDetail(context) {
            console.log('Get room details');
            //socket.on('room-detail', (data) => {
                //console.log(data.users, 'dari room Detail')
                //context.commit('roomDetail', data)
            //})
        },
        startGame(context, data) {
            console.log('Start game');
            startSignalR(data);
        },
        nextQuestion(context, data) {
            //socket.emit('next-question', data)
        },
        clearCanvas(context, username) {
            if (!connected)
                return;

            connection.invoke("Clear", username).catch(function (err) {
                return console.error(err.toString());
            });
        },
        canvasLine(context, data) {
            if (!connected)
                return;

            const drawCommand = {
                Username: data.username,
                IsMouseDown: data.mouseDown,
                Color: {
                    Hex: data.color
                },
                Position: {
                    X: data.coorX,
                    Y: data.coorY,
                },
                Radius: 1
            };

            connection.invoke("Draw", data.username, drawCommand).catch(function (err) {
                return console.error(err.toString());
            });
        },
        canvasStroke(context) {
            console.log('Canvas stroke');
            //socket.on('canvas-stroke', (data) => {
                //context.commit('canvasStroke', data)
            //})
        },
        sendAnswer(context, data) {
            connection.invoke("Message", data.username, data.message).catch(function (err) {
                return console.error(err.toString());
            });
        },
        hasilTebakan(context) {
            //socket.on('hasil-tebakan', (data) => {
                //console.log(data, 'hasil tebakan')
            //})
        },
        roomMessage(context, data) {
            vueApp = data;
        }
    },
    modules: {
    }
});