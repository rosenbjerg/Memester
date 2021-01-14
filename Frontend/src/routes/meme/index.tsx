import { Component, h } from "preact";
import { Meme, MemeIdentification } from "../../models";
import Loading from "../../components/Loading";
import ky from "ky";
import * as style from "./style.css";
import { route } from "preact-router";
import IconButton from "../../components/IconButton"
import CopiedText from "../../components/CopiedText";
import { useCallback, useEffect } from "preact/hooks";

interface Props {
    threadId: number;
    memeId: number;
}

interface State {
    meme?: Meme;
}

let muted = true;

export default class MemePage extends Component<Props, State> {
    componentDidMount() {
        this.loadMeme();
    }

    componentDidUpdate(previousProps: Readonly<Props>, _previousState: Readonly<State>, _snapshot: any) {
        if(previousProps.memeId !== this.props.memeId){
            this.loadMeme();
        }
    }

    loadMeme = () => {
        ky.get(`/api/memes/${this.props.threadId}/${this.props.memeId}`)
            .json<Meme>()
            .then(meme => this.setState({ meme }));
    };

    nextMeme = () => {
         return ky.get(`/api/memes`)
            .json<MemeIdentification>()
            .then(meme => route(`/${meme.threadId}/${meme.id}`));
    }

    escFunction = useCallback((event) => {
        console.log(event.key)
        if(event.key === "ArrowRight") {
            this.nextMeme();
        }
        else if(event.key === "ArrowLeft" && document.referrer != ""){
            window.history.back();
        }
        else if(event.key === "c"){
            navigator.clipboard.writeText(window.location.href);
        }
    },[]);
    //<video src={`/api/memes/${this.props.threadId}/${this.props.memeId}/video`} />

    render(_:Props, { meme }: State) {

        if (!meme) return <Loading title={"Fetching Meme"}/>;

        useEffect(() => {
            document.addEventListener("keydown", this.escFunction, false);
            return () => {
                document.removeEventListener("keydown", this.escFunction, false);
            };
        }, []);

        return (
            <div class={style.wrapper}>
                <div class={style.memeDiv}>
                    <div class={style.memeHeader}>
                        <div class={`${style.threadIdText} truncate`} onClick={() => route(`/${meme.threadId}`)} key={meme.threadId} tabIndex={0}>{meme.threadName}</div>
                        <div class={`truncate`}>{meme.name}</div>
                        <div class={style.headerControls}>
                            <IconButton icon={"navigate_next"} style={'font-size: 32px'} onClick={this.nextMeme}/>
                        </div>
                    </div>
                    <div class={style.videoWrapper}>
                        <video
                            className={style.video}
                            key={meme.id}
                            autoPlay
                            muted={muted}
                            controls
                            onVolumeChange={(x: any) => {
                                muted = x.target.muted;
                            }}
                        >
                            <source
                                src={`https://memester.club/api/file/${meme.id}/webm`}
                                type={"video/webm"}
                            />
                            {/*<source src={`/api/memes/${this.props.threadId}/${this.props.memeId}/video`} type={"video/webm"}/>*/}
                        </video>
                    </div>
                    <div className={`${style.memeFooter}`}>
                        <div className={`${style.memeFooterControls}`}>
                            {/*<div className={`${style.likeDiv}`}><IconButton icon={"thumb_down"} onClick={this.nextMeme}*/}
                            {/*                                                style={'color: rgba(0,0,0,0); font-size: 32px'} />*/}
                            {/*</div>*/}
                            <button className={`${style.copyButton}`} onClick={function() {
                                navigator.clipboard.writeText(window.location.href);
                                return (<CopiedText />)
                            }}>COPY LINK!
                            </button>
                            {/*<div className={`${style.likeDiv}`}><IconButton icon={"thumb_up"} onClick={this.nextMeme}*/}
                            {/*                                                style={'color: rgba(0,0,0,0); font-size: 32px'} />*/}
                            {/*</div>*/}
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}
