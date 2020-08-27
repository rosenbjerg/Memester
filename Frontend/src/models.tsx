export interface Meme extends MemeIdentification {
    name: string;
    threadName: string;
    fileId: number;
}

export interface MemeIdentification{
    threadId: number;
    id: number;
}

export interface Thread {
    id: number;
    name: string;
}

export interface FullThread {
    id: number;
    name: string;
    memes: Meme[];
}

