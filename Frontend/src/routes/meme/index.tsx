import { Component, h } from "preact";
import { Meme, MemeIdentification } from "../../models";
import Loading from "../../components/Loading";
import ky from "ky";
import * as style from "./style.css";
import { route } from "preact-router";

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

    componentDidUpdate(previousProps: Readonly<Props>, previousState: Readonly<State>, snapshot: any) {
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
        ky.get(`/api/memes`)
            .json<MemeIdentification>()
            .then(meme => route(`/${meme.threadId}/${meme.id}`));
    }

    //<video src={`/api/memes/${this.props.threadId}/${this.props.memeId}/video`} />

    render({ memeId, threadId }: Props, { meme }: State) {
        if (!meme) return <Loading />;

        return (
            <div class={style.wrapper}>
                <div class={style.memeDiv}>
                    <div class={style.memeHeader}>
                        <div>{meme.name}</div>
                        <div class={style.headerControls}>
                            <button
                                class={`${style.nextButton} material-icons`}
                                onClick={this.nextMeme}
                            >
                                navigate_next
                            </button>
                        </div>
                    </div>
                    <video
                        key={meme.id}
                        autoPlay
                        muted={muted}
                        controls
                        onVolumeChange={(x:any) => {
                             muted = x.target.muted;
                        }}
                    >
                        <source
                            src={`https://is2.4chan.org/wsg/${meme.fileId}.webm`}
                            type={"video/webm"}
                        />
                        {/*<source src={`/api/memes/${this.props.threadId}/${this.props.memeId}/video`} type={"video/webm"}/>*/}
                    </video>
                    <div class={style.memeFooter}>
                        <button class={`${style.playButton} material-icons`}>
                            play
                        </button>
                    </div>
                </div>
            </div>
        );
    }
}
