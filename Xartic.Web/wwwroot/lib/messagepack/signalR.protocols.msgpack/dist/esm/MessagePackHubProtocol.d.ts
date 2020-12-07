/// <reference types="node" />
import { MessagePackOptions } from "./MessagePackOptions";
import { HubMessage, IHubProtocol, ILogger, TransferFormat } from "@microsoft/signalr";
/** Implements the MessagePack Hub Protocol */
export declare class MessagePackHubProtocol implements IHubProtocol {
    /** The name of the protocol. This is used by SignalR to resolve the protocol between the client and server. */
    readonly name: string;
    /** The version of the protocol. */
    readonly version: number;
    /** The TransferFormat of the protocol. */
    readonly transferFormat: TransferFormat;
    private readonly errorResult;
    private readonly voidResult;
    private readonly nonVoidResult;
    private readonly messagePackOptions?;
    /**
     *
     * @param messagePackOptions MessagePack options passed to msgpack5
     */
    constructor(messagePackOptions?: MessagePackOptions);
    /** Creates an array of HubMessage objects from the specified serialized representation.
     *
     * @param {ArrayBuffer | Buffer} input An ArrayBuffer or Buffer containing the serialized representation.
     * @param {ILogger} logger A logger that will be used to log messages that occur during parsing.
     */
    parseMessages(input: ArrayBuffer | Buffer, logger: ILogger): HubMessage[];
    /** Writes the specified HubMessage to an ArrayBuffer and returns it.
     *
     * @param {HubMessage} message The message to write.
     * @returns {ArrayBuffer} An ArrayBuffer containing the serialized representation of the message.
     */
    writeMessage(message: HubMessage): ArrayBuffer;
    private parseMessage;
    private createCloseMessage;
    private createPingMessage;
    private createInvocationMessage;
    private createStreamItemMessage;
    private createCompletionMessage;
    private writeInvocation;
    private writeStreamInvocation;
    private writeStreamItem;
    private writeCompletion;
    private writeCancelInvocation;
    private readHeaders;
}
