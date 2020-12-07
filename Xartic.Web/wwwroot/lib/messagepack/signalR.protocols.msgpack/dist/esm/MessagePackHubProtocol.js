// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
var __assign = (this && this.__assign) || Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
        s = arguments[i];
        for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
            t[p] = s[p];
    }
    return t;
};
import { Buffer } from "buffer";
import * as msgpack5 from "msgpack5";
import { LogLevel, MessageType, NullLogger, TransferFormat, } from "@microsoft/signalr";
import { BinaryMessageFormat } from "./BinaryMessageFormat";
import { isArrayBuffer } from "./Utils";
// TypeDoc's @inheritDoc and @link don't work across modules :(
// constant encoding of the ping message
// see: https://github.com/aspnet/SignalR/blob/dev/specs/HubProtocol.md#ping-message-encoding-1
// Don't use Uint8Array.from as IE does not support it
var SERIALIZED_PING_MESSAGE = new Uint8Array([0x91, MessageType.Ping]);
/** Implements the MessagePack Hub Protocol */
var MessagePackHubProtocol = /** @class */ (function () {
    /**
     *
     * @param messagePackOptions MessagePack options passed to msgpack5
     */
    function MessagePackHubProtocol(messagePackOptions) {
        /** The name of the protocol. This is used by SignalR to resolve the protocol between the client and server. */
        this.name = "messagepack";
        /** The version of the protocol. */
        this.version = 1;
        /** The TransferFormat of the protocol. */
        this.transferFormat = TransferFormat.Binary;
        this.errorResult = 1;
        this.voidResult = 2;
        this.nonVoidResult = 3;
        if (messagePackOptions) {
            this.messagePackOptions = __assign({}, messagePackOptions, { compatibilityMode: false });
        }
    }
    /** Creates an array of HubMessage objects from the specified serialized representation.
     *
     * @param {ArrayBuffer | Buffer} input An ArrayBuffer or Buffer containing the serialized representation.
     * @param {ILogger} logger A logger that will be used to log messages that occur during parsing.
     */
    MessagePackHubProtocol.prototype.parseMessages = function (input, logger) {
        // The interface does allow "string" to be passed in, but this implementation does not. So let's throw a useful error.
        if (!(input instanceof Buffer) && !(isArrayBuffer(input))) {
            throw new Error("Invalid input for MessagePack hub protocol. Expected an ArrayBuffer or Buffer.");
        }
        if (logger === null) {
            logger = NullLogger.instance;
        }
        var messages = BinaryMessageFormat.parse(input);
        var hubMessages = [];
        for (var _i = 0, messages_1 = messages; _i < messages_1.length; _i++) {
            var message = messages_1[_i];
            var parsedMessage = this.parseMessage(message, logger);
            // Can be null for an unknown message. Unknown message is logged in parseMessage
            if (parsedMessage) {
                hubMessages.push(parsedMessage);
            }
        }
        return hubMessages;
    };
    /** Writes the specified HubMessage to an ArrayBuffer and returns it.
     *
     * @param {HubMessage} message The message to write.
     * @returns {ArrayBuffer} An ArrayBuffer containing the serialized representation of the message.
     */
    MessagePackHubProtocol.prototype.writeMessage = function (message) {
        switch (message.type) {
            case MessageType.Invocation:
                return this.writeInvocation(message);
            case MessageType.StreamInvocation:
                return this.writeStreamInvocation(message);
            case MessageType.StreamItem:
                return this.writeStreamItem(message);
            case MessageType.Completion:
                return this.writeCompletion(message);
            case MessageType.Ping:
                return BinaryMessageFormat.write(SERIALIZED_PING_MESSAGE);
            case MessageType.CancelInvocation:
                return this.writeCancelInvocation(message);
            default:
                throw new Error("Invalid message type.");
        }
    };
    MessagePackHubProtocol.prototype.parseMessage = function (input, logger) {
        if (input.length === 0) {
            throw new Error("Invalid payload.");
        }
        var msgpack = msgpack5(this.messagePackOptions);
        var properties = msgpack.decode(Buffer.from(input));
        if (properties.length === 0 || !(properties instanceof Array)) {
            throw new Error("Invalid payload.");
        }
        var messageType = properties[0];
        switch (messageType) {
            case MessageType.Invocation:
                return this.createInvocationMessage(this.readHeaders(properties), properties);
            case MessageType.StreamItem:
                return this.createStreamItemMessage(this.readHeaders(properties), properties);
            case MessageType.Completion:
                return this.createCompletionMessage(this.readHeaders(properties), properties);
            case MessageType.Ping:
                return this.createPingMessage(properties);
            case MessageType.Close:
                return this.createCloseMessage(properties);
            default:
                // Future protocol changes can add message types, old clients can ignore them
                logger.log(LogLevel.Information, "Unknown message type '" + messageType + "' ignored.");
                return null;
        }
    };
    MessagePackHubProtocol.prototype.createCloseMessage = function (properties) {
        // check minimum length to allow protocol to add items to the end of objects in future releases
        if (properties.length < 2) {
            throw new Error("Invalid payload for Close message.");
        }
        return {
            // Close messages have no headers.
            allowReconnect: properties.length >= 3 ? properties[2] : undefined,
            error: properties[1],
            type: MessageType.Close,
        };
    };
    MessagePackHubProtocol.prototype.createPingMessage = function (properties) {
        // check minimum length to allow protocol to add items to the end of objects in future releases
        if (properties.length < 1) {
            throw new Error("Invalid payload for Ping message.");
        }
        return {
            // Ping messages have no headers.
            type: MessageType.Ping,
        };
    };
    MessagePackHubProtocol.prototype.createInvocationMessage = function (headers, properties) {
        // check minimum length to allow protocol to add items to the end of objects in future releases
        if (properties.length < 5) {
            throw new Error("Invalid payload for Invocation message.");
        }
        var invocationId = properties[2];
        if (invocationId) {
            return {
                arguments: properties[4],
                headers: headers,
                invocationId: invocationId,
                streamIds: [],
                target: properties[3],
                type: MessageType.Invocation,
            };
        }
        else {
            return {
                arguments: properties[4],
                headers: headers,
                streamIds: [],
                target: properties[3],
                type: MessageType.Invocation,
            };
        }
    };
    MessagePackHubProtocol.prototype.createStreamItemMessage = function (headers, properties) {
        // check minimum length to allow protocol to add items to the end of objects in future releases
        if (properties.length < 4) {
            throw new Error("Invalid payload for StreamItem message.");
        }
        return {
            headers: headers,
            invocationId: properties[2],
            item: properties[3],
            type: MessageType.StreamItem,
        };
    };
    MessagePackHubProtocol.prototype.createCompletionMessage = function (headers, properties) {
        // check minimum length to allow protocol to add items to the end of objects in future releases
        if (properties.length < 4) {
            throw new Error("Invalid payload for Completion message.");
        }
        var resultKind = properties[3];
        if (resultKind !== this.voidResult && properties.length < 5) {
            throw new Error("Invalid payload for Completion message.");
        }
        var error;
        var result;
        switch (resultKind) {
            case this.errorResult:
                error = properties[4];
                break;
            case this.nonVoidResult:
                result = properties[4];
                break;
        }
        var completionMessage = {
            error: error,
            headers: headers,
            invocationId: properties[2],
            result: result,
            type: MessageType.Completion,
        };
        return completionMessage;
    };
    MessagePackHubProtocol.prototype.writeInvocation = function (invocationMessage) {
        var msgpack = msgpack5(this.messagePackOptions);
        var payload;
        if (invocationMessage.streamIds) {
            payload = msgpack.encode([MessageType.Invocation, invocationMessage.headers || {}, invocationMessage.invocationId || null,
                invocationMessage.target, invocationMessage.arguments, invocationMessage.streamIds]);
        }
        else {
            payload = msgpack.encode([MessageType.Invocation, invocationMessage.headers || {}, invocationMessage.invocationId || null,
                invocationMessage.target, invocationMessage.arguments]);
        }
        return BinaryMessageFormat.write(payload.slice());
    };
    MessagePackHubProtocol.prototype.writeStreamInvocation = function (streamInvocationMessage) {
        var msgpack = msgpack5(this.messagePackOptions);
        var payload;
        if (streamInvocationMessage.streamIds) {
            payload = msgpack.encode([MessageType.StreamInvocation, streamInvocationMessage.headers || {}, streamInvocationMessage.invocationId,
                streamInvocationMessage.target, streamInvocationMessage.arguments, streamInvocationMessage.streamIds]);
        }
        else {
            payload = msgpack.encode([MessageType.StreamInvocation, streamInvocationMessage.headers || {}, streamInvocationMessage.invocationId,
                streamInvocationMessage.target, streamInvocationMessage.arguments]);
        }
        return BinaryMessageFormat.write(payload.slice());
    };
    MessagePackHubProtocol.prototype.writeStreamItem = function (streamItemMessage) {
        var msgpack = msgpack5(this.messagePackOptions);
        var payload = msgpack.encode([MessageType.StreamItem, streamItemMessage.headers || {}, streamItemMessage.invocationId,
            streamItemMessage.item]);
        return BinaryMessageFormat.write(payload.slice());
    };
    MessagePackHubProtocol.prototype.writeCompletion = function (completionMessage) {
        var msgpack = msgpack5(this.messagePackOptions);
        var resultKind = completionMessage.error ? this.errorResult : completionMessage.result ? this.nonVoidResult : this.voidResult;
        var payload;
        switch (resultKind) {
            case this.errorResult:
                payload = msgpack.encode([MessageType.Completion, completionMessage.headers || {}, completionMessage.invocationId, resultKind, completionMessage.error]);
                break;
            case this.voidResult:
                payload = msgpack.encode([MessageType.Completion, completionMessage.headers || {}, completionMessage.invocationId, resultKind]);
                break;
            case this.nonVoidResult:
                payload = msgpack.encode([MessageType.Completion, completionMessage.headers || {}, completionMessage.invocationId, resultKind, completionMessage.result]);
                break;
        }
        return BinaryMessageFormat.write(payload.slice());
    };
    MessagePackHubProtocol.prototype.writeCancelInvocation = function (cancelInvocationMessage) {
        var msgpack = msgpack5(this.messagePackOptions);
        var payload = msgpack.encode([MessageType.CancelInvocation, cancelInvocationMessage.headers || {}, cancelInvocationMessage.invocationId]);
        return BinaryMessageFormat.write(payload.slice());
    };
    MessagePackHubProtocol.prototype.readHeaders = function (properties) {
        var headers = properties[1];
        if (typeof headers !== "object") {
            throw new Error("Invalid headers.");
        }
        return headers;
    };
    return MessagePackHubProtocol;
}());
export { MessagePackHubProtocol };
//# sourceMappingURL=MessagePackHubProtocol.js.map