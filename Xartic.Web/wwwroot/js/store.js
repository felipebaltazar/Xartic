let connected = false;
let connection;

function startSignalR(data) {
    if (isNullOrEmpty(store.state.host)) {
        setTimeout(() => startSignalR(data), 5000)
        return;
    }

    connection = new signalR.HubConnectionBuilder()
        .withUrl("https://api-xartic.azurewebsites.net/Xartic?username=" + store.state.host + "&roomName=Animais")
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

    connection.start().then(function () {
        console.assert(connection.state === signalR.HubConnectionState.Connected);
        connected = true;
        console.log("Xartic hub Connected.");

        subscribeRoomMessages(data);
        
        connection.invoke("CheckRoomStatus").catch(function (err) {
            return console.error(err.toString());
        });

        connection.invoke("StartGame").catch(function (err) {
            return console.error(err.toString());
        });

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

    connection.on('OnGameWordChanged', (currentWord) => {
        console.log("Xartic current word " + currentWord);
        data.currentWord = currentWord;
    });

    connection.on('OnRoomStatusChanged', (roomStatus) => {
        console.log(roomStatus);
        data.data = roomStatus;
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
        },
        updateUsers(context, payload) {
            context.commit('setUsers', data)
        },
        setHost(context, data) {
            context.commit('setHost', data);
        },
        updateRooms(context) {
        },
        getRooms(context) {
            console.log('start get rooms');
        },
        createRoom(context, payload) {
            console.log(`dalam store.index create room`)
        },
        joinRoom(context, payload) {
            console.log('join room');
        },
        roomDetail(context) {
            console.log('Get room details');
        },
        startGame(context, data) {
            console.log('Start game');
            startSignalR(data);
        },
        nextQuestion(context, data) {
        },
        clearCanvas(context, username) {
            if (!connected)
                return;

            connection.invoke("Clear").catch(function (err) {
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

            connection.invoke("Draw", drawCommand).catch(function (err) {
                return console.error(err.toString());
            });
        },
        canvasStroke(context) {
            console.log('Canvas stroke');
        },
        sendAnswer(context, data) {
            connection.invoke("Message", data.message).catch(function (err) {
                return console.error(err.toString());
            });
        },
        hasilTebakan(context) {
        },
        roomMessage(context, data) {
            vueApp = data;
        }
    },
    modules: {
    }
});