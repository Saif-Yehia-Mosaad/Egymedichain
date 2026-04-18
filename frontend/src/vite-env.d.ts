/// <reference types="vite/client" />

declare module '*.css' {
  const content: string;
  export default content;
}

declare module '@microsoft/signalr' {
  export enum LogLevel { Warning = 2, Error = 4 }
  export class HubConnectionBuilder {
    withUrl(url: string, options?: { accessTokenFactory: () => string }): HubConnectionBuilder;
    withAutomaticReconnect(): HubConnectionBuilder;
    configureLogging(level: LogLevel): HubConnectionBuilder;
    build(): HubConnection;
  }
  export interface HubConnection {
    on(methodName: string, newMethod: (...args: unknown[]) => void): void;
    start(): Promise<void>;
    stop(): Promise<void>;
  }
}
