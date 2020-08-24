
export interface Meme {
    name: string;
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

