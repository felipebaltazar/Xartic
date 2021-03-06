/**
 * MessagePack Options per:
 * {@link https://github.com/mcollina/msgpack5#msgpackoptionsobj Msgpack5 Options Object}
 */
export interface MessagePackOptions {
    /**
     * @name sortKeys Force a determinate key order
     */
    sortKeys?: boolean;
    /**
     * @name disableTimestampEncoding Disable the encoding of Dates into the timestamp extension type
     */
    disableTimestampEncoding?: boolean;
    /**
     * @name forceFloat64 Force floats to be encoded as 64-bit floats
     */
    forceFloat64?: boolean;
}
