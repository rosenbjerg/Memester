import { Component, h } from "preact";
import { Meme, MemeIdentification } from "../../models";
import Loading from "../../components/loading";
import ky from "ky";
import * as style from "./style.css";
import { route } from "preact-router";
import IconButton from "../../components/iconButton"
import CopiedText from "../../components/copiedText";

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

    //<video src={`/api/memes/${this.props.threadId}/${this.props.memeId}/video`} />

    render(_:Props, { meme }: State) {

        if (!meme) return <Loading />;

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
                                src={`/api/file/${meme.threadId}/${meme.id}/webm`}
                                type={"video/webm"}
                            />
                            {/*<source src={`/api/memes/${this.props.threadId}/${this.props.memeId}/video`} type={"video/webm"}/>*/}
                        </video>
                    </div>
                    <div className={`${style.memeFooter}`}>
                        <button class={`${style.copyButton}`} onClick={function(){
                            navigator.clipboard.writeText(window.location.href);
                            setTimeout(function(){

                            },300)
                            return(<CopiedText/>)
                        }}>COPY LINK!
                        </button>

                    </div>

                </div>
            </div>
        );
    }
}
